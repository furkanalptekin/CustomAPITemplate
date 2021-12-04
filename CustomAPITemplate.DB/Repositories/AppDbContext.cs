using CustomAPITemplate.DB.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CustomAPITemplate.DB.Repositories;

public partial class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    private readonly IConfiguration _configuration;
    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Example> Example { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //TODO: Secure connection string
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("NpgsqlConnection"));
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}