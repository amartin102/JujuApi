using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Interface;
using Repository.Logger;
using System.Linq.Expressions;
namespace Repository.Repositories
{
    public class BaseRepository
    {
        public abstract class Repository<TEntity, TContext> : IBaseRepository<TEntity>
         where TEntity : class
         where TContext : JujuTestContext
        {
            protected readonly TContext _context;           

            public Repository(TContext context)
            {
                this._context = context;
            }

            //Agregar uno a uno
            public async Task<TEntity> AddAsync(TEntity entity)
            {
               await _context.Set<TEntity>().AddAsync(entity);
               await _context.SaveChangesAsync();
               return entity;
            }

            //Agregar todos los registros relacionados de una entidad
            public async Task<List<TEntity>> AddRangeAsync(List<TEntity> entity)
            {
                await _context.Set<TEntity>().AddRangeAsync(entity);
                await _context.SaveChangesAsync();
                return entity;                  
            }

            //Inactivar un registro de una entidad
            public async Task<bool> DeleteAsync(int id)
            {
                    var entity = await _context.Set<TEntity>().FindAsync(id);

                    if (entity != null)
                    {
                        var propiedad = entity.GetType().GetProperty("State");

                        if (propiedad != null)
                        {
                            propiedad.SetValue(entity, false);
                            _context.Update(entity);
                            await _context.SaveChangesAsync();
                        }
                        return true;
                    }

                    return false;                

            }

            //Inactivar todos los registros relacionados de una entidad
            public async Task<bool> DeleteAllAsync(int id)
            {                
                    var propertyActive = typeof(TEntity).GetProperty("State");

                    if (propertyActive != null)
                    {
                        var entities = await _context.Set<TEntity>()
                            .Where(e => EF.Property<int>(e, "CustomerId") == id)
                            .ToListAsync();

                        entities.ForEach(e => propertyActive.SetValue(e, false));

                        await _context.SaveChangesAsync();
                        return true;
                    }

                    return false;                
            }

            public Task<IQueryable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = null)
            {
                    IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

                    if (includeFunc != null)
                    {
                        query = includeFunc(query);
                    }

                    return Task.FromResult(query);               

            }

            /*
            public async Task<TEntity> GetByIdAsync(int id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeFunc = null)
            {
                try
                {
                    IQueryable<TEntity> query = _context.Set<TEntity>();

                    if (includeFunc != null)
                    {
                        query = includeFunc(query);
                    }

                    return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "CustomerId") == id);// Carga los detalles relacionados

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            
             */

            public async Task<TEntity?> GetByIdAsync(int id)
            {
                var result = await _context.Set<TEntity>().FindAsync(id);
                return result;
            }

            public async Task<(TEntity entity, bool changed)> UpdateAsync(TEntity editedEntity, TEntity originalEntity)
            {             
                _context.Entry(originalEntity).CurrentValues.SetValues(editedEntity);

                bool changed = _context.Entry(originalEntity).State == EntityState.Modified;
                if (changed)
                {
                  await _context.SaveChangesAsync();
                }

                return (originalEntity, changed);                
                
            }

        }
    }
}
