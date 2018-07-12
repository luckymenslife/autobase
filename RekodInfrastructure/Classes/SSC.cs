using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RESTLib.Enums;
using REST = RESTLib.Model.REST;
using WFS = RESTLib.Model.WFS;

namespace Rekod.Classes
{
    public class SSC
    {
        private REST.User _sscUser;

        List<GroupSSCInfo> GroupSSC;
        private RESTLib.Queries.Layers _sscLayers;

        public SSC(string login, string pass, Uri url)
        {
            _sscUser = new REST.User(login, pass, url);
            if (_sscUser != null)
            {
                _sscLayers = new RESTLib.Queries.Layers(_sscUser);
                if (_sscLayers != null)
                    Load();
            }
        }

        public GroupSSCInfo GetGroupByName(string name)
        {
            if (name == null)
                return null;
            return GroupSSC.FirstOrDefault(f => f.GroupSSC.name.ToLower() == name.ToLower());
        }
        public WFS.Layer GetLayerByName(GroupSSCInfo gr, string name)
        {
            if (gr == null || name == null)
                return null;
            return gr.Layers.FirstOrDefault(f => f.Name.ToLower() == name.ToLower());
        }

        public void ClearId()
        {
            foreach (var item in GroupSSC)
            {
                item.GroupId = null;
                item.WorkLayers.Clear();
            }
        }
        public GroupSSCInfo GetGroupByGroupId(int id)
        {
            foreach (var item in GroupSSC)
            {
                if (item.GroupId == id)
                    return item;
            }
            return null;
        }
        /// <summary>
        /// Прверяет если такая таблица в SSC
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckedWorkTable(int id)
        {
            foreach (var item in GroupSSC)
            {
                if (item.WorkLayers.ContainsKey(id))
                    return true;
            }
            return false;
        }

        private void Load()
        {
            var userQueries = new RESTLib.Queries.Users(_sscUser);
            
            var SSCGroupList = _sscLayers.GetGroups();
            //var SSCLayerList = _sscLayers.GetLayers();

            GroupSSC = new List<GroupSSCInfo>();
            if (SSCGroupList != null)
            {
                foreach (var item in SSCGroupList)
                {
                    GroupSSC.Add(new GroupSSCInfo(item));
                }
            }
        }
    }
    public class GroupSSCInfo
    {
        public REST.Group GroupSSC;
        public List<WFS.Layer> Layers;

        public int? GroupId;
        public Dictionary<int, WFS.Layer> WorkLayers;

        public GroupSSCInfo(REST.Group groupSSC)
        {

            GroupSSC = groupSSC;
            Layers = new List<WFS.Layer>();
            WorkLayers = new Dictionary<int, WFS.Layer>();
        }
    }
}
