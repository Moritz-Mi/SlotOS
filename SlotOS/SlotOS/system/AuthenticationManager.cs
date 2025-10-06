using System;
using System.Collections.Generic;

namespace SlotOS.System
{
    /// <summary>
    /// Verwaltet die Authentifizierung von Benutzern im System
    /// </summary>
    public class AuthenticationManager
    {
        #region Eigenschaften

        /// <summary>
        /// Der aktuell angemeldete Benutzer
        /// </summary>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// Gibt an, ob ein Benutzer aktuell angemeldet ist
        /// </summary>
        public bool IsAuthenticated => CurrentUser != null;

        /// <summary>
        /// Zeitpunkt der letzten Aktivität (für Session-Timeout)
        /// </summary>
        public DateTime LastActivity { get; private set; }

        /// <summary>
        /// Liste aller verfügbaren Benutzer (wird von außen gesetzt)
        /// </summary>
        private List<User> _users;

        /// <summary>
        /// Maximale Anzahl fehlgeschlagener Login-Versuche
        /// </summary>
        private const int MAX_LOGIN_ATTEMPTS = 3;

        /// <summary>
        /// Aktuelle Anzahl fehlgeschlagener Login-Versuche
        /// </summary>
        private int _failedLoginAttempts = 0;

        /// <summary>
        /// Zeitpunkt der Sperrung nach zu vielen fehlgeschlagenen Versuchen
        /// </summary>
        private DateTime _lockoutUntil = DateTime.MinValue;

        /// <summary>
        /// Sperrzeit in Sekunden nach zu vielen fehlgeschlagenen Versuchen
        /// </summary>
        private const int LOCKOUT_DURATION_SECONDS = 30;

        #endregion

        #region Konstruktor

        /// <summary>
        /// Erstellt einen neuen AuthenticationManager
        /// </summary>
        public AuthenticationManager()
        {
            CurrentUser = null;
            LastActivity = DateTime.MinValue;
            _users = new List<User>();
        }

        #endregion

        #region Öffentliche Methoden

        /// <summary>
        /// Setzt die Benutzerliste für die Authentifizierung
        /// </summary>
        /// <param name="users">Liste der verfügbaren Benutzer</param>
        public void SetUsers(List<User> users)
        {
            _users = users ?? new List<User>();
        }

        /// <summary>
        /// Meldet einen Benutzer am System an
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <param name="password">Passwort (Klartext)</param>
        /// <returns>True wenn Login erfolgreich, sonst False</returns>
        public bool Login(string username, string password)
        {
            // Prüfen ob System gesperrt ist
            if (IsLockedOut())
            {
                int remainingSeconds = (int)(_lockoutUntil - DateTime.Now).TotalSeconds;
                throw new InvalidOperationException(
                    $"Zu viele fehlgeschlagene Login-Versuche. Bitte warten Sie {remainingSeconds} Sekunden.");
            }

            // Eingaben validieren
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _failedLoginAttempts++;
                return false;
            }

            // Benutzer suchen
            User user = FindUser(username);
            if (user == null)
            {
                _failedLoginAttempts++;
                CheckLockout();
                return false;
            }

            // Prüfen ob Benutzer aktiv ist
            if (!user.IsActive)
            {
                throw new InvalidOperationException("Benutzerkonto ist deaktiviert.");
            }

            // Passwort überprüfen
            bool passwordCorrect = PasswordHasher.Verify(password, user.PasswordHash);
            
            if (!passwordCorrect)
            {
                _failedLoginAttempts++;
                CheckLockout();
                return false;
            }

            // Login erfolgreich
            CurrentUser = user;
            CurrentUser.UpdateLastLogin();
            LastActivity = DateTime.Now;
            _failedLoginAttempts = 0; // Fehlversuche zurücksetzen
            
            return true;
        }

