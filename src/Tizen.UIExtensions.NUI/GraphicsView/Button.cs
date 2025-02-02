﻿using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.Common.GraphicsView;

namespace Tizen.UIExtensions.NUI.GraphicsView
{
    /// <summary>
    /// A button View that reacts to touch events.
    /// </summary>
    public class Button : GraphicsView<ButtonDrawable>, IButton
    {
        /// <summary>
        /// Initializes a new instance of the Button class.
        /// </summary>
        public Button()
        {
            Drawable = new ButtonDrawable(this);
        }

        /// <summary>
        /// Occurs when the Button is clicked.
        /// </summary>
        public event EventHandler? Clicked;

        /// <summary>
        /// Occurs when the Button is pressed.
        /// </summary>
        public event EventHandler? Pressed;

        /// <summary>
        /// Occurs when the Button is released.
        /// </summary>
        public event EventHandler? Released;

        public bool IsPressed
        {
            get => GetProperty<bool>(nameof(IsPressed));
            set => SetProperty(nameof(IsPressed), value);
        }

        /// <summary>
        /// Gets or sets the Text displayed as the content of the button.
        /// </summary>
        public string Text
        {
            get => GetProperty<string>(nameof(Text));
            set => SetProperty(nameof(Text), value);
        }

        /// <summary>
        /// Gets or sets the Color for the text of the button.
        /// </summary>
        public Color TextColor
        {
            get => GetProperty<Color>(nameof(TextColor));
            set => SetProperty(nameof(TextColor), value);
        }

        /// <summary>
        /// Gets or sets the color which will fill the background of a Button
        /// </summary>
        public new Color BackgroundColor
        {
            get => GetProperty<Color>(nameof(BackgroundColor));
            set => SetProperty(nameof(BackgroundColor), value);
        }
        Color IButton.BackgroundColor => BackgroundColor;

        /// <summary>
        /// Gets or sets the corner radius for the button, in device-independent units.
        /// </summary>
        public new double CornerRadius
        {
            get => GetProperty<double>(nameof(CornerRadius));
            set => SetProperty(nameof(CornerRadius), value);
        }
        double IButton.CornerRadius => CornerRadius;

        protected override bool OnTouch(object source, TouchEventArgs e)
        {
            if (!IsEnabled)
                return false;

            var state = e.Touch.GetState(0);

            if (state == Tizen.NUI.PointStateType.Down)
            {
                IsPressed = true;
                Pressed?.Invoke(this, EventArgs.Empty);
                Clicked?.Invoke(this, EventArgs.Empty);
            }
            else if (state == Tizen.NUI.PointStateType.Up)
            {
                IsPressed = false;
                Released?.Invoke(this, EventArgs.Empty);
            }
            return base.OnTouch(source, e);
        }
    }
}
