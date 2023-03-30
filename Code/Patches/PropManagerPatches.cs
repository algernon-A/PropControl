// <copyright file="PropManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches to implement prop scaling.
    /// </summary>
    [HarmonyPatch(typeof(PropManager))]
    public static class PropManagerPatches
    {
        /// <summary>
        /// Harmony postfix to PropManager.CreateProp to implement prop scaling.
        /// </summary>>
        /// <param name="prop">ID of newly-created prop.</param>
        [HarmonyPatch(nameof(PropManager.CreateProp))]
        [HarmonyPostfix]
        public static void SetBlockedPostfix(ushort prop)
        {
            Logging.KeyMessage("Created prop ", prop);
            if (prop != 0)
            {
                PropInstancePatches.ScalingArray[prop] = PropToolPatches.Scaling;
            }
        }
    }
}