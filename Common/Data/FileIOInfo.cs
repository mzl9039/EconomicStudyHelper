/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-16
 * 时间: 21:44
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.IO;

namespace Common
{
	/// <summary>
	/// Description of FileIOInfo.
	/// </summary>
	public class FileIOInfo
	{
		public FileIOInfo(string filename)
		{
			FullName = filename;
			FilePath = System.IO.Path.GetDirectoryName(filename);
			FileName = System.IO.Path.GetFileName(filename);
			FileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(filename);
		}
		
		public void CreateDirectory() {
			if (FilePath != null && !Directory.Exists(FilePath))
				Directory.CreateDirectory(FilePath);
		}
		
		public void CreateFile() {
			CreateDirectory();
			if (FullName != null && !File.Exists(FullName)) {
				FileStream fs = File.Create(FullName);
				fs.Close();
			}
		}
		
		public string FullName { get; set; }
		public string FilePath { get; set; }
		public string FileNameWithoutExt { get; set; }
		public string FileName { get; set; }
	}
}
