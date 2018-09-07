using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace DevTreks.Extensions.Algorithms
{
    /// <summary>
    ///Purpose:		ML03 algorithm
    ///Author:		www.devtreks.org
    ///Date:		2018, May
    ///References:	neural network machine learning
    ///adapted from McCaffrey (MSDN, August, September, October, 2017) 
    ///</summary>
    public class ML03 : MLBase
    {
        public ML03()
            : base() { }
        public ML03(int indicatorIndex, string label, string[] mathTerms,
            string[] colNames, string[] depColNames,
            string subalgorithm, int ciLevel, int iterations,
            int random, IndicatorQT1 qT1, CalculatorParameters calcParams)
            : base(indicatorIndex, label, mathTerms,
            colNames, depColNames, subalgorithm, ciLevel, iterations,
            random, qT1, calcParams)
        {}

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

                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_03.ToString())
                {
                    //use sb for feedback when training and testing; but don't use for production
                    StringBuilder sb = new StringBuilder();
                    ////classify testdata and return new dataset
                    sb = await Predict(trainData, rowNames, testData);
                    bHasCalculations = await SetMathResult(rowNames);

                    //debug first with reference dataset and show debugging messages in results
                    //put the results in MathResult
                    //sb = await DebugPredict(trainData, rowNames, testData);
                    //bHasCalculations = await SetMathResult(rowNames, sb);
                }
            }
            catch (Exception ex)
            {
                IndicatorQT.ErrorMessage = ex.Message;
            }
            return bHasCalculations;
        }

        private async Task<StringBuilder> Predict(List<List<string>> trainData,
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
                   this.IndicatorQT, _colNames, _depColNames, normTypes, "F2");
                List<List<double>> testDB = Shared.GetNormalizedDData(testData,
                    this.IndicatorQT, _colNames, _depColNames, normTypes, "F2");
                //make a new list with same matrix, to be replaced with results
                int iColCount = testDB.Count;
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_03.ToString().ToString())
                {
                    //subalgo02 needs qtm and percent probability of accuracy, qtm, low ci, high ci
                    iColCount = testDB.Count + 5;
                    //normtypes need full columns before insertion
                    normTypes = Shared.FixNormTypes(normTypes, iColCount);
                }
                //row count comes from original testdata to account for the instructions row
                DataResults = CalculatorHelpers.GetList(testData.Count, iColCount);
                DataResults[0]= normTypes;
                //dep var output count
                int numOutput = 1;
                //less col[0]
                int numInput = trainDB.Count - 1;
                int numHidden = 12;
                //can truncate the data to iRowCount
                double[][] trainInputs = Shared.MakeInputDData(trainDB, iRowCount, this.IndicatorQT,
                    numInput);
                //build a neural network
                NeuralNetwork2 nn2 = new NeuralNetwork2(numInput, numHidden, numOutput);
                int maxEpochs = iRowCount;
                double learnRate = 0.001;
                //train nn2
                double[] wts = nn2.Train(trainInputs, maxEpochs, learnRate, sb);
                //mean squared error
                double trainErr = nn2.Error(trainInputs);
                //final model accuracy
                double trainAcc = nn2.Accuracy(trainInputs, dbPlusorMinus);
                //add classified test data to DataResults
                bool bHasNewClassifs = await AddNewClassifications(nn2, testDB,
                    trainAcc, trainErr, iRowCount, dbPlusorMinus, _ciLevel);
            }
            catch (Exception ex)
            {
                IndicatorQT.ErrorMessage = ex.Message;
            }
            return sb;
        }
        
        static double[][] MakeData(List<List<double>> trainData, int numItems,
            IndicatorQT1 qt1, int numInput)
        {
            
            // make the result matrix holder (now first index is row)
            double[][] result = new double[numItems][];
            for (int r = 0; r < numItems; ++r)
                // allocate the cols
                result[r] = new double[numInput];
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
                        inputs[i] = dbInput;
                    }
                    int c = 0;
                    for (int i = 0; i < numInput; ++i)
                        result[r][c++] = inputs[i];
                }
            }
            return result;
        }
        
        static void ShowMatrix(StringBuilder sb, double[][] matrix, int numRows,
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

        static void ShowVector(StringBuilder sb, double[] vector, int decimals,
          int lineLen, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                if (i > 0 && i % lineLen == 0) sb.AppendLine("");
                if (vector[i] >= 0) sb.Append(" ");
                sb.Append(vector[i].ToString("F" + decimals) + " ");
            }
            if (newLine == true)
                sb.AppendLine("");
        }
        private async Task<bool> AddNewClassifications(NeuralNetwork2 nn2, List<List<double>> testData,
            double trainAccuracy, double mse, int rowCount, double hiLow, int ciPercent)
        {
            bool bHasNewClassifs = false;
            if (ciPercent > 5)
            {
                hiLow = CalculatorHelpers.GetConfidenceIntervalFromMSE(
                    ciPercent, rowCount, mse);
            }
            int iRowLength = 0;
            string sResult = string.Empty;
            List<double> predictors = new List<double>();
            double dbInput = 0;
            double dbTarget = 0;
            //test data stores cols in testdata[0] first index
            for (int r = 0; r < DataResults.Count - 1; r++)
            {
                if (r == 0)
                {
                    iRowLength = DataResults[r + 1].Count;
                }
                predictors = new List<double>();
                for (int c = 0; c < testData.Count; c++)
                {
                    //prepare mathresults
                    dbInput = testData[c][r];
                    DataResults[r + 1][c] = dbInput.ToString("F4");
                    if (c > 0)
                    {
                        predictors.Add(dbInput);
                    }
                }
                //predict 
                double[] forecast = nn2.ComputeOutputs(predictors.ToArray());
                double dbMostLikelyQTM = forecast[0];
                if (r == 0)
                {
                    DataResults[r][iRowLength - 5] = mse.ToString("F4");
                    DataResults[r][iRowLength - 4] = trainAccuracy.ToString("F4");
                }
                dbTarget = testData[0][r];
                double dbError = Math.Abs(dbTarget - dbMostLikelyQTM);
                this.DataResults[r + 1][iRowLength - 5] = dbError.ToString("F4");
                if (dbError < hiLow)
                {
                    DataResults[r + 1][iRowLength - 4] = "true";
                }
                else
                {
                    DataResults[r + 1][iRowLength - 4] = "false";
                }
                DataResults[r + 1][iRowLength - 3] = dbMostLikelyQTM.ToString("F4");
                if (r == DataResults.Count - 2)
                {
                    this.IndicatorQT.QTM = dbMostLikelyQTM;
                    this.IndicatorQT.QTL = dbMostLikelyQTM - hiLow;
                    this.IndicatorQT.QTU = dbMostLikelyQTM + hiLow;
                }
                //low estimation
                DataResults[r + 1][iRowLength - 2] = (dbMostLikelyQTM - hiLow).ToString("F4");
                //high estimation
                DataResults[r + 1][iRowLength - 1] = (dbMostLikelyQTM + hiLow).ToString("F4");
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
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_03.ToString())
                {
                    sb.AppendLine("ml results");
                    string[] newColNames = new string[_actualColNames.Length + 5];
                    for (int i = 0; i < _actualColNames.Length; i++)
                    {
                        newColNames[i] = _actualColNames[i];
                    }
                    //new cols changed by algo
                    newColNames[_actualColNames.Length] = "mse";
                    newColNames[_actualColNames.Length + 1] = "accuracy";
                    newColNames[_actualColNames.Length + 2] = "qtm";
                    newColNames[_actualColNames.Length + 3] = "qtl";
                    newColNames[_actualColNames.Length + 4] = "qtu";
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
    public class NeuralNetwork2
    {
        private int numInput; // number input nodes
        private int numHidden;
        private int numOutput;

        private double[] iNodes;
        private double[][] ihWeights; // input-hidden
        private double[] hBiases;
        private double[] hNodes;

        private double[][] hoWeights; // hidden-output
        private double[] oBiases;
        private double[] oNodes;
        public double MSE;

        private Random rnd;

        public NeuralNetwork2(int numInput, int numHidden, int numOutput)
        {
            this.numInput = numInput;
            this.numHidden = numHidden;
            this.numOutput = numOutput;

            this.iNodes = new double[numInput];

            this.ihWeights = MakeMatrix(numInput, numHidden, 0.0);
            this.hBiases = new double[numHidden];
            this.hNodes = new double[numHidden];

            this.hoWeights = MakeMatrix(numHidden, numOutput, 0.0);
            this.oBiases = new double[numOutput];
            this.oNodes = new double[numOutput];

            this.rnd = new Random(0);
            // all weights and biases
            this.InitializeWeights();
            //mean squared error
            this.MSE = 0;
        } // ctor

        private static double[][] MakeMatrix(int rows,
          int cols, double v) // helper for ctor, Train
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = v;
            return result;
        }

        private void InitializeWeights() // helper for ctor
        {
            // initialize weights and biases to small random values
            int numWeights = (numInput * numHidden) +
              (numHidden * numOutput) + numHidden + numOutput;
            double[] initialWeights = new double[numWeights];
            for (int i = 0; i < initialWeights.Length; ++i)
                initialWeights[i] = (0.001 - 0.0001) * rnd.NextDouble() + 0.0001;
            this.SetWeights(initialWeights);
        }

        public void SetWeights(double[] weights)
        {
            // copy serialized weights and biases in weights[] array
            // to i-h weights, i-h biases, h-o weights, h-o biases
            int numWeights = (numInput * numHidden) +
              (numHidden * numOutput) + numHidden + numOutput;
            if (weights.Length != numWeights)
                throw new Exception("Bad weights array in SetWeights");
            // points into weights param
            int k = 0; 

            for (int i = 0; i < numInput; ++i)
                for (int j = 0; j < numHidden; ++j)
                    ihWeights[i][j] = weights[k++];
            for (int i = 0; i < numHidden; ++i)
                hBiases[i] = weights[k++];
            for (int i = 0; i < numHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    hoWeights[i][j] = weights[k++];
            for (int i = 0; i < numOutput; ++i)
                oBiases[i] = weights[k++];
        }

        public double[] GetWeights()
        {
            int numWeights = (numInput * numHidden) +
              (numHidden * numOutput) + numHidden + numOutput;
            double[] result = new double[numWeights];
            int k = 0;
            for (int i = 0; i < ihWeights.Length; ++i)
                for (int j = 0; j < ihWeights[0].Length; ++j)
                    result[k++] = ihWeights[i][j];
            for (int i = 0; i < hBiases.Length; ++i)
                result[k++] = hBiases[i];
            for (int i = 0; i < hoWeights.Length; ++i)
                for (int j = 0; j < hoWeights[0].Length; ++j)
                    result[k++] = hoWeights[i][j];
            for (int i = 0; i < oBiases.Length; ++i)
                result[k++] = oBiases[i];
            return result;
        }

        public double[] ComputeOutputs(double[] xValues)
        {
            // hidden nodes sums scratch array
            double[] hSums = new double[numHidden];
            // output nodes sums
            double[] oSums = new double[numOutput];
            // copy x-values to inputs
            for (int i = 0; i < xValues.Length; ++i) 
                this.iNodes[i] = xValues[i];
            // compute i-h sum of weights * inputs
            for (int j = 0; j < numHidden; ++j)  
                for (int i = 0; i < numInput; ++i)
                    hSums[j] += this.iNodes[i] * this.ihWeights[i][j]; // note +=
            // add biases to hidden sums
            for (int i = 0; i < numHidden; ++i)  
                hSums[i] += this.hBiases[i];
            // apply activation
            for (int i = 0; i < numHidden; ++i)
                // hard-coded
                this.hNodes[i] = HyperTan(hSums[i]);
            // compute h-o sum of weights * hOutputs
            for (int j = 0; j < numOutput; ++j)  
                for (int i = 0; i < numHidden; ++i)
                    oSums[j] += hNodes[i] * hoWeights[i][j];
            // add biases to output sums
            for (int i = 0; i < numOutput; ++i)  
                oSums[i] += oBiases[i];
            // really only 1 value
            Array.Copy(oSums, this.oNodes, oSums.Length);  

            double[] retResult = new double[numOutput]; 
            Array.Copy(this.oNodes, retResult, retResult.Length);
            return retResult;
        }

        private static double HyperTan(double x)
        {
            if (x < -20.0) return -1.0; // approximation is correct to 30 decimals
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        private static double LogSig(double x)
        {
            if (x < -20.0) return 0.0; // approximation
            else if (x > 20.0) return 1.0;
            else return 1.0 / (1.0 + Math.Exp(x));
        }

        public double[] Train(double[][] trainData, int maxEpochs,
          double learnRate, StringBuilder sb = null)
        {
            // train using back-prop
            // back-prop specific arrays
            // hidden-to-output weight gradients
            double[][] hoGrads = MakeMatrix(numHidden, numOutput, 0.0);
            // output bias gradients
            double[] obGrads = new double[numOutput];
            // input-to-hidden weight gradients
            double[][] ihGrads = MakeMatrix(numInput, numHidden, 0.0);
            // hidden bias gradients
            double[] hbGrads = new double[numHidden];
            // local gradient output signals
            double[] oSignals = new double[numOutput];
            // local gradient hidden node signals
            double[] hSignals = new double[numHidden];                  

            int epoch = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // target values
            double derivative = 0.0;
            double errorSignal = 0.0;

            int[] sequence = new int[trainData.Length];
            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;
            // interval to check error
            int errInterval = maxEpochs / 5; 
            while (epoch < maxEpochs)
            {
                ++epoch;

                if (epoch % errInterval == 0 && epoch < maxEpochs)
                {
                    if (sb != null)
                    {
                        double trainErr = Error(trainData);
                        sb.AppendLine("epoch = " + epoch + "  error = " +
                          trainErr.ToString("F4"));
                    }
                }
                // visit each training data in random order
                Shuffle(sequence); 
                for (int ii = 0; ii < trainData.Length; ++ii)
                {
                    int idx = sequence[ii];
                    Array.Copy(trainData[idx], xValues, numInput);
                    //always use dep var col[0]
                    Array.Copy(trainData[idx], 0, tValues, 0, numOutput);
                    ComputeOutputs(xValues); // copy xValues in, compute outputs 

                    // indices: i = inputs, j = hiddens, k = outputs

                    // 1. compute output node signals (assumes softmax)
                    for (int k = 0; k < numOutput; ++k)
                    {
                        errorSignal = tValues[k] - oNodes[k];  // Wikipedia uses (o-t)
                        // for Identity activation
                        derivative = 1.0;  
                        oSignals[k] = errorSignal * derivative;
                    }

                    // 2. compute hidden-to-output weight gradients using output signals
                    for (int j = 0; j < numHidden; ++j)
                        for (int k = 0; k < numOutput; ++k)
                            hoGrads[j][k] = oSignals[k] * hNodes[j];

                    // 2b. compute output bias gradients using output signals
                    for (int k = 0; k < numOutput; ++k)
                        obGrads[k] = oSignals[k] * 1.0; // dummy assoc. input value

                    // 3. compute hidden node signals
                    for (int j = 0; j < numHidden; ++j)
                    {
                        derivative = (1 + hNodes[j]) * (1 - hNodes[j]); // for tanh
                        double sum = 0.0; // need sums of output signals times hidden-to-output weights
                        for (int k = 0; k < numOutput; ++k)
                        {
                            sum += oSignals[k] * hoWeights[j][k]; // represents error signal
                        }
                        hSignals[j] = derivative * sum;
                    }

                    // 4. compute input-hidden weight gradients
                    for (int i = 0; i < numInput; ++i)
                        for (int j = 0; j < numHidden; ++j)
                            ihGrads[i][j] = hSignals[j] * iNodes[i];

                    // 4b. compute hidden node bias gradients
                    for (int j = 0; j < numHidden; ++j)
                        hbGrads[j] = hSignals[j] * 1.0; // dummy 1.0 input

                    // == update weights and biases

                    // update input-to-hidden weights
                    for (int i = 0; i < numInput; ++i)
                    {
                        for (int j = 0; j < numHidden; ++j)
                        {
                            double delta = ihGrads[i][j] * learnRate;
                            ihWeights[i][j] += delta; // would be -= if (o-t)
                        }
                    }

                    // update hidden biases
                    for (int j = 0; j < numHidden; ++j)
                    {
                        double delta = hbGrads[j] * learnRate;
                        hBiases[j] += delta;
                    }

                    // update hidden-to-output weights
                    for (int j = 0; j < numHidden; ++j)
                    {
                        for (int k = 0; k < numOutput; ++k)
                        {
                            double delta = hoGrads[j][k] * learnRate;
                            hoWeights[j][k] += delta;
                        }
                    }

                    // update output node biases
                    for (int k = 0; k < numOutput; ++k)
                    {
                        double delta = obGrads[k] * learnRate;
                        oBiases[k] += delta;
                    }

                } // each training item

            } // while
            double[] bestWts = GetWeights();
            return bestWts;
        } // Train

        private void Shuffle(int[] sequence) // instance method
        {
            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = this.rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }
        } // Shuffle

        public double Error(double[][] trainData)
        {
            // average squared error per training item
            double sumSquaredError = 0.0;
            // first numInput values in trainData
            double[] xValues = new double[numInput];
            // last numOutput values
            double[] tValues = new double[numOutput]; 

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < trainData.Length; ++i)
            {
                Array.Copy(trainData[i], xValues, numInput);
                //always use dep var col[0]
                Array.Copy(trainData[i], 0, tValues, 0, numOutput);
                // outputs using current weights
                double[] yValues = this.ComputeOutputs(xValues); 
                for (int j = 0; j < numOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            return sumSquaredError / trainData.Length;
        } // MeanSquaredError

        public double Accuracy(double[][] testData, double howClose)
        {
            // percentage correct using winner-takes all
            int numCorrect = 0;
            int numWrong = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // targets
            double[] yValues; // computed Y

            for (int i = 0; i < testData.Length; ++i)
            {
                // get x-values
                Array.Copy(testData[i], xValues, numInput);
                // get t-values
                //always use dep var col[0]
                Array.Copy(testData[i], 0, tValues, 0, numOutput); 
                yValues = this.ComputeOutputs(xValues);
                // howclose is passed in training dataset
                if (Math.Abs(yValues[0] - tValues[0]) < howClose)
                {
                    ++numCorrect;
                }
                else
                {
                    ++numWrong;
                }
            }
            return (numCorrect * 1.0) / (numCorrect + numWrong);
        }
    } // class NeuralNetwork2
}
