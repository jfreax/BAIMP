using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;

namespace bachelorarbeit_implementierung
{
	public class ScanView : Table
	{
		public Dictionary<string, object> Data = new Dictionary<string, object> ();

		private double scale = 1.0f;
		private ImageView image;
		private ImageView mask;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanView"/> class.
		/// </summary>
		public ScanView() {
			image = new ImageView ();
			mask = new ImageView ();

			this.HorizontalPlacement = WidgetPlacement.Center;
			this.VerticalPlacement = WidgetPlacement.Center;

			this.Add (image, 0, 0);
			this.Add (mask, 0, 0);
		}

		public void Scale(double scale)
		{
			this.scale *= scale;

			if (image.Image != null) {
				image.Image = image.Image.Scale (scale);
			}
			if (mask.Image != null) {
				mask.Image = mask.Image.Scale (scale);
			}
		}


		/// <summary>
		/// Gets or sets the scan image to show.
		/// </summary>
		/// <value>The image.</value>
		public Image Image {
			get {
				return image.Image;
			}

			set {
				image.Image = value;
				if (value != null) {
					ImageBuilder ib = new ImageBuilder (30, 30);
					ib.Context.Arc (15, 15, 15, 0, 360);
					ib.Context.SetColor (Color.FromBytes(255, 0, 100));
					ib.Context.Fill ();
					ib.Context.SetColor (Colors.DarkKhaki);
					ib.Context.Rectangle (0, 0, 5, 5);
					ib.Context.Fill ();
					var img = ib.ToVectorImage ();

					mask.Image = img;

					//ImageBuilder ib = new ImageBuilder (value.Width, value.Height);
					//ib.Context.SetColor (Color.FromBytes(255, 0, 100));
					//ib.Context.Rectangle (new Rectangle (Point.Zero, new Size (100, 100)));
					//mask.Image = ib.ToBitmap ();
				}
			}
		}


		/// <summary>
		/// Gets or sets the mask.
		/// </summary>
		/// <value>The mask.</value>
		public Image Mask {
			get {
				return mask.Image;
			}

			set {
				mask.Image = value;
			}
		}
	}
}

