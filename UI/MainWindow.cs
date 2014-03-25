using System;
using Xwt;
using Xwt.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using baimp.Properties;

namespace baimp
{
	public class MainWindow : Window
	{
		Project project;
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
		/// Initializes a new instance of the <see cref="baimp.MainWindow"/> class.
		/// </summary>
		/// <param name="project">Project.</param>
		public MainWindow(Project project)
		{
			this.project = project;

			if (project.Files != null) {
				scanCollection = new ScanCollection(project.Files.ToArray());
			} else {
				scanCollection = new ScanCollection();
			}

			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.MainWindow"/> class.
		/// </summary>
		/// <param name="path">Path.</param>
		public MainWindow(string path)
		{
			// load metadata
			scanCollection = new ScanCollection(path);

			Initialize();
		}

		#region initialize

		private void Initialize()
		{
			InitializeUI();
			InitializeEvents();

			fileTree.InitializeUI(); // call after initialize events!
			fileTree.Reload();
		}

		/// <summary>
		/// Initializes the user inferface
		/// </summary>
		private void InitializeUI()
		{
			// restore last window size and location
			this.Location = new Point(
				Settings.Default.WindowLocationX, 
				Settings.Default.WindowLocationY
			);

			this.Size = new Size(
				Settings.Default.WindowSizeWidth,
				Settings.Default.WindowSizeHeight
			);

			// set window preference
			Title = "BAIMP";

			// file menu
			var file = new MenuItem("_File");
			file.SubMenu = new Menu();

			MenuItem menuNew = new MenuItem("_New...");
			menuNew.Clicked += (object sender, EventArgs e) => project.NewDialog();
			file.SubMenu.Items.Add(menuNew);

			MenuItem menuOpen = new MenuItem("_Open...");
			menuOpen.Clicked += delegate {
				if (!project.OpenDialog() && !string.IsNullOrEmpty(project.ErrorMessage)) {
					MessageDialog.ShowMessage ("Error while opening the file", project.ErrorMessage);
					project.ErrorMessage = null;
				}
			};
			file.SubMenu.Items.Add(menuOpen);

			if (Settings.Default.LastOpenedProjects != null) {
				MenuItem menuLastOpened = new MenuItem("Recently opened");
				menuLastOpened.SubMenu = new Menu();
				file.SubMenu.Items.Add(menuLastOpened);

				for (int i = Settings.Default.LastOpenedProjects.Count-1; i >= 0; i--) {
					string path = Settings.Default.LastOpenedProjects[i];

					MenuItem menuLastOpenedi = new MenuItem(path);
					menuLastOpenedi.Clicked += delegate(object sender, EventArgs e) {
						if (!project.Open(path) && !string.IsNullOrEmpty(project.ErrorMessage)) {
							MessageDialog.ShowMessage ("Error while opening the file", project.ErrorMessage);
							project.ErrorMessage = null;
						}
					};

					menuLastOpened.SubMenu.Items.Add(menuLastOpenedi);
				}
			}

			file.SubMenu.Items.Add(new SeparatorMenuItem());

			MenuItem menuImport = new MenuItem("_Import...");
			menuImport.Clicked += (object sender, EventArgs e) => project.ImportDialog();
			file.SubMenu.Items.Add(menuImport);

			MenuItem menuSave = new MenuItem("_Save");
			menuSave.Clicked += (object sender, EventArgs e) => SaveAll();
			file.SubMenu.Items.Add(menuSave);

			file.SubMenu.Items.Add(new SeparatorMenuItem());

			MenuItem menuClose = new MenuItem("_Exit");
			menuClose.Clicked += (object sender, EventArgs e) => this.Close();
			file.SubMenu.Items.Add(menuClose);


			// Pipeline menu
			MenuItem pipelineMenu = new MenuItem("_Pipeline");
			pipelineMenu.SubMenu = new Menu();

			MenuItem menuExecute = new MenuItem("_Execute");
			menuExecute.Clicked += (object sender, EventArgs e) => pipeline.Execute();
			pipelineMenu.SubMenu.Items.Add(menuExecute);

			// main menu
			Menu menu = new Menu();
			menu.Items.Add(file);
			menu.Items.Add(pipelineMenu);
			MainMenu = menu;

			// initialize preview widget
			preview = new Preview();

			// load tree view with all available files
			fileTree = new FileTreeView(scanCollection);

			// load metadata viewer
			metadata = new MetadataView();

			// load algorithm list viewer
			algorithm = new AlgorithmTreeView();

			// load algorithm tree viever
			ScrollView pipelineScroller = new ScrollView();
			pipeline = new PipelineView(pipelineScroller, project.LoadedNodes);
			pipelineScroller.MinHeight = (PipelineNode.NodeSize.Height + PipelineNode.NodeMargin.VerticalSpacing) * 6;
			pipelineScroller.Content = pipeline;

			// set layout
			splitFileTree_Algo = new VPaned();
			splitFileTree_Algo.Panel1.Content = fileTree;
			splitFileTree_Algo.Panel2.Content = algorithm;

			splitPreview_Metadata = new HBox();
			splitPreview_Metadata.PackStart(preview, true, true);
			splitPreview_Metadata.PackEnd(metadata, false, false);

			splitFiletreeAlgo_Preview = new HPaned();
			splitFiletreeAlgo_Preview.Panel1.Content = splitFileTree_Algo;
			splitFiletreeAlgo_Preview.Panel2.Content = splitPreview_Metadata;
			splitFiletreeAlgo_Preview.Panel2.Resize = true;

			splitAlgorithmTree = new VPaned();
			splitAlgorithmTree.Panel1.Content = splitFiletreeAlgo_Preview;
			splitAlgorithmTree.Panel2.Content = pipelineScroller;

			Content = splitAlgorithmTree;
		}

		/// <summary>
		/// Initializes all event handlers.
		/// </summary>
		private void InitializeEvents()
		{
			// initialize global events
			//CloseRequested += HandleCloseRequested;
			Closed += OnClosing;

			fileTree.SelectionChanged += delegate(object sender, EventArgs e) {
				if (fileTree.SelectedRow != null) {
					object value = 
						fileTree.store
							.GetNavigatorAt(fileTree.SelectedRow)
							.GetValue(fileTree.nameCol);

					if (value is ScanWrapper) {
						ScanWrapper s = (ScanWrapper) value;
						preview.ShowPreviewOf(s);
						metadata.Load(s);
					}
				}
			};

			foreach (string key in scanCollection.Keys) {
				foreach (ScanWrapper scan in scanCollection[key]) {
					scan.ScanDataChanged += fileTree.OnScanDataChanged;
					scan.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
						if (e.Unsaved != null && e.Unsaved.Count > 0) {
							if (!this.Title.EndsWith("*")) {
								this.Title += "*";
							}
						}
					};
				}
			}

			project.ProjectChanged += delegate(object sender, ProjectChangedEventArgs e) {
				if (e != null) {
					if (e.refresh) {
						scanCollection.Clear();
						scanCollection.AddFiles(project.Files.ToArray());
						pipeline.Nodes = project.LoadedNodes;
					} else if (e.addedFiles != null && e.addedFiles.Length > 0) {
						scanCollection.AddFiles(e.addedFiles);
					}

					fileTree.Reload();
				}
			};

			pipeline.DataChanged += delegate(object sender, SaveStateEventArgs e) {
				if (!this.Title.EndsWith("*")) {
					this.Title += "*";
				}
			};


			// global key events
			splitAlgorithmTree.KeyPressed += GlobalKeyPressed;
		}

		#endregion

		private void SaveAll()
		{
			if (project.Save(pipeline)) {
				scanCollection.SaveAll();

				if (this.Title.EndsWith("*")) {
					this.Title = this.Title.Remove(this.Title.Length - 1);
				}
			} else {
				// TODO show error "cannot save"
			}
		}

		#region events

		private void GlobalKeyPressed(object sender, KeyEventArgs e)
		{
			if (e.Modifiers.HasFlag(ModifierKeys.Command) ||
			    e.Modifiers.HasFlag(ModifierKeys.Control)) {

				switch (e.Key) {
				case Key.s:
					SaveAll();
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
		private void HandleCloseRequested(object sender, CloseRequestedEventArgs args)
		{
			args.AllowClose = MessageDialog.Confirm("Close?", Command.Ok);
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

		#endregion
	}
}

