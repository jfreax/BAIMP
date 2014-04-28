//
//  PipelineController.cs
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
﻿using System;
using System.Linq;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class PipelineController : IDisposable
	{
		VBox pipelineShelf;
		VBox splitControllTab_pipelineScroller;
		CustomTabHost tabHost;

		Project project;
		readonly ScrollView pipelineScroller;
		AlgorithmTreeView algorithm;
		PipelineCollection pipelines = new PipelineCollection();
		PipelineView currentPipeline;

		FrameBox playStopButtonPlacement;
		ControllButton playButton;
		ControllButton stopButton;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.PipelineController"/> class.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <param name="pipelineShelf">Frame where the pipeline should be.</param>
		public PipelineController(Project project, VBox pipelineShelf)
		{
			this.pipelineShelf = pipelineShelf;

			pipelineScroller = new ScrollView();
			pipelineScroller.Content = currentPipeline;

			if (project.LoadedPipelines == null || project.LoadedPipelines.Count == 0) {
				CurrentPipeline = new PipelineView();
				CurrentPipeline.Initialize(pipelineScroller);
				pipelines.Add(currentPipeline.PipelineName, currentPipeline);
			} else {
				foreach (PipelineNodeWrapper wrapper in project.LoadedPipelines) {
					PipelineView newPipeline = new PipelineView();
					newPipeline.Initialize(pipelineScroller, wrapper.pNodes, wrapper.scrollX, wrapper.scrollY);
					newPipeline.PipelineName = wrapper.name;
					pipelines.Add(newPipeline.PipelineName, newPipeline);
				}
				CurrentPipeline = pipelines.Values.ToList()[0];
			}

			this.project = project;

			InitializeControllerbar();
			splitControllTab_pipelineScroller.PackEnd(pipelineScroller, true, true);

			algorithm = new AlgorithmTreeView();
			algorithm.MinWidth = 200;

			HPaned splitPipeline_Algorithm = new HPaned();
			splitPipeline_Algorithm.Panel1.Content = algorithm;
			splitPipeline_Algorithm.Panel1.Resize = false;
			splitPipeline_Algorithm.Panel1.Shrink = false;
			splitPipeline_Algorithm.Panel2.Content = splitControllTab_pipelineScroller;
			splitPipeline_Algorithm.Panel2.Resize = true;
			splitPipeline_Algorithm.Panel2.Shrink = false;

			pipelineShelf.PackStart(splitPipeline_Algorithm, true, true);

			InitializeEvents();
		}

		/// <summary>
		/// Initializes the controll bars.
		/// </summary>
		void InitializeControllerbar()
		{
			HBox controllbar = new HBox();
			controllbar.Spacing = 0;

			playButton = new ControllButton(Image.FromResource("Baimp.Resources.icoExecute-Normal.png"));
			playButton.Size = new Size(24, 24);
			playButton.TooltipText = "Execute pipeline";

			playButton.ButtonPressed += delegate {
				if (currentPipeline.Execute(project)) {
					playButton.Disable();
					stopButton.Enable();
				}
			};

			stopButton = new ControllButton(Image.FromResource("Baimp.Resources.icoStop-Normal.png"));
			stopButton.Size = playButton.Size;
			stopButton.TooltipText = "Stop pipeline execution";
			stopButton.ButtonPressed += delegate {
				currentPipeline.StopExecution();
				stopButton.Disable();
			};


			playStopButtonPlacement = new FrameBox();
			playStopButtonPlacement.Content = playButton;

			controllbar.PackStart(
				playStopButtonPlacement, false, WidgetPlacement.Center, WidgetPlacement.Center, 8);

			// tab bar
			tabHost = new CustomTabHost();
			tabHost.Closeable = true;
			tabHost.HeightRequest = 24;

			tabHost.TabClose += OnTabClose;

			controllbar.PackStart(tabHost, false, WidgetPlacement.End, WidgetPlacement.Start);

			// tab add worksheet 
			ImageView addWorksheet = new ImageView();
			addWorksheet.Image = Image.FromResource("Baimp.Resources.btAdd.png").WithBoxSize(14);
			addWorksheet.MouseEntered += delegate {
				addWorksheet.Image = addWorksheet.Image.WithAlpha(0.8);
			};
			addWorksheet.MouseExited += delegate {
				addWorksheet.Image = addWorksheet.Image.WithAlpha(1.0);
			};
			addWorksheet.ButtonPressed += OnWorksheetAdd;

			controllbar.PackStart(addWorksheet, false, WidgetPlacement.Center, WidgetPlacement.Start);

			splitControllTab_pipelineScroller = new VBox();
			splitControllTab_pipelineScroller.Spacing = 0;
			splitControllTab_pipelineScroller.PackStart(controllbar, false, margin: 0.0);

			ReloadProjectMap();
		}

		/// <summary>
		/// Initializes the events.
		/// </summary>
		private void InitializeEvents()
		{
			project.ProjectChanged += delegate(object sender, ProjectChangedEventArgs e) {
				OnProjectDataChangged(e);
				ReloadProjectMap();
			};

			foreach (PipelineView pView in pipelines.Values) {
				pView.DataChanged += delegate(object sender, SaveStateEventArgs e) {
					MainWindow mainWindow = pipelineShelf.ParentWindow as MainWindow;
					if (mainWindow != null) {
						mainWindow.MarkAsUnsaved();
					}
				};
			}

			project.PipelineExecuted += delegate {
				playStopButtonPlacement.Content = stopButton;
			};

			project.PipelineFinished += delegate {
				playButton.Enable();
				playStopButtonPlacement.Content = playButton;
			};

		}

		/// <summary>
		/// Reloads the project map combobox
		/// </summary>
		void ReloadProjectMap()
		{
			tabHost.SelectionChanged -= OnProjectMapSelectionChanged;

			tabHost.Clear();
			foreach (PipelineView pView in pipelines.Values) {
				tabHost.Add(pView.PipelineName);
			}
			tabHost.SelectedIndex = 0;

			tabHost.SelectionChanged += OnProjectMapSelectionChanged;
		}

		#region events

		public void OnProjectDataChangged(ProjectChangedEventArgs e)
		{
			if (e != null) {
				if (e.refresh) {
					pipelines.Clear();
					if (project.LoadedPipelines != null && project.LoadedPipelines.Count != 0) {

						foreach (PipelineNodeWrapper wrapper in project.LoadedPipelines) {
							PipelineView newPipeline = new PipelineView();
							newPipeline.Initialize(pipelineScroller, wrapper.pNodes, wrapper.scrollX, wrapper.scrollY);
							newPipeline.PipelineName = wrapper.name;
							pipelines.Add(newPipeline.PipelineName, newPipeline);

							newPipeline.DataChanged += delegate(object sender, SaveStateEventArgs e2) {
								MainWindow mainWindow = pipelineShelf.ParentWindow as MainWindow;
								if (mainWindow != null) {
									mainWindow.MarkAsUnsaved();
								}
							};
						}

						CurrentPipeline = pipelines.Values.ToArray()[0];
					} else {
						CurrentPipeline = new PipelineView();
						CurrentPipeline.Initialize(pipelineScroller);
					}
				} else if (e.addedFiles != null && e.addedFiles.Length > 0) {
				}
			}
		}

		/// <summary>
		/// Gets called when worksheet selection should change.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnProjectMapSelectionChanged(object sender, EventArgs e)
		{
			if (tabHost.SelectedItem != null) {
				CurrentPipeline = pipelines[tabHost.SelectedItem.Label];
			}
		}

		/// <summary>
		/// Get called when a new worksheet should be added.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		void OnWorksheetAdd(object sender, EventArgs e)
		{
			Tuple<Command, string> ret = WorksheetNameDialog();
			Command r = ret.Item1;
			if (r != null && r.Id == Command.Ok.Id) {
				PipelineView newPipeline = new PipelineView();
				newPipeline.Initialize(pipelineScroller);
				newPipeline.PipelineName = ret.Item2;
				pipelines.Add(newPipeline.PipelineName, newPipeline);
				CurrentPipeline = newPipeline;

				tabHost.Add(newPipeline.PipelineName);
				tabHost.SelectedIndex = tabHost.Count - 1;

				Log.Add(LogLevel.Info, this.GetType().Name,
					"Added new worksheet \"" + newPipeline.PipelineName + "\"");
			}
		}

		/// <summary>
		/// Gets called when a worksheet tab was closed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		void OnTabClose(object sender, CloseEventArgs e)
		{
			TabButton button = sender as TabButton;
			if (button != null) {
				Dialog d = new Dialog();
				d.Title = "Remove worksheet";

				d.Content = new Label(string.Format("Remove worksheet \"{0}\"?", button.Label));
				d.Buttons.Add(new DialogButton("Remove", Command.Ok));
				d.Buttons.Add(new DialogButton(Command.Cancel));

				Command r = d.Run();
				if (r.Id == Command.Ok.Id) {
					pipelines.Remove(button.Label);

					Log.Add(LogLevel.Info, this.GetType().Name,
						"Removed worksheet \"" + button.Label + "\"");

					PipelineView nextPipeline;
					pipelines.TryGetValue(tabHost.SelectedItem.Label, out nextPipeline);

					if (nextPipeline != null) {
						CurrentPipeline = nextPipeline;
					}

					e.Close = true;
				} else {
					e.Close = false;
				}

				d.Dispose();
			}
		}

		#endregion

		#region dialogs

		/// <summary>
		/// Dialog to choose name for a worksheet
		/// </summary>
		/// <returns>The user chosed name of the worksheet.</returns>
		/// <param name="oldName">Optional old name of an existing worksheet</param>
		public Tuple<Command, string> WorksheetNameDialog(string oldName = "")
		{
			Dialog d = new Dialog();
			d.Title = "Choose name";
			TextEntry nameEntry = new TextEntry();
			nameEntry.PlaceholderText = "Name";

			bool createNew = true;
			if (!string.IsNullOrEmpty(oldName)) {
				nameEntry.Text = oldName;
				createNew = false;
			}

			d.Content = nameEntry;
			d.Buttons.Add(new DialogButton(createNew ? "Create Worksheet" : "Rename Worksheet", Command.Ok));
			d.Buttons.Add(new DialogButton(Command.Cancel));

			Command r;
			while ((r = d.Run()) != null &&
			       r.Id != Command.Cancel.Id &&
			       (nameEntry.Text.Length < 3 || pipelines.ContainsKey(nameEntry.Text))) {

				if (nameEntry.Text.Length < 3) {
					MessageDialog.ShowMessage("Worksheets name must consist of at least 3 letters.");
				} else if (pipelines.ContainsKey(nameEntry.Text)) {
					MessageDialog.ShowMessage("Worksheet name already taken.");
				}
			}

			string text = nameEntry.Text;
			d.Dispose();

			return new Tuple<Command, string>(r, text);
		}

		#endregion

		/// <summary>
		/// Show dialog to rename the current worksheet.
		/// </summary>
		public void RenameCurrentWorksheetDialog()
		{
			Tuple<Command, string> ret = WorksheetNameDialog(CurrentPipeline.PipelineName);
			if (ret.Item1 != null && ret.Item1.Id == Command.Ok.Id) {
				pipelines.Remove(CurrentPipeline.PipelineName);
				pipelines.Add(ret.Item2, CurrentPipeline);
				CurrentPipeline.PipelineName = ret.Item2;

				tabHost.SelectedItem.Label = ret.Item2;
			}
		}

		#region properties

		/// <summary>
		/// Gets or sets the current shown pipeline.
		/// </summary>
		/// <value>The current pipeline.</value>
		public PipelineView CurrentPipeline {
			get {
				return currentPipeline;
			}
			set {
				if (currentPipeline != null) {
					CurrentPipeline.Dispose();
				}

				currentPipeline = value;
				currentPipeline.ExpandHorizontal = true;
				currentPipeline.ExpandVertical = true;
				pipelineScroller.Content = currentPipeline;
			}
		}

		/// <summary>
		/// Collection of all worksheets.
		/// </summary>
		/// <value>The pipelines.</value>
		public PipelineCollection Pipelines {
			get {
				return pipelines;
			}
			set {
				pipelines = value;
			}
		}

		#endregion

		#region IDisposable implementation
		public void Dispose()
		{
			currentPipeline.Dispose();
			pipelineScroller.Dispose();

		}
		#endregion
	}
}

