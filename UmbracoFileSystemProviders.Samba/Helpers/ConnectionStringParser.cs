namespace Our.Umbraco.FileSystemProviders.Samba.Helpers
{
	/// <summary>
    /// Parser to encode/decode authentication connection string.
    /// </summary>
    internal class ConnectionStringParser
    {
        /// <summary>
        /// Create a ConnectionString to log in.
        /// </summary>
        /// <param name="connectionStringData">The connectionString data to encode.</param>
        /// <returns>Encoded connection string</returns>
        public string Encode(ConnectionStringModel connectionStringData)
        {
            return this.Encode(connectionStringData.SambaPath, connectionStringData.Username, connectionStringData.Domain, connectionStringData.Password);
        }

        /// <summary>
        /// Create a ConnectionString to log in.
        /// </summary>
        /// <param name="sambaPath">The path of Samba shared folder; ex.: "\\server\folder"..</param>
        /// <param name="username">Username to log in.</param>
        /// <param name="domain">Domain of Username to log in.</param>
        /// <param name="password">Password to log in.</param>
        /// <returns>Encoded connection string</returns>
        public string Encode(string sambaPath, string username, string domain, string password)
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();

            builder.Add(Constants.ConnectionString.SambaPathKey, sambaPath);
            builder.Add(Constants.ConnectionString.UsernameKey, username);
            builder.Add(Constants.ConnectionString.DomainKey, domain);
            builder.Add(Constants.ConnectionString.PasswordKey, password);

            return builder.ConnectionString;
        }

        /// <summary>
        /// Decode a connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to decode.</param>
        /// <returns>The model of connection string.</returns>
        public ConnectionStringModel Decode(string connectionString)
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();
            var model = new ConnectionStringModel();

            builder.ConnectionString = connectionString;

            model.SambaPath = this.GetConnectionStringValue(builder, Constants.ConnectionString.SambaPathKey);
            model.Username = this.GetConnectionStringValue(builder, Constants.ConnectionString.UsernameKey);
            model.Domain = this.GetConnectionStringValue(builder, Constants.ConnectionString.DomainKey);
            model.Password = this.GetConnectionStringValue(builder, Constants.ConnectionString.PasswordKey);

            return model;
        }

        /// <summary>
        /// Get attribute value fom connection string builder.
        /// </summary>
        /// <param name="connectionStringBuilder">Builder where to read attribute.</param>
        /// <param name="attributeKey">Attribute to read.</param>
        /// <param name="fallbackValue">Fallback value where attribute does not exists.</param>
        /// <returns>Value of requested attribute.</returns>
        private string GetConnectionStringValue(System.Data.Common.DbConnectionStringBuilder connectionStringBuilder, string attributeKey, string fallbackValue = null)
        {
            object result;

            if (connectionStringBuilder.TryGetValue(attributeKey, out result) == false)
            {
                return fallbackValue;
            }

            return result.ToString();
        }
    }


}
