using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class ServiceProvider
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Governorate { get; set; }
        public string City { get; set; }
        public string StreetName { get; set; }
        public string Age { get; set; }
        public Gender Gender { get; set; }
        public string Img { get; set; }
        public string profession {  get; set; }
        public string NumberOfYearExperience { get; set; }
        public string BriefSummary { get; set; }
        public string NationalNumberPDF { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        [ForeignKey("Category")]
        public string categoryId { get; set; }
        [JsonIgnore]
        public virtual Category? Category { get; set; }
        [JsonIgnore]
        public ICollection<ProviderServices>? providerServices { get; set; } = new List<ProviderServices>();
        [JsonIgnore]

        public ICollection<Servicesgallery>? servicesgalleries { get; set; } = new List<Servicesgallery>();
        [JsonIgnore]

        public ICollection<Review>? Reviews { get; set; } = new List<Review>();

    }
}
