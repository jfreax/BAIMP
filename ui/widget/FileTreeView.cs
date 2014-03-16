using System;
using Xwt;

namespace bachelorarbeit_implementierung
{
	public class FileTreeView : TreeView
	{
		private DataField<object> nameCol;
		private TreeStore store;
		//private ScanCollection scans;

		private Preview preview;


		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.FileTreeView"/> class.
		/// </summary>
		/// <param name="scans">Collection of all open scans</param>
		/// <param name="preview">Reference to preview widget</param>
		public FileTreeView (ScanCollection scans, Preview preview)
		{
			//this.scans = scans;
			this.preview = preview;

			nameCol = new DataField<object> ();
			store = new TreeStore (nameCol);

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

			InitializeEvents ();

			this.DataSource = store;
			this.ExpandAll ();
			this.SelectRow (pos);
		}


		/// <summary>
		/// Initializes all event handlers.
		/// </summary>
		private void InitializeEvents()
		{
			this.SelectionChanged += delegate(object sender, EventArgs e) {
				if(this.SelectedRow != null) {
					object value = store.GetNavigatorAt (this.SelectedRow).GetValue (nameCol);
					if( value is ScanWrapper ) {
						Scan s = (ScanWrapper)value;

						preview.ShowPreviewOf(s);
					}

				}
			};
		}
	}
}

