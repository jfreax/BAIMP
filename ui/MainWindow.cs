using System;
using System.Windows.Forms;
using System.Drawing;

namespace bachelorarbeit_implementierung
{
	public class MainWindow : Form
	{
        // Container
        SplitContainer splitFiletreePreview;
        SplitContainer splitPreviewMetadata;
        TabControl previewTabControl;

        // Widgets
        PictureBox box;
        TreeView filetree;


		public MainWindow ()
		{
			ProgressBar progressBar = new ProgressBar ();
			this.Controls.Add(progressBar);
		}

		public void Initialize (Bitmap bmp)
		{
            // Split container
            splitFiletreePreview = new SplitContainer();
            splitFiletreePreview.Orientation = Orientation.Vertical;
            splitFiletreePreview.BorderStyle = BorderStyle.FixedSingle;
            splitFiletreePreview.Dock = DockStyle.Fill;

            splitPreviewMetadata = new SplitContainer();
            splitPreviewMetadata.Orientation = Orientation.Vertical;
            splitPreviewMetadata.BorderStyle = BorderStyle.FixedSingle;
            splitPreviewMetadata.Dock = DockStyle.Fill;

            // Vorschaufenster
            previewTabControl = new TabControl();
            previewTabControl.Dock = DockStyle.Fill;
            //previewTabControl.Appearance = TabAppearance.FlatButtons;

            box = new PictureBox();
            box.SizeMode = PictureBoxSizeMode.Zoom;
            box.Image = bmp;
            box.Dock = DockStyle.Fill;

            TabPage tabIntensity = new TabPage();
            tabIntensity.Text = "Intensity";
            tabIntensity.Controls.Add(box);

            TabPage tabTopography = new TabPage();
            tabTopography.Text = "Topography";
            //tabTopography.Controls.Add(box);

            TabPage tabColor = new TabPage();
            tabColor.Text = "Color";
            //tabColor.Controls.Add(box);

            previewTabControl.TabPages.Add(tabIntensity);
            previewTabControl.TabPages.Add(tabTopography);
            previewTabControl.TabPages.Add(tabColor);
            splitFiletreePreview.Panel2.Controls.Add(previewTabControl);


            // Scanauswahl
			filetree = new TreeView ();
			filetree.Dock = DockStyle.Left;
			filetree.BorderStyle = BorderStyle.Fixed3D;
			filetree.Nodes.Add ("TreeView Node");
            filetree.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;


            splitFiletreePreview.Panel1.Controls.Add(filetree);
            splitFiletreePreview.Panel2.Controls.Add(splitPreviewMetadata);
            Controls.Add(splitFiletreePreview);

			//Controls.AddRange (new Control [] { box, splitter, treeView1});
		}
	}
}

