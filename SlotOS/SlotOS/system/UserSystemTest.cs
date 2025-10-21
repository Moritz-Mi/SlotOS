using System;
using System.Collections.Generic;

namespace SlotOS.System
{
    /// <summary>
    /// Test-Klasse für Phase 1 und Phase 2 des Nutzerverwaltungssystems
    /// </summary>
    public static class UserSystemTest
    {
        /// <summary>
        /// Führt alle Tests aus
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("=== SlotOS Nutzerverwaltung - Systemtest ===\n");

            int totalTests = 0;
            int passedTests = 0;

            // Phase 1 Tests
            Console.WriteLine("--- Phase 1: Grundlegende Datenstrukturen ---");
            if (TestUserCreation()) passedTests++;
            totalTests++;
            
            if (TestUserRoles()) passedTests++;
            totalTests++;

            if (TestPasswordUpdate()) passedTests++;
            totalTests++;

            Console.WriteLine();

            // Phase 2 Tests - PasswordHasher
            Console.WriteLine("--- Phase 2.1: PasswordHasher ---");
            if (TestPasswordHashing()) passedTests++;
            totalTests++;

            if (TestPasswordVerification()) passedTests++;
            totalTests++;

            if (TestSaltUniqueness()) passedTests++;
            totalTests++;

            if (TestLegacyHashCompatibility()) passedTests++;
            totalTests++;

            Console.WriteLine();

            // Phase 2 Tests - AuthenticationManager
            Console.WriteLine("--- Phase 2.2: AuthenticationManager ---");
            if (TestSuccessfulLogin()) passedTests++;
            totalTests++;

            if (TestFailedLogin()) passedTests++;
            totalTests++;

            if (TestLogout()) passedTests++;
            totalTests++;

            if (TestAdminRights()) passedTests++;
            totalTests++;

            if (TestLoginAttemptLimiting()) passedTests++;
            totalTests++;

            if (TestSessionTracking()) passedTests++;
            totalTests++;

            Console.WriteLine();

            // Phase 3 Tests - UserManager
            Console.WriteLine("--- Phase 3: UserManager ---");
            if (TestUserManagerInitialization()) passedTests++;
            totalTests++;

            if (TestUserManagerCreateUser()) passedTests++;
            totalTests++;

            if (TestUserManagerDeleteUser()) passedTests++;
            totalTests++;

            if (TestUserManagerGetUser()) passedTests++;
            totalTests++;

            if (TestUserManagerUpdateUser()) passedTests++;
            totalTests++;

            if (TestUserManagerChangePassword()) passedTests++;
            totalTests++;

            if (TestUserManagerResetPassword()) passedTests++;
            totalTests++;

            if (TestUserManagerUserExists()) passedTests++;
            totalTests++;

            if (TestUserManagerLastAdminProtection()) passedTests++;
            totalTests++;

            if (TestUserManagerStatistics()) passedTests++;
            totalTests++;

            // Zusammenfassung
            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine($"Tests abgeschlossen: {passedTests}/{totalTests} erfolgreich");
            
            if (passedTests == totalTests)
            {
                Console.WriteLine("✅ Alle Tests bestanden!");
            }
            else
            {
                Console.WriteLine($"❌ {totalTests - passedTests} Test(s) fehlgeschlagen!");
            }
            Console.WriteLine("==========================================");
        }

        #region Phase 1 Tests

