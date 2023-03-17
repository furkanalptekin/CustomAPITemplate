using CustomAPITemplate.Contract.V1.Response;

namespace CustomAPITemplate.Contract.V1;

public interface IAuditResponseBase<TKey> : IEntityResponseBase<TKey>
{
    public DateTime CreationTime { get; set; }
    public DateTime? UpdateTime { get; set; }

    public AppUserResponse CreatorUser { get; set; }
    public AppUserResponse UpdateUser { get; set; }
}