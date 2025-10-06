using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmosKernel1.UserManagement
{
    /// <summary>
    /// Verwaltet die Benutzerdatenbank (In-Memory für Cosmos OS)
    /// In einer erweiterten Version kann dies auf ein Dateisystem serialisiert werden
    /// </summary>
    public class UserDatabase
    {
        private Dictionary<int, User> users;
        private Dictionary<int, UserGroup> groups;
        private int nextUserId;
        private int nextGroupId;

        public UserDatabase()
        {
            users = new Dictionary<int, User>();
            groups = new Dictionary<int, UserGroup>();
            nextUserId = 1;
            nextGroupId = 1;

            // Erstelle Standard-Gruppen
            CreateDefaultGroups();
            
            // Erstelle Standard-Admin-Benutzer
            CreateDefaultAdmin();
        }

        /// <summary>
        /// Erstellt Standard-Gruppen
        /// </summary>
        private void CreateDefaultGroups()
        {
            var adminGroup = new UserGroup(nextGroupId++, "Administrators", Permission.Administrator);
            adminGroup.Description = "System Administrators with full control";
            groups.Add(adminGroup.GroupId, adminGroup);

            var userGroup = new UserGroup(nextGroupId++, "Users", Permission.ReadWrite | Permission.Execute);
            userGroup.Description = "Standard users with basic permissions";
            groups.Add(userGroup.GroupId, userGroup);

            var guestGroup = new UserGroup(nextGroupId++, "Guests", Permission.Read);
            guestGroup.Description = "Guest users with read-only access";
            groups.Add(guestGroup.GroupId, guestGroup);
        }

        /// <summary>
        /// Erstellt einen Standard-Administrator-Account
        /// </summary>
        private void CreateDefaultAdmin()
        {
            string adminPassword = "admin";
            string passwordHash = AuthenticationManager.HashPassword(adminPassword);
            var adminUser = new User(nextUserId++, "admin", passwordHash, UserRole.Administrator);
            adminUser.GroupIds.Add(1); // Administrators group
            users.Add(adminUser.UserId, adminUser);
        }

        /// <summary>
        /// Fügt einen neuen Benutzer hinzu
        /// </summary>
        public bool AddUser(User user)
        {
            if (users.ContainsKey(user.UserId))
            {
                return false;
            }

            if (GetUserByUsername(user.Username) != null)
            {
                return false; // Username bereits vergeben
            }

            users.Add(user.UserId, user);
            return true;
        }

        /// <summary>
        /// Aktualisiert einen existierenden Benutzer
        /// </summary>
        public bool UpdateUser(User user)
        {
            if (!users.ContainsKey(user.UserId))
            {
                return false;
            }

            users[user.UserId] = user;
            return true;
        }

        /// <summary>
        /// Löscht einen Benutzer
        /// </summary>
        public bool DeleteUser(int userId)
        {
            return users.Remove(userId);
        }

        /// <summary>
        /// Gibt einen Benutzer anhand der ID zurück
        /// </summary>
        public User GetUserById(int userId)
        {
            if (users.ContainsKey(userId))
            {
                return users[userId];
            }
            return null;
        }

        /// <summary>
        /// Gibt einen Benutzer anhand des Usernames zurück
        /// </summary>
        public User GetUserByUsername(string username)
        {
            return users.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gibt alle Benutzer zurück
        /// </summary>
        public List<User> GetAllUsers()
        {
            return users.Values.ToList();
        }

        /// <summary>
        /// Gibt alle aktiven Benutzer zurück
        /// </summary>
        public List<User> GetActiveUsers()
        {
            return users.Values.Where(u => u.IsActive).ToList();
        }

        /// <summary>
        /// Gibt die nächste verfügbare User-ID zurück
        /// </summary>
        public int GetNextUserId()
        {
            return nextUserId++;
        }

        /// <summary>
        /// Fügt eine neue Gruppe hinzu
        /// </summary>
        public bool AddGroup(UserGroup group)
        {
            if (groups.ContainsKey(group.GroupId))
            {
                return false;
            }

            groups.Add(group.GroupId, group);
            return true;
        }

        /// <summary>
        /// Gibt eine Gruppe anhand der ID zurück
        /// </summary>
        public UserGroup GetGroupById(int groupId)
        {
            if (groups.ContainsKey(groupId))
            {
                return groups[groupId];
            }
            return null;
        }

        /// <summary>
        /// Gibt alle Gruppen zurück
        /// </summary>
        public List<UserGroup> GetAllGroups()
        {
            return groups.Values.ToList();
        }

        /// <summary>
        /// Fügt einen Benutzer zu einer Gruppe hinzu
        /// </summary>
        public bool AddUserToGroup(int userId, int groupId)
        {
            var user = GetUserById(userId);
            var group = GetGroupById(groupId);

            if (user == null || group == null)
            {
                return false;
            }

            group.AddMember(userId);
            if (!user.GroupIds.Contains(groupId))
            {
                user.GroupIds.Add(groupId);
            }

            return true;
        }

        /// <summary>
        /// Entfernt einen Benutzer aus einer Gruppe
        /// </summary>
        public bool RemoveUserFromGroup(int userId, int groupId)
        {
            var user = GetUserById(userId);
            var group = GetGroupById(groupId);

            if (user == null || group == null)
            {
                return false;
            }

            group.RemoveMember(userId);
            user.GroupIds.Remove(groupId);

            return true;
        }

        /// <summary>
        /// Gibt die Anzahl der Benutzer zurück
        /// </summary>
        public int GetUserCount()
        {
            return users.Count;
        }

        /// <summary>
        /// Gibt die Anzahl der aktiven Benutzer zurück
        /// </summary>
        public int GetActiveUserCount()
        {
            return users.Values.Count(u => u.IsActive);
        }
    }
}
