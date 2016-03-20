using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Dir.Read
{
    public static class FileSystemNodeBuilder
    {
        private static readonly IFormatProvider TimeFormat = DateTimeFormatInfo.InvariantInfo;

        public static FileSystemNode Create(FileInfo file)
        {
            var properties = new List<NameValue>();

            file.AddFileSystemInfoPropertiesTo(properties);

            string owner = file.GetAccessControl().GetOwner(typeof(NTAccount)).ToString();
            properties.Add(new NameValue(nameof(owner), owner));

            FileSystemRights effectivePermissions = GetEffectivePermissionsOnFileForCurrentUser(file);
            if (effectivePermissions != default (FileSystemRights))
            {
                properties.Add(new NameValue(nameof(effectivePermissions), effectivePermissions.ToString2()));
            }

            return new FileSystemNode(GetFullPath(file), file.Length, properties.ToArray());
        }

        public static FileSystemNode Create(DirectoryInfo dir, long calculatedSize = 0)
        {
            var properties = new List<NameValue>();

            dir.AddFileSystemInfoPropertiesTo(properties);

            string owner = dir.GetAccessControl().GetOwner(typeof(NTAccount)).ToString();
            properties.Add(new NameValue(nameof(owner), owner));

            FileSystemRights effectivePermissions = GetEffectivePermissionsOnDirectoryForCurrentUser(dir);
            if (effectivePermissions != default(FileSystemRights))
            {
                properties.Add(new NameValue(nameof(effectivePermissions), effectivePermissions.ToString2()));
            }

            return new FileSystemNode(GetFullPath(dir), calculatedSize, properties.ToArray());
        }

        /// <summary>
        /// Adds the following properties: created, modified, attributes
        /// </summary>
        private static void AddFileSystemInfoPropertiesTo(this FileSystemInfo info, List<NameValue> properties)
        {
            string created = info.CreationTime.ToString(TimeFormat);
            string modified = info.LastWriteTime.ToString(TimeFormat);
            string attributes = info.Attributes.ToString();

            properties.Add(new NameValue(nameof(created), created));
            properties.Add(new NameValue(nameof(modified), modified));
            properties.Add(new NameValue(nameof(attributes), attributes));
        }

        private static FileSystemRights GetEffectivePermissionsOnDirectoryForCurrentUser(DirectoryInfo dir)
        {
            AuthorizationRuleCollection authorizationRules = dir.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            return GetEffectivePermissionsForCurrentUser(authorizationRules);
        }

        private static FileSystemRights GetEffectivePermissionsOnFileForCurrentUser(FileInfo file)
        {
            AuthorizationRuleCollection authorizationRules = file.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            return GetEffectivePermissionsForCurrentUser(authorizationRules);
        }

        private static FileSystemRights GetEffectivePermissionsForCurrentUser(AuthorizationRuleCollection authorizationRules)
        {
            FileSystemAccessRule[] accessRules = GetAccessRulesForCurrentUser(authorizationRules);

            FileSystemRights allowRights = accessRules
                .Where(r => r.AccessControlType == AccessControlType.Allow)
                .Select(r => r.FileSystemRights)
                .Aggregate((agg, allowPermission) => agg | allowPermission);

            FileSystemRights withDenies = accessRules
                .Where(r => r.AccessControlType == AccessControlType.Deny)
                .Select(r => r.FileSystemRights)
                .Aggregate(allowRights, (agg, denyPermission) => agg & ~denyPermission);

            return withDenies;
        }

        private static FileSystemAccessRule[] GetAccessRulesForCurrentUser(AuthorizationRuleCollection authorizationRules)
        {
            // Here it is better to supply the user as a parameter from the proper design perspective;
            // However, this does not give performance benefits (checked with VS CPU profiler) and
            // adds more lines to code (need to supply windows identity in all the methods or to set up a DTO that has both the authorizationRules and the user 
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            if (currentUser == null)
            {
                return new FileSystemAccessRule[0];
            }

            // The way the user SIDs are provided is not a brilliant one, since these are the permissions cached on a local PC;
            // Instead, the user SIDs could be searched against the Acti Directory
            // (see System.DirectoryServices.AccountManagement.PrincipalContext and System.DirectoryServices.AccountManagement.PrincipalSearcher)
            // and then fall back to this method if AD is not available.
            // But IMHO this is too much for a test task :)
            HashSet<string> sids = GetUserSids(currentUser);

            FileSystemAccessRule[] rulesForCurrentUser = authorizationRules
                .OfType<FileSystemAccessRule>()
                .Where(rule => sids.Contains(rule.IdentityReference.Value)).ToArray();

            return rulesForCurrentUser;
        }

        public static string GetFullPath(FileSystemInfo info)
        {
            try
            {
                return info.FullName;
            }
            catch (PathTooLongException)
            {
                return (string)info.GetType()
                    .GetField("FullPath", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(info);
            }
        }

        private static HashSet<string> GetUserSids(WindowsIdentity windowsIdentity)
        {
            var sids = new HashSet<string>();

            if (windowsIdentity.User != null)
            {
                sids.Add(windowsIdentity.User.Value);
            }

            if (windowsIdentity.Groups != null)
            {
                foreach (IdentityReference @group in windowsIdentity.Groups)
                {
                    sids.Add(group.Value);
                }
            }

            return sids;
        }
    }
}
