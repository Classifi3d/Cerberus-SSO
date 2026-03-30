using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Client;

[Index(nameof(ClientId), IsUnique = true)]
[Table("Client")]
public class Client : BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    //public string AllowedScopes { get; set; } = string.Empty;
}