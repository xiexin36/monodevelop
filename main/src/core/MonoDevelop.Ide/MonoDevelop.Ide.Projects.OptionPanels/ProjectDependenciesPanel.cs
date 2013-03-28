//
// ProjectDependenciesPanel.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using MonoDevelop.Ide.Gui.Dialogs;
using Xwt;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using System.Linq;
using MonoDevelop.Components;
using System.Collections.Generic;

namespace MonoDevelop.Ide.Projects.OptionPanels
{
	public class ProjectDependenciesPanel: IOptionsPanel
	{
		VBox box;
		ListStore store;
		SolutionEntityItem project;

		public ProjectDependenciesPanel ()
		{
		}

		DataField<bool> isDependencyField = new DataField<bool> ();
		DataField<string> projectNameField = new DataField<string> ();
		DataField<SolutionEntityItem> projectField = new DataField<SolutionEntityItem> ();
		DataField<bool> canEditField = new DataField<bool> ();

		public void Initialize (OptionsDialog dialog, object dataObject)
		{
			box = new VBox ();
			box.PackStart (new Label (GettextCatalog.GetString ("This project depends on:")));

			store = new ListStore (isDependencyField, projectNameField, projectField, canEditField);
			var list = new Xwt.ListView (store);
			list.HeadersVisible = false;

			var checkCell = new CheckBoxCellView (isDependencyField);
			checkCell.ActiveChanged += delegate {
				var row = list.SelectedRow;
				if (store.GetValue (row, canEditField))
					store.SetValue (row, isDependencyField, !store.GetValue (row, isDependencyField));
			};

			var col = new ListViewColumn ();
			col.Views.Add (checkCell);
			col.Views.Add (new TextCellView (projectNameField));

			list.Columns.Add (col);

			project = (SolutionEntityItem)dataObject;
			var implicitDependencies = project.GetReferencedItems (ConfigurationSelector.Default).Except (project.ItemDependencies).ToArray ();

			foreach (var dep in project.ParentSolution.GetAllSolutionItems<SolutionEntityItem> ().OrderBy (p => p.Name)) {
				var r = store.AddRow ();
				var isImplicit = implicitDependencies.Contains (dep);
				string name = dep.Name + (isImplicit ? " " + GettextCatalog.GetString ("(project reference)") : "");
				store.SetValue (r, isDependencyField, isImplicit || project.ItemDependencies.Contains (dep));
				store.SetValue (r, projectNameField, name);
				store.SetValue (r, projectField, dep);
				store.SetValue (r, canEditField, !isImplicit);
			}
			box.PackStart (list, BoxMode.FillAndExpand);
		}

		public Gtk.Widget CreatePanelWidget ()
		{
			return box.ToGtkWidget ();
		}

		public bool IsVisible ()
		{
			return true;
		}

		public bool ValidateChanges ()
		{
			return true;
		}

		public void ApplyChanges ()
		{
			for (int n=0; n<store.RowCount; n++) {
				var proj = store.GetValue (n, projectField);
				if (store.GetValue (n, isDependencyField)) {
					if (!proj.ItemDependencies.Contains (proj))
						proj.ItemDependencies.Add (proj);
				} else
					proj.ItemDependencies.Remove (proj);
			}
		}
	}
}

