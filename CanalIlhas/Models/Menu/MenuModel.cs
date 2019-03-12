using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CanalIlhas.Models.Menu
{
    public class MenuModel
    {
        public string ItemMaster { get; set; }
        public string ItemSub { get; set; }
        public string ItemSub2 { get; set; }
        public string Desc { get; set; }
        public string Controller { get; set; }
        public string Icon { get; set; }
    }
    public class MenuIconModel
    {
        public string idIcon { get; set; }
        public string descIcon { get; set; }

        public static IList<MenuIconModel> listaIcons()
        {
            var Listaitems = new List<MenuIconModel>();
            Listaitems.Add(new MenuIconModel { idIcon = "CopCalendar", descIcon = "CLD" });
            Listaitems.Add(new MenuIconModel { idIcon = "CopConnections", descIcon = "CCN" });
            Listaitems.Add(new MenuIconModel { idIcon = "CopLeader", descIcon = "CLR" });
            Listaitems.Add(new MenuIconModel { idIcon = "CopPool", descIcon = "CPL" });
            Listaitems.Add(new MenuIconModel { idIcon = "CopReorientation", descIcon = "CCR" });
            Listaitems.Add(new MenuIconModel { idIcon = "CopReorientationGe", descIcon = "CRG" });
            Listaitems.Add(new MenuIconModel { idIcon = "DesligueNet", descIcon = "DLN" });
            Listaitems.Add(new MenuIconModel { idIcon = "BacklogOuvidoria", descIcon = "BKO" });

            return Listaitems;
        }
    }
    public class MenuHtml
    {
        public static HtmlString menuHtml
        {
            get;
            set;
        }
    }
}
