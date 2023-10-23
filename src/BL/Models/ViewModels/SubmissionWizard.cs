﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public enum CrateOrigin
    {
        
        [Display(Name = "Upload to project bucket")]
        FileUpload = 0,
            [Display(Name = "External URL")]
        External = 1,
    }

    public class SubmissionWizard
    {
        public int ProjectId { get; set; }
        public string TESName { get; set; }

        [Display(Name = "Submitting to Project: ")]
        public string ProjectName { get; set; }

        [Display(Name = "Select TREs or leave blank for all")]
        public List<string>? Tres { get; set; }

        public List<string>? SelectTresOptions { get; set; }
        [Display(Name = "Select TREs or leave blank for all")]
        public string SelectedTres { get; set; }

        [Display(Name = "Select external URL or upload to project bucket")]
        public CrateOrigin OriginOption { get; set; }

        [Display(Name = "External URL")]
        public string? ExternalURL { get; set; }

        [Display(Name = "Select file to upload to bucket")]
        public IFormFile? File { get; set; }
    }
}