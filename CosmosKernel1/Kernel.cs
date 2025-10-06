using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using CosmosKernel1.UserManagement;

namespace CosmosKernel1
{
    public class Kernel : Sys.Kernel
    {
        private UserDatabase userDatabase;
        private AuthenticationManager authManager;
        private PermissionManager permissionManager;
        private FilePermissionManager filePermissionManager;
        private CommandHandler commandHandler;

        protected override void BeforeRun()
        {
            Console.Clear();
            Console.WriteLine("===========================================");
            Console.WriteLine("   SlotOS - Cosmos Kernel mit Benutzerverwaltung");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            // Initialisiere Benutzerverwaltung
            InitializeUserManagement();

            Console.WriteLine("System erfolgreich gestartet.");
            Console.WriteLine();

            // Login-Prozess
            PerformLogin();
        }

        protected override void Run()
        {
            if (!authManager.IsLoggedIn)
            {
                Console.WriteLine("Fehler: Nicht eingeloggt. System wird beendet.");
                Stop();
                return;
            }

            // Zeige Prompt
            var user = authManager.CurrentUser;
            Console.Write($"{user.Username}@SlotOS> ");
            
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            // Behandle spezielle Befehle
            if (input.Trim().ToLower() == "logout")
            {
                authManager.Logout();
                Console.WriteLine("Erfolgreich abgemeldet.");
                Console.WriteLine();
                PerformLogin();
                return;
            }

            if (input.Trim().ToLower() == "exit" || input.Trim().ToLower() == "shutdown")
            {
                Console.WriteLine("System wird heruntergefahren...");
                authManager.Logout();
                Stop();
                return;
            }

            // Verarbeite Benutzer-Befehle
            commandHandler.ProcessCommand(input);
        }

        /// <summary>
        /// Initialisiert das Benutzerverwaltungssystem
        /// </summary>
        private void InitializeUserManagement()
        {
            try
            {
                userDatabase = new UserDatabase();
                authManager = new AuthenticationManager(userDatabase);
                permissionManager = new PermissionManager(userDatabase, authManager);
                filePermissionManager = new FilePermissionManager(authManager, userDatabase);
                commandHandler = new CommandHandler(authManager, permissionManager, filePermissionManager, userDatabase);

                Console.WriteLine("Benutzerverwaltung initialisiert.");
                Console.WriteLine($"Standard-Admin: Benutzername='admin', Passwort='admin'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der Initialisierung: {ex.Message}");
            }
        }

        /// <summary>
        /// Führt den Login-Prozess durch
        /// </summary>
        private void PerformLogin()
        {
            int maxAttempts = 3;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Console.WriteLine("=== Login ===");
                Console.Write("Benutzername: ");
                string username = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(username))
                {
                    continue;
                }

                Console.Write("Passwort: ");
                string password = ReadPassword();

                if (authManager.Login(username, password))
                {
                    Console.WriteLine();
                    Console.WriteLine($"Willkommen, {username}!");
                    Console.WriteLine($"Rolle: {authManager.CurrentUser.Role}");
                    Console.WriteLine();
                    Console.WriteLine("Geben Sie 'help' ein, um verfuegbare Befehle anzuzeigen.");
                    Console.WriteLine();
                    return;
                }
                else
                {
                    attempts++;
                    Console.WriteLine();
                    Console.WriteLine($"Login fehlgeschlagen. Verbleibende Versuche: {maxAttempts - attempts}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Maximale Anzahl an Login-Versuchen erreicht. System wird beendet.");
            Stop();
        }

        /// <summary>
        /// Liest ein Passwort mit maskierter Eingabe
        /// </summary>
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
