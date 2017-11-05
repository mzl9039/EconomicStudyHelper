/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-16
 * 时间: 20:57
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Common;
// using LogHelper;
using ESRI.ArcGIS.Geometry;
using System.Collections.Concurrent;
using DataHelper.BaseUtil;
using Common.Data;

namespace DataHelper
{
    /// <summary>
    /// Description of DataProcess.
    /// </summary>
    public class DataProcess
    {
        /// <summary>
        /// 读取 excel 中的数据，返回一个list
        /// </summary>
        /// <param name="filename">要读取的 excel 文件的文件名</param>
        /// <param name="pointCheckGeo">数据点所在的范围，若在此范围内则记录，否则跳过</param>
        /// <param name="ft">方法类型，不同方法对点的坐标转换不同</param>
        /// <returns></returns>
        public static List<Enterprise> ReadExcel(string filename, DataTable t, bool convert = true, IGeometry pointCheckGeo = null, FunctionType ft = FunctionType.Default)
        {
            List<Enterprise> enterprises = new List<Enterprise>();
            FileIOInfo fileIO = new FileIOInfo(filename);
            IExcelOp excelReader = ExcelOp.GetExcelReader(filename);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            excelReader.SetTable = t;
            try
            {
                DataTable table = excelReader.ReadExcel(filename, (int)DefaltExcel.DefaltSheet);
                if (table == null)
                {
                    Log.Log.Error(string.Format("读取 Excel 文件：{0}失败！", filename));
                    return null;
                }

                DataRow dr = null;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    dr = table.Rows[i];
                    // 如果本行ID为空，认为本行数据为空，删除本行
                    if (dr["ID"] == null || dr["Position"] == null ||
                        string.IsNullOrWhiteSpace(dr["ID"].ToString().Trim()) ||
                        string.IsNullOrWhiteSpace(dr["Position"].ToString().Trim()))
                        continue;

                    // 如果本行男女总人数为0，认为本行数据为符合要求，删除本行
                    int man = Convert.ToInt32(double.Parse(dr["Man"].ToString()));
                    int woman = Convert.ToInt32(double.Parse(dr["Woman"].ToString()));
                    if (0 == (man + woman))
                        continue;

                    double lat = double.Parse(dr["Latitude"].ToString());
                    double lng = double.Parse(dr["Longitude"].ToString());

                    int zone = 0;
                    // 方法一、原来的代码 [6/5/2016 0:57:15 mzl]
                    // 若 convert 为false，则保留原来的经纬度；否则转为平面坐标 [10/15/2017 14:16:32 mzl]
                    IPoint point = new PointClass();
                    if (convert)
                    {
                        point = ExcelToShp.CastPointByFunctionType(lng, lat, ft);
                    }
                    else
                    {
                        point.PutCoords(lng, lat);
                    }
                    // 方法二、利用WKSPoint [6/5/2016 0:58:11 mzl]
                    //IPoint point = ExcelToShp.GCS2PRJ(lng, lat);
                    // 方法三、网上抄的代码 [6/5/2016 0:58:34 mzl]
                    double x = 0, y = 0;
                    //ExcelToShp.GeoToPrj(lng, lat, out x, out y, out zone);
                    IPoint newpoint = new PointClass();
                    newpoint.PutCoords(point.X, point.Y);

                    if (pointCheckGeo != null && !ExcelToShp.IsPointValid(point, pointCheckGeo))
                        continue;

                    string id = fileIO.FileNameWithoutExt + "." + dr["ID"].ToString();
                    // 由于3131内存不足，new Enterprise时取消第4个变量，这个变量在代码中似乎从来没用到过 [1/22/2017 20:14:07 mzl]           
                    //Enterprise e = new Enterprise(id, point, man + woman, new Common.Point(lng, lat, zone));
                    Enterprise e = new Enterprise(id, point, man + woman);
                    enterprises.Add(e);
                }
                table.Clear(); table = null;
                stopwatch.Stop();
                Log.Log.Info(string.Format("读取excel文件{0}的时间为：{1}", fileIO.FileName, stopwatch.ElapsedTicks / Stopwatch.Frequency));
                return enterprises;
            }
            catch (Exception ex)
            {
                Log.Log.Error(string.Format("读取文件{0}异常。\r\n异常为:{1}", filename, ex.Message));
                return null;
            }
        }

        public static List<KdMaxDistance> ReadExcelMaxDistance(string filename, DataTable t)
        {
            List<KdMaxDistance> max = new List<KdMaxDistance>();
            FileIOInfo fileIO = new FileIOInfo(filename);
            IExcelOp excelReader = ExcelOp.GetExcelReader(filename);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            excelReader.SetTable = t;

            try
            {
                DataTable table = excelReader.ReadExcel(filename, (int)DefaltExcel.DefaltSheet);
                if (table == null)
                {
                    Log.Log.Error(string.Format("读取 Excel 文件：{0}失败！", filename));
                    return null;
                }

                max = table.AsEnumerable().AsParallel().Select(r => new KdMaxDistance(r.Field<string>("EC"), r.Field<double>("Max"))).ToList();

                table.Clear(); table = null;
            }
            catch (Exception ex)
            {
                Log.Log.Error(string.Format("读取文件{0}异常。\r\n异常为:{1}", filename, ex.Message));
                throw ex;
            }

            stopwatch.Stop();
            Log.Log.Info(string.Format("读取excel文件{0}用时为：{1}", fileIO.FileName, stopwatch.ElapsedTicks / Stopwatch.Frequency));
            return max;
        }

