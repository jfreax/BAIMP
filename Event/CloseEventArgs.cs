using System;

namespace Baimp
{
	public class CloseEventArgs : EventArgs
	{
		public CloseEventArgs()
		{
			Close = true;
		}

		public bool Close {
			get;
			set;
		}
	}
}

