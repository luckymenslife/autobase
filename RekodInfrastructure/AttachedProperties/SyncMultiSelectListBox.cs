using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Rekod.AttachedProperties
{
    public static class SyncMultiSelectListBox
    {
        #region SelectedItems

        /// <summary>
        /// SelectedItems Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(SyncMultiSelectListBox),
                new FrameworkPropertyMetadata((IList)null,
                    new PropertyChangedCallback(OnSelectedItemsChanged)));

        /// <summary>
        /// Gets the SelectedItems property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static IList GetSelectedItems(DependencyObject d)
        {
            return (IList)d.GetValue(SelectedItemsProperty);
        }

        /// <summary>
        /// Sets the SelectedItems property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetSelectedItems(DependencyObject d, IList value)
        {
            d.SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        /// Handles changes to the SelectedItems property.
        /// </summary>
        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as ListBox;
            if (selector == null)
                return;
            ReSetSelectedItems(selector);
            selector.SelectionChanged += delegate
            {
                ReSetSelectedItems(selector);
            };
        }
        #endregion

        private static void ReSetSelectedItems(ListBox selector)
        {
            IList selectedItems = GetSelectedItems(selector);
            selectedItems.Clear();
            if (selector.SelectedItems != null)
            {
                foreach (var item in selector.SelectedItems)
                    selectedItems.Add(item);
            }
        }
    }
}
