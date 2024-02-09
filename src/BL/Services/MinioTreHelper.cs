using BL.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services
{
    public class MinioTreHelper: MinioHelper, IMinioTreHelper
    {
        public MinioTreHelper(MinioTRESettings settings): base(settings) { 
        }
    }
}
