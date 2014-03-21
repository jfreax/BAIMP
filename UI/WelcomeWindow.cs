using System;
using Xwt;
using System.IO;

namespace baimp
{
	public class WelcomeWindow : Window
	{
		public WelcomeWindow ()
		{
			// initialize global events
			Closed += OnClosing;

			// widgets
			VBox vbox = new VBox ();
			Button openProject = new Button ("Open project");
			Button newProject = new Button ("New project");

			vbox.PackStart (openProject);
			vbox.PackEnd (newProject);

			Content = vbox;

			// events
			openProject.Clicked += open;
			newProject.Clicked += save;

		}

		protected void open(object sender, EventArgs e)
		{
			OpenFileDialog openDialog = new OpenFileDialog ("Open Project");
			openDialog.Filters.Add (new FileDialogFilter ("BAIMP Project file", "*.baimp"));
			if (openDialog.Run ()) {
				Project project = new Project (openDialog.FileName);
				(new MainWindow (project)).Show ();
			}
		}

		protected void save(object sender, EventArgs e)
		{
			SaveFileDialog saveDialog = new SaveFileDialog ("New Project");
			saveDialog.Filters.Add (new FileDialogFilter ("BAIMP Project file", "*.baimp"));
			if (saveDialog.Run ()) {
				string filename = saveDialog.FileName;
				if (Path.GetExtension (filename) != "baimp") {
					filename = Path.GetDirectoryName(filename) + "/" + Path.GetFileNameWithoutExtension (filename) + ".baimp";
				}

				Project project = new Project (filename);
				this.Hide();
				(new MainWindow (project)).Show ();
			}
		}

		/// <summary>
		/// Raises the closing event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnClosing(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}
}