        private static bool TestUserCreation()
        {
            try
            {
                var user = new User("testuser", "hash123", UserRole.Standard);
                
                bool result = user.Username == "testuser" &&
                             user.PasswordHash == "hash123" &&
                             user.Role == UserRole.Standard &&
                             user.IsActive == true &&
                             user.HomeDirectory == "/home/testuser";

                PrintTestResult("User-Erstellung", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("User-Erstellung", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserRoles()
        {
            try
            {
                var admin = new User("admin", "hash", UserRole.Admin);
                var standard = new User("user", "hash", UserRole.Standard);
                var guest = new User("guest", "hash", UserRole.Guest);

                bool result = admin.IsAdmin() == true &&
                             standard.IsAdmin() == false &&
                             guest.IsAdmin() == false;

                PrintTestResult("Benutzerrollen", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Benutzerrollen", false, ex.Message);
                return false;
            }
        }

        private static bool TestPasswordUpdate()
        {
            try
            {
                var user = new User("test", PasswordHasher.Hash("oldpass"), UserRole.Standard);
                string oldHash = user.PasswordHash;
                
                user.UpdatePassword("newpass");
                string newHash = user.PasswordHash;

                bool result = oldHash != newHash &&
                             PasswordHasher.Verify("newpass", newHash);

                PrintTestResult("Passwort-Update", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Passwort-Update", false, ex.Message);
                return false;
            }
        }

        #endregion

        #region Phase 2.1 Tests - PasswordHasher

        private static bool TestPasswordHashing()
        {
            try
            {
                string password = "TestPasswort123";
                string hash = PasswordHasher.Hash(password);

                bool result = !string.IsNullOrEmpty(hash) &&
                             hash.Contains(":") &&  // Salt:Hash Format
                             hash.Length > 20;      // Ausreichende Länge

                PrintTestResult("Passwort-Hashing", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Passwort-Hashing", false, ex.Message);
                return false;
            }
        }

        private static bool TestPasswordVerification()
        {
            try
            {
                string password = "MeinPasswort";
                string hash = PasswordHasher.Hash(password);

                bool correctPassword = PasswordHasher.Verify(password, hash);
                bool wrongPassword = PasswordHasher.Verify("FalschesPasswort", hash);

                bool result = correctPassword == true && wrongPassword == false;

                PrintTestResult("Passwort-Verifikation", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Passwort-Verifikation", false, ex.Message);
                return false;
            }
        }

        private static bool TestSaltUniqueness()
        {
            try
            {
                string password = "gleichesPasswort";
                string hash1 = PasswordHasher.Hash(password);
                string hash2 = PasswordHasher.Hash(password);

                // Gleiche Passwörter sollten unterschiedliche Hashes haben (wegen Salt)
                bool result = hash1 != hash2;

                PrintTestResult("Salt-Einzigartigkeit", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Salt-Einzigartigkeit", false, ex.Message);
                return false;
            }
        }

        private static bool TestLegacyHashCompatibility()
        {
            try
            {
                // Simuliere alten Hash ohne Salt
                string legacyHash = "12345678"; // Altes Format
                
                // Sollte nicht crashen, auch wenn Format nicht passt
                bool result = true;
                try
                {
                    PasswordHasher.Verify("test", legacyHash);
                }
                catch
                {
                    result = false;
                }

                PrintTestResult("Legacy-Hash-Kompatibilität", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Legacy-Hash-Kompatibilität", false, ex.Message);
                return false;
            }
        }

        #endregion

        #region Phase 2.2 Tests - AuthenticationManager

        private static bool TestSuccessfulLogin()
        {
            try
            {
                var authManager = new AuthenticationManager();
                var users = new List<User>
                {
                    new User("admin", PasswordHasher.Hash("admin123"), UserRole.Admin)
                };
                authManager.SetUsers(users);

                bool loginSuccess = authManager.Login("admin", "admin123");
                bool isAuthenticated = authManager.IsAuthenticated;
                string username = authManager.CurrentUser?.Username;

                bool result = loginSuccess && isAuthenticated && username == "admin";

                PrintTestResult("Erfolgreicher Login", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Erfolgreicher Login", false, ex.Message);
                return false;
            }
        }

        private static bool TestFailedLogin()
        {
            try
            {
                var authManager = new AuthenticationManager();
                var users = new List<User>
                {
                    new User("user", PasswordHasher.Hash("correct"), UserRole.Standard)
                };
                authManager.SetUsers(users);

                bool wrongPassword = authManager.Login("user", "wrong");
                bool wrongUser = authManager.Login("nobody", "correct");

                bool result = !wrongPassword && !wrongUser && !authManager.IsAuthenticated;

                PrintTestResult("Fehlgeschlagener Login", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Fehlgeschlagener Login", false, ex.Message);
                return false;
            }
        }

        private static bool TestLogout()
        {
            try
            {
                var authManager = new AuthenticationManager();
                var users = new List<User>
                {
                    new User("user", PasswordHasher.Hash("pass"), UserRole.Standard)
                };
                authManager.SetUsers(users);

                authManager.Login("user", "pass");
                bool wasAuthenticated = authManager.IsAuthenticated;

                authManager.Logout();
                bool isNowLoggedOut = !authManager.IsAuthenticated;

                bool result = wasAuthenticated && isNowLoggedOut;

                PrintTestResult("Logout", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Logout", false, ex.Message);
                return false;
            }
        }

        private static bool TestAdminRights()
        {
            try
            {
                var authManager = new AuthenticationManager();
                var users = new List<User>
                {
                    new User("admin", PasswordHasher.Hash("pass"), UserRole.Admin),
                    new User("user", PasswordHasher.Hash("pass"), UserRole.Standard)
                };
                authManager.SetUsers(users);

                // Admin login
                authManager.Login("admin", "pass");
                bool adminHasRights = false;
                try
                {
                    authManager.RequireAdmin();
                    adminHasRights = true;
                }
                catch { }

                // Standard user login
                authManager.Logout();
                authManager.Login("user", "pass");
                bool userHasNoRights = false;
                try
                {
                    authManager.RequireAdmin();
                }
                catch (UnauthorizedAccessException)
                {
                    userHasNoRights = true;
                }

                bool result = adminHasRights && userHasNoRights;

                PrintTestResult("Admin-Rechte-Prüfung", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Admin-Rechte-Prüfung", false, ex.Message);
                return false;
            }
        }

        private static bool TestLoginAttemptLimiting()
        {
            try
            {
                var authManager = new AuthenticationManager();
                var users = new List<User>
                {
                    new User("user", PasswordHasher.Hash("correct"), UserRole.Standard)
                };
                authManager.SetUsers(users);

                // 3 fehlgeschlagene Versuche
                authManager.Login("user", "wrong1");
                int remaining1 = authManager.GetRemainingLoginAttempts();
                
                authManager.Login("user", "wrong2");
                int remaining2 = authManager.GetRemainingLoginAttempts();
                
                authManager.Login("user", "wrong3");
                int remaining3 = authManager.GetRemainingLoginAttempts();

                // 4. Versuch sollte Exception werfen
                bool isLocked = false;
                try
                {
                    authManager.Login("user", "wrong4");
                }
                catch (InvalidOperationException)
                {
                    isLocked = true;
                }

                bool result = remaining1 == 2 && remaining2 == 1 && remaining3 == 0 && isLocked;

                PrintTestResult("Login-Versuchs-Limitierung", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Login-Versuchs-Limitierung", false, ex.Message);
                return false;
            }
        }

        private static bool TestSessionTracking()
        {
            try
            {
                var authManager = new AuthenticationManager();
                var users = new List<User>
                {
                    new User("user", PasswordHasher.Hash("pass"), UserRole.Standard)
                };
                authManager.SetUsers(users);

                authManager.Login("user", "pass");
                DateTime firstActivity = authManager.LastActivity;

                // Warte bis DateTime.Now sich ändert (Cosmos OS kompatibel)
                DateTime startWait = DateTime.Now;
                while (DateTime.Now == startWait)
                {
                    // Busy-wait bis sich die Zeit ändert
                    for (int i = 0; i < 1000; i++) { /* Kleine Schleife */ }
                }
                
                authManager.UpdateActivity();
                DateTime secondActivity = authManager.LastActivity;

                bool result = secondActivity > firstActivity;

                PrintTestResult("Session-Tracking", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Session-Tracking", false, ex.Message);
                return false;
            }
        }

        #endregion

        #region Phase 3 Tests - UserManager

        private static bool TestUserManagerInitialization()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers(); // Cleanup
                manager.Initialize();

                bool hasDefaultAdmin = manager.UserExists(UserManager.DEFAULT_ADMIN_USERNAME);
                var admin = manager.GetUser(UserManager.DEFAULT_ADMIN_USERNAME);
                bool isAdmin = admin != null && admin.Role == UserRole.Admin;

                bool result = hasDefaultAdmin && isAdmin && manager.UserCount >= 1;

                PrintTestResult("UserManager Initialisierung", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("UserManager Initialisierung", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerCreateUser()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();

                bool created = manager.CreateUser("testuser", "testpass", UserRole.Standard);
                bool exists = manager.UserExists("testuser");
                var user = manager.GetUser("testuser");

                bool result = created && exists && user != null && user.IsActive;

                PrintTestResult("Benutzer erstellen", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Benutzer erstellen", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerDeleteUser()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();
                manager.Initialize(); // Erstellt Admin

                manager.CreateUser("deleteMe", "pass", UserRole.Standard);
                bool existedBefore = manager.UserExists("deleteMe");

                bool deleted = manager.DeleteUser("deleteMe");
                bool existsAfter = manager.UserExists("deleteMe");

                bool result = existedBefore && deleted && !existsAfter;

                PrintTestResult("Benutzer löschen", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Benutzer löschen", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerGetUser()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();

                manager.CreateUser("findMe", "pass", UserRole.Standard);
                var user = manager.GetUser("findMe");
                var notFound = manager.GetUser("notExist");

                bool result = user != null && user.Username == "findMe" && notFound == null;

                PrintTestResult("Benutzer abrufen", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Benutzer abrufen", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerUpdateUser()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();

                manager.CreateUser("updateMe", "pass", UserRole.Standard);
                var user = manager.GetUser("updateMe");
                
                user.Role = UserRole.Admin;
                user.HomeDirectory = "/custom/home";
                bool updated = manager.UpdateUser(user);

                var updatedUser = manager.GetUser("updateMe");
                bool result = updated && 
                             updatedUser.Role == UserRole.Admin &&
                             updatedUser.HomeDirectory == "/custom/home";

                PrintTestResult("Benutzer aktualisieren", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Benutzer aktualisieren", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerChangePassword()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();

                manager.CreateUser("passChange", "oldpass", UserRole.Standard);
                bool changed = manager.ChangePassword("passChange", "oldpass", "newpass");

                var user = manager.GetUser("passChange");
                bool canLoginWithNew = user.VerifyPassword("newpass");
                bool cannotLoginWithOld = !user.VerifyPassword("oldpass");

                bool result = changed && canLoginWithNew && cannotLoginWithOld;

                PrintTestResult("Passwort ändern", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Passwort ändern", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerResetPassword()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();

                manager.CreateUser("resetMe", "oldpass", UserRole.Standard);
                bool reset = manager.ResetPassword("resetMe", "resetpass");

                var user = manager.GetUser("resetMe");
                bool canLoginWithReset = user.VerifyPassword("resetpass");

                bool result = reset && canLoginWithReset;

                PrintTestResult("Passwort zurücksetzen", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Passwort zurücksetzen", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerUserExists()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();

                bool notExistsBefore = !manager.UserExists("checkMe");
                manager.CreateUser("checkMe", "pass", UserRole.Standard);
                bool existsAfter = manager.UserExists("checkMe");

                bool result = notExistsBefore && existsAfter;

                PrintTestResult("Benutzer-Existenz prüfen", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Benutzer-Existenz prüfen", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerLastAdminProtection()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();
                manager.Initialize(); // Erstellt einen Admin

                bool exceptionThrown = false;
                try
                {
                    manager.DeleteUser(UserManager.DEFAULT_ADMIN_USERNAME);
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }

                bool adminStillExists = manager.UserExists(UserManager.DEFAULT_ADMIN_USERNAME);

                bool result = exceptionThrown && adminStillExists;

                PrintTestResult("Letzter Admin-Schutz", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Letzter Admin-Schutz", false, ex.Message);
                return false;
            }
        }

        private static bool TestUserManagerStatistics()
        {
            try
            {
                var manager = UserManager.Instance;
                manager.ClearAllUsers();

                manager.CreateUser("admin1", "pass", UserRole.Admin);
                manager.CreateUser("user1", "pass", UserRole.Standard);
                manager.CreateUser("guest1", "pass", UserRole.Guest);

                string stats = manager.GetStatistics();
                bool hasContent = !string.IsNullOrEmpty(stats) && 
                                 stats.Contains("Gesamt") &&
                                 stats.Contains("Administratoren");

                int adminCount = manager.GetAdminCount();
                bool result = hasContent && adminCount == 1 && manager.UserCount == 3;

                PrintTestResult("Statistiken", result);
                return result;
            }
            catch (Exception ex)
            {
                PrintTestResult("Statistiken", false, ex.Message);
                return false;
            }
        }

        #endregion

        #region Hilfsmethoden

        private static void PrintTestResult(string testName, bool passed, string error = null)
        {
            string status = passed ? "✅ PASS" : "❌ FAIL";
            Console.Write($"  {status} - {testName}");
            
            if (!passed && !string.IsNullOrEmpty(error))
            {
                Console.Write($" ({error})");
            }
            
            Console.WriteLine();
        }

        #endregion
    }
}
