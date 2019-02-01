using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GeometricLayouts.Models;

namespace GeometricLayouts.Controllers
{
    public class CoordinatesController : ApiController
    {
        // Define constants for values in the event our row and col ranges change or the triangle side lengths change in the future
        private const int _SideLength = 10;
        private const char _MinRow = 'A';
        private const char _MaxRow = 'F';
        private const int _MinCol = 1;
        private const int _MaxCol = 12;
        private const int _MinX = 1;
        private const int _MaxX = 60;
        private const int _MinY = 1;
        private const int _MaxY = 60;

        Coordinate coordinates = new Coordinate();

        // This method will return the coordinates for the given Row and Column values
        public IHttpActionResult GetCoordinatesRowCol(string rowString, string colString)
        {
            // Log the URI request
            FileLogger.Instance.WriteLog(Url.Request.RequestUri.ToString());

            if (CalculateCoordinates(rowString, colString))
                FileLogger.Instance.WriteLog(string.Format("Coordinates calculated.  Status code={0}, ({1},{2}) ({3}, {4}) ({5}, {6})", coordinates.Status, coordinates.V1x, coordinates.V1y, 
                    coordinates.V2x, coordinates.V2y, coordinates.V3x, coordinates.V3y));

            else
                FileLogger.Instance.WriteLog(string.Format("Could not calculate the triangle coordinates.  Status code={0}", coordinates.Status));

            return Ok(coordinates);

        }


        // Verify the coordinates are in the acceptable range values
        private bool CoordIsValid(int coord, char xory)
        {
            bool result = false;

            if (xory == 'X')
            {
                if ((coord >= _MinX) && (coord <= _MaxY))
                    result = true;
            }

            else if (xory == 'Y')
            {
                if ((coord >= _MinY) && (coord <= _MaxY))
                    result = true;
            }

            return result;
        }

        // This method will determine the Row and Column values based on a set of 3 points of a triangle.
        public IHttpActionResult GetRowCol(string ptx1, string pty1, string ptx2, string pty2, string ptx3, string pty3)

