using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using ELabel = ElmSharp.Label;
using ESize = ElmSharp.Size;
using Color = Tizen.UIExtensions.Common.Color;

namespace Tizen.UIExtensions.ElmSharp
{
    /// <summary>
    /// The Label class extends <c>ElmSharp.Label</c> to be better suited for Xamarin renderers.
    /// Mainly the formatted text support.
    /// </summary>
    public class Label : ELabel, ITextable, IMeasurable, IBatchable
    {
        /// <summary>
        /// The _span holds the content of the label.
        /// </summary>
        readonly Span _span = new Span();
        FormattedString? _formattedText;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tizen.UIExtensions.ElmSharp.Label"/> class.
        /// </summary>
        /// <param name="parent">Parent evas object.</param>
        public Label(EvasObject parent) : base(parent)
        {
        }

        /// <summary>
        /// Get or sets the formatted text.
        /// </summary>
        /// <remarks>Setting <c>FormattedText</c> changes the value of the <c>Text</c> property.</remarks>
        /// <value>The formatted text.</value>
        public FormattedString? FormattedText
        {
            get
            {
                return _formattedText;
            }

            set
            {
                if (value != _formattedText)
                {
                    _formattedText = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <remarks>Setting <c>Text</c> overwrites the value of the <c>FormattedText</c> property too.</remarks>
        /// <value>The content of the label.</value>
        public override string Text
        {
            get
            {
                return _span.Text;
            }

            set
            {
                if (value != _span.Text)
                {
                    _span.Text = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the formatted text.
        /// </summary>
        /// <value>The color of the text.</value>
        public Color TextColor
        {
            get
            {
                return _span.ForegroundColor;
            }

            set
            {
                if (!_span.ForegroundColor.Equals(value))
                {
                    _span.ForegroundColor = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color for the text.
        /// </summary>
        /// <value>The color of the label's background.</value>
        public Color TextBackgroundColor
        {
            get
            {
                return _span.BackgroundColor;
            }

            set
            {
                if (!_span.BackgroundColor.Equals(value))
                {
                    _span.BackgroundColor = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font family for the text.
        /// </summary>
        /// <value>The font family.</value>
        public string FontFamily
        {
            get
            {
                return _span.FontFamily;
            }

            set
            {
                if (value != _span.FontFamily)
                {
                    _span.FontFamily = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font attributes.
        /// </summary>
        /// <value>The font attributes.</value>
        public FontAttributes FontAttributes
        {
            get
            {
                return _span.FontAttributes;
            }

            set
            {
                if (value != _span.FontAttributes)
                {
                    _span.FontAttributes = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font size for the text.
        /// </summary>
        /// <value>The size of the font.</value>
        public double FontSize
        {
            get
            {
                return _span.FontSize;
            }

            set
            {
                if (value != _span.FontSize)
                {
                    _span.FontSize = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the line height.
        /// </summary>
        public double LineHeight
        {
            get
            {
                return _span.LineHeight;
            }
            set
            {
                if (value != _span.LineHeight)
                {
                    _span.LineHeight = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the line wrap option.
        /// </summary>
        /// <value>The line break mode.</value>
        public LineBreakMode LineBreakMode
        {
            get
            {
                return _span.LineBreakMode;
            }

            set
            {
                if (value != _span.LineBreakMode)
                {
                    _span.LineBreakMode = value;
                    switch (value)
                    {
                        case LineBreakMode.NoWrap:
                            LineWrapType = WrapType.None;
                            IsEllipsis = false;
                            break;
                        case LineBreakMode.CharacterWrap:
                            LineWrapType = WrapType.Char;
                            IsEllipsis = false;
                            break;
                        case LineBreakMode.WordWrap:
                            LineWrapType = WrapType.Word;
                            IsEllipsis = false;
                            break;
                        case LineBreakMode.MixedWrap:
                            LineWrapType = WrapType.Mixed;
                            IsEllipsis = false;
                            break;
                        default:
                            LineWrapType = WrapType.None;
                            IsEllipsis = true;
                            break;
                    }
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal text alignment.
        /// </summary>
        /// <value>The horizontal text alignment.</value>
        public TextAlignment HorizontalTextAlignment
        {
            get
            {
                return _span.HorizontalTextAlignment;
            }

            set
            {
                if (value != _span.HorizontalTextAlignment)
                {
                    _span.HorizontalTextAlignment = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertical text alignment.
        /// </summary>
        /// <value>The vertical text alignment.</value>
        public TextAlignment VerticalTextAlignment
        {
            // README: It only work on Tizen 4.0
            get
            {
                double valign = this.GetVerticalTextAlignment();
                if (valign == 0.0)
                {
                    return TextAlignment.Start;
                }
                else if (valign == 0.5)
                {
                    return TextAlignment.Center;
                }
                else if (valign == 1.0)
                {
                    return TextAlignment.End;
                }
                else
                {
                    return TextAlignment.Auto;
                }
            }
            set
            {
                double valign = 0;
                switch (value)
                {
                    case TextAlignment.Auto:
                        valign = -1;
                        break;
                    case TextAlignment.None:
                    case TextAlignment.Start:
                        valign = 0;
                        break;
                    case TextAlignment.Center:
                        valign = 0.5;
                        break;
                    case TextAlignment.End:
                        valign = 1.0;
                        break;
                }
                this.SetVerticalTextAlignment(valign);
            }
        }

        /// <summary>
        /// Gets or sets the TextDecorations applied to Text.
        /// </summary>
        public TextDecorations TextDecorations
        {
            get
            {
                return _span.TextDecorations;
            }
            set
            {
                if (value != _span.TextDecorations)
                {
                    _span.TextDecorations = value;
                    ApplyTextAndStyle();
                }
            }
        }

        /// <summary>
        /// Implements <see cref="Xamarin.Forms.Platform.Tizen.Native.IMeasurable"/> to provide a desired size of the label.
        /// </summary>
        /// <param name="availableWidth">Available width.</param>
        /// <param name="availableHeight">Available height.</param>
        /// <returns>Size of the control that fits the available area.</returns>
        public Common.Size Measure(double availableWidth, double availableHeight)
        {
            var size = Geometry;

            Resize((int)availableWidth, size.Height);

            var formattedSize = this.GetTextBlockFormattedSize();
            Resize(size.Width, size.Height);

            // Set bottom padding for lower case letters that have segments below the bottom line of text (g, j, p, q, y).
            var verticalPadding = (int)Math.Ceiling(0.05 * FontSize);
            formattedSize.Height += verticalPadding;

            // This is the EFL team's guide.
            // For wrap to work properly, the label must be 1 pixel larger than the size of the formatted text.
            formattedSize.Width += 1;

            return formattedSize.ToCommon();
        }

        void IBatchable.OnBatchCommitted()
        {
            ApplyTextAndStyle();
        }

        void ApplyTextAndStyle()
        {
            if (!this.IsBatched())
            {
                SetInternalTextAndStyle(GetDecoratedText(), _span.GetStyle());
            }
        }

        string GetDecoratedText()
        {
            return _formattedText?.ToMarkupText() ?? _span.GetDecoratedText();
        }


        void SetInternalTextAndStyle(string formattedText, string textStyle)
        {
            base.Text = formattedText;
            TextStyle = textStyle;
        }
    }
}
