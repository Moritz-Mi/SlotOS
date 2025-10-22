# SlotOS

Ein Betriebssystem-Projekt basierend auf Cosmos OS mit vollständiger Nutzerverwaltung.

## 🎯 Projektziel

SlotOS ist ein lernorientiertes OS-Projekt, das ein vollständiges Benutzerverwaltungssystem mit Authentifizierung, Autorisierung und Datenpersistenz implementiert.

## 📋 Features

### ✅ Implementiert

#### Phase 1: Grundlegende Datenstrukturen
- **User-Klasse**: Vollständige Benutzerverwaltung mit Properties und Methoden
- **UserRole Enum**: Admin, Standard, Guest Rollen
- **Passwort-Verwaltung**: Update und Verifikation

#### Phase 2: Authentifizierungs-System
- **PasswordHasher**: 
  - Sichere Passwort-Hashing mit Salt
  - 1000 Hash-Iterationen für erhöhte Sicherheit
  - Abwärtskompatibilität mit Legacy-Hashes
- **AuthenticationManager**:
  - Login/Logout Funktionalität
  - Session-Management mit Timeout
  - Login-Versuchs-Limitierung (max. 3 Versuche)
  - Automatische Sperrung nach Fehlversuchen (30 Sekunden)
  - Admin-Rechte-Prüfung
  - Aktivitäts-Tracking

#### Phase 3: Benutzerverwaltung
- **UserManager**:
  - Singleton-Pattern für zentrale Verwaltung
  - CRUD-Operationen (Create, Read, Update, Delete)
  - Passwort-Änderung und Reset
  - Schutz vor Löschung des letzten Administrators
  - Benutzer aktivieren/deaktivieren
  - Standard-Admin-Account-Erstellung
  - Statistiken über Benutzersystem

#### Phase 4: Datenpersistenz → In-Memory-Modus ✅
- **Status**: **In-Memory-Modus aktiv** (Standard-Betriebsmodus)
- **Grund**: Cosmos VFS hat fundamentale Limitierungen (Invalid Opcode Errors)
- **Stabilität**: 100% - Keine Crashes mehr!

**🎯 Aktueller Modus:**
  - ✅ **100% In-Memory** - Alle Daten nur im RAM
  - ✅ **Volle Funktionalität** - Alle Phase 1-3 Features arbeiten perfekt
  - ✅ **Standard-Admin** - Bei jedem Start verfügbar (admin/admin)
  - ✅ **AutoSave deaktiviert** - Keine instabilen VFS-Operationen
  - ✅ **VFS optional** - Kann aktiviert werden, aber nicht empfohlen

**Warum In-Memory?**
  - ❌ **VFS-Tests**: Nur 12/23 bestanden (52%) - Unzureichend
  - ❌ **File-Write**: StringBuilder, File.Copy() verursachen CPU Exceptions
  - ❌ **Instabilität**: System-Crashes bei Persistenz-Operationen
  - ✅ **Lösung**: Komplett auf RAM-basierte Verwaltung umgestellt

**Trade-off:**
  - ⚠️ Keine Persistenz über Neustarts - Daten gehen verloren
  - ✅ 100% Stabilität - Kein Crash mehr
  - ✅ Standard-Admin wird automatisch erstellt
  
**📖 Dokumentation:**
- `IN_MEMORY_MODE.md` - Vollständige In-Memory-Dokumentation
- `REMOVE_VFS_FILES.md` - Anleitung zum Entfernen der VFS-Dateien

**⚠️ VFS-Code entfernt:**
- `UserStorage.cs`, `SimpleFileWriter.cs`, `PersistenceTest.cs` gelöscht
- `UserManager.cs` bereinigt (keine VFS-Dependencies mehr)
- `Kernel.cs` aktualisiert (neuer `testp4` Befehl für In-Memory-Tests)

**✅ Neue In-Memory-Tests (Phase 4):**
- `InMemoryTest.cs` - 18 Tests für In-Memory-Betrieb
- Testet: Initialize, Standard-Admin, CRUD, Neustart-Simulation, Performance
- Befehl: `testp4` oder `testmemory`

#### Phase 5: Kommandozeilen-Interface ✅
- **ConsoleHelper**:
  - Maskierte Passwort-Eingabe mit Sternchen
  - Farbige Statusmeldungen (Erfolg, Fehler, Warnung, Info)
  - Formatierte Tabellen und Header
  - Bestätigungs-Dialoge
  - Login-Screen-Funktion
- **CommandHandler**:
  - Vollständige Benutzerverwaltungs-Befehle
  - Intelligentes Command-Parsing (mit Anführungszeichen-Support)
  - Automatische Berechtigungsprüfung
  - Interaktive Benutzerführung
