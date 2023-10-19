using BL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class Stages
    {
        public List<StageInfo> StageInfos { get; set; }

        public List<StatusType> GreenStages { get; set; }
        public List<StatusType> RedStages { get; set; }

        public bool IsRed(StatusType statusType)
        {
            return RedStages.Contains(statusType);
        }

        public bool IsGreen(StatusType statusType)
        {
            return GreenStages.Contains(statusType);
        }
    }
}
