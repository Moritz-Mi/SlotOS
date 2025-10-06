# SlotOS - Cosmos Kernel mit Benutzerverwaltung

Ein Cosmos OS Kernel mit vollständiger Benutzerverwaltung, Rechtesystem und Zugriffskontrolle.

## Features

### ✅ Implementierte Funktionen

- **Multi-User System**: Mehrere Benutzer mit individuellen Accounts
- **Authentifizierung**: Login/Logout-System mit Passwort-Hashing
- **Rollenbasierte Berechtigungen**: 4 Benutzerrollen (Guest, User, PowerUser, Administrator)
- **Detaillierte Rechteverwaltung**: Granulare Berechtigungen (Read, Write, Execute, Delete, etc.)
- **Gruppenverwaltung**: Benutzergruppen für vereinfachte Rechtevergabe
- **Dateisystem-Berechtigungen**: Unix-ähnliche Dateirechte (Owner/Group/Others)
- **Zugriffskontrolle**: Überprüfung von Berechtigungen bei allen Operationen
- **Benutzerverwaltung**: Erstellen, Deaktivieren, Aktivieren von Benutzern
- **Sicherheit**: Passwort-Hashing, Login-Versuche limitiert

## Architektur

### Komponenten

```
CosmosKernel1/
├── Kernel.cs                          # Haupt-Kernel mit Login-System
└── UserManagement/
    ├── Permissions.cs                 # Enums und Berechtigungen
    ├── User.cs                        # User- und UserGroup-Klassen
    ├── AuthenticationManager.cs       # Login/Logout/Authentifizierung
    ├── PermissionManager.cs           # Rechteverwaltung
    ├── UserDatabase.cs                # In-Memory Benutzerdatenbank
    ├── FilePermissionManager.cs       # Dateisystem-Berechtigungen
    └── CommandHandler.cs              # Befehlsverarbeitung
```

### Klassenübersicht

#### 1. **Permissions.cs**
- `Permission` (Enum): Definiert einzelne Berechtigungen (Read, Write, Execute, etc.)
- `UserRole` (Enum): Vordefinierte Rollen (Guest, User, PowerUser, Administrator)
- `PermissionHelper`: Hilfsmethoden für Berechtigungsprüfungen

#### 2. **User.cs**
- `User`: Repräsentiert einen Benutzer mit Username, Passwort-Hash, Rolle, Permissions
- `UserGroup`: Repräsentiert eine Benutzergruppe mit gemeinsamen Berechtigungen

#### 3. **AuthenticationManager.cs**
- Login/Logout-Funktionalität
- Passwort-Hashing (djb2-Algorithmus)
- Session-Verwaltung
- Passwort-Änderung

#### 4. **PermissionManager.cs**
- Berechtigungsprüfung für Operationen
- Berechtigungen gewähren/entziehen
- Benutzerrollen ändern
- Benutzer aktivieren/deaktivieren

#### 5. **UserDatabase.cs**
- In-Memory Datenbank für Benutzer und Gruppen
- CRUD-Operationen für Benutzer
- Standard-Admin-Account und Gruppen

#### 6. **FilePermissionManager.cs**
- Unix-ähnliche Dateiberechtigungen (Owner/Group/Others)
- Berechtigungsprüfung für Dateioperationen
- Besitzer- und Gruppenverwaltung für Dateien

#### 7. **CommandHandler.cs**
- Verarbeitet Benutzerbefehle
- Implementiert Command-Line Interface

## Verwendung

### Systemstart

Beim Systemstart wird automatisch:
1. Die Benutzerverwaltung initialisiert
2. Ein Standard-Admin-Account erstellt
3. Der Login-Prozess gestartet

**Standard-Admin-Zugangsdaten:**
- Benutzername: `admin`
- Passwort: `admin`

⚠️ **WICHTIG:** Ändern Sie das Admin-Passwort nach dem ersten Login!

### Login

```
=== Login ===
Benutzername: admin
Passwort: ****
```

Nach erfolgreichem Login wird ein Prompt angezeigt:
```
admin@SlotOS>
```

## Befehle

### Allgemeine Befehle

| Befehl | Beschreibung |
|--------|-------------|
| `help` | Zeigt alle verfügbaren Befehle |
| `whoami` | Zeigt den aktuellen Benutzer und seine Rolle |
| `logout` | Meldet den Benutzer ab |
| `exit` / `shutdown` | Fährt das System herunter |

### Benutzerverwaltung

| Befehl | Beschreibung | Erforderliche Berechtigung |
|--------|-------------|---------------------------|
| `adduser <username> <password> [role]` | Erstellt einen neuen Benutzer | CreateUser |
| `passwd <neues_passwort>` | Ändert das eigene Passwort | - |
| `listusers` | Listet alle Benutzer auf | SystemAdmin |
| `userinfo <username>` | Zeigt Benutzerinformationen | - |
| `deactivate <username>` | Deaktiviert einen Benutzer | DeleteUser |
| `activate <username>` | Aktiviert einen Benutzer | CreateUser |

**Beispiele:**
```bash
adduser testuser password123 User
adduser poweruser secret PowerUser
passwd neuespasswort
listusers
userinfo testuser
deactivate testuser
```

### Rechteverwaltung

