using System;

namespace bachelorarbeit_implementierung
{
	public partial class Window : Gtk.Window
	{
		public Window () : 
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}
	}
}

