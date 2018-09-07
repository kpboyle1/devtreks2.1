using System.Xml.Linq;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run health care stock calculations for operations and components.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class OCHCStockCalculator
    {
        public OCHCStockCalculator(CalculatorParameters calcParameters)
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
                == OperationComponent.OPERATION_PRICE_TYPES.operationgroup.ToString()
                || currentElement.Name.LocalName
                == OperationComponent.COMPONENT_PRICE_TYPES.componentgroup.ToString())
            {
                //the operation group can be used to insert calculators into 
                //descendant operations and run totals for each operation
                bHasCalculations = BIHC1Calculator.SetTotalHCStockCalculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(OperationComponent.OPERATION_PRICE_TYPES.operation.ToString())
                || currentElement.Name.LocalName
                .EndsWith(OperationComponent.COMPONENT_PRICE_TYPES.component.ToString()))
            {
                //bimachinerystockcalculator handles calculations
                bHasCalculations = BIHC1Calculator.SetOpOrCompHCStockCalculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Input.INPUT_PRICE_TYPES.input.ToString()))
            {
                //resource stock calcs come from calculator results
                if (currentCalculationsElement != null)
                {
                    bHasCalculations = BIHC1Calculator.SetInputHCStockCalculations(
                        currentCalculationsElement, currentElement);
                }
            }
            return bHasCalculations;
        }
    }
}
