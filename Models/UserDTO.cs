using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ITTP23.Models
{
    [SwaggerSchema(Description = "User Data Transfer Object model")]
    public class UserDTO
    {
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
    }
}
