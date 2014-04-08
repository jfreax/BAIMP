using System;

namespace Baimp
{
	public class MyWaitCallback : PriorityQueueNode
	{
		public MyWaitCallback(ManagedThreadPool.WaitingCallback wc)
		{
			Waiting = wc;
		}

		public ManagedThreadPool.WaitingCallback Waiting {
			get;
			private set;
		}
	}
}

