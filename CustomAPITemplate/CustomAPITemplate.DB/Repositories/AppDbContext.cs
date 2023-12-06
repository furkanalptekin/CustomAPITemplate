using CustomAPITemplate.DB.Interceptors;
using CustomAPITemplate.DB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Repositories;

public partial class AppDbContext(DbContextOptions<AppDbContext> _options, IHttpContextAccessor _httpContextAccessor)
    : IdentityDbContext<AppUser, AppRole, Guid>(_options)
{
    public virtual DbSet<RefreshToken> RefreshToken { get; set; }
    public virtual DbSet<Example> Example { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.AddInterceptors(new SaveChangesAuditInterceptor(_httpContextAccessor));
}