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
		double imageScale = 1.0;
		Scan currentScan;

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

			tab.BorderVisible = false;
			this.Margin = 0;
			this.PackEnd (tab, true, true);
		}

		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void ShowPreviewOf (Scan scan)
		{
			this.currentScan = scan;
			imageScale = 1.0;

			scanView = new ScanView (scan, (ScanType)notebook.CurrentTabIndex);
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
			if (imageScale == 1.0) {
				ResizeImageToFit ();
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
				ResizeImageToFit ();
			};

			view.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				//ScrollView sv = (ScrollView) sender;
				//ScanView scanView = (ScanView) sv.Content;

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
			if (e.Direction == ScrollDirection.Down) {
				imageScale *= 0.9;
			} else {
				imageScale *= 1.1;
			}

			if (scanView != null) {
				if (e.Direction == ScrollDirection.Down) {
					scanView.Scale (0.9);
				} else {
					scanView.Scale (1.1);
				}
			}

			e.Handled = true;
		}

		private void ResizeImageToFit ()
		{
			ScanView iv = (ScanView)tab.Content;

			if (iv != null && iv.Image != null) {

				double width = tab.VisibleRect.Width / iv.Image.Size.Width;
				double height = tab.VisibleRect.Height / iv.Image.Size.Height;

				imageScale *= Math.Min (width, height);
				if (scanView != null) {
					scanView.Scale (Math.Min (width, height));
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">ScrollView.</param>
		/// <param name="e">Mouse event args.</param>
		private void MouseMovedNotGtk (object sender, MouseMovedEventArgs e)
		{
			e.Handled = false;

			tab.MouseMoved -= MouseMovedNotGtk;

			if (scanView != null && scanView.Image != null) {
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
			}

			scanView.Data ["pressedPosition"] = e.Position;
			tab.MouseMoved += MouseMovedNotGtk;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">ScrollView</param>
		/// <param name="e">Mouse event args.</param>
		private void MouseMovedGtk (object sender, MouseMovedEventArgs e)
		{
			e.Handled = false;

			tab.MouseMoved -= MouseMovedGtk;

			//ScanView img = (ScanView)sc.Content;

			if (scanView != null && scanView.Image != null) {
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
			}

			tab.MouseMoved += MouseMovedGtk;
		}
	}
}

