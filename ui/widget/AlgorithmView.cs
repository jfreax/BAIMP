using System;
using Xwt;
using Xwt.Drawing;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace baimp
{
	public class AlgorithmView : TreeView
	{
		public DataField<object> nameCol;
		public TreeStore store;

		private Dictionary<string, List<BaseAlgorithm>> algorithmCollection;

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.AlgorithmView"/> class.
		/// </summary>
		public AlgorithmView ()
		{
			nameCol = new DataField<object> ();
			store = new TreeStore (nameCol);

			Type baseType = typeof(BaseAlgorithm);
			IEnumerable<Type> algorithms = AppDomain.CurrentDomain.GetAssemblies ()
				.SelectMany (s => s.GetTypes ())
				.Where(t => t.GetInterfaces().Contains(baseType));

			algorithmCollection = new Dictionary<string, List<BaseAlgorithm>> ();
			foreach (Type algorithm in algorithms) {

				BaseAlgorithm instance = Activator.CreateInstance(algorithm) as BaseAlgorithm;
				string algorithmType = instance.AlgorithmType.ToString ();

				if (!algorithmCollection.ContainsKey (algorithmType)) {
					algorithmCollection [algorithmType] = new List<BaseAlgorithm> ();
				}

				algorithmCollection [algorithmType].Add (instance);
			}

			InitializeUI ();
		}

		/// <summary>
		/// Initialize the user interface.
		/// </summary>
		private void InitializeUI()
		{
			this.Columns.Add ("Name", nameCol);

			this.DataSource = store;

			foreach (string key in algorithmCollection.Keys) {
				var p = store.AddNode (null).SetValue (nameCol, key).CurrentPosition;

				foreach (BaseAlgorithm algo in algorithmCollection[key]) {
					var v = store.AddNode (p)
						.SetValue (nameCol, algo)
						.CurrentPosition;
				}
			}
		}
	}
}

