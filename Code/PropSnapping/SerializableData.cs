﻿// <copyright file="SerializableData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), BloodyPenguin (Egor Aralov). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropSnapping
{
    using System;
    using System.IO;
    using System.Linq;
    using ColossalFramework.IO;
    using ICities;

    /// <summary>
    /// Serialization for prop snapping data.
    /// </summary>
    public class SerializableData : SerializableDataExtensionBase
    {
        /// <summary>
        /// Legacy Prop Snapping data ID.
        /// </summary>
        internal const string DataID = "PropSnapping";

        // Data version (last legacy prop snapping version was 1).
        private const int DataVersion = 1;

        /// <summary>
        /// Gets a value indicating whether snapping data exists in this save.
        /// </summary>
        internal static bool HasSnappingData { get; private set; } = false;

        /// <summary>
        /// Deserializes data from a savegame.
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            base.OnLoadData();

            // Don't read data if the DataID isn't present.
            if (!serializableDataManager.EnumerateData().Contains(DataID))
            {
                return;
            }

            // Snapping data exists.
            HasSnappingData = true;
            byte[] data = serializableDataManager.LoadData(DataID);
            using (MemoryStream stream = new MemoryStream(data))
            {
                // Deserialise data.
                try
                {
                    DataSerializer.Deserialize<Data>(stream, DataSerializer.Mode.Memory, LegacyTypeConverter);
                }
                catch (Exception e)
                {
                    AlgernonCommons.Logging.LogException(e, "exception deserializing snapping data");
                }
            }
        }

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise data.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new Data());

                // Write to savegame.
                serializableDataManager.SaveData(DataID, stream.ToArray());
            }
        }

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>Data type.</returns>
        private static Type LegacyTypeConverter(string legacyTypeName) => typeof(Data);
    }
}