using System;
using Xwt;
using Xwt.Drawing;

namespace baimp
{
	public class PipelineControllerView : VBox
	{
		private HBox controllbar;
		private PipelineView pipelineView;
		private Project project;
		private ScrollView pipelineScroller;
		private AlgorithmTreeView algorithm;

		public PipelineControllerView(PipelineView pipelineView, Project project)
		{
			this.pipelineView = pipelineView;
			this.project = project;

			pipelineScroller = pipelineView.Scrollview;
			pipelineScroller.MinHeight = (PipelineNode.NodeSize.Height + PipelineNode.NodeMargin.VerticalSpacing) * 6;
			pipelineScroller.Content = pipelineView;
			pipelineView.ExpandHorizontal = true;
			pipelineView.ExpandVertical = true;

			algorithm = new AlgorithmTreeView();
			algorithm.MinWidth = 200;

			HPaned splitPipeline_Algorithm = new HPaned();
			splitPipeline_Algorithm.Panel2.Content = pipelineScroller;
			splitPipeline_Algorithm.Panel2.Resize = true;
			splitPipeline_Algorithm.Panel2.Shrink = false;
			splitPipeline_Algorithm.Panel1.Content = algorithm;
			splitPipeline_Algorithm.Panel1.Resize = false;
			splitPipeline_Algorithm.Panel1.Shrink = false;


			this.PackEnd(splitPipeline_Algorithm, true, true);
			InitializeControllerbar();
		}

		private void InitializeControllerbar()
		{
			controllbar = new HBox();
			ControllButton playButton = new ControllButton(Image.FromResource("baimp.Resources.play.png"));
			ControllButton pauseButton = new ControllButton(Image.FromResource("baimp.Resources.pause.png"));
			ControllButton stopButton = new ControllButton(Image.FromResource("baimp.Resources.stop.png"));

			playButton.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				pipelineView.Execute(project);
			};

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
	}
}

