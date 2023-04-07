// <copyright file="VisibilityOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;
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

        // Panel components.
        private UIPanel _sliderPanel;
        private UISlider _fallbackDistanceSlider;
        private UISlider _minimumDistanceSlider;
        private UISlider _distanceMultiplierSlider;
        private UISlider _lodTransitionSlider;

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

            // Create sub-panel for sliders.
            _sliderPanel = panel.AddUIComponent<UIPanel>();
            _sliderPanel.relativePosition = new Vector2(0f, currentY);
            _sliderPanel.autoSize = false;
            _sliderPanel.autoLayout = false;
            _sliderPanel.width = panel.width;
            float panelY = 0f;

            _fallbackDistanceSlider = UISliders.AddPlainSliderWithIntegerValue(_sliderPanel, LeftMargin, panelY, Translations.Translate("FALLBACK_DISTANCE"), MinFallbackDistance, MaxFallbackDistance, 1000f, FallbackRenderDistance);
            _fallbackDistanceSlider.eventValueChanged += (c, value) => FallbackRenderDistance = value;
            _fallbackDistanceSlider.parent.tooltip = Translations.Translate("FALLBACK_DISTANCE_TIP");
            panelY += _fallbackDistanceSlider.parent.height + Margin;

            _minimumDistanceSlider = UISliders.AddPlainSliderWithIntegerValue(_sliderPanel, LeftMargin, panelY, Translations.Translate("MIN_DISTANCE"), MinMinimumDistance, MaxMinimumDistance, 1f, MinimumDistance);
            _minimumDistanceSlider.eventValueChanged += (c, value) => MinimumDistance = value;
            _minimumDistanceSlider.parent.tooltip = Translations.Translate("MIN_DISTANCE_TIP");
            panelY += _minimumDistanceSlider.parent.height + Margin;

            _distanceMultiplierSlider = UISliders.AddPlainSliderWithIntegerValue(_sliderPanel, LeftMargin, panelY, Translations.Translate("DISTANCE_MULT"), MinDistanceMultiplier, MaxDistanceMultiplier, 1f, DistanceMultiplier);
            _distanceMultiplierSlider.eventValueChanged += (c, value) => DistanceMultiplier = value;
            _distanceMultiplierSlider.parent.tooltip = Translations.Translate("DISTANCE_MULT_TIP");
            panelY += _distanceMultiplierSlider.parent.height + Margin;

            _lodTransitionSlider = UISliders.AddPlainSliderWithValue(_sliderPanel, LeftMargin, panelY, Translations.Translate("LOD_TRANSITION"), MinLODTransitionMultiplier, MaxLODTransitionMultiplier, 0.05f, LODTransitionMultiplier);
            _lodTransitionSlider.eventValueChanged += (c, value) => LODTransitionMultiplier = value;
            _lodTransitionSlider.parent.tooltip = Translations.Translate("LOD_TRANSITION_TIP");
            panelY += _lodTransitionSlider.parent.height + Margin;

            UIButton defaultsButton = UIButtons.AddButton(_sliderPanel, LeftMargin, panelY, Translations.Translate("RESET_DEFAULT"), 300f);
            defaultsButton.eventClicked += (c, p) =>
            {
                _fallbackDistanceSlider.value = DefaultFallbackDistance;
                _minimumDistanceSlider.value = DefaultMinimumDistance;
                _distanceMultiplierSlider.value = DefaultDistanceMultiplier;
                _lodTransitionSlider.value = DefaultLODTransitionMultiplier;
            };
            panelY += 25f;

            // Adaptive visibility checkbox event handler.
            enableAPVDCheck.eventCheckChanged += (c, isChecked) =>
            {
                Patcher.EnableAdaptiveVisibility = isChecked;

                // Toggle slider visibility.
                _sliderPanel.isVisible = isChecked;
            };

            // Set slider panel initial visibility.
            _sliderPanel.isVisible = enableAPVDCheck.isChecked;
        }
    }
}