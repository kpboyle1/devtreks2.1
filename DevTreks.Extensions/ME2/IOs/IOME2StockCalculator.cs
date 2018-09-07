using System.Xml.Linq;


namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run monitoring and evaluation indicator stock calculations for inputs 
    ///             and outputs.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class IOME2StockCalculator
    {
        public IOME2StockCalculator(CalculatorParameters calcParameters)
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
                == Input.INPUT_PRICE_TYPES.inputgroup.ToString())
            {
                //the input group can be used to insert calculators into 
                //descendant inputs and run totals for each input
                bHasCalculations = BIME2Calculator.SetInputGroupME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                == Output.OUTPUT_PRICE_TYPES.outputgroup.ToString())
            {
                //the input group can be used to insert calculators into 
                //descendant inputs and run totals for each input
                bHasCalculations = BIME2Calculator.SetOutputGroupME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Input.INPUT_PRICE_TYPES.input.ToString()))
            {
                //bimachinerystockcalculator handles calculations
                bHasCalculations = BIME2Calculator.SetInputME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Output.OUTPUT_PRICE_TYPES.output.ToString()))
            {
                //bimachinerystockcalculator handles calculations
                bHasCalculations = BIME2Calculator.SetOutputME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Input.INPUT_PRICE_TYPES.inputseries.ToString()))
            {
                bHasCalculations = BIME2Calculator.SetInputSeriesME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Output.OUTPUT_PRICE_TYPES.outputseries.ToString()))
            {
                bHasCalculations = BIME2Calculator.SetOutputSeriesME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            return bHasCalculations;
        }
    }
}