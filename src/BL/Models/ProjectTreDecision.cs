using BL.Models.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class ProjectTreDecision
    {
        public int Id { get; set; }
        public virtual Project? SubmissionProj { get; set; }

        public virtual Tre? Tre { get; set; }

        public Decision Decision { get; set; }

    }
}
