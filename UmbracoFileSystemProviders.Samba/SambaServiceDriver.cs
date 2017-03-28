using System.Net;
using Our.Umbraco.FileSystemProviders.Samba.Helpers;
using Umbraco.Core;

#pragma warning disable SA1027 // Use tabs correctly

namespace Our.Umbraco.FileSystemProviders.Samba {
    using System;
    using System.Collections.Generic;
	using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Umbraco.Core.IO;

    /// <summary>
    /// Manage the OpenStack ObjectStorage service
    /// </summary>
    internal class SambaServiceDriver : IFileSystem
    {
        private static List<SambaServiceDriver> serviceDriverInstances = new List<SambaServiceDriver>();

        /// <summary>
        /// The delimiter.
        /// </summary>
        public const string Delimiter = FileSystemPathHelper.Delimiter;

        /// <summary>
        /// Our object to lock against during initialization.
        /// </summary>
        private static readonly object Locker = new object();

        /// <summary>
        /// The key of instance.
        /// </summary>
        private readonly string instanceKey;

        /// <summary>
        /// The working path.
        /// </summary>
        private readonly string fullPath;

        /// <summary>
        /// The root url of container for public access.
        /// </summary>
        private readonly string rootHostUrl;

		/// <summary>
		/// Samba credential to access to fullPath.
		/// </summary>
	    private readonly NetworkCredential sambaCredential;

        /// <summary>
        /// Initializes a new instance of the <see cref="SambaServiceDriver"/> class.
        /// </summary>
        /// <param name="fullPath">The working full path.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <param name="virtualPathRoute">When defined, Whether to use the default "media" route in the url independent of the blob container.</param>
        protected SambaServiceDriver(string fullPath, string rootUrl, string connectionString, int maxDays, string virtualPathRoute)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                throw new ArgumentNullException(nameof(fullPath));
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this.VirtualPathRouteDisabled = string.IsNullOrEmpty(virtualPathRoute);
			
            var connectionStringParser = new ConnectionStringParser();
            var connectionStringData = connectionStringParser.Decode(connectionString);

			// Full path must use `directorySeparatorChar` and ends with separator.
	        this.fullPath = fullPath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
			if (this.fullPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) == false)
            {
                this.fullPath += System.IO.Path.DirectorySeparatorChar;
            }

