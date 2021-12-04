namespace CustomAPITemplate.DB.Entity;

public interface IFileEntityBase : IEntityBase
{
    public string FilePath { get; set; }
}