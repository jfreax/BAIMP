using System;
using Xwt;
using System.Collections.Generic;

namespace Baimp
{
	public class ResultPopupView : VBox
	{
		List<Tuple<IType[], Result[]>> results;
		int position;
		Widget widget = null;
		ComboBox combo = new ComboBox();


		public ResultPopupView(List<Tuple<IType[], Result[]>> results, int position)
		{
			this.results = results;
			this.position = position;

			// TODO search all nodes until we find the right root
			// right root == node without further input data and from type "BaseScan"
			foreach (var result in results) {
				Result[] input = result.Item2;
				while (true) {
					if (input[0].Input == null || input[0].Input.Length == 0) {
						break;
					}
					input = input[0].Input;
				}
				combo.Items.Add(input[0].Data);
			}

			InitializeEvents();

			this.PackStart(combo);
			combo.SelectedIndex = 0;
		}

		private void InitializeEvents()
		{
			combo.SelectionChanged += delegate(object sender, EventArgs e) {
				if (widget != null) {
					this.Remove(widget);
				}

				widget = results[combo.SelectedIndex].Item1[position].ToWidget();
				this.PackEnd( widget );
			};

			this.KeyPressed += delegate(object sender, KeyEventArgs e) {
				if(e.Key == Key.Escape) {
					this.Hide();
				}
			};
		}

		protected override void Dispose(bool disposing)
		{
			if (widget != null) {
				this.Remove(widget);
			}
			base.Dispose(disposing);
		}
	}
}

