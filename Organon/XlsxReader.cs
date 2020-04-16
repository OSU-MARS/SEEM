using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Osu.Cof.Ferm
{
    internal class XlsxReader
    {
        private int GetExcelColumnIndex(string cellReference)
        {
            int index = cellReference[0] - 'A';
            if ((cellReference[1] > '9') || (cellReference[1] < '0'))
            {
                index = 26 * index + cellReference[1] - 'A';
                if ((cellReference[2] > '9') && (cellReference[2] < '0'))
                {
                    // as of Excel 2017, the maximum column is XFD; no need to check more than the first three characters of the cell reference
                    index = 26 * index + cellReference[2] - 'A';
                }
            }
            return index;
        }

        public void ReadWorksheet(string xlsxFilePath, string worksheetName, Action<int, string[]> parseRow)
        {
            using FileStream stream = new FileStream(xlsxFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using SpreadsheetDocument xlsx = SpreadsheetDocument.Open(stream, false);
            WorkbookPart workbook = xlsx.WorkbookPart;

            // read shared strings
            List<string> sharedStrings = new List<string>();
            using Stream sharedStringStream = workbook.SharedStringTablePart.GetStream(FileMode.Open, FileAccess.Read);
            using XmlReader sharedStringReader = XmlReader.Create(sharedStringStream);
            sharedStringReader.MoveToContent();
            while (sharedStringReader.EOF == false)
            {
                if (sharedStringReader.NodeType != XmlNodeType.Element)
                {
                    sharedStringReader.Read();
                }
                else if (String.Equals(sharedStringReader.LocalName, Constant.OpenXml.Element.SharedString, StringComparison.Ordinal))
                {
                    if (sharedStringReader.ReadToDescendant(Constant.OpenXml.Element.SharedStringText, Constant.OpenXml.Namespace) == false)
                    {
                        throw new XmlException("Value of shared string not found.");
                    }
                    string value = sharedStringReader.ReadElementContentAsString();
                    sharedStrings.Add(value);
                    sharedStringReader.ReadEndElement();
                }
                else
                {
                    sharedStringReader.Read();
                }
            }

            // read worksheet
            Sheet worksheetInfo = workbook.Workbook.Sheets.Elements<Sheet>().FirstOrDefault(sheet => String.Equals(sheet.Name, worksheetName, StringComparison.Ordinal));
            if (worksheetInfo == null)
            {
                throw new XmlException("Worksheet not found.");
            }
            WorksheetPart worksheet = (WorksheetPart)workbook.GetPartById(worksheetInfo.Id);

            using Stream worksheetStream = worksheet.GetStream();
            using XmlReader worksheetReader = XmlReader.Create(worksheetStream);
            // match the length of the pre-populated Excel row to the current worksheet
            worksheetReader.MoveToContent();
            if (worksheetReader.ReadToDescendant(Constant.OpenXml.Element.Dimension, Constant.OpenXml.Namespace) == false)
            {
                throw new XmlException("Worksheet dimension element not found.");
            }
            string dimension = worksheetReader.GetAttribute(Constant.OpenXml.Attribute.Reference);
            if (dimension == null)
            {
                throw new XmlException("Worksheet dimension reference not found.");
            }
            string[] range = dimension.Split(':');
            if ((range == null) || (range.Length != 2))
            {
                throw new XmlException(String.Format("Worksheet dimension reference '{0}' is malformed.", dimension));
            }
            int maximumColumnIndex = this.GetExcelColumnIndex(range[1]);

            worksheetReader.ReadToNextSibling(Constant.OpenXml.Element.SheetData, Constant.OpenXml.Namespace);
            int rowIndex = 0;
            string[] rowAsStrings = new string[maximumColumnIndex + 1];
            while (worksheetReader.EOF == false)
            {
                if (worksheetReader.NodeType != XmlNodeType.Element)
                {
                    worksheetReader.Read();
                }
                else if (String.Equals(worksheetReader.LocalName, Constant.OpenXml.Element.Row, StringComparison.Ordinal))
                {
                    // read data in row
                    using (XmlReader rowReader = worksheetReader.ReadSubtree())
                    {
                        while (rowReader.EOF == false)
                        {
                            if (rowReader.NodeType != XmlNodeType.Element)
                            {
                                rowReader.Read();
                            }
                            else if (String.Equals(rowReader.LocalName, Constant.OpenXml.Element.Cell, StringComparison.Ordinal))
                            {
                                string cellReference = rowReader.GetAttribute(Constant.OpenXml.Attribute.CellReference);

                                // get cell's column
                                // The XML is sparse in the sense empty cells are omitted, so this is required to correctly output
                                // rows.
                                int column = this.GetExcelColumnIndex(cellReference);

                                // get cell's value
                                bool isSharedString = String.Equals(rowReader.GetAttribute(Constant.OpenXml.Attribute.CellType), Constant.OpenXml.CellType.SharedString, StringComparison.Ordinal);
                                if (rowReader.ReadToDescendant(Constant.OpenXml.Element.CellValue, Constant.OpenXml.Namespace))
                                {
                                    string value = rowReader.ReadElementContentAsString();

                                    if (isSharedString)
                                    {
                                        int sharedStringIndex = 0;
                                        for (int index = 0; index < value.Length; ++index)
                                        {
                                            char character = value[index];
                                            if ((character > '9') || (character < '0'))
                                            {
                                                throw new FormatException("Shared string index '" + value + "' is not an integer greater than or equal to zero.");
                                            }
                                            sharedStringIndex = 10 * sharedStringIndex + character - '0';
                                        }
                                        value = sharedStrings[sharedStringIndex];
                                    }

                                    // capture cell's value in row
                                    rowAsStrings[column] = value;
                                    rowReader.ReadEndElement();
                                }
                                else
                                {
                                    rowReader.Read();
                                }
                            }
                            else
                            {
                                rowReader.Read();
                            }
                        }
                    }
                    worksheetReader.ReadEndElement();

                    // parse row
                    parseRow(rowIndex, rowAsStrings);

                    // reset for next row
                    Array.Clear(rowAsStrings, 0, rowAsStrings.Length);
                    ++rowIndex;
                }
                else
                {
                    worksheetReader.Read();
                }
            }
        }
    }
}
