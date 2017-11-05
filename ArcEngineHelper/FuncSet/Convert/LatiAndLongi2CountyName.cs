using Common;
using DataHelper.BaseUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using DataHelper.FuncSet.EGIndex;

namespace DataHelper.FuncSet.Convert
{
    /*
     *	实现类型转换的一个类，主要根据带经纬度坐标的企业点信息excel的点的坐标，获取
     *	shp中相对应的县的名称
     */
    public class LatiAndLongi2CountyName : IConvert
    {
        public LatiAndLongi2CountyName(IEnumerable<string> excels)
        {
            if (excels == null || excels.Count() == 0)
            {
                Log.Log.Error("你选择的目录下找不到符合规定的excel文件，退出！");
                throw new Exception("你选择的目录下找不到符合规定的excel文件，退出！");
            }
            Excels = excels as List<string>;

            try
            {
                // 检查目录和文件是否存在，若存在，则删除后重新创建 [9/17/2017 17:49:11 mzl]
                FileIOInfo fileIo = new FileIOInfo(excels.First());
                string directory = string.Format("{0}", fileIo.FilePath);
                EnterpriseWithCountyCsv = string.Format("{0}\\{1}.{2}", fileIo.FilePath, "EnterprisesWithCounty", "csv");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }              
                if (File.Exists(EnterpriseWithCountyCsv))
                {
                    File.Delete(EnterpriseWithCountyCsv);
                }
                using (FileStream fs = File.Create(EnterpriseWithCountyCsv)) { }
            }
            catch (System.Exception ex)
            {
                Log.Log.Error(ex);
            }

        }

