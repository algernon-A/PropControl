﻿// <copyright file="StatusPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace PropControl
{
    using System.Text;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using PropControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Icon status panel.
    /// </summary>
    internal class StatusPanel : StandalonePanelBase
    {
        // Layout constants.
        private const float ButtonSize = 36f;
        private const float ButtonSpacing = 2f;

        // Panel settings.
        private static bool s_showButtons = true;
        private static bool s_transparentUI = false;

        // Panel components.
        private UIMultiStateButton _anarchyButton;
        private UIMultiStateButton _snappingButton;

        // Dragging.
        private bool _dragging = false;
        private Vector3 _lastDragPosition;

        // Event handling.
        private bool _ignoreEvents = false;

        /// <summary>
        /// Gets or sets a value indicating whether the status panel should be shown.
        /// </summary>
        public static bool ShowButtons
        {
            get => s_showButtons;

            set
            {
                // Don't do anything if no change.
                if (value != s_showButtons)
                {
                    s_showButtons = value;

                    // Showing - create panel if in-game.
                    if (value)
                    {
                        if (Loading.IsLoaded)
                        {
                            StandalonePanelManager<StatusPanel>.Create();
                        }
                    }
                    else
                    {
                        // Hiding - close status panel if open.
                        StandalonePanelManager<StatusPanel>.Panel?.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the status panel should use transparent buttons.
        /// </summary>
        public static bool TransparentUI
        {
            get => s_transparentUI;

            set
            {
                // Don't do anything if no change.
                if (value != s_transparentUI)
                {
                    s_transparentUI = value;

                    // Regnerate status panel if open.
                    if (StandalonePanelManager<StatusPanel>.Panel is StatusPanel panel)
                    {
                        panel.Close();
                        StandalonePanelManager<StatusPanel>.Create();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public override float PanelWidth => (ButtonSize * 2f) + ButtonSpacing;

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        public override float PanelHeight => ButtonSize;

        /// <summary>
        /// Called by Unity before the first frame.
        /// Used to perform setup.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Options panel toggles.
            UITextureAtlas tcAtlas = UITextures.CreateSpriteAtlas("PropControl", 1024, string.Empty);

            _anarchyButton = AddToggleButton(this, "Prop anarchy status", tcAtlas, "AnarchyOff", "AnarchyOn");
            _anarchyButton.relativePosition = Vector2.zero;
            _anarchyButton.tooltipBox = UIToolTips.WordWrapToolTipBox();
            _anarchyButton.eventActiveStateIndexChanged += (c, state) =>
            {
                // Don't do anything if ignoring events.
                if (!_ignoreEvents)
                {
                    PropToolPatches.AnarchyEnabled = state != 0;
                }
            };

            _snappingButton = AddToggleButton(this, "Prop snapping status", tcAtlas, "SnappingOff", "SnappingOn");
            _snappingButton.tooltipBox = UIToolTips.WordWrapToolTipBox();
            _snappingButton.relativePosition = new Vector2(ButtonSize + ButtonSpacing, 0f);
            _snappingButton.eventActiveStateIndexChanged += (c, state) =>
            {
                // Don't do anything if ignoring events.
                if (!_ignoreEvents)
                {
                    PropToolPatches.SnappingEnabled = state != 0;
                }
            };

            // Enable right-click dragging.
            _snappingButton.eventMouseMove += Drag;
            _anarchyButton.eventMouseMove += Drag;

            // Set intial button states.
            Refresh();
        }

        /// <summary>
        /// Applies the panel's default position.
        /// </summary>
        public override void ApplyDefaultPosition()
        {
            // Set position.
            UIComponent optionsBar = GameObject.Find("OptionsBar").GetComponent<UIComponent>();
            absolutePosition = optionsBar.absolutePosition - new Vector3(PanelWidth + Margin + 47f + ((ButtonSize + ButtonSpacing) * 3f), 0f);
        }

        /// <summary>
        /// Refreshes button states.
        /// </summary>
        internal void Refresh()
        {
            // Suppress events while changing state.
            _ignoreEvents = true;
            _snappingButton.activeStateIndex = PropToolPatches.SnappingEnabled ? 1 : 0;
            _anarchyButton.activeStateIndex = PropToolPatches.AnarchyEnabled ? 1 : 0;
            _ignoreEvents = false;

            // Set button tooltips.
            UpdateTooltips();
        }

        /// <summary>
        /// Updates button tooltips.
        /// </summary>
        internal void UpdateTooltips()
        {
            // A lot of string manipluations, so use a StringBuilder.
            StringBuilder tooltipText = new StringBuilder();

            // Anarchy button tooltip.
            tooltipText.Append(Translations.Translate("ANARCHY_STATUS"));
            tooltipText.Append(' ');
            tooltipText.AppendLine(Translations.Translate(PropToolPatches.AnarchyEnabled ? "ON" : "OFF"));
            tooltipText.AppendLine(Translations.Translate("ANARCHY_TIP"));
            tooltipText.Append(Translations.Translate("KEY_ANARCHY"));
            tooltipText.Append(": ");
            tooltipText.Append(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.AnarchyKey.Encode()));
            _anarchyButton.tooltip = tooltipText.ToString();

            // Update anarchy button tooltip if open.
            if (_anarchyButton.tooltipBox is UILabel anarchyTipBox && anarchyTipBox.isVisible)
            {
                anarchyTipBox.text = _anarchyButton.tooltip;
            }

            // Snapping button tooltip.
            tooltipText.Length = 0;
            tooltipText.Append(Translations.Translate("SNAPPING_STATUS"));
            tooltipText.Append(' ');
            tooltipText.AppendLine(Translations.Translate(PropToolPatches.SnappingEnabled ? "ON" : "OFF"));
            tooltipText.AppendLine(Translations.Translate("SNAPPING_TIP"));
            tooltipText.Append(Translations.Translate("KEY_SNAPPING"));
            tooltipText.Append(": ");
            tooltipText.Append(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.SnappingKey.Encode()));
            _snappingButton.tooltip = tooltipText.ToString();

            // Update snapping button tooltip if open.
            if (_snappingButton.tooltipBox is UILabel snappingTipBox && snappingTipBox.isVisible)
            {
                snappingTipBox.text = _snappingButton.tooltip;
            }
        }

        /// <summary>
        /// Drags the panel when the right mouse button is held down.
        /// </summary>
        /// <param name="c">Calling component (ignored).</param>
        /// <param name="p">Mouse event parameter.</param>
        private void Drag(UIComponent c, UIMouseEventParameter p)
        {
            p.Use();

            // Check for right button press.
            if ((p.buttons & UIMouseButton.Right) != 0)
            {
                // Peform dragging actions if already dragging.
                if (_dragging)
                {
                    // Calculate correct position by raycast - this is from game's UIDragHandle.
                    // Raw mouse position doesn't align with the game's UI scaling.
                    Ray ray = p.ray;
                    Vector3 inNormal = GetUIView().uiCamera.transform.TransformDirection(Vector3.back);
                    new Plane(inNormal, _lastDragPosition).Raycast(ray, out float enter);
                    Vector3 currentPosition = (ray.origin + (ray.direction * enter)).Quantize(PixelsToUnits());
                    Vector3 vectorDelta = currentPosition - _lastDragPosition;
                    Vector3[] corners = GetUIView().GetCorners();
                    Vector3 newTransformPosition = (transform.position + vectorDelta).Quantize(PixelsToUnits());

                    // Calculate panel bounds for screen constraint.
                    Vector3 upperLeft = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
                    Vector3 bottomRight = upperLeft + new Vector3(size.x, 0f - size.y);
                    upperLeft *= PixelsToUnits();
                    bottomRight *= PixelsToUnits();

                    // Constrain to screen.
                    if (newTransformPosition.x + upperLeft.x < corners[0].x)
                    {
                        newTransformPosition.x = corners[0].x - upperLeft.x;
                    }

                    if (newTransformPosition.x + bottomRight.x > corners[1].x)
                    {
                        newTransformPosition.x = corners[1].x - bottomRight.x;
                    }

                    if (newTransformPosition.y + upperLeft.y > corners[0].y)
                    {
                        newTransformPosition.y = corners[0].y - upperLeft.y;
                    }

                    if (newTransformPosition.y + bottomRight.y < corners[2].y)
                    {
                        newTransformPosition.y = corners[2].y - bottomRight.y;
                    }

                    // Apply calculated position.
                    transform.position = newTransformPosition;
                    _lastDragPosition = currentPosition;
                }
                else
                {
                    // Not already dragging, but dragging has started - commence.
                    _dragging = true;

                    // Calculate and record initial position.
                    Plane plane = new Plane(transform.TransformDirection(Vector3.back), this.transform.position);
                    Ray ray = p.ray;
                    plane.Raycast(ray, out float enter);
                    _lastDragPosition = ray.origin + (ray.direction * enter);
                }
            }
            else if (_dragging)
            {
                // We were dragging, but the mouse button is no longer held down - stop dragging.
                _dragging = false;

                // Record new position.
                StandalonePanelManager<StatusPanel>.LastSavedXPosition = absolutePosition.x;
                StandalonePanelManager<StatusPanel>.LastSavedYPosition = absolutePosition.y;
                ModSettings.Save();
            }
        }

        /// <summary>
        /// Adds a multi-state toggle button to the specified UIComponent.
        /// </summary>
        /// <param name="parent">Parent UIComponent.</param>
        /// <param name="name">Button name.</param>
        /// <param name="atlas">Button atlas.</param>
        /// <param name="disabledSprite">Foreground sprite for 'disabled' state..</param>
        /// <param name="enabledSprite">Foreground sprite for 'enabled' state.</param>
        /// <returns>New UIMultiStateButton.</returns>
        private UIMultiStateButton AddToggleButton(UIComponent parent, string name, UITextureAtlas atlas, string disabledSprite, string enabledSprite)
        {
            // Create button.
            UIMultiStateButton newButton = parent.AddUIComponent<UIMultiStateButton>();
            newButton.name = name;
            newButton.atlas = atlas;

            // Get sprite sets.
            UIMultiStateButton.SpriteSetState fgSpriteSetState = newButton.foregroundSprites;
            UIMultiStateButton.SpriteSetState bgSpriteSetState = newButton.backgroundSprites;

            // State 0 background.
            UIMultiStateButton.SpriteSet bgSpriteSetZero = bgSpriteSetState[0];
            if (s_transparentUI)
            {
                bgSpriteSetZero.hovered = "TransparentBaseHovered";
                bgSpriteSetZero.pressed = "TransparentBaseFocused";
            }
            else
            {
                bgSpriteSetZero.normal = "OptionBase";
                bgSpriteSetZero.focused = "OptionBase";
                bgSpriteSetZero.hovered = "OptionBaseHovered";
                bgSpriteSetZero.pressed = "OptionBasePressed";
                bgSpriteSetZero.disabled = "OptionBase";
            }

            // State 0 foreground.
            UIMultiStateButton.SpriteSet fgSpriteSetZero = fgSpriteSetState[0];
            fgSpriteSetZero.normal = disabledSprite;
            fgSpriteSetZero.focused = disabledSprite;
            fgSpriteSetZero.hovered = disabledSprite;
            fgSpriteSetZero.pressed = disabledSprite;
            fgSpriteSetZero.disabled = disabledSprite;

            // Add state 1.
            fgSpriteSetState.AddState();
            bgSpriteSetState.AddState();

            // State 1 background.
            UIMultiStateButton.SpriteSet bgSpriteSetOne = bgSpriteSetState[1];
            if (s_transparentUI)
            {
                bgSpriteSetOne.normal = "TransparentBaseFocused";
                bgSpriteSetOne.focused = "TransparentBaseFocused";
                bgSpriteSetOne.hovered = "TransparentBaseHovered";
            }
            else
            {
                bgSpriteSetOne.normal = "OptionBaseFocused";
                bgSpriteSetOne.focused = "OptionBaseFocused";
                bgSpriteSetOne.hovered = "OptionBaseHovered";
                bgSpriteSetOne.pressed = "OptionBasePressed";
                bgSpriteSetOne.disabled = "OptionBase";
            }

            // State 1 foreground.
            UIMultiStateButton.SpriteSet fgSpriteSetOne = fgSpriteSetState[1];
            fgSpriteSetOne.normal = enabledSprite;
            fgSpriteSetOne.focused = enabledSprite;
            fgSpriteSetOne.hovered = enabledSprite;
            fgSpriteSetOne.pressed = enabledSprite;
            fgSpriteSetOne.disabled = enabledSprite;

            // Set initial state.
            newButton.state = UIMultiStateButton.ButtonState.Normal;
            newButton.activeStateIndex = 0;

            // Size and appearance.
            newButton.autoSize = false;
            newButton.width = ButtonSize;
            newButton.height = ButtonSize;
            newButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            newButton.spritePadding = new RectOffset(0, 0, 0, 0);
            newButton.playAudioEvents = true;

            // Enforce defaults.
            newButton.canFocus = false;
            newButton.enabled = true;
            newButton.isInteractive = true;
            newButton.isVisible = true;

            return newButton;
        }
    }
}
