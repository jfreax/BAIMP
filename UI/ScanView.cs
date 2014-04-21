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

		/// <summary>Current image</summary>
		private Image image;
		private Image mask;
		private BaseScan scan;
		private string currentShownType;
		// mouse actions
		Pointer pointer;
		int pointerSize = 16;
		/// <summary>
		/// Is edit mode (to draw mask) active?
		/// </summary>
		bool isEditMode = false;

		bool finishedImageLoading;

		/// <summary>
		/// Position of mouse pointer, Point.Zero if mouse exited view
		/// </summary>
		Point mousePosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.ScanView"/> class.
		/// </summary>
		public ScanView()
		{
			// build context menu
			InitializeContextMenu();
		}

		public void Initialize(BaseScan scan, string scantype, bool showColoried = false)
		{
			this.scan = scan;
			this.WidthRequest = scan.Size.Width;
			this.HeightRequest = scan.Size.Height;

			this.CanGetFocus = true;

			// event subscribe
			scan.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
				if (e.Changed.Equals("mask")) {
					Mask = scan.Mask.GetMaskAsImage();
				}
			};

			IsThumbnail = false;

			Image thumbnail = scan.GetThumbnail(scantype);
			if (thumbnail != null) {
				this.Image = thumbnail;
			}

			// do not use the property method,
			// Scantype already loads the initial image async
			this.showColorized = showColoried;

			ScanType = scantype;
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			base.OnDraw(ctx, dirtyRect);

			if (image != null) {
				if (Heighlighted && IsThumbnail) {
					ctx.RoundRectangle(new Rectangle(Point.Zero, image.Size), 3);
					ctx.SetColor(Colors.LightSteelBlue);
					ctx.Fill();
				}

				ctx.DrawImage(image, (new Rectangle(Point.Zero, image.Size)).Inflate(-3, -3));

				if (mask != null && ShowMask) {
					ctx.DrawImage(mask, (new Rectangle(Point.Zero, image.Size)).Inflate(-3, -3));
				}
			}

			if (isEditMode && mousePosition != Point.Zero) {
				Point scaleFactor = new Point(
					                    scan.Size.Width / image.Size.Width, 
					                    scan.Size.Height / image.Size.Height);

				ctx.Arc(mousePosition.X, mousePosition.Y, pointerSize / scaleFactor.X, 0, 360);
				ctx.SetColor(maskColor);
				ctx.Fill();
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
			contextResetMask.Clicked += (object sender, EventArgs e) => scan.Mask.ResetMask();
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
			finishedImageLoading = false;

			scan.Mask.GetMaskAsImageAsync(new Mask.ImageLoadedCallback(delegate(Image loadedMask) {
				Mask = loadedMask;
				QueueDraw();
			}));
				
			scan.GetAsImageAsync(scanType, ShowColorized, new BaseScan.ImageLoadedCallback(delegate(Image loadedImage) {
				Image = loadedImage;

				if (imageLoadedCallback != null) {
					imageLoadedCallback(scanType);
				}
				if (imageLoaded != null) {
					imageLoaded(this, new EventArgs());
				}

				finishedImageLoading = true;
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
			base.OnButtonPressed(args);

			Point scaleFactor = new Point(
				                    scan.Size.Width / image.Size.Width, 
				                    scan.Size.Height / image.Size.Height);

			switch (args.Button) {
			case PointerButton.Left:
				pointer |= Pointer.Left;

				if (isEditMode) {
					scan.NotifyChange("mask");

					Point positionInImage = new Point(args.X * scaleFactor.X, args.Y * scaleFactor.Y);
					ImageBuilder ib = scan.Mask.GetMaskBuilder();
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
					ImageBuilder ib = scan.Mask.GetMaskBuilder();
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
			mousePosition = new Point(args.X, args.Y);
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

				QueueDraw();
			}
		}

		protected override void OnMouseExited(EventArgs args)
		{
			pointer = Pointer.None;
			mousePosition = Point.Zero;

			if (isEditMode) {
				ImageBuilder ib = scan.Mask.GetMaskBuilder();
				ib.Context.ClosePath();
			}

			Heighlighted = false;

			QueueDraw();
		}

		protected override void OnMouseEntered(EventArgs args)
		{
			SetFocus();

			Heighlighted = true;
		}

		protected override void OnMouseScrolled(MouseScrolledEventArgs args)
		{
			base.OnMouseScrolled(args);

			if (isEditMode) {
				mousePosition = new Point(args.X, args.Y);
				QueueDraw();

				if (Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Control) ||
				    Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Command)) {

					if (args.Direction == ScrollDirection.Up) {
						pointerSize++;
					} else {
						pointerSize--;
					}
					args.Handled = true;
				}
			}
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

		EventHandler<EventArgs> showMaskToggled;

		/// <summary>
		/// Occurs when the flag show mask was toggled
		/// </summary>
		public event EventHandler<EventArgs> ShowMaskToggled {
			add {
				showMaskToggled += value;
			}
			remove {
				showMaskToggled -= value;
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

				if (finishedImageLoading) {
					image = image.WithBoxSize(scan.RequestedBitmapSize);

					if (mask != null) {
						mask = mask.WithBoxSize(scan.RequestedBitmapSize);
					}
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
			ImageBuilder ib = scan.Mask.GetMaskBuilder();
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

			Mask = scan.Mask.GetMaskAsImage();
			QueueDraw();
		}

		/// <summary>
		/// Save mask into metadata file
		/// </summary>
		public void SaveMask()
		{
			scan.Mask.Save();
			mask = scan.Mask.GetMaskAsImage();
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

		public BaseScan Scan {
			get {
				return scan;
			}
		}

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

				if (!scan.IsScaled() && Parent != null && Parent.Parent != null) {
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
					ShowMask = true;
				} else {
					contextEditMask.Label = "Edit mask";
					this.Cursor = CursorType.Arrow;

					isEditMode = false;
				}
			}
		}

		bool Heighlighted {
			get;
			set;
		}

		public bool IsThumbnail {
			get;
			set;
		}

		bool showMask = false;

		/// <summary>
		/// Gets or sets a value indicating whether mask should be drawn or not.
		/// </summary>
		/// <value><c>true</c> if show mask; otherwise, <c>false</c>.</value>
		public bool ShowMask {
			get {
				return showMask;
			}
			set {
				showMask = value;

				if (!showMask) {
					EditMode = false;
				}

				if (showMaskToggled != null) {
					showMaskToggled(this, new EventArgs());
				}

				QueueDraw();
			}
		}

		bool showColorized = false;

		/// <summary>
		/// Gets or sets a value indicating whether the image should be colorized or not.
		/// </summary>
		/// <value><c>true</c> if colorized; otherwise, <c>false</c>.</value>
		public bool ShowColorized {
			get {
				return showColorized;
			}
			set {
				showColorized = value;

				ShowType(currentShownType);
			}
		}

		#endregion
	}
}

