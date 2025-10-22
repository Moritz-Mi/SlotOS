# SlotOS - Architecture Overview

**Version:** 0.7.0  
**Status:** ✅ Produktionsreif (In-Memory-Modus)  
**Datum:** 22. Oktober 2025

## 📊 Quick Reference

| Kategorie | Anzahl | Details |
|-----------|--------|---------|
| **Phasen** | 7 | Phase 1-7 vollständig implementiert |
| **Komponenten** | 11 | Core-Module + Tests |
| **Tests** | 113 | Automatisierte Unit/Integration Tests |
| **Befehle** | 15+ | User Management CLI Commands |
| **Code-Zeilen** | ~3000+ | Produktionscode (ohne Tests) |
| **Dokumentation** | 10+ | Markdown + LikeC4 Diagramme |

## 🏗️ System Architecture (Hierarchisch)

```
SlotOS (Cosmos OS Kernel)
│
├── 🖥️  Kernel.cs
│   ├── Systemstart & Initialisierung
│   ├── Command Loop (mit Session-Timeout)
│   └── Dynamischer Prompt (user@SlotOS>)
│
├── 👥 User Management Subsystem
│   ├── UserManager.cs (Singleton)
│   │   ├── CreateUser(username, password, role)
│   │   ├── DeleteUser(username)
│   │   ├── UpdateUser(username, property, value)
│   │   ├── ChangePassword(username, oldPwd, newPwd)
│   │   ├── GetUser(username)
│   │   └── GetStatistics()
│   │
│   ├── AuthenticationManager.cs
│   │   ├── Login(username, password)
│   │   ├── Logout()
│   │   ├── CheckSessionTimeout(minutes)
│   │   └── IsAuthenticated (Property)
│   │
│   ├── PasswordHasher.cs (Static)
│   │   ├── HashPassword(password) → (hash, salt)
│   │   └── VerifyPassword(password, hash, salt) → bool
│   │
│   ├── PermissionChecker.cs (Static)
│   │   ├── IsAdmin(user) → bool
│   │   ├── CanModifyUser(actor, target) → bool
│   │   └── RequireAdmin(user) → throws
│   │
│   ├── User.cs (Model)
│   │   ├── Username, PasswordHash, PasswordSalt
│   │   ├── Role (UserRole enum)
│   │   ├── IsActive, HomeDirectory
│   │   └── CreatedAt, LastLogin
│   │
│   └── UserRole.cs (Enum)
│       ├── Admin = 0
│       ├── Standard = 1
│       └── Guest = 2
│
├── 🎮 Command Interface Subsystem
│   ├── CommandHandler.cs
│   │   ├── ProcessCommand(input) → bool
│   │   ├── HandleLogin()
│   │   ├── HandleLogout()
│   │   ├── HandleUserAdd/Del/Mod/List()
│   │   ├── HandlePasswd/AdminPasswd()
│   │   └── HandleWhoAmI()
│   │
│   └── ConsoleHelper.cs (Static)
│       ├── ReadPassword(prompt) → string
│       ├── WriteSuccess/Error/Warning/Info(msg)
│       ├── WriteHeader(title)
│       ├── WriteTableRow/Header(columns)
│       ├── Confirm(message) → bool
│       └── FormatRole/Status(value) → string
│
└── 🔒 Audit & Security Subsystem
    ├── AuditLogger.cs (Singleton)
    │   ├── Log(username, action, details, success)
    │   ├── LogLogin/Logout/SessionTimeout(...)
    │   ├── LogUserAction/PasswordChange(...)
    │   ├── GetEntries/GetRecentEntries()
    │   ├── FormatLogs(count) → string
    │   └── Clear()
    │
    └── AuditEntry.cs (Model)
        ├── Timestamp (DateTime)
        ├── Username (string)
        ├── Action (string)
        ├── Details (string)
        └── IsSuccess (bool)
```

## 🔄 Key Flows

