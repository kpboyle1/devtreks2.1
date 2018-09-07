using System.Xml.Linq;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run monitoring and evaluation indicator stock calculations for outcomes
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class OutcomeME2StockCalculator
    {
        public OutcomeME2StockCalculator(CalculatorParameters calcParameters)
        {
            BIME2Calculator = new BIME2StockCalculator(calcParameters);
        }
        //stateful health care stock
        public BIME2StockCalculator BIME2Calculator { get; set; }

        public bool AddCalculationsToCurrentElement(
            XElement currentCalculationsElement, XElement currentElement)
        {
            bool bHasCalculations = false;
            if (currentElement.Name.LocalName
                == Outcome.OUTCOME_PRICE_TYPES.outcomegroup.ToString())
            {
                //the operation group can be used to insert calculators into 
                //descendant operations and run totals for each operation
                bHasCalculations = BIME2Calculator.SetOutcomeGroupME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Outcome.OUTCOME_PRICE_TYPES.outcome.ToString()))
            {
                //bimachinerystockcalculator handles calculations
                bHasCalculations = BIME2Calculator.SetOutcomeME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Output.OUTPUT_PRICE_TYPES.output.ToString()))
            {
                bHasCalculations = BIME2Calculator.SetTechOutputME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            return bHasCalculations;
        }
        
    }
}
