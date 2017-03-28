// <copyright file="Constants.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Samba
{
    /// <summary>
    /// Constant strings for use within the application.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The default route path for media objects.
        /// </summary>
        public const string DefaultMediaRoute = "media";

        /// <summary>
        /// The default MaxDays value for browser cache.
        /// </summary>
        public const int DefaultMaxDays = 365;

        /// <summary>
        /// The default VirtualPathRoute value.
        /// </summary>
        public const string DefaultVirtualPathRoute = null;

        /// <summary>
        /// The web.config configuration setting constants.
        /// </summary>
        public static class WebConfiguration
        {
            /// <summary>
            /// The configuration key for providing the working path via the web.config
            /// </summary>
            public const string FullPathKey = "SambaFileSystem.FullPath";

            /// <summary>
            /// The configuration key for providing the Root URL via the web.config
            /// </summary>
            public const string RootUrlKey = "SambaFileSystem.RootUrl";

            /// <summary>
            /// The configuration key for providing the ConnectionString via the web.config
            /// </summary>
            public const string ConnectionStringKey = "SambaFileSystem.ConnectionString";

            /// <summary>
            /// The configuration key for providing the Maximum Days Cache value via the web.config
            /// </summary>
            public const string MaxDaysKey = "SambaFileSystem.MaxDays";

            /// <summary>
            /// The configuration key for providing the Use VirtualPath Root value via the web.config
            /// </summary>
            public const string VirtualPathRouteKey = "SambaFileSystem.VirtualPathRoute";
        }

		/// <summary>
		/// The config/FileSystemProviders.config configuration setting constants
		/// </summary>
	    public static class FileSystemConfiguration
	    {
		    /// <summary>
            /// The working path.
            /// </summary>
            public const string FullPathKey = "fullPath";

            /// <summary>
            /// The configuration key for providing the Root URL
            /// </summary>
            public const string RootUrlKey = "rootUrl";

            /// <summary>
            /// The configuration key for providing the ConnectionString
            /// </summary>
            public const string ConnectionStringKey = "connectionString";

            /// <summary>
            /// The configuration key for providing the Maximum Days
            /// </summary>
            public const string MaxDaysKey = "maxDays";

            /// <summary>
            /// The configuration key for providing the Use VirtualPath Root value
            /// </summary>
            public const string VirtualPathRouteKey = "virtualPathRoute";
	    }

        /// <summary>
        /// The connection string arguments.
        /// </summary>
        public static class ConnectionString
        {
            /// <summary>
            /// The path of Samba shared folder; ex.: "\\server\folder".
            /// </summary>
            public const string SambaPathKey = "sambaPath";

            /// <summary>
            /// The Username attribute name.
            /// </summary>
            public const string UsernameKey = "username";

            /// <summary>
            /// The Domain of Username attribute name.
            /// </summary>
            public const string DomainKey = "domain";

            /// <summary>
            /// The Password attribute name.
            /// </summary>
            public const string PasswordKey = "password";
        }
    }
}
