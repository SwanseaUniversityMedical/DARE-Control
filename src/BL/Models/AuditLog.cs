
namespace BL.Models
{
    public class AuditLog : BaseModel
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Module { get; set; }
        public string AuditValues { get; set; }
        public string UserName { get; set; }
        public string FormData { get; set; }
        public DateTime Date { get; set; }

    }

}
