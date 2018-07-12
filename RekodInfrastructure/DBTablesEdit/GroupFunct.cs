using System;
using System.Collections.Generic;
using System.Text;
using Interfaces;
using Npgsql;
using System.Windows.Forms;
using Rekod.Services;

namespace Rekod
{
    public class GroupFunct
    {
        public GroupFunct()
        {
        }
        /// <summary>
        /// Метод загружает из базы все группы в List
        /// </summary>
        public List<itemObjOrdered> LoadGroups()
        {
            List<itemObjOrdered> list_groups = new List<itemObjOrdered>();
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT id,name_group,descript,order_num FROM " + Program.scheme + ".table_groups ORDER BY order_num";
            try
            {
                sqlCmd.Execute(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                sqlCmd.Close();
                return null;
            }
            while (sqlCmd.CanRead())
            {
                int id_gr = sqlCmd.GetInt32(0);
                string name_gr = sqlCmd.GetString(1);
                string desc_gr = sqlCmd.GetString(2);
                int order_num = sqlCmd.GetInt32(3);
                itemObjOrdered io = new itemObjOrdered(id_gr, name_gr, desc_gr, order_num);//использую уже готовую структуру, вместо Layer там будет храниться описание
                list_groups.Add(io);
            }
            sqlCmd.Close();

            return list_groups;
        }
        public void SetGroupsOrder(List<itemObjOrdered> newGroupsOrder)
        {
            try
            {
                SqlWork sqlCmd = new SqlWork();
                sqlCmd.BeginTransaction();
                for (int i = 0; i < newGroupsOrder.Count; i++)
                {
                    itemObjOrdered itemObjOrd = newGroupsOrder[i];
                    int objId = itemObjOrd.Id_o;
                    int orderNum = i + 1;
                    sqlCmd.sql= String.Format("UPDATE {0}.{1} SET order_num={2} WHERE id={3}", Program.scheme, "table_groups", orderNum, objId);

                    sqlCmd.ExecuteNonQuery();
                }
                sqlCmd.EndTransaction();
            }
            catch (Exception ex)
            {
                Rekod.Classes.workLogFile.writeLogFile(Rekod.Properties.Resources.GF_OrderGroup+" \n" + ex.Message, false, true);
            }
        }
        /// <summary>
        /// Создает новую группу в базе
        /// </summary>
        /// <param name="name_gr">Название группы</param>
        /// <param name="desc_gr">Описание группы</param>
        public void AddNewGroup(string name_gr, string desc_gr)
        {
            using (SqlWork sqlCmd = new SqlWork(true))
            {
                sqlCmd.sql = "INSERT INTO " + Program.scheme + ".table_groups (name_group,descript,order_num) VALUES (@name_group,@descript,1000000) RETURNING id";

                var parms = new IParams[]
                            {
                                new Params
                                    {
                                        paramName = "@name_group",
                                        typeData = NpgsqlTypes.NpgsqlDbType.Text,
                                        value = name_gr
                                    },
                                new Params
                                    {
                                        paramName = "@descript", 
                                        typeData = NpgsqlTypes.NpgsqlDbType.Text, 
                                        value = desc_gr
                                    }
                            };
                try
                {
                    int id_new_group = sqlCmd.ExecuteScalar<int>(parms);
                    Rekod.Classes.workLogFile.writeLogFile("added new group --> id=" + id_new_group + " name_group=" + name_gr, false, false);

                }
                catch (NpgsqlException ex)
                {
                    if (ex.Code.CompareTo("42501") == 0)
                        MessageBox.Show(Rekod.Properties.Resources.GF_ErrorCreateGroup);
                    else
                        throw ex;
                }
            }

            List<itemObjOrdered> newGroupsOrder = getRecalcedGroupsOrderList();
            SetGroupsOrder(newGroupsOrder);
        }
        /// <summary>
        /// Удаляет группу с определенным ID, пересчитывает их порядок
        /// </summary>
        /// <param name="id">ID группы</param>
        public void DeleteGroup(int id)
        {
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "DELETE FROM " + Program.scheme + ".table_groups WHERE id=" + id.ToString() + " RETURNING name_group";
            try
            {
                sqlCmd.Execute(false);
                if (sqlCmd.CanRead())
                {
                    string name_gr = sqlCmd.GetString(0);
                    Rekod.Classes.workLogFile.writeLogFile("deleted group --> id=" + id.ToString() + " name_group=" + name_gr, false, false);
                }
            }
            catch (NpgsqlException ex)
            {
                if (ex.Code.CompareTo("42501") == 0)
                {
                    MessageBox.Show(Rekod.Properties.Resources.GF_ErrorCreateGroup);
                }
                else throw ex;
            }
            finally
            {
                sqlCmd.Close();
            }

            List<itemObjOrdered> newGroupsOrder = getRecalcedGroupsOrderList();
            SetGroupsOrder(newGroupsOrder);
        }

        private List<itemObjOrdered> getRecalcedGroupsOrderList()
        {
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = String.Format("SELECT id, name_group, descript FROM {0}.{1} ORDER BY order_num", Program.scheme, "table_groups");
            sqlCmd.Execute(false);
            List<itemObjOrdered> newGroupsOrder = new List<itemObjOrdered>();
            int orderObj = 1;
            while (sqlCmd.CanRead())
            {
                int idObj = sqlCmd.GetInt32(0);
                String nameObj = sqlCmd.GetValue<string>(1);
                String descObj = sqlCmd.GetValue<string>(2);
                itemObjOrdered itObjOrd = new itemObjOrdered(idObj, nameObj, descObj, orderObj);
                orderObj++;
            }
            return newGroupsOrder;
        }

        /// <summary>
        /// Возвращает список таблиц в(вне) группе
        /// </summary>
        /// <param name="id">ID группы</param>
        /// <param name="in_group">Если TRUE, то то список будет таблиц в группе, FALSE- список вне группы</param>
        /// <returns></returns>
        public List<itemObjOrdered> GetListTablesInOrOutGroup(int id, bool in_group)
        {
            List<itemObjOrdered> list_of_tables = new List<itemObjOrdered>();
            SqlWork sqlCmd = new SqlWork();
            if (in_group)
            {

                sqlCmd.sql = "SELECT t.id,t.name_db,t.name_map,g.order_num FROM " + Program.scheme + ".table_groups_table AS g,"
                    + Program.scheme + ".table_info AS t WHERE g.id_group=" + id.ToString() + " AND g.id_table=t.id AND t.type=1 and hidden = false ORDER BY g.order_num";
                try
                {
                    sqlCmd.Execute(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }
            else
            {
                sqlCmd.sql =
                    "SELECT * FROM " +
                    "(SELECT t.id,t.name_db,t.name_map FROM " + Program.scheme + ".table_info AS t WHERE t.type=1 and hidden=false" +
                    " EXCEPT " +
                    "SELECT DISTINCT t.id,t.name_db,t.name_map FROM " + Program.scheme + ".table_groups_table AS g," + Program.scheme +
                    ".table_info AS t WHERE g.id_group=" + id.ToString() + " AND g.id_table=t.id and hidden = false " +
                    ")AS al ORDER BY al.name_map";
                try
                {
                    sqlCmd.Execute(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return null;
                }
            }

            while (sqlCmd.CanRead())
            {
                int id_table = sqlCmd.GetInt32(0);
                string db_name_table = sqlCmd.GetString(1);
                string map_name_table = sqlCmd.GetString(2);
                int ord = 0;
                if (in_group)
                    ord = sqlCmd.GetInt32(3);
                itemObjOrdered io = new itemObjOrdered(id_table, map_name_table, db_name_table, ord);
                list_of_tables.Add(io);
            }
            sqlCmd.Close();
            return list_of_tables;
        }
        /// <summary>
        /// Добавляет таблицу в группу
        /// </summary>
        /// <param name="id_group"></param>
        /// <param name="id_table"></param>
        /// <returns>Была ли добавлена таблица в группу, если FALSE то были переданы неверные ID</returns>
        public bool MoveTableToGroup(int id_group, int id_table)
        {
            bool moved = false;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT " + Program.scheme + ".add_table_in_group(" + id_table.ToString() + "," + id_group.ToString() + ")";
            //"INSERT INTO " + Program.scheme + ".table_groups_table (id_table,id_group) VALUES ("+id_table.ToString()+","+id_group.ToString()+")";
            try
            {
                sqlCmd.Execute(true);
            }
            catch (NpgsqlException ex)
            {
                if (ex.Code.CompareTo("23503") == 0)
                {
                    MessageBox.Show(Rekod.Properties.Resources.GF_NonexistentGroupTable);
                    return false;
                }
                else
                    if (ex.Code.CompareTo("23505") == 0)
                    {
                        moved = true;
                    }
                    else throw ex;
            }
            finally
            {
                sqlCmd.Close();
            }
            if (!moved)
                Rekod.Classes.workLogFile.writeLogFile("move table to group --> id_group=" + id_group.ToString() + " id_table=" + id_table.ToString(), false, false);
            return true;
        }
        public void SetOrderTableInGroup(int id_group, int id_table, int ord)
        {
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "UPDATE " + Program.scheme + ".table_groups_table SET order_num=" + ord.ToString() + " WHERE id_table=" + id_table.ToString() + " AND id_group=" + id_group.ToString();
            //"INSERT INTO " + Program.scheme + ".table_groups_table (id_table,id_group) VALUES ("+id_table.ToString()+","+id_group.ToString()+")";
            try
            {
                sqlCmd.Execute(true);
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.BaseMessage);
            }
            finally
            {
                sqlCmd.Close();
            }
        }
        public void DeleteTableFromGroup(int id_group, int id_table)
        {
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "DELETE FROM " + Program.scheme + ".table_groups_table WHERE id_table=" + id_table.ToString() + " AND id_group=" + id_group.ToString();
            try
            {
                sqlCmd.Execute(true);
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.BaseMessage);
            }
            finally
            {
                sqlCmd.Close();
            }
        }
        public itemObj GetGroup(int id_gr)
        {
            itemObj group = null;
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "SELECT name_group,descript FROM " + Program.scheme + ".table_groups WHERE id=" + id_gr.ToString();
            try
            {
                sqlCmd.Execute(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                sqlCmd.Close();
                return null;
            }
            if (sqlCmd.CanRead())
            {
                string name_gr = sqlCmd.GetString(0);
                string desc_gr = sqlCmd.GetString(1);
                group = new itemObj(id_gr, name_gr, desc_gr);//использую уже готовую структуру, вместо Layer там будет храниться описание
            }
            sqlCmd.Close();

            return group;
        }
        public void SaveGroup(int id_gr, string name_gr, string desc_gr)
        {
            SqlWork sqlCmd = new SqlWork();
            sqlCmd.sql = "UPDATE " + Program.scheme + ".table_groups SET name_group=@name_group, descript=@descript WHERE id=" + id_gr.ToString();
            //"SELECT name_group,descript FROM " + Program.scheme + ".table_groups WHERE id=" + id_gr.ToString();
            Params[] parms = new Params[2];

            parms[0] = new Params();
            parms[0].paramName = "@name_group";
            parms[0].typeData = NpgsqlTypes.NpgsqlDbType.Text;
            parms[0].value = name_gr;

            parms[1] = new Params();
            parms[1].paramName = "@descript";
            parms[1].typeData = NpgsqlTypes.NpgsqlDbType.Text;
            parms[1].value = desc_gr;

            try
            {
                sqlCmd.ExecuteNonQuery(parms);
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.BaseMessage);
            }
            finally
            {
                sqlCmd.Close();
            }
        }
        public List<itemObj> GetGroupsWithTable(int id, bool with)
        {
            List<itemObj> list_of_tables = new List<itemObj>();
            SqlWork sqlCmd = new SqlWork();
            if (with)
                sqlCmd.sql = "SELECT * FROM " + Program.scheme + ".get_group_list_for_table(" + id.ToString() + ")";
            else
                sqlCmd.sql = "SELECT * FROM " + Program.scheme + ".get_group_list_no_for_table(" + id.ToString() + ")";

            try
            {
                sqlCmd.Execute(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                sqlCmd.Close();
                return null;
            }
            while (sqlCmd.CanRead())
            {
                int id_table = sqlCmd.GetInt32(0);
                string name_group = sqlCmd.GetString(1);
                string descript = sqlCmd.GetString(2);
                itemObj io = new itemObj(id_table, name_group, descript);
                list_of_tables.Add(io);
            }
            sqlCmd.Close();
            return list_of_tables;
        }
    }
}