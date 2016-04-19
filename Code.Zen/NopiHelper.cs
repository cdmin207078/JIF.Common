// ----------------------------------------------------------------
// Copyright (C) JIF
// 版权所有。
//
// 文件名：NpoiExcelHelper.cs
// 文件功能描述：基于Npoi的Excel 操作帮助类
//
// 
// 创建标识：chenning20150516 
//
// ----------------------------------------------------------------
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sample.Test;
using System.Dynamic;

namespace Code.Zen
{
    public class NpoiExcelHelper : IExcelHelper
    {
        private IWorkbook _workbook;

        #region ctor

        public NpoiExcelHelper()
        {
            _workbook = new HSSFWorkbook();

            //若要支持2007 格式 单Sheet 超过 62235行数组,则用下方实例化
            //_workbook = new XSSFWorkbook();

            CreateSheet("Sheet1");
        }

        public NpoiExcelHelper(string fileFullName)
        {
            using (var fs = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
            {
                _workbook = WorkbookFactory.Create(fs);
            }
        }

        #endregion

        #region Private

        public ISheet Sheet(int sheetIndex)
        {
            return _workbook.GetSheetAt(sheetIndex);
        }

        public IRow Row(int rowIndex, int sheetIndex = 0)
        {
            return Sheet(sheetIndex).GetRow(rowIndex);
        }

        public ICell Cell(int rowIndex, int CellIndex, int sheetIndex = 0)
        {
            return Row(sheetIndex: sheetIndex, rowIndex: rowIndex).GetCell(CellIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            //return Row(sheetIndex, rowIndex).CreateCell(cellIndex);

            //默认情况，返回空或Blank
            //row.GetCell(2, MissingCellPolicy.RETURN_NULL_AND_BLANK);
            //当单元格为BlankRecord时，也返回类型为Blank的ICell实例；
            //row.GetCell(2, MissingCellPolicy.RETURN_BLANK_AS_NULL);
            //当单元格不存在时，返回类型为Blank的ICell实例；如果存在则返回当前类型
            //row.GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK);
        }

        #endregion

        #region Implements Interface

        public IWorkbook GetWorkBook()
        {
            return _workbook;
        }

        public void CreateSheet(string sheetName)
        {
            _workbook.CreateSheet(sheetName);
        }

        public void CreateRow(int sheetIndex = 0, int rowIndex = 0)
        {
            _workbook.GetSheetAt(sheetIndex).CreateRow(rowIndex);
        }

        public void Write(IList<dynamic> source, int sheetIndex = 0, int rowIndex = 0, int CellIndex = 0)
        {
            if (source == null || source.Count() == 0)
                return;

            for (int i = 0; i < source.Count(); i++)
            {
                int col = 0;
                foreach (var initem in source[i])
                {
                    Write(initem.Value, sheetIndex, rowIndex + i, CellIndex + col);
                    col++;
                }
            }
        }

        public void Write<T>(T[] source, int sheetIndex = 0, int rowIndex = 0, int CellIndex = 0)
        {
            if (source == null || source.Length == 0)
                return;

            for (int i = 0; i < source.Length; i++)
            {
                Write(source[i], sheetIndex, rowIndex, CellIndex + i);
            }
        }

        public void Write<T>(T[,] source, int sheetIndex = 0, int rowIndex = 0, int CellIndex = 0)
        {
            if (source == null || source.Length == 0)
                return;

            var row = source.GetLength(0);
            var col = source.GetLength(1);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Write(source[i, j], sheetIndex, rowIndex + i, CellIndex + j);
                }
            }
        }

        public void Write<T>(IList<T> source, int sheetIndex = 0, int rowIndex = 0, int CellIndex = 0)
        {
            if (source == null || source.Count() == 0)
                return;

            var type = typeof(T);

            if (type == typeof(ValueType) || type == typeof(string))
            {
                Write(source.ToArray());
            }
            else
            {
                var props = typeof(T).GetProperties();
                for (int i = 0; i < source.Count(); i++)
                {
                    for (int j = 0; j < props.Length; j++)
                    {
                        Write(props[j].GetValue(source[i], null), sheetIndex, rowIndex + i, CellIndex + j);
                    }
                }
            }
        }

        public void Write<T>(T value, int rowIndex, int CellIndex)
        {
            Write(value, 0, rowIndex, CellIndex);
        }

        public void Write<T>(T value, int sheetIndex, int rowIndex, int CellIndex)
        {
            if (Row(rowIndex, sheetIndex) == null)
                CreateRow(sheetIndex: sheetIndex, rowIndex: rowIndex);

            var currentCell = Cell(sheetIndex: sheetIndex, rowIndex: rowIndex, CellIndex: CellIndex);

            if (value != null)
            {
                var tp = value.GetType();

                if (tp == typeof(decimal) || tp == typeof(double) || tp == typeof(float) || tp == typeof(int))
                {
                    currentCell.SetCellValue(Convert.ToDouble(value));
                }
                //else if (tp == typeof(DateTime))
                //{
                //    //IDataFormat format = _workbook.CreateDataFormat();
                //    //currentCell.CellStyle.DataFormat = format.GetFormat("yyyy-MM-dd HH:mm:ss");

                //    currentCell.CellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("yyyy-MM-dd HH:mm:ss");
                //    currentCell.SetCellValue(Convert.ToDateTime(value));
                //}
                else
                {
                    currentCell.SetCellValue(value.ToString());
                }
            }
            else
            {
                currentCell.SetCellValue("");
            }
        }

