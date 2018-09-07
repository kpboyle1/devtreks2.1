﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Threading.Tasks;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		This class derives from the CalculatorContracts.GeneralCalculator 
    ///             class. It subscribes to the RunCalculation events raised by that class. 
    ///             It runs calculations on the nodes returned by that class, 
    ///             and returns a calculated xelement to the publisher for saving.
    ///Author:		www.devtreks.org
    ///Date:		2016, May
    ///NOTES        1. This class has not been debugged. It's the same pattern as the 
    ///             devpacks in the NPVCalculators extension which has been debugged.
    /// </summary>
    public class DevPacksSB1Subscriber : GeneralCalculator
    {
        //constructors
        public DevPacksSB1Subscriber() { }
        //constructor sets class (base) properties
        public DevPacksSB1Subscriber(CalculatorParameters calcParameters)
            : base(calcParameters)
        {
        }

        //methods
        //subscribe to the events raised by the base class
        public async Task<bool> RunDevPackCalculator()
        {
            bool bHasAnalysis = false;
            //subscribe to the event raised by the publisher delegate
            this.RunDevPackCalculation += AddDevPackCalculations;
            //run the calculation (raising the events
            //from the base event publisher for each node)
            bHasAnalysis = await this.RunDevPackCalculations();
            return bHasAnalysis;
        }
        //define the actions to take when the event is raised
        public void AddDevPackCalculations(object sender, CustomEventArgs e)
        {
            bool bHasCalculations = false;
            //e.CalculatorParams = current devpack or devpackpart being processed
            //the file paths identify the document to run calcs on and the 
            //uripattern identifies the node holding the linkedviews 
            //note that e.CalcParams.ExtensionDoctoCalcURI.Resources[0] holds
            //parent contenturi used to start the extension 
            //(with original doctocalc file that is used when the linkedview is needed)
            SB1CalculatorHelper fnCalculatorHelper
                = new SB1CalculatorHelper(e.CalculatorParams);
            //run the calculations 
            //bHasCalculations = fnCalculatorHelper
            //    .RunCalculations();
            //pass the bool back to the publisher
            //by setting the HasCalculations property of CustomEventArgs
            e.HasCalculations = bHasCalculations;
        }
    }
}
