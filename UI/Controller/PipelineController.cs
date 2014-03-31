﻿using System;
using System.Linq;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;

namespace Baimp
{
	public class PipelineController
	{
		private FrameBox controllbarShelf;
		private FrameBox pipelineShelf;

		private HBox controllbar;
		private Project project;
		private ScrollView pipelineScroller;
		private AlgorithmTreeView algorithm;

		private ComboBox projectMap;

		PipelineCollection pipelines = new PipelineCollection();
		PipelineView currentPipeline;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.PipelineController"/> class.
		/// </summary>
		/// <param name="project">Project.</param>
		/// <param name="controllbarShelf">Frame where the controllbar should be.</param>
		/// <param name="pipelineShelf">Frame where the pipeline should be.</param>
		public PipelineController(Project project, FrameBox controllbarShelf, FrameBox pipelineShelf)
		{
			this.controllbarShelf = controllbarShelf;
			this.pipelineShelf = pipelineShelf;

			pipelineScroller = new ScrollView();
			pipelineScroller.MinHeight = (PipelineNode.NodeSize.Height + PipelineNode.NodeMargin.VerticalSpacing) * 6;
			pipelineScroller.Content = currentPipeline;

			if (project.LoadedPipelines == null || project.LoadedPipelines.Count == 0) {
				CurrentPipeline = new PipelineView();
				CurrentPipeline.Initialize(pipelineScroller);
				pipelines.Add(currentPipeline.PipelineName, currentPipeline);
			} else {
				foreach (PipelineNodeWrapper wrapper in project.LoadedPipelines) {
					PipelineView newPipeline = new PipelineView();
					newPipeline.Initialize(pipelineScroller, wrapper.pNodes);
					pipelines.Add(newPipeline.PipelineName, newPipeline);
				}
				CurrentPipeline = pipelines.Values.ToList()[0];
			}

			this.project = project;

			algorithm = new AlgorithmTreeView();
			algorithm.MinWidth = 200;

			HPaned splitPipeline_Algorithm = new HPaned();
			splitPipeline_Algorithm.Panel2.Content = pipelineScroller;
			splitPipeline_Algorithm.Panel2.Resize = true;
			splitPipeline_Algorithm.Panel2.Shrink = false;
			splitPipeline_Algorithm.Panel1.Content = algorithm;
			splitPipeline_Algorithm.Panel1.Resize = false;
			splitPipeline_Algorithm.Panel1.Shrink = false;


			pipelineShelf.Content = splitPipeline_Algorithm;

			InitializeControllerbar();
			InitializeEvents();
		}

		/// <summary>
		/// Initializes the controllerbar.
		/// </summary>
		private void InitializeControllerbar()
		{
			controllbar = new HBox();
			ControllButton playButton = new ControllButton(Image.FromResource("Baimp.Resources.play.png"));
			ControllButton pauseButton = new ControllButton(Image.FromResource("Baimp.Resources.pause.png"));
			ControllButton stopButton = new ControllButton(Image.FromResource("Baimp.Resources.stop.png"));

			playButton.ButtonPressed += (object sender, ButtonEventArgs e) => currentPipeline.Execute(project);

			projectMap = new ComboBox();
			foreach (PipelineView pView in pipelines.Values) {
				projectMap.Items.Add(pView.PipelineName);
			}
			projectMap.Items.Add("Add new worksheet...");
			projectMap.SelectedIndex = 0;
			projectMap.MinWidth = 100;

			playButton.MarginLeft = 8;
			controllbar.PackStart(playButton, false, true);
			controllbar.PackStart(pauseButton, false, true);
			controllbar.PackStart(stopButton, false, true);

			controllbar.PackStart(projectMap, false, false);
			controllbarShelf.Content = controllbar;
		}

