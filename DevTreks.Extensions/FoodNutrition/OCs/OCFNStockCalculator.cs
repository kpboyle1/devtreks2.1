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
    public class OCFNStockCalculator
    {
        public OCFNStockCalculator(CalculatorParameters calcParameters)
        {
            BIFN1Calculator = new BIFNStockCalculator(calcParameters);
        }
        //stateful food nutrition stock
        public BIFNStockCalculator BIFN1Calculator { get; set; }

        public bool AddCalculationsToCurrentElement(
            XElement currentCalculationsElement, XElement currentElement)
        {
            bool bHasCalculations = false;
            if (currentElement.Name.LocalName
                == OperationComponent.OPERATION_PRICE_TYPES.operationgroup.ToString()
                || currentElement.Name.LocalName
                == OperationComponent.COMPONENT_PRICE_TYPES.componentgroup.ToString())
            {
                //the operation group can be used to insert calculators into 
                //descendant operations and run totals for each operation
                bHasCalculations = BIFN1Calculator.SetTotalFNStockCalculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(OperationComponent.OPERATION_PRICE_TYPES.operation.ToString())
                || currentElement.Name.LocalName
                .EndsWith(OperationComponent.COMPONENT_PRICE_TYPES.component.ToString()))
            {
                //bifnsr01stockcalculator handles calculations
                bHasCalculations = BIFN1Calculator.SetOpOrCompFNStockCalculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Input.INPUT_PRICE_TYPES.input.ToString()))
            {
                //resource stock calcs come from calculator results
                if (currentCalculationsElement != null)
                {
                    bHasCalculations = BIFN1Calculator.SetInputFNStockCalculations(
                        currentCalculationsElement, currentElement);
                }
            }
            return bHasCalculations;
        }
    }
}
