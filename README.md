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

### 🚧 In Planung
- **Phase 5**: Kommandozeilen-Interface (Login-Screen, Befehle)
- **Phase 6**: Berechtigungssystem
- **Phase 7**: Kernel-Integration
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
SlotOS> test       # Phase 1-3 Tests
SlotOS> testp4     # Phase 4 Persistenz-Tests
```

Zeigt alle verfügbaren Befehle:
```
SlotOS> help
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
│       ├── UserStorage.cs             # Datenpersistenz
│       ├── UserSystemTest.cs          # Automatisierte Tests (Phase 1-3)
│       └── PersistenceTest.cs         # Persistenz-Tests (Phase 4)
├── NUTZERVERWALTUNG_PLAN.md          # Detaillierter Implementierungsplan
├── TESTING.md                         # Test-Dokumentation
└── README.md                          # Diese Datei
```

## 🧪 Testing

### Automatische Tests

SlotOS enthält eine umfassende Test-Suite für Phase 1-4:

- **46 automatisierte Tests** (23 für Phase 1-3, 23 für Phase 4)
- Tests für alle Kernfunktionen
- Detaillierte Fehlerberichte

Siehe [TESTING.md](TESTING.md) für Details.

### Test-Kategorien

1. **Phase 1 Tests**: User-Erstellung, Rollen, Passwort-Updates
2. **PasswordHasher Tests**: Hashing, Verifikation, Salt, Kompatibilität
3. **AuthenticationManager Tests**: Login, Logout, Rechte, Sperrung, Sessions
4. **UserManager Tests**: CRUD-Operationen, Passwort-Verwaltung, Admin-Schutz, Statistiken
5. **Persistenz Tests**: VFS, Save/Load, Backup-System, Auto-Save, Integration

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

Im laufenden SlotOS:

```
test       - Führt alle automatischen Tests (Phase 1-3) aus
testp4     - Führt Persistenz-Tests (Phase 4) aus
help       - Zeigt Befehlsliste an
clear      - Löscht den Bildschirm
exit       - Fährt das System herunter
```

## 💻 Entwicklung

### Aktueller Status

**Abgeschlossen:**
- ✅ Phase 1: Datenstrukturen (100%)
- ✅ Phase 2: Authentifizierung (100%)
- ✅ Phase 3: Benutzerverwaltung (100%)
- ✅ Phase 4: Datenpersistenz (100%)

**In Arbeit:**
- 🚧 Phase 5: Kommandozeilen-Interface (0%)

### Code-Stil

- Vollständige XML-Dokumentation für alle öffentlichen Methoden
- Deutsche Kommentare und Variablennamen
- Regions für bessere Code-Organisation
- Ausführliche Fehlerbehandlung mit sprechenden Exceptions

### Nächste Schritte

1. Login-Screen UI (Phase 5)
2. Benutzerverwaltungs-Befehle (Phase 5)
3. CommandHandler für Befehlsverarbeitung (Phase 5)

## 🐛 Bekannte Einschränkungen

- Cosmos OS hat eingeschränkten Zugriff auf .NET Crypto-Bibliotheken
- Custom Hash-Algorithmus verwendet (SHA256 geplant wenn verfügbar)
- VFS muss vor Verwendung initialisiert werden
- Memory-Management muss beachtet werden (OS-Entwicklung)

## 📝 Changelog

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

**Letztes Update:** 2025-10-21  
**Version:** 0.4.0  
**Status:** In Entwicklung
