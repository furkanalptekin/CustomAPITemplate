using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Extensions;

public static class EntityBaseExtensions
{
    public static Response InjectData<TKey, TEntity>(this TEntity entity, ControllerBase controller, bool isCreation = true) 
        where TEntity: IEntityBase<TKey>
    {
        var response = new Response();
        if (entity == null)
        {
            response.Results.Add(new()
            {
                Message = "entity is null",
                Severity = Severity.Error
            });
            return response;
        }

        if (!typeof(TEntity).IsAssignableTo(typeof(IAuditEntityBase<TKey>)))
        {
            return response;
        }

        var auditEntity = entity as IAuditEntityBase<TKey>;

        var userIdClaim = controller.HttpContext.GetUserId();
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            response.Results.Add(new()
            {
                Message = "UserId is null",
                Severity = Severity.Error
            });
            return response;
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            response.Results.Add(new()
            {
                Message = "UserId is not valid guid",
                Severity = Severity.Error
            });
            return response;
        }

        var ipAdress = controller.HttpContext.GetIPAdress();

        if (isCreation)
        {
            auditEntity.CreationTime = DateTime.UtcNow;
            auditEntity.CreatorUserId = userId;
            auditEntity.HostIP = ipAdress;
            auditEntity.IsActive = true;

            return response;
        }

        auditEntity.UpdateTime = DateTime.UtcNow;
        auditEntity.UpdateHostIP = ipAdress;
        auditEntity.UpdateUserId = userId;

        return response;
    }
}
