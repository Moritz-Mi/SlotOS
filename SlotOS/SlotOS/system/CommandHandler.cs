using System;
using System.Collections.Generic;
using System.Text;

namespace SlotOS.System
{
    /// <summary>
    /// Behandelt alle Benutzerverwaltungs-Befehle
    /// Führt Berechtigungsprüfungen durch und leitet Befehle an entsprechende Manager weiter
    /// </summary>
    public class CommandHandler
    {
        private readonly UserManager userManager;
        private readonly AuthenticationManager authManager;

        public CommandHandler(UserManager userMgr, AuthenticationManager authMgr)
        {
            userManager = userMgr ?? throw new ArgumentNullException(nameof(userMgr));
            authManager = authMgr ?? throw new ArgumentNullException(nameof(authMgr));
        }

        /// <summary>
        /// Verarbeitet einen Befehl
        /// </summary>
        /// <param name="input">Der Befehlsstring</param>
        /// <returns>True wenn der Befehl erkannt wurde, sonst false</returns>
        public bool ProcessCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var parts = ParseCommand(input);
            if (parts.Length == 0)
                return false;

            var command = parts[0].ToLower();
            
            // Cosmos OS compatible: avoid LINQ Skip
            var args = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
            {
                args[i - 1] = parts[i];
            }

            switch (command)
            {
                case "useradd":
                    HandleUserAdd(args);
                    return true;

                case "userdel":
                    HandleUserDel(args);
                    return true;

                case "usermod":
                    HandleUserMod(args);
                    return true;

                case "userlist":
                    HandleUserList(args);
                    return true;

                case "passwd":
                    HandlePasswd(args);
                    return true;

                case "whoami":
                    HandleWhoAmI();
                    return true;

                case "logout":
                    HandleLogout();
                    return true;

                case "login":
                    HandleLogin();
                    return true;

                case "userstats":
                    HandleUserStats();
                    return true;

                default:
                    return false; // Befehl nicht erkannt
            }
        }

        /// <summary>
        /// Zeigt die Hilfe für Benutzerverwaltungs-Befehle an
        /// </summary>
        public void ShowHelp()
        {
            ConsoleHelper.WriteHeader("Benutzerverwaltungs-Befehle");

            Console.WriteLine("Für alle Benutzer:");
            Console.WriteLine("  whoami              - Zeigt den aktuellen Benutzer an");
            Console.WriteLine("  passwd              - Ändert das eigene Passwort");
            Console.WriteLine("  logout              - Meldet den Benutzer ab");
            Console.WriteLine("  login               - Meldet einen Benutzer an");
            Console.WriteLine();

            if (authManager.IsAuthenticated && authManager.CurrentUser.Role == UserRole.Admin)
            {
                Console.WriteLine("Administrator-Befehle:");
                Console.WriteLine("  useradd <username> <password> [role]  - Erstellt einen neuen Benutzer");
                Console.WriteLine("                                          role: admin, standard, guest (Standard: standard)");
                Console.WriteLine("  userdel <username>                     - Löscht einen Benutzer");
                Console.WriteLine("  usermod <username> <option> <wert>     - Ändert Benutzereigenschaften");
                Console.WriteLine("                                          Optionen: role, active, home");
                Console.WriteLine("  userlist                               - Listet alle Benutzer auf");
                Console.WriteLine("  passwd <username>                      - Setzt Passwort für Benutzer");
                Console.WriteLine("  userstats                              - Zeigt Benutzerstatistiken");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Login-Befehl: Meldet einen Benutzer an
        /// </summary>
        private void HandleLogin()
        {
            if (authManager.IsAuthenticated)
            {
                ConsoleHelper.WriteWarning($"Bereits angemeldet als '{authManager.CurrentUser.Username}'");
                ConsoleHelper.WriteInfo("Benutze 'logout' zum Abmelden");
                return;
            }

            ConsoleHelper.DisplayLoginScreen();

            // Login-Schleife mit max. 3 Versuchen
            int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                Console.Write("Benutzername: ");
                var username = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(username))
                {
                    ConsoleHelper.WriteWarning("Benutzername darf nicht leer sein");
                    continue;
                }

                var password = ConsoleHelper.ReadPassword("Passwort: ");

                if (password == null) // Escape gedrückt
                {
                    ConsoleHelper.WriteInfo("Login abgebrochen");
                    return;
                }

                Console.WriteLine();

                if (authManager.Login(username, password))
                {
                    AuditLogger.Instance.LogLogin(username, true);
                    ConsoleHelper.WriteSuccess($"Willkommen, {authManager.CurrentUser.Username}!");
                    ConsoleHelper.WriteInfo($"Rolle: {ConsoleHelper.FormatRole(authManager.CurrentUser.Role)}");
                    // Cosmos OS compatible: simple concatenation
                    var ll = authManager.CurrentUser.LastLogin;
                    string lly = ll.Year.ToString();
                    string llm = ll.Month.ToString();
                    string lld = ll.Day.ToString();
                    string llh = ll.Hour.ToString();
                    string llmin = ll.Minute.ToString();
                    if (llm.Length == 1) llm = "0" + llm;
                    if (lld.Length == 1) lld = "0" + lld;
                    if (llh.Length == 1) llh = "0" + llh;
                    if (llmin.Length == 1) llmin = "0" + llmin;
                    var lastLoginStr = lly + "-" + llm + "-" + lld + " " + llh + ":" + llmin;
                    ConsoleHelper.WriteInfo($"Letzter Login: {lastLoginStr}");
                    Console.WriteLine();
                    return;
                }
                else
                {
                    AuditLogger.Instance.LogLogin(username, false);
                    int remaining = authManager.GetRemainingLoginAttempts();
                    ConsoleHelper.WriteError($"Login fehlgeschlagen! Verbleibende Versuche: {remaining}");

                    if (remaining == 0)
                    {
                        ConsoleHelper.WriteWarning("Account vorübergehend gesperrt (30 Sekunden)");
                        return;
                    }
                }
            }

            ConsoleHelper.WriteError("Maximale Anzahl von Login-Versuchen erreicht");
        }

