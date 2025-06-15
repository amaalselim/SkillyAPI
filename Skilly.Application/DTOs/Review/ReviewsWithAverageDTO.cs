using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Review
{
    public class ReviewsWithAverageDTO
    {
        public decimal AverageRating { get; set; }
        public List<ReviewserviceDisplayDTO> Reviews { get; set; }
    }

}
