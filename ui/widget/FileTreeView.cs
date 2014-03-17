using System;
using Xwt;

namespace bachelorarbeit_implementierung
{
	public class FileTreeView : TreeView
	{
		public DataField<object> nameCol;
		public TreeStore store;

		private ScanCollection scans;


		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.FileTreeView"/> class.
		/// </summary>
		/// <param name="scans">Collection of all open scans</param>
		/// <param name="preview">Reference to preview widget</param>
		public FileTreeView (ScanCollection scans)
		{
			this.scans = scans;

			nameCol = new DataField<object> ();
			store = new TreeStore (nameCol);
		}

		public void Initialize()
		{
			this.Columns.Add ("Name", nameCol);

			TreePosition pos = null;
			foreach (string key in scans.Keys)
			{
				var p = store.AddNode (null).SetValue (nameCol, key).CurrentPosition;

				foreach (ScanWrapper scan in scans[key]) {
					var v = store.AddNode (p).SetValue (nameCol, scan).CurrentPosition;
					scan.position = v;
					scan.parentPosition = p;
					if (pos == null) {
						pos = v;

					}
				}
			}

			this.DataSource = store;
			this.ExpandAll ();
			this.SelectRow (pos);
		}
	}
}

