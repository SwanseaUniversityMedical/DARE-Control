using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BL.Models
{
    /// <summary>
    /// FormData is a repository for the JSON data submitted from a FormIo Form
    /// </summary>
    public class FormData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FormIoUrl { get; set; }
        public string? FormIoString { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("Id")]
        public virtual Projects Project { get; set; }
        [ForeignKey("Id")]
        public virtual User User { get; set; }
    }
}