        /// <summary>
        /// Logout-Befehl: Meldet den aktuellen Benutzer ab
        /// </summary>
        private void HandleLogout()
        {
            if (!authManager.IsAuthenticated)
            {
                ConsoleHelper.WriteWarning("Sie sind nicht angemeldet");
                return;
            }

            string username = authManager.CurrentUser.Username;
            AuditLogger.Instance.LogLogout(username);
            ConsoleHelper.WriteInfo($"Benutzer '{username}' wurde abgemeldet");
            authManager.Logout();
        }

        /// <summary>
        /// WhoAmI-Befehl: Zeigt Informationen über den aktuellen Benutzer
        /// </summary>
        private void HandleWhoAmI()
        {
            if (!authManager.IsAuthenticated)
            {
                ConsoleHelper.WriteWarning("Sie sind nicht angemeldet");
                return;
            }

            var user = authManager.CurrentUser;
            Console.WriteLine();
            Console.WriteLine($"Benutzername:  {user.Username}");
            Console.WriteLine($"Rolle:         {ConsoleHelper.FormatRole(user.Role)}");
            Console.WriteLine($"Status:        {ConsoleHelper.FormatStatus(user.IsActive)}");
            Console.WriteLine($"Home-Ordner:   {user.HomeDirectory}");
            
            // Cosmos OS compatible: simple concatenation
            var ca = user.CreatedAt;
            string cay = ca.Year.ToString();
            string cam = ca.Month.ToString();
            string cad = ca.Day.ToString();
            string cah = ca.Hour.ToString();
            string camin = ca.Minute.ToString();
            if (cam.Length == 1) cam = "0" + cam;
            if (cad.Length == 1) cad = "0" + cad;
            if (cah.Length == 1) cah = "0" + cah;
            if (camin.Length == 1) camin = "0" + camin;
            var createdStr = cay + "-" + cam + "-" + cad + " " + cah + ":" + camin;
            
            var ll2 = user.LastLogin;
            string ll2y = ll2.Year.ToString();
            string ll2m = ll2.Month.ToString();
            string ll2d = ll2.Day.ToString();
            string ll2h = ll2.Hour.ToString();
            string ll2min = ll2.Minute.ToString();
            if (ll2m.Length == 1) ll2m = "0" + ll2m;
            if (ll2d.Length == 1) ll2d = "0" + ll2d;
            if (ll2h.Length == 1) ll2h = "0" + ll2h;
            if (ll2min.Length == 1) ll2min = "0" + ll2min;
            var lastLoginStr = ll2y + "-" + ll2m + "-" + ll2d + " " + ll2h + ":" + ll2min;
            
            Console.WriteLine($"Erstellt am:   {createdStr}");
            Console.WriteLine($"Letzter Login: {lastLoginStr}");
            Console.WriteLine();
        }

