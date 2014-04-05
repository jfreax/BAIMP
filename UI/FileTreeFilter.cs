using System;
using Xwt;

namespace Baimp
{
	public class FileTreeFilter : TextEntry
	{
		FileTreeView fileTree;
		public FileTreeFilter(FileTreeView fileTree)
		{
			this.fileTree = fileTree;

			this.PlaceholderText = "Filter...";
		}

		protected override void OnKeyReleased(KeyEventArgs args)
		{
			base.OnKeyPressed(args);

			fileTree.Filter(Text);
		}
	}
}

