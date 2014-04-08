using System;
using System.Linq;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;

namespace Baimp
{
	public class FileTreeView : TreeView
	{
		ScanCollection scanCollection;
		ScanCollection scanCollectionUnfiltered;
		public DataField<Image> thumbnailCol = new DataField<Image>();
		public DataField<string> nameCol = new DataField<string>();
		public DataField<string> saveStateCol = new DataField<string>();
		public DataField<Image> thumbnailColFilter = new DataField<Image>();
		public DataField<string> nameColFilter = new DataField<string>();
		public DataField<string> saveStateColFilter = new DataField<string>();
		public TreeStore store;
		public TreeStore storeFilter;
		private Dictionary<string, TreePosition> filteredPositions = new Dictionary<string, TreePosition>();
		bool isFiltered = false;
		Menu contextMenu;
		private Dictionary<string, TreePosition> fiberTypeNodes;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.FileTreeView"/> class.
		/// </summary>
		public FileTreeView()
		{
			store = new TreeStore(thumbnailCol, nameCol, saveStateCol);
			storeFilter = new TreeStore(thumbnailColFilter, nameColFilter, saveStateColFilter);

			this.SelectionMode = SelectionMode.Multiple;
			this.BoundsChanged += InitializeSize;

			InitializeContextMenu();
		}

