using System;
using Xwt.Drawing;
using System.IO;
using System.Collections.Generic;

namespace baimp
{
	public class Result
	{
		private object removeLock = new object();

		private bool preserve;
		private HashSet<PipelineNode> usedBy = new HashSet<PipelineNode>();
		public readonly IType data;

		public Result(ref IType data, bool preserve = false)
		{
			this.data = data;
			this.preserve = preserve;
		}

		/// <summary>
		/// Call when you use this data.
		/// </summary>
		public void Used(PipelineNode by)
		{
			usedBy.Add(by);
		}

		/// <summary>
		/// Call when finished using these data.
		/// </summary>
		public void Finish(PipelineNode by)
		{
			lock (removeLock) {
				usedBy.Remove(by);
				if (usedBy.Count <= 0 && !preserve) {
					data.Dispose();
				}
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

		#region helper functions

		public bool IsUsed(PipelineNode by)
		{
			return usedBy.Contains(by);
		}

		#endregion

		#region properties

		public int InUse {
			get {
				return usedBy.Count;
			}
		}

		#endregion
	}
}

