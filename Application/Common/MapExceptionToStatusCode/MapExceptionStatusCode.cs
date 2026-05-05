using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.MapExceptionToStatusCode
{
    public class MapExceptionStatusCode
    {
        public static int GetStatusCode(Exception ex)
        {
            // DbUpdateException con SqlException interno (EF Core -> SQL Server)
            if (ex is DbUpdateException dbUpd && dbUpd.InnerException is SqlException innerSql)
            {
                if (innerSql.Number == 547) // FK violation
                    return 409;
            }

            // Directo SqlException
            if (ex is SqlException sqlEx)
            {
                if (sqlEx.Number == 547)
                    return 409;
            }

            var text = (ex.InnerException?.Message ?? ex.Message ?? string.Empty).ToLowerInvariant();

            if (text.Contains("foreign key") || text.Contains("fk_") || text.Contains("conflict") || text.Contains("the insert statement conflicted"))
                return 409;

            if (text.Contains("validation") || text.Contains("invalid") || text.Contains("required") || text.Contains("bad request") || text.Contains("constraint"))
                return 400;

            return 500;
        }
    }
}
