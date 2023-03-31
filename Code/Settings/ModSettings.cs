// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using static Patches.PropInfoPatches;

    /// <summary>
    /// Global mod settings.
    /// </summary>
    [XmlRoot("PropControl")]
    public class ModSettings : SettingsXMLBase
    {
        /// <summary>
        /// Gets the settings file name.
        /// </summary>
        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "PropControl.xml");

        /// <summary>
        /// Gets or sets the prop anarchy hotkey.
        /// </summary>
        [XmlElement("AnarchyKey")]
        public Keybinding AnarchyKey { get => UIThreading.AnarchyKey; set => UIThreading.AnarchyKey = value; }

        /// <summary>
        /// Gets or sets the prop upscaling key.
        /// </summary>
        [XmlElement("ScaleUpKey")]
        public Keybinding ScaleUpKey { get => UIThreading.ScaleUpKey; set => UIThreading.ScaleUpKey = value; }

        /// <summary>
        /// Gets or sets the prop upscaling key.
        /// </summary>
        [XmlElement("ScaleDownKey")]
        public Keybinding ScaleDownKey { get => UIThreading.ScaleDownKey; set => UIThreading.ScaleDownKey = value; }

        /// <summary>
        /// Gets or sets the prop upscaling key.
        /// </summary>
        [XmlElement("ElevationUpKey")]
        public Keybinding ElevationUpKey { get => UIThreading.ElevationUpKey; set => UIThreading.ElevationUpKey = value; }

        /// <summary>
        /// Gets or sets the prop upscaling key.
        /// </summary>
        [XmlElement("ElevationDownKey")]
        public Keybinding ElevationDownKey { get => UIThreading.ElevationDownKey; set => UIThreading.ElevationDownKey = value; }

        /// <summary>
        /// Gets or sets the key repeat delay.
        /// </summary>
        [XmlElement("KeyRepeatDelay")]
        public float KeyRepeatDelay { get => UIThreading.KeyRepeatDelay; set => UIThreading.KeyRepeatDelay = value; }

        /// <summary>
        /// Gets or sets the fallback prop render distance.
        /// </summary>
        [XmlElement("PropFallbackRenderDistance")]
        public float PropFallbackRenderDistance { get => FallbackRenderDistance; set => FallbackRenderDistance = value; }

        /// <summary>
        /// Gets or sets the minimum visibility distance.
        /// </summary>
        [XmlElement("PropMinimumDistance")]
        public float PropMinimumDistance { get => MinimumDistance; set => MinimumDistance = value; }

        /// <summary>
        /// Gets or sets the distance multiplier.
        /// </summary>
        [XmlElement("PropDistanceMultiplier")]
        public float PropDistanceMultiplier { get => DistanceMultiplier; set => DistanceMultiplier = value; }

        /// <summary>
        /// Gets or sets the LOD transition multiplier.
        /// </summary>
        [XmlElement("PropLODTransitionMultiplier")]
        public float PropLODTransitionMultiplier { get => LODTransitionMultiplier; set => LODTransitionMultiplier = value; }

        /// <summary>
        /// Loads settings from file.
        /// </summary>
        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);
    }
}