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
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using static ToolBase;

    /// <summary>
    /// Harmony patch to implement free prop placement.
    /// </summary>
    [HarmonyPatch(typeof(PropTool))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class PropToolPatches
    {
        /// <summary>
        /// Default prop scaling factor.
        /// </summary>
        internal const float DefaultScale = 1.0f;

        /// <summary>
        /// Default elevation adjustment factor.
        /// </summary>
        internal const float DefaultElevationAdjustment = 0f;

        // Prop scaling factor.
        private static float s_scaling = DefaultScale;

        // Prop elevation adjustment.
        private static float s_elevationAdjustment = DefaultElevationAdjustment;

        /// <summary>
        /// Gets or sets a value indicating whether prop anarchy is enabled.
        /// </summary>
        internal static bool AnarchyEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the current prop scaling factor.
        /// </summary>
        internal static float Scaling
        {
            get => s_scaling;

            set
            {
                // Only change value if a prop is selected.
                if (Singleton<ToolController>.instance.CurrentTool is PropTool propTool && propTool.m_prefab is PropInfo)
                {
                    s_scaling = value;
                }
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
        /// Harmony pre-emptive prefix to PropTool.CheckPlacementErrors to implement prop tool anarchy.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>False (don't execute original method) if anarchy is enabled, true otherwise.</returns>
        [HarmonyPatch(nameof(PropTool.CheckPlacementErrors))]
        [HarmonyPrefix]
        public static bool CheckPlacementErrorsPrefix(out ToolErrors __result)
        {
            // Set default original result to no errors.
            __result = ToolErrors.None;

            // If anarchy isn't enabled, go on to execute original method (will override default original result assigned above).
            return !AnarchyEnabled;
        }

        /// <summary>
        /// Harmony prefix to PropTool.CreateProp to implement prop elevation adjustment.
        /// </summary>
        /// <param name="___m_mousePosition">PropTool private field m_mousePosition (used for prop placement).</param>
        [HarmonyPatch("CreateProp")]
        [HarmonyPrefix]
        public static void CreatePropPrefix(ref Vector3 ___m_mousePosition)
        {
            // Apply elevation adjustment.
            ___m_mousePosition.y += s_elevationAdjustment;
        }

        /// <summary>
        /// Harmony postifx to PropTool.OnToolLateUpdate to implement prop elevation adjustment.
        /// </summary>
        /// <param name="___m_cachedPosition">PropTool private field m_cachedPosition (used for prop preview rendering).</param>
        [HarmonyPatch("OnToolLateUpdate")]
        [HarmonyPostfix]
        public static void OnToolLateUpdatePostfix(ref Vector3 ___m_cachedPosition)
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
        internal static IEnumerable<CodeInstruction> RenderGeometryTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Looking for new RaycastInput constructor call.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 4)
                {
                    // Change the RaycastInput for prop snapping.
                    yield return new CodeInstruction(OpCodes.Ldsfld, typeof(PropToolPatches).GetField(nameof(s_scaling), BindingFlags.NonPublic | BindingFlags.Static));
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
        internal static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            int editObjectCount = 0;

            // Looking for new RaycastInput constructor call.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand is ConstructorInfo constructor && constructor.DeclaringType == typeof(RaycastInput))
                {
                    // Change the RaycastInput for prop snapping.
                    Logging.Message("found raycast constructor");
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PropToolPatches), nameof(PropSnappingRaycast)));
                    continue;
                }
                else if (instruction.LoadsField(AccessTools.Field(typeof(RaycastOutput), nameof(RaycastOutput.m_currentEditObject))))
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
            raycast.m_ignoreBuildingFlags = Building.Flags.None;
            raycast.m_ignoreNodeFlags = NetNode.Flags.None;
            raycast.m_ignoreSegmentFlags = NetSegment.Flags.None;
            raycast.m_buildingService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            raycast.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            raycast.m_netService2 = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            raycast.m_currentEditObject = true;
        }
    }
}