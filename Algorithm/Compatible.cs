using System;

namespace baimp
{
	public class Compatible
	{
		private string name;
		private Type type;
		public readonly BaseConstraint[] constraints;

		public Compatible ()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.Compatible"/> class.
		/// </summary>
		/// <param name="name">Name of this compatible.</param>
		/// <param name="type">In- or output type.</param>
		/// <param name="constraints">Constraints.</param>
		public Compatible (string name, Type type, params BaseConstraint[] constraints)
		{
			this.name = name;
			this.type = type;
			this.constraints = constraints;
		}

		/// <summary>
		/// Tests if another instance is compatible with this one
		/// </summary>
		/// <returns><c>true</c>, if compatible, <c>false</c> otherwise.</returns>
		/// <param name="another">The other compatible instance.</param>
		public bool Match(MarkerNode nodeFrom, MarkerNode nodeTo)
		{
			Compatible another = nodeTo.compatible;

			if (!Type.Equals (another.Type)) {
				return false;
			}
				
			foreach (BaseConstraint constraint in constraints) {
				if (!constraint.FulFills (nodeFrom, nodeTo)) {
					return false;
				}
			}

			foreach (BaseConstraint constraint in another.constraints) {
				if (!constraint.FulFills (nodeTo, nodeFrom)) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Type of in-/output
		/// </summary>
		/// <value>The type.</value>
		public Type Type {
			get { return type; }
		}


		public override string ToString ()
		{
			return name;
		}
	}
}

