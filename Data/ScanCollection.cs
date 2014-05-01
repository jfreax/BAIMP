//
//  ScanCollection.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Collections;

namespace Baimp
{
	public class ScanCollection : List<BaseScan>
	{

		#region initialize

		public ScanCollection() : base()
		{
		}

		/// <summary>
		/// Copy constructur.
		/// </summary>
		/// <param name="scans">Scans.</param>
		public ScanCollection(ScanCollection scans)
		{
			AddRange(scans);
		}

		/// <summary>
		/// Copy constructur.
		/// </summary>
		/// <param name="scans">List of scans.</param>
		public ScanCollection(List<BaseScan> scans)
		{
			AddRange(scans);
		}

		#endregion

		public new void AddRange(IEnumerable<BaseScan> range)
		{
			base.AddRange(range);

			if (filesChanged != null) {
				filesChanged(this, EventArgs.Empty);
			}
		}

		public new void Add(BaseScan scan)
		{
			base.Add(scan);

			if (filesChanged != null) {
				filesChanged(this, EventArgs.Empty);
			}
		}

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
				while (Find(f => f != scan && f.Name == scan.Name) != null) {
					string partname = scan.Name;
					int index = scan.Name.LastIndexOf('#');
					if (index > 0) {
						partname = scan.Name.Remove(index, partname.Length - index); 
					}

					scan.Name = partname + "#" + i;
					i++;
				}
			}

			if (filesChanged != null) {
				filesChanged(this, EventArgs.Empty);
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

		#region custom events

		EventHandler<EventArgs> filesChanged;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<EventArgs> FilesChanged {
			add {
				filesChanged += value;
			}
			remove {
				filesChanged -= value;
			}
		}

		#endregion
	}
}

