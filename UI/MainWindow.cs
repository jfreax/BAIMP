using System;
using System.Linq;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;

namespace Baimp
{
	public class MainWindow : Window
	{
		Project project;

		// widgets
		VPaned splitAlgorithmTree;
		HPaned splitFiletree_Preview;
		HBox splitPreview_Metadata;

		Preview preview;
		FileTreeView fileTree;
		MetadataView metadata;

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

			// main menu
			Menu menu = new Menu();
			menu.Items.Add(fileMenu);
			menu.Items.Add(editMenu);
			menu.Items.Add(pipelineMenu);
			MainMenu = menu;

			// initialize preview widget
			preview = new Preview();

			// load tree view with all available files
			fileTree = new FileTreeView();

			// load metadata viewer
			metadata = new MetadataView();

			// load pipeline controller
			FrameBox controllbarShelf = new FrameBox();
			FrameBox pipelineShelf = new FrameBox();
			pipelineController = new PipelineController(project, controllbarShelf, pipelineShelf);

			// set layout
			splitPreview_Metadata = new HBox();
			splitPreview_Metadata.PackStart(preview, true, true);
			splitPreview_Metadata.PackEnd(metadata, false, false);

			splitFiletree_Preview = new HPaned();
			splitFiletree_Preview.Panel1.Content = fileTree;
			splitFiletree_Preview.Panel1.Shrink = true;
			fileTree.HorizontalScrollPolicy = ScrollPolicy.Never;
			splitFiletree_Preview.Panel2.Content = splitPreview_Metadata;
			splitFiletree_Preview.Panel2.Resize = true;

			VBox splitController_Preview = new VBox();
			splitController_Preview.PackStart(controllbarShelf, false, false);
			splitController_Preview.PackEnd(splitFiletree_Preview, true, true);

			splitAlgorithmTree = new VPaned();
			splitAlgorithmTree.Panel1.Content = splitController_Preview;
			splitAlgorithmTree.Panel2.Content = pipelineShelf;

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
				if (fileTree.SelectedRow != null && fileTree.SelectedRows.Length == 1) {
					string fiberName = 
						fileTree.store
							.GetNavigatorAt(fileTree.SelectedRow)
							.GetValue(fileTree.nameCol);
					Image previewImage = 
						fileTree.store
							.GetNavigatorAt(fileTree.SelectedRow)
							.GetValue(fileTree.thumbnailCol);

					var baseScan = project.scanCollection.Find(s => s.Name == fiberName);
					if (baseScan != null) {
						preview.ShowPreviewOf(baseScan, previewImage);
						metadata.Load(baseScan);
					}

				} else if(fileTree.SelectedRows != null) {
					List<BaseScan> scans = new List<BaseScan>();
					foreach (TreePosition pos in fileTree.SelectedRows) {
						string fiberName = 
							fileTree.store
								.GetNavigatorAt(pos)
								.GetValue(fileTree.nameCol);
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
				}
			};

			// global key events
			splitAlgorithmTree.KeyPressed += GlobalKeyPressed;
		}

		#endregion

		private void SaveAll()
		{
			if (project.Save(pipelineController)) {
				if (Title.EndsWith("*", StringComparison.Ordinal)) {
					Title = Title.Remove(Title.Length - 1);
				}
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