        /// <summary>
        /// 把所有企业点信息保存成csv，通过arcgis转为dbf，然后导出为shp，即可，这样有较快的shp生成速度
        /// 还保留了原有的经纬度信息，但需要手动操作一次
        /// </summary>
        public void excelSaveAsCsv()
        {
            try
            {
                Enterprises = DataProcess.ReadExcels(Excels, Static.Table, false);
                FileIOInfo fileIo = new FileIOInfo(EnterpriseWithCountyCsv);
                string csvName = string.Format("{0}\\{1}.{2}", fileIo.FilePath, "oldEnterprises", "csv");
                using (FileStream fs = new FileStream(csvName, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(string.Format("{0}, {1}, {2}, {3}", "ExcelId", "Lng", "Lat", "man"));
                    for (int i = 0; i < Enterprises.Count; i++)
                    {
                        sw.WriteLine(Enterprises[i].ToString());
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public void convert()
        {
            try
            {
                // 不论是读取excel，还是创建shp都是一个比较耗时的工作，放在构造函数里感觉不是太合适 [10/15/2017 11:18:49 mzl]
                // Enterprises = DataProcess.ReadExcels(Excels, Static.Table, false);
                // 选择 shp 文件 [9/17/2017 17:07:03 mzl]
                // 选择shp文件，并生成所有企业点的shp文件 [10/15/2017 11:00:03 mzl]
                string countyShpName = DataPreProcess.GetShpName("选择中国县界shp");
                string enterpriseShpName = DataPreProcess.GetShpName("选择企业点shp");
                if (countyShpName == null || countyShpName == "" || !File.Exists(countyShpName) ||
                    enterpriseShpName == null || enterpriseShpName == "" || !File.Exists(enterpriseShpName))
                {
                    Log.Log.Error("文件名为空或文件不存在，退出！");
                    throw new Exception("文件名为空或文件不存在，退出！");
                }
                countyFeatureClass = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(countyShpName);
                enterpriseFeatureClass = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(enterpriseShpName);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            Enterprise2ConvertedEnterprise e2ce = new Enterprise2ConvertedEnterprise(enterpriseFeatureClass, countyFeatureClass, EnterpriseWithCountyCsv);            
        }    

        /// <summary>
        /// 保存所有的excel的文件名
        /// </summary>
        public List<string> Excels { get; set; }

        // 要写入的 csv 文件 [9/17/2017 17:56:15 mzl]
        private string EnterpriseWithCountyCsv { get; set; }

        /// <summary>
        /// 需要创建所有点的shp，所以需要读取所有的企业点信息
        /// </summary>
        public List<Enterprise> Enterprises = new List<Enterprise>();

        public IFeatureClass countyFeatureClass = null;

        public IFeatureClass enterpriseFeatureClass = null;
    }

    class Enterprise2ConvertedEnterprise
    {
        public List<ConvertedEnterprise> result;
        /// <summary>
        /// 查询shp并进行转换
        /// </summary>
        /// <param name="enterpriseFeatCls">企业点的FeatureClass</param>
        /// <param name="countyFeatCls">指定县域shp的feutureClass，遍历这个shp里的polygon，可以知道哪些企业点在这个polygon里</param>
        /// <param name="csvName">要定入的csv文件</param>
        /// <returns></returns>
        public Enterprise2ConvertedEnterprise(IFeatureClass enterpriseFeatCls, IFeatureClass countyFeatCls, string csvName)
        {            
            result = new List<ConvertedEnterprise>();
            if (enterpriseFeatCls == null || countyFeatCls == null)            
                return;

            const int BATCH_SIZE = 1000;
            try
            {
                int countyIndex = countyFeatCls.Fields.FindField("NAME");

                int idIndex = enterpriseFeatCls.Fields.FindField("ExcelId");
                //int lngIndex = featureClass.Fields.FindField("Lng");
                //int latIndex = featureClass.Fields.FindField("Lat");
                int manIndex = enterpriseFeatCls.FindField("man");
                // 遍历指定县域的polygon shp
                IFeatureCursor countyCursor = Geodatabase.GeodatabaseOp.QuerySearch(countyFeatCls, "", null, false);
                IFeature county = countyCursor.NextFeature();
                while (county != null)
                {
                    string countyName = county.Value[countyIndex].ToString();
                    // 查询所有被这个县覆盖的企业点 [10/15/2017 11:42:52 mzl]
                    IFeatureCursor featureCursor = Geodatabase.GeodatabaseOp.SpatialRelQurey(enterpriseFeatCls, county.Shape,
                        esriSpatialRelEnum.esriSpatialRelContains, null, esriSearchOrder.esriSearchOrderSpatial, null, false);
                    IFeature feature = featureCursor.NextFeature();
                    if (feature != null)
                    {
                        while (feature != null)
                        {
                            string enterpriseId = feature.Value[idIndex].ToString();
                            if (enterpriseId != null && enterpriseId != "")
                            {
                                Enterprise e = new Enterprise(
                                    feature.Value[idIndex].ToString(),
                                    feature.Shape as IPoint,
                                    int.Parse(feature.Value[manIndex].ToString()));
                                result.Add(new ConvertedEnterprise(e, countyName));
                                if (result.Count >= BATCH_SIZE)
                                {
                                    Write2Csv(result, csvName);
                                    result.Clear();
                                }
                            }
                            feature = featureCursor.NextFeature();
                        }
                    }
                    else
                    {
                        Log.Log.Info(string.Format("无法在{0}范围内找到企业点", county.Value[countyIndex].ToString()));
                    }
                                    
                    if (featureCursor != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                        featureCursor = null;
                    }
                    county = countyCursor.NextFeature();
                }
                if (countyCursor != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(countyCursor);
                    countyCursor = null;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将转换后的结果写入 csv 文件
        /// </summary>
        /// <param name="convertedEnterprises"></param>
        private void Write2Csv(List<ConvertedEnterprise> convertedEnterprises, string CsvName)
        {
            if (convertedEnterprises == null)
            {
                Log.Log.Error("参数 convertedEnterprises 为null, 退出");
                throw new Exception("参数 convertedEnterprises 为null, 退出");
            }
            try
            {
                using (FileStream fs = new FileStream(CsvName, FileMode.Append))
                {
                    StreamWriter sw = new StreamWriter(fs);

                    for (int i = 0; i < convertedEnterprises.Count; i++)
                    {
                        sw.WriteLine(convertedEnterprises[i].ToString());
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }

    /*
     *	原有的enterprise格林不符合要求，因为添加一个字段 县名
     *	这个类是一个临时使用的类
     */
    class ConvertedEnterprise
    {
        public ConvertedEnterprise(Enterprise e, string countyName)
        {
            Enterprise = e;
            CountyName = countyName;
        }

        // 原来的 Enterprise [9/17/2017 16:42:29 mzl]
        public Enterprise Enterprise { get; set; }
        // 原来的Enterprise对应的县名 [9/17/2017 16:42:44 mzl]
        public string CountyName { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}", 
                Enterprise.ID, 
                Enterprise.Point.X, 
                Enterprise.Point.Y, 
                Enterprise.man, 
                CountyName);
        }
    }
}
