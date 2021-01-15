// ***********************************************************************
// Assembly         : TGL Editor
// Author           : Mario
// Created          : 01-15-2021
//
// Last Modified By : Mario
// Last Modified On : 01-15-2021
// ***********************************************************************
// <copyright file="TGLData.cs" company="TGL Editor">
//     Copyright (c) Mario. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System;
using System.ComponentModel;

namespace TGL_Editor
{
    /// <summary>
    /// Class TGLData.
    /// Implements the <see cref="System.ComponentModel.INotifyPropertyChanged" />
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class TGLData : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The data
        /// </summary>
        private string data;

        /// <summary>
        /// The identifier
        /// </summary>
        private string id;

        /// <summary>
        /// The SFX
        /// </summary>
        private string sfx;

        #endregion Fields

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));
            }
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }

        /// <summary>
        /// Gets or sets the SFX.
        /// </summary>
        /// <value>The SFX.</value>
        public string SFX
        {
            get
            {
                return sfx;
            }
            set
            {
                sfx = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SFX)));
            }
        }

        #endregion Properties
    }
}