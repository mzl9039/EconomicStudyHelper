/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-17
 * 时间: 22:18
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using LogHelper;
using System.Collections.Concurrent;
using DataHelper;

namespace DataHelper
{
    /// <summary>
    /// Description of FindMedium.
    /// </summary>
    public partial class FindMedium : FindMediumBase
    {
        /// <summary>
        /// 计算中位数或其它第n位数，尤其适用于针对某一个excel内部的中位数
        /// 若计算全部的excel,则参数excel可为空
        /// </summary>
        /// <param name="excel"></param>
        /// <param name="enterprises"></param>
        /// <param name="MaxDistance"></param>
        public FindMedium(string excel, List<Enterprise> enterprises, double MaxDistance = 0.0)
            : base(enterprises, MaxDistance)
        {
            Filename = excel;
        }

        public override void InitDistanceFiles()
        {
            DistanceFiles = new ConcurrentDictionary<int, DistanceFile>();
            FileIOInfo fio = new FileIOInfo(this.Filename);
            string dfDir = fio.FilePath + @"\" + fio.FileNameWithoutExt;
            for (int i = 0; i < 5000; i++)
            {
                DistanceFile df = new DistanceFile(string.Format(dfDir + @"\{0}.txt", i.ToString()), i);
                DistanceFiles.TryAdd(i, df);
            }
        }

        public string Filename { get; private set; }

    }
}
