/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-17
 * 时间: 22:36
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Common;

namespace DataHelper.FuncSet.PopulationStatics
{
	/// <summary>
	/// Description of PopuStatMethod.
	/// </summary>
	public class PopuStatMethod
	{
		public PopuStatMethod(List<Enterprise> enterprises, string filename)
		{
			Enterprises = enterprises;
			Filename = filename;
			m_PopulationStat = new PopulationStat();
		}
		
		public string Filename { get; set; }
		private List<Enterprise> Enterprises { get; set; }
        private PopulationStat m_PopulationStat = null;
        public PopulationStat MyPopulationStat
        {
            get 
            {
                return m_PopulationStat;
            }
            set { m_PopulationStat = value; }
        }
        
        private void SetPopulationTableCount() {
        	MyPopulationStat.TableCount = Enterprises.Count;
        }
        
        public void CaculatePopulation(double tSij, double distance)
        {
            MyPopulationStat.Sij += tSij;
            MyPopulationStat.DistanceSum += distance;
        }
        
        private void GetTotolPopulation()
        {            
        	MyPopulationStat.PopulationSum = Enterprises.Sum(e => e.man);
        }
        
        public void PopulationOver90Stat()
        {
        	Enterprises.Sort((e1, e2) => 
        	{
              	if (e1.man == e2.man)
              		return 0;
              	else
              		return e1.man.CompareTo(e2.man);
        	});   
        	WritePopOver90();
        }        
        
        public void WritePopOver90() {
        	const double limit = 0.9;
            int populationSum = 0;
            FileIOInfo fileIO = new FileIOInfo(Filename);
            if (File.Exists(fileIO.FilePath + @"\0.9就业人口企业.txt"))
            {
                File.Delete(fileIO.FilePath + @"\0.9就业人口企业.txt");                
            }
            using (StreamWriter sw = File.AppendText(fileIO.FilePath + @"\0.9就业人口企业.txt"))
            {
                for (int i = 0; i < Enterprises.Count; i++)
                {
                	populationSum += Enterprises[i].man;
                    if (limit > 1.0 * populationSum / MyPopulationStat.PopulationSum)
                    {
                    	sw.WriteLine("ID:" + Enterprises[i].ID + "\t\t" + "就业人数：" + Enterprises[i].man);
                    }
                }
            }      
        }
	}
}
