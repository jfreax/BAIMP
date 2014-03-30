using System;

namespace Baimp
{
	public class ProgressEventArgs : EventArgs
	{
		public readonly int progress;

		public ProgressEventArgs(int progress)
		{
			this.progress = progress;
		}
	}
}

