using System;
using Xwt;
using System.Collections.Generic;

namespace bachelorarbeit_implementierung
{
	public class MetadataView : Table
	{
		Label name;
		Dictionary<int, Widget>[] widgets;

		public MetadataView ()
		{
			widgets = new Dictionary<int, Widget>[2];
			widgets [0] = new Dictionary<int, Widget> ();
			widgets [1] = new Dictionary<int, Widget> ();

			name = new Label ("Name");
			name.TextAlignment = Alignment.Center;
			Add (name, 0, 0, colspan:2);


			//Add (new Label ("Two:"), 0, 1);
			//Add (new TextEntry (), 1, 1);
			//Add (new Label ("Threefcddddddddddddddddddddjjjjjjjjj:"), 0, 2, colspan:2);
			//t.Add (new TextEntry (), 1, 2);
			//InsertRow (1, 2);
			//Add (new Label ("One-and-a-half"), 0, 1);
			//Add (new TextEntry () { PlaceholderText = "Just inserted" }, 1, 1);
		}


		public void Load(Scan scan)
		{
			Remove (name);
			name = new Label ("Name");
			name.TextAlignment = Alignment.Center;
			name.Text = scan.ToString();
			Add (name, 0, 0, colspan:2);

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
				this.Remove (widgets[left][top]);
			}

			Add (widget, left, top, rowspan, colspan, hexpand, vexpand, hpos, vpos, marginLeft, marginTop, marginRight, marginBottom, margin);
			widgets [left] [top] = widget;
		}
	}
}

