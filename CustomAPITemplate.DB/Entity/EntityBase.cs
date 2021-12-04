using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CustomAPITemplate.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Entity;

[Index(nameof(IsActive))]
public class EntityBase : IEntityBase
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? CreatorUserId { get; set; }
    public string HostIP { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid? UpdateUserId { get; set; }
    public string UpdateHostIP { get; set; }
    public bool IsActive { get; set; }


    [ForeignKey(nameof(CreatorUserId))]
    public virtual AppUser CreatorUser { get; set; }

    [ForeignKey(nameof(UpdateUserId))]
    public virtual AppUser UpdateUser { get; set; }
}