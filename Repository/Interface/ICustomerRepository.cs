using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ICustomerRepository
    {
            Task<List<CustomerEntity>> GetAll();
    
            Task<CustomerEntity?> GetById(int id);

            Task<List<CustomerEntity>> GetByIds(List<int> ids);

            Task<CustomerEntity> Create(CustomerEntity customer);
    
            Task<(CustomerEntity? updatedCustomer, bool changed)> Update(CustomerEntity customer);
    
            Task<bool> Delete(int id);

            Task<CustomerEntity?> GetByName(string name);
    }
}
