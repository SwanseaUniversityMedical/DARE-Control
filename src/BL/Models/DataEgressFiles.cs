﻿using BL.Models.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class DataEgressFiles
    {
        public int Id { get; set; }
        public int submissionId { get; set; }
        public string? FileSize { get; set; }
        public virtual List<Files>? files { get; set; }
        public string? FileStatus { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public byte[]? FileData { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? Reviewer { get; set; }
    }
}
