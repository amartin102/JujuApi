using Application.Common.GenericResponse;
using Application.Dtos;
using Application.Services.Interface;
using AutoMapper;
using Domain.Entities;
using Repository.Interface;
using Repository.Logger;
using System.Linq.Expressions;
using System.Linq;
using Application.Common.DateHelper;
using Application.Common.MapExceptionToStatusCode;

namespace Application.Services
{
    public class PostService : IPostService
    {

        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly ILogInterface _logService;

        public PostService(IPostRepository postRepository, IMapper mapper, ILogInterface logService)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _logService = logService;
        }

        public async Task<GenericResponse<GetPostDto>> Create(CreatePostDto postDto)
        {
            try
            {
                var response = new GenericResponse<GetPostDto>();

                var entity = _mapper.Map<PostEntity>(postDto);

                entity.CreatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.CreatedBy = "System";

                var created = await _postRepository.Create(entity);

                response.Data = _mapper.Map<GetPostDto>(created);
                response.Success = true;
                response.StatusCode = 201;

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en Create", ex.Message, ex);

                return new GenericResponse<GetPostDto>
                {
                    Success = false,
                    Message = $"Error al crear el post: {ex.InnerException?.Message ?? ex.Message}",
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        public async Task<GenericResponse<bool>> Delete(int id)
        {
            try
            {
                var response = new GenericResponse<bool>();
                var deleted = _postRepository.Delete(id).Result;
                response.Data = deleted;
                response.Success = deleted;

                if (deleted)
                {
                    response.StatusCode = 204;
                    response.Message = "Eliminación realizada correctamente.";
                }
                else
                {                    
                    await _logService.LogAsync("Warning", $"Delete no realizado: PostId={id} no encontrado o no eliminado", null, null);
                    response.StatusCode = 404;
                    response.Message = $"Post con id={id} no encontrado o no eliminado.";
                }

                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en Delete id={id}", ex.Message, ex);
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = $"Error al eliminar el post con id: {id}. Detalles: {ex.InnerException?.Message ?? ex.Message}",
                    Data = false,
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        public async Task<GenericResponse<IQueryable<GetPostDto>>> GetAll()
        {
            try
            {
                var response = new GenericResponse<IQueryable<GetPostDto>>();
                var posts = _postRepository.GetAll().Result;
                response.Data = posts.Select(p => _mapper.Map<GetPostDto>(p)).AsQueryable();
                response.Success = true;
                response.StatusCode = 200;

                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en GetAll", ex.Message, ex);
                return new GenericResponse<IQueryable<GetPostDto>>
                {
                    Success = false,
                    Message = $"Error al obtener posts: {ex.InnerException?.Message ?? ex.Message}",
                    Data = Enumerable.Empty<GetPostDto>().AsQueryable(),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        public async Task<GenericResponse<List<GetPostDto>>> GetByCustomerId(int customerId)
        {
            try
            {
                var response = new GenericResponse<List<GetPostDto>>();
                var posts = _postRepository.GetByCustomerId(customerId).Result;
                response.Data = posts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
                response.Success = true;
                response.StatusCode = 200;
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en GetByCustomerId customerId={customerId}", ex.Message, ex);
                return new GenericResponse<List<GetPostDto>>
                {
                    Success = false,
                    Message = $"Error al obtener posts por cliente con id={customerId}. Detalles : {ex.InnerException?.Message ?? ex.Message}",
                    Data = new List<GetPostDto>(),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        public async Task<GenericResponse<GetPostDto?>> GetById(int id)
        {
            try
            {
                var response = new GenericResponse<GetPostDto?>();
                var post = _postRepository.GetById(id).Result;
                response.Data = post != null ? _mapper.Map<GetPostDto>(post) : null;
                response.Success = post != null;
                response.Message = post == null ? "Post not found." : string.Empty;
                response.StatusCode = post != null ? 200 : 404;
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en GetById id={id}", ex.Message, ex);
                return new GenericResponse<GetPostDto?>
                {
                    Success = false,
                    Message = $"Error al obtener el post por id={id}. Detalles: {ex.InnerException?.Message ?? ex.Message}",
                    Data = null,
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        public async Task<GenericResponse<(GetPostDto? updatedPost, bool changed)>> Update(UpdatePostDto post)
        {
            try
            {
                var response = new GenericResponse<(GetPostDto? updatedPost, bool changed)>();

                // Obtener el registro existente de forma asíncrona
                var entity = await _postRepository.GetById(post.PostId);
                if (entity == null)
                {
                    response.Data = (null, false);
                    response.Success = false;
                    response.Message = "Post not found.";
                    response.StatusCode = 404;

                    await _logService.LogAsync("Warning", $"Update no realizado: PostId={post.PostId} no encontrado", null, null);
                    return response;
                }

                // Mapear los cambios del DTO sobre la entidad existente (preserva CreatedAt/CreatedBy)
                _mapper.Map(post, entity);
                entity.UpdatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.UpdatedBy = "System";
                // Llamada al repositorio para actualizar (se espera un tuple: (entity, changed))
                var updatedResult = await _postRepository.Update(entity);
                // Desestructurar el resultado (compatible con ValueTuple)
                var (updatedEntity, changed) = updatedResult;

                if (changed)
                {
                    response.Data = (_mapper.Map<GetPostDto>(updatedEntity), true);
                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "Actualización realizada correctamente.";
                }
                else
                {
                    response.Data = (null, false);
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "No se realizaron cambios.";

                    await _logService.LogAsync("Warning", $"Update no realizado: PostId={post.PostId} sin cambios", null, null);
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en Update PostId={post.PostId}", ex.Message, ex);
                return new GenericResponse<(GetPostDto? updatedPost, bool changed)>
                {
                    Success = false,
                    Message = $"Error al actualizar el post: {ex.InnerException?.Message ?? ex.Message}",
                    Data = (null, false),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
    }
}
