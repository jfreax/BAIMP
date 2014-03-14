using System;
using System.Windows.Forms;
using System.Drawing;
using bachelorarbeit_implementierung.Properties;

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


        public MainWindow(Bitmap bmp)
        {
            this.Load += OnLoad;
            this.FormClosing += OnClosing;

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
            filetree = new TreeView();
            filetree.Dock = DockStyle.Left;
            filetree.BorderStyle = BorderStyle.Fixed3D;
            filetree.Nodes.Add("TreeView Node");
            filetree.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;


            splitFiletreePreview.Panel1.Controls.Add(filetree);
            splitFiletreePreview.Panel2.Controls.Add(splitPreviewMetadata);
            Controls.Add(splitFiletreePreview);

            //Controls.AddRange (new Control [] { box, splitter, treeView1});
        }

        private void OnLoad(object sender, EventArgs e)
        {
            // Set window location
            if (Settings.Default.WindowLocation != null)
            {
                this.Location = Settings.Default.WindowLocation;
            }

            // Set window size
            if (Settings.Default.WindowSize != null)
            {
                this.Size = Settings.Default.WindowSize;
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            // Copy window location to app settings
            Settings.Default.WindowLocation = this.Location;

            // Copy window size to app settings
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowSize = this.Size;
            }
            else
            {
                Settings.Default.WindowSize = this.RestoreBounds.Size;
            }

            // Save settings
            Settings.Default.Save();
        }
    }
}

