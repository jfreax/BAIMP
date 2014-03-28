using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace baimp
{
	public class ScanCollection : List<BaseScan>
	{

		#region initialize

		public ScanCollection()
		{
		}

		#endregion

		public void AddFiles(List<string> files, Type importerType)
		{
			foreach (String file in files) {
				// parse scan metadata
				BaseScan instance = Activator.CreateInstance(importerType) as BaseScan;
				instance.Initialize(file);
				Add(instance);

				//scan.ScanDataChanged += fileTree.OnScanDataChanged;
			}
		}

		/// <summary>
		/// Saves all changes.
		/// </summary>
		public void SaveAll()
		{
			foreach (BaseScan scan in this) {
				//scan.Save();
			}
		}

		#region properties

		#endregion
	}
}

