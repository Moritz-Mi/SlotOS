using System;
using System.Text;

namespace SlotOS.System
{
    /// <summary>
    /// Klasse für sichere Passwort-Hashing mit Salt
    /// </summary>
    public static class PasswordHasher
    {
        // Salt-Länge in Bytes
        private const int SALT_LENGTH = 16;
        
        // Trennzeichen zwischen Salt und Hash
        private const char SEPARATOR = ':';
        
        // Statischer Zähler für zusätzliche Entropie
        private static int _saltCounter = 0;

        /// <summary>
        /// Hasht ein Passwort mit einem zufällig generierten Salt
        /// </summary>
        /// <param name="password">Das zu hashende Passwort (Klartext)</param>
        /// <returns>Ein String im Format "salt:hash"</returns>
        /// <exception cref="ArgumentException">Wenn das Passwort leer ist</exception>
        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Passwort darf nicht leer sein", nameof(password));

            // Salt generieren
            string salt = GenerateSalt();

            // Hash mit Salt berechnen
            string hash = ComputeHash(password, salt);

            // Salt und Hash zusammen speichern
            return $"{salt}{SEPARATOR}{hash}";
        }

        /// <summary>
        /// Überprüft, ob ein Passwort mit einem gespeicherten Hash übereinstimmt
        /// </summary>
        /// <param name="password">Das zu prüfende Passwort (Klartext)</param>
        /// <param name="hashedPassword">Der gespeicherte Hash im Format "salt:hash"</param>
        /// <returns>True wenn das Passwort korrekt ist, sonst False</returns>
        public static bool Verify(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                // Salt und Hash trennen
                int separatorIndex = hashedPassword.IndexOf(SEPARATOR);
                if (separatorIndex == -1)
                {
                    // Legacy-Format ohne Salt (für Abwärtskompatibilität)
                    return VerifyLegacy(password, hashedPassword);
                }

                string salt = hashedPassword.Substring(0, separatorIndex);
                string storedHash = hashedPassword.Substring(separatorIndex + 1);

                // Hash des eingegebenen Passworts mit gleichem Salt berechnen
                string computedHash = ComputeHash(password, salt);

                // Vergleichen
                return storedHash == computedHash;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generiert einen zufälligen Salt-String
        /// </summary>
        /// <returns>Hexadezimaler Salt-String</returns>
        private static string GenerateSalt()
        {
            // Cosmos OS hat eingeschränkten Zugriff auf Krypto-Funktionen
            // Verwende eine kombinierte Methode aus Zeit und pseudo-zufälligen Werten
            
            // Erhöhe Zähler für Einzigartigkeit
            _saltCounter++;
            
            // Kombiniere mehrere Entropiequellen für bessere Zufälligkeit
            int seed = (int)(DateTime.Now.Ticks ^ (_saltCounter * 1000000) ^ DateTime.Now.Millisecond);
            var random = new Random(seed);
            var saltBytes = new byte[SALT_LENGTH];
            
            // Fülle die ersten 4 Bytes mit dem Zähler für garantierte Einzigartigkeit
            byte[] counterBytes = BitConverter.GetBytes(_saltCounter);
            Buffer.BlockCopy(counterBytes, 0, saltBytes, 0, 4);
            
            // Fülle den Rest mit Zufallswerten
            for (int i = 4; i < SALT_LENGTH; i++)
            {
                saltBytes[i] = (byte)random.Next(0, 256);
            }

            // In Hexadezimal-String konvertieren
            return BytesToHex(saltBytes);
        }

        /// <summary>
        /// Berechnet den Hash eines Passworts mit einem gegebenen Salt
        /// HINWEIS: Dies ist eine Implementierung für Cosmos OS.
        /// In Cosmos OS ist System.Security.Cryptography möglicherweise nicht vollständig verfügbar.
        /// Diese Implementierung verwendet SHA256-ähnliche Logik, die für Cosmos OS geeignet ist.
        /// </summary>
        /// <param name="password">Das Passwort</param>
        /// <param name="salt">Der Salt</param>
        /// <returns>Hash als Hexadezimal-String</returns>
        private static string ComputeHash(string password, string salt)
        {
            // Kombiniere Passwort und Salt
            string combined = salt + password + salt;

            // Verwende einen robusten Hash-Algorithmus
            // Da Cosmos OS möglicherweise keinen Zugriff auf SHA256 hat,
            // verwenden wir eine stärkere Version des einfachen Hash
            
            // TODO: Falls System.Security.Cryptography.SHA256 in Cosmos verfügbar ist,
            // sollte dieser Code ersetzt werden durch:
            // using (var sha256 = System.Security.Cryptography.SHA256.Create())
            // {
            //     byte[] bytes = Encoding.UTF8.GetBytes(combined);
            //     byte[] hash = sha256.ComputeHash(bytes);
            //     return BytesToHex(hash);
            // }

            return ComputeCustomHash(combined);
        }

        /// <summary>
        /// Berechnet einen benutzerdefinierten Hash für Cosmos OS
        /// Verwendet mehrere Hash-Runden für erhöhte Sicherheit
        /// </summary>
        private static string ComputeCustomHash(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            
            // Mehrere Hash-Runden für bessere Sicherheit
            uint h1 = 0x6A09E667;
            uint h2 = 0xBB67AE85;
            uint h3 = 0x3C6EF372;
            uint h4 = 0xA54FF53A;

            for (int round = 0; round < 1000; round++) // 1000 Iterationen
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    uint b = bytes[i];
                    h1 = RotateLeft(h1 ^ b, 7) + h2;
                    h2 = RotateLeft(h2 ^ b, 13) + h3;
                    h3 = RotateLeft(h3 ^ b, 19) + h4;
                    h4 = RotateLeft(h4 ^ b, 23) + h1;
                }
                
                // Mix zwischen den Runden
                h1 ^= (uint)(round * 0x9E3779B9);
                h2 ^= (uint)(round * 0x6A09E667);
                h3 ^= (uint)(round * 0xBB67AE85);
                h4 ^= (uint)(round * 0x3C6EF372);
            }

            // Kombiniere die Hash-Werte
            byte[] result = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(h1), 0, result, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(h2), 0, result, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(h3), 0, result, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(h4), 0, result, 12, 4);

            return BytesToHex(result);
        }

        /// <summary>
        /// Rotiert Bits nach links
        /// </summary>
        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        /// <summary>
        /// Konvertiert Bytes in einen Hexadezimal-String
        /// </summary>
        private static string BytesToHex(byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:X2}", b);
            }
            return hex.ToString();
        }

        /// <summary>
        /// Überprüft Passwörter im alten Format (ohne Salt) für Abwärtskompatibilität
        /// </summary>
        private static bool VerifyLegacy(string password, string hash)
        {
            // Altes einfaches Hash-Format
            int legacyHash = 17;
            foreach (char c in password)
            {
                legacyHash = legacyHash * 31 + c;
            }
            return legacyHash.ToString("X8") == hash;
        }
    }
}
