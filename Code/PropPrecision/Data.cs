// <copyright file="Data.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropPrecision
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using static PropControl.Patches.PropInstancePatches;

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
                // Local refrerence.
                PropInstance[] props = Singleton<PropManager>.instance.m_props.m_buffer;

                // Count number of prop entries to be serialized.
                int numEntries = 0;
                foreach (ushort prop in PrecisionDict.Keys)
                {
                    // Only including props with created flags set.
                    if ((props[prop].m_flags & (ushort)PropInstance.Flags.Created) == (ushort)PropInstance.Flags.Created)
                    {
                        ++numEntries;
                    }
                }

                serializer.WriteInt32(numEntries);

                // Write each prop entry - prop ID, X, Z.
                foreach (ushort prop in PrecisionDict.Keys)
                {
                    if ((props[prop].m_flags & (ushort)PropInstance.Flags.Created) == (ushort)PropInstance.Flags.Created)
                    {
                        serializer.WriteUInt16(prop);
                        PrecisionCoordinates coordinates = PrecisionDict[prop];
                        serializer.WriteUInt16(coordinates.X);
                        serializer.WriteUInt16(coordinates.Z);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing prop precision data");
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
                // Local refrerence.
                PropInstance[] props = Singleton<PropManager>.instance.m_props.m_buffer;

                // Clear dictionary.
                PrecisionDict.Clear();

                // Read each prop entry.
                int numEntries = serializer.ReadInt32();
                for (int i = 0; i < numEntries; ++i)
                {
                    // Format is propID, X, Z.
                    ushort prop = (ushort)serializer.ReadUInt16();

                    PrecisionCoordinates value = new PrecisionCoordinates
                    {
                        X = (ushort)serializer.ReadUInt16(),
                        Z = (ushort)serializer.ReadUInt16(),
                    };

                    // Check that prop is created before assigning flags.
                    if ((props[prop].m_flags & (ushort)PropInstance.Flags.Created) == (ushort)PropInstance.Flags.Created)
                    {
                        PrecisionDict[prop] = value;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing prop precision data");
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