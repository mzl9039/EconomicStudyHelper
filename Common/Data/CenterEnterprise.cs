using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Data
{
    public class CenterEnterprise
    {
        public CenterEnterprise() { }

        public CenterEnterprise(string excel, double diameter)
        {
            this.Excel = excel;
            this.Diameter = diameter;
            EnterpriseId = string.Empty;
            Enterprises = new List<Enterprise>();
        }

        public string Excel { get; set; }
        public string EnterpriseId{ get; set; }
        public double Diameter { get; set; }
        public List<Enterprise> Enterprises { get; set; }
    }
}
