﻿using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		This class derives from the CalculatorContracts.BudgetInvestmentCalculator 
    ///             class. It subscribes to the RunCalculation event
    ///             raised by that class. It runs calculations on the nodes 
    ///             returned by that class, and returns a calculated xelement 
    ///             to the publisher for saving.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///NOTES        1. 
    /// </summary>
    public class BIME2StockSubscriber : BudgetInvestmentCalculatorAsync
    {
        //constructors
        //constructor sets class (base) properties
        public BIME2StockSubscriber(CalculatorParameters calcParameters)
            : base(calcParameters)
        {
            //this calls the constructors which also inherit from bicalculator for base params
            if (calcParameters.RelatedCalculatorsType
               == ME2CalculatorHelper.CALCULATOR_TYPES.me2.ToString())
            {
                this.ME2StockCalculator = new BIME2StockCalculator(calcParameters);
            }
        }

        //properties
        //stateful calculator being run by this extension
        public BIME2StockCalculator ME2StockCalculator { get; set; }
        //if true, saves the totals using new object model
        public bool HasTotals { get; set; }
        //methods
        //subscribe to the events raised by the base class
        public async Task<bool> RunCalculator()
        {
            bool bHasAnalysis = false;
            //subscribe to the events raised by the publisher delegate
            //AddAncestors was not needed
            this.RunCalculation += AddCalculations;
            //run the calculation (raising the events
            //from the base event publisher for each node)
            bHasAnalysis = await this.StreamAndSaveCalculationAsync();
            return bHasAnalysis;
        }
        //define the actions to take when the event is raised
        public void AddCalculations(object sender, CustomEventArgs e)
        {
            //pass a byref xelement from the publisher's data
            XElement statElement = null;
            XElement linkedViewElement = null;
            if (e.CurrentElement != null)
                statElement = new XElement(e.CurrentElement);
            if (e.LinkedViewElement != null)
                linkedViewElement = new XElement(e.LinkedViewElement);
            ME2CalculatorHelper.CALCULATOR_TYPES eCalculatorType
               = ME2CalculatorHelper.GetCalculatorType(
               this.GCCalculatorParams.CalculatorType);
            if (eCalculatorType == ME2CalculatorHelper.CALCULATOR_TYPES.me2)
            {
                e.HasCalculations = RunME2Calculation(eCalculatorType,
                    statElement, linkedViewElement);
            }
            else
            {
                //run normally and save the same statelement and linkedviewelement
                e.HasCalculations = RunME2Analysis(
                    statElement, linkedViewElement);
            }
            if (e.HasCalculations)
            {
                //pass the new statelement back to the publisher
                //by setting the CalculatedElement property of CustomEventArgs
                if (statElement != null)
                    e.CurrentElement = new XElement(statElement);
                if (linkedViewElement != null)
                    e.LinkedViewElement = new XElement(linkedViewElement);
            }
        }
        private bool RunME2Calculation(ME2CalculatorHelper.CALCULATOR_TYPES calculatorType,
            XElement currentElement, XElement currentCalculationsElement)
        {
            bool bHasCalculations = false;
            switch (calculatorType)
            {
                case ME2CalculatorHelper.CALCULATOR_TYPES.me2:
                    //serialize, run calcs, and deserialize
                    ME2Calculator me2 = new ME2Calculator(this.GCCalculatorParams);
                    bHasCalculations = me2.SetME2Calculations(calculatorType, this.GCCalculatorParams,
                        currentCalculationsElement, currentElement);
                    break;
                default:
                    //should be running an analysis
                    break;
            }
            return bHasCalculations;
        }
        private bool RunME2Analysis(XElement currentElement,
            XElement currentCalculationsElement)
        {
            bool bHasCalculations = false;
            if (this.GCCalculatorParams.SubApplicationType
                == Constants.SUBAPPLICATION_TYPES.budgets)
            {
                if (this.GCCalculatorParams.RelatedCalculatorsType
                    == ME2CalculatorHelper.CALCULATOR_TYPES.me2.ToString())
                {
                    bHasCalculations
                        = ME2StockCalculator.AddME2CalculationsToCurrentElement(
                            currentCalculationsElement, currentElement);
                }
            }
            else if (this.GCCalculatorParams.SubApplicationType
                == Constants.SUBAPPLICATION_TYPES.investments)
            {
                if (this.GCCalculatorParams.RelatedCalculatorsType
                    == ME2CalculatorHelper.CALCULATOR_TYPES.me2.ToString())
                {
                    bHasCalculations
                        = ME2StockCalculator.AddME2CalculationsToCurrentElement(
                            currentCalculationsElement, currentElement);
                }
            }
            bHasCalculations = TransferErrors(bHasCalculations);
            //some calcparams have to be passed back to this.calcparams
            UpdateCalculatorParams(currentElement.Name.LocalName);
            return bHasCalculations;
        }        
        public static double GetMultiplierForTimePeriod(TimePeriod tp)
        {
            //this subscriber does not use these multipliers
            double multiplier = 1;
            if (tp != null)
            {
                multiplier = tp.Amount;
                if (multiplier == 0)
                {
                    multiplier = 1;
                }
            }
            return multiplier;
        }
        private bool TransferErrors(bool hasCalculations)
        {
            bool bHasGoodCalculations = hasCalculations;
            //the bicalculators have their own set of calcs and analysis params
            if (this.ME2StockCalculator != null)
            {
                this.GCCalculatorParams.ErrorMessage += this.ME2StockCalculator.GCCalculatorParams.ErrorMessage;
            }
            if (!string.IsNullOrEmpty(this.GCCalculatorParams.ErrorMessage))
            {
                bHasGoodCalculations = false;
            }
            return bHasGoodCalculations;
        }
        private void UpdateCalculatorParams(string currentNodeName)
        {
            //pass back NeedsXmlDocOnly in calcparams
            //note that 0.8.8 did not end up using Use Same Calculator Pack in Children
            //for resources 02 or resources 02a analyses
            //but keep this pattern for future use
            if (this.GCCalculatorParams.NeedsCalculators
                && (this.GCCalculatorParams.StartingDocToCalcNodeName
                == BudgetInvestment.BUDGET_TYPES.budget.ToString()
                || this.GCCalculatorParams.StartingDocToCalcNodeName
                == BudgetInvestment.INVESTMENT_TYPES.investment.ToString()))
            {
                //see corresponding machinery stock example
            }
        }

    }
}

