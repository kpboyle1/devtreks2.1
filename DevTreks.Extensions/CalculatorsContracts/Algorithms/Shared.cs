using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using System.Globalization;
using System.Threading.Tasks;

namespace DevTreks.Extensions.Algorithms
{
    /// <summary>
    ///Purpose:		Shared static functions that support algos
    ///Author:		www.devtreks.org
    ///Date:		2018, May
    ///References:	Stock, M&E, CTA, and Social Performance tutorials
    ///</summary>
    public static class Shared
    {
        public enum TRANSFORM_DATA_TYPE
        {
            none = 0,
            categories = 1,
            indexes = 2,
            normalized = 3
        }
        public static double[] t025 = new double[] { 12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228, 2.201,
            2.179, 2.160, 2.145, 2.131, 2.120, 2.110, 2.101, 2.093, 2.086, 2.080, 2.074, 2.069, 2.064, 2.060, 2.056, 2.052,
            2.048, 2.045, 1.96, };
        public static double GetTStat95(int df)
        {
            double tstat = 0;
            ////2 sided test for 95% ci
            //tstat = MathNet.Numerics.ExcelFunctions.TInv(0.025, df);

            //1 sided result
            //tstat = t025[t025.Count() - 1];
            ////df is one based
            if (df > 29)
            {
                //1 sided result
                tstat = t025[t025.Count() - 1];
            }
            else
            {
                tstat = t025[df - 1];
            }
            return tstat;
        }
        public static double GetPValueForFDist(int dfn, int dfd, double testValue)
        {
            //f dist = fishersnedecor distribution
            double pvalue = MathNet.Numerics.Distributions.FisherSnedecor.CDF(dfn, dfd, testValue);
            pvalue = 1 - pvalue;
            //which is equal to:
            //double pv1 = MathNet.Numerics.ExcelFunctions.FDist(testValue, dfn, dfd);
            pvalue = Math.Round(pvalue, 4);
            if (pvalue == 0)
            {
                pvalue = .0001;
            }
            if (pvalue < 0)
            {
                pvalue = pvalue * -1;
            }
            //1d - FisherSnedecor.CDF(degreesFreedom1, degreesFreedom2, x)
            //FDIST(15.20675,6,4) equals 0.01
            //FINV(0.01,6,4) equals 15.20675
            return pvalue;
        }
        public static double GetPValueForTDist(int df, double testValue, double mean, double variance)
        {
            if (mean < 0)
            {
                mean = mean * -1;
            }
            if (variance == 0)
            {
                variance = 1;
            }
            double lowertail = MathNet.Numerics.Distributions.StudentT.CDF(mean, variance, df, testValue);
            double uppertail = MathNet.Numerics.Distributions.StudentT.CDF(mean, variance, df, (testValue * -1));
            double pvalue = 2 * lowertail;
            if (testValue > 0)
            {
                pvalue = 2 * uppertail;
            }
            pvalue = Math.Round(pvalue, 4);
            if (pvalue == 0)
            {
                pvalue = .0001;
            }
            if (pvalue < 0)
            {
                pvalue = pvalue * -1;
            }
            //TDIST(1.96,60,2) equals 0.054645, or 5.46 percent
            //TINV(0.054645,60) equals 1.96
            return pvalue;
        }
        public static bool IsDouble(string test)
        {
            bool bIsDouble = false;
            double dbTest = CalculatorHelpers.ConvertStringToDouble(test);
            if (dbTest != 0)
            {
                bIsDouble = true;
            }
            return bIsDouble;
        }
        public static void AddStringArrayToDoubleArray(string[] strArray,
            List<List<double>> trends)
        {
            //trends
            List<double> trend = new List<double>();
            for (int k = 0; k < strArray.Count(); k++)
            {
                trend.Add(CalculatorHelpers
                    .ConvertStringToDouble(strArray[k]));
            }
            trends.Add(trend);
        }
        public static string[] AddStringArrayToStringArray(string[] strArray,
            List<List<double>> trends, int rowIndex)
        {
            string[] newArray = new string[] { };
            if (trends.Count > rowIndex)
            {
                List<double> trend = trends[rowIndex];
                newArray = new string[trend.Count];
                if (strArray == null)
                    strArray = new string[trend.Count];
                if (strArray.Count() == 0)
                {
                    strArray = new string[trend.Count];
                }
                double trendValue = 0;
                double stringArrayValue = 0;
                for (int k = 0; k < strArray.Count(); k++)
                {
                    stringArrayValue = CalculatorHelpers
                        .ConvertStringToDouble(strArray[k]);
                    if (trend.Count > k)
                    {
                        trendValue = trend[k] + stringArrayValue;
                        newArray[k] = trendValue.ToString();
                    }
                }
            }
            return newArray;
        }
        public static string[] AddStringArrayToStringArray(string[] strArray, int strArrayCount,
            string[] strArray2)
        {
            string[] newArray = new string[strArrayCount];
            if (strArray == null)
                strArray = new string[strArrayCount];
            if (strArray.Count() == 0)
            {
                strArray = new string[strArrayCount];
            }
            if (strArray2 == null)
                strArray2 = new string[strArrayCount];
            if (strArray2.Count() == 0)
            {
                strArray2 = new string[strArrayCount];
            }
            double stringArrayValue = 0;
            double stringArrayValue2 = 0;
            for (int k = 0; k < strArray.Count(); k++)
            {
                stringArrayValue = CalculatorHelpers
                    .ConvertStringToDouble(strArray[k]);
                if (strArray2.Count() > k)
                {
                    stringArrayValue2 = CalculatorHelpers
                        .ConvertStringToDouble(strArray2[k]);
                    newArray[k] = (stringArrayValue + stringArrayValue2).ToString();
                }
            }
            return newArray;
        }
        public static void CopyStringDataToStringData(List<List<string>> dataToCopy, List<List<string>> data,
            int numOfCols, int startColIndex)
        {
            int iRow = 0;
            int iCol = 0;
            int iColStop = numOfCols + startColIndex;
            double dbZero = 0;
            foreach (var row in dataToCopy)
            {
                foreach (var col in dataToCopy)
                {
                    if (data.Count() > iRow)
                    {
                        for (int i = startColIndex; i < iColStop; i++)
                        {
                            if (row.Count > i && data[iRow].Count > i)
                            {
                                //don't overwrite the location and tr indexes (they init with zero)
                                dbZero = CalculatorHelpers.ConvertStringToDouble(row[i]);
                                if (dbZero != 0)
                                {
                                    data[iRow][i] = row[i];
                                }
                            }
                        }

                    }
                    iCol++;
                }
                iRow++;
            }
        }
        public static double[,] GetDoubleArray(List<List<double>> data)
        {
            double[,] problemData = new double[,] { };
            int i = 0;
            foreach (var dlist in data)
            {
                if (i == 0)
                {
                    problemData = new double[data.Count(), dlist.Count()];
                }
                int j = 0;
                foreach (var d in dlist)
                {
                    problemData[i, j] = d;
                    j++;
                }
                i++;
            }
            return problemData;
        }
        public static double[] GetDoubleArrayColumn(int colIndex, List<List<double>> data)
        {
            double[] colData = new double[data.Count()];
            int i = 0;
            foreach (var dlist in data)
            {
                int j = 0;
                foreach (var d in dlist)
                {
                    if (j == colIndex)
                    {
                        colData[i] = d;
                        break;
                    }
                    j++;
                }
                i++;
            }
            return colData;
        }
        public static string[] GetStringArrayColumn(int colIndex, List<List<string>> data)
        {
            string[] colData = new string[data.Count()];
            for (int i = 0; i < data.Count(); i++)
            {
                colData[i] = data[i][colIndex];
            }
            return colData;
        }
        public static double[] GetDoubleArrayColumn(int colIndex, List<List<string>> data)
        {
            double[] colData = new double[data.Count()];
            for (int i = 0; i < data.Count(); i++)
            {
                colData[i] = CalculatorHelpers.ConvertStringToDouble(data[i][colIndex]);
            }
            return colData;
        }
        public static Matrix<double> GetDoubleMatrix(List<List<double>> data, string[] colNames,
            string[] dataColNames)
        {
            //convert data to a Math.Net Matrix
            //positive definite matrix, or square (replace y col with an intercept col)
            Matrix<double> m = Matrix<double>.Build.Dense(data.Count(), dataColNames.Count());
            //let bad indexes throw errors -they return bad index error message
            //skip first column holding y vars
            int k = 0;
            int iDataColumn = 0;
            for (int i = 0; i < colNames.Count(); i++)
            {
                if (i == 0)
                {
                    //ignore the yvector in the first column
                    //instead, add an intercept column for B0 with all 1s
                    double[] intercepts = new double[data.Count()];
                    for (int j = 0; j < intercepts.Count(); j++)
                    {
                        intercepts[j] = 1;
                    }
                    m.SetColumn(i, intercepts.ToArray());
                    //first colname is the intercept
                    k++;
                }
                else
                {
                    //datacolnames coincides with what is in data
                    if (NeedsColumnName(dataColNames, colNames[i]))
                    {
                        //dependent vars start in colNames[4]
                        //data[1] corresponds to that column, or 4 - 3 = 1
                        iDataColumn = i - 3;
                        var colX = from col in data select col.ElementAt(iDataColumn);
                        m.SetColumn(k, colX.ToArray());
                        k++;
                    };

                }
            }
            return m;
        }
        public static double GetObservationsPerCell(List<List<double>> data, int distinctDataColumnIndex,
            double factor1, double level1)
        {
            double r = 0;
            //obs per cell coincide with first factor col[1] and first level col[2] in first row of data
            //error checking already carried out on cols and rows counts
            int i = 0;
            bool bIsFactor1 = false;
            bool bIsLevel1 = false;
            foreach (var factor in data)
            {
                foreach (var level in factor)
                {
                    if (i == 1)
                    {
                        if (level == factor1)
                        {
                            bIsFactor1 = true;
                        }
                    }
                    else if (i == 2)
                    {
                        if (level == level1)
                        {
                            bIsLevel1 = true;
                        }
                    }
                    i++;
                }
                if (bIsFactor1 && bIsLevel1)
                {
                    r++;
                }
                bIsFactor1 = false;
                bIsLevel1 = false;
                i = 0;
            }
            return r;
        }
        public static double GetMeanPerCell(List<List<double>> data, int distinctFColIndex, int distinctLColIndex,
            int distinctRow1Index, int distinctRow2Index, double r)
        {
            double ymean = 0;
            //second col are factors
            var colX = from col in data select col.ElementAt(distinctFColIndex);
            if (distinctRow1Index > (colX.Count() - 1))
            {
                return ymean;
            }
            var factor1 = colX.Distinct().ElementAt(distinctRow1Index);

            var colX2 = from col in data select col.ElementAt(distinctLColIndex);
            if (distinctRow2Index > (colX2.Count() - 1))
            {
                return ymean;
            }
            var level1 = colX2.Distinct().ElementAt(distinctRow2Index);
            //obs per cell coincide with first factor col[1] and first level col[2] in first row of data
            //error checking already carried out on cols and rows counts
            int i = 0;
            bool bIsFactor1 = false;
            bool bIsLevel1 = false;
            double dbY = 0;
            foreach (var factor in data)
            {
                foreach (var level in factor)
                {
                    if (i == 1)
                    {
                        if (level == factor1)
                        {
                            bIsFactor1 = true;
                        }
                    }
                    else if (i == 2)
                    {
                        if (level == level1)
                        {
                            bIsLevel1 = true;
                        }
                    }
                    else
                    {
                        //y obs in col[0]
                        dbY = level;
                    }
                    i++;
                }
                if (bIsFactor1 && bIsLevel1)
                {
                    ymean += dbY;
                }
                bIsFactor1 = false;
                bIsLevel1 = false;
                i = 0;
                dbY = 0;
            }
            ymean = ymean / r;
            return ymean;
        }
        public static double GetTotalInteraction(Matrix<double> forl, double r)
        {
            double totalinteraction = 0;
            double singleinteraction = 0;
            int i = 1;
            for (int j = 0; j < r; j++)
            {
                i = 1;
                //either block or treats can be used
                foreach (var item in forl.Column(j))
                {
                    singleinteraction += item;
                    if (i == r)
                    {
                        totalinteraction += Math.Pow(singleinteraction, 2);
                        singleinteraction = 0;
                        i = 1;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            return totalinteraction;
        }

        public static Matrix<double> GetDistinctMatrix(List<List<double>> data, int distinctDataColumnIndex)
        {
            //error checking already checked the number of columns
            var colY = from col in data select col.ElementAt(0);
            var colX = from col in data select col.ElementAt(distinctDataColumnIndex);
            var distinctX = colX.Distinct();
            List<List<double>> ydata = new List<List<double>>();
            foreach (var colIndex in distinctX)
            {
                List<double> newcol = new List<double>();
                ydata.Add(newcol);
            }
            int i = 0;
            foreach (var item in colX)
            {
                int j = 0;
                foreach (var colitem in distinctX)
                {
                    if (item == colitem)
                    {
                        ydata.ElementAt(j).Add(colY.ElementAt(i));
                    }
                    j++;
                }
                i++;
            }
            int rowcount = 0;
            if (ydata.ElementAt(0) != null)
            {
                rowcount = ydata.ElementAt(0).Count();
            }
            Matrix<double> m = GetMatrix(ydata, rowcount, distinctX.Count());
            return m;
        }
        public static Matrix<double> GetMatrix(List<List<double>> data, int rowcount, int colCount)
        {
            Matrix<double> m = Matrix<double>.Build.Dense(rowcount, colCount);
            int i = 0;
            foreach (var col in data)
            {
                m.SetColumn(i, col.ToArray());
                i++;
            }
            return m;
        }
        public static List<List<string>> GetRProjectData(List<List<string>> data, string[] colNames,
            string[] dataColNames)
        {
            //convert data to 2 d string lists
            List<List<string>> rData = new List<List<string>>();
            int k = 0;
            for (int i = 0; i < colNames.Count(); i++)
            {
                if (i == 0)
                {
                    //dependent vars start in first columns
                    var colX = from col in data select col.ElementAt(k);
                    rData.Add(colX.ToList());
                    k++;
                }
                else
                {
                    //datacolnames coincides with what is in data
                    if (NeedsColumnName(dataColNames, colNames[i]))
                    {
                        var colX = from col in data select col.ElementAt(k);
                        rData.Add(colX.ToList());
                        k++;
                    }
                }
            }
            return rData;
        }
        public static List<List<double>> GetDoubleData(List<List<string>> data,
            string[] colNames, string[] dataColNames)
        {
            //colnames includes row id columns while data does not
            string[] allDataNames = colNames.Skip(3).ToArray();
            //convert data to 2 d string lists
            List<List<double>> rData = new List<List<double>>();
            for (int i = 0; i < allDataNames.Count(); i++)
            {
                if (i == 0)
                {
                    //dependent vars start in first columns
                    var colX = from col in data select
                               CalculatorHelpers.ConvertStringToDouble(col.ElementAt(i));
                    rData.Add(colX.ToList());
                }
                else
                {
                    //datacolnames coincides with what is in data
                    if (NeedsColumnName(dataColNames, allDataNames[i]))
                    {
                        var colX = from col in data select
                                   CalculatorHelpers.ConvertStringToDouble(col.ElementAt(i));
                        rData.Add(colX.ToList());
                    }
                }
            }
            return rData;
        }
        public static List<List<string>> GetStringMLData(List<List<string>> data,
            string[] colNames, string[] dataColNames)
        {
            //colnames includes row id columns while data does not
            string[] allDataNames = colNames.Skip(3).ToArray();
            //convert data to 2 d string lists
            List<List<string>> rData = new List<List<string>>();
            for (int i = 0; i < allDataNames.Count(); i++)
            {
                if (i == 0)
                {
                    //dependent vars start in first columns
                    var colX = from col in data select col.ElementAt(i);
                    //ml algos skip the instructions row in 1st row
                    rData.Add(colX.Skip(1).ToList());
                }
                else
                {
                    //datacolnames coincides with what is in data
                    if (NeedsColumnName(dataColNames, allDataNames[i]))
                    {
                        var colX = from col in data select col.ElementAt(i);
                        //ml algos skip the instructions row in 1st row
                        rData.Add(colX.Skip(1).ToList());
                    }
                }
            }
            return rData;
        }
        public static List<string> GetNormTypes(List<string> mlInstructs,
            string[] colNames, string[] dataColNames)
        {
            List<string> normTypes = new List<string>();
            //colnames includes row id columns while data does not
            string[] allDataNames = colNames.Skip(3).ToArray();
            CalculatorHelpers.NORMALIZATION_TYPES normType
                = CalculatorHelpers.NORMALIZATION_TYPES.none;
            string sNormType = normType.ToString();
            for (int i = 0; i < allDataNames.Count(); i++)
            {
                if (i == 0)
                {
                    sNormType = mlInstructs[i];
                    normTypes.Add(sNormType);
                }
                else
                {
                    //datacolnames coincides with what is in data
                    if (NeedsColumnName(dataColNames, allDataNames[i]))
                    {
                        sNormType = mlInstructs[i];
                        normTypes.Add(sNormType);
                    }
                }
            }
            return normTypes;
        }
        public static List<string> FixNormTypes(List<string> normTypes, int colCount)
        {
            List<string> fullNormTypes = new List<string>();
            for (int i = 0; i < colCount; i++)
            {
                if (normTypes.Count > i)
                {
                    fullNormTypes.Add(normTypes[i]);
                }
                else
                {
                    fullNormTypes.Add("none");
                }
            }
            return fullNormTypes;
        }
        public static List<string> GetActualColNames(string[] colNames, string[] dataColNames)
        {
            List<string> actualColNames = new List<string>();
            //need 3row ids
            actualColNames.AddRange(colNames.Take(3));
            string[] allDataNames = colNames.Skip(3).ToArray();
            for (int i = 0; i < allDataNames.Count(); i++)
            {
                if (i == 0)
                {
                    actualColNames.Add(allDataNames[i]);
                }
                else
                {
                    //datacolnames coincides with what is in data
                    if (NeedsColumnName(dataColNames, allDataNames[i]))
                    {
                        actualColNames.Add(allDataNames[i]);
                    }
                }
            }
            return actualColNames;
        }
        public static StringBuilder GetRProjectDataFile(List<List<string>> data, string[] colNames,
            string[] depColNames)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GetCSVRowHeader(colNames, depColNames));
            //get the raw data
            List<List<string>> rData = GetRProjectData(data, colNames, depColNames);
            if (rData.Count > 0)
            {
                List<string> row = new List<string>();
                int iRowCount = rData[0].Count();
                for (int i = 0; i < rData.Count(); i++)
                {
                    row = new List<string>();
                    //has 3 rows of column data
                    for (int j = 0; j < iRowCount; j++)
                    {
                        row.Add(rData[i][j]);
                    }
                    sb.AppendLine(GetCSVRow(row.ToArray()));
                }
            }
            return sb;
        }
        public static string GetCSVRowHeader(string[] colNames, string[] row)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            //4th column is dep var
            sb.Append(colNames[3]);
            sb.Append(Constants.CSV_DELIMITER);
            foreach (var s in row)
            {
                sb.Append(s);
                //row doesn't end in a delimiter
                if (i != (row.Count() - 1))
                {
                    sb.Append(Constants.CSV_DELIMITER);
                }
                i++;
            }
            return sb.ToString();
        }
        public static string GetCSVRow(string[] row)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var s in row)
            {
                if (i == 0)
                {
                    sb.Append(s);
                }
                else
                {
                    sb.Append(s);
                    //row doesn't end in a delimiter
                    if (i != (row.Count() - 1))
                    {
                        sb.Append(Constants.CSV_DELIMITER);
                    }
                }
                i++;
            }
            return sb.ToString();
        }
        public static bool NeedsColumnName(string[] colNames, string colName)
        {
            //only use the columns that are in the mathexpression
            bool bHasTerm = false;
            for (int i = 0; i < colNames.Count(); i++)
            {
                if (colName.ToLower() == colNames[i].ToLower())
                {
                    return true;
                }
            }
            return bHasTerm;
        }


        public static int GetColNameCount(string[] mathTerms, string[] colNames)
        {
            int iColCount = 0;
            //only use the columns that are in the mathexpression;
            for (int i = 0; i < colNames.Count(); i++)
            {
                for (int j = 0; j < mathTerms.Count(); j++)
                {
                    if (colNames[i].ToLower() == mathTerms[j].ToLower())
                    {
                        iColCount++;
                    }
                }
            }
            return iColCount;
        }
        public static bool MathExpressHasColumnName(string[] mathTerms, string colName)
        {
            //only use the columns that are in the mathexpression
            bool bHasTerm = false;
            for (int i = 0; i < mathTerms.Count(); i++)
            {
                if (colName.ToLower() == mathTerms[i].ToLower())
                {
                    return true;
                }
            }
            return bHasTerm;
        }

        public static string GetMathTermQxString(string mathTerm, int zerobasedIndex)
        {
            string[] arrMathTerms = mathTerm.Split(Constants.FILEEXTENSION_DELIMITERS);
            string sColName = string.Empty;
            if (arrMathTerms.Count() >= (zerobasedIndex + 1))
            {
                sColName = arrMathTerms[zerobasedIndex];
            }
            return sColName;
        }
        public static Vector<double> GetYData(List<List<double>> data)
        {
            //first columns holds the ys
            var ys = from row in data select row.ElementAt(0);
            Vector<double> y = Vector<double>.Build.Dense(ys.ToArray());
            return y;
        }

        public static double[] GetVector(List<List<double>> data, int col)
        {
            //first columns holds the ys
            var ys = from row in data select row.ElementAt(col);
            double[] y = ys.ToArray();
            return y;
        }


        public static Vector<double> GetYData(List<List<string>> data)
        {
            //first columns holds the ys as strings
            var ys = from row in data
                     select row.ElementAt(0);
            //convert to doubles
            var yds = ys
                .Select(y1 => CalculatorHelpers.ConvertStringToDouble(y1))
                .ToArray();
            //convert to vector
            Vector<double> y = Vector<double>.Build.Dense(yds.ToArray());
            return y;
        }
        public static int[][] GetYDataCategories(List<List<string>> data)
        {
            //first columns holds the ys
            var ys = GetYData(data);
            List<double> colCategories = ys.Distinct().ToList();
            var counts = new int[ys.Count()][];
            int[] category = new int[colCategories.Count()];
            int i = 0;
            foreach (var y in ys)
            {
                int j = 0;
                category = new int[colCategories.Count()];
                foreach (var c in colCategories)
                {
                    if (c == y)
                    {
                        //the position defines the category (i.e. {0, 0, 1, 0, 0} index[2] = green)
                        category[j] = 1;
                    }
                    else
                    {
                        category[j] = 0;
                    }
                    j++;
                }
                counts[i] = category;
                i++;
            }
            return counts;
        }
        private static Vector<double> GetYData1()
        {
            double[] yd = new double[5] { 1, 1, 2, 2, 4 };
            //first columns holds the ys
            Vector<double> y = Vector<double>.Build.Dense(yd.ToArray());
            return y;
        }
        private static Matrix<double> GetXMatrix(List<List<double>> data, string[] colNames)
        {
            //convert data to a Math.Net Matrix
            //positive definite matrix, or square (replace y col with an intercept col)
            Matrix<double> m = Matrix<double>.Build.Dense(data.Count(), colNames.Count());
            //skip first column holding y vars
            for (int i = 0; i < colNames.Count(); i++)
            {
                if (i == 0)
                {
                    //add an intercept column for B0 with all 1s
                    double[] intercepts = new double[data.Count()];
                    for (int j = 0; j < intercepts.Count(); j++)
                    {
                        intercepts[j] = 1;
                    }
                    m.SetColumn(i, intercepts.ToArray());
                }
                else
                {
                    var colX = from col in data select col.ElementAt(i);
                    m.SetColumn(i, colX.ToArray());
                    //var rowX = from row in data select row.ElementAt(i);
                    //m.SetColumn(i, rowX.ToArray());
                }
            }
            return m;
        }
        private static Matrix<double> GetXMatrix1()
        {
            //convert data to a Math.Net Matrix
            //positive definite matrix, or square
            Matrix<double> m = Matrix<double>.Build.Dense(5, 2);
            double[] xo = new double[5] { 1, 1, 1, 1, 1 };
            m.SetColumn(0, xo.ToArray());
            double[] xi = new double[5] { 1, 2, 3, 4, 5 };
            m.SetColumn(1, xi.ToArray());
            return m;
        }
        private static double[][] GetXData(List<List<double>> data, string[] colNames)
        {
            Matrix<double> xm = GetXMatrix(data, colNames);
            //convert data to a Math.Net Matrix
            //positive definite matrix, or square
            double[][] x = new double[xm.RowCount][];
            for (int i = 0; i < x.Length; ++i)
            {
                //CTA example uses 4 inputvars and 3 outputvals: (x0, x1, x2, x3), (y0, y1, y2)
                x[i] = new double[xm.ColumnCount];
            }
            for (int row = 0; row < xm.RowCount; ++row)
            {
                //inputs (cols - 1)
                //var ys = from yi in data select yi.ElementAt(row + 1);
                for (int col = 0; col < (xm.ColumnCount); ++col)
                {
                    x[row][col] = xm.Row(row)[col];
                }
            }
            return x;
        }
        public static string GetLine(string[] inputs, bool isTitles)
        {
            StringBuilder sb = new StringBuilder();
            string spaces = string.Empty;
            for (int i = 0; i < 17; i++)
            {
                if (isTitles)
                {
                    //spaces += "--";
                    spaces += " ";
                }
                else
                {
                    spaces += " ";
                }
            }
            int iLengthMissing = 0;
            for (int i = 0; i < inputs.Count(); i++)
            {
                iLengthMissing = 17 - inputs[i].Length;
                if (iLengthMissing < 0)
                    iLengthMissing = 0;
                sb.Append(inputs[i]);
                if (i != inputs.Count() - 1)
                {
                    sb.Append(spaces.Substring(0, iLengthMissing));
                }
            }
            return sb.ToString();
        }
        public static string MakeUniformString(string input, bool isTitles)
        {
            StringBuilder sb = new StringBuilder();
            string spaces = string.Empty;
            for (int i = 0; i < 17; i++)
            {
                if (isTitles)
                {
                    //spaces += "--";
                    spaces += " ";
                }
                else
                {
                    spaces += " ";
                }
            }
            int iLengthMissing = 0; iLengthMissing = 17 - input.Length;
            if (iLengthMissing < 0)
                iLengthMissing = 0;
            sb.Append(input);
            sb.Append(spaces.Substring(0, iLengthMissing));
            return sb.ToString();
        }
      

        public static double GetNormalizedValue(string subIndNormType, double startValue,
            MathNet.Numerics.Statistics.DescriptiveStatistics stats)
        {
            double dbNValue = startValue;
            if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.none.ToString()
                || string.IsNullOrEmpty(subIndNormType))
            {
                //data has already been normalized
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.zscore.ToString())
            {
                //z-score: (x – mean(x)) / stddev(x)
                dbNValue = (startValue - stats.Mean) / stats.StandardDeviation;
                dbNValue = CalculatorHelpers.CheckForNaNandRound4(dbNValue);
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.minmax.ToString())
            {
                //min-max: (x – min(x)) / (max(x) – min(x))
                dbNValue = (startValue - stats.Minimum) / (stats.Maximum - stats.Minimum);
                dbNValue = CalculatorHelpers.CheckForNaNandRound4(dbNValue);
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.logistic.ToString())
            {
                //logistic: 1 / (1 + exp(-x))
                dbNValue = MathNet.Numerics.SpecialFunctions.Logistic(startValue);
                dbNValue = CalculatorHelpers.CheckForNaNandRound4(dbNValue);
                //or
                //siIndex[x] = 1 / (1 + Math.Exp(-siIndex[x]));
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.logit.ToString())
            {
                //logit: inverese of logistic for y between 0 and 1
                //this assumes x is actually y 
                dbNValue = MathNet.Numerics.SpecialFunctions.Logit(startValue);
                dbNValue = CalculatorHelpers.CheckForNaNandRound4(dbNValue);
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.tanh.ToString())
            {
                //hyperbolic tangent
                dbNValue = MathNet.Numerics.Trig.Tanh(startValue);
                dbNValue = CalculatorHelpers.CheckForNaNandRound4(dbNValue);
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.pnorm.ToString())
            {
                //pnorm is for complete vectors in next function
            }
            else
            {
                //indicator 2 in drr1 (p and q, not norm and index)
            }
            return dbNValue;
        }
        public static double ConvertNormalizedValue(string subIndNormType, double normValue,
            MathNet.Numerics.Statistics.DescriptiveStatistics stats)
        {
            double dbNValue = normValue;
            if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.none.ToString()
                || string.IsNullOrEmpty(subIndNormType))
            {
                //data has already been normalized
                return dbNValue;
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.zscore.ToString())
            {
                //z-score: (x – mean(x)) / stddev(x)
                dbNValue = (normValue * stats.StandardDeviation) + stats.Mean;
                dbNValue = CalculatorHelpers.CheckForNaNandRound4(dbNValue);
            }
            else if (subIndNormType == CalculatorHelpers.NORMALIZATION_TYPES.minmax.ToString())
            {
                //min-max: (x – min(x)) / (max(x) – min(x))
                dbNValue = (normValue * (stats.Maximum - stats.Minimum)) + stats.Minimum;
                dbNValue = CalculatorHelpers.CheckForNaNandRound4(dbNValue);
            }
            return dbNValue;
        }
        public static Vector<double> GetNormalizedVector(
             string normType, double startValue,
             bool scaleUp4Digits, double[] data)
        {
            //normalize them
            var stats = new MathNet.Numerics.Statistics.DescriptiveStatistics(data);
            Vector<double> siIndex = Vector<double>.Build.Dense(data);
            if (normType == CalculatorHelpers.NORMALIZATION_TYPES.none.ToString()
                || string.IsNullOrEmpty(normType))
            {
                //data has already been normalized
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.zscore.ToString())
            {
                //z-score: (x – mean(x)) / stddev(x)
                for (int x = 0; x < siIndex.Count; x++)
                {
                    siIndex[x] = (siIndex[x] - stats.Mean) / stats.StandardDeviation;
                    if (scaleUp4Digits)
                    {
                        //scale the 4 digits by multiplying by 10,000
                        siIndex[x] = siIndex[x] * 10000.00;
                        siIndex[x] = Math.Round(siIndex[x], 2);
                    }
                    else
                    {
                        siIndex[x] = CalculatorHelpers.CheckForNaNandRound4(siIndex[x]);
                    }
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.minmax.ToString())
            {
                //min-max: (x – min(x)) / (max(x) – min(x))
                for (int x = 0; x < siIndex.Count; x++)
                {
                    siIndex[x] = (siIndex[x] - stats.Minimum) / (stats.Maximum - stats.Minimum);
                    if (scaleUp4Digits)
                    {
                        //scale the 4 digits by multiplying by 10,000
                        siIndex[x] = siIndex[x] * 10000.00;
                        siIndex[x] = Math.Round(siIndex[x], 2);
                    }
                    else
                    {
                        siIndex[x] = CalculatorHelpers.CheckForNaNandRound4(siIndex[x]);
                    }
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.logistic.ToString())
            {
                for (int x = 0; x < siIndex.Count; x++)
                {
                    //logistic: 1 / (1 + exp(-x))
                    siIndex[x] = MathNet.Numerics.SpecialFunctions.Logistic(siIndex[x]);
                    //or
                    //siIndex[x] = 1 / (1 + Math.Exp(-siIndex[x]));
                    if (scaleUp4Digits)
                    {
                        //scale the 4 digits by multiplying by 10,000
                        siIndex[x] = siIndex[x] * 10000.00;
                        siIndex[x] = Math.Round(siIndex[x], 2);
                    }
                    else
                    {
                        siIndex[x] = CalculatorHelpers.CheckForNaNandRound4(siIndex[x]);
                    }
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.logit.ToString())
            {
                for (int x = 0; x < siIndex.Count; x++)
                {
                    //logit: inverese of logistic for y between 0 and 1
                    //this assumes x is actually y 
                    siIndex[x] = MathNet.Numerics.SpecialFunctions.Logit(siIndex[x]);
                    if (scaleUp4Digits)
                    {
                        //scale the 4 digits by multiplying by 10,000
                        siIndex[x] = siIndex[x] * 10000.00;
                        siIndex[x] = Math.Round(siIndex[x], 2);
                    }
                    else
                    {
                        siIndex[x] = CalculatorHelpers.CheckForNaNandRound4(siIndex[x]);
                    }
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.tanh.ToString())
            {
                for (int x = 0; x < siIndex.Count; x++)
                {
                    //hyperbolic tangent
                    siIndex[x] = MathNet.Numerics.Trig.Tanh(siIndex[x]);
                    if (scaleUp4Digits)
                    {
                        //scale the 4 digits by multiplying by 10,000
                        siIndex[x] = siIndex[x] * 10000.00;
                        siIndex[x] = Math.Round(siIndex[x], 2);
                    }
                    else
                    {
                        siIndex[x] = CalculatorHelpers.CheckForNaNandRound4(siIndex[x]);
                    }
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.pnorm.ToString())
            {
                //p value for ttest with n-1
                double pValue = GetPValueForTDist(siIndex.Count() - 1,
                    startValue, stats.Mean, stats.Variance);
                pValue = CalculatorHelpers.CheckForNaNandRound4(pValue);
                //p norm
                siIndex = siIndex.Normalize(pValue);
                if (scaleUp4Digits)
                {
                    //scale the 4 digits by multiplying by 10,000
                    for (int x = 0; x < siIndex.Count; x++)
                    {
                        siIndex[x] = siIndex[x] * 10000.00;
                        siIndex[x] = Math.Round(siIndex[x], 2);
                    }
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.weights.ToString())
            {
                for (int x = 0; x < siIndex.Count; x++)
                {
                    //rand 2016 technique
                    siIndex[x] = siIndex[x] / startValue;
                    if (scaleUp4Digits)
                    {
                        //scale the 4 digits by multiplying by 10,000
                        siIndex[x] = siIndex[x] * 10000.00;
                        siIndex[x] = Math.Round(siIndex[x], 2);
                    }
                    else
                    {
                        siIndex[x] = CalculatorHelpers.CheckForNaNandRound4(siIndex[x]);
                    }
                }
            }
            else
            {
                //indicator 2 in drr1 (p and q, not norm and index)
            }
            //add them to parent cat index
            return siIndex;
        }
        public static string GetStringValue(List<List<double>> trends, int rowIndex, int colIndex)
        {
            string sValue = string.Empty;
            if (trends.Count > rowIndex)
            {
                if (trends[rowIndex].Count > colIndex)
                {
                    sValue = trends[rowIndex]
                        .ElementAt(colIndex).ToString("F4", CultureInfo.InvariantCulture);
                }
            }
            return sValue;
        }
        public static double GetDoubleValue(List<List<double>> trends, int rowIndex, int colIndex)
        {
            double dbValue = 0;
            if (trends.Count > rowIndex)
            {
                if (trends[rowIndex].Count > colIndex)
                {
                    dbValue = trends[rowIndex]
                        .ElementAt(colIndex);
                }
            }
            return dbValue;
        }
        public static double GetDoubleValue(string[] strArray, int colIndex)
        {
            double dbValue = 0;
            if (strArray == null)
                strArray = new string[colIndex];
            if (strArray.Count() > colIndex)
            {
                dbValue = CalculatorHelpers
                    .ConvertStringToDouble(strArray[colIndex]);
            }
            return dbValue;
        }
        public static List<List<double>> GetNormalizedandWeightedLists(
            string subIndNormType, double startValue, bool scaleUp4Digits,
            List<double> weights, List<List<double>> trends)
        {
            List<List<double>> normTrends = new List<List<double>>();
            int i = 0;
            for (i = 0; i < trends[0].Count; i++)
            {
                //the columns are being normalized
                double[] colArray = GetDoubleArrayColumn(i, trends);
                Vector<double> normTrend = GetNormalizedVector(subIndNormType,
                    weights.Sum(), scaleUp4Digits, colArray);
                normTrends.Add(normTrend.ToList());
            }
            //but the normalized columns have to be returned back to the original rows in trends
            //and weighted
            List<List<double>> normTrends2 = new List<List<double>>();
            i = 0;
            for (i = 0; i < normTrends[0].Count; i++)
            {
                double[] colArray = GetDoubleArrayColumn(i, normTrends);
                List<double> normRow = new List<double>();
                //weighting is transitive math, shouldn't matter if before or after normaliz
                for (int j = 0; j < colArray.Count(); j++)
                {
                    if (weights.Count > i)
                    {
                        normRow.Add(colArray[j] * weights[i]);
                    }
                }
                normTrends2.Add(normRow);
            }
            return normTrends2;
        }
        public static double GetDiscountedTotal(string discountType,
            double price, double quantity,
            double life, double realRate, double nominalRate,
            double escalationRate, double times, double planningYear,
            double serviceYears, double yearFromBase)
        {
            double dbDiscTotal = price * quantity;
            GeneralRules.GROWTH_SERIES_TYPES eGrowthType = GeneralRules.GetGrowthType(discountType);
            double dbSalvVal = 0;
            if (realRate > 0)
                realRate = realRate / 100;
            if (nominalRate > 0)
                nominalRate = nominalRate / 100;
            dbDiscTotal = GeneralRules.GetGradientRealDiscountValue(dbDiscTotal,
                realRate, serviceYears, yearFromBase,
                planningYear, eGrowthType, escalationRate,
                escalationRate, life, times, dbSalvVal);
            return dbDiscTotal;
        }
        public static double GetDiscountedAmount(double initialAmount, double life, double rate)
        {
            double dbDiscAmount = initialAmount;
            dbDiscAmount = initialAmount * (1 / Math.Pow((1 + rate), life));
            return dbDiscAmount;
        }
        public static double GetUPV(double seriesAmount, double life, double rate)
        {
            double dbUPV = seriesAmount;
            dbUPV = seriesAmount * ((Math.Pow(1 + rate, life) - 1) / (rate * (Math.Pow((1 + rate), life))));
            return dbUPV;
        }
        public static double GetAvgAmortizedAmount(double amount,
            double life, double rate)
        {
            double dbAAC = amount;
            //(amount / ((1 - (1 + rate) ^ (-1 * life)) / rate))
            int iPower = (int)(-1 * life);
            double dbAvgAnnFactor = (1 - Math.Pow((1 + rate), iPower)) / rate;
            dbAAC = amount / dbAvgAnnFactor;
            return dbAAC;
        }
        public static List<List<string>> GetURLData(List<string> lines)
        {
            List<List<string>> urldata = new List<List<string>>();
            int i = 0;
            foreach (var line in lines)
            {
                //skip the factors titles row
                if (i != 0)
                {
                    //skip the 3 colname columns
                    List<string> dataRow = line
                        .Split(Constants.CSV_DELIMITERS)
                        .Skip(3).ToList();
                    if (dataRow != null)
                    {
                        urldata.Add(dataRow);
                    }
                }
                i++;
            }
            return urldata;
        }
        public static List<List<string>> CopyData(List<List<string>> fromData)
        {
            List<List<string>> todata = new List<List<string>>();
            foreach (var row in fromData)
            {
                todata.Add(row);
            }
            return todata;
        }
        public static List<List<double>> CopyDoubleData(List<List<string>> fromData)
        {
            List<List<double>> todata = new List<List<double>>();
            foreach (var row in fromData)
            {
                todata.Add(new List<double>());
                foreach (var col in row)
                {
                    todata.Last().Add(CalculatorHelpers.ConvertStringToDouble(col));
                }

            }
            return todata;
        }

        public static double[] GetAttributeGroups(int colIndex, List<List<double>> data)
        {
            List<double> groups = new List<double>();
            double dbAttribute = 0;
            if (colIndex < data.Count)
            {
                foreach (double row in data[colIndex])
                {
                    //groups mean 1 digit
                    dbAttribute = Math.Round(row, 1);
                    if (!groups.Contains(dbAttribute))
                    {
                        groups.Add(dbAttribute);
                    }
                }
            }
            groups.Sort();
            return groups.ToArray();
        }
        public static double[] GetAttributeGroups(int colIndex, List<List<double>> data,
            IndicatorQT1 qt1)
        {
            List<double> groups = new List<double>();
            double dbAttribute = 0;
            if (colIndex < data.Count)
            {
                //data has normalized columns in data[0] first index
                //so this is iterating rows
                for (int i = 0; i < data[colIndex].Count; i++)
                {
                    //groups mean digit
                    dbAttribute = Math.Round(data[colIndex][i], 1);
                    if (!groups.Contains(dbAttribute))
                    {
                        groups.Add(dbAttribute);
                    }
                }
            }
            groups.Sort();
            return groups.ToArray();
        }
        public static string[] GetAttributeGroups(List<string> data)
        {
            List<string> groups = new List<string>();
            string sAttribute = string.Empty;
            foreach (string attribute in data)
            {
                if (!groups.Contains(attribute))
                {
                    groups.Add(sAttribute);
                }
            }
            return groups.ToArray();
        }
        public static string[] GetAttributeGroups(int colIndex, List<List<string>> data)
        {
            List<string> groups = new List<string>();
            string sAttribute = string.Empty;
            if (colIndex < data.Count)
            {
                foreach (string row in data[colIndex])
                {
                    sAttribute = row;
                    if (!groups.Contains(sAttribute))
                    {
                        groups.Add(sAttribute);
                    }
                }
            }
            return groups.ToArray();
        }
        public static string[] GetAttributeGroups(int colIndex,
            List<List<string>> data, IndicatorQT1 qt1)
        {
            List<string> groups = new List<string>();
            string sAttribute = string.Empty;
            if (colIndex < data.Count)
            {
                foreach (string row in data[colIndex])
                {
                    sAttribute = row;
                    if (!string.IsNullOrEmpty(sAttribute))
                    {
                        if (!groups.Contains(sAttribute))
                        {
                            groups.Add(sAttribute);
                        }
                    }
                }
            }
            return groups.ToArray();
        }
        public static string ConvertAttributeToLabel(string attribute, IndicatorQT1 qt1)
        {
            string sAttribute = attribute;
            if (!string.IsNullOrEmpty(qt1.Q1Unit) && !string.IsNullOrEmpty(qt1.Q2Unit))
            {
                double dbAttribute = CalculatorHelpers.ConvertStringToDouble(sAttribute);
                if (dbAttribute < qt1.Q1)
                {
                    sAttribute = qt1.Q1Unit;
                }
                else if (dbAttribute < qt1.Q2)
                {
                    sAttribute = qt1.Q2Unit;
                }
                else if (dbAttribute < qt1.Q3)
                {
                    sAttribute = qt1.Q3Unit;
                }
                else if (dbAttribute < qt1.Q4)
                {
                    sAttribute = qt1.Q4Unit;
                }
                else if (dbAttribute < qt1.Q5)
                {
                    sAttribute = qt1.Q5Unit;
                }
            }
            return sAttribute;
        }
        public static string ConvertIndexToLabel(int index, IndicatorQT1 qt1)
        {
            string sAttribute = Constants.NONE;
            if (!string.IsNullOrEmpty(qt1.Q1Unit) && !string.IsNullOrEmpty(qt1.Q2Unit))
            {
                if (index == 0)
                {
                    sAttribute = qt1.Q1Unit;
                }
                else if (index == 1)
                {
                    sAttribute = qt1.Q2Unit;
                }
                else if (index == 2)
                {
                    sAttribute = qt1.Q3Unit;
                }
                else if (index == 3)
                {
                    sAttribute = qt1.Q4Unit;
                }
                else if (index == 4)
                {
                    sAttribute = qt1.Q5Unit;
                }
            }
            return sAttribute;
        }
        public static List<string> GetAlgoInstructs(List<List<string>> data)
        {
            List<string> algoInstructs = new List<string>(data[0]);
            return algoInstructs;
        }
        public static List<List<string>> GetNormalizedSData(List<List<string>> data,
            IndicatorQT1 qt1, string[] colNames, string[] depColNames,
            List<string> normTypes, string stringDataType = null)
        {
            //train[0] will be normalized columns (while data[0] are rows)
            //skips the ml instructions row
            List<List<string>> trainData = GetStringMLData(data, colNames, depColNames);
            //normalize the data
            List<List<string>> trainDB = new List<List<string>>();
            List<string> norms = new List<string>();
            string sAttribute = string.Empty;
            for (int d = 0; d < trainData.Count; ++d)
            {
                //see if the data needs transformation
                CalculatorHelpers.NORMALIZATION_TYPES normType
                    = CalculatorHelpers.GetNormalizationType(normTypes[d]);
                if (normType == CalculatorHelpers.NORMALIZATION_TYPES.qtext)
                {
                    for (int i = 0; i < trainData[d].Count(); i++)
                    {
                        //needs qcategories from q1 to q5
                        sAttribute = ConvertAttributeToLabel(trainData[d][i], qt1);
                        norms.Add(sAttribute);
                    }
                    trainDB.Add(norms);
                }
                else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.text)
                {
                    //leave it alone
                    trainDB.Add(trainData[d]);
                }
                else
                {
                    double[] normColumn = GetNormalizedData(trainData[d], qt1, normType);
                    if (d < trainData.Count)
                    {
                        norms = new List<string>();
                        for (int i = 0; i < normColumn.Count(); i++)
                        {
                            if (stringDataType == null)
                                stringDataType = "F4";
                            norms.Add(normColumn[i].ToString(stringDataType));
                        }
                        //fill the col
                        trainDB.Add(norms);
                    }
                }
            }
            return trainDB;
        }
        public static List<List<double>> GetNormalizedDData(List<List<string>> data,
            IndicatorQT1 qt1, string[] colNames, string[] depColNames,
            List<string> normTypes, string stringDataType = null)
        {
            //train[0] will be normalized columns (while data[0] are rows)
            List<List<string>> trainData = GetStringMLData(data, colNames, depColNames);
            //normalize the data
            List<List<double>> trainDB = new List<List<double>>();
            for (int d = 0; d < trainData.Count; ++d)
            {
                //see if the data needs transformation
                CalculatorHelpers.NORMALIZATION_TYPES normType
                    = CalculatorHelpers.GetNormalizationType(normTypes[d]);
                double[] normColumn = GetNormalizedData(trainData[d], qt1, normType);
                if (d < trainData.Count)
                {
                    //fill the col
                    List<double> norms = new List<double>(normColumn);
                    trainDB.Add(norms);
                }
            }
            return trainDB;
        }
        public static string[][] MakeInputSData(List<List<string>> trainData, int numItems,
            IndicatorQT1 qt1, int numInput)
        {

            // make the result matrix holder (now first index is row)
            string[][] result = new string[numItems][];
            for (int r = 0; r < numItems; ++r)
                // allocate the cols
                result[r] = new string[numInput];
            string sInput = string.Empty;
            for (int r = 0; r < numItems; ++r)
            {
                string[] inputs = new string[numInput];
                if ((trainData.Count + 1) > numInput)
                {
                    for (int i = 0; i < numInput; ++i)
                    {
                        //skip col[0]
                        sInput = trainData[i + 1][r];
                        inputs[i] = sInput;
                    }
                    int c = 0;
                    for (int i = 0; i < numInput; ++i)
                        result[r][c++] = inputs[i];
                }
            }
            return result;
        }
        public static string[][] MakeSData(List<List<string>> trainData, int numItems,
            IndicatorQT1 qt1)
        {
            //reverses rows and cols for std ml datasets
            int iAllColsCount = trainData.Count;
            // make the result matrix holder (now first index is row)
            string[][] result = new string[numItems][];
            for (int r = 0; r < numItems; ++r)
                // allocate the cols
                result[r] = new string[iAllColsCount];
            string sInput = string.Empty;
            for (int r = 0; r < numItems; ++r)
            {
                for (int i = 0; i < iAllColsCount; ++i)
                {
                    result[r][i] = trainData[i][r];
                }
            }
            return result;
        }
        public static int[][] GetInputs(int[][] alls)
        {
            int iNumRows = alls.Length;
            int iNumCols = alls[0].Length;
            int[][] inputs = new int[iNumRows][];
            List<int> rowData = new List<int>();
            for (int r = 0; r < iNumRows; ++r)
            {
                rowData = new List<int>();
                for (int c = 1; c < iNumCols; ++c)
                {
                    rowData.Add(alls[r][c]);
                }
                inputs[r] = rowData.ToArray();
            }
            return inputs;
        }
        public static int[] GetOutputs(int[][] alls)
        {
            int iNumRows = alls.Length;
            int iNumCols = alls[0].Length;
            int[] outputs = new int[iNumRows];
            List<int> rowData = new List<int>();
            for (int r = 0; r < iNumRows; ++r)
            {
                rowData.Add(alls[r][0]);
                outputs[r] = alls[r][0];
            }
            return outputs;
        }
        public static double[][] MakeInputDData(List<List<double>> trainData, int numItems,
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
        //public static string[][] MakeInputIData(List<List<string>> trainData, int numItems,
        //    IndicatorQT1 qt1, int numInput)
        //{
        //    // make the result matrix holder (now first index is row)
        //    int[][] result = new int[numItems][];
        //    for (int r = 0; r < numItems; ++r)
        //        // allocate the cols
        //        result[r] = new int[numInput];
        //    double dbInput = 0;
        //    for (int r = 0; r < numItems; ++r)
        //    {
        //        double[] inputs = new double[numInput];
        //        if ((trainData.Count + 1) > numInput)
        //        {
        //            for (int i = 0; i < numInput; ++i)
        //            {
        //                //skip col[0]
        //                dbInput = trainData[i + 1][r];
        //                inputs[i] = dbInput;
        //            }
        //            int c = 0;
        //            for (int i = 0; i < numInput; ++i)
        //                result[r][c++] = inputs[i];
        //        }
        //    }
        //    return result;
        //}
        public static double GetScaledData(double inLo, double inHi, double input)
        {
            double dbScaledInput = (inHi - inLo) * input + inLo;
            return dbScaledInput;
        }
        public static double[] GetNormalizedData(List<string> data, 
            IndicatorQT1 qt1, CalculatorHelpers.NORMALIZATION_TYPES normType)
        {
            double[] colNorms = new double[data.Count];
            for (int i = 0; i < data.Count(); i++)
            {
                colNorms[i] = CalculatorHelpers.ConvertStringToDouble(data[i]);
            }
            double dbAtt = 0;
            if (normType == CalculatorHelpers.NORMALIZATION_TYPES.qcategory)
            {
                for (int i = 0; i < data.Count(); i++)
                {
                    dbAtt = CalculatorHelpers.ConvertStringToDouble(data[i]);
                    colNorms[i] = ConvertAttributeToCategory(dbAtt, qt1);
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.qindex)
            {
                for (int i = 0; i < data.Count(); i++)
                {
                    dbAtt = CalculatorHelpers.ConvertStringToDouble(data[i]);
                    colNorms[i] = ConvertAttributeToIndexPosition(dbAtt, qt1);
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.index)
            {
                string[] attributeValues = GetAttributeGroups(data);
                for (int i = 0; i < attributeValues.Count(); i++)
                {
                    
                    dbAtt = CalculatorHelpers.ConvertStringToDouble(attributeValues[i]);
                    colNorms[i] = dbAtt;
                }
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.zscore
                || normType == CalculatorHelpers.NORMALIZATION_TYPES.minmax
                || normType == CalculatorHelpers.NORMALIZATION_TYPES.logistic
                || normType == CalculatorHelpers.NORMALIZATION_TYPES.logit
                || normType == CalculatorHelpers.NORMALIZATION_TYPES.tanh
                || normType == CalculatorHelpers.NORMALIZATION_TYPES.weights
                || normType == CalculatorHelpers.NORMALIZATION_TYPES.pnorm)
            {
                colNorms = GetNormalizedVector(normType.ToString(), 
                    0, false, colNorms).ToArray();
            }
            else if (normType == CalculatorHelpers.NORMALIZATION_TYPES.none)
            {
                //don't normalize the data
                //return colNorms
            }
            return colNorms;
        }
        
        
        public static double[] ConvertAttributeToOutputs(string attribute, IndicatorQT1 qt1, int numOutputs)
        {
            double[] outputs = new double[numOutputs];
            if (!string.IsNullOrEmpty(qt1.Q1Unit) && !string.IsNullOrEmpty(qt1.Q2Unit))
            {
                double dbAttribute = CalculatorHelpers.ConvertStringToDouble(attribute);
                outputs = ConvertAttributeToOutputs(dbAttribute, qt1, numOutputs);
            }
            return outputs;
        }
        public static double[] ConvertAttributeToOutputs(double attribute, 
            IndicatorQT1 qt1, int numOutputs)
        {
            double[] outputs = new double[numOutputs];
            int iIndexPosition = 0;
            if (!string.IsNullOrEmpty(qt1.Q1Unit) && !string.IsNullOrEmpty(qt1.Q2Unit))
            {
                iIndexPosition = ConvertAttributeToIndexPosition(attribute, qt1);
            }
            for (int i = 0; i < numOutputs; i++)
            {
                if (i == iIndexPosition)
                {
                    outputs[i] = 1;
                }
                else
                {
                    outputs[i] = 0;
                }
            }
            return outputs;
        }
        public static double[] ConvertDoubleToOutputsIndex(double attribute,
            IndicatorQT1 qt1, double[] outputCategories)
        {
            double[] outputs = new double[outputCategories.Length];
            for (int i = 0; i < outputCategories.Length; i++)
            {
                outputs[i] = 0;
            }
            for (int i = 0; i < outputCategories.Length; i++)
            {
                if (attribute <= outputCategories[i])
                {
                    outputs[i] = 1;
                    break;
                }
            }
            return outputs;
        }
        public static int ConvertAttributeToIndexPosition(double attribute,
            IndicatorQT1 qt1)
        {
            int iIndexPosition = CalculatorHelpers.ConvertStringToInt(attribute.ToString());
            if ((!string.IsNullOrEmpty(qt1.Q1Unit) && !string.IsNullOrEmpty(qt1.Q2Unit))
                && (qt1.Q1 > 0 && qt1.Q2 > 0))
            {
                if (attribute < qt1.Q1)
                {
                    iIndexPosition = 0;
                }
                else if (attribute < qt1.Q2)
                {
                    iIndexPosition = 1;
                }
                else if (attribute < qt1.Q3)
                {
                    iIndexPosition = 2;
                }
                else if (attribute < qt1.Q4)
                {
                    iIndexPosition = 3;
                }
                else if (attribute < qt1.Q5)
                {
                    iIndexPosition = 4;
                }
            }
            return iIndexPosition;
        }
        public static double ConvertIndexToAttribute(int index, IndicatorQT1 qt1)
        {
            double dbAttribute = 0;
            if ((!string.IsNullOrEmpty(qt1.Q1Unit) && !string.IsNullOrEmpty(qt1.Q2Unit))
                && (qt1.Q1 > 0 && qt1.Q2 > 0))
            {
                if (index == 0)
                {
                    //return midpoints
                    dbAttribute = qt1.Q1 / 2;
                }
                else if (index == 1)
                {
                    dbAttribute = (qt1.Q2 - ((qt1.Q2 - qt1.Q1) / 2));
                }
                else if (index == 2)
                {
                    dbAttribute = (qt1.Q3 - ((qt1.Q3 - qt1.Q2) / 2));
                }
                else if (index == 3)
                {
                    dbAttribute = (qt1.Q4 - ((qt1.Q4 - qt1.Q3) / 2));
                }
                else if (index == 4)
                {
                    dbAttribute = (qt1.Q5 - ((qt1.Q5 - qt1.Q4) / 2));
                }
            }
            return dbAttribute;
        }
        
        public static double ConvertAttributeToCategory(double attribute, IndicatorQT1 qt1)
        {
            double dbAttribute = attribute;
            if ((!string.IsNullOrEmpty(qt1.Q1Unit) && !string.IsNullOrEmpty(qt1.Q2Unit))
                && (qt1.Q1 > 0 && qt1.Q2 >0))
            {
                if (attribute < qt1.Q1)
                {
                    //return midpoints
                    dbAttribute = qt1.Q1 / 2;
                }
                else if (dbAttribute < qt1.Q2)
                {
                    dbAttribute = (qt1.Q2 - ((qt1.Q2 - qt1.Q1) / 2));
                }
                else if (dbAttribute < qt1.Q3)
                {
                    dbAttribute = (qt1.Q3 - ((qt1.Q3 - qt1.Q2) / 2));
                }
                else if (dbAttribute < qt1.Q4)
                {
                    dbAttribute = (qt1.Q4 - ((qt1.Q4 - qt1.Q3) / 2));
                }
                else if (dbAttribute < qt1.Q5)
                {
                    dbAttribute = (qt1.Q5 - ((qt1.Q5 - qt1.Q4) / 2));
                }
            }
            return dbAttribute;
        }
        public static double GetPopulationStartCount(double popStartCount,
            double popStartAllocation)
        {
            double dbPopCount = 0;
            dbPopCount = (popStartCount * (popStartAllocation / 100));
            dbPopCount = CalculatorHelpers.CheckForNaNandRound4(dbPopCount);
            if (dbPopCount <= 0)
            {
                dbPopCount = 1;
            }
            return dbPopCount;
        }
        public static double GetPopulationEndCount(double popStartCount,
            double popStartAllocation, double popEndAllocation)
        {
            double dbPopCount = 0;
            dbPopCount = (popStartCount * ((popStartAllocation / 100) * (popEndAllocation / 100)));
            dbPopCount = CalculatorHelpers.CheckForNaNandRound4(dbPopCount);
            if (dbPopCount <= 0)
            {
                dbPopCount = 1;
            }
            return dbPopCount;
        }
        public static double GetSDGPerPopulation(double sdgQuantity, double sdgStartAllocation,
            double sdgEndAllocation)
        {
            double sdgPerPopulation = 0;
            sdgPerPopulation = (sdgQuantity * (sdgStartAllocation / 100) * (sdgEndAllocation / 100));
            return sdgPerPopulation;
        }
        public static double GetSDGPerPopulationMember(double sdgQuantity, double sdgStartAllocation,
            double sdgEndAllocation, double popStartCount, double popStartAllocation, double popEndAllocation)
        {
            double sdgPerPopulationMember = 0;
            double perPopCount = (popStartCount * ((popStartAllocation / 100) * (popEndAllocation / 100)));
            perPopCount = CalculatorHelpers.CheckForNaNandRound4(perPopCount);
            if (perPopCount <= 0)
            {
                perPopCount = 1;
            }
            sdgPerPopulationMember = (sdgQuantity * (sdgStartAllocation / 100) * (sdgEndAllocation / 100)) / perPopCount;
            return sdgPerPopulationMember;
        }
        //does the script return more data to parse for meta?
        public enum META_TYPE
        {
            none = 0,
            row3_col4 = 1
        }
        public static async Task<META_TYPE> GetMetaType(CalculatorParameters calcParams, 
            string scriptFilePath)
        {
            META_TYPE mType = META_TYPE.none;
            if (!string.IsNullOrEmpty(scriptFilePath)
                && calcParams.ExtensionDocToCalcURI != null)
            {
                string sScript = await CalculatorHelpers.ReadText(
                    calcParams.ExtensionDocToCalcURI, scriptFilePath);
                //scripts configured to return ci ranges use "predict" as the clue
                if (sScript.Contains("predict"))
                {
                    mType = META_TYPE.row3_col4;
                }
            }
            return mType;
        }
        
        public static async Task<bool> FillMathResult(IndicatorQT1 meta, 
            META_TYPE metaType, CalculatorParameters calcParams, 
            StringBuilder sb, List<string> lastLines)
        {
            bool bHasMathResults = false;
            if (meta.MathResult.ToLower().StartsWith("http"))
            {
                bool bHasSaved = await CalculatorHelpers.SaveTextInURI(
                    calcParams.ExtensionDocToCalcURI, sb.ToString(), meta.MathResult);
                if (!string.IsNullOrEmpty(calcParams.ExtensionDocToCalcURI.ErrorMessage))
                {
                    meta.MathResult += calcParams.ExtensionDocToCalcURI.ErrorMessage;
                    //done with errormsg
                    calcParams.ExtensionDocToCalcURI.ErrorMessage = string.Empty;
                }
            }
            else
            {
                meta.MathResult = sb.ToString();
            }
            if (metaType == META_TYPE.row3_col4)
            {
                //last line of string should have the QTM vars
                if (lastLines.Count > 0)
                {
                    string sNewLine = string.Empty;
                    for (int x = 0; x < lastLines.Count; x++)
                    {
                        string[] vars = lastLines[x].Split(Constants.CSV_DELIMITERS);
                        bool bHasVars = false;
                        if (vars != null)
                        {
                            if (vars.Count() > 1)
                            {
                                bHasVars = true;
                            }
                            if (!bHasVars)
                            {
                                //manipulate to get it to work
                                sNewLine = lastLines[x].Replace(Constants.SPACE_DELIMITER, Constants.CSV_DELIMITER);
                                //r issue with dataframes
                                sNewLine = sNewLine.Replace(string.Concat(Constants.CSV_DELIMITER, Constants.CSV_DELIMITER),
                                    Constants.CSV_DELIMITER);
                                vars = sNewLine.Split(Constants.CSV_DELIMITERS);
                            }
                            if (vars != null)
                            {
                                if (vars.Count() != 4)
                                {
                                    if (vars.Count() == 3)
                                    {
                                        //py
                                        List<string> lstVars = new List<string>();
                                        lstVars.Add(string.Empty);
                                        lstVars.Add(vars[0]);
                                        lstVars.Add(vars[1]);
                                        lstVars.Add(vars[2]);
                                        vars = lstVars.ToArray();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                int iPos = vars.Count() - 3;
                                if (x == 1)
                                {
                                    //row count may be in first pos
                                    iPos = vars.Count() - 3;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.Q1 = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                    iPos = vars.Count() - 2;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.Q2 = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                    iPos = vars.Count() - 1;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.Q3 = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                }
                                else if (x == 2)
                                {
                                    //row count may be in first pos
                                    iPos = vars.Count() - 3;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.Q4 = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                    iPos = vars.Count() - 2;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.Q5 = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                    iPos = vars.Count() - 1;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.QT = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                }
                                else
                                {
                                    //row count may be in first pos
                                    iPos = vars.Count() - 3;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.QTM = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                    iPos = vars.Count() - 2;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.QTL = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                    iPos = vars.Count() - 1;
                                    if (iPos >= 0)
                                        if (vars[iPos] != null)
                                            meta.QTU = CalculatorHelpers.ConvertStringToDouble(vars[iPos]);
                                }
                            }
                        }
                    }
                }
            }
            //indicators store errors and then move to next calculation
            bHasMathResults = true;
            return bHasMathResults;
        }
        public static int GetRowCount(int iterations, int testCount)
        {
            int iRowCount = (iterations > 0) ? iterations : testCount;
            //iRowCount = (testCount < iRowCount) ? iRowCount : testCount;
            return iRowCount;
        }
    }

}
