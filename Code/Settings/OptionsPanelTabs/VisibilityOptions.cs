// <copyright file="VisibilityOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using static Patches.PropInfoPatches;

    /// <summary>
    /// Options panel for setting adaptive prop visibility options.
    /// </summary>
    internal sealed class VisibilityOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal VisibilityOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("PROP_VISIBILITY"), tabIndex, out UIButton _, autoLayout: false);

            // Y position indicator.
            float currentY = GroupMargin;

            // Update on terrain change checkboxes.
            UICheckBox enableAPVDCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("ADAPTIVE_VISIBILITY"));
            enableAPVDCheck.tooltip = Translations.Translate("ADAPTIVE_VISIBILITY_TIP");
            enableAPVDCheck.isChecked = Patcher.EnableAdaptiveVisibility;
            currentY += enableAPVDCheck.height + GroupMargin;

            UISlider fallbackDistanceSlider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("FALLBACK_DISTANCE"), MinFallbackDistance, MaxFallbackDistance, 1000f, FallbackRenderDistance);
            fallbackDistanceSlider.eventValueChanged += (c, value) => FallbackRenderDistance = value;
            fallbackDistanceSlider.parent.tooltip = Translations.Translate("FALLBACK_DISTANCE_TIP");
            currentY += fallbackDistanceSlider.parent.height + Margin;

            UISlider minimumDistanceSlider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("MIN_DISTANCE"), MinMinimumDistance, MaxMinimumDistance, 1f, MinimumDistance);
            minimumDistanceSlider.eventValueChanged += (c, value) => MinimumDistance = value;
            minimumDistanceSlider.parent.tooltip = Translations.Translate("MIN_DISTANCE_TIP");
            currentY += minimumDistanceSlider.parent.height + Margin;

            UISlider distanceMultiplierSlider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("DISTANCE_MULT"), MinDistanceMultiplier, MaxDistanceMultiplier, 1f, DistanceMultiplier);
            distanceMultiplierSlider.eventValueChanged += (c, value) => DistanceMultiplier = value;
            distanceMultiplierSlider.parent.tooltip = Translations.Translate("DISTANCE_MULT_TIP");
            currentY += distanceMultiplierSlider.parent.height + Margin;

            UISlider lodTransitionSlider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("LOD_TRANSITION"), MinLODTransitionMultiplier, MaxLODTransitionMultiplier, 0.05f, LODTransitionMultiplier);
            lodTransitionSlider.eventValueChanged += (c, value) => LODTransitionMultiplier = value;
            lodTransitionSlider.parent.tooltip = Translations.Translate("LOD_TRANSITION_TIP");
            currentY += lodTransitionSlider.parent.height + Margin;

            // Adaptive visibility checkbox event handler.
            enableAPVDCheck.eventCheckChanged += (c, isChecked) =>
            {
                Patcher.EnableAdaptiveVisibility = isChecked;

                // Toggle slider visibility.
                fallbackDistanceSlider.parent.isVisible = isChecked;
                minimumDistanceSlider.parent.isVisible = isChecked;
                distanceMultiplierSlider.parent.isVisible = isChecked;
                lodTransitionSlider.parent.isVisible = isChecked;
            };
        }
    }
}