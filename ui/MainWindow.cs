using System;
using Xwt;
using bachelorarbeit_implementierung.Properties;
using Xwt.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace bachelorarbeit_implementierung
{
	public class MainWindow : Window
	{
		ScanCollection scanCollection;
		ToolkitType toolkitType;

		// widgets
		HPaned splitFiletreePreview;
		HPaned splitPreviewMetadata;


		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.MainWindow"/> class.
		/// </summary>
		/// <param name="path">Path.</param>
		public MainWindow (ToolkitType toolkitType, string path)
		{
			this.toolkitType = toolkitType;

			// restore last window size and location
            this.Location = new Point(
                Settings.Default.WindowLocationX, 
                Settings.Default.WindowLocationY
            );

			this.Size = new Size (
				Settings.Default.WindowSizeWidth,
				Settings.Default.WindowSizeHeight
			);

			// set window preference
			Title = "Bachelorarbeit - Jens Dieskau";

			// initialize global events
			//CloseRequested += HandleCloseRequested;
			Closed += OnClosing;

			// load metadata
			scanCollection = new ScanCollection (path);

			// initialize the user interface
			InitializeUI ();
		}


		/// <summary>
		/// Initializes the user inferface
		/// </summary>
		private void InitializeUI() {
			//var keys = scans.scans.Keys;
			//foreach (string key in keys)
			//{
			//	Console.WriteLine("Key: {0}", key);
			//}


			splitFiletreePreview = new HPaned ();
			splitPreviewMetadata = new HPaned ();

			Label title1 = new Label ("Title");
			splitFiletreePreview.Panel1.Content = title1;

			ScanView img = new ScanView ();

			// test load image
			MemoryStream memoryStream = new MemoryStream();
			(scanCollection.scans ["Unbekannt"] [0]).GetAsBitmap(ScanType.Intensity)
				.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			memoryStream.Position = 0;

			img.Image = Image.FromStream (memoryStream);
			ScrollView sc = new ScrollView(img);
			splitPreviewMetadata.Panel1.Content = sc;

			sc.BoundsChanged += delegate(object sender, EventArgs e) {
				double width = sc.VisibleRect.Width / ((ImageView)sc.Content).Image.Size.Width;
				double height = sc.VisibleRect.Height / ((ImageView)sc.Content).Image.Size.Height;

				((ImageView)sc.Content).Image = 
					((ImageView)sc.Content).Image.Scale( Math.Min(width, height) );
			};

            sc.ButtonPressed += delegate(object sender, ButtonEventArgs e)
            {
				if( e.Button == PointerButton.Middle ) {
                    img.Data["pressed"] = true;
                    img.Data["pressedPosition"] = e.Position;
				}
			};

            sc.ButtonReleased += delegate(object sender, ButtonEventArgs e)
            {
                if (e.Button == PointerButton.Middle)
                {
                    img.Data.Remove("pressed");
                }
            };

            sc.MouseExited += delegate(object sender, EventArgs e)
            {
                img.Data.Remove("pressed");
            };

			if (toolkitType == ToolkitType.Gtk) {
				sc.MouseMoved += MouseMovedGtk;
			} else {
				sc.MouseMoved += MouseMoved;
			}


			img.MouseScrolled += OnPreviewScroll;
            sc.MouseScrolled += delegate(object sender, MouseScrolledEventArgs e)
            {
                OnPreviewScroll((object) img, e);
            };

			splitFiletreePreview.Panel2.Content = splitPreviewMetadata;

			Content = splitFiletreePreview;
		}


		/// <summary>
		/// Shows the preview of specified scan data
		/// </summary>
		/// <param name="scan">Scan.</param>
		private void ShowPreviewOf(Scan scan) {

		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		private void HandleCloseRequested (object sender, CloseRequestedEventArgs args)
		{
			args.AllowClose = MessageDialog.Confirm ("Close?", Command.Ok);
		}


		/// <summary>
		/// Raises the closing event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void OnClosing(object sender, EventArgs e)
		{
			// Copy window location to app settings
			Settings.Default.WindowLocationX = this.Location.X;
            Settings.Default.WindowLocationY = this.Location.Y;

			// Copy window size to app settings
			Settings.Default.WindowSizeWidth = this.Size.Width;
			Settings.Default.WindowSizeHeight = this.Size.Height;

			// Save settings
			Settings.Default.Save();
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

		void MouseMoved (object sender, MouseMovedEventArgs e) {
			e.Handled = false;

			ScrollView sc = (ScrollView)sender;
			sc.MouseMoved -= MouseMoved;

            ScanView img = (ScanView)sc.Content;

            if (img.Data.ContainsKey("pressed") &&
                img.Data.ContainsKey("pressedPosition") &&
                img.Data["pressedPosition"] != null)
            {
                Point oldPosition = (Point)img.Data["pressedPosition"];

				double newScrollX = sc.HorizontalScrollControl.Value + oldPosition.X - e.Position.X;
				double newScrollY = sc.VerticalScrollControl.Value + oldPosition.Y - e.Position.Y;

				sc.HorizontalScrollControl.Value =
                    Math.Min(sc.HorizontalScrollControl.UpperValue, newScrollX);
				sc.VerticalScrollControl.Value =
                    Math.Min(sc.VerticalScrollControl.UpperValue, newScrollY);
			}

            img.Data["pressedPosition"] = e.Position;
			sc.MouseMoved += MouseMoved;
		}


        void MouseMovedGtk(object sender, MouseMovedEventArgs e)
        {
            e.Handled = false;

            ScrollView sc = (ScrollView)sender;
			sc.MouseMoved -= MouseMovedGtk;

            ScanView img = (ScanView)sc.Content;

            if (img.Data.ContainsKey("pressed") &&
                img.Data.ContainsKey("pressedPosition") &&
                img.Data["pressedPosition"] != null)
            {
                Point oldPosition = (Point)img.Data["pressedPosition"];

                double newScrollX = sc.HorizontalScrollControl.Value + oldPosition.X - e.Position.X;
                double newScrollY = sc.VerticalScrollControl.Value + oldPosition.Y - e.Position.Y;

                sc.HorizontalScrollControl.Value =
                    Math.Min(sc.HorizontalScrollControl.UpperValue - sc.VisibleRect.Width, newScrollX);
                sc.VerticalScrollControl.Value =
                    Math.Min(sc.VerticalScrollControl.UpperValue - sc.VisibleRect.Height, newScrollY);
            }

			sc.MouseMoved += MouseMovedGtk;
        }
	}
}

