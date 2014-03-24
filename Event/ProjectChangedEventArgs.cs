using System;

namespace baimp
{
	public class ProjectChangedEventArgs : EventArgs
	{
		public string[] addedFiles;
		public bool refresh = false;

		public ProjectChangedEventArgs(string[] addedFiles)
		{
			this.addedFiles = addedFiles;
		}

		public ProjectChangedEventArgs(bool refresh)
		{
			this.refresh = refresh;
		}
	}
}

