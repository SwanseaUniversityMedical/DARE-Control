using BL.Models.Enums;
using BL.Models.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using BL.Models.Helpers;

namespace BL.Models
{
    public class SearchResult : BaseModel
    {
   
     public string? Title { get; set; }
     public string? Description { get; set; }

    }


}

