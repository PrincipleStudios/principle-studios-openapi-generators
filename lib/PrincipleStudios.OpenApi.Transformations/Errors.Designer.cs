﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PrincipleStudios.OpenApi.Transformations {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Errors {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Errors() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PrincipleStudios.OpenApi.Transformations.Errors", typeof(Errors).Assembly);
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
        ///   Looks up a localized string similar to The given document URI contained a fragment; fragments are not allowed in document URIs..
        /// </summary>
        internal static string InvalidDocumentBaseUri {
            get {
                return ResourceManager.GetString("InvalidDocumentBaseUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The document&apos;s retrieval URI must be absolute..
        /// </summary>
        internal static string InvalidRetrievalUri {
            get {
                return ResourceManager.GetString("InvalidRetrievalUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parser could not handle the document provided.
        /// </summary>
        internal static string ParserCannotHandleDocument {
            get {
                return ResourceManager.GetString("ParserCannotHandleDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred resolving a document from a URI.
        /// </summary>
        internal static string ResolveDocumentException {
            get {
                return ResourceManager.GetString("ResolveDocumentException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred resolving a node from a URI.
        /// </summary>
        internal static string ResolveNodeException {
            get {
                return ResourceManager.GetString("ResolveNodeException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred while loading JSON or YAML file.
        /// </summary>
        internal static string UnableToLoadYaml {
            get {
                return ResourceManager.GetString("UnableToLoadYaml", resourceCulture);
            }
        }
    }
}
