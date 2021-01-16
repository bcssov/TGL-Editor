// ***********************************************************************
// Assembly         : TGL Editor
// Author           : Mario
// Created          : 01-15-2021
//
// Last Modified By : Mario
// Last Modified On : 01-16-2021
// ***********************************************************************
// <copyright file="TGL.cs" company="TGL Editor">
//     Copyright (c) Mario. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// The TGL byte headers
        /// </summary>
        private static readonly byte[] tglHeaders = new byte[] { 1, 23, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 }; // 1701 :)

        // Don't know what exactly these represent and in all tgls I've tried they were empty... probably padding but I recall that there was something else to it?
        /// <summary>
        /// The TGL byte information
        /// </summary>
        private static readonly byte[] tglInfo = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

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
        /// <param name="stream">The stream.</param>
        /// <param name="data">The data.</param>
        public void Save(Stream stream, IReadOnlyCollection<TGLData> data)
        {
            var bytes = new List<byte>();
            void writeSequence(IReadOnlyCollection<string> col, int? number, bool separateWithNull = false)
            {
                foreach (var item in col)
                {
                    var text = item;
                    if (separateWithNull)
                    {
                        text = string.Join(NullChar, item.ToCharArray()) + NullChar + NullChar;
                    }
                    text += NullChar;
                    bytes.AddRange(Encoding.UTF8.GetBytes(text));
                }
                if (number.HasValue)
                {
                    bytes.AddRange(GetBytes(number.GetValueOrDefault()));
                }
            }

            // Write static header
            bytes.AddRange(tglHeaders);
            // Write total count
            bytes.AddRange(GetBytes(data.Count));
            // Write what seemingly seems empty bytes
            bytes.AddRange(tglInfo);
            // Now need to write length information summed up
            int idLen = 0, dataLen = 0, sfxLen = 0;
            var last = data.Last();
            foreach (var item in data)
            {
                idLen += item.Id.Length + 1;
                dataLen += item.Data.Length + 1;
                sfxLen += item.SFX.Length + 1;
                bytes.AddRange(GetBytes(idLen));
                if (item != last)
                {
                    // These are written as separators after id columns
                    bytes.AddRange(GetBytes(dataLen));
                    bytes.AddRange(GetBytes(sfxLen));
                }
            }
            writeSequence(data.Select(p => p.Id).ToList(), dataLen, false);
            writeSequence(data.Select(p => p.Data).ToList(), sfxLen, true);
            writeSequence(data.Select(p => p.SFX).ToList(), null, false);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(bytes.ToArray(), 0, bytes.Count);
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>System.Byte[].</returns>
        private byte[] GetBytes(int number)
        {
            var result = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(result, number);
            return result;
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>System.Int32.</returns>
        private int GetInt(byte[] bytes)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(bytes);
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