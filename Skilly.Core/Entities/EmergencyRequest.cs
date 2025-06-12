using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class EmergencyRequest
    {
        public string Id { get; set; }= Guid.NewGuid().ToString();
        [ForeignKey("User")]
        public string UserId { get; set; }
        [NotMapped]
        public string UserName { get; set; }
        [JsonIgnore]
        public virtual User? User { get; set; }
        [ForeignKey("Category")]
        public string CategoryId { get; set; }
        [NotMapped]
        public string CategoryName { get; set; } 
        [JsonIgnore]
        public virtual Category? Category { get; set; }
        public string ProblemDescription { get; set; }
        public DateOnly RequestTime { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string Status { get; set; } = "Pending";


        [ForeignKey("AssignedProviderId")]
        public string? AssignedProviderId { get; set; }
        [JsonIgnore]
        public virtual User? AssignedProvider { get; set; }
        [NotMapped]
        public string AssignedProviderName { get; set; }
        public decimal? Finalprice { get; set; } = 0.0m;




    }
}
