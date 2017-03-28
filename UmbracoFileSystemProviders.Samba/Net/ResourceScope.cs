using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Our.Umbraco.FileSystemProviders.Samba.Net {
	
	public enum ResourceScope : int
	{
		Connected = 1,
		GlobalNetwork,
		Remembered,
		Recent,
		Context
	};

}
