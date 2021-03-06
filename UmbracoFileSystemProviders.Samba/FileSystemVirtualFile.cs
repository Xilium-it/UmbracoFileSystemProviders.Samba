﻿// <copyright file="FileSystemVirtualFile.cs" company="James Jackson-South, Jeavon Leopold, and contributors">
// Copyright (c) James Jackson-South, Jeavon Leopold, and contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace Our.Umbraco.FileSystemProviders.Samba
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Hosting;

    /// <summary>
    /// Represents a file object in a virtual file.
    /// </summary>
    internal class FileSystemVirtualFile : VirtualFile
    {
        /// <summary>
        /// The stream function delegate.
        /// </summary>
        private readonly Func<Stream> getStream;

        /// <summary>
        /// The FileSystem where file is stored.
        /// </summary>
        private readonly Func<SambaFileSystem> getFileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemVirtualFile"/> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="fileSystem">The lazy file system implementation.</param>
        /// <param name="fileSystemPath">The modified file system path.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="fileSystem"/> is null.
        /// </exception>
        public FileSystemVirtualFile(string virtualPath, Lazy<SambaFileSystem> fileSystem, string fileSystemPath)
            : base(virtualPath)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            this.getFileSystem = () => fileSystem.Value;
            this.getStream = () => this.getFileSystem().OpenFile(fileSystemPath);
        }

        /// <summary>
        /// Gets a value indicating whether this is a virtual resource that should be treated as a file.
        /// </summary>
        /// <returns>
        /// Always false.
        /// </returns>
        public override bool IsDirectory => false;

        /// <summary>
        /// When overridden in a derived class, returns a read-only stream to the virtual resource.
        /// </summary>
        /// <returns>
        /// A read-only stream to the virtual file.
        /// </returns>
        public override Stream Open()
        {
            // Set the response headers here. It's a bit hacky.
            if (HttpContext.Current != null)
            {
                HttpCachePolicy cache = HttpContext.Current.Response.Cache;
                cache.SetCacheability(HttpCacheability.Public);
                cache.VaryByHeaders["Accept-Encoding"] = true;

                var sambaFileSystem = getFileSystem();
                int maxDays = sambaFileSystem.FileSystem.MaxDays;

                cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(maxDays));
                cache.SetMaxAge(new TimeSpan(maxDays, 0, 0, 0));
                cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            }

            return this.getStream();
        }
    }
}
