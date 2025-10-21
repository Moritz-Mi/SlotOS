# Plan zur Erstellung einer Nutzerverwaltung für SlotOS

## Übersicht
Dieses Dokument beschreibt den schrittweisen Plan zur Implementierung eines vollständigen Nutzerverwaltungssystems für SlotOS.

## Ziele
- Mehrere Benutzer mit unterschiedlichen Berechtigungen verwalten
- Sichere Authentifizierung (Login/Logout)
- Passwort-Speicherung mit Hashing
- Benutzerprofile und -daten persistent speichern
- Berechtigungssystem (Admin/Standard-Benutzer)

---

## Phase 1: Grundlegende Datenstrukturen

### 1.1 User-Klasse erstellen
**Datei:** `User.cs`

**Eigenschaften:**
- `string Username` - Benutzername (eindeutig)
- `string PasswordHash` - Gehashtes Passwort
- `UserRole Role` - Benutzerrolle (Admin/User)
- `DateTime CreatedAt` - Erstellungsdatum
- `DateTime LastLogin` - Letzter Login
- `bool IsActive` - Kontostatus
- `string HomeDirectory` - Benutzer-Heimverzeichnis

**Methoden:**
- `bool VerifyPassword(string password)` - Passwort überprüfen
- `void UpdatePassword(string newPassword)` - Passwort ändern

### 1.2 UserRole Enum erstellen
**Datei:** `UserRole.cs`

```csharp
public enum UserRole
{
    Admin,      // Volle Systemrechte
    Standard,   // Normale Benutzerrechte
    Guest       // Eingeschränkte Rechte
}
```

---

## Phase 2: Authentifizierungs-System ✅ ABGESCHLOSSEN

### 2.1 PasswordHasher-Klasse ✅
**Datei:** `PasswordHasher.cs`

**Funktionen:**
- `string Hash(string password)` - Passwort hashen mit Salt
- `bool Verify(string password, string hash)` - Hash überprüfen
- Salt-Generierung für zusätzliche Sicherheit
- Mehrfache Hash-Iterationen (1000 Runden)
- Abwärtskompatibilität mit altem Hash-Format

**Implementiert am:** 2025-10-06

### 2.2 AuthenticationManager-Klasse ✅
**Datei:** `AuthenticationManager.cs`

**Eigenschaften:**
- `User CurrentUser` - Aktuell angemeldeter Benutzer
- `bool IsAuthenticated` - Login-Status
- `DateTime LastActivity` - Zeitpunkt der letzten Aktivität

**Methoden:**
- `bool Login(string username, string password)` - Benutzer anmelden
- `void Logout()` - Benutzer abmelden
- `bool RequireAdmin()` - Admin-Rechte prüfen
- `void RequireAuthentication()` - Authentifizierung prüfen
- `bool IsSessionExpired()` - Session-Timeout prüfen
- `int GetRemainingLoginAttempts()` - Verbleibende Versuche

**Zusätzliche Features:**
- Login-Versuche limitieren (max. 3 Versuche)
- Automatische Sperrung nach Fehlversuchen (30 Sekunden)
- Session-Timeout-Verwaltung
- Aktivitäts-Tracking

**Implementiert am:** 2025-10-06

### 2.3 User-Klasse Integration ✅
- `VerifyPassword()` nutzt jetzt `PasswordHasher.Verify()`
- `UpdatePassword()` nutzt jetzt `PasswordHasher.Hash()`
- Alte `ComputeHash()` Methode entfernt
- Vollständige Abwärtskompatibilität gewährleistet

**Aktualisiert am:** 2025-10-06

---

## Phase 3: Benutzerverwaltung ✅ ABGESCHLOSSEN

### 3.1 UserManager-Klasse ✅
**Datei:** `UserManager.cs`

**Funktionen:**
- `bool CreateUser(string username, string password, UserRole role)` - Neuen Benutzer anlegen
- `bool DeleteUser(string username)` - Benutzer löschen
- `User GetUser(string username)` - Benutzer abrufen
- `List<User> GetAllUsers()` - Alle Benutzer auflisten
- `bool UpdateUser(User user)` - Benutzerdaten aktualisieren
- `bool ChangePassword(string username, string oldPassword, string newPassword)` - Passwort ändern
- `bool ResetPassword(string username, string newPassword)` - Passwort zurücksetzen (Admin)
- `bool UserExists(string username)` - Prüfen ob Benutzer existiert
- `bool SetUserActive(string username, bool isActive)` - Benutzer aktivieren/deaktivieren
- `string GetStatistics()` - Statistiken über Benutzer abrufen

