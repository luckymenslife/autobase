using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Interfaces.UserControls
{
    /// <summary> Интерфейс выводимого UserControl'a
    /// </summary>
    public interface IUserControlMain
    {
        /// <summary> Заголовок окна
        /// </summary>
        string Title { get; }

        Object ViewModel { get; }

        /// <summary> Событие на закрытие окна
        /// </summary>
        event EventHandler<eventCloseForm> CloseForm;

        /// <summary> Подключение UserControl'a на форму
        /// </summary>
        /// <returns></returns>
        UserControl GetUserControl();

        bool CancelOpen { get; }

        Size SizeWindow { get; }
    }

    /// <summary> Интерфейс выводимого UserControl'a 
    /// </summary>
    public interface IAttributeObject : IUserControlMain
    {
        UserControl GetUserControl(int idObject, bool isNew);
        UserControl GetUserControl(int idObject, bool isNew, string wkt);
    }

    /// <summary> Предоставляет данные для события закрытия окна 
    /// </summary>
    public class eventCloseForm : EventArgs
    {
        public bool IsSave;
        public eventCloseForm(bool IsSave)
        { this.IsSave = IsSave; }
    }

}
