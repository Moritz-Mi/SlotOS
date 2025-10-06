# Implementierungsleitfaden - SlotOS Benutzerverwaltung

## Übersicht

Diese Dokumentation erklärt, wie die Benutzerverwaltung im SlotOS implementiert wurde und wie Sie diese erweitern können.

## Architektur-Entscheidungen

### 1. In-Memory Datenbank

**Warum:** 
- Cosmos OS hat eingeschränkte Filesystem-Unterstützung
- Schneller Zugriff während der Laufzeit
- Einfache Implementierung für Proof-of-Concept

**Nachteil:**
- Daten gehen beim Neustart verloren

**Lösung für Produktion:**
Implementieren Sie Serialisierung in eine Datei (JSON/XML) beim Shutdown und Laden beim Startup.

### 2. Einfacher Hash-Algorithmus (djb2)

**Warum:**
- Funktioniert in Cosmos OS ohne externe Dependencies
- Kein Zugriff auf .NET Kryptographie-APIs in Cosmos

**Nachteil:**
- Nicht kryptographisch sicher

**Lösung für Produktion:**
- Portieren Sie einen stärkeren Hash-Algorithmus (SHA-256)
- Implementieren Sie Salt für jeden Benutzer
- Verwenden Sie Key Stretching (PBKDF2)

### 3. Flag-basierte Berechtigungen (Enum Flags)

**Warum:**
- Effiziente Speicherung (ein Integer pro Benutzer)
- Schnelle Überprüfung mit Bitwise-Operationen
- Einfache Kombination von Berechtigungen

**Beispiel:**
```csharp
[Flags]
public enum Permission
{
    Read = 1,      // 0001
    Write = 2,     // 0010
    Execute = 4,   // 0100
    Delete = 8     // 1000
}

// Kombination: ReadWrite = 0011 (3)
Permission combined = Permission.Read | Permission.Write;

// Prüfung:
bool canWrite = (userPerms & Permission.Write) == Permission.Write;
```

## Implementierungsdetails

### Authentifizierung-Flow

```
1. Systemstart
   └─> InitializeUserManagement()
       ├─> UserDatabase erstellen
       ├─> Standard-Admin erstellen
       └─> Manager initialisieren

2. Login-Prozess
   └─> PerformLogin()
       ├─> Username eingeben
       ├─> Passwort eingeben (maskiert)
       ├─> Passwort hashen
       ├─> Mit gespeichertem Hash vergleichen
       └─> Session erstellen

3. Command-Loop
   └─> Run()
       ├─> Berechtigungen prüfen
       ├─> Befehl ausführen
       └─> Zurück zu Run()

4. Logout
   └─> Session beenden
       └─> Zurück zu Login
```

### Berechtigungs-Prüfung

```csharp
// 1. Direkte Prüfung
if (user.HasPermission(Permission.Write))
{
    // Operation ausführen
}

// 2. Über AuthenticationManager
if (authManager.CheckPermission(Permission.CreateUser))
{
    // Benutzer erstellen
}

// 3. Über PermissionManager
if (permissionManager.CanPerformOperation(Permission.Delete))
{
    // Löschen erlauben
}
```

### Dateirechte-System

Das Dateirechtesystem orientiert sich an Unix/Linux:

```
Owner  Group  Others
rwx    rwx    rwx
421    421    421

r = Read (4)
w = Write (2)
x = Execute (1)
```

**Beispiel:**
- `rwxr-xr--` = Owner: rwx(7), Group: r-x(5), Others: r--(4)
- `rw-rw-r--` = Owner: rw-(6), Group: rw-(6), Others: r--(4)

## Erweiterungen implementieren

### 1. Persistente Speicherung

```csharp
public class UserDatabase
{
    private const string USER_DB_FILE = "0:\\system\\users.db";

    // Beim Shutdown speichern
    public void SaveToFile()
    {
        try
        {
            var data = new
            {
                Users = users,
                Groups = groups,
                NextUserId = nextUserId,
                NextGroupId = nextGroupId
            };

            string json = SerializeToJson(data);
            File.WriteAllText(USER_DB_FILE, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
        }
    }

    // Beim Startup laden
    public void LoadFromFile()
    {
        if (!File.Exists(USER_DB_FILE))
        {
            CreateDefaultAdmin();
            return;
        }

        try
        {
            string json = File.ReadAllText(USER_DB_FILE);
            var data = DeserializeFromJson(json);
            users = data.Users;
            groups = data.Groups;
            nextUserId = data.NextUserId;
            nextGroupId = data.NextGroupId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden: {ex.Message}");
            CreateDefaultAdmin();
        }
    }
}

// Im Kernel:
protected override void Stop()
{
    userDatabase.SaveToFile();
    base.Stop();
}
```

### 2. Erweiterte Passwort-Sicherheit

