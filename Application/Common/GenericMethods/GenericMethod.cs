using Application.Dtos.Post;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.GenericMethods
{
    public class GenericMethod
    {
        private const int MAX_POSTS = 1000;
   
        public string GetCategory(int typeId, string userCategory)
        {
            return typeId switch
            {
                (int)TypeCategory.Farandula => "Farándula",
                (int)TypeCategory.Politica => "Política",
                (int)TypeCategory.Futbol => "Futbol",
                _ => string.IsNullOrWhiteSpace(userCategory) ? "General" : userCategory
            };
        }

        public void Validate(List<CreatePostDto> posts)
        {
            if (posts == null || !posts.Any())
                throw new ArgumentException("La lista está vacía");

            if (posts.Count > MAX_POSTS)
                throw new ArgumentException($"Máximo permitido: {MAX_POSTS}");

            if (posts.Any(p => string.IsNullOrWhiteSpace(p.Title)))
                throw new ArgumentException("Todos los posts deben tener título");           
        }
              
        // Si el texto del Body es mayor a 20 caracteres se corta a 97 caracteres y se agrega "..."
        //Pero sólo si llega a los 97 caracteres, ahí es donde lo corta
        public string FormatBodyPreview(string? body)
        {
            if (string.IsNullOrEmpty(body))
                return body ?? string.Empty;

            if (body.Length <= 97)
                return body;

            var length = Math.Min(97, body.Length);
            return body.Substring(0, length) + "...";
        }
    }
}
