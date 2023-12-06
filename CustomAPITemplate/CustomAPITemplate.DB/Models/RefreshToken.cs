using System.ComponentModel.DataAnnotations.Schema;
using CustomAPITemplate.DB.Entity;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Models;

[Index(nameof(UserId))]
public class RefreshToken : EntityBase<Guid>
{
    public string JwtId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool Invalidated { get; set; }
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; }
}