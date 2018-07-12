using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfaces;
using NotificationPlugins.Enums;
using NotificationPlugins.Models;
using NotificationPlugins.ViewModels;
using NpgsqlTypes;

namespace NotificationPlugins.Infrastructure
{
    public class NotificationRepository
    {
        #region Поля
        private IMainApp _app;
        #endregion

        #region Конструктор
        public NotificationRepository(IMainApp app)
        {
            _app = app;
        }
        #endregion

        #region Закрытые Методы
        private string GenerateWhereSQL(ENotificationStatus status, NotificationPriorityM priority, NotificationTypeM type, string filterText)
        {
            string result = "";
            switch (status)
            {
                case ENotificationStatus.All:
                    //command.sql += " AND seen_datetime is null";
                    break;
                case ENotificationStatus.Unreads:
                    result += " AND seen_datetime is null";
                    break;
                case ENotificationStatus.Favorites:
                    result += " AND favorite = true";
                    break;
                default:
                    break;
            }
            if (priority != null && priority.Gid > 0)
            {
                result += " AND priority_type_gid = " + priority.Gid.ToString();
            }
            if (type != null && type.Gid > 0)
            {
                result += " AND type_gid = " + type.Gid.ToString();
            }
            if (!string.IsNullOrEmpty(filterText))
            {
                result += " AND (upper(subject) like '%" + filterText.ToUpper() + "%' OR upper(message) like '%" + filterText.ToUpper() + "%')";
            }
            return result;
        }
        #endregion

