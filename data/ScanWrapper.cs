using System;
using Xwt;

namespace baimp
{
	/// <summary>
	/// Extends the scan class, to hold extra information
	/// </summary>
	public class ScanWrapper : Scan
	{
		/// <summary>
		/// Position in file tree view
		/// </summary>
		public TreePosition position;

		/// <summary>
		/// Position of category item in file tree view
		/// </summary>
		public TreePosition parentPosition;


		public ScanWrapper(string filePath) : base(filePath) {}
	}
}

