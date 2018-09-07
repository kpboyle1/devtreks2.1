using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		This class derives from the CalculatorContracts.OperationComponentCalculator 
    ///             class. It subscribes to the RunCalculation and SetAncestorObjects events 
    ///             raised by that class. It runs calculations on the nodes 
    ///             returned by that class, and returns a calculated xelement 
    ///             to the publisher for saving.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///NOTES        1. 
    /// </summary>
    public class OutcomeFNStockSubscriber : OperationComponentCalculatorAsync
    {
        //constructors
        public OutcomeFNStockSubscriber() { }
        //constructor sets class (base) properties
        public OutcomeFNStockSubscriber(CalculatorParameters calcParameters)
            : base(calcParameters)
        {
            if (calcParameters.RelatedCalculatorType
                == FNCalculatorHelper.CALCULATOR_TYPES.foodfactUSA1.ToString())
            {
                this.FNStockCalculator = new OutcomeFNStockCalculator(calcParameters);
            }
            else if (calcParameters.RelatedCalculatorType
                == FNCalculatorHelper.CALCULATOR_TYPES.foodnutSR01.ToString())
            {
                this.FNSR01StockCalculator = new OutcomeFNSR01Calculator(calcParameters);
            }
        }

        //properties
        //stateful calculators that can be run by this extension
        public OutcomeFNStockCalculator FNStockCalculator { get; set; }
        public OutcomeFNSR01Calculator FNSR01StockCalculator { get; set; }
        //methods
        //subscribe to the events raised by the base class
        public async Task<bool> RunCalculator()
        {
            bool bHasAnalysis = false;
            //subscribe to the event raised by the publisher delegate
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
            e.HasCalculations = RunOperationComponentCalculation(
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
        private bool RunOperationComponentCalculation(XElement currentElement,
            XElement currentCalculationsElement)
        {
            bool bHasCalculations = false;
            if (this.GCCalculatorParams.CalculatorType.StartsWith(
                CalculatorHelpers.CALCULATOR_TYPES.operation.ToString()))
            {
                if (this.GCCalculatorParams.RelatedCalculatorType
                    == FNCalculatorHelper.CALCULATOR_TYPES.foodfactUSA1.ToString())
                {
                    //assume resources01 is default
                    bHasCalculations
                        = this.FNStockCalculator.AddCalculationsToCurrentElement(
                            currentCalculationsElement, currentElement);
                }
                else if (this.GCCalculatorParams.RelatedCalculatorType
                    == FNCalculatorHelper.CALCULATOR_TYPES.foodnutSR01.ToString())
                {
                    //assume resources01 is default
                    bHasCalculations
                        = this.FNSR01StockCalculator.AddCalculationsToCurrentElement(
                            currentCalculationsElement, currentElement);
                }
            }
            else if (this.GCCalculatorParams.CalculatorType.StartsWith(
                CalculatorHelpers.CALCULATOR_TYPES.component.ToString()))
            {
                if (this.GCCalculatorParams.RelatedCalculatorType
                    == FNCalculatorHelper.CALCULATOR_TYPES.foodfactUSA1.ToString())
                {
                    //assume resources01 is default
                    bHasCalculations
                        = this.FNStockCalculator.AddCalculationsToCurrentElement(
                            currentCalculationsElement, currentElement);
                }
            }
            bHasCalculations = TransferErrors(bHasCalculations);
            //some calcparams have to be passed back to this.calcparams
            UpdateCalculatorParams(currentElement.Name.LocalName);
            return bHasCalculations;
        }
        private void AddAncestorObjects(XElement currentElement)
        {
            XElement currentCalculationsElement = null;
            currentCalculationsElement
                    = CalculatorHelpers.GetCalculator(this.GCCalculatorParams,
                    currentElement);
            if (currentElement.Name.LocalName
                .EndsWith(OperationComponent.OPERATION_PRICE_TYPES.operation.ToString())
                || currentElement.Name.LocalName
                .EndsWith(OperationComponent.COMPONENT_PRICE_TYPES.component.ToString()))
            {
                //need the Outcome.amount multiplier at the input level (so set the parentopcomp here)
                this.GCCalculatorParams.ParentOperationComponent
                 = new OperationComponent();
                this.GCCalculatorParams.ParentOperationComponent
                    .SetOperationComponentProperties(
                    this.GCCalculatorParams, currentCalculationsElement,
                    currentElement);
                //init the total costs to zero
                this.GCCalculatorParams.ParentOperationComponent.InitTotalCostsProperties();
                //all calculators need it
                if (this.FNStockCalculator != null)
                {
                    this.FNStockCalculator.BIFN1Calculator.GCCalculatorParams.ParentOperationComponent
                        = this.GCCalculatorParams.ParentOperationComponent;
                }
                else if (this.FNSR01StockCalculator != null)
                {
                    this.FNSR01StockCalculator.BIFNSR01Calculator.GCCalculatorParams.ParentOperationComponent
                        = this.GCCalculatorParams.ParentOperationComponent;
                }
            }
        }
        public static double GetMultiplierForOperation(OperationComponent opComp)
        {
            //ops, comps, budgets and investments use the times multiplier
            double multiplier = 1;
            if (opComp != null)
            {
                multiplier = opComp.Amount;
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
            if (this.FNStockCalculator != null)
            {
                this.GCCalculatorParams.ErrorMessage += this.FNStockCalculator.BIFN1Calculator.GCCalculatorParams.ErrorMessage;
                this.GCCalculatorParams.ErrorMessage += this.FNStockCalculator.BIFN1Calculator.GCCalculatorParams.ErrorMessage;
            }
            else if (this.FNSR01StockCalculator != null)
            {
                this.GCCalculatorParams.ErrorMessage += this.FNSR01StockCalculator.BIFNSR01Calculator.GCCalculatorParams.ErrorMessage;
                this.GCCalculatorParams.ErrorMessage += this.FNSR01StockCalculator.BIFNSR01Calculator.GCCalculatorParams.ErrorMessage;
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
            //pass back NeedsXmlDOutcomeOnly in calcparams
            //note that 0.8.8 did not end up using Use Same Calculator Pack in Children
            //for resources 02 or resources 02a analyses
            //but keep this pattern for future use
            if (this.GCCalculatorParams.NeedsCalculators
                && (this.GCCalculatorParams.StartingDocToCalcNodeName
                == OperationComponent.COMPONENT_PRICE_TYPES.componentgroup.ToString()
                || this.GCCalculatorParams.StartingDocToCalcNodeName
                == OperationComponent.OPERATION_PRICE_TYPES.operationgroup.ToString()))
            {
                //see the corresponding food nutrition Stock pattern
            }
        }
    }
}


