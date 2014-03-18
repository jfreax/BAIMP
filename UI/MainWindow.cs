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
		VPaned splitAlgorithmTree;
		HPaned splitFiletreeAlgo_Preview;
		HBox splitPreview_Metadata;
		VPaned splitFileTree_Algo;

		Preview preview;
		FileTreeView fileTree;
		MetadataView metadata;
		AlgorithmTreeView algorithm;
		PipelineView pipeline;

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

			// initialize preview widget
			preview = new Preview ();
			//splitPreviewMetadata.Panel1.Content = preview;

			// load tree view with all available files
			fileTree = new FileTreeView (scanCollection);

			// load metadata viewer
			metadata = new MetadataView ();

			// load algorithm list viewer
			algorithm = new AlgorithmTreeView();

			// load algorithm tree viever
			pipeline = new PipelineView();

			// set layout
			splitFileTree_Algo = new VPaned ();
			splitFileTree_Algo.Panel1.Content = fileTree;
			splitFileTree_Algo.Panel2.Content = algorithm;

			splitPreview_Metadata = new HBox ();
			splitPreview_Metadata.PackStart (preview, true, true);
			splitPreview_Metadata.PackEnd (metadata, false, false);

			splitFiletreeAlgo_Preview = new HPaned ();
			splitFiletreeAlgo_Preview.Panel1.Content = splitFileTree_Algo;
			splitFiletreeAlgo_Preview.Panel2.Content = splitPreview_Metadata;
			splitFiletreeAlgo_Preview.Panel2.Resize = true;

			splitAlgorithmTree = new VPaned ();
			splitAlgorithmTree.Panel1.Content = splitFiletreeAlgo_Preview;
			splitAlgorithmTree.Panel2.Content = pipeline;

			Content = splitAlgorithmTree;

			InitializeEvents ();
			fileTree.InitializeUI (); // call after initialize events!
			fileTree.Reload ();
		}


		/// <summary>
		/// Initializes all event handlers.
		/// </summary>
		private void InitializeEvents()
		{
			fileTree.SelectionChanged += delegate(object sender, EventArgs e) {
				if(fileTree.SelectedRow != null) {
					object value = 
						fileTree.store
							.GetNavigatorAt (fileTree.SelectedRow)
							.GetValue (fileTree.nameCol);

					if( value is ScanWrapper ) {
						ScanWrapper s = (ScanWrapper)value;
						preview.ShowPreviewOf(s);
						metadata.Load(s);
					}
				}
			};

			foreach (string key in scanCollection.Keys) {
				foreach (ScanWrapper scan in scanCollection[key]) {
					scan.ScanDataChanged += fileTree.OnScanDataChanged;
				}
			}

			// global key events
			splitFiletreeAlgo_Preview.KeyPressed += GlobalKeyPressed;
			splitPreview_Metadata.KeyPressed += GlobalKeyPressed;
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

