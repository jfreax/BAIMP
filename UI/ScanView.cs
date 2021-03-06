//
//  ScanView.cs
//
//  Author:
//       Jens Dieskau <jens@dieskau.pm>
//
//  Copyright (c) 2014 Jens Dieskau
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Xwt;
using System.Collections.Generic;
using Xwt.Drawing;


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

		Preview.MyCallBack imageLoadedCallback = null;
		public Dictionary<string, object> data = new Dictionary<string, object>();
		/// <summary>Current image</summary>
		Image image;
		Image mask;
		BitmapImage maskBitmap;
		BaseScan scan;
		string currentShownType;
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
		Point mousePositionStart;

		bool showOnlyPreviewImage;

		/// <summary>
		/// Initializes a new instance of the <see cref="Baimp.ScanView"/> class.
		/// </summary>
		public ScanView()
		{
			// build context menu
			InitializeContextMenu();
		}

		/// <summary>
		/// Initialize the a new view for the specified scan
		/// </summary>
		/// <param name="scan">Scan.</param>
		/// <param name="scantype">Scantype.</param>
		/// <param name="showColorized">If set to <c>true</c> show the colorized version.</param>
		/// <param name="showOnlyPreviewImage">If set to <c>true</c> show only preview image, 
		/// do not load the full res version.</param>
		/// <remarks>
		/// Set all other properties _after_ you called this function.
		/// </remarks>
		public void Initialize(
			BaseScan scan, string scantype, bool showColorized = false, bool showOnlyPreviewImage = false)
		{
			this.scan = scan;
			this.showOnlyPreviewImage = showOnlyPreviewImage;
			this.WidthRequest = scan.Size.Width;
			this.HeightRequest = scan.Size.Height;

			this.CanGetFocus = true;

			// event subscribe
			scan.ScanDataChanged += delegate(object sender, ScanDataEventArgs e) {
				if (e.Changed.Equals("mask") && e.Unsaved.Contains("mask")) {
					MaskImage = scan.Mask.GetMaskAsImage();
					QueueDraw();
				}
			};

			IsThumbnail = false;

			Image thumbnail = scan.GetThumbnail(scantype);
			if (thumbnail != null) {
				this.Image = thumbnail;
			}

			// do not use the property method,
			// Scantype already loads the initial image async
			this.showColorized = showColorized;

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
					ctx.DrawImage(MaskBitmap, (new Rectangle(Point.Zero, image.Size)).Inflate(-3, -3), 0.6);
				}
			}

			if (isEditMode) {
				Point scaleFactor = new Point(
					                    scan.Size.Width / image.Size.Width, 
					                    scan.Size.Height / image.Size.Height);
				ctx.SetColor(Mask.maskColor);

				foreach (MaskEntry p in scan.Mask.MaskPositions) {
					switch (p.type) {
					case MaskEntryType.Point:
						ctx.SetLineWidth(p.pointerSize / scaleFactor.Y * 2);
						ctx.LineTo(p.position.X / scaleFactor.X, p.position.Y / scaleFactor.Y);
						ctx.Stroke();

						ctx.Arc(
							p.position.X / scaleFactor.X, p.position.Y / scaleFactor.Y,
							p.pointerSize / scaleFactor.Y, 0, 360);
						ctx.Fill();

						ctx.MoveTo(p.position.X / scaleFactor.X, p.position.Y / scaleFactor.Y);
						break;
					case MaskEntryType.Space:
						ctx.Stroke();
						ctx.ClosePath();
						break;
					case MaskEntryType.Delete:
						ctx.Arc(
							p.position.X / scaleFactor.X, p.position.Y / scaleFactor.Y,
							p.pointerSize / scaleFactor.Y, 0, 360);
						ctx.Save();
						ctx.Clip();
						int newX = (int) Math.Min(Math.Max(
							           p.position.X / scaleFactor.X - pointerSize / scaleFactor.Y, 0), scan.Size.Width);
						int newY = (int) Math.Min(Math.Max(
							           p.position.Y / scaleFactor.Y - pointerSize / scaleFactor.Y, 0), scan.Size.Height);

						using (ImageBuilder ib = 
							       new ImageBuilder((pointerSize / scaleFactor.Y * 2), (pointerSize / scaleFactor.Y * 2))) {
							BitmapImage bi = ib.ToBitmap();
							image.WithBoxSize(image.Size).ToBitmap().CopyArea(
								newX, newY, 
								(int) (pointerSize / scaleFactor.Y * 2), (int) (pointerSize / scaleFactor.Y * 2),
								bi, 0, 0);
							ctx.DrawImage(bi, new Point(newX, newY));
						}
						ctx.Restore();
						ctx.ClosePath();
						break;
					}
				}
					
				ctx.Stroke();

				if (mousePosition != Point.Zero) {
					ctx.Arc(mousePosition.X, mousePosition.Y, pointerSize / Math.Max(scaleFactor.X, scaleFactor.Y), 0, 360);
					ctx.Fill();

					if (mousePositionStart != Point.Zero) {
						ctx.SetLineWidth((pointerSize / Math.Max(scaleFactor.X, scaleFactor.Y)) * 2);
						ctx.SetColor(Mask.maskColor);
						ctx.Arc(mousePositionStart.X, mousePositionStart.Y, pointerSize / Math.Max(scaleFactor.X, scaleFactor.Y), 0, 360);
						ctx.Fill();
						ctx.MoveTo(mousePosition);
						ctx.LineTo(mousePositionStart);
						ctx.Stroke();
					}
				}
			}
		}

		#region contextmenu

		Menu contextMenu;
		MenuItem contextEditMask;

		/// <summary>
		/// Initializes the context menu.
		/// </summary>
		void InitializeContextMenu()
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
		void ShowType(string scanType)
		{
			currentShownType = scanType;
			EditMode = false;
			finishedImageLoading = false;

			if (showOnlyPreviewImage) {
				finishedImageLoading = true;
				if (imageLoadedCallback != null) {
					imageLoadedCallback(scanType);
				}
				if (imageLoaded != null) {
					imageLoaded(this, new EventArgs());
				}
			} else {
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
			SetFocus();

			base.OnButtonPressed(args);

			Point scaleFactor = new Point(
				                    scan.Size.Width / image.Size.Width, 
				                    scan.Size.Height / image.Size.Height);

			switch (args.Button) {
			case PointerButton.Left:
				pointer |= Pointer.Left;

				if (isEditMode && !Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Shift)) {
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
				if (isEditMode && Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Shift)) {
					mousePositionStart = new Point(args.X, args.Y);
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
					scan.Mask.FlushMaskPositions(ib.Context);

					scan.Mask.MaskPositions.Add(new MaskEntry(Point.Zero, MaskEntryType.Space, pointerSize));

					if (mousePositionStart != Point.Zero) {
						Point scaleFactor = new Point(
							                    scan.Size.Width / image.Size.Width, 
							                    scan.Size.Height / image.Size.Height);

						Point start = 
							new Point(mousePositionStart.X * scaleFactor.X, mousePositionStart.Y * scaleFactor.Y);
						ib.Context.MoveTo(start);
						SetMask(start);
						SetMask(new Point(mousePosition.X * scaleFactor.X, mousePosition.Y * scaleFactor.Y));
						scan.Mask.MaskPositions.Add(new MaskEntry(Point.Zero, MaskEntryType.Space, pointerSize));
					}

					mousePositionStart = Point.Zero;
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
			if (isEditMode && !Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Shift)) {
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

			if (Keyboard.CurrentModifiers.HasFlag(ModifierKeys.Shift)) {
				QueueDraw();
			}
		}

		protected override void OnMouseExited(EventArgs args)
		{
			pointer = Pointer.None;
			mousePosition = Point.Zero;
			mousePositionStart = Point.Zero;

			if (isEditMode) {
				ImageBuilder ib = scan.Mask.GetMaskBuilder();
				ib.Context.ClosePath();
			}

			Heighlighted = false;

			QueueDraw();
		}

		protected override void OnMouseEntered(EventArgs args)
		{
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

		protected override void OnKeyPressed(KeyEventArgs args)
		{
			base.OnKeyPressed(args);

			if (args.Modifiers.HasFlag(ModifierKeys.Command) || args.Modifiers.HasFlag(ModifierKeys.Control)) {
				switch (args.Key) {
				case Key.z:
					scan.Mask.Undo();
					QueueDraw();
					break;
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
						MaskImage = mask.WithBoxSize(scan.RequestedBitmapSize);
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

			scan.Mask.MaskPositions.Add(
				new MaskEntry(position, unset ? MaskEntryType.Delete : MaskEntryType.Point, pointerSize)
			);
		}

		/// <summary>
		/// Save mask into metadata file
		/// </summary>
		public void SaveMask()
		{
			scan.Mask.Save();
			MaskImage = scan.Mask.GetMaskAsImage();
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
					MaskImage = mask.WithBoxSize(s);
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
						MaskImage = mask.WithBoxSize(scan.RequestedBitmapSize);
					}
				}

				if (IsThumbnail) {
					image = image.WithBoxSize(Size);
					if (mask != null) {
						MaskImage = mask.WithBoxSize(Size);
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
		public Image MaskImage {
			get {
				return mask;
			}

			set {
				mask = value;
				if (image != null) {
					mask = mask.WithSize(image.Size);
				}

				MaskBitmap = null;
			}
		}

		BitmapImage MaskBitmap {
			get {
				if (maskBitmap == null && mask != null) {
					if (mask.Width > scan.Size.Width || mask.Height > scan.Size.Height) {
						maskBitmap = mask.WithSize(scan.Size).ToBitmap();
					} else {
						maskBitmap = mask.ToBitmap();
					}
				}
				return maskBitmap;
			}
			set {
				if (maskBitmap != null) {
					maskBitmap.Dispose();
				}
				maskBitmap = value;
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
				scan.Mask.CurrentScanType = value;
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

					if (scan != null) {
						scan.Mask.FlushMaskPositions(0);
					}

					isEditMode = false;
				}

				QueueDraw();
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
				} else {
					scan.Mask.GetMaskAsImageAsync(new Mask.ImageLoadedCallback(delegate(Image loadedMask) {
						MaskImage = loadedMask;
						QueueDraw();
					}));
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

