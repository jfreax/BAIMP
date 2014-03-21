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

		#region Initialize

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
//                this.MinWidth = this.ParentWindow.Width;
				//TODO
            }
		}

		#endregion

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
			if (scans.Count > 0) {
				this.SelectRow (pos);
			}
		}

		/// <summary>
		/// Gets called when a scan has changed
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		public void OnScanDataChanged(object sender, ScanDataEventArgs e) {
			ScanWrapper scan = (ScanWrapper) sender;

			if(e.Changed.Equals("FiberType") && e.Unsaved.Contains("FiberType")) {
				scans.Refresh(scan);
				Reload(scan);
			}

			store.GetNavigatorAt (scan.position)
				.SetValue(
					saveStateCol, 
					e.Unsaved == null || e.Unsaved.Count == 0 ? "" : "*"
				);
		}
	}
}

