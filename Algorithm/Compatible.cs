using System;

namespace baimp
{
	public class Compatible
	{
		private string name;
		private Type type;
		public readonly Constraint[] constraints;

		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.Compatible"/> class.
		/// </summary>
		/// <param name="name">Name of this compatible.</param>
		/// <param name="type">In- or output type.</param>
		/// <param name="constraints">Constraints.</param>
		public Compatible (string name, Type type, params Constraint[] constraints)
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
		public bool Match(Compatible another)
		{
			// TODO test constraints
			return type.Equals (another.Type);
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

