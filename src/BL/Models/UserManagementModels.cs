using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool CanLogin { get; set; } = true;
        public bool CanCreateDb { get; set; } = false;
        public bool CanCreateRole { get; set; } = false;
        public List<SchemaPermission> SchemaPermissions { get; set; } = new List<SchemaPermission>();
    }

    public class SchemaPermission
    {
        public string SchemaName { get; set; }
        public DatabasePermissions Permissions { get; set; }
    }

    public class UserCreationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public static UserCreationResult Ok() => new() { Success = true };
        public static UserCreationResult Error(string errorMessage) => new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }

    [Flags]
    public enum DatabasePermissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        CreateTables = 4,
        DropTables = 8,
        All = Read | Write | CreateTables | DropTables
    }
}
