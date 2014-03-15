using System;
using Xwt;
using bachelorarbeit_implementierung.Properties;
using Xwt.Drawing;
using System.IO;

namespace bachelorarbeit_implementierung
{
	public class MainWindow : Window
	{
		ScanCollection scanCollection;

		// widgets
		HPaned splitFiletreePreview;
		HPaned splitPreviewMetadata;


		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.MainWindow"/> class.
		/// </summary>
		/// <param name="path">Path.</param>
		public MainWindow (string path)
		{
			// restore last window size and location
			this.Location = Settings.Default.WindowLocation;
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

			ImageView img = new ImageView ();

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

			img.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				Console.WriteLine(e.Position);
				Console.WriteLine(e.X + "x" + e.Y);
			};
			sc.MouseScrolled += OnPreviewScroll;

			this.BoundsChanged += delegate(object sender, EventArgs e) {
				Console.WriteLine("OK");
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
			Settings.Default.WindowLocation = this.Location;

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
		/// <param name="e">E.</param>
		private void OnPreviewScroll(object sender, MouseScrolledEventArgs e) 
		{
			ScrollView sv = (ScrollView) sender;
			ImageView iv = (ImageView) sv.Content;

			if (e.Direction == ScrollDirection.Down) {
				iv.Image = iv.Image.Scale (0.90);
			} else {
				iv.Image = iv.Image.Scale (1.10);
			}

			e.Handled = true;
		}
	}
}

