using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using Our.Umbraco.FileSystemProviders.Samba.Net;

namespace Our.Umbraco.FileSystemProviders.Samba.Net
{
	/// <summary>
	/// Permette di creare un ambito con credenziali di accesso specifiche.
	/// Sorgente: http://stackoverflow.com/a/1197430/1387407
	/// Esempio d'uso: http://stackoverflow.com/a/295703/1387407
	/// </summary>
	public class NetworkConnection : IDisposable {
		private string _networkName;
		private string _userName;
		private string _domain;

		public NetworkConnection(string networkName, NetworkCredential credentials)
		{
			_networkName = networkName;
			_userName = credentials.UserName;
			_domain = credentials.Domain;

			var netResource = new NetResource() {
				Scope = ResourceScope.GlobalNetwork,
				ResourceType = ResourceType.Disk,
				DisplayType = ResourceDisplaytype.Share,
				RemoteName = networkName
			};

			var userName = string.IsNullOrEmpty(credentials.Domain)
				? credentials.UserName
				: string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

			var result = WNetAddConnection2(
				netResource,
				credentials.Password,
				userName,
				0);

			if (result != 0) {
				throw new Win32Exception(result, "Error connecting to remote share");
			}
		}

		public string NetworkName
		{
			get { return this._networkName; }
		}

		public string UserName
		{
			get { return this._userName; }
		}

		public string Domain
		{
			get { return this._domain; }
		}

		~NetworkConnection() {
			this.Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			WNetCancelConnection2(_networkName, 0, true);
		}

		[DllImport("mpr.dll")]
		private static extern int WNetAddConnection2(NetResource netResource,
			string password, string username, int flags);

		[DllImport("mpr.dll")]
		private static extern int WNetCancelConnection2(string name, int flags,
			bool force);
	}
}