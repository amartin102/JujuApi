using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Messages
{
    public class ErrorMessages
    {
        public static string NotFound(string entity)
            => $"{entity} no encontrado.";

        public static string NotFound(string entity, int id)
            => $"{entity} con id={id} no encontrado.";

        public static string Deleted(string entity)
            => $"{entity} eliminado correctamente.";

        public static string DeleteFailed(string entity, int id)
            => $"{entity} con id={id} no encontrado o no eliminado.";

        public static string Updated(string entity)
            => $"{entity} actualizado correctamente.";

        public static string NoChanges(string entity)
            => $"No se realizaron cambios en {entity}.";

        public static string Created(string entity)
            => $"{entity} creado correctamente.";

        public static string Invalid(string field)
            => $"{field} es inválido.";

        public static string Required(string field)
            => $"{field} es requerido.";

        public static string Error(string action, string entity)
            => $"Error al {action} {entity}.";

        public static string ErrorWithDetails(string action, string entity, Exception ex)
            => $"Error al {action} {entity}. Detalles: {ex.InnerException?.Message ?? ex.Message}";

        public static string AlreadyExists(string entity, string field)
            => $"{entity} con el mismo {field} ya existe.";

        public static string Success(string action, string entity)
            => $"{entity} obtenidos correctamente.";

        public static string DeleteFailedByParent(string entity, string parentEntity, int parentId)
            => $"{entity} para {parentEntity} con id={parentId} no encontrados o no eliminados.";

        public static string Error(string action, string entity, int? id = null)
            => id.HasValue
                ? $"Error al {action} {entity} con id={id}."
                : $"Error al {action} {entity}.";
    }
}
