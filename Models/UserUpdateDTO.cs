using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ITTP23.Models
{
    [SwaggerSchema(Description = "User Update Data Transfer Object model")]
    public class UserUpdateDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Gender { get; set; }

        [MaybeNull]
        public DateTime? Birthday { get; set; }

        [MaybeNull]
        public bool? IsActive { get; set; }
    }
}
