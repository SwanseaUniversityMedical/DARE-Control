
namespace BL.Models
{
    public class TreAuditLog : BaseModel
    {
        public int Id { get; set; }
        public string? Decision { get; set; }
        public string? ApprovedBy { get; set; }
        public string? IPaddress { get; set; }
        public DateTime Date { get; set; }

    }

}
