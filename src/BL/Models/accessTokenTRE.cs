using BL.Models.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BL.Models
{
    public class accessTokenTRE
    {
        public int Id { get; set; }
        public string? accessToken { get; set; }
        public DateTime expirationDate { get; set; }
    }
}
