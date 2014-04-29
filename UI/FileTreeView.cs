//
//  FileTreeView.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using System.Threading.Tasks;

namespace Baimp
{
	public class FileTreeView : TreeView
	{
		const int THUMB_SIZE = 32;

		ScanCollection scanCollection;
		public DataField<Image> thumbnailCol = new DataField<Image>();
		public DataField<Image> thumbnailColFilter = new DataField<Image>();
		public DataField<string> nameCol = new DataField<string>();
		public DataField<string> nameColFilter = new DataField<string>();
		public DataField<string> saveStateCol = new DataField<string>();
		public DataField<string> saveStateColFilter = new DataField<string>();
		public DataField<Image> finishCol = new DataField<Image>();
		public DataField<Image> finishColFiltered = new DataField<Image>();

		public TreeStore store;
		public TreeStore storeFilter;
		Dictionary<string, TreePosition> filteredPositions = new Dictionary<string, TreePosition>();
		bool isFiltered;
		Menu contextMenu;
		Menu contextMenuFibertype;
		Dictionary<string, TreePosition> fiberTypeNodes;

		Image tick = Image.FromResource("Baimp.Resources.tick.png");
		Image cross = Image.FromResource("Baimp.Resources.cross.png");

		string filterText;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.FileTreeView"/> class.
		/// </summary>
		public FileTreeView()
		{
			// center tick/cross symbols
			using (ImageBuilder ib = new ImageBuilder(THUMB_SIZE, THUMB_SIZE)) {
				ib.Context.DrawImage(tick, new Point((THUMB_SIZE - tick.Width) / 2, (THUMB_SIZE - tick.Height) / 2));
				Image tickNew = ib.ToBitmap();
				tick.Dispose();
				tick = tickNew;
			}
			using (ImageBuilder ib = new ImageBuilder(THUMB_SIZE, THUMB_SIZE)) {
				ib.Context.DrawImage(cross, new Point((THUMB_SIZE - cross.Width) / 2, (THUMB_SIZE - cross.Height) / 2));
				Image crossNew = ib.ToBitmap();
				cross.Dispose();
				cross = crossNew;
			}

			store = new TreeStore(thumbnailCol, nameCol, finishCol, saveStateCol);
			storeFilter = new TreeStore(thumbnailColFilter, nameColFilter, finishColFiltered, saveStateColFilter);

			SelectionMode = SelectionMode.Multiple;
			BoundsChanged += InitializeSize;

			InitializeContextMenu();
		}

