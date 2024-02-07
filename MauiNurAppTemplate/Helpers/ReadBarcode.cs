using NurApiDotNet;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static NurApiDotNet.NurApi;

namespace MauiNurAppTemplate.Helpers
{
    public static class Barcode
    {
        /// <summary>
        /// Start barcode reading. Blocked until success barcode read, reading aborted or timeout
        /// </summary>
        /// <param name="nurApi">NurApi handle</param>
        /// <param name="timeOut">Timeout in milliseconds. Barcode must read within 'timeOut'</param>
        /// <returns>Empty string if aborted or timeout. therwise barcode string readed.</returns>
        public static async Task<string> Read(NurApi nurApi, ushort timeOut = 4000)
        {
            ReadBarcode rb = new ReadBarcode(nurApi, timeOut);
            rb.Start();
            return await rb.BarcodeResult;
        }
    }

    /// <summary>
    /// Read barcode. Simple to use.
    /// </summary>   
    /// <remarks>
    /// Simple barcode read.
    /// <example>
    /// <code>
    /// ==========================================
    /// ReadBarcode rb = new ReadBarcode(_nurApi);        
    /// rb.Start();
    /// string barcode = await rb.BarcodeResult;
    /// ==========================================
    /// ReadBarcode rb = new ReadBarcode(_nurApi);
    /// rb.ResultNumericOnly = true;
    /// rb.Start();
    /// string barcode = await rb.BarcodeResult;
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="nurApi">NurApi handle</param>
    /// <param name="timeout">read timeout in milliseconds. Default 4000ms</param>        
    public class ReadBarcode(NurApi nurApi, ushort timeout = 4000)
    {
        private TaskCompletionSource<string>? _completionSource;
        private string _result = "";

        /// <summary>
        /// Start barcode reading.
        /// </summary>
        /// <exception cref="NurApiException"> thrown if error</exception>
        public void Start()
        {
            _result = "";
            nurApi.AccBarcodeStart(timeout);
            nurApi.OnAccBarcodeResult += NurApi_OnAccBarcodeResult;
            nurApi.IOChangeEvent += NurApi_IOChangeEvent;
            _completionSource = new TaskCompletionSource<string>();
        }

        /// <summary>
        /// Cancel barcode reading immediately
        /// </summary>
        public void Cancel()
        {
            nurApi.AccBarcodeCancel();
            StopAndSetResult();
        }

        /// <summary>
        /// All letters will be removed from scanning result
        /// </summary>
        public bool ResultNumericOnly { get; set; } = false;

        /// <summary>
        /// Barcode may contains leading zeroes. (UPC, EAN). Settings this true all leading zeroes are removed from scanning result.
        /// </summary>
        public bool ResultRemoveLeadingZeroes { get; set; } = false;

        /// <summary>
        /// Remove last digit from scanning result. UPC barcodes usually have check digit added to end of barcode.
        /// </summary>
        public bool RemoveLastDigit { get; set; } = false;

        private void NurApi_IOChangeEvent(object sender, NurApi.IOChangeEventArgs e)
        {
            AccessorySensorSource source = (AccessorySensorSource)e.data.source;
            
            try
            {
                if (source == AccessorySensorSource.ButtonTrigger)
                {
                    if (e.data.dir == 0)
                    {
                        Debug.WriteLine("ReadBarcode:Release");
                        try
                        {
                            nurApi.AccBarcodeCancel();
                        }
                        catch (Exception)
                        {
                            //No action required in this case if cancelling fails for some reason.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IOChangeEvent exception=" + ex.Message);
            }
        }

        private void NurApi_OnAccBarcodeResult(object sender, AccBarcodeResult e)
        {
            if (e.status == BarcodeReadStatus.Success)
            {
                HandleResult(e.Barcode);
            }

            StopAndSetResult();
        }

        private void HandleResult(string result)
        {
            //result = result.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            result = Regex.Replace(result, @"[\r\n]+", "");

            if (ResultNumericOnly)
            {
                result = Regex.Replace(result, "[^0-9]+", string.Empty);
            }

            if (ResultRemoveLeadingZeroes)
            {
                result = result.TrimStart(new Char[] { '0' });
            }

            if (RemoveLastDigit)
            {
                result = result.Remove(result.Length - 1, 1);
            }

            _result = result;
        }

        private void StopAndSetResult()
        {
            if (_completionSource != null)
            {
                nurApi.OnAccBarcodeResult -= NurApi_OnAccBarcodeResult;
                nurApi.IOChangeEvent -= NurApi_IOChangeEvent;
                _completionSource.SetResult(_result);
                _completionSource = null;
            }
        }

        public Task<string> BarcodeResult
        {
            get { return _completionSource.Task; }
        }
    }
}
