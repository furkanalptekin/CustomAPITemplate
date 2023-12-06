using CustomAPITemplate.DB.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CustomAPITemplate.DB.Interceptors;

public class SaveChangesAuditInterceptor(IHttpContextAccessor _httpContextAccessor) : SaveChangesInterceptor
{
    private static readonly Type s_entityBaseType = typeof(IEntityBase);

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        InjectAuditData(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        InjectAuditData(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void InjectAuditData(DbContextEventData eventData)
    {
        var entries = eventData.Context.ChangeTracker
            .Entries()
            .Where(x => x.State is EntityState.Modified or EntityState.Added && x.Entity != null && x.Entity.GetType().IsAssignableTo(s_entityBaseType));

        foreach (var entry in entries)
        {
            var entryEntityBase = (IEntityBase)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entryEntityBase.IsActive = true;
            }

            if (entry.Entity is not IAuditEntityBase auditEntityBase)
            {
                continue;
            }

            var userIdString = GetUserId();
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();

            _ = Guid.TryParse(userIdString, out var userId);

            if (entry.State == EntityState.Added)
            {
                auditEntityBase.CreationTime = DateTime.UtcNow;
                auditEntityBase.CreatorUserId = userId;
                auditEntityBase.HostIP = ipAddress;
                continue;
            }

            auditEntityBase.UpdateTime = DateTime.UtcNow;
            auditEntityBase.UpdateHostIP = ipAddress;
            auditEntityBase.UpdateUserId = userId;
        }
    }

    private string GetUserId()
    {
        if (_httpContextAccessor.HttpContext?.User == null)
        {
            return null;
        }

        return _httpContextAccessor.HttpContext.User.Claims.Single(x => x.Type == "id").Value;
    }
}