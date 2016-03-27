
/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-17
 * 时间: 22:53
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Common
{
    /// <summary>
    /// DistanceFile 是描述距离集合的类，包括当前的文件名，
    /// 当前的距离（每个文件存储某段范围的距离值，范围是一公里长）
    /// 文件内的值的个数，以及这些距离的集合
    /// 原因是：计算的距离值太多，约有500多亿个值，内存不足，所以以一公里为范围缓存在硬盘上
    /// </summary>
    public class DistanceFile
    {
        public DistanceFile(string filename, int distance)
        {
            Filename = filename;
            Distance = distance;
            FileRowCount = 0;
            Distances = new List<double>();
        }

        public DistanceFile(DistanceFile disFile)
        {
            Filename = disFile.Filename;
            Distance = disFile.Distance;
            FileRowCount = disFile.FileRowCount;
            Distances = new List<double>();
            (Distances as List<double>).AddRange(disFile.Distances);
        }

        public void DistanceStoreInMemory(double distance)
        {
            (Distances as List<double>).Add(distance);
        }

        public void Close()
        {
            if (this.SR != null)
                this.SR.Close();

            if (this.SW != null)
                this.SW.Close();

            if (Distances != null)
            {
                (Distances as List<double>).Clear();
                Distances = null;
            }
        }

        public string Filename { get; set; }
        public int Distance { get; set; }
        public int FileRowCount { get; set; }
        // 当前的对象存储的所有的distance
        public IEnumerable<double> Distances { get; set; }

        private StreamReader m_sr = StreamReader.Null;
        public StreamReader SR
        {
            get
            {
                if (m_sr != StreamReader.Null)
                    return m_sr;

                if (string.IsNullOrEmpty(Filename))
                    return m_sr;

                if (m_sw != StreamWriter.Null)
                    m_sw.Close();

                FileIOInfo fileIO = new FileIOInfo(Filename);
                fileIO.CreateFile();

                m_sr = File.OpenText(Filename);
                return m_sr;
            }
        }

        private StreamWriter m_sw = StreamWriter.Null;
        public StreamWriter SW
        {
            get
            {
                if (m_sw != StreamWriter.Null)
                    return m_sw;

                if (string.IsNullOrEmpty(Filename))
                    return m_sw;

                if (m_sr != StreamReader.Null)
                    m_sr.Close();

                FileIOInfo fileIO = new FileIOInfo(Filename);
                fileIO.CreateFile();

                m_sw = File.AppendText(Filename);
                return m_sw;
            }
        }
    }
}
