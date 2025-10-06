# SlotOS - Architektur Dokumentation

## System-Übersicht

```
┌─────────────────────────────────────────────────────────────┐
│                         Kernel.cs                           │
│                    (Main Entry Point)                       │
│                                                             │
│  - BeforeRun(): System-Initialisierung + Login             │
│  - Run(): Command Loop                                      │
│  - InitializeUserManagement()                               │
│  - PerformLogin()                                           │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ├──────────────────────────────────────────┐
                   │                                          │
         ┌─────────▼──────────┐                  ┌───────────▼──────────┐
         │  UserDatabase      │                  │  CommandHandler      │
         │                    │                  │                      │
         │ - Users            │                  │ - ProcessCommand()   │
         │ - Groups           │                  │ - Help, WhoAmI, etc  │
         │ - CRUD Ops         │                  └──────────────────────┘
         └─────────┬──────────┘
                   │
      ┌────────────┼────────────┬─────────────┐
      │            │            │             │
┌─────▼────────┐ ┌─▼────────────▼──┐ ┌────────▼──────────┐
│ Authentication│ │ PermissionManager│ │FilePermissionMgr │
│   Manager     │ │                  │ │                  │
│               │ │ - CheckPerm()    │ │ - CanAccessFile()│
│ - Login()     │ │ - GrantPerm()    │ │ - SetFilePerm()  │
│ - Logout()    │ │ - RevokePerm()   │ │ - ChangeOwner()  │
│ - Current User│ └──────────────────┘ └──────────────────┘
└───────────────┘
```

## Datenmodell

```
┌─────────────────┐
│      User       │
├─────────────────┤
│ + UserId        │
│ + Username      │
│ + PasswordHash  │
│ + Role          │
│ + Permissions   │◄────┐
│ + GroupIds[]    │     │
│ + IsActive      │     │
│ + CreatedDate   │     │
│ + LastLoginDate │     │
└────────┬────────┘     │
         │              │
         │              │
         │         ┌────┴──────────┐
         │         │   Permission  │ (Enum Flags)
         │         ├───────────────┤
         │         │ None = 0      │
         │         │ Read = 1      │
         │         │ Write = 2     │
         │         │ Execute = 4   │
         │         │ Delete = 8    │
         │         │ CreateUser=16 │
         │         │ ...           │
         │         └───────────────┘
         │
         │ 0..*
         │
    ┌────▼─────────┐
    │  UserGroup   │
    ├──────────────┤
    │ + GroupId    │
    │ + GroupName  │
    │ + Permissions│
    │ + MemberIds[]│
    └──────────────┘
```

## Authentifizierungs-Flow

```
┌─────────┐
│  Start  │
└────┬────┘
     │
     ▼
┌─────────────────────┐
│ Initialize System   │
│ - Create UserDB     │
│ - Create Managers   │
│ - Create Admin User │
└─────────┬───────────┘
          │
          ▼
     ┌─────────┐
     │  Login  │◄──────────────┐
     └────┬────┘               │
          │                    │
          ▼                    │
    ┌──────────┐               │
    │ Username │               │
    └────┬─────┘               │
         │                     │
         ▼                     │
    ┌──────────┐               │
    │ Password │               │
    └────┬─────┘               │
         │                     │
         ▼                     │
    ┌────────────┐             │
    │ Hash Pass  │             │
    └────┬───────┘             │
         │                     │
         ▼                     │
    ┌──────────────┐           │
    │ Check in DB  │           │
    └────┬────┬────┘           │
         │    │                │
    OK   │    │ FAIL           │
         │    └────────────────┤
         │                     │
         ▼                  Retry < 3?
  ┌────────────┐               │
  │ Set Session│               │
  └────┬───────┘               │
       │                       │
       ▼                       │
  ┌────────────┐               │
  │Command Loop│               │
  └────┬───┬───┘               │
       │   │                   │
       │   └─ logout ──────────┘
       │
       ▼
   ┌────────┐
   │ Logout │
   └────────┘
```

## Berechtigungs-Prüfung

