using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Baimp
{
	public class ScanCollection : List<BaseScan>
	{

		public void AddFiles(List<string> files, Type importerType, bool reimport = true)
		{
			foreach (String file in files) {
				// parse scan metadata
				BaseScan instance = Activator.CreateInstance(importerType) as BaseScan;

				if(instance == null) {
					// TODO error handling
					break;
				}

				var localFile = file;
				BaseScan otherInstance = this.Find(f => f.FilePath == localFile);
				if (otherInstance == null) {
					instance.Initialize(file, reimport);
					Add(instance);

					int i = 1;
					while (this.Find(f => f != instance && f.Name == instance.Name) != null) {
						string[] splitted = instance.Name.Split('_');
						string partname = instance.Name;
						if (splitted.Length > 1) {
							int index = instance.Name.LastIndexOf('_');
							partname = instance.Name.Remove(index, partname.Length - index); 
						}

						instance.Name = partname + "_" + i;
						i++;
					}
				}

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