		void InitializeSize(object sender, EventArgs e)
		{
			MinWidth = ScreenBounds.Width;
			HorizontalScrollPolicy = ScrollPolicy.Automatic;
			BoundsChanged -= InitializeSize;
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		public void InitializeUI()
		{
			Columns.Add("", thumbnailCol).CanResize = false;
			Columns.Add("Name", nameCol).CanResize = true;
			Columns.Add("X", finishCol).CanResize = true;
			Columns.Add("*", saveStateCol).CanResize = true;

			Columns[1].SortDataField = nameCol;
			Columns[2].SortDataField = finishCol;
			Columns[3].SortDataField = saveStateCol;
		}

		/// <summary>
		/// Initializes the context menu.
		/// </summary>
		void InitializeContextMenu()
		{
			contextMenu = new Menu();
			contextMenuFibertype = new Menu();

			contextMenu.Items.Add(new MenuItem { Label = "Fiber Type", SubMenu = contextMenuFibertype });

			MenuItem magnificationMenu = new MenuItem { Label = "Magnification..." };
			contextMenu.Items.Add(magnificationMenu);

			magnificationMenu.Clicked += delegate {
				Dialog d = new Dialog();
				d.Title = "Change magnification factor to...";
				d.Buttons.Add(new DialogButton(Command.Apply));
				d.Buttons.Add(new DialogButton(Command.Cancel));

				TextEntry newMagnification = new TextEntry { PlaceholderText = "Magnification factor" };
				d.Content = newMagnification;

				Command ret = d.Run();
				if (ret != null && ret.Id == Command.Apply.Id) {
					TreeStore currentStore = DataSource as TreeStore;
					foreach (TreePosition x in SelectedRows) {
						string n = currentStore.GetNavigatorAt(x)
							.GetValue(isFiltered ? nameColFilter : nameCol);

						BaseScan found = scanCollection.Find(o => o.Name == n);
						if (found != null) {
							try {
								found.Metadata["LensMagnification"] = float.Parse(newMagnification.Text);
							} catch (Exception e) {
								// TODO show error
								Console.WriteLine(e.Message);
								Console.WriteLine(e.StackTrace);
							}
						}
					}
				}

				d.Dispose();
			};
		}

		#endregion

		/// <summary>
		/// Filter
		/// </summary>
		/// <param name="text">Text.</param>
		public void Filter(string text)
		{
			filterText = text;
			string current = string.Empty;
			if (SelectedRow != null) {
				current = (DataSource as TreeStore).GetNavigatorAt(SelectedRow).GetValue(nameCol);
			}

			if (string.IsNullOrEmpty(text)) {
				this.DataSource = store;
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
							.SetValue(finishColFiltered, typeNode.GetValue(finishCol))
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
				
			if (selectedRow != null) {
				ExpandToRow(selectedRow);
				SelectRow(selectedRow);
				ScrollToRow(selectedRow);
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
			if (scans.Count > 0) {
				scans.Sort(scans[0]);
			}

			if (save) {
				scanCollection = scans;
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

					text.Dispose();
					ib.Dispose();
				}

				var v = store.AddNode(currentNode)
					.SetValue(nameCol, scan.ToString())
					.SetValue(finishCol, scan.IsFinish() ? tick : cross)
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

			if (scans.Count > 0) {
				ExpandToRow(pos);
				SelectRow(pos);
			}

			LoadPreviewsAsync(scans);
		}

		/// <summary>
		/// Refresh the specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		/// <param name="changedFiberType">Set to true, if the fibertype of the given scan has changed</param>
		void Refresh(BaseScan scan, bool changedFiberType = false)
		{
			Image thumbnail = store.GetNavigatorAt(scan.position).GetValue(thumbnailCol);
			TreePosition currentNode = scan.position;

			if (changedFiberType) {
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

					text.Dispose();
					ib.Dispose();

				}
				store.GetNavigatorAt(currentNode).Remove();
				scan.position = currentNode = store.AddNode(parentNodePosition).CurrentPosition;
				
				ExpandToRow(scan.position);
				ScrollToRow(scan.position);
				SelectRow(scan.position);

				scan.parentPosition = parentNodePosition;
			}

			store.GetNavigatorAt(currentNode)
				.SetValue(nameCol, scan.ToString())
				.SetValue(thumbnailCol, thumbnail)
				.SetValue(finishCol, scan.IsFinish() ? tick : cross)
				.SetValue(saveStateCol, scan.HasUnsaved() ? "*" : "");


			if (DataSource.GetChildrenCount(scan.parentPosition) <= 0) {
				store.GetNavigatorAt(scan.parentPosition).Remove();
			}

			if (!string.IsNullOrEmpty(filterText)) {
				Filter(filterText);
			}
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

			Task.Factory.StartNew(() => {
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
											.SetValue(thumbnailColFilter, thumbnails[0].WithBoxSize(int.MaxValue, THUMB_SIZE));
									}
								}

								store.GetNavigatorAt(lScan.position)
									.SetValue(thumbnailCol, thumbnails[0].WithBoxSize(int.MaxValue, THUMB_SIZE));
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
						string scanName = (DataSource as TreeStore).GetNavigatorAt(selected)
							.GetValue(isFiltered ? nameColFilter : nameCol);
						Image thumbnail = (DataSource as TreeStore).GetNavigatorAt(selected)
							.GetValue(isFiltered ? thumbnailColFilter : thumbnailCol);
						BaseScan scan = scanCollection.Find(((BaseScan obj) => obj.Name == scanName));

						if (scan != null) {
							MetadataDialog metaDialog = new MetadataDialog(scan, thumbnail);
							Command r = metaDialog.Run();

							if (r != null && r.Id == Command.Apply.Id) {
								metaDialog.Save();
							}

							metaDialog.Dispose();

						}
					}
				}
				break;
			case PointerButton.Right:
				contextMenuFibertype.Items.Clear();

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
							RadioButtonMenuItem r = sender as RadioButtonMenuItem;
							if (r == null) {
								return;
							}

							List<BaseScan> foundScan = new List<BaseScan>();
							foreach (TreePosition x in SelectedRows) {
								if (r != null) {
									string n = currentStore.GetNavigatorAt(x)
										.GetValue(isFiltered ? nameColFilter : nameCol);
									BaseScan found = scanCollection.Find(o => o.Name == n);
									if (found != null) {
										foundScan.Add(found);
									}
								}
							}

							foreach (BaseScan found in foundScan) {
								found.FiberType = r.Label;
							}
						};