		private void InitializeSize(object sender, EventArgs e)
		{
			this.MinWidth = this.ScreenBounds.Width;
			this.HorizontalScrollPolicy = ScrollPolicy.Automatic;
			this.BoundsChanged -= InitializeSize;
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		public void InitializeUI()
		{
			this.Columns.Add("", thumbnailCol).CanResize = false;
			this.Columns.Add("Name", nameCol).CanResize = true;
			this.Columns.Add("*", saveStateCol).CanResize = true;

			this.Columns[0].SortDataField = nameCol;
			this.Columns[2].SortDataField = saveStateCol;
		}

		/// <summary>
		/// Initializes the context menu.
		/// </summary>
		private void InitializeContextMenu()
		{
			contextMenu = new Menu();
		}

		#endregion

		/// <summary>
		/// Filter
		/// </summary>
		/// <param name="text">Text.</param>
		public void Filter(string text)
		{
			string current = string.Empty;
			if (SelectedRow != null) {
				current = (DataSource as TreeStore).GetNavigatorAt(SelectedRow).GetValue(nameCol);
			}

			if (string.IsNullOrEmpty(text)) {
				this.DataSource = store;
				this.ExpandAll();
				isFiltered = false;
				return;
			}

			isFiltered = true;

			this.DataSource = storeFilter;
			storeFilter.Clear();
			filteredPositions.Clear();

			TreePosition selectedRow = null;
			TreeNavigator typeNode = store.GetFirstNode();
			do {
				var newTypeNode = storeFilter.AddNode(null)
						.SetValue(thumbnailColFilter, typeNode.GetValue(thumbnailCol));
				typeNode.MoveToChild();
				do {
					string name = typeNode.GetValue(nameCol);
					if (name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) {
						var newElem = storeFilter.AddNode(newTypeNode.CurrentPosition)
							.SetValue(thumbnailColFilter, typeNode.GetValue(thumbnailCol))
							.SetValue(nameColFilter, typeNode.GetValue(nameCol))
							.SetValue(saveStateColFilter, typeNode.GetValue(saveStateCol));
							
						string nameColValue = typeNode.GetValue(nameCol);
						filteredPositions[nameColValue] = newElem.CurrentPosition;
						if (nameColValue == current) {
							selectedRow = newElem.CurrentPosition;
						}
					}

				} while (typeNode.MoveNext());
				typeNode.MoveToParent();
			} while (typeNode.MoveNext());

			this.ExpandAll();

			if (selectedRow != null) {
				this.SelectRow(selectedRow);
			}
		}

		/// <summary>
		/// Reloads file tree information.
		/// </summary>
		/// <param name="scans">Collection of loaded scans</param>
		/// <param name="currentScan">Current focused scan</param>
		/// <param name="save">Update scan collection</param>
		public void Reload(ScanCollection scans, BaseScan currentScan = null, bool save = true)
		{
			if (save) {
				scanCollection = scans;
				scanCollectionUnfiltered = new ScanCollection(scans);
			}

			this.DataSource = store;
			store.Clear();

			TreePosition pos = null;
			fiberTypeNodes = new Dictionary<string, TreePosition>();
			foreach (BaseScan scan in scans) {
				TreePosition currentNode;
				if (fiberTypeNodes.ContainsKey(scan.FiberType)) {
					currentNode = fiberTypeNodes[scan.FiberType];
				} else {
					TextLayout text = new TextLayout();
					text.Text = scan.FiberType;
					ImageBuilder ib = new ImageBuilder(text.GetSize().Width, text.GetSize().Height);
					ib.Context.DrawTextLayout(text, Point.Zero);

					currentNode = store.AddNode(null).SetValue(thumbnailCol, ib.ToVectorImage()).CurrentPosition;
					fiberTypeNodes[scan.FiberType] = currentNode;
				}

				var v = store.AddNode(currentNode)
					.SetValue(nameCol, scan.ToString())
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

			LoadPreviewsAsync(scans);
		}

		/// <summary>
		/// Refresh the specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		private void Refresh(BaseScan scan)
		{
			Image thumbnail = store.GetNavigatorAt(scan.position).GetValue(thumbnailCol);
			store.GetNavigatorAt(scan.position).Remove();

			TreePosition parentNodePosition;
			if (fiberTypeNodes.ContainsKey(scan.FiberType)) {
				parentNodePosition = fiberTypeNodes[scan.FiberType]; 
			} else {
				TextLayout text = new TextLayout();
				text.Text = scan.FiberType;
				ImageBuilder ib = new ImageBuilder(text.GetSize().Width, text.GetSize().Height);
				ib.Context.DrawTextLayout(text, Point.Zero);

				parentNodePosition = store.AddNode(null).SetValue(thumbnailCol, ib.ToBitmap()).CurrentPosition;
				fiberTypeNodes[scan.FiberType] = parentNodePosition;
			}

			scan.position = store.AddNode(parentNodePosition)
				.SetValue(nameCol, scan.ToString())
				.SetValue(thumbnailCol, thumbnail)
				.SetValue(saveStateCol, "*").CurrentPosition;

			this.ExpandToRow(scan.position);
			this.SelectRow(scan.position);

			if (this.DataSource.GetChildrenCount(scan.parentPosition) <= 0) {
				store.GetNavigatorAt(scan.parentPosition).Remove();
			}
			scan.parentPosition = parentNodePosition;
		}

		/// <summary>
		/// Loads previews of all loaded file async.
		/// Show them in tree view.
		/// </summary>
		/// <param name="scans">Scans.</param>
		private void LoadPreviewsAsync(ScanCollection scans)
		{
			foreach (BaseScan scan in scans) {
				scan.isLoadingThumbnail = true;
			}

			ManagedThreadPool.QueueUserWorkItem(o => {
				List<BaseScan> scansCopy = new List<BaseScan>(scans);
				Project.RequestZipAccess(new Project.ZipUsageCallback(zipFile => {
					foreach (BaseScan scan in scansCopy) {
						var lScan = scan;
						Image[] thumbnails = lScan.GetThumbnails(zipFile);

						if (thumbnails.Length > 0 && thumbnails[0] != null) {
							Application.Invoke(() => {
								if (isFiltered) {
									string name = store.GetNavigatorAt(lScan.position).GetValue(nameCol);
									if (filteredPositions.ContainsKey(name)) {
										storeFilter.GetNavigatorAt(filteredPositions[name])
										.SetValue(thumbnailColFilter, thumbnails[0].WithBoxSize(48));
									}
								}

								store.GetNavigatorAt(lScan.position)
								.SetValue(thumbnailCol, thumbnails[0].WithBoxSize(48));

							});
						}
					}
					return null;
				}));

				foreach (BaseScan scan in scans) {
					scan.isLoadingThumbnail = false;
				}
			});
		}

		#region events

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			base.OnButtonPressed(args);

			if (scanCollection == null) {
				return;
			}

			switch (args.Button) {
			case PointerButton.Left:
				if (args.MultiplePress >= 2) {
					TreePosition selected = SelectedRow;
					if (selected != null) {
						string scanName = store.GetNavigatorAt(selected).GetValue(nameCol);
						Image thumbnail = store.GetNavigatorAt(selected).GetValue(thumbnailCol);
						BaseScan scan = scanCollection.Find(((BaseScan obj) => obj.Name == scanName));

						if (scan != null) {
							MetadataDialog metaDialog = new MetadataDialog(scan, thumbnail);
							Command r = metaDialog.Run();

							if (r.Id == Command.Apply.Id) {
								metaDialog.Save();
							}

							metaDialog.Dispose();

						}
					}
				}
				break;
			case PointerButton.Right:
				contextMenu.Items.Clear();
				TreeStore currentStore = DataSource as TreeStore;
				string currentFiberType;
				if (SelectedRow != null && currentStore != null) {
					TreeNavigator row = currentStore.GetNavigatorAt(SelectedRow);

					string name = row.GetValue(isFiltered ? nameColFilter : nameCol);
					currentFiberType = scanCollection.Find(o => o.Name == name).FiberType;
				
					foreach (string typeName in fiberTypeNodes.Keys) {
						RadioButtonMenuItem radioButton = new RadioButtonMenuItem(typeName);
						if (typeName == currentFiberType) {
							radioButton.Checked = true;
						}

						radioButton.Clicked += delegate(object sender, EventArgs e) {
							foreach (TreePosition x in SelectedRows) {
								RadioButtonMenuItem r = sender as RadioButtonMenuItem;
								if (r != null) {
									string n = currentStore.GetNavigatorAt(x)
										.GetValue(isFiltered ? nameColFilter : nameCol);
									scanCollection.Find(o => o.Name == n).FiberType = r.Label;
								}
							}
						};

						contextMenu.Items.Add(radioButton);
					}
					contextMenu.Popup();
				}
				break;
			}
		}

		protected override void OnKeyPressed(KeyEventArgs args)
		{
			base.OnKeyPressed(args);
			switch (args.Key) {
			case Key.Delete:
				TreeNavigator selected = store.GetNavigatorAt(SelectedRow);

				if (selected != null) {
					Dialog d = new Dialog();
					d.Title = "Remove this scan";
					VBox nameList = new VBox();

					foreach (TreePosition selectPos in SelectedRows) {
						nameList.PackStart(
							new Label(store.GetNavigatorAt(selectPos).GetValue(nameCol))
						);
					}

					d.Content = nameList;
					d.Buttons.Add(new DialogButton(Command.Delete));
					d.Buttons.Add(new DialogButton(Command.Cancel));

					Command r = d.Run();
					if (r != null && r.Id == Command.Delete.Id) {
						foreach (TreePosition selectPos in SelectedRows) {
							string name = store.GetNavigatorAt(selectPos).GetValue(nameCol);
							if (!string.IsNullOrEmpty(name)) {
								store.GetNavigatorAt(selectPos).Remove();

								scanCollection.RemoveAll(scan => scan.Name == name);
							}
						}
					}
					d.Dispose();
				}
				break;
			}
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
			if (e.Changed.Equals("Name") && e.Unsaved.Contains("Name")) {
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

		#endregion
	}
}

