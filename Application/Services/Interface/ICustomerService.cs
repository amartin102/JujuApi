using Application.Common.GenericResponse;
using Application.Dtos.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface ICustomerService
    {
            Task<GenericResponse<IQueryable<GetCustomerDto>>> GetAll();

            Task<GenericResponse<GetCustomerDto?>> GetById(int id);
    
            Task<GenericResponse<GetCustomerDto>> Create(CreateCustomerDto customer);
    
            Task<GenericResponse<(GetCustomerDto? updatedCustomer, bool changed)>> Update(UpdateCustomerDto customer);
    
            Task<GenericResponse<bool>> Delete(int id); 
    }
}
