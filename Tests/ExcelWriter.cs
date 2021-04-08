using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
//using Excel = Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using System.IO;

//the purpose of this class is to export test data to excel, where it can be preserved and graphed.
namespace Tests
{
    class ExcelWriter
    {
        ExcelPackage xlPackage; //object model for excel
        ExcelWorksheet workSheet; //represents the spreadsheet within the application
        object empty = System.Reflection.Missing.Value;

        private string fileName;
        private int rowIndex = 1; //how far down the next entry must be written

        public ExcelWriter(string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;


            this.fileName = fileName;

            //create Excel things to be worked on
            xlPackage = new ExcelPackage(); //create insance of excel 
            workSheet = xlPackage.Workbook.Worksheets.Add(fileName); //this is the actual table I want to write to

            //now write the headers
            workSheet.Cells[rowIndex, 1].Value = "Key Length";
            workSheet.Cells[rowIndex, 2].Value = "Data Length";
            workSheet.Cells[rowIndex, 3].Value = "Time";
            workSheet.Cells[rowIndex, 4].Value = "<Time data> -->";
            rowIndex++; //increment index so next writes to go to row below
        }

        public void addRow(int keyLength, int dataLength, TestResult[] results)
        {
            int tests = results.Length; //number of tests results to display
            workSheet.Cells[rowIndex, 1].Value = keyLength;
            workSheet.Cells[rowIndex, 2].Value = dataLength;
            //formula to average data on that row from column 4 all the way up to the final piece of test data
            workSheet.Cells[rowIndex, 3].Formula = $"=AVERAGE(D{rowIndex}:{colName(tests+4)}{rowIndex})";

            int columnIndex = 4; //start inserting data from the 4th column
            foreach (TestResult result in results) //iterate over results and put them into the table
            {
                workSheet.Cells[rowIndex, columnIndex].Value = result.totalTime;
                columnIndex++;
            }
            rowIndex++;
        }

        //converts column number to letter
        private string BadcolChar(int column)
        {
            //e.g. column 1 == column A. 64+1 = 65, which is A in ASCII. this doesn't work beyond Z, but it won't go that high
            char character = (char)(64 + column);
            return character + "";
        }

        // this was taken from https://stackoverflow.com/questions/181596/how-to-convert-a-column-number-e-g-127-into-an-excel-column-e-g-aa
        private string colName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        /// <summary>
        /// Saves the current Excel spreadsheet in working directory with the name provided to the constructor.
        /// Releases unmanaged memory associated with Excel.
        /// </summary>
        public void saveAndRelease()
        {
            FileInfo file = new FileInfo(fileName+".xlsx");
            if(file.Exists)
            {
                file.Delete(); //replace existing file
            }
            workSheet.Cells.AutoFitColumns();
            xlPackage.SaveAs(file);
        }
    }
}
