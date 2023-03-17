namespace CustomAPITemplate.Contract.V1.Response;

public interface IEntityResponseBase<TKey>
{
    public TKey Id { get; set; }

    public bool IsActive { get; set; }
}