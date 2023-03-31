// <copyright file="GeneralOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using AlgernonCommons;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal sealed class GeneralOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;
        private const float TitleMargin = 50f;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal GeneralOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("OPTIONS_GENERAL"), tabIndex, out UIButton _, autoLayout: false);

            // Y position indicator.
            float currentY = GroupMargin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(panel, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += languageDropDown.parent.height + Margin;

            // Logging checkbox.
            currentY += 20f;
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
            currentY += loggingCheck.height + GroupMargin;

            // Update on terrain change checkbox.
            UICheckBox terrainUpdateCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("TERRAIN_UPDATE"));
            terrainUpdateCheck.tooltip = Translations.Translate("TERRAIN_UPDATE_TIP");
            terrainUpdateCheck.isChecked = Patches.PropInstancePatches.UpdateOnTerrain;
            terrainUpdateCheck.eventCheckChanged += (c, isChecked) => { Patches.PropInstancePatches.UpdateOnTerrain = isChecked; };
            currentY += terrainUpdateCheck.height + 20f;

            UICheckBox keepAboveGroundCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("KEEP_ABOVEGROUND"));
            keepAboveGroundCheck.tooltip = Translations.Translate("KEEP_ABOVEGROUND_TIP");
            keepAboveGroundCheck.isChecked = Patches.PropInstancePatches.KeepAboveGround;
            keepAboveGroundCheck.eventCheckChanged += (c, isChecked) => { Patches.PropInstancePatches.KeepAboveGround = isChecked; };
            currentY += keepAboveGroundCheck.height + GroupMargin;

            // Key options.
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("KEYS"));
            currentY += TitleMargin;

            // Anarchy hotkey control.
            OptionsKeymapping anarchyKeyMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            anarchyKeyMapping.Label = Translations.Translate("KEY_ANARCHY");
            anarchyKeyMapping.Binding = UIThreading.AnarchyKey;
            anarchyKeyMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += anarchyKeyMapping.Panel.height + Margin;

            // Raise elevation key control.
            OptionsKeymapping elevationUpMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            elevationUpMapping.Label = Translations.Translate("KEY_ELEVATION_UP");
            elevationUpMapping.Binding = UIThreading.ElevationUpKey;
            elevationUpMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += elevationUpMapping.Panel.height + Margin;

            // Lower elevation key control.
            OptionsKeymapping elevationDownMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            elevationDownMapping.Label = Translations.Translate("KEY_ELEVATION_DOWN");
            elevationDownMapping.Binding = UIThreading.ElevationDownKey;
            elevationDownMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += elevationDownMapping.Panel.height + Margin;

            // Upscaling key control.
            OptionsKeymapping scaleUpMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            scaleUpMapping.Label = Translations.Translate("KEY_SCALE_UP");
            scaleUpMapping.Binding = UIThreading.ScaleUpKey;
            scaleUpMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += scaleUpMapping.Panel.height + Margin;

            // Downscaling key control.
            OptionsKeymapping scaleDownMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            scaleDownMapping.Label = Translations.Translate("KEY_SCALE_DOWN");
            scaleDownMapping.Binding = UIThreading.ScaleDownKey;
            scaleDownMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += scaleDownMapping.Panel.height + GroupMargin;

            UISlider keyDelaySlider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("REPEAT_DELAY"), 0.1f, 1.0f, 0.05f, UIThreading.KeyRepeatDelay);
            keyDelaySlider.eventValueChanged += (c, value) => UIThreading.KeyRepeatDelay = value;
        }
    }
}