        /// <summary>
        /// Passwd-Befehl: Ändert das Passwort
        /// </summary>
        private void HandlePasswd(string[] args)
        {
            if (!authManager.IsAuthenticated)
            {
                ConsoleHelper.WriteError("Sie müssen angemeldet sein");
                return;
            }

            // Admin kann Passwort für andere Benutzer setzen
            if (args.Length > 0 && authManager.CurrentUser.Role == UserRole.Admin)
            {
                HandleAdminPasswd(args[0]);
                return;
            }

            // Normaler Benutzer ändert eigenes Passwort
            Console.WriteLine();
            ConsoleHelper.WriteInfo("Passwort ändern für: " + authManager.CurrentUser.Username);
            Console.WriteLine();

            var oldPassword = ConsoleHelper.ReadPassword("Altes Passwort: ");
            if (oldPassword == null)
            {
                ConsoleHelper.WriteInfo("Abgebrochen");
                return;
            }

            var newPassword = ConsoleHelper.ReadPassword("Neues Passwort: ");
            if (newPassword == null)
            {
                ConsoleHelper.WriteInfo("Abgebrochen");
                return;
            }

            var confirmPassword = ConsoleHelper.ReadPassword("Passwort bestätigen: ");
            if (confirmPassword == null)
            {
                ConsoleHelper.WriteInfo("Abgebrochen");
                return;
            }

            Console.WriteLine();

            if (newPassword != confirmPassword)
            {
                ConsoleHelper.WriteError("Passwörter stimmen nicht überein");
                return;
            }

            if (userManager.ChangePassword(authManager.CurrentUser.Username, oldPassword, newPassword))
            {
                ConsoleHelper.WriteSuccess("Passwort erfolgreich geändert");
            }
            else
            {
                ConsoleHelper.WriteError("Passwort konnte nicht geändert werden (falsches altes Passwort oder zu kurz)");
            }
        }

        /// <summary>
        /// Admin-Passwort-Reset für anderen Benutzer
        /// </summary>
        private void HandleAdminPasswd(string username)
        {
            if (!RequireAdmin())
                return;

            if (!userManager.UserExists(username))
            {
                ConsoleHelper.WriteError($"Benutzer '{username}' existiert nicht");
                return;
            }

            Console.WriteLine();
            var newPassword = ConsoleHelper.ReadPassword($"Neues Passwort für '{username}': ");
            if (newPassword == null)
            {
                ConsoleHelper.WriteInfo("Abgebrochen");
                return;
            }

            var confirmPassword = ConsoleHelper.ReadPassword("Passwort bestätigen: ");
            if (confirmPassword == null)
            {
                ConsoleHelper.WriteInfo("Abgebrochen");
                return;
            }

            Console.WriteLine();

            if (newPassword != confirmPassword)
            {
                ConsoleHelper.WriteError("Passwörter stimmen nicht überein");
                return;
            }

            bool success = userManager.ResetPassword(username, newPassword);
            AuditLogger.Instance.LogPasswordChange(authManager.CurrentUser.Username, username, success);
            if (success)
            {
                ConsoleHelper.WriteSuccess($"Passwort für '{username}' erfolgreich zurückgesetzt");
            }
            else
            {
                ConsoleHelper.WriteError("Passwort konnte nicht zurückgesetzt werden (zu kurz?)");
            }
        }

        /// <summary>
        /// UserAdd-Befehl: Erstellt einen neuen Benutzer (nur Admin)
        /// </summary>
        private void HandleUserAdd(string[] args)
        {
            if (!RequireAdmin())
                return;

            if (args.Length < 2)
            {
                ConsoleHelper.WriteError("Verwendung: useradd <username> <password> [role]");
                ConsoleHelper.WriteInfo("role: admin, standard, guest (Standard: standard)");
                return;
            }

            var username = args[0];
            var password = args[1];
            var role = UserRole.Standard;

            if (args.Length >= 3)
            {
                if (!ParseRole(args[2], out role))
                {
                    ConsoleHelper.WriteError($"Ungültige Rolle: '{args[2]}'");
                    ConsoleHelper.WriteInfo("Gültige Rollen: admin, standard, guest");
                    return;
                }
            }

            bool success = userManager.CreateUser(username, password, role);
            AuditLogger.Instance.LogUserAction(authManager.CurrentUser.Username, "USER_CREATE", username, success);
            if (success)
            {
                ConsoleHelper.WriteSuccess($"Benutzer '{username}' erfolgreich erstellt");
                ConsoleHelper.WriteInfo($"Rolle: {ConsoleHelper.FormatRole(role)}");
            }
            else
            {
                ConsoleHelper.WriteError($"Benutzer '{username}' konnte nicht erstellt werden");
                ConsoleHelper.WriteWarning("Mögliche Gründe: Benutzer existiert bereits oder Passwort zu kurz");
            }
        }

