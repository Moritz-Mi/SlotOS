using System;
using System.Collections.Generic;
using System.Text;
using SlotOS.System;
using Sys = Cosmos.System;

namespace SlotOS
{
    public class Kernel : Sys.Kernel
    {
        private UserManager userManager;
        private AuthenticationManager authManager;
        private CommandHandler commandHandler;

        protected override void BeforeRun()
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.WriteLine("   SlotOS - User Management System   ");
            Console.WriteLine("======================================");
            Console.WriteLine();

            // Initialisiere User Management System
            userManager = UserManager.Instance;
            userManager.Initialize(); // In-Memory-Modus
            
            authManager = new AuthenticationManager();
            // FIX: Verwende GetInternalUserList() für gemeinsame Referenz
            authManager.SetUsers(userManager.GetInternalUserList());
            commandHandler = new CommandHandler(userManager, authManager);

            // Zeige Login-Screen
            Console.WriteLine("Bitte melden Sie sich an, um das System zu nutzen.");
            Console.WriteLine("Standard-Login: admin / admin");
            Console.WriteLine("Geben Sie 'login' ein, um sich anzumelden.");
            Console.WriteLine();
        }

        protected override void Run()
        {
            // Prüfe Session-Timeout (30 Minuten Inaktivität)
            if (authManager.IsAuthenticated && authManager.CheckAndHandleSessionTimeout(30))
            {
                ConsoleHelper.WriteWarning("Session abgelaufen aufgrund von Inaktivität");
                AuditLogger.Instance.LogSessionTimeout(authManager.GetCurrentUsername());
                ConsoleHelper.WriteInfo("Bitte melden Sie sich erneut an.");
            }

            // Zeige Prompt mit Benutzername wenn angemeldet
            if (authManager.IsAuthenticated)
            {
                Console.Write($"{authManager.CurrentUser.Username}@SlotOS> ");
            }
            else
            {
                Console.Write("SlotOS (nicht angemeldet)> ");
            }

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            var command = input.Trim().ToLower();

            // Versuche zuerst, den Befehl mit CommandHandler zu verarbeiten
            if (commandHandler != null && commandHandler.ProcessCommand(input))
            {
                return; // Befehl wurde von CommandHandler verarbeitet
            }

            // System-Befehle (einige erfordern Authentication)
            switch (command)
            {
                case "test":
                    // Führe Systemtests aus (Phase 1-3)
                    Console.WriteLine();
                    UserSystemTest.RunAllTests();
                    Console.WriteLine();
                    break;

                case "testmemory":
                case "testp4":
                    // Führe In-Memory Tests aus (Phase 4)
                    Console.WriteLine();
                    InMemoryTest.RunAllTests();
                    Console.WriteLine();
                    break;

                case "testp5":
                case "testcommands":
                    // Führe Command Handler Tests aus (Phase 5)
                    Console.WriteLine();
                    CommandHandlerTest.RunAllTests();
                    Console.WriteLine();
                    break;

                case "testp6":
                case "testpermissions":
                    // Führe Permission Checker Tests aus (Phase 6)
                    Console.WriteLine();
                    PermissionCheckerTest.RunAllTests();
                    Console.WriteLine();
                    break;

                case "auditlog":
                    // Zeige Audit-Log (nur Admin)
                    if (!authManager.IsAuthenticated)
                    {
                        ConsoleHelper.WriteError("Sie müssen angemeldet sein");
                    }
                    else if (authManager.CurrentUser.Role != UserRole.Admin)
                    {
                        ConsoleHelper.WriteError("Dieser Befehl erfordert Administrator-Rechte");
                    }
                    else
                    {
                        Console.WriteLine();
                        AuditLogger.Instance.FormatLogs(20); // Gibt direkt auf Console aus
                        Console.WriteLine();
                    }
                    break;

                case "help":
                    Console.WriteLine();
                    Console.WriteLine("Verfügbare System-Befehle:");
                    Console.WriteLine("  login         - Meldet einen Benutzer an");
                    Console.WriteLine("  test          - Führt Tests für Phase 1-3 aus");
                    Console.WriteLine("  testp4        - Führt In-Memory Tests für Phase 4 aus");
                    Console.WriteLine("  testp5        - Führt Command Handler Tests für Phase 5 aus");
                    Console.WriteLine("  testp6        - Führt Permission Checker Tests für Phase 6 aus");
                    Console.WriteLine("  userhelp      - Zeigt Benutzerverwaltungs-Befehle an");
                    Console.WriteLine("  auditlog      - Zeigt Audit-Log an (nur Admin)");
                    Console.WriteLine("  help          - Zeigt diese Hilfe an");
                    Console.WriteLine("  clear         - Löscht den Bildschirm");
                    Console.WriteLine("  exit          - Beendet das System");
                    Console.WriteLine();
                    break;

                case "userhelp":
                    Console.WriteLine();
                    commandHandler?.ShowHelp();
                    break;

                case "clear":
                    Console.Clear();
                    break;

                case "exit":
                    if (authManager.IsAuthenticated)
                    {
                        ConsoleHelper.WriteInfo($"Benutzer '{authManager.CurrentUser.Username}' wird abgemeldet...");
                        AuditLogger.Instance.LogLogout(authManager.CurrentUser.Username);
                        authManager.Logout();
                    }
                    Console.WriteLine("System wird heruntergefahren...");
                    Sys.Power.Shutdown();
                    break;

                default:
                    Console.WriteLine($"Unbekannter Befehl: '{input}'. Gib 'help' ein für Hilfe.");
                    break;
            }
        }
    }
}
