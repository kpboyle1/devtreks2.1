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
    public class BISB1StockSubscriber : BudgetInvestmentCalculatorAsync
    {
        //constructors
        //constructor sets class (base) properties
        public BISB1StockSubscriber(CalculatorParameters calcParameters)
            : base(calcParameters)
        {
            //this calls the constructors which also inherit from bicalculator for base params
            if (calcParameters.RelatedCalculatorsType
               == SB1CalculatorHelper.CALCULATOR_TYPES.sb01.ToString())
            {
                this.SB1StockCalculator = new BISB1StockCalculatorAsync(calcParameters);
            }
        }
        //properties
        //stateful calculator being run by this extension
        public BISB1StockCalculatorAsync SB1StockCalculator { get; set; }
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
            SB1CalculatorHelper.CALCULATOR_TYPES eCalculatorType
               = SB1CalculatorHelper.GetCalculatorType(
               this.GCCalculatorParams.CalculatorType);
            if (eCalculatorType == SB1CalculatorHelper.CALCULATOR_TYPES.sb101
                || eCalculatorType == SB1CalculatorHelper.CALCULATOR_TYPES.sb102)
            {
                //only ios use calculators -remaining base elements just run analyses
            }
            else
            {
                //run normally and save the same statelement and linkedviewelement
                e.HasCalculations = RunSB1Analysis(
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
        private bool RunSB1Analysis(XElement currentElement,
            XElement currentCalculationsElement)
        {
            bool bHasCalculations = false;
            if (this.GCCalculatorParams.RelatedCalculatorsType
                    == SB1CalculatorHelper.CALCULATOR_TYPES.sb01.ToString())
            {
                bHasCalculations
                    = this.SB1StockCalculator.AddSB1CalculationsToCurrentElement(
                        currentCalculationsElement, currentElement);
            }
            return bHasCalculations;
        }        
    }
}

