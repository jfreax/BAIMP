using System;

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

		public int InUse {
			get {
				return inUse;
			}
		}
	}
}

