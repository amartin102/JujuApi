using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Context;
using Repository.Interface;
using Repository.Loggin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class BaseRepository
    {
        public abstract class Repository<TEntity, TContext> : IBaseRepository<TEntity>
         where TEntity : class
         where TContext : JujuTestContext
        {
            protected readonly TContext _context;
            private readonly ILogInterface _logService;

            public Repository(TContext context, ILogInterface logService)
            {
                this._context = context;
                this._logService = logService;
            }

            //Agregar uno a uno
            public async Task<TEntity> AddAsync(TEntity entity)
            {
                try
                {
                    if (entity != null)
                    {
                        _context.Set<TEntity>().Add(entity);
                        await _context.SaveChangesAsync();
                        return entity;
                    }

                    return entity;
                }
                catch (Exception ex)
                {
                    await _logService.LogAsync("Error", "Error en AddAsync", ex);
                    return entity;
                }

            }

            //Agregar todos los registros relacionados de una entidad
            public async Task<List<TEntity>> AddRangeAsync(List<TEntity> entity)
            {
                try
                {
                    if (entity != null)
                    {
                        await _context.Set<TEntity>().AddRangeAsync(entity);
                        await _context.SaveChangesAsync();
                        return entity;
                    }
                    return entity;
                }
                catch (Exception ex)
                {
                    await _logService.LogAsync("Error", "Error en AddRangeAsync", ex);
                    return entity;
                }
            }

            //Inactivar un registro de una entidad
            public async Task<bool> DeleteAsync(int id)
            {
                try
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
                catch (Exception ex)
                {
                    await _logService.LogAsync("Error", "Error en DeleteAsync", ex);
                    return false;
                    throw;
                }

            }

            //Inactivar todos los registros relacionados de una entidad
            public async Task<bool> DeleteAllAsync(int id)
            {
                try
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
                catch (Exception ex)
                {
                    await _logService.LogAsync("Error", "Error en DeleteAllAsync", ex);
                    return false;
                    throw;
                }
            }


            public async Task<IQueryable<TEntity>> GetAllAsync()
            {
                return (IQueryable<TEntity>)await _context.Posts.AsNoTracking().ToListAsync();
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
                try
                {
                    // Copia los valores de la editada a la original
                    _context.Entry(originalEntity).CurrentValues.SetValues(editedEntity);

                    // Verificamos si hubo cambios antes de guardar
                    bool changed = _context.Entry(originalEntity).State == EntityState.Modified;

                    if (changed)
                    {
                        await _context.SaveChangesAsync();
                    }

                    return (originalEntity, changed);
                }
                catch (Exception ex)
                {                                        
                    await _logService.LogAsync("Error", "Error en UpdateAsync", ex);
                    throw;
                }
                
            }

        }
    }
}
