using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Category
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Img { get; set; }
        public string ProfessionName { get; set; }
        [JsonIgnore]
        public ICollection<ServiceProvider>? serviceProviders { get; set; } = new List<ServiceProvider>();
        [JsonIgnore]
        public ICollection<ProviderServices>? providerServices { get; set; } = new List<ProviderServices>();


    }
}