        /// <summary>
        /// UserDel-Befehl: Löscht einen Benutzer (nur Admin)
        /// </summary>
        private void HandleUserDel(string[] args)
        {
            if (!RequireAdmin())
                return;

            if (args.Length < 1)
            {
                ConsoleHelper.WriteError("Verwendung: userdel <username>");
                return;
            }

            var username = args[0];

            if (!userManager.UserExists(username))
            {
                ConsoleHelper.WriteError($"Benutzer '{username}' existiert nicht");
                return;
            }

            if (username.ToLower() == authManager.CurrentUser.Username.ToLower())
            {
                ConsoleHelper.WriteError("Sie können sich nicht selbst löschen");
                return;
            }

            Console.WriteLine();
            if (!ConsoleHelper.Confirm($"Benutzer '{username}' wirklich löschen?", false))
            {
                ConsoleHelper.WriteInfo("Abgebrochen");
                return;
            }

            bool success = userManager.DeleteUser(username);
            AuditLogger.Instance.LogUserAction(authManager.CurrentUser.Username, "USER_DELETE", username, success);
            if (success)
            {
                ConsoleHelper.WriteSuccess($"Benutzer '{username}' erfolgreich gelöscht");
            }
            else
            {
                ConsoleHelper.WriteError($"Benutzer '{username}' konnte nicht gelöscht werden");
                ConsoleHelper.WriteWarning("Der letzte Administrator kann nicht gelöscht werden");
            }
        }

        /// <summary>
        /// UserMod-Befehl: Ändert Benutzereigenschaften (nur Admin)
        /// </summary>
        private void HandleUserMod(string[] args)
        {
            if (!RequireAdmin())
                return;

            if (args.Length < 3)
            {
                ConsoleHelper.WriteError("Verwendung: usermod <username> <option> <wert>");
                ConsoleHelper.WriteInfo("Optionen:");
                ConsoleHelper.WriteInfo("  role <admin|standard|guest>  - Rolle ändern");
                ConsoleHelper.WriteInfo("  active <true|false>          - Status ändern");
                ConsoleHelper.WriteInfo("  home <pfad>                  - Home-Verzeichnis ändern");
                return;
            }

            var username = args[0];
            var option = args[1].ToLower();
            var value = args[2];

            var user = userManager.GetUser(username);
            if (user == null)
            {
                ConsoleHelper.WriteError($"Benutzer '{username}' existiert nicht");
                return;
            }

            switch (option)
            {
                case "role":
                    if (!ParseRole(value, out UserRole newRole))
                    {
                        ConsoleHelper.WriteError($"Ungültige Rolle: '{value}'");
                        return;
                    }
                    user.Role = newRole;
                    if (userManager.UpdateUser(user))
                    {
                        ConsoleHelper.WriteSuccess($"Rolle von '{username}' geändert zu: {ConsoleHelper.FormatRole(newRole)}");
                    }
                    else
                    {
                        ConsoleHelper.WriteError("Änderung fehlgeschlagen");
                    }
                    break;

                case "active":
                    if (!bool.TryParse(value, out bool isActive))
                    {
                        ConsoleHelper.WriteError($"Ungültiger Wert: '{value}' (erwarte: true oder false)");
                        return;
                    }
                    if (userManager.SetUserActive(username, isActive))
                    {
                        ConsoleHelper.WriteSuccess($"Status von '{username}' geändert zu: {ConsoleHelper.FormatStatus(isActive)}");
                    }
                    else
                    {
                        ConsoleHelper.WriteError("Änderung fehlgeschlagen");
                        ConsoleHelper.WriteWarning("Der letzte Administrator kann nicht deaktiviert werden");
                    }
                    break;

                case "home":
                    user.HomeDirectory = value;
                    if (userManager.UpdateUser(user))
                    {
                        ConsoleHelper.WriteSuccess($"Home-Verzeichnis von '{username}' geändert zu: {value}");
                    }
                    else
                    {
                        ConsoleHelper.WriteError("Änderung fehlgeschlagen");
                    }
                    break;

                default:
                    ConsoleHelper.WriteError($"Unbekannte Option: '{option}'");
                    break;
            }
        }

