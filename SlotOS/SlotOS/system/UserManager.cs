using System;
using System.Collections.Generic;
using System.Linq;

namespace SlotOS.System
{
    /// <summary>
    /// Singleton-Klasse zur Verwaltung aller Benutzer im System
    /// Verwaltet CRUD-Operationen für Benutzer
    /// </summary>
    public class UserManager
    {
        #region Singleton Pattern

        private static UserManager _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gibt die Singleton-Instanz des UserManagers zurück
        /// </summary>
        public static UserManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserManager();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Eigenschaften

        /// <summary>
        /// Liste aller Benutzer im System
        /// </summary>
        private List<User> _users;

        /// <summary>
        /// Gibt die Anzahl der Benutzer im System zurück
        /// </summary>
        public int UserCount => _users.Count;

        /// <summary>
        /// Standard-Admin-Benutzername
        /// </summary>
        public const string DEFAULT_ADMIN_USERNAME = "admin";

        /// <summary>
        /// Standard-Admin-Passwort (sollte beim ersten Login geändert werden)
        /// </summary>
        public const string DEFAULT_ADMIN_PASSWORD = "admin";

        #endregion

        #region Konstruktor

        /// <summary>
        /// Privater Konstruktor für Singleton-Pattern
        /// </summary>
        private UserManager()
        {
            _users = new List<User>();
        }

        #endregion

        #region Initialisierung

        /// <summary>
        /// Initialisiert den UserManager und erstellt bei Bedarf den Standard-Admin
        /// </summary>
        public void Initialize()
        {
            // Prüfen ob bereits Benutzer existieren
            if (_users.Count == 0)
            {
                CreateDefaultAdmin();
            }
        }

        /// <summary>
        /// Erstellt den Standard-Administrator-Account
        /// </summary>
        private void CreateDefaultAdmin()
        {
            try
            {
                string passwordHash = PasswordHasher.Hash(DEFAULT_ADMIN_PASSWORD);
                var adminUser = new User(DEFAULT_ADMIN_USERNAME, passwordHash, UserRole.Admin)
                {
                    IsActive = true,
                    HomeDirectory = "/home/admin"
                };

                _users.Add(adminUser);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler beim Erstellen des Standard-Admins: {ex.Message}");
            }
        }

        #endregion

        #region Benutzer-CRUD-Operationen

        /// <summary>
        /// Erstellt einen neuen Benutzer im System
        /// </summary>
        /// <param name="username">Benutzername (muss eindeutig sein)</param>
        /// <param name="password">Passwort (Klartext, wird gehasht)</param>
        /// <param name="role">Benutzerrolle</param>
        /// <returns>True wenn Benutzer erfolgreich erstellt wurde, sonst False</returns>
        public bool CreateUser(string username, string password, UserRole role)
        {
            // Eingaben validieren
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Benutzername darf nicht leer sein", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Passwort darf nicht leer sein", nameof(password));
            }

            if (password.Length < 4)
            {
                throw new ArgumentException("Passwort muss mindestens 4 Zeichen lang sein", nameof(password));
            }

            // Prüfen ob Benutzer bereits existiert
            if (UserExists(username))
            {
                return false;
            }

            try
            {
                // Passwort hashen
                string passwordHash = PasswordHasher.Hash(password);

                // Neuen Benutzer erstellen
                var newUser = new User(username, passwordHash, role)
                {
                    IsActive = true,
                    HomeDirectory = $"/home/{username}"
                };

                _users.Add(newUser);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Löscht einen Benutzer aus dem System
        /// </summary>
        /// <param name="username">Benutzername des zu löschenden Benutzers</param>
        /// <returns>True wenn Benutzer erfolgreich gelöscht wurde, sonst False</returns>
        public bool DeleteUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            // Verhindere Löschung des letzten Admins
            if (IsLastAdmin(username))
            {
                throw new InvalidOperationException("Der letzte Administrator kann nicht gelöscht werden.");
            }

            // Benutzer suchen und löschen
            var user = GetUser(username);
            if (user != null)
            {
                _users.Remove(user);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gibt einen bestimmten Benutzer zurück
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <returns>User-Objekt oder null wenn nicht gefunden</returns>
        public User GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gibt alle Benutzer im System zurück
        /// </summary>
        /// <returns>Liste aller Benutzer</returns>
        public List<User> GetAllUsers()
        {
            // Erstelle eine Kopie der Liste, um direkte Manipulation zu verhindern
            return new List<User>(_users);
        }

        /// <summary>
        /// Gibt alle aktiven Benutzer zurück
        /// </summary>
        public List<User> GetActiveUsers()
        {
            return _users.Where(u => u.IsActive).ToList();
        }

        /// <summary>
        /// Aktualisiert einen existierenden Benutzer
        /// </summary>
        /// <param name="user">Benutzer mit aktualisierten Daten</param>
        /// <returns>True wenn Update erfolgreich war, sonst False</returns>
        public bool UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var existingUser = GetUser(user.Username);
            if (existingUser == null)
            {
                return false;
            }

            // Aktualisiere Eigenschaften (außer Username und Passwort)
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;
            existingUser.HomeDirectory = user.HomeDirectory;
            existingUser.LastLogin = user.LastLogin;

            return true;
        }

        #endregion

        #region Passwort-Verwaltung

        /// <summary>
        /// Ändert das Passwort eines Benutzers
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <param name="oldPassword">Altes Passwort</param>
        /// <param name="newPassword">Neues Passwort</param>
        /// <returns>True wenn Passwort erfolgreich geändert wurde, sonst False</returns>
        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(oldPassword) || 
                string.IsNullOrWhiteSpace(newPassword))
            {
                return false;
            }