        {
            int RowOffset;  // Used to determine the row offset for calculating the Y coordinate
            int ColOffset;  // Used to determine the col offset for calculating the X coordinate

            char Row;       // The calculated Row value based on the given coordinates
            int Col;        // The calculated Column value based on the given coordinates

            bool Verified = false;        // Flag used to determine if the calculated Row and Col values are valid
            RowCol rowcol = new RowCol(); // Instance of the data model we will be returning


            // Set up variables for the triangle vertix coordinates.
            int Pt1x;
            int Pt1y;
            int Pt2x;
            int Pt2y;
            int Pt3x;
            int Pt3y;

            try
            {
                // Log the URI request
                FileLogger.Instance.WriteLog(Url.Request.RequestUri.ToString());

                // Try to conver the triangle coordinates from text to integer.
                if (!(Int32.TryParse(ptx1, out Pt1x) && Int32.TryParse(pty1, out Pt1y) && Int32.TryParse(ptx2, out Pt2x) && Int32.TryParse(pty2, out Pt2y) && 
                    Int32.TryParse(ptx3, out Pt3x) && Int32.TryParse(pty3, out Pt3y)))
                                {
                // Converting the triangle coordinates to integer values fail.
                rowcol.Row = "";
                rowcol.Col = "";
                rowcol.Status = -5;

                FileLogger.Instance.WriteLog(string.Format("One or more triangle coordinates are invalid integer values, coordinates= ({0}, {1})  ({2}, {3})  ({4}, {5})" + 
                    ".  Status={6}", ptx1, pty1, ptx2, pty2, ptx3, pty3, rowcol.Status));

                }

                // Valid integer values were provided.
                else
                {
                    // Verify the x and y coordinates are between our allowed ranges.
                    if (!(CoordIsValid(Pt1x, 'X') && CoordIsValid(Pt1y, 'Y') && CoordIsValid(Pt2x, 'X') && CoordIsValid(Pt2y, 'Y') &&
                        CoordIsValid(Pt3x, 'X') && CoordIsValid(Pt3y, 'Y')))
                    {
                        rowcol.Row = "";
                        rowcol.Col = "";
                        rowcol.Status = -4;

                        FileLogger.Instance.WriteLog(string.Format("One or more triangle coordinates are out of range.  X coordinates must be between {0}-{1}.  Y coordinates must be between {2}-{3}.  Status={4}",
                        _MinX, _MaxX, _MinY, _MaxY, rowcol.Status));
                    }

                    else
                    {
                        // This next section of code will determine the minimum y value for the given points, basically the upper most y value
                        if (Pt1y <= Pt2y)
                        {
                            if (Pt1y <= Pt3y)
                            {
                                RowOffset = Pt1y;
                            }

                            else
                            {
                                RowOffset = Pt3y;
                            }
                        }

                        else if (Pt2y <= Pt3y)
                        {
                            RowOffset = Pt2y;
                        }

                        else
                        {
                            RowOffset = Pt3y;
                        }

                        // Determine the Row based on our RowOffset value (which is based on the upper most y value)
                        Row = Convert.ToChar(_MinRow + (RowOffset / _SideLength));

                        // This next section will determine the minimum x value for the given points so we can determine the left most x value
                        if (Pt1x <= Pt2x)
                        {
                            if (Pt1x <= Pt3x)
                            {
                                ColOffset = Pt1x;
                            }

                            else
                            {
                                ColOffset = Pt3x;
                            }
                        }

                        else if (Pt2x <= Pt3x)
                        {
                            ColOffset = Pt2x;
                        }

                        else
                        {
                            ColOffset = Pt3x;
                        }

                        // Calculate the initial Col value for the left most triangle in the column.
                        Col = ColOffset / _SideLength * 2 + 1;

                        // Because the columns are divided diagonaly we need to determine which of the 2 triangles we are associated with
                        // the left most or the right most.  This if section will determine if we are in the right most triangle and if so,
                        // we need to add one to our Col value. 
                        if (Pt1x == Pt2x)
                        {
                            if (Pt1x > Pt3x)
                                Col++;
                        }

                        else if (Pt1x == Pt3x)
                        {
                            if (Pt1x > Pt2x)
                                Col++;
                        }

                        else if (Pt2x == Pt3x)
                        {
                            if (Pt2x > Pt1x)
                                Col++;
                        }

                        // The row and col values have been calculated, but because each triangle has a unique set of points, 
                        // we can use the calculated Row and Col values to input them into the CalculatedCoordinates function to verify 
                        // the returned points from the function match the give points.
                        if (CalculateCoordinates(Row.ToString(), Col.ToString()))
                        {
                            //The function successfully returned a set of points based on the Row and Col values we provided.  
                            // With the calculated points from the function and the points provided in our API call, make sure the 
                            // two set of triangle points match exactly. Verify all points in the two given sets match.
                            if ((coordinates.V1x == Pt1x) && (coordinates.V1y == Pt1y))
                            {
                                if ((coordinates.V2x == Pt2x) && (coordinates.V2y == Pt2y) && (coordinates.V3x == Pt3x) && (coordinates.V3y == Pt3y))
                                {
                                    Verified = true;
                                }

                                else if ((coordinates.V2x == Pt3x) && (coordinates.V2y == Pt3y) && (coordinates.V3x == Pt2x) && (coordinates.V3y == Pt2y))
                                {
                                    Verified = true;
                                }
                            }

                            if ((coordinates.V2x == Pt1x) && (coordinates.V2y == Pt1y))
                            {
                                if ((coordinates.V1x == Pt2x) && (coordinates.V1y == Pt2y) && (coordinates.V3x == Pt3x) && (coordinates.V3y == Pt3y))
                                {
                                    Verified = true;
                                }

                                else if ((coordinates.V1x == Pt3x) && (coordinates.V1y == Pt3y) && (coordinates.V3x == Pt2x) && (coordinates.V3y == Pt2y))
                                {
                                    Verified = true;
                                }
                            }

                            if ((coordinates.V3x == Pt1x) && (coordinates.V3y == Pt1y))
                            {
                                if ((coordinates.V1x == Pt2x) && (coordinates.V1y == Pt2y) && (coordinates.V2x == Pt3x) && (coordinates.V2y == Pt3y))
                                {
                                    Verified = true;
                                }

                                else if ((coordinates.V1x == Pt3x) && (coordinates.V1y == Pt3y) && (coordinates.V2x == Pt2x) && (coordinates.V2y == Pt2y))
                                {
                                    Verified = true;
                                }
                            }

                            // The calculated points have been verifed, return the values
                            if (Verified)
                            {
                                rowcol.Row = Row.ToString();
                                rowcol.Col = Col.ToString();
                                rowcol.Status = 0;
                                FileLogger.Instance.WriteLog(string.Format("Row and column were calculated.  Status={0} Row={1}, Column={2}", rowcol.Status, rowcol.Row, rowcol.Col));
                            }

                            // The calculated points do not match the points returned by the function, return an error Status value
                            else
                            {
                                rowcol.Row = "";
                                rowcol.Col = "";
                                rowcol.Status = -2;
                                FileLogger.Instance.WriteLog(string.Format("Could not determine the row and column values.  Status={0}", rowcol.Status));
                            }
                        }

                        // Could not determine the triangle coordinates based on our calculated row and col values.
                        else
                        {
                            rowcol.Row = "";
                            rowcol.Col = "";
                            rowcol.Status = -3;

                            FileLogger.Instance.WriteLog(string.Format("Could not verify calculated row and column.  Status={0}", rowcol.Status));
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                rowcol.Row = "";
                rowcol.Col = "";
                rowcol.Status = -1;
                FileLogger.Instance.WriteLog(string.Format("An error has occured.  {0}.", ex));
            }
            
            return Ok(rowcol);
        }


        // This function will calculate the triangles points based on a given row and col value.  If the
        // points were successfully calculated then return true, otherwise false.
        public bool CalculateCoordinates(string rowString, string colString)
        {
            bool Result = false;

            int ColNum;     
            char RowNum;

            int xOffset;    // The x value of the upper-left point of the triangle
            int yOffset;    // The y value of the upper-left point of the triangle

            coordinates.Status = -1;
            coordinates.V1x = 0;
            coordinates.V1y = 0;

            coordinates.V2x = 0;
            coordinates.V2y = 0;

            coordinates.V3x = 0;
            coordinates.V3y = 0;

            try
            {
                RowNum = Convert.ToChar(rowString.ToUpper());
                ColNum = Convert.ToInt32(colString);

                // Verify the row and column values are in the expected ranges
                if ((RowNum >= _MinRow) && (RowNum <= _MaxRow) && (ColNum >= _MinCol) && (ColNum <= _MaxCol))
                {
                    // The calculated xOffset and yOffset will be the x and y coordinates for the upper-left most point of the triangle
                    // Determine the yOffset based on the given row letter based on the _MinRow being the starting letter.
                    yOffset = (RowNum - _MinRow) * _SideLength;

                    // If the modulus of the given column number by 2 is 0 then we are in the "right most"
                    // triangle in the column
                    if (ColNum % 2 == 0)
                        xOffset = (ColNum / 2 - 1) * _SideLength;

                    // We are in the "left most" triangle in the column
                    else
                        xOffset = ((ColNum - 1) / 2) * _SideLength;

                    // Now that we have the x and y coordinates for the upper-left most point of the triangle
                    // we can calculated the other points based on the _SideLength value

                    // We are in the "left most" triangle...
                    if (ColNum % 2 == 1)
                    {
                        coordinates.V1x = xOffset + 1;
                        coordinates.V1y = yOffset + 1;

                        coordinates.V2x = coordinates.V1x;
                        coordinates.V2y = coordinates.V1y + _SideLength - 1;

                        coordinates.V3x = coordinates.V2x + _SideLength - 1;
                        coordinates.V3y = coordinates.V2y;
                    }

                    // We are in the "right most" triangle...
                    else
                    {
                        coordinates.V1x = xOffset + 1;
                        coordinates.V1y = yOffset + 1;

                        coordinates.V2x = coordinates.V1x + _SideLength - 1;
                        coordinates.V2y = coordinates.V1y;

                        coordinates.V3x = coordinates.V2x;
                        coordinates.V3y = coordinates.V2y + _SideLength - 1;
                    }

                    coordinates.Status = 0;
                    Result = true;
                }

                // The given row and column are outside our range(s) return an error status code
                else
                {
                    coordinates.Status = -1;
                    FileLogger.Instance.WriteLog(string.Format("Row and/or column values out of range.  Row={0} (Range: {1}-{2}), Column={3} (Range: {4}-{5})",
                        RowNum, _MinRow, _MaxRow, ColNum, _MinCol, _MaxCol));
                }
 
            }

            catch (Exception ex)
            {
                coordinates.Status = -1;
                FileLogger.Instance.WriteLog(string.Format("An error has occured.  {0}.", ex));
            }

            return Result;
        }
    }
}