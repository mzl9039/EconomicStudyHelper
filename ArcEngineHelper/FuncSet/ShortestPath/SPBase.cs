using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using System.Windows.Forms;

namespace DataHelper.FuncSet.ShortestPath
{
    /************************************************************************/
    /* Description:	计算两点之间的最短路径（Shortest Path）的基础类
    /* Authon:		mzl
    /* Date:		2017/11/26 15:00:39
    /************************************************************************/
    public class SPBase
    {
        public List<string> excels { get; set; }

        public SPBase(IEnumerable<String> excels) {
            if (excels == null) {
                Log.Log.Warn("找不到 excel 文件，参数 excels 为空");
            }
            this.excels = excels as List<String>;
        }

        /// <summary>
        /// 计算所有企业最短路径的平均值和中位数
        /// </summary>
        public void SPStat()
        {
            GetSpeed getSpeed = new GetSpeed();
            double speed = -1;
            string cutOff = "";
            if (getSpeed.ShowDialog() == DialogResult.OK)
            {
                speed = getSpeed.Speed();
                cutOff = getSpeed.CutOff();
            }

            if (speed == -1 || string.IsNullOrWhiteSpace(cutOff))
            {
                Log.Log.Warn("无法正确获取速度或cutOff，退出。");
                return;
            }
            string railPoints = DataPreProcess.GetShpName("获取公路/铁路线点集");
            if (!File.Exists(railPoints) && !railPoints.EndsWith(".shp"))
            {
                Log.Log.Warn("公路/铁路线点集文件不存在");
                return;
            }
            SPStats stats = new SPStats(excels, cutOff);
            stats.setSpeed(speed);
            stats.setRailPoints(railPoints);
            stats.caculateShortestPath();
        }
    }
}
