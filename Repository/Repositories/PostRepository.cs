using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Interface;
using Repository.Logger;
using static Repository.Repositories.BaseRepository;

namespace Repository.Repositories
{
    public class PostRepository : Repository<PostEntity, JujuTestContext>, IPostRepository 
    {   
        public PostRepository(JujuTestContext context) : base(context)
        {
        }
        public async Task<IQueryable<PostEntity>> GetAll()
        {
            var query = await GetAllAsync(q => q.Include(p => p.Customer));

            return query.ToListAsync().Result.AsQueryable();
        }

        public async Task<PostEntity> Create(PostEntity post)
        {
            return await AddAsync(post);
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

        public async Task<bool> Delete(int id)
        {
           return await DeleteAsync(id);
        }              

        public async Task<PostEntity?> GetById(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<List<PostEntity>> GetByCustomerId(int customerId)
        {
            var res = await _context.Posts.Where(p => p.CustomerId == customerId).Include(p => p.Customer).ToListAsync();
            return res;
        }
                
    }
}
