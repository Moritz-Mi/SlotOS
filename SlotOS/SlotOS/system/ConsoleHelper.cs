using System;
using System.Text;

namespace SlotOS.System
{
    /// <summary>
    /// Hilfsklasse für Console-UI-Funktionen
    /// Bietet Utility-Methoden für Passwort-Eingabe, formatierte Ausgabe, etc.
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// Liest ein Passwort ein und maskiert die Eingabe mit Sternchen
        /// </summary>
        /// <param name="prompt">Der anzuzeigende Prompt</param>
        /// <returns>Das eingegebene Passwort</returns>
        public static string ReadPassword(string prompt = "Passwort: ")
        {
            Console.Write(prompt);
            StringBuilder password = new StringBuilder();
            
            while (true)
            {
                var key = Console.ReadKey(true);
                
                // Enter beendet die Eingabe
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                
                // Backspace löscht das letzte Zeichen
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Length--;
                        Console.Write("\b \b"); // Cursor zurück, Leerzeichen, Cursor zurück
                    }
                    continue;
                }
                
                // Escape bricht ab
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    return null;
                }
                
                // Normale Zeichen werden hinzugefügt
                if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            
            return password.ToString();
        }

        /// <summary>
        /// Gibt eine Erfolgsmeldung in grün aus
        /// </summary>
        public static void WriteSuccess(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[OK] {message}");
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Gibt eine Fehlermeldung in rot aus
        /// </summary>
        public static void WriteError(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {message}");
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Gibt eine Warnung in gelb aus
        /// </summary>
        public static void WriteWarning(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN] {message}");
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Gibt eine Info-Nachricht in cyan aus
        /// </summary>
        public static void WriteInfo(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[INFO] {message}");
            Console.ForegroundColor = oldColor;
        }

        /// <summary>
        /// Gibt einen Header mit Rahmen aus
        /// </summary>
        public static void WriteHeader(string title)
        {
            int width = 60;
            Console.WriteLine();
            Console.WriteLine("+" + new string('=', width - 2) + "+");
            Console.WriteLine("|" + CenterText(title, width - 2) + "|");
            Console.WriteLine("+" + new string('=', width - 2) + "+");
            Console.WriteLine();
        }

        /// <summary>
        /// Gibt eine Trennlinie aus
        /// </summary>
        public static void WriteSeparator(int length = 60)
        {
            Console.WriteLine(new string('-', length));
        }

        /// <summary>
        /// Zentriert einen Text innerhalb einer gegebenen Breite
        /// </summary>
        private static string CenterText(string text, int width)
        {
            if (text.Length >= width)
                return text.Substring(0, width);
            
            int padding = (width - text.Length) / 2;
            return new string(' ', padding) + text + new string(' ', width - text.Length - padding);
        }

        /// <summary>
        /// Gibt eine Tabellen-Zeile aus
        /// </summary>
        public static void WriteTableRow(params string[] columns)
        {
            Console.Write("| ");
            for (int i = 0; i < columns.Length; i++)
            {
                Console.Write(columns[i]);
                if (i < columns.Length - 1)
                    Console.Write(" | ");
            }
            Console.WriteLine(" |");
        }

        /// <summary>
        /// Gibt einen Tabellen-Header aus
        /// </summary>
        public static void WriteTableHeader(params string[] columns)
        {
            WriteTableRow(columns);
            WriteSeparator();
        }

        /// <summary>
        /// Fragt den Benutzer nach Bestätigung (Ja/Nein)
        /// </summary>
        public static bool Confirm(string message, bool defaultValue = false)
        {
            string prompt = defaultValue ? "[J/n]" : "[j/N]";
            Console.Write($"{message} {prompt}: ");
            
            var input = Console.ReadLine()?.Trim().ToLower();
            
            if (string.IsNullOrEmpty(input))
                return defaultValue;
            
            return input == "j" || input == "ja" || input == "y" || input == "yes";
        }

        /// <summary>
        /// Zeigt einen Login-Bildschirm an
        /// </summary>
        public static void DisplayLoginScreen()
        {
            Console.Clear();
            WriteHeader("SlotOS - Benutzer-Login");
            Console.WriteLine();
        }

        /// <summary>
        /// Formatiert die Zeitspanne für Anzeige
        /// </summary>
        public static string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays} Tag(e)";
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours} Stunde(n)";
            if (span.TotalMinutes >= 1)
                return $"{(int)span.TotalMinutes} Minute(n)";
            return $"{(int)span.TotalSeconds} Sekunde(n)";
        }

        /// <summary>
        /// Formatiert die UserRole für Anzeige
        /// </summary>
        public static string FormatRole(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin:
                    return "Administrator";
                case UserRole.Standard:
                    return "Standard-Benutzer";
                case UserRole.Guest:
                    return "Gast";
                default:
                    return role.ToString();
            }
        }

        /// <summary>
        /// Formatiert einen Status (aktiv/inaktiv) für Anzeige
        /// </summary>
        public static string FormatStatus(bool isActive)
        {
            return isActive ? "Aktiv" : "Inaktiv";
        }

        /// <summary>
        /// Padded einen String auf eine bestimmte Länge
        /// </summary>
        public static string PadRight(string text, int length)
        {
            if (text.Length >= length)
                return text.Substring(0, length);
            return text + new string(' ', length - text.Length);
        }

        /// <summary>
        /// Truncated einen String auf eine bestimmte Länge
        /// </summary>
        public static string Truncate(string text, int maxLength)
        {
            if (text == null)
                return string.Empty;
            if (text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
