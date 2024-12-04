using Microsoft.AspNetCore.Http;
using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class ServiceProviderDTO
    {
        public string Governorate { get; set; }
        public string City { get; set; }
        public string StreetName { get; set; }
        public string Age { get; set; }
        public Gender Gender { get; set; }
        public IFormFile Img { get; set; }
        public string profession { get; set; }
        public string NumberOfYearExperience { get; set; }
        public string BriefSummary { get; set; }
        public IFormFile NationalNumberPDF { get; set; }
    }
}
