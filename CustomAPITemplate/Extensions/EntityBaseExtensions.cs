using CustomAPITemplate.Core;
using CustomAPITemplate.DB.Entity;
using Microsoft.AspNetCore.Mvc;

namespace CustomAPITemplate.Extensions;

public static class EntityBaseExtensions
{
    public static Response InjectData<TEntity>(this TEntity entity, ControllerBase controller, bool isCreation = true) 
        where TEntity: IEntityBase
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
            entity.CreationTime = DateTime.UtcNow;
            entity.CreatorUserId = userId;
            entity.HostIP = ipAdress;
            entity.IsActive = true;

            return response;
        }

        entity.UpdateTime = DateTime.UtcNow;
        entity.UpdateHostIP = ipAdress;
        entity.UpdateUserId = userId;

        return response;
    }
}
