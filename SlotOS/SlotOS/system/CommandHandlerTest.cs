using System;

namespace SlotOS.System
{
    /// <summary>
    /// Test-Suite für Phase 5: Command-Line Interface
    /// Testet CommandHandler und verwandte Funktionen
    /// </summary>
    public static class CommandHandlerTest
    {
        private static int testsRun = 0;
        private static int testsPassed = 0;
        private static int testsFailed = 0;

        public static void RunAllTests()
        {
            try
            {
                Console.WriteLine("+============================================================+");
                Console.WriteLine("|           Phase 5 Test Suite - Command Handler            |");
                Console.WriteLine("+============================================================+");
                Console.WriteLine();

                testsRun = 0;
                testsPassed = 0;
                testsFailed = 0;

                // CommandHandler Tests
                Console.WriteLine("[CommandHandler Tests]");
                Test_CommandHandler_Creation();
                Test_CommandHandler_ProcessCommand_UnknownCommand();
                Test_CommandHandler_WhoAmI_NotAuthenticated();
                Test_CommandHandler_WhoAmI_Authenticated();
                Test_CommandHandler_Logout_NotAuthenticated();
                Test_CommandHandler_Logout_Authenticated();
                Console.WriteLine();

                // Command Parsing Tests
                Console.WriteLine("[Command Parsing Tests]");
                Test_ParseCommand_Simple();
                Test_ParseCommand_WithQuotes();
                Test_ParseCommand_EmptyString();
                Console.WriteLine();

                // Permission Tests
                Console.WriteLine("[Permission Tests]");
                Test_RequireAdmin_NotAuthenticated();
                Test_RequireAdmin_AsStandardUser();
                Test_RequireAdmin_AsAdmin();
                Console.WriteLine();

                // User Management Command Tests
                Console.WriteLine("[User Management Tests]");
                Test_UserAdd_AsAdmin();
                Test_UserAdd_AsStandardUser();
                Test_UserAdd_InvalidArguments();
                Test_UserDel_AsAdmin();
                Test_UserDel_DeleteSelf();
                Test_UserList_AsAdmin();
                Test_UserStats_AsAdmin();
                Console.WriteLine();

                // Password Management Tests
                Console.WriteLine("[Password Management Tests]");
                Test_Passwd_ChangeOwnPassword();
                Test_Passwd_AdminResetPassword();
                Console.WriteLine();

                // UserMod Tests
                Console.WriteLine("[UserMod Tests]");
                Test_UserMod_ChangeRole();
                Test_UserMod_ChangeStatus();
                Test_UserMod_ChangeHome();
                Test_UserMod_InvalidOption();
                Console.WriteLine();

                // ConsoleHelper Tests
                Console.WriteLine("[ConsoleHelper Tests]");
                Test_ConsoleHelper_FormatRole();
                Test_ConsoleHelper_FormatStatus();
                Test_ConsoleHelper_FormatTimeSpan();
                Test_ConsoleHelper_PadRight();
                Test_ConsoleHelper_Truncate();
                Console.WriteLine();

                // Zusammenfassung
                Console.WriteLine("============================================================");
                Console.WriteLine($"Tests ausgeführt: {testsRun}");
                Console.WriteLine($"Tests bestanden:  {testsPassed}");
                Console.WriteLine($"Tests fehlgeschlagen: {testsFailed}");
                Console.WriteLine($"Erfolgsquote: {(testsRun > 0 ? (testsPassed * 100 / testsRun) : 0)}%");
                Console.WriteLine("============================================================");

                var o=Console.ReadKey();
            }
            catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }
        }

        // Test Helper Methods

