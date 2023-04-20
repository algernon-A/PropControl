// <copyright file="MoveItPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl.MoveItSupport
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;
    using MoveIt;
    using PropControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to add Move It integration.
    /// </summary>
    internal class MoveItPatches
    {
        // Move It type and field - using reflection and delegates here to avoid a hard dependency with Move It.
        private readonly Type _moveablePropType;
        private readonly FieldInfo _lastInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveItPatches"/> class.
        /// Attempts to patch Move It for integration.
        /// </summary>
        /// <param name="moveIt">Move It assembly.</param>
        internal MoveItPatches(Assembly moveIt)
        {
            // Reflect Move It tool.
            Type moveItToolType = moveIt.GetType("MoveIt.MoveItTool");
            if (moveItToolType == null)
            {
                Logging.KeyMessage("Move It tool type not found");
                return;
            }

            Logging.KeyMessage("found MoveItTool");

            // Set Move It tree snapping field.
            FieldInfo treeSnapping = AccessTools.Field(moveItToolType, "treeSnapping");
            if (treeSnapping != null)
            {
                treeSnapping.SetValue(null, true);
            }
            else
            {
                Logging.Error("unable to reflect MoveItTool.treeSnapping field");
            }

            // Get Move It MoveableProp type.
            _moveablePropType = moveIt.GetType("MoveIt.MoveableProp");
            if (_moveablePropType == null)
            {
                Logging.Error("unable to reflect MoveIt.MoveableProp");
            }

            // Get last instance field.
            _lastInstance = AccessTools.Field(moveItToolType, "m_lastInstance");
            if (_lastInstance == null)
            {
                Logging.Error("unable to reflect MoveItTool.m_lastInstance");
            }

            // Apply tranpiler to MoveIt.RenderCloneGeometry.
            PatcherManager<Patcher>.Instance.TranspileMethod(
                AccessTools.Method(_moveablePropType, "RenderCloneGeometryImplementation"),
                AccessTools.Method(typeof(MoveItPatches), nameof(RenderCloneGeometryImplementationTranspiler)));
        }

        /// <summary>
        /// Applies the given scaling increment to any props currently selected by Move It.
        /// </summary>
        /// <param name="increment">Scaling increment to apply.</param>
        internal void IncrementScaling(float increment)
        {
            Logging.KeyMessage("incrementPropSize");

            // Check for active Move It tool in its default state.
            if (Singleton<ToolController>.instance.CurrentTool is MoveItTool && MoveItTool.ToolState == MoveItTool.ToolStates.Default)
            {
                // See if any active selection.
                if (MoveIt.Action.selection.Count > 0)
                {

                    Logging.KeyMessage("iterating through props");

                    // Active selection - iterate through each item, checking for props.
                    PropManager propManager = Singleton<PropManager>.instance;
                    foreach (Instance instance in MoveIt.Action.selection)
                    {
                        ushort propID = instance.id.Prop;
                        if (instance is MoveableProp && propID > 0)
                        {
                            // Found a prop - apply scaling.
                            float newValue = PropInstancePatches.ScalingArray[propID] + increment;
                            PropInstancePatches.ScalingArray[propID] = Mathf.Max(0.01f, newValue);

                            Logging.KeyMessage("incremented scaling to ", PropInstancePatches.ScalingArray[propID]);

                            // Update the prop.
                            propManager.UpdateProp(propID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the Move It tool is active, and if so, if a prop is selected.
        /// </summary>
        /// <param name="currentTool">Currently selected tool.</param>
        /// <returns><c>true</c> if the Move It tool is active and a prop is selected, <c>false</c> otherwise.</returns>
        internal bool IsMoveItProp(ToolBase currentTool)
        {
            // Check for MoveIt tool.
            if (currentTool is MoveItTool)
            {
                // Get Move It's m_lastInstance and check if it's a MoveIt MoveableProp.
                object lastInstanceObj = _lastInstance.GetValue(currentTool);
                Logging.KeyMessage("IsMoveItProp called ", lastInstanceObj is MoveableProp);

                return lastInstanceObj is MoveableProp;
            }

            // Default is no.
            return false;
        }

        /// <summary>
        /// Harmony transpiler for MoveIt.MoveableProp.RenderCloneGeometryImplementation to implement prop scaling.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> RenderCloneGeometryImplementationTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Looking for stloc.3, which stores the previewed prop's scale.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_3)
                {
                    // Multiply the calculated value by our scaling factor before storing.
                    Logging.Message("found stloc.s");
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PropToolPatches), "s_scaling"));
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return instruction;
            }
        }
    }
}