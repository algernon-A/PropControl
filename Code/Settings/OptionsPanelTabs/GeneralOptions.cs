// <copyright file="GeneralOptions.cs" company="algernon (K. Algernon A. Sheppard)">
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

            // Header.
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);

            // UI options.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("UI_OPTIONS"));
            currentY += TitleMargin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(panel, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += languageDropDown.parent.height + Margin;

            // Show panel checkbox.
            UICheckBox showButtonCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("SHOW_BUTTONS"));
            showButtonCheck.isChecked = StatusPanel.ShowButtons;
            showButtonCheck.eventCheckChanged += (c, isChecked) => { StatusPanel.ShowButtons = isChecked; };
            currentY += showButtonCheck.height + 10f;

            // UI transparency checkbox.
            UICheckBox transparencyCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("TRANSPARENT_UI"));
            transparencyCheck.isChecked = StatusPanel.TransparentUI;
            transparencyCheck.eventCheckChanged += (c, isChecked) => { StatusPanel.TransparentUI = isChecked; };
            currentY += transparencyCheck.height + 10f;

            // Initial anarchy state.
            UICheckBox enableAfterLoadCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("INITIAL_STATUS"));
            enableAfterLoadCheck.isChecked = Loading.InitialAnarchyState;
            enableAfterLoadCheck.eventCheckChanged += (c, isChecked) => { Loading.InitialAnarchyState = isChecked; };
            currentY += enableAfterLoadCheck.height + GroupMargin;

            // Reset position button.
            UIButton resetPositionButton = UIButtons.AddButton(panel, LeftMargin, currentY, Translations.Translate("RESET_POS"), 300f);
            resetPositionButton.eventClicked += (c, p) => StandalonePanelManager<StatusPanel>.ResetPosition();
            currentY += resetPositionButton.height + 20f;

            // Prop options.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("PROP_OPTIONS"));
            currentY += TitleMargin;

            // Update on terrain change checkboxes.
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

            // Troubleshooting options.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("TROUBLESHOOTING"));
            currentY += TitleMargin;

            // Logging checkbox.
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
        }
    }
}