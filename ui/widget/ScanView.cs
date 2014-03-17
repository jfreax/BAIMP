using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using System.Threading;
using System.IO;

namespace bachelorarbeit_implementierung
{
	public class ScanView : Table
	{
		private object lock_image_loading = new object ();
		Preview.MyCallBack imageLoadedCallback = null;

		public Dictionary<string, object> Data = new Dictionary<string, object> ();

		private double scale = 1.0f;
		private ImageView image;
		private ImageView mask;
		private Scan scan;
		private ScanType currentShownType;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanView"/> class.
		/// </summary>
		public ScanView (Scan scan, ScanType scanType)
		{
			this.scan = scan;

			image = new ImageView ();
			mask = new ImageView ();

			this.HorizontalPlacement = WidgetPlacement.Center;
			this.VerticalPlacement = WidgetPlacement.Center;

			this.Add (image, 0, 0);
			this.Add (mask, 0, 0);

			ScanType = scanType;
		}

		/// <summary>
		/// Scale scan and mask at once.
		/// </summary>
		/// <param name="scale">Scale factor.</param>
		public void Scale (double scale)
		{
			this.scale *= scale;

			ScaleIntern (scale);
		}

		private void ScaleIntern (double scale)
		{
			if (image.Image != null) {
				image.Image = image.Image.Scale (scale);
			}
			if (mask.Image != null) {
				mask.Image = mask.Image.Scale (scale);
			}
		}


		/// <summary>
		/// Display image of selected scan type
		/// </summary>
		/// <param name="type">Type.</param>
		private void ShowType (ScanType type)
		{
			currentShownType = type;

			lock (this.lock_image_loading) {
				if (scan == null) {
					return;
				}

				Image rendered = scan.GetAsImage (type);
				Console.WriteLine (rendered.Height);

				Application.Invoke (delegate() {
					image.Image = rendered;
					ScaleIntern (scale);

					if (imageLoadedCallback != null) {
						imageLoadedCallback (type);
					}
				});
			}
		}

		/// <summary>
		/// Registers the image loaded callback.
		/// </summary>
		/// <param name="cb">Callback function.</param>
		public void RegisterImageLoadedCallback (bachelorarbeit_implementierung.Preview.MyCallBack cb)
		{
			this.imageLoadedCallback = cb;
		}


		protected override void OnButtonPressed(ButtonEventArgs e) {
			switch (e.Button) {
			case PointerButton.Left:
				Console.WriteLine ("bla");
				break;
			}
		}

		public void WithBoxSize(Size s) {
			if (image.Image != null) {
				image.Image = image.Image.WithBoxSize (s);
			}
			// TODO mask
		}

		/// <summary>
		/// Gets or sets the scan image to show.
		/// </summary>
		/// <value>The image.</value>
		public Image Image {
			get {
				return image.Image;
			}

			//set {
			//	image.Image = value;

//				if (value != null) {
//					ImageBuilder ib = new ImageBuilder (value.Width, value.Height);
//					ib.Context.Arc (150, 15, 15, 0, 360);
//					ib.Context.SetColor (Color.FromBytes(255, 0, 100));
//					ib.Context.Fill ();
//					ib.Context.SetColor (Colors.Aqua);
//					ib.Context.Rectangle (0, 0, 50, 50);
//					ib.Context.Fill ();
//					var img = ib.ToVectorImage ();
//
//					mask.Image = img;
//
//					//ImageBuilder ib = new ImageBuilder (value.Width, value.Height);
//					//ib.Context.SetColor (Color.FromBytes(255, 0, 100));
//					//ib.Context.Rectangle (new Rectangle (Point.Zero, new Size (100, 100)));
//					//mask.Image = ib.ToBitmap ();
//				}
				//}
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

		public ScanType ScanType {
			get {
				return currentShownType;
			}
			set {
				Thread imageLoaderThread = new Thread (() => ShowType (value));
				imageLoaderThread.Start ();
			}
		}
	}
}

