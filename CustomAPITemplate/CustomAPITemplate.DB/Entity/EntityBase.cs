using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CustomAPITemplate.DB.Entity;

[Index(nameof(IsActive))]
public class EntityBase<TKey> : IEntityBase<TKey>
{
    [Key]
    public TKey Id { get; set; }

    public bool IsActive { get; set; }
}