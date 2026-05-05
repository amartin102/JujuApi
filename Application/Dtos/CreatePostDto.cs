using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class CreatePostDto
    {
        public string? Title { get; set; }

        public string? Body { get; set; }

        public int Type { get; set; }

        public string? Category { get; set; }

        public int CustomerId { get; set; }

        public bool State { get; set; }

    }
}
