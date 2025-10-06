using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmosKernel1.UserManagement
{
    /// <summary>
    /// Verwaltet Berechtigungen und Zugriffskontrolle
    /// </summary>
    public class PermissionManager
    {
        private UserDatabase userDatabase;
        private AuthenticationManager authManager;

        public PermissionManager(UserDatabase database, AuthenticationManager authentication)
        {
            userDatabase = database;
            authManager = authentication;
        }

        /// <summary>
        /// Prüft ob der aktuelle Benutzer eine Operation ausführen darf
        /// </summary>
        public bool CanPerformOperation(Permission requiredPermission)
        {
            if (!authManager.IsLoggedIn)
            {
                return false;
            }

            return authManager.CheckPermission(requiredPermission);
        }

        /// <summary>
        /// Prüft ob der aktuelle Benutzer Zugriff auf einen Benutzer hat
        /// </summary>
        public bool CanAccessUser(int targetUserId)
        {
            if (!authManager.IsLoggedIn)
            {
                return false;
            }

            var currentUser = authManager.CurrentUser;
            
            // Benutzer kann auf sich selbst zugreifen
            if (currentUser.UserId == targetUserId)
            {
                return true;
            }

            // Admin kann auf alle Benutzer zugreifen
            if (currentUser.HasPermission(Permission.SystemAdmin))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Fügt einem Benutzer zusätzliche Berechtigungen hinzu
        /// </summary>
        public bool GrantPermission(string username, Permission permission)
        {
            if (!authManager.CheckPermission(Permission.ModifyPermissions))
            {
                return false;
            }

            var targetUser = userDatabase.GetUserByUsername(username);
            if (targetUser == null)
            {
                return false;
            }

            targetUser.Permissions |= permission;
            return userDatabase.UpdateUser(targetUser);
        }

        /// <summary>
        /// Entzieht einem Benutzer bestimmte Berechtigungen
        /// </summary>
        public bool RevokePermission(string username, Permission permission)
        {
            if (!authManager.CheckPermission(Permission.ModifyPermissions))
            {
                return false;
            }

            var targetUser = userDatabase.GetUserByUsername(username);
            if (targetUser == null)
            {
                return false;
            }

            targetUser.Permissions &= ~permission;
            return userDatabase.UpdateUser(targetUser);
        }

        /// <summary>
        /// Ändert die Rolle eines Benutzers
        /// </summary>
        public bool ChangeUserRole(string username, UserRole newRole)
        {
            if (!authManager.CheckPermission(Permission.ModifyPermissions))
            {
                return false;
            }

            var targetUser = userDatabase.GetUserByUsername(username);
            if (targetUser == null)
            {
                return false;
            }

            targetUser.Role = newRole;
            targetUser.Permissions = PermissionHelper.RoleToPermission(newRole);
            return userDatabase.UpdateUser(targetUser);
        }

        /// <summary>
        /// Deaktiviert einen Benutzer
        /// </summary>
        public bool DeactivateUser(string username)
        {
            if (!authManager.CheckPermission(Permission.DeleteUser))
            {
                return false;
            }

            var targetUser = userDatabase.GetUserByUsername(username);
            if (targetUser == null)
            {
                return false;
            }

            // Admin kann sich nicht selbst deaktivieren
            if (targetUser.UserId == authManager.CurrentUser.UserId)
            {
                return false;
            }

            targetUser.IsActive = false;
            return userDatabase.UpdateUser(targetUser);
        }

        /// <summary>
        /// Aktiviert einen Benutzer wieder
        /// </summary>
        public bool ActivateUser(string username)
        {
            if (!authManager.CheckPermission(Permission.CreateUser))
            {
                return false;
            }

            var targetUser = userDatabase.GetUserByUsername(username);
            if (targetUser == null)
            {
                return false;
            }

            targetUser.IsActive = true;
            return userDatabase.UpdateUser(targetUser);
        }

        /// <summary>
        /// Gibt alle Berechtigungen eines Benutzers zurück
        /// </summary>
        public string GetUserPermissionsInfo(string username)
        {
            var user = userDatabase.GetUserByUsername(username);
            if (user == null)
            {
                return "Benutzer nicht gefunden.";
            }

            var info = $"Benutzer: {user.Username}\n";
            info += $"Rolle: {user.Role}\n";
            info += $"Berechtigungen:\n";
            
            foreach (Permission perm in Enum.GetValues(typeof(Permission)))
            {
                if (perm != Permission.None && user.HasPermission(perm))
                {
                    info += $"  - {perm}\n";
                }
            }

            return info;
        }
    }
}
