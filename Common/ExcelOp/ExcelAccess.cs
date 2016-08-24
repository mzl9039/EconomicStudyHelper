using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.Data;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.OleDb;
using OfficeOpenXml.Style.XmlAccess;
using OfficeOpenXml.Style;

namespace Common
{
    /// <summary>
    ///     功能说明：Excel 操作类
    ///     作    者：mzl
    ///     创建日期：2016-2-19
    ///     说   明： 只能处理office 2007及以上版本
    /// </summary>
    public class ExcelAccess : IDisposable
    {
        private ExcelPackage _package = null;
        private string _filePath = null;
        private ExcelWorkbook _workBook = null;
        private ExcelWorksheet _workSheet = null;

        /// <summary>
        /// 当前Sheet
        /// </summary>
        public int SheetIndex { get; private set; }

        /// <summary>
        /// 当前文件名
        /// </summary>
        public string FileName
        {
            get { return _filePath; }
        }

        #region dataTable operation

        /// <summary>
        /// 移除与上一级重复的值
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="colName">列名 如"FWBH" 移除与上一个重复的列值 必须是字符型的列</param>
        /// <returns>返回DataTable</returns>
        public static DataTable RemoveSameColValue(DataTable dt, string colName)
        {
            if (dt != null)
            {
                if (dt.Columns[colName] != null && dt.Columns[colName].DataType == Type.GetType("System.String"))
                {
                    var indexCol = dt.Columns[colName].Ordinal;

                    //处理datatable
                    var lastValue = "";
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr[colName].ToString() == lastValue)
                        {
                            dr[indexCol] = DBNull.Value;
                        }
                        else
                        {
                            lastValue = dr[colName].ToString();
                        }
                    }
                }
            }
            return dt;
        }

        #endregion dataTable operation

        #region private operations

        private void OpenPackage(string filePath)
        {
            _package = new ExcelPackage(new FileInfo(filePath));
            _filePath = filePath;            
            _workBook = _package.Workbook;

            SetCurrentSheet(1);
        }

        private bool CheckPackage()
        {
            if (_package == null)
            {
                System.Diagnostics.Debug.WriteLine("ExcelPackage对象未初始化");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查Sheet
        /// </summary>
        private bool CheckWorkBook()
        {
            if (_workBook == null)
            {
                System.Diagnostics.Debug.WriteLine("ExcelWorkbook对象未初始化");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查Sheet
        /// </summary>
        private bool CheckWorksheet()
        {
            if (_workSheet == null)
            {
                System.Diagnostics.Debug.WriteLine("ExcelWorksheet对象未初始化");
                return false;
            }
            return true;
        }

        #endregion private operations

        #region basic operation

        public ExcelAccess(string filePath)
        {
            SheetIndex = 0;
            OpenPackage(filePath);
        }

        /// <summary>
        ///     保存当前文档
        /// </summary>
        public void Save()
        {
            if (!CheckPackage())
                return;

            _package.Save();
        }

        /// <summary>
        ///     另存当前文档
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAs(string filePath)
        {
            if (!CheckWorkBook())
                return;
            
            _package.SaveAs(new FileInfo(filePath));
        }

        public void SaveAs(string filePath, string password)
        {
            if (!CheckWorkBook())
                return;

            _package.SaveAs(new FileInfo(filePath), password);
        }

        /// <summary>
        ///     关闭当前操作
        /// </summary>
        public void Close()
        {
            //if (_workBook != null)
            //{
            //    _workBook.Dispose();
            //    _workBook = null;
            //}

            //if (_workSheet != null)
            //{
            //    _workSheet.Dispose();
            //    _workSheet = null;
            //}

            if (_package == null)
                return;
            _package.Dispose();
            _package = null;
        }

        /// <summary>
        ///     创建Sheet
        /// </summary>
        /// <param name="sheetName">工作表名称</param>
        public void CreateSheet(string sheetName)
        {
            // check workbook    
            if (!CheckWorkBook())
                return;

            _workBook.Worksheets.Add(sheetName);
        }

        /// <summary>
        ///     设置当前工作Sheet
        /// </summary>
        /// <param name="sheetIndex">Sheet序号(从1开始)</param>
        public void SetCurrentSheet(int sheetIndex)
        {
            if (!CheckWorkBook())
                return;

            SheetIndex = sheetIndex;
            _workSheet = _workBook.Worksheets[sheetIndex];
        }

        public void SetCurrentSheet(string sheetName)
        {
            if (!CheckWorkBook())
                return;

            _workSheet = _workBook.Worksheets[sheetName];
            SheetIndex = _workSheet.Index;            
        }

        /// <summary>
        ///     设置当前工作Sheet并将当前Sheet设为Activate
        /// </summary>
        /// <param name="sheetIndex">Sheet序号(从1开始)</param>
        public void SetCurrentSheetActivate(int sheetIndex)
        {
            SetCurrentSheet(sheetIndex);
            _workSheet.Select();
        }

        /// <summary>
        ///     删除当前工作表
        /// </summary>
        public void DeleteCurrentSheet()
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workBook.Worksheets.Delete(SheetIndex);
            _workSheet = null;
            SheetIndex = -1;
        }

        /// <summary>
        ///     更改当前工作表名称
        /// </summary>
        /// <param name="newName"></param>
        public void RenameCurrentSheet(string newName)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.Name = newName;
        }

        /// <summary>
        ///     获取数量
        /// </summary>
        /// <returns></returns>
        public int GetSheetCount()
        {
            // check workbook    
            if (!CheckWorkBook())
                return -1;

            return _workBook.Worksheets.Count;
        }

        /// <summary>
        /// 从Excel文本读取文件内批量导入的信息
        /// </summary>
        /// <param name="filePath">文本路径</param>
        /// <returns></returns>
        public DataSet ReadExcel(string filePath)
        {
            DataSet excel_ds = new DataSet();
            DataTable dt = ReadExceltoDataTable(filePath, true);
            if (dt != null)
                excel_ds.Tables.Add(dt);

            return excel_ds;
        }

        /// <summary>
        /// 将excel数据读取到datatabel中
        /// </summary>
        /// <param name="filePath">excel路径</param>
        /// <param name="header">excel第一行作为数据行（false）还是作为列名（true）</param>
        /// <returns></returns>
        public DataTable ReadExceltoDataTable(string filePath, bool header)
        {
            var dt = new DataTable();
            using (ExcelPackage excel = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = excel.Workbook.Worksheets.First();
                if (sheet == null)
                    return null;

                if (header)
                {
                    foreach (var cell in sheet.Cells[1, 1, 1, sheet.Dimension.End.Column])
                    {
                        dt.Columns.Add(cell.Value.ToString());
                    }
                }
                
                var rows = sheet.Dimension.End.Row;
                // 若第一行作为数据库行，则从第一行开始，否则从第二行开始读取 [3/1/2016 mzl]
                var firstRow = header ? 2 : 1;
                for (var i = firstRow; i <= rows; i++)
                {
                    var row = sheet.Cells[i, 1, i, sheet.Dimension.End.Column];
                    dt.Rows.Add(row.Select(cell => cell.Value).ToArray());
                }
                return dt;
            }
        }

        /// <summary>
        /// 从Excel文本读取文件内批量导入的信息
        /// </summary>
        /// <param name="filePath">文本路径</param>
        /// <returns></returns>
        public DataSet ReadJZDExcel(string filePath)
        {
            //从第一行开始读取，第一行不当列名
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=No;IMEX=1;'";
            //string strExcel = "select * from [sheet1$]"; 
            DataSet excel_ds = new DataSet();

            OleDbConnection Oleconn = new OleDbConnection(strConn);
            OleDbDataAdapter excelCommand = null;

            try
            {
                Oleconn.Open();
                //获取Excel第一张表的表名
                System.Data.DataTable dtTab = Oleconn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string TabName = dtTab.Rows[0][2].ToString().Trim();
                string strExcel = String.Format("select * from [{0}]", TabName);
                excelCommand = new OleDbDataAdapter(strExcel, Oleconn);
                excelCommand.Fill(excel_ds);//得到dataset                
                return excel_ds;
            }
            catch (System.Exception ex)
            {
                return excel_ds;
            }
            finally
            {
                Oleconn.Close();
                Oleconn.Dispose();
            }
        }

        #endregion basic operation

        #region sheet operation

        /// <summary>
        ///     选择数据
        /// </summary>
        /// <param name="str"></param>
        public void RowsSelect(string str)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.Select(str, true);
        }

        public void RowsDelete(int fromRow, int rowLen)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.DeleteRow(fromRow, rowLen);
        }

        public double GetRowHeight(int rowNum)
        {
            // check workbook    
            if (!CheckWorksheet())
                return -1;

            return _workSheet.Row(rowNum).Height;
        }

        /// <summary>
        /// 选择数据
        /// </summary>
        public void CellsSelect(int startTop, int startLeft, int endTop, int endLeft)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.Select(new ExcelAddress(startTop, startLeft, endTop, endLeft));
        }

        /// <summary>
        /// 选择数据
        /// </summary>
        /// <param name="startTop">表格左上角</param>
        /// <param name="startLeft">表格左上角</param>
        /// <param name="endTop">end右下角</param>
        /// <param name="endLeft">end右下角</param>
        public void CellsPrintArea(int startTop, int startLeft, int endTop, int endLeft)
        {
            if (!CheckWorksheet())
                return;

            string str = new ExcelAddress(startTop, startLeft, endTop, endLeft).Address;
            _workSheet.PrinterSettings.PrintArea = _workSheet.Cells[str];
        }

        /// <summary>
        /// 设置打印方向
        /// </summary>
        /// <param name="orientaton">打印方向，0代表纵向，1代表横向</param>
        public void CellsPrintOrientation(int orientaton)
        {
            if (!CheckWorksheet())
                return;

            _workSheet.PrinterSettings.Orientation = (orientaton == 0)
                ? OfficeOpenXml.eOrientation.Portrait
                : OfficeOpenXml.eOrientation.Landscape;
        }

        /// <summary>
        /// 复制单元格
        /// </summary>
        /// <param name="top">源单元格top</param>
        /// <param name="left">源单元格left</param>
        /// <param name="tarTop">目标单元格top</param>
        /// <param name="tarLeft">目标单元格left</param>
        public void RangeCopy(int top, int left, int tarTop, int tarLeft)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[top, left].Copy(_workSheet.Cells[tarTop, tarLeft]);
        }

        /// <summary>
        /// 复制单元格
        /// </summary>
        /// <param name="sourceAddress"></param>
        /// <param name="targetAddress"></param>
        public void RangeCopy(string sourceAddress, string targetAddress)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[sourceAddress].Copy(_workSheet.Cells[targetAddress]);
        }
         


        /// <summary>
        /// 拷贝选择区域
        /// </summary>
        public void CopySelection(string targetAddress)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.SelectedRange.Copy(_workSheet.Cells[targetAddress]);
        }

        /// <summary>
        ///     合并单元格
        /// </summary>
        /// <param name="top">开始单元格top</param>
        /// <param name="left">开始单元格left</param>
        /// <param name="rowLen">行长度</param>
        /// <param name="colLen">列长度</param>
        public void MergeRange(int top, int left, int rowLen, int colLen)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[top, left, top + rowLen, left + colLen].Merge = true;
        }

        public void MergeRange(string range)
        {
            try
            {
                // check workbook    
                if (!CheckWorksheet())
                    return;

                _workSheet.Cells[range].Merge = true;
                _workSheet.Cells[range].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                _workSheet.Cells[range].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            catch (System.Exception ex)
            {
                Log.Log.Error(ex);
                Log.Log.Error("MergeRange:" + range);
            }
        }

        /// <summary>
        ///     合并单元格
        /// </summary>
        /// <param name="startTop">开始单元格top</param>
        /// <param name="startLeft">开始单元格left</param>
        /// <param name="endTop">结束单元格top</param>
        /// <param name="endLeft">结束单元格left</param>
        public void MergeRange2(int startTop, int startLeft, int endTop, int endLeft)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[startTop, startLeft, endTop, endLeft].Merge = true;
        }

        /// <summary>
        /// 合并单元格,并将单元格的格式设置为格式目标单元格
        /// </summary>
        /// <param name="top">开始单元格Top</param>
        /// <param name="left">开始单元格Left</param>
        /// <param name="rowLen">要合并的单元格长度</param>
        /// <param name="colLen">要合并的单元格长度</param>
        /// <param name="rangeVal">合并后填充单元格的内容</param>
        /// <param name="formatRangeTop">格式目标单元格Top</param>
        /// <param name="formatRangeLeft">格式目标单元格Left</param>
        /// <returns>void</returns>
        public void MergeRange(int top, int left, int rowLen, int colLen, string rangeVal, int formatRangeTop,
            int formatRangeLeft)
        {
            // check workbook    
            if (!CheckWorksheet())
                return;

            MergeRange(top, left, rowLen, colLen);
            FormatRange(_workSheet.Cells[top, left, top + rowLen, left + colLen].Address,
                _workSheet.Cells[formatRangeTop, formatRangeLeft].Address);
        }

        /// <summary>
        ///     获得列的字母标示
        /// </summary>
        /// <param name="columnNum">列数</param>
        /// <returns></returns>
        public string GetColLetter(int columnNum)
        {
            if (columnNum == 0)
                return "";

            var colLetter = string.Empty;
            const string colCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var colCharsetLen = colCharset.Length;

            while (columnNum >= 0)
            {
                colLetter = colCharset.Substring(columnNum % colCharsetLen, 1) + colLetter;
                columnNum = columnNum / colCharsetLen - 1;
            }
            //colLetter = colCharset.Substring((columnNum) % colCharsetLen, 1);

            return colLetter;
        }

        /// <summary>
        /// 填充一个合计行
        /// </summary>
        /// <param name="startRow">第一个要进行合计的单元格所在的行号</param>
        /// <param name="startColumn">第一个要进行合计的单元格所在的列号 也是合计行第一个单元格所在的列号</param>
        /// <param name="statRowLength">要合计的行的长度</param>
        /// <param name="columnCount">有多少列要合计</param>
        /// <param name="hjRow">合计行所处的行号</param>
        public void SetHJRow(int startRow, int startColumn, int statRowLength, int columnCount, int hjRow)
        {
            for (var i=0;i<columnCount;i++)
            {
                string formula = string.Format("=SUM({0}:{1})", _workSheet.Cells[startRow, startColumn +i].Address,
                    _workSheet.Cells[startRow + statRowLength -1, startColumn +i].Address);
                _workSheet.Cells[hjRow, startColumn + i].Formula = formula;
            }
        }

        /// <summary>
        ///     填充一个合计列
        /// </summary>
        /// <param name="startRow">第一个要进行合计的单元格所在的行号 也是合计列第一个单元格所在的行号</param>
        /// <param name="startColumn">第一个要进行合计的单元格所在的列号</param>
        /// <param name="statColumnLength">要合计的列的长度</param>
        /// <param name="rowCount">有多少行要合计</param>
        /// <param name="hjColumn">合计列所处的列号</param>
        public void SetHJColumn(int startRow, int startColumn, int statColumnLength, int rowCount, int hjColumn)
        {
            for (var i = 0; i < rowCount; i++)
            {
                string formula = string.Format("=SUM({0}:{1})", _workSheet.Cells[startRow + i, startColumn].Address,
                    _workSheet.Cells[startRow + i, startColumn + statColumnLength - 1].Address);
                _workSheet.Cells[startRow + i, hjColumn].Formula = formula;
            }
        }

        private static bool CheckIsZero(object obj)
        {
            if (obj != null)
            {
                var str = obj.ToString().Trim('0');
                if (string.IsNullOrEmpty(str) || str == ".")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     快速导出DataTable到Excel指定工作表的指定位置
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startTop"></param>
        /// <param name="startLeft"></param>
        public void FastExportToExcel(DataTable dt, int startTop, int startLeft)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return;
            }
            if (!CheckWorksheet())
                return;

            var finalColLetter = GetColLetter(startLeft + dt.Columns.Count - 1);

            // Fast data export to Excel
            var excelRange = string.Format("{0}{1}:{2}{3}", GetColLetter(startLeft), startTop,
                finalColLetter, dt.Rows.Count + startTop - 1);

            _workSheet.Cells[excelRange].LoadFromDataTable(dt, false);
        }

        /// <summary>
        ///     设置Range小数点位数
        /// </summary>
        /// <param name="range"></param>
        /// <param name="precisionNum">小数点位数 0为显示到个数……</param>
        public void SetRangeNumberFormatLocal(string range, int precisionNum)
        {
            if (precisionNum < 0)
                return;

            var format = "0";
            for (var i = 0; i < precisionNum; i++)
            {
                if (i == 0)
                {
                    format += ".0";
                }
                else
                {
                    format += "0";
                }
            }
            format += "_ ";
            _workSheet.Cells[range].Style.Numberformat.Format = format;
        }

        public void SetRangeWrapText(string range)
        {
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[range].Style.WrapText = true;
        }

        /// <summary>
        ///     获取单元格值
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        public object GetCellValue(string cellName)
        {
            if (!CheckWorksheet())
                return null;

            return _workSheet.Cells[cellName].Value;
        }

        /// <summary>
        /// 获取单元格值
        /// </summary>
        /// <param name="top">开始单元格top</param>
        /// <param name="left">开始单元格left</param>
        /// <returns></returns>
        public object GetCellValue(int top, int left)
        {
            if (!CheckWorksheet())
                return null;

            return _workSheet.Cells[top, left].Value;
        }

        public string GetStringValue(string cellName)
        {
            var val = GetCellValue(cellName);
            var result = "";

            if (val != null)
                result = val.ToString();

            return result;
        }

        /// <summary>
        ///     获取单元格值
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        public string GetStringValue(int top, int left)
        {
            var val = GetCellValue(top, left);
            var result = "";

            if (val != null)
                result = val.ToString();

            return result;
        }

        public double GetDoubleValue(string cellName)
        {
            var val = GetCellValue(cellName);
            var result = "";

            if (val != null)
                result = val.ToString();

            var number = 0d;
            number = double.TryParse(result, out number) ? double.Parse(result) : 0d;

            return number;
        }

        /// <summary>
        ///     获取单元格值
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        public double GetDoubleValue(int top, int left)
        {
            var val = GetCellValue(top, left);
            var result = "";

            if (val != null) result = val.ToString();

            var number = 0d;
            number = double.TryParse(result, out number) ? double.Parse(result) : 0d;

            return number;
        }

        /// <summary>
        /// 向Sheet里插入一定数目的行
        /// </summary>
        /// <param name="top">基准单元格距顶端的距离</param>
        /// <param name="rowLen">行数</param>
        public void InsertRowToSheet(int top, int rowLen)
        {
            if (!CheckWorksheet())
                return;

            _workSheet.InsertRow(top, rowLen, top);
        }

        /// <summary>
        /// 向Sheet里插入一定数目的列
        /// </summary>
        /// <param name="top">基准单元格距顶端的距离</param>
        /// <param name="left">基准单元格距左端的距离</param>
        /// <param name="columnLen">列数</param>
        public void InsertColumnToSheet(int top, int left, int columnLen)
        {
            if (!CheckWorksheet())
                return;

            _workSheet.InsertColumn(left, columnLen, left);
        }

        /// <summary>
        /// 设置单元值
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="value"></param>
        public void SetRangeValue(int top, int left, object value)
        {
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[top, left].Value = value;
        }

        /// <summary>
        ///     设置单个单元格值
        /// </summary>
        /// <param name="top">单元格Top</param>
        /// <param name="left">单元格Left</param>
        /// <param name="content">要填的内容</param>
        /// <param name="isWriteZero">零值是否写入（true写入0）</param>
        public void SetRangeValue(int top, int left, object content, bool isWriteZero)
        {
        }

        /// <summary>
        ///     设置单个单元格值
        /// </summary>
        /// <param name="range">单元格</param>
        /// <param name="content">要填的内容</param>
        /// <param name="isWriteZero">零值是否写入（true写入0）</param>
        public void SetRangeValue(string range, object content, bool isWriteZero)
        {
            if (!CheckWorksheet())
                return;

            if (!string.IsNullOrEmpty(content.ToString()))
            {
                _workSheet.Cells[range].Style.Numberformat.Format = "@"; // 文本格式
                if (content.ToString() != "0")
                {
                    _workSheet.Cells[range].Value = content;
                }
                else if (isWriteZero)
                {
                    _workSheet.Cells[range].Value = content;
                }
            }
        }

        /// <summary>
        /// 设置单个单元格值
        /// </summary>
        /// <param name="sRangeName">单元格名称"D3",或者excel中定义好的名称"QSDWMC3"</param>
        /// <param name="content"></param>
        public void SetRangeValue(string sRangeName, object content)
        {
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[sRangeName].Value = content;
        }

        /// <summary>
        /// 设置单元格值，并将其中的值的左部内容设置某种特殊的字体，以此完成特殊符号的插入
        /// 如 R：代表方框里打对号，即选中；v：代表方框里没有对号，即没选中
        /// </summary>
        /// <param name="range"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <param name="fontName"></param>
        public void SetRichTextValue(string range, int start, int length, string value, string fontName)
        {
            if (!CheckWorksheet())
                return;

            if (!string.IsNullOrEmpty(value))
            {
                //_workSheet.Cells[range].Style.Numberformat.Format = "@";

                if (value.Length > 0)
                {
                    _workSheet.Cells[range].RichText.Clear();
                    _workSheet.Cells[range].RichText[0].Text = value.Substring(start, length);
                    _workSheet.Cells[range].RichText[0].FontName = fontName;
                    if (!string.IsNullOrEmpty(value.Substring(start + length)))
                    {
                        _workSheet.Cells[range].RichText.Add(value.Substring(start + length));
                        _workSheet.Cells[range].RichText[1].FontName = "宋体";
                    }
                }
            }
        }

        public void SetRangeBorderThick(string range)
        {
            if (!CheckWorksheet())
                return;

            _workSheet.Cells[range].Style.Border.BorderAround(ExcelBorderStyle.Medium);
        }

        public string GetRange(int top, int left)
        {
            if (!CheckWorksheet())
                return string.Empty;

            return _workSheet.Cells[top, left].Address;
        }

        public string GetRange(int startTop, int startLeft, int rowLength, int columnLength)
        {
            if (!CheckWorksheet())
                return string.Empty;

            return _workSheet.Cells[startTop, startLeft, startTop + rowLength, startLeft + columnLength].Address;
        }

        /// <summary>
        ///     获得单元格
        /// </summary>
        /// <param name="startTop">开始单元格top</param>
        /// <param name="startLeft">开始单元格left</param>
        /// <param name="endTop">结束单元格top</param>
        /// <param name="endLeft">结束单元格left</param>
        public string GetRange2(int startTop, int startLeft, int endTop, int endLeft)
        {
            if (!CheckWorksheet())
                return string.Empty;

            return _workSheet.Cells[startTop, startLeft, endTop, endLeft].Address;
        }

        public void SetRowHeight(int fromRow, int toRow, double Height)
        {
            if (!CheckWorksheet())
                return;

            for (int i = fromRow; i <= toRow; i++)
            {
                _workSheet.Row(i).Height = Height;
            }
        }

        public void SetColumnWidth(int fromColumn, int toColumn, double Width)
        {
            if (!CheckWorksheet())
                return;

            for (int i = fromColumn; i <= toColumn; i++)
            {
                _workSheet.Column(i).Width = Width;
            }
        }

        /// <summary>
        ///     将单元格设置为水平居中和垂直居中
        /// </summary>
        /// <param name="top">单元格距顶端的距离</param>
        /// <param name="left">单元格距左端的距离</param>
        public void SetRangAlignCenter(int top, int left)
        {
            string range = GetRange(top, left);
            SetRangAlignCenter(range);
        }

        /// <summary>
        ///     将单元格设置为水平居中和垂直居中
        /// </summary>
        /// <param name="range"></param>
        public void SetRangAlignCenter(string range)
        {
            if (!string.IsNullOrEmpty(range))
            {
                _workSheet.Cells[range].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                _workSheet.Cells[range].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
        }

        public void SetRangAlignLeft(int top, int left)
        {
            var range = GetRange(top, left);
            SetRangAlignLeft(range);
        }

        /// <summary>
        ///     将单元格设置为水平居左和垂直居中
        /// </summary>
        /// <param name="range"></param>
        public void SetRangAlignLeft(string range)
        {
            if (!string.IsNullOrEmpty(range))
            {
                _workSheet.Cells[range].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                _workSheet.Cells[range].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
        }

        /// <summary>
        ///     替换单元格内容
        /// </summary>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public void ReplaceRangValue(int top, int left, string oldValue, string newValue)
        {
            var range = GetRange(top, left);
            ReplaceRangValue(range, oldValue, newValue);
        }

        /// <summary>
        ///     替换单元格内容
        /// </summary>
        /// <param name="range"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public void ReplaceRangValue(string range, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(range))
                return;

            if (!string.IsNullOrEmpty(newValue) && newValue != "0")
            {
                string strValue = _workSheet.Cells[range].Value.ToString();
                _workSheet.Cells[range].Value = strValue.Replace(oldValue, newValue);
            }
        }

        /// <summary>
        ///     拼接单元格内容(拼接在左边)
        /// </summary>
        /// <param name="top">单元格top</param>
        /// <param name="left">单元格left</param>
        /// <param name="content">要拼接的内容</param>
        public void LeftConcatenateRang(int top, int left, string content)
        {
            var range = GetRange(top, left);
            LeftConcatenateRang(range, content);
        }

        /// <summary>
        ///     拼接单元格内容(拼接在左边)
        /// </summary>
        /// <param name="range">单元格</param>
        /// <param name="content">要拼接的内容</param>
        public void LeftConcatenateRang(string range, string content)
        {
            if (string.IsNullOrEmpty(range))
                return;

            if (!string.IsNullOrEmpty(content) && content != "0")
            {
                _workSheet.Cells[range].Style.Numberformat.Format = "@";
                _workSheet.Cells[range].Value = content + _workSheet.Cells[range].Value;
            }
        }

        /// <summary>
        ///     拼接单元格内容(拼接在右边)
        /// </summary>
        /// <param name="top">单元格top</param>
        /// <param name="left">单元格left</param>
        /// <param name="content">要拼接的内容</param>
        public void RightConcatenateRang(int top, int left, string content)
        {
            var range = GetRange(top, left);
            RightConcatenateRang(range, content);
        }

        /// <summary>
        ///     拼接单元格内容(拼接在右边)
        /// </summary>
        /// <param name="range">单元格</param>
        /// <param name="content">要拼接的内容</param>
        public void RightConcatenateRang(string range, string content)
        {
            if (string.IsNullOrEmpty(range))
                return;

            if (!string.IsNullOrEmpty(content) && content != "0")
            {
                _workSheet.Cells[range].Style.Numberformat.Format = "@";
                _workSheet.Cells[range].Value = _workSheet.Cells[range].Value.ToString() + content;
            }
        }

        /// <summary>
        ///     格式化目标单元格
        /// </summary>
        /// <param name="targetRange">目标单元格</param>
        /// <param name="styleRangeTop">格式目标单元格Top</param>
        /// <param name="styleRangeLeft">格式目标单元格Left</param>
        public void FormatRange(string targetRange, int styleRangeTop, int styleRangeLeft)
        {
            FormatRange(_workSheet.Cells[styleRangeTop, styleRangeLeft].Address, _workSheet.Cells[targetRange].Address);
        }

        /// <summary>
        /// 格式化单元格，由于style不能set，只能一个一个赋值，求好的办法
        /// </summary>
        public void FormatRange(string target, string source)
        {
            if (!CheckWorksheet())
                return;
            
            using (ExcelRange sourceRange = _workSheet.Cells[source])
            {
                using (ExcelRange targetRange = _workSheet.Cells[target])
                {
                    if (sourceRange == null || targetRange == null)
                        return;

                    targetRange.Style.Border = sourceRange.Style.Border;
                    targetRange.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    targetRange.Style.Fill = sourceRange.Style.Fill;
                    targetRange.Style.Font = sourceRange.Style.Font;
                    targetRange.Style.Indent = sourceRange.Style.Indent;
                    targetRange.Style.Numberformat = sourceRange.Style.Numberformat;
                    targetRange.Style.ShrinkToFit = sourceRange.Style.ShrinkToFit;
                    targetRange.Style.TextRotation = sourceRange.Style.TextRotation;
                    targetRange.Style.WrapText = sourceRange.Style.WrapText;
                    targetRange.Style.Hidden = sourceRange.Style.Hidden;
                    targetRange.Style.HorizontalAlignment = sourceRange.Style.HorizontalAlignment;
                    targetRange.Style.Locked = sourceRange.Style.Locked;
                    targetRange.Style.ReadingOrder = sourceRange.Style.ReadingOrder;
                    targetRange.Style.VerticalAlignment = sourceRange.Style.VerticalAlignment;
                }
            }
        }

        #endregion sheet operation

        #region assist methods
        
        /// <summary>
        /// datatable行列转置
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataTable DataTableTranspose(DataTable table)
        {
            DataTable result = new DataTable();
            result.Columns.Add("ColumnName", typeof(string));
            for (int i = 0; i < table.Rows.Count; i++)
            {
                result.Columns.Add("Column" + (i + 1).ToString(), typeof(string));
            }
            foreach (DataColumn dc in table.Columns)
            {
                DataRow drNew = result.NewRow();
                drNew["ColumnName"] = dc.ColumnName;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    drNew[i + 1] = table.Rows[i][dc].ToString();
                }
                result.Rows.Add(drNew);
            }

            return result;
        }

        /// <summary>
        /// 设置行高
        /// </summary>
        /// <param name="location">行值</param>
        /// <param name="height">行高</param>
        public void setRowHeight(int location,int height)
        {
            if (!this.CheckWorksheet())
                return;

            _workSheet.Row(location).Height = height;            
        }
        #endregion assist methods

        public void Dispose()
        {
            if (_workSheet != null)
            {
                _workSheet.Dispose();
                _workSheet = null;
            }

            if (_workBook != null)
            {
                _workBook.Dispose();
                _workBook = null;
            }

            if (_package == null)
                return;
            _package.Dispose();
            _package = null;
        }
    }
}
