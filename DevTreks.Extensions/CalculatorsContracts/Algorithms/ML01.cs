using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
//using Accord.MachineLearning.Bayes;
//using Accord.Statistics.Filters;

namespace DevTreks.Extensions.Algorithms
{
    /// <summary>
    ///Purpose:		ML01 algorithm
    ///Author:		www.devtreks.org
    ///Date:		2018, May
    ///References:	naive bayes machine learning
    ///adapted from McCaffrey (MSDN, February, 2013) 
    ///</summary>
    public class ML01 : MLBase
    {
        public ML01()
            : base() { }
        public ML01(int indicatorIndex, string label, string[] mathTerms,
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
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_01.ToString())
                {
                    //use sb for feedback when training and testing; 
                    //but use SetMathResult for reporting
                    StringBuilder sb = new StringBuilder();
                    //classify testdata and return new dataset
                    if (IndicatorQT.Label2.ToLower() == "accord")
                    {
                        ////use the accord ml library
                        //sb = await ClassifyAccord(trainData, rowNames, testData);
                    }
                    else
                    {
                        //use standard dotnet libraries
                        sb = await Classify(trainData, rowNames, testData);
                    }
                    bHasCalculations = await SetMathResult(rowNames);
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
                //columns of data used and returned in DataResults
                int iRowCount = (Shared.GetRowCount(_iterations, trainData.Count) - 1);
                _actualColNames = Shared.GetActualColNames(_colNames, _depColNames).ToArray();
                //ml instructions associated with actual colNames
                List<string> normTypes = Shared.GetNormTypes(trainData[0], _colNames, _depColNames);
                //instructions in both row names and datasets
                List<string> actualMLInstructs = Shared.GetAlgoInstructs(rowNames);
                actualMLInstructs.AddRange(normTypes);
                // prevent joint counts with 0
                bool withLaplacian 
                    = actualMLInstructs[0].ToLower().Contains("true") ? true : false;
                //converts rows to columns with normalized data
                List<List<string>> trainDB = Shared.GetNormalizedSData(trainData,
                    this.IndicatorQT, _colNames, _depColNames, normTypes, "F0");
                List<List<string>> testDB = Shared.GetNormalizedSData(testData,
                    this.IndicatorQT, _colNames, _depColNames, normTypes, "F0");
                int iColCount = testDB.Count;
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_01.ToString().ToString())
                {
                    //subalgo02 needs qtm and percent probability of accuracy, mse, qtm, low ci, high ci
                    iColCount = testDB.Count + 2;
                }
                //row count comes from original testdata to account for the instructions row
                DataResults = CalculatorHelpers.GetList(testData.Count, iColCount);
                DataResults[0] = normTypes;
                // trainData columns define number of rows (depcolumns.Length + 1)
                string[][] attributeValues = new string[trainDB.Count][];
                //for each column of trainDB, fill in the unique attribute names (i.e. gender = 2 unique atts)
                for (int i = 0; i < trainDB.Count; i++)
                {
                    attributeValues[i] = Shared.GetAttributeGroups(i, trainDB, this.IndicatorQT);
                }
                int[][][] jointCounts = MakeJointCounts(trainDB, attributeValues);
                int[] dependentCounts = MakeDependentCounts(jointCounts, attributeValues[0].Length);
                //classify everything in test dataset and add result to new columns in test dataset
                List<string> predictors = new List<string>();
                int d = 0;
                int iRowLength = DataResults[1].Count;
                string sAttribute = string.Empty;
                for (int r = 0; r < DataResults.Count - 1; r++)
                {
                    predictors = new List<string>();
                    //cols have separate set of predictors
                    for (int j = 0; j < testDB.Count; j++)
                    {
                        //prepare mathresults
                        DataResults[r+1][j] = testDB[j][r];
                        if (j > 0)
                        {
                            //going down the rows (j) in the column (r)
                            predictors.Add(testDB[j][r]);
                        }
                    }
                    d = await Classify(r+1, attributeValues, predictors.ToArray(),
                        jointCounts, dependentCounts, withLaplacian, attributeValues.Length - 1);
                    for (int l = 0; l < attributeValues[0].Length; l++)
                    {
                        if (d == l)
                        {
                            sAttribute = Shared.ConvertAttributeToLabel(attributeValues[0][l], 
                                this.IndicatorQT);
                            DataResults[r+1][iRowLength - 2] = sAttribute;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IndicatorQT.ErrorMessage = ex.Message;
            }
            return sb;
        }
        //private async Task<StringBuilder> ClassifyAccord(List<List<string>> trainData,
        //    List<List<string>> rowNames, List<List<string>> testData)
        //{
        //    StringBuilder sb = null;
        //    try
        //    {
        //        int iRowCount = (Shared.GetRowCount(_iterations, trainData.Count) - 1);
        //        //columns of data used and returned in DataResults
        //        _actualColNames = Shared.GetActualColNames(_colNames, _depColNames).ToArray();
        //        //ml instructions associated with actual colNames
        //        List<string> normTypes = Shared.GetNormTypes(trainData[0], _colNames, _depColNames);
        //        //instructions in both row names and datasets
        //        List<string> actualMLInstructs = Shared.GetAlgoInstructs(rowNames);
        //        actualMLInstructs.AddRange(normTypes);
        //        // prevent joint counts with 0
        //        bool withLaplacian
        //            = actualMLInstructs[0].ToLower().Contains("true") ? true : false;
        //        //converts rows to columns with normalized data
        //        List<List<string>> trainDB = Shared.GetNormalizedSData(trainData,
        //            this.IndicatorQT, _colNames, _depColNames, normTypes, "F0");
        //        List<List<string>> testDB = Shared.GetNormalizedSData(testData,
        //            this.IndicatorQT, _colNames, _depColNames, normTypes, "F0");
        //        int iColCount = testDB.Count;
        //        if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_01.ToString().ToString())
        //        {
        //            //subalgo02 needs qtm and percent probability of accuracy, mse, qtm, low ci, high ci
        //            iColCount = testDB.Count + 2;
        //            //normtypes need full columns before insertion
        //            normTypes = Shared.FixNormTypes(normTypes, iColCount);
        //        }
        //        //row count comes from original testdata to account for the instructions row
        //        DataResults = CalculatorHelpers.GetList(testData.Count, iColCount);
        //        DataResults[0] = normTypes;
        //        // testData columns define number of rows (depcolumns.Length + 1)
        //        string[][] attributeValues = new string[testDB.Count][];
        //        //for each column of testDB, fill in the unique attribute names 
        //        for (int i = 0; i < testDB.Count; i++)
        //        {
        //            attributeValues[i] = Shared.GetAttributeGroups(i, testDB, this.IndicatorQT);
        //        }
        //        //convert column-row to std row column array
        //        string[][] trainInsandOuts = Shared.MakeSData(trainDB, iRowCount, this.IndicatorQT);
        //        // Create a new codification codebook to
        //        // convert strings into discrete symbols
        //        string[] colNames = new string[_depColNames.Length + 1];
        //        colNames[0] = _colNames[3];
        //        _depColNames.CopyTo(colNames, 1);
        //        //codebook converts to integer classifs
        //        Codification codebook = new Codification(colNames, trainInsandOuts);
        //        // Extract input and output pairs to train
        //        int[][] insAndOuts = codebook.Transform(trainInsandOuts);
        //        int[][] inputs = Shared.GetInputs(insAndOuts);
        //        int[] outputs = Shared.GetOutputs(insAndOuts);
        //        // Create a new Naive Bayes learning
        //        var learner = new NaiveBayesLearning();
        //        NaiveBayes nb = learner.Learn(inputs, outputs);
        //        //same transform with test dataset
        //        string[][] testInsandOuts = Shared.MakeSData(testDB, testDB[0].Count, this.IndicatorQT);
        //        codebook = new Codification(colNames, testInsandOuts);
        //        // Extract input and output pairs to train
        //        insAndOuts = codebook.Transform(testInsandOuts);
        //        inputs = Shared.GetInputs(insAndOuts);
        //        string sAttribute = string.Empty;
        //        string result = string.Empty;
        //        double dbHighestP = 0;
        //        int iRowLength = DataResults[1].Count;
        //        int[] instance = new int[] { };
        //        int c = 0;
        //        double[] probs = new double[] { };
        //        for (int r = 0; r < DataResults.Count - 1; r++)
        //        {
        //            instance = inputs[r];
        //            for (int j = 0; j < inputs[r].Length; j++)
        //            {
        //                //prepare mathresults
        //                DataResults[r + 1][j] = inputs[r][j].ToString("F4");
        //            }
        //            // numeric output that represents the answer
        //            c = nb.Decide(instance);
        //            //convert the numeric output to original output
        //            result = codebook.Revert(colNames[0], c); 
        //            //get the probabilities
        //            probs = nb.Probabilities(instance); 
        //            //get the probability associated with the answer
        //            for(int p = 0; p < probs.Length; p++)
        //            {
        //                if (probs[p] > dbHighestP)
        //                {
        //                    dbHighestP = probs[p];
        //                }
        //            }
        //            DataResults[r + 1][iRowLength - 1] = dbHighestP.ToString("F4");
        //            for (int l = 0; l < attributeValues[0].Length; l++)
        //            {
        //                if (attributeValues[0][l] == result)
        //                {
        //                    sAttribute = Shared.ConvertAttributeToLabel(attributeValues[0][l], this.IndicatorQT);
        //                    DataResults[r + 1][iRowLength - 2] = sAttribute;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        IndicatorQT.ErrorMessage = ex.Message;
        //    }
        //    return sb;
        //}
        private int[][][] MakeJointCounts(List<List<string>> trainData,
            string[][] attributeValues)
        {
            // assumes binned trainData is occupation, dominance, height, sex
            // result[][][] -> [attribute][att value][sex]
            // ex: result[0][3][1] is the count of (occupation) (technology) (female), i.e., the count of technology AND female
            // note the -1 (no label or dep variable in joint counts)
            int[][][] jointCounts = new int[attributeValues.Length - 1][][];
            //first column holds labels and first row has label names
            int iLabelCount = attributeValues[0].Length;
            for (int i = 1; i < attributeValues.Length; i++)
            {
                //features start in 2nd row
                jointCounts[i - 1] = new int[attributeValues[i].Length][];
                for (int j = 0; j < attributeValues[i].Length; j++)
                {
                    //number of labels for each feature
                    jointCounts[i - 1][j] = new int[iLabelCount];
                }
            }
            //skip the dep var column
            for (int i = 1; i < attributeValues.Length; ++i)
            {
                //rows
                for (int k = 0; k < trainData[i].Count; k++)
                {
                    int iLabelIndex = AttributeValueToLabelIndex(trainData[0][k], attributeValues);
                    int iFeatureIndex = 0;
                    //feature comes from each column
                    string sFeature = trainData[i][k];
                    iFeatureIndex = AttributeValueToIndex(i, sFeature, attributeValues);
                    if (jointCounts[i - 1].Length > iFeatureIndex)
                    {
                        ++jointCounts[i - 1][iFeatureIndex][iLabelIndex];
                    }
                }
            }
            return jointCounts;
        }
        
        private int AttributeValueToLabelIndex(string attributeValue, string[][] attributeValues)
        {
            int iAttributeIndex = -1;
            //string sAttribute = string.Empty;
            //labels are in atts[0]
            for (int j = 0; j < attributeValues[0].Length; j++)
            {
                //sAttribute = Shared.ConvertAttributeToLabel(attributeValue, this.IndicatorQT);
                if (attributeValue.Equals(attributeValues[0][j]))
                {
                    iAttributeIndex = j;
                    break;
                }
            }
            return iAttributeIndex;
        }
        static int AttributeValueToIndex(int firstIndex, string attributeValue, string[][] attributeValues)
        {
            int iAttributeIndex = -1;
            if (firstIndex < attributeValues.Length)
            {
                for (int j = 0; j < attributeValues[firstIndex].Length; j++)
                {
                    if (attributeValue.Equals(attributeValues[firstIndex][j]))
                    {
                        iAttributeIndex = j;
                        break;
                    }
                }
            }
            return iAttributeIndex;
        }

        static void ShowJointCounts(StringBuilder sb, int[][][] jointCounts, string[][] attributeValues)
        {
            string sFeature = string.Empty;
            for (int k = 0; k < attributeValues[0].Length; ++k)
            {
                for (int i = 0; i < jointCounts.Length; ++i)
                    for (int j = 0; j < jointCounts[i].Length; ++j)
                        if (((i+1) < attributeValues.Length) 
                            && (j < attributeValues[i+1].Length))
                        {
                            sb.AppendLine(string.Concat(attributeValues[i+1][j].PadRight(15),
                                "& ", attributeValues[0][k].PadRight(6),
                                " = ", jointCounts[i][j][k]));
                        }
                // separate sexes
                sb.AppendLine(""); 
            }
        }

        static int[] MakeDependentCounts(int[][][] jointCounts, int numDependents)
        {
            int[] result = new int[numDependents];
            // male then female
            for (int k = 0; k < numDependents; ++k)
                // scanning attribute 0 = occupation. could use any attribute
                for (int j = 0; j < jointCounts[0].Length; ++j) 
                    result[k] += jointCounts[0][j][k];

            return result;
        }

        private async Task<int> Classify(int row, string[][] attributeValues, 
            string[] featuresToTest, int[][][] jointCounts, int[] dependentCounts, 
            bool withSmoothing, int xClasses, StringBuilder sb = null)
        {
            double partProbLabel = 0;
            double totalProbability = 0;
            //1st row holds the dependent vars classifations
            double[] partProbLabels = new double[attributeValues[0].Length];
            for (int k = 0; k < attributeValues[0].Length; ++k)
            {
                partProbLabel = await PartialProbability(attributeValues[0][k], attributeValues,
                    featuresToTest, jointCounts, dependentCounts, 
                    withSmoothing, xClasses);
                totalProbability += partProbLabel;
                partProbLabels.SetValue(partProbLabel, k);
            }
            double fullProbability = 0;
            double highestProbability = 0;
            int iLabelIndex = -1;
            for (int k = 0; k < attributeValues[0].Length; ++k)
            {
                if (sb != null)
                {
                    sb.AppendLine(string.Concat("Partial prob of ",
                        attributeValues[0][k], "= ", partProbLabels[k].ToString("F6")));
                }
                if (partProbLabels[k] > highestProbability)
                {
                    highestProbability = partProbLabels[k];
                    iLabelIndex = k;
                }
                fullProbability = partProbLabels[k] / totalProbability;
                if (sb != null)
                {
                    sb.AppendLine(string.Concat("Probability of ",
                        attributeValues[0][k], "= ", fullProbability.ToString("F4")));
                }
                else
                {
                    int iRowLength = DataResults[row].Count;
                    DataResults[row][iRowLength - 1] = fullProbability.ToString("F4");
                }
            }
            //return label that has highest probability
            return iLabelIndex;
        }
        
        private static async Task<double> PartialProbability(string label, string[][] attributeValues, string[] featuresToTest, 
            int[][][] jointCounts, int[] dependentCounts, bool withSmoothing, int xClasses)
        {
            int iLabelIndex = AttributeValueToIndex(0, label, attributeValues);
            int iFeatureIndex = 0;
            //test data includes dependent var with na or none
            int[] iFeatureIndexes = new int[featuresToTest.Length];
            int i = 0;
            foreach (string feature in featuresToTest)
            {
                //attvalues include depvar row
                iFeatureIndex = AttributeValueToIndex(i + 1, feature, attributeValues);
                if (i < iFeatureIndexes.Length)
                {
                    iFeatureIndexes[i] = iFeatureIndex;
                }
                i++;
            }
            int totalCases = 0;
            for (i = 0; i < dependentCounts.Length; i++)
            {
                totalCases += dependentCounts[i];
            }
            int totalToUse = 0;
            i = 0;
            foreach (string attLabel in attributeValues[0])
            {
                if (attLabel.Equals(label))
                {
                    if (i < dependentCounts.Length)
                    {
                        totalToUse = dependentCounts[i];
                        break;
                    }
                }
                i++;
            }
            double[] ps = new double[iFeatureIndexes.Length + 1];
            // prob of either male or female
            double p0 = (totalToUse * 1.0) / (totalCases);
            ps.SetValue(p0, 0);
            double p1 = 0.0;
            if (withSmoothing == false)
            {
                i = 0;
                foreach (int featureIndex in iFeatureIndexes)
                {
                    // conditional prob of label given the features
                    p1 = (jointCounts[i][featureIndex][iLabelIndex] * 1.0) / totalToUse;
                    if (ps.Length > (i + 1))
                    {
                        ps.SetValue(p1, i + 1);
                    }
                    i++;
                }    
            }
            else if (withSmoothing == true) 
            {
                // Laplacian smoothing to avoid 0-count joint probabilities
                i = 0;
                foreach (int featureIndex in iFeatureIndexes)
                {
                    // conditional prob of label given the features
                    p1 = (jointCounts[i][featureIndex][iLabelIndex] + 1) / ((totalToUse + xClasses) * 1.0);
                    if (ps.Length > (i + 1))
                    {
                        ps.SetValue(p1, i + 1);
                    }
                    i++;
                }
            }
            double dbTotalProbability = 0;
            foreach (double p in ps)
            {
                dbTotalProbability += Math.Log(p);
            }
            dbTotalProbability = Math.Exp(dbTotalProbability);
            return dbTotalProbability;
        }
        
        static int AnalyzeJointCounts(int[][][] jointCounts)
        {
            // check for any joint-counts that are 0 which could blow up Naive Bayes
            int zeroCounts = 0;
            // attribute
            for (int i = 0; i < jointCounts.Length; ++i)
                // value
                for (int j = 0; j < jointCounts[i].Length; ++j)
                    // label
                    for (int k = 0; k < jointCounts[i][j].Length; ++k) 
                        if (jointCounts[i][j][k] == 0)
                            ++zeroCounts;
            return zeroCounts;
        }
        private async Task<bool> SetMathResult(List<List<string>> rowNames, StringBuilder sb = null)
        {
            bool bHasSet = false;
            if (sb == null)
            {
                //add the data to a string builder
                sb = new StringBuilder();
                if (_subalgorithm == MATHML_SUBTYPES.subalgorithm_01.ToString())
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
}
