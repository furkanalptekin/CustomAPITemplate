using System.ComponentModel.DataAnnotations;

namespace CustomAPITemplate.DB.Entity;

public class FileEntityBase : EntityBase, IFileEntityBase
{
    [Display(Name = "File")]
    public string FilePath { get; set; }
}