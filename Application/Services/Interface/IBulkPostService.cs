using Application.Dtos.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface IBulkPostService
    {
        Task BulkInsertAsync(List<CreatePostDto> posts);
    }
}
