using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace DevTreks.Extensions.Algorithms
{
    /// <summary>
    ///Purpose:		ML02 algorithm
    ///Author:		www.devtreks.org
    ///Date:		2018, May
    ///References:	neural network machine learning
    ///adapted from McCaffrey (MSDN, August, September, October, 2017) 
    ///</summary>
    public class ML02 : MLBase
    {
        public ML02()
            : base() { }
        public ML02(int indicatorIndex, string label, string[] mathTerms,
            string[] colNames, string[] depColNames,
            string subalgorithm, int ciLevel, int iterations,
            int random, IndicatorQT1 qT1, CalculatorParameters calcParams)
            : base(indicatorIndex, label, mathTerms,
            colNames, depColNames, subalgorithm, ciLevel, iterations,
            random, qT1, calcParams)
        { }
        public async Task<bool> RunAlgorithmAsync(List<List<string>> trainData,
            List<List<string>> rowNames, List<List<string>> testData)
        {
            //the bool allows errors to be propagated
            bool bHasCalculations = false;
            try
            {
                //minimal data requirement is first five cols and 3 rows
                if (_colNames.Length < 5 || rowNames.Count < 3)
                {
                    IndicatorQT.ErrorMessage = "ML analysis requires at least 1 output " +
                        "variable and 1 input variable with 3 rows of test data.";
                    return bHasCalculations;
                }
                
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_02.ToString())
                {
                    //use sb for feedback when training and testing; but don't use for production
                    StringBuilder sb = new StringBuilder();
                    ////classify testdata and return new dataset
                    sb = await Classify(trainData, rowNames, testData);
                    bHasCalculations = await SetMathResult(rowNames);

                    //debug first with reference dataset and show debugging messages in results
                    //put the results in MathResult
                    //sb = await DebugClassify(trainData, rowNames, testData);
                    //bHasCalculations = await SetMathResult(rowNames, sb);
                }
            }
            catch (Exception ex)
            {
                IndicatorQT.ErrorMessage = ex.Message;
            }
            return bHasCalculations;
        }

        private async Task<StringBuilder> Classify(List<List<string>> trainData,
            List<List<string>> rowNames, List<List<string>> testData)
        {
            StringBuilder sb = null;
            try
            {
                //ml algo rule: iterations can also set rowcount and -1 for mlinstructs removed
                int iRowCount = (Shared.GetRowCount(_iterations, trainData.Count) - 1);
                //columns of data used and returned in DataResults
                _actualColNames = Shared.GetActualColNames(_colNames, _depColNames).ToArray();
                //ml instructions associated with actual colNames
                List<string> normTypes = Shared.GetNormTypes(trainData[0], _colNames, _depColNames);
                //instructions in both row names and datasets
                List<string> actualMLInstructs = Shared.GetAlgoInstructs(rowNames);
                actualMLInstructs.AddRange(normTypes);
                // error allowance
                double dbPlusorMinus
                    = CalculatorHelpers.ConvertStringToDouble(actualMLInstructs[0]);
                //converts rows to columns with normalized data
                List<List<double>> trainDB = Shared.GetNormalizedDData(trainData,
                   this.IndicatorQT, _colNames, _depColNames, normTypes, "F0");
                List<List<double>> testDB = Shared.GetNormalizedDData(testData,
                    this.IndicatorQT, _colNames, _depColNames, normTypes, "F0");
                //make a new list with same matrix, to be replaced with results
                int iColCount = testDB.Count;
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_02.ToString().ToString())
                {
                    //subalgo02 needs qtm and percent probability of accuracy, mse, qtm, low ci, high ci
                    iColCount = testDB.Count + 7;
                    //normtypes need full columns before insertion
                    normTypes = Shared.FixNormTypes(normTypes, iColCount);
                }
                //row count comes from original testdata to account for the instructions row
                DataResults = CalculatorHelpers.GetList(testData.Count, iColCount);
                DataResults[0] = normTypes;
                //dep var output count
                double[] arrLabelCategories = Shared.GetAttributeGroups(0, trainDB);
                int numOutput = arrLabelCategories.Length;
                //less col[0]
                int numInput = trainDB.Count - 1;
                //dynamic?
                int[] numHidden = new int[] { 10, 10, 10 };
                double[][] trainArrData = MakeData(trainDB, iRowCount, this.IndicatorQT, 
                    numInput, numHidden, _random, arrLabelCategories);
                //build a 4-(10,10,10)-3 deep neural network (tanh & softmax)
                DeepNet dn = new DeepNet(numInput, numHidden, numOutput);

                int maxEpochs = iRowCount;
                double learnRate = 0.001;
                double momentum = 0.01;
                //start training using back-prop with mean squared error \n");
                double[] wts = dn.Train(trainArrData, maxEpochs, learnRate, momentum, 10, sb);
                bool bVerbose = false;
                //final model MS error
                double trainMSE = dn.Error(trainArrData, bVerbose, sb);
                //final model accuracy
                double trainAcc = dn.Accuracy(trainArrData, bVerbose, sb);
                //add classified test data to DataResults
                bool bHasNewClassifs = await AddNewClassifications(dn, testDB, 
                    trainAcc, trainMSE, iRowCount, dbPlusorMinus, _ciLevel);
            }
            catch (Exception ex)
            {
                IndicatorQT.ErrorMessage = ex.Message;
            }
            return sb;
        }
        //strictly used to debug algorithms
        private async Task<StringBuilder> DebugClassify(List<List<string>> trainData,
            List<List<string>> rowNames, List<List<string>> testData)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendLine("\nBegin deep net training demo \n");

                int numInput = 4;
                int[] numHidden = new int[] { 10, 10, 10 };
                int numOutput = 3;

                int numDataItems = 2000;
                sb.AppendLine("Generating " + numDataItems + " artificial training data items ");
                double[][] trainArrData = MakeData(numDataItems, numInput, numHidden, numOutput, 5);
                sb.AppendLine("\nDone. Training data is: ");
                ShowMatrix(sb, trainArrData, 3, 2, true);

                sb.AppendLine("\nCreating a 4-(10,10,10)-3 deep neural network (tanh & softmax) \n");
                DeepNet dn = new DeepNet(numInput, numHidden, numOutput);
                //dn.Dump();

                int maxEpochs = 2000;
                double learnRate = 0.001;
                double momentum = 0.01;
                sb.AppendLine("Setting maxEpochs = " + maxEpochs);
                sb.AppendLine("Setting learnRate = " + learnRate.ToString("F3"));
                sb.AppendLine("Setting momentumm = " + momentum.ToString("F3"));
                sb.AppendLine("\nStart training using back-prop with mean squared error \n");
                double[] wts = dn.Train(trainArrData, maxEpochs, learnRate, 
                    momentum, 10, sb);  // show error every maxEpochs / 10 
                sb.AppendLine("Training complete \n");

                double trainError = dn.Error(trainArrData, false, sb);
                double trainAcc = dn.Accuracy(trainArrData, false, sb);
                sb.AppendLine("Final model MS error = " + trainError.ToString("F4"));
                sb.AppendLine("Final model accuracy = " + trainAcc.ToString("F4"));


                //make predictions for first row of data , 0.00, 1.00, 0.00 
                double[] predictors = new double[] { 2.24, 1.91, 2.52, 2.41};
                double[] probs = dn.ComputeOutputs(predictors);  // 4.33362252510741
                sb.AppendLine("\nPredicted savings for household x: ");
                //sb.AppendLine((forecast[0] * 100).ToString("F0"));
                double[] outputs = ProbsToClasses(probs);  // convert to outputs like [0, 0, 1, 0]

                sb.Append("\npredictor for: ");
                for (int i = 0; i < predictors.Length; ++i)
                    sb.Append(string.Concat(" , ", predictors[i]));
                sb.Append("\nprobabilities: ");
                for (int i = 0; i < probs.Length; ++i)
                    sb.Append(string.Concat(" , ", probs[i].ToString("F4")));
                sb.Append("\npredicted category: ");
                for (int i = 0; i < outputs.Length; ++i)
                    sb.Append(string.Concat(" , ", outputs[i]));
                //, 0.00, 0.00, 1.00 
                predictors = new double[] { -3.61, 2.50, 1.29, -1.59};
                probs = dn.ComputeOutputs(predictors);  // 4.33362252510741
                sb.AppendLine("\nPredicted savings for household x: ");
                //sb.AppendLine((forecast[0] * 100).ToString("F0"));
                outputs = ProbsToClasses(probs);  // convert to outputs like [0, 0, 1, 0]

                sb.Append("\npredictor for: ");
                for (int i = 0; i < predictors.Length; ++i)
                    sb.Append(string.Concat(" , ", predictors[i]));
                sb.Append("\nprobabilities: ");
                for (int i = 0; i < probs.Length; ++i)
                    sb.Append(string.Concat(" , ", probs[i].ToString("F4")));
                sb.Append("\npredicted category: ");
                for (int i = 0; i < outputs.Length; ++i)
                    sb.Append(string.Concat(" , ", outputs[i]));

                //, 1.00, 0.00, 0.00
                predictors = new double[] { 3.18, -1.67, 3.22, 0.71};
                probs = dn.ComputeOutputs(predictors);  // 4.33362252510741
                sb.AppendLine("\nPredicted savings for household x: ");
                //sb.AppendLine((forecast[0] * 100).ToString("F0"));
                outputs = ProbsToClasses(probs);  // convert to outputs like [0, 0, 1, 0]

                sb.Append("\npredictor for: ");
                for (int i = 0; i < predictors.Length; ++i)
                    sb.Append(string.Concat(" , ", predictors[i]));
                sb.Append("\nprobabilities: ");
                for (int i = 0; i < probs.Length; ++i)
                    sb.Append(string.Concat(" , ", probs[i].ToString("F4")));
                sb.Append("\npredicted category: ");
                for (int i = 0; i < outputs.Length; ++i)
                    sb.Append(string.Concat(" , ", outputs[i]));
                sb.AppendLine("\nEnd demo ");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message);
            }
            return sb;
        }
        
        static double[][] MakeData(List<List<double>> trainData, int numItems, 
            IndicatorQT1 qt1, int numInput, int[] numHidden,  
            int seed, double[] arrLabelCategories)
        {
            int numOutput = arrLabelCategories.Length;
            // generate data using a Deep NN (tanh hidden activation)
            DeepNet dn = new DeepNet(numInput, numHidden, numOutput);
            // make a DNN generator
            // to make random weights & biases, [random] input vals
            Random rrnd = new Random(seed);  
            double wtLo = -9.0;
            double wtHi = 9.0;
            int nw = DeepNet.NumWeights(numInput, numHidden, numOutput);
            double[] wts = new double[nw];

            for (int i = 0; i < nw; ++i)
                wts[i] = (wtHi - wtLo) * rrnd.NextDouble() + wtLo;
            dn.SetWeights(wts);
            // make the result matrix holder (now first index is row)
            double[][] result = new double[numItems][];  
            for (int r = 0; r < numItems; ++r)
                // allocate the cols
                result[r] = new double[numInput + numOutput];
            //already normalized but assume this scaling helps: pseudo-Gaussian scaling
            double inLo = -4.0;
            double inHi = 4.0;
            double dbInput = 0;
            for (int r = 0; r < numItems; ++r)
            {
                double[] inputs = new double[numInput];
                if ((trainData.Count + 1) > numInput)
                {
                    for (int i = 0; i < numInput; ++i)
                    {
                        //skip col[0]
                        dbInput = trainData[i + 1][r];
                        inputs[i] = Shared.GetScaledData(inLo, inHi, dbInput);
                    }
                    //traindata has been categorized into doubles
                    double[] outputs = Shared.ConvertDoubleToOutputsIndex(trainData[0][r],
                        qt1, arrLabelCategories);
                    int c = 0;
                    for (int i = 0; i < numInput; ++i)
                        result[r][c++] = inputs[i];
                    for (int i = 0; i < numOutput; ++i)
                        result[r][c++] = outputs[i];
                }
            }
            return result;
        }
        static double[][] MakeData(int numItems, int numInput, int[] numHidden, int numOutput, int seed)
        {
            // generate data using a Deep NN (tanh hidden activation)
            DeepNet dn = new DeepNet(numInput, numHidden, numOutput);  // make a DNN generator
            Random rrnd = new Random(seed);  // to make random weights & biases, random input vals
            double wtLo = -9.0;
            double wtHi = 9.0;
            int nw = DeepNet.NumWeights(numInput, numHidden, numOutput);
            double[] wts = new double[nw];

            for (int i = 0; i < nw; ++i)
                wts[i] = (wtHi - wtLo) * rrnd.NextDouble() + wtLo;
            dn.SetWeights(wts);

            double[][] result = new double[numItems][];  // make the result matrix holder
            for (int r = 0; r < numItems; ++r)
                result[r] = new double[numInput + numOutput];  // allocate the cols

            double inLo = -4.0;    // pseudo-Gaussian scaling
            double inHi = 4.0;
            for (int r = 0; r < numItems; ++r)  // each row
            {
                double[] inputs = new double[numInput];  // random input values

                for (int i = 0; i < numInput; ++i)
                    inputs[i] = (inHi - inLo) * rrnd.NextDouble() + inLo;

                //ShowVector(inputs, 2);

                double[] probs = dn.ComputeOutputs(inputs);  // compute the outputs (as softmax probs) like [0.10, 0.15, 0.55, 0.20]
                                                             //dn.Dump();
                                                             //Console.ReadLine();
                                                             //ShowVector(probs, 4);
                double[] outputs = ProbsToClasses(probs);  // convert to outputs like [0, 0, 1, 0]

                int c = 0;
                for (int i = 0; i < numInput; ++i)
                    result[r][c++] = inputs[i];
                for (int i = 0; i < numOutput; ++i)
                    result[r][c++] = outputs[i];
                //sb.AppendLine("");
            }
            return result;

        } // MakeData

        static double[] ProbsToClasses(double[] probs)
        {
            double[] result = new double[probs.Length];
            int idx = MaxIndex(probs);
            result[idx] = 1.0;
            return result;
        }

        static int MaxIndex(double[] probs)
        {
            int maxIdx = 0;
            double maxVal = probs[0];

            for (int i = 0; i < probs.Length; ++i)
            {
                if (probs[i] > maxVal)
                {
                    maxVal = probs[i];
                    maxIdx = i;
                }
            }
            return maxIdx;
        }

        public static void ShowMatrix(StringBuilder sb, double[][] matrix, int numRows,
          int decimals, bool indices)
        {
            int len = matrix.Length.ToString().Length;
            for (int i = 0; i < numRows; ++i)
            {
                if (indices == true)
                    sb.Append("[" + i.ToString().PadLeft(len) + "]  ");
                for (int j = 0; j < matrix[i].Length; ++j)
                {
                    double v = matrix[i][j];
                    if (v >= 0.0)
                        sb.Append(" "); // '+'
                    sb.Append(v.ToString("F" + decimals) + "  ");
                }
                sb.AppendLine("");
            }

            if (numRows < matrix.Length)
            {
                sb.AppendLine(". . .");
                int lastRow = matrix.Length - 1;
                if (indices == true)
                    sb.Append("[" + lastRow.ToString().PadLeft(len) + "]  ");
                for (int j = 0; j < matrix[lastRow].Length; ++j)
                {
                    double v = matrix[lastRow][j];
                    if (v >= 0.0)
                        sb.Append(" "); // '+'
                    sb.Append(v.ToString("F" + decimals) + "  ");
                }
            }
            sb.AppendLine("\n");
        }

        static void ShowMatrix(StringBuilder sb, double[][] matrix, int numRows, int numDec)
        {
            for (int r = 0; r < numRows; ++r)
            {
                for (int c = 0; c < matrix[r].Length; ++c)
                {
                    if (matrix[r][c] >= 0.0) sb.Append(" ");  // '+'
                    sb.Append(matrix[r][c].ToString("F" + numDec) + "  ");
                }
                sb.AppendLine("");
            }
            sb.AppendLine("");
        }

        static void ShowVector(StringBuilder sb, double[] vector, int numDec)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] >= 0.0) sb.Append(" ");
                sb.Append(vector[i].ToString("F" + numDec) + "  ");
            }
            sb.AppendLine("");
        }
        private async Task<bool> AddNewClassifications(DeepNet dn, List<List<double>> testData,
            double trainAccuracy, double trainMSE, int rowCount, double hiLow, int ciPercent)
        {
            bool bHasNewClassifs = false;
            int iRowLength = 0;
            string sResult = string.Empty;
            List<double> predictors = new List<double>();
            double dbHighest = 0;
            int iIndex = 0;
            string sLabelClass = string.Empty;
            double dbProb = 0;
            if (ciPercent > 5)
            {
                hiLow = CalculatorHelpers.GetConfidenceIntervalFromMSE(
                    ciPercent, rowCount, trainMSE);
            }
            //same scaling as makedata
            double inLo = -4.0;
            double inHi = 4.0;
            double dbInput = 0;
            double dbTarget = 0;
            //test data stores cols in testdata[0] first index
            for (int r = 0; r < DataResults.Count - 1; r++)
            {
                if (r == 0)
                {
                    iRowLength = DataResults[r+1].Count;
                }
                predictors = new List<double>();
                dbHighest = 0;
                iIndex = 0;
                for (int c = 0; c < testData.Count; c++)
                {
                    //prepare mathresults
                    dbInput = testData[c][r];
                    DataResults[r+1][c] = dbInput.ToString("F4");
                    if (c > 0)
                    {
                        predictors.Add(Shared.GetScaledData(inLo, inHi, dbInput));
                    }
                }
                //predict classifications
                double[] probs = dn.ComputeOutputs(predictors.ToArray());  
                double[] outputs = ProbsToClasses(probs);
                for (int i = 0; i < probs.Length; i++)
                {
                    dbProb = 100 * probs[i];
                    if (dbProb > dbHighest)
                    {
                        dbHighest = dbProb;
                        iIndex = i;
                    }
                }
                DataResults[r+1][iRowLength - 5] = dbHighest.ToString("F4");
                double dbMostLikelyQTM = Shared.ConvertIndexToAttribute(
                    iIndex, this.IndicatorQT);
                if (r == 0)
                {
                    DataResults[r][iRowLength - 7] = trainMSE.ToString("F4");
                    DataResults[r][iRowLength - 6] = trainAccuracy.ToString("F4");
                }
                dbTarget = testData[0][r];
                double dbError = Math.Abs(dbTarget - dbMostLikelyQTM);
                this.DataResults[r + 1][iRowLength - 7] = dbError.ToString("F4");
                if (dbTarget == dbMostLikelyQTM)
                {
                    DataResults[r + 1][iRowLength - 6] = "true";
                }
                else
                {
                    DataResults[r + 1][iRowLength - 6] = "false";
                }

                DataResults[r+1][iRowLength - 4] = dbMostLikelyQTM.ToString("F4");
                //fill in indicator from last row
                sLabelClass = Shared.ConvertIndexToLabel(iIndex, this.IndicatorQT);
                if (r == DataResults.Count - 2)
                {
                    this.IndicatorQT.QTM = dbMostLikelyQTM;
                    if ((string.IsNullOrEmpty(this.IndicatorQT.QTMUnit)
                        || this.IndicatorQT.QTMUnit == Constants.NONE)
                        && !string.IsNullOrEmpty(sLabelClass))
                    {
                        this.IndicatorQT.QTMUnit = sLabelClass;
                    }
                    this.IndicatorQT.QTL = dbMostLikelyQTM - hiLow;
                    this.IndicatorQT.QTU = dbMostLikelyQTM + hiLow;
                }
                //convert output with a 1 to text classification
                DataResults[r+1][iRowLength - 3] = sLabelClass;
                //low estimation
                DataResults[r+1][iRowLength - 2] = (dbMostLikelyQTM - hiLow).ToString("F4");
                //high estimation
                DataResults[r+1][iRowLength - 1] = (dbMostLikelyQTM + hiLow).ToString("F4");
            }
            return bHasNewClassifs;
        }
        private async Task<bool> SetMathResult(List<List<string>> rowNames, StringBuilder sb = null)
        {
            bool bHasSet = false;
            if (sb == null)
            {
                //add the data to a string builder
                sb = new StringBuilder();
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_02.ToString())
                {
                    sb.AppendLine("ml results");
                    string[] newColNames = new string[_actualColNames.Length + 7];
                    for (int i = 0; i < _actualColNames.Length; i++)
                    {
                        newColNames[i] = _actualColNames[i];
                    }
                    //new cols changed by algo
                    newColNames[_actualColNames.Length] = "mse";
                    newColNames[_actualColNames.Length + 1] = "accuracy";
                    newColNames[_actualColNames.Length + 2] = "probability";
                    newColNames[_actualColNames.Length + 3] = "qtm";
                    newColNames[_actualColNames.Length + 4] = "class";
                    newColNames[_actualColNames.Length + 5] = "qtl";
                    newColNames[_actualColNames.Length + 6] = "qtu";
                    _actualColNames = newColNames;
                    CalculatorHelpers.SetIndMathResult(sb, _actualColNames, rowNames, DataResults);
                }
                else if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_03.ToString())
                {
                    sb.AppendLine("ml results");
                    string[] newColNames = new string[_actualColNames.Length + 2];
                    for (int i = 0; i < _actualColNames.Length; i++)
                    {
                        newColNames[i] = _actualColNames[i];
                    }
                    //new cols changed by algo
                    newColNames[_actualColNames.Length] = "label";
                    newColNames[_actualColNames.Length + 1] = "probability";
                    _actualColNames = newColNames;
                    CalculatorHelpers.SetIndMathResult(sb, _actualColNames, rowNames, DataResults);
                }
            }
            if (this.IndicatorQT.MathResult.ToLower().StartsWith("http"))
            {
                bool bHasSaved = await CalculatorHelpers.SaveTextInURI(
                    Params.ExtensionDocToCalcURI, sb.ToString(), this.IndicatorQT.MathResult);
                if (!string.IsNullOrEmpty(Params.ExtensionDocToCalcURI.ErrorMessage))
                {
                    this.IndicatorQT.MathResult += Params.ExtensionDocToCalcURI.ErrorMessage;
                    //done with errormsg
                    Params.ExtensionDocToCalcURI.ErrorMessage = string.Empty;
                }
                else
                {
                    bHasSet = true;
                }
            }
            else
            {
                this.IndicatorQT.MathResult = sb.ToString();
                bHasSet = true;
            }
            return bHasSet;
        }
    }

 //} // Program

    public class DeepNet
    {
        public static Random rnd;  // weight init and train shuffle

        public int nInput;  // number input nodes
        public int[] nHidden;  // number hidden nodes, each layer
        public int nOutput;  // number output nodes
        public int nLayers;  // number hidden node layers

        public double[] iNodes;  // input nodes
        public double[][] hNodes;
        public double[] oNodes;

        public double[][] ihWeights;  // input- 1st hidden
        public double[][][] hhWeights; // hidden-hidden
        public double[][] hoWeights;  // last hidden-output

        public double[][] hBiases;  // hidden node biases
        public double[] oBiases;  // output node biases

        public double ihGradient00;  // one gradient to monitor

        public DeepNet(int numInput, int[] numHidden, int numOutput)
        {
            rnd = new Random(0);  // seed could be a ctor parameter

            this.nInput = numInput;
            this.nHidden = new int[numHidden.Length];
            for (int i = 0; i < numHidden.Length; ++i)
                this.nHidden[i] = numHidden[i];
            this.nOutput = numOutput;
            this.nLayers = numHidden.Length;

            iNodes = new double[numInput];
            hNodes = MakeJaggedMatrix(numHidden);
            oNodes = new double[numOutput];

            ihWeights = MakeMatrix(numInput, numHidden[0]);
            hoWeights = MakeMatrix(numHidden[nLayers - 1], numOutput);

            hhWeights = new double[nLayers - 1][][];  // if 3 h layer, 2 h-h weights[][]
            for (int h = 0; h < hhWeights.Length; ++h)
            {
                int rows = numHidden[h];
                int cols = numHidden[h + 1];
                hhWeights[h] = MakeMatrix(rows, cols);
            }

            hBiases = MakeJaggedMatrix(numHidden);  // pass an array of lengths
            oBiases = new double[numOutput];

            InitializeWeights();  // small randomm non-zero values
        } // ctor

        public void InitializeWeights()
        {
            // make wts
            double lo = -0.10;
            double hi = +0.10;
            int numWts = DeepNet.NumWeights(this.nInput, this.nHidden, this.nOutput);
            double[] wts = new double[numWts];
            for (int i = 0; i < numWts; ++i)
                wts[i] = (hi - lo) * rnd.NextDouble() + lo;
            this.SetWeights(wts);
        }

        public double[] ComputeOutputs(double[] xValues)
        {
            // 'xValues' might have class label or not
            // copy vals into iNodes
            for (int i = 0; i < nInput; ++i)  // possible trunc
                iNodes[i] = xValues[i];

            // zero-out all hNodes, oNodes
            for (int h = 0; h < nLayers; ++h)
                for (int j = 0; j < nHidden[h]; ++j)
                    hNodes[h][j] = 0.0;

            for (int k = 0; k < nOutput; ++k)
                oNodes[k] = 0.0;

            // input to 1st hid layer
            for (int j = 0; j < nHidden[0]; ++j)  // each hidden node, 1st layer
            {
                for (int i = 0; i < nInput; ++i)
                    hNodes[0][j] += ihWeights[i][j] * iNodes[i];
                // add the bias
                hNodes[0][j] += hBiases[0][j];
                // apply activation
                hNodes[0][j] = Math.Tanh(hNodes[0][j]);
            }

            // each remaining hidden node
            for (int h = 1; h < nLayers; ++h)
            {
                for (int j = 0; j < nHidden[h]; ++j)  // 'to index'
                {
                    for (int jj = 0; jj < nHidden[h - 1]; ++jj)  // 'from index'
                    {
                        hNodes[h][j] += hhWeights[h - 1][jj][j] * hNodes[h - 1][jj];
                    }
                    hNodes[h][j] += hBiases[h][j];  // add bias value
                    hNodes[h][j] = Math.Tanh(hNodes[h][j]);  // apply activation
                }
            }

            // compute ouput node values
            for (int k = 0; k < nOutput; ++k)
            {
                for (int j = 0; j < nHidden[nLayers - 1]; ++j)
                {
                    oNodes[k] += hoWeights[j][k] * hNodes[nLayers - 1][j];
                }
                oNodes[k] += oBiases[k];  // add bias value
                                          //sb.AppendLine("Pre-softmax output node [" + k + "] value = " + oNodes[k].ToString("F4"));
            }

            double[] retResult = Softmax(oNodes);  // softmax activation all oNodes

            for (int k = 0; k < nOutput; ++k)
                oNodes[k] = retResult[k];
            return retResult;  // calling convenience

        } // ComputeOutputs

        public double[] Train(double[][] trainData, int maxEpochs, double learnRate, 
            double momentum, int showEvery, StringBuilder sb = null)
        {
            // no momentum right now
            // each weight (and bias) needs a big_delta. big_delta is just learnRate * "a gradient"
            // so goal is to find "a gradient".
            // the gradient (the term can have several meanings) is "a signal" * "an input"
            // the signal 

            // 1. each weight and bias has a 'gradient' (partial dervative)
            double[][] hoGrads = MakeMatrix(nHidden[nLayers - 1], nOutput);  // last_hidden layer - output weights grads
            double[][][] hhGrads = new double[nLayers - 1][][];
            for (int h = 0; h < hhGrads.Length; ++h)
            {
                int rows = nHidden[h];
                int cols = nHidden[h + 1];
                hhGrads[h] = MakeMatrix(rows, cols);
            }
            double[][] ihGrads = MakeMatrix(nInput, nHidden[0]);  // input-first_hidden wts gradients
                                                                  // biases
            double[] obGrads = new double[nOutput];  // output node bias grads
            double[][] hbGrads = MakeJaggedMatrix(nHidden);  // hidden node bias grads

            // 2. each output node and each hidden node has a 'signal' == gradient without associated input (lower case delta in Wikipedia)
            double[] oSignals = new double[nOutput];
            double[][] hSignals = MakeJaggedMatrix(nHidden);

            // 3. for momentum, each weight and bias needs to store the prev epoch delta
            // the structure for prev deltas is same as for Weights & Biases, which is same as for Grads

            double[][] hoPrevWeightsDelta = MakeMatrix(nHidden[nLayers - 1], nOutput);  // last_hidden layer - output weights momentum term
            double[][][] hhPrevWeightsDelta = new double[nLayers - 1][][];
            for (int h = 0; h < hhPrevWeightsDelta.Length; ++h)
            {
                int rows = nHidden[h];
                int cols = nHidden[h + 1];
                hhPrevWeightsDelta[h] = MakeMatrix(rows, cols);
            }
            double[][] ihPrevWeightsDelta = MakeMatrix(nInput, nHidden[0]);  // input-first_hidden wts gradients
            double[] oPrevBiasesDelta = new double[nOutput];  // output node bias prev deltas
            double[][] hPrevBiasesDelta = MakeJaggedMatrix(nHidden);  // hidden node bias prev deltas

            int epoch = 0;
            double[] xValues = new double[nInput];  // not necessary - could copy direct from data item to iNodes
            double[] tValues = new double[nOutput];  // not necessary
            double derivative = 0.0;  // of activation (softmax or tanh or log-sigmoid or relu)
            double errorSignal = 0.0;  // target - output

            int[] sequence = new int[trainData.Length];
            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;

            int errInterval = maxEpochs / showEvery; // interval to check & display  error
            while (epoch < maxEpochs)
            {
                ++epoch;
                if (epoch % errInterval == 0 && epoch < maxEpochs)  // display curr MSE
                {
                    if (sb != null)
                    {
                        double trainErr = Error(trainData, false, sb);  // using curr weights & biases
                        double trainAcc = this.Accuracy(trainData, false, sb);
                        sb.Append("epoch = " + epoch + "  MS error = " +
                          trainErr.ToString("F4"));
                        sb.AppendLine("  accuracy = " +
                          trainAcc.ToString("F4"));

                        sb.AppendLine("input-to-hidden [0][0] gradient = " + this.ihGradient00.ToString("F6"));
                        sb.AppendLine("");
                        //this.Dump();
                        // Console.ReadLine();
                    }
                }

                Shuffle(sequence); // must visit each training data in random order in stochastic GD

                for (int ii = 0; ii < trainData.Length; ++ii)  // each train data item
                {
                    int idx = sequence[ii];  // idx points to a data item
                    Array.Copy(trainData[idx], xValues, nInput);
                    Array.Copy(trainData[idx], nInput, tValues, 0, nOutput);
                    ComputeOutputs(xValues); // copy xValues in, compute outputs using curr weights & biases, ignore return

                    // must compute signals from right-to-left
                    // weights and bias gradients can be computed left-to-right
                    // weights and bias gradients can be updated left-to-right

                    // x. compute output node signals (assumes softmax) depends on target values to the right
                    for (int k = 0; k < nOutput; ++k)
                    {
                        errorSignal = tValues[k] - oNodes[k];  // Wikipedia uses (o-t)
                        derivative = (1 - oNodes[k]) * oNodes[k]; // for softmax (same as log-sigmoid) with MSE
                                                                  //derivative = 1.0;  // for softmax with cross-entropy
                        oSignals[k] = errorSignal * derivative;  // we'll use this for ho-gradient and hSignals
                    }

                    // x. compute signals for last hidden layer (depends on oNodes values to the right)
                    int lastLayer = nLayers - 1;
                    for (int j = 0; j < nHidden[lastLayer]; ++j)
                    {
                        derivative = (1 + hNodes[lastLayer][j]) * (1 - hNodes[lastLayer][j]); // for tanh!
                        double sum = 0.0; // need sums of output signals times hidden-to-output weights
                        for (int k = 0; k < nOutput; ++k)
                        {
                            sum += oSignals[k] * hoWeights[j][k]; // represents error signal
                        }
                        hSignals[lastLayer][j] = derivative * sum;
                    }

                    // x. compute signals for all the non-last layer hidden nodes (depends on layer to the right)
                    for (int h = lastLayer - 1; h >= 0; --h)  // each hidden layer, right-to-left
                    {
                        for (int j = 0; j < nHidden[h]; ++j)  // each node
                        {
                            derivative = (1 + hNodes[h][j]) * (1 - hNodes[h][j]); // for tanh
                                                                                  // derivative = hNodes[h][j];

                            double sum = 0.0; // need sums of output signals times hidden-to-output weights

                            for (int jj = 0; jj < nHidden[h + 1]; ++jj)  // layer to right of curr layer
                                sum += hSignals[h + 1][jj] * hhWeights[h][j][jj];

                            hSignals[h][j] = derivative * sum;

                        } // j
                    } // h

                    // at this point, all hidden and output node signals have been computed
                    // calculate gradients left-to-right

                    // x. compute input-to-hidden weights gradients using iNodes & hSignal[0]
                    for (int i = 0; i < nInput; ++i)
                        for (int j = 0; j < nHidden[0]; ++j)
                            ihGrads[i][j] = iNodes[i] * hSignals[0][j];  // "from" input & "to" signal

                    // save the special monitored ihGradient00
                    this.ihGradient00 = ihGrads[0][0];

                    // x. compute hidden-to-hidden gradients
                    for (int h = 0; h < nLayers - 1; ++h)
                    {
                        for (int j = 0; j < nHidden[h]; ++j)
                        {
                            for (int jj = 0; jj < nHidden[h + 1]; ++jj)
                            {
                                hhGrads[h][j][jj] = hNodes[h][j] * hSignals[h + 1][jj];
                            }
                        }
                    }

                    // x. compute hidden-to-output gradients
                    for (int j = 0; j < nHidden[lastLayer]; ++j)
                    {
                        for (int k = 0; k < nOutput; ++k)
                        {
                            hoGrads[j][k] = hNodes[lastLayer][j] * oSignals[k];  // from last hidden, to oSignals
                        }
                    }

                    // compute bias gradients
                    // a bias is like a weight on the left/before
                    // so there's a dummy input of 1.0 and we use the signal of the 'current' layer

                    // x. compute all hidden bias gradients
                    // a gradient needs the "left/from" input and the "right/to" signal
                    // for biases we use a dummy 1.0 input

                    for (int h = 0; h < nLayers; ++h)
                    {
                        for (int j = 0; j < nHidden[h]; ++j)
                        {
                            hbGrads[h][j] = 1.0 * hSignals[h][j];
                        }
                    }

                    // x. output bias gradients
                    for (int k = 0; k < nOutput; ++k)
                    {
                        obGrads[k] = 1.0 * oSignals[k];
                    }

                    // at this point all signals have been computed and all gradients have been computed 
                    // so can use gradients to update all weights and biases.
                    // save each delta for the momentum

                    // x. update input-to-first_hidden weights using ihWeights & ihGrads
                    for (int i = 0; i < nInput; ++i)
                    {
                        for (int j = 0; j < nHidden[0]; ++j)
                        {
                            double delta = ihGrads[i][j] * learnRate;
                            ihWeights[i][j] += delta;
                            ihWeights[i][j] += ihPrevWeightsDelta[i][j] * momentum;
                            ihPrevWeightsDelta[i][j] = delta;
                        }
                    }

                    // other hidden-to-hidden weights using hhWeights & hhGrads
                    for (int h = 0; h < nLayers - 1; ++h)
                    {
                        for (int j = 0; j < nHidden[h]; ++j)
                        {
                            for (int jj = 0; jj < nHidden[h + 1]; ++jj)
                            {
                                double delta = hhGrads[h][j][jj] * learnRate;
                                hhWeights[h][j][jj] += delta;
                                hhWeights[h][j][jj] += hhPrevWeightsDelta[h][j][jj] * momentum;
                                hhPrevWeightsDelta[h][j][jj] = delta;
                            }
                        }
                    }

                    // update hidden-to-output weights using hoWeights & hoGrads
                    for (int j = 0; j < nHidden[lastLayer]; ++j)
                    {
                        for (int k = 0; k < nOutput; ++k)
                        {
                            double delta = hoGrads[j][k] * learnRate;
                            hoWeights[j][k] += delta;
                            hoWeights[j][k] += hoPrevWeightsDelta[j][k] * momentum;
                            hoPrevWeightsDelta[j][k] = delta;
                        }
                    }

                    // update hidden biases using hBiases & hbGrads
                    for (int h = 0; h < nLayers; ++h)
                    {
                        for (int j = 0; j < nHidden[h]; ++j)
                        {
                            double delta = hbGrads[h][j] * learnRate;
                            hBiases[h][j] += delta;
                            hBiases[h][j] += hPrevBiasesDelta[h][j] * momentum;
                            hPrevBiasesDelta[h][j] = delta;
                        }
                    }

                    // update output biases using oBiases & obGrads
                    for (int k = 0; k < nOutput; ++k)
                    {
                        double delta = obGrads[k] * learnRate;
                        oBiases[k] += delta;
                        oBiases[k] += oPrevBiasesDelta[k] * momentum;
                        oPrevBiasesDelta[k] = delta;
                    }

                    // Whew!
                }  // for each train data item
            }  // while

            double[] bestWts = this.GetWeights();
            return bestWts;
        } // Train

        public double Error(double[][] data, bool verbose, StringBuilder sb = null)
        {
            // mean squared error using current weights & biases
            double sumSquaredError = 0.0;
            double[] xValues = new double[nInput]; // first numInput values in trainData
            double[] tValues = new double[nOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, nInput);
                Array.Copy(data[i], nInput, tValues, 0, nOutput); // get target values
                double[] yValues = this.ComputeOutputs(xValues); // outputs using current weights
                if (sb != null)
                {
                    if (verbose == true)
                    {
                        ShowVector(sb, yValues, 4);
                        ShowVector(sb, tValues, 4);
                        sb.AppendLine("");
                    }
                }

                for (int j = 0; j < nOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            // average per item
            return sumSquaredError / (data.Length * nOutput);  
        } 

        public double Error(double[][] data, double[] weights)
        {
            // mean squared error using supplied weights & biases
            this.SetWeights(weights);

            double sumSquaredError = 0.0;
            double[] xValues = new double[nInput]; // first numInput values in trainData
            double[] tValues = new double[nOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, nInput);
                Array.Copy(data[i], nInput, tValues, 0, nOutput); // get target values
                double[] yValues = this.ComputeOutputs(xValues); // outputs using current weights
                for (int j = 0; j < nOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            return sumSquaredError / data.Length;
        } 

        public double Accuracy(double[][] data, bool verbose, StringBuilder sb = null)
        {
            // percentage correct using winner-takes all
            int numCorrect = 0;
            int numWrong = 0;
            double[] xValues = new double[nInput]; // inputs
            double[] tValues = new double[nOutput]; // targets
            double[] yValues; // computed Y

            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, nInput); // get x-values
                Array.Copy(data[i], nInput, tValues, 0, nOutput); // get t-values
                yValues = this.ComputeOutputs(xValues);
                if (sb != null)
                {
                    if (verbose == true)
                    {
                        ShowVector(sb, yValues, 4);
                        ShowVector(sb, tValues, 4);
                        sb.AppendLine("");
                    }
                }
                // which cell in yValues has largest value?
                int maxIndex = MaxIndex(yValues); 
                int tMaxIndex = MaxIndex(tValues);

                if (maxIndex == tMaxIndex)
                    ++numCorrect;
                else
                    ++numWrong;
            }
            return (numCorrect * 1.0) / (numCorrect + numWrong);
        }

        private static void ShowVector(StringBuilder sb, double[] vector, int dec)
        {
            for (int i = 0; i < vector.Length; ++i)
                sb.Append(vector[i].ToString("F" + dec) + " ");
            sb.AppendLine("");
        }

        public double Accuracy(double[][] data, double[] weights)
        {
            this.SetWeights(weights);
            // percentage correct using winner-takes all
            int numCorrect = 0;
            int numWrong = 0;
            double[] xValues = new double[nInput]; // inputs
            double[] tValues = new double[nOutput]; // targets
            double[] yValues; // computed Y

            for (int i = 0; i < data.Length; ++i)
            {
                Array.Copy(data[i], xValues, nInput); // get x-values
                Array.Copy(data[i], nInput, tValues, 0, nOutput); // get t-values
                yValues = this.ComputeOutputs(xValues);
                int maxIndex = MaxIndex(yValues); // which cell in yValues has largest value?
                int tMaxIndex = MaxIndex(tValues);

                if (maxIndex == tMaxIndex)
                    ++numCorrect;
                else
                    ++numWrong;
            }
            return (numCorrect * 1.0) / (numCorrect + numWrong);
        }

        private static int MaxIndex(double[] vector) // helper for Accuracy()
        {
            // index of largest value in vector[]
            int bigIndex = 0;
            double biggestVal = vector[0];
            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] > biggestVal)
                {
                    biggestVal = vector[i];
                    bigIndex = i;
                }
            }
            return bigIndex;
        }

        private void Shuffle(int[] sequence) // instance method
        {
            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }
        } // Shuffle



        private static double MyTanh(double x)
        {
            if (x < -20.0) return -1.0; // approximation is correct to 30 decimals
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        private static double[] Softmax(double[] oSums)
        {
            // does all output nodes at once so scale
            // doesn't have to be re-computed each time.
            // possible overflow . . . use max trick

            double sum = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                sum += Math.Exp(oSums[i]);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i]) / sum;

            return result; // now scaled so that xi sum to 1.0
        }


        public void SetWeights(double[] wts)
        {
            // order: ihweights - hhWeights[] - hoWeights - hBiases[] - oBiases
            int nw = NumWeights(this.nInput, this.nHidden, this.nOutput);  // total num wts + biases
            if (wts.Length != nw)
                throw new Exception("Bad wts[] length in SetWeights()");
            int ptr = 0;  // pointer into wts[]

            for (int i = 0; i < nInput; ++i)  // input node
                for (int j = 0; j < hNodes[0].Length; ++j)  // 1st hidden layer nodes
                    ihWeights[i][j] = wts[ptr++];

            for (int h = 0; h < nLayers - 1; ++h)  // not last h layer
            {
                for (int j = 0; j < nHidden[h]; ++j)  // from node
                {
                    for (int jj = 0; jj < nHidden[h + 1]; ++jj)  // to node
                    {
                        hhWeights[h][j][jj] = wts[ptr++];
                    }
                }
            }

            int hi = this.nLayers - 1;  // if 3 hidden layers (0,1,2) last is 3-1 = [2]
            for (int j = 0; j < this.nHidden[hi]; ++j)
            {
                for (int k = 0; k < this.nOutput; ++k)
                {
                    hoWeights[j][k] = wts[ptr++];
                }
            }

            for (int h = 0; h < nLayers; ++h)  // hidden node biases
            {
                for (int j = 0; j < this.nHidden[h]; ++j)
                {
                    hBiases[h][j] = wts[ptr++];
                }
            }

            for (int k = 0; k < nOutput; ++k)
            {
                oBiases[k] = wts[ptr++];
            }
        } // SetWeights

        public double[] GetWeights()
        {
            // order: ihweights -> hhWeights[] -> hoWeights -> hBiases[] -> oBiases
            int nw = NumWeights(this.nInput, this.nHidden, this.nOutput);  // total num wts + biases
            double[] result = new double[nw];

            int ptr = 0;  // pointer into result[]

            for (int i = 0; i < nInput; ++i)  // input node
                for (int j = 0; j < hNodes[0].Length; ++j)  // 1st hidden layer nodes
                    result[ptr++] = ihWeights[i][j];

            for (int h = 0; h < nLayers - 1; ++h)  // not last h layer
            {
                for (int j = 0; j < nHidden[h]; ++j)  // from node
                {
                    for (int jj = 0; jj < nHidden[h + 1]; ++jj)  // to node
                    {
                        result[ptr++] = hhWeights[h][j][jj];
                    }
                }
            }

            int hi = this.nLayers - 1;  // if 3 hidden layers (0,1,2) last is 3-1 = [2]
            for (int j = 0; j < this.nHidden[hi]; ++j)
            {
                for (int k = 0; k < this.nOutput; ++k)
                {
                    result[ptr++] = hoWeights[j][k];
                }
            }

            for (int h = 0; h < nLayers; ++h)  // hidden node biases
            {
                for (int j = 0; j < this.nHidden[h]; ++j)
                {
                    result[ptr++] = hBiases[h][j];
                }
            }

            for (int k = 0; k < nOutput; ++k)
            {
                result[ptr++] = oBiases[k];
            }
            return result;
        }

        public static int NumWeights(int numInput, int[] numHidden, int numOutput)
        {
            // total num weights and biases
            int ihWts = numInput * numHidden[0];

            int hhWts = 0;
            for (int j = 0; j < numHidden.Length - 1; ++j)
            {
                int rows = numHidden[j];
                int cols = numHidden[j + 1];
                hhWts += rows * cols;
            }
            int hoWts = numHidden[numHidden.Length - 1] * numOutput;

            int hbs = 0;
            for (int i = 0; i < numHidden.Length; ++i)
                hbs += numHidden[i];

            int obs = numOutput;
            int nw = ihWts + hhWts + hoWts + hbs + obs;
            return nw;
        }

        public static double[][] MakeJaggedMatrix(int[] cols)
        {
            // array of arrays using size info in cols[]
            int rows = cols.Length;  // num rows inferred by col count
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
            {
                int ncol = cols[i];
                result[i] = new double[ncol];
            }
            return result;
        }

        public static double[][] MakeMatrix(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        public void Dump(StringBuilder sb)
        {
            for (int i = 0; i < nInput; ++i)
            {
                sb.AppendLine("input node [" + i + "] = " + iNodes[i].ToString("F4"));
            }
            for (int h = 0; h < nLayers; ++h)
            {
                sb.AppendLine("");
                for (int j = 0; j < nHidden[h]; ++j)
                {
                    sb.AppendLine("hidden layer " + h + " node [" + j + "] = " + hNodes[h][j].ToString("F4"));
                }
            }
            sb.AppendLine("");
            for (int k = 0; k < nOutput; ++k)
            {
                sb.AppendLine("output node [" + k + "] = " + oNodes[k].ToString("F4"));
            }

            sb.AppendLine("");
            for (int i = 0; i < nInput; ++i)
            {
                for (int j = 0; j < nHidden[0]; ++j)
                {
                    sb.AppendLine("input-hidden wt [" + i + "][" + j + "] = " + ihWeights[i][j].ToString("F4"));
                }
            }

            for (int h = 0; h < nLayers - 1; ++h)  // note
            {
                sb.AppendLine("");
                for (int j = 0; j < nHidden[h]; ++j)
                {
                    for (int jj = 0; jj < nHidden[h + 1]; ++jj)
                    {
                        sb.AppendLine("hidden-hidden wt layer " + h + " to layer " + (h + 1) + " node [" + j + "] to [" + jj + "] = " + hhWeights[h][j][jj].ToString("F4"));
                    }
                }
            }

            sb.AppendLine("");
            for (int j = 0; j < nHidden[nLayers - 1]; ++j)
            {
                for (int k = 0; k < nOutput; ++k)
                {
                    sb.AppendLine("hidden-output wt [" + j + "][" + k + "] = " + hoWeights[j][k].ToString("F4"));
                }
            }

            for (int h = 0; h < nLayers; ++h)
            {
                sb.AppendLine("");
                for (int j = 0; j < nHidden[h]; ++j)
                {
                    sb.AppendLine("hidden layer " + h + " bias [" + j + "] = " + hBiases[h][j].ToString("F4"));
                }
            }

            sb.AppendLine("");
            for (int k = 0; k < nOutput; ++k)
            {
                sb.AppendLine("output node bias [" + k + "] = " + oBiases[k].ToString("F4"));
            }

        } // Dump
    }
        
}
