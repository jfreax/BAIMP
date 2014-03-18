using System;
using System.IO;
using System.Collections.Generic;

namespace baimp
{
	public class ScanCollection : Dictionary<string, List<ScanWrapper> >
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanCollection"/> class.
		/// </summary>
		/// <param name="path">Path.</param>
		public ScanCollection (string path)
		{
			string[] files = Directory.GetFiles(path, "*.dd+", SearchOption.AllDirectories);

			int n = files.Length;
			foreach (String file in files) {
				// parse scan metadata
				ScanWrapper scan = new ScanWrapper (file);

				if (!this.ContainsKey (scan.FiberType)) {
					this [scan.FiberType] = new List<ScanWrapper> ();
				}

				this [scan.FiberType].Add (scan);
			}
		}

		/// <summary>
		/// Refresh specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void Refresh(ScanWrapper scan) {
			string oldKey = "";
			foreach (string key in this.Keys) {
				Scan s = this [key].Find (x => x == scan);
				if (s != null) {
					oldKey = key;
					break;
				}
			}

			if(!String.IsNullOrEmpty(oldKey)) {
				this [oldKey].Remove (scan);
			}

			if (!this.ContainsKey (scan.FiberType)) {
				this [scan.FiberType] = new List<ScanWrapper> ();
			}

			this [scan.FiberType].Add (scan);
		}
	}
}

