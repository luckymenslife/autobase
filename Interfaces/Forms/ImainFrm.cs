using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Interfaces.Forms
{
    public interface ImainFrm
    {
        /// <summary> Дбавление кнопок в панель TbPlugins для плагинов
        /// </summary>
        /// <param name="swItem">Контрол для добавления в панель инструментов</param>
        void PanelTools(System.Windows.Controls.Control swItem);

        /// <summary> Удаление кнопок в панели TbPlugins для плагинов
        /// </summary>
        /// <param name="item"></param>
        void RemovePanelTools(System.Windows.Controls.Control swItem);

        /// <summary> Добавление меню в главную форму
        /// </summary>
        /// <param name="item"></param>
        void Menu(System.Windows.Controls.MenuItem item);

        /// <summary> Удаление меню в главной форме
        /// </summary>
        /// <param name="item"></param>
        void RemoveMenu(System.Windows.Controls.MenuItem item);
    }
}