using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace baimp
{
	public class ScanCollection
	{
		[XmlArrayItem("scan")]
		public readonly List<ScanWrapper> data = new List<ScanWrapper>();

		#region initialize

		public ScanCollection()
		{
		}

		public ScanCollection(List<string> files)
		{
			if (files != null && files.Count > 0) {
				AddFiles(files);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanCollection"/> class.
		/// </summary>
		/// <param name="path">Path to load recursive.</param>
		public ScanCollection(string path)
		{
			string[] files = Directory.GetFiles(path, "*.dd+", SearchOption.AllDirectories);

			if (files.Length == 0) {
				throw new FileNotFoundException("No files found");
			}

			AddFiles(new List<string>(files));
		}

		#endregion

		public void AddFiles(List<string> files)
		{
			foreach (String file in files) {
				// parse scan metadata
				ScanWrapper scan = new ScanWrapper(file);

				data.Add(scan);

				//scan.ScanDataChanged += fileTree.OnScanDataChanged;
			}
		}

		/// <summary>
		/// Saves all changes.
		/// </summary>
		public void SaveAll()
		{
			foreach (ScanWrapper scan in data) {
				scan.Save();
			}
		}

		#region properties

		#endregion
	}
}