```
                    ┌────────────────┐
                    │  User Action   │
                    └───────┬────────┘
                            │
                            ▼
                  ┌──────────────────┐
                  │ Permission Check │
                  └────────┬─────────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
   ┌──────────┐    ┌──────────────┐  ┌──────────┐
   │Is Admin? │    │Has User Perm?│  │In Group? │
   └─────┬────┘    └──────┬───────┘  └─────┬────┘
         │                │                 │
      YES│             YES│              YES│
         └────────────────┴─────────────────┘
                          │
                          ▼
                    ┌──────────┐
                    │  ALLOW   │
                    └──────────┘
                          
    NO from all
         │
         ▼
    ┌──────────┐
    │   DENY   │
    └──────────┘
```

## Datei-Berechtigungen

```
File: test.txt
Owner: alice (ID: 2)
Group: users (ID: 2)

┌──────────────────────────────────────┐
│  Permission String: rwxr-xr--        │
└──────────────────────────────────────┘
        │      │     │
        │      │     └── Others: r-- (Read only)
        │      │
        │      └──────── Group: r-x (Read + Execute)
        │
        └─────────────── Owner: rwx (Full access)

User versucht Zugriff:
1. Ist User = Owner? → Owner-Rechte verwenden
2. Ist User in Group? → Group-Rechte verwenden
3. Sonst → Others-Rechte verwenden

Beispiel:
- alice (Owner) will schreiben → rwx → ERLAUBT
- bob (in 'users' group) will schreiben → r-x → VERWEIGERT
- charlie (nicht in group) will lesen → r-- → ERLAUBT
```

## Komponenten-Interaktion

```
┌──────────────────────────────────────────────────────────┐
│                    Kernel.cs (Run)                       │
└──────────────────┬───────────────────────────────────────┘
                   │ User Input: "adduser bob pass123 User"
                   │
                   ▼
┌──────────────────────────────────────────────────────────┐
│              CommandHandler.ProcessCommand()             │
│              Parse: cmd="adduser", args=["bob",...]      │
└──────────────────┬───────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────────────────────────┐
│       AuthenticationManager.CreateNewUser()              │
│       1. Check Permission (CreateUser)                   │
└──────────────────┬───────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────────────────────────┐
│           PermissionManager.CheckPermission()            │
│           → AuthManager.CurrentUser.HasPermission()      │
└──────────────────┬───────────────────────────────────────┘
                   │ Permission OK
                   ▼
┌──────────────────────────────────────────────────────────┐
│         UserDatabase.AddUser(new User(...))              │
│         1. Check if username exists                      │
│         2. Add to dictionary                             │
│         3. Return success                                │
└──────────────────┬───────────────────────────────────────┘
                   │
                   ▼
                Success → "Benutzer erstellt"
```

## Rollenbasierte Zugriffskontrolle (RBAC)

```
┌───────────────────────────────────────────────────────┐
│                    User Roles                         │
└───────────────────────────────────────────────────────┘

Guest (Role)
   └─→ Permission.Read

User (Role)
   └─→ Permission.Read | Permission.Write | Permission.Execute

PowerUser (Role)
   └─→ Permission.Read | Write | Execute | Delete

Administrator (Role)
   └─→ Permission.Administrator (all flags)


┌───────────────────────────────────────────────────────┐
│              Permission Inheritance                   │
└───────────────────────────────────────────────────────┘

User "alice"
├─ Direct Permissions: Read | Write
├─ Group "developers"
│  └─ Group Permissions: Execute | Delete
│
└─ Effective Permissions: Read | Write | Execute | Delete
```

## Session-Management (Zukunft)

```
┌────────────┐
│   Login    │
└─────┬──────┘
      │
      ▼
┌───────────────┐
│Create Session │
│- SessionId    │
│- UserId       │
│- StartTime    │
│- LastActivity │
│- Timeout=15min│
└───────┬───────┘
        │
        ▼
   ┌─────────┐
   │Activity?│
   └────┬────┘
        │
    ┌───┴────┐
    │        │
   YES      NO
    │        │
    │        ▼
    │   ┌────────────┐
    │   │ Timeout?   │
    │   └──┬─────┬───┘
    │      │     │
    │     NO    YES
    │      │     │
    │      │     ▼
    │      │  ┌────────────┐
    │      │  │Force Logout│
    │      │  └────────────┘
    │      │
    └──────┘
       │
       ▼
  Update Activity
```

## Audit-Log (Zukunft)