            if (newPassword.Length < 4)
            {
                throw new ArgumentException("Neues Passwort muss mindestens 4 Zeichen lang sein");
            }

            var user = GetUser(username);
            if (user == null)
            {
                return false;
            }

            // Altes Passwort überprüfen
            if (!user.VerifyPassword(oldPassword))
            {
                return false;
            }

            // Neues Passwort setzen
            try
            {
                user.UpdatePassword(newPassword);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Setzt das Passwort eines Benutzers zurück (nur für Admins)
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <param name="newPassword">Neues Passwort</param>
        /// <returns>True wenn erfolgreich, sonst False</returns>
        public bool ResetPassword(string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
            {
                return false;
            }

            if (newPassword.Length < 4)
            {
                throw new ArgumentException("Neues Passwort muss mindestens 4 Zeichen lang sein");
            }

            var user = GetUser(username);
            if (user == null)
            {
                return false;
            }

            try
            {
                user.UpdatePassword(newPassword);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Hilfsmethoden

        /// <summary>
        /// Prüft ob ein Benutzer mit dem angegebenen Namen existiert
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <returns>True wenn Benutzer existiert, sonst False</returns>
        public bool UserExists(string username)
        {
            return GetUser(username) != null;
        }

        /// <summary>
        /// Prüft ob ein Benutzer der letzte Administrator ist
        /// </summary>
        private bool IsLastAdmin(string username)
        {
            var user = GetUser(username);
            if (user == null || user.Role != UserRole.Admin)
            {
                return false;
            }

            // Zähle aktive Admins
            int adminCount = _users.Count(u => u.Role == UserRole.Admin && u.IsActive);
            return adminCount <= 1;
        }

        /// <summary>
        /// Gibt die Anzahl der Admins zurück
        /// </summary>
        public int GetAdminCount()
        {
            return _users.Count(u => u.Role == UserRole.Admin && u.IsActive);
        }

        /// <summary>
        /// Setzt einen Benutzer aktiv oder inaktiv
        /// </summary>
        public bool SetUserActive(string username, bool isActive)
        {
            var user = GetUser(username);
            if (user == null)
            {
                return false;
            }

            // Verhindere Deaktivierung des letzten Admins
            if (!isActive && IsLastAdmin(username))
            {
                throw new InvalidOperationException("Der letzte Administrator kann nicht deaktiviert werden.");
            }

            user.IsActive = isActive;
            return true;
        }

        /// <summary>
        /// Gibt Statistiken über das Benutzersystem zurück
        /// </summary>
        public string GetStatistics()
        {
            int totalUsers = _users.Count;
            int activeUsers = _users.Count(u => u.IsActive);
            int admins = _users.Count(u => u.Role == UserRole.Admin);
            int standardUsers = _users.Count(u => u.Role == UserRole.Standard);
            int guests = _users.Count(u => u.Role == UserRole.Guest);

            return $"Benutzer-Statistiken:\n" +
                   $"  Gesamt: {totalUsers}\n" +
                   $"  Aktiv: {activeUsers}\n" +
                   $"  Administratoren: {admins}\n" +
                   $"  Standard-Benutzer: {standardUsers}\n" +
                   $"  Gäste: {guests}";
        }

        #endregion

        #region Interne Verwaltung

        /// <summary>
        /// Gibt die interne Benutzerliste zurück (für AuthenticationManager)
        /// </summary>
        internal List<User> GetInternalUserList()
        {
            return _users;
        }

        /// <summary>
        /// Setzt die Benutzerliste (für Persistenz-Layer)
        /// </summary>
        internal void SetUsers(List<User> users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            _users = users;
        }

        /// <summary>
        /// Löscht alle Benutzer (nur für Tests!)
        /// </summary>
        internal void ClearAllUsers()
        {
            _users.Clear();
        }

        #endregion
    }
}