						contextMenuFibertype.Items.Add(radioButton);
					}

					// other fibertype
					MenuItem otherFibertype = new MenuItem { Label = "Other..." };
					contextMenuFibertype.Items.Add(new SeparatorMenuItem());
					contextMenuFibertype.Items.Add(otherFibertype);
					otherFibertype.Clicked += delegate {
						Dialog d = new Dialog();
						d.Title = "Change fiber type to...";
						d.Buttons.Add(new DialogButton(Command.Apply));
						d.Buttons.Add(new DialogButton(Command.Cancel));

						TextEntry newFiberType = new TextEntry { PlaceholderText = "Fiber type name" };
						d.Content = newFiberType;

						Command ret = d.Run();
						if (ret != null && ret.Id == Command.Apply.Id) {
							List<BaseScan> foundScan = new List<BaseScan>();
							foreach (TreePosition x in SelectedRows) {
								string n = currentStore.GetNavigatorAt(x)
										.GetValue(isFiltered ? nameColFilter : nameCol);
								BaseScan found = scanCollection.Find(o => o.Name == n);
								if (found != null) {
									foundScan.Add(found);
								}
							}

							foreach (BaseScan found in foundScan) {
								found.FiberType = newFiberType.Text;
							}
						}

						d.Dispose();
					};

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
				TreeStore currentStore = DataSource as TreeStore;

				if (currentStore != null) {
					TreeNavigator selected = currentStore.GetNavigatorAt(SelectedRow);

					if (selected != null) {
						Dialog d = new Dialog();
						d.Title = "Remove this scan";

						VBox nameList = new VBox();
						ScrollView nameListScroll = new ScrollView(nameList);

						foreach (TreePosition selectPos in SelectedRows) {
							nameList.PackStart(
								new Label(currentStore.GetNavigatorAt(selectPos)
								.GetValue(isFiltered ? nameColFilter : nameCol))
							);
						}
						TextLayout text = new TextLayout();
						text.Text = "M";
						double textHeight = text.GetSize().Height;
						text.Dispose();

						nameListScroll.MinHeight = Math.Min(10, SelectedRows.Length) * textHeight;
						d.Content = nameListScroll;
						d.Buttons.Add(new DialogButton(Command.Delete));
						d.Buttons.Add(new DialogButton(Command.Cancel));

						Command r = d.Run();
						if (r != null && r.Id == Command.Delete.Id) {
							foreach (TreePosition selectPos in SelectedRows) {
								string name = currentStore.GetNavigatorAt(selectPos)
									.GetValue(isFiltered ? nameColFilter : nameCol);
								if (!string.IsNullOrEmpty(name)) {
									currentStore.GetNavigatorAt(selectPos).Remove();

									scanCollection.RemoveAll(scan => scan.Name == name);
								}
							}
						}
						d.Dispose();
					}
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
				Refresh(scan, true);
			}

			if (e.Changed.Equals("Name") && e.Unsaved.Contains("Name")) {
				Refresh(scan);
			}

			if (e.Changed.Equals("mask") && e.Unsaved.Contains("mask")) {
				Refresh(scan);
			}

			if (e.Unsaved.Count == 0) {
				Refresh(scan, e.Changed.Equals("FiberType"));
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

		public bool IsFiltered {
			get {
				return isFiltered;
			}
		}
	}
}

