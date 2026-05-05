using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity> AddAsync(TEntity entity);

        Task<List<TEntity>> AddRangeAsync(List<TEntity> entity);

        Task<bool> DeleteAsync(int id);

        Task<bool> DeleteAllAsync(int id);

        Task<IQueryable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null);

        Task<TEntity?> GetByIdAsync(int id);

        Task<(TEntity entity, bool changed)> UpdateAsync(TEntity editedEntity, TEntity originalEntity);
    }
}
