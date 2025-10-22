using System;

namespace SlotOS.System
{
    /// <summary>
    /// Verwaltet Berechtigungen und Zugriffsrechte im SlotOS-System
    /// </summary>
    public class PermissionChecker
    {
        #region Konstanten

        // Definierte Aktionen im System
        public const string ACTION_USER_CREATE = "user.create";
        public const string ACTION_USER_DELETE = "user.delete";
        public const string ACTION_USER_MODIFY = "user.modify";
        public const string ACTION_USER_VIEW = "user.view";
        public const string ACTION_USER_LIST = "user.list";
        public const string ACTION_PASSWORD_RESET = "password.reset";
        
        public const string ACTION_FILE_READ = "file.read";
        public const string ACTION_FILE_WRITE = "file.write";
        public const string ACTION_FILE_DELETE = "file.delete";
        public const string ACTION_FILE_EXECUTE = "file.execute";
        
        public const string ACTION_SYSTEM_CONFIG = "system.config";
        public const string ACTION_SYSTEM_SHUTDOWN = "system.shutdown";
        public const string ACTION_SYSTEM_REBOOT = "system.reboot";
        
        public const string ACTION_VIEW_LOGS = "logs.view";
        public const string ACTION_CLEAR_LOGS = "logs.clear";

        #endregion

        #region Singleton Pattern

        private static PermissionChecker _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Singleton-Instanz des PermissionCheckers
        /// </summary>
        public static PermissionChecker Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PermissionChecker();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Privater Konstruktor für Singleton-Pattern
        /// </summary>
        private PermissionChecker() { }

        #endregion

        #region Öffentliche Methoden

        /// <summary>
        /// Prüft ob ein Benutzer die Berechtigung für eine bestimmte Aktion hat
        /// </summary>
        /// <param name="user">Der zu prüfende Benutzer</param>
        /// <param name="action">Die zu prüfende Aktion</param>
        /// <returns>True wenn der Benutzer die Berechtigung hat</returns>
        public bool HasPermission(User user, string action)
        {
            if (user == null)
                return false;

            if (string.IsNullOrWhiteSpace(action))
                return false;

            // Inaktive Benutzer haben keine Berechtigungen
            if (!user.IsActive)
                return false;

            // Admin hat immer alle Berechtigungen
            if (IsAdmin(user))
                return true;

            // Berechtigungen für Standard-Benutzer
            if (user.Role == UserRole.Standard)
            {
                return CheckStandardUserPermissions(action);
            }

            // Berechtigungen für Gast-Benutzer
            if (user.Role == UserRole.Guest)
            {
                return CheckGuestUserPermissions(action);
            }

            return false;
        }

        /// <summary>
        /// Prüft ob ein Benutzer Administrator-Rechte hat
        /// </summary>
        /// <param name="user">Der zu prüfende Benutzer</param>
        /// <returns>True wenn der Benutzer Admin ist</returns>
        public bool IsAdmin(User user)
        {
            if (user == null)
                return false;

            return user.Role == UserRole.Admin && user.IsActive;
        }

        /// <summary>
        /// Verweigert den Zugriff mit einer Fehlermeldung
        /// </summary>
        /// <param name="reason">Grund für die Verweigerung</param>
        /// <exception cref="UnauthorizedAccessException">Wird immer geworfen</exception>
        public void DenyAccess(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                reason = "Zugriff verweigert";

            throw new UnauthorizedAccessException(reason);
        }

        /// <summary>
        /// Prüft ob ein Benutzer die Berechtigung hat und wirft eine Exception wenn nicht
        /// </summary>
        /// <param name="user">Der zu prüfende Benutzer</param>
        /// <param name="action">Die zu prüfende Aktion</param>
        /// <exception cref="UnauthorizedAccessException">Wenn keine Berechtigung vorhanden</exception>
        public void RequirePermission(User user, string action)
        {
            if (!HasPermission(user, action))
            {
                string actionName = GetActionDisplayName(action);
                DenyAccess("Sie haben keine Berechtigung fur: " + actionName);
            }
        }

        /// <summary>
        /// Prüft ob ein Benutzer Admin-Rechte hat und wirft eine Exception wenn nicht
        /// </summary>
        /// <param name="user">Der zu prüfende Benutzer</param>
        /// <exception cref="UnauthorizedAccessException">Wenn keine Admin-Rechte vorhanden</exception>
        public void RequireAdmin(User user)
        {
            if (!IsAdmin(user))
            {
                DenyAccess("Diese Aktion erfordert Administrator-Rechte");
            }
        }

        /// <summary>
        /// Prüft ob ein Benutzer auf eine Datei zugreifen darf
        /// </summary>
        /// <param name="user">Der Benutzer</param>
        /// <param name="filePath">Pfad zur Datei</param>
        /// <param name="action">Die gewünschte Aktion (read, write, delete, execute)</param>
        /// <returns>True wenn Zugriff erlaubt ist</returns>
        public bool CanAccessFile(User user, string filePath, string action)
        {
            if (user == null || string.IsNullOrWhiteSpace(filePath))
                return false;

            // Admin kann alles
            if (IsAdmin(user))
                return true;

            // System-Dateien sind geschützt (nur Admin)
            if (IsSystemPath(filePath))
            {
                return false;
            }

            // Benutzer kann auf sein Home-Verzeichnis zugreifen
            if (IsInUserHomeDirectory(user, filePath))
            {
                // Standard-Benutzer hat volle Rechte in seinem Home-Verzeichnis
                if (user.Role == UserRole.Standard)
                    return true;

                // Gast hat nur Lese-Rechte
                if (user.Role == UserRole.Guest)
                    return action == ACTION_FILE_READ;
            }

            // Öffentliche Dateien können gelesen werden
            if (IsPublicPath(filePath) && action == ACTION_FILE_READ)
            {
                return user.Role == UserRole.Standard || user.Role == UserRole.Guest;
            }

            return false;
        }

