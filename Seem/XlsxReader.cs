using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Mars.Seem
{
    internal class XlsxReader
    {
        private static int GetExcelColumnIndex(string cellReference)
        {
            // cell reference format: column letters (capitalized) followed by row number
            // A-Z: first 26 columns, indices 0-25 = A = 0...Z = 25
            // AA-ZZ: next 676 columns, indices 26-701 = (A-Z + 1) * 26 + A-Z (last index is 26 * 26 + Z)
            // AAA-XFD: remaining columns, indices 702+ = (A-Z + 1) * 26 * 26 + (A-Z + 1) * 26 + A-Z
            if ((cellReference.Length < 2) || (cellReference[0] < 'A') || (cellReference[0] > 'Z'))
            {
                throw new ArgumentOutOfRangeException(nameof(cellReference), "Cell reference '" + cellReference + "' is too short or does not begin with a letter.");
            }

            int firstColumn = cellReference[0] - 'A';
            if ((cellReference[1] >= 'A') && (cellReference[1] <= 'Z')) // availability of cellReference[1] guaranteed by check above
            {
                int secondColumn = cellReference[1] - 'A';
                if ((cellReference[2] >= 'A') && (cellReference[2] <= 'Z')) // for now, assume well formed reference such that cellReference[2] is available
                {
                    int thirdColumn = cellReference[2] - 'A';
                    return 26 * 26 * (firstColumn + 1) + 26 * (secondColumn + 1) + thirdColumn;
                    // as of Excel 2017, the maximum column is XFD; no need to check beyond the first three characters of the cell reference
                }
                else
                {
                    return (firstColumn + 1) * 26 + secondColumn;
                }
            }
            else
            {
                return firstColumn;
            }
        }

        public static void ReadWorksheet(string xlsxFilePath, string worksheetName, Action<int, string[]> parseRow)
        {
            using FileStream stream = new(xlsxFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using SpreadsheetDocument xlsx = SpreadsheetDocument.Open(stream, false);
            WorkbookPart? workbook = xlsx.WorkbookPart;
            if ((workbook == null) || (workbook.Workbook.Sheets == null))
            {
                throw new NotSupportedException("Could not find workbook for worksheet '" + worksheetName + "'. The workbook is null or is missing a sheets part.");
            }

            // read shared strings
            List<string> sharedStrings = new();
            if (workbook.SharedStringTablePart != null)
            {
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
                        if (sharedStringReader.ReadToDescendant(Constant.OpenXml.Element.String, Constant.OpenXml.Namespace) == false)
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
            }

            // read worksheet
            Sheet? worksheetInfo = workbook.Workbook.Sheets.Elements<Sheet>().FirstOrDefault(sheet => String.Equals(sheet.Name, worksheetName, StringComparison.Ordinal));
            if ((worksheetInfo == null) || (worksheetInfo.Id!.Value == null)) // StringValue? confuses VS 16.9.6 nullability checking as it is seen as both nullable and non-nullabe
            {
                throw new XmlException("Worksheet not found or worksheet's ID is missing.");
            }
            WorksheetPart worksheet = (WorksheetPart)workbook.GetPartById(worksheetInfo.Id.Value);

            using Stream worksheetStream = worksheet.GetStream();
            using XmlReader worksheetReader = XmlReader.Create(worksheetStream);
            // match the length of the pre-populated Excel row to the current worksheet
            worksheetReader.MoveToContent();
            if (worksheetReader.ReadToDescendant(Constant.OpenXml.Element.Dimension, Constant.OpenXml.Namespace) == false)
            {
                throw new XmlException("Worksheet dimension element not found.");
            }
            string? dimension = worksheetReader.GetAttribute(Constant.OpenXml.Attribute.Reference);
            if (dimension == null)
            {
                throw new XmlException("Worksheet dimension reference not found.");
            }
            string[] range = dimension.Split(':');
            if ((range == null) || (range.Length != 2))
            {
                throw new XmlException(String.Format("Worksheet dimension reference '{0}' is malformed.", dimension));
            }
            int maximumColumnIndex = XlsxReader.GetExcelColumnIndex(range[1]);

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
                    bool rowHasCellsWithContent = false;
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
                                string? cellReference = rowReader.GetAttribute(Constant.OpenXml.Attribute.CellReference);
                                if (String.IsNullOrEmpty(cellReference))
                                {
                                    throw new NotSupportedException("Missing cell reference.");
                                }

                                // get cell's column
                                // The XML is sparse in the sense empty cells are omitted, so this is required to correctly output
                                // rows.
                                int column = XlsxReader.GetExcelColumnIndex(cellReference);

                                // get cell's value
                                string? cellType = rowReader.GetAttribute(Constant.OpenXml.Attribute.CellType);
                                string valueElement = Constant.OpenXml.Element.CellValue;
                                if (String.Equals(cellType, Constant.OpenXml.CellType.InlineString, StringComparison.Ordinal))
                                {
                                    valueElement = Constant.OpenXml.Element.String;
                                }
                                if (rowReader.ReadToDescendant(valueElement, Constant.OpenXml.Namespace))
                                {
                                    string value = rowReader.ReadElementContentAsString();

                                    if (String.Equals(cellType, Constant.OpenXml.CellType.SharedString, StringComparison.Ordinal))
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

                                    rowHasCellsWithContent |= value.Length > 0;
                                }
                                else
                                {
                                    throw new XmlException("Value element <" + valueElement + "> not found for cell of type t=\"" + cellType + "\".");
                                }
                            }
                            else
                            {
                                rowReader.Read();
                            }
                        }
                    }
                    worksheetReader.ReadEndElement();

                    if (rowAsStrings.Length > 0)
                    {
                        // parse row if it has content, content being defined per above checks as at least one non-empty (put potentially
                        // whitespace) cell
                        if (rowHasCellsWithContent)
                        {
                            parseRow(rowIndex, rowAsStrings);
                        }
                        // reset for next row
                        Array.Clear(rowAsStrings, 0, rowAsStrings.Length);
                    }

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
