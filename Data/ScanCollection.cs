using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace baimp
{
	public class ScanCollection : List<BaseScan>
	{

		Dictionary<string, int> allFileNames = new Dictionary<string, int>();

		#region initialize

		public ScanCollection()
		{
		}

		#endregion

		public void AddFiles(List<string> files, Type importerType, bool reimport = true)
		{
			foreach (String file in files) {
				// parse scan metadata
				BaseScan instance = Activator.CreateInstance(importerType) as BaseScan;
				instance.Initialize(file, reimport);

//				int i = 0;
//				while (this.Find(f => f.Name == instance.Name) != null) {
//					instance.Name = instance.Name + "_" + i;
//					i++;
//				}
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
				scan.Save();
			}
		}

		#region properties

		#endregion
	}
}

