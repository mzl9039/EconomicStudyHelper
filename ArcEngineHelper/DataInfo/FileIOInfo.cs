/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-16
 * 时间: 21:44
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;

namespace DataHelper
{
	/// <summary>
	/// Description of FileIOInfo.
	/// </summary>
	public class FileIOInfo
	{
		public FileIOInfo(string filename)
		{
            FullFileName = filename;
			FilePath = System.IO.Path.GetDirectoryName(filename);
			FileName = System.IO.Path.GetFileName(filename);
			FileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(filename);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filename);
            string[] names = fileInfo.Name.Split('.');
            FileNameWidthoutPath = names[0];
		}
		
        public string FullFileName { get; set; }
		public string FilePath { get; set; }
		public string FileNameWithoutExt { get; set; }
		public string FileName { get; set; }
        public string FileNameWidthoutPath { get; set; }
    }
}
