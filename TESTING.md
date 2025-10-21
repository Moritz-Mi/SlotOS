# SlotOS - Testing Guide

## Tests für Phase 1 & 2

### Schnellstart

**Im laufenden SlotOS:**
```
SlotOS> test
```

Das System führt automatisch alle Tests aus und zeigt die Ergebnisse an.

---

## Was wird getestet?

### Phase 1: Grundlegende Datenstrukturen

#### ✅ User-Erstellung
- Prüft ob User-Objekte korrekt erstellt werden
- Validiert alle Properties (Username, Role, IsActive, HomeDirectory)

#### ✅ Benutzerrollen
- Testet UserRole.Admin, UserRole.Standard, UserRole.Guest
- Prüft `IsAdmin()` Methode

#### ✅ Passwort-Update
- Testet `UpdatePassword()` Methode
- Verifiziert dass Hash sich ändert

---

### Phase 2.1: PasswordHasher

#### ✅ Passwort-Hashing
- Prüft ob Hash generiert wird
- Validiert Format (salt:hash)
- Überprüft Länge

#### ✅ Passwort-Verifikation
- Testet `Verify()` mit korrektem Passwort
- Testet `Verify()` mit falschem Passwort

#### ✅ Salt-Einzigartigkeit
- Hasht gleiches Passwort zweimal
- Verifiziert dass unterschiedliche Hashes entstehen

#### ✅ Legacy-Kompatibilität
- Prüft dass alte Hashes ohne Salt nicht crashen

---

### Phase 2.2: AuthenticationManager

#### ✅ Erfolgreicher Login
- Login mit korrekten Credentials
- Prüft `IsAuthenticated` Status
- Validiert `CurrentUser`

#### ✅ Fehlgeschlagener Login
- Falsches Passwort
- Nicht existierender Benutzer
- System bleibt unauthenticated

#### ✅ Logout
- User wird ausgeloggt
- `CurrentUser` wird null
- `IsAuthenticated` wird false

#### ✅ Admin-Rechte-Prüfung
- Admin kann `RequireAdmin()` aufrufen
- Standard-User erhält Exception

#### ✅ Login-Versuchs-Limitierung
- 3 fehlgeschlagene Versuche möglich
- 4. Versuch löst Sperre aus
- `GetRemainingLoginAttempts()` zählt korrekt

#### ✅ Session-Tracking
- `LastActivity` wird gesetzt
- `UpdateActivity()` aktualisiert Zeitstempel

---

## Test-Ausgabe verstehen

### Erfolgreiche Tests
```
✅ PASS - Test-Name
```

### Fehlgeschlagene Tests
```
❌ FAIL - Test-Name (Fehlermeldung)
```

### Zusammenfassung
```
==========================================
Tests abgeschlossen: 13/13 erfolgreich
✅ Alle Tests bestanden!
==========================================
```

---

## Manuelle Tests

### Test 1: PasswordHasher manuell testen

```csharp
// In Kernel.cs oder einer Test-Methode:
string password = "meinPasswort123";
string hash = PasswordHasher.Hash(password);
Console.WriteLine($"Hash: {hash}");

bool correct = PasswordHasher.Verify("meinPasswort123", hash);
bool wrong = PasswordHasher.Verify("falsch", hash);
Console.WriteLine($"Korrekt: {correct}, Falsch: {wrong}");
```

### Test 2: User erstellen und Passwort prüfen

```csharp
var user = new User("admin", PasswordHasher.Hash("admin123"), UserRole.Admin);
Console.WriteLine($"User: {user}");
Console.WriteLine($"Ist Admin: {user.IsAdmin()}");

bool passwordOk = user.VerifyPassword("admin123");
Console.WriteLine($"Passwort korrekt: {passwordOk}");
```

### Test 3: AuthenticationManager

```csharp
var authManager = new AuthenticationManager();
var users = new List<User>
{
    new User("admin", PasswordHasher.Hash("admin"), UserRole.Admin),
    new User("user", PasswordHasher.Hash("pass"), UserRole.Standard)
};
authManager.SetUsers(users);

// Login als Admin
if (authManager.Login("admin", "admin"))
{
    Console.WriteLine($"Eingeloggt als: {authManager.CurrentUser.Username}");
    Console.WriteLine($"Ist Admin: {authManager.CurrentUser.IsAdmin()}");
    
    try
    {
        authManager.RequireAdmin();
        Console.WriteLine("✅ Admin-Rechte bestätigt");
    }
    catch
    {
        Console.WriteLine("❌ Admin-Rechte fehlen");
    }
}

// Logout
authManager.Logout();
Console.WriteLine($"Nach Logout authenticated: {authManager.IsAuthenticated}");

// Login mit falschen Daten
bool failed = authManager.Login("admin", "wrong");
Console.WriteLine($"Login mit falschem Passwort: {failed}");
Console.WriteLine($"Verbleibende Versuche: {authManager.GetRemainingLoginAttempts()}");
```

---

## Probleme und Lösungen

### Problem: Tests schlagen fehl

**Lösung:**
1. Prüfe die Fehlermeldung in Klammern
2. Überprüfe dass alle Dateien kompilieren
3. Stelle sicher dass `system/` Verzeichnis alle Dateien enthält:
   - User.cs
   - UserRole.cs
   - PasswordHasher.cs
   - AuthenticationManager.cs
   - UserSystemTest.cs

### Problem: "test" Befehl nicht gefunden

**Lösung:**
- Stelle sicher dass Kernel.cs aktualisiert wurde
- Kompiliere das Projekt neu
- Starte SlotOS neu

### Problem: Cosmos OS kompiliert nicht

**Lösung:**
- Überprüfe dass alle using-Statements vorhanden sind
- Stelle sicher dass Cosmos.System referenziert ist
- Prüfe ob .NET Version kompatibel ist

---

## Erweiterte Tests

### Performance-Test: Hash-Geschwindigkeit

```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
for (int i = 0; i < 100; i++)
{
    PasswordHasher.Hash($"password{i}");
}
sw.Stop();
Console.WriteLine($"100 Hashes in {sw.ElapsedMilliseconds}ms");
```

### Sicherheits-Test: Brute-Force-Schutz

```csharp
var authManager = new AuthenticationManager();
var users = new List<User> { new User("test", PasswordHasher.Hash("pass"), UserRole.Standard) };
authManager.SetUsers(users);

for (int i = 0; i < 5; i++)
{
    try
    {
        bool success = authManager.Login("test", $"wrong{i}");
        Console.WriteLine($"Versuch {i+1}: {(success ? "Erfolg" : "Fehlgeschlagen")}");
        Console.WriteLine($"Verbleibende Versuche: {authManager.GetRemainingLoginAttempts()}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Versuch {i+1}: GESPERRT - {ex.Message}");
    }
}
```

---

## Nächste Schritte

Nach erfolgreichen Tests von Phase 1 & 2:

1. **Phase 3**: UserManager implementieren
2. **Phase 4**: Datenpersistenz mit Cosmos VFS
3. **Phase 5**: Vollständiges Login-System mit UI

---

**Erstellt:** 2025-10-06  
**Version:** 1.0  
**Für:** SlotOS Nutzerverwaltung
