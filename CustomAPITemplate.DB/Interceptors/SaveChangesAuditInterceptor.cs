using CustomAPITemplate.DB.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CustomAPITemplate.DB.Interceptors;
public class SaveChangesAuditInterceptor : SaveChangesInterceptor
{
    private static readonly Type _auditEntityBaseType = typeof(IAuditEntityBase);

    private readonly IHttpContextAccessor _httpContextAccessor;
    public SaveChangesAuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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

    //TODO: return error on invalid user
    private void InjectAuditData(DbContextEventData eventData)
    {
        var entries = eventData.Context.ChangeTracker
            .Entries()
            .Where(x => x.State is EntityState.Modified or EntityState.Added && x.Entity != null && x.Entity.GetType().IsAssignableTo(_auditEntityBaseType));

        foreach (var entry in entries)
        {
            var auditEntity = (IAuditEntityBase)entry.Entity;
            var userIdString = GetUserId();
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();

            _ = Guid.TryParse(userIdString, out var userId);

            if (entry.State == EntityState.Added)
            {
                auditEntity.CreationTime = DateTime.UtcNow;
                auditEntity.CreatorUserId = userId;
                auditEntity.HostIP = ipAddress;
                auditEntity.IsActive = true;
                continue;
            }

            auditEntity.UpdateTime = DateTime.UtcNow;
            auditEntity.UpdateHostIP = ipAddress;
            auditEntity.UpdateUserId = userId;
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