        public static List<CircleDiameter> ReadExcelCircleDiameter(string filename, DataTable t)
        {
            List<CircleDiameter> result = new List<CircleDiameter>();
            FileIOInfo fileIO = new FileIOInfo(filename);
            IExcelOp excelReader = ExcelOp.GetExcelReader(filename);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            excelReader.SetTable = t;

            try
            {
                DataTable table = excelReader.ReadExcel(filename, (int)DefaltExcel.DefaltSheet);
                if (table == null)
                {
                    Log.Log.Error(string.Format("读取 Excel 文件：{0}失败！", filename));
                    return null;
                }
                result = table.AsEnumerable().AsParallel().Select(r => new CircleDiameter(r.Field<string>("en"), r.Field<double>("dm"))).ToList();
                table.Clear(); table = null;
            }
            catch (Exception ex)
            {
                Log.Log.Error(string.Format("读取文件{0}异常。\r\n异常为:{1}", filename, ex.Message));
                throw;
            }
            stopwatch.Stop();
            Log.Log.Info(string.Format("读取excel文件{0}用时为：{1}", fileIO.FileName, stopwatch.ElapsedTicks / Stopwatch.Frequency));

            result.Sort((x, y) => int.Parse(x.EnterpriseCode).CompareTo(int.Parse(y.EnterpriseCode)));
            return result;
        }

        public static List<MultiCircleDiameters> ReadMultiCircleDiameter(string filename, DataTable t)
        {
            List<MultiCircleDiameters> result = new List<MultiCircleDiameters>();
            FileIOInfo fileIO = new FileIOInfo(filename);
            IExcelOp excelReader = ExcelOp.GetExcelReader(filename);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            excelReader.SetTable = t;

            try
            {
                DataTable table = excelReader.ReadExcel(filename, (int)DefaltExcel.DefaltSheet);
                if (table == null)
                {
                    Log.Log.Error("读取 Excel 文件：" + filename + "失败！");
                    return null;
                }
                result = table.AsEnumerable().AsParallel()
                    .Select(r => new MultiCircleDiameters(
                        r.Field<string>("id"), 
                        new List<double>() {
                            r.Field<double>("first_dm"),
                            r.Field<double>("second_dm")
                        }, 
                        r.Field<double>("density")))
                        .ToList();
                table.Clear(); table = null;
            }
            catch (Exception ex)
            {
                Log.Log.Error("出错文件：" + filename + " " + ex.Message);
                throw;
            }
            stopwatch.Stop();
            Log.Log.Info("文件" + fileIO.FileName + "读取excel值的时间为" + stopwatch.ElapsedTicks / Stopwatch.Frequency);

            result.Sort((x, y) => int.Parse(x.EnterpriseCode).CompareTo(int.Parse(y.EnterpriseCode)));
            return result;
        }

        public static DataTable ReadEnterpriseDistributedInCountryExcel(String filename, DataTable table)
        {
            FileIOInfo fileIO = new FileIOInfo(filename);
            IExcelOp excelReader = ExcelOp.GetExcelReader(filename);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            excelReader.SetTable = table;

            try
            {
                table = excelReader.ReadExcel(filename, (int)DefaltExcel.DefaltSheet);
            }
            catch (System.Exception ex)
            {
                Log.Log.Error("出错文件：" + filename + " " + ex.Message);
                throw;
            }

            stopwatch.Stop();
            Log.Log.Info("文件" + fileIO.FileName + "读取excel值的时间为" + stopwatch.ElapsedTicks / Stopwatch.Frequency);
            return table;
        }

        /// <summary>
        /// 读取所有给定 excel 中的数据
        /// </summary>
        /// <param name="filename">要读取的 excel 文件的文件名</param>
        /// <param name="pointCheckGeo">数据点所在的范围，若在此范围内则记录，否则跳过</param>
        /// <param name="ft">方法类型，不同方法对点的坐标转换不同</param>
        /// <returns></returns>
        public static List<Enterprise> ReadExcels(List<string> excels, DataTable table, bool convert, IGeometry pointCheckGeo = null, FunctionType ft = FunctionType.Default)
        {
            List<Enterprise> enterprises = new List<Enterprise>();
            excels.ForEach(e =>
            {
                enterprises.AddRange(ReadExcel(e, Static.Table, convert, pointCheckGeo, ft));
            });
            return enterprises;
        }
    }
}
