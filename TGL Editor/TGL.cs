// ***********************************************************************
// Assembly         : TGL Editor
// Author           : Mario
// Created          : 01-15-2021
//
// Last Modified By : Mario
// Last Modified On : 01-15-2021
// ***********************************************************************
// <copyright file="TGL.cs" company="TGL Editor">
//     Copyright (c) Mario. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TGL_Editor
{
    /// <summary>
    /// Class TGL.
    /// </summary>
    public class TGL
    {
        #region Fields

        /// <summary>
        /// The first column length index
        /// </summary>
        private const int ColumnIndex = 28;

        /// <summary>
        /// The count index
        /// </summary>
        private const int CountIndex = 12;

        /// <summary>
        /// The null character
        /// </summary>
        private const char NullChar = '\0';

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TGL" /> class.
        /// </summary>
        public TGL()
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Parses the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>IEnumerable&lt;TGLObject&gt;.</returns>
        public IEnumerable<TGLData> Parse(IEnumerable<byte> bytes)
        {
            if (bytes.Count() > ColumnIndex)
            {
                var result = new List<TGLData>();
                // Total entries
                var count = bytes.Skip(CountIndex).Take(4);
                var total = GetInt(count.ToArray());
                if (total > 0)
                {
                    var positionData = new List<PositionData>();
                    var idPos = ColumnIndex;
                    int dataPos = 0;
                    int sfxPos = 0;
                    for (int i = 1; i <= total; i++)
                    {
                        PositionData data;
                        if (i == total)
                        {
                            // Need to traverse to separators to get the rest of the data
                            var idLength = GetInt(bytes.Skip(idPos).Take(4).ToArray());
                            dataPos = idPos + idLength + 4;
                            var dataLength = GetInt(bytes.Skip(dataPos).Take(4).ToArray());
                            sfxPos = idPos + idLength + 8 + dataLength * 2;
                            var sfxLength = GetInt(bytes.Skip(sfxPos).Take(4).ToArray());
                            data = new PositionData()
                            {
                                Id = idLength,
                                Data = dataLength,
                                SFX = sfxLength
                            };
                            idPos += 4;
                            dataPos += 4;
                            sfxPos += 4;
                        }
                        else
                        {
                            data = new PositionData()
                            {
                                Id = GetInt(bytes.Skip(idPos).Take(4).ToArray()),
                                Data = GetInt(bytes.Skip(idPos + 4).Take(4).ToArray()),
                                SFX = GetInt(bytes.Skip(idPos + 8).Take(4).ToArray())
                            };
                            idPos += 12;
                        }
                        positionData.Add(data);
                    }
                    PositionData prevData = null;
                    foreach (var data in positionData)
                    {
                        var idLen = data.Id - (prevData?.Id).GetValueOrDefault();
                        var dataLen = data.Data - (prevData?.Data).GetValueOrDefault();
                        var sfxLen = data.SFX - (prevData?.SFX).GetValueOrDefault();
                        var tgl = new TGLData()
                        {
                            Id = ReadString(bytes, idPos + data.Id - idLen, idLen),
                            Data = ReadString(bytes, dataPos + (data.Data * 2) - (dataLen * 2), dataLen * 2),
                            SFX = ReadString(bytes, sfxPos + data.SFX - sfxLen, sfxLen)
                        };
                        prevData = data;
                        result.Add(tgl);
                    }
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>System.Int32.</returns>
        private int GetInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes.ToArray());
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="position">The position.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.ValueTuple&lt;System.String, System.Int32&gt;.</returns>
        private string ReadString(IEnumerable<byte> bytes, int position, int length)
        {
            var textArray = bytes.Skip(position).Take(length);
            var text = Encoding.UTF8.GetString(textArray.Where(p => p != NullChar).ToArray());
            return text;
        }

        #endregion Methods

        #region Classes

        /// <summary>
        /// Class PositionData.
        /// </summary>
        private class PositionData
        {
            #region Properties

            /// <summary>
            /// Gets or sets the data.
            /// </summary>
            /// <value>The data.</value>
            public int Data { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the SFX.
            /// </summary>
            /// <value>The SFX.</value>
            public int SFX { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}