		/// <summary>
		/// Initializes the events.
		/// </summary>
		private void InitializeEvents()
		{
			project.ProjectChanged += (object sender, ProjectChangedEventArgs e) => OnProjectDataChangged(e);

			foreach (PipelineView pView in pipelines.Values) {
				pView.DataChanged += delegate(object sender, SaveStateEventArgs e) {
					if (!controllbarShelf.ParentWindow.Title.EndsWith("*", StringComparison.Ordinal)) {
						controllbarShelf.ParentWindow.Title += "*";
					}
				};
			}

			projectMap.SelectionChanged += delegate(object sender, EventArgs e) {
				if (projectMap.SelectedIndex == projectMap.Items.Count-1) {
					Tuple<Command, string> ret = WorksheetNameDialog();
					Command r = ret.Item1;
					if(r != null && r.Id == Command.Ok.Id) {
						Console.WriteLine(ret.Item2);
						PipelineView newPipeline = new PipelineView();
						newPipeline.Initialize(pipelineScroller);
						newPipeline.PipelineName = ret.Item2;
						pipelines.Add(newPipeline.PipelineName, newPipeline);
						CurrentPipeline = newPipeline;

						projectMap.Items.Insert(projectMap.Items.Count-1, newPipeline.PipelineName);
						projectMap.SelectedIndex = projectMap.Items.Count-2;
					} else {
						int idx = projectMap.Items.IndexOf(CurrentPipeline.PipelineName);
						projectMap.SelectedIndex = idx;
					}
				} else if(projectMap.SelectedItem != null) {
					Console.WriteLine("Selected " + projectMap.SelectedItem);
					CurrentPipeline = pipelines[projectMap.SelectedItem as string];
				}
			};
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
							newPipeline.Initialize(pipelineScroller, wrapper.pNodes);
							pipelines.Add(newPipeline.PipelineName, newPipeline);
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

		#endregion

		#region dialogs

		/// <summary>
		/// Dialog to choose name for a worksheet
		/// </summary>
		/// <returns>The user chosed name of the worksheet.</returns>
		/// <param name="oldName">Optional old name of an existing worksheet</param>
		public Tuple<Command, string> WorksheetNameDialog(string oldName = "")
		{
			Dialog d = new Dialog ();
			d.Title = "Choose name";
			TextEntry nameEntry = new TextEntry();
			nameEntry.PlaceholderText = "Name";

			bool createNew = true;
			if(!string.IsNullOrEmpty(oldName)) {
				nameEntry.Text = oldName;
				createNew = false;
			}

			d.Content = nameEntry;
			d.Buttons.Add (new DialogButton (createNew ? "Create Worksheet" : "Rename Worksheet", Command.Ok));
			d.Buttons.Add (new DialogButton (Command.Cancel));

			Command r;
			while((r = d.Run()) != null && r.Id != Command.Cancel.Id && nameEntry.Text.Length < 3) {
				MessageDialog.ShowMessage ("Worksheets name must consist of at least 3 letters.");
			}

			string text = nameEntry.Text;
			d.Dispose();

			return new Tuple<Command, string>(r, text);
		}

		#endregion

		public void RenameCurrentWorksheetDialog()
		{
			Tuple<Command, string> ret = WorksheetNameDialog(CurrentPipeline.PipelineName);
			if (ret.Item1 != null && ret.Item1.Id == Command.Ok.Id) {
				pipelines.Remove(CurrentPipeline.PipelineName);
				pipelines.Add(ret.Item2, CurrentPipeline);
				CurrentPipeline.PipelineName = ret.Item2;

				int idx = projectMap.SelectedIndex;
				projectMap.Items.RemoveAt(projectMap.SelectedIndex);
				projectMap.Items.Insert(idx, ret.Item2);
				projectMap.SelectedIndex = idx;
			}
		}

		#region properties

		public PipelineView CurrentPipeline {
			get {
				return currentPipeline;
			}
			set {
				currentPipeline = value;
				currentPipeline.ExpandHorizontal = true;
				currentPipeline.ExpandVertical = true;
				pipelineScroller.Content = currentPipeline;
			}
		}


		public PipelineCollection Pipelines {
			get {
				return pipelines;
			}
			set {
				pipelines = value;
			}
		}
		#endregion
	}
}

