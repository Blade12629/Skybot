using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes.Google.SpreadSheets
{
    /// <summary>
    /// Used to submit data to a spreadsheet
    /// </summary>
    public interface ISpreadsheet
    {
        /// <summary>
        /// Updates through a batch update from a given start cell
        /// </summary>
        /// <param name="start">Start cell</param>
        /// <param name="data">[Row][Cell] to update</param>
        /// <returns>Updated cells</returns>
        public void SetData(string start, List<IList<object>> data, string valueInputOption = "USER_ENTERED");

        /// <summary>
        /// Updates data for a specific range
        /// </summary>
        /// <param name="range">Update range</param>
        /// <param name="cells">Cells to update</param>
        /// <returns>Updated cells</returns>
        public void SetData(string range, List<object> cells);
    }

}
