using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public enum TypeCategory
    {
        Farandula = 1,
        Politica = 2,
        Futbol = 3
    }

    public enum HttpStatusCode
    {
        OK = 200,
        Created = 201,
        NoContent = 204,

        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,

        InternalServerError = 500
    }

}
