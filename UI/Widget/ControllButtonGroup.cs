using System;
using Xwt;
using Xwt.Drawing;

namespace Baimp
{
	public class ControllButtonGroup : HBox
	{
			
		public ControllButtonGroup()
		{
			this.Spacing = 0;
		}

		/// <summary>
		/// Adds a new button.
		/// </summary>
		/// <returns>The button.</returns>
		/// <param name="icon">Icon.</param>
		/// <param name="toggleButton">If set to <c>true</c>, then is this a toggle button.</param>
		public ButtonSegment AddButton(Image icon, bool toggleButton = false)
		{
			int childCounter = 0;
			foreach (var child in Children) {
				ButtonSegment s = child as ButtonSegment;
				if (s != null) {
					if (childCounter == 0) {
						s.SegmentType = SegmentType.Left;
					} else {
						s.SegmentType = SegmentType.Middle;
					}

					childCounter++;
				}
			}

			ButtonSegment segment = 
				new ButtonSegment(childCounter == 0 ? SegmentType.Left : SegmentType.Right, icon, toggleButton);
			PackStart(segment);

			return segment;
		}
	}
}

