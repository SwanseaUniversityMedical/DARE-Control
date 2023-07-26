using BL.Models.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BL.Models
{
    public class ProjectApproval
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public string? Projectname { get; set; }
        public string? Username { get; set; }
     
        public string? LocalProjectName { get; set; }
        public string? Approved { get; set; } 
        public string? ApprovedBy { get; set; } 
        public DateTime Date { get; set; }
    }
}
