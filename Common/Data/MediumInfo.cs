/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-18
 * 时间: 0:44
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;

namespace Common
{
	/// <summary>
	/// 描述中位数的类
	/// </summary>
	public class MediumInfo
	{
		public MediumInfo(double symbol, Int64 counter){
			Symbol = symbol;
			Counter = counter;
			Stop = false;		
		}

        /// <summary>
        /// 根据DistanceFile，即某一公里范围内的文件距离类，来
        /// 在求中位数或第n个数时，只需要知道这个距离在当前这个文件类中是第几个，
        /// 而不需要知道这个距离在这500多亿个距离值中是第几个
        /// </summary>
        /// <param name="distance"></param>
        public void SetMedium(DistanceFile distance)
        {
            if (!Stop)
            {
                if (Counter - distance.FileRowCount > 0)
                    Counter -= distance.FileRowCount;
                else
                {
                    DistanceFile = new DistanceFile(distance);
                    Stop = true;
                }
            }
        }
		
		// 标识当前的这个对象的是哪个中位数，可以是1/4,1/2,3/4等等
		public double Symbol { get; set; }
		// 标识上面的Symbol对应总距离里的第几个
		public Int64 Counter { get; set; }
		// 是否找到了上面的Counter，找到了则设为true
		public bool Stop { get; set; }
		// 当前对象存储的DistanceFile信息
		public DistanceFile DistanceFile { get; set; }		
		
		public void Close() {
			if (this.DistanceFile != null) {
				this.DistanceFile.Close();
				this.DistanceFile = null;
			}
		}
	}
}
