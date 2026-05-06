using Application.Dtos.Post;
using Application.Services.Interface;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateBulkPostsUseCase
    {
        private readonly IBulkPostService _bulkService;
        private readonly ICustomerRepository _customerRepository;

        public CreateBulkPostsUseCase(IBulkPostService bulkService, ICustomerRepository customerRepository)
        {
            _bulkService = bulkService;
            _customerRepository = customerRepository;
        }
        public async Task ExecuteAsync(List<CreatePostDto> posts)
        {            
            var customerIds = posts
                .Select(p => p.CustomerId)
                .Distinct().ToList();
                      
            var validCustomers = await _customerRepository.GetByIds(customerIds);

            var validCustomerIds = validCustomers
                .Select(c => c.CustomerId).ToHashSet();
                       
            var invalidPosts = posts
                .Where(p => !validCustomerIds.Contains(p.CustomerId))
                .ToList();

            if (invalidPosts.Any())
                throw new Exception("No es posible procesar, hay CustomerId inválidos");

            await _bulkService.BulkInsertAsync(posts);
        }
    }
}
