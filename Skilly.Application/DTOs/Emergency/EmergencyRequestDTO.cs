using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Emergency
{
    public class EmergencyRequestDTO
    {
        
        public string CategoryId { get; set; }
        public string ProblemDescription { get; set; }
    }
}
