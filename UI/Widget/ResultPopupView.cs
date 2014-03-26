using System;
using Xwt;
using System.Collections.Generic;

namespace baimp
{
	public class ResultPopupView : VBox
	{
		private List<IType[]> results;
		private int position;
		private Widget widget = null;
		private ComboBox combo = new ComboBox();


		public ResultPopupView(List<IType[]> results, int position)
		{
			this.results = results;
			this.position = position;

			for (int i = 0; i < results.Count; i++) {
				combo.Items.Add(i);
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

				widget = results[combo.SelectedIndex][position].ToWidget();
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

