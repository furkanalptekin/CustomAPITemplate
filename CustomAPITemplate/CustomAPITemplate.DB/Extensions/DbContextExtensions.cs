using System.Linq.Expressions;
using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Extensions;

public static class DbContextExtensions
{
    public static async Task<Response<TEntity>> AddEntityAsync<TKey, TEntity>(this DbContext context, TEntity entity, CancellationToken token)
        where TEntity : EntityBase<TKey>
    {
        var response = new Response<TEntity>();
        if (entity == null)
        {
            return response.AddError("Entity is null");
        }

        await context.AddAsync(entity, token).ConfigureAwait(false);

        var changes = await context.SaveChangesAsync(token).ConfigureAwait(false);
        if (changes <= 0)
        {
            return response.AddError("Error occurred during savechanges!");
        }

        response.Value = entity;
        return response.AddInfo("Entity is successfully created!");
    }

    public static async Task<Response<int>> UpdateEntityAsync<TKey, TEntity>(this DbContext context, TKey id, TEntity entity, CancellationToken token, params string[] propertiesToIgnore)
        where TEntity : EntityBase<TKey>
    {
        var response = new Response<int>();
        if (entity == null)
        {
            response.Value = -1;
            return response.AddError("Entity is null");
        }

        var dbEntity = await context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken: token).ConfigureAwait(false);
        if (dbEntity == null)
        {
            response.Value = -1;
            return response.AddError("Entity not found");
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
            return response.AddError("An error occurred during savechanges!");
        }

        response.Value = changes;
        return response.AddInfo("Entity is successfully updated!");
    }

    public static async Task<Response<int>> RemoveEntityAsync<TKey, TEntity>(this DbContext context, TKey id, CancellationToken token)
        where TEntity : EntityBase<TKey>
    {
        var response = new Response<int>();
        var dbEntity = await context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken: token).ConfigureAwait(false);
        if (dbEntity == null)
        {
            response.Value = -1;
            return response.AddError("Entity not found");
        }

        dbEntity.IsActive = false;
        var changes = await context.SaveChangesAsync(token).ConfigureAwait(false);
        if (changes <= 0)
        {
            response.Value = changes;
            return response.AddError("Error occurred during savechanges!");
        }

        response.Value = changes;
        return response.AddInfo("Successfully deleted");
    }

    public static async Task<Response<TEntity>> FindEntityAsync<TKey, TEntity>(this DbContext context, TKey id, CancellationToken token)
        where TEntity : EntityBase<TKey>
    {
        var dbEntity = await context.Set<TEntity>()
            .Where(x => x.Id.Equals(id))
            .IncludeUsersIfAuditEntity<TKey, TEntity>()
            .FirstOrDefaultAsync(token)
            .ConfigureAwait(false);

        if (dbEntity == null)
        {
            return Result.Error("Entity not found");
        }

        return dbEntity;
    }

    public static async Task<Response<IEnumerable<TEntity>>> WhereAsync<TKey, TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate, CancellationToken token, params string[] includes)
        where TEntity : EntityBase<TKey>
    {
        var response = new Response<IEnumerable<TEntity>>();
        var query = context.Set<TEntity>()
            .Where(predicate)
            .IncludeUsersIfAuditEntity<TKey, TEntity>();

        foreach (var item in includes ?? [])
        {
            query = query.Include(item);
        }

        var entities = await query.ToListAsync(token).ConfigureAwait(false);
        if (entities == null || entities.Count == 0)
        {
            response.Value = [];
            return response.AddInfo("Entity not found");
        }

        response.Value = entities;
        return response;
    }

    private static Predicate<TEntity> UserFilterPredicate<TKey, TEntity>(TEntity entity, string searchValue)
        where TEntity : AuditEntityBase<TKey>
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

    private static Predicate<TEntity> ActivePredicate<TKey, TEntity>()
        where TEntity : EntityBase<TKey>
    {
        Predicate<TEntity> isActivePredicate = (x) =>
        {
            return x.IsActive;
        };

        return isActivePredicate;
    }

    private static IQueryable<TEntity> IncludeUsersIfAuditEntity<TKey, TEntity>(this IQueryable<TEntity> queryable)
        where TEntity : EntityBase<TKey>
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