using System;
using System.Linq;
using Xwt;
using System.IO;
using Xwt.Drawing;
using System.Threading;
using System.Collections.Generic;

namespace Baimp
{
	public class Preview : VBox
	{
		public delegate void MyCallBack(string scanType);

		ScrollView tab = new ScrollView();
		ScanView scanView;
		Notebook notebook;
		BaseScan currentScan;
		MouseMover mouseMover = new MouseMover();
		GridView gridView = new GridView(96.0);

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.Preview"/> class.
		/// </summary>
		public Preview()
		{
			InitializeEvents(tab);

			this.Spacing = 0.0;
			this.MinWidth = 320;
			this.MinHeight = 320;
			this.PackEnd(tab, true, true);
		}

		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		/// <param name="thumbnail">Optional thumbnail image to show before scan image is ready</param>
		public void ShowPreviewOf(BaseScan scan, Image thumbnail = null)
		{
			if (currentScan != null) {
				currentScan.ScanDataChanged -= OnScanDataChanged;
			}

			string currentTabLabel = string.Empty;
			if (notebook != null) {
				currentTabLabel = notebook.CurrentTab.Label.TrimEnd('*');
				this.Remove(notebook);
				notebook.Dispose();
			}

			notebook = new Notebook();
			this.PackStart(notebook, false, false);

			int lastTab = 0;
			int i = 0;
			foreach (string scanType in scan.AvailableScanTypes()) {
				if (!string.IsNullOrEmpty(currentTabLabel) && scanType == currentTabLabel) {
					lastTab = i;
				}
				notebook.Add(new FrameBox(), scanType);
				i++;
			}

			notebook.CurrentTabChanged += delegate(object sender, EventArgs e) {
				string scanType = notebook.CurrentTab.Label.TrimEnd('*');
				ShowPreview(scanType);
			};

			notebook.CurrentTabIndex = lastTab;

			this.currentScan = scan;
			scanView = new ScanView(scan);
			tab.Content = scanView;

			currentScan.ScanDataChanged += OnScanDataChanged;
				
			scanView.RegisterImageLoadedCallback(new MyCallBack(ImageLoadCallBack));
			scanView.MouseScrolled += (object sender, MouseScrolledEventArgs e) => OnPreviewZoom(e);

			string currentScanType = notebook.CurrentTab.Label;
			ShowPreview(currentScanType);

			// show thumbnail
			if (thumbnail != null) {
				scanView.Image = thumbnail.WithSize(scan.RequestedBitmapSize);
				ImageLoadCallBack(currentScanType); // sets the correct image size
			}
		}

		public void ShowPreviewOf(List<BaseScan> scans)
		{
			if (currentScan != null) {
				currentScan.ScanDataChanged -= OnScanDataChanged;
			}

			currentScan = null;

			gridView.Clear();
			tab.Content = gridView;

			List<Widget> widgets = new List<Widget>();
			foreach (BaseScan scan in scans) {
				ScanView lScanView = new ScanView(scan);
				lScanView.IsThumbnail = true;
				lScanView.ScanType = scan.AvailableScanTypes()[0];
				widgets.Add(lScanView);
			}
			gridView.AddRange(widgets);

		}

		/// <summary>
		/// Shows an specific preview into appropriate tab.
		/// </summary>
		/// <param name="scanType">Scan type</param>
		private void ShowPreview(string scanType)
		{
			if (scanView != null) {
				scanView.ScanType = scanType;
			}
		}

		#region callbacks

		/// <summary>
		/// Gets called when image is ready to display.
		/// </summary>
		/// <param name="scanType">Scan type</param>
		private void ImageLoadCallBack(string scanType)
		{
			if (!scanView.IsScaled()) {
				if (tab.VisibleRect.Width < 10) {
					scanView.WithBoxSize(tab.Size);
				} else {
					scanView.WithBoxSize(tab.VisibleRect.Size);
				}
			}
		}

		#endregion

		#region events

		/// <summary>
		/// Initializes all events.
		/// </summary>
		/// <param name="view">View.</param>
		private void InitializeEvents(ScrollView view)
		{
			mouseMover.RegisterMouseMover(tab);

			view.BoundsChanged += delegate(object sender, EventArgs e) {
				if (scanView != null)
					scanView.WithBoxSize(tab.VisibleRect.Size);
			};

			view.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				switch (e.Button) {
				case PointerButton.Left:

					break;
				case PointerButton.Middle:
					if (scanView != null) {
						mouseMover.EnableMouseMover(e.Position);

						if (scanView.Cursor != CursorType.Move) {
							scanView.data["oldMouseButton"] = scanView.Cursor;
							scanView.Cursor = CursorType.Move;
						}
					}
					break;
				}
			};

			view.ButtonReleased += delegate(object sender, ButtonEventArgs e) {
				switch (e.Button) {
				case PointerButton.Middle:
					if (mouseMover.Enabled) {
						mouseMover.DisableMouseMover();

						if (scanView != null) {
							scanView.Cursor = (CursorType) scanView.data["oldMouseButton"];
						}
					}
					break;
				}
			};

			view.MouseExited += delegate(object sender, EventArgs e) {
				if (mouseMover.Enabled) {
					mouseMover.DisableMouseMover();
					if (scanView != null) {
						scanView.Cursor = (CursorType) scanView.data["oldMouseButton"];
					}
				}
			};

			view.MouseScrolled += (object sender, MouseScrolledEventArgs e) => OnPreviewZoom(e);
		}

		/// <summary>
		/// Resize preview image on scrolling
		/// </summary>
		/// <param name="e">Event args</param>
		private void OnPreviewZoom(MouseScrolledEventArgs e)
		{
			if (scanView != null) {
				if (e.Direction == ScrollDirection.Down) {
					scanView.Scale(0.9);
				} else {
					scanView.Scale(1.1);
				}
			}

			e.Handled = true;
		}

		/// <summary>
		/// Gets called when current selected scan data changed.
		/// </summary>
		/// <param name="sender">Changed scan.</param>
		/// <param name="e">Event arguments.</param>
		private void OnScanDataChanged(object sender, ScanDataEventArgs e)
		{
			// propagate
			if (scanDataChanged != null) {
				scanDataChanged(sender, e);
			}
								
			if (e.Changed.StartsWith("mask_", StringComparison.Ordinal)) {
				string[] splitted = e.Changed.Split('_');
				if (splitted.Length >= 2) {
					IEnumerable<NotebookTab> changedTab = 
						notebook.Tabs.Where(t => t.Label.TrimEnd('*') == splitted[1]);

					var first = changedTab.First();
					if (first != null) {
						first.Label = splitted[1] + (e != null && e.Unsaved.Contains(e.Changed) ? "*" : "");
					}
				}
			}
		}

		EventHandler<ScanDataEventArgs> scanDataChanged;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<ScanDataEventArgs> ScanDataChanged {
			add {
				scanDataChanged += value;
			}
			remove {
				scanDataChanged -= value;
			}
		}

		#endregion
	}
}

