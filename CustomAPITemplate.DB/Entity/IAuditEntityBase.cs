using CustomAPITemplate.DB.Models;

namespace CustomAPITemplate.DB.Entity;
public interface IAuditEntityBase<TKey> : IEntityBase<TKey>
{
    public DateTime CreationTime { get; set; }
    public Guid? CreatorUserId { get; set; }
    public string HostIP { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid? UpdateUserId { get; set; }
    public string UpdateHostIP { get; set; }
    
    public AppUser CreatorUser { get; set; }
    public AppUser UpdateUser { get; set; }
}