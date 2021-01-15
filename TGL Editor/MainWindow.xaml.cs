// ***********************************************************************
// Assembly         : TGL Editor
// Author           : Mario
// Created          : 01-15-2021
//
// Last Modified By : Mario
// Last Modified On : 01-15-2021
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="TGL Editor">
//     Copyright (c) Mario. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace TGL_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Implements the <see cref="System.Windows.Window" />
    /// Implements the <see cref="System.Windows.Markup.IComponentConnector" />
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            RegisterGlobalBinding(new KeyGesture(Key.O, ModifierKeys.Control), LoadFile);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Loads the file.
        /// </summary>
        private void LoadFile()
        {
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

        #endregion Methods
    }
}