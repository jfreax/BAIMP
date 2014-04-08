using System;
using Xwt;
using Xwt.Drawing;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Baimp
{
	public class AlgorithmTreeView : TreeView
	{
		public DataField<object> nameCol;
		public TreeStore store;
		private Dictionary<string, List<BaseAlgorithm>> algorithmCollection;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.AlgorithmTreeView"/> class.
		/// </summary>
		public AlgorithmTreeView()
		{
			nameCol = new DataField<object>();
			store = new TreeStore(nameCol);

			Type baseType = typeof(BaseAlgorithm);
			IEnumerable<Type> algorithms = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(t => t.BaseType == baseType);

			algorithmCollection = new Dictionary<string, List<BaseAlgorithm>>();
			foreach (Type algorithm in algorithms) {

				BaseAlgorithm instance = Activator.CreateInstance(algorithm, (PipelineNode) null) as BaseAlgorithm;
				string algorithmType = instance.AlgorithmType.ToString();

				if (!algorithmCollection.ContainsKey(algorithmType)) {
					algorithmCollection[algorithmType] = new List<BaseAlgorithm>();
				}

				algorithmCollection[algorithmType].Add(instance);
			}

			InitializeUI();
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		private void InitializeUI()
		{
			this.Columns.Add("Name", nameCol);

			this.DataSource = store;

			foreach (string key in algorithmCollection.Keys) {
				var p = store.AddNode(null).SetValue(nameCol, key).CurrentPosition;

				foreach (BaseAlgorithm algo in algorithmCollection[key]) {
					store.AddNode(p).SetValue(nameCol, algo);
				}
			}

			this.ExpandAll();
		}

		protected override void OnSelectionChanged(EventArgs e)
		{
			if (SelectedRow == null)
				return;

			object value = store.GetNavigatorAt(SelectedRow).GetValue(nameCol);
			if (value is BaseAlgorithm) {
			
				TextLayout text = new TextLayout();
				text.Text = value.ToString();

				Size textSize = text.GetSize();
				var ib = new ImageBuilder(textSize.Width, textSize.Height);
				ib.Context.DrawTextLayout(text, 0, 0);

				var d = CreateDragOperation();
				d.Data.AddValue(value.GetType().AssemblyQualifiedName);
				d.SetDragImage(ib.ToVectorImage(), -6, -4);
				d.AllowedActions = DragDropAction.Link;
				d.Start();

				d.Finished += (object sender, DragFinishedEventArgs e2) => this.UnselectAll();

				text.Dispose();
				ib.Dispose();
			} else {
				this.UnselectRow(SelectedRow);
			}
		}
	}
}