			// SambaPath must be use `directorySeparatorChar` and do not end with separator.
	        this.SambaPath = connectionStringData.SambaPath
				.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar)
				.TrimEnd(System.IO.Path.DirectorySeparatorChar);
			this.sambaCredential = new NetworkCredential(connectionStringData.Username, connectionStringData.Password, connectionStringData.Domain);
			
            this.instanceKey = CreateInstanceKey(fullPath, rootUrl, connectionString, virtualPathRoute);
            this.MaxDays = maxDays;
            this.VirtualPathRoute = this.VirtualPathRouteDisabled ? null : virtualPathRoute;
			
            this.rootHostUrl = rootUrl;
            if (string.IsNullOrEmpty(this.rootHostUrl))
            {
                this.rootHostUrl = null;
            }
            else if (this.rootHostUrl.EndsWith("/") == false)
            {
                this.rootHostUrl += "/";
            }

            this.LogHelper = new WrappedLogHelper();
            this.MimeTypeResolver = new MimeTypeResolver();
        }

        /// <summary>
        /// Gets or sets the log helper.
        /// </summary>
        public ILogHelper LogHelper { get; set; }

        /// <summary>
        /// Gets or sets the MIME type resolver.
        /// </summary>
        public IMimeTypeResolver MimeTypeResolver { get; set; }

        /// <summary>
        /// Gets the Samba path.
        /// </summary>
        public string SambaPath { get; }

        /// <summary>
        /// Gets the maximum number of days to cache blob items for in the browser.
        /// </summary>
        public int MaxDays { get; }

        /// <summary>
        /// Gets or sets a value indicating the VirtualPath route. When not defined the route is disabled.
        /// </summary>
        public string VirtualPathRoute { get; }

        /// <summary>
        /// Gets or sets a value indicating if VirtualPath is disabled.
        /// </summary>
        public bool VirtualPathRouteDisabled { get; }

        /// <summary>
        /// Returns a singleton instance of the <see cref="SambaServiceDriver"/> class.
        /// </summary>
        /// <param name="fullPath">The working full path.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="maxDays">The maximum number of days to cache blob items for in the browser.</param>
        /// <param name="virtualPathRoute">When defined, Whether to use the default "media" route in the url independent of the blob container.</param>
        /// <returns>The <see cref="SambaServiceDriver"/></returns>
        public static SambaServiceDriver GetInstance(string fullPath, string rootUrl, string connectionString, int maxDays, string virtualPathRoute)
        {
            var newestInstanceKey = CreateInstanceKey(fullPath, rootUrl, connectionString, virtualPathRoute);

            lock (Locker)
            {
                var fileSystem = serviceDriverInstances.SingleOrDefault(fs => fs.instanceKey == newestInstanceKey);

                if (fileSystem == null)
                {
                    if (maxDays < 0)
                    {
                        maxDays = Constants.DefaultMaxDays;
                    }

                    fileSystem = new SambaServiceDriver(fullPath, rootUrl, connectionString, maxDays, virtualPathRoute);

                    serviceDriverInstances.Add(fileSystem);
                }

                return fileSystem;
            }
        }
        
        /// <summary>
        /// Gets all directories matching the given path.
        /// </summary>
        /// <param name="path">The path to the directories.</param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/> representing the matched directories.
        /// </returns>
        public IEnumerable<string> GetDirectories(string path)
        {
            var fullPath = this.GetFullPath(path);

            try
            {
	            using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	            {
					if (Directory.Exists(fullPath))
						return Directory.EnumerateDirectories(fullPath).Select(GetRelativePath);
	            }
            }
            catch (UnauthorizedAccessException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Not authorized to get directories", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Directory not found", ex);
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        public void DeleteDirectory(string path)
        {
            this.DeleteDirectory(path, false);
        }

        /// <summary>
        /// Deletes the specified directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <remarks>ObjectStorage storage has no real concept of directories so deletion is always recursive.</remarks>
        /// <param name="path">The name of the directory to remove.</param>
        /// <param name="recursive">
        /// <c>true</c> to remove directories, subdirectories, and files in path; otherwise, <c>false</c>.
        /// </param>
        public void DeleteDirectory(string path, bool recursive)
        {
            var fullPath = GetFullPath(path);
            if (Directory.Exists(fullPath) == false)
                return;

            try
            {
	            using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	            {
		            Directory.Delete(fullPath, recursive);
	            }
            }
            catch (DirectoryNotFoundException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Directory not found", ex);
            }
        }

        /// <summary>
        /// Determines whether the specified directory exists.
        /// </summary>
        /// <param name="path">The directory to check.</param>
        /// <returns>
        /// <c>True</c> if the directory exists and the user has permission to view it; otherwise <c>false</c>.
        /// </returns>
        public bool DirectoryExists(string path)
        {
            var fullPath = GetFullPath(path);
			using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	        {
		        return Directory.Exists(fullPath);
	        }
        }

        /// <summary>
        /// Adds a file to the file system.
        /// </summary>
        /// <param name="path">The path to the given file.</param>
        /// <param name="stream">The <see cref="Stream"/> containing the file contents.</param>
        public void AddFile(string path, Stream stream)
        {
            this.AddFile(path, stream, true);
        }

        /// <summary>
        /// Adds a file to the file system.
        /// </summary>
        /// <param name="path">The path to the given file.</param>
        /// <param name="stream">The <see cref="Stream"/> containing the file contents.</param>
        /// <param name="overrideIfExists">Whether to override the file if it already exists.</param>
        public void AddFile(string path, Stream stream, bool overrideIfExists) {
            var fullPath = GetFullPath(path);

	        using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	        {
				var exists = File.Exists(fullPath);
				if (exists && overrideIfExists == false) 
					throw new InvalidOperationException(string.Format("A file at path '{0}' already exists", path));

				Directory.CreateDirectory(Path.GetDirectoryName(fullPath)); // ensure it exists

				if (stream.CanSeek)
					stream.Seek(0, 0);

				using (var destination = (Stream)File.Create(fullPath))
					stream.CopyTo(destination);
	        }
        }

        /// <summary>
        /// Gets all files matching the given path.
        /// </summary>
        /// <param name="path">The path to the files.</param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/> representing the matched files.
        /// </returns>
        public IEnumerable<string> GetFiles(string path)
        {
            return this.GetFiles(path, "*.*");
        }

        /// <summary>
        /// Gets all files matching the given path and filter.
        /// </summary>
        /// <param name="path">The path to the files.</param>
        /// <param name="filter">A filter that allows the querying of file extension. <example>*.jpg</example></param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/> representing the matched files.
        /// </returns>
        public IEnumerable<string> GetFiles(string path, string filter)
        {
            var fullPath = GetFullPath(path);

            try
            {
	            using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	            {
					if (Directory.Exists(fullPath))
						return Directory.EnumerateFiles(fullPath, filter).Select(GetRelativePath);
	            }
            }
            catch (UnauthorizedAccessException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Not authorized to get directories", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Directory not found", ex);
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets a <see cref="Stream"/> containing the contains of the given file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// <see cref="Stream"/>.
        /// </returns>
        public Stream OpenFile(string path)
        {
            var fullPath = GetFullPath(path);
	        using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	        {
		        return File.OpenRead(fullPath);
	        }
        }
        
        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to remove.</param>
        public void DeleteFile(string path)
        {
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath) == false)
                return;

            try
            {
	            using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	            {
		            File.Delete(fullPath);
	            }
            }
            catch (FileNotFoundException ex)
            {
                LogHelper.Info<PhysicalFileSystem>(string.Format("DeleteFile failed with FileNotFoundException: {0}", ex.InnerException));
            }
        }
        
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>
        /// <c>True</c> if the file exists and the user has permission to view it; otherwise <c>false</c>.
        /// </returns>
        public bool FileExists(string path)
        {
            var fullpath = GetFullPath(path);
	        using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	        {
		        return File.Exists(fullpath);
	        }
        }

        /// <summary>
        /// Gets the relative path.
        /// </summary>
        /// <param name="fullPathOrUrl">The full path or url.</param>
        /// <returns>The path, relative to this filesystem's root.</returns>
        /// <remarks>
        /// <para>The relative path is relative to this filesystem's root, not starting with any
        /// directory separator. If input was recognized as a url (path), then output uses url (path) separator
        /// chars.</para>
        /// </remarks>
        public string GetRelativePath(string fullPathOrUrl)
        {
            // test url
            var path = fullPathOrUrl.Replace('\\', '/'); // ensure url separator char

            if (IOHelper.PathStartsWith(path, this.fullPath, '/')) // if it starts with the root url...
                return path.Substring(this.fullPath.Length) // strip it
                            .TrimStart('/'); // it's relative

            // test path
            path = this.EnsureDirectorySeparatorChar(fullPathOrUrl);

            if (IOHelper.PathStartsWith(path, this.fullPath, Path.DirectorySeparatorChar)) // if it starts with the root path
                return path.Substring(this.fullPath.Length) // strip it
                            .TrimStart(Path.DirectorySeparatorChar); // it's relative

            // unchanged - including separators
            return fullPathOrUrl;
        }


        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="path">The full or relative path.</param>
        /// <returns>The full path.</returns>
        /// <remarks>
        /// <para>On the physical filesystem, the full path is the rooted (ie non-relative), safe (ie within this
        /// filesystem's root) path. All separators are converted to Path.DirectorySeparatorChar.</para>
        /// </remarks>
        public string GetFullPath(string path)
        {
            // normalize
            var opath = path;
            path = this.EnsureDirectorySeparatorChar(path);

            // not sure what we are doing here - so if input starts with a (back) slash,
            // we assume it's not a FS relative path and we try to convert it... but it
            // really makes little sense?
            if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                path = GetRelativePath(path);

            // if already a full path, return
            if (IOHelper.PathStartsWith(path, this.fullPath.TrimEnd('\\'), Path.DirectorySeparatorChar))
                return path;

            // else combine and sanitize, ie GetFullPath will take care of any relative
            // segments in path, eg '../../foo.tmp' - it may throw a SecurityException
            // if the combined path reaches illegal parts of the filesystem
            var fpath = Path.Combine(this.fullPath, path);
            fpath = Path.GetFullPath(fpath);

            // at that point, path is within legal parts of the filesystem, ie we have
            // permissions to reach that path, but it may nevertheless be outside of
            // our root path, due to relative segments, so better check
            if (IOHelper.PathStartsWith(fpath, this.fullPath.TrimEnd('\\'), Path.DirectorySeparatorChar))
                return fpath;

            throw new FileSecurityException("File '" + opath + "' is outside this filesystem's root.");
        }

        /// <summary>
        /// Returns the url to the media item.
        /// </summary>
        /// <remarks>If the virtual path provider is enabled this returns a relative url.</remarks>
        /// <param name="path">The path to return the url for.</param>
        /// <returns>
        /// <see cref="string"/>.
        /// </returns>
        public string GetUrl(string path) {
            if (this.VirtualPathRouteDisabled)
            {
				// Absolute path
	            return System.IO.Path.Combine(this.rootHostUrl, path).Replace('\\', '/');
            }

			// Relative path
            return "/" + System.IO.Path.Combine(this.VirtualPathRoute, path).Replace('\\', '/');
        }

        /// <summary>
        /// Gets the last modified date/time of the file, expressed as a UTC value.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// <see cref="DateTimeOffset"/>.
        /// </returns>
        public DateTimeOffset GetLastModified(string path)
        {
	        using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	        {
				return DirectoryExists(path) 
					? new DirectoryInfo(GetFullPath(path)).LastWriteTimeUtc 
					: new FileInfo(GetFullPath(path)).LastWriteTimeUtc;
	        }
        }

        /// <summary>
        /// Gets the created date/time of the file, expressed as a UTC value.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// <see cref="DateTimeOffset"/>.
        /// </returns>
        public DateTimeOffset GetCreated(string path)
        {
            using (new Net.NetworkConnectionClient(this.SambaPath, this.sambaCredential))
	        {
				return DirectoryExists(path) 
					? Directory.GetCreationTimeUtc(GetFullPath(path)) 
					: File.GetCreationTimeUtc(GetFullPath(path));
	        }
        }

        /// <summary>
        /// Returns the instance Key for constructor arguments.
        /// </summary>
        /// <param name="fullPath">The working full path.</param>
        /// <param name="rootUrl">The root url.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="virtualPathRoute">When defined, Whether to use the default "media" route in the url independent of the blob container.</param>
        /// <returns>The <see cref="SambaServiceDriver"/> instance key</returns>
        protected static string CreateInstanceKey(string fullPath, string rootUrl, string connectionString, string virtualPathRoute)
        {
            return $"{connectionString}/{rootUrl}@{fullPath}({virtualPathRoute})";
        }
		

		#region Helper Methods

        protected virtual void EnsureDirectory(string path)
        {
            path = GetFullPath(path);
            Directory.CreateDirectory(path);
        }

        protected string EnsureTrailingSeparator(string path)
        {
            return path.EnsureEndsWith(Path.DirectorySeparatorChar);
        }

        protected string EnsureDirectorySeparatorChar(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            return path;
        }

        protected string EnsureUrlSeparatorChar(string path)
        {
            path = path.Replace('\\', '/');
            return path;
        }

        #endregion
    }
}
#pragma warning restore SA1027 // Use tabs correctly
