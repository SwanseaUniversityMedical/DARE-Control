using BL.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services
{
    public class MinioSubHelper: MinioHelper, IMinioSubHelper
    {
        public MinioSubHelper(MinioSubSettings settings): base(settings) { 
        }
    }
}
