using System;

namespace SlotOS.System
{
    /// <summary>
    /// Repräsentiert einen Benutzer im SlotOS-System
    /// </summary>
    public class User
    {
        #region Eigenschaften

        /// <summary>
        /// Eindeutiger Benutzername
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gehashtes Passwort des Benutzers
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Rolle des Benutzers (Admin, Standard, Guest)
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Zeitpunkt der Benutzererstellung
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Zeitpunkt des letzten Logins
        /// </summary>
        public DateTime LastLogin { get; set; }

        /// <summary>
        /// Status des Benutzerkontos (aktiv/inaktiv)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Heimverzeichnis des Benutzers
        /// </summary>
        public string HomeDirectory { get; set; }

        #endregion

        #region Konstruktoren

        /// <summary>
        /// Standardkonstruktor für Serialisierung
        /// </summary>
        public User()
        {
        }

        /// <summary>
        /// Erstellt einen neuen Benutzer mit den angegebenen Werten
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <param name="passwordHash">Gehashtes Passwort</param>
        /// <param name="role">Benutzerrolle</param>
        public User(string username, string passwordHash, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Benutzername darf nicht leer sein", nameof(username));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Passwort-Hash darf nicht leer sein", nameof(passwordHash));

            Username = username;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.Now;
            LastLogin = DateTime.MinValue;
            IsActive = true;
            HomeDirectory = $"/home/{username}";
        }

        #endregion

        #region Methoden

        /// <summary>
        /// Überprüft, ob das angegebene Passwort mit dem gespeicherten Hash übereinstimmt
        /// </summary>
        /// <param name="password">Zu prüfendes Passwort (Klartext)</param>
        /// <returns>True wenn das Passwort korrekt ist, sonst False</returns>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Hash des eingegebenen Passworts berechnen
            string inputHash = ComputeHash(password);

            // Mit gespeichertem Hash vergleichen
            return PasswordHash == inputHash;
        }

        /// <summary>
        /// Aktualisiert das Passwort des Benutzers
        /// </summary>
        /// <param name="newPassword">Neues Passwort (Klartext)</param>
        public void UpdatePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Neues Passwort darf nicht leer sein", nameof(newPassword));

            if (newPassword.Length < 4)
                throw new ArgumentException("Passwort muss mindestens 4 Zeichen lang sein", nameof(newPassword));

            PasswordHash = ComputeHash(newPassword);
        }

        /// <summary>
        /// Aktualisiert den Zeitpunkt des letzten Logins
        /// </summary>
        public void UpdateLastLogin()
        {
            LastLogin = DateTime.Now;
        }

        /// <summary>
        /// Prüft ob der Benutzer Administrator-Rechte hat
        /// </summary>
        /// <returns>True wenn der Benutzer Admin ist</returns>
        public bool IsAdmin()
        {
            return Role == UserRole.Admin;
        }

        #endregion

        #region Private Hilfsmethoden

        /// <summary>
        /// Berechnet einen einfachen Hash für das Passwort
        /// HINWEIS: Dies ist eine vereinfachte Implementierung für Cosmos OS.
        /// In einer Produkteionsumgebung sollte ein sicherer Algorithmus wie SHA256 mit Salt verwendet werden.
        /// </summary>
        /// <param name="password">Passwort im Klartext</param>
        /// <returns>Hash-String</returns>
        private string ComputeHash(string password)
        {
            // Einfacher Hash-Algorithmus für Cosmos OS
            // TODO: Später durch SHA256 oder stärkeren Algorithmus ersetzen
            int hash = 17;
            foreach (char c in password)
            {
                hash = hash * 31 + c;
            }
            return hash.ToString("X8");
        }

        #endregion

        #region Überschriebene Methoden

        /// <summary>
        /// String-Repräsentation des Benutzers
        /// </summary>
        public override string ToString()
        {
            string roleStr = Role switch
            {
                UserRole.Admin => "Admin",
                UserRole.Standard => "User",
                UserRole.Guest => "Guest",
                _ => "Unknown"
            };

            string statusStr = IsActive ? "Aktiv" : "Inaktiv";
            
            return $"{Username} ({roleStr}) - {statusStr}";
        }

        #endregion
    }
}
