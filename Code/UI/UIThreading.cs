// <copyright file="UIThreading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using AlgernonCommons.Keybinding;
    using ICities;
    using PropControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Threading to capture hotkeys.
    /// </summary>
    public class UIThreading : ThreadingExtensionBase
    {
        // Scaling step - initial (on keydown).
        private const float InitialScalingIncrement = 0.05f;

        // Scaling step - repeating, per second.
        private const float RepeatScalingIncrement = 0.5f;

        // Delay before key repeating activates.
        private const float InitialRepeatDelay = 0.5f;

        // Hotkeys.
        private static Keybinding s_anarchyKey = new Keybinding(KeyCode.P, true, false, false);

        // Function keys.
        private static Keybinding s_scaleUpKey = new Keybinding(KeyCode.Period, false, false, false);
        private static Keybinding s_scaleDownKey = new Keybinding(KeyCode.Comma, false, false, false);

        // Flags.
        private bool _anarchyKeyProcessed = false;
        private bool _scaleUpKeyProcessed = false;
        private bool _scaleDownKeyProcessed = false;

        // Timestamps.
        private float _keyTimer;

        /// <summary>
        /// Gets or sets the prop anarchy hotkey.
        /// </summary>
        internal static Keybinding AnarchyKey { get => s_anarchyKey; set => s_anarchyKey = value; }

        /// <summary>
        /// Gets or sets the prop upscaling key.
        /// </summary>
        internal static Keybinding ScaleUpKey { get => s_scaleUpKey; set => s_scaleUpKey = value; }

        /// <summary>
        /// Gets or sets the prop downscaling key.
        /// </summary>
        internal static Keybinding ScaleDownKey { get => s_scaleDownKey; set => s_scaleDownKey = value; }

        /// <summary>
        /// Look for keypress to activate tool.
        /// </summary>
        /// <param name="realTimeDelta">Real-time delta since last update.</param>
        /// <param name="simulationTimeDelta">Simulation time delta since last update.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Check for anarchy hotkey.
            if (s_anarchyKey.IsPressed())
            {
                // Only process if we're not already doing so.
                if (!_anarchyKeyProcessed)
                {
                    // Set processed flag.
                    _anarchyKeyProcessed = true;

                    // Toggle anarchy.
                    PropToolPatches.AnarchyEnabled = !PropToolPatches.AnarchyEnabled;
                }
            }
            else
            {
                // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                _anarchyKeyProcessed = false;
            }

            // Check for upscaling keypress.
            if (s_scaleUpKey.IsPressed())
            {
                // Get time.
                float now = Time.time;

                // Only process if we're not already doing so.
                if (!_scaleUpKeyProcessed)
                {
                    // Set processed flag.
                    _scaleUpKeyProcessed = true;

                    // Increment scaling.
                    PropToolPatches.Scaling += InitialScalingIncrement;

                    // Record keypress time.
                    _keyTimer = now + InitialRepeatDelay;
                }
                else
                {
                    // Handle key repeat, if appropriate.
                    if (now > _keyTimer)
                    {
                        PropToolPatches.Scaling += RepeatScalingIncrement * Time.deltaTime;
                    }
                }
            }
            else
            {
                // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                _scaleUpKeyProcessed = false;
            }

            // Check for upscaling hotkey.
            if (s_scaleDownKey.IsPressed())
            {
                // Get time.
                float now = Time.time;

                // Only process if we're not already doing so.
                if (!_scaleDownKeyProcessed)
                {
                    // Set processed flag.
                    _scaleDownKeyProcessed = true;

                    // Increment scaling.
                    PropToolPatches.Scaling -= InitialScalingIncrement;

                    // Record keypress time.
                    _keyTimer = now + InitialRepeatDelay;
                }
                else
                {
                    // Handle key repeat, if appropriate.
                    if (now > _keyTimer)
                    {
                        PropToolPatches.Scaling -= RepeatScalingIncrement * Time.deltaTime;
                    }
                }
            }
            else
            {
                // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                _scaleDownKeyProcessed = false;
            }
        }
    }
}