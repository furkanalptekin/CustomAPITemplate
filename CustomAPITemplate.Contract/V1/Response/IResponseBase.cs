namespace CustomAPITemplate.Contract.V1;

public interface IResponseBase
{
    public Guid Id { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid CreatorUserId { get; set; }
    public DateTime? UpdateTime { get; set; }
    public Guid? UpdateUserId { get; set; }
    public string UpdateHostIP { get; set; }
    public bool IsActive { get; set; }

    public AppUserResponse CreatorUser { get; set; }
    public AppUserResponse UpdateUser { get; set; }

}