**Zusätzliche Features:**
- Singleton-Pattern für zentrale Verwaltung
- Schutz vor Löschung/Deaktivierung des letzten Administrators
- Passwortvalidierung (mindestens 4 Zeichen)
- Benutzernamen-Validierung
- Automatische Home-Verzeichnis-Zuweisung

**Implementiert am:** 2025-10-21

### 3.2 Standard-Administrator erstellen ✅
- Bei Erststart: Admin-Account mit Default-Passwort anlegen
- Standard-Benutzername: `admin`
- Standard-Passwort: `admin`
- Nutzer sollte beim ersten Login Passwort ändern

**Implementiert am:** 2025-10-21

---

## Phase 4: Datenpersistenz

### 4.1 UserStorage-Klasse
**Datei:** `UserStorage.cs`

**Funktionen:**
- `void SaveUsers(List<User> users)` - Benutzerliste speichern
- `List<User> LoadUsers()` - Benutzerliste laden
- `bool FileExists()` - Prüfen ob Datei existiert

**Speicherformat:**
- JSON-Format für einfache Serialisierung
- Datei: `/system/users.dat` oder ähnlich
- Cosmos VFS (Virtual File System) nutzen

### 4.2 Backup-System
- Automatische Backups bei Änderungen
- Wiederherstellung bei Dateikorruption

---

## Phase 5: Kommandozeilen-Interface

### 5.1 Login-Screen
**Funktion:** `DisplayLoginScreen()`

- Benutzername-Eingabe
- Passwort-Eingabe (versteckt mit ***)
- Login-Versuche limitieren (max. 3 Versuche)
- Fehlermeldungen anzeigen

### 5.2 Benutzerverwaltungs-Befehle

**Für Admin-Benutzer:**
- `useradd <username> <password> [role]` - Benutzer hinzufügen
- `userdel <username>` - Benutzer löschen
- `usermod <username> [optionen]` - Benutzer bearbeiten
- `userlist` - Alle Benutzer auflisten
- `passwd <username>` - Passwort ändern (Admin)

**Für alle Benutzer:**
- `passwd` - Eigenes Passwort ändern
- `whoami` - Aktuellen Benutzer anzeigen
- `logout` - Abmelden

### 5.3 CommandHandler-Klasse
**Datei:** `CommandHandler.cs`

- Befehle parsen und an entsprechende Manager weiterleiten
- Berechtigungsprüfung vor Ausführung
- Hilfe-System (`help`, `man`)

---

## Phase 6: Berechtigungssystem

### 6.1 PermissionChecker-Klasse
**Datei:** `PermissionChecker.cs`

**Funktionen:**
- `bool HasPermission(User user, string action)` - Berechtigung prüfen
- `bool IsAdmin(User user)` - Admin-Status prüfen
- `void DenyAccess(string reason)` - Zugriff verweigern

### 6.2 Geschützte Operationen definieren
- Systemdateien ändern (nur Admin)
- Andere Benutzer verwalten (nur Admin)
- Eigene Dateien verwalten (alle Benutzer)

---

## Phase 7: Integration in Kernel

### 7.1 Kernel.cs erweitern
**Änderungen:**

```csharp
protected override void BeforeRun()
{
    // VFS initialisieren
    // UserManager initialisieren
    // Standard-Admin erstellen falls nicht vorhanden
    // Login-Screen anzeigen
}

protected override void Run()
{
    // Nur ausführen wenn eingeloggt
    if (AuthenticationManager.IsAuthenticated)
    {
        // Kommandozeile mit Benutzername anzeigen
        // Befehle entgegennehmen und verarbeiten
        // CommandHandler aufrufen
    }
}
```

### 7.2 Sicherheits-Features
- Automatischer Logout nach Inaktivität
- Session-Management
- Audit-Log für Admin-Aktionen

---

## Phase 8: Testing & Validierung

### 8.1 Test-Szenarien
1. **Ersteinrichtung**
   - System startet ohne Benutzer
   - Admin-Account wird erstellt
   - Erstes Login funktioniert

2. **Benutzerverwaltung**
   - Benutzer erstellen/löschen
   - Passwörter ändern
   - Rollen zuweisen

3. **Authentifizierung**
   - Korrektes Login
   - Falsches Passwort
   - Nicht existierender Benutzer

4. **Berechtigungen**
   - Admin-Befehle als Standard-User blockiert
   - Admin kann alle Befehle ausführen