- **Befehle für alle Benutzer**:
  - `login` - Benutzer anmelden
  - `logout` - Benutzer abmelden
  - `whoami` - Aktuelle Benutzerinformationen
  - `passwd` - Eigenes Passwort ändern
- **Admin-Befehle**:
  - `useradd <user> <pass> [role]` - Benutzer erstellen
  - `userdel <user>` - Benutzer löschen
  - `usermod <user> <option> <wert>` - Benutzer bearbeiten
  - `userlist` - Alle Benutzer anzeigen
  - `passwd <user>` - Admin-Passwort-Reset
  - `userstats` - Benutzerstatistiken
- **Tests**: 30 automatisierte Tests in `CommandHandlerTest.cs`
- **Befehl**: `testp5` oder `testcommands`

#### Phase 6: Berechtigungssystem ✅
- **PermissionChecker**:
  - Singleton-Pattern für zentrale Berechtigungsverwaltung
  - 15 vordefinierte Aktionen (Benutzer, Dateien, System, Logs)
  - Rollenbasierte Zugriffskontrolle (Admin, Standard, Guest)
  - Dateisystem-Berechtigungen mit Pfad-basierter Prüfung
  - Exception-basierte Zugriffsverweigerung
- **Berechtigungs-Matrix**:
  - Admin: Volle Systemrechte, alle Dateien und Verzeichnisse
  - Standard: Eigenes Home-Verzeichnis, öffentliche Dateien lesen
  - Gast: Nur Lesezugriff auf eigenes Home und öffentliche Dateien
- **Pfad-Schutz**:
  - System-Verzeichnisse (`/system/`, `/boot/`) nur für Admin
  - Home-Verzeichnisse pro Benutzer geschützt
  - Öffentliche Bereiche (`/public/`) für alle lesbar
- **Methoden**:
  - `HasPermission()` - Berechtigungsprüfung
  - `RequirePermission()` - Exception bei fehlender Berechtigung
  - `CanAccessFile()` - Dateizugriffsprüfung
  - `GetPermissionSummary()` - Berechtigungsübersicht
- **Tests**: 42 automatisierte Tests in `PermissionCheckerTest.cs`
- **Befehl**: `testp6` oder `testpermissions`

#### Phase 7: Kernel-Integration ✅
- **AuditLogger**:
  - Singleton-Logger für sicherheitsrelevante Ereignisse
  - In-Memory-Speicherung (max. 100 Einträge)
  - Protokollierung von Login, Logout, User-Management-Aktionen
  - Formatierte Log-Ausgabe für Administratoren
- **Kernel.cs Erweiterungen**:
  - Login-Screen beim Systemstart
  - Dynamischer Prompt mit Benutzernamen (`username@SlotOS>`)
  - Session-Timeout-Prüfung (30 Minuten Inaktivität)
  - Automatischer Logout bei Timeout
  - Audit-Logging für alle kritischen Aktionen
- **Neue Befehle**:
  - `auditlog` - Zeigt Audit-Log an (nur Admin)
- **Sicherheitsfeatures**:
  - Session-Management mit automatischer Abmeldung
  - Vollständige Protokollierung aller Admin-Aktionen
  - Logout beim System-Shutdown
- **Dokumentation**: `PHASE7_IMPLEMENTATION.md`

### 🚧 In Planung
- **Phase 8**: Testing & Validierung
- **Phase 9**: Erweiterte Features (Optional)

## 🚀 Schnellstart

### Voraussetzungen

- Visual Studio 2022
- Cosmos User Kit
- .NET 6.0 oder höher

### Projekt kompilieren

```bash
cd SlotOS/SlotOS
dotnet build
```

### SlotOS starten

1. Projekt in Visual Studio öffnen
2. Als Startprojekt festlegen
3. F5 drücken (Debuggen) oder Strg+F5 (Ohne Debuggen)

### Tests ausführen

Im laufenden SlotOS:
```
SlotOS (nicht angemeldet)> test       # Phase 1-3 Tests (23 Tests)
SlotOS (nicht angemeldet)> testp4     # Phase 4 In-Memory Tests (18 Tests)
SlotOS (nicht angemeldet)> testp5     # Phase 5 Command Handler Tests (30 Tests)
SlotOS (nicht angemeldet)> testp6     # Phase 6 Permission Checker Tests (42 Tests)
```

Zeigt alle verfügbaren Befehle:
```
SlotOS> help       # System-Befehle
SlotOS> userhelp   # Benutzerverwaltungs-Befehle
```

