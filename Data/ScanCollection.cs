using System;
using System.IO;
using System.Collections.Generic;

namespace baimp
{
	public class ScanCollection : Dictionary<string, List<ScanWrapper> >
	{
		#region initialize

		public ScanCollection()
		{
		}

		public ScanCollection(string[] files)
		{
			if (files != null && files.Length > 0) {
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

			AddFiles(files);
		}

		#endregion

		public void AddFiles(string[] files)
		{
			foreach (String file in files) {
				// parse scan metadata
				ScanWrapper scan = new ScanWrapper(file);

				if (!this.ContainsKey(scan.FiberType)) {
					this[scan.FiberType] = new List<ScanWrapper>();
				}

				this[scan.FiberType].Add(scan);
			}
		}

		/// <summary>
		/// Refresh specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void Refresh(ScanWrapper scan)
		{
			string oldKey = "";
			foreach (string key in this.Keys) {
				Scan s = this[key].Find(x => x == scan);
				if (s != null) {
					oldKey = key;
					break;
				}
			}

			if (!String.IsNullOrEmpty(oldKey)) {
				this[oldKey].Remove(scan);
				if (this[oldKey].Count == 0) {
					this.Remove(oldKey);
				}
			}

			if (!this.ContainsKey(scan.FiberType)) {
				this[scan.FiberType] = new List<ScanWrapper>();
			}

			this[scan.FiberType].Add(scan);
		}

		/// <summary>
		/// Saves all changes.
		/// </summary>
		public void SaveAll()
		{
			foreach (string key in this.Keys) {
				foreach (ScanWrapper scan in this[key]) {
					scan.Save();
				}
				
			}
		}
	}
}

