using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Post
{
    public class GetPostDto
    {
        public int PostId { get; set; }

        public string? Title { get; set; }

        public string? Body { get; set; }

        public int Type { get; set; }

        public string? Category { get; set; }

        public int CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public bool State { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
