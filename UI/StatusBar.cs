using System;
using Xwt;
using System.Threading;

namespace Baimp
{
	public class StatusBar : HBox
	{
		readonly Timer timer;
		Label threadLabel = new Label();
		int maxThreads;

		public StatusBar()
		{
			InitializeUI();

			timer = new Timer (UpdateThreadLabel, null, 1000, 1000);
		}

		private void InitializeUI()
		{
			PackEnd(threadLabel);

			int completionPortThreads;
			ThreadPool.GetMaxThreads(out maxThreads, out completionPortThreads);
		}
			
		private void UpdateThreadLabel(object o)
		{
			int workerThreads;
			int completionPortThreads;
			ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);

			Application.Invoke( () => threadLabel.Text = "#Threads: " + (maxThreads-workerThreads));
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			timer.Dispose();
		}

	}
}

