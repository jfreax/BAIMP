using System;
using Xwt;

namespace baimp
{
	public class FileTreeView : TreeView
	{
		public DataField<object> nameCol;
		public DataField<object> saveStateCol;
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
			saveStateCol = new DataField<object> ();
			store = new TreeStore (nameCol, saveStateCol);
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		public void InitializeUI()
		{
			this.Columns.Add ("Name", nameCol).CanResize = true;
			this.Columns.Add ("*", saveStateCol).CanResize = true;

			this.DataSource = store;

            if (MainClass.toolkitType == ToolkitType.Gtk) {
                this.MinWidth = this.ParentWindow.Width;
            }
		}


		/// <summary>
		/// Reloads file tree information.
		/// </summary>
		/// <param name="currentScan">Current focused scan</param>
		public void Reload(ScanWrapper currentScan = null) {
			store.Clear ();

			TreePosition pos = null;
			foreach (string key in scans.Keys) {
				var p = store.AddNode (null).SetValue (nameCol, key).CurrentPosition;

				foreach (ScanWrapper scan in scans[key]) {
					var v = store.AddNode (p)
						.SetValue (nameCol, scan)
						.SetValue (saveStateCol, scan.HasUnsaved() ? "*" : "" )
						.CurrentPosition;
					scan.position = v;
					scan.parentPosition = p;
					if (currentScan != null) {
						if (currentScan == scan) {
							pos = v;
						}
					} else {
						if (pos == null) {
							pos = v;
						}
					}
				}
			}

			this.ExpandAll ();
			this.SelectRow (pos);
		}
	}
}

