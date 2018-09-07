using System.Xml.Linq;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run monitoring and evaluation indicator stock calculations for outcomes
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class OutcomeFNStockCalculator
    {
        public OutcomeFNStockCalculator(CalculatorParameters calcParameters)
        {
            BIFN1Calculator = new BIFNStockCalculator(calcParameters);
        }
        //stateful health care stock
        public BIFNStockCalculator BIFN1Calculator { get; set; }

        public bool AddCalculationsToCurrentElement(
            XElement currentCalculationsElement, XElement currentElement)
        {
            bool bHasCalculations = false;
            if (currentElement.Name.LocalName
                == Outcome.OUTCOME_PRICE_TYPES.outcomegroup.ToString())
            {
                //outcomes are not used yet in this analyzer
                //bHasCalculations = BIFN1Calculator.SetOutcomeGroupFNCalculations(
                //    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Outcome.OUTCOME_PRICE_TYPES.outcome.ToString()))
            {
                //outcomes are not used yet in this analyzer
                //bHasCalculations = BIFN1Calculator.SetOutcomeFNCalculations(
                //    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Output.OUTPUT_PRICE_TYPES.output.ToString()))
            {
                //outputs are not used yet in this analyzer
                //bHasCalculations = BIFN1Calculator.SetTechOutputFNCalculations(
                //    currentCalculationsElement, currentElement);
            }
            return bHasCalculations;
        }

    }
}