﻿using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace JIF.Common
{
    public class NpoiExcelHelper
    {
        private IWorkbook _workbook;

        public NpoiExcelHelper(bool IsExcel2007 = false)
        {
            if (IsExcel2007)
            {
                _workbook = new XSSFWorkbook();
            }
            else
            {
                _workbook = new HSSFWorkbook();
            }


            CreateSheet("Sheet1");
        }

        #region Private

        ISheet Sheet(int sheetIndex)
        {
            return _workbook.GetSheetAt(sheetIndex);
        }

        IRow Row(int sheetIndex, int rowIndex)
        {
            return Sheet(sheetIndex).GetRow(rowIndex);
        }

        ICell Cell(int sheetIndex, int rowIndex, int cellIndex)
        {
            return Row(sheetIndex, rowIndex).GetCell(cellIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK);
        }

        #endregion

        public void CreateSheet(string sheetName)
        {
            _workbook.CreateSheet(sheetName);
        }

        public void CreateRow(int sheetIndex, int rowIndex)
        {
            _workbook.GetSheetAt(sheetIndex).CreateRow(rowIndex);
        }

        public void Write<T>(T source, int sheetIndex, int rowIndex, int cellIndex)
        {
            if (Row(sheetIndex, rowIndex) == null)
                CreateRow(sheetIndex, rowIndex);

            var currentCell = Cell(sheetIndex, rowIndex, cellIndex);

            if (source != null)
            {
                var tp = source.GetType();

                if (tp == typeof(decimal) || tp == typeof(double) || tp == typeof(float) || tp == typeof(int))
                {
                    currentCell.SetCellValue(Convert.ToDouble(source));
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
                    currentCell.SetCellValue(source.ToString());
                }
            }
            else
            {
                currentCell.SetCellValue("");
            }
        }

        public void Write<T>(T[] source, int sheetIndex, int rowIndex, int cellIndex)
        {
            if (source == null || source.Length == 0)
                return;

            for (int i = 0; i < source.Length; i++)
            {
                Write(source[i], sheetIndex, rowIndex, cellIndex + i);
            }
        }

        public void Write<T>(T[,] source, int sheetIndex, int rowIndex, int cellIndex)
        {
            if (source == null || source.Length == 0)
                return;

            var row = source.GetLength(0);
            var col = source.GetLength(1);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Write(source[i, j], sheetIndex, rowIndex + i, cellIndex + j);
                }
            }
        }

        /// <summary>
        /// 注意 :
        /// 使用 使用此方法,写入用户自定义实体类型时,会自动根据类型属性名称字母排序数据列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sheetIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="cellIndex"></param>
        public void Write<T>(List<T> source, int sheetIndex, int rowIndex, int cellIndex)
        {
            if (source == null || source.Count() == 0)
                return;

            var type = typeof(T);

            if (type == typeof(ValueType) || type == typeof(string))
            {
                Write(source.ToArray(), sheetIndex, rowIndex, cellIndex);
            }
            else
            {
                var props = typeof(T).GetProperties();
                for (int i = 0; i < source.Count(); i++)
                {
                    for (int j = 0; j < props.Length; j++)
                    {
                        Write(props[j].GetValue(source[i], null), sheetIndex, rowIndex + i, cellIndex + j);
                    }
                }
            }
        }

        public void Write(List<dynamic> source, int sheetIndex, int rowIndex, int cellIndex)
        {
            if (source == null || source.Count() == 0)
                return;

            for (int i = 0; i < source.Count(); i++)
            {
                int col = 0;
                foreach (var initem in source[i])
                {

                    Write(initem.Value, sheetIndex, rowIndex + i, cellIndex + col);
                    col++;
                }
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

        public static List<dynamic> Read(string file, int sheetIndex = 0, int rowIndex = 0, int cellIndex = 0)
        {
            IWorkbook workbook = null;

            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                workbook = WorkbookFactory.Create(fs);
            }

            if (workbook == null)
            {
                throw new Exception(" 没有读取到有效Excel数据 - NpoiExcelHelper.Read ");
            }

            var sheet = workbook.GetSheetAt(sheetIndex);

            List<dynamic> result = new List<dynamic>();

            //遍历数据行
            for (int r = rowIndex; r <= sheet.LastRowNum; r++)
            {
                dynamic dyData = new ExpandoObject();
                var DicdyData = dyData as IDictionary<string, object>;

                IRow row = sheet.GetRow(r);

                //遍历一行的每一个单元格
                for (int c = cellIndex; c < row.LastCellNum; c++)
                {
                    ICell cel = row.GetCell(c);
                    if (cel == null)
                    {
                        DicdyData[Utils.ToNumberSystem26(c + 1)] = null;
                    }
                    else
                    {
                        DicdyData[Utils.ToNumberSystem26(c + 1)] = cel.ToString();
                    }
                }

                result.Add(dyData);
            }

            return result;
        }
    }
}
