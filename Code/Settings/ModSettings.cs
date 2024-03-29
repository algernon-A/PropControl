﻿// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.UI;
    using AlgernonCommons.XML;
    using PropControl.Patches;
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
        /// Gets or sets a value indicating whether the status panel should be shown.
        /// </summary>
        [XmlElement("ShowButtons")]
        public bool ShowButtons { get => StatusPanel.ShowButtons; set => StatusPanel.ShowButtons = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the status panel should use transparent buttons.
        /// </summary>
        [XmlElement("TransparentButtons")]
        public bool UseTransparentButtons { get => StatusPanel.TransparentUI; set => StatusPanel.TransparentUI = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the UI anarchy toggle should be enabled (<c>true</c>) or disabled (<c>false</c>) after loading.
        /// </summary>
        [XmlElement("EnableAnarchyAfterLoad")]
        public bool InitialAnarchyState { get => Loading.InitialAnarchyState; set => Loading.InitialAnarchyState = value; }

        /// <summary>
        /// Gets or sets the panel's saved X-position.
        /// </summary>
        [XmlElement("StatusPanelX")]
        public float StatusPanelX { get => StandalonePanelManager<StatusPanel>.LastSavedXPosition; set => StandalonePanelManager<StatusPanel>.LastSavedXPosition = value; }

        /// <summary>
        /// Gets or sets the panel's saved Y-position.
        /// </summary>
        [XmlElement("StatusPanelY")]
        public float StatusPanelY { get => StandalonePanelManager<StatusPanel>.LastSavedYPosition; set => StandalonePanelManager<StatusPanel>.LastSavedYPosition = value; }

        /// <summary>
        /// Gets or sets a value indicating whether prop Y-positions should be updated on terrain changes.
        /// </summary>
        [XmlElement("UpdateOnTerrain")]
        public bool UpdateOnTerrain { get => PropInstancePatches.UpdateOnTerrain; set => PropInstancePatches.UpdateOnTerrain = value; }

        /// <summary>
        /// Gets or sets a value indicating whether props should be raised to ground level if the terrain is raised above them.
        /// </summary>
        [XmlElement("KeepAboveGround")]
        public bool KeepAboveGround { get => PropInstancePatches.KeepAboveGround; set => PropInstancePatches.KeepAboveGround = value; }

        /// <summary>
        /// Gets or sets the prop anarchy hotkey.
        /// </summary>
        [XmlElement("AnarchyKey")]
        public Keybinding AnarchyKey { get => UIThreading.AnarchyKey; set => UIThreading.AnarchyKey = value; }

        /// <summary>
        /// Gets or sets the tree anarchy hotkey.
        /// </summary>
        [XmlElement("SnappingKey")]
        public Keybinding SnappingKey { get => UIThreading.SnappingKey; set => UIThreading.SnappingKey = value; }

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
        /// Gets or sets the raise elevation key.
        /// </summary>
        [XmlElement("ElevationUpKey")]
        public Keybinding ElevationUpKey { get => UIThreading.ElevationUpKey; set => UIThreading.ElevationUpKey = value; }

        /// <summary>
        /// Gets or sets the lower elevation key.
        /// </summary>
        [XmlElement("ElevationDownKey")]
        public Keybinding ElevationDownKey { get => UIThreading.ElevationDownKey; set => UIThreading.ElevationDownKey = value; }

        /// <summary>
        /// Gets or sets the key repeat delay.
        /// </summary>
        [XmlElement("KeyRepeatDelay")]
        public float KeyRepeatDelay { get => UIThreading.KeyRepeatDelay; set => UIThreading.KeyRepeatDelay = value; }

        /// <summary>
        /// Gets or sets a value indicating whether adaptive visibility is enabled.
        /// </summary>
        [XmlElement("EnableAdaptiveVisibility")]
        public bool EnableAdaptiveVisibility { get => Patcher.EnableAdaptiveVisibility; set => Patcher.EnableAdaptiveVisibility = value; }

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