using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using Interfaces;
using RESTLib.Enums;
using RESTLib.Model.REST;
using RESTLib.Model.REST.LayerStyle;
using RESTLib.Model.WFS;
using sscSync.ViewModel;

namespace sscSync.Controller
{
    public class MapAdmin
    {
        private User _user = null;
        private SSCData sscData;
        private PgData pgData;

        private List<Layer> _sscLayers = new List<Layer>();
        private List<Group> _sscGroups = new List<Group>();


        private IMainApp _app;
        private IWorkClass _work;

        public SSCData SscData
        {
            get { return sscData; }
        }

        public PgData PgData
        {
            get { return pgData; }
        }

        public List<Layer> SscLayers
        {
            get { return _sscLayers; }
        }

        public List<Group> SscGroups
        {
            get { return _sscGroups; }
        }

        public MapAdmin(Interfaces.sscUserInfo userInfo, IMainApp app, IWorkClass work, ProxySettings proxySettings = null)
        {
            _app = app;
            _work = work;
            try
            {
                if (proxySettings != null)
                {                    
                    RESTLib.Settings.ProxyEnable(proxySettings.Server, proxySettings.Login, proxySettings.Password);
                }
                _user = new User(userInfo.Login, userInfo.Password, userInfo.Server);
                sscData = new SSCData(_user);
                pgData = new PgData(app);
                this.ReloadInfo();
            }
            catch (Exception e)
            {
                MessageBox.Show("При подключении к серверу произошла ошибка:" + Environment.NewLine +
                    e.Message + Environment.NewLine +
                    "Проверьте подключение к сети.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ReloadInfo()
        {
            string key = _work.OpenForm.ProcOpen("ReloadInfo");
            try
            {
                _sscLayers = sscData.GetSSCLayers(_work.OpenForm);
                _sscGroups = sscData.GetGroups();
            }
            finally
            {
                _work.OpenForm.ProcClose(key);
            }
        }
        /// <summary>
        /// Перезагрузить таблицу в SSC
        /// </summary>
        /// <param name="layer">Слой для перезагрузки</param>
        public void ReloadLayer(Layer layer)
        {
            if (layer == null)
                throw new Exception("Не указан слой для регистрации");

            string key = _work.OpenForm.ProcOpen();
            try
            {
                SscData.ReloadTable(layer);
            }
            finally
            {
                _work.OpenForm.ProcClose(key);
            }
        }

        /// <summary>
        /// Перезагрузка стиля
        /// </summary>
        /// <param name="layer">Слой</param>
        public void ReloadStyle(Layer layer)
        {
            if (layer == null)
                throw new Exception("Не указан слой для регистрации");
            
            string key = _work.OpenForm.ProcOpen("");

            try
            {
                SscData.ReloadStyle(layer);
            }
            finally
            {
                _work.OpenForm.ProcClose(key);
            }
        }


        /// <summary>
        /// Публикация слоя на SSC
        /// </summary>
        /// <param name="table">Информация о таблице</param>
        /// <returns>Результат операции</returns>
        public bool RegisterTable(tablesInfo table, Group group)
        {
            if (table == null)
                throw new Exception("Не указана таблица для регистрации");

            if (group == null)
                throw new Exception("Невозможно зарегистрировать таблицу без указанной группы");

            string key = _work.OpenForm.ProcOpen();

            try
            {
                return SscData.RegisterTable(
                    table,
                    SscData.getDefaultStyle(table.TypeGeom),
                    group);
            }
            finally
            {
                _work.OpenForm.ProcClose(key);
            }
        }

        /// <summary>
        /// Удалить слой из SSC
        /// </summary>
        /// <param name="layer">Выбранный слой для удаления</param>
        /// <param name="deleteFromDB">Нужно ли удалить соответствующию таблицу из БД</param>
        /// <returns>Результат операции</returns>
        public bool DeleteLayer(Layer layer, bool deleteFromDB)
        {
            return this.sscData.DeleteLayer(layer, deleteFromDB);
        }

        /// <summary>
        /// Имеет ли текущий пользователь право на изменение данного слоя
        /// </summary>
        /// <param name="layer">Выбранный слой</param>
        /// <returns>True - если имеет право, False - иначе</returns>
        public bool HasRight(Layer layer)
        {
            if (layer.edit == 't' || _user.IsAdminAllDepartaments)                // администратор всех ведомств
            {
                return true;
            }

            if (_user.Role == RESTUserRoles.DepartamentAdmin)
            {
                return SscData.IsInCurrentDepartment(layer.User_id);
            }

            return false;
        }
    }

    public class ProxySettings
    {
        public Uri Server { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