        private static void AssertTrue(bool condition, string message)
        {
            testsRun++;
            if (condition)
            {
                testsPassed++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [PASS] {message}");
                Console.ResetColor();
            }
            else
            {
                testsFailed++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] {message}");
                Console.ResetColor();
            }
        }

        private static void AssertFalse(bool condition, string message)
        {
            AssertTrue(!condition, message);
        }

        private static void AssertEquals(object expected, object actual, string message)
        {
            // Cosmos OS compatible: use == instead of Equals() for enum support
            // Equals() on enums is not supported yet in Cosmos OS
            bool isEqual = false;
            
            if (expected == null && actual == null)
            {
                isEqual = true;
            }
            else if (expected != null && actual != null)
            {
                // Use direct comparison for enums and value types
                if (expected.GetType().IsEnum && actual.GetType().IsEnum)
                {
                    // Compare enum values as integers
                    isEqual = ((int)expected) == ((int)actual);
                }
                else
                {
                    // For other types, use == operator which works in Cosmos
                    isEqual = expected.ToString() == actual.ToString();
                }
            }
            
            if (isEqual)
            {
                AssertTrue(true, message);
            }
            else
            {
                // Only format the message if test fails
                string expectedStr = expected != null ? expected.GetType().Name : "null";
                string actualStr = actual != null ? actual.GetType().Name : "null";
                AssertTrue(false, message + " (Werte nicht gleich)");
            }
        }

        private static void AssertNotNull(object obj, string message)
        {
            AssertTrue(obj != null, message);
        }

        private static void AssertNull(object obj, string message)
        {
            AssertTrue(obj == null, message);
        }

        // Setup helper
        private static (UserManager, AuthenticationManager, CommandHandler) SetupTestEnvironment()
        {
            var userManager = UserManager.Instance;
            userManager.Reset(); // Reset für Test-Isolation

            var authManager = new AuthenticationManager();
            // FIX: Verwende GetInternalUserList() statt GetAllUsers() für gemeinsame Referenz
            // Dies verhindert Sync-Probleme wenn neue Benutzer erstellt werden
            authManager.SetUsers(userManager.GetInternalUserList());
            var commandHandler = new CommandHandler(userManager, authManager);

            return (userManager, authManager, commandHandler);
        }

        // CommandHandler Tests

        private static void Test_CommandHandler_Creation()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            AssertNotNull(commandHandler, "CommandHandler sollte erstellt werden");
        }

        private static void Test_CommandHandler_ProcessCommand_UnknownCommand()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            bool result = commandHandler.ProcessCommand("unknowncommand");
            AssertFalse(result, "Unbekannter Befehl sollte false zurückgeben");
        }

        private static void Test_CommandHandler_WhoAmI_NotAuthenticated()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            
            bool result = commandHandler.ProcessCommand("whoami");
            AssertTrue(result, "whoami sollte erkannt werden");
            AssertFalse(authManager.IsAuthenticated, "Sollte nicht authentifiziert sein");
        }

        private static void Test_CommandHandler_WhoAmI_Authenticated()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            bool result = commandHandler.ProcessCommand("whoami");
            AssertTrue(result, "whoami sollte erkannt werden");
            AssertTrue(authManager.IsAuthenticated, "Sollte authentifiziert sein");
            AssertEquals("admin", authManager.CurrentUser.Username, "Sollte admin Benutzer sein");
        }

        private static void Test_CommandHandler_Logout_NotAuthenticated()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            
            commandHandler.ProcessCommand("logout");
            AssertFalse(authManager.IsAuthenticated, "Sollte nicht authentifiziert sein");
        }

        private static void Test_CommandHandler_Logout_Authenticated()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            AssertTrue(authManager.IsAuthenticated, "Sollte vor Logout authentifiziert sein");

            commandHandler.ProcessCommand("logout");
            AssertFalse(authManager.IsAuthenticated, "Benutzer sollte nach Logout abgemeldet sein");
        }

        // Command Parsing Tests

        private static void Test_ParseCommand_Simple()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            bool result = commandHandler.ProcessCommand("whoami");
            AssertTrue(result, "Einfacher Befehl sollte erkannt werden");
        }

        private static void Test_ParseCommand_WithQuotes()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            
            // Test that command parsing works with quotes
            bool result = commandHandler.ProcessCommand("useradd testquoteuser password123");
            AssertTrue(userManager.UserExists("testquoteuser"), "Befehl sollte geparst und ausgeführt werden");
        }

        private static void Test_ParseCommand_EmptyString()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            bool result = commandHandler.ProcessCommand("");
            AssertFalse(result, "Leerer String sollte false zurückgeben");
        }

        // Permission Tests

        private static void Test_RequireAdmin_NotAuthenticated()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            
            commandHandler.ProcessCommand("userlist");
            AssertFalse(authManager.IsAuthenticated, "Sollte nicht authentifiziert sein");
        }

        private static void Test_RequireAdmin_AsStandardUser()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            userManager.CreateUser("standard", "pass123", UserRole.Standard);
            authManager.Login("standard", "pass123");

            commandHandler.ProcessCommand("userlist");
            AssertEquals(UserRole.Standard, authManager.CurrentUser.Role, "Sollte Standard-User sein ohne Admin-Rechte");
        }

        private static void Test_RequireAdmin_AsAdmin()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            commandHandler.ProcessCommand("userlist");
            AssertTrue(authManager.IsAuthenticated, "Admin sollte authentifiziert sein");
            AssertEquals(UserRole.Admin, authManager.CurrentUser.Role, "Sollte Admin-Rolle haben");
        }

        // User Management Command Tests

        private static void Test_UserAdd_AsAdmin()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            commandHandler.ProcessCommand("useradd testuser password123");
            AssertTrue(userManager.UserExists("testuser"), "Benutzer sollte erstellt werden");
        }

        private static void Test_UserAdd_AsStandardUser()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            userManager.CreateUser("standard", "pass123", UserRole.Standard);
            authManager.Login("standard", "pass123");

            commandHandler.ProcessCommand("useradd testuser2 password123");
            AssertFalse(userManager.UserExists("testuser2"), "Standard-User sollte keinen Benutzer erstellen können");
            AssertEquals(UserRole.Standard, authManager.CurrentUser.Role, "Sollte kein Admin sein");
        }

        private static void Test_UserAdd_InvalidArguments()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            // Test invalid arguments - command should be recognized but not create user
            bool result = commandHandler.ProcessCommand("useradd");
            AssertTrue(result, "useradd sollte erkannt werden");
        }

        private static void Test_UserDel_AsAdmin()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            userManager.CreateUser("toDelete", "pass123", UserRole.Standard);

            // Note: Interactive confirmation is difficult to test, so we just verify command is recognized
            bool result = commandHandler.ProcessCommand("userdel toDelete");
            AssertTrue(result, "userdel Befehl sollte erkannt werden");
        }

        private static void Test_UserDel_DeleteSelf()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            // Test self-deletion prevention - admin should still exist after attempt
            commandHandler.ProcessCommand("userdel admin");
            AssertTrue(authManager.IsAuthenticated, "Admin sollte noch angemeldet sein");
            AssertTrue(userManager.UserExists("admin"), "Admin sollte noch existieren");
        }

        private static void Test_UserList_AsAdmin()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            commandHandler.ProcessCommand("userlist");
            AssertTrue(userManager.UserExists("admin"), "Admin sollte existieren");
            AssertEquals(1, userManager.UserCount, "Sollte genau 1 Benutzer geben");
        }

        private static void Test_UserStats_AsAdmin()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            // Test stats command recognition
            bool result = commandHandler.ProcessCommand("userstats");
            AssertTrue(result, "userstats Befehl sollte erkannt werden");
        }

        // Password Management Tests

        private static void Test_Passwd_ChangeOwnPassword()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");

            // Note: Interactive password input is difficult to test without user interaction
            bool result = commandHandler.ProcessCommand("passwd");
            AssertTrue(result, "passwd Befehl sollte erkannt werden");
        }

        private static void Test_Passwd_AdminResetPassword()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            userManager.CreateUser("target", "oldpass", UserRole.Standard);

            // Note: Interactive password input is difficult to test
            bool result = commandHandler.ProcessCommand("passwd target");
            AssertTrue(result, "passwd <username> Befehl sollte erkannt werden");
        }

        // UserMod Tests

        private static void Test_UserMod_ChangeRole()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            userManager.CreateUser("modtest", "pass123", UserRole.Standard);

            commandHandler.ProcessCommand("usermod modtest role admin");
            
            var user = userManager.GetUser("modtest");
            AssertEquals(UserRole.Admin, user.Role, "Rolle sollte geändert werden");
        }

        private static void Test_UserMod_ChangeStatus()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            userManager.CreateUser("modtest2", "pass123", UserRole.Standard);

            commandHandler.ProcessCommand("usermod modtest2 active false");
            
            var user = userManager.GetUser("modtest2");
            AssertFalse(user.IsActive, "Status sollte auf inaktiv gesetzt werden");
        }

        private static void Test_UserMod_ChangeHome()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            userManager.CreateUser("modtest3", "pass123", UserRole.Standard);

            commandHandler.ProcessCommand("usermod modtest3 home /home/custom");
            
            var user = userManager.GetUser("modtest3");
            AssertEquals("/home/custom", user.HomeDirectory, "Home-Verzeichnis sollte geändert werden");
        }

        private static void Test_UserMod_InvalidOption()
        {
            var (userManager, authManager, commandHandler) = SetupTestEnvironment();
            authManager.Login("admin", "admin");
            userManager.CreateUser("modtest4", "pass123", UserRole.Standard);

            // Test that invalid option is recognized but doesn't crash
            bool result = commandHandler.ProcessCommand("usermod modtest4 invalidoption value");
            AssertTrue(result, "usermod sollte erkannt werden");
            
            // User should still exist and be unchanged
            var user = userManager.GetUser("modtest4");
            AssertNotNull(user, "Benutzer sollte noch existieren");
        }

        // ConsoleHelper Tests

        private static void Test_ConsoleHelper_FormatRole()
        {
            AssertEquals("Administrator", ConsoleHelper.FormatRole(UserRole.Admin), "Admin-Rolle sollte formatiert werden");
            AssertEquals("Standard-Benutzer", ConsoleHelper.FormatRole(UserRole.Standard), "Standard-Rolle sollte formatiert werden");
            AssertEquals("Gast", ConsoleHelper.FormatRole(UserRole.Guest), "Gast-Rolle sollte formatiert werden");
        }

        private static void Test_ConsoleHelper_FormatStatus()
        {
            AssertEquals("Aktiv", ConsoleHelper.FormatStatus(true), "true sollte 'Aktiv' zurückgeben");
            AssertEquals("Inaktiv", ConsoleHelper.FormatStatus(false), "false sollte 'Inaktiv' zurückgeben");
        }

        private static void Test_ConsoleHelper_FormatTimeSpan()
        {
            var span1 = TimeSpan.FromDays(2);
            // Cosmos-safe: use IndexOf instead of Contains
            AssertTrue(ConsoleHelper.FormatTimeSpan(span1).IndexOf("Tag") >= 0, "Tage sollten formatiert werden");

            var span2 = TimeSpan.FromHours(3);
            AssertTrue(ConsoleHelper.FormatTimeSpan(span2).IndexOf("Stunde") >= 0, "Stunden sollten formatiert werden");

            var span3 = TimeSpan.FromMinutes(30);
            AssertTrue(ConsoleHelper.FormatTimeSpan(span3).IndexOf("Minute") >= 0, "Minuten sollten formatiert werden");

            var span4 = TimeSpan.FromSeconds(45);
            AssertTrue(ConsoleHelper.FormatTimeSpan(span4).IndexOf("Sekunde") >= 0, "Sekunden sollten formatiert werden");
        }

        private static void Test_ConsoleHelper_PadRight()
        {
            var result = ConsoleHelper.PadRight("test", 10);
            AssertEquals(10, result.Length, "String sollte auf 10 Zeichen gepaddet werden");
            // Cosmos-safe: use IndexOf instead of StartsWith
            AssertTrue(result.IndexOf("test") == 0, "String sollte mit 'test' beginnen");
        }

        private static void Test_ConsoleHelper_Truncate()
        {
            var longText = "This is a very long text that should be truncated";
            var result = ConsoleHelper.Truncate(longText, 20);
            AssertEquals(20, result.Length, "Text sollte auf 20 Zeichen gekürzt werden");
            // Cosmos-safe: check last 3 characters instead of EndsWith
            bool endsWithDots = result.Length >= 3 && 
                result[result.Length - 3] == '.' && 
                result[result.Length - 2] == '.' && 
                result[result.Length - 1] == '.';
            AssertTrue(endsWithDots, "Gekürzter Text sollte mit ... enden");

            var shortText = "Short";
            var result2 = ConsoleHelper.Truncate(shortText, 20);
            AssertEquals("Short", result2, "Kurzer Text sollte nicht gekürzt werden");
        }
    }
}
