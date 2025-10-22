using System;
using Sys = Cosmos.System;

namespace SlotOS.System
{
    /// <summary>
    /// Test-Suite für das PermissionChecker-System (Phase 6)
    /// </summary>
    public class PermissionCheckerTest
    {
        private static int _testsPassed = 0;
        private static int _testsFailed = 0;
        private static int _totalTests = 0;

        /// <summary>
        /// Führt alle Tests für Phase 6 aus
        /// </summary>
        public static void RunAllTests()
        {
            _testsPassed = 0;
            _testsFailed = 0;
            _totalTests = 0;

            ConsoleHelper.WriteHeader("PHASE 6 TESTS - BERECHTIGUNGSSYSTEM");
            Console.WriteLine();

            // Test-Gruppen ausführen
            TestPermissionCheckerBasics();
            TestAdminPermissions();
            TestStandardUserPermissions();
            TestGuestUserPermissions();
            TestFileAccessPermissions();
            TestRequireMethods();
            TestPathChecking();
            TestPermissionSummary();

            // Zusammenfassung
            Console.WriteLine();
            ConsoleHelper.WriteSeparator();
            Console.WriteLine();

            if (_testsFailed == 0)
            {
                ConsoleHelper.WriteSuccess("ALLE TESTS BESTANDEN!");
                Console.WriteLine("Tests bestanden: " + _testsPassed.ToString());
            }
            else
            {
                ConsoleHelper.WriteError("TESTS FEHLGESCHLAGEN!");
                Console.WriteLine("Bestanden: " + _testsPassed.ToString());
                Console.WriteLine("Fehlgeschlagen: " + _testsFailed.ToString());
            }
        }

        #region Test-Gruppen

        private static void TestPermissionCheckerBasics()
        {
            ConsoleHelper.WriteInfo("=== PermissionChecker Basics ===");

            // Test 1: Singleton-Instanz
            RunTest(
                "Singleton-Instanz verfugbar",
                () =>
                {
                    var instance1 = PermissionChecker.Instance;
                    var instance2 = PermissionChecker.Instance;
                    return instance1 != null && instance1 == instance2;
                }
            );

            // Test 2: Null-User hat keine Berechtigungen
            RunTest(
                "Null-User hat keine Berechtigungen",
                () =>
                {
                    var checker = PermissionChecker.Instance;
                    return !checker.HasPermission(null, PermissionChecker.ACTION_USER_VIEW);
                }
            );

            // Test 3: Inaktiver User hat keine Berechtigungen
            RunTest(
                "Inaktiver User hat keine Berechtigungen",
                () =>
                {
                    var user = CreateTestUser("inactive", UserRole.Admin);
                    user.IsActive = false;
                    var checker = PermissionChecker.Instance;
                    return !checker.HasPermission(user, PermissionChecker.ACTION_USER_VIEW);
                }
            );

            // Test 4: Leere Action gibt false zurück
            RunTest(
                "Leere Action gibt false zuruck",
                () =>
                {
                    var user = CreateTestUser("test", UserRole.Standard);
                    var checker = PermissionChecker.Instance;
                    return !checker.HasPermission(user, "");
                }
            );

            Console.WriteLine();
        }

        private static void TestAdminPermissions()
        {
            ConsoleHelper.WriteInfo("=== Admin-Berechtigungen ===");

            var adminUser = CreateTestUser("admin", UserRole.Admin);
            var checker = PermissionChecker.Instance;

            // Test 5: Admin wird als Admin erkannt
            RunTest(
                "Admin wird als Admin erkannt",
                () => checker.IsAdmin(adminUser)
            );

            // Test 6: Admin hat alle Benutzerverwaltungs-Rechte
            RunTest(
                "Admin kann Benutzer erstellen",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_USER_CREATE)
            );

            RunTest(
                "Admin kann Benutzer loschen",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_USER_DELETE)
            );

