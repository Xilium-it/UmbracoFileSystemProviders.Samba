using System.Runtime.InteropServices;
using Our.Umbraco.FileSystemProviders.Samba.Net;
using ResourceScope = Our.Umbraco.FileSystemProviders.Samba.Net.ResourceScope;
using ResourceType = Our.Umbraco.FileSystemProviders.Samba.Net.ResourceType;

namespace Our.Umbraco.FileSystemProviders.Samba.Net
{

	[StructLayout(LayoutKind.Sequential)]
	public class NetResource
	{
		public ResourceScope Scope;
		public ResourceType ResourceType;
		public ResourceDisplaytype DisplayType;
		public int Usage;
		public string LocalName;
		public string RemoteName;
		public string Comment;
		public string Provider;
	}
}
