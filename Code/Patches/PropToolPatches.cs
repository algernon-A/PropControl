// <copyright file="PropToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using HarmonyLib;
    using PropControl.MoveItSupport;
    using UnityEngine;
    using static ToolBase;

    /// <summary>
    /// Harmony patches to implement prop anarchy, snapping, and scaling.
    /// </summary>
    [HarmonyPatch(typeof(PropTool))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class PropToolPatches
    {
        /// <summary>
        /// Default prop scaling factor.
        /// </summary>
        internal const float DefaultScale = 1.0f;

        /// <summary>
        /// Default elevation adjustment factor.
        /// </summary>
        internal const float DefaultElevationAdjustment = 0f;

        // Status
        private static bool s_anarchyEnabled = true;
        private static bool s_snappingEnabled = false;

        // Prop scaling factor.
        private static float s_scaling = DefaultScale;

        // Prop elevation adjustment.
        private static float s_elevationAdjustment = DefaultElevationAdjustment;

        // Move It patches and integration.
        private static MoveItPatches s_moveItPatches;

        /// <summary>
        /// Gets or sets a value indicating whether prop anarchy is enabled.
        /// </summary>
        internal static bool AnarchyEnabled
        {
            get => s_anarchyEnabled;

            set
            {
                s_anarchyEnabled = value;

                // Update status panel.
                StandalonePanelManager<StatusPanel>.Panel?.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether prop snapping is enabled.
        /// </summary>
        internal static bool SnappingEnabled
        {
            get => s_snappingEnabled;

            set
            {
                s_snappingEnabled = value;

                // Update status panel.
                StandalonePanelManager<StatusPanel>.Panel?.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the current prop scaling factor.
        /// </summary>
        internal static float Scaling
        {
            get => s_scaling;

            set
            {
                // Enforce minimum bound.
                s_scaling = Mathf.Max(0.01f, value);
            }
        }

        /// <summary>
        /// Gets or sets the current elevation adjustment.
        /// </summary>
        internal static float ElevationAdjustment
        {
            get => s_elevationAdjustment;

            set
            {
                // Only change value if a prop is selected.
                if (Singleton<ToolController>.instance.CurrentTool is PropTool propTool && propTool.m_prefab is PropInfo)
                {
                    s_elevationAdjustment = value;
                }
            }
        }

        /// <summary>
        /// Increments the current scaling factor by the provided amount.
        /// </summary>
        /// <param name="increment">Amount to increment.</param>
        internal static void IncrementScaling(float increment)
        {
            // Update scaling.
            Scaling = s_scaling + increment;

            // Change Move It scaling, if applicable.
            s_moveItPatches?.IncrementScaling(increment);
        }

        /// <summary>
        /// Enables Move It integration and patching if Move It is enabled.
        /// </summary>
        internal static void CheckMoveIt()
        {
            // Check for enabled Move It mod.
            if (AssemblyUtils.GetEnabledAssembly("MoveIt") is Assembly moveIt)
            {
                // Create Move It patching instance.
                s_moveItPatches = new MoveItPatches(moveIt);
            }
        }

        /// <summary>
        /// Harmony pre-emptive prefix to PropTool.CheckPlacementErrors to implement prop tool anarchy.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>False (don't execute original method) if anarchy is enabled, true otherwise.</returns>
        [HarmonyPatch(nameof(PropTool.CheckPlacementErrors))]
        [HarmonyPrefix]
        private static bool CheckPlacementErrorsPrefix(out ToolErrors __result)
        {
            // Set default original result to no errors.
            __result = ToolErrors.None;

            // If anarchy isn't enabled, go on to execute original method (will override default original result assigned above).
            return !AnarchyEnabled;
        }

        /// <summary>
        /// Harmony postifx to PropTool.OnToolLateUpdate to implement prop elevation adjustment.
        /// </summary>
        /// <param name="___m_cachedPosition">PropTool private field m_cachedPosition (used for prop preview rendering).</param>
        [HarmonyPatch("OnToolLateUpdate")]
        [HarmonyPostfix]
        private static void OnToolLateUpdatePostfix(ref Vector3 ___m_cachedPosition)
        {
            // Apply elevation adjustment.
            ___m_cachedPosition.y += s_elevationAdjustment;
        }

        /// <summary>
        /// Harmony Transpiler for PropTool.RenderGeometry to implement prop scaling.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(PropTool.RenderGeometry))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RenderGeometryTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Looking for stloc.s 4, which stores the previewed prop's scale.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 4)
                {
                    // Multiply the calculated value by our scaling factor before storing.
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PropToolPatches), nameof(s_scaling)));
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony Transpiler for PropTool.SimulationStep to implement prop snapping.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(PropTool.SimulationStep))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Targeting m_currentEditObject alterations.
            int editObjectCount = 0;
            FieldInfo currentEditObject = AccessTools.Field(typeof(RaycastOutput), nameof(RaycastOutput.m_currentEditObject));

            foreach (CodeInstruction instruction in instructions)
            {
                // Looking for new RaycastInput constructor call.
                if (instruction.operand is ConstructorInfo constructor && constructor.DeclaringType == typeof(RaycastInput))
                {
                    // Change the RaycastInput for prop snapping.
                    Logging.Message("found raycast constructor");
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PropToolPatches), nameof(PropSnappingRaycast)));
                    continue;
                }
                else if (instruction.LoadsField(currentEditObject))
                {
                    // Replace calls to output.m_currentEditObject with predefined values.
                    switch (editObjectCount++)
                    {
                        // First and third cases are set to true (disable terrain height forcing and setting fixed height flag respectively).
                        case 0:
                        case 2:
                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            continue;

                        // Second case is set to false (CheckPlacementErrors call).
                        case 1:
                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            continue;
                    }
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Fixes a RaycastInput to implement prop snapping.
        /// </summary>
        /// <param name="raycast">Raycast to fix.</param>
        private static void PropSnappingRaycast(ref RaycastInput raycast)
        {
            // Building snapping.
            raycast.m_ignoreBuildingFlags = SnappingEnabled ? Building.Flags.None : Building.Flags.All;

            // Network snapping.
            raycast.m_ignoreNodeFlags = SnappingEnabled ? NetNode.Flags.None : NetNode.Flags.All;
            raycast.m_ignoreSegmentFlags = SnappingEnabled ? NetSegment.Flags.None : NetSegment.Flags.All;
        }
    }
}