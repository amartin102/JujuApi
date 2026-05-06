using Application.Common;
using Application.Common.DateHelper;
using Application.Common.GenericMethods;
using Application.Common.GenericResponse;
using Application.Common.MapExceptionToStatusCode;
using Application.Common.Messages;
using Application.Dtos.Post;
using Application.Services.Interface;
using AutoMapper;
using Domain.Entities;
using Repository.Interface;
using Repository.Logger;
using Repository.Repositories;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly ILogInterface _logService;
        private readonly ICustomerRepository _customerRepository;
        private readonly GenericMethod _genericMethod;

        public PostService(IPostRepository postRepository, IMapper mapper, ILogInterface logService, ICustomerRepository customerRepository)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _logService = logService;
            _customerRepository = customerRepository;
            _genericMethod = new GenericMethod();
        }

        /// <summary>
        /// Crea un nuevo post. Valida que el customer exista antes de crear el post.
        /// Aplica las reglas de negocio para formatear el body, asignar la categoría y agrega las fechas auditoría y usuario.
        /// </summary>
        /// <param name="postDto">Datos del post a crear.</param>
        /// <returns>GenericResponse<GetPostDto></returns>
        public async Task<GenericResponse<GetPostDto>> Create(CreatePostDto postDto)
        {
            try
            {
                var response = new GenericResponse<GetPostDto>();

                if (!await ValidateCustomerExists(postDto.CustomerId))
                {
                    response.Success = false;
                    response.Message = ErrorMessages.NotFound("Customer", postDto.CustomerId);
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return response;
                }

                var entity = _mapper.Map<PostEntity>(postDto);

                entity.Body = _genericMethod.FormatBodyPreview(entity.Body);
                entity.Category = _genericMethod.GetCategory(entity.Type, entity.Category);
                entity.CreatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.CreatedBy = "System";

                var created = await _postRepository.Create(entity);

                response.Data = _mapper.Map<GetPostDto>(created);
                response.Success = true;
                response.StatusCode = (int)HttpStatusCode.Created;
                response.Message = ErrorMessages.Created("Post");

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en Create", ex.Message, ex);

                return new GenericResponse<GetPostDto>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("crear", "Post", ex),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
        
        /// <summary>
        /// Elimina un post por su ID.
        /// </summary>
        /// <param name="id">ID del post a eliminar.</param>
        /// <returns>GenericResponse<bool></returns>
        public async Task<GenericResponse<bool>> Delete(int id)
        {
            try
            {
                var response = new GenericResponse<bool>();
                //Validamos si el post existe antes de intentar eliminarlo
                var exists = await _postRepository.GetById(id);
                if (exists == null)
                {
                    await _logService.LogAsync("Warning",
                        $"Delete no realizado: PostId={id} no encontrado", null, null);
                    response.Data = false;
                    response.Success = false;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.NotFound("Post", id);
                    return response;
                }

                var deleted = await _postRepository.Delete(id);

                response.Data = deleted;
                response.Success = deleted;

                if (deleted)
                {
                    response.StatusCode = (int)HttpStatusCode.NoContent;
                    response.Message = ErrorMessages.Deleted("Post");
                }
                else
                {
                    await _logService.LogAsync("Warning",
                        $"Delete no realizado: PostId={id}", null, null);

                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.DeleteFailed("Post", id);
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en Delete id={id}", ex.Message, ex);

                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("eliminar", "Post", ex),
                    Data = false,
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
        
        /// <summary>
        /// Elimina todos los posts de un customer por su ID.
        /// </summary>
        /// <param name="customerId">ID del customer cuyos posts se eliminarán.</param>
        /// <returns>GenericResponse<bool></returns>
        public async Task<GenericResponse<bool>> DeleteAll(int customerId)
        {
            try
            {
                var response = new GenericResponse<bool>();
                var deleted = await _postRepository.DeleteAllAsync(customerId);

                response.Data = deleted;
                response.Success = deleted;

                if (deleted)
                {
                    response.StatusCode = (int)HttpStatusCode.NoContent;
                    response.Message = ErrorMessages.Deleted("Posts");
                }
                else
                {
                    await _logService.LogAsync("Warning",
                        $"DeleteAll no realizado: CustomerId={customerId}", null, null);

                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.DeleteFailed("Posts", customerId);
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en DeleteAll customerId={customerId}", ex.Message, ex);

                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("eliminar", "Posts", ex),
                    Data = false,
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
        
        /// <summary>
        /// Obtiene todos los posts.
        /// </summary>
        /// <returns>GenericResponse<IQueryable<GetPostDto>></returns>
        public async Task<GenericResponse<IQueryable<GetPostDto>>> GetAll()
        {
            try
            {
                var response = new GenericResponse<IQueryable<GetPostDto>>();
                var posts = await _postRepository.GetAll();

                response.Data = posts.Select(p => _mapper.Map<GetPostDto>(p)).AsQueryable();
                response.Success = true;
                response.StatusCode = (int)HttpStatusCode.OK;    
                response.Message = ErrorMessages.Success("obtener", "Posts");

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en GetAll", ex.Message, ex);

                return new GenericResponse<IQueryable<GetPostDto>>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("obtener", "Posts", ex),
                    Data = Enumerable.Empty<GetPostDto>().AsQueryable(),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
        
        /// <summary>
        /// Obtiene todos los posts de un customer por su ID.
        /// </summary>
        /// <param name="customerId">ID del customer cuyos posts se obtendrán.</param>
        /// <returns>GenericResponse<List<GetPostDto>></returns>
        public async Task<GenericResponse<List<GetPostDto>>> GetByCustomerId(int customerId)
        {
            try
            {
                var response = new GenericResponse<List<GetPostDto>>();
                var posts = await _postRepository.GetByCustomerId(customerId);

                response.Data = posts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
                response.Success = true;
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Message = ErrorMessages.Success("obtener", "posts asociados");

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en GetByCustomerId customerId={customerId}", ex.Message, ex);

                return new GenericResponse<List<GetPostDto>>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("obtener", "Posts", ex),
                    Data = new List<GetPostDto>(),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
        
        /// <summary>
        /// Obtiene un post por su ID.
        /// </summary>
        /// <param name="id">ID del post a obtener.</param>
        /// <returns>GenericResponse<GetPostDto?></returns>
        public async Task<GenericResponse<GetPostDto?>> GetById(int id)
        {
            try
            {
                var response = new GenericResponse<GetPostDto?>();
                var post = await _postRepository.GetById(id);

                response.Data = post != null ? _mapper.Map<GetPostDto>(post) : null;
                response.Success = post != null;
                response.StatusCode = post != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound;
                response.Message = post == null ? ErrorMessages.NotFound("Post", id) : string.Empty;

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en GetById id={id}", ex.Message, ex);

                return new GenericResponse<GetPostDto?>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("obtener", "Post", ex),
                    Data = null,
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        /// <summary>
        /// Actualiza un post existente.
        /// Aplica las reglas de negocio para formatear el body, asignar la categoría y actualizar las fechas auditoría y usuario.
        /// </summary>
        /// <param name="post">Datos del post a actualizar.</param>
        /// <returns>GenericResponse<(GetPostDto? updatedPost, bool changed)></returns>
        public async Task<GenericResponse<(GetPostDto? updatedPost, bool changed)>> Update(UpdatePostDto post)
        {
            try
            {
                var response = new GenericResponse<(GetPostDto? updatedPost, bool changed)>();

                var entity = await _postRepository.GetById(post.PostId);

                if (entity == null)
                {
                    await _logService.LogAsync("Warning",
                        $"Update no realizado: PostId={post.PostId}", null, null);

                    return new GenericResponse<(GetPostDto?, bool)>
                    {
                        Data = (null, false),
                        Success = false,
                        Message = ErrorMessages.NotFound("Post", post.PostId),
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

                _mapper.Map(post, entity);
                entity.Body = _genericMethod.FormatBodyPreview(entity.Body);
                entity.Category = _genericMethod.GetCategory(entity.Type, entity.Category);
                entity.UpdatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.UpdatedBy = "System";

                var (updatedEntity, changed) = await _postRepository.Update(entity);

                if (changed)
                {
                    response.Data = (_mapper.Map<GetPostDto>(updatedEntity), true);
                    response.Success = true;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Message = ErrorMessages.Updated("Post");
                }
                else
                {
                    await _logService.LogAsync("Warning",
                        $"Update sin cambios: PostId={post.PostId}", null, null);

                    response.Data = (null, false);
                    response.Success = false;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.NoChanges("Post");
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en Update PostId={post.PostId}", ex.Message, ex);

                return new GenericResponse<(GetPostDto?, bool)>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("actualizar", "Post", ex),
                    Data = (null, false),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        // Método privado para validar si el customer existe antes de crear un post
        private async Task<bool> ValidateCustomerExists(int customerId)
        {
            var customerResponse = await _customerRepository.GetById(customerId);

            if (customerResponse == null || !customerResponse.State)
            {
                return false;
            }

            return true;
        }

    }
}
