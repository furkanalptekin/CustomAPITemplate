using System.Linq.Expressions;
using CustomAPITemplate.Core;
using CustomAPITemplate.Core.Extensions;
using CustomAPITemplate.DB.Entity;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Extensions;

public static class DbContextExtensions
{
    public static async Task<Response<TEntity>> AddEntityAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken token) where TEntity : EntityBase
    {
        var response = new Response<TEntity>();
        if (entity == null)
        {
            response.Results.Add(new()
            {
                Message = "Entity is null",
                Severity = Severity.Error
            });
            return response;
        }

        await context.AddAsync(entity, token).ConfigureAwait(false);

        var changes = await context.SaveChangesAsync(token).ConfigureAwait(false);
        if (changes <= 0)
        {
            response.Results.Add(new()
            {
                Message = "Error occurred during savechanges!",
                Severity = Severity.Error
            });
            return response;
        }

        response.Results.Add(new()
        {
            Message = "Entity is successfully created!",
            Severity = Severity.Info
        });

        response.Value = entity;
        return response;
    }

    public static async Task<Response<int>> UpdateEntityAsync<TEntity>(this DbContext context, Guid id, TEntity entity, CancellationToken token, params string[] propertiesToIgnore) where TEntity : EntityBase
    {
        var response = new Response<int>();
        if (entity == null)
        {
            response.Results.Add(new()
            {
                Message = "Entity is null",
                Severity = Severity.Error
            });
            response.Value = -1;
            return response;
        }

        var dbEntity = await context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken: token).ConfigureAwait(false);
        if (dbEntity == null)
        {
            response.Results.Add(new()
            {
                Message = "Entity not found",
                Severity = Severity.Error
            });
            response.Value = -1;
            return response;
        }

        foreach (var property in entity.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty))
        {
            if (!property.CanRead || !property.CanWrite || propertiesToIgnore.Contains(property.Name))
                continue;

            property.SetValue(dbEntity, property.GetValue(entity, null), null);
        }
        context.Entry(dbEntity).State = EntityState.Modified;

        var changes = await context.SaveChangesAsync(token).ConfigureAwait(false);
        if (changes <= 0)
        {
            response.Results.Add(new()
            {
                Message = "An error occurred during savechanges!",
                Severity = Severity.Error
            });
            return response;
        }

        response.Results.Add(new()
        {
            Message = "Entity is successfully updated!",
            Severity = Severity.Info
        });

        response.Value = changes;
        return response;
    }

    public static async Task<Response<int>> RemoveEntityAsync<TEntity>(this DbContext context, Guid Id, CancellationToken token) where TEntity : EntityBase
    {
        var response = new Response<int>();
        var dbEntity = await context.Set<TEntity>().FindAsync(new object[] { Id }, cancellationToken: token).ConfigureAwait(false);
        if (dbEntity == null)
        {
            response.Results.Add(new()
            {
                Message = "Entity not found",
                Severity = Severity.Error
            });
            response.Value = -1;
            return response;
        }

        dbEntity.IsActive = false;
        var changes = await context.SaveChangesAsync(token).ConfigureAwait(false);
        if (changes <= 0)
        {
            response.Value = changes;
            response.Results.Add(new()
            {
                Message = "Error occurred during savechanges!",
                Severity = Severity.Error
            });
            return response;
        }

        response.Results.Add(new()
        {
            Message = "Successfully deleted",
            Severity = Severity.Info
        });

        response.Value = changes;
        return response;
    }

    public static async Task<Response<TEntity>> FindEntityAsync<TEntity>(this DbContext context, Guid Id, CancellationToken token) where TEntity : EntityBase
    {
        var response = new Response<TEntity>();
        var dbEntity = await context.Set<TEntity>()
            .Where(x => x.Id == Id)
            .Include(x => x.CreatorUser)
            .Include(x => x.UpdateUser)
            .FirstOrDefaultAsync(token)
            .ConfigureAwait(false);

        if (dbEntity == null)
        {
            response.Results.Add(new()
            {
                Message = "Entity not found",
                Severity = Severity.Error
            });
            return response;
        }

        response.Value = dbEntity;
        return response;
    }

    public static async Task<Response<IEnumerable<TEntity>>> WhereAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate, CancellationToken token, params object[] includes) where TEntity : EntityBase
    {
        var response = new Response<IEnumerable<TEntity>>();
        var entities = await context.Set<TEntity>()
            .Where(predicate)
            .Include(x => x.CreatorUser)
            .Include(x => x.UpdateUser)
            .ToListAsync(token)
            .ConfigureAwait(false);

        if (entities == null || entities.Count == 0)
        {
            response.Value = Enumerable.Empty<TEntity>();
            response.Results.Add(new()
            {
                Message = "No entity is found!",
                Severity = Severity.Info
            });
            return response;
        }

        response.Value = entities;
        return response;
    }

    //TODO: Async
    public static Response<IEnumerable<TEntity>> FilterEntity<TEntity>(this DbContext context, Predicate<TEntity> predicate, CancellationToken token, IEnumerable<string> includes = null) where TEntity : EntityBase
    {
        var response = new Response<IEnumerable<TEntity>>();
        var query = context.Set<TEntity>()
            .Include(x => x.CreatorUser)
            .Include(x => x.UpdateUser)
            .AsQueryable();

        foreach (var item in includes ?? Enumerable.Empty<string>())
        {
            query = query.Include(item);
        }

        //TODO: custom filtering
        var entities = query
            .AsEnumerable()
            .Where(x => ActivePredicate(x).Invoke(x) && (UserFilterPredicate(x, string.Empty).Invoke(x) || predicate.Invoke(x)))
            //.Skip(model.Start)
            //.Take(model.Length)
            .ToList();

        if (entities == null || entities.Count() == 0)
        {
            response.Value = new List<TEntity>();
            response.Results.Add(new()
            {
                Message = "Entity not found",
                Severity = Severity.Error
            });
            return response;
        }
        response.Value = entities;
        return response;
    }

    private static Predicate<TEntity> UserFilterPredicate<TEntity>(TEntity entity, string searchValue) where TEntity : EntityBase
    {
        Predicate<TEntity> creatorUserPredicate = (x) =>
        {
            return x.CreatorUser.FullName.ContainsLoweredTR(searchValue);
        };

        if (entity.UpdateUser != null)
        {
            Predicate<TEntity> updateUserPredicate = (x) =>
            {
                return x.UpdateUser.FullName.ContainsLoweredTR(searchValue);
            };

            return (x) => creatorUserPredicate(x) || updateUserPredicate(x);
        }

        return creatorUserPredicate;
    }

    private static Predicate<TEntity> ActivePredicate<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        Predicate<TEntity> isActivePredicate = (x) =>
        {
            return x.IsActive;
        };

        return isActivePredicate;
    }
}