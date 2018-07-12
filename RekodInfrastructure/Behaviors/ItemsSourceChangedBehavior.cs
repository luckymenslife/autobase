using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;

namespace Rekod.Behaviors
{
    /// <summary>
    /// ItemsSourceChanged Behavior uses an Attached Dependency Property
    /// to add and raise a rotued event whenever an ItemsControl's ItemsSource property
    /// changes. Also looks for INotifyCollectionChanged on the ItemsSource and raises the
    /// event on every collection changed event
    /// </summary>
    public static class ItemsSourceChangedBehavior
    {
        #region ItemsSourceChanged Property

        private static Dictionary<String, object> _lastSelectedValues = new Dictionary<string, object>(); 

        /// <summary>
        /// ItemsSourceChanged Attached Dependency Property with Callback method
        /// </summary>
        public static readonly DependencyProperty SpyItemsSourceChangedProperty =
                                                  DependencyProperty.RegisterAttached("SpyItemsSourceChanged",
                                                  typeof(bool), typeof(ItemsSourceChangedBehavior),
                                                  new FrameworkPropertyMetadata(false, OnSpyItemsSourceChangedChanged));

        /// <summary>
        /// Static Get method allowing easy Xaml usage and to simplify the
        /// GetValue process
        /// </summary>
        /// <param name="obj">The dependency obj.</param>
        /// <returns>True or False</returns>
        public static bool GetSpyItemsSourceChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(SpyItemsSourceChangedProperty);
        }

        /// <summary>
        /// Static Set method allowing easy Xaml usage and to simplify the
        /// Setvalue process
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetSpyItemsSourceChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(SpyItemsSourceChangedProperty, value);
        }

        /// <summary>
        /// Dependency Property Changed Call Back method. This will be called anytime
        /// the ItemsSourceChangedProperty value changes on a Dependency Object
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnSpyItemsSourceChangedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = obj as ItemsControl;
            itemsControl.Loaded += new RoutedEventHandler(itemsControl_Loaded);
            
            if (itemsControl == null)
                return;

            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;

            if (!oldValue && newValue) // If changed from false to true
            {
                // Create a binding to the ItemsSourceProperty on the ItemsControl
                Binding b = new Binding
                                {
                                    Source = itemsControl,
                                    Path = new PropertyPath(ItemsControl.ItemsSourceProperty)
                                };

                // Since the ItemsSourceListenerProperty is now bound to the
                // ItemsSourceProperty on the ItemsControl, whenever the 
                // ItemsSourceProperty changes the ItemsSourceListenerProperty
                // callback method will execute
                itemsControl.SetBinding(ItemsSourceListenerProperty, b);
            }
            else if (oldValue && !newValue) // If changed from true to false
            {
                // Clear Binding on the ItemsSourceListenerProperty
                BindingOperations.ClearBinding(itemsControl, ItemsSourceListenerProperty);
            }
        }

        /// <summary>
        /// При загрузке контрола пробуем установить выбранный элемент
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void itemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            Selector selector = sender as Selector;
            if (selector != null)
            {
                if(!String.IsNullOrEmpty(selector.Name) && _lastSelectedValues.ContainsKey(selector.Name))
                {
                    selector.SelectedItem = _lastSelectedValues[selector.Name]; 
                }
                selector.SelectionChanged += new SelectionChangedEventHandler(selector_SelectionChanged);
            }
        }
        /// <summary>
        /// Отслеживаем изменение выбранного элемента, чтобы в следующий раз установить при загрузке контрола с заданным именем
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selector selector = sender as Selector;
            if (selector != null)
            {
                if (!String.IsNullOrEmpty(selector.Name))
                {
                    if (_lastSelectedValues.ContainsKey(selector.Name))
                    {
                        _lastSelectedValues[selector.Name] = selector.SelectedItem;
                    }
                    else
                    {
                        _lastSelectedValues.Add(selector.Name, selector.SelectedItem); 
                    }
                }
            }
        }
        #endregion

        #region Items Source Listener Property

        /// <summary>
        /// The ItemsSourceListener Attached Dependency Property is a private property
        /// the ItemsSourceChangedBehavior will use silently to bind to the ItemsControl
        /// ItemsSourceProperty.
        /// Once bound, the callback method will execute anytime the ItemsSource property changes
        /// </summary>
        private static readonly DependencyProperty ItemsSourceListenerProperty =
            DependencyProperty.RegisterAttached("ItemsSourceListener",
                                                typeof(object), typeof(ItemsSourceChangedBehavior),
                                                new FrameworkPropertyMetadata(null, OnItemsSourceListenerChanged));


        /// <summary>
        /// Dependency Property Changed Call Back method. This will be called anytime
        /// the ItemsSourceListenerProperty value changes on a Dependency Object
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnItemsSourceListenerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = obj as ItemsControl;

            if (itemsControl == null)
                return;

            INotifyCollectionChanged collection = e.NewValue as INotifyCollectionChanged;

            if (collection != null)
            {
                collection.CollectionChanged +=
                delegate(object coll, NotifyCollectionChangedEventArgs changedArgs)
                {
                    if (changedArgs.Action == NotifyCollectionChangedAction.Add)
                    {
                        Selector selector = itemsControl as Selector;
                        if (selector != null)
                        {
                            //DependencyObject depObj = itemsControl.ItemContainerGenerator.ContainerFromItem(changedArgs.NewItems[0]);
                            selector.SelectedItem = changedArgs.NewItems[0]; 
                        }                        
                    }
                    itemsControl.RaiseEvent(new RoutedEventArgs(ItemsSourceChangedEvent));
                };

            }

            if (GetSpyItemsSourceChanged(itemsControl))
                itemsControl.RaiseEvent(new RoutedEventArgs(ItemsSourceChangedEvent));
        }

        static void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Items Source Changed Event
        /// <summary>
        /// Routed Event to raise whenever the ItemsSource changes on an ItemsControl
        /// </summary>
        public static readonly RoutedEvent ItemsSourceChangedEvent =
            EventManager.RegisterRoutedEvent("ItemsSourceChanged",
                                                 RoutingStrategy.Bubble,
                                                 typeof(RoutedEventHandler),
                                                 typeof(ItemsSourceChangedBehavior));

        public static void AddItemsSourceChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.AddHandler(ItemsSourceChangedEvent, handler);
            }
        }

        public static void RemoveItemsSourceChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(ItemsSourceChangedEvent, handler);
            }
        }
        #endregion
    }
}