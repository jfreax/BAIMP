using System;
using System.Linq;

namespace Baimp
{
	public class Compatible
	{
		private readonly string name;
		private Type type;
		public readonly BaseConstraint[] Constraints;

		public Compatible()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.Compatible"/> class.
		/// </summary>
		/// <param name="name">Name of this compatible.</param>
		/// <param name="type">In- or output type.</param>
		/// <param name="constraints">Constraints.</param>
		public Compatible(string name, Type type, params BaseConstraint[] constraints)
		{
			this.name = name;
			this.type = type;
			this.Constraints = constraints;
		}

		/// <summary>
		/// Tests if another instance is compatible with this one
		/// </summary>
		/// <returns><c>true</c>, if compatible, <c>false</c> otherwise.</returns>
		/// <param name="nodeFrom"></param>
		/// <param name="nodeTo"></param>
		public bool Match(MarkerNode nodeFrom, MarkerNode nodeTo)
		{
			Compatible another = nodeTo.compatible;


			Type typeFrom = Type;
			Type typeTo = another.Type;

			if (typeFrom.IsGenericType) {
				typeFrom = typeFrom.GetGenericArguments()[0];
			}

			if (typeTo.IsGenericType) {
				typeTo = typeTo.GetGenericArguments()[0];
			}

			if (typeTo.IsInterface) {
				if (!typeFrom.GetInterfaces().Contains(typeTo)) {
					return false;
				}
			} else if (typeFrom.IsInterface) {
				if (!typeTo.GetInterfaces().Contains(typeFrom)) {
					return false;
				}
			} else {
				if (!typeFrom.Equals(typeTo)) {
					return false;
				}
			}
				
			foreach (BaseConstraint constraint in Constraints) {
				if (!constraint.FulFills(nodeFrom, nodeTo)) {
					return false;
				}
			}

			foreach (BaseConstraint constraint in another.Constraints) {
				if (!constraint.FulFills(nodeTo, nodeFrom)) {
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether this is an end part. Aka a feature(-list).
		/// </summary>
		/// <returns><c>true</c> if this instance is end; otherwise, <c>false</c>.</returns>
		public bool IsFinal(){
			if (Type.IsGenericType) {
				Type genericType = Type.GetGenericTypeDefinition();
				if (genericType == typeof(TFeatureList<int>).GetGenericTypeDefinition() ||
				   genericType == typeof(TFeature<int>).GetGenericTypeDefinition()) {

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Type of in-/output
		/// </summary>
		/// <value>The type.</value>
		public Type Type {
			get { return type; }
		}

		public override string ToString()
		{
			return name;
		}
	}
}

