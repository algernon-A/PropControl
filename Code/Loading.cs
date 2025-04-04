﻿// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using System.Collections.Generic;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using AlgernonCommons.UI;
    using ICities;
    using PropControl.Patches;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, Patcher>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the UI anarchy toggle should be enabled (<c>true</c>) or disabled (<c>false</c>) after loading.
        /// </summary>
        internal static bool InitialAnarchyState { get; set; } = true;

        /// <summary>
        /// Gets a list of permitted loading modes.
        /// </summary>
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor };

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Apply decal prop fix.
            new DecalPropFix();

            // Set initial anarchy state.
            PropToolPatches.AnarchyEnabled = InitialAnarchyState;

            // Add status panel.
            if (StatusPanel.ShowButtons)
            {
                StandalonePanelManager<StatusPanel>.Create();
            }


            // Patch Move It.
            PropToolPatches.CheckMoveIt();


            // If we're deserializing a save that doesn't have snapping data, we need to reset existing props to ground level.
            if (!PropSnapping.SerializableData.HasSnappingData)
            {
                Logging.KeyMessage("No snapping data found in savegame; resetting props to ground level");
                PropInstance[] props = PropManager.instance.m_props.m_buffer;
                for (int i = 0; i < props.Length; ++i)
                {
                    PropInstancePatches.CalculatePropPrefix(ref props[i]);
                }

                PropInfoPatches.RefreshLODs(true);
            }
        }
    }
}