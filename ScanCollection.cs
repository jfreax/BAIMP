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

			foreach (String file in files) {
				Scan scan = new Scan (file);
				scans [scan.FiberType].Add (scan);
			}
		}
	}
}

