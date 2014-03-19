using System;
using Xwt;
using Xwt.Drawing;

namespace baimp
{
	public class PipelineView : Canvas
	{
		TreeNode<BaseAlgorithm> tree;


		/// <summary>
		/// Initializes a new instance of the <see cref="baimp.PipelineView"/> class.
		/// </summary>
		public PipelineView ()
		{
			this.MinHeight = 120;
			this.SetDragDropTarget (TransferDataType.Text);
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
			e.Success = true;
			try {
				Type elementType = Type.GetType(e.Data.GetValue (TransferDataType.Text).ToString());
				BaseAlgorithm algoInstance = Activator.CreateInstance(elementType) as BaseAlgorithm;
				if(tree == null) {
					tree = new TreeNode<BaseAlgorithm>(algoInstance);
				} else {
					tree.AddChild(algoInstance);
				}

				this.QueueDraw();

			} catch( Exception exception) {
				Console.WriteLine (exception.Message);
				e.Success = false;
			}
		}
	}
}

