using Application.Common;
using Application.Common.DateHelper;
using Application.Common.GenericResponse;
using Application.Common.MapExceptionToStatusCode;
using Application.Common.Messages;
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

                if (validateUser != null)
                {
                    response.Success = false;
                    response.Message = ErrorMessages.AlreadyExists("Customer", "nombre");
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    return response;
                }

                var entity = _mapper.Map<CustomerEntity>(customerDto);

                entity.CreatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.CreatedBy = "System";

                var created = await _customerRepository.Create(entity);

                response.Data = _mapper.Map<GetCustomerDto>(created);
                response.Success = true;
                response.StatusCode = (int)HttpStatusCode.Created;
                response.Message = ErrorMessages.Created("Customer");

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en Create Customer", ex.Message, ex);

                return new GenericResponse<GetCustomerDto>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("crear", "Customer", ex),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }

        public async Task<GenericResponse<bool>> Delete(int id)
        {
            try
            {
                var response = new GenericResponse<bool>();

                //Validar que exista el id
                var exists = await _customerRepository.GetById(id);

                if (exists == null)
                {
                    response.Success = false;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.NotFound("Customer", id);
                    return response;
                }

                var deletePostsResult = await _postService.DeleteAll(id);

                if (!deletePostsResult.Success)
                {
                    await _logService.LogAsync("Warning",
                        $"DeleteAll posts no realizado: CustomerId={id}", null, null);

                    response.Success = false;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.DeleteFailedByParent("Posts", "Customer", id);
                    return response;
                }

                var deleted = await _customerRepository.Delete(id);

                response.Data = deleted;
                response.Success = deleted;

                if (deleted)
                {
                    response.StatusCode = (int)HttpStatusCode.NoContent;
                    response.Message = ErrorMessages.Deleted("Customer");
                }
                else
                {
                    await _logService.LogAsync("Warning",
                        $"Delete no realizado: CustomerId={id}", null, null);

                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.DeleteFailed("Customer", id);
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en Delete Customer id={id}", ex.Message, ex);

                return new GenericResponse<bool>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("eliminar", "Customer", ex),
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
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Message = ErrorMessages.Success("obtener", "Customers");

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", "ServiceException en GetAll", ex.Message, ex);

                return new GenericResponse<IQueryable<GetCustomerDto>>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("obtener", "Customers", ex),
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
                response.StatusCode = customer != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound;
                response.Message = customer == null ? ErrorMessages.NotFound("Customer", id) : string.Empty;

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en GetById id={id}", ex.Message, ex);

                return new GenericResponse<GetCustomerDto?>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("obtener", "Customer", ex),
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

                var entity = await _customerRepository.GetById(customer.CustomerId);

                if (entity == null)
                {
                    await _logService.LogAsync("Warning",
                        $"Update no realizado: CustomerId={customer.CustomerId}", null, null);

                    return new GenericResponse<(GetCustomerDto?, bool)>
                    {
                        Data = (null, false),
                        Success = false,
                        Message = ErrorMessages.NotFound("Customer", customer.CustomerId),
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

                _mapper.Map(customer, entity);
                entity.UpdatedAt = DateHelper.ToLocalTime(DateTime.UtcNow);
                entity.UpdatedBy = "System";

                var (updatedEntity, changed) = await _customerRepository.Update(entity);

                if (changed)
                {
                    response.Data = (_mapper.Map<GetCustomerDto>(updatedEntity), true);
                    response.Success = true;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Message = ErrorMessages.Updated("Customer");
                }
                else
                {
                    await _logService.LogAsync("Warning",
                        $"Update sin cambios: CustomerId={customer.CustomerId}", null, null);

                    response.Data = (null, false);
                    response.Success = false;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ErrorMessages.NoChanges("Customer");
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error",
                    $"ServiceException en Update CustomerId={customer.CustomerId}", ex.Message, ex);

                return new GenericResponse<(GetCustomerDto?, bool)>
                {
                    Success = false,
                    Message = ErrorMessages.ErrorWithDetails("actualizar", "Customer", ex),
                    Data = (null, false),
                    StatusCode = MapExceptionStatusCode.GetStatusCode(ex)
                };
            }
        }
    }
}
