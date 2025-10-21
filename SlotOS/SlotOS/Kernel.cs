using System;
using System.Collections.Generic;
using System.Text;
using SlotOS.System;
using Sys = Cosmos.System;

namespace SlotOS
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("SlotOS gestartet!");
            Console.WriteLine("Gib 'test' ein, um das Nutzerverwaltungssystem zu testen.");
            Console.WriteLine("Gib 'help' ein für eine Liste der Befehle.");
            Console.WriteLine();
        }

        protected override void Run()
        {
            Console.Write("SlotOS> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            var command = input.Trim().ToLower();

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

                case "help":
                    Console.WriteLine();
                    Console.WriteLine("Verfügbare Befehle:");
                    Console.WriteLine("  test       - Führt Tests für Phase 1-3 aus");
                    Console.WriteLine("  testp4     - Führt In-Memory Tests für Phase 4 aus");
                    Console.WriteLine("  help       - Zeigt diese Hilfe an");
                    Console.WriteLine("  clear      - Löscht den Bildschirm");
                    Console.WriteLine("  exit       - Beendet das System");
                    Console.WriteLine();
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
