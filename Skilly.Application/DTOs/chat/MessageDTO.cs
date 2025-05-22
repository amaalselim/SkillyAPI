using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class MessageDTO
    {
        public string receiverId {  get; set; } 
        public string? content { get; set; }
        public IFormFile? Img { get; set; }
        [JsonIgnore]
        public string? imageUrl { get; set; }

    }
}
