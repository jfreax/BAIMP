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

		Scan currentScan;

		Table table;
		TextEntry entryFiberType;


		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.MetadataView"/> class.
		/// </summary>
		/// <remarks>
		/// Call <see cref="bachelorarbeit_implementierung.MetadataView.Load" to actual see the metadata information/> 
		/// </remarks>
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

			InitializeUI ();
		}


		/// <summary>
		/// Initializes the user interface and their events.
		/// </summary>
		private void InitializeUI()
		{
			entryFiberType = new TextEntry ();
			entryFiberType.BackgroundColor = Colors.White;
			entryFiberType.ShowFrame = false;

			entryFiberType.LostFocus += ChangeFiberType;
			entryFiberType.KeyPressed += delegate(object sender, KeyEventArgs e) {
				if(e.Key == Key.Return) {
					ChangeFiberType(sender, e);
				}
			};
		}


		/// <summary>
		/// Show metadata of specified scan.
		/// </summary>
		/// <param name="scan">Scan.</param>
		public void Load(Scan scan)
		{
			this.currentScan = scan;

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
				entry.BackgroundColor = Color.FromBytes (232, 232, 232);
				Replace (entry, 1, i);

				i++;
			}
				
			entryFiberType.Text = scan.FiberType;
			Replace (new Label ("FiberType"), 0, i);
			Replace (entryFiberType, 1, i++);


		}

		/// <summary>
		/// Replace a widget on a given position with a new one
		/// </summary>
		public void Replace (Widget widget, int left, int top, int rowspan = 1, int colspan = 1, bool hexpand = false, bool vexpand = false, WidgetPlacement hpos = WidgetPlacement.Fill, WidgetPlacement vpos = WidgetPlacement.Fill, double marginLeft = -1.0, double marginTop = -1.0, double marginRight = -1.0, double marginBottom = -1.0, double margin = -1.0)
		{
			if (widgets [left].ContainsKey (top)) {
				table.Remove (widgets[left][top]);
			}

			table.Add (widget, left, top, rowspan, colspan, hexpand, vexpand, hpos, vpos, marginLeft, marginTop, marginRight, marginBottom, margin);
			widgets [left] [top] = widget;
		}


		private void ChangeFiberType(object sender, EventArgs e)
		{
			currentScan.FiberType = entryFiberType.Text;
		}
	}
}

