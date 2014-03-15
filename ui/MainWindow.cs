using System;
using Xwt;
using bachelorarbeit_implementierung.Properties;
using Xwt.Drawing;
using System.IO;

namespace bachelorarbeit_implementierung
{
	public class MainWindow : Window
	{

		HPaned splitFiletreePreview;
		HPaned splitPreviewMetadata;

		public MainWindow (string path)
		{
			// restore last window size and location
			this.Location = Settings.Default.WindowLocation;
			this.Size = new Size (
				Settings.Default.WindowSizeWidth,
				Settings.Default.WindowSizeHeight
			);

			Title = "Bachelorarbeit - Jens Dieskau";

			ScanCollection scanCollection = new ScanCollection (path);
			//var keys = scans.scans.Keys;
			//foreach (string key in keys)
			//{
			//	Console.WriteLine("Key: {0}", key);
			//}


			splitFiletreePreview = new HPaned ();
			splitPreviewMetadata = new HPaned ();

			Label title1 = new Label ("Title");
			splitFiletreePreview.Panel1.Content = title1;

			Label title2 = new Label ("Title2");
			ImageView img = new ImageView ();


			MemoryStream memoryStream = new MemoryStream();
			(scanCollection.scans ["Unbekannt"] [0]).GetAsBitmap(ScanType.Intensity)
				.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

			memoryStream.Position = 0;
			img.Image = Image.FromStream(memoryStream);

			splitPreviewMetadata.Panel1.Resize = true;
			splitPreviewMetadata.Panel1.Shrink = true;
			//HBox box = new HBox ();
			//box.PackStart (img);
			//splitPreviewMetadata.Panel1.Content = box;

			ScrollView sc = new ScrollView();

			sc.Content = img;
			splitPreviewMetadata.Panel1.Content = sc;

			splitFiletreePreview.Panel2.Content = splitPreviewMetadata;


			Content = splitFiletreePreview;

			//CloseRequested += HandleCloseRequested;
			Closed += OnClosing;
		}


		void HandleCloseRequested (object sender, CloseRequestedEventArgs args)
		{
			args.AllowClose = MessageDialog.Confirm ("Close?", Command.Ok);
		}


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
	}
}

