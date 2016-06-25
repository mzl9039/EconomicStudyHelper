/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-18
 * 时间: 15:47
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;

namespace DataHelper.FuncSet.EGIndex
{
	/// <summary>
	/// Description of EGIndex.
	/// </summary>
	public class EGIndex : BaseEGIndex
	{
		public EGIndex(List<string> excels) : base(excels)
		{
		    OutputFile = System.Windows.Forms.Application.StartupPath + "\\EGIndex\\Result.txt";
				
	        InitEGIndex();
		}

        public void OutPutEGIndex()
        {
            Common.FileIOInfo fileIo = new Common.FileIOInfo(OutputFile);
            if (!Directory.Exists(fileIo.FilePath))
                Directory.CreateDirectory(fileIo.FilePath);

            GetCacuResult(EGIndexShp, EGType.EGIndex);
            using (StreamWriter sw = File.AppendText(OutputFile))
            {
                foreach (var str in OutputResult)
                {
                    sw.WriteLine(str);
                }
            }
        }

		private void InitEGIndex() {

            List<Enterprise> enterprises = new List<Enterprise>();
            foreach (var kv in EGIndexEnterpriseData)
            {
                enterprises.AddRange(kv.Value.Enterprises);
            }

            EGIndexShp = new EGIndexShpInfo(enterprises, "选择全国范围的县级行政区划shp文件", "EGIndex.shp");
		}

        public EGIndexShpInfo EGIndexShp { get; protected set; }
	}
}
