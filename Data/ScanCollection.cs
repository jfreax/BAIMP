using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Baimp
{
	public class ScanCollection : List<BaseScan>
	{

		#region initialize

		public ScanCollection() : base()
		{
		}

		/// <summary>
		/// Copy constructur
		/// </summary>
		/// <param name="scans">Scans.</param>
		public ScanCollection(ScanCollection scans)
		{
			AddRange(scans);
		}

		#endregion

		public void AddFiles(List<string> files, Type importerType, bool reimport = true)
		{
			List<Tuple<BaseScan, string>> toInitialize = new List<Tuple<BaseScan, string>>();
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
					Add(instance);
					toInitialize.Add(new Tuple<BaseScan, string>(instance, file));
				}
			}

			foreach (var scanTuple in toInitialize) {
				BaseScan scan = scanTuple.Item1;
				string file = scanTuple.Item2;
				scan.Initialize(file, reimport);

				int i = 1;
				while (this.Find(f => f != scan && f.Name == scan.Name) != null) {
					string[] splitted = scan.Name.Split('_');
					string partname = scan.Name;
					if (splitted.Length > 1) {
						int index = scan.Name.LastIndexOf('_');
						partname = scan.Name.Remove(index, partname.Length - index); 
					}

					scan.Name = partname + "_" + i;
					i++;
				}
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

