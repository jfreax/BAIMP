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

		/// <summary>
		/// Reference to scanview if only one scan if currently shown.
		/// </summary>
		ScanView scanView;

		/// <summary>
		/// List of all currently shown scan algorithms.
		/// </summary>
		List<BaseScan> currentScans;

		/// <summary>
		/// List of all currenntly shown fiber types.
		/// </summary>
		HashSet<string> currentFiberTypes = new HashSet<string>();

		MouseMover mouseMover = new MouseMover();
		HBox controller = new HBox();
		GridView gridView = new GridView(96.0, 10.0);

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.Preview"/> class.
		/// </summary>
		public Preview()
		{
			InitializeEvents();

			this.Spacing = 0.0;
			this.MinWidth = 320;
			this.MinHeight = 320;
			this.PackStart(controller, false, false);
			this.PackEnd(gridView, true);
		}

		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void ShowPreviewOf(BaseScan scan)
		{
			List<BaseScan> l = new List<BaseScan>();
			l.Add(scan);

			ShowPreviewOf(l);
		}

		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scans">List of scans with their matching thumbnail image.</param>
		public void ShowPreviewOf(List<BaseScan> scans)
		{
			if (scans == null || scans.Count == 0) {
				return;
			}

			List<string> scanTypes = new List<string>(scans[0].AvailableScanTypes());
			if (currentScans != null) {
				foreach (BaseScan cScan in currentScans) {
					cScan.ScanDataChanged -= OnScanDataChanged;
					scanTypes.Union(cScan.AvailableScanTypes());
				}
			}

			controller.Clear();
			foreach (string scanType in scanTypes) {
				if (currentFiberTypes.Count == 0) {
					currentFiberTypes.Add(scanType);
				}

				CheckBox cb = new CheckBox();
				cb.Label = scanType;

				if (currentFiberTypes.Contains(scanType)) {
					cb.Active = true;
				}

				cb.Toggled += delegate(object sender, EventArgs e) {
					if (cb.Active) {
						AddFibertypeToShow(cb.Label);
					} else {
						RemoveFibertypeToShow(cb.Label);
					}
				};
					
				controller.PackStart(cb);
			}

			// set new list of scans
			currentScans = scans;

			// register new ones
			foreach (BaseScan cScan in currentScans) {
				cScan.ScanDataChanged += OnScanDataChanged;
			}

			bool isOnlyOne = currentScans.Count == 1 && currentFiberTypes.Count == 1;

			gridView.Clear();
			List<Widget> widgets = new List<Widget>();
			foreach (BaseScan scan in scans) {
				if (!scan.IsScaled() && this.Size.Width > 10) {
					scan.RequestedBitmapSize = this.Size;
				}

				foreach (string type in currentFiberTypes) {
					ScanView lScanView = new ScanView();

					if (isOnlyOne) {
						gridView.Add(lScanView);
						scanView = lScanView;
					} else {
						widgets.Add(lScanView);
					}

					lScanView.Initialize(scan, type);
					lScanView.IsThumbnail = !isOnlyOne;

					lScanView.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
						if (e.MultiplePress >= 2) {
							currentFiberTypes.Clear();
							currentFiberTypes.Add(lScanView.ScanType);
							ShowPreviewOf(lScanView.Scan);
						}
					};
				}
			}

			if (!isOnlyOne) {
				gridView.AddRange(widgets);
			}
		}

		public void AddFibertypeToShow(string newType)
		{
			currentFiberTypes.Add(newType);
			ShowPreviewOf(currentScans);
		}

		public void RemoveFibertypeToShow(string type)
		{
			currentFiberTypes.Remove(type);
			ShowPreviewOf(currentScans);
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

