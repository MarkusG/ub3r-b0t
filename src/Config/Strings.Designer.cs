﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UB3RB0T.Config {
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
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UB3RB0T.Config.Strings", typeof(Strings).Assembly);
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
        ///   Looks up a localized string similar to yeah I don&apos;t have the permissions to delete messages, buttwad..
        /// </summary>
        internal static string RequireManageMessages {
            get {
                return ResourceManager.GetString("RequireManageMessages", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to gee thanks asswad I can&apos;t manage roles in this server. not much I can do for ya here buddy. unless you wanna, y&apos;know, up my permissions.
        /// </summary>
        internal static string RequireManageRoles {
            get {
                return ResourceManager.GetString("RequireManageRoles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to gee thanks I can&apos;t manage roles in this server. not much I can do for ya here buddy. unless you wanna, y&apos;know, up my permissions.
        /// </summary>
        internal static string RequireManageRolesNice {
            get {
                return ResourceManager.GetString("RequireManageRolesNice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I need to have add reactions permissions..
        /// </summary>
        internal static string RequireReactionAdd {
            get {
                return ResourceManager.GetString("RequireReactionAdd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Restricted to server owner..
        /// </summary>
        internal static string RequireUserGuildOwner {
            get {
                return ResourceManager.GetString("RequireUserGuildOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must have manage server permissions to use that command. nice try, dungheap.
        /// </summary>
        internal static string RequireUserManageGuild {
            get {
                return ResourceManager.GetString("RequireUserManageGuild", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to you don&apos;t have permissions to clear messages, fartface.
        /// </summary>
        internal static string RequireUserManageMessages {
            get {
                return ResourceManager.GetString("RequireUserManageMessages", resourceCulture);
            }
        }
    }
}