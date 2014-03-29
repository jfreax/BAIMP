using System;
using Xwt;

namespace baimp
{
	public class PipelineControllerView : VBox
	{
		private HBox controllbar;
		private PipelineView pipelineView;
		private ScrollView pipelineScroller;

		public PipelineControllerView(PipelineView pipelineView)
		{
			this.pipelineView = pipelineView;

			InitializeControllerbar();


			pipelineScroller = pipelineView.Scrollview;
			pipelineScroller.MinHeight = (PipelineNode.NodeSize.Height + PipelineNode.NodeMargin.VerticalSpacing) * 6;
			pipelineScroller.Content = pipelineView;
			pipelineView.ExpandHorizontal = true;
			pipelineView.ExpandVertical = true;

			AlgorithmTreeView algorithm = new AlgorithmTreeView();
			algorithm.MinWidth = 200;

			HPaned splitPipeline_Algorithm = new HPaned();
			splitPipeline_Algorithm.Panel2.Content = pipelineScroller;
			splitPipeline_Algorithm.Panel2.Resize = true;
			splitPipeline_Algorithm.Panel2.Shrink = false;
			splitPipeline_Algorithm.Panel1.Content = algorithm;
			splitPipeline_Algorithm.Panel1.Resize = false;
			splitPipeline_Algorithm.Panel1.Shrink = false;


			this.PackStart(splitPipeline_Algorithm, true, true);
		}

		private void InitializeControllerbar()
		{
			controllbar = new HBox();
			Button testButton = new Button();
			testButton.Label = "Play";

			controllbar.PackEnd(testButton, false, false);

			this.PackStart(controllbar);
		}
	}
}

