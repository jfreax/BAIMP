﻿using System;
using System.Collections.Generic;

namespace Baimp
{
	public class ScanDataEventArgs : EventArgs
	{
		HashSet<string> unsaved;
		readonly string changed;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.ScanDataEventArgs"/> class.
		/// </summary>
		/// <param name="unsaved">List of unsaved elements</param>
		/// <param name="changed"></param>
		public ScanDataEventArgs(string changed, HashSet<string> unsaved = null)
		{
			this.changed = changed;
			this.unsaved = unsaved;
		}

		public HashSet<string> Unsaved { 
			get {
				return unsaved;
			}
		}

		public string Changed { 
			get {
				return changed;
			}
		}
	}
}

