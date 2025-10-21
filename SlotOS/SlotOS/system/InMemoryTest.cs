using System;
using System.Collections.Generic;
using System.Linq;

namespace SlotOS.System
{
    /// <summary>
    /// Tests für Phase 4: In-Memory-Modus
    /// Testet die RAM-basierte Benutzerverwaltung ohne Persistenz
    /// </summary>
    public static class InMemoryTest
    {
        private static int _testsRun = 0;
        private static int _testsPassed = 0;
        private static int _testsFailed = 0;

        /// <summary>
        /// Führt alle In-Memory Tests aus
        /// </summary>
        public static void RunAllTests()
        {
            _testsRun = 0;
            _testsPassed = 0;
            _testsFailed = 0;

            Console.WriteLine();
            Console.WriteLine("=" + new string('=', 60));
            Console.WriteLine("Phase 4: In-Memory-Modus Tests");
            Console.WriteLine("=" + new string('=', 60));
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("HINWEIS: Diese Tests validieren den In-Memory-Betrieb.");
            Console.WriteLine("Keine Persistenz - Alle Daten nur im RAM.");
            Console.ResetColor();
            Console.WriteLine();

            // Initialisierungs-Tests
            Console.WriteLine("--- Initialisierung Tests ---");
            RunTest("Initialize ohne VFS", Test_Initialize_NoVFS);
            RunTest("Standard-Admin wird erstellt", Test_DefaultAdmin_Created);
            RunTest("Standard-Admin ist Admin-Rolle", Test_DefaultAdmin_IsAdmin);
            RunTest("Standard-Admin Login funktioniert", Test_DefaultAdmin_Login);
            Console.WriteLine();

            // In-Memory-Operationen
            Console.WriteLine("--- In-Memory CRUD-Operationen ---");
            RunTest("CreateUser im RAM", Test_CreateUser_InMemory);
            RunTest("DeleteUser im RAM", Test_DeleteUser_InMemory);
            RunTest("UpdateUser im RAM", Test_UpdateUser_InMemory);
            RunTest("Mehrere Benutzer verwalten", Test_MultipleUsers_InMemory);
            RunTest("GetAllUsers funktioniert", Test_GetAllUsers_InMemory);
            Console.WriteLine();

            // Neustart-Simulation
            Console.WriteLine("--- Neustart-Simulation ---");
            RunTest("Daten gehen bei Neustart verloren", Test_DataLoss_OnRestart);
            RunTest("Standard-Admin nach Neustart", Test_DefaultAdmin_AfterRestart);
            RunTest("Benutzer-Anzahl nach Neustart", Test_UserCount_AfterRestart);
            Console.WriteLine();

            // Performance-Tests
            Console.WriteLine("--- Performance Tests ---");
            RunTest("100 Benutzer erstellen", Test_Performance_100Users);
            RunTest("Schnelle CRUD-Operationen", Test_Performance_FastCRUD);
            Console.WriteLine();

            // Memory-Management
            Console.WriteLine("--- Memory-Management ---");
            RunTest("ClearAllUsers funktioniert", Test_ClearAllUsers);
            RunTest("Keine Memory-Leaks bei vielen Ops", Test_NoMemoryLeaks);
            Console.WriteLine();

            // Zusammenfassung
            PrintSummary();
        }

        private static void RunTest(string testName, Func<bool> testFunc)
        {
            _testsRun++;
            try
            {
                bool result = testFunc();
                if (result)
                {
                    _testsPassed++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✅ PASS - {testName}");
                    Console.ResetColor();
                }
                else
                {
                    _testsFailed++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ FAIL - {testName}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                _testsFailed++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ FAIL - {testName} (Exception: {ex.Message})");
                Console.ResetColor();
            }
        }

        private static void PrintSummary()
        {
            Console.WriteLine();
            Console.WriteLine("=" + new string('=', 60));
            Console.WriteLine($"Tests abgeschlossen: {_testsPassed}/{_testsRun} erfolgreich");
            
            if (_testsFailed == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Alle Tests bestanden!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ {_testsFailed} Test(s) fehlgeschlagen");
                Console.ResetColor();
            }
            
            Console.WriteLine("=" + new string('=', 60));
        }

        #region Initialisierungs-Tests

        private static bool Test_Initialize_NoVFS()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            
            // Initialize ohne Parameter (In-Memory-Modus)
            manager.Initialize();
            
            // Manager sollte funktionieren
            return manager.UserCount >= 1; // Mindestens Standard-Admin
        }

        private static bool Test_DefaultAdmin_Created()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // Standard-Admin sollte existieren
            return manager.UserExists(UserManager.DEFAULT_ADMIN_USERNAME);
        }