### 1. Login Flow
```
User
  ↓ "login" command
CommandHandler.HandleLogin()
  ↓ displays login screen
ConsoleHelper.ReadPassword()
  ↓ validates
AuthenticationManager.Login()
  ↓ checks user
UserManager.GetUser()
  ↓ verifies password
PasswordHasher.VerifyPassword()
  ↓ creates session
AuthenticationManager.CreateSession()
  ↓ logs event
AuditLogger.LogLogin()
  ↓ updates
User.LastLogin
```

### 2. User Creation Flow (Admin)
```
Admin
  ↓ "useradd username password role"
CommandHandler.HandleUserAdd()
  ↓ checks permission
PermissionChecker.RequireAdmin()
  ↓ creates user
UserManager.CreateUser()
  ↓ hashes password
PasswordHasher.HashPassword()
  ↓ stores user
UserManager._users.Add()
  ↓ logs action
AuditLogger.LogUserAction()
```

### 3. Session Timeout Flow
```
Kernel.Run() Loop
  ↓ every command
AuthenticationManager.CheckSessionTimeout(30)
  ↓ if > 30 minutes
AuthenticationManager.HandleSessionTimeout()
  ↓ logs timeout
AuditLogger.LogSessionTimeout()
  ↓ logs out
AuthenticationManager.Logout()
  ↓ displays warning
ConsoleHelper.WriteWarning()
```

## 💾 Data Storage (In-Memory)

### UserManager Storage
```csharp
private static List<User> _users = new List<User>();
```
- **Default User**: admin/admin (Administrator)
- **Max Users**: Unlimited (RAM-limited)
- **Persistence**: None (resets on restart)

### AuditLogger Storage
```csharp
private List<AuditEntry> _entries = new List<AuditEntry>();
private const int MAX_ENTRIES = 100;
```
- **Max Entries**: 100 (älteste werden entfernt)
- **Persistence**: None (resets on restart)

## 🔐 Security Architecture

### Three-Layer Security Model

1. **Authentication Layer**
   - Login with username/password
   - Salt-based password hashing (1000 iterations)
   - 3-attempt limit with 30-second lockout
   - Session management with 30-minute timeout

2. **Authorization Layer**
   - Role-based access control (RBAC)
   - Admin/Standard/Guest roles
   - Permission checks before sensitive operations
   - Self-service for own account

3. **Audit Layer**
   - All security-relevant events logged
   - Timestamped with username and action
   - Admin-only access to logs
   - Max 100 entries (FIFO)

### Security Features

| Feature | Implementation | Status |
|---------|---------------|--------|
| Password Hashing | Salt + 1000 iterations | ✅ |
| Login Attempts | Max 3, 30s lockout | ✅ |
| Session Timeout | 30 minutes inactivity | ✅ |
| Role-based Access | Admin/Standard/Guest | ✅ |
| Audit Logging | All actions logged | ✅ |
| Last Admin Protection | Can't delete/deactivate | ✅ |
| Password Validation | Min 4 characters | ✅ |

## 🧪 Testing Infrastructure

### Test Coverage

| Test Suite | Tests | Coverage | Status |
|------------|-------|----------|--------|
| UserSystemTest | 23 | Phase 1-3 | ✅ 100% |
| InMemoryTest | 18 | Phase 4 | ✅ 100% |
| CommandHandlerTest | 30 | Phase 5 | ✅ 100% |
| PermissionCheckerTest | 42 | Phase 6 | ✅ 100% |
| **Total** | **113** | **All** | ✅ |

### Test Commands

```bash
SlotOS> test      # Phase 1-3 Tests
SlotOS> testp4    # Phase 4 Tests (In-Memory)
SlotOS> testp5    # Phase 5 Tests (CommandHandler)
SlotOS> testp6    # Phase 6 Tests (PermissionChecker)
```

## 📦 Module Dependencies

```
Kernel.cs
  ├── UserManager
  ├── AuthenticationManager
  ├── CommandHandler
  └── AuditLogger

CommandHandler
  ├── UserManager
  ├── AuthenticationManager
  ├── PermissionChecker
  ├── ConsoleHelper
  └── AuditLogger

UserManager
  ├── User
  ├── UserRole
  └── PasswordHasher

AuthenticationManager
  ├── User
  └── PasswordHasher

User
  ├── UserRole
  └── PasswordHasher

PermissionChecker
  ├── User
  └── UserRole
```

