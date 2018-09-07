using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevTreks.Extensions
{
    //enum for exportmetadata value
    public enum CALULATOR_CONTRACT_TYPES
    {
        none = 0,
        defaultcalculatormanager = 1
    }
    /// <summary>
    ///Purpose:		Host's metadata from imported calculators, 
    ///             analyzers, and storytellers. 
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///References:	
    ///NOTES
    ///             1. Version 2.1.0 refactored to simpler mef pattern
    /// </summary>

    public interface IDoStepsHostMetaData
    {
        CALULATOR_CONTRACT_TYPES CONTRACT_TYPE { get; set; }
        string CalculatorsExtensionName { get; }
        //run calculator extensions
        Task<bool> RunCalculatorStep(
           ExtensionContentURI docToCalcURI, ExtensionContentURI calcDocURI,
           string stepNumber, IList<string> urisToAnalyze,
           IDictionary<string, string> updates, CancellationToken cancellationToken);
        //run analyzer extensions
        Task<bool> RunAnalyzerStep(
            ExtensionContentURI docToCalcURI, ExtensionContentURI calcDocURI,
            string stepNumber, IList<string> urisToAnalyze,
            IDictionary<string, string> updates, CancellationToken cancellationToken);
    }
}
