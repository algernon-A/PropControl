// <copyright file="Data.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), BloodyPenguin (Egor Aralov). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropSnapping
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;

    /// <summary>
    /// Savegame data container for prop height data.
    /// Data format is that from BloodyPenguin's original Prop Snapping mod.
    /// </summary>
    public sealed class Data : IDataContainer
    {
        /// <summary>
        /// Saves prop snapping data (prop heights) to savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Serialize(DataSerializer serializer)
        {
            try
            {
                // Local reference.
                PropInstance[] propBuffer = Singleton<PropManager>.instance.m_props.m_buffer;

                // Prop buffer length.
                int bufferSize = propBuffer.Length;
                serializer.WriteInt32(bufferSize);

                // Write prop heights.
                EncodedArray.UShort heights = EncodedArray.UShort.BeginWrite(serializer);
                for (int i = 0; i < bufferSize; ++i)
                {
                    heights.Write(propBuffer[i].m_posY);
                }

                heights.EndWrite();
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing prop snapping data");
            }
        }

        /// <summary>
        /// Reads prop snapping data (prop heights) to savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Deserialize(DataSerializer serializer)
        {
            try
            {
                // Local reference.
                PropInstance[] propBuffer = Singleton<PropManager>.instance.m_props.m_buffer;

                // Read prop heights.
                int bufferSize = serializer.ReadInt32();
                EncodedArray.UShort heights = EncodedArray.UShort.BeginRead(serializer);
                for (int i = 0; i < bufferSize; ++i)
                {
                    ushort height = heights.Read();

                    // Check for questionable data - ignore 0x0000 and 0xFFFF.
                    if (height != 0 & height != ushort.MaxValue)
                    {
                        propBuffer[i].m_posY = height;
                    }
                    else
                    {
                        // Clear the fixed height flag of any prop without valid snapping data.
                        propBuffer[i].FixedHeight = false;
                    }
                }

                heights.EndRead();
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deserializing prop snapping data");
            }
        }

        /// <summary>
        /// Performs post-deserialization actions.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}