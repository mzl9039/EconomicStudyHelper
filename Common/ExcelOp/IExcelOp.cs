using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Common
{
     public interface IExcelOp
    {
         DataTable SetTable
         {
             set;
         }
         /// <summary>
         /// 读取Excel文件
         /// </summary>
         /// <param name="fileName">Excel文件名</param>
         /// <param name="sheetName">Excel的Sheet名</param>
         /// <returns></returns>
        DataTable  ReadExcel(string fileName,int sheetIndex);
    }
}
