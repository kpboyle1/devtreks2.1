using System.Xml.Linq;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Run monitoring and evaluation indicator stock calculations for operations 
    ///             and components.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class OCME2StockCalculator
    {
        public OCME2StockCalculator(CalculatorParameters calcParameters)
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
                == OperationComponent.OPERATION_PRICE_TYPES.operationgroup.ToString()
                || currentElement.Name.LocalName
                == OperationComponent.COMPONENT_PRICE_TYPES.componentgroup.ToString())
            {
                //the object model uses a BudgetGroup but all that's needed is id and name 
                bHasCalculations = BIME2Calculator.SetOCGroupME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(OperationComponent.OPERATION_PRICE_TYPES.operation.ToString())
                || currentElement.Name.LocalName
                .EndsWith(OperationComponent.COMPONENT_PRICE_TYPES.component.ToString()))
            {
                //bimachinerystockcalculator handles calculations
                bHasCalculations = BIME2Calculator.SetOpOrCompME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            else if (currentElement.Name.LocalName
                .EndsWith(Input.INPUT_PRICE_TYPES.input.ToString()))
            {
                //resource stock calcs come from calculator results
                bHasCalculations = BIME2Calculator.SetTechInputME2Calculations(
                    currentCalculationsElement, currentElement);
            }
            return bHasCalculations;
        }
       
    }
}