## 🎯 Design Patterns

### Singleton Pattern
- **UserManager**: Zentrale Benutzerverwaltung
- **AuditLogger**: Zentrale Event-Protokollierung

### Strategy Pattern
- **PasswordHasher**: Abstrahiert Hashing-Algorithmus

### Factory Pattern
- **UserManager.CreateUser()**: Erstellt User-Objekte

### Observer Pattern (Implizit)
- **AuditLogger**: Alle Komponenten loggen Events

## 🚫 Cosmos OS Kompatibilität

### Was NICHT funktioniert:
```csharp
❌ StringBuilder sb = new StringBuilder();
❌ DateTime.ToString("yyyy-MM-dd HH:mm:ss");
❌ string text = condition ? "yes" : "no";  // in Strings
❌ string text = $"Value: {(x > 0 ? "positive" : "negative")}";
❌ File.Copy(), File.WriteAllText() (VFS instabil)
```

### Was funktioniert:
```csharp
✅ string text = "Hello " + username;
✅ Console.WriteLine("Output");
✅ List<T> collection = new List<T>();
✅ string day = date.Day.ToString();
✅ if (condition) text = "yes"; else text = "no";
```

## 📈 Performance Characteristics

| Operation | Time Complexity | Space Complexity |
|-----------|----------------|------------------|
| Login | O(n) | O(1) |
| GetUser | O(n) | O(1) |
| CreateUser | O(1) | O(1) |
| DeleteUser | O(n) | O(1) |
| ListUsers | O(n) | O(n) |
| LogEvent | O(1) | O(1) |
| GetAuditLogs | O(n) | O(n) |

*n = Anzahl der Benutzer/Audit-Einträge*

## 🔮 Future Enhancements (Optional)

### Phase 8: Testing & Validation
- [ ] End-to-End Tests
- [ ] Performance Tests
- [ ] Stress Tests

### Phase 9: Extended Features
- [ ] Log-Filter (`auditlog --user admin`)
- [ ] User Groups
- [ ] Extended Statistics Dashboard
- [ ] Session Timeout Warning (5 min before)

### VFS Integration (wenn stabil)
- [ ] Persistent User Storage
- [ ] Persistent Audit Logs
- [ ] Log Rotation & Archivierung

## 📚 Documentation Map

```
SlotOS/
├── README.md                    # Hauptdokumentation
├── IN_MEMORY_MODE.md           # In-Memory-Entscheidung
├── TESTING.md                  # Test-Ergebnisse
├── PHASE5_SUMMARY.md           # Phase 5 Zusammenfassung
├── PHASE6_SUMMARY.md           # Phase 6 Zusammenfassung
├── PHASE7_IMPLEMENTATION.md    # Phase 7 Details
├── PHASE7_SUMMARY.md           # Phase 7 Zusammenfassung
├── PHASE7_BUGFIX.md            # GPF 0x0D Bugfixes
└── docs/
    ├── README.md               # Diagramm-Dokumentation
    ├── ARCHITECTURE_OVERVIEW.md # Diese Datei
    ├── architecture.c4         # System-Architektur
    ├── flows.c4                # Prozess-Flüsse
    └── phases.c4               # Entwicklungsphasen
```

## 🎓 Lessons Learned

### ✅ Was gut funktioniert hat:
- **Singleton-Pattern** für zentrale Komponenten
- **In-Memory-Modus** ist stabil und performant
- **Phasenweise Entwicklung** ermöglicht inkrementellen Fortschritt
- **Umfassende Tests** fangen Bugs früh ab
- **Minimale String-Operationen** verhindern Crashes

### ⚠️ Herausforderungen:
- **Cosmos OS VFS** ist instabil (StringBuilder, DateTime.ToString())
- **Komplexe String-Operationen** führen zu GPF/Stack Corruption
- **Keine Persistenz** möglich ohne stabiles VFS
- **Manuelle String-Formatierung** erforderlich

---

**Erstellt am:** 22. Oktober 2025  
**Autor:** Cascade AI  
**Version:** 1.0  
**Status:** ✅ Vollständig und aktuell
