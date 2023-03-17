namespace CustomAPITemplate.DB.Entity;

public interface IEntityBase<TKey>
{
    public TKey Id { get; set; }

    public bool IsActive { get; set; }
}