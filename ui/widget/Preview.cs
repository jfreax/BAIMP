using System;
using Xwt;
using System.IO;
using Xwt.Drawing;
using System.Threading;

namespace bachelorarbeit_implementierung
{
	public class Preview : VBox
	{
		public delegate void MyCallBack (ScanType type);

		ScrollView tab;
		ScanView scanView;
		Notebook notebook;

		ScanWrapper currentScan;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.Preview"/> class.
		/// </summary>
		public Preview ()
		{
			tab = new ScrollView ();
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
			this.PackEnd (tab, true, true);
		}

		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void ShowPreviewOf (ScanWrapper scan)
		{
			this.currentScan = scan;

			scanView = new ScanView (scan);

			scanView.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
				scanDataChanged(sender, e);
			};

			tab.Content = scanView;

			scanView.RegisterImageLoadedCallback (new MyCallBack (ImageLoadCallBack));
			scanView.MouseScrolled += delegate(object sender, MouseScrolledEventArgs e) {
				OnPreviewZoom (e);
			};

			ScanType type = (ScanType)notebook.CurrentTabIndex;
			ShowPreview (type);
		}

		private void ImageLoadCallBack (ScanType type)
		{
			if (!scanView.IsScaled()) {
				scanView.WithBoxSize (tab.Size);
			}
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

		/// <summary>
		/// Initializes all events.
		/// </summary>
		/// <param name="view">View.</param>
		private void InitializeEvents (ScrollView view)
		{

			view.BoundsChanged += delegate(object sender, EventArgs e) {
				scanView.WithBoxSize (tab.VisibleRect.Size);
			};

			view.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				switch (e.Button) {
				case PointerButton.Left:

					break;
				case PointerButton.Middle:
					if (scanView != null) {
						scanView.Data ["pressed"] = true;
						scanView.Data ["pressedPosition"] = e.Position;
					}
					break;
				}
			};

			view.ButtonReleased += delegate(object sender, ButtonEventArgs e) {
				switch (e.Button) {
				case PointerButton.Middle:
					if (scanView != null) {
						scanView.Data.Remove ("pressed");
					}
					break;
				}
			};

			view.MouseExited += delegate(object sender, EventArgs e) {
				if (scanView != null) {
					scanView.Data.Remove ("pressed");
				}
			};

			if (MainClass.toolkitType == ToolkitType.Gtk) {
				view.MouseMoved += MouseMovedGtk;
			} else {
				view.MouseMoved += MouseMovedNotGtk;
			}


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
		/// 
		/// </summary>
		/// <param name="sender">ScrollView.</param>
		/// <param name="e">Mouse event args.</param>
		private void MouseMovedNotGtk (object sender, MouseMovedEventArgs e)
		{
			if (scanView != null && scanView.Image != null) {
				tab.MouseMoved -= MouseMovedNotGtk;

				e.Handled = true;

				if (scanView.Data.ContainsKey ("pressed") &&
				    scanView.Data.ContainsKey ("pressedPosition") &&
				    scanView.Data ["pressedPosition"] != null) {

					Point oldPosition = (Point)scanView.Data ["pressedPosition"];

					double newScrollX = tab.HorizontalScrollControl.Value + oldPosition.X - e.Position.X;
					double newScrollY = tab.VerticalScrollControl.Value + oldPosition.Y - e.Position.Y;

					tab.HorizontalScrollControl.Value =
						Math.Min (tab.HorizontalScrollControl.UpperValue, newScrollX);
					tab.VerticalScrollControl.Value =
						Math.Min (tab.VerticalScrollControl.UpperValue, newScrollY);
				}

				scanView.Data ["pressedPosition"] = e.Position;
				tab.MouseMoved += MouseMovedNotGtk;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">ScrollView</param>
		/// <param name="e">Mouse event args.</param>
		private void MouseMovedGtk (object sender, MouseMovedEventArgs e)
		{
			if (scanView != null && scanView.Image != null) {
				tab.MouseMoved -= MouseMovedGtk;

				e.Handled = true;

				if (scanView.Data.ContainsKey ("pressed") &&
				    scanView.Data.ContainsKey ("pressedPosition") &&
				    scanView.Data ["pressedPosition"] != null) {

					Point oldPosition = (Point)scanView.Data ["pressedPosition"];

					double newScrollX = tab.HorizontalScrollControl.Value + oldPosition.X - e.Position.X;
					double newScrollY = tab.VerticalScrollControl.Value + oldPosition.Y - e.Position.Y;

					tab.HorizontalScrollControl.Value =
						Math.Min (tab.HorizontalScrollControl.UpperValue - tab.VisibleRect.Width, newScrollX);
					tab.VerticalScrollControl.Value =
						Math.Min (tab.VerticalScrollControl.UpperValue - tab.VisibleRect.Height, newScrollY);
				}

				tab.MouseMoved += MouseMovedGtk;
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
	}
}

