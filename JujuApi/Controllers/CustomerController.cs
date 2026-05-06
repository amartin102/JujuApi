using Application.Common.GenericResponse;
using Application.Dtos.Customer;
using Application.Dtos.Post;
using Application.Services;
using Application.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JujuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService; 

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("GetById/{customerId}")]
        public async Task<ActionResult<GenericResponse<List<GetCustomerDto>>>> GetById(int customerId)
        {
            try
            {
                var customers = await _customerService.GetById(customerId);

                if (customers.Success)
                    return Ok(customers);

                if (customers.StatusCode.HasValue)
                    return StatusCode(customers.StatusCode.Value, customers);

                // Si no hay StatusCode y no fue success, asumir NotFound
                return NotFound(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<List<GetCustomerDto>> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<GenericResponse<IQueryable<GetCustomerDto>>>> GetAll()
        {
            try
            {
                var customers = await _customerService.GetAll();
                if (customers.Success)
                    return Ok(customers);

                if (customers.StatusCode.HasValue)
                    return StatusCode(customers.StatusCode.Value, customers);

                return BadRequest(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<IQueryable<GetCustomerDto>> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<GenericResponse<GetCustomerDto>>> Create(CreateCustomerDto customerDto)
        {
            try
            {
                var createdCustomer = await _customerService.Create(customerDto);
                if (createdCustomer.Success)
                    return CreatedAtAction(nameof(GetById), new { customerId = createdCustomer.Data?.CustomerId }, createdCustomer);

                if (createdCustomer.StatusCode.HasValue)
                    return StatusCode(createdCustomer.StatusCode.Value, createdCustomer);

                return BadRequest(createdCustomer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<GetCustomerDto> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpPut("Update")]
        public async Task<ActionResult<GenericResponse<GetCustomerDto>>> Update(UpdateCustomerDto customerDto)
        {
            try
            {
                var updateResult = await _customerService.Update(customerDto);
                var response = new GenericResponse<GetCustomerDto>
                {
                    Success = updateResult.Success,
                    Message = updateResult.Message,
                    StatusCode = updateResult.StatusCode,
                    Data = updateResult.Data.updatedCustomer
                };

                if (response.StatusCode.HasValue)
                {
                    if (response.StatusCode.Value == StatusCodes.Status204NoContent)
                        return NoContent();

                    return StatusCode(response.StatusCode.Value, response);
                }

                return response.Success ? Ok(response) : NotFound(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<GetCustomerDto   > { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> Delete(int id)
        {
            try
            {
                var deleteResult = await _customerService.Delete(id);

                if (deleteResult.StatusCode.HasValue)
                {
                    if (deleteResult.StatusCode.Value == StatusCodes.Status204NoContent)
                        return NoContent();
                    return StatusCode(deleteResult.StatusCode.Value, deleteResult);
                }

                return deleteResult.Success ? Ok(deleteResult) : BadRequest(deleteResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GenericResponse<bool> { Message = $"Error interno del servidor. {ex.Message}", Success = false });
            }
        }

    }
}
