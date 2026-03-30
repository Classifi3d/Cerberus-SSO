using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class BaseEntity
{
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    [ConcurrencyCheck]
    public ulong ConcurrencyIndex { get; set; } = 1;
}
