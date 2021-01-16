// ***********************************************************************
// Assembly         : TGL Editor
// Author           : Mario
// Created          : 01-15-2021
//
// Last Modified By : Mario
// Last Modified On : 01-16-2021
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="TGL Editor">
//     Copyright (c) Mario. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TGL_Editor
{
    /// <summary>
    /// Class MainWindow.
    /// Implements the <see cref="System.Windows.Window" />
    /// Implements the <see cref="System.Windows.Markup.IComponentConnector" />
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window

    {
        #region Fields

        /// <summary>
        /// The TGL
        /// </summary>
        private readonly TGL tgl = new TGL();

        /// <summary>
        /// The file name
        /// </summary>
        private string fileName = string.Empty;

        /// <summary>
        /// The TGL data
        /// </summary>
        private ObservableCollection<TGLData> tglData = new ObservableCollection<TGLData>();

        /// <summary>
        /// The TGL view source
        /// </summary>
        private CollectionViewSource tglViewSource;

        /// <summary>
        /// The visible items
        /// </summary>
        private int visibleItems = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));
            DataContext = this;
            InitializeGrid();
            RegisterGlobalBinding(new KeyGesture(Key.O, ModifierKeys.Control), LoadFile);
            RegisterGlobalBinding(new KeyGesture(Key.S, ModifierKeys.Control), SaveFile);
            RegisterGlobalBinding(new KeyGesture(Key.N, ModifierKeys.Control), NewFile);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Handles the AddingNewItem event of the dataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AddingNewItemEventArgs" /> instance containing the event data.</param>
        private void dataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            filter.Text = string.Empty;
        }

        /// <summary>
        /// Handles the LoadingRow event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridRowEventArgs" /> instance containing the event data.</param>
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.GetIndex() < visibleItems)
            {
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            }
            else
            {
                e.Row.Header = string.Empty;
            }
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            tglViewSource = new CollectionViewSource() { Source = tglData };
            tglViewSource.View.Filter = null;
            visibleItems = tglData.Count;
            dataGrid.ItemsSource = tglViewSource.View;
            filter.Text = string.Empty;
        }

        /// <summary>
        /// Loads the file.
        /// </summary>
        private void LoadFile()
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "TGL|*.tgl",
                Multiselect = false,
                Title = "Select TGL to load"
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                fileName = dialog.FileName;
                using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs.Seek(0, SeekOrigin.Begin);
                using var br = new BinaryReader(fs);
                var data = tgl.Parse(br.ReadBytes((int)fs.Length));
                tglData = new ObservableCollection<TGLData>(data ?? new List<TGLData>());
                InitializeGrid();
            }
        }

        /// <summary>
        /// Creates new file.
        /// </summary>
        private void NewFile()
        {
            var init = true;
            if (tglData.Count > 0)
            {
                var dialog = new Dialog(this, "Confirm", "Grid could have unsaved changes, do you want to proceed?");
                init = dialog.ShowDialog().GetValueOrDefault();
            }
            if (init)
            {
                tglData = new ObservableCollection<TGLData>(new List<TGLData>());
                InitializeGrid();
            }
        }

        /// <summary>
        /// Registers the global bind.
        /// </summary>
        /// <param name="gesture">The gesture.</param>
        /// <param name="action">The action.</param>
        private void RegisterGlobalBinding(KeyGesture gesture, Action action)
        {
            var cmd = new RoutedCommand();
            var inputBinding = new InputBinding(cmd, gesture);
            InputBindings.Add(inputBinding);
            var commandBinding = new CommandBinding(cmd);
            commandBinding.Executed += (sender, args) => action.Invoke();
            CommandBindings.Add(commandBinding);
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        private void SaveFile()
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "TGL|*.tgl",
                Title = "Save TGL file",
                FileName = fileName
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                fileName = dialog.FileName;
                using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
                tgl.Save(fs, tglData);
            }
        }

        /// <summary>
        /// Texts the box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs" /> instance containing the event data.</param>
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                tglViewSource.View.Filter = null;
                visibleItems = tglData.Count;
            }
            else
            {
                string filter = string.Empty;
                string term = text;
                var split = text.Split(":");
                if (split.Count() == 2)
                {
                    filter = split[0];
                    term = split[1];
                }
                tglViewSource.View.Filter = new Predicate<object>(p =>
                {
                    var model = (TGLData)p;
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        return model.Id.Contains(term, StringComparison.OrdinalIgnoreCase) || model.Data.Contains(term, StringComparison.OrdinalIgnoreCase) || model.SFX.Contains(term, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        switch (filter.ToLowerInvariant())
                        {
                            case "id":
                                return model.Id.Contains(term, StringComparison.OrdinalIgnoreCase);

                            case "data":
                                return model.Data.Contains(term, StringComparison.OrdinalIgnoreCase);

                            case "sfx":
                                return model.SFX.Contains(term, StringComparison.OrdinalIgnoreCase);

                            default:
                                return model.Id.Contains(term, StringComparison.OrdinalIgnoreCase) || model.Data.Contains(text, StringComparison.OrdinalIgnoreCase) || model.SFX.Contains(text, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                });
                visibleItems = tglViewSource.View.Cast<object>().Count() - 1;
            }
        }

        #endregion Methods
    }
}