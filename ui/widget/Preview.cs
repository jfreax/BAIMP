using System;
using Xwt;
using System.IO;
using Xwt.Drawing;

namespace bachelorarbeit_implementierung
{
	public class Preview : Notebook
	{
		ScrollView[] tabs;
		Image[] images;

		Scan currentScan;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.Preview"/> class.
		/// </summary>
		public Preview ()
		{
			tabs = new ScrollView[(int)ScanType.Metadata];
			images = new Image[(int)ScanType.Metadata];

			for (int i = 0; i < (int)ScanType.Metadata; i++) {
				ScanView img = new ScanView ();
				tabs[i] = new ScrollView(img);
				InitializeEvents (tabs [i]);

				Add (tabs [i], Enum.GetName(typeof(ScanType), i));
			}
		}


		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void ShowPreviewOf(Scan scan) {
			this.currentScan = scan;

			for(int i = 0; i < images.Length; i++) {
				if (images[i] != null) {
					images[i].Dispose ();
					images[i] = null;
				}
			}

			this.LoadPreview ((ScanType) this.CurrentTabIndex);
		}


		/// <summary>
		/// Loads an specific preview into appropriate tab.
		/// </summary>
		/// <param name="type">Type.</param>
		private void LoadPreview(ScanType type) {
			if (images [(int)type] == null && currentScan != null) {

				MemoryStream memoryStream = new MemoryStream ();
				currentScan.GetAsBitmap (type)
					.Save (memoryStream, System.Drawing.Imaging.ImageFormat.Png);
				memoryStream.Position = 0;

				ScrollView tab = tabs [(int)type];
				ScanView scanView = (ScanView)tab.Content;

				scanView.Image = Image.FromStream (memoryStream);
				images [(int)type] = scanView.Image;

				ResizeImageToFit (tab);
				scanView.MouseScrolled += OnPreviewScroll;
			}
		}


		/// <summary>
		/// Initializes all events.
		/// </summary>
		/// <param name="view">View.</param>
		private void InitializeEvents(ScrollView view) {

			view.BoundsChanged += delegate(object sender, EventArgs e) {
				ResizeImageToFit((ScrollView) sender);
			};

			view.ButtonPressed += delegate(object sender, ButtonEventArgs e)
			{
				ScrollView sv = (ScrollView) sender;
				ScanView scanView = (ScanView) sv.Content;

				switch(e.Button) {
				case PointerButton.Middle:
					if(scanView != null) {
						scanView.Data["pressed"] = true;
						scanView.Data["pressedPosition"] = e.Position;
					}
					break;
				}
			};

			view.ButtonReleased += delegate(object sender, ButtonEventArgs e)
			{
				ScrollView sv = (ScrollView) sender;
				ScanView scanView = (ScanView) sv.Content;

				switch(e.Button) {
				case PointerButton.Middle:
					if(scanView != null) {
						scanView.Data.Remove("pressed");
					}
					break;
				}
			};

			view.MouseExited += delegate(object sender, EventArgs e)
			{
				ScrollView sv = (ScrollView) sender;
				ScanView scanView = (ScanView) sv.Content;

				if(scanView != null) {
					scanView.Data.Remove("pressed");
				}
			};

			if (MainClass.toolkitType == ToolkitType.Gtk) {
				view.MouseMoved += MouseMovedGtk;
			} else {
				view.MouseMoved += MouseMovedNotGtk;
			}


			//img.MouseScrolled += OnPreviewScroll;
			view.MouseScrolled += delegate(object sender, MouseScrolledEventArgs e)
			{
				ScrollView sv = (ScrollView) sender;
				ScanView scanView = (ScanView) sv.Content;

				if(scanView != null) {
					OnPreviewScroll((object) scanView, e);
				}
			};
		}


		/// <summary>
		/// Resize preview image on scrolling
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args</param>
		private void OnPreviewScroll(object sender, MouseScrolledEventArgs e) 
		{
			ImageView iv = (ImageView)sender;
			ScrollView sv = (ScrollView) iv.Parent;

			if (e.Direction == ScrollDirection.Down) {
				iv.Image = iv.Image.Scale (0.90);
			} else {
				iv.Image = iv.Image.Scale (1.10);
			}

			e.Handled = true;
		}


		void ResizeImageToFit(ScrollView sv) {
			ImageView iv = (ImageView) sv.Content;

			if(iv != null && iv.Image != null) {

				double width = sv.VisibleRect.Width / iv.Image.Size.Width;
				double height = sv.VisibleRect.Height / iv.Image.Size.Height;

				iv.Image = iv.Image.Scale( Math.Min(width, height) );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">ScrollView.</param>
		/// <param name="e">Mouse event args.</param>
		void MouseMovedNotGtk (object sender, MouseMovedEventArgs e) {
			e.Handled = false;

			ScrollView sc = (ScrollView)sender;
			sc.MouseMoved -= MouseMovedNotGtk;

			ScanView img = (ScanView)sc.Content;

			if (img != null && img.Image != null) {
				if (img.Data.ContainsKey ("pressed") &&
				    img.Data.ContainsKey ("pressedPosition") &&
				    img.Data ["pressedPosition"] != null) {

					Point oldPosition = (Point)img.Data ["pressedPosition"];

					double newScrollX = sc.HorizontalScrollControl.Value + oldPosition.X - e.Position.X;
					double newScrollY = sc.VerticalScrollControl.Value + oldPosition.Y - e.Position.Y;

					sc.HorizontalScrollControl.Value =
					Math.Min (sc.HorizontalScrollControl.UpperValue, newScrollX);
					sc.VerticalScrollControl.Value =
					Math.Min (sc.VerticalScrollControl.UpperValue, newScrollY);
				}
			}

			img.Data["pressedPosition"] = e.Position;
			sc.MouseMoved += MouseMovedNotGtk;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">ScrollView</param>
		/// <param name="e">Mouse event args.</param>
		void MouseMovedGtk(object sender, MouseMovedEventArgs e)
		{
			e.Handled = false;

			ScrollView sc = (ScrollView)sender;
			sc.MouseMoved -= MouseMovedGtk;

			ScanView img = (ScanView)sc.Content;

			if (img != null && img.Image != null) {
				if (img.Data.ContainsKey ("pressed") &&
					img.Data.ContainsKey ("pressedPosition") &&
				    img.Data ["pressedPosition"] != null) {

					Point oldPosition = (Point)img.Data ["pressedPosition"];

					double newScrollX = sc.HorizontalScrollControl.Value + oldPosition.X - e.Position.X;
					double newScrollY = sc.VerticalScrollControl.Value + oldPosition.Y - e.Position.Y;

					sc.HorizontalScrollControl.Value =
					Math.Min (sc.HorizontalScrollControl.UpperValue - sc.VisibleRect.Width, newScrollX);
					sc.VerticalScrollControl.Value =
					Math.Min (sc.VerticalScrollControl.UpperValue - sc.VisibleRect.Height, newScrollY);
				}
			}

			sc.MouseMoved += MouseMovedGtk;
		}


		/// <summary>
		/// Raises the current tab changed event.
		/// </summary>
		/// <param name="e">Event args</param>
		protected override void OnCurrentTabChanged (EventArgs e) {
			base.OnCurrentTabChanged (e);

			LoadPreview((ScanType)this.CurrentTabIndex);
		}
	}
}

