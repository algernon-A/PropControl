// <copyright file="PropInstancePatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to implement prop anarchy, snapping, and scaling.
    /// </summary>
    [HarmonyPatch(typeof(PropInstance))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class PropInstancePatches
    {
        // Prop precision data.
        private static readonly Dictionary<ushort, PrecisionCoordinates> PrecisionData = new Dictionary<ushort, PrecisionCoordinates>();

        // Prop scaling data.
        private static readonly float[] ScalingData;

        // Update on terrain change.
        private static bool s_updateOnTerrain = false;
        private static bool s_keepAboveGround = true;

        /// <summary>
        /// Initializes static members of the <see cref="PropInstancePatches"/> class.
        /// </summary>
        static PropInstancePatches()
        {
            // Initialize scaling data array.
            ScalingData = new float[PropManager.MAX_PROP_COUNT];
            for (int i = 0; i < PropManager.MAX_PROP_COUNT; ++i)
            {
                ScalingData[i] = 1.0f;
            }
        }

        /// <summary>
        /// Gets the prop precision data dictionary.
        /// </summary>
        internal static Dictionary<ushort, PrecisionCoordinates> PrecisionDict => PrecisionData;

        /// <summary>
        /// Gets the prop scaling data array.
        /// </summary>
        internal static float[] ScalingArray => ScalingData;

        /// <summary>
        /// Gets or sets a value indicating whether prop Y-positions should be updated on terrain changes.
        /// </summary>
        internal static bool UpdateOnTerrain { get => s_updateOnTerrain; set => s_updateOnTerrain = value; }

        /// <summary>
        /// Gets or sets a value indicating whether props should be raised to ground level if the terrain is raised above them.
        /// </summary>
        internal static bool KeepAboveGround { get => s_keepAboveGround; set => s_keepAboveGround = value; }

        /// <summary>
        /// Harmony pre-emptive Prefix to PropInstance.Blocked setter to implement prop tool anarchy.
        /// </summary>
        /// <param name="__instance">PropInstance instance.</param>
        /// <param name="value">Original setter argument.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.Blocked), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool SetBlockedPrefix(ref PropInstance __instance, bool value)
        {
            // Never apply blocked flag; only unblock.
            if (!value)
            {
                __instance.m_flags = (ushort)(__instance.m_flags & (int)~PropInstance.Flags.Blocked);
            }

            return !PropToolPatches.AnarchyEnabled;
        }

        /// <summary>
        /// Harmony pre-emptive prefix to PropInstance.Position getter to implement prop precision.
        /// </summary>
        /// <param name="__instance">PropInstance instance.</param>
        /// <param name="__result">Original getter result.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.Position), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool GetPositionPrefix(ref PropInstance __instance, ref Vector3 __result)
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
        /// Harmony pre-emptive prefix to PropInstance.Position setter to implement prop precision.
        /// </summary>
        /// <param name="__instance">PropInstance instance.</param>
        /// <param name="value">Original setter argument.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.Position), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool SetPositionPostfix(ref PropInstance __instance, Vector3 value)
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
        /// Harmony pre-emptive prefix to PropInstance.CalculateProp to implement prop snapping.
        /// </summary>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropInstance.CalculateProp))]
        [HarmonyPrefix]
        private static bool CalculatePropPrefix(ref PropInstance __instance)
        {
            // Only do this for created props with no recorded Y position.
            if (((__instance.m_flags & (ushort)PropInstance.Flags.Created) == 1) &
                     __instance.m_posY == 0)
            {
                // Move prop to terrain height.
                Vector3 position = __instance.Position;
                position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                __instance.m_posY = (ushort)Mathf.Clamp(Mathf.RoundToInt(position.y * 64f), 0, 65535);
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Harmony transpiler to PropInstance.AfterTerrainUpdated to implement prop snapping.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(PropInstance.AfterTerrainUpdated))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AfterTerrainUpdatedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo m_posY = AccessTools.Field(typeof(PropInstance), nameof(PropInstance.m_posY));

            // Looking for store to ushort num (local var 1).
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.StoresField(m_posY))
                {
                    // Insert call to our custom method.
                    AlgernonCommons.Logging.KeyMessage("Found store m_posY");
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, m_posY);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PropInstancePatches), nameof(CalculateElevation)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for PropInstance.RenderInstance to implement prop scaling.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(PropInstance.RenderInstance), new Type[] { typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RenderInstanceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Looking for stloc.s 4, which is the scale to be rendered.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 4)
                {
                    // Multiply the calculated value by our scaling factor before storing.
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PropInstancePatches), nameof(ScalingData)));
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldelem, typeof(float));
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Calculates a prop's elevation given current settings.
        /// </summary>
        /// <param name="terrainY">Terrain elevation.</param>
        /// <param name="propY">Prop elevation.</param>
        /// <returns>Calculated prop Y coordinate per current settings.</returns>
        private static ushort CalculateElevation(ushort terrainY, ushort propY)
        {
            if (s_updateOnTerrain)
            {
                // Default game behaviour - terrain height.
                // However, only this if the TerrainTool is active, to avoid surface ruining changes triggering a reset of newly-placed props.
                return Singleton<ToolController>.instance.CurrentTool is TerrainTool ? terrainY : propY;
            }

            if (s_keepAboveGround)
            {
                // Keeping prop above ground - return higher of the two values.
                return Math.Max(terrainY, propY);
            }

            // Not updating with terrain changes - keep original prop height.
            return propY;
        }

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