        #region Открытые Методы
        public int GetFavoriteCount()
        {
            int z = 0;
            using (ISQLCommand sqlCmd = _app.SqlWork())
            {
                sqlCmd.sql = "SELECT cnt_favorite FROM notification.notification_counter WHERE user_gid =  (SELECT notification.get_current_user_id())";
                sqlCmd.ExecuteReader();
                return sqlCmd.CanRead() ? sqlCmd.GetValue<int>("cnt_favorite") : 0;
            }
        }
        public int GetUnreadCount()
        {
            int z = 0;
            using (ISQLCommand sqlCmd = _app.SqlWork(true))
            {
                sqlCmd.sql = "SELECT cnt_unread FROM notification.notification_counter WHERE user_gid =  (SELECT notification.get_current_user_id())";
                sqlCmd.ExecuteReader();
                return sqlCmd.CanRead() ? sqlCmd.GetValue<int>("cnt_unread") : 0;
            }
        }
        public List<NotificationPriorityM> GetPriorityTypes()
        {
            List<NotificationPriorityM> result = new List<NotificationPriorityM>();
            using (ISQLCommand sqlCmd = _app.SqlWork())
            {
                sqlCmd.sql = "SELECT gid, typename, weight, knowncolor FROM notification.notification_priority_types ORDER BY typename";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    result.Add(new NotificationPriorityM(sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<int>("weight"),
                        sqlCmd.GetValue<string>("typename"), (System.Drawing.KnownColor)sqlCmd.GetValue<int>("knowncolor")));
                }
                return result;
            }
        }
        public List<NotificationM> GetNotifications(
            ENotificationStatus status,
            NotificationPriorityM priority,
            NotificationTypeM type,
            string filterText,
            int? countInPage = 100,
            int? pageNum = null)
        {
            List<NotificationM> outList = new List<NotificationM>();
            using (ISQLCommand sqlCmd = _app.SqlWork())
            {
                sqlCmd.sql = @"SELECT gid, type_gid, type_name, priority_type_gid, priority_name, weight, subject, 
message, created, seen_datetime, table_gid, row_gid, alias_text, user_gid, favorite 
FROM notification.view_user_notifications";
                sqlCmd.sql += " WHERE 1=1";
                string where = GenerateWhereSQL(status, priority, type, filterText);
                sqlCmd.sql += where;
                sqlCmd.sql += " ORDER BY created DESC";
                if (pageNum != null)
                {
                    int begin = (pageNum.Value - 1) * 100;
                    countInPage = countInPage.HasValue ? countInPage.Value : 100;
                    sqlCmd.sql += string.Format(" LIMIT {0} OFFSET {1};", countInPage, begin);
                }
                try
                {
                    sqlCmd.ExecuteReader();
                    while (sqlCmd.CanRead())
                    {
                        outList.Add(new NotificationM(
                            sqlCmd.GetValue<int>("gid"),
                            sqlCmd.GetValue<int>("type_gid"),
                            sqlCmd.GetValue<int>("priority_type_gid"),
                            sqlCmd.GetValue<int>("weight"),
                            sqlCmd.GetValue<string>("subject"),
                            sqlCmd.GetValue<string>("message"),
                            sqlCmd.GetValue<DateTime?>("created"),
                            sqlCmd.GetValue<DateTime?>("seen_datetime"),
                            sqlCmd.GetValue<int>("user_gid"),
                            sqlCmd.GetValue<bool>("favorite"),
                            new RefObjM() { IdObj = sqlCmd.GetValue<int?>("row_gid"), IdTable = sqlCmd.GetValue<int?>("table_gid"), Name = sqlCmd.GetValue<string>("alias_text") }
                            ));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return outList;
        }
        public int GetCountNotifications(
            ENotificationStatus status,
            NotificationPriorityM priority,
            NotificationTypeM type,
            string filterText,
            int? countInPage = 100,
            int? pageNum = null)
        {
            using (ISQLCommand sqlCmd = _app.SqlWork())
            {
                sqlCmd.sql = @"SELECT count(*) as cnt FROM notification.view_user_notifications";
                sqlCmd.sql += " WHERE 1=1";
                string where = GenerateWhereSQL(status, priority, type, filterText);
                sqlCmd.sql += where;
                sqlCmd.sql += ";";
                try
                {
                    return sqlCmd.ExecuteScalar<int>();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public List<NotificationTypeM> GetNotificationTypes()
        {
            List<NotificationTypeM> result = new List<NotificationTypeM>();
            using (var sqlCmd = _app.SqlWork())
            {
                sqlCmd.sql = "SELECT gid, typename, system_type FROM notification.notification_types ORDER BY typename;";
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    result.Add(new NotificationTypeM()
                    {
                        Gid = sqlCmd.GetValue<int>("gid"),
                        Name = sqlCmd.GetValue<string>("typename"),
                        SystemType = sqlCmd.GetValue<bool>("system_type")
                    });
                }
            }
            return result;
        }
        public bool SetReadedNotification(NotificationM notification)
        {
            try
            {
                using (var sqlCmd = PluginInitialization._app.SqlWork(true))
                {
                    sqlCmd.sql = @"UPDATE notification.notification_users
   SET seen_datetime=now()
 WHERE notification_gid = @notification_gid AND user_gid = @user_gid;";
                    sqlCmd.AddParam(new Params("@notification_gid", notification.Gid, NpgsqlDbType.Integer));
                    sqlCmd.AddParam(new Params("@user_gid", PluginInitialization._app.user_info.id_user, NpgsqlDbType.Integer));
                    sqlCmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool SetFavorite(NotificationM notification, bool value)
        {
            try
            {
                using (var sqlCmd = PluginInitialization._app.SqlWork(true))
                {
                    sqlCmd.sql = @"UPDATE notification.notification_users
   SET favorite=@favorite
 WHERE notification_gid = @notification_gid AND user_gid = @user_gid;";
                    sqlCmd.AddParam(new Params("@favorite", value, NpgsqlDbType.Boolean));
                    sqlCmd.AddParam(new Params("@notification_gid", notification.Gid, NpgsqlDbType.Integer));
                    sqlCmd.AddParam(new Params("@user_gid", PluginInitialization._app.user_info.id_user, NpgsqlDbType.Integer));
                    sqlCmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public NotificationM GetNotification(int gid)
        {
            using (ISQLCommand sqlCmd = _app.SqlWork())
            {
                sqlCmd.sql = @"SELECT gid, type_gid, type_name, priority_type_gid, priority_name, weight, subject, 
message, created, seen_datetime, table_gid, row_gid, alias_text, user_gid, favorite 
FROM notification.view_user_notifications WHERE gid=" + gid.ToString();
                sqlCmd.ExecuteReader();
                if (sqlCmd.CanRead())
                {
                    return new NotificationM(
                        sqlCmd.GetValue<int>("gid"),
                        sqlCmd.GetValue<int>("type_gid"),
                        sqlCmd.GetValue<int>("priority_type_gid"),
                        sqlCmd.GetValue<int>("weight"),
                        sqlCmd.GetValue<string>("subject"),
                        sqlCmd.GetValue<string>("message"),
                        sqlCmd.GetValue<DateTime?>("created"),
                        sqlCmd.GetValue<DateTime?>("seen_datetime"),
                        sqlCmd.GetValue<int>("user_gid"),
                        sqlCmd.GetValue<bool>("favorite"),
                        new RefObjM() { IdObj = sqlCmd.GetValue<int?>("row_gid"), IdTable = sqlCmd.GetValue<int?>("table_gid"), Name = sqlCmd.GetValue<string>("alias_text") }
                        );
                }
            }
            return null;
        }
        public void SetReadedAllNotification(ENotificationStatus status,
            NotificationPriorityM priority,
            NotificationTypeM type,
            string filterText)
        {
            using (var sqlCmd = PluginInitialization._app.SqlWork())
            {
                sqlCmd.sql = @"UPDATE notification.notification_users
   SET seen_datetime = now()
 FROM notification.notifications n
 WHERE notification_users.user_gid = "+PluginInitialization._app.user_info.id_user.ToString()+
 " AND notification_users.seen_datetime IS NULL AND n.gid = notification_users.notification_gid "
+ GenerateWhereSQL(status, priority, type, filterText);
                sqlCmd.ExecuteNonQuery();
            }
        }
        public List<int> GetUserSettingsNotificationTypes(int idUser)
        {
            var result = new List<int>();
            using (var sqlCmd = PluginInitialization._app.SqlWork())
            {
                sqlCmd.sql = "SELECT user_id, type_id FROM notification.unsubscribes WHERE user_id = " +
                             idUser.ToString();
                sqlCmd.ExecuteReader();
                while (sqlCmd.CanRead())
                {
                    result.Add(sqlCmd.GetValue<int>("type_id"));
                }
            }
            return result;
        }
        public void UpdateUnsubscribes(Dictionary<int, bool> types)
        {
            using (var sqlCmd = PluginInitialization._app.SqlWork())
            {
                StringBuilder strs = new StringBuilder();
                strs.AppendFormat(
                            "DELETE FROM notification.unsubscribes WHERE user_id = {0};" +
                            Environment.NewLine,
                            PluginInitialization._app.user_info.id_user);

                foreach (var type in types)
                {
                    if (!type.Value)
                    {
                        strs.AppendFormat(
                            "INSERT INTO notification.unsubscribes (user_id, type_id) VALUES ({0},{1});" +
                            Environment.NewLine,
                            PluginInitialization._app.user_info.id_user,
                            type.Key);
                    }
                    sqlCmd.sql = strs.ToString();
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
    }
}
