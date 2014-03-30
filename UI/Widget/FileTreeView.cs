using System;
using Xwt;
using System.Collections.Generic;

namespace Baimp
{
	public class FileTreeView : TreeView
	{
		public DataField<object> nameCol;
		public DataField<object> saveStateCol;
		public TreeStore store;

		private Dictionary<string, TreePosition> fiberTypeNodes;

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

			TreePosition pos = null;
			fiberTypeNodes = new Dictionary<string, TreePosition>();
			foreach (BaseScan scan in scans) {
				TreePosition currentNode;
				if (fiberTypeNodes.ContainsKey(scan.FiberType)) {
					currentNode = fiberTypeNodes[scan.FiberType];
				} else {
					currentNode = store.AddNode(null).SetValue(nameCol, scan.FiberType).CurrentPosition;
					fiberTypeNodes[scan.FiberType] = currentNode;
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

				scan.ScanDataChanged += OnScanDataChanged;
			}

			this.ExpandAll();
			if (scans.Count > 0) {
				this.SelectRow(pos);
			}
		}

		private void Refresh(BaseScan scan)
		{
			store.GetNavigatorAt(scan.position).Remove();

			TreePosition parentNodePosition = null;
			if (fiberTypeNodes.ContainsKey(scan.FiberType)) {
				parentNodePosition = fiberTypeNodes[scan.FiberType]; 
			} else {
				parentNodePosition = store.AddNode(null).SetValue(nameCol, scan.FiberType).CurrentPosition;
				fiberTypeNodes[scan.FiberType] = parentNodePosition;

			}

			scan.position = store.AddNode(parentNodePosition)
				.SetValue(nameCol, scan)
				.SetValue(saveStateCol, "*").CurrentPosition;

			this.ExpandToRow(scan.position);

			if (this.DataSource.GetChildrenCount(scan.parentPosition) <= 0) {
				store.GetNavigatorAt(scan.parentPosition).Remove();
			}
			scan.parentPosition = parentNodePosition;
		}

		/// <summary>
		/// Gets called when a scan has changed
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments</param>
		private void OnScanDataChanged(object sender, ScanDataEventArgs e)
		{
			BaseScan scan = (BaseScan) sender;

			if (e.Changed.Equals("FiberType") && e.Unsaved.Contains("FiberType")) {
				//scans.Refresh(scan);
				Refresh(scan);
			}

			store.GetNavigatorAt(scan.position)
				.SetValue(
				saveStateCol, 
				e.Unsaved == null || e.Unsaved.Count == 0 ? "" : "*"
			);
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

