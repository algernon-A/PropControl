﻿// <copyright file="PanelPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches for prefab selection panels to reset elevation and scaling when a new prop is selected.
    /// </summary>
    [HarmonyPatch]
    internal static class PanelPatches
    {
        /// <summary>
        /// Determines list of target methods to patch.
        /// </summary>
        /// <returns>List of target methods to patch.</returns>
        internal static IEnumerable<MethodBase> TargetMethods()
        {
            // Vanilla game panels.
            yield return AccessTools.Method(typeof(BeautificationPanel), "OnButtonClicked");
            yield return AccessTools.Method(typeof(LandscapingPanel), "OnButtonClicked");

            // Natural resources brush (detours BeautificationGroupPanel).
            Type nrbType = Type.GetType("NaturalResourcesBrush.Detours.BeautificationPanelDetour,NaturalResourcesBrush");
            if (nrbType != null)
            {
                Logging.Message("Extra Landscaping Tools found; patching");
                yield return AccessTools.Method(nrbType, "OnButtonClicked");
            }
        }

        /// <summary>
        /// Harmony postfix patch to reset scaling and elevation adjustment when a new prop is selected.
        /// </summary>
        internal static void Postfix()
        {
            PropToolPatches.Scaling = PropToolPatches.DefaultScale;
            PropToolPatches.ElevationAdjustment = PropToolPatches.DefaultElevationAdjustment;
        }
    }
}
