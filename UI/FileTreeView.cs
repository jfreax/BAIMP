using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace Baimp
{
	public class FileTreeView : TreeView
	{
		public DataField<Image> thumbnailCol = new DataField<Image>();

		public DataField<string> nameCol = new DataField<string>();
		public DataField<string> saveStateCol = new DataField<string>();
		public TreeStore store;

		private Dictionary<string, TreePosition> fiberTypeNodes;

		#region initialize

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.FileTreeView"/> class.
		/// </summary>
		public FileTreeView()
		{
			store = new TreeStore(thumbnailCol, nameCol, saveStateCol);

			this.SelectionMode = SelectionMode.Multiple;

			this.BoundsChanged += InitializeSize;
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

			this.DataSource = store;
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
				.SetValue(saveStateCol, "*").CurrentPosition;

			this.ExpandToRow(scan.position);

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
			Thread imageLoaderThread = new Thread(delegate() {
				List<BaseScan> scansCopy = new List<BaseScan>(scans);
				foreach (BaseScan scan in scansCopy) {
					var lScan = scan;
					Image image = Project.RequestZipAccess(new Project.ZipUsageCallback(delegate(ZipFile zipFile) {
						if(zipFile != null) {
							ZipEntry maskEntry = zipFile.GetEntry(String.Format("thumbnails/{0}.png", lScan.Name));
							if(maskEntry != null) {
								Stream previewStream = zipFile.GetInputStream(maskEntry);
								return Image.FromStream(previewStream);
							}
						}

						Image newImage = lScan.GetAsImage(lScan.AvailableScanTypes()[0], false);

						BitmapImage newRenderedImage = newImage.WithBoxSize(48).ToBitmap();
						newImage.Dispose();

						MemoryStream mStream = new MemoryStream();
						newRenderedImage.Save(mStream, ImageFileType.Png);
						mStream.Position = 0;
						CustomStaticDataSource source = new CustomStaticDataSource(mStream);

						if(zipFile != null) {
							zipFile.BeginUpdate();
							zipFile.Add(source, String.Format("thumbnails/{0}.png", lScan.Name));
							zipFile.IsStreamOwner = true;
							zipFile.CommitUpdate();
						}

						return newRenderedImage;

					})) as Image;

					Application.Invoke(
						() => store.GetNavigatorAt(lScan.position).SetValue(thumbnailCol, image)
					);
				}
			});
			imageLoaderThread.Start();
		}

		#region events

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

		#endregion
	}
}