        /// <summary>
        /// UserList-Befehl: Listet alle Benutzer auf (nur Admin)
        /// </summary>
        private void HandleUserList(string[] args)
        {
            if (!RequireAdmin())
                return;

            var users = userManager.GetAllUsers();

            if (users.Count == 0)
            {
                ConsoleHelper.WriteWarning("Keine Benutzer vorhanden");
                return;
            }

            Console.WriteLine();
            ConsoleHelper.WriteHeader("Benutzerliste");

            // Tabellen-Header
            Console.WriteLine(ConsoleHelper.PadRight("Benutzername", 15) + " " +
                            ConsoleHelper.PadRight("Rolle", 18) + " " +
                            ConsoleHelper.PadRight("Status", 10) + " " +
                            ConsoleHelper.PadRight("Erstellt", 20));
            ConsoleHelper.WriteSeparator();

            // Benutzer auflisten (ohne Sortierung für maximale Cosmos OS Kompatibilität)
            foreach (var user in users)
            {
                var username = ConsoleHelper.PadRight(user.Username, 15);
                var role = ConsoleHelper.PadRight(ConsoleHelper.FormatRole(user.Role), 18);
                var status = ConsoleHelper.PadRight(ConsoleHelper.FormatStatus(user.IsActive), 10);
                // Cosmos OS compatible: simple concatenation without format specifiers
                string year = user.CreatedAt.Year.ToString();
                string month = user.CreatedAt.Month.ToString();
                string day = user.CreatedAt.Day.ToString();
                string hour = user.CreatedAt.Hour.ToString();
                string minute = user.CreatedAt.Minute.ToString();
                if (month.Length == 1) month = "0" + month;
                if (day.Length == 1) day = "0" + day;
                if (hour.Length == 1) hour = "0" + hour;
                if (minute.Length == 1) minute = "0" + minute;
                var created = year + "-" + month + "-" + day + " " + hour + ":" + minute;

                Console.WriteLine($"{username} {role} {status} {created}");
            }

            Console.WriteLine();
            ConsoleHelper.WriteInfo($"Gesamt: {users.Count} Benutzer");
            Console.WriteLine();
        }

        /// <summary>
        /// UserStats-Befehl: Zeigt Benutzerstatistiken (nur Admin)
        /// </summary>
        private void HandleUserStats()
        {
            if (!RequireAdmin())
                return;

            var stats = userManager.GetStatistics();
            Console.WriteLine();
            ConsoleHelper.WriteHeader("Benutzerstatistiken");
            Console.WriteLine(stats);
        }

        // Hilfsmethoden

        /// <summary>
        /// Prüft ob der aktuelle Benutzer Administrator ist
        /// </summary>
        private bool RequireAdmin()
        {
            if (!authManager.IsAuthenticated)
            {
                ConsoleHelper.WriteError("Sie müssen angemeldet sein");
                return false;
            }

            if (authManager.CurrentUser.Role != UserRole.Admin)
            {
                ConsoleHelper.WriteError("Dieser Befehl erfordert Administrator-Rechte");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parst einen String in eine UserRole
        /// </summary>
        private bool ParseRole(string roleStr, out UserRole role)
        {
            role = UserRole.Standard;

            switch (roleStr.ToLower())
            {
                case "admin":
                case "administrator":
                    role = UserRole.Admin;
                    return true;

                case "standard":
                case "user":
                    role = UserRole.Standard;
                    return true;

                case "guest":
                case "gast":
                    role = UserRole.Guest;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Parst einen Befehlsstring in Teile
        /// Unterstützt Anführungszeichen für Argumente mit Leerzeichen
        /// </summary>
        private string[] ParseCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new string[0];

            var parts = new List<string>();
            bool inQuotes = false;
            var currentPart = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (currentPart.Length > 0)
                    {
                        parts.Add(currentPart.ToString());
                        currentPart.Clear();
                    }
                }
                else
                {
                    currentPart.Append(c);
                }
            }

            if (currentPart.Length > 0)
                parts.Add(currentPart.ToString());

            return parts.ToArray();
        }
    }
}
