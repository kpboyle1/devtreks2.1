using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run health care stock calculations for operations and components.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class OutcomeHCStockCalculator
    {
        public OutcomeHCStockCalculator(CalculatorParameters calcParameters)
        {
            BIHC1Calculator = new BIHCStockCalculator(calcParameters);
        }
        //stateful health care stock
        public BIHCStockCalculator BIHC1Calculator { get; set; }

        public bool AddCalculationsToCurrentElement(
            XElement currentCalculationsElement, XElement currentElement)
        {
            bool bHasCalculations = false;
            if (currentElement.Name.LocalName
                == Outcome.OUTCOME_PRICE_TYPES.outcomegroup.ToString())
            {
                //the outcome group can be used to insert calculators into 
                //descendant operations and run totals for each outcome
                bHasCalculations = BIHC1Calculator.SetTotalHCStockCalculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Outcome.OUTCOME_PRICE_TYPES.outcome.ToString()))
            {
                //bimachinerystockcalculator handles calculations
                bHasCalculations = BIHC1Calculator.SetOutcomeHCStockCalculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Output.OUTPUT_PRICE_TYPES.output.ToString()))
            {
                //resource stock calcs come from calculator results
                if (currentCalculationsElement != null)
                {
                    bHasCalculations = BIHC1Calculator.SetOutputHCStockCalculations(
                        currentCalculationsElement, currentElement);
                }
            }
            return bHasCalculations;
        }
    }
}
