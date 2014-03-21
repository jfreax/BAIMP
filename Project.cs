using System;

namespace baimp
{
	public class Project
	{
		public Project (string filePath)
		{
			this.FilePath = filePath;
		}

		#region Properties

		string FilePath {
			get;
			set;
		}

		#endregion
	}
}

