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

		// widgets
		HPaned splitFiletreePreview;
		HBox splitPreviewMetadata;

		Preview preview;
		FileTreeView fileTreeView;
		MetadataView metadata;


		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.MainWindow"/> class.
		/// </summary>
		/// <param name="path">Path.</param>
		public MainWindow (string path)
		{
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

			splitFiletreePreview = new HPaned ();
			splitPreviewMetadata = new HBox ();

			// initialize preview widget
			preview = new Preview ();
			//splitPreviewMetadata.Panel1.Content = preview;
			splitPreviewMetadata.PackStart (preview, true, true);

			// load tree view with all available files
			fileTreeView = new FileTreeView (scanCollection);
			splitFiletreePreview.Panel1.Content = fileTreeView;

			// load metadata viewer
			metadata = new MetadataView ();
			splitPreviewMetadata.PackEnd (metadata, false, false);


			splitFiletreePreview.Panel2.Content = splitPreviewMetadata;
			splitFiletreePreview.Panel2.Resize = true;

			Content = splitFiletreePreview;

			InitializeEvents ();
			fileTreeView.Initialize (); // call after initialize events!
		}


		/// <summary>
		/// Initializes all event handlers.
		/// </summary>
		private void InitializeEvents()
		{
			fileTreeView.SelectionChanged += delegate(object sender, EventArgs e) {
				if(fileTreeView.SelectedRow != null) {
					object value = 
						fileTreeView.store
							.GetNavigatorAt (fileTreeView.SelectedRow)
							.GetValue (fileTreeView.nameCol);

					if( value is ScanWrapper ) {
						ScanWrapper s = (ScanWrapper)value;
						preview.ShowPreviewOf(s);
						metadata.Load(s);
					}
				}
			};

			preview.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
				ScanWrapper scan = (ScanWrapper) sender;

				fileTreeView
					.store
					.GetNavigatorAt (scan.position)
					.SetValue(fileTreeView.saveStateCol, e.Saved ? "" : "*");
			};
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

