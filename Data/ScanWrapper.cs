using System;
using Xwt;
using System.Xml.Serialization;

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
		[XmlIgnore]
		public TreePosition position;

		/// <summary>
		/// Position of category item in file tree view
		/// </summary>
		[XmlIgnore]
		public TreePosition parentPosition;

		public ScanWrapper()
		{
		}

		public ScanWrapper(string filePath) : base(filePath)
		{
		}
	}
}

