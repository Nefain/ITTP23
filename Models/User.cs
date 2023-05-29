using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ITTP23.Models
{
    [SwaggerSchema(Description = "User model")]
    public class User
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Gender { get; set; }

        [MaybeNull]
        public DateTime? Birthday { get; set; }

        [Required]
        public bool Admin { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime ModifiedOn { get; set;}

        [Required]
        public string ModifiedBy { get; set;}

        [MaybeNull]
        public DateTime? RevokedOn { get; set; } = null;

        [Required]
        public string RevokedBy { get; set; } = String.Empty;

        [MaybeNull]
        public string? Token { get; set; } = null;
    }
}
