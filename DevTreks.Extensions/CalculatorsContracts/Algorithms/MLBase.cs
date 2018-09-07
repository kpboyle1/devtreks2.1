using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DevTreks.Extensions.Algorithms
{
    /// <summary>
    ///Purpose:		MLBase algorithm
    ///Author:		www.devtreks.org
    ///Date:		2018, May
    ///References:	base machine learning algorithm
    ///</summary>
    public class MLBase : Calculator1
    {
        public MLBase()
            : base() { }
        public MLBase(int indicatorIndex, string label, string[] mathTerms,
            string[] colNames, string[] depColNames, string subalgorithm, 
            int ciLevel, int iterations, int random, 
            IndicatorQT1 qT1, CalculatorParameters calcParams)
            : base()
        {
            _colNames = colNames;
            _depColNames = depColNames;
            _mathTerms = mathTerms;
            _subalgorithm = subalgorithm;
            _label = label;
            _ciLevel = ciLevel;
            _random = random;
            _iterations = iterations;
            if (qT1 == null)
                qT1 = new IndicatorQT1();
            IndicatorQT = new IndicatorQT1(qT1);
            Params = calcParams;
        }
        public CalculatorParameters Params { get; set; }
        public static string[] _colNames { get; set; }
        //all of the the dependent var column names including intercept
        public static string[] _depColNames { get; set; }
        //corresponding Ix.Qx names (1 less count because no dependent var)
        public static string[] _mathTerms { get; set; }
        //which regression algorithm is being run?
        public static string _subalgorithm { get; set; }
        public static string _label { get; set; }
        public static int _ciLevel { get; set; }
        public static int _iterations { get; set; }
        public static int _random { get; set; }
        //input and output
        public IndicatorQT1 IndicatorQT { get; set; }
        //output
        //store the results of calculations
        public List<List<string>> DataResults = new List<List<string>>();
        //actual columnnames used in analysis
        public static string[] _actualColNames { get; set; }
        //actual instructions used in analysis
        public static string[] _actualMLInstructions { get; set; }

    }
}
