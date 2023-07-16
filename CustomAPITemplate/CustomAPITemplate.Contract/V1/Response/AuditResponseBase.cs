using CustomAPITemplate.Contract.V1.Response;

namespace CustomAPITemplate.Contract.V1;

public class AuditResponseBase<TKey> : EntityResponseBase<TKey>, IAuditResponseBase<TKey>
{
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    
    public virtual AppUserResponse CreatorUser { get; set; }
    public virtual AppUserResponse UpdateUser { get; set; }
}