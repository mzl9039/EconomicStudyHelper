using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;

namespace DataHelper.FuncSet.EGIndex.RobustCheck
{
    public class EGRobust : BaseEGIndex
    {
        public EGRobust(List<string> excels): base(excels)
		{
		    OutputFile = System.Windows.Forms.Application.StartupPath + "\\EGRobust\\Result.txt";
		    InitEGIndex();
		}

        public void OutPutEGIndex()
        {
            Common.FileIOInfo fileIo = new Common.FileIOInfo(OutputFile);
            if (!Directory.Exists(fileIo.FilePath))
                Directory.CreateDirectory(fileIo.FilePath);

            GetCacuResult(EGIndexShp, EGType.EGRobust);
            using (StreamWriter sw = File.AppendText(OutputFile))
            {
                foreach (var str in OutputResult)
                {
                    sw.WriteLine(str);
                }
            }
        }

        private void InitEGIndex()
        {
            List<Enterprise> enterprises = new List<Enterprise>();
            foreach (var kv in EGIndexEnterpriseData)
            {
                enterprises.AddRange(kv.Value.Enterprises);
            }

            EGIndexShp = new EGIndexShpInfo(enterprises, "选择黑河-腾冲线以东的全国范围的县级行政区划shp文件", "EGRobust.shp");
        }

        public EGIndexShpInfo EGIndexShp { get; protected set; }
    }
}