        /// <summary>
        /// Gibt eine lesbare Beschreibung der Berechtigungen eines Benutzers zurück
        /// </summary>
        /// <param name="user">Der Benutzer</param>
        /// <returns>String mit Berechtigungen</returns>
        public string GetPermissionSummary(User user)
        {
            if (user == null)
                return "Keine Berechtigungen (kein Benutzer)";

            if (!user.IsActive)
                return "Keine Berechtigungen (Konto deaktiviert)";

            // Cosmos OS: Keine String-Interpolation oder += verwenden!
            // Verwende feste Strings für jede Rolle
            if (IsAdmin(user))
            {
                return "Administrator-Rechte: Volle Kontrolle uber alle Systemfunktionen und Dateien";
            }
            else if (user.Role == UserRole.Standard)
            {
                return "Standard-Benutzer: Zugriff auf eigenes Home-Verzeichnis und offentliche Dateien";
            }
            else if (user.Role == UserRole.Guest)
            {
                return "Gast: Eingeschrankte Berechtigungen, nur Lesezugriff";
            }

            return "Unbekannte Rolle";
        }

        #endregion

        #region Private Hilfsmethoden

        /// <summary>
        /// Prüft Berechtigungen für Standard-Benutzer
        /// </summary>
        private bool CheckStandardUserPermissions(string action)
        {
            // Standard-Benutzer dürfen:
            switch (action)
            {
                // Eigene Informationen anzeigen
                case ACTION_USER_VIEW:
                    return true;

                // Eigenes Passwort ändern (wird separat über Benutzername geprüft)
                // In der Implementierung wird geprüft ob user == currentUser

                // Dateien lesen/schreiben in eigenem Home
                case ACTION_FILE_READ:
                case ACTION_FILE_WRITE:
                    return true; // Wird in CanAccessFile weiter eingeschränkt

                default:
                    return false;
            }
        }

        /// <summary>
        /// Prüft Berechtigungen für Gast-Benutzer
        /// </summary>
        private bool CheckGuestUserPermissions(string action)
        {
            // Gast-Benutzer dürfen nur sehr eingeschränkt:
            switch (action)
            {
                // Eigene Informationen anzeigen
                case ACTION_USER_VIEW:
                    return true;

                // Nur Dateien lesen
                case ACTION_FILE_READ:
                    return true; // Wird in CanAccessFile weiter eingeschränkt

                default:
                    return false;
            }
        }

        /// <summary>
        /// Prüft ob ein Pfad ein System-Pfad ist
        /// </summary>
        private bool IsSystemPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string normalizedPath = path.ToLower().Replace("\\", "/");

            // System-Verzeichnisse
            return normalizedPath.StartsWith("0:/system") ||
                   normalizedPath.StartsWith("/system") ||
                   normalizedPath.StartsWith("0:/boot") ||
                   normalizedPath.StartsWith("/boot") ||
                   normalizedPath.Contains("/system/") ||
                   normalizedPath.Contains("/boot/");
        }

        /// <summary>
        /// Prüft ob ein Pfad im Home-Verzeichnis des Benutzers ist
        /// </summary>
        private bool IsInUserHomeDirectory(User user, string path)
        {
            if (user == null || string.IsNullOrWhiteSpace(path))
                return false;

            string normalizedPath = path.ToLower().Replace("\\", "/");
            string homeDir = user.HomeDirectory.ToLower().Replace("\\", "/");

            return normalizedPath.StartsWith(homeDir) ||
                   normalizedPath.StartsWith("0:" + homeDir) ||
                   normalizedPath.Contains(homeDir);
        }

        /// <summary>
        /// Prüft ob ein Pfad ein öffentlicher Pfad ist
        /// </summary>
        private bool IsPublicPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string normalizedPath = path.ToLower().Replace("\\", "/");

            return normalizedPath.StartsWith("0:/public") ||
                   normalizedPath.StartsWith("/public") ||
                   normalizedPath.Contains("/public/");
        }

        /// <summary>
        /// Gibt einen lesbaren Namen für eine Aktion zurück
        /// </summary>
        private string GetActionDisplayName(string action)
        {
            switch (action)
            {
                case ACTION_USER_CREATE: return "Benutzer erstellen";
                case ACTION_USER_DELETE: return "Benutzer loschen";
                case ACTION_USER_MODIFY: return "Benutzer bearbeiten";
                case ACTION_USER_VIEW: return "Benutzer anzeigen";
                case ACTION_USER_LIST: return "Benutzerliste anzeigen";
                case ACTION_PASSWORD_RESET: return "Passwort zurucksetzen";
                case ACTION_FILE_READ: return "Datei lesen";
                case ACTION_FILE_WRITE: return "Datei schreiben";
                case ACTION_FILE_DELETE: return "Datei loschen";
                case ACTION_FILE_EXECUTE: return "Datei ausfuhren";
                case ACTION_SYSTEM_CONFIG: return "System konfigurieren";
                case ACTION_SYSTEM_SHUTDOWN: return "System herunterfahren";
                case ACTION_SYSTEM_REBOOT: return "System neu starten";
                case ACTION_VIEW_LOGS: return "Logs anzeigen";
                case ACTION_CLEAR_LOGS: return "Logs loschen";
                default: return action;
            }
        }

        #endregion
    }
}
