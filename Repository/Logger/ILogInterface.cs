using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Logger
{
    public interface ILogInterface
    {
        Task LogAsync(string level, string messageTemplate, string message, Exception? ex = null, object? properties = null);
    }
}
