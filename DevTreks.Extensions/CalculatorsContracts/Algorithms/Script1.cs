using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTreks.Extensions.Algorithms
{
    /// <summary>
    ///Purpose:		Run scripting language algorithms
    ///Author:		www.devtreks.org
    ///Date:		2018, September
    ///References:	CTA 1, 2, and 3 references
    ///</summary>
    public class Script1 : Calculator1
    {

        public Script1(string[] mathTerms, string[] colNames, string[] depColNames,
            double[] qs, string algorithm, string subAlgorithm,
            CalculatorParameters calcParams, IndicatorQT1 qt1)
            : base()
        {
            _colNames = colNames;
            _depColNames = depColNames;
            _mathTerms = mathTerms;
            _algorithm = algorithm;
            _subalgorithm = subAlgorithm;
            //estimators
            //add an intercept to qs 
            _qs = new double[qs.Count() + 1];
            //1 * b0 = b0
            _qs[0] = 1;
            qs.CopyTo(_qs, 1);
            _params = calcParams;
            //public and by ref back to algos
            meta = new IndicatorQT1(qt1);
        }
        private CalculatorParameters _params { get; set; }
        //all of the the dependent and independent var column names
        private string[] _colNames { get; set; }
        //all of the the dependent var column names including intercept
        private string[] _depColNames { get; set; }
        //corresponding Ix.Qx names (1 less count because no dependent var)
        private string[] _mathTerms { get; set; }
        //corresponding Qx amounts
        private double[] _qs { get; set; }

        private string _algorithm { get; set; }
        private string _subalgorithm { get; set; }

        //output
        public IndicatorQT1 meta = new IndicatorQT1();
        
        public async Task<bool> RunAlgorithmAsync(string inputFilePath, string scriptFilePath, 
            System.Threading.CancellationToken ctk)
        {
            bool bHasCalcs = false;
            try
            {
                meta.ErrorMessage =string.Empty;
                if (string.IsNullOrEmpty(inputFilePath) || (!inputFilePath.EndsWith(".csv")))
                {
                    meta.ErrorMessage ="The dataset file URL has not been added to the Data URL. The file must be stored in a Resource and use a csv file extension.";
                }
                if (string.IsNullOrEmpty(scriptFilePath) || (!scriptFilePath.EndsWith(".txt")))
                {
                    meta.ErrorMessage += "The script file URL has not been added to the Joint Data.The file must be stored in a Resource and use a txt file extension.";
                }
                //unblock after debug
                if (!string.IsNullOrEmpty(meta.ErrorMessage))
                {
                    return bHasCalcs;
                }
                StringBuilder sb = new StringBuilder();
                //get the path to the script executable
                string sScriptExecutable = CalculatorHelpers.GetAppSettingString(
                    this._params.ExtensionDocToCalcURI, "RExecutable");
                if (_algorithm == Calculator1.MATH_TYPES.algorithm3.ToString())
                {
                    //python must be installed to automatically run 
                    sScriptExecutable = CalculatorHelpers.GetAppSettingString(
                        this._params.ExtensionDocToCalcURI, "PyExecutable");
                    if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm1.ToString())
                    {
                        //python scripts must be run by executable as '.pyw' files
                        //save the 'txt' file as a 'pyw' file in temp path
                        //has to be done each time because can't be sure when scriptfile changed last
                        if (!scriptFilePath.EndsWith(".pyw"))
                        {
                            //allow the console to generate error messages
                            string sFileName = Path.GetFileName(scriptFilePath);
                            string sPyScriptFileName = sFileName.Replace(".txt", ".pyw");
                            bool bIsLocalCache = false;
                            string sFilePath = CalculatorHelpers.GetTempDocsPath(
                                _params.ExtensionDocToCalcURI, bIsLocalCache, sPyScriptFileName);
                            bool bHasFile = await CalculatorHelpers.CopyFiles(
                                _params.ExtensionDocToCalcURI, scriptFilePath, sFilePath);
                            scriptFilePath = sFilePath;
                            //216: pythonw.exe stopped accepting urls as input file paths (https redirect?)
                            sFileName = Path.GetFileName(inputFilePath);
                            sFilePath = CalculatorHelpers.GetTempDocsPath(
                                _params.ExtensionDocToCalcURI, bIsLocalCache, sFileName);
                            bool bHasCopied = await CalculatorHelpers.CopyWebFileToFileSystemAsync(
                                inputFilePath, sFilePath);
                            if (!string.IsNullOrEmpty(sFilePath))
                            {
                                inputFilePath = sFilePath;
                            }

                            //pre 216
                            //210: deprecated bottom code in favor of moving it to temp docs path -overcomes localhost:5509 versus 5000 debugging
                            //string sFileName = Path.GetFileName(scriptFilePath);
                            //bool bIsLocalCache = false;
                            //string sTempPath = CalculatorHelpers.GetTempDocsPath(_params.ExtensionDocToCalcURI, bIsLocalCache, sFileName);
                            //pre 210: 
                            //bool bHasFile = CalculatorHelpers.SaveTextInURI(_params.ExtensionDocToCalcURI, sPyScript, sPyPath, out sError);
                            //bool bHasFile = await CalculatorHelpers.CopyFiles(
                            //    _params.ExtensionDocToCalcURI, scriptFilePath, sTempPath);
                            //scriptFilePath = sTempPath;
                            //pre 210
                            //string sPyScript = CalculatorHelpers.ReadText(_params.ExtensionDocToCalcURI, scriptFilePath, out sError);
                            //if (!string.IsNullOrEmpty(sPyScript))
                            //{
                            //    string sFileName = Path.GetFileName(scriptFilePath);
                            //    string sPyScriptFileName = sFileName.Replace(".txt", ".pyw");
                            //    bool bIsLocalCache = false;
                            //    string sPyPath = CalculatorHelpers.GetTempDocsPath(_params.ExtensionDocToCalcURI, bIsLocalCache, sPyScriptFileName);
                            //    bool bHasFile = CalculatorHelpers.SaveTextInURI(_params.ExtensionDocToCalcURI, sPyScript, sPyPath, out sError);
                            //    scriptFilePath = sPyPath;
                            //}
                        }
                    }
                    sb.AppendLine("python results");
                }
                else 
                {
                    if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm1.ToString())
                    {
                        //check for 2.0.2 -R Open can run from a url
                        //rscript.exe can't run from a url 
                        if (scriptFilePath.StartsWith("http"))
                        {
                            //210: started using temp docs path -overcomes localhost:5509 versus 5000 debugging
                            string sFileName = Path.GetFileName(scriptFilePath);
                            bool bIsLocalCache = false;
                            string sTempPath = CalculatorHelpers.GetTempDocsPath(_params.ExtensionDocToCalcURI, bIsLocalCache, sFileName);
                            bool bHasFile = await CalculatorHelpers.CopyFiles(
                                _params.ExtensionDocToCalcURI, scriptFilePath, sTempPath);
                            scriptFilePath = sTempPath;
                        }
                    }
                    //r is default
                    sb.AppendLine("r results");
                }
                List<string> lastLines = new List<string>();
                string sLastLine = string.Empty;
                //2.0.2: algo 2 subalgo2 is r or algo 3 subalgo2 Python; subalgo 2 is virtual machine
                if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm2.ToString())
                {
                    //run on remote servers that have the DevTreksStatsApi WebApi app deployed
                    string sStatType = Data.Helpers.StatScript.STAT_TYPE.r.ToString();
                    if (_algorithm == Calculator1.MATH_TYPES.algorithm3.ToString())
                    {
                        //python is significantly slower than R
                        sStatType = Data.Helpers.StatScript.STAT_TYPE.py.ToString();
                    }
                    else if (_algorithm == Calculator1.MATH_TYPES.algorithm6.ToString())
                    {
                        //julia has not been tested in 2.0.2
                        sStatType = Data.Helpers.StatScript.STAT_TYPE.julia.ToString();
                    }

                    DevTreks.Data.Helpers.StatScript statScript
                       = DevTreks.Data.Helpers.StatScript.GetStatScript(
                            sStatType, scriptFilePath, inputFilePath);
                    string sPlatformType = CalculatorHelpers.GetAppSettingString(
                        this._params.ExtensionDocToCalcURI, "PlatformType");
                    if (sPlatformType.Contains("azure"))
                    {
                        //webapi web domain
                        if (inputFilePath.Contains("localhost"))
                        {
                            statScript.DefaultWebDomain = "https://localhost:5001/";
                        }
                        else
                        {
                            statScript.DefaultWebDomain = "http://devtreksapi1.southcentralus.cloudapp.azure.com/";
                        }
                    }
                    else
                    {
                        if (inputFilePath.Contains("localhost"))
                        {
                            statScript.DefaultWebDomain = "https://localhost:5001/";
                        }
                        else
                        {
                            //run tests on cloud webapi site too
                            statScript.DefaultWebDomain = "http://devtreksapi1.southcentralus.cloudapp.azure.com/";
                        }
                    }
                    //use a console app to post to a webapi CreateClient controller action
                    bool bIsSuccess = await CalculatorHelpers.ClientCreate(statScript);
                    if (bIsSuccess)
                    {
                        if ((!string.IsNullOrEmpty(statScript.StatisticalResult)))
                        {
                            List<string> lines = CalculatorHelpers
                                .GetLinesFromUTF8Encoding(statScript.StatisticalResult);
                            if (lines != null)
                            {
                                if (lines.Count > 0)
                                {
                                    //store the result in the MathResult (or in the MathResult.URL below)
                                    sb.Append(statScript.StatisticalResult);
                                    sLastLine = lines.Last();
                                    int iLastIndex = lines.Count - 1;
                                    //datasets sometime inadvertently include blank end rows
                                    if (string.IsNullOrEmpty(sLastLine)
                                        && iLastIndex > 0)
                                    {
                                        iLastIndex = lines.Count - 2;
                                        if (iLastIndex >= 0)
                                        sLastLine = lines[iLastIndex];
                                    }
                                    if (iLastIndex >= 2)
                                        lastLines.Add(lines[iLastIndex - 2]);
                                    if (iLastIndex >= 1)
                                        lastLines.Add(lines[iLastIndex - 1]);
                                    lastLines.Add(sLastLine);
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((!string.IsNullOrEmpty(statScript.StatisticalResult)))
                        {
                            meta.MathResult += statScript.ErrorMessage;
                        }
                        else
                        {
                            meta.MathResult += "The remote server returned a successful response header but failed to generate the statistical results.";
                        }
                    }
                }
                else
                {
                    //default subalgo1 runs statpackages on the same server
                    lastLines = RunScript(sb, sScriptExecutable, scriptFilePath, inputFilePath);
                }
                if (lastLines.Count > 0)
                {
                    Shared.META_TYPE mType = await Shared.GetMetaType(_params, scriptFilePath);
                    bHasCalcs = await Shared.FillMathResult(meta, mType, 
                        _params, sb, lastLines);
                    if (!bHasCalcs)
                    {
                        meta.MathResult = "The script did not run successfully. Please check the dataset and script. Verify their urls.";
                    }
                }
                else
                {
                    meta.MathResult = "The script did not run successfully. Please check the dataset and script. Verify their urls.";
                }
            }
            catch (Exception ex)
            {
                meta.ErrorMessage += ex.Message;
            }
            return bHasCalcs;
        }
        private List<string> RunScript(StringBuilder sb, string scriptExecutable, 
            string scriptFilePath, string inputFilePath)
        {
            bool bIsPy = scriptFilePath.EndsWith("pyw") ? true : false;
            //run the excecutable as a console app
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = scriptExecutable;
            start.RedirectStandardOutput = true;
            start.UseShellExecute = false;
            if (bIsPy)
            {
                start.Arguments = string.Format("{0} {1}", scriptFilePath, @"C:\DevTreks.2.1.6\wwwroot\resources\network_carbon\resourcepack_526\resource_1771\Regress1.csv");
                //start.Arguments = string.Format("{0} {1} {2}", "py", scriptFilePath, inputFilePath);
            }
            else
            {
                start.Arguments = string.Format("{0} {1}", scriptFilePath, inputFilePath);
            }
            start.CreateNoWindow = true;
            List<string> last3Lines = new List<string>();
            string sLastLine = string.Empty;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    while (!reader.EndOfStream)
                    {
                        sLastLine = reader.ReadLine();
                        last3Lines.Add(sLastLine);
                        sb.AppendLine(sLastLine);
                    }
                }
                process.WaitForExit();
            }
            if (last3Lines.Count > 3)
            {
                last3Lines.RemoveRange(0, last3Lines.Count - 3);
            }
            return last3Lines;
        }
    }
}
