using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Data;

namespace Common
{
    public  class Excel03:IExcelOp
    {
        private  DataTable table;

        public DataTable SetTable
        {
            set { this.table = value; }
        }

        public virtual DataTable ReadExcel(string fileName, int sheetIndex)
        {
            if (fileName == null)
                return null;

            //对于2003版本的excel
            if (fileName.EndsWith(".xls"))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    HSSFWorkbook workbook = new HSSFWorkbook(fs);
                    IRow headerRow = null;
                    ISheet sheet = workbook.GetSheetAt(sheetIndex - 1);
                    if (sheet == null)
                        return null;
                    //在前20行前20列内查找，查找不到则意味着格式不正确
                    for (int rowNum = 0; rowNum < 20; rowNum++)
                    {
                        IRow tempRow = sheet.GetRow(rowNum);
                        //找到包含 “调出单位” 的单元格
                        for (int colNum = 0; colNum < 20; colNum++)
                        {
                            if ((tempRow != null) && (tempRow.GetCell(colNum) != null))
                            {
                                headerRow = sheet.GetRow(rowNum);
                                break;
                            }
                        }
                        if (headerRow != null)
                            break;
                    }

                    //若获取失败，或没有内容，则返回空
                    if (headerRow == null)
                        return null;

                    //遍历所有的非空单元格，获取相应的值
                    for (int i = headerRow.RowNum + 2; i < sheet.LastRowNum - 1; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                            continue;

                        DataRow dataRow = table.NewRow();
                        
                        for (int j = row.FirstCellNum; j < headerRow.LastCellNum; j++)
                        {
                            try
                            {
                                if (row != null && row.GetCell(j) != null)
                                    dataRow[j - row.FirstCellNum] = GetCellValue(row.GetCell(j));
                            }
                            catch (Exception ex)
                            {
                                Log.Log.Error("row.GetCell():" + row.GetCell(j).ToString() + "\r\n" + ex.Message);
                                continue;
                            }                            
                        }

                        table.Rows.Add(dataRow);
                    }

                    workbook = null;
                    sheet = null;
                }

            }
            return table;
        }

        //获取NPOI单元格的值，注意是将值转为string类型再返回
        private  string GetCellValue(ICell cell)
        {
            string cellValue = null;
            switch (cell.CellType)
            {
                case CellType.String:
                    cellValue = cell.StringCellValue.Trim();
                    break;
                case CellType.Numeric:
                    cellValue = cell.NumericCellValue.ToString();
                    break;
                case CellType.Boolean:
                    cellValue = (cell.BooleanCellValue) ? "true" : "false";
                    break;
                case CellType.Formula:
                    cellValue = cell.NumericCellValue.ToString();
                    break;
                default:
                    cellValue = "";
                    break;
            }
            return cellValue;
        }
    }
}
