using System;

namespace baimp
{
	public class ScanDataEventArgs : EventArgs
	{
		private bool saved;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanDataEventArgs"/> class.
		/// </summary>
		/// <param name="saved"><c>true</c> symbolizes that changed data was saved.</param>
		public ScanDataEventArgs (bool saved = false)
		{
			this.saved = saved;
		}

		/// <summary>
		/// Gets or sets the X coordinate of the mouse cursor
		/// </summary>
		/// <value>The x.</value>
		public bool Saved { 
			get {
				return saved;
			}
		}
	}
}