```csharp
public class PasswordManager
{
    // Salt generieren
    public static string GenerateSalt()
    {
        var random = new Random();
        var salt = new byte[16];
        random.NextBytes(salt);
        return Convert.ToBase64String(salt);
    }

    // Passwort mit Salt hashen
    public static string HashPassword(string password, string salt)
    {
        string combined = password + salt;
        // Mehrfaches Hashen (Key Stretching)
        string hash = combined;
        for (int i = 0; i < 1000; i++)
        {
            hash = SimpleHash(hash);
        }
        return hash;
    }

    // Passwort-Komplexität prüfen
    public static bool IsPasswordStrong(string password)
    {
        if (password.Length < 8) return false;
        
        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            if (char.IsLower(c)) hasLower = true;
            if (char.IsDigit(c)) hasDigit = true;
            if (!char.IsLetterOrDigit(c)) hasSpecial = true;
        }

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}

// User-Klasse erweitern:
public class User
{
    public string Salt { get; set; }
    
    // Passwort setzen
    public void SetPassword(string password)
    {
        Salt = PasswordManager.GenerateSalt();
        PasswordHash = PasswordManager.HashPassword(password, Salt);
    }
    
    // Passwort verifizieren
    public bool VerifyPassword(string password)
    {
        string hash = PasswordManager.HashPassword(password, Salt);
        return hash == PasswordHash;
    }
}
```

### 3. Audit-Log System

```csharp
public class AuditLog
{
    public DateTime Timestamp { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; }
    public string Details { get; set; }
    public bool Success { get; set; }
}

public class AuditLogger
{
    private List<AuditLog> logs = new List<AuditLog>();
    
    public void Log(int userId, string action, string details, bool success)
    {
        logs.Add(new AuditLog
        {
            Timestamp = DateTime.Now,
            UserId = userId,
            Action = action,
            Details = details,
            Success = success
        });
    }
    
    public List<AuditLog> GetLogs(int? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = logs.AsEnumerable();
        
        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);
            
        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);
            
        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);
            
        return query.ToList();
    }
}

// Verwendung in AuthenticationManager:
public bool Login(string username, string password)
{
    var user = userDatabase.GetUserByUsername(username);
    
    if (user == null)
    {
        auditLogger.Log(0, "LOGIN_FAILED", $"Unknown user: {username}", false);
        return false;
    }
    
    if (!user.VerifyPassword(password))
    {
        auditLogger.Log(user.UserId, "LOGIN_FAILED", "Wrong password", false);
        return false;
    }
    
    currentUser = user;
    isLoggedIn = true;
    auditLogger.Log(user.UserId, "LOGIN_SUCCESS", "", true);
    return true;
}
```

### 4. Session-Timeout

```csharp
public class SessionManager
{
    private DateTime lastActivity;
    private TimeSpan timeout = TimeSpan.FromMinutes(15);
    
    public void UpdateActivity()
    {
        lastActivity = DateTime.Now;
    }
    
    public bool IsSessionExpired()
    {
        return (DateTime.Now - lastActivity) > timeout;
    }
    
    public void SetTimeout(int minutes)
    {
        timeout = TimeSpan.FromMinutes(minutes);
    }
}

// Im Kernel Run():
protected override void Run()
{
    if (sessionManager.IsSessionExpired())
    {
        Console.WriteLine("Session abgelaufen. Bitte erneut einloggen.");
        authManager.Logout();
        PerformLogin();
        return;
    }
    
    sessionManager.UpdateActivity();
    // ... restlicher Code
}
```

### 5. Sudo-Funktionalität

```csharp
public class SudoManager
{
    private AuthenticationManager authManager;
    private DateTime? sudoExpiry;
    private TimeSpan sudoDuration = TimeSpan.FromMinutes(5);
    
    public bool RequestSudo(string password)
    {
        if (!authManager.IsLoggedIn)
            return false;
            
        var user = authManager.CurrentUser;
        if (!user.VerifyPassword(password))
            return false;
            
        sudoExpiry = DateTime.Now.Add(sudoDuration);
        return true;
    }
    
    public bool HasSudoPrivileges()
    {
        if (!sudoExpiry.HasValue)
            return false;
            
        if (DateTime.Now > sudoExpiry.Value)
        {
            sudoExpiry = null;
            return false;
        }
        
        return true;
    }
}

// Befehl implementieren:
case "sudo":
    Console.Write("Passwort: ");
    string pwd = ReadPassword();
    if (sudoManager.RequestSudo(pwd))
    {
        Console.WriteLine("Sudo-Rechte gewaehrt fuer 5 Minuten.");
    }
    break;
```

## Best Practices

### 1. Berechtigungsprüfung

