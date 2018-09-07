using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run food nutrition stock calculations for operations and components.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class OutcomeFNSR01Calculator
    {
        public OutcomeFNSR01Calculator(CalculatorParameters calcParameters)
        {
            BIFNSR01Calculator = new BIFNSR01StockCalculator(calcParameters);
        }
        //stateful food nutrition stock
        public BIFNSR01StockCalculator BIFNSR01Calculator { get; set; }

        public bool AddCalculationsToCurrentElement(
            XElement currentCalculationsElement, XElement currentElement)
        {
            bool bHasCalculations = false;
            if (currentElement.Name.LocalName
                == Outcome.OUTCOME_PRICE_TYPES.outcomegroup.ToString())
            {
                //outcomes are not used yet in this analyzer
                //bHasCalculations = BIFNSR01Calculator.SetTotalFNSR01StockCalculations(
                //    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Outcome.OUTCOME_PRICE_TYPES.outcome.ToString()))
            {
                //outcomes are not used yet in this analyzer
                ////bifnsr01stockcalculator handles calculations
                //bHasCalculations = BIFNSR01Calculator.SetOutcomeFNSR01StockCalculations(
                //    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Output.OUTPUT_PRICE_TYPES.output.ToString()))
            {
                //resource stock calcs come from calculator results
                if (currentCalculationsElement != null)
                {
                    //outputs are not used yet in this analyzer
                    //bHasCalculations = BIFNSR01Calculator.SetOutputFNSR01StockCalculations(
                    //    currentCalculationsElement, currentElement);
                }
            }
            return bHasCalculations;
        }
    }
}
