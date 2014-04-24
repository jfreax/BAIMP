using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class LightImageWidget
	{
		public LightImageWidget(Image image)
		{
			this.Image = image;
			Visible = false;
		}

		public void OnButtonPressed(object sender, ButtonEventArgs e)
		{
			if (buttonPressed != null) {
				buttonPressed(sender, e);
			}
		}

		public Image Image {
			get;
			set;
		}

		public Rectangle Bounds {
			get;
			set;
		}

		public bool Visible {
			get;
			set;
		}


		EventHandler<ButtonEventArgs> buttonPressed;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<ButtonEventArgs> ButtonPressed {
			add {
				buttonPressed += value;
			}
			remove {
				buttonPressed -= value;
			}
		}
	}
}