5. **Persistenz**
   - Daten bleiben nach Neustart erhalten
   - Backup/Restore funktioniert

### 8.2 Error Handling
- Ungültige Eingaben abfangen
- Datei-I/O Fehler behandeln
- Speicher-Probleme berücksichtigen

---

## Phase 9: Erweiterte Features (Optional)

### 9.1 Zusätzliche Funktionen
- **Gruppen-System:** Benutzer in Gruppen organisieren
- **Erweiterte Berechtigungen:** Feinkörnige Rechte-Verwaltung
- **Benutzerquota:** Speicherplatz-Limits
- **Login-Historie:** Alle Logins protokollieren
- **Passwort-Regeln:** Komplexitätsanforderungen
- **Zwei-Faktor-Authentifizierung:** Zusätzliche Sicherheitsebene

### 9.2 GUI-Oberfläche (Zukunft)
- Grafische Benutzerverwaltung
- Visuelles Login-Formular
- Benutzerprofile mit Avataren

---

## Implementierungs-Reihenfolge

### Sprint 1: Grundlagen (Woche 1) ✅
- [x] User-Klasse und UserRole Enum
- [x] PasswordHasher implementieren
- [x] Grundlegende UserManager-Funktionen

### Sprint 2: Authentifizierung (Woche 2) ✅
- [x] AuthenticationManager erstellen
- [ ] Login-Screen implementieren (Teil von Phase 5)
- [x] Logout-Funktionalität

### Sprint 3: Persistenz (Woche 3)
- [ ] VFS in Cosmos einrichten
- [ ] UserStorage implementieren
- [ ] Daten speichern/laden

### Sprint 4: Befehle (Woche 4)
- [ ] CommandHandler erstellen
- [ ] Alle Benutzerverwaltungs-Befehle
- [ ] Hilfe-System

### Sprint 5: Integration & Testing (Woche 5)
- [ ] In Kernel integrieren
- [ ] Alle Test-Szenarien durchführen
- [ ] Bugs fixen

### Sprint 6: Feinschliff (Woche 6)
- [ ] PermissionChecker verfeinern
- [ ] Sicherheits-Features
- [ ] Dokumentation vervollständigen

---

## Technische Anforderungen

### Abhängigkeiten
- Cosmos VFS für Dateisystem
- System.Security.Cryptography für Hashing (falls verfügbar)
- System.Text.Json für Serialisierung (oder alternative)

### Dateistruktur
```
SlotOS/
├── Kernel.cs                      # Haupt-Kernel
├── Authentication/
│   ├── AuthenticationManager.cs
│   ├── PasswordHasher.cs
│   └── PermissionChecker.cs
├── UserManagement/
│   ├── User.cs
│   ├── UserRole.cs
│   ├── UserManager.cs
│   └── UserStorage.cs
├── Commands/
│   └── CommandHandler.cs
└── Utils/
    └── ConsoleHelper.cs           # Hilfsfunktionen für UI
```

---

## Sicherheitsüberlegungen

1. **Passwort-Sicherheit**
   - Niemals Klartext-Passwörter speichern
   - Salt für jeden Hash verwenden
   - SHA256 oder stärker verwenden

2. **Datei-Sicherheit**
   - User-Datei nur für Admin lesbar
   - Backup-Verschlüsselung erwägen

3. **Session-Sicherheit**
   - Timeout nach Inaktivität
   - Logout bei kritischen Operationen

4. **Input-Validierung**
   - Benutzernamen-Format prüfen
   - Passwort-Länge enforzen
   - SQL-Injection-ähnliche Angriffe vermeiden

---

## Erfolgs-Kriterien

✅ Benutzer kann sich anmelden und abmelden
✅ Admin kann neue Benutzer erstellen und verwalten
✅ Passwörter sind sicher gehasht gespeichert
✅ Daten bleiben nach Neustart erhalten
✅ Berechtigungssystem funktioniert korrekt
✅ Alle Befehle sind implementiert und funktionieren
✅ Error Handling ist robust
✅ Code ist dokumentiert und wartbar

---

## Notizen

- Cosmos OS hat Einschränkungen gegenüber Standard-.NET
- Nicht alle Crypto-Funktionen könnten verfügbar sein
- VFS muss vor Verwendung initialisiert werden
- Memory-Management beachten (OS-Entwicklung!)

---

**Erstellt am:** 2025-10-06
**Version:** 1.0
**Projekt:** SlotOS Nutzerverwaltung
