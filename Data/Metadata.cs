using System;

namespace baimp
{
	public class Metadata
	{
		public string key = "";
		public string value = "";

		public Metadata()
		{
		}

		public Metadata(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}
}

