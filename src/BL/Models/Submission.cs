using BL.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class Submission : BaseModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string? TesId { get; set; }
        public string SourceCrate { get; set; }

        public string TesName { get; set; }
        public string? TesJson { get; set; }
        public string DockerInputLocation { get; set; }

        public virtual Project Project { get; set; }

        [ForeignKey("ParentID")]
        public virtual Submission? Parent { get; set; }

        public virtual List<Submission> Children { get; set; }
        public virtual List<HistoricStatus> HistoricStatuses { get; set; }



        public virtual Endpoint? EndPoint { get; set; }

        public virtual User SubmittedBy { get; set; }

        public DateTime LastStatusUpdate { get; set; }
        


        public StatusType Status { get; set; }


        public string? StatusDescription { get; set; }





    }

  
}