✅ **Gut:**
```csharp
public bool DeleteFile(string path)
{
    if (!permissionManager.CanPerformOperation(Permission.Delete))
    {
        Console.WriteLine("Keine Berechtigung zum Loeschen.");
        return false;
    }
    
    if (!filePermissionManager.CanAccessFile(path, Permission.Delete))
    {
        Console.WriteLine("Keine Berechtigung fuer diese Datei.");
        return false;
    }
    
    // Datei löschen
    return true;
}
```

❌ **Schlecht:**
```csharp
public bool DeleteFile(string path)
{
    // Keine Berechtigungsprüfung!
    // Datei löschen
    return true;
}
```

### 2. Fehlerbehandlung

✅ **Gut:**
```csharp
try
{
    var user = userDatabase.GetUserByUsername(username);
    if (user == null)
    {
        Console.WriteLine("Benutzer nicht gefunden.");
        return false;
    }
    // Verarbeitung
}
catch (Exception ex)
{
    Console.WriteLine($"Fehler: {ex.Message}");
    return false;
}
```

### 3. Input-Validierung

✅ **Gut:**
```csharp
public bool CreateUser(string username, string password)
{
    if (string.IsNullOrWhiteSpace(username))
    {
        Console.WriteLine("Username darf nicht leer sein.");
        return false;
    }
    
    if (username.Length < 3 || username.Length > 20)
    {
        Console.WriteLine("Username muss zwischen 3 und 20 Zeichen lang sein.");
        return false;
    }
    
    if (!IsValidUsername(username))
    {
        Console.WriteLine("Username enthaelt ungueltige Zeichen.");
        return false;
    }
    
    // Benutzer erstellen
}
```

## Testszenarien

### Test 1: Benutzer erstellen und einloggen
```
1. Als Admin einloggen (admin/admin)
2. Befehl: adduser testuser password123 User
3. Befehl: logout
4. Als testuser einloggen (testuser/password123)
5. Befehl: whoami
   Erwartung: "Eingeloggt als: testuser"
```

### Test 2: Berechtigungen testen
```
1. Als Admin einloggen
2. Befehl: adduser restricted secret Guest
3. Befehl: logout
4. Als restricted einloggen
5. Befehl: adduser newuser pass User
   Erwartung: "Keine Berechtigung"
6. Befehl: listusers
   Erwartung: "Keine Berechtigung"
```

### Test 3: Passwort ändern
```
1. Als Admin einloggen
2. Befehl: passwd neupass123
3. Altes Passwort eingeben: admin
   Erwartung: "Passwort erfolgreich geaendert"
4. Befehl: logout
5. Login mit neuem Passwort: admin/neupass123
   Erwartung: Erfolgreicher Login
```

## Fehlerbehebung

### Problem: "Benutzerverwaltung initialisiert" wird nicht angezeigt

**Lösung:** Prüfen Sie, ob alle Dateien korrekt kompiliert wurden und im UserManagement-Namespace liegen.

### Problem: Login schlägt immer fehl

**Lösung:** 
1. Prüfen Sie die Hash-Funktion
2. Stellen Sie sicher, dass der Admin-User korrekt erstellt wurde
3. Debuggen Sie die Login-Methode

### Problem: Berechtigungen funktionieren nicht

**Lösung:**
1. Prüfen Sie, ob `HasPermission` korrekt implementiert ist
2. Stellen Sie sicher, dass Benutzer die richtigen Permissions gesetzt bekommen
3. Debuggen Sie die Bitwise-Operationen

## Performance-Optimierungen

### 1. Dictionary statt List für Benutzersuche
✅ Bereits implementiert: O(1) Zugriff per UserId

### 2. Caching von Berechtigungen
```csharp
private Dictionary<int, Permission> permissionCache = new Dictionary<int, Permission>();

public Permission GetEffectivePermissions(int userId)
{
    if (permissionCache.ContainsKey(userId))
        return permissionCache[userId];
        
    var user = GetUserById(userId);
    Permission effective = user.Permissions;
    
    // Gruppen-Berechtigungen hinzufügen
    foreach (int groupId in user.GroupIds)
    {
        var group = GetGroupById(groupId);
        effective |= group.GroupPermissions;
    }
    
    permissionCache[userId] = effective;
    return effective;
}
```

## Zusammenfassung

Die implementierte Benutzerverwaltung bietet eine solide Grundlage für ein Multi-User-System. Die modulare Architektur erlaubt einfache Erweiterungen und Anpassungen.

**Nächste Schritte:**
1. Implementieren Sie persistente Speicherung
2. Verbessern Sie die Passwort-Sicherheit
3. Fügen Sie ein Audit-Log hinzu
4. Integrieren Sie das Rechtesystem vollständig ins Dateisystem

---

Bei Fragen oder Problemen, konsultieren Sie die README.md oder den Quellcode.
