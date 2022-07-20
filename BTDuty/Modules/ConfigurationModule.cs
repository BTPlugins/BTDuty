using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BTDuty.Modules
{
    public class DutyGroups
    {
        public string DutyName { get; set; }
        public string GroupID { get; set; }
        public string Permission { get; set; }
        public bool BlueHammer { get; set; }
    }
}
