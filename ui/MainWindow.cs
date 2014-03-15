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

			Preview preview = new Preview ();
			preview.ShowPreviewOf (scanCollection.scans ["Unbekannt"] [0]);


			splitPreviewMetadata.Panel1.Content = preview;


			splitFiletreePreview.Panel2.Content = splitPreviewMetadata;

			Content = splitFiletreePreview;
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
	}
}