        private static bool Test_DefaultAdmin_IsAdmin()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            var admin = manager.GetUser(UserManager.DEFAULT_ADMIN_USERNAME);
            return admin != null && admin.Role == UserRole.Admin && admin.IsActive;
        }

        private static bool Test_DefaultAdmin_Login()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            var admin = manager.GetUser(UserManager.DEFAULT_ADMIN_USERNAME);
            if (admin == null) return false;
            
            // Passwort sollte "admin" sein
            return admin.VerifyPassword(UserManager.DEFAULT_ADMIN_PASSWORD);
        }

        #endregion

        #region In-Memory CRUD-Tests

        private static bool Test_CreateUser_InMemory()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            int countBefore = manager.UserCount;
            
            // Benutzer erstellen
            bool created = manager.CreateUser("testuser", "password123", UserRole.Standard);
            
            if (!created) return false;
            
            int countAfter = manager.UserCount;
            
            // Benutzer sollte im RAM existieren
            return countAfter == countBefore + 1 && manager.UserExists("testuser");
        }

        private static bool Test_DeleteUser_InMemory()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // Benutzer erstellen und löschen
            manager.CreateUser("tobedeleted", "pass123", UserRole.Standard);
            
            bool deleted = manager.DeleteUser("tobedeleted");
            
            // Benutzer sollte nicht mehr existieren
            return deleted && !manager.UserExists("tobedeleted");
        }

        private static bool Test_UpdateUser_InMemory()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // Benutzer erstellen
            manager.CreateUser("updatetest", "pass123", UserRole.Standard);
            
            var user = manager.GetUser("updatetest");
            if (user == null) return false;
            
            // Rolle ändern
            user.Role = UserRole.Admin;
            bool updated = manager.UpdateUser(user);
            
            // Änderung sollte im RAM gespeichert sein
            var updatedUser = manager.GetUser("updatetest");
            return updated && updatedUser != null && updatedUser.Role == UserRole.Admin;
        }

        private static bool Test_MultipleUsers_InMemory()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // 10 Benutzer erstellen
            for (int i = 1; i <= 10; i++)
            {
                manager.CreateUser($"user{i}", $"pass{i}", UserRole.Standard);
            }
            
            // Alle sollten im RAM sein
            return manager.UserCount >= 11; // 10 + Standard-Admin
        }

        private static bool Test_GetAllUsers_InMemory()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            manager.CreateUser("user1", "pass1", UserRole.Standard);
            manager.CreateUser("user2", "pass2", UserRole.Admin);
            manager.CreateUser("user3", "pass3", UserRole.Guest);
            
            var allUsers = manager.GetAllUsers();
            
            // Sollte alle Benutzer zurückgeben (inkl. Standard-Admin)
            return allUsers != null && allUsers.Count >= 4;
        }

        #endregion

        #region Neustart-Simulation

        private static bool Test_DataLoss_OnRestart()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // Benutzer erstellen
            manager.CreateUser("lostuser", "password", UserRole.Standard);
            
            bool existsBefore = manager.UserExists("lostuser");
            
            // "Neustart" simulieren
            manager.ClearAllUsers();
            manager.Initialize();
            
            bool existsAfter = manager.UserExists("lostuser");
            
            // Benutzer sollte verloren sein (außer Standard-Admin)
            return existsBefore && !existsAfter;
        }

        private static bool Test_DefaultAdmin_AfterRestart()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // "Neustart" simulieren
            manager.ClearAllUsers();
            manager.Initialize();
            
            // Standard-Admin sollte wieder da sein
            return manager.UserExists(UserManager.DEFAULT_ADMIN_USERNAME);
        }

        private static bool Test_UserCount_AfterRestart()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // 5 Benutzer erstellen
            for (int i = 1; i <= 5; i++)
            {
                manager.CreateUser($"temp{i}", "pass", UserRole.Standard);
            }
            
            int countBefore = manager.UserCount;
            
            // "Neustart"
            manager.ClearAllUsers();
            manager.Initialize();
            
            int countAfter = manager.UserCount;
            
            // Nur Standard-Admin sollte übrig sein
            return countBefore >= 6 && countAfter == 1;
        }

        #endregion

        #region Performance-Tests

        private static bool Test_Performance_100Users()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // 100 Benutzer erstellen (sollte schnell sein im RAM)
            for (int i = 1; i <= 100; i++)
            {
                bool created = manager.CreateUser($"perf{i}", $"pass{i}", UserRole.Standard);
                if (!created) return false;
            }
            
            // Alle sollten im RAM sein
            return manager.UserCount >= 101;
        }

        private static bool Test_Performance_FastCRUD()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // Schnelle CRUD-Operationen
            manager.CreateUser("fast1", "pass", UserRole.Standard);
            manager.CreateUser("fast2", "pass", UserRole.Standard);
            var user = manager.GetUser("fast1");
            user.Role = UserRole.Admin;
            manager.UpdateUser(user);
            manager.DeleteUser("fast2");
            
            // Alles sollte funktioniert haben
            return manager.UserExists("fast1") && !manager.UserExists("fast2");
        }

        #endregion

        #region Memory-Management

        private static bool Test_ClearAllUsers()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            manager.CreateUser("user1", "pass", UserRole.Standard);
            manager.CreateUser("user2", "pass", UserRole.Standard);
            
            int countBefore = manager.UserCount;
            
            // Alle löschen
            manager.ClearAllUsers();
            
            int countAfter = manager.UserCount;
            
            // Sollte leer sein
            return countBefore >= 3 && countAfter == 0;
        }

        private static bool Test_NoMemoryLeaks()
        {
            var manager = UserManager.Instance;
            manager.ClearAllUsers();
            manager.Initialize();
            
            // Viele Operationen durchführen
            for (int cycle = 0; cycle < 10; cycle++)
            {
                for (int i = 0; i < 20; i++)
                {
                    manager.CreateUser($"leak{cycle}_{i}", "pass", UserRole.Standard);
                }
                
                // Wieder löschen
                for (int i = 0; i < 20; i++)
                {
                    manager.DeleteUser($"leak{cycle}_{i}");
                }
            }
            
            // Sollte nur Standard-Admin übrig haben
            return manager.UserCount == 1;
        }

        #endregion
    }
}
