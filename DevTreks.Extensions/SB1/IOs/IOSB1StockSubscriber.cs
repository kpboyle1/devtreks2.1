using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		This class derives from the CalculatorContracts.InputOutputCalculator 
    ///             class. It subscribes to the RunCalculation events raised by that class. 
    ///             It runs calculations on the nodes returned by that class, 
    ///             and returns a calculated xelement to the publisher for saving.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///NOTES        1. This calculator didn't need the SetAncestorObjects event raised 
    ///             by the base class.
    ///             2. This calculator doesn't use cumulative totals, so analyzers using 
    ///             this class need to carry out all aggregations.
    /// </summary>
    public class IOSB1StockSubscriber : InputOutputCalculatorAsync
    {
        //constructors
        public IOSB1StockSubscriber() { }
        //constructor sets class (base) properties
        public IOSB1StockSubscriber(CalculatorParameters calcParameters)
            : base(calcParameters)
        {
            if (calcParameters.RelatedCalculatorsType
                == SB1CalculatorHelper.CALCULATOR_TYPES.sb01.ToString())
            {
                this.SB1StockCalculator = new BISB1StockCalculatorAsync(calcParameters);
            }
        }

        //properties
        //stateful calculators that can be run by this extension
        public BISB1StockCalculatorAsync SB1StockCalculator { get; set; }
        //if true, saves the totals using new object model
        public bool HasTotals { get; set; }
        //methods
        //subscribe to the events raised by the base class
        public async Task<bool> RunCalculator()
        {
            bool bHasAnalysis = false;
            //subscribe to the event raised by the publisher delegate
            //does not use addancestors event
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
            //run the stats and add them to statelement
            SB1CalculatorHelper.CALCULATOR_TYPES eCalculatorType
                = SB1CalculatorHelper.GetCalculatorType(
                this.GCCalculatorParams.CalculatorType);
            if (eCalculatorType == SB1CalculatorHelper.CALCULATOR_TYPES.sb101
                || eCalculatorType == SB1CalculatorHelper.CALCULATOR_TYPES.sb102)
            {
                e.HasCalculations = RunSB1Calculation(eCalculatorType,
                    statElement, linkedViewElement);
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

        protected bool RunSB1Calculation(SB1CalculatorHelper.CALCULATOR_TYPES calcType, 
            XElement currentElement, XElement linkedViewElement)
        {
            bool bHasCalculations = false;
            SB1CalculatorHelper.CALCULATOR_TYPES eCalculatorType
                = SB1CalculatorHelper.GetCalculatorType(
                this.GCCalculatorParams.CalculatorType);
            switch (eCalculatorType)
            {
                case SB1CalculatorHelper.CALCULATOR_TYPES.sb101:
                    //serialize, run calcs, and deserialize
                    SBC1Calculator sbc1 = new SBC1Calculator();
                    bHasCalculations = sbc1.SetSB1C1Calculations(eCalculatorType, this.GCCalculatorParams,
                        linkedViewElement, currentElement);
                    break;
                case SB1CalculatorHelper.CALCULATOR_TYPES.sb102:
                    //serialize, run calcs, and deserialize
                    SBB1Calculator sbb1 = new SBB1Calculator();
                    bHasCalculations = sbb1.SetSB1B1Calculations(eCalculatorType, this.GCCalculatorParams,
                        linkedViewElement, currentElement);
                    break;
                default:
                    //should be running an analysis
                    break;
            }
            return bHasCalculations;
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
            //bHasCalculations = TransferErrors(bHasCalculations);
            ////some calcparams have to be passed back to this.calcparams
            //UpdateCalculatorParams(currentElement.Name.LocalName);
            return bHasCalculations;
        }
       
        //private bool TransferErrors(bool hasCalculations)
        //{
        //    bool bHasGoodCalculations = hasCalculations;
        //    if (this.ME2StockCalculator != null)
        //    {
        //        this.GCCalculatorParams.ErrorMessage += this.ME2StockCalculator.BIME2Calculator.GCCalculatorParams.ErrorMessage;
        //        this.GCCalculatorParams.ErrorMessage += this.ME2StockCalculator.BIME2Calculator.GCCalculatorParams.ErrorMessage;
        //    }
        //    if (!string.IsNullOrEmpty(this.GCCalculatorParams.ErrorMessage))
        //    {
        //        this.GCCalculatorParams.ErrorMessage += this.GCCalculatorParams.ErrorMessage;
        //        bHasGoodCalculations = false;
        //    }
        //    return bHasGoodCalculations;
        //}
        //private void UpdateCalculatorParams(string currentNodeName)
        //{
        //    //pass back NeedsXmlDocOnly in calcparams
        //    if (this.GCCalculatorParams.NeedsCalculators
        //        && (this.GCCalculatorParams.StartingDocToCalcNodeName
        //        == Input.INPUT_PRICE_TYPES.inputgroup.ToString()
        //        || this.GCCalculatorParams.StartingDocToCalcNodeName
        //        == Output.OUTPUT_PRICE_TYPES.outputgroup.ToString()))
        //    {
        //        //see the corresponding machinery stock pattern
        //    }
        //}
        //public static double GetMultiplierForInput(Input input)
        //{
        //    //input apps don't have a multipler (ops, comps, invests, and budgets do)
        //    double multiplier = 1;
        //    return multiplier;
        //}
        //public static double GetMultiplierForOutput(Output output)
        //{
        //    //output apps don't have a multipler (ops, comps, invests, and budgets do)
        //    double multiplier = 1;
        //    return multiplier;
        //}
        //public static double GetMultiplierForTechOutput(Output output)
        //{
        //    double multiplier = 1;
        //    if (output != null)
        //    {
        //        //outputs times, and compositionamount multipliers
        //        //output.Amount used in analyzers
        //        multiplier = output.CompositionAmount * output.Times;
        //        if (multiplier == 0)
        //        {
        //            multiplier = 1;
        //        }
        //    }
        //    return multiplier;
        //}
        //public static double GetMultiplierForTechInput(Input input)
        //{
        //    double multiplier = 1;
        //    if (input != null)
        //    {
        //        //inputs have time multipliers
        //        //the OCAmount, AOHAmount, and CAPAmount are used in analyzers
        //        multiplier = input.Times;
        //        if (multiplier == 0)
        //        {
        //            multiplier = 1;
        //        }
        //    }
        //    return multiplier;
        //}

    }
}
