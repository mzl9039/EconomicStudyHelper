using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DataHelper.BaseUtil;
using System.Threading.Tasks;

namespace DataHelper.FuncSet.EnterpriseInCircleBufferStatics
{
    public class EnterpriseInCircleBufferStatics
    {
        public EnterpriseInCircleBufferStatics() { }

        public EnterpriseInCircleBufferStatics(string excelFileName, double radius) {
            this.excelFileName = excelFileName;
            this.radius = radius;
            this.excelFileId = Path.GetFileNameWithoutExtension(this.excelFileName).TrimEnd(new char[] { 'g','p','s',' ' });
            this.GetEnterprises();
            this.GetOutputFileName();
        }

        public void GetEnterprises()
        {
            if (File.Exists(this.excelFileName))
            {
                DataTable table = Static.Table;
                this.enterprises = DataProcess.ReadExcel(this.excelFileName, table);
            }
        }

        public void GetOutputFileName() {
            if (this.excelFileName != "")
            {
                string path = System.IO.Path.GetDirectoryName(this.excelFileName);
                string filenameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(this.excelFileName);
                this.outputFileName = string.Format("{0}\\{1}.txt", path, filenameWithoutExt);
            }
            else
            {
                Log.Log.Info(string.Format("文件名{0}为空", this.excelFileName));
            }
        }

        public SortedDictionary<int, List<Output>> CaculateStatics() {
            SortedDictionary<int, List<Output>> result = new SortedDictionary<int, List<Output>>();

            foreach (Enterprise e in this.enterprises)
            {
                List<Enterprise> tmp = new List<Enterprise>();
                this.enterprises.ForEach(inn =>
                {
                    double dis = Math.Sqrt((e.Point.X - inn.Point.X) * (e.Point.X - inn.Point.X) +
                        (e.Point.Y - inn.Point.Y) * (e.Point.Y - inn.Point.Y)) / 1000;
                    // TODO 不是同一个企业，但是是同一个位置(一般这个是错误企业位置)
                    if (inn.ID != e.ID && dis <= this.radius)
                    {
                        tmp.Add(inn);
                    }
                });
                double total = tmp.Sum(t => t.man);
                List<Output> output = tmp.Select(t => new Output(this.excelFileId, e.ID, t.ID, total)).ToList();
                string id = e.ID.Split(new char[] { '.' })[1];
                result.Add(int.Parse(id), output);
            }

            return result;
        }

        public void WriteOutputFileName() {
            if (File.Exists(this.outputFileName))
            {
                Log.Log.Info(string.Format("文件{0}已经存在，跳过该文件的输出。", this.outputFileName));
            }
            else
            {
                SortedDictionary<int, List<Output>> output = this.CaculateStatics();         
                
                using (FileStream fs = new FileStream(this.outputFileName, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    foreach (KeyValuePair<int, List<Output>> kv in output)
                    {
                        kv.Value.ForEach(l => sw.WriteLine(string.Format("{0}, {1}, {2}, {3}", l.fileId, l.circleId, l.enterpriseIdInBuffer, l.total)));
                        sw.Flush();
                    }
                }
            }
        }

        private string excelFileName = "";
        private string excelFileId = "";
        private List<Enterprise> enterprises = new List<Enterprise>();
        private double radius = 0;        // 以 circleId 为圆心的buffer的大小，即圆的半径
        private string outputFileName = "";
        private List<Output> outputs = new List<Output>();

        public class Output {
            public Output(string fileId, string circleId, string enterpriseIdInBuffer, double total) {
                this.fileId = fileId;
                this.circleId = circleId;
                this.enterpriseIdInBuffer = enterpriseIdInBuffer;
                this.total = total;
            }

            public string fileId { get; set; }
            public string circleId { get; set; }
            public string enterpriseIdInBuffer { get; set; }
            public double total { get; set; }
        }
    }
}
