﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class SubmissionList
    {
        public List<Submission> Submissions { get; set; }
        public bool UseParent { get; set; }
    }
}
