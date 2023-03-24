// <copyright file="PropInstancePatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using System.Collections.Generic;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to implement prop anarchy and snapping.
    /// </summary>
    [HarmonyPatch(typeof(PropInstance))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class PropInstancePatches
    {
        // Prop precision data.
        private static readonly Dictionary<ushort, PrecisionCoordinates> PrecisionData = new Dictionary<ushort, PrecisionCoordinates>();

        /// <summary>
        /// Gets the prop precision data dictionary.
        /// </summary>
        internal static Dictionary<ushort, PrecisionCoordinates> PrecisionDict => PrecisionData;

        /// <summary>
        /// Harmony pre-emptive Prefix to PropInstance.Blocked setter to implement prop tool anarchy.
        /// </summary>
        /// <param name="__instance">PropInstance instance.</param>
        /// <param name="value">Original setter argument.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.Blocked), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool SetBlockedPrefix(ref PropInstance __instance, bool value)
        {
            // Never apply blocked flag; only unblock.
            if (!value)
            {
                __instance.m_flags = (ushort)(__instance.m_flags & (int)~PropInstance.Flags.Blocked);
            }

            return false;
        }

        /// <summary>
        /// Harmony pre-emptive Prefix to PropInstance.Position getter to implement prop precision.
        /// </summary>
        /// <param name="__instance">PropInstance instance.</param>
        /// <param name="__result">Original getter result.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.Position), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool GetPositionPrefix(ref PropInstance __instance, ref Vector3 __result)
        {
            // Unsafe, because we need to reverse-engineer the instance ID from the address offset.
            unsafe
            {
                fixed (void* pointer = &__instance)
                {
                    PropInstance* prop = (PropInstance*)pointer;

                    // Y is always default.
                    __result.y = prop->m_posY * (1f / 64f);

                    // Default editor behaviour for asset editor.
                    if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor)
                    {
                        __result.x = prop->m_posX * 0.0164794922f;
                        __result.z = prop->m_posZ * 0.0164794922f;
                    }
                    else
                    {
                        // Calculate instance ID from buffer offset.
                        ushort propIndex;
                        fixed (PropInstance* buffer = Singleton<PropManager>.instance.m_props.m_buffer)
                        {
                            propIndex = (ushort)(prop - buffer);
                        }

                        // If precision data is available, use that.
                        if (PrecisionData.TryGetValue(propIndex, out PrecisionCoordinates precisionCoordinates))
                        {
                            if (prop->m_posX > 0)
                            {
                                __result.x = (prop->m_posX + (precisionCoordinates.X / (float)ushort.MaxValue)) * 0.263671875f;
                            }
                            else
                            {
                                __result.x = (prop->m_posX - (precisionCoordinates.X / (float)ushort.MaxValue)) * 0.263671875f;
                            }

                            if (prop->m_posZ > 0)
                            {
                                __result.z = (prop->m_posZ + (precisionCoordinates.Z / (float)ushort.MaxValue)) * 0.263671875f;
                            }
                            else
                            {
                                __result.z = (prop->m_posZ - (precisionCoordinates.Z / (float)ushort.MaxValue)) * 0.263671875f;
                            }
                        }
                        else
                        {
                            // No precision data available - use game default.
                            __result.x = (float)prop->m_posX * 0.263671875f;
                            __result.z = (float)prop->m_posZ * 0.263671875f;
                        }
                    }
                }
            }

            // Never execute original method.
            return false;
        }

        /// <summary>
        /// Harmony pre-emptive Prefix to PropInstance.Position setter to implement prop precision.
        /// </summary>
        /// <param name="__instance">PropInstance instance.</param>
        /// <param name="value">Original setter argument.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.Position), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool SetPositionPostfix(ref PropInstance __instance, Vector3 value)
        {
            // Unsafe, because we need to reverse-engineer the instance ID from the address offset.
            unsafe
            {
                fixed (void* pointer = &__instance)
                {
                    // Pointer to prop instance (used to calculate instance ID).
                    PropInstance* prop = (PropInstance*)pointer;

                    // Default editor behaviour for asset editor.
                    if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor)
                    {
                        prop->m_posX = (short)Mathf.Clamp(Mathf.RoundToInt(value.x * 60.68148f), -32767, 32767);
                        prop->m_posZ = (short)Mathf.Clamp(Mathf.RoundToInt(value.z * 60.68148f), -32767, 32767);
                        prop->m_posY = (ushort)Mathf.Clamp(Mathf.RoundToInt(value.y * 64f), 0, 65535);
                    }
                    else
                    {
                        // Default.
                        prop->m_posX = (short)Mathf.Clamp(/* Mathf.RoundToInt */(int)(value.x * 3.79259253f), -32767, 32767);
                        prop->m_posZ = (short)Mathf.Clamp(/* Mathf.RoundToInt */(int)(value.z * 3.79259253f), -32767, 32767);
                        prop->m_posY = (ushort)Mathf.Clamp(Mathf.RoundToInt(value.y * 64f), 0, 65535);

                        // Calculate precise coordinates.
                        PrecisionCoordinates precisionCoordinates = new PrecisionCoordinates
                        {
                            X = (ushort)(ushort.MaxValue * Mathf.Abs((value.x * 3.79259253f) - prop->m_posX)),
                            Z = (ushort)(ushort.MaxValue * Mathf.Abs((value.z * 3.79259253f) - prop->m_posZ)),
                        };

                        // Calculate instance ID from buffer offset.
                        fixed (PropInstance* buffer = Singleton<PropManager>.instance.m_props.m_buffer)
                        {
                            PrecisionData[(ushort)(prop - buffer)] = precisionCoordinates;
                        }
                    }
                }
            }

            // Never execute original method.
            return false;
        }

        /// <summary>
        /// Harmony pre-emptive Prefix to PropInstance.CalculateProp to implement prop snapping.
        /// </summary>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.CalculateProp))]
        [HarmonyPrefix]
        public static bool CalculatePropPrefix() => false;

        /// <summary>
        /// Harmony pre-emptive Prefix to PropInstance.AfterTerrainUpdated to implement prop snapping.
        /// </summary>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.AfterTerrainUpdated))]
        [HarmonyPrefix]
        public static bool AfterTerrainUpdatedPrefix() => false;

        /// <summary>
        /// Prop precision data struct.
        /// </summary>
        public struct PrecisionCoordinates
        {
            /// <summary>
            /// Precise X-coordinate.
            /// </summary>
            public ushort X;

            /// <summary>
            /// Precise Z-coordinate.
            /// </summary>
            public ushort Z;
        }
    }
}