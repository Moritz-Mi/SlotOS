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
            Console.WriteLine("SlotOS gestartet!");
            Console.WriteLine("Gib 'test' ein, um das Nutzerverwaltungssystem zu testen.");
            Console.WriteLine("Gib 'help' ein für eine Liste der Befehle.");
            Console.WriteLine();

            // Initialisiere User Management System
            userManager = UserManager.Instance;
            userManager.Initialize(); // In-Memory-Modus
            
            authManager = new AuthenticationManager();
            // FIX: Verwende GetInternalUserList() für gemeinsame Referenz
            authManager.SetUsers(userManager.GetInternalUserList());
            commandHandler = new CommandHandler(userManager, authManager);
        }

        protected override void Run()
        {
            Console.Write("SlotOS> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            var command = input.Trim().ToLower();

            // Versuche zuerst, den Befehl mit CommandHandler zu verarbeiten
            if (commandHandler != null && commandHandler.ProcessCommand(input))
            {
                return; // Befehl wurde von CommandHandler verarbeitet
            }

            // System-Befehle
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

                case "help":
                    Console.WriteLine();
                    Console.WriteLine("Verfügbare System-Befehle:");
                    Console.WriteLine("  test          - Führt Tests für Phase 1-3 aus");
                    Console.WriteLine("  testp4        - Führt In-Memory Tests für Phase 4 aus");
                    Console.WriteLine("  testp5        - Führt Command Handler Tests für Phase 5 aus");
                    Console.WriteLine("  userhelp      - Zeigt Benutzerverwaltungs-Befehle an");
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
