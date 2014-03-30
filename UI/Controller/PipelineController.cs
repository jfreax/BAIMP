using System;
using System.Linq;
using Xwt;
using Xwt.Drawing;
using System.Collections.Generic;

namespace Baimp
{
	public class PipelineController : VBox
	{
		private HBox controllbar;
		private Project project;
		private ScrollView pipelineScroller;
		private AlgorithmTreeView algorithm;

		PipelineCollection pipelines = new PipelineCollection();
		PipelineView currentPipeline;


		public PipelineController(Project project)
		{
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


			PackEnd(splitPipeline_Algorithm, true, true);

			InitializeControllerbar();
			InitializeEvents();
		}

		private void InitializeControllerbar()
		{
			controllbar = new HBox();
			ControllButton playButton = new ControllButton(Image.FromResource("Baimp.Resources.play.png"));
			ControllButton pauseButton = new ControllButton(Image.FromResource("Baimp.Resources.pause.png"));
			ControllButton stopButton = new ControllButton(Image.FromResource("Baimp.Resources.stop.png"));

			playButton.ButtonPressed += (object sender, ButtonEventArgs e) => currentPipeline.Execute(project);

			ComboBox projectMap = new ComboBox();
			projectMap.Items.Add("Untitled");
			projectMap.Items.Add("Add new pipeline map...");
			projectMap.SelectedIndex = 0;
			projectMap.MinWidth = 100;

			playButton.MarginLeft = 8;
			controllbar.PackStart(playButton, false, true);
			controllbar.PackStart(pauseButton, false, true);
			controllbar.PackStart(stopButton, false, true);

			controllbar.PackStart(projectMap, false, false);
			this.PackStart(controllbar);
		}

		private void InitializeEvents()
		{
			project.ProjectChanged += (object sender, ProjectChangedEventArgs e) => OnProjectDataChangged(e);

			foreach (PipelineView pView in pipelines.Values) {
				pView.DataChanged += delegate(object sender, SaveStateEventArgs e) {
					if (!ParentWindow.Title.EndsWith("*", StringComparison.Ordinal)) {
						ParentWindow.Title += "*";
					}
				};
			}
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

