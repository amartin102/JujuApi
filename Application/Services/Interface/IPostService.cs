using Application.Common.GenericResponse;
using Application.Dtos.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface IPostService
    {
        Task<GenericResponse<List<GetPostDto>>> GetByCustomerId(int customerId);

        Task<GenericResponse<GetPostDto?>> GetById(int id);

        Task<GenericResponse<IQueryable<GetPostDto>>> GetAll();
        Task<GenericResponse<GetPostDto>> Create(CreatePostDto post);

        Task<GenericResponse<(GetPostDto? updatedPost, bool changed)>> Update(UpdatePostDto post);

        Task<GenericResponse<bool>> Delete(int id);

        Task<GenericResponse<bool>> DeleteAll(int customerId);
    }
}
