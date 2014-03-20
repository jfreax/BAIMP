using System;
using Xwt;
using System.IO;
using Xwt.Drawing;
using System.Threading;

namespace baimp
{
	public class Preview : VBox
	{
		public delegate void MyCallBack (ScanType type);

		ScrollView tab;
		ScanView scanView;
		Notebook notebook;

		MouseMover mouseMover;

		ScanWrapper currentScan;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.Preview"/> class.
		/// </summary>
		public Preview ()
		{
			tab = new ScrollView ();
			mouseMover = new MouseMover ();
			InitializeEvents (tab);

			notebook = new Notebook ();
			for (int i = 0; i < (int)ScanType.Metadata; i++) {
				notebook.Add (new FrameBox(), Enum.GetName (typeof(ScanType), i));
			}

			notebook.CurrentTabChanged += delegate(object sender, EventArgs e) {
				ScanType type = (ScanType)notebook.CurrentTabIndex;
				ShowPreview (type);
			};

			this.PackStart (notebook, false, false);

			this.Spacing = 0.0;
			this.MinWidth = 320;
			this.PackEnd (tab, true, true);
		}

		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void ShowPreviewOf (ScanWrapper scan)
		{
			if (currentScan != null) {
				currentScan.ScanDataChanged -= OnScanDataChanged;
			}

			this.currentScan = scan;
			scanView = new ScanView (scan);
			tab.Content = scanView;

			currentScan.ScanDataChanged += OnScanDataChanged;
				
			scanView.RegisterImageLoadedCallback (new MyCallBack (ImageLoadCallBack));
			scanView.MouseScrolled += delegate(object sender, MouseScrolledEventArgs e) {
				OnPreviewZoom (e);
			};

			ScanType type = (ScanType)notebook.CurrentTabIndex;
			ShowPreview (type);
		}

		/// <summary>
		/// Shows an specific preview into appropriate tab.
		/// </summary>
		/// <param name="type">Type.</param>
		private void ShowPreview (ScanType type)
		{

			if (scanView != null) {
				scanView.ScanType = type;
			}
		}

		#region callbacks

		/// <summary>
		/// Gets called when image is ready to display.
		/// </summary>
		/// <param name="type">Scan type</param>
		private void ImageLoadCallBack (ScanType type)
		{
			if (!scanView.IsScaled()) {
				if (tab.VisibleRect.Width < 10) {
					scanView.WithBoxSize (tab.Size);
				} else {
					scanView.WithBoxSize (tab.VisibleRect.Size);
				}
			}
		}

		#endregion

		#region events

		/// <summary>
		/// Initializes all events.
		/// </summary>
		/// <param name="view">View.</param>
		private void InitializeEvents (ScrollView view)
		{
			mouseMover.RegisterMouseMover (tab);

			view.BoundsChanged += delegate(object sender, EventArgs e) {
				scanView.WithBoxSize (tab.VisibleRect.Size);
			};

			view.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				switch (e.Button) {
				case PointerButton.Left:

					break;
				case PointerButton.Middle:
					if (scanView != null) {
						mouseMover.EnableMouseMover(e.Position);

						if(scanView.Cursor != CursorType.Move) {
							scanView.Data ["oldMouseButton"] = scanView.Cursor;
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

						if(scanView != null) {
							scanView.Cursor = (CursorType) scanView.Data["oldMouseButton"];
						}
					}
					break;
				}
			};

			view.MouseExited += delegate(object sender, EventArgs e) {
				if (mouseMover.Enabled) {
					mouseMover.DisableMouseMover();
					if(scanView != null) {
						scanView.Cursor = (CursorType) scanView.Data["oldMouseButton"];
					}
				}
			};

			view.MouseScrolled += delegate(object sender, MouseScrolledEventArgs e) {
				OnPreviewZoom (e);
			};
		}

		/// <summary>
		/// Resize preview image on scrolling
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args</param>
		private void OnPreviewZoom (MouseScrolledEventArgs e)
		{
			if (scanView != null) {
				if (e.Direction == ScrollDirection.Down) {
					scanView.Scale (0.9);
				} else {
					scanView.Scale (1.1);
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
				scanDataChanged (sender, e);
			}
				
			if (e.Changed.StartsWith ("mask_")) {
				string[] splitted = e.Changed.Split ('_');
				if (splitted.Length >= 2) {
					int index = Convert.ToInt32 (splitted [1]);

					if (notebook.Tabs.Count >= index) {
						notebook.Tabs [index].Label =
							Enum.GetName (typeof(ScanType), index) +
							(e != null && e.Unsaved.Contains ("mask_" + index) ? "*" : "");
					}
				}
			}
		}

		EventHandler<ScanDataEventArgs> scanDataChanged;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<ScanDataEventArgs> ScanDataChanged
		{
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

