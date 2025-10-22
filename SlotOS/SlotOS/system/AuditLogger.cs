using System;
using System.Collections.Generic;

namespace SlotOS.System
{
    /// <summary>
    /// Protokolliert sicherheitsrelevante Aktionen im System
    /// </summary>
    public class AuditLogger
    {
        private static AuditLogger _instance;
        private List<AuditEntry> _entries;
        private const int MAX_ENTRIES = 100; // Maximale Anzahl gespeicherter Einträge

        /// <summary>
        /// Singleton-Instanz des AuditLoggers
        /// </summary>
        public static AuditLogger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AuditLogger();
                return _instance;
            }
        }

        private AuditLogger()
        {
            _entries = new List<AuditEntry>();
        }

        /// <summary>
        /// Protokolliert eine Aktion
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <param name="action">Durchgeführte Aktion</param>
        /// <param name="details">Zusätzliche Details</param>
        /// <param name="isSuccess">Gibt an, ob die Aktion erfolgreich war</param>
        public void Log(string username, string action, string details = "", bool isSuccess = true)
        {
            var entry = new AuditEntry
            {
                Timestamp = DateTime.Now,
                Username = username ?? "System",
                Action = action,
                Details = details,
                IsSuccess = isSuccess
            };

            _entries.Add(entry);

            // Begrenze die Anzahl der Einträge
            if (_entries.Count > MAX_ENTRIES)
            {
                _entries.RemoveAt(0);
            }

            // Ausgabe in Console für Debugging (deaktiviert wegen Cosmos OS Kompatibilität)
            // DEBUG-Ausgabe kann Stack Corruption verursachen
        }

        /// <summary>
        /// Protokolliert einen Login-Versuch
        /// </summary>
        public void LogLogin(string username, bool success)
        {
            string details = "Login fehlgeschlagen";
            if (success) details = "Erfolgreich angemeldet";
            Log(username, "LOGIN", details, success);
        }

        /// <summary>
        /// Protokolliert einen Logout
        /// </summary>
        public void LogLogout(string username)
        {
            Log(username, "LOGOUT", "Benutzer abgemeldet", true);
        }

        /// <summary>
        /// Protokolliert eine Benutzer-Aktion
        /// </summary>
        public void LogUserAction(string username, string action, string targetUser, bool success)
        {
            string details = "Ziel: " + targetUser;
            Log(username, action, details, success);
        }

        /// <summary>
        /// Protokolliert eine Passwort-Änderung
        /// </summary>
        public void LogPasswordChange(string username, string targetUser, bool success)
        {
            string details = "Passwort für " + targetUser;
            if (username == targetUser) details = "Eigenes Passwort";
            Log(username, "PASSWORD_CHANGE", details, success);
        }

        /// <summary>
        /// Protokolliert einen Session-Timeout
        /// </summary>
        public void LogSessionTimeout(string username)
        {
            Log(username, "SESSION_TIMEOUT", "Automatisch abgemeldet nach Inaktivität", true);
        }

        /// <summary>
        /// Gibt alle Audit-Einträge zurück
        /// </summary>
        public List<AuditEntry> GetEntries()
        {
            return new List<AuditEntry>(_entries);
        }

        /// <summary>
        /// Gibt die letzten N Einträge zurück
        /// </summary>
        public List<AuditEntry> GetRecentEntries(int count)
        {
            int startIndex = Math.Max(0, _entries.Count - count);
            int actualCount = Math.Min(count, _entries.Count);
            
            var result = new List<AuditEntry>();
            for (int i = startIndex; i < _entries.Count; i++)
            {
                result.Add(_entries[i]);
            }
            return result;
        }

        /// <summary>
        /// Gibt Einträge für einen bestimmten Benutzer zurück
        /// </summary>
        public List<AuditEntry> GetEntriesForUser(string username)
        {
            var result = new List<AuditEntry>();
            foreach (var entry in _entries)
            {
                if (entry.Username.ToLower() == username.ToLower())
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// Formatiert die Audit-Logs für Anzeige
        /// HINWEIS: Gibt Logs direkt auf Console aus statt String zurückzugeben
        /// (Cosmos OS StringBuilder-Kompatibilität)
        /// </summary>
        public string FormatLogs(int count = 10)
        {
            var entries = GetRecentEntries(count);
            if (entries.Count == 0)
            {
                return "Keine Audit-Einträge vorhanden.";
            }

            // Direkte Console-Ausgabe statt StringBuilder (Cosmos OS Kompatibilität)
            Console.WriteLine("Letzte " + entries.Count.ToString() + " Audit-Einträge:");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine(ConsoleHelper.PadRight("Zeit", 20) + " | " + 
                         ConsoleHelper.PadRight("Benutzer", 15) + " | " + 
                         ConsoleHelper.PadRight("Aktion", 20) + " | " + 
                         "Details");
            Console.WriteLine(new string('-', 80));

            foreach (var entry in entries)
            {
                string status = "[FEHLER] ";
                if (entry.IsSuccess) status = "";
                // Cosmos OS compatible: manuelle Datum-Formatierung
                string day = entry.Timestamp.Day.ToString();
                string month = entry.Timestamp.Month.ToString();
                string year = entry.Timestamp.Year.ToString();
                string hour = entry.Timestamp.Hour.ToString();
                string minute = entry.Timestamp.Minute.ToString();
                if (day.Length == 1) day = "0" + day;
                if (month.Length == 1) month = "0" + month;
                if (hour.Length == 1) hour = "0" + hour;
                if (minute.Length == 1) minute = "0" + minute;
                // Safe year formatting: only take last 2 digits if year has at least 2 chars
                string yearShort = year.Length >= 2 ? year.Substring(year.Length - 2) : year;
                string timeStr = day + "." + month + "." + yearShort + " " + hour + ":" + minute;
                
                Console.WriteLine(
                    ConsoleHelper.PadRight(timeStr, 20) + " | " +
                    ConsoleHelper.PadRight(entry.Username, 15) + " | " +
                    ConsoleHelper.PadRight(entry.Action, 20) + " | " +
                    status + entry.Details
                );
            }

            return ""; // Gibt leeren String zurück da direkt auf Console ausgegeben
        }

        /// <summary>
        /// Löscht alle Audit-Einträge
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }
    }

    /// <summary>
    /// Repräsentiert einen einzelnen Audit-Eintrag
    /// </summary>
    public class AuditEntry
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public bool IsSuccess { get; set; }
    }
}
