using System;

namespace baimp
{
	public class SaveStateEventArgs : EventArgs
	{
		public bool saved = false;

		public SaveStateEventArgs (bool saved)
		{
			this.saved = saved;
		}
	}
}

