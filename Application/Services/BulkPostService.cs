using Application.Common;
using Application.Common.GenericMethods;
using Application.Dtos.Post;
using Application.Services.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repository.Logger;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Application.Services
{
    public class BulkPostService: IBulkPostService
    {
        private readonly string _connectionString;
        private readonly ILogInterface _logService;
        private readonly GenericMethod _genericMethod;

        public BulkPostService(IConfiguration configuration, ILogInterface logService)
        {
            _connectionString = configuration.GetConnectionString("SqlServerConnectionString");
            _logService = logService;
            _genericMethod = new GenericMethod();
        }

        public async Task BulkInsertAsync(List<CreatePostDto> posts)
        {
            _genericMethod.Validate(posts);

            var table = BuildDataTable(posts);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock,transaction)
                {
                    DestinationTableName = "dbo.Post",
                    BatchSize = 500,
                    BulkCopyTimeout = 60
                };

                bulkCopy.ColumnMappings.Add("Title", "Title");
                bulkCopy.ColumnMappings.Add("Body", "Body");
                bulkCopy.ColumnMappings.Add("Type", "Type");
                bulkCopy.ColumnMappings.Add("Category", "Category");
                bulkCopy.ColumnMappings.Add("CustomerId", "CustomerId");
                bulkCopy.ColumnMappings.Add("State", "State");
                bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");
                bulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");

                await bulkCopy.WriteToServerAsync(table);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await _logService.LogAsync("Error", $"ServiceException en BulkInsertAsync.", ex.Message, ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        private DataTable BuildDataTable(List<CreatePostDto> posts)
        {
            var table = new DataTable();

            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("Body", typeof(string));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("CustomerId", typeof(int));
            table.Columns.Add("State", typeof(bool));
            table.Columns.Add("CreatedAt", typeof(DateTime));
            table.Columns.Add("CreatedBy", typeof(string));

            foreach (var post in posts)
            {
                table.Rows.Add(
                    post.Title,
                    _genericMethod.FormatBodyPreview(post.Body),
                    post.Type,
                    _genericMethod.GetCategory(post.Type, post.Category),
                    post.CustomerId,
                    post.State,
                    DateTime.UtcNow,
                    "System"
                );
            }

            return table;
        }
    }
}
