using System;

namespace baimp
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

