using System;

namespace SlotOS.System
{
    /// <summary>
    /// Definiert die verschiedenen Benutzerrollen im System
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Administrator mit vollen Systemrechten
        /// </summary>
        Admin,

        /// <summary>
        /// Standard-Benutzer mit normalen Berechtigungen
        /// </summary>
        Standard,

        /// <summary>
        /// Gast-Benutzer mit eingeschränkten Rechten
        /// </summary>
        Guest
    }
}
