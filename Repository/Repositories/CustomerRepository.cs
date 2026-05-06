using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Interface;
using static Repository.Repositories.BaseRepository;

namespace Repository.Repositories
{
    public class CustomerRepository : Repository<CustomerEntity, JujuTestContext>, ICustomerRepository
    {
        public CustomerRepository(JujuTestContext context) : base(context)
        {
        }
        
        public async Task<List<CustomerEntity>> GetAll()
        {
            var query = await GetAllAsync(q => q.Include(p => p.Posts));

            return query.ToListAsync().Result;
        }

        public async Task<CustomerEntity> Create(CustomerEntity customer)
        {
            return await AddAsync(customer);
        }

        public async Task<(CustomerEntity? updatedCustomer, bool changed)> Update(CustomerEntity customer)
        {
            var existingCustomer = await GetById(customer.CustomerId);
            if (existingCustomer != null)
            {
                return await UpdateAsync(customer, existingCustomer);
            }
            return (null, false);
        }

        public async Task<List<CustomerEntity>> GetByIds(List<int> ids)
        {
            return await _context.Customers
                .Where(c => ids.Contains(c.CustomerId) && c.State)
                .ToListAsync();
        }

        public async Task<bool> Delete(int id)
        {
            return await DeleteAsync(id);
        }

        public async Task<CustomerEntity?> GetById(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<CustomerEntity?> GetByName(string name)
        {            
            return await _context.Customers.Where(p => p.Name.ToLower() == name.ToLower()).Include(p => p.Posts).FirstOrDefaultAsync();
        }


    }
}
