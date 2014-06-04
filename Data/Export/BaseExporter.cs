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
using System.IO;

namespace Baimp
{
	public abstract class BaseExporter
	{
		protected PipelineView pipeline;
		protected string filename;

		Dialog dialog;

		protected BaseExporter(PipelineView pipeline)
		{
			this.pipeline = pipeline;
		}

		/// <summary>
		/// Shows a dialog to configure this export algorithm.
		/// </summary>
		public void ShowDialog()
		{
			if (dialog == null) {
				dialog = new Dialog();
				dialog.Title = "Export as " + this;
				dialog.Content = Options();

				dialog.Buttons.Add(new DialogButton("Export", Command.Save));
				dialog.Buttons.Add(new DialogButton(Command.Cancel));
			}

			dialog.Show();

			Command r = dialog.Run();
			if (r != null && r.Id == Command.Save.Id) {
				Log.Add(LogLevel.Info, "Exporter " + this.GetType().Name, "Start exporting results.");
				if (Run()) {
					Log.Add(LogLevel.Info, "Exporter " + this.GetType().Name, "Finish exporting results.");
					dialog.Hide();
				} else {
					ShowDialog();
				}
			} else if (r != null && r.Id == Command.Cancel.Id) {
				dialog.Hide();
			}
		}

		protected void Browse(object sender, EventArgs e)
		{
			SaveFileDialog d = new SaveFileDialog("Export " + this);
			d.Filters.Add(new FileDialogFilter("Arff", "*.arff"));
			d.Filters.Add(new FileDialogFilter("Other", "*.*"));

			if (Filename != null) {
				d.InitialFileName = Path.GetFileName(Filename);
				d.CurrentFolder = Path.GetDirectoryName(Filename);
			}
			if (d.Run()) {
				string tmpFilename = d.FileName;

				if (string.IsNullOrEmpty(Path.GetExtension(tmpFilename))) {
					tmpFilename += ".arff";
				}

				Filename = tmpFilename;
			}
		}

		/// <summary>
		/// Get widget to change options of this exporter.
		/// </summary>
		/// <remarks>
		/// Output file name should be saved under the 'Filename' variable.
		/// </remarks>
		public abstract Widget Options();

		/// <summary>
		/// Run the actual export algorithm.
		/// </summary>
		/// <remarks>
		/// Save result to 'Filename'.
		/// </remarks>
		public abstract bool Run();

		/// <summary>
		/// Path to output file.
		/// </summary>
		/// <value>The filename.</value>
		public virtual string Filename {
			get {
				return filename;
			}
			set {
				filename = value;
			}
		}
	}
}

