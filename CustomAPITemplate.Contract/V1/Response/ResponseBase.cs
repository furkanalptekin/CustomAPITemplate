namespace CustomAPITemplate.Contract.V1;

public class ResponseBase : IResponseBase
{
    public Guid Id { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid CreatorUserId { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid? UpdateUserId { get; set; }
    public string UpdateHostIP { get; set; }
    public bool IsActive { get; set; }

    public virtual AppUserResponse CreatorUser { get; set; }
    public virtual AppUserResponse UpdateUser { get; set; }
}