        /// <summary>
        /// Meldet den aktuellen Benutzer ab
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
            LastActivity = DateTime.MinValue;
        }

        /// <summary>
        /// Prüft ob der aktuelle Benutzer Administrator-Rechte hat
        /// </summary>
        /// <returns>True wenn Admin-Rechte vorhanden</returns>
        /// <exception cref="UnauthorizedAccessException">Wenn keine Admin-Rechte vorhanden</exception>
        public bool RequireAdmin()
        {
            if (!IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Sie müssen angemeldet sein.");
            }

            if (!CurrentUser.IsAdmin())
            {
                throw new UnauthorizedAccessException("Dieser Befehl erfordert Administrator-Rechte.");
            }

            return true;
        }

        /// <summary>
        /// Prüft ob der aktuelle Benutzer angemeldet ist
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Wenn nicht angemeldet</exception>
        public void RequireAuthentication()
        {
            if (!IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Sie müssen angemeldet sein.");
            }

            UpdateActivity();
        }

        /// <summary>
        /// Aktualisiert den Zeitpunkt der letzten Aktivität
        /// </summary>
        public void UpdateActivity()
        {
            if (IsAuthenticated)
            {
                LastActivity = DateTime.Now;
            }
        }

        /// <summary>
        /// Prüft ob eine Session aufgrund von Inaktivität abgelaufen ist
        /// </summary>
        /// <param name="timeoutMinutes">Timeout in Minuten (Standard: 30)</param>
        /// <returns>True wenn Session abgelaufen ist</returns>
        public bool IsSessionExpired(int timeoutMinutes = 30)
        {
            if (!IsAuthenticated)
                return true;

            TimeSpan inactiveTime = DateTime.Now - LastActivity;
            return inactiveTime.TotalMinutes >= timeoutMinutes;
        }

        /// <summary>
        /// Meldet Benutzer automatisch ab wenn Session abgelaufen ist
        /// </summary>
        /// <param name="timeoutMinutes">Timeout in Minuten</param>
        /// <returns>True wenn Benutzer abgemeldet wurde</returns>
        public bool CheckAndHandleSessionTimeout(int timeoutMinutes = 30)
        {
            if (IsSessionExpired(timeoutMinutes))
            {
                Logout();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gibt die Anzahl der verbleibenden Login-Versuche zurück
        /// </summary>
        public int GetRemainingLoginAttempts()
        {
            return Math.Max(0, MAX_LOGIN_ATTEMPTS - _failedLoginAttempts);
        }

        #endregion

        #region Private Hilfsmethoden

        /// <summary>
        /// Sucht einen Benutzer nach Namen
        /// </summary>
        private User FindUser(string username)
        {
            if (_users == null)
                return null;

            foreach (var user in _users)
            {
                if (user.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    return user;
                }
            }
            return null;
        }

        /// <summary>
        /// Prüft ob das System gesperrt ist
        /// </summary>
        private bool IsLockedOut()
        {
            if (_lockoutUntil > DateTime.Now)
                return true;

            // Sperre ist abgelaufen, zurücksetzen
            if (_lockoutUntil != DateTime.MinValue)
            {
                _lockoutUntil = DateTime.MinValue;
                _failedLoginAttempts = 0;
            }

            return false;
        }

        /// <summary>
        /// Prüft ob System gesperrt werden soll nach zu vielen Fehlversuchen
        /// </summary>
        private void CheckLockout()
        {
            if (_failedLoginAttempts >= MAX_LOGIN_ATTEMPTS)
            {
                _lockoutUntil = DateTime.Now.AddSeconds(LOCKOUT_DURATION_SECONDS);
            }
        }

        #endregion

        #region Informations-Methoden

        /// <summary>
        /// Gibt Informationen über die aktuelle Session zurück
        /// </summary>
        public string GetSessionInfo()
        {
            if (!IsAuthenticated)
                return "Nicht angemeldet";

            TimeSpan loginDuration = DateTime.Now - CurrentUser.LastLogin;
            TimeSpan inactiveTime = DateTime.Now - LastActivity;

            return $"Benutzer: {CurrentUser.Username}\n" +
                   $"Rolle: {CurrentUser.Role}\n" +
                   $"Angemeldet seit: {loginDuration.TotalMinutes:F1} Minuten\n" +
                   $"Letzte Aktivität: vor {inactiveTime.TotalSeconds:F0} Sekunden";
        }

        /// <summary>
        /// Gibt den aktuellen Benutzernamen zurück oder "Gast" wenn nicht angemeldet
        /// </summary>
        public string GetCurrentUsername()
        {
            return IsAuthenticated ? CurrentUser.Username : "Gast";
        }

        #endregion
    }
}
