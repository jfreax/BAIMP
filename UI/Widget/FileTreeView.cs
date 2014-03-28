using System;
using Xwt;
using System.Collections.Generic;

namespace baimp
{
	public class FileTreeView : TreeView
	{
		public DataField<object> nameCol;
		public DataField<object> saveStateCol;
		public TreeStore store;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.FileTreeView"/> class.
		/// </summary>
		/// <param name="scans">Collection of all open scans</param>
		/// <param name="preview">Reference to preview widget</param>
		public FileTreeView()
		{
			nameCol = new DataField<object>();
			saveStateCol = new DataField<object>();
			store = new TreeStore(nameCol, saveStateCol);
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		public void InitializeUI()
		{
			this.Columns.Add("Name", nameCol).CanResize = true;
			this.Columns.Add("*", saveStateCol).CanResize = true;

			this.DataSource = store;

			if (MainClass.toolkitType == ToolkitType.Gtk) {
				this.MinWidth = this.ParentWindow.Width;
			}
		}

		#endregion

		/// <summary>
		/// Reloads file tree information.
		/// </summary>
		/// <param name="currentScan">Current focused scan</param>
		public void Reload(ScanCollection scans, BaseScan currentScan = null)
		{
			store.Clear();

//			foreach (string key in scans.Keys) {
//				foreach (ScanWrapper scan in scans[key]) {
//					scan.ScanDataChanged -= OnScanDataChanged;
//					scan.ScanDataChanged -= OnScanDataChanged;
//					scan.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
//						if (e.Unsaved != null && e.Unsaved.Count > 0) {
//							if (!this.Title.EndsWith("*")) {
//								this.Title += "*";
//							}
//						}
//					};
//				}
//			}

			TreePosition pos = null;
			Dictionary<string, TreePosition> treeTmp = new Dictionary<string, TreePosition>();
			foreach (BaseScan scan in scans) {
				TreePosition currentNode;
				if(treeTmp.ContainsKey(scan.FiberType)) {
					currentNode = treeTmp[scan.FiberType];
				} else {
					currentNode = store.AddNode(null).SetValue(nameCol, scan.FiberType).CurrentPosition;
					treeTmp[scan.FiberType] = currentNode;
				}

				var v = store.AddNode(currentNode)
					.SetValue(nameCol, scan)
					.SetValue(saveStateCol, scan.HasUnsaved() ? "*" : "")
					.CurrentPosition;
				scan.position = v;
				scan.parentPosition = currentNode;
				if (currentScan != null) {
					if (currentScan == scan) {
						pos = v;
					}
				} else {
					if (pos == null) {
						pos = v;
					}
				}

				scan.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
					TreeNavigator changedElem = store.GetNavigatorAt(scan.position);
					if (e.Unsaved != null) {
						Console.WriteLine(e.Changed);
						if (e.Unsaved.Count > 0) {
							changedElem.SetValue(saveStateCol, "*");
						} else {
							changedElem.SetValue(saveStateCol, "");
						}
					}
				};
			}

			this.ExpandAll();
			if (scans.Count > 0) {
				this.SelectRow(pos);
			}
		}

		/// <summary>
		/// Gets called when a scan has changed
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments</param>
		public void OnScanDataChanged(object sender, ScanDataEventArgs e)
		{
			BaseScan scan = (BaseScan) sender;

//			if (e.Changed.Equals("FiberType") && e.Unsaved.Contains("FiberType")) {
//				scans.Refresh(scan);
//				Reload(scan);
//			}
//
//			store.GetNavigatorAt(scan.position)
//				.SetValue(
//				saveStateCol, 
//				e.Unsaved == null || e.Unsaved.Count == 0 ? "" : "*"
//			);
		}

		/// <summary>
		/// Raised when current data changed
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event argument.</param>
		public void OnDataChanged(object sender, SaveStateEventArgs e)
		{
			if (e.saved) {
				store.GetNavigatorAt(this.SelectedRow).SetValue(saveStateCol, "");
			} else {
				store.GetNavigatorAt(this.SelectedRow).SetValue(saveStateCol, "*");
			}
		}
	}
}

