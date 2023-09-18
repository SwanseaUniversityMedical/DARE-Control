using BL.Models.Enums;

namespace BL.Models
{
    public class HistoricStatus : BaseModel
    {
        
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        
        public virtual Submission Submission { get; set; }

       

        public StatusType Status { get; set; }

       
        public string? StatusDescription { get; set; }
        
        public string? LastHeartBeatReceived { get; set; }
    }

    
}
