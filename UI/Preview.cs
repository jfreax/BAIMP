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

		Notebook notebook;

		/// <summary>
		/// Reference to scanview if only one scan if currently shown.
		/// </summary>
		ScanView scanView;

		/// <summary>
		/// List of all currently shown scan algorithms.
		/// </summary>
		List<BaseScan> currentScans;

		MouseMover mouseMover = new MouseMover();
		GridView gridView = new GridView(96.0);

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.Preview"/> class.
		/// </summary>
		public Preview()
		{
			InitializeEvents();

			this.Spacing = 0.0;
			this.MinWidth = 320;
			this.MinHeight = 320;
			this.PackEnd(gridView, true);
		}

		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		/// <param name="thumbnail">Optional thumbnail image to show before scan image is ready</param>
		public void ShowPreviewOf(BaseScan scan, Image thumbnail = null)
		{
			Dictionary<BaseScan, Image> dic = new Dictionary<BaseScan, Image>();
			dic[scan] = thumbnail;

			ShowPreviewOf(dic);
		}

		public void ShowPreviewOf(Dictionary<BaseScan, Image> scans)
		{
			// deregister old events
			if (currentScans != null) {
				foreach (BaseScan cScan in currentScans) {
					cScan.ScanDataChanged -= OnScanDataChanged;
				}
			}

			// set new list of scans
			currentScans = scans.Keys.ToList();

			// register new ones
			foreach (BaseScan cScan in currentScans) {
				cScan.ScanDataChanged += OnScanDataChanged;
			}

			bool isOnlyOne = currentScans.Count == 1;

			gridView.Clear();
			List<Widget> widgets = new List<Widget>();
			int i = 0;
			foreach (var scan in scans) {
				if (!scan.Key.IsScaled() && this.Size.Width > 10) {
					scan.Key.RequestedBitmapSize = this.Size;
				}

				ScanView lScanView = new ScanView(scan.Key, scan.Value);
				lScanView.IsThumbnail = !isOnlyOne;
				lScanView.ScanType = scan.Key.AvailableScanTypes()[0]; // TODO

				widgets.Add(lScanView);
				i++;
			}
			gridView.AddRange(widgets);

			if (isOnlyOne) {
				scanView = widgets[0] as ScanView;
			}
		}

		#region callbacks
	

		#endregion

		#region events

		/// <summary>
		/// Initializes all events.
		/// </summary>
		private void InitializeEvents()
		{
			mouseMover.RegisterMouseMover(gridView);

			gridView.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
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

			gridView.ButtonReleased += delegate(object sender, ButtonEventArgs e) {
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

			gridView.MouseExited += delegate(object sender, EventArgs e) {
				if (mouseMover.Enabled) {
					mouseMover.DisableMouseMover();
					if (scanView != null) {
						scanView.Cursor = (CursorType) scanView.data["oldMouseButton"];
					}
				}
			};

			gridView.MouseScrolled += (object sender, MouseScrolledEventArgs e) => OnPreviewZoom(e);
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
//					IEnumerable<NotebookTab> changedTab = 
//						notebook.Tabs.Where(t => t.Label.TrimEnd('*') == splitted[1]);
//
//					var first = changedTab.First();
//					if (first != null) {
//						first.Label = splitted[1] + (e != null && e.Unsaved.Contains(e.Changed) ? "*" : "");
//					}
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

