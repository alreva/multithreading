using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using Dir.Display;

namespace Dir.Read
{
    public static class FileSystemNodeBuilder
    {
        private static readonly IFormatProvider TimeFormat = DateTimeFormatInfo.InvariantInfo;

        public static FileSystemNode Create(FileInfo file)
        {
            string fullPath = GetFullPath(file);

            NameValue[] properties = BuildProperties(file, File.GetAccessControl, fullPath);

            return new FileSystemNode(fullPath, file.Length, properties);
        }

        public static FileSystemNode Create(DirectoryInfo dir, long calculatedSize = 0)
        {
            string fullPath = GetFullPath(dir);

            NameValue[] properties = BuildProperties(dir, Directory.GetAccessControl, fullPath);

            return new FileSystemNode(fullPath, calculatedSize, properties);
        }

        private static NameValue[] BuildProperties(
            FileSystemInfo info,
            Func<string, CommonObjectSecurity> accessControlGetter,
            string fullPath)
        {
            var properties = new List<NameValue>();

            info.AddFileSystemInfoPropertiesTo(properties);

            CommonObjectSecurity security;
            if (TryGetAccessControl(fullPath, accessControlGetter, out security))
            {
                security.AddSecurityPropertiesTo(properties);
            }

            return properties.ToArray();
        }

        private static bool TryGetAccessControl(
            this string fullPath,
            Func<string, CommonObjectSecurity> accessControlGetter,
            out CommonObjectSecurity accessControl)
        {
            if (fullPath.IsPathTooLong())
            {
                // Unfortunately standard .Net System.IO does not provide access control for long paths.
                // Need to use non-standard libraries such as
                //     - Pri.LongPath (see https://www.nuget.org/packages/Pri.LongPath/)
                //     - AlphaFS (see http://alphafs.alphaleonis.com/)
                //     - P/Invoke:
                //         - GetSecurityInfo https://msdn.microsoft.com/en-us/library/windows/desktop/aa446654(v=vs.85).aspx
                //         - GetEffectiveRightsFromAcl https://msdn.microsoft.com/en-us/library/windows/desktop/aa446637(v=vs.85).aspx
                // For the time being this case is not supported anf the TryGetAccessControl will fail safely.
                accessControl = null;
                return false;
            }

            try
            {
                accessControl = accessControlGetter(fullPath);
                return true;
            }
            catch (Exception exception)
            {
                if (!IsSecurityRelatedException(exception))
                {
                    throw;
                }

                accessControl = null;
                return false;
            }
        }

        private static bool IsSecurityRelatedException(Exception exception)
        {
            return
                exception is UnauthorizedAccessException
                    // Well, this is a hard code that tells the exact reason why the exception occured.
                    // Maybe there is an error code that corresponds to this particular error.
                || (exception is SystemException && exception.Message == "The trust relationship between this workstation and the primary domain failed");
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

        private static void AddSecurityPropertiesTo(this CommonObjectSecurity security, List<NameValue> properties)
        {
            string owner = security.GetOwner(typeof (NTAccount)).ToString();
            properties.Add(new NameValue(nameof(owner), owner));

            FileSystemRights effectivePermissions = GetEffectivePermissionsForCurrentUser(security);
            if (effectivePermissions != default(FileSystemRights))
            {
                properties.Add(new NameValue(nameof(effectivePermissions), effectivePermissions.EnumValueToString()));
            }
        }

        private static FileSystemRights GetEffectivePermissionsForCurrentUser(CommonObjectSecurity security)
        {
            AuthorizationRuleCollection authorizationRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
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
