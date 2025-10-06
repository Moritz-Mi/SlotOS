using System;
using System.Text;

namespace CosmosKernel1.UserManagement
{
    /// <summary>
    /// Verwaltet Authentifizierung und Session-Handling
    /// </summary>
    public class AuthenticationManager
    {
        private UserDatabase userDatabase;
        private User currentUser;
        private bool isLoggedIn;

        public User CurrentUser => currentUser;
        public bool IsLoggedIn => isLoggedIn;

        public AuthenticationManager(UserDatabase database)
        {
            userDatabase = database;
            isLoggedIn = false;
        }

        /// <summary>
        /// Authentifiziert einen Benutzer mit Username und Passwort
        /// </summary>
        public bool Login(string username, string password)
        {
            if (isLoggedIn)
            {
                return false; // Bereits eingeloggt
            }

            var user = userDatabase.GetUserByUsername(username);
            if (user == null)
            {
                return false; // Benutzer nicht gefunden
            }

            if (!user.IsActive)
            {
                return false; // Benutzer ist deaktiviert
            }

            string passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
            {
                return false; // Falsches Passwort
            }

            // Erfolgreicher Login
            currentUser = user;
            isLoggedIn = true;
            user.UpdateLastLogin();
            userDatabase.UpdateUser(user);

            return true;
        }

        /// <summary>
        /// Meldet den aktuellen Benutzer ab
        /// </summary>
        public void Logout()
        {
            currentUser = null;
            isLoggedIn = false;
        }

        /// <summary>
        /// Prüft ob der aktuelle Benutzer eine bestimmte Berechtigung hat
        /// </summary>
        public bool CheckPermission(Permission requiredPermission)
        {
            if (!isLoggedIn || currentUser == null)
            {
                return false;
            }

            return currentUser.HasPermission(requiredPermission);
        }

        /// <summary>
        /// Ändert das Passwort des aktuellen Benutzers
        /// </summary>
        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if (!isLoggedIn || currentUser == null)
            {
                return false;
            }

            string oldHash = HashPassword(oldPassword);
            if (currentUser.PasswordHash != oldHash)
            {
                return false; // Altes Passwort stimmt nicht
            }

            currentUser.PasswordHash = HashPassword(newPassword);
            userDatabase.UpdateUser(currentUser);
            return true;
        }

        /// <summary>
        /// Ändert das Passwort eines anderen Benutzers (erfordert Admin-Rechte)
        /// </summary>
        public bool ResetUserPassword(string username, string newPassword)
        {
            if (!CheckPermission(Permission.ModifyPermissions))
            {
                return false; // Keine Berechtigung
            }

            var targetUser = userDatabase.GetUserByUsername(username);
            if (targetUser == null)
            {
                return false;
            }

            targetUser.PasswordHash = HashPassword(newPassword);
            userDatabase.UpdateUser(targetUser);
            return true;
        }

        /// <summary>
        /// Einfache Hash-Funktion für Passwörter (für Cosmos OS angepasst)
        /// In einem produktiven System sollte ein stärkerer Algorithmus verwendet werden
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }

            // Einfacher Hash-Algorithmus (djb2)
            uint hash = 5381;
            foreach (char c in password)
            {
                hash = ((hash << 5) + hash) + c; // hash * 33 + c
            }

            return hash.ToString("X8");
        }

        /// <summary>
        /// Erstellt einen neuen Benutzer (erfordert CreateUser-Berechtigung)
        /// </summary>
        public bool CreateNewUser(string username, string password, UserRole role)
        {
            if (!CheckPermission(Permission.CreateUser))
            {
                return false; // Keine Berechtigung
            }

            if (userDatabase.GetUserByUsername(username) != null)
            {
                return false; // Benutzer existiert bereits
            }

            string passwordHash = HashPassword(password);
            int newUserId = userDatabase.GetNextUserId();
            var newUser = new User(newUserId, username, passwordHash, role);
            
            return userDatabase.AddUser(newUser);
        }
    }
}
