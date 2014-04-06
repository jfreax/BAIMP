using System;
using Xwt;

namespace Baimp
{
	public class StatusBar : HBox
	{
		Label threadLabel = new Label();

		public StatusBar()
		{
			InitializeUI();
			InitializeEvents();
		}

		private void InitializeUI()
		{
			PackEnd(threadLabel);
		}

		private void InitializeEvents()
		{
			ManagedThreadPool.ActiveThreadsChanged += UpdateThreadLabel;
		}

		private void UpdateThreadLabel(object sender, EventArgs e)
		{
			threadLabel.Text = "#Threads: " + (ManagedThreadPool.ActiveThreads+1);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			ManagedThreadPool.ActiveThreadsChanged -= UpdateThreadLabel;
		}
	}
}

