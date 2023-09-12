
namespace BL.Models
{
    public class AuditLog : BaseModel
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public int? TreId { get; set; }
        public int? TestaskId { get; set; }
        public string? UserName { get; set; }
        public string? FormData { get; set; }
        public string? IPaddress { get; set; }
        public DateTime Date { get; set; }

    }

}
