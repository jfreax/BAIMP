//
//  MainWindow.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
		VPaned splitScan_Pipeline;
		HPaned splitFiletree_Preview;
		VBox splitMain_Status;
		VBox splitController_Preview;
		VBox pipelineShelf;

		Preview preview;
		FileTreeView fileTree;

		PipelineController pipelineController;

		/// <summary>
		/// Cache already instanciated exporter.
		/// </summary>
		readonly Dictionary<string, BaseExporter> exporterList = new Dictionary<string, BaseExporter>();

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
				splitMain_Status.Remove(pipelineShelf);
				splitScan_Pipeline.Panel2.Content = pipelineShelf;
				splitMain_Status.PackStart(splitScan_Pipeline, true, true);
			};
			menuViewPipeline.Clicked += delegate {
				splitScan_Pipeline.Panel2.Content = null;
				splitMain_Status.Remove(splitScan_Pipeline);
				splitMain_Status.PackStart(pipelineShelf, true, true);
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

			Type exporterType = typeof(BaseExporter);
			IEnumerable<Type> exporter = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(t => t.BaseType == exporterType);

			foreach (Type export in exporter) {
				MenuItem ni = new MenuItem(string.Format("As {0}...", export.Name));
				ni.Tag = export.Name;
				menuExport.SubMenu.Items.Add(ni);
				var lExport = export;
				ni.Clicked += delegate(object sender, EventArgs e) {
					MenuItem s = sender as MenuItem;

					if (s != null) {
						if (exporterList.ContainsKey(s.Tag.ToString())) {
							exporterList[s.Tag.ToString()].ShowDialog(pipelineController.CurrentPipeline.Nodes);
						} else {
							BaseExporter instance = 
								Activator.CreateInstance(lExport, pipelineController.CurrentPipeline.PipelineName) as BaseExporter;
							if (instance != null) {
								exporterList[s.Tag.ToString()] = instance;
								instance.ShowDialog(pipelineController.CurrentPipeline.Nodes);
							}
						}
					}
				};
			}

			// Extras menu
			MenuItem extrasMenu = new MenuItem("E_xtras");
			extrasMenu.SubMenu = new Menu();
			MenuItem menuResetAllMasks = new MenuItem("_Reset all masks");
			menuResetAllMasks.Clicked += delegate {
				foreach (BaseScan scan in project.scanCollection) {
					scan.Mask.ResetMask();
				}
			};
			MenuItem menuExportLog = new MenuItem("Export _logs");
			menuExportLog.Clicked += delegate {
				SaveFileDialog d = new SaveFileDialog("Export logs");
				if (d.Run()) {
					string filename = d.FileName;
					if (!string.IsNullOrEmpty(filename)) {
						System.IO.File.WriteAllText(filename, Log.ToText());
					}
				}
				d.Dispose();
			};

			extrasMenu.SubMenu.Items.Add(menuResetAllMasks);
			extrasMenu.SubMenu.Items.Add(menuExportLog);

			// main menu
			Menu menu = new Menu();
			menu.Items.Add(fileMenu);
			menu.Items.Add(viewMenu);
			menu.Items.Add(editMenu);
			menu.Items.Add(pipelineMenu);
			menu.Items.Add(extrasMenu);
			MainMenu = menu;

			// initialize preview widget
			preview = new Preview();

			// load tree view with all available files
			fileTree = new FileTreeView();
			VBox splitFileTreeSearch_FileTree = new VBox();
			splitFileTreeSearch_FileTree.PackStart(new FileTreeFilter(fileTree));
			splitFileTreeSearch_FileTree.PackStart(fileTree, true);

			// load pipeline controller
			pipelineShelf = new VBox();
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

			splitScan_Pipeline = new VPaned();
			splitScan_Pipeline.Panel1.Content = splitController_Preview;
			splitScan_Pipeline.Panel2.Content = pipelineShelf;
			splitScan_Pipeline.Panel2.Resize = true;

			splitMain_Status = new VBox();
			splitMain_Status.PackStart(splitScan_Pipeline, true, true);
			splitMain_Status.PackEnd(new StatusBar());

			Content = splitMain_Status;
		}

		/// <summary>
		/// Initializes all event handlers.
		/// </summary>
		void InitializeEvents()
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

					if (!e.refresh) {
						MarkAsUnsaved();
					}
				}
			};

			// global key events
			splitScan_Pipeline.KeyPressed += GlobalKeyPressed;
		}

		protected override void OnShown()
		{
			base.OnShown();

			// Restore positions
			splitFiletree_Preview.PositionFraction = Settings.Default.FileTreePreviewPosition;
			splitScan_Pipeline.PositionFraction = Settings.Default.ScanPipelineFraction;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			preview.Dispose();
			fileTree.Dispose();
			pipelineController.Dispose();
			splitScan_Pipeline.Dispose();
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

			Settings.Default.FileTreePreviewPosition = splitFiletree_Preview.PositionFraction;
			Settings.Default.ScanPipelineFraction = splitScan_Pipeline.PositionFraction;

			// Save settings
			Settings.Default.Save();

			Application.Exit();
		}

		#endregion
	}
}

