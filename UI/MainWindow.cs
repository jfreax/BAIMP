using System;
using System.Linq;
using Xwt;
using System.Collections.Generic;

namespace Baimp
{
	public class MainWindow : Window
	{
		Project project;

		// widgets
		VPaned splitAlgorithmTree;
		HPaned splitFiletree_Preview;
		VBox splitController_Preview;

		Preview preview;
		FileTreeView fileTree;

		PipelineController pipelineController;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.MainWindow"/> class.
		/// </summary>
		/// <param name="project">Project.</param>
		public MainWindow(Project project)
		{
			this.project = project;

			Initialize();
		}

		#region initialize

		private void Initialize()
		{
			InitializeUI();
			InitializeEvents();

			fileTree.InitializeUI(); // call after initialize events!
			fileTree.Reload(project.scanCollection);
		}

		/// <summary>
		/// Initializes the user inferface
		/// </summary>
		private void InitializeUI()
		{
			// restore last window size and location
			Location = new Point(
				Settings.Default.WindowLocationX, 
				Settings.Default.WindowLocationY
			);

			Size = new Size(
				Settings.Default.WindowSizeWidth,
				Settings.Default.WindowSizeHeight
			);

			// set window preference
			Title = "BAIMP";

			// file menu
			var fileMenu = new MenuItem("_File");
			fileMenu.SubMenu = new Menu();

			MenuItem menuNew = new MenuItem("_New...");
			menuNew.Clicked += (object sender, EventArgs e) => project.NewDialog();
			fileMenu.SubMenu.Items.Add(menuNew);

			MenuItem menuOpen = new MenuItem("_Open...");
			menuOpen.Clicked += delegate {
				if (!project.OpenDialog() && !string.IsNullOrEmpty(project.ErrorMessage)) {
					MessageDialog.ShowMessage ("Error while opening the file", project.ErrorMessage);
					project.ErrorMessage = null;
				}
			};
			fileMenu.SubMenu.Items.Add(menuOpen);

			if (Settings.Default.LastOpenedProjects != null) {
				MenuItem menuLastOpened = new MenuItem("Recently opened");
				menuLastOpened.SubMenu = new Menu();
				fileMenu.SubMenu.Items.Add(menuLastOpened);

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

			fileMenu.SubMenu.Items.Add(new SeparatorMenuItem());

			MenuItem menuImport = new MenuItem("_Import...");
			menuImport.Clicked += (object sender, EventArgs e) => project.ImportDialog();
			fileMenu.SubMenu.Items.Add(menuImport);

			MenuItem menuSave = new MenuItem("_Save");
			menuSave.Clicked += (object sender, EventArgs e) => SaveAll();
			fileMenu.SubMenu.Items.Add(menuSave);

			fileMenu.SubMenu.Items.Add(new SeparatorMenuItem());

			MenuItem menuClose = new MenuItem("_Exit");
			menuClose.Clicked += (object sender, EventArgs e) => Close();
			fileMenu.SubMenu.Items.Add(menuClose);

			// View menu
			MenuItem viewMenu = new MenuItem("_View");
			viewMenu.SubMenu = new Menu();
			RadioButtonMenuItemGroup viewRadioGroup = new RadioButtonMenuItemGroup();
			RadioButtonMenuItem menuViewOverview = new RadioButtonMenuItem("Overview");
			menuViewOverview.Checked = true;
			menuViewOverview.Group = viewRadioGroup;
			viewMenu.SubMenu.Items.Add(menuViewOverview);

			RadioButtonMenuItem menuViewPipeline = new RadioButtonMenuItem("Pipeline");
			menuViewPipeline.Group = menuViewOverview.Group;
			viewMenu.SubMenu.Items.Add(menuViewPipeline);

			menuViewOverview.Clicked += delegate {
				splitController_Preview.PackEnd(splitFiletree_Preview, true, true);
			};
			menuViewPipeline.Clicked += delegate {
				splitController_Preview.Remove(splitFiletree_Preview);
			};


			// Edit menu
			MenuItem editMenu = new MenuItem("_Edit");
			editMenu.SubMenu = new Menu();
			MenuItem menuWorksheetRename = new MenuItem("_Rename worksheet...");
			editMenu.SubMenu.Items.Add(menuWorksheetRename);
			menuWorksheetRename.Clicked += (object sender, EventArgs e) => pipelineController.RenameCurrentWorksheetDialog();

			// Pipeline menu
			MenuItem pipelineMenu = new MenuItem("_Pipeline");
			pipelineMenu.SubMenu = new Menu();

			MenuItem menuExecute = new MenuItem("_Execute");
			menuExecute.Clicked += (object sender, EventArgs e) => pipelineController.CurrentPipeline.Execute(project);
			pipelineMenu.SubMenu.Items.Add(menuExecute);

			pipelineMenu.SubMenu.Items.Add(new SeparatorMenuItem());

			MenuItem menuExport = new MenuItem("Export");
			menuExport.SubMenu = new Menu();
			pipelineMenu.SubMenu.Items.Add(menuExport);

			Type exporterType = typeof(IExporter);
			IEnumerable<Type> exporter = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(t => t.GetInterfaces().Contains(exporterType));

			foreach (Type export in exporter) {
				MenuItem ni = new MenuItem(string.Format("As {0}...", export.Name));
				menuExport.SubMenu.Items.Add(ni);
				var lExport = export;
				ni.Clicked += delegate {
					IExporter instance = 
						Activator.CreateInstance(lExport) as IExporter;
					if (instance != null) {
						instance.Run(pipelineController.CurrentPipeline);
					}
				};
			}

			// main menu
			Menu menu = new Menu();
			menu.Items.Add(fileMenu);
			menu.Items.Add(viewMenu);
			menu.Items.Add(editMenu);
			menu.Items.Add(pipelineMenu);
			MainMenu = menu;

			// initialize preview widget
			preview = new Preview();

			// load tree view with all available files
			fileTree = new FileTreeView();
			VBox splitFileTreeSearch_FileTree = new VBox();
			splitFileTreeSearch_FileTree.PackStart(new FileTreeFilter(fileTree));
			splitFileTreeSearch_FileTree.PackStart(fileTree, true);

			// load pipeline controller
			VBox pipelineShelf = new VBox();
			pipelineController = new PipelineController(project, pipelineShelf);

			splitFiletree_Preview = new HPaned();
			splitFiletree_Preview.Panel1.Content = splitFileTreeSearch_FileTree;
			splitFiletree_Preview.Panel1.Shrink = true;
			fileTree.HorizontalScrollPolicy = ScrollPolicy.Never;
			splitFiletree_Preview.Panel2.Content = preview;
			splitFiletree_Preview.Panel2.Resize = true;

			splitController_Preview = new VBox();
			//splitController_Preview.PackStart(controllbarShelf, false, false);
			splitController_Preview.PackEnd(splitFiletree_Preview, true, true);

			splitAlgorithmTree = new VPaned();
			splitAlgorithmTree.Panel1.Content = splitController_Preview;
			splitAlgorithmTree.Panel2.Content = pipelineShelf;
			splitAlgorithmTree.Panel2.Resize = true;

			VBox splitMain_Status = new VBox();
			splitMain_Status.PackStart(splitAlgorithmTree, true, true);
			splitMain_Status.PackEnd(new StatusBar());

			Content = splitMain_Status;
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
				TreeStore currentStore = fileTree.DataSource as TreeStore;
				if (currentStore != null && fileTree.SelectedRow != null && fileTree.SelectedRows.Length == 1) {
					string fiberName = 	currentStore
						.GetNavigatorAt(fileTree.SelectedRow)
						.GetValue(fileTree.IsFiltered ? fileTree.nameColFilter : fileTree.nameCol);

					var baseScan = project.scanCollection.Find(s => s.Name == fiberName);
					if (baseScan != null) {
						preview.ShowPreviewOf(baseScan);
					}

				} else if(currentStore != null && fileTree.SelectedRows != null) {
					List<BaseScan> scans = new List<BaseScan>();
					foreach (TreePosition pos in fileTree.SelectedRows) {
						string fiberName =	currentStore
							.GetNavigatorAt(pos)
							.GetValue(fileTree.IsFiltered ? fileTree.nameColFilter : fileTree.nameCol);

						var baseScan = project.scanCollection.Find(s => s.Name == fiberName);
						if(baseScan != null) {
							scans.Add(baseScan);
						}
					}

					preview.ShowPreviewOf(scans);
				}
			};
				
			project.ProjectChanged += delegate(object sender, ProjectChangedEventArgs e) {
				if (e != null) {
					fileTree.Reload(project.scanCollection);

					MarkAsUnsaved();
				}
			};

			// global key events
			splitAlgorithmTree.KeyPressed += GlobalKeyPressed;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			preview.Dispose();
			fileTree.Dispose();
			pipelineController.Dispose();
			splitAlgorithmTree.Dispose();
			splitController_Preview.Dispose();
			splitFiletree_Preview.Dispose();
		}

		#endregion

		private void SaveAll()
		{
			if (project.Save(pipelineController)) {
				MarkAsSaved();
			}
		}

		public void MarkAsUnsaved()
		{
			if (!Title.EndsWith("*", StringComparison.Ordinal)) {
				Title += "*";
			}
		}

		public void MarkAsSaved()
		{
			if (Title.EndsWith("*", StringComparison.Ordinal)) {
				Title = Title.Remove(Title.Length - 1);
			}
		}

		public bool IsSaved()
		{
			if (Title.EndsWith("*", StringComparison.Ordinal)) {
				return false;
			}

			return true;
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
			Settings.Default.WindowLocationX = Location.X;
			Settings.Default.WindowLocationY = Location.Y;

			// Copy window size to app settings
			Settings.Default.WindowSizeWidth = Size.Width;
			Settings.Default.WindowSizeHeight = Size.Height;

			// Save settings
			Settings.Default.Save();

			Application.Exit();
		}

		#endregion
	}
}

