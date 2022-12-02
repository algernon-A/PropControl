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
    using HarmonyLib;
    using static ToolBase;

    /// <summary>
    /// Harmony patch to implement free prop placement.
    /// </summary>
    [HarmonyPatch(typeof(PropTool))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class PropToolPatches
    {
        /// <summary>
        /// Harmony pre-emptive Prefix to PropTool.CheckPlacementErrors to implement prop tool anarchy.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(PropTool.CheckPlacementErrors))]
        [HarmonyPrefix]
        public static bool CheckPlacementErrorsPrefix(out ToolErrors __result)
        {
            // Override original result.
            __result = ToolErrors.None;
            return false;
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