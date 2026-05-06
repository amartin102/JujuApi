using Domain.Entities;

namespace Repository.Interface
{
    public interface IPostRepository
    {
        Task<List<PostEntity>> GetByCustomerId(int customerId);

        Task<PostEntity?> GetById(int id);

        Task<IQueryable<PostEntity>> GetAll();

        Task<PostEntity> Create(PostEntity post);

        Task<(PostEntity? updatedPost, bool changed)> Update(PostEntity post);

        Task<bool> Delete(int id);

        Task<bool> DeleteAllAsync(int customerId);
    }
}
