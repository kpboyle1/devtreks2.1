﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using Errors = DevTreks.Exceptions.DevTreksErrors;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		This class derives from the CalculatorContracts.OperationComponentCalculator 
    ///             class. It subscribes to the RunCalculation event raised by that 
    ///             class. It runs statistical calculations on the nodes returned 
    ///             by that class, and passes back the calculated nodes to the 
    ///             publishing class.
    ///Author:		www.devtreks.org
    ///Date:		2014, April
    ///References:	www.devtreks.org/helptreks/linkedviews/help/linkedview/HelpFile/148
    ///NOTES        1. 
    /// </summary>
    public class OCAnalyzerSubscriber : OperationComponentCalculatorAsync
    {
        //constructors
        public OCAnalyzerSubscriber() { }
        //constructor sets class properties
        public OCAnalyzerSubscriber(CalculatorParameters calcParameters,
            ARSAnalyzerHelper.ANALYZER_TYPES analyzerType)
            : base(calcParameters)
        {
            this.AnalyzerType = analyzerType;
        }

        //properties
        //analyzers specific to this extension
        public ARSAnalyzerHelper.ANALYZER_TYPES AnalyzerType { get; set; }
        //analyzers that need persistent state for a full observation doc
        CostEffectivenessAnalyzer1 ceAnalyzer { get; set; }

        public async Task<bool> RunAnalyzer()
        {
            bool bHasAnalysis = false;
            //subscribe to the event raised by the publisher delegate
            this.RunCalculation += AddCalculations;
            //run the analyses (raising the RunCalculation event 
            //from the base event publisher for each node)
            bHasAnalysis = await this.StreamAndSaveCalculationAsync();
            return bHasAnalysis;
        }
        //define the actions to take when the event is raised
        public void AddCalculations(object sender, CustomEventArgs e)
        {
            //pass a byref xelement from the publisher's data
            XElement statElement = new XElement(e.CurrentElement);
            //set parameters to those set in the publisher
            this.GCCalculatorParams = e.CalculatorParams;
            //run the stats and add them to statelement
            RunStatisticalCalculation(statElement);
            //pass the new statelement back to the publisher
            //by setting the CalculatedElement property of CustomEventArgs
            e.CurrentElement = new XElement(statElement);
        }
        private bool RunStatisticalCalculation(XElement currentElement)
        {
            bool bHasCalculations = false;
            //1. set parameters needed by updates collection
            this.GCCalculatorParams.CurrentElementNodeName
                = currentElement.Name.LocalName;
            this.GCCalculatorParams.CurrentElementURIPattern
                = CalculatorHelpers.MakeNewURIPatternFromElement(
                this.GCCalculatorParams.ExtensionDocToCalcURI.URIPattern, currentElement);
            //2. carry out calculations 
            bHasCalculations = RunOperationComponentStatistic(currentElement);
            if (!bHasCalculations
                && string.IsNullOrEmpty(this.GCCalculatorParams.ErrorMessage))
                this.GCCalculatorParams.ErrorMessage
                    += Errors.MakeStandardErrorMsg("ANALYSES_CANTRUN");
            return bHasCalculations;
        }
        private bool RunOperationComponentStatistic(XElement currentElement)
        {
            bool bHasStatistics = false;
            if (this.AnalyzerType
                == ARSAnalyzerHelper.ANALYZER_TYPES.statistics01)
            {
                StatisticalAnalyzer1 statsAnalyzer1
                    = new StatisticalAnalyzer1(this.GCCalculatorParams);
                bHasStatistics = statsAnalyzer1.SetStatisticalCalculation(
                    currentElement);
                statsAnalyzer1 = null;
            }
            else if (ARSAnalyzerHelper.IsEffectivenessAnalysis(
                this.AnalyzerType))
            {
                if (this.ceAnalyzer == null)
                {
                    this.ceAnalyzer
                        = new CostEffectivenessAnalyzer1(this.GCCalculatorParams);
                }
                bHasStatistics = SetEffectivenessStatistic(currentElement);
            }
            else
            {
                this.GCCalculatorParams.ErrorMessage
                    = Errors.MakeStandardErrorMsg("ANALYSES_NOANALYZER");
            }
            return bHasStatistics;
        }

        public bool SetEffectivenessStatistic(XElement currentElement)
        {
            bool bHasStatistics = false;
            if (currentElement.HasAttributes)
            {
                //refactor: is a new stats analyzer needed by every node?
                StatisticalAnalyzer1 statsAnalyzer2
                    = new StatisticalAnalyzer1(this.GCCalculatorParams);
                //run the regular mean and standard deviation statistics
                bHasStatistics = statsAnalyzer2.SetStatisticalCalculation(
                    currentElement);
                if (bHasStatistics)
                {
                    //run the cost effectiveness statistic
                    bHasStatistics = this.ceAnalyzer
                        .SetCostEffectivenessCalculation(currentElement);
                }
                statsAnalyzer2 = null;
            }
            else
            {
                //grouping nodes sometimes need to manipulate children for display
                bHasStatistics = this.ceAnalyzer.SetCostEffectivenessCalculation(
                       currentElement);
            }
            return bHasStatistics;
        }
    }
}
