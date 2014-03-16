using System;
using Xwt;
using System.IO;
using Xwt.Drawing;
using System.Threading;

namespace bachelorarbeit_implementierung
{
	public class Preview : Notebook
	{
		ScrollView[] tabs;

		Thread imageLoaderThread;

		double imageScale = 1.0;
		double vScroll = 0.0;
		double hScroll = 0.0;

        private object lock_image_loading = new object();

		Scan currentScan;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.Preview"/> class.
		/// </summary>
		public Preview ()
		{
			tabs = new ScrollView[(int)ScanType.Metadata];

			for (int i = 0; i < (int)ScanType.Metadata; i++) {
				ScanView img = new ScanView ();
				tabs[i] = new ScrollView(img);
				tabs [i].BorderVisible = false;
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

			for (int i = 0; i < tabs.Length; i++) {
				if (tabs [i].Content != null) {
					Image image = ((ScanView)tabs [i].Content).Image;
					if (image != null) {
						image.Dispose ();
						((ScanView)tabs [i].Content).Image = null;
					}
				}
			}
            ScanType type = (ScanType)this.CurrentTabIndex;
            Thread imageLoaderThread = new Thread(() => LoadPreview(scan, type));
			imageLoaderThread.Start ();
		}


		/// <summary>
		/// Loads an specific preview into appropriate tab.
		/// </summary>
		/// <param name="type">Type.</param>
		private void LoadPreview(Scan scan, ScanType type) {
            lock (this.lock_image_loading)
            {
                if (scan == null)
                {
                    return;
                }

                ScrollView tab = tabs[(int)type];
                ScanView scanView = (ScanView)tab.Content;

                if (scanView.Image != null)
                {
                    return;
                }

                MemoryStream memoryStream = new MemoryStream();
                System.Drawing.Bitmap bmp = scan.GetAsBitmap(type);
                if (bmp == null)
                {
                    Console.WriteLine("bmp == null " + (int)type);
                    // TODO raise error
                    return;
                }
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Tiff);

                memoryStream.Position = 0;

                Application.Invoke(delegate()
                {
                    scanView.Image = Image.FromStream(memoryStream);
                    scanView.Scale(imageScale);


                    // resize image to fit window, only on standard zoom!
                    if (imageScale == 1.0)
                    {
                        ResizeImageToFit(tab);
                    }

                    scanView.MouseScrolled += delegate(object sender, MouseScrolledEventArgs e)
                    {
                        OnPreviewZoom(e);
                    };

                    tab.HorizontalScrollControl.Value = hScroll;
                    tab.VerticalScrollControl.Value = vScroll;
                });
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
				case PointerButton.Left:

					break;
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

			view.HorizontalScrollControl.ValueChanged += delegate(object sender, EventArgs e) {
				ScrollControl sc = (ScrollControl) sender;

				hScroll = sc.Value;
				foreach (ScrollView s in tabs) {
					s.HorizontalScrollControl.Value = sc.Value;
				}
			};

			view.VerticalScrollControl.ValueChanged += delegate(object sender, EventArgs e) {
				ScrollControl sc = (ScrollControl) sender;

				vScroll = sc.Value;
				foreach (ScrollView s in tabs) {
					s.VerticalScrollControl.Value = sc.Value;
				}
			};

			view.MouseScrolled += delegate(object sender, MouseScrolledEventArgs e)
			{
				OnPreviewZoom(e);
			};
		}


		/// <summary>
		/// Resize preview image on scrolling
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event args</param>
		private void OnPreviewZoom(MouseScrolledEventArgs e) 
		{
			if (e.Direction == ScrollDirection.Down) {
				imageScale *= 0.9;
			} else {
				imageScale *= 1.1;
			}

			for( int i = 0; i < tabs.Length; i++) {
				if (tabs [i].Content != null) {
					ScanView scanview = (ScanView)tabs [i].Content;
					if (scanview != null) {
						if (e.Direction == ScrollDirection.Down) {
							scanview.Scale (0.9);
						} else {
							scanview.Scale (1.1);
						}
					}
				}
			}

			e.Handled = true;
		}


		private void ResizeImageToFit(ScrollView sv) {
			ScanView iv = (ScanView) sv.Content;

			if(iv != null && iv.Image != null) {

				double width = sv.VisibleRect.Width / iv.Image.Size.Width;
				double height = sv.VisibleRect.Height / iv.Image.Size.Height;

				imageScale *= Math.Min (width, height);
				iv.Scale (Math.Min (width, height));
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">ScrollView.</param>
		/// <param name="e">Mouse event args.</param>
		private void MouseMovedNotGtk (object sender, MouseMovedEventArgs e) {
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

					foreach(ScrollView s in tabs) {
						s.HorizontalScrollControl.Value =
							Math.Min (sc.HorizontalScrollControl.UpperValue, newScrollX);
						s.VerticalScrollControl.Value =
							Math.Min (sc.VerticalScrollControl.UpperValue, newScrollY);
					}
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
		private void MouseMovedGtk(object sender, MouseMovedEventArgs e)
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

            ScanType type = (ScanType)this.CurrentTabIndex;
			imageLoaderThread = new Thread (() => LoadPreview (currentScan, type));
			imageLoaderThread.Start ();
		}
	}
}

