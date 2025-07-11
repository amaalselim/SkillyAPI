﻿using Microsoft.AspNetCore.Http;
using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Payment
{
    public class ServiceProviderDTO
    {
        public string Governorate { get; set; }
        public string City { get; set; }
        public string StreetName { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public IFormFile Img { get; set; }
        public int NumberOfYearExperience { get; set; }
        public string BriefSummary { get; set; }
        public IFormFile NationalNumberPDF { get; set; }
        public string categoryId { get; set; }
    }
}
