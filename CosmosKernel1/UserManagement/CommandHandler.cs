using System;

namespace CosmosKernel1.UserManagement
{
    /// <summary>
    /// Verarbeitet Benutzerverwaltungs-Befehle
    /// </summary>
    public class CommandHandler
    {
        private AuthenticationManager authManager;
        private PermissionManager permissionManager;
        private FilePermissionManager filePermissionManager;
        private UserDatabase userDatabase;

        public CommandHandler(AuthenticationManager auth, PermissionManager perm, 
            FilePermissionManager filePerm, UserDatabase db)
        {
            authManager = auth;
            permissionManager = perm;
            filePermissionManager = filePerm;
            userDatabase = db;
        }

        /// <summary>
        /// Verarbeitet einen Befehl
        /// </summary>
        public void ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            string[] parts = command.Trim().Split(' ');
            string cmd = parts[0].ToLower();

            try
            {
                switch (cmd)
                {
                    case "whoami":
                        CommandWhoAmI();
                        break;

                    case "adduser":
                        if (parts.Length < 3)
                        {
                            Console.WriteLine("Verwendung: adduser <username> <password> [role]");
                            return;
                        }
                        CommandAddUser(parts);
                        break;

                    case "passwd":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Verwendung: passwd <neues_passwort>");
                            return;
                        }
                        CommandChangePassword(parts[1]);
                        break;

                    case "listusers":
                        CommandListUsers();
                        break;

                    case "userinfo":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Verwendung: userinfo <username>");
                            return;
                        }
                        CommandUserInfo(parts[1]);
                        break;

                    case "grant":
                        if (parts.Length < 3)
                        {
                            Console.WriteLine("Verwendung: grant <username> <permission>");
                            return;
                        }
                        CommandGrantPermission(parts[1], parts[2]);
                        break;

                    case "revoke":
                        if (parts.Length < 3)
                        {
                            Console.WriteLine("Verwendung: revoke <username> <permission>");
                            return;
                        }
                        CommandRevokePermission(parts[1], parts[2]);
                        break;

                    case "chmod":
                        if (parts.Length < 3)
                        {
                            Console.WriteLine("Verwendung: chmod <datei> <rechte>");
                            return;
                        }
                        CommandChmod(parts[1], parts[2]);
                        break;

                    case "chown":
                        if (parts.Length < 3)
                        {
                            Console.WriteLine("Verwendung: chown <datei> <username>");
                            return;
                        }
                        CommandChown(parts[1], parts[2]);
                        break;

                    case "fileinfo":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Verwendung: fileinfo <datei>");
                            return;
                        }
                        CommandFileInfo(parts[1]);
                        break;

