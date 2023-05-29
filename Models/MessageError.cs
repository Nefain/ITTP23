using Swashbuckle.AspNetCore.Annotations;

namespace ITTP23.Models
{
    [SwaggerSchema(Description = "Error message model")]
    public class MessageError
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}
