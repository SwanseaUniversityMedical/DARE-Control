﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class MQFetchFile
    {
        public string Url { get; set; }
        public string BucketName { get; set; }
        public string? Key { get; set; }
    }
}