using System;
using System.IO;
using System.Collections.Generic;

namespace bachelorarbeit_implementierung
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
	}
}
