using System;
using Xwt.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Baimp
{
	public class Result : IDisposable
	{
		private object removeLock = new object();

		private bool preserve;
		private readonly HashSet<PipelineNode> usedBy = new HashSet<PipelineNode>();
		public readonly IType Data;

		public Result(ref IType data, bool preserve = false)
		{
			this.Data = data;
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
					Data.Dispose();
				}
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Baimp.Result"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Baimp.Result"/>. The <see cref="Dispose"/>
		/// method leaves the <see cref="Baimp.Result"/> in an unusable state. After calling <see cref="Dispose"/>, you must
		/// release all references to the <see cref="Baimp.Result"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Baimp.Result"/> was occupying.</remarks>
		public void Dispose()
		{
			Data.Dispose();
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

