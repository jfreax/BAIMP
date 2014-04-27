//
//  StatusBar.cs
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
using Xwt;
using System.Threading;
using Xwt.Drawing;

namespace Baimp
{
	public class StatusBar : HBox
	{
		readonly Timer timer;
		Label threadLabel = new Label();
		Label logEntry = new Label();
		int maxThreads;

		public StatusBar()
		{
			InitializeUI();

			timer = new Timer (UpdateThreadLabel, null, 1000, 1000);
			Log.LogAdded += 
				(object sender, LogEventArgs e) => 
				logEntry.Markup = 
					string.Format("<b>{0}:</b> <span color='{1}'>{2}</span>", 
						e.LogMessage.source, Log.LevelToColorString(e.LogMessage.logLevel), e.LogMessage.message);
		}

		void InitializeUI()
		{
			PackStart(logEntry, true);
			PackEnd(threadLabel);

			int completionPortThreads;
			ThreadPool.GetMaxThreads(out maxThreads, out completionPortThreads);
		}
			
		void UpdateThreadLabel(object o)
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

