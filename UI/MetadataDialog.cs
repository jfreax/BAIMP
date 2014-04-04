using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class MetadataDialog : Dialog
	{
		BaseScan mScan;

		readonly TextEntry nameEntry = new TextEntry();
		readonly Label nameEntryUnedited = new Label();

		readonly TextEntry fiberTypeEntry = new TextEntry();
		readonly Label fiberTypeEntryUnedited = new Label();

		public MetadataDialog(BaseScan scan, Image thumbnail)
		{
			InitializeUI(scan, thumbnail);
			InitializeEvents();
		}

		private void InitializeUI(BaseScan scan, Image thumbnail)
		{
			mScan = scan;

			Icon = thumbnail;
			Title = scan.Name;

			Buttons.Add(new DialogButton(Command.Cancel));
			Buttons.Add(new DialogButton(Command.Apply));

			Font fontH1 = Font.SystemFont.WithStyle(FontStyle.Oblique).WithSize(16);
			Font fontH2 = fontH1.WithStyle(FontStyle.Normal).WithSize(12);

			VBox mainLayout = new VBox();
			Content = mainLayout;

			HBox overviewLayout = new HBox();
			VBox overviewDataLayout = new VBox();

			ImageView thumbnailView = new ImageView(thumbnail.WithBoxSize(96));
			nameEntryUnedited.Text = scan.Name;
			nameEntryUnedited.Font = fontH1;

			fiberTypeEntryUnedited.Text = scan.FiberType;
			fiberTypeEntryUnedited.Font = fontH2;

			nameEntry.Text = scan.Name;
			nameEntry.Visible = false;
			nameEntry.Font = fontH1;

			fiberTypeEntry.Text = scan.FiberType;
			fiberTypeEntry.Visible = false;
			fiberTypeEntry.Font = fontH2;

			overviewDataLayout.MarginTop = 4;
			overviewDataLayout.MarginLeft = 4;
			overviewDataLayout.PackStart(nameEntryUnedited);
			overviewDataLayout.PackStart(nameEntry);
			overviewDataLayout.PackStart(fiberTypeEntryUnedited);
			overviewDataLayout.PackStart(fiberTypeEntry);

			overviewLayout.PackStart(thumbnailView);
			overviewLayout.PackStart(overviewDataLayout, true, true);

			MetadataView mView = new MetadataView();
			mView.Load(scan);

			mainLayout.PackStart(overviewLayout);
			mainLayout.PackStart(mView);
		}

		void InitializeEvents()
		{
			nameEntryUnedited.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				nameEntryUnedited.Visible = false;
				nameEntry.Visible = true;
				nameEntry.SetFocus();
			};

			nameEntry.LostFocus += delegate(object sender, EventArgs e) {
				nameEntryUnedited.Visible = true;
				nameEntry.Visible = false;

				nameEntryUnedited.Text = nameEntry.Text;
			};	

			fiberTypeEntryUnedited.ButtonPressed += delegate(object sender, ButtonEventArgs e) {
				fiberTypeEntryUnedited.Visible = false;
				fiberTypeEntry.Visible = true;
				fiberTypeEntry.SetFocus();
			};

			fiberTypeEntry.LostFocus += delegate(object sender, EventArgs e) {
				fiberTypeEntryUnedited.Visible = true;
				fiberTypeEntry.Visible = false;

				fiberTypeEntryUnedited.Text = fiberTypeEntry.Text;
			};
		}

		public void Save()
		{
			mScan.Name = nameEntry.Text;
			mScan.FiberType = fiberTypeEntry.Text;
		}
	}
}

