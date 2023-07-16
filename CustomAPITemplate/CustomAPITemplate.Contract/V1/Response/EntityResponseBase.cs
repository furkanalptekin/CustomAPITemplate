namespace CustomAPITemplate.Contract.V1.Response;

public class EntityResponseBase<TKey> : IEntityResponseBase<TKey>
{
    public TKey Id { get; set; }

    public bool IsActive { get; set; }
}