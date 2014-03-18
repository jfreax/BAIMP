using System;
using Xwt;
using Xwt.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace baimp
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
		private void InitializeUI()
		{
			// main menu
			Menu menu = new Menu ();
			var file = new MenuItem ("_File");
			file.SubMenu = new Menu ();
			//file.SubMenu.Items.Add (new MenuItem ("_Open"));

			MenuItem menuSave = new MenuItem ("_Save");
			menuSave.Clicked += (object sender, EventArgs e) => scanCollection.SaveAll ();
			file.SubMenu.Items.Add (menuSave);

			MenuItem menuClose = new MenuItem ("_Close");
			menuClose.Clicked += (object sender, EventArgs e) => this.Close();
			file.SubMenu.Items.Add (menuClose);

			menu.Items.Add (file);
			MainMenu = menu;

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
			fileTreeView.InitializeUI (); // call after initialize events!
			fileTreeView.Reload ();
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

			foreach (string key in scanCollection.Keys) {
				foreach (ScanWrapper scan in scanCollection[key]) {
					scan.ScanDataChanged += fileTreeView.OnScanDataChanged;
				}
			}

			// global key events
			splitFiletreePreview.KeyPressed += GlobalKeyPressed;
			splitPreviewMetadata.KeyPressed += GlobalKeyPressed;
		}


		private void GlobalKeyPressed(object sender, KeyEventArgs e) {
			if (e.Modifiers.HasFlag (ModifierKeys.Command) ||
				e.Modifiers.HasFlag (ModifierKeys.Control)) {

				switch (e.Key) {
				case Key.s:
					scanCollection.SaveAll ();
					break;
				}
			}


			e.Handled = true;
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
		/// <param name="e">Event arguments.</param>
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

			Application.Exit();
		}
	}
}

