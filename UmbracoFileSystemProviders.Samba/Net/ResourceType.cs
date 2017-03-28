using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Our.Umbraco.FileSystemProviders.Samba.Net {

	public enum ResourceType : int
	{
		Any = 0,
		Disk = 1,
		Print = 2,
		Reserved = 8,
	}

}
