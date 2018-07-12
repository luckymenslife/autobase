using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rekod.DataAccess.TableManager.View
{
    public class TextBlockService: DependencyObject
    {
        public static readonly DependencyProperty IsTextTrimmedProperty =
          DependencyProperty.RegisterAttached("IsTextTrimmed", typeof(bool), typeof(TextBlockService), new PropertyMetadata(false));
        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static bool GetIsTextTrimmed(TextBlock target)
        {
            return (bool)target.GetValue(IsTextTrimmedProperty);
        }
        private static void SetIsTextTrimmed(TextBlock target, bool trimmed)
        {
            target.SetValue(IsTextTrimmedProperty, trimmed);
        }
        private static bool CalculateIsTextTrimmed(TextBlock target)
        {
            if (!target.IsArrangeValid)
            {
                return GetIsTextTrimmed(target);
            }
            
            Typeface typeface = new Typeface(target.FontFamily, target.FontStyle, target.FontWeight, target.FontStretch);
            // Форматированный текст используется для измерения ширины содержащегося в своем контейнере
            FormattedText formText = new FormattedText(
                target.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                target.FlowDirection,
                typeface,
                target.FontSize,
                target.Foreground);
            formText.MaxTextWidth = target.ActualWidth;

            return (formText.Height > target.ActualHeight); 
        }
        static TextBlockService()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), FrameworkElement.SizeChangedEvent, 
                                                 new SizeChangedEventHandler(OnTextBlockSizeChanged), true); 
        }
        public static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (null == textBlock)
            { 
                return; 
            }
            if (TextTrimming.None == textBlock.TextTrimming)
            {
                SetIsTextTrimmed(textBlock, false);
            }
            else 
            {
                SetIsTextTrimmed(textBlock, CalculateIsTextTrimmed(textBlock)); 
            }
        }
    }
}