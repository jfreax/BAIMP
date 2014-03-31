using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class ControllButton : ImageView
	{
		public ControllButton()
		{
		}

		public ControllButton(Image image)
		{
			this.Image = image.WithSize(24).WithAlpha(0.6);
		}

		protected override void OnMouseEntered(EventArgs args)
		{
			base.OnMouseEntered(args);
			this.Image = this.Image.WithAlpha(1.0);
		}

		protected override void OnMouseExited(EventArgs args)
		{
			base.OnMouseExited(args);
			this.Image = this.Image.WithAlpha(0.6);
		}
	}
}

