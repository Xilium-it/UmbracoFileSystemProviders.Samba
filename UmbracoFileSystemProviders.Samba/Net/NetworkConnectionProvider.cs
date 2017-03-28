using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.FileSystemProviders.Samba.Net {
	public class NetworkConnectionProvider
	{
		private static Lazy<NetworkConnectionProvider> __instance = new Lazy<NetworkConnectionProvider>(() => new NetworkConnectionProvider());
		private static object Locker = new object();

		private ConcurrentDictionary<string, NetworkConnectionData> _networkConnectionsData;

		private NetworkConnectionProvider()
		{	
			this._networkConnectionsData = new ConcurrentDictionary<string, NetworkConnectionData>();
		}

		public static NetworkConnectionProvider Current
		{
			get
			{
				return __instance.Value;
			}
		}

		public NetworkConnection GetNetworkConnection(string networkName, NetworkCredential credentials)
		{
			var connectionKey = this.CreateNetworkConnectionKey(networkName, credentials.UserName, credentials.Domain);
			
			lock (Locker)
			{
				var data = this._networkConnectionsData.GetOrAdd(connectionKey, (key) =>
				{
					return new NetworkConnectionData()
					{
						ActiveRequests = 0,
						NetworkConnection = new NetworkConnection(networkName, credentials)
					};
				});
				
				data.ActiveRequests++;
				System.Threading.Thread.MemoryBarrier();

				return data.NetworkConnection;
			}
		}

		public void CloseNetworkConnection(NetworkConnection networkConnection)
		{
			var connectionKey = this.CreateNetworkConnectionKey(networkConnection.NetworkName, networkConnection.UserName, networkConnection.Domain);

			lock (Locker)
			{
				NetworkConnectionData data;
				if (this._networkConnectionsData.TryGetValue(connectionKey, out data))
				{
					// Decrement `ActiveRequests`
					data.ActiveRequests--;
					if (data.ActiveRequests > 0) return;
					
					// If `ActiveRequests` is equal to 0 then it is ready to be removed by dictionary.
					data.NetworkConnection.Dispose();
					this._networkConnectionsData.TryRemove(connectionKey, out data);
				}
			}
		}
		
		private string CreateNetworkConnectionKey(string networkName, string userName, string domain)
		{
			return $"{userName.ToLowerInvariant()}@{domain.ToLowerInvariant()}:{networkName}";
		}

		private class NetworkConnectionData
		{
			public NetworkConnection NetworkConnection { get; set; }
			public int ActiveRequests { get; set; }
		}

	}
}
