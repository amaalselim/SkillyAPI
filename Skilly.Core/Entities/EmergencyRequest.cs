using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class EmergencyRequest
    {
        public string Id { get; set; }= Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string CategoryId { get; set; }
        public string ProblemDescription { get; set; }
        public DateOnly RequestTime { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string Status { get; set; } = "Pending";

        public string? AssignedProviderId { get; set; }
        



    }
}
