using System;
using Xwt;
using Xwt.Drawing;

namespace baimp
{
	public class AlgorithmView : TreeView
	{
		public DataField<object> nameCol;
		public TreeStore store;


		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.AlgorithmView"/> class.
		/// </summary>
		public AlgorithmView ()
		{
			nameCol = new DataField<object> ();
			store = new TreeStore (nameCol);
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		public void InitializeUI()
		{
			this.Columns.Add ("Name", nameCol);

			this.DataSource = store;

			if (MainClass.toolkitType == ToolkitType.Gtk) {
				this.MinWidth = this.ParentWindow.Width;
			}
		}
	}
}

