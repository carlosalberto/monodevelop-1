// DebugValueWindow.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Linq;
using Mono.Debugging.Client;
using MonoDevelop.Debugger;
using MonoDevelop.Components;
using Gtk;
using Mono.TextEditor;
using MonoDevelop.Core.Gui;
using Gdk;

namespace MonoDevelop.SourceEditor
{
	public class BaseWindow : Gtk.Window
	{
		public BaseWindow () : base(Gtk.WindowType.Popup)
		{
			this.SkipPagerHint = true;
			this.SkipTaskbarHint = true;
			this.Decorated = false;
			this.BorderWidth = 2;
			this.TypeHint = WindowTypeHint.Tooltip;
			this.AllowShrink = false;
			this.AllowGrow = false;
		}
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			int winWidth, winHeight;
			this.GetSize (out winWidth, out winHeight);
			evnt.Window.DrawRectangle (Style.BaseGC (StateType.Normal), true, 0, 0, winWidth - 1, winHeight - 1);
			evnt.Window.DrawRectangle (Style.MidGC (StateType.Normal), false, 0, 0, winWidth - 1, winHeight - 1);
			foreach (var child in this.Children)
				this.PropagateExpose (child, evnt);
			return false;
		}
	}
	
	public class DebugValueWindow : BaseWindow
	{
		ObjectValueTreeView tree;
		ScrolledWindow sw;
		bool resetSelection = true;
		Mono.TextEditor.TextEditor editor;
		int offset;
		ObjectValue value;
		
		public DebugValueWindow (Mono.TextEditor.TextEditor editor, int offset, StackFrame frame, ObjectValue value, bool unpin)
		{
			this.editor = editor;
			this.offset = offset;
			this.value  = value;
			TransientFor = (Gtk.Window) editor.Toplevel;
			AcceptFocus = true;
			
			sw = new ScrolledWindow ();
			sw.HscrollbarPolicy = PolicyType.Never;
			sw.VscrollbarPolicy = PolicyType.Never;
			
			tree = new ObjectValueTreeView ();
			sw.Add (tree);
			HBox box = new HBox ();
			box.Add (sw); 
			
			Gtk.EventBox imageEventBox = new Gtk.EventBox ();
			imageEventBox.VisibleWindow = false;
			Gtk.Image image = new Gtk.Image ();
			image.Pixbuf = ImageService.GetPixbuf (unpin ? "md-pin-down" : "md-pin-up", IconSize.Menu);
			if (!unpin) {
				imageEventBox.EnterNotifyEvent += delegate {
					image.Pixbuf = ImageService.GetPixbuf ("md-pin-active", IconSize.Menu);
				};
				imageEventBox.LeaveNotifyEvent += delegate {
					image.Pixbuf = ImageService.GetPixbuf ("md-pin-up", IconSize.Menu);
				};
			}
			imageEventBox.ButtonPressEvent += delegate(object o, ButtonPressEventArgs args) {
				OnPinButtonClicked (EventArgs.Empty);
				if (!unpin)
					HandlePinButtonClicked (this, EventArgs.Empty);
			};
			imageEventBox.Add (image);
			VBox vbox = new VBox ();
			vbox.PackStart (imageEventBox, false, false, 0);
			
			box.PackEnd (vbox, false, false, 0);
			Add (box);
			
			tree.Frame = frame;
			tree.CompactView = true;
			tree.AllowAdding = false;
			tree.AllowEditing = true;
			tree.HeadersVisible = false;
			
			tree.AddValue (value);
			tree.Selection.UnselectAll ();
			tree.SizeAllocated += OnTreeSizeChanged;
			
			sw.ShowAll ();
			
			tree.StartEditing += delegate {
				Modal = true;
			};
			
			tree.EndEditing += delegate {
				Modal = false;
			};

			tree.Selection.Changed += delegate {
				if (resetSelection) {
					resetSelection = false;
					tree.Selection.UnselectAll ();
				}
			};
		}
		
		public event EventHandler PinButtonClicked;
		protected virtual void OnPinButtonClicked (EventArgs e)
		{
			EventHandler handler = this.PinButtonClicked;
			if (handler != null)
				handler (this, e);
		}
		
		void HandlePinButtonClicked (object sender, EventArgs e)
		{
			
			LineSegment lineSegment = editor.Document.GetLineByOffset (offset);
			DebugValueMarker marker = (DebugValueMarker)lineSegment.Markers.FirstOrDefault (m => m is DebugValueMarker);
			
			if (marker == null) {
				marker = new DebugValueMarker (editor, lineSegment);
				editor.Document.AddMarker (lineSegment, marker);
			}
			if (!marker.HasValue (value)) {
				marker.AddValue (value);
				editor.Document.CommitLineUpdate (lineSegment);
			}
			Destroy ();
		}
		
		void OnTreeSizeChanged (object s, SizeAllocatedArgs a)
		{
			int x,y,w,h;
			GetPosition (out x, out y);
			h = (int) sw.Vadjustment.Upper;
			w = (int) sw.Hadjustment.Upper;
			int dy = y + h - this.Screen.Height;
			int dx = x + w - this.Screen.Width;
			
			if (dy > 0 && sw.VscrollbarPolicy == PolicyType.Never) {
				sw.VscrollbarPolicy = PolicyType.Always;
				sw.HeightRequest = h - dy - 10;
			} else if (sw.VscrollbarPolicy == PolicyType.Always && sw.Vadjustment.Upper == sw.Vadjustment.PageSize) {
				sw.VscrollbarPolicy = PolicyType.Never;
				sw.HeightRequest = -1;
			}
			
			if (dx > 0 && sw.HscrollbarPolicy == PolicyType.Never) {
				sw.HscrollbarPolicy = PolicyType.Always;
				sw.WidthRequest = w - dx - 10;
			} else if (sw.HscrollbarPolicy == PolicyType.Always && sw.Hadjustment.Upper == sw.Hadjustment.PageSize) {
				sw.HscrollbarPolicy = PolicyType.Never;
				sw.WidthRequest = -1;
			}
		}
	}
}
