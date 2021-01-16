// ***********************************************************************
// Assembly         : TGL Editor
// Author           : Mario
// Created          : 01-16-2021
//
// Last Modified By : Mario
// Last Modified On : 01-16-2021
// ***********************************************************************
// <copyright file="Dialog.xaml.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows;

namespace TGL_Editor
{
    /// <summary>
    /// Class Dialog.
    /// Implements the <see cref="System.Windows.Window" />
    /// Implements the <see cref="System.Windows.Markup.IComponentConnector" />
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class Dialog : Window
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public Dialog(Window owner, string title, string message)
        {
            InitializeComponent();
            Owner = owner;
            Title = title;
            this.message.Text = message;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Handles the Click event of the Cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Ok control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        #endregion Methods
    }
}