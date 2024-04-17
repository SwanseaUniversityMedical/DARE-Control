using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BL.Models.ViewModels
{
    public class AddiSubmissionWizard
    {
        public int ProjectId { get; set; }

        [Display(Name = "TES Name")]
        public string TESName { get; set; }

        [Display(Name = "TES Description")]
        public string TESDescription { get; set; }

        [Display(Name = "Submitting to Project: ")]
        public string ProjectName { get; set; }

        [Display(Name = "Select TREs")]
        public List<string>? Tres { get; set; }

        public List<TreInfo>? TreRadios { get; set; }

        public List<string>? SelectTresOptions { get; set; }
        [Display(Name = "Select TREs or leave blank for all")]
        public string SelectedTres { get; set; }
        public List<Executors>? Executors { get; set; }
        public string? RawInput { get; set; }

        public string? Query { get; set; }

    }
    public class Executors
    {
        public string Image { get; set; }
        public string Env { get; set; }
    }
}
