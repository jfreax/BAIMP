using System;
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
				foreach (List<PipelineNode> pNodes in project.LoadedPipelines) {

					PipelineView newPipeline = new PipelineView();
					newPipeline.Initialize(pipelineScroller, pNodes);
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
					Dialog d = new Dialog ();
					d.Title = "Choose name";
					TextEntry nameEntry = new TextEntry();
					nameEntry.PlaceholderText = "Name";
					d.Content = nameEntry;
					d.Buttons.Add (new DialogButton ("Create Worksheet", Command.Ok));
					d.Buttons.Add (new DialogButton (Command.Cancel));

					Command r;
					while((r = d.Run()) != null && r.Id != Command.Cancel.Id && nameEntry.Text.Length < 3) {
						MessageDialog.ShowMessage ("Worksheets name must consist of at least 3 letters.");
					}
					if(r != null && r.Id == Command.Ok.Id) {
						PipelineView newPipeline = new PipelineView();
						newPipeline.Initialize(pipelineScroller);
						newPipeline.PipelineName = nameEntry.Text;
						pipelines.Add(newPipeline.PipelineName, newPipeline);
						CurrentPipeline = newPipeline;

						projectMap.Items.Insert(projectMap.Items.Count-1, newPipeline.PipelineName);
						projectMap.SelectedIndex = projectMap.Items.Count-2;
					} else {
						int idx = projectMap.Items.IndexOf(CurrentPipeline.PipelineName);
						projectMap.SelectedIndex = idx;
					}
					d.Dispose();
				} else {
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

						foreach (List<PipelineNode> pNodes in project.LoadedPipelines) {
							PipelineView newPipeline = new PipelineView();
							newPipeline.Initialize(pipelineScroller, pNodes);
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

