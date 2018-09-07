﻿using System.Xml.Linq;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run monitoring and evaluation indicator stock calculations for outcomes
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class OutcomeNPV1StockCalculator
    {
        public OutcomeNPV1StockCalculator(CalculatorParameters calcParameters)
        {
            BINPV1Calculator = new BINPV1StockCalculator(calcParameters);
        }
        //stateful health care stock
        public BINPV1StockCalculator BINPV1Calculator { get; set; }

        public bool AddCalculationsToCurrentElement(
            ref XElement currentCalculationsElement, ref XElement currentElement)
        {
            bool bHasCalculations = false;
            if (currentElement.Name.LocalName
                == Outcome.OUTCOME_PRICE_TYPES.outcomegroup.ToString())
            {
                //the operation group can be used to insert calculators into 
                //descendant operations and run totals for each operation
                bHasCalculations = BINPV1Calculator.SetOutcomeGroupNPV1Calculations(
                    ref currentCalculationsElement, ref currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Outcome.OUTCOME_PRICE_TYPES.outcome.ToString()))
            {
                //bimachinerystockcalculator handles calculations
                bHasCalculations = BINPV1Calculator.SetOutcomeNPV1Calculations(
                    ref currentCalculationsElement, ref currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Output.OUTPUT_PRICE_TYPES.output.ToString()))
            {
                bHasCalculations = BINPV1Calculator.SetTechOutputNPV1Calculations(
                    ref currentCalculationsElement, ref currentElement);
            }
            return bHasCalculations;
        }
        
    }
}
