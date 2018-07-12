using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Shapes;

namespace Rekod.AttachedProperties
{
    /// <summary>
    /// Используется для создания функции перемещения
    /// Особенности:
    ///     Нужно указывать этот параметр на принимающей стороне, и указывать ссылку на отправляющий элемент
    /// </summary>
    public class DragAndDrop
    {
        #region Поля
        private static ListBox DragSource;
        public static readonly DependencyProperty MoveEnableProperty;
        public static readonly DependencyProperty DragAndDropProperty;

        #endregion // Поля

        #region Статический конструктор
        static DragAndDrop()
        {
            MoveEnableProperty =
        DependencyProperty.RegisterAttached("MoveEnable", typeof(bool), typeof(DragAndDrop), new PropertyMetadata(OnMoveEnabledChanged));
            DragAndDropProperty =
        DependencyProperty.RegisterAttached("DragAndDrop", typeof(bool), typeof(DragAndDrop), new PropertyMetadata(OnDragAndDropEnabledChanged));

        }
        #endregion // Статический конструктор

        #region MoveEnabled
        public static bool GetMoveEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(MoveEnableProperty);
        }
        public static void SetMoveEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(MoveEnableProperty, value);
        }
        public static void OnMoveEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ListBox listbox = obj as ListBox;
            listbox.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(listbox_PreviewMouseLeftButtonDown);
            listbox.Drop -= new DragEventHandler(listbox_Move);
            listbox.GiveFeedback -= new GiveFeedbackEventHandler(listbox_GiveFeedback);
            if ((bool)args.NewValue == true)
            {
                listbox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(listbox_PreviewMouseLeftButtonDown);
                listbox.AllowDrop = true;
                listbox.Drop += new DragEventHandler(listbox_Move);
                listbox.GiveFeedback += new GiveFeedbackEventHandler(listbox_GiveFeedback);
            }
        }
        #endregion // MoveEnabled

        #region DragAndDrop
        public static bool GetDragAndDrop(DependencyObject obj)
        {
            return (bool)obj.GetValue(DragAndDropProperty);
        }
        public static void SetDragAndDrop(DependencyObject obj, bool value)
        {
            obj.SetValue(DragAndDropProperty, value);
        }
        public static void OnDragAndDropEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ListBox listbox = obj as ListBox;
            listbox.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(listbox_PreviewMouseLeftButtonDown);
            listbox.Drop -= new DragEventHandler(listbox_Drop);
            listbox.GiveFeedback -= new GiveFeedbackEventHandler(listbox_GiveFeedback);
            if ((bool)args.NewValue == true)
            {
                listbox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(listbox_PreviewMouseLeftButtonDown);
                listbox.AllowDrop = true;
                listbox.Drop += new DragEventHandler(listbox_Drop);
                listbox.GiveFeedback += new GiveFeedbackEventHandler(listbox_GiveFeedback);
            }
        }
        #endregion // DragAndDrop

        #region Обработчики
        private static Cursor _allOpsCursor = null;
        private static Rectangle _draggedOne = new Rectangle(); 
        static void listbox_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (_allOpsCursor == null)
            {
                _allOpsCursor = CursorHelper.CreateCursor(_draggedOne, 5, 5);
            }
            Mouse.SetCursor(_allOpsCursor);
            e.UseDefaultCursors = false;
            e.Handled = true;
        }
        static void listbox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _allOpsCursor = null; 

            DragSource = (ListBox)sender;
            object data = (object)GetObjectDataFromPoint(DragSource, e.GetPosition(DragSource));
            if (data != null)
            {
                DragSource.SelectedItem = data;
                var d = DragSource.ItemContainerGenerator.ContainerFromItem(DragSource.SelectedItem) as FrameworkElement;

                _draggedOne.Fill = new VisualBrush(d as Visual);
                _draggedOne.Width = d.ActualWidth;
                _draggedOne.Height = d.ActualHeight;

                DragDrop.DoDragDrop(DragSource, d, DragDropEffects.Move);
            }
        }
        static void listbox_Drop(object sender, DragEventArgs e)
        {
            // Получение контролов участвующих в перемещении
            var inElement = sender as ListBox;
            var outElement = DragSource;

            if (inElement == outElement)
                return;

            // Получаем доступ к коллециям
            var outList = outElement.ItemsSource as IList;
            var inList = inElement.ItemsSource as IList;
            if (inList == null || outList == null)
                return;


            if (inList == outList)
                return;

            // Получаем доступ к объектам
            object outItem = outElement.SelectedItem;
            object inItem = GetObjectDataFromPoint(inElement, e.GetPosition(inElement));

            if (outItem == null)
                return;

            // Проверяем соответствие типов 
            var inType = inList.GetType().GetGenericArguments()[0];
            var outType = outList.GetType().GetGenericArguments()[0];
            if (inType != outType)
                return;

            // Задаем индекс для вставки
            int inIndex;
            if (GetMoveEnable(inElement))
            {
                inIndex = (inItem != null)
                                ? inList.IndexOf(inItem)
                                : inList.Count;
            }
            else
                inIndex = 0;
            outList.Remove(outItem);
            inList.Insert(inIndex, outItem);

            inElement.SelectedItem = outItem;
        }
        static void listbox_Move(object sender, DragEventArgs e)
        {
            // Получение контролов участвующих в перемещении
            var inElement = sender as ListBox;
            var outElement = DragSource;

            // Получаем доступ к коллециям
            var outList = outElement.ItemsSource as IList;
            var inList = inElement.ItemsSource as IList;
            if (inList == null || outList == null)
                return;

            // Получаем доступ к объектам
            object outItem = outElement.SelectedItem;
            object inItem = GetObjectDataFromPoint(inElement, e.GetPosition(inElement));

            if (outItem == null)
                return;


            if (inList != outList)
                return;


            // Если это тот же объект
            if (inItem == outItem)
                return;

            // Задаем индекс для вставки
            int inIndex = (inItem != null)
                        ? inList.IndexOf(inItem)
                        : (inList.Count - 1);


            outList.Remove(outItem);
            inList.Insert(inIndex, outItem);

            inElement.SelectedItem = outItem;
        }
        #endregion // Обработчики

        #region Методы
        /// <summary>
        /// Получить элемент по позиции мышки
        /// </summary>
        /// <param name="source"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static object GetObjectDataFromPoint(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    if (element == source)
                        return null;
                }
                if (data != DependencyProperty.UnsetValue)
                    return data;
            }

            return null;
        }

        #endregion // Методы
    }
}