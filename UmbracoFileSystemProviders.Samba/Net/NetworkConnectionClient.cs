using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.FileSystemProviders.Samba.Net {
	public class NetworkConnectionClient : IDisposable
	{

		private readonly NetworkConnection _networkConnection;

		public NetworkConnectionClient(string networkName, NetworkCredential credentials)
		{
			this._networkConnection = NetworkConnectionProvider.Current.GetNetworkConnection(networkName, credentials);
		}

		public NetworkConnection NetworkConnection
		{
			get { return this._networkConnection; }
		}

		~NetworkConnectionClient()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			NetworkConnectionProvider.Current.CloseNetworkConnection(this.NetworkConnection);
		}
	}
}
