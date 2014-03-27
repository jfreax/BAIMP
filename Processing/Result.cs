using System;
using Xwt.Drawing;
using System.IO;

namespace baimp
{
	public class Result
	{
		private int inUse = 0;
		private bool preserve;
		public readonly IType data;

		public Result(ref IType data, bool preserve = false)
		{
			this.data = data;
			this.preserve = preserve;
		}

		/// <summary>
		/// Call when you use this data
		/// </summary>
		public void Use()
		{
			inUse++;
		}

		/// <summary>
		/// Call when finished using these data
		/// </summary>
		public void Finish()
		{
			inUse--;
			if (inUse <= 0 && !preserve) {
				data.Dispose();
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="baimp.Result"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="baimp.Result"/>. The <see cref="Dispose"/>
		/// method leaves the <see cref="baimp.Result"/> in an unusable state. After calling <see cref="Dispose"/>, you must
		/// release all references to the <see cref="baimp.Result"/> so the garbage collector can reclaim the memory that the
		/// <see cref="baimp.Result"/> was occupying.</remarks>
		public void Dispose()
		{
			data.Dispose();
		}

		public int InUse {
			get {
				return inUse;
			}
		}
	}
}