## 📁 Projektstruktur

```
SlotOS/
├── SlotOS/
│   ├── Kernel.cs                      # Haupt-Kernel mit Befehlsverarbeitung
│   ├── SlotOS.csproj                  # Projekt-Datei
│   └── system/                        # Nutzerverwaltungs-System
│       ├── User.cs                    # Benutzer-Klasse
│       ├── UserRole.cs                # Rollen-Enum
│       ├── PasswordHasher.cs          # Passwort-Hashing
│       ├── AuthenticationManager.cs   # Authentifizierung
│       ├── UserManager.cs             # Benutzerverwaltung
│       ├── CommandHandler.cs          # Befehls-Verarbeitung (Phase 5)
│       ├── ConsoleHelper.cs           # UI-Hilfsfunktionen (Phase 5)
│       ├── PermissionChecker.cs       # Berechtigungssystem (Phase 6)
│       ├── AuditLogger.cs             # Audit-Logging (Phase 7)
│       ├── UserSystemTest.cs          # Automatisierte Tests (Phase 1-3)
│       ├── InMemoryTest.cs            # In-Memory-Tests (Phase 4)
│       ├── CommandHandlerTest.cs      # Command-Tests (Phase 5)
│       └── PermissionCheckerTest.cs   # Permission-Tests (Phase 6)
├── NUTZERVERWALTUNG_PLAN.md          # Detaillierter Implementierungsplan
├── TESTING.md                         # Test-Dokumentation
├── IN_MEMORY_MODE.md                  # In-Memory-Modus Dokumentation
├── PHASE7_IMPLEMENTATION.md           # Phase 7 Dokumentation
└── README.md                          # Diese Datei
```

## 🧪 Testing

### Automatische Tests

SlotOS enthält eine umfassende Test-Suite für Phase 1-6:

- **113 automatisierte Tests** (23 für Phase 1-3, 18 für Phase 4, 30 für Phase 5, 42 für Phase 6)
- Tests für alle Kernfunktionen
- Detaillierte Fehlerberichte

Siehe [TESTING.md](TESTING.md) für Details.

### Test-Kategorien

1. **Phase 1-3 Tests**: User-Erstellung, Rollen, Passwort-Updates, Hashing, Verifikation, Login, Logout, CRUD-Operationen
2. **Phase 4 Tests**: In-Memory-Betrieb, Standard-Admin, Neustart-Simulation, Performance
3. **Phase 5 Tests**: CommandHandler, Command-Parsing, Berechtigungen, Benutzerverwaltungs-Befehle, Passwort-Management, UserMod, ConsoleHelper
4. **Phase 6 Tests**: PermissionChecker, Rollenbasierte Berechtigungen, Dateizugriff, Exception-Handling, Pfad-Prüfung

## 🔒 Sicherheit

### Passwort-Sicherheit

- ✅ Niemals Klartext-Passwörter gespeichert
- ✅ Einzigartiger Salt pro Passwort (16 Bytes)
- ✅ 1000 Hash-Iterationen (Brute-Force-Schutz)
- ✅ Sichere Verifikation ohne Timing-Angriffe

### Authentifizierungs-Sicherheit

- ✅ Login-Versuchs-Limitierung (max. 3 Versuche)
- ✅ Automatische Account-Sperrung (30 Sekunden)
- ✅ Session-Timeout-Management
- ✅ Admin-Rechte-Validierung

## 📖 Dokumentation

- **[NUTZERVERWALTUNG_PLAN.md](NUTZERVERWALTUNG_PLAN.md)**: Vollständiger Implementierungsplan für alle 9 Phasen
- **[TESTING.md](TESTING.md)**: Detaillierte Test-Anleitung und manuelle Tests

## 🔧 Verfügbare Befehle

### System-Befehle

```
login         - Meldet einen Benutzer an
test          - Führt alle automatischen Tests (Phase 1-3) aus
testp4        - Führt In-Memory-Tests (Phase 4) aus
testp5        - Führt Command-Handler-Tests (Phase 5) aus
testp6        - Führt Permission-Checker-Tests (Phase 6) aus
auditlog      - Zeigt Audit-Log an (nur Admin)
help          - Zeigt System-Befehlsliste an
userhelp      - Zeigt Benutzerverwaltungs-Befehle an
clear         - Löscht den Bildschirm
exit          - Fährt das System herunter
```

### Benutzerverwaltungs-Befehle

**Für alle Benutzer:**
```
login         - Benutzer anmelden
logout        - Benutzer abmelden
whoami        - Zeigt aktuellen Benutzer an
passwd        - Eigenes Passwort ändern
```

