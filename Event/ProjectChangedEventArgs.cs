using System;

namespace baimp
{
	public class ProjectChangedEventArgs : EventArgs
	{
		public string[] addedFiles;

		public ProjectChangedEventArgs (string[] addedFiles)
		{
			this.addedFiles = addedFiles;
		}
	}
}

