﻿using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Composition;
using Errors = DevTreks.Exceptions.DevTreksErrors;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		economics calculators based on AAEA guidelines
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///Notes:       2.1.0 refactored to simpler mef pattern
    /// </summary> 
    [Export(typeof(IDoStepsHostMetaData))]
    public class NPVCalculators : IDoStepsHostMetaData
    {
        public NPVCalculators()
        {
            this.CalculatorsExtensionName = "NPVCalculators";
            this.CONTRACT_TYPE = CALULATOR_CONTRACT_TYPES.defaultcalculatormanager;
        }
        public string CalculatorsExtensionName { get; set; }
        public CALULATOR_CONTRACT_TYPES CONTRACT_TYPE { get; set; }
        public async Task<bool> RunCalculatorStep(
            ExtensionContentURI extDocToCalcURI, ExtensionContentURI extCalcDocURI,
            string stepNumber, IList<string> urisToAnalyze,
            IDictionary<string, string> updates, CancellationToken cancellationToken)
        {
            bool bHasCalculation = false;
            CalculatorHelpers eCalcHelpers = new CalculatorHelpers();
            CalculatorParameters npvCalcParams
                = CalculatorHelpers.SetCalculatorParameters(
                extDocToCalcURI, extCalcDocURI, stepNumber, urisToAnalyze,
                updates);
            if (!await CalculatorHelpers.URIAbsoluteExists(extDocToCalcURI,
                extDocToCalcURI.URIDataManager.TempDocPath))
            {
                //this calculator requires the base document to be built first
                extDocToCalcURI.ErrorMessage = Errors.MakeStandardErrorMsg("CALCULATORS_MISSINGBASEDOC");
                return false;
            }
            NPVCalculatorHelper npvBudgetHelpers = new NPVCalculatorHelper(npvCalcParams);
            ContractHelpers.EXTENSION_STEPS eStepNumber
                = ContractHelpers.GetEnumStepNumber(stepNumber);
            bool bHasUpdates = false;
            switch (eStepNumber)
            {
                case ContractHelpers.EXTENSION_STEPS.stepzero:
                    bHasCalculation = true;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepone:
                    bHasCalculation = true;
                    break;
                case ContractHelpers.EXTENSION_STEPS.steptwo:
                    updates.Clear();
                    if (npvCalcParams != null)
                    {
                        //set constants for this step
                        bHasCalculation = await NPVCalculatorHelper.SetConstants(npvCalcParams);
                        //this step does not generate 3rd htmldocs
                        extDocToCalcURI.URIDataManager.NeedsFullView = false;
                        extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    }
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepthree:
                    //get rid of any update member that was added after running the same step 2x
                    bHasUpdates = await CalculatorHelpers.RefreshUpdates(npvCalcParams, stepNumber, updates);
                    if (npvCalcParams != null)
                    {
                        if (npvCalcParams.CalculatorType == CalculatorHelpers.CALCULATOR_TYPES.operation2.ToString()
                            || npvCalcParams.CalculatorType == CalculatorHelpers.CALCULATOR_TYPES.component2.ToString())
                        {
                            //run the scheduling calculations 
                            bHasCalculation = await NPVCalculatorHelper.RunSchedulingCalculations(npvCalcParams);
                        }
                        else
                        {
                            //run the calculations 
                            bHasCalculation = await npvBudgetHelpers.RunCalculations();
                        }
                        extDocToCalcURI.ErrorMessage = npvCalcParams.ErrorMessage;
                        extDocToCalcURI.ErrorMessage += npvCalcParams.ExtensionDocToCalcURI.ErrorMessage;
                        if (!bHasCalculation)
                        {
                            extDocToCalcURI.ErrorMessage = (extDocToCalcURI.ErrorMessage == string.Empty) ?
                                Errors.MakeStandardErrorMsg("CALCULATORS_URI_MISMATCH")
                                : extDocToCalcURI.ErrorMessage;
                            return bHasCalculation;
                        }
                        if (string.IsNullOrEmpty(extDocToCalcURI.ErrorMessage))
                        {
                            //two step calculators need to be saved now
                            CheckForLastStepCalculator(npvCalcParams,
                                eStepNumber, extDocToCalcURI);
                            //replace the old calculator with the new one
                            //and save the new calculations document
                            bHasCalculation
                                = await CalculatorHelpers.SaveNewCalculationsDocument(npvCalcParams);
                        }
                        else
                        {
                            bHasCalculation = false;
                        }
                    }
                    //this step needs a full 3rd htmldocs
                    extDocToCalcURI.URIDataManager.NeedsFullView = true;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepfour:
                    if (npvCalcParams.ExtensionDocToCalcURI.URIDataManager.SubAppType
                        == Constants.SUBAPPLICATION_TYPES.componentprices.ToString()
                        || npvCalcParams.ExtensionDocToCalcURI.URIDataManager.SubAppType
                        == Constants.SUBAPPLICATION_TYPES.operationprices.ToString())
                    {
                        if (npvCalcParams.CalculatorType == CalculatorHelpers.CALCULATOR_TYPES.operation2.ToString()
                            || npvCalcParams.CalculatorType == CalculatorHelpers.CALCULATOR_TYPES.component2.ToString())
                        {
                            //get rid of any update member that was added after running the same step 2x
                            bHasUpdates = await CalculatorHelpers.RefreshUpdates(npvCalcParams, stepNumber, updates);
                            //run the scheduling calculations 
                            bHasCalculation = await npvBudgetHelpers.RunCalculations();
                            bHasCalculation = await CalculatorHelpers.SaveNewCalculationsDocument(npvCalcParams);
                            //this step needs a full 3rd htmldocs
                            extDocToCalcURI.URIDataManager.NeedsFullView = true;
                            extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                        }
                        else
                        {
                            //save the calculations
                            bHasCalculation = await SaveCalculations(extDocToCalcURI, extCalcDocURI,
                                npvCalcParams);
                        }
                    }
                    else
                    {
                        //save the calculations
                        bHasCalculation = await SaveCalculations(extDocToCalcURI, extCalcDocURI,
                            npvCalcParams);
                    }
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepfive:
                    if (npvCalcParams.ExtensionDocToCalcURI.URIDataManager.SubAppType
                        == Constants.SUBAPPLICATION_TYPES.componentprices.ToString()
                        || npvCalcParams.ExtensionDocToCalcURI.URIDataManager.SubAppType
                        == Constants.SUBAPPLICATION_TYPES.operationprices.ToString())
                    {
                        //save the calculations 
                        bHasCalculation = await SaveCalculations(extDocToCalcURI, extCalcDocURI,
                            npvCalcParams);
                    }
                    break;
                default:
                    //as many steps as needed can be added to this addin
                    break;
            }
            extDocToCalcURI.ErrorMessage += npvCalcParams.ErrorMessage;
            return bHasCalculation;
        }
        private static void CheckForLastStepCalculator(
            CalculatorParameters npvCalcParams,
            ContractHelpers.EXTENSION_STEPS stepNumber,
            ExtensionContentURI extDocToCalcURI)
        {
            CalculatorHelpers.CALCULATOR_TYPES eCalculatorType
                = CalculatorHelpers.GetCalculatorType(
                npvCalcParams.CalculatorType);
            //keep for code sample but Locals moved to Locals Extension
            if (eCalculatorType
                == CalculatorHelpers.CALCULATOR_TYPES.locals)
            {
                if (stepNumber == ContractHelpers.EXTENSION_STEPS.stepthree)
                {
                    //tells addinhelper to save calcs
                    CalculatorHelpers.SetTempDocSaveCalcsProperty(extDocToCalcURI);
                }
            }
        }
        private static async Task<bool> SaveCalculations(
            ExtensionContentURI extDocToCalcURI, ExtensionContentURI extCalcDocURI,
            CalculatorParameters npvCalcParams)
        {
            extDocToCalcURI.URIDataManager.NeedsFullView = false;
            extDocToCalcURI.URIDataManager.NeedsSummaryView = true;
            extCalcDocURI.URIFileExtensionType = CalculatorHelpers.GetAttribute(npvCalcParams.LinkedViewElement,
                    Calculator1.cFileExtensionType);
            bool bHasCalculation = true;
            if (extDocToCalcURI.URIDataManager.TempDocSaveMethod
                == Constants.SAVECALCS_METHOD.saveastext.ToString())
            {
                //text files can be filtered by subscribers
                KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(
                    extDocToCalcURI.URIPattern, extDocToCalcURI.URIDataManager.TempDocPath);
                npvCalcParams.AnalyzerParms.FileOrFolderPaths.Add(kvp);
                NPVCalcsTextSubscriber obsTextSubscriber
                    = new NPVCalcsTextSubscriber(npvCalcParams);
                bHasCalculation = await obsTextSubscriber.BuildObservations();
                if (!bHasCalculation
                    || (!string.IsNullOrEmpty(obsTextSubscriber
                    .ObsCalculatorParams.ErrorMessage)))
                {
                    extDocToCalcURI.ErrorMessage = obsTextSubscriber.ObsCalculatorParams.ErrorMessage;
                    CalculatorHelpers.SetTempDocSaveNoneProperty(extDocToCalcURI);
                }
                else
                {
                    //tells addinhelper to save calcs
                    CalculatorHelpers.SetTempDocSaveCalcsProperty(extDocToCalcURI);
                }
            }
            else
            {
                //tells addinhelper to save calcs
                CalculatorHelpers.SetTempDocSaveCalcsProperty(extDocToCalcURI);
            }
            return bHasCalculation;
        }
        //implement the interface, but this is not an analyzers addin
        public async Task<bool> RunAnalyzerStep(
            ExtensionContentURI extDocToCalcURI, ExtensionContentURI extCalcDocURI,
            string stepNumber, IList<string> urisToAnalyze,
            IDictionary<string, string> updates, CancellationToken cancellationToken)
        {
            return false;
        }
    }
}
