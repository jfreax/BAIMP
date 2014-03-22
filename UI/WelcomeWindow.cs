using System;
using Xwt;
using System.IO;

namespace baimp
{
	public class WelcomeWindow : Window
	{
		public WelcomeWindow ()
		{
			Title = "BAIMP";

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
			Project project = new Project ();
			if (project.OpenDialog ()) {
				this.Hide ();
				(new MainWindow (project)).Show ();
			}
		}

		protected void save(object sender, EventArgs e)
		{
			Project project = new Project ();
			project.NewDialog ();

			this.Hide();
			(new MainWindow (project)).Show ();

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

