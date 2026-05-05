using Domain.Entities;
using Microsoft.Extensions.Logging;
using Repository.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Repository.Loggin
{
    public class LogService: ILogInterface
    {
        private readonly ILogger<LogService> _logger;
        private readonly JujuTestContext _context;

        public LogService(ILogger<LogService> logger, JujuTestContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task LogAsync(string level, string message, Exception? ex = null, object? properties = null)
        {
            try
            {
                if (ex != null)
                    _logger.LogError(ex, "{Message}", message);
                else
                    _logger.LogInformation("{Message}", message);
                              
                var log = new LogEntity
                {
                    Level = level,
                    Message = message,
                    Exception = ex?.ToString(),
                    Properties = properties is null ? null : JsonSerializer.Serialize(properties),
                    TimeStamp = DateTime.UtcNow
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            { }
        }
    }
}
