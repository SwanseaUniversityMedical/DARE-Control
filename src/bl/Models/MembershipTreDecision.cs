﻿using BL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class MembershipTreDecision
    {
        public int Id { get; set; }
        public virtual Project? SubmissionProj { get; set; }

        public virtual User? User { get; set; }
        public virtual Tre? Tre { get; set; }

        public Decision Decision { get; set; }

    }
}
