using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;

namespace Baimp
{
	public class MetadataView : VBox
	{
		Dictionary<int, Widget>[] widgets;
		Table table;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.MetadataView"/> class.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Baimp.MetadataView.Load"></see> to actual see the metadata information/> 
		/// </remarks>
		public MetadataView()
		{
			table = new Table();
			this.PackStart(table, true);

			widgets = new Dictionary<int, Widget>[2];
			widgets[0] = new Dictionary<int, Widget>();
			widgets[1] = new Dictionary<int, Widget>();
		}

		/// <summary>
		/// Show metadata of specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void Load(BaseScan scan)
		{
			table.Clear();

			int i = 1;
			foreach (Metadata d in scan.Metadata) {
				table.Add(new Label(d.key), 0, i);

				TextEntry entry = new TextEntry();
				entry.Text = d.value;
				entry.ReadOnly = true;
				entry.ShowFrame = false;
				entry.BackgroundColor = Color.FromBytes(232, 232, 232);
				table.Add(entry, 1, i);

				i++;
			}
		}
	}
}

