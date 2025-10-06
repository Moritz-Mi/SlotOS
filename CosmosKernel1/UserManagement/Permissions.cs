using System;

namespace CosmosKernel1.UserManagement
{
    /// <summary>
    /// Definiert die verschiedenen Berechtigungen im System
    /// </summary>
    [Flags]
    public enum Permission
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4,
        Delete = 8,
        CreateUser = 16,
        DeleteUser = 32,
        ModifyPermissions = 64,
        SystemAdmin = 128,
        ReadWrite = Read | Write,
        FullControl = Read | Write | Execute | Delete,
        Administrator = Read | Write | Execute | Delete | CreateUser | DeleteUser | ModifyPermissions | SystemAdmin
    }

    /// <summary>
    /// Benutzerrollen für einfachere Rechteverwaltung
    /// </summary>
    public enum UserRole
    {
        Guest,          // Minimale Rechte, nur Lesen
        User,           // Normale Benutzerrechte
        PowerUser,      // Erweiterte Rechte
        Administrator   // Volle Systemrechte
    }

    /// <summary>
    /// Hilfsklasse für Permission-Operationen
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// Konvertiert eine UserRole in entsprechende Permissions
        /// </summary>
        public static Permission RoleToPermission(UserRole role)
        {
            switch (role)
            {
                case UserRole.Guest:
                    return Permission.Read;
                case UserRole.User:
                    return Permission.ReadWrite | Permission.Execute;
                case UserRole.PowerUser:
                    return Permission.FullControl;
                case UserRole.Administrator:
                    return Permission.Administrator;
                default:
                    return Permission.None;
            }
        }

        /// <summary>
        /// Prüft ob eine Permission eine bestimmte Berechtigung enthält
        /// </summary>
        public static bool HasPermission(Permission userPermissions, Permission requiredPermission)
        {
            return (userPermissions & requiredPermission) == requiredPermission;
        }
    }
}
