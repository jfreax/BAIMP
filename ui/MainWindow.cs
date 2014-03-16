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
		HPaned splitPreviewMetadata;


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
			splitPreviewMetadata = new HPaned ();

			// initialize preview widget
			Preview preview = new Preview ();
			splitPreviewMetadata.Panel1.Content = preview;
			splitFiletreePreview.Panel2.Content = splitPreviewMetadata;

			// load tree view with all available files
			DataField<object> nameCol = new DataField<object> ();

			TreeView fileTreeView = new TreeView ();
			TreeStore fileTreeStore = new TreeStore (nameCol);
			fileTreeView.Columns.Add ("Name", nameCol);

			var keys = scanCollection.scans.Keys;
			TreePosition pos = null;
			foreach (string key in keys)
			{
				var w = fileTreeStore.AddNode (null).SetValue (nameCol, key).CurrentPosition;

				foreach (Scan scan in scanCollection.scans[key]) {
					var v = fileTreeStore.AddNode (w).SetValue (nameCol, scan).CurrentPosition;
					if (pos == null) {
						pos = v;

					}
				}
			}

			fileTreeView.SelectionChanged += delegate(object sender, EventArgs e) {
				if(fileTreeView.SelectedRow != null) {
					object value = fileTreeStore.GetNavigatorAt (fileTreeView.SelectedRow).GetValue (nameCol);
					if( value is Scan ) {
						Scan s = (Scan)value;

						preview.ShowPreviewOf(s);
					}

				}
			};


			fileTreeView.DataSource = fileTreeStore;
			fileTreeView.ExpandAll ();
			fileTreeView.SelectRow (pos);
			splitFiletreePreview.Panel1.Content = fileTreeView;


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

