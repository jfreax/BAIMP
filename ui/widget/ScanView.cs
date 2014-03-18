using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using System.Threading;
using System.IO;

namespace baimp
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
		#region static member

		public static Color maskColor = Colors.DarkBlue;

		#endregion

		Preview.MyCallBack imageLoadedCallback = null;
		public Dictionary<string, object> Data = new Dictionary<string, object> ();
		private ImageView image;
		private ImageView mask;
		private ScanWrapper scan;
		private ScanType currentShownType;

		// mouse actions
		Pointer pointer;
		int pointerSize = 16;

		// state
		bool isEditMode = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="bachelorarbeit_implementierung.ScanView"/> class.
		/// </summary>
		public ScanView (ScanWrapper scan)
		{
			this.scan = scan;

			image = new ImageView ();
			mask = new ImageView ();

			this.HorizontalPlacement = WidgetPlacement.Center;
			this.VerticalPlacement = WidgetPlacement.Center;
			this.CanGetFocus = true;

			this.Add (image, 0, 0);
			this.Add (mask, 0, 0);
		
			// build context menu
			InitializeContextMenu ();

			// event subscribe
			scan.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
				if(e.Changed.Equals("mask_"+((int)currentShownType))) {
					mask.Image = scan.GetMaskAsImage (currentShownType);
				}
			};
		}

		#region contextmenu 

		private Menu contextMenu;
		private MenuItem contextEditMask;

		/// <summary>
		/// Initializes the context menu.
		/// </summary>
		private void InitializeContextMenu ()
		{
			contextMenu = new Menu ();

			contextEditMask = new MenuItem ("Edit mask");
			contextEditMask.UseMnemonic = true;
			contextEditMask.Clicked += delegate(object sender, EventArgs e) {
				EditMode ^= true;
			};
			contextMenu.Items.Add (contextEditMask);

			MenuItem contextSaveMask = new MenuItem ("Save changes");
			contextSaveMask.UseMnemonic = true;
			contextSaveMask.Clicked += delegate(object sender, EventArgs e) {
				SaveMask();
			};
			contextMenu.Items.Add (contextSaveMask);
		}

		#endregion

		/// <summary>
		/// Display image of selected scan type
		/// </summary>
		/// <param name="type">Type.</param>
		private void ShowType (ScanType type)
		{
			currentShownType = type;
			EditMode = false;

			scan.GetAsImageAsync (type, new baimp.Scan.ImageLoadedCallback (delegate(Image loadedImage) {
				image.Image = loadedImage;
				mask.Image = scan.GetMaskAsImage (currentShownType);
				if (imageLoadedCallback != null) {
					imageLoadedCallback (type);
				}
			}));
		}

		#region callback

		/// <summary>
		/// Registers the image loaded callback.
		/// </summary>
		/// <param name="cb">Callback function.</param>
		public void RegisterImageLoadedCallback (baimp.Preview.MyCallBack cb)
		{
			this.imageLoadedCallback = cb;
		}

		#endregion

		#region events

		protected override void OnButtonPressed (ButtonEventArgs e)
		{
			Point scaleFactor = scan.GetScaleFactor ();

			switch (e.Button) {
			case PointerButton.Left:
				pointer |= Pointer.Left;

				if (isEditMode) {
					scan.NotifyChange ("mask_"+((int)currentShownType));

                    Point positionInImage = new Point(e.X * scaleFactor.X, e.Y * scaleFactor.Y);
					ImageBuilder ib = scan.GetMaskBuilder (currentShownType);
					ib.Context.NewPath ();
                    ib.Context.MoveTo(positionInImage);

					SetMask (
                        positionInImage,
						Keyboard.CurrentModifiers.HasFlag (ModifierKeys.Control) ||
						Keyboard.CurrentModifiers.HasFlag (ModifierKeys.Command)
					);
				}
				break;
			case PointerButton.Right:
				pointer |= Pointer.Right;

				contextMenu.Popup ();
				break;
			}
		}

		protected override void OnButtonReleased (ButtonEventArgs e)
		{
			switch (e.Button) {
			case PointerButton.Left:
				pointer ^= Pointer.Left;

				if (isEditMode) {
					ImageBuilder ib = scan.GetMaskBuilder (currentShownType);
					ib.Context.ClosePath ();
				}
				break;
			case PointerButton.Right:
				pointer ^= Pointer.Right;
				break;
			}
		}

		protected override void OnMouseMoved (MouseMovedEventArgs e)
		{

			if (isEditMode) {
				Point scaleFactor = scan.GetScaleFactor ();

				if (pointer.HasFlag (Pointer.Left)) {
					SetMask (
						new Point (e.X * scaleFactor.X, e.Y * scaleFactor.Y),
						Keyboard.CurrentModifiers.HasFlag (ModifierKeys.Control) ||
						Keyboard.CurrentModifiers.HasFlag (ModifierKeys.Command)
					);
				}
			}
		}

		protected override void OnMouseExited (EventArgs e)
		{
			pointer = Pointer.None;

			if (isEditMode) {
				ImageBuilder ib = scan.GetMaskBuilder (currentShownType);
				ib.Context.ClosePath ();
			}
		}

		protected override void OnMouseEntered(EventArgs e)
		{
			this.SetFocus ();
		}

		#endregion

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
		/// Set mask at given position.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="unset">If set to <c>true</c> delete mask on position.</param>
		private void SetMask (Point position, bool unset = false)
		{
			if (unset) {
				ImageBuilder ib = scan.GetMaskBuilder (currentShownType);

				ib.Context.Save ();
				ib.Context.Arc (position.X, position.Y, pointerSize, 0, 360);
				ib.Context.Clip ();

				double newX = Math.Min (Math.Max (position.X - pointerSize, 0), scan.Size.Width);
				double newY = Math.Min (Math.Max (position.Y - pointerSize, 0), scan.Size.Height);

				Image i = image.Image.WithSize (scan.Size).ToBitmap ().Crop (
					          new Rectangle (newX, newY, pointerSize * 2, pointerSize * 2)
				          );

				ib.Context.DrawImage (i, new Point (newX, newY));
				ib.Context.Fill ();
				ib.Context.Restore ();

			} else {
				ImageBuilder ib = scan.GetMaskBuilder (currentShownType);
				ib.Context.SetLineWidth (pointerSize*2);

				ib.Context.SetColor (maskColor);
				ib.Context.LineTo (position);
				ib.Context.Stroke ();

				ib.Context.Arc (position.X, position.Y, pointerSize, 0, 360);
				ib.Context.Fill ();

				ib.Context.MoveTo (position);
			}

			mask.Image = scan.GetMaskAsImage (currentShownType);
		}

		/// <summary>
		/// Save mask into metadata file
		/// </summary>
		public void SaveMask() {
			scan.SaveMask (currentShownType);
			mask.Image = scan.GetMaskAsImage(currentShownType);
		}

		/// <summary>
		/// Change the shown image to a size that fits in the provided size limits
		/// </summary>
		/// <param name="size">Max width and height</param>
		public void WithBoxSize (Size s)
		{
			if (image.Image != null) {
				image.Image = image.Image.WithBoxSize (s);
				scan.RequestedBitmapSize = image.Image.Size;

				if (mask.Image != null) {
					mask.Image = mask.Image.WithBoxSize (s);
				}
			}
		}

		/// <summary>
		/// Determines whether the image of this scan is scaled.
		/// </summary>
		/// <returns><c>true</c> if this instance is scaled; otherwise, <c>false</c>.</returns>
		public bool IsScaled ()
		{
			return scan.IsScaled ();
		}

		#region getter/setter

		/// <summary>
		/// Gets or sets the scan image to show.
		/// </summary>
		/// <value>The image.</value>
		public Image Image {
			get {
				return image.Image;
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

		/// <summary>
		/// Gets or sets the type of the scan.
		/// </summary>
		/// <value>The type of the scan.</value>
		/// <remarks>
		/// Reload shown bitmap automatically.
		/// </remarks>
		public ScanType ScanType {
			get {
				return currentShownType;
			}
			set {
				//Thread imageLoaderThread = new Thread (() => ShowType (value));
				//imageLoaderThread.Start ();
				ShowType (value);
			}
		}


		private bool EditMode {
			get {
				return isEditMode;
			}
			set {
				if (value) {
					contextEditMask.Label = "End edit mask";
					this.Cursor = CursorType.Crosshair;

					isEditMode = true;
				} else {
					contextEditMask.Label = "Edit mask";
					this.Cursor = CursorType.Arrow;

					isEditMode = false;
				}
			}
		}

		#endregion
	}
}

