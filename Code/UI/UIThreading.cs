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
        private static Keybinding s_upscaleKey = new Keybinding(KeyCode.Period, false, false, false);
        private static Keybinding s_downscaleKey = new Keybinding(KeyCode.Comma, false, false, false);

        // Flags.
        private bool _anarchyKeyProcessed = false;
        private bool _upscaleKeyProcessed = false;
        private bool _downscaleKeyProcessed = false;

        // Timestamps.
        private float _keyTimer;

        /// <summary>
        /// Gets or sets the prop anarchy hotkey.
        /// </summary>
        internal static Keybinding AnarchyKey { get => s_anarchyKey; set => s_anarchyKey = value; }

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
            if (s_upscaleKey.IsPressed())
            {
                // Get time.
                float now = Time.time;

                // Only process if we're not already doing so.
                if (!_upscaleKeyProcessed)
                {
                    // Set processed flag.
                    _upscaleKeyProcessed = true;

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
                _upscaleKeyProcessed = false;
            }

            // Check for upscaling hotkey.
            if (s_downscaleKey.IsPressed())
            {
                // Get time.
                float now = Time.time;

                // Only process if we're not already doing so.
                if (!_downscaleKeyProcessed)
                {
                    // Set processed flag.
                    _downscaleKeyProcessed = true;

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
                _downscaleKeyProcessed = false;
            }
        }
    }
}