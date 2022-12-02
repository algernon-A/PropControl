// <copyright file="DataSerializerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches to implement prop anarchy and snapping.
    /// </summary>
    [HarmonyPatch(typeof(DataSerializer))]
    public static class DataSerializerPatches
    {
        /// <summary>
        /// Harmony Transpiler to DataSerializer.WriteSharedType setter to pretend to be the original BP Prop Snapping mod when saving data.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DataSerializer.WriteSharedType))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Target method.
            MethodInfo writeUniqueString = AccessTools.Method(typeof(DataSerializer), nameof(DataSerializer.WriteUniqueString));

            // Replace call to WriteUniqueString with a call to our custom method.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(writeUniqueString))
                {
                    instruction.operand = AccessTools.Method(typeof(DataSerializerPatches), nameof(InterceptWriteString));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces Prop Snapping data type string for serialization with original PropSnapping string (for backwards-compatibility).
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        /// <param name="value">Data type string.</param>
        private static void InterceptWriteString(DataSerializer serializer, string value)
        {
            if (value.StartsWith("PropSnapping.Data, PropControl"))
            {
                Logging.Message("saving prop snapping data while impersonating PropSnapping mod (shh, don't tell anyone)");
                serializer.WriteUniqueString("PropSnapping.Data, PropSnapping, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null.");
            }
            else if (value.StartsWith("PropPrecision.Data, PropControl"))
            {
                Logging.Message("saving prop precision data while impersonating PropPrecision mod (shh, don't tell anyone)");
                serializer.WriteUniqueString("PropPrecision.Data, PropPrecision, Version=1.0.6149.17591, Culture=neutral, PublicKeyToken=null.");
            }
            else
            {
                serializer.WriteUniqueString(value);
            }
        }
    }
}