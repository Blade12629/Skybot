using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.GoogleAPI.SpreadSheets
{
    public class SpreadSheet : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public string SheetId { get; set; }
        public string Table { get; set; }

        GoogleCredential _credents;
        SheetsService _service;
        
        public SpreadSheet(string sheetId, string table)
        {
            _credents = GoogleCredential.FromJson(Environment.GetEnvironmentVariable("SheetCredents", EnvironmentVariableTarget.Process))
                                        .CreateScoped(SheetsService.Scope.Spreadsheets);

            _service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = _credents,
                ApplicationName = "SkyBot"
            });

            SheetId = sheetId;
            Table = table;
        }

        ~SpreadSheet()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                _service?.Dispose();
                _service = null;
                _credents = null;

                IsDisposed = true;
            }
        }

        /// <summary>
        /// Updates through a batch update from a given start cell
        /// </summary>
        /// <param name="start">Start cell</param>
        /// <param name="data">Rows+Cells to update</param>
        /// <returns>Updated cells</returns>
        public void SetData(string start, List<IList<object>> data, string valueInputOption = "USER_ENTERED")
        {
            SpreadSheetRateLimit.RateLimit.Increment<object>(async () =>
            {
                int updated = await SetDataAsync(start, data, valueInputOption);
                Logger.Log($"Updated {updated} cells");
            });
        }

        /// <summary>
        /// Updates data for a specific range
        /// </summary>
        /// <param name="range">Update range</param>
        /// <param name="cells">Cells to update</param>
        /// <returns>Updated cells</returns>
        public void SetData(string range, List<object> cells)
        {
            SpreadSheetRateLimit.RateLimit.Increment<object>(async () =>
            {
                int updated = await SetDataAsync(range, cells);
                Logger.Log($"Updated {updated} cells");
            });
        }

        /// <summary>
        /// Gets cells within a specific range
        /// </summary>
        /// <param name="start">Start Cell (example: A1)</param>
        /// <param name="end">End Cell (example: B2)</param>
        /// <returns>Rows+Cells, can be null if empty</returns>
        public IList<IList<object>> GetRange(string start, string end)
        {
            bool response = false;
            IList<IList<object>> result = null;

            SpreadSheetRateLimit.RateLimit.Increment<object>(async () =>
            {
                result = await GetRangeAsync(start, end).ConfigureAwait(false);
                response = true;
            });

            while (!response)
                Task.Delay(25).ConfigureAwait(false).GetAwaiter().GetResult();

            return result;
        }

        /// <summary>
        /// Updates through a batch update from a given start cell
        /// </summary>
        /// <param name="start">Start cell</param>
        /// <param name="data">Rows+Cells to update</param>
        /// <returns>Updated cells</returns>
        async Task<int> SetDataAsync(string start, List<IList<object>> data, string valueInputOption = "USER_ENTERED")
        {
            BatchUpdateValuesRequest update = new BatchUpdateValuesRequest()
            {
                Data = new List<ValueRange>()
                {
                    new ValueRange()
                    {
                        Range = $"{Table}!{start}",
                        Values = data
                    }
                },
                ValueInputOption = valueInputOption
            };

            var request = _service.Spreadsheets.Values.BatchUpdate(update, SheetId);
            var response = await request.ExecuteAsync().ConfigureAwait(false);

            return response.TotalUpdatedCells ?? 0;
        }
        
        /// <summary>
        /// Updates data for a specific range
        /// </summary>
        /// <param name="range">Update range</param>
        /// <param name="cells">Cells to update</param>
        /// <returns>Updated cells</returns>
        async Task<int> SetDataAsync(string range, List<object> cells)
        {
            ValueRange data = new ValueRange()
            {
                Range = $"{Table}!{range}",
                Values = new List<IList<object>>()
                {
                    cells
                }
            };

            var request = _service.Spreadsheets.Values.Update(data, SheetId, data.Range);
            var response = await request.ExecuteAsync().ConfigureAwait(false);

            return response.UpdatedCells ?? 0;
        }

        /// <summary>
        /// Gets cells within a specific range
        /// </summary>
        /// <param name="start">Start Cell (example: A1)</param>
        /// <param name="end">End Cell (example: B2)</param>
        /// <returns>Rows+Cells, can be null if empty</returns>
        async Task<IList<IList<object>>> GetRangeAsync(string start, string end)
        {
            var getRequest = _service.Spreadsheets.Values.Get(SheetId, $"{Table}!{start.ToUpper()}:{end.ToUpper()}");
            var response = await getRequest.ExecuteAsync().ConfigureAwait(false);

            return response.Values;
        }
    }
}
