using System;
using System.IO;
using System.Collections.Generic;

namespace Baimp
{
	public class Result : IDisposable
	{
		object removeLock = new object();

		bool preserve;
		readonly Dictionary<PipelineNode, int> usedBy = new Dictionary<PipelineNode, int>();

		/// <summary>
		/// The payload.
		/// </summary>
		public IType Data;

		/// <summary>
		/// The input that was used to compute these data.
		/// </summary>
		public readonly Result[] Input;

		public Result(IType data, Result[] input, bool preserve = false)
		{
			this.Data = data;
			this.Input = input;
			this.preserve = preserve;
		}

		/// <summary>
		/// Call when you use this data.
		/// </summary>
		public void Used(PipelineNode by)
		{
			lock (removeLock) {
				if (!usedBy.ContainsKey(by)) {
					usedBy[by] = 0;
				}
				usedBy[by]++;
			}
		}

		/// <summary>
		/// Call when finished using these data.
		/// </summary>
		public void Finish(PipelineNode by)
		{
			lock (removeLock) {
				if (usedBy.ContainsKey(by)) {
					usedBy[by] = usedBy[by]-1;
					if(usedBy[by] <= 0) {
						usedBy.Remove(by);

						if (!preserve) {
							Dispose();
						}
					}
				} else {
					if(!preserve) {
						Dispose();
					}
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
			if (Data != null) {
				Data.Dispose();
				Data = null;
			}
		}

		#region helper functions

		public bool IsUsed(PipelineNode by)
		{
			return usedBy.ContainsKey(by);
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

