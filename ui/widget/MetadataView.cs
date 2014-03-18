using System;
using Xwt;
using System.Collections.Generic;

namespace bachelorarbeit_implementierung
{
	public class MetadataView : VBox
	{
		Label name;
		Dictionary<int, Widget>[] widgets;

		Table table;

		public MetadataView ()
		{
			table = new Table ();
			Expander expander = new Expander ();
			expander.Expanded = true;
			expander.Content = table;
			this.PackStart (expander, true);

			widgets = new Dictionary<int, Widget>[2];
			widgets [0] = new Dictionary<int, Widget> ();
			widgets [1] = new Dictionary<int, Widget> ();

			name = new Label ("Name");
			name.TextAlignment = Alignment.Center;
			table.Add (name, 0, 0, colspan:2);
		}


		public void Load(Scan scan)
		{
			table.Remove (name);
			name = new Label ("Name");
			name.TextAlignment = Alignment.Center;
			name.Text = scan.ToString();
			table.Add (name, 0, 0, colspan:2);

			int i = 1;
			foreach (Tuple<string, string> d in scan.generalMetadata) {
				Replace (new Label (d.Item1), 0, i);

				TextEntry entry = new TextEntry ();
				entry.Text = d.Item2;
				entry.ReadOnly = true;
				entry.ShowFrame = false;
				Replace (entry, 1, i);

				i++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Replace (Widget widget, int left, int top, int rowspan = 1, int colspan = 1, bool hexpand = false, bool vexpand = false, WidgetPlacement hpos = WidgetPlacement.Fill, WidgetPlacement vpos = WidgetPlacement.Fill, double marginLeft = -1.0, double marginTop = -1.0, double marginRight = -1.0, double marginBottom = -1.0, double margin = -1.0)
		{
			if (widgets [left].ContainsKey (top)) {
				table.Remove (widgets[left][top]);
			}

			table.Add (widget, left, top, rowspan, colspan, hexpand, vexpand, hpos, vpos, marginLeft, marginTop, marginRight, marginBottom, margin);
			widgets [left] [top] = widget;
		}
	}
}

