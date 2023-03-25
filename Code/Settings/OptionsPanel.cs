// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;
    using static Patches.PropInfoPatches;

    /// <summary>
    /// The mod's settings options panel.
    /// </summary>
    public sealed class OptionsPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        internal OptionsPanel()
        {
            // Add controls.
            // Y position indicator.
            float currentY = Margin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(this, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += languageDropDown.parent.height + Margin;

            // Logging checkbox.
            currentY += 20f;
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
            currentY += 25f;

            // Adaptive prop visibility sliders.
            UISpacers.AddTitleSpacer(this, 0f, currentY, OptionsPanelManager<OptionsPanel>.PanelWidth, Translations.Translate("PROP_VISIBILITY"));
            currentY += 50f;

            UISlider fallbackDistanceSlider = UISliders.AddPlainSliderWithValue(this, LeftMargin, currentY, Translations.Translate("FALLBACK_DISTANCE"), MinFallbackDistance, MaxFallbackDistance, 1000f, FallbackRenderDistance);
            fallbackDistanceSlider.eventValueChanged += (c, value) => FallbackRenderDistance = value;
            fallbackDistanceSlider.tooltip = Translations.Translate("FALLBACK_DISTANCE_TIP");
            currentY += fallbackDistanceSlider.parent.height + Margin;

            UISlider minimumDistanceSlider = UISliders.AddPlainSliderWithValue(this, LeftMargin, currentY, Translations.Translate("MIN_DISTANCE"), MinMinimumDistance, MaxMinimumDistance, 1f, MinimumDistance);
            minimumDistanceSlider.eventValueChanged += (c, value) => MinimumDistance = value;
            minimumDistanceSlider.tooltip = Translations.Translate("MIN_DISTANCE_TIP");
            currentY += minimumDistanceSlider.parent.height + Margin;

            UISlider distanceMultiplierSlider = UISliders.AddPlainSliderWithValue(this, LeftMargin, currentY, Translations.Translate("DISTANCE_MULT"), MinDistanceMultiplier, MaxDistanceMultiplier, 1f, DistanceMultiplier);
            distanceMultiplierSlider.eventValueChanged += (c, value) => DistanceMultiplier = value;
            distanceMultiplierSlider.tooltip = Translations.Translate("DISTANCE_MULT_TIP");
            currentY += distanceMultiplierSlider.parent.height + Margin;

            UISlider lodTransitionSlider = UISliders.AddPlainSliderWithValue(this, LeftMargin, currentY, Translations.Translate("LOD_TRANSITION"), MinLODTransitionMultiplier, MaxLODTransitionMultiplier, 0.05f, LODTransitionMultiplier);
            lodTransitionSlider.eventValueChanged += (c, value) => LODTransitionMultiplier = value;
            lodTransitionSlider.tooltip = Translations.Translate("LOD_TRANSITION_TIP");
            currentY += lodTransitionSlider.parent.height + Margin;
        }
    }
}