// <copyright file="PropManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to implement prop scaling.
    /// </summary>
    [HarmonyPatch(typeof(PropManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class PropManagerPatches
    {
        /// <summary>
        /// Harmony postfix to PropManager.CreateProp to implement prop scaling.
        /// </summary>>
        /// <param name="__instance">PropManager instance.</param>
        /// <param name="prop">ID of newly-created prop.</param>
        [HarmonyPatch(nameof(PropManager.CreateProp))]
        [HarmonyPostfix]
        private static void CreatePropPostfix(PropManager __instance, ushort prop)
        {
            if (prop != 0)
            {
                ref PropInstance propInstance = ref __instance.m_props.m_buffer[prop];

                // Apply elevation adjustment.
                propInstance.Position += new Vector3(0f, PropToolPatches.ElevationAdjustment, 0f);

                // Record scaling.
                PropInstancePatches.ScalingArray[prop] = PropToolPatches.Scaling;
            }
        }
    }
}