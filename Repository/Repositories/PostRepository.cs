using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Interface;
using Repository.Loggin;
using static Repository.Repositories.BaseRepository;

namespace Repository.Repositories
{
    public class PostRepository : Repository<PostEntity, JujuTestContext>, IPostRepository 
    {   
        public PostRepository(JujuTestContext context, ILogInterface logService) : base(context, logService)
        {
        }

        public async Task<PostEntity> Create(PostEntity post)
        {
            return await AddAsync(post);
        }

        public async Task<bool> Delete(int id)
        {
           return await DeleteAsync(id);
        }

        public async Task<IQueryable<PostEntity>> GetAll()
        {
            return await GetAllAsync();
        }

        public async Task<PostEntity?> GetById(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<List<PostEntity>> GetByCustomerId(int customerId)
        {
            return await _context.Posts.Where(p => p.CustomerId == customerId).ToListAsync();
        }

        public async Task<(PostEntity? updatedPost, bool changed)> Update(PostEntity post)
        {
            var existingPost = await GetById(post.PostId);
            if (existingPost != null)
            {                
                return await UpdateAsync(post, existingPost);
            }
            return (null, false);
        }
    }
}
