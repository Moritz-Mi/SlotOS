# SlotOS Architecture Documentation

Dieses Verzeichnis enthält die Architektur-Dokumentation von SlotOS in Form von LikeC4-Diagrammen.

## 📋 Verfügbare Diagramme

### 1. `architecture.c4` - System Architecture
Zeigt die komplette Systemarchitektur von SlotOS mit allen Komponenten und deren Beziehungen.

**Enthaltene Views:**
- **Systemkontext**: Überblick über SlotOS und seine Benutzer (Admin, User, Guest)
- **Interne Architektur**: Die 4 Hauptcontainer (Kernel, User Management, Command Interface, Audit System)
- **User Management Subsystem**: Detaillierte Komponenten der Benutzerverwaltung
- **Command Interface Subsystem**: CLI-Komponenten für Benutzerinteraktion
- **Audit & Security Subsystem**: Sicherheits- und Logging-Komponenten
- **Komplette Architektur**: Alle Komponenten und Beziehungen

**Hauptkomponenten:**
- Kernel (Cosmos OS Entry Point)
- UserManager (CRUD Operations)
- AuthenticationManager (Login/Logout/Sessions)
- PasswordHasher (Secure Password Storage)
- PermissionChecker (Role-based Access Control)
- CommandHandler (CLI Command Processing)
- ConsoleHelper (UI Utilities)
- AuditLogger (Security Event Logging)

### 2. `flows.c4` - System Flows
Zeigt die wichtigsten Prozess- und Datenflüsse im System.

**Enthaltene Views:**
- **System Flows Übersicht**: Hauptprozesse und Datenflüsse
- **Login Flow**: Detaillierter Login-Prozess mit Validierung und Session-Erstellung
- **User Management Flow**: CRUD-Operationen mit Berechtigungsprüfung
- **Session Management Flow**: Session-Timeout und Auto-Logout
- **Password Management Flow**: Passwort-Änderung mit Hashing
- **Audit Logging Flow**: Zentrale Event-Protokollierung
- **Kompletter Datenfluss**: Alle Prozesse und Speicher

**Hauptprozesse:**
- Login mit 3-Attempts-Limit und Account-Sperre
- User CRUD mit Admin-Berechtigungsprüfung
- Session-Timeout nach 30 Minuten
- Password-Hashing mit Salt (1000 Iterationen)
- Audit-Logging aller sicherheitsrelevanten Events

### 3. `phases.c4` - Development Phases
Zeigt die Entwicklungsstruktur und Phasen des Projekts.

**Enthaltene Views:**
- **Entwicklungsphasen Übersicht**: Struktur des gesamten Entwicklungsprozesses
- **Phase 1-7 Details**: Einzelne Phasen mit Modulen und Tests
- **Testing Infrastructure**: Umfassendes Test-Framework (113 Tests)
- **Documentation System**: Vollständige Projekt-Dokumentation
- **Komplette Entwicklungsstruktur**: Alle Phasen, Tests und Docs

**Phasen:**
1. **Phase 1**: Basic Data Structures (User, UserRole)
2. **Phase 2**: Authentication System (PasswordHasher, AuthenticationManager)
3. **Phase 3**: User Management (UserManager mit CRUD)
4. **Phase 4**: Data Persistence (In-Memory-Modus, VFS deaktiviert)
5. **Phase 5**: Command Interface (CommandHandler, ConsoleHelper)
6. **Phase 6**: Permission System (PermissionChecker)
7. **Phase 7**: Kernel Integration (Kernel.cs, AuditLogger)

## 🚀 Verwendung

### Online Playground
Die einfachste Methode ist die Verwendung des LikeC4 Playgrounds:

