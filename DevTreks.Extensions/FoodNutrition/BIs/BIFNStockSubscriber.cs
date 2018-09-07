﻿using System;
using System.Threading.Tasks;
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
    public class BIFNStockSubscriber : BudgetInvestmentCalculatorAsync
    {
        //constructors
        //constructor sets class (base) properties
        public BIFNStockSubscriber(CalculatorParameters calcParameters)
            : base(calcParameters)
        {
            //this calls the constructors which also inherit from bicalculator for base params
            if (calcParameters.RelatedCalculatorType
                == FNCalculatorHelper.CALCULATOR_TYPES.foodfactUSA1.ToString())
            {
                this.BIFN1Calculator = new BIFNStockCalculator(calcParameters);
            }
            else if (calcParameters.RelatedCalculatorType
                == FNCalculatorHelper.CALCULATOR_TYPES.foodnutSR01.ToString())
            {
                this.BIFNSR01Calculator = new BIFNSR01StockCalculator(calcParameters);
            }
        }

        //properties
        //stateful calculator being run by this extension
        public BIFNStockCalculator BIFN1Calculator { get; set; }
        public BIFNSR01StockCalculator BIFNSR01Calculator { get; set; }

        //methods
        //subscribe to the events raised by the base class
        public async Task<bool> RunCalculator()
        {
            bool bHasAnalysis = false;
            //subscribe to the events raised by the publisher delegate
            this.SetAncestorObjects += AddAncestorObjects;
            this.RunCalculation += AddCalculations;
            //run the calculation (raising the events
            //from the base event publisher for each node)
            bHasAnalysis = await this.StreamAndSaveCalculationAsync();
            return bHasAnalysis;
        }
        //define the actions to take when the event is raised
        public void AddAncestorObjects(object sender, CustomEventArgs e)
        {
            //pass a byref xelement from the publisher's data
            XElement statElement = new XElement(e.CurrentElement);
            //run the stats and add them to statelement
            AddAncestorObjects(statElement);
            //pass the new statelement back to the publisher
            //by setting the CalculatedElement property of CustomEventArgs
            e.CurrentElement = new XElement(statElement);
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
            //run the stats and add them to statelement
            e.HasCalculations = RunBudgetInvestmentCalculation(
                statElement, linkedViewElement);
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

        private bool RunBudgetInvestmentCalculation(XElement currentElement,
            XElement currentCalculationsElement)
        {
            bool bHasCalculations = false;
            CalculatorHelpers.CALCULATOR_TYPES eCalculatorType
                = CalculatorHelpers.GetCalculatorType(this.GCCalculatorParams.CalculatorType);
            switch (eCalculatorType)
            {
                case CalculatorHelpers.CALCULATOR_TYPES.budget:
                    if (this.GCCalculatorParams.RelatedCalculatorType
                        == FNCalculatorHelper.CALCULATOR_TYPES.foodfactUSA1.ToString())
                    {
                        bHasCalculations
                            = BIFN1Calculator.AddStock1CalculationsToCurrentElement(
                                currentCalculationsElement, currentElement);
                    }
                    else if (this.GCCalculatorParams.RelatedCalculatorType
                        == FNCalculatorHelper.CALCULATOR_TYPES.foodnutSR01.ToString())
                    {
                        bHasCalculations
                            = BIFNSR01Calculator.AddStock1CalculationsToCurrentElement(
                                currentCalculationsElement, currentElement);
                    }
                    break;
                case CalculatorHelpers.CALCULATOR_TYPES.investment:
                    if (this.GCCalculatorParams.RelatedCalculatorType
                        == FNCalculatorHelper.CALCULATOR_TYPES.foodfactUSA1.ToString())
                    {
                        bHasCalculations
                            = BIFN1Calculator.AddStock1CalculationsToCurrentElement(
                                currentCalculationsElement, currentElement);
                    }
                    break;
                default:
                    break;
            }
            bHasCalculations = TransferErrors(bHasCalculations);
            //some calcparams have to be passed back to this.calcparams
            UpdateCalculatorParams(currentElement.Name.LocalName);
            return bHasCalculations;
        }
        //only tps are needed by descendants using this event 
        private void AddAncestorObjects(XElement currentElement)
        {
            XElement currentCalculationsElement = null;
            currentCalculationsElement
                    = CalculatorHelpers.GetCalculator(this.GCCalculatorParams, currentElement);
            if (currentElement.Name.LocalName
                == BudgetInvestment.BUDGET_TYPES.budgettimeperiod.ToString()
                || currentElement.Name.LocalName
                == BudgetInvestment.INVESTMENT_TYPES.investmenttimeperiod.ToString())
            {
                //need the tp.amount multiplier
                //set the stateful ancestor object
                //last time period date's are used in some calculations
                DateTime dtLastTimePeriodDate = new DateTime(1000, 12, 1);
                bool bHasLastDate = false;
                if (this.GCCalculatorParams.ParentTimePeriod != null)
                {
                    bHasLastDate = true;
                    dtLastTimePeriodDate = this.GCCalculatorParams.ParentTimePeriod.Date;
                }
                this.GCCalculatorParams.ParentTimePeriod = new TimePeriod();
                //deserialize xml to object, just need the amount, maybe date
                this.GCCalculatorParams.ParentTimePeriod.SetTimePeriodProperties(
                    this.GCCalculatorParams, currentCalculationsElement, currentElement);
                //copy some of the last tps properties to current tp 
                if (!bHasLastDate) dtLastTimePeriodDate
                    = this.GCCalculatorParams.ParentTimePeriod.Date;
                this.GCCalculatorParams.ParentTimePeriod.LastPeriodDate
                    = dtLastTimePeriodDate;
                //all calculators need it
                if (this.BIFN1Calculator != null)
                {
                    this.BIFN1Calculator.GCCalculatorParams.ParentTimePeriod
                        = this.GCCalculatorParams.ParentTimePeriod;
                }
                else if (this.BIFNSR01Calculator != null)
                {
                    this.BIFNSR01Calculator.GCCalculatorParams.ParentTimePeriod
                        = this.GCCalculatorParams.ParentTimePeriod;
                }
            }
            else if (currentElement.Name.LocalName
                .EndsWith(OperationComponent.OPERATION_PRICE_TYPES.operation.ToString())
                || currentElement.Name.LocalName
                .EndsWith(OperationComponent.COMPONENT_PRICE_TYPES.component.ToString()))
            {
                //need the oc.amount multiplier
                this.GCCalculatorParams.ParentOperationComponent
                 = new OperationComponent();
                this.GCCalculatorParams.ParentOperationComponent
                    .SetOperationComponentProperties(
                    this.GCCalculatorParams, currentCalculationsElement,
                    currentElement);
                //init the total costs to zero
                this.GCCalculatorParams.ParentOperationComponent.InitTotalCostsProperties();
                //all calculators need it
                if (this.BIFN1Calculator != null)
                {
                    this.BIFN1Calculator.GCCalculatorParams.ParentOperationComponent
                        = this.GCCalculatorParams.ParentOperationComponent;
                }
                else if (this.BIFNSR01Calculator != null)
                {
                    this.BIFNSR01Calculator.GCCalculatorParams.ParentOperationComponent
                        = this.GCCalculatorParams.ParentOperationComponent;
                }
            }
        }
        public static double GetMultiplierForTimePeriod(TimePeriod tp)
        {
            //ops, comps, budgets and investments use the times multiplier
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
            if (this.BIFN1Calculator != null)
            {
                this.GCCalculatorParams.ErrorMessage += BIFN1Calculator.GCCalculatorParams.ErrorMessage;
            }
            else if (this.BIFNSR01Calculator != null)
            {
                this.GCCalculatorParams.ErrorMessage += BIFNSR01Calculator.GCCalculatorParams.ErrorMessage;
            }
            if (!string.IsNullOrEmpty(this.GCCalculatorParams.ErrorMessage))
            {
                this.GCCalculatorParams.ErrorMessage += this.GCCalculatorParams.ErrorMessage;
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
                //see corresponding food nutrition stock example
            }
        }

    }
}
