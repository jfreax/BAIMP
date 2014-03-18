using System;
using System.Collections.Generic;

namespace baimp
{
	public class ScanDataEventArgs : EventArgs
	{
		private HashSet<string> unsaved;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanDataEventArgs"/> class.
		/// </summary>
		/// <param name="unsaved">List of unsaved elements/param>
		public ScanDataEventArgs (HashSet<string> unsaved = null)
		{
			this.unsaved = unsaved;
		}

		/// <summary>
		/// Gets or sets the X coordinate of the mouse cursor
		/// </summary>
		/// <value>The x.</value>
		public HashSet<string> Unsaved { 
			get {
				return unsaved;
			}
		}
	}
}

