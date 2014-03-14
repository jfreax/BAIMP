using System;
using System.Windows.Forms;
using System.Drawing;

namespace bachelorarbeit_implementierung
{
	public class MainWindow : Form
	{
		PictureBox box;
		TreeView treeView1;
		Splitter splitter;

		public MainWindow (Bitmap bmp)
		{
//			Splitter splitter = new Splitter ();
//			splitter.Anchor = AnchorStyles.Top | AnchorStyles.Left;
//			splitter.Dock = DockStyle.Left;
//
//			//FlowLayoutPanel layout = new FlowLayoutPanel ();
//			//layout.Size = new System.Drawing.Size(228, 20);
//			//layout.TabIndex = 0;
//			//layout.Anchor = AnchorStyles.Top | AnchorStyles.Left;
//			//layout.Dock = DockStyle.Fill;
//
//			tree_view = new TreeView ();
//			tree_view.Anchor = AnchorStyles.Top | AnchorStyles.Left;
//			//tree_view.Dock = DockStyle.Fill;
//			//tree_view.Height = Height;
//
//			//layout.Controls.Add (tree_view);
//			//Controls.Add (tree_view);

			DockStyle teststyle = DockStyle.Left;

//
			box = new PictureBox ();
			box.SizeMode = PictureBoxSizeMode.Zoom;
			box.Image = bmp;
			box.Dock = DockStyle.Fill;
			//box.Click += new EventHandler (clickhandler);
			box.MouseDown += new MouseEventHandler (mouseup);


			treeView1 = new TreeView ();

			splitter = new Splitter ();
			splitter.Dock = teststyle;
			splitter.MinExtra = 0;
			splitter.MinSize = 0;
			splitter.BorderStyle = BorderStyle.Fixed3D;
			splitter.BackColor = Color.Red;
			splitter.SplitterMoving += new SplitterEventHandler(splithandler);
			splitter.SplitterMoved += new SplitterEventHandler(splittermoved);
			//splitter.MouseClick += new MouseEventHandler ();
			//splitter.MouseUp += new MouseEventHandler(mouseup);

			treeView1.Dock = teststyle;
			treeView1.BorderStyle = BorderStyle.Fixed3D;
			treeView1.Nodes.Add ("TreeView Node");

			Controls.AddRange (new Control [] { box, splitter, treeView1});

			this.ClientSize = new Size(500, 500);
			this.CreateControl();
		}


		public void splithandler(object sender, SplitterEventArgs e) {
			Console.WriteLine("SplitterMoving: SplitPosition: {0} (Event: split: {1},{2} mouse: {3},{4})", ((Splitter)sender).SplitPosition, e.SplitX, e.SplitY, e.X, e.Y);
		}

		public void splittermoved(object sender, SplitterEventArgs e) {
			Console.WriteLine("SplitterMoved: SplitPosition: {0} (Event: split: {1},{2} mouse: {3},{4})", ((Splitter)sender).SplitPosition, e.SplitX, e.SplitY, e.X, e.Y);
		}

		public void splitterClicked(object sender, MouseEventArgs e) {
		}

		public void mouseup(object sender, MouseEventArgs e) {
			Console.WriteLine("Got mouseup event " + e.Location);
		}

		public void clickhandler(object sender, EventArgs e) {
			Console.WriteLine (e.ToString());
			splitter.SplitPosition = 150;
		}


	}
}

