using System;
using System.Collections.Generic;

namespace CosmosKernel1.UserManagement
{
    /// <summary>
    /// Repräsentiert Dateiberechtigungen
    /// </summary>
    public class FilePermission
    {
        public string FilePath { get; set; }
        public int OwnerId { get; set; }
        public int GroupId { get; set; }
        public Permission OwnerPermissions { get; set; }
        public Permission GroupPermissions { get; set; }
        public Permission OthersPermissions { get; set; }

        public FilePermission(string filePath, int ownerId, int groupId)
        {
            FilePath = filePath;
            OwnerId = ownerId;
            GroupId = groupId;
            OwnerPermissions = Permission.FullControl;
            GroupPermissions = Permission.ReadWrite;
            OthersPermissions = Permission.Read;
        }
    }

    /// <summary>
    /// Verwaltet Dateisystem-Berechtigungen
    /// </summary>
    public class FilePermissionManager
    {
        private Dictionary<string, FilePermission> filePermissions;
        private AuthenticationManager authManager;
        private UserDatabase userDatabase;

        public FilePermissionManager(AuthenticationManager authentication, UserDatabase database)
        {
            filePermissions = new Dictionary<string, FilePermission>();
            authManager = authentication;
            userDatabase = database;
        }

        /// <summary>
        /// Setzt Berechtigungen für eine Datei/Ordner
        /// </summary>
        public bool SetFilePermission(string filePath, int ownerId, int groupId, 
            Permission ownerPerms, Permission groupPerms, Permission othersPerms)
        {
            if (!authManager.IsLoggedIn)
            {
                return false;
            }

            // Normalisiere Pfad
            filePath = NormalizePath(filePath);

            // Prüfe ob der Benutzer berechtigt ist
            if (!CanModifyPermissions(filePath))
            {
                return false;
            }

            var filePermission = new FilePermission(filePath, ownerId, groupId)
            {
                OwnerPermissions = ownerPerms,
                GroupPermissions = groupPerms,
                OthersPermissions = othersPerms
            };

            filePermissions[filePath] = filePermission;
            return true;
        }

        /// <summary>
        /// Gibt Berechtigungen für eine Datei zurück
        /// </summary>
        public FilePermission GetFilePermission(string filePath)
        {
            filePath = NormalizePath(filePath);
            
            if (filePermissions.ContainsKey(filePath))
            {
                return filePermissions[filePath];
            }

            // Standardberechtigung wenn keine spezifische gesetzt ist
            return null;
        }

        /// <summary>
        /// Prüft ob der aktuelle Benutzer Zugriff auf eine Datei hat
        /// </summary>
        public bool CanAccessFile(string filePath, Permission requiredPermission)
        {
            if (!authManager.IsLoggedIn)
            {
                return false;
            }

            var currentUser = authManager.CurrentUser;
            
            // Admin hat immer Zugriff
            if (currentUser.HasPermission(Permission.SystemAdmin))
            {
                return true;
            }

            filePath = NormalizePath(filePath);
            var filePermission = GetFilePermission(filePath);

            // Wenn keine Berechtigung gesetzt ist, verwende Standardverhalten
            if (filePermission == null)
            {
                return true; // Standardmäßig erlaubt
            }

            // Prüfe Besitzer
            if (filePermission.OwnerId == currentUser.UserId)
            {
                return PermissionHelper.HasPermission(filePermission.OwnerPermissions, requiredPermission);
            }

            // Prüfe Gruppe
            if (currentUser.GroupIds.Contains(filePermission.GroupId))
            {
                return PermissionHelper.HasPermission(filePermission.GroupPermissions, requiredPermission);
            }

            // Prüfe Others
            return PermissionHelper.HasPermission(filePermission.OthersPermissions, requiredPermission);
        }

        /// <summary>
        /// Prüft ob der Benutzer Berechtigungen ändern darf
        /// </summary>
        private bool CanModifyPermissions(string filePath)
        {
            var currentUser = authManager.CurrentUser;

            // Admin kann immer Berechtigungen ändern
            if (currentUser.HasPermission(Permission.ModifyPermissions))
            {
                return true;
            }

            var filePermission = GetFilePermission(filePath);
            
            // Besitzer kann Berechtigungen ändern
            if (filePermission != null && filePermission.OwnerId == currentUser.UserId)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ändert den Besitzer einer Datei
        /// </summary>
        public bool ChangeOwner(string filePath, int newOwnerId)
        {
            if (!authManager.CheckPermission(Permission.ModifyPermissions))
            {
                return false;
            }

            filePath = NormalizePath(filePath);
            var filePermission = GetFilePermission(filePath);

            if (filePermission == null)
            {
                return false;
            }

            filePermission.OwnerId = newOwnerId;
            return true;
        }

        /// <summary>
        /// Ändert die Gruppe einer Datei
        /// </summary>
        public bool ChangeGroup(string filePath, int newGroupId)
        {
            if (!CanModifyPermissions(filePath))
            {
                return false;
            }

            filePath = NormalizePath(filePath);
            var filePermission = GetFilePermission(filePath);

            if (filePermission == null)
            {
                return false;
            }

            filePermission.GroupId = newGroupId;
            return true;
        }

        /// <summary>
        /// Normalisiert einen Dateipfad
        /// </summary>
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            // Konvertiere zu Lowercase für case-insensitive Vergleiche
            return path.ToLower().Replace('/', '\\');
        }

        /// <summary>
        /// Erstellt eine Berechtigungszeichenkette im Unix-Stil (rwx)
        /// </summary>
        public string GetPermissionString(string filePath)
        {
            var filePermission = GetFilePermission(filePath);
            if (filePermission == null)
            {
                return "rwxrwxrwx"; // Standard
            }

            string result = "";
            result += PermissionToString(filePermission.OwnerPermissions);
            result += PermissionToString(filePermission.GroupPermissions);
            result += PermissionToString(filePermission.OthersPermissions);
            
            return result;
        }

        /// <summary>
        /// Konvertiert Permission zu rwx-String
        /// </summary>
        private string PermissionToString(Permission perm)
        {
            string result = "";
            result += PermissionHelper.HasPermission(perm, Permission.Read) ? "r" : "-";
            result += PermissionHelper.HasPermission(perm, Permission.Write) ? "w" : "-";
            result += PermissionHelper.HasPermission(perm, Permission.Execute) ? "x" : "-";
            return result;
        }

        /// <summary>
        /// Gibt Informationen über Dateiberechtigungen zurück
        /// </summary>
        public string GetFilePermissionInfo(string filePath)
        {
            filePath = NormalizePath(filePath);
            var filePermission = GetFilePermission(filePath);

            if (filePermission == null)
            {
                return $"Keine spezifischen Berechtigungen für: {filePath}";
            }

            var owner = userDatabase.GetUserById(filePermission.OwnerId);
            var group = userDatabase.GetGroupById(filePermission.GroupId);

            string info = $"Datei: {filePath}\n";
            info += $"Besitzer: {owner?.Username ?? "Unbekannt"} (ID: {filePermission.OwnerId})\n";
            info += $"Gruppe: {group?.GroupName ?? "Unbekannt"} (ID: {filePermission.GroupId})\n";
            info += $"Berechtigungen: {GetPermissionString(filePath)}\n";
            info += $"  Owner: {PermissionToString(filePermission.OwnerPermissions)}\n";
            info += $"  Group: {PermissionToString(filePermission.GroupPermissions)}\n";
            info += $"  Others: {PermissionToString(filePermission.OthersPermissions)}\n";

            return info;
        }
    }
}
