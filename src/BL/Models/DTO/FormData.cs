using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BL.Models.DTO
{
    /// <summary>
    /// FormData is a repository for the JSON data submitted from a FormIo Form.
    /// </summary>
    public class FormData
    {
        public int Id { get; set; }
        public string FormIoUrl { get; set; }
        public string? FormIoString { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
               
    }
}