using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataHelper.BaseUtil;
using System.Data;
using ESRI.ArcGIS.Geometry;

namespace DataHelper.FuncSet.ShortestPath
{
    /************************************************************************/
    /* Description:	提供最短路径计算的一般方法
    /* Authon:		mzl
    /* Date:		2017/11/26 15:55:24
    /************************************************************************/
    public class SPUtils
    {
        /// <summary>
        /// 计算起点与终点之间的直线距离，不忽略距离等于0的情况
        /// </summary>
        /// <param name="enterprises"></param>
        /// <param name="srcEnterId"></param>
        /// <param name="dstEnterId"></param>
        /// <returns></returns>
        public static double caculateStraightDistance(IPoint oriEnt, IPoint destEnt) {
            double result = Math.Sqrt((oriEnt.X - destEnt.X) * (oriEnt.X - destEnt.X) +
                (oriEnt.Y - destEnt.Y) * (oriEnt.Y - destEnt.Y)) / 1000;
            return result;
        }

        /// <summary>
        /// 遍历所有的excels，读取企业点信息
        /// </summary>
        /// <param name="excels"></param>
        /// <returns></returns>
        public static List<Enterprise> getAllEnterprises(List<string> excels) {
            try
            {
                if (Static.Enterprises == null)
                {
                    DataTable table = Static.Table;
                    Static.Enterprises = DataProcess.ReadExcels(excels, table, true, null, FunctionType.Kd);
                }
                return Static.Enterprises;
            }
            catch (System.Exception ex)
            {
                Log.Log.Error("读取excels文件失败", ex);
                return new List<Enterprise>();
            }            
        }    

    }
}
