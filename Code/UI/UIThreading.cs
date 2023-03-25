// <copyright file="UIThreading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ACME
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
        // Instance reference.
        private static UIThreading s_instance;

        // Hotkey.
        private static Keybinding s_anarchyKey = new Keybinding(KeyCode.P, true, false, false);

        // Flags.
        private bool _anarchyKeyProcessed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIThreading"/> class.
        /// </summary>
        public UIThreading()
        {
            // Set instance reference.
            s_instance = this;
        }

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
        }
    }
}