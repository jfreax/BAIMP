using System;
using Xwt;
using Xwt.Drawing;

namespace baimp
{
	public class PipelineView : Canvas
	{
		public PipelineView ()
		{
			this.MinHeight = 120;

			SetDragDropTarget (TransferDataType.Text);
		}


		/// <summary>
		/// Called when the widget needs to be redrawn
		/// </summary>
		/// <param name='ctx'>
		/// Drawing context
		/// </param>
		protected override void OnDraw (Xwt.Drawing.Context ctx, Rectangle dirtyRect)
		{
			if (Bounds.IsEmpty)
				return;

			ctx.Rectangle (0, 0, Bounds.Width, Bounds.Height);
			ctx.SetColor (Color.FromBytes (232, 232, 232));
			ctx.SetLineWidth (1);
			ctx.Fill ();

		}


		protected override void OnDragOver(DragOverEventArgs e)
		{
			e.AllowedAction = DragDropAction.Link;
		}


		protected override void OnDragDrop(DragEventArgs e)
		{
			Console.WriteLine ("Dropped! " + e.Action);
			Console.WriteLine ("Text: " + e.Data.GetValue (TransferDataType.Text));

			e.Success = true;
		}
	}
}

