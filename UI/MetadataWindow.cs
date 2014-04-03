using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class MetadataWindow : Window
	{
		public MetadataWindow(BaseScan scan, Image thumbnail)
		{
			VBox mainLayout = new VBox();
			Content = mainLayout;

			HBox overviewLayout = new HBox();

			ImageView thumbnailView = new ImageView(thumbnail);
			overviewLayout.PackStart(thumbnailView);

			mainLayout.PackStart(overviewLayout);
		}
	}
}

