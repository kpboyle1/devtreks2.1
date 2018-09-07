using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Composition;
using DevTreks.Data;

namespace DevTreks.Extensions
{
    /// <summary>
    ///Purpose:		Host's view of imported calculators, 
    ///             analyzers, and storytellers. The 
    ///             host defines its own views so that extenders
    ///             have more flexibility i.e. they may choose to 
    ///             develop their own Calculator Contracts, Analyzer Contracts ...
    ///             rather than use DevTreks defaults.
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///References:	none
    ///             1. 2.1.0: Retain until examples demonstrate whether or not this is better
    /// </summary>
    //    [Export(typeof(IDoStepsHostMetaData))]
    //    public abstract class DoStepsHostView : IDoStepsHostMetaData
    //    {
    //        //run calculator extensions
    //        public abstract Task<bool> RunCalculatorStep(
    //           ExtensionContentURI docToCalcURI, ExtensionContentURI calcDocURI,
    //           string stepNumber, IList<string> urisToAnalyze,
    //           IDictionary<string, string> updates, CancellationToken cancellationToken);
    //        //run analyzer extensions
    //        public abstract Task<bool> RunAnalyzerStep(
    //            ExtensionContentURI docToCalcURI, ExtensionContentURI calcDocURI,
    //            string stepNumber, IList<string> urisToAnalyze,
    //            IDictionary<string, string> updates, CancellationToken cancellationToken);
    //    }
}
