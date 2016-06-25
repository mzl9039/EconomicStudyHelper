using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections.Concurrent;

namespace DataHelper.FuncSet.EGIndex
{
    public class BaseEGIndex
    {
        public BaseEGIndex(List<string> excels)
        {
            EGIndexEnterpriseData = new ConcurrentDictionary<string, EGEnterpriseData>();

            excels.AsParallel().ForAll(x => {
                FileIOInfo fileIO = new FileIOInfo(x);
                EGIndexEnterpriseData.TryAdd(fileIO.FileNameWithoutExt, new EGEnterpriseData(x));
            });            
        }

        protected void GetCacuResult(EGIndexShpInfo egShp, EGType egType)
        {            
            OutputResult = EGIndexBaseUtil.CaculateSaiAndSi(EGIndexEnterpriseData, egShp.FeatureClassCounty, egType);                      
        }

        public void Close()
        {
            if (EGIndexEnterpriseData != null)
            {
                foreach (var kv in EGIndexEnterpriseData)
                {
                    kv.Value.Close();
                }
                EGIndexEnterpriseData.Clear();
                EGIndexEnterpriseData = null;
            }
        }

        public string OutputFile { get; set; }

        public ConcurrentDictionary<string, EGEnterpriseData> EGIndexEnterpriseData { get; protected set; }

        public List<string> OutputResult { get; set; }
    }

    public enum EGType
    {
        EGIndex = 0,
        EGRobust = 1
    }
}