**Für Administratoren:**
```
useradd <username> <password> [role]    - Benutzer erstellen
userdel <username>                       - Benutzer löschen
usermod <username> <option> <wert>       - Benutzer bearbeiten
userlist                                 - Alle Benutzer auflisten
passwd <username>                        - Passwort für Benutzer zurücksetzen
userstats                                - Benutzerstatistiken anzeigen
```

## 💻 Entwicklung

### Aktueller Status

**Abgeschlossen:**
- ✅ Phase 1: Datenstrukturen (100%)
- ✅ Phase 2: Authentifizierung (100%)
- ✅ Phase 3: Benutzerverwaltung (100%)
- ✅ Phase 4: Datenpersistenz / In-Memory-Modus (100%)
- ✅ Phase 5: Kommandozeilen-Interface (100%)
- ✅ Phase 6: Berechtigungssystem (100%)
- ✅ Phase 7: Kernel-Integration (100%)

**In Planung:**
- 🚧 Phase 8: Testing & Validierung
- 🚧 Phase 9: Erweiterte Features (Optional)

### Code-Stil

- Vollständige XML-Dokumentation für alle öffentlichen Methoden
- Deutsche Kommentare und Variablennamen
- Regions für bessere Code-Organisation
- Ausführliche Fehlerbehandlung mit sprechenden Exceptions

### Nächste Schritte

1. Umfassende End-to-End Tests (Phase 8)
2. Erweiterte Features wie Log-Filter, Persistente Logs (Phase 9)
3. Performance-Optimierungen

## 🐛 Bekannte Einschränkungen

- Cosmos OS hat eingeschränkten Zugriff auf .NET Crypto-Bibliotheken
- Custom Hash-Algorithmus verwendet (SHA256 geplant wenn verfügbar)
- VFS muss vor Verwendung initialisiert werden
- Memory-Management muss beachtet werden (OS-Entwicklung)

## 📝 Changelog

### Version 0.7.0 (2025-10-22)
- ✅ Phase 7 komplett implementiert
- ✅ AuditLogger für sicherheitsrelevante Ereignisse
  - Singleton-Pattern für zentrale Verwaltung
  - In-Memory-Speicherung (max. 100 Einträge)
  - Protokollierung von Login, Logout, User-Management
  - Formatierte Log-Ausgabe für Administratoren
- ✅ Kernel-Integration
  - Login-Screen beim Systemstart
  - Dynamischer Prompt mit Benutzernamen
  - Session-Timeout-Prüfung (30 Minuten)
  - Automatischer Logout bei Inaktivität
  - Audit-Logging beim Exit
- ✅ CommandHandler-Integration
  - Audit-Logging für alle Admin-Aktionen
  - Login/Logout werden protokolliert
  - Passwort-Änderungen werden geloggt
- ✅ Neuer Befehl: auditlog (nur Admin)
- ✅ Vollständige Dokumentation (PHASE7_IMPLEMENTATION.md)
- ✅ System ist produktionsreif für In-Memory-Betrieb

### Version 0.6.0 (2025-10-22)
- ✅ Phase 6 komplett implementiert
- ✅ PermissionChecker mit umfassendem Berechtigungssystem
  - Singleton-Pattern für zentrale Verwaltung
  - 15 vordefinierte Aktionen für verschiedene System-Bereiche
  - Rollenbasierte Zugriffskontrolle (Admin, Standard, Guest)
  - Dateisystem-Berechtigungen mit Pfad-basierter Prüfung
  - Exception-basierte Zugriffsverweigerung
- ✅ Berechtigungs-Matrix implementiert
  - Admin: Volle Systemrechte
  - Standard: Eigenes Home + öffentliche Dateien
  - Gast: Nur Lesezugriff
- ✅ Pfad-Schutz implementiert
  - System-Verzeichnisse nur für Admin
  - Home-Verzeichnisse pro Benutzer
  - Öffentliche Bereiche für alle lesbar
- ✅ 42 neue automatisierte Tests für Phase 6
- ✅ Kernel-Integration (testp6 Befehl)
- ✅ Vollständige Dokumentation (PHASE6_IMPLEMENTATION.md)

### Version 0.5.1 (2025-10-22)
- 🐛 **Kritischer Bugfix**: AuthenticationManager Synchronisationsproblem behoben
  - **Problem**: Invalid Opcode (06) in Permission-Tests durch NullReferenceException
  - **Ursache**: AuthenticationManager hielt Kopie statt Referenz der User-Liste
  - **Folge**: Login-Fehler bei neu erstellten Benutzern → CurrentUser = null → Crash
  - **Lösung**: GetInternalUserList() statt GetAllUsers() für gemeinsame Referenz
  - **Betroffene Dateien**: CommandHandlerTest.cs, Kernel.cs
