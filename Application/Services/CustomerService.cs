using Application.Common.DateHelper;
using Application.Common.GenericResponse;
using Application.Common.MapExceptionToStatusCode;
using Application.Dtos.Customer;
using Application.Dtos.Post;
using Application.Services.Interface;
using AutoMapper;
using Domain.Entities;
using Repository.Interface;
using Repository.Logger;

namespace Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly ILogInterface _logService;
        private readonly IPostService _postService;

        public CustomerService(ICustomerRepository customerRepository, IMapper mapper, ILogInterface logService, IPostService postService)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _logService = logService;
            _postService = postService; 
        }    

        public async Task<GenericResponse<GetCustomerDto>> Create(CreateCustomerDto customerDto)
        {
            try
            {
                var response = new GenericResponse<GetCustomerDto>();

                var validateUser = await _customerRepository.GetByName(customerDto.Name.Trim());

                if(validateUser != null)
                {
                    response.Success = false;
                    response.Message = "Customer with the same name already exists.";
                    response.StatusCode = 409;
                    return response;
                }

                var entity = _mapper.Map<CustomerEntity>(customerDto);

                entity.CreatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.CreatedBy = "System";

                var created = await _customerRepository.Create(entity);

                response.Data = _mapper.Map<GetCustomerDto>(created);
                response.Success = true;
                response.StatusCode = 201;

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en Create Customer", ex.Message, ex);

                return new GenericResponse<GetCustomerDto>
                {
                    Success = false,
                    Message = $"Error al crear el cliente: {ex.InnerException?.Message ?? ex.Message}",
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }

        }

        public async Task<GenericResponse<bool>> Delete(int id)
        {
            try
            {
                var response = new GenericResponse<bool>();

                var deletePostsResult = await _postService.DeleteAll(id);

                if (!deletePostsResult.Success)
                {
                    await _logService.LogAsync("Warning", $"DeleteAll posts no realizado: CustomerId={id} no encontrado o sin posts para eliminar", null, null);

                    await _logService.LogAsync("Warning", $"Delete no realizado: CustomerId={id} no encontrado o no eliminado", null, null);
                    response.StatusCode = 404;
                    response.Message = $"Customer con id={id} no encontrado o no eliminado.";
                }
                else {
                    var deleted = await _customerRepository.Delete(id);
                    response.Data = deleted;
                    response.Success = deleted;

                    if (deleted)
                    {
                        response.StatusCode = 204;
                        response.Message = "Eliminación realizada correctamente.";
                    }
                    else
                    {
                        await _logService.LogAsync("Warning", $"Delete no realizado: CustomerId={id} no encontrado o no eliminado", null, null);
                        response.StatusCode = 404;
                        response.Message = $"Customer con id={id} no encontrado o no eliminado.";
                    }
                }               

                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en Delete Customer id={id}", ex.Message, ex);
                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = $"Error al eliminar el customer con id: {id}. Detalles: {ex.InnerException?.Message ?? ex.Message}",
                    Data = false,
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }

        }

        public async Task<GenericResponse<IQueryable<GetCustomerDto>>> GetAll()
        {
            try
            {
                var response = new GenericResponse<IQueryable<GetCustomerDto>>();
                var customers = await _customerRepository.GetAll();
                response.Data = customers.Select(c => _mapper.Map<GetCustomerDto>(c)).AsQueryable();
                response.Success = true;
                response.StatusCode = 200;

                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en GetAll", ex.Message, ex);
                return new GenericResponse<IQueryable<GetCustomerDto>>
                {
                    Success = false,
                    Message = $"Error al obtener customers: {ex.InnerException?.Message ?? ex.Message}",
                    Data = Enumerable.Empty<GetCustomerDto>().AsQueryable(),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        public async Task<GenericResponse<GetCustomerDto?>> GetById(int id)
        {
            try
            {
                var response = new GenericResponse<GetCustomerDto?>();
                var customer = await _customerRepository.GetById(id);
                response.Data = customer != null ? _mapper.Map<GetCustomerDto>(customer) : null;
                response.Success = customer != null;
                response.Message = customer == null ? "Customer not found." : string.Empty;
                response.StatusCode = customer != null ? 200 : 404;
                return await Task.FromResult(response);
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en GetById id={id}", ex.Message, ex);
                return new GenericResponse<GetCustomerDto?>
                {
                    Success = false,
                    Message = $"Error al obtener el customer por id={id}. Detalles: {ex.InnerException?.Message ?? ex.Message}",
                    Data = null,
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }              

        public async Task<GenericResponse<(GetCustomerDto? updatedCustomer, bool changed)>> Update(UpdateCustomerDto customer)
        {
            try
            {
                var response = new GenericResponse<(GetCustomerDto? updatedCustomer, bool changed)>();
                // Obtener el registro existente de forma asíncrona
                var entity = await _customerRepository.GetById(customer.CustomerId);
                if (entity == null)
                {
                    response.Data = (null, false);
                    response.Success = false;
                    response.Message = "Customer not found.";
                    response.StatusCode = 404;

                    await _logService.LogAsync("Warning", $"Update no realizado: CustomerId={customer.CustomerId} no encontrado", null, null);
                    return response;
                }

                _mapper.Map(customer, entity);
                entity.UpdatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.UpdatedBy = "System";

                var updatedResult = await _customerRepository.Update(entity);
                var (updatedEntity, changed) = updatedResult;

                if (changed)
                {
                    response.Data = (_mapper.Map<GetCustomerDto>(updatedEntity), true);
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

                    await _logService.LogAsync("Warning", $"Update no realizado: CustomerId={customer.CustomerId} sin cambios", null, null);
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en Update CustomerId={customer.CustomerId}", ex.Message, ex);
                return new GenericResponse<(GetCustomerDto? updatedCustomer, bool changed)>
                {
                    Success = false,
                    Message = $"Error al actualizar el customer: {ex.InnerException?.Message ?? ex.Message}",
                    Data = (null, false),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
    }
}