1. Öffne [LikeC4 Playground](https://playground.likec4.dev/w/blank/)
2. Kopiere den Inhalt einer `.c4`-Datei
3. Füge ihn in den Editor ein
4. Die Diagramme werden automatisch generiert

### VS Code Extension
Für lokale Entwicklung:

1. Installiere die [LikeC4 VS Code Extension](https://marketplace.visualstudio.com/items?itemName=likec4.likec4-vscode)
2. Öffne eine `.c4`-Datei
3. Klicke auf "Preview" oder drücke `Ctrl+Shift+P` → "LikeC4: Open Preview"

### CLI (Optional)
```bash
npm install -g @likec4/cli
likec4 view architecture.c4
```

## 📚 Weitere Dokumentation

- **[LikeC4 Documentation](https://likec4.dev/)** - Offizielle Dokumentation
- **[LikeC4 Tutorial](https://likec4.dev/tutorial/)** - Schnellstart-Tutorial
- **[LikeC4 GitHub](https://github.com/likec4/likec4)** - Source Code und Beispiele

## 🎨 Diagramm-Struktur

### Specification
Definiert die Element-Typen:
- `person` - Benutzer/Akteure
- `system` - Systeme
- `container` - Große Subsysteme
- `component` - Einzelne Komponenten
- `process` - Prozesse und Flows
- `datastore` - Datenspeicher

### Model
Definiert die eigentliche Architektur mit:
- Elementen und deren Hierarchie
- Beschreibungen und Technologien
- Beziehungen zwischen Elementen
- Styling (Icons, Farben, Shapes)

### Views
Definiert die verschiedenen Ansichten:
- `view index` - Standardansicht
- `view of <element>` - Zoom in ein Element
- `include *` - Alle direkten Kinder
- `include ->` - Alle Beziehungen
- `style` - View-spezifisches Styling

## 🔍 Wichtige Erkenntnisse aus der Architektur

### In-Memory-Modus
- SlotOS läuft im 100% In-Memory-Modus
- Keine Persistenz über Neustarts hinweg
- Grund: Cosmos OS VFS-Limitierungen (StringBuilder, DateTime.ToString())

### Sicherheitsarchitektur
- **3-Layer-Sicherheit**: Authentication → Authorization → Audit
- **Password-Hashing**: Salt-basiert mit 1000 Iterationen
- **Session-Management**: 30-Minuten-Timeout
- **Audit-Logging**: Max. 100 Einträge in-memory

### Testabdeckung
- **113 automatisierte Tests** über alle Phasen
- **Test-Commands**: `test`, `testp4`, `testp5`, `testp6`
- **100% Code Coverage** für kritische Komponenten

### Cosmos OS Kompatibilität
- ⛔ **NIEMALS**: StringBuilder, DateTime.ToString() mit Format, Ternäre Operatoren in Strings
- ✅ **IMMER**: Manuelle String-Konkatenation, direkte Console-Ausgaben

## 📝 Diagramm-Updates

Wenn du die Diagramme aktualisieren möchtest:

1. Editiere die entsprechende `.c4`-Datei
2. Folge der LikeC4-Syntax
3. Teste im Playground oder VS Code
4. Dokumentiere Änderungen in dieser README

## 🏗️ Architektur-Prinzipien

### Single Responsibility
Jede Komponente hat eine klar definierte Verantwortung:
- **UserManager**: Nur User CRUD
- **AuthenticationManager**: Nur Login/Session
- **PasswordHasher**: Nur Password-Hashing
- **PermissionChecker**: Nur Permission-Checks

### Singleton Pattern
Zentrale Komponenten als Singletons:
- `UserManager.Instance`
- `AuditLogger.Instance`

### Separation of Concerns
- **UI Layer**: ConsoleHelper
- **Business Logic**: UserManager, AuthenticationManager
- **Data Layer**: In-Memory Lists
- **Security**: PermissionChecker, AuditLogger

### Dependency Injection
- CommandHandler bekommt UserManager und AuthManager injiziert
- Testbar durch Dependency Injection

---

**Erstellt am:** 22. Oktober 2025  
**Version:** 1.0  
**Status:** ✅ Vollständig und aktuell