| Befehl | Beschreibung | Erforderliche Berechtigung |
|--------|-------------|---------------------------|
| `grant <username> <permission>` | Gewährt eine Berechtigung | ModifyPermissions |
| `revoke <username> <permission>` | Entzieht eine Berechtigung | ModifyPermissions |

**Verfügbare Berechtigungen:**
- `Read` - Leseberechtigung
- `Write` - Schreibberechtigung
- `Execute` - Ausführungsberechtigung
- `Delete` - Löschberechtigung
- `CreateUser` - Benutzer erstellen
- `DeleteUser` - Benutzer löschen
- `ModifyPermissions` - Berechtigungen ändern
- `SystemAdmin` - System-Administrator

**Beispiele:**
```bash
grant testuser Write
grant testuser Execute
revoke testuser Delete
```

### Dateisystem-Berechtigungen

| Befehl | Beschreibung | Erforderliche Berechtigung |
|--------|-------------|---------------------------|
| `chmod <datei> <rechte>` | Ändert Dateiberechtigungen | Owner oder ModifyPermissions |
| `chown <datei> <username>` | Ändert Dateibesitzer | ModifyPermissions |
| `fileinfo <datei>` | Zeigt Dateiberechtigungen | - |

**Beispiele:**
```bash
chown 0:\test.txt testuser
fileinfo 0:\test.txt
```

## Berechtigungssystem

### Benutzerrollen

| Rolle | Berechtigungen | Beschreibung |
|-------|---------------|-------------|
| **Guest** | Read | Nur Lesezugriff |
| **User** | Read, Write, Execute | Standard-Benutzerrechte |
| **PowerUser** | Read, Write, Execute, Delete | Erweiterte Rechte |
| **Administrator** | Alle Berechtigungen | Volle Systemkontrolle |

### Standard-Gruppen

1. **Administrators** - Volle Systemrechte
2. **Users** - Standard-Benutzerrechte
3. **Guests** - Nur-Lese-Zugriff

## Sicherheit

### Implementierte Sicherheitsmaßnahmen

1. **Passwort-Hashing**: Passwörter werden nie im Klartext gespeichert
2. **Login-Begrenzung**: Maximal 3 Login-Versuche
3. **Maskierte Passworteingabe**: Passwörter werden als `***` angezeigt
4. **Berechtigungsprüfung**: Jede Operation wird auf Berechtigungen geprüft
5. **Account-Deaktivierung**: Accounts können deaktiviert werden
6. **Selbstschutz**: Admin kann sich nicht selbst deaktivieren

### Hinweise für Produktivumgebungen

⚠️ Der aktuelle Passwort-Hash-Algorithmus (djb2) ist für Demonstrationszwecke.
Für produktive Systeme sollten Sie:
- Einen stärkeren Hash-Algorithmus verwenden (z.B. SHA-256 mit Salt)
- Passwort-Komplexitätsregeln implementieren
- Persistente Speicherung der Benutzerdaten (aktuell nur in-memory)

## Erweiterungsmöglichkeiten

### Geplante Features

- [ ] Persistente Speicherung in Dateisystem
- [ ] Erweiterte Passwort-Richtlinien
- [ ] Audit-Log für Sicherheitsrelevante Aktionen
- [ ] Session-Timeout
- [ ] Erweiterte ACL (Access Control Lists)
- [ ] Sudo-Funktionalität
- [ ] Home-Verzeichnisse für Benutzer
- [ ] Erweiterte Gruppenverwaltung

### Implementierungsvorschläge

#### Persistente Speicherung

```csharp
// Benutzer in JSON-Datei speichern
public void SaveToFile(string filePath)
{
    var json = JsonSerializer.Serialize(users);
    File.WriteAllText(filePath, json);
}

// Benutzer aus Datei laden
public void LoadFromFile(string filePath)
{
    var json = File.ReadAllText(filePath);
    users = JsonSerializer.Deserialize<Dictionary<int, User>>(json);
}
```

#### Stärkerer Passwort-Hash

```csharp
// SHA-256 mit Salt implementieren
public static string HashPassword(string password, string salt)
{
    using (var sha256 = SHA256.Create())
    {
        var saltedPassword = password + salt;
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(bytes);
    }
}
```

## Technische Details

### Anforderungen

- **Framework**: .NET 6.0
- **Platform**: Cosmos
- **Packages**:
  - Cosmos.Build (0-*)
  - Cosmos.Debug.Kernel (0-*)
  - Cosmos.System2 (0-*)

### Build & Deployment

1. Öffnen Sie die Solution in Visual Studio
2. Stellen Sie sicher, dass alle Cosmos-Packages installiert sind
3. Kompilieren Sie das Projekt
4. Deployen Sie auf VMware/QEMU/Hardware

### Bekannte Einschränkungen

- Benutzerdaten werden nur im RAM gespeichert (verloren nach Neustart)
- Vereinfachter Hash-Algorithmus
- Keine Passwort-Komplexitätsregeln
- Dateirechte noch nicht vollständig in Filesystem integriert

## Lizenz

Dieses Projekt ist für Bildungszwecke erstellt.

## Autoren

- SlotOS Development Team

## Version

**Version 1.0.0** - Initiale Release mit vollständiger Benutzerverwaltung

---

© 2025 SlotOS - Cosmos Kernel Project
#   S l o t O S  
 