                    case "deactivate":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Verwendung: deactivate <username>");
                            return;
                        }
                        CommandDeactivateUser(parts[1]);
                        break;

                    case "activate":
                        if (parts.Length < 2)
                        {
                            Console.WriteLine("Verwendung: activate <username>");
                            return;
                        }
                        CommandActivateUser(parts[1]);
                        break;

                    case "help":
                        CommandHelp();
                        break;

                    case "logout":
                        // Wird im Kernel behandelt
                        break;

                    default:
                        Console.WriteLine($"Unbekannter Befehl: {cmd}");
                        Console.WriteLine("Geben Sie 'help' ein, um verfuegbare Befehle anzuzeigen.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Ausfuehren des Befehls: {ex.Message}");
            }
        }

        private void CommandWhoAmI()
        {
            if (!authManager.IsLoggedIn)
            {
                Console.WriteLine("Nicht eingeloggt.");
                return;
            }

            var user = authManager.CurrentUser;
            Console.WriteLine($"Eingeloggt als: {user.Username}");
            Console.WriteLine($"Rolle: {user.Role}");
            Console.WriteLine($"User-ID: {user.UserId}");
            Console.WriteLine($"Letzter Login: {user.LastLoginDate}");
        }

        private void CommandAddUser(string[] parts)
        {
            string username = parts[1];
            string password = parts[2];
            UserRole role = UserRole.User;

            if (parts.Length > 3)
            {
                if (!Enum.TryParse(parts[3], true, out role))
                {
                    Console.WriteLine("Ungueltige Rolle. Verfuegbare Rollen: Guest, User, PowerUser, Administrator");
                    return;
                }
            }

            if (authManager.CreateNewUser(username, password, role))
            {
                Console.WriteLine($"Benutzer '{username}' wurde erfolgreich erstellt.");
            }
            else
            {
                Console.WriteLine($"Fehler: Benutzer konnte nicht erstellt werden. (Keine Berechtigung oder Benutzer existiert bereits)");
            }
        }

        private void CommandChangePassword(string newPassword)
        {
            Console.Write("Aktuelles Passwort: ");
            string oldPassword = ReadPassword();

            if (authManager.ChangePassword(oldPassword, newPassword))
            {
                Console.WriteLine("Passwort erfolgreich geaendert.");
            }
            else
            {
                Console.WriteLine("Fehler: Passwort konnte nicht geaendert werden. (Falsches aktuelles Passwort)");
            }
        }

        private void CommandListUsers()
        {
            if (!authManager.CheckPermission(Permission.SystemAdmin))
            {
                Console.WriteLine("Keine Berechtigung zum Auflisten aller Benutzer.");
                return;
            }

            var users = userDatabase.GetAllUsers();
            Console.WriteLine($"\n=== Benutzerliste ({users.Count} Benutzer) ===");
            
            foreach (var user in users)
            {
                string status = user.IsActive ? "Aktiv" : "Deaktiviert";
                Console.WriteLine($"  {user.UserId,3} | {user.Username,-15} | {user.Role,-13} | {status}");
            }
            Console.WriteLine();
        }

        private void CommandUserInfo(string username)
        {
            string info = permissionManager.GetUserPermissionsInfo(username);
            Console.WriteLine(info);
        }

        private void CommandGrantPermission(string username, string permString)
        {
            if (!Enum.TryParse(permString, true, out Permission perm))
            {
                Console.WriteLine("Ungueltige Berechtigung.");
                return;
            }

            if (permissionManager.GrantPermission(username, perm))
            {
                Console.WriteLine($"Berechtigung '{perm}' wurde '{username}' gewaehrt.");
            }
            else
            {
                Console.WriteLine("Fehler: Berechtigung konnte nicht gewaehrt werden.");
            }
        }

        private void CommandRevokePermission(string username, string permString)
        {
            if (!Enum.TryParse(permString, true, out Permission perm))
            {
                Console.WriteLine("Ungueltige Berechtigung.");
                return;
            }

            if (permissionManager.RevokePermission(username, perm))
            {
                Console.WriteLine($"Berechtigung '{perm}' wurde '{username}' entzogen.");
            }
            else
            {
                Console.WriteLine("Fehler: Berechtigung konnte nicht entzogen werden.");
            }
        }

        private void CommandChmod(string filePath, string permissions)
        {
            // Vereinfachte chmod-Implementierung
            Console.WriteLine($"chmod {permissions} auf {filePath} - Feature in Entwicklung");
        }

        private void CommandChown(string filePath, string username)
        {
            var user = userDatabase.GetUserByUsername(username);
            if (user == null)
            {
                Console.WriteLine("Benutzer nicht gefunden.");
                return;
            }

            if (filePermissionManager.ChangeOwner(filePath, user.UserId))
            {
                Console.WriteLine($"Besitzer von '{filePath}' wurde zu '{username}' geaendert.");
            }
            else
            {
                Console.WriteLine("Fehler: Besitzer konnte nicht geaendert werden.");
            }
        }

        private void CommandFileInfo(string filePath)
        {
            string info = filePermissionManager.GetFilePermissionInfo(filePath);
            Console.WriteLine(info);
        }

        private void CommandDeactivateUser(string username)
        {
            if (permissionManager.DeactivateUser(username))
            {
                Console.WriteLine($"Benutzer '{username}' wurde deaktiviert.");
            }
            else
            {
                Console.WriteLine("Fehler: Benutzer konnte nicht deaktiviert werden.");
            }
        }

        private void CommandActivateUser(string username)
        {
            if (permissionManager.ActivateUser(username))
            {
                Console.WriteLine($"Benutzer '{username}' wurde aktiviert.");
            }
            else
            {
                Console.WriteLine("Fehler: Benutzer konnte nicht aktiviert werden.");
            }
        }

        private void CommandHelp()
        {
            Console.WriteLine("\n=== Verfuegbare Befehle ===");
            Console.WriteLine("Allgemein:");
            Console.WriteLine("  whoami                       - Zeigt aktuellen Benutzer");
            Console.WriteLine("  logout                       - Abmelden");
            Console.WriteLine("  help                         - Zeigt diese Hilfe");
            Console.WriteLine("\nBenutzerverwaltung:");
            Console.WriteLine("  adduser <user> <pw> [rolle]  - Neuen Benutzer erstellen");
            Console.WriteLine("  passwd <neues_pw>            - Passwort aendern");
            Console.WriteLine("  listusers                    - Alle Benutzer auflisten (Admin)");
            Console.WriteLine("  userinfo <user>              - Benutzerinfo anzeigen");
            Console.WriteLine("  deactivate <user>            - Benutzer deaktivieren");
            Console.WriteLine("  activate <user>              - Benutzer aktivieren");
            Console.WriteLine("\nRechteverwaltung:");
            Console.WriteLine("  grant <user> <berechtigung>  - Berechtigung gewaehren");
            Console.WriteLine("  revoke <user> <berechtigung> - Berechtigung entziehen");
            Console.WriteLine("\nDateisystem:");
            Console.WriteLine("  chmod <datei> <rechte>       - Dateiberechtigungen aendern");
            Console.WriteLine("  chown <datei> <user>         - Dateibesitzer aendern");
            Console.WriteLine("  fileinfo <datei>             - Dateirechte anzeigen");
            Console.WriteLine();
        }

        private string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }
    }
}
