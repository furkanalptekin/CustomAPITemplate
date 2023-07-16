using System.Linq;
using System.Linq.Expressions;
using CustomAPITemplate.Core;
using CustomAPITemplate.Core.Extensions;
using CustomAPITemplate.DB.Entity;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Extensions;

public static class DbContextExtensions
{
    public static async Task<Response<TEntity>> AddEntityAsync<TKey, TEntity>(this DbContext context, TEntity entity, CancellationToken token) where TEntity : EntityBase<TKey>
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

    public static async Task<Response<int>> UpdateEntityAsync<TKey, TEntity>(this DbContext context, TKey id, TEntity entity, CancellationToken token, params string[] propertiesToIgnore) where TEntity : EntityBase<TKey>
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

    public static async Task<Response<int>> RemoveEntityAsync<TKey, TEntity>(this DbContext context, TKey id, CancellationToken token) where TEntity : EntityBase<TKey>
    {
        var response = new Response<int>();
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

    public static async Task<Response<TEntity>> FindEntityAsync<TKey, TEntity>(this DbContext context, TKey id, CancellationToken token) where TEntity : EntityBase<TKey>
    {
        var response = new Response<TEntity>();
        var dbEntity = await context.Set<TEntity>()
            .Where(x => x.Id.Equals(id))
            .IncludeUsersIfAuditEntity<TKey, TEntity>()
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

    public static async Task<Response<IEnumerable<TEntity>>> WhereAsync<TKey, TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate, CancellationToken token, params string[] includes) where TEntity : EntityBase<TKey>
    {
        var response = new Response<IEnumerable<TEntity>>();
        var query = context.Set<TEntity>()
            .Where(predicate)
            .IncludeUsersIfAuditEntity<TKey, TEntity>();

        foreach (var item in includes ?? Array.Empty<string>())
        {
            query = query.Include(item);
        }

        var entities = await query.ToListAsync().ConfigureAwait(false);

        if (entities == null || entities.Count == 0)
        {
            response.Value = Enumerable.Empty<TEntity>();
            response.Results.Add(new()
            {
                Message = "Entity not found",
                Severity = Severity.Info
            });
            return response;
        }

        response.Value = entities;
        return response;
    }

    private static Predicate<TEntity> UserFilterPredicate<TKey, TEntity>(TEntity entity, string searchValue) where TEntity : AuditEntityBase<TKey>
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

    private static Predicate<TEntity> ActivePredicate<TKey, TEntity>() where TEntity : EntityBase<TKey>
    {
        Predicate<TEntity> isActivePredicate = (x) =>
        {
            return x.IsActive;
        };

        return isActivePredicate;
    }

    private static IQueryable<TEntity> IncludeUsersIfAuditEntity<TKey, TEntity>(this IQueryable<TEntity> queryable) where TEntity : EntityBase<TKey>
    {
        if (typeof(TEntity).IsAssignableTo(typeof(IAuditEntityBase<TKey>)))
        {
            return queryable
                .Include("CreatorUser")
                .Include("UpdateUser");
        }

        return queryable;
    }
}