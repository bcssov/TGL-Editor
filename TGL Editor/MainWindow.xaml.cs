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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TGL_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Implements the <see cref="System.Windows.Window" />
    /// Implements the <see cref="System.Windows.Markup.IComponentConnector" />
    /// Implements the <see cref="System.ComponentModel.INotifyPropertyChanged" />
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window, INotifyPropertyChanged

    {
        #region Fields

        /// <summary>
        /// The TGL
        /// </summary>
        private readonly TGL tgl = new TGL();

        /// <summary>
        /// The full TGL data
        /// </summary>
        private IEnumerable<TGLData> fullTGLData = null;

        /// <summary>
        /// The TGL data
        /// </summary>
        private ObservableCollection<TGLData> tglData = new ObservableCollection<TGLData>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            RegisterGlobalBinding(new KeyGesture(Key.O, ModifierKeys.Control), LoadFile);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the TGL data.
        /// </summary>
        /// <value>The TGL data.</value>
        public ObservableCollection<TGLData> TGLData
        {
            get
            {
                return tglData;
            }
            set
            {
                tglData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TGLData)));
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Loads the file.
        /// </summary>
        private void LoadFile()
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "TGL|*.tgl",
                Multiselect = false,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Title = "Select TGL to load"
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var fileName = dialog.FileName;
                using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs.Seek(0, SeekOrigin.Begin);
                using var br = new BinaryReader(fs);
                fullTGLData = tgl.Parse(br.ReadBytes((int)fs.Length));
                TGLData = new ObservableCollection<TGLData>(fullTGLData);
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
        /// Texts the box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            IEnumerable<TGLData> filtered;
            if (string.IsNullOrWhiteSpace(text))
            {
                filtered = fullTGLData;
            }
            else
            {
                filtered = fullTGLData.Where(p => p.Id.Contains(text, StringComparison.OrdinalIgnoreCase) || p.Data.Contains(text, StringComparison.OrdinalIgnoreCase) || p.SFX.Contains(text, StringComparison.OrdinalIgnoreCase));
            }
            TGLData = new ObservableCollection<TGLData>(filtered);
        }

        #endregion Methods
    }
}