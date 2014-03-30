using System;
using ICSharpCode.SharpZipLib.Zip;

namespace Baimp
{
	public class CustomStaticDataSource : IStaticDataSource
	{
		System.IO.Stream stream;

		public CustomStaticDataSource(System.IO.Stream stream)
		{
			this.stream = stream;
		}

		#region IStaticDataSource implementation

		public System.IO.Stream GetSource()
		{
			return stream;
		}

		#endregion
	}
}

