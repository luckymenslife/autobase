using System;
using System.Collections.Generic;
using System.Text;

namespace Rekod
{

    struct relatinStrukt
    {
        public string NameForUser { get; set; }
        public string NameInBd { get; set; }
        public int idT { get; set; }
    }
    class relation : Interfaces.IRelation
    {
        private List<relatinStrukt> RelationList = new List<relatinStrukt>();

        public string GetNameForUser(string NameInBd)
        {
            foreach (relatinStrukt RelationStrukt1 in RelationList)
            {
                if (NameInBd == RelationStrukt1.NameInBd)
                {
                    return RelationStrukt1.NameForUser;
                }
            }
            return NameInBd;
        }
        public int GetIdTable(string NameInBd)
        {
            foreach (relatinStrukt RelationStrukt1 in RelationList)
            {
                if (NameInBd == RelationStrukt1.NameInBd)
                {
                    return RelationStrukt1.idT;
                }
            }
            return -1;
        }
        public string GetNameInBd(int idT)
        {
            foreach (relatinStrukt RelationStrukt1 in RelationList)
            {
                if (idT == RelationStrukt1.idT)
                {
                    return RelationStrukt1.NameInBd;
                }
            }
            return "";
        }
        public string GetNameForUser(int idT)
        {
            foreach (relatinStrukt RelationStrukt1 in RelationList)
            {
                if (idT == RelationStrukt1.idT)
                {
                    return RelationStrukt1.NameForUser;
                }
            }
            return "";
        }
        //public string GetNameInBd(string NameForUser)
        //{
        //    foreach (relatinStrukt RelationStrukt1 in RelationList)
        //    {
        //        if (NameForUser == RelationStrukt1.NameForUser)
        //        {
        //            return RelationStrukt1.NameInBd;
        //        }
        //    }
        //    return NameForUser;
        //}
        public void addRelation(string NameForUser, string NameForBd)
        {
            // relatinStrukt RelationStrukt1 = new relatinStrukt(NameForUser, NameForBd);
            relatinStrukt RelationStrukt1 = new relatinStrukt();
            RelationStrukt1.NameForUser = NameForUser;
            RelationStrukt1.NameInBd = NameForBd;
            if (RelationList.FindAll(w => w.NameForUser == NameForUser && w.NameInBd == NameForBd).Count==0)
            {
                RelationList.Add(RelationStrukt1);
            }
        }
        public void addRelation(string NameForUser, string NameForBd, int idT)
        {
//Предприятия по переработке шин действующие 01 03 2010 г в РФ
            // relatinStrukt RelationStrukt1 = new relatinStrukt(NameForUser, NameForBd);
            relatinStrukt RelationStrukt1 = new relatinStrukt();
            RelationStrukt1.NameForUser = NameForUser;
            if (NameForBd.Length > 55)
                RelationStrukt1.NameInBd = NameForBd.Remove(55);
            else
                RelationStrukt1.NameInBd = NameForBd;
            RelationStrukt1.idT = idT;
            if (RelationList.FindAll(w => w.idT == idT).Count == 0)
            {
                RelationList.Add(RelationStrukt1);
            }
        }
        public void deleteRelation(string NameForUser, string NameForBd)
        {
            foreach (relatinStrukt RelationStrukt1 in RelationList)
            {
                if (NameForUser == RelationStrukt1.NameForUser && NameForBd == RelationStrukt1.NameInBd)
                {
                    RelationList.Remove(RelationStrukt1);
                    return;
                }
            }
        }
    }
}