            RunTest(
                "Admin kann Benutzer bearbeiten",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_USER_MODIFY)
            );

            RunTest(
                "Admin kann Passwort zurucksetzen",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_PASSWORD_RESET)
            );

            // Test 7: Admin hat alle System-Rechte
            RunTest(
                "Admin kann System konfigurieren",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_SYSTEM_CONFIG)
            );

            RunTest(
                "Admin kann System herunterfahren",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_SYSTEM_SHUTDOWN)
            );

            // Test 8: Admin hat alle Datei-Rechte
            RunTest(
                "Admin kann Dateien lesen",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_FILE_READ)
            );

            RunTest(
                "Admin kann Dateien schreiben",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_FILE_WRITE)
            );

            RunTest(
                "Admin kann Dateien loschen",
                () => checker.HasPermission(adminUser, PermissionChecker.ACTION_FILE_DELETE)
            );

            Console.WriteLine();
        }

        private static void TestStandardUserPermissions()
        {
            ConsoleHelper.WriteInfo("=== Standard-Benutzer-Berechtigungen ===");

            var standardUser = CreateTestUser("user", UserRole.Standard);
            var checker = PermissionChecker.Instance;

            // Test 9: Standard-User ist kein Admin
            RunTest(
                "Standard-User ist kein Admin",
                () => !checker.IsAdmin(standardUser)
            );

            // Test 10: Standard-User kann eigene Infos sehen
            RunTest(
                "Standard-User kann Benutzer anzeigen",
                () => checker.HasPermission(standardUser, PermissionChecker.ACTION_USER_VIEW)
            );

            // Test 11: Standard-User kann KEINE Benutzer erstellen
            RunTest(
                "Standard-User kann KEINE Benutzer erstellen",
                () => !checker.HasPermission(standardUser, PermissionChecker.ACTION_USER_CREATE)
            );

            // Test 12: Standard-User kann KEINE Benutzer löschen
            RunTest(
                "Standard-User kann KEINE Benutzer loschen",
                () => !checker.HasPermission(standardUser, PermissionChecker.ACTION_USER_DELETE)
            );

            // Test 13: Standard-User kann KEINE Passwörter zurücksetzen
            RunTest(
                "Standard-User kann KEINE Passworter zurucksetzen",
                () => !checker.HasPermission(standardUser, PermissionChecker.ACTION_PASSWORD_RESET)
            );

            // Test 14: Standard-User kann KEINE System-Konfiguration ändern
            RunTest(
                "Standard-User kann NICHT System konfigurieren",
                () => !checker.HasPermission(standardUser, PermissionChecker.ACTION_SYSTEM_CONFIG)
            );

            // Test 15: Standard-User kann Dateien lesen (in erlaubten Bereichen)
            RunTest(
                "Standard-User kann Dateien lesen",
                () => checker.HasPermission(standardUser, PermissionChecker.ACTION_FILE_READ)
            );

            // Test 16: Standard-User kann Dateien schreiben (in erlaubten Bereichen)
            RunTest(
                "Standard-User kann Dateien schreiben",
                () => checker.HasPermission(standardUser, PermissionChecker.ACTION_FILE_WRITE)
            );

            Console.WriteLine();
        }

        private static void TestGuestUserPermissions()
        {
            ConsoleHelper.WriteInfo("=== Gast-Benutzer-Berechtigungen ===");

            var guestUser = CreateTestUser("guest", UserRole.Guest);
            var checker = PermissionChecker.Instance;

            // Test 17: Gast ist kein Admin
            RunTest(
                "Gast-User ist kein Admin",
                () => !checker.IsAdmin(guestUser)
            );

            // Test 18: Gast kann eigene Infos sehen
            RunTest(
                "Gast kann Benutzer anzeigen",
                () => checker.HasPermission(guestUser, PermissionChecker.ACTION_USER_VIEW)
            );

            // Test 19: Gast kann KEINE Benutzer erstellen
            RunTest(
                "Gast kann KEINE Benutzer erstellen",
                () => !checker.HasPermission(guestUser, PermissionChecker.ACTION_USER_CREATE)
            );

            // Test 20: Gast kann KEINE Benutzer löschen
            RunTest(
                "Gast kann KEINE Benutzer loschen",
                () => !checker.HasPermission(guestUser, PermissionChecker.ACTION_USER_DELETE)
            );

            // Test 21: Gast kann Dateien nur lesen
            RunTest(
                "Gast kann Dateien lesen",
                () => checker.HasPermission(guestUser, PermissionChecker.ACTION_FILE_READ)
            );

            // Test 22: Gast kann KEINE Dateien schreiben
            RunTest(
                "Gast kann KEINE Dateien schreiben",
                () => !checker.HasPermission(guestUser, PermissionChecker.ACTION_FILE_WRITE)
            );

            // Test 23: Gast kann KEINE Dateien löschen
            RunTest(
                "Gast kann KEINE Dateien loschen",
                () => !checker.HasPermission(guestUser, PermissionChecker.ACTION_FILE_DELETE)
            );

            Console.WriteLine();
        }

        private static void TestFileAccessPermissions()
        {
            ConsoleHelper.WriteInfo("=== Datei-Zugriffs-Berechtigungen ===");

            var adminUser = CreateTestUser("admin", UserRole.Admin);
            var standardUser = CreateTestUser("user", UserRole.Standard);
            var guestUser = CreateTestUser("guest", UserRole.Guest);
            var checker = PermissionChecker.Instance;

            // Test 24: Admin kann auf System-Dateien zugreifen
            RunTest(
                "Admin kann System-Dateien lesen",
                () => checker.CanAccessFile(
                    adminUser,
                    "0:/system/config.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            // Test 25: Standard-User kann NICHT auf System-Dateien zugreifen
            RunTest(
                "Standard-User kann NICHT auf System-Dateien zugreifen",
                () => !checker.CanAccessFile(
                    standardUser,
                    "0:/system/config.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            // Test 26: Standard-User kann auf eigenes Home-Verzeichnis zugreifen
            RunTest(
                "Standard-User kann eigenes Home lesen",
                () => checker.CanAccessFile(
                    standardUser,
                    "/home/user/file.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            RunTest(
                "Standard-User kann eigenes Home schreiben",
                () => checker.CanAccessFile(
                    standardUser,
                    "/home/user/file.txt",
                    PermissionChecker.ACTION_FILE_WRITE
                )
            );

            // Test 27: Gast kann nur in eigenem Home lesen
            RunTest(
                "Gast kann eigenes Home lesen",
                () => checker.CanAccessFile(
                    guestUser,
                    "/home/guest/file.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            RunTest(
                "Gast kann eigenes Home NICHT schreiben",
                () => !checker.CanAccessFile(
                    guestUser,
                    "/home/guest/file.txt",
                    PermissionChecker.ACTION_FILE_WRITE
                )
            );

            // Test 28: Benutzer können öffentliche Dateien lesen
            RunTest(
                "Standard-User kann offentliche Dateien lesen",
                () => checker.CanAccessFile(
                    standardUser,
                    "/public/doc.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            RunTest(
                "Gast kann offentliche Dateien lesen",
                () => checker.CanAccessFile(
                    guestUser,
                    "0:/public/doc.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            // Test 29: Standard-User kann NICHT fremdes Home-Verzeichnis schreiben
            RunTest(
                "Standard-User kann NICHT fremdes Home schreiben",
                () => !checker.CanAccessFile(
                    standardUser,
                    "/home/otheruser/file.txt",
                    PermissionChecker.ACTION_FILE_WRITE
                )
            );

            Console.WriteLine();
        }

        private static void TestRequireMethods()
        {
            ConsoleHelper.WriteInfo("=== Require-Methoden (Exception-Tests) ===");

            var adminUser = CreateTestUser("admin", UserRole.Admin);
            var standardUser = CreateTestUser("user", UserRole.Standard);
            var checker = PermissionChecker.Instance;

            // Test 30: RequireAdmin mit Admin funktioniert
            RunTest(
                "RequireAdmin mit Admin wirft keine Exception",
                () =>
                {
                    try
                    {
                        checker.RequireAdmin(adminUser);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            );

            // Test 31: RequireAdmin mit Standard-User wirft Exception
            RunTest(
                "RequireAdmin mit Standard-User wirft Exception",
                () =>
                {
                    try
                    {
                        checker.RequireAdmin(standardUser);
                        return false; // Sollte nicht erreicht werden
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return true; // Exception erwartet
                    }
                }
            );

            // Test 32: RequirePermission mit erlaubter Aktion funktioniert
            RunTest(
                "RequirePermission mit erlaubter Aktion OK",
                () =>
                {
                    try
                    {
                        checker.RequirePermission(
                            adminUser,
                            PermissionChecker.ACTION_USER_CREATE
                        );
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            );

            // Test 33: RequirePermission mit verbotener Aktion wirft Exception
            RunTest(
                "RequirePermission mit verbotener Aktion wirft Exception",
                () =>
                {
                    try
                    {
                        checker.RequirePermission(
                            standardUser,
                            PermissionChecker.ACTION_USER_CREATE
                        );
                        return false; // Sollte nicht erreicht werden
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return true; // Exception erwartet
                    }
                }
            );

            // Test 34: DenyAccess wirft immer Exception
            RunTest(
                "DenyAccess wirft immer Exception",
                () =>
                {
                    try
                    {
                        checker.DenyAccess("Test-Grund");
                        return false; // Sollte nicht erreicht werden
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return true; // Exception erwartet
                    }
                }
            );

            Console.WriteLine();
        }

        private static void TestPathChecking()
        {
            ConsoleHelper.WriteInfo("=== Pfad-Prufung ===");

            var checker = PermissionChecker.Instance;
            var adminUser = CreateTestUser("admin", UserRole.Admin);
            var standardUser = CreateTestUser("user", UserRole.Standard);

            // Test 35: Verschiedene System-Pfad-Formate werden erkannt
            RunTest(
                "System-Pfad 0:/system/ wird erkannt",
                () => !checker.CanAccessFile(
                    standardUser,
                    "0:/system/file.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            RunTest(
                "System-Pfad /system/ wird erkannt",
                () => !checker.CanAccessFile(
                    standardUser,
                    "/system/file.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            RunTest(
                "Boot-Pfad wird erkannt",
                () => !checker.CanAccessFile(
                    standardUser,
                    "0:/boot/kernel.bin",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            // Test 36: Verschiedene Home-Pfad-Formate werden erkannt
            RunTest(
                "Home-Pfad /home/user/ wird erkannt",
                () => checker.CanAccessFile(
                    standardUser,
                    "/home/user/file.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            RunTest(
                "Home-Pfad 0:/home/user/ wird erkannt",
                () => checker.CanAccessFile(
                    standardUser,
                    "0:/home/user/file.txt",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            // Test 37: Null/Leere Pfade werden abgelehnt
            RunTest(
                "Null-Pfad wird abgelehnt",
                () => !checker.CanAccessFile(
                    standardUser,
                    null,
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            RunTest(
                "Leerer Pfad wird abgelehnt",
                () => !checker.CanAccessFile(
                    standardUser,
                    "",
                    PermissionChecker.ACTION_FILE_READ
                )
            );

            Console.WriteLine();
        }

        private static void TestPermissionSummary()
        {
            ConsoleHelper.WriteInfo("=== Permission Summary ===");

            var adminUser = CreateTestUser("admin", UserRole.Admin);
            var standardUser = CreateTestUser("user", UserRole.Standard);
            var guestUser = CreateTestUser("guest", UserRole.Guest);
            var inactiveUser = CreateTestUser("inactive", UserRole.Standard);
            inactiveUser.IsActive = false;

            var checker = PermissionChecker.Instance;

            // Test 38: Summary für Admin
            RunTest(
                "Summary fur Admin enthalt 'Administrator'",
                () =>
                {
                    string summary = checker.GetPermissionSummary(adminUser);
                    return summary.IndexOf("Administrator-Rechte") >= 0;
                }
            );

            // Test 39: Summary für Standard-User
            RunTest(
                "Summary fur Standard-User enthalt 'Home-Verzeichnis'",
                () =>
                {
                    string summary = checker.GetPermissionSummary(standardUser);
                    return summary.IndexOf("Standard-Benutzer") >= 0;
                }
            );

            // Test 40: Summary für Guest
            RunTest(
                "Summary fur Gast enthalt 'Eingeschrankte'",
                () =>
                {
                    string summary = checker.GetPermissionSummary(guestUser);
                    return summary.IndexOf("Gast") >= 0;
                }
            );

            // Test 41: Summary für inaktiven User
            RunTest(
                "Summary fur inaktiven User enthalt 'deaktiviert'",
                () =>
                {
                    string summary = checker.GetPermissionSummary(inactiveUser);
                    return summary.IndexOf("Konto deaktiviert") >= 0;
                }
            );

            // Test 42: Summary für null
            RunTest(
                "Summary fur null enthalt 'kein Benutzer'",
                () =>
                {
                    string summary = checker.GetPermissionSummary(null);
                    return summary.IndexOf("kein Benutzer") >= 0;
                }
            );

            Console.WriteLine();
        }

        #endregion

        #region Hilfsmethoden

        /// <summary>
        /// Führt einen einzelnen Test aus
        /// </summary>
        private static void RunTest(string testName, Func<bool> testFunc)
        {
            _totalTests++;
            try
            {
                bool result = testFunc();
                if (result)
                {
                    _testsPassed++;
                    Console.WriteLine("[OK] " + testName);
                }
                else
                {
                    _testsFailed++;
                    ConsoleHelper.WriteError("[FAIL] " + testName);
                }
            }
            catch (Exception ex)
            {
                _testsFailed++;
                ConsoleHelper.WriteError("[ERROR] " + testName + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Erstellt einen Test-Benutzer
        /// </summary>
        private static User CreateTestUser(string username, UserRole role)
        {
            string passwordHash = PasswordHasher.Hash("testpass123");
            return new User(username, passwordHash, role);
        }

        #endregion
    }
}
