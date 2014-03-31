using System;
using Xwt;
using System.Collections.Generic;

namespace Baimp
{
	public class FileTreeView : TreeView
	{
		public DataField<string> nameCol = new DataField<string>();
		public DataField<string> typeCol = new DataField<string>();
		public DataField<string> saveStateCol = new DataField<string>();
		public TreeStore store;

		private Dictionary<string, TreePosition> fiberTypeNodes;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.FileTreeView"/> class.
		/// </summary>
		public FileTreeView()
		{
			store = new TreeStore(nameCol, typeCol, saveStateCol);

//			this.SelectionMode = SelectionMode.Multiple;
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		public void InitializeUI()
		{
			this.Columns.Add("Name", nameCol).CanResize = true;
			this.Columns.Add("Fiber Type", typeCol).CanResize = true;
			this.Columns.Add("*", saveStateCol).CanResize = true;

			this.Columns[0].SortDataField = nameCol;
			this.Columns[1].SortDataField = typeCol;
			this.Columns[2].SortDataField = saveStateCol;

			this.DataSource = store;


			if (MainClass.toolkitType == ToolkitType.Gtk) {
				this.MinWidth = this.ParentWindow.Width;
			}
		}

		#endregion

		/// <summary>
		/// Reloads file tree information.
		/// </summary>
		/// <param name="scans">Collection of loaded scans</param>
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
					.SetValue(nameCol, scan.ToString())
					.SetValue(typeCol, scan.FiberType)
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

			TreePosition parentNodePosition;
			if (fiberTypeNodes.ContainsKey(scan.FiberType)) {
				parentNodePosition = fiberTypeNodes[scan.FiberType]; 
			} else {
				parentNodePosition = store.AddNode(null).SetValue(nameCol, scan.FiberType).CurrentPosition;
				fiberTypeNodes[scan.FiberType] = parentNodePosition;

			}

			scan.position = store.AddNode(parentNodePosition)
				.SetValue(nameCol, scan.ToString())
				.SetValue(typeCol, scan.FiberType)
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
				Refresh(scan);
			}
		}

		/// <summary>
		/// Raised when data changed
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

