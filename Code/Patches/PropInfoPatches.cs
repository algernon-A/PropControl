// <copyright file="PropInfoPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using AlgernonCommons.Patching;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to implement adaptive prop visibility distance.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class PropInfoPatches
    {
        /// <summary>
        /// Minimum permitted fallback distance.
        /// </summary>
        internal const float MinFallbackDistance = 1000f;

        /// <summary>
        /// Maximum permitted fallback distance.
        /// </summary>
        internal const float MaxFallbackDistance = 100000f;

        /// <summary>
        /// Default fallback render distance.
        /// </summary>
        internal const float DefaultFallbackDistance = 1000f;

        /// <summary>
        /// Minimum permitted minimum distance.
        /// </summary>
        internal const float MinMinimumDistance = 1f;

        /// <summary>
        /// Maximum permitted minimum distance.
        /// </summary>
        internal const float MaxMinimumDistance = 1000f;

        /// <summary>
        /// Default minimum distance.
        /// </summary>
        internal const float DefaultMinimumDistance = 100f;

        /// <summary>
        /// Minimum permitted distance multiplier.
        /// </summary>
        internal const float MinDistanceMultiplier = 1f;

        /// <summary>
        /// Maximum permitted distance multiplier.
        /// </summary>
        internal const float MaxDistanceMultiplier = 1000f;

        /// <summary>
        /// Default distance multiplier.
        /// </summary>
        internal const float DefaultDistanceMultiplier = 200f;

        /// <summary>
        /// Minimum permitted LOD transition distance multiplier.
        /// </summary>
        internal const float MinLODTransitionMultiplier = 0.05f;

        /// <summary>
        /// Maximum permitted LOD transition distance multiplier.
        /// </summary>
        internal const float MaxLODTransitionMultiplier = 1.0f;

        /// <summary>
        /// Default LOD transition distance multiplier.
        /// </summary>
        internal const float DefaultLODTransitionMultiplier = 0.25f;

        // LOD rendering factors.
        private static float s_fallbackRenderDistance = DefaultFallbackDistance;
        private static float s_minimumDistance = DefaultMinimumDistance;
        private static float s_distanceMultiplier = DefaultDistanceMultiplier;
        private static float s_LODTransitionMultipler = DefaultLODTransitionMultiplier;

        /// <summary>
        /// Gets or sets the fallback prop render distance.
        /// This is used when no other value can be calculated.
        /// </summary>
        internal static float FallbackRenderDistance
        {
            get => s_fallbackRenderDistance;

            set
            {
                s_fallbackRenderDistance = Mathf.Clamp(value, MinFallbackDistance, MaxFallbackDistance);
                RefreshLODs();
            }
        }

        /// <summary>
        /// Gets or sets the minimum visibility distance.
        /// Props will always be visible within this distance.
        /// </summary>
        internal static float MinimumDistance
        {
            get => s_minimumDistance;

            set
            {
                s_minimumDistance = Mathf.Clamp(value, MinMinimumDistance, MaxMinimumDistance);
                RefreshLODs();
            }
        }

        /// <summary>
        /// Gets or sets the distance multiplier.
        /// This determines how far away the prop is visible.
        /// </summary>
        internal static float DistanceMultiplier
        {
            get => s_distanceMultiplier;

            set
            {
                s_distanceMultiplier = Mathf.Clamp(value, MinDistanceMultiplier, MaxDistanceMultiplier);
                RefreshLODs();
            }
        }

        /// <summary>
        /// Gets or sets the LOD transition multiplier.
        /// This determines how far away the model will transition from full mesh to LOD.
        /// </summary>
        internal static float LODTransitionMultiplier
        {
            get => s_LODTransitionMultipler;

            set
            {
                s_LODTransitionMultipler = Mathf.Clamp(value, MinLODTransitionMultiplier, MaxLODTransitionMultiplier);
                RefreshLODs();
            }
        }

        /// <summary>
        /// Gets or sets the maximum render distance for props.
        /// </summary>
        private static float RenderDistanceMaximum { get; set; } = 100000f; // Game default 1000f

        /// <summary>
        /// Gets or sets the render distance threshold for effects.
        /// </summary>
        private static float EffectRenderDistanceMaximum { get; set; } = 100000f; // Game default 1000f

        /// <summary>
        /// Harmony pre-emptive prefix to PropInfo.RefreshLevelOfDetail to implement adaptive prop visibility distance.
        /// </summary>
        /// <param name="__instance">PropInfo instance.</param>
        /// <returns>Always false (never execute original method).</returns>
        internal static bool RefreshLevelOfDetailPrefix(PropInfo __instance)
        {
            // Calculate maximum render distance.
            if (__instance.m_generatedInfo.m_triangleArea == 0.0f || float.IsNaN(__instance.m_generatedInfo.m_triangleArea))
            {
                // Invalid info for calculation - use fallback distance.
                __instance.m_maxRenderDistance = s_fallbackRenderDistance;
            }
            else
            {
                // Calculate dynamic visibility distance.
                double lodFactor = RenderManager.LevelOfDetailFactor * s_distanceMultiplier;
                __instance.m_maxRenderDistance = Mathf.Min(RenderDistanceMaximum, (float)((Mathf.Sqrt(__instance.m_generatedInfo.m_triangleArea) * lodFactor) + s_minimumDistance));
            }

            // Calculate LOD render distance.
            if (__instance.m_isDecal | __instance.m_isMarker)
            {
                // Decals and markers have 0 LOD render distance.
                __instance.m_lodRenderDistance = 0f;
            }
            else
            {
                // Does this prop have a LOD mesh?
                if (__instance.m_lodMesh == null)
                {
                    // No LOD mesh - LOD render distance is the same as maximum render distance (so never render a LOD).
                    __instance.m_lodRenderDistance = __instance.m_maxRenderDistance;
                }
                else
                {
                    // LOD mesh presence - LOD render distance the maximum render distance multiplied by the LOD distance transition multipler.
                    __instance.m_lodRenderDistance = __instance.m_maxRenderDistance * s_LODTransitionMultipler;
                }
            }

            // Update effect distances.
            if (__instance.m_effects != null)
            {
                for (int i = 0; i < __instance.m_effects.Length; ++i)
                {
                    if (__instance.m_effects[i].m_effect != null)
                    {
                        __instance.m_maxRenderDistance = Mathf.Max(__instance.m_maxRenderDistance, __instance.m_effects[i].m_effect.RenderDistance());
                    }
                }

                __instance.m_maxRenderDistance = Mathf.Min(EffectRenderDistanceMaximum, __instance.m_maxRenderDistance);
            }

            // Pre-empt original method.
            return false;
        }

        /// <summary>
        /// Refreshes all prop LODs.
        /// </summary>
        /// <param name="forceRefresh">True to force a refresh of LODs even when adaptive visibility isn't active.</param>
        internal static void RefreshLODs(bool forceRefresh = false)
        {
            // Don't do anything if we haven't loaded yet.
            if (PatcherLoadingBase<OptionsPanel, PatcherBase>.IsLoaded)
            {
                // Only refresh if adaptive visibility is enabled, or if we're forcing a refresh anyway.
                if (forceRefresh || Patcher.EnableAdaptiveVisibility)
                {
                    // Iterate through all loaded props and refresh their LODs with current settings.
                    for (ushort i = 0; i < PrefabCollection<PropInfo>.LoadedCount(); ++i)
                    {
                        PrefabCollection<PropInfo>.GetLoaded(i)?.RefreshLevelOfDetail();
                    }
                }
            }
        }
    }
}