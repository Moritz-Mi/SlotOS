using System;
using System.Collections.Generic;

namespace CosmosKernel1.UserManagement
{
    /// <summary>
    /// Repräsentiert einen Benutzer im System
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public Permission Permissions { get; set; }
        public List<int> GroupIds { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        public string HomeDirectory { get; set; }

        public User()
        {
            GroupIds = new List<int>();
            IsActive = true;
            CreatedDate = DateTime.Now;
        }

        public User(int userId, string username, string passwordHash, UserRole role)
        {
            UserId = userId;
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
            Permissions = PermissionHelper.RoleToPermission(role);
            GroupIds = new List<int>();
            CreatedDate = DateTime.Now;
            IsActive = true;
            HomeDirectory = $"0:\\users\\{username}\\";
        }

        /// <summary>
        /// Prüft ob der Benutzer eine bestimmte Berechtigung hat
        /// </summary>
        public bool HasPermission(Permission permission)
        {
            return PermissionHelper.HasPermission(Permissions, permission);
        }

        /// <summary>
        /// Aktualisiert das LastLoginDate
        /// </summary>
        public void UpdateLastLogin()
        {
            LastLoginDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{UserId}] {Username} ({Role}) - Active: {IsActive}";
        }
    }

    /// <summary>
    /// Repräsentiert eine Benutzergruppe
    /// </summary>
    public class UserGroup
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public Permission GroupPermissions { get; set; }
        public string Description { get; set; }
        public List<int> MemberUserIds { get; set; }

        public UserGroup()
        {
            MemberUserIds = new List<int>();
        }

        public UserGroup(int groupId, string groupName, Permission permissions)
        {
            GroupId = groupId;
            GroupName = groupName;
            GroupPermissions = permissions;
            MemberUserIds = new List<int>();
        }

        /// <summary>
        /// Fügt einen Benutzer zur Gruppe hinzu
        /// </summary>
        public void AddMember(int userId)
        {
            if (!MemberUserIds.Contains(userId))
            {
                MemberUserIds.Add(userId);
            }
        }

        /// <summary>
        /// Entfernt einen Benutzer aus der Gruppe
        /// </summary>
        public void RemoveMember(int userId)
        {
            MemberUserIds.Remove(userId);
        }

        public override string ToString()
        {
            return $"[{GroupId}] {GroupName} - Members: {MemberUserIds.Count}";
        }
    }
}
