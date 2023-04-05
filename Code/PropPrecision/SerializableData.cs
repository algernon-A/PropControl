﻿// <copyright file="SerializableData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), BloodyPenguin (Egor Aralov). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropPrecision
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
        /// Legacy 81 tiles data ID.
        /// </summary>
        internal const string DataID = "PropPrecision";

        // Data version (last legacy prop precision version was 1).
        private const int DataVersion = 1;

        /// <summary>
        /// Deserializes data from a savegame.
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            base.OnLoadData();

            // Don't load data if not in-game.
            if (ToolManager.instance.m_properties.m_mode != ItemClass.Availability.Game)
            {
                return;
            }

            // Don't read data if the DataID isn't present.
            if (!serializableDataManager.EnumerateData().Contains(DataID))
            {
                return;
            }

            byte[] data = serializableDataManager.LoadData(DataID);
            using (MemoryStream stream = new MemoryStream(data))
            {
                // Deserialise data.
                DataSerializer.Deserialize<Data>(stream, DataSerializer.Mode.Memory, LegacyTypeConverter);
            }
        }

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            // Don't save data if not in-game.
            if (ToolManager.instance.m_properties.m_mode != ItemClass.Availability.Game)
            {
                return;
            }

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
        /// <returns>ElectricityDataContainer type.</returns>
        private static Type LegacyTypeConverter(string legacyTypeName) => typeof(Data);
    }
}