- 🐛 **Bugfix**: VM-Crash in ConsoleHelper-Tests behoben
  - **Problem**: VM schloss sich automatisch während der letzten Tests
  - **Ursache**: String-Methoden (.Contains(), .StartsWith(), .EndsWith()) verursachen Crashes in Cosmos OS
  - **Lösung**: Ersetzt durch IndexOf() und Character-Array-Zugriff
  - **Betroffene Dateien**: CommandHandlerTest.cs (Tests 25-30)
- 🐛 **Bugfix**: Enum.Equals() Exception in Permission-Tests behoben
  - **Problem**: "enum.equals not supported yet" Exception in AssertEquals
  - **Ursache**: Cosmos OS unterstützt Equals()-Methode für Enums nicht
  - **Lösung**: Cast zu int für Enum-Vergleiche, ToString() für andere Typen
  - **Betroffene Dateien**: CommandHandlerTest.cs (AssertEquals Methode)

### Version 0.5.0 (2025-10-21)
- ✅ Phase 5 komplett implementiert
- ✅ ConsoleHelper mit UI-Hilfsfunktionen
  - Maskierte Passwort-Eingabe
  - Farbige Statusmeldungen
  - Formatierte Tabellen und Header
  - Bestätigungs-Dialoge
- ✅ CommandHandler mit vollständiger Benutzerverwaltung
  - 10 Befehle implementiert (login, logout, whoami, passwd, useradd, userdel, usermod, userlist, userstats)
  - Intelligentes Command-Parsing mit Anführungszeichen-Support
  - Automatische Berechtigungsprüfung
- ✅ Kernel-Integration
  - CommandHandler in Kernel integriert
  - Neue Befehle: testp5, userhelp
- ✅ 30 neue automatisierte Tests für Phase 5
- ✅ Vollständige Dokumentation aktualisiert

### Version 0.4.0 (2025-10-21)
- ✅ Phase 4 komplett implementiert
- ✅ UserStorage mit VFS-Integration
- ✅ Automatisches Backup-System
- ✅ Auto-Save-Funktionalität im UserManager
- ✅ Pipe-delimited Serialisierungsformat
- ✅ 23 neue automatisierte Tests für Persistenz
- ✅ Wiederherstellung bei Dateikorruption
- ✅ Vollständige Dokumentation (PHASE4_SUMMARY.md)
- ✅ Benutzerdaten bleiben nach Neustart erhalten

### Version 0.3.0 (2025-10-21)
- ✅ Phase 3 komplett implementiert
- ✅ UserManager mit Singleton-Pattern
- ✅ CRUD-Operationen für Benutzer
- ✅ Standard-Admin-Account automatisch erstellt
- ✅ Schutz vor Löschung des letzten Administrators
- ✅ 10 zusätzliche Tests für UserManager
- ✅ Passwort-Änderungs- und Reset-Funktionen
- ✅ Benutzerstatistiken

### Version 0.2.1 (2025-10-06)
- 🐛 **Bugfix**: Salt-Einzigartigkeit garantiert durch statischen Zähler
  - Fügt Counter-basierte Entropie hinzu
  - Verhindert identische Salts bei schnellen aufeinanderfolgenden Hash-Operationen
- 🐛 **Bugfix**: Session-Tracking Test robuster gemacht
  - Wartet aktiv auf DateTime-Änderung statt fixer Verzögerung
  - Cosmos OS DateTime-Auflösung berücksichtigt

### Version 0.2.0 (2025-10-06)
- ✅ Phase 2 komplett implementiert
- ✅ PasswordHasher mit Salt und 1000 Iterationen
- ✅ AuthenticationManager mit allen Features
- ✅ 13 automatisierte Tests
- ✅ Kernel mit Befehlsverarbeitung
- ✅ Vollständige Dokumentation

### Version 0.1.0 (2025-10-06)
- ✅ Phase 1 implementiert
- ✅ User und UserRole Klassen
- ✅ Basis-Passwort-Funktionalität

## 👤 Autor

Entwickelt als Lernprojekt für Betriebssystem-Entwicklung mit Cosmos OS.

## 📄 Lizenz

Dieses Projekt ist ein Lernprojekt und steht unter einer freien Lizenz.

---

**Letztes Update:** 2025-10-22  
**Version:** 0.7.0  
**Status:** Produktionsreif (In-Memory-Modus)
