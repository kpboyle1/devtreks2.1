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
    public class OCMN1StockSubscriber : OperationComponentCalculatorAsync
    {
        //constructors
        public OCMN1StockSubscriber() { }
        //constructor sets class (base) properties
        public OCMN1StockSubscriber(CalculatorParameters calcParameters)
            : base(calcParameters)
        {
            if (calcParameters.RelatedCalculatorsType
               == MN1CalculatorHelper.CALCULATOR_TYPES.mn01.ToString())
            {
                this.MN1StockCalculator = new OCMN1StockCalculator(calcParameters);
            }
        }

        //properties
        //stateful calculators that can be run by this extension
        public OCMN1StockCalculator MN1StockCalculator { get; set; }
        //if true, saves the totals using new object model
        public bool HasTotals { get; set; }
        //methods
        //subscribe to the events raised by the base class
        public async Task<bool> RunCalculator()
        {
            bool bHasAnalysis = false;
            //subscribe to the event raised by the publisher delegate
            //this.SetAncestorObjects += AddAncestorObjects;
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
            MN1CalculatorHelper.CALCULATOR_TYPES eCalculatorType
                = MN1CalculatorHelper.GetCalculatorType(
                this.GCCalculatorParams.CalculatorType);
            //run the stats and add them to statelement
            //run normally and save the same statelement and linkedviewelement
            e.HasCalculations = RunMN1Analysis(
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
        
        private bool RunMN1Analysis(XElement currentElement,
            XElement currentCalculationsElement)
        {
            bool bHasCalculations = false;
            if (this.GCCalculatorParams.SubApplicationType 
                == Constants.SUBAPPLICATION_TYPES.operationprices)
            {
                if (this.GCCalculatorParams.RelatedCalculatorsType
                    == MN1CalculatorHelper.CALCULATOR_TYPES.mn01.ToString())
                {
                    bHasCalculations
                        = this.MN1StockCalculator.AddCalculationsToCurrentElement(
                            currentCalculationsElement, currentElement);
                }
            }
            else if (this.GCCalculatorParams.SubApplicationType 
                == Constants.SUBAPPLICATION_TYPES.componentprices)
            {
                if (this.GCCalculatorParams.RelatedCalculatorsType
                    == MN1CalculatorHelper.CALCULATOR_TYPES.mn01.ToString())
                {
                    //assume progress01 is default
                    bHasCalculations
                        = this.MN1StockCalculator.AddCalculationsToCurrentElement(
                            currentCalculationsElement, currentElement);
                }
            }
            return bHasCalculations;
        }
        private bool TransferErrors(bool hasCalculations)
        {
            bool bHasGoodCalculations = hasCalculations;
            if (this.MN1StockCalculator != null)
            {
                this.GCCalculatorParams.ErrorMessage += this.MN1StockCalculator.BIMN1Calculator.GCCalculatorParams.ErrorMessage;
                this.GCCalculatorParams.ErrorMessage += this.MN1StockCalculator.BIMN1Calculator.GCCalculatorParams.ErrorMessage;
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
            if (this.GCCalculatorParams.NeedsCalculators
                && (this.GCCalculatorParams.StartingDocToCalcNodeName
                == OperationComponent.COMPONENT_PRICE_TYPES.componentgroup.ToString()
                || this.GCCalculatorParams.StartingDocToCalcNodeName
                == OperationComponent.OPERATION_PRICE_TYPES.operationgroup.ToString()))
            {
                //see the corresponding machinery stock pattern
            }
        }
    }
}
