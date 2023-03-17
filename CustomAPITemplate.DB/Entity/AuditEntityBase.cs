using System.ComponentModel.DataAnnotations.Schema;
using CustomAPITemplate.DB.Models;

namespace CustomAPITemplate.DB.Entity;
public class AuditEntityBase<TKey> : EntityBase<TKey>, IAuditEntityBase<TKey>
{
    public DateTime CreationTime { get; set; }
    public Guid? CreatorUserId { get; set; }
    public string HostIP { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid? UpdateUserId { get; set; }
    public string UpdateHostIP { get; set; }

    [ForeignKey(nameof(CreatorUserId))]
    public virtual AppUser CreatorUser { get; set; }

    [ForeignKey(nameof(UpdateUserId))]
    public virtual AppUser UpdateUser { get; set; }
}