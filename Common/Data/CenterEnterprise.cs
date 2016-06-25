using System;
using System.Collections.Generic;
using System.IO;
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

        public void PrintEnterprise(string filename)
        {
            if (File.Exists(filename))
                return;

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine("excel文件：" + Excel);
                sw.WriteLine("圆心企业Id:" + EnterpriseId);
                sw.WriteLine("圆直径：" + Diameter);
                sw.WriteLine("企业数量浓度为：" + NumDensity);
                sw.WriteLine("人口规模浓度为：" + ScaleDensity);
                sw.WriteLine("圆内企业：");
                for (int i = 0; i < Enterprises.Count; i++)
                {
                    sw.WriteLine("第" + i + "个企业ID：" + Enterprises[i].ID + ";" + Enterprises[i].Point.X + ";" + Enterprises[i].Point.Y);
                }

                sw.Flush();
                sw.Close();
            }
        }

        public string Excel { get; set; }
        public string EnterpriseId{ get; set; }
        public double Diameter { get; set; }
        public List<Enterprise> Enterprises { get; set; }
        public double NumDensity
        {
            // 因为Diameter为直径，所以需要在分母 * 4
            get { return Enterprises.Count * 4 / (Math.PI * Diameter * Diameter); }
        }
        public double ScaleDensity
        {
            get { return Enterprises.Sum(e => e.man) * 4 / (Math.PI * Diameter * Diameter); }
        }
    }
}
