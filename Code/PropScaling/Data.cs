// <copyright file="Data.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropScaling
{
    using System;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using static PropControl.Patches.PropInstancePatches;

    /// <summary>
    /// Savegame data container for prop scaling data.
    /// </summary>
    public sealed class Data : IDataContainer
    {
        private const int DataVersion = 0;

        /// <summary>
        /// Saves prop snapping data (prop heights) to savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Serialize(DataSerializer serializer)
        {
            try
            {
                // Write data version and array size.
                serializer.WriteInt32(DataVersion);
                serializer.WriteInt32(ScalingArray.Length);

                // Write each prop scale entry.
                for (int i = 0; i < ScalingArray.Length; ++i)
                {
                    serializer.WriteFloat(ScalingArray[i]);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing prop scaling data");
            }
        }

        /// <summary>
        /// Reads prop scaling data to savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Deserialize(DataSerializer serializer)
        {
            try
            {
                // Write data version and array size (currently ignored).
                int version = serializer.ReadInt32();
                if (version > DataVersion)
                {
                    Logging.Error("invalid scaling data version ", version, "; aborting read");
                    return;
                }

                // Read array length.
                int length = serializer.ReadInt32();
                if (length != ScalingArray.Length)
                {
                    Logging.Error("invalid scaling data length ", length, "; aborting read");
                    return;
                }

                // Read each prop scale entry.
                for (int i = 0; i < ScalingArray.Length; ++i)
                {
                    ScalingArray[i] = serializer.ReadFloat();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deserializing prop scaling data");
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