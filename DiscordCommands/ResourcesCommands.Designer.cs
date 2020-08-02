﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DiscordCommands {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ResourcesCommands {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ResourcesCommands() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DiscordCommands.ResourcesCommands", typeof(ResourcesCommands).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to accesslevel.
        /// </summary>
        internal static string AccessLevelCommand {
            get {
                return ResourceManager.GetString("AccessLevelCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shows or sets your accesslevel.
        /// </summary>
        internal static string AccessLevelDescription {
            get {
                return ResourceManager.GetString("AccessLevelDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You need to be atleast Dev to set someone to host!.
        /// </summary>
        internal static string AccessLevelDevOnlyAddHost {
            get {
                return ResourceManager.GetString("AccessLevelDevOnlyAddHost", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only the host can add more admins!.
        /// </summary>
        internal static string AccessLevelHostOnlyAddAdmins {
            get {
                return ResourceManager.GetString("AccessLevelHostOnlyAddAdmins", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dev permission can only be set via db.
        /// </summary>
        internal static string AccessLevelSetDevPermission {
            get {
                return ResourceManager.GetString("AccessLevelSetDevPermission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Set {0} from {1} to {2}.
        /// </summary>
        internal static string AccessLevelSetPermission {
            get {
                return ResourceManager.GetString("AccessLevelSetPermission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !accesslevel
        ///!accesslevel &lt;userId&gt;\n
        ///Admin:\n
        ///!accesslevel &lt;userId&gt; &lt;new level (User, VIP, Moderator, Admin, Host, Dev)&gt;.
        /// </summary>
        internal static string AccessLevelUsage {
            get {
                return ResourceManager.GetString("AccessLevelUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} your access level is {1}.
        /// </summary>
        internal static string AccessLevelUser {
            get {
                return ResourceManager.GetString("AccessLevelUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access level of user {0} is {1}.
        /// </summary>
        internal static string AccessLevelUserOther {
            get {
                return ResourceManager.GetString("AccessLevelUserOther", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to bws.
        /// </summary>
        internal static string BWSCommand {
            get {
                return ResourceManager.GetString("BWSCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calculates the bws rank.
        /// </summary>
        internal static string BWSCommandDescription {
            get {
                return ResourceManager.GetString("BWSCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not parse the badge count {0}.
        /// </summary>
        internal static string BWSCommandFailedParseBadgeCount {
            get {
                return ResourceManager.GetString("BWSCommandFailedParseBadgeCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not parse the rank {0}.
        /// </summary>
        internal static string BWSCommandFailedParseRank {
            get {
                return ResourceManager.GetString("BWSCommandFailedParseRank", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} BWS: {1}.
        /// </summary>
        internal static string BWSCommandResult {
            get {
                return ResourceManager.GetString("BWSCommandResult", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !bws &lt;rank&gt; &lt;badgeCount&gt;.
        /// </summary>
        internal static string BWSCommandUsage {
            get {
                return ResourceManager.GetString("BWSCommandUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command.
        /// </summary>
        internal static string Command {
            get {
                return ResourceManager.GetString("Command", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command Info.
        /// </summary>
        internal static string CommandInfo {
            get {
                return ResourceManager.GetString("CommandInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command List.
        /// </summary>
        internal static string CommandList {
            get {
                return ResourceManager.GetString("CommandList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Command Type.
        /// </summary>
        internal static string CommandType {
            get {
                return ResourceManager.GetString("CommandType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Description.
        /// </summary>
        internal static string Description {
            get {
                return ResourceManager.GetString("Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to help.
        /// </summary>
        internal static string HelpCommand {
            get {
                return ResourceManager.GetString("HelpCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Displays a command list or infos about a specific command.
        /// </summary>
        internal static string HelpDescription {
            get {
                return ResourceManager.GetString("HelpDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt; &gt; = required
        ///[ ] = optional
        //// = choose between
        ///!! !! = atleast one marked parameter required.
        /// </summary>
        internal static string HelpFooter {
            get {
                return ResourceManager.GetString("HelpFooter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !help [page]
        ///!help &lt;command&gt;.
        /// </summary>
        internal static string HelpUsage {
            get {
                return ResourceManager.GetString("HelpUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Is Disabled.
        /// </summary>
        internal static string IsDisabled {
            get {
                return ResourceManager.GetString("IsDisabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to maintenance.
        /// </summary>
        internal static string MaintenanceCommand {
            get {
                return ResourceManager.GetString("MaintenanceCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sets the maintenance message/status.
        /// </summary>
        internal static string MaintenanceCommandDescription {
            get {
                return ResourceManager.GetString("MaintenanceCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to parse status.
        /// </summary>
        internal static string MaintenanceCommandFailedParseStatus {
            get {
                return ResourceManager.GetString("MaintenanceCommandFailedParseStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Set status to: {0} and message to: {1}.
        /// </summary>
        internal static string MaintenanceCommandSetStatus {
            get {
                return ResourceManager.GetString("MaintenanceCommandSetStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !maintenance &lt;status&gt; &lt;message&gt;.
        /// </summary>
        internal static string MaintenanceCommandUsage {
            get {
                return ResourceManager.GetString("MaintenanceCommandUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Notice.
        /// </summary>
        internal static string Notice {
            get {
                return ResourceManager.GetString("Notice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Page.
        /// </summary>
        internal static string Page {
            get {
                return ResourceManager.GetString("Page", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to permission.
        /// </summary>
        internal static string PermissionCommand {
            get {
                return ResourceManager.GetString("PermissionCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access Level {0} not found!.
        /// </summary>
        internal static string PermissionCommandAccessLevelNotFound {
            get {
                return ResourceManager.GetString("PermissionCommandAccessLevelNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission bind already exists.
        /// </summary>
        internal static string PermissionCommandBindAlreadyExists {
            get {
                return ResourceManager.GetString("PermissionCommandBindAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Binded access level {0} to role {1}.
        /// </summary>
        internal static string PermissionCommandBinded {
            get {
                return ResourceManager.GetString("PermissionCommandBinded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Binds or unbinds a permission.
        /// </summary>
        internal static string PermissionCommandDescription {
            get {
                return ResourceManager.GetString("PermissionCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You do not have enough permissions.
        /// </summary>
        internal static string PermissionCommandInsufficientPermission {
            get {
                return ResourceManager.GetString("PermissionCommandInsufficientPermission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No permission binds found.
        /// </summary>
        internal static string PermissionCommandNoPermissionsFound {
            get {
                return ResourceManager.GetString("PermissionCommandNoPermissionsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Discord Role {0} not found!.
        /// </summary>
        internal static string PermissionCommandRoleNotFound {
            get {
                return ResourceManager.GetString("PermissionCommandRoleNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unbinded {0} permissions from role {1}.
        /// </summary>
        internal static string PermissionCommandUnbinded {
            get {
                return ResourceManager.GetString("PermissionCommandUnbinded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !permission bind &lt;discordRoleId&gt; &lt;accessLevel&gt;
        ///!permission unbind &lt;discordRoleId&gt; [accessLevel].
        /// </summary>
        internal static string PermissionCommandUsage {
            get {
                return ResourceManager.GetString("PermissionCommandUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to privacy.
        /// </summary>
        internal static string PrivacyPolicyCommand {
            get {
                return ResourceManager.GetString("PrivacyPolicyCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Displays the bots privacy policy.
        /// </summary>
        internal static string PrivacyPolicyCommandDescription {
            get {
                return ResourceManager.GetString("PrivacyPolicyCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Privacy Policy
        ///```
        ///The following data is collected:
        ///Your Discord user data (username, ID, mention, roles)
        ///Your osu! data (username, ID, Scores)
        ///This data is used to verify users, analyze tournament matches, deliver tourney stats and improve the general discord user feeling
        ///
        ///
        ///If you have any questions or want your data deleted contact me on discord: ??????#0284
        ///I will try to answer withing 48 hours
        ///(If you request the deletion of your data, your data will be deleted and you will be blacklisted from [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PrivacyPolicyCommandPrivacyPolicy {
            get {
                return ResourceManager.GetString("PrivacyPolicyCommandPrivacyPolicy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !privacy.
        /// </summary>
        internal static string PrivacyPolicyCommandUsage {
            get {
                return ResourceManager.GetString("PrivacyPolicyCommandUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Registered {0} for discord command exceptions.
        /// </summary>
        internal static string RegisteredCommand {
            get {
                return ResourceManager.GetString("RegisteredCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to stats.
        /// </summary>
        internal static string StatsCommand {
            get {
                return ResourceManager.GetString("StatsCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Displays stats for players/teams/matches.
        /// </summary>
        internal static string StatsCommandDescription {
            get {
                return ResourceManager.GetString("StatsCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !stats p/player p/profile osuUserId/osuUsername
        ///!stats p/player t/top [page]
        ///
        ///!stats t/team p/profile osuUserId/osuUsername
        ///!stats t/team t/top [page]
        ///
        ///!stats m/match &lt;matchId&gt;
        ///!stats m/match &lt;team a&gt; vs &lt;team b&gt;.
        /// </summary>
        internal static string StatsCommandUsage {
            get {
                return ResourceManager.GetString("StatsCommandUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage.
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to warmup.
        /// </summary>
        internal static string WarmupCommand {
            get {
                return ResourceManager.GetString("WarmupCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added/Deleted maps.
        /// </summary>
        internal static string WarmupCommandConfirmation {
            get {
                return ResourceManager.GetString("WarmupCommandConfirmation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add or remove warmup maps.
        /// </summary>
        internal static string WarmupCommandDescription {
            get {
                return ResourceManager.GetString("WarmupCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to !warmup add &lt;beatmapId&gt; [beatmapId] [etc.]
        ///!warmup remove &lt;beatmapId&gt; [beatmapId] [etc.].
        /// </summary>
        internal static string WarmupCommandUsage {
            get {
                return ResourceManager.GetString("WarmupCommandUsage", resourceCulture);
            }
        }
    }
}
