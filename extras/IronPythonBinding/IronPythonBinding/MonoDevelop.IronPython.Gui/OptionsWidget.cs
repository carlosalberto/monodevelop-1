//
// OptionsWidget.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2012 Carlos Alberto Cortez
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

using System;
using Gtk;

using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Dialogs;

namespace MonoDevelop.IronPython.Gui
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OptionsWidget : Gtk.Bin
	{
		ListStore versionsStore;
		
		public OptionsWidget ()
		{
			this.Build ();
			
			versionsStore = new ListStore (typeof (string), typeof (LangVersion));
			versionsStore.AppendValues ("2.7", LangVersion.Python27);
			versionsStore.AppendValues ("3.0", LangVersion.Python30);
			versionComboBox.Model = versionsStore;
			
			Visible = true;
		}
		
		public string DefaultModule {
			get { return moduleEntry.Text ?? String.Empty; }
			set { moduleEntry.Text = value ?? String.Empty; }
		}
		
		public bool Optimize {
			get { return optimizeCheck.Active; }
			set { optimizeCheck.Active = value; }
		}
		
		public bool ShowClrExceptions {
			get { return showClrExcCheck.Active; }
			set { showClrExcCheck.Active = value; }
		}
		
		public bool ShowExceptionDetail {
			get { return showExcDetailCheck.Active; }
			set { showExcDetailCheck.Active = value; }
		}
		
		public bool WarnInconsistentTabbing {
			get { return warnInconsistentTabCheck.Active; }
			set { warnInconsistentTabCheck.Active = value; }
		}
	}
	
	public class OptionsPanel : MultiConfigItemOptionsPanel
	{
		OptionsWidget widget;
		
		public override Gtk.Widget CreatePanelWidget ()
		{
			widget = new OptionsWidget ();
			return widget;
		}
				
		public override void LoadConfigData ()
		{
			var config = CurrentConfiguration as PythonConfiguration;
			
			widget.DefaultModule = config.MainModule;
			widget.Optimize = config.Optimize;
			widget.ShowClrExceptions = config.ShowClrExceptions;
			widget.ShowExceptionDetail = config.ShowExceptionDetails;
			widget.WarnInconsistentTabbing = config.WarnInconsistentTabbing;
		}
		
		public override void ApplyChanges ()
		{
			var config = CurrentConfiguration as PythonConfiguration;
			
			config.MainModule = widget.DefaultModule;
			config.Optimize = widget.Optimize;
			config.ShowClrExceptions = widget.ShowClrExceptions;
			config.ShowExceptionDetails = widget.ShowExceptionDetail;
			config.WarnInconsistentTabbing = widget.WarnInconsistentTabbing;
		}
	}
}

