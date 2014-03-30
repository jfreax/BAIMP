using System;

namespace Baimp
{
	public class SaveStateEventArgs : EventArgs
	{
		public bool saved = false;

		public SaveStateEventArgs(bool saved)
		{
			this.saved = saved;
		}
	}
}