        public void Export(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    _workbook.Write(fs);
                }
            }
            _workbook = null;
        }

        public T Read<T>(string filePath, int sheetIndex, int rowIndex, int CellIndex)
        {
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// 读取Excel数据至DataTable
        ///// </summary>
        ///// <remarks>sxh20150709#4074</remarks>
        ///// <param name="workbook"></param>
        ///// <param name="sheetIndex"></param>
        ///// <returns></returns>
        //private DataTable Read(IWorkbook workbook, int sheetIndex)
        //{
        //    if (workbook == null)
        //    {
        //        throw new Exception("没有读取到有效Excel数据");
        //    }

        //    if (workbook.NumberOfSheets < sheetIndex + 1)
        //    {
        //        throw new Exception("指定Sheet索引值超出当前Excel拥有Sheet数");
        //    }

        //    var sheet = workbook.GetSheetAt(0);
        //    DataTable dt = new DataTable();

        //    //默认，第一行是字段
        //    IRow headRow = sheet.GetRow(0);

        //    //设置datatable字段
        //    for (int i = headRow.FirstCellNum, len = headRow.LastCellNum; i < len; i++)
        //    {
        //        dt.Columns.Add(headRow.Cells[i].StringCellValue.Trim());
        //    }
        //    //遍历数据行
        //    for (int i = (sheet.FirstRowNum + 1), len = sheet.LastRowNum + 1; i < len; i++)
        //    {
        //        IRow tempRow = sheet.GetRow(i);
        //        DataRow dataRow = dt.NewRow();

        //        //遍历一行的每一个单元格
        //        for (int r = 0, j = headRow.FirstCellNum, len2 = headRow.LastCellNum; j < len2; j++, r++)
        //        {

        //            ICell cell = tempRow.GetCell(j);


        //            if (cell != null)
        //            {
        //                dataRow[r] = cell.ToString();

        //                /*
        //                switch (cell.CellType)
        //                {
        //                    case CellType.String:
        //                        dataRow[r] = cell.StringCellValue;
        //                        break;
        //                    case CellType.Numeric:
        //                        dataRow[r] = cell.NumericCellValue;
        //                        break;
        //                    case CellType.Boolean:
        //                        dataRow[r] = cell.BooleanCellValue;
        //                        break;
        //                    default: dataRow[r] = "error";
        //                        break;
        //                }
        //                 * */
        //            }
        //            else
        //            {
        //                dataRow[r] = DBNull.Value;
        //            }

        //        }
        //        dt.Rows.Add(dataRow);
        //    }
        //    return dt;
        //}

        public IList<T> ReadList<T>(string filePath, int sheetIndex, int rowIndex, int CellIndex, int endRowIndex, int endCellIndex)
        {
            throw new NotImplementedException();
        }

        public void SetStyle(ICellStyle style, int rowIndex, int sheetIndex = 0)
        {
            var row = Row(sheetIndex: sheetIndex, rowIndex: rowIndex);
            foreach (var item in row.Cells)
            {
                item.CellStyle = style;
            }
        }

        public void SetStyle(ICellStyle style, int rowIndex, int CellIndex, int sheetIndex = 0)
        {
            Cell(sheetIndex: sheetIndex, rowIndex: rowIndex, CellIndex: CellIndex).CellStyle = style;
        }

        #endregion


        /// <summary>
        /// <![CDATA[读取Excel数据为IList<Dynamic>数据]]>
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <returns></returns>
        public List<dynamic> ReadAsDynamicList(string filePath)
        {
            IWorkbook workbook = null;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                workbook = WorkbookFactory.Create(fs);
            }

            if (workbook == null)
            {
                throw new Exception("没有读取到有效Excel数据");
            }

            var sheet = workbook.GetSheetAt(0);
            List<dynamic> result = new List<dynamic>();

            //默认，第一行 为 属性
            IRow headRow = sheet.GetRow(0);

            //遍历数据行
            for (int i = (sheet.FirstRowNum + 1), len = sheet.LastRowNum + 1; i < len; i++)
            {
                dynamic dyData = new ExpandoObject();
                var DicdyData = dyData as IDictionary<string, object>;

                IRow dataRow = sheet.GetRow(i);

                //遍历一行的每一个单元格
                for (int r = 0, j = headRow.FirstCellNum, len2 = headRow.LastCellNum; j < len2; j++, r++)
                {
                    ICell celHead = headRow.GetCell(j);
                    ICell celData = dataRow.GetCell(j);

                    if (celData != null)
                    {
                        DicdyData[celHead.ToString().Trim()] = celData.ToString();
                    }
                    else
                    {
                        DicdyData[celHead.ToString().Trim()] = null;
                    }
                }

                result.Add(dyData);
            }

            return result;
        }
    }
}
