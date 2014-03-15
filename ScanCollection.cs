using System;
using System.IO;
using System.Collections.Generic;

namespace bachelorarbeit_implementierung
{
	public class ScanCollection
	{
		public Dictionary<string, List<Scan> > scans;


		public ScanCollection (string path)
		{
			string[] files = Directory.GetFiles(path, "*.dd+", SearchOption.AllDirectories);

			scans = new Dictionary<string, List<Scan> > ();

			int n = files.Length;
			int i = 0;
			foreach (String file in files) {
				// parse scan metadata
				Scan scan = new Scan (file);

				if (!scans.ContainsKey (scan.FiberType)) {
					scans [scan.FiberType] = new List<Scan> ();
				}

				scans [scan.FiberType].Add (scan);

				// increase progressbar
				//mainRef.progressBar.Value = (i * 100) / n;
				i++;
			}

			//mainRef.progressBar.Value = 100;
		}
	}
}

