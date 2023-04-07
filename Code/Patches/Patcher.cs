// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using CitiesHarmony.API;
    using HarmonyLib;
    using PropControl.Patches;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public class Patcher : PatcherBase
    {
        // Flags.
        private static bool s_enableAdaptiveVisibility = false;
        private static bool s_adaptiveVisibilityPatched = false;

        /// <summary>
        /// Gets or sets a value indicating whether adaptive visibility is enabled.
        /// </summary>
        internal static bool EnableAdaptiveVisibility
        {
            get => s_enableAdaptiveVisibility;

            set
            {
                // Don't do anything if no change.
                if (value != s_enableAdaptiveVisibility)
                {
                    // Update value and toggle patch application.
                    s_enableAdaptiveVisibility = value;

                    // Toggle patch if the patcher is ready.
                    if (PatcherManager<Patcher>.IsReady)
                    {
                        PatcherManager<Patcher>.Instance.PatchAdaptiveVisibility(value);
                    }
                }
            }
        }

        /// <summary>
        /// Applies or unapplies adaptive prop visibility patches.
        /// </summary>
        /// <param name="active">True to apply patch, false to unapply.</param>
        internal void PatchAdaptiveVisibility(bool active)
        {
            // Don't do anything if we're already at the current state.
            if (s_adaptiveVisibilityPatched != active)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    // Target method: PropInfo.RefreshLevelOfDetail.
                    MethodInfo targetMethod = AccessTools.Method(typeof(PropInfo), nameof(PropInfo.RefreshLevelOfDetail));
                    if (targetMethod == null)
                    {
                        Logging.Error("unable to find PropInfo.RefreshLevelOfDetail target method");
                        return;
                    }

                    // Patch method.
                    MethodInfo patchMethod = AccessTools.Method(typeof(PropInfoPatches), nameof(PropInfoPatches.RefreshLevelOfDetailPrefix));
                    if (patchMethod == null)
                    {
                        Logging.Error("unable to find adaptive prop visibility patch method");
                        return;
                    }

                    Harmony harmonyInstance = new Harmony(HarmonyID);

                    // Apply or remove patches according to flag.
                    if (active)
                    {
                        PrefixMethod(targetMethod, patchMethod);
                    }
                    else
                    {
                        UnpatchMethod(targetMethod, patchMethod);
                    }

                    // Update status flag.
                    s_adaptiveVisibilityPatched = active;

                    // Refresh LODs (forcing refresh).
                    PropInfoPatches.RefreshLODs(true);
                }
                else
                {
                    Logging.Error("Harmony not ready");
                }
            }
        }

        /// <summary>
        /// Peforms any additional actions (such as custom patching) after PatchAll is called.
        /// </summary>
        /// <param name="harmonyInstance">Haromny instance for patching.</param>
        protected override void OnPatchAll(Harmony harmonyInstance) => PatchAdaptiveVisibility(s_enableAdaptiveVisibility);
    }
}