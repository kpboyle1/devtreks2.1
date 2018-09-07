﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Errors = DevTreks.Exceptions.DevTreksErrors;

namespace DevTreks.Extensions.Algorithms
{
    /// <summary>
    ///Purpose:		Simple AML script algorithms
    ///Author:		www.devtreks.org
    ///Date:		2018, April
    ///References:	CTA 4 reference
    ///</summary>
    public class Script2 : Calculator1
    {

        public Script2(string[] mathTerms, string[] colNames, string[] depColNames,
            double[] qs, string algorithm, string subalgorithm, CalculatorParameters calcParams)
            : base()
        {
            _colNames = colNames;
            _depColNames = depColNames;
            _mathTerms = mathTerms;
            _algorithm = algorithm;
            _subalgorithm = subalgorithm;
            //estimators
            //add an intercept to qs 
            _qs = new double[qs.Count() + 1];
            //1 * b0 = b0
            _qs[0] = 1;
            qs.CopyTo(_qs, 1);
            //init calculated response 
            _response = string.Empty;
            _params = calcParams;
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
        private string _response { get; set; }

        //output
        //q6 = marginal productivity
        public double QTSlope { get; set; }
        //QTM = predicted y
        public double QTPredicted { get; set; }
        //lower % ci
        public double QTL { get; set; }
        public string QTLUnit { get; set; }
        //upper % ci
        public double QTU { get; set; }
        public string QTUUnit { get; set; }

        //running this truly async returns to UI w/o saving final calcs or an endless wait
        public async Task<bool> RunAlgorithmAsync(string inputFilePath, string scriptFilePath,
            System.Threading.CancellationToken ctk)
        {
            bool bHasCalcs = false;
            try
            {
                //azure emulator gets debugged using 127:0 domain
                //but web service expects real azure dataset:
                //when released, this must be blocked out
                //r and python debug
                //inputFilePath = "https://devtreks1.blob.core.windows.net/resources/network_carbon/resourcepack_1534/resource_7969/Regress1.csv";

                //aml debug
                //inputFilePath = "https://devtreks1.blob.core.windows.net/resources/network_carbon/resourcepack_1534/resource_7961/Ex6R.csv";
                //scriptFilePath = "https://devtreks1.blob.core.windows.net/resources/network_carbon/resourcepack_1534/resource_7971/Regress2.csv";
                //label, date, learningstep, population, q6
                this.ErrorMessage =string.Empty;
                if (string.IsNullOrEmpty(inputFilePath) || (!inputFilePath.EndsWith(".csv")))
                {
                    this.ErrorMessage ="The dataset file URL has not been added to the Data URL. The file must be stored in a Resource and use a txt file extension.";
                }
                if (string.IsNullOrEmpty(scriptFilePath) || (!scriptFilePath.EndsWith(".txt")))
                {
                    if (_algorithm == Calculator1.MATH_TYPES.algorithm4.ToString())
                    {
                        if (!scriptFilePath.EndsWith(".csv"))
                            this.ErrorMessage += "The scoring data file URL has not been added to the Joint Data.The file must be stored in a Resource and use a csv file extension.";
                    }
                    else
                    {
                        this.ErrorMessage += "The file URL has not been added to the Joint Data.The file must be stored in a Resource and use a txt file extension.";
                    }
                }
                if (!string.IsNullOrEmpty(this.ErrorMessage))
                {
                    return bHasCalcs;
                }
                if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm2.ToString())
                {
                    bHasCalcs = await RunSubAlgorithm1or2Async(inputFilePath, scriptFilePath);
                }
                else if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm3.ToString())
                {
                    bHasCalcs = await RunSubAlgorithm3Async(inputFilePath, scriptFilePath);
                }
                else 
                {
                    //r is default
                    bHasCalcs = await RunSubAlgorithm1or2Async(inputFilePath, scriptFilePath);
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage += ex.Message;
            };
            return bHasCalcs;
        }
        public async Task<bool> RunSubAlgorithm1or2Async(string inputFilePath, string rFile)
        {
            bool bHasCalcs = false;
            string sBaseURL =
                "https://ussouthcentral.services.azureml.net/workspaces/d454361ecdcb4ec4b03fa1aec5a7c0e2/services/b10e6b4c4e63438999cc45147bbe006c/jobs";
            if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm2.ToString())
            {
                sBaseURL = "https://ussouthcentral.services.azureml.net/workspaces/d454361ecdcb4ec4b03fa1aec5a7c0e2/services/abd32060dc014d0e8fe1256e0f694daa/jobs";
            }
            string sPlatForm = _params.ExtensionDocToCalcURI.URIDataManager.PlatformType.ToString();
            if (sPlatForm != CalculatorHelpers.PLATFORM_TYPES.azure.ToString())
            {
                sBaseURL = "https://ussouthcentral.services.azureml.net/workspaces/d454361ecdcb4ec4b03fa1aec5a7c0e2/services/b10e6b4c4e63438999cc45147bbe006c/execute?api-version=2.0&details=true";
                if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm2.ToString())
                {
                    sBaseURL = "https://ussouthcentral.services.azureml.net/workspaces/d454361ecdcb4ec4b03fa1aec5a7c0e2/services/abd32060dc014d0e8fe1256e0f694daa/execute?api-version=2.0&details=true";
                }
            }
            //r web service is default
            //temporary and not used
            string sApiKey =
                "fxBeL9LJ3ORm0kW0DtKhT99OfUK6YgBlc59crizYhlxKoEjRd3kuDHvPRuehCQ02VJhPPXcdYTp2pDUynb9gMA==";
            if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm2.ToString())
            {
                //python
                sApiKey =
                    "/bjDNKx4OWdMIQu6CkvWCIhcfUOCTp9jUE9kD7uylwhOYyhVFOqAFA7M75mJjHS6p6jnAhCvFn1jSl678gzPVA==";
            }
            //convert the script file to the script string expected by the algorithm
            List<string> rlines = new List<string>();
            rlines = await CalculatorHelpers.ReadLines(_params.ExtensionDocToCalcURI, rFile);
            this.ErrorMessage += _params.ExtensionDocToCalcURI.ErrorMessage;
            if (rlines == null)
            {
                this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                return bHasCalcs;
            }
            if (rlines.Count == 0)
            {
                this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                return bHasCalcs;
            }
            StringBuilder sbR = new StringBuilder();
            for (int i = 0; i < rlines.Count(); i++)
            {
                sbR.AppendLine(rlines[i]);
            }
            string rScript = sbR.ToString();

            //web server expects to store in temp/randomid/name.csv
            //web service stores in temp blob
            string sOutputDataURL = CalculatorHelpers.GetTempContainerPath("Routput.csv");

            //web service expects urls that start with container names
            //regular rproject file must be stored in JDataURL
            string sInputContainerPath = CalculatorHelpers.GetContainerPathFromFullURIPath("resources", inputFilePath);

            //async wait so that results can be stored in output file location and parsed into string lines
            SetResponse(sBaseURL, sApiKey, sInputContainerPath, sOutputDataURL, rScript).Wait();
            StringBuilder sb = new StringBuilder();
            //if web service successully saved the results, the response will start with Success
            if (_response.StartsWith("Success"))
            {
                //return the output file contents in a string list of lines
                //must convert container path to full path
                string sOutputFullDataURL = string.Concat("https://devtreks1.blob.core.windows.net/", sOutputDataURL);
                List<string> lines = new List<string>();
                //azure emulator can't process real Azure URL so this won't work
                //instead, double check that output url is actually saved
                lines = await CalculatorHelpers.ReadLines(_params.ExtensionDocToCalcURI, sOutputFullDataURL);
                this.ErrorMessage += _params.ExtensionDocToCalcURI;
                //this results in endless wait-try ReadLinesAsync(sOutputDataURL).ConfigureAwait(false)
                //lines = await CalculatorHelpers.ReadLinesAsync(sOutputDataURL);
                if (lines == null)
                {
                    this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                    return bHasCalcs;
                }
                if (lines.Count == 0)
                {
                    this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                    return bHasCalcs;
                }
                sb = new StringBuilder();
                if (_subalgorithm == Calculator1.MATH_SUBTYPES.subalgorithm2.ToString())
                {
                    sb.AppendLine("py results");
                }
                else
                {
                    sb.AppendLine("r results");
                }
                //dep var has to be in the R project 1st column
                string sLine = string.Concat("first variable:  ", _colNames[0]);
                string[] line = new List<string>().ToArray();
                for (int i = 0; i < lines.Count(); i++)
                {
                    line = lines[i].Split(Constants.CSV_DELIMITERS);
                    //lineout[1] = CalculatorHelpers.ConvertStringToDouble(line[0]).ToString("N4", CultureInfo.InvariantCulture);
                    sb.AppendLine(Shared.GetLine(line, false));
                }
                if (this.MathResult.ToLower().StartsWith("http"))
                {
                    bool bHasSaved = await CalculatorHelpers.SaveTextInURI(
                        _params.ExtensionDocToCalcURI, sb.ToString(), this.MathResult);
                    if (!string.IsNullOrEmpty(_params.ExtensionDocToCalcURI.ErrorMessage))
                    {
                        this.MathResult += _params.ExtensionDocToCalcURI.ErrorMessage;
                        //done with errormsg
                        _params.ExtensionDocToCalcURI.ErrorMessage = string.Empty;
                    }
                }
                else
                {
                    this.MathResult = sb.ToString();
                }
                bHasCalcs = true;
                //last line of string should have the QTM vars
                if (line != null)
                {
                    int iPos = 0;
                    if (line[iPos] != null)
                        this.QTPredicted = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                    iPos = 1;
                    if (line[iPos] != null)
                        this.QTL = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                    iPos = 2;
                    if (line[iPos] != null)
                        this.QTU = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                }
            }
            else
            {
                this.ErrorMessage += string.Concat(_response, "The calculations could not be run using the web service.");
            }
            return bHasCalcs;
        }
        
        public async Task<bool> RunSubAlgorithm3Async(string inputFile1Path, string inputFile2Path)
        {
            bool bHasCalcs = false;
            string sBaseURL =
                "https://ussouthcentral.services.azureml.net/workspaces/d454361ecdcb4ec4b03fa1aec5a7c0e2/services/8b1074b465ea4258a11ec48ce64ae257/jobs";
            string sPlatForm = CalculatorHelpers.GetPlatform(_params.ExtensionDocToCalcURI, inputFile1Path);
            if (sPlatForm != CalculatorHelpers.PLATFORM_TYPES.azure.ToString())
            {
                sBaseURL = "https://ussouthcentral.services.azureml.net/workspaces/d454361ecdcb4ec4b03fa1aec5a7c0e2/services/8b1074b465ea4258a11ec48ce64ae257/execute?api-version=2.0&details=true";
            }
            string sApiKey =
                "RO2Ev5dRSKqNJ4jz+zoT0qDntEsKbyizbgZKlhOR2vGztsjBD3S3C8nmIlZI9TbbmCcsw+unwhky1GgZ5qiHmg==";

            //web server expects to store in temp/randomid/name.csv
            //scoring results
            string sOutputData1URL = CalculatorHelpers.GetTempContainerPath("outputdata1.csv");
            //model results
            string sOutputData2URL = CalculatorHelpers.GetTempContainerPath("outputdata2.csv");
            //web service expects urls that start with container names
            string sInput1ContainerPath = CalculatorHelpers.GetContainerPathFromFullURIPath("resources", inputFile1Path);
            string sInput2ContainerPath = CalculatorHelpers.GetContainerPathFromFullURIPath("resources", inputFile2Path);
            //async wait so that results can be stored in output file location and parsed into string lines
            SetResponse2(sBaseURL, sApiKey, sInput1ContainerPath, sInput2ContainerPath, sOutputData1URL, sOutputData2URL).Wait();
            StringBuilder sb = new StringBuilder();
            //if web service successully saved the results, the response will start with Success
            if (_response.StartsWith("Success"))
            {
                //return the output file contents in a string list of lines
                //must convert container path to full path
                string sOutput1FullDataURL = string.Concat("https://devtreks1.blob.core.windows.net/", sOutputData1URL);
                List<string> lines = new List<string>();
                //azure emulator can't process real Azure URL so this won't work
                //instead, double check that output url is actually saved
                lines = await CalculatorHelpers.ReadLines(_params.ExtensionDocToCalcURI, sOutput1FullDataURL);
                this.ErrorMessage += _params.ExtensionDocToCalcURI.ErrorMessage;
                //this results in endless wait
                //lines = await CalculatorHelpers.ReadLinesAsync(sOutputDataURL);
                if (lines == null)
                {
                    this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                    return bHasCalcs;
                }
                if (lines.Count == 0)
                {
                    this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                    return bHasCalcs;
                }
                sb = new StringBuilder();
                sb.AppendLine("aml results");
                //dep var has to be in the R project 1st column
                //string sLine = string.Concat("first variable:  ", _colNames[0]);
                string[] line = new List<string>().ToArray();
                int iPos = 0;
                for (int i = 0; i < lines.Count(); i++)
                {
                    line = lines[i].Split(Constants.CSV_DELIMITERS);
                    //lineout[1] = CalculatorHelpers.ConvertStringToDouble(line[0]).ToString("N4", CultureInfo.InvariantCulture);
                    sb.AppendLine(Shared.GetLine(line, false));
                }
                if (this.MathResult.ToLower().StartsWith("http"))
                {
                    bool bHasSaved = await CalculatorHelpers.SaveTextInURI(
                        _params.ExtensionDocToCalcURI, sb.ToString(), this.MathResult);
                    if (!string.IsNullOrEmpty(_params.ExtensionDocToCalcURI.ErrorMessage))
                    {
                        this.MathResult += _params.ExtensionDocToCalcURI.ErrorMessage;
                        //done with errormsg
                        _params.ExtensionDocToCalcURI.ErrorMessage = string.Empty;
                    }
                }
                else
                {
                    this.MathResult = sb.ToString();
                }
                bHasCalcs = true;
                //last line of string should have the QTM vars
                if (line != null)
                {
                    //last string is prediction
                    iPos = line.Count() - 1;
                    //int iPos = line.Count() - 3;
                    if (line[iPos] != null)
                        this.QTPredicted = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                }
                string sOutput2FullDataURL = string.Concat("https://devtreks1.blob.core.windows.net/", sOutputData2URL);
                lines = new List<string>();
                //azure emulator can't process real Azure URL so this won't work
                //instead, double check that output url is actually saved
                lines = await CalculatorHelpers.ReadLines(_params.ExtensionDocToCalcURI, sOutput2FullDataURL);
                this.ErrorMessage += _params.ExtensionDocToCalcURI.ErrorMessage;
                if (lines == null)
                {
                    this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                    return bHasCalcs;
                }
                if (lines.Count == 0)
                {
                    this.ErrorMessage += string.Concat(" ", Errors.MakeStandardErrorMsg("DATAURL_BAD"));
                    return bHasCalcs;
                }
                sb = new StringBuilder();
                //dep var has to be in the R project 1st column
                //string sLine = string.Concat("first variable:  ", _colNames[0]);
                line = new List<string>().ToArray();
                double dbCI = 0;
                for (int i = 0; i < lines.Count(); i++)
                {
                    line = lines[i].Split(Constants.CSV_DELIMITERS);
                    if (line != null)
                    {
                        iPos = 0;
                        //used to derive conf interval
                        dbCI = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                        sb.AppendLine(string.Format("{0} {1}", "Mean Absolute Error: ", dbCI.ToString()));
                        double dbScore = 0;
                        if (line.Count() >= 2)
                        {
                            iPos = 1;
                            dbScore = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                            sb.AppendLine(string.Format("{0} {1}", "Root Mean Squared Error: ", dbScore.ToString()));
                        }
                        if (line.Count() >= 3)
                        {
                            iPos = 2;
                            dbScore = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                            sb.AppendLine(string.Format("{0} {1}", "Relative Absolute Error: ", dbScore.ToString()));
                        }
                        if (line.Count() >= 4)
                        {
                            iPos = 3;
                            dbScore = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                            sb.AppendLine(string.Format("{0} {1}", "Relative Squared Error: ", dbScore.ToString()));
                        }
                        if (line.Count() >= 5)
                        {
                            iPos = 4;
                            dbScore = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                            sb.AppendLine(string.Format("{0} {1}", "Coefficient of Determination: ", dbScore.ToString()));
                        }
                        //sb.AppendLine(Shared.GetLine(line, false));
                    }
                }
                this.MathResult += sb.ToString();
                bHasCalcs = true;
                //last line of string should have the QTM vars
                if (line != null)
                {
                    if (line[iPos] != null)
                        dbCI = CalculatorHelpers.ConvertStringToDouble(line[iPos]);
                    this.QTL = this.QTPredicted - dbCI;
                    this.QTU = this.QTPredicted + dbCI;
                }

            }
            else
            {
                this.ErrorMessage += "The calculations could not be run using the web service.";
            }
            return bHasCalcs;
        }
        public async Task<bool> SetResponse(string baseURL, string apiKey,
            string inputBlobLocation, string outputBlobLocation, string rScript)
        {
            bool bHasCompleted = false;
            _response = await CalculatorHelpers.InvokeHttpRequestResponseService(
                _params.ExtensionDocToCalcURI, baseURL, apiKey,
                inputBlobLocation, outputBlobLocation, rScript).ConfigureAwait(false);
            bHasCompleted = true;
            return bHasCompleted;
        }
        public async Task<bool> SetResponse2(string baseURL, string apiKey,
           string inputBlob1Location, string inputBlob2Location,
           string outputBlob1Location, string outputBlob2Location)
        {
            bool bHasCompleted = false;
            _response = await CalculatorHelpers.InvokeHttpRequestResponseService2(
                _params.ExtensionDocToCalcURI, baseURL, apiKey,
                inputBlob1Location, inputBlob2Location, outputBlob1Location, outputBlob2Location).ConfigureAwait(false);
            bHasCompleted = true;
            return bHasCompleted;
        }
        public class BatchResult
        {
            public String ConnectionString { get; set; }
            public String RelativeLocation { get; set; }
            public String BaseLocation { get; set; }
            public String SasBlobToken { get; set; }
        }
        public class BatchResponseStructure
        {
            public int StatusCode { get; set; }
            public BatchResult Result { get; set; }
            public String Details { get; set; }
            public BatchResponseStructure()
            {
                this.Result = new BatchResult();
            }
        }
    }
}
