//
//  IExporter.cs
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

namespace Baimp
{
	public abstract class BaseExporter
	{
		protected PipelineView pipeline;
		protected string filename;

		protected BaseExporter(PipelineView pipeline)
		{
			this.pipeline = pipeline;
		}

		/// <summary>
		/// Shows a dialog to configure this export algorithm.
		/// </summary>
		public void ShowDialog()
		{
			Dialog d = new Dialog();
			d.Title = "Export as " + this;
			d.Content = Options();

			d.Buttons.Add(new DialogButton(Command.Save));
			d.Buttons.Add(new DialogButton(Command.Cancel));

			Command r = d.Run();
			if (r != null && r.Id == Command.Save.Id) {
				Run();
			}

			d.Dispose();
		}

		/// <summary>
		/// Get widget to change options of this exporter.
		/// </summary>
		/// <remarks>
		/// Output file name should be saved under the 'filename' variable.
		/// </remarks>
		public abstract Widget Options();

		/// <summary>
		/// Run the actual export algorithm.
		/// </summary>
		/// <remarks>
		/// Save result to 'filename'.
		/// </remarks>
		public abstract void Run();
	}
}

