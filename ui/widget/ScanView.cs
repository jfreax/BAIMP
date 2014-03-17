using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using System.Threading;
using System.IO;

namespace bachelorarbeit_implementierung
{
	[Flags]
	public enum Pointer
	{
		None = 0,
		Left = 1,
		Middle = 2,
		Right = 4,
		ExtendedButton1 = 8,
		ExtendedButton2 = 16
	}

	public class ScanView : Table
	{
		Preview.MyCallBack imageLoadedCallback = null;

		public Dictionary<string, object> Data = new Dictionary<string, object> ();

		private ImageView image;
		private ImageView mask;
		private Scan scan;
		private ScanType currentShownType;

		// mouse actions
		Pointer pointer;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanView"/> class.
		/// </summary>
		public ScanView (Scan scan)
		{
			this.scan = scan;

			image = new ImageView ();
			mask = new ImageView ();

			this.HorizontalPlacement = WidgetPlacement.Center;
			this.VerticalPlacement = WidgetPlacement.Center;

			this.Add (image, 0, 0);
			this.Add (mask, 0, 0);
		}

		/// <summary>
		/// Scale scan and mask at once.
		/// </summary>
		/// <param name="scale">Scale factor.</param>
		public void Scale (double scale)
		{
			scan.ScaleImage (scale);
			image.Image = scan.GetAsImage (currentShownType);
			mask.Image = scan.GetMaskAsImage (currentShownType);
		}


		/// <summary>
		/// Display image of selected scan type
		/// </summary>
		/// <param name="type">Type.</param>
		private void ShowType (ScanType type)
		{
			currentShownType = type;

            scan.GetAsImageAsync(type, new bachelorarbeit_implementierung.Scan.ImageLoadedCallback(delegate(Image loadedImage)
            {
                image.Image = loadedImage;
                if (imageLoadedCallback != null)
                {
                    imageLoadedCallback(type);
                }
            }));
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
				pointer |= Pointer.Left;
				break;
			case PointerButton.Right:
				pointer |= Pointer.Right;
				break;
			}
		}


		protected override void OnButtonReleased(ButtonEventArgs e) {
			switch (e.Button) {
			case PointerButton.Left:
				pointer ^= Pointer.Left;
				break;
			case PointerButton.Right:
				pointer ^= Pointer.Right;
				break;
			}
		}


		protected override void OnMouseMoved(MouseMovedEventArgs e) {
			Point scaleFactor = scan.GetScaleFactor ();
			int pointerSize = 16;

			if (pointer.HasFlag(Pointer.Left)) {
				ImageBuilder ib = scan.GetMaskBuilder (currentShownType);
				ib.Context.Arc (e.X * scaleFactor.X, e.Y * scaleFactor.Y, 15, 0, 360);
				ib.Context.SetColor (Colors.Coral);
				ib.Context.Fill ();

				mask.Image = scan.GetMaskAsImage (currentShownType);
			} else if(pointer.HasFlag(Pointer.Right)) {
				ImageBuilder ib = scan.GetMaskBuilder (currentShownType);

				ib.Context.Save ();
				ib.Context.Arc (e.X * scaleFactor.X, e.Y * scaleFactor.Y, pointerSize, 0, 360);
				ib.Context.Clip ();

				Image i = image.Image.WithSize(scan.Size).ToBitmap ().Crop (new Rectangle (
					e.X * scaleFactor.X - pointerSize, e.Y * scaleFactor.Y - pointerSize,
					pointerSize * 2, pointerSize * 2
				));
				ib.Context.DrawImage (
					i, 
					new Point(e.X * scaleFactor.X - pointerSize, e.Y * scaleFactor.Y - pointerSize)
				);

				ib.Context.Fill ();
				ib.Context.Restore ();

				mask.Image = scan.GetMaskAsImage (currentShownType);
			}
		}


		protected override void OnMouseExited(EventArgs e) {
			pointer = Pointer.None;
		}


		/// <summary>
		/// Change the shown image to a size that fits in the provided size limits
		/// </summary>
		/// <param name="size">Max width and height</param>
		public void WithBoxSize(Size s) {
			if (image.Image != null) {
				image.Image = image.Image.WithBoxSize (s);
				scan.RequestedBitmapSize = image.Image.Size;
			}
			// TODO mask
		}


		public bool IsScaled() {
			return scan.IsScaled ();
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
				//Thread imageLoaderThread = new Thread (() => ShowType (value));
				//imageLoaderThread.Start ();
                ShowType(value);
			}
		}
	}
}

