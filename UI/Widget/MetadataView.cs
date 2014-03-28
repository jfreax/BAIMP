using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;

namespace baimp
{
	public class MetadataView : VBox
	{
		Label name;
		Dictionary<int, Widget>[] widgets;
		BaseScan currentScan;
		Table table;
		TextEntry entryFiberType;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.MetadataView"/> class.
		/// </summary>
		/// <remarks>
		/// Call <see cref="bachelorarbeit_implementierung.MetadataView.Load" to actual see the metadata information/> 
		/// </remarks>
		public MetadataView()
		{
			table = new Table();
			Expander expander = new Expander();
			expander.Expanded = true;
			expander.Content = table;
			this.PackStart(expander, true);

			widgets = new Dictionary<int, Widget>[2];
			widgets[0] = new Dictionary<int, Widget>();
			widgets[1] = new Dictionary<int, Widget>();

			name = new Label("Name");
			name.TextAlignment = Alignment.Center;
			table.Add(name, 0, 0, colspan: 2);

			InitializeUI();
		}

		/// <summary>
		/// Initializes the user interface and their events.
		/// </summary>
		private void InitializeUI()
		{
			entryFiberType = new TextEntry();
			entryFiberType.BackgroundColor = Colors.WhiteSmoke;
			entryFiberType.ShowFrame = false;

			entryFiberType.LostFocus += ChangeFiberType;
			entryFiberType.KeyPressed += delegate(object sender, KeyEventArgs e) {
				if (e.Key == Key.Return) {
					ChangeFiberType(sender, e);
					e.Handled = true;
				}

				e.Handled = false;
			};
		}

		/// <summary>
		/// Show metadata of specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void Load(BaseScan scan)
		{
			this.currentScan = scan;

			table.Clear();
			//table.Remove (name);
			name = new Label("Name");
			name.TextAlignment = Alignment.Center;
			name.Text = scan.ToString();
			table.Add(name, 0, 0, colspan: 2);

			int i = 1;
			// TODO load and show metadata
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
				
			entryFiberType.Text = scan.FiberType;
			table.Add(new Label("FiberType"), 0, i);
			table.Add(entryFiberType, 1, i++);
		}

		/// <summary>
		/// Changes the type of the fiber.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void ChangeFiberType(object sender, EventArgs e)
		{
			currentScan.FiberType = entryFiberType.Text;
		}
	}
}

