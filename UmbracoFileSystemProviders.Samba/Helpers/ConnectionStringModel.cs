namespace Our.Umbraco.FileSystemProviders.Samba.Helpers {

    /// <summary>
    /// ConnectionString data
    /// </summary>
    internal class ConnectionStringModel {

        /// <summary>
        /// Gets or Sets the path of Samba shared folder; ex.: "\\server\folder".
        /// </summary>
        public string SambaPath { get; set; }
		
        /// <summary>
        /// Gets or Sets the Username to log in.
        /// </summary>
        public string Username { get; set; }
		
        /// <summary>
        /// Gets or Sets the Domain of Username.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or Sets the Password to log in.
        /// </summary>
        public string Password { get; set; }

    }
}