```
┌──────────────────────────────────────┐
│          Every Action                │
└──────────────┬───────────────────────┘
               │
               ▼
       ┌───────────────┐
       │  Log Action   │
       │ - Timestamp   │
       │ - UserId      │
       │ - Action      │
       │ - Details     │
       │ - Success     │
       └───────┬───────┘
               │
               ▼
     ┌─────────────────┐
     │Store in LogFile │
     └─────────────────┘

Beispiel Logs:
2025-10-06 09:15:23 | User: admin (1) | LOGIN_SUCCESS | 
2025-10-06 09:16:45 | User: admin (1) | CREATE_USER | Username: bob
2025-10-06 09:17:12 | User: bob (2) | LOGIN_SUCCESS | 
2025-10-06 09:18:33 | User: bob (2) | DELETE_FILE | DENIED (no permission)
```

## Erweiterbarkeit

### Plugin-System (Konzept)

```
┌─────────────────────────────────────────┐
│         Plugin Interface                │
├─────────────────────────────────────────┤
│ + Initialize()                          │
│ + ProcessCommand(cmd, args)             │
│ + GetCommands(): string[]               │
└─────────────────────────────────────────┘
                    ▲
                    │
        ┌───────────┴───────────┐
        │                       │
┌───────┴────────┐    ┌─────────┴────────┐
│ FileSystemPlugin│    │ NetworkPlugin    │
├─────────────────┤    ├──────────────────┤
│ Commands:       │    │ Commands:        │
│ - ls            │    │ - ping           │
│ - mkdir         │    │ - wget           │
│ - rm            │    │ - ssh            │
└─────────────────┘    └──────────────────┘
```

## Deployment

```
┌──────────────────┐
│  Source Code     │
│  (.cs files)     │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│   Compilation    │
│  (Visual Studio) │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Cosmos Compiler │
│  (IL to native)  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Bootable Image  │
│  (.iso / .bin)   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│   Deployment     │
│ - VMware         │
│ - QEMU           │
│ - Physical HW    │
└──────────────────┘
```

## Sicherheits-Layers

```
Layer 1: Authentication
  └─→ Login-Prozess, Passwort-Hashing

Layer 2: Authorization
  └─→ Berechtigungs-Prüfung bei jeder Operation

Layer 3: Access Control
  └─→ Datei/Ressourcen-spezifische Rechte

Layer 4: Audit
  └─→ Logging aller sicherheitsrelevanten Aktionen

Layer 5: Session Management
  └─→ Timeout, Concurrent Sessions Control
```

## Performance-Überlegungen

```
Operation               | Komplexität | Optimierung
------------------------|-------------|---------------------------
Login                   | O(1)        | Dictionary-Lookup
Permission Check        | O(1)        | Bitwise-Operation
User by ID              | O(1)        | Dictionary-Lookup
User by Username        | O(n)        | Index erstellen
List all Users          | O(n)        | Unvermeidbar
Add User                | O(1)        | Dictionary-Insert
Delete User             | O(1)        | Dictionary-Remove
Check File Permission   | O(1)        | Dictionary + Bitwise
```

## Speicher-Layout (RAM)

```
Heap:
├── UserDatabase
│   ├── Dictionary<int, User> users (32 bytes per User)
│   └── Dictionary<int, UserGroup> groups (24 bytes per Group)
│
├── AuthenticationManager
│   └── User currentUser (reference, 8 bytes)
│
├── PermissionManager
│   └── References (16 bytes)
│
└── FilePermissionManager
    └── Dictionary<string, FilePermission> (40 bytes per entry)

Geschätzte RAM-Nutzung:
- 10 Users: ~320 bytes
- 5 Groups: ~120 bytes
- 100 File Permissions: ~4000 bytes
Total: ~4.5 KB (sehr effizient!)
```

## Zusammenfassung

Die Architektur folgt dem **Separation of Concerns** Prinzip:
- Jede Komponente hat eine klare Verantwortung
- Loose Coupling durch Manager-Pattern
- Erweiterbar durch klare Interfaces
- Performant durch effiziente Datenstrukturen

Die modulare Struktur erlaubt einfache Wartung und Erweiterung ohne bestehenden Code zu ändern.

---

Diese Architektur bietet eine solide Basis für ein produktionsreifes Benutzerverwaltungssystem.
