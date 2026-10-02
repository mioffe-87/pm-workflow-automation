using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Csp.Web.HelperClasses {
	public class ExcelHelper {
		public static bool CreateExcelDocumentAsStream(DataSet ds, string filename, System.Web.HttpResponse Response) {
			try {
				var stream = new System.IO.MemoryStream();
				using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true)) {
					WriteExcelFile(ds, document);
				}
				stream.Flush();
				stream.Position = 0;



				Response.Clear();
				Response.ClearContent();
				Response.BufferOutput = false;
				Response.AppendCookie(new HttpCookie("fileDownload", "true") { Path = "/" });

				Response.AddHeader("content-disposition", "attachment; filename=" + filename);


				Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				byte[] data1 = new byte[stream.Length];
				stream.Read(data1, 0, data1.Length);
				stream.Close();
				Response.BinaryWrite(data1);
				Response.Flush();

				System.Web.HttpContext.Current.Response.Flush();
				System.Web.HttpContext.Current.Response.SuppressContent = true;
				System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();

				return true;
			}
			catch (Exception ex) {
			
				throw ex;

			}
		}

		private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet) {

			spreadsheet.AddWorkbookPart();
			spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

			spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

			WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
			Stylesheet stylesheet = new Stylesheet();
			workbookStylesPart.Stylesheet = stylesheet;

			uint worksheetNumber = 1;
			Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
			foreach (DataTable dt in ds.Tables) {
				//  For each worksheet you want to create
				string worksheetName = dt.TableName;

				WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
				Sheet sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart), SheetId = worksheetNumber, Name = worksheetName };


				sheets.Append(sheet);

				WriteDataTableToExcelWorksheet(dt, newWorksheetPart);

				worksheetNumber++;
			}

			spreadsheet.WorkbookPart.Workbook.Save();
		}

		private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart) {
			OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart, Encoding.ASCII);
			writer.WriteStartElement(new Worksheet());
			writer.WriteStartElement(new SheetData());

			string cellValue = "";


			int numberOfColumns = dt.Columns.Count;
			bool[] IsNumericColumn = new bool[numberOfColumns];
			bool[] IsDateColumn = new bool[numberOfColumns];

			string[] excelColumnNames = new string[numberOfColumns];
			for (int n = 0; n < numberOfColumns; n++)
				excelColumnNames[n] = GetExcelColumnName(n);

			//
			//  Create the Header row in our Excel Worksheet
			//
			uint rowIndex = 1;

			writer.WriteStartElement(new Row { RowIndex = rowIndex });
			for (int colInx = 0; colInx < numberOfColumns; colInx++) {
				DataColumn col = dt.Columns[colInx];
				AppendTextCell(excelColumnNames[colInx] + "1", col.ColumnName, ref writer);
				IsNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32") || (col.DataType.FullName == "System.Double") || (col.DataType.FullName == "System.Single");
				IsDateColumn[colInx] = (col.DataType.FullName == "System.DateTime");
			}
			writer.WriteEndElement();   //  End of header "Row"

			//
			//  Now, step through each row of data in our DataTable...
			//
			double cellNumericValue = 0;
			foreach (DataRow dr in dt.Rows) {
				// ...create a new row, and append a set of this row's data to it.
				++rowIndex;

				writer.WriteStartElement(new Row { RowIndex = rowIndex });

				for (int colInx = 0; colInx < numberOfColumns; colInx++) {
					cellValue = dr.ItemArray[colInx].ToString();
					cellValue = ReplaceHexadecimalSymbols(cellValue);

					// Create cell with data
					if (IsNumericColumn[colInx]) {
						//  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
						//  If this numeric value is NULL, then don't write anything to the Excel file.
						cellNumericValue = 0;
						if (double.TryParse(cellValue, out cellNumericValue)) {
							cellValue = cellNumericValue.ToString();
							AppendNumericCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, ref writer);
						}
					}
					else if (IsDateColumn[colInx]) {
						//  This is a date value.
						DateTime dtValue;
						string strValue = "";
						if (DateTime.TryParse(cellValue, out dtValue))
							strValue = dtValue.ToShortDateString();
						AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), strValue, ref writer);
					}
					else {
						//  For text cells, just write the input data straight out to the Excel file.
						AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, ref writer);
					}
				}
				writer.WriteEndElement(); //  End of Row
			}
			writer.WriteEndElement(); //  End of SheetData
			writer.WriteEndElement(); //  End of worksheet

			writer.Close();
		}
		private static void AppendTextCell(string cellReference, string cellStringValue, ref OpenXmlWriter writer) {
			//  Add a new Excel Cell to our Row 
			writer.WriteElement(new Cell {
				CellValue = new CellValue(cellStringValue),
				CellReference = cellReference,
				DataType = CellValues.String
			});
		}

		private static void AppendNumericCell(string cellReference, string cellStringValue, ref OpenXmlWriter writer) {
			//  Add a new Excel Cell to our Row 
			writer.WriteElement(new Cell {
				CellValue = new CellValue(cellStringValue),
				CellReference = cellReference,
				DataType = CellValues.Number
			});
		}

		private static string ReplaceHexadecimalSymbols(string txt) {
			string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
			return Regex.Replace(txt, r, "", RegexOptions.Compiled);
		}

		//  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
		public static string GetExcelColumnName(int columnIndex) {
			//  eg  (0) should return "A"
			//      (1) should return "B"
			//      (25) should return "Z"
			//      (26) should return "AA"
			//      (27) should return "AB"
			//      ..etc..
			char firstChar;
			char secondChar;
			char thirdChar;

			if (columnIndex < 26) {
				return ((char)('A' + columnIndex)).ToString();
			}

			if (columnIndex < 702) {
				firstChar = (char)('A' + (columnIndex / 26) - 1);
				secondChar = (char)('A' + (columnIndex % 26));

				return string.Format("{0}{1}", firstChar, secondChar);
			}

			int firstInt = columnIndex / 26 / 26;
			int secondInt = (columnIndex - firstInt * 26 * 26) / 26;
			if (secondInt == 0) {
				secondInt = 26;
				firstInt = firstInt - 1;
			}
			int thirdInt = (columnIndex - firstInt * 26 * 26 - secondInt * 26);

			firstChar = (char)('A' + firstInt - 1);
			secondChar = (char)('A' + secondInt - 1);
			thirdChar = (char)('A' + thirdInt);

			return string.Format("{0}{1}{2}", firstChar, secondChar, thirdChar);
		}

	}
}