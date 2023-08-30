using System.ComponentModel.DataAnnotations.Schema;

namespace BL.Models
{
    public class BaseModel
    {
        [NotMapped]
        public bool Error { get; set; }
        [NotMapped]

        public string? ErrorMessage { get; set; }
    }

    
}
