using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using ExcelModel;
using System.IO;

namespace Lib.Helper
{
    public class ExcelHelper
    {
        public static ICollection<bkIm> Import(string filepath)
        {
            ICollection<bkIm> bilst = new List<bkIm>();
            FileInfo file = new FileInfo(filepath);

            #region read excel

            ExcelPackage package = new ExcelPackage(file);

            ExcelWorksheet sheet = package.Workbook.Worksheets[1];

            #region check excel format

            if (sheet == null)
            {
                return bilst;
            }
            if (!sheet.Cells[1, 1].Value.Equals("序号") ||
                 !sheet.Cells[1, 2].Value.Equals("漫画来源") ||
                 !sheet.Cells[1, 3].Value.Equals("漫画名称") ||
                 !sheet.Cells[1, 4].Value.Equals("漫画地址"))

            {
                return bilst;
            }

            #endregion check excel format

            #region get last row index

            int lastRow = sheet.Dimension.End.Row;
            while (sheet.Cells[lastRow, 1].Value == null)
            {
                lastRow--;
            }

            #endregion get last row index

            #region read datas

            for (int i = 2; i <= lastRow; i++)
            {
                bilst.Add(new bkIm
                {

                    source = sheet.Cells[i, 2].Value == null ? "" : sheet.Cells[i, 2].Value.ToString(),
                    name = sheet.Cells[i, 3].Value == null ? "" : sheet.Cells[i, 3].Value.ToString(),
                    bookurl = sheet.Cells[i, 4].Value == null ? "" : sheet.Cells[i, 4].Value.ToString(),

                });
            }

            #endregion read datas

            #endregion read excel

            return bilst;
        }
    }
}
