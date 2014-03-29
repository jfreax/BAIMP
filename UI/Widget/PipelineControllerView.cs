using System;
using Xwt;

namespace baimp
{
	public class PipelineControllerView : VBox
	{
		private PipelineView pipelineView;

		public PipelineControllerView(PipelineView pipelineView)
		{
			this.pipelineView = pipelineView;

			this.PackStart(pipelineView);
		}
	}
}

