using System;
using System.Collections.Generic;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using System.IO;
using System.Data;
using LogHelper;

namespace Common
{
     public class Excel07:IExcelOp
    {
         private  DataTable table;

         public DataTable SetTable
         {
             set { this.table = value;}
         }

         public virtual DataTable ReadExcel(string fileName, int sheetIndex)
         {
             try
             {
                 if (fileName.EndsWith(".xlsx"))
                 {
                     using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                     {
                         ExcelPackage package = new ExcelPackage(fs);
                         ExcelWorksheet sheet = package.Workbook.Worksheets[sheetIndex];
                         if (sheet == null)
                             return null;

                         //遍历所有的非空单元格，获取相应的值
                         int minRow, maxRow, minCol, maxCol;
                         minRow = sheet.Dimension.Start.Row;
                         maxRow = sheet.Dimension.End.Row;
                         minCol = sheet.Dimension.Start.Column;      //默认值从1开始，故需要重新设置到第1个非空值
                         maxCol = sheet.Dimension.End.Column;
                         ExcelRow headerRow = null;
                         int colNum = 0;
                         for (int rowNum = minRow; rowNum < maxRow; rowNum++)
                         {
                             for (colNum = minCol; colNum < maxCol; colNum++)
                             {
                                 if (sheet.Cells[rowNum, colNum].Value != null)
                                 {
                                     headerRow = sheet.Row(rowNum);
                                     break;
                                 }
                             }
                             if (headerRow != null)
                                 break;
                         }

                         //此时由于colNum记录的是当前行第一个非空值，故用此值替换minCol的默认值1
                         minCol = colNum;
                         if (headerRow == null)
                             return null;

                         for (int rowNum = headerRow.Row + 1; rowNum <= maxRow; rowNum++)
                         {
                             DataRow dataRow = table.NewRow();
                             for (colNum = minCol; colNum <= maxCol; colNum++)
                             {
                                 try
                                 {
                                     if (sheet.Cells[rowNum, colNum].Value != null)
                                         dataRow[colNum - minCol] = sheet.Cells[rowNum, colNum].Value.ToString().Trim();
                                 }
                                 catch (Exception ex)
                                 {
                                     Log.WriteError("sheet.Cells[rowNum, colNum].Value:" + sheet.Cells[rowNum, colNum].Value.ToString().Trim() + "\r\n" + ex.Message);
                                     continue;
                                 }

                             }
                             table.Rows.Add(dataRow);
                         }
                         headerRow = null;
                         sheet = null;
                         package = null;
                     }
                 }
                 return table;
             }
             catch (Exception ex)
             {
                 Log.WriteError(ex.ToString());
                 //throw ex;
                 return null;
             }
         }
    }
}
