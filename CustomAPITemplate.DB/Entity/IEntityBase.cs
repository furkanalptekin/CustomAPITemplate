namespace CustomAPITemplate.DB.Entity;

public interface IEntityBase<TKey> : IEntityBase
{
    public TKey Id { get; set; }
}

public interface IEntityBase
{
    public bool IsActive { get; set; }
}