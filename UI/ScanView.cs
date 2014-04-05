using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;
using System.Threading;
using System.IO;

namespace Baimp
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

	public class ScanView : Canvas
	{
		#region static member

		public static Color maskColor = Colors.DarkBlue;

		#endregion

		Preview.MyCallBack imageLoadedCallback = null;
		public Dictionary<string, object> data = new Dictionary<string, object>();

		private Image image;
		private Image mask;
		private BaseScan scan;
		private string currentShownType;

		// mouse actions
		Pointer pointer;
		const int pointerSize = 16;
		// state
		bool isEditMode = false;

		bool loadingComplete = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.ScanView"/> class.
		/// </summary>
		public ScanView()
		{
		}

		public void Initialize(BaseScan scan, string fibertype)
		{
			this.scan = scan;
			this.WidthRequest = scan.Size.Width;
			this.HeightRequest = scan.Size.Height;

			this.CanGetFocus = true;

			// build context menu
			InitializeContextMenu();

			// event subscribe
			scan.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
				if (e.Changed.Equals("mask_" + currentShownType)) {
					Mask = scan.Masks.GetMaskAsImage(currentShownType);
				}
			};

			IsThumbnail = false;

			Image thumbnail = scan.GetThumbnail(fibertype);
			if (thumbnail != null) {
				this.Image = thumbnail;
			}
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			if (image != null) {
				ctx.DrawImage(image, Point.Zero);
			}

			if (mask != null) {
				ctx.DrawImage(mask, Point.Zero);
			}
		}

		#region contextmenu

		private Menu contextMenu;
		private MenuItem contextEditMask;

		/// <summary>
		/// Initializes the context menu.
		/// </summary>
		private void InitializeContextMenu()
		{
			contextMenu = new Menu();

			contextEditMask = new MenuItem("Edit mask");
			contextEditMask.UseMnemonic = true;
			contextEditMask.Clicked += (object sender, EventArgs e) => EditMode ^= true;
			contextMenu.Items.Add(contextEditMask);

			MenuItem contextResetMask = new MenuItem("Reset mask");
			contextResetMask.UseMnemonic = true;
			contextResetMask.Clicked += (object sender, EventArgs e) => scan.Masks.ResetMask(currentShownType);
			contextMenu.Items.Add(contextResetMask);

			MenuItem contextSaveMask = new MenuItem("Save changes");
			contextSaveMask.UseMnemonic = true;
			contextSaveMask.Clicked += (object sender, EventArgs e) => SaveMask();
			contextMenu.Items.Add(contextSaveMask);

		}

		#endregion

		/// <summary>
		/// Display image of selected scan type
		/// </summary>
		/// <param name="scanType">Scan type.</param>
		private void ShowType(string scanType)
		{
			currentShownType = scanType;
			EditMode = false;

			scan.GetAsImageAsync(scanType, new BaseScan.ImageLoadedCallback(delegate(Image loadedImage) {
				Image = loadedImage;
				Mask = scan.Masks.GetMaskAsImage(currentShownType);

				QueueDraw();

				if (imageLoadedCallback != null) {
					imageLoadedCallback(scanType);
				}
				if (imageLoaded != null) {
					imageLoaded(this, new EventArgs());
				}

				loadingComplete = true;
			}));
		}

		#region callback

		/// <summary>
		/// Registers the image loaded callback.
		/// </summary>
		/// <param name="cb">Callback function.</param>
		public void RegisterImageLoadedCallback(Preview.MyCallBack cb)
		{
			this.imageLoadedCallback = cb;
		}

		#endregion

		#region events

		protected override void OnButtonPressed(ButtonEventArgs args)
		{
			Point scaleFactor = new Point(
				scan.Size.Width / image.Size.Width, 
				scan.Size.Height / image.Size.Height);

			switch (args.Button) {
			case PointerButton.Left:
				pointer |= Pointer.Left;

				if (isEditMode) {
					scan.NotifyChange("mask_" + currentShownType);

					Point positionInImage = new Point(args.X * scaleFactor.X, args.Y * scaleFactor.Y);
					ImageBuilder ib = scan.Masks.GetMaskBuilder(currentShownType);
					ib.Context.NewPath();
					ib.Context.MoveTo(positionInImage);

					SetMask(
						positionInImage,
						Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Control) ||
						Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Command)
					);
				}
				break;
			case PointerButton.Right:
				pointer |= Pointer.Right;
				contextMenu.Popup();
				break;
			}
		}

		protected override void OnButtonReleased(ButtonEventArgs args)
		{
			switch (args.Button) {
			case PointerButton.Left:
				pointer ^= Pointer.Left;

				if (isEditMode) {
					ImageBuilder ib = scan.Masks.GetMaskBuilder(currentShownType);
					ib.Context.ClosePath();
				}
				break;
			case PointerButton.Right:
				pointer ^= Pointer.Right;
				break;
			}
		}

		protected override void OnMouseMoved(MouseMovedEventArgs args)
		{
			if (isEditMode) {
				Point scaleFactor = new Point(
					scan.Size.Width / image.Size.Width, 
					scan.Size.Height / image.Size.Height);

				if (pointer.HasFlag(Pointer.Left)) {
					SetMask(
						new Point(args.X * scaleFactor.X, args.Y * scaleFactor.Y),
						Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Control) ||
						Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Command)
					);
				}
			}
		}

		protected override void OnMouseExited(EventArgs args)
		{
			pointer = Pointer.None;

			if (isEditMode) {
				ImageBuilder ib = scan.Masks.GetMaskBuilder(currentShownType);
				ib.Context.ClosePath();
			}
		}

		protected override void OnMouseEntered(EventArgs args)
		{
			this.SetFocus();
		}

		protected override void OnBoundsChanged()
		{
			base.OnBoundsChanged();

			if (Bounds.Width > 10 && IsThumbnail) {
				WithBoxSize(Bounds.Size);
			}
		}

		#endregion

		#region events to emit

		EventHandler<EventArgs> imageLoaded;

		/// <summary>
		/// Occurs when scan data changed
		/// </summary>
		public event EventHandler<EventArgs> ImageLoaded {
			add {
				imageLoaded += value;
			}
			remove {
				imageLoaded -= value;
			}
		}

		#endregion

		/// <summary>
		/// Scale scan and mask at once.
		/// </summary>
		/// <param name="scale">Scale factor.</param>
		public void Scale(double scale)
		{
			if (image != null) {
				scan.ScaleImage(scale);

				if (loadingComplete) {
					image = scan.GetAsImage(currentShownType);
					mask = scan.Masks.GetMaskAsImage(currentShownType);
				}

				if (Parent != null && Parent.Parent != null) {
					if (image.Width > Parent.Parent.WidthRequest) {
						Parent.WidthRequest = image.Width;
					}
					if (image.Height > Parent.Parent.HeightRequest) {
						Parent.HeightRequest = image.Height;
					}
				}
				QueueDraw();
			}
		}

		/// <summary>
		/// Set mask at given position.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="unset">If set to <c>true</c> delete mask on position.</param>
		private void SetMask(Point position, bool unset = false)
		{
			ImageBuilder ib = scan.Masks.GetMaskBuilder(currentShownType);
			if (position.X > ib.Width || position.Y > ib.Height) {
				return;
			}


			if (unset) {
				ib.Context.Save();
				ib.Context.Arc(position.X, position.Y, pointerSize, 0, 360);
				ib.Context.Clip();

				double newX = Math.Min(Math.Max(position.X - pointerSize, 0), scan.Size.Width);
				double newY = Math.Min(Math.Max(position.Y - pointerSize, 0), scan.Size.Height);

				Image i = image.WithBoxSize(scan.Size).ToBitmap().Crop(
					          new Rectangle(newX, newY, pointerSize * 2, pointerSize * 2)
				          );

				ib.Context.DrawImage(i, new Point(newX, newY));
				ib.Context.Fill();
				ib.Context.Restore();

			} else {
				ib.Context.SetLineWidth(pointerSize * 2);

				ib.Context.SetColor(maskColor);
				ib.Context.LineTo(position);
				ib.Context.Stroke();

				ib.Context.Arc(position.X, position.Y, pointerSize, 0, 360);
				ib.Context.Fill();

				ib.Context.MoveTo(position);
			}

			Mask = scan.Masks.GetMaskAsImage(currentShownType);
			QueueDraw();
		}

		/// <summary>
		/// Save mask into metadata file
		/// </summary>
		public void SaveMask()
		{
			scan.Masks.Save(currentShownType);
			mask = scan.Masks.GetMaskAsImage(currentShownType);
		}

		/// <summary>
		/// Change the shown image to a size that fits in the provided size limits
		/// </summary>
		/// <param name="s">Max width and height</param>
		public void WithBoxSize(Size s)
		{
			if (image != null) {
				image = image.WithBoxSize(s);

				if (!IsThumbnail) {
					scan.RequestedBitmapSize = image.Size;
				}

				if (mask != null) {
					mask = mask.WithBoxSize(s);
				}

				QueueDraw();
			}
		}

		/// <summary>
		/// Determines whether the image of this scan is scaled.
		/// </summary>
		/// <returns><c>true</c> if this instance is scaled; otherwise, <c>false</c>.</returns>
		public bool IsScaled()
		{
			return scan.IsScaled();
		}

		#region properties

		/// <summary>
		/// Gets or sets the scan image to show.
		/// </summary>
		/// <value>The image.</value>
		public Image Image {
			get {
				return image;
			}
			set {
				image = value;
				if(!scan.IsScaled() && Parent != null && Parent.Parent != null) {
					WithBoxSize(Parent.Parent.Size);
				} else {
					image = image.WithBoxSize(scan.RequestedBitmapSize);
					if (mask != null) {
						mask = mask.WithBoxSize(scan.RequestedBitmapSize);
					}
				}

				if (IsThumbnail) {
					image = image.WithBoxSize(Size);
					if (mask != null) {
						mask = mask.WithBoxSize(Size);
					}
				}

				Scale(1.0);
				QueueDraw();
			}
		}

		/// <summary>
		/// Gets or sets the mask.
		/// </summary>
		/// <value>The mask.</value>
		public Image Mask {
			get {
				return mask;
			}

			set {
				mask = value;
				if (image != null) {
					mask = mask.WithSize(image.Size);
				}
			}
		}

		/// <summary>
		/// Gets or sets the type of the scan.
		/// </summary>
		/// <value>The type of the scan.</value>
		/// <remarks>
		/// Reload shown bitmap automatically.
		/// </remarks>
		public string ScanType {
			get {
				return currentShownType;
			}
			set {
				ShowType(value);
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
			
		public bool IsThumbnail {
			get;
			set;
		}
		#endregion
	}
}

