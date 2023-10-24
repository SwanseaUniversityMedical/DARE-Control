using BL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class StageInfo
    {
        public List<StatusType> statusTypeList;
        public string stageName;
        public int stageNumber;
        public Dictionary<int, List<StatusType>> stagesDict;
    }
}
