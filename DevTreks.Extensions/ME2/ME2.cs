using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Errors = DevTreks.Exceptions.DevTreksErrors;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		Run both calculators and analyzers for monitoring and evaluations data
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///Notes:       2.1.0 refactored to simpler mef pattern
    ///         
    /// </summary>

    [Export(typeof(IDoStepsHostMetaData))]
    public class ME2 : IDoStepsHostMetaData
    {
        public ME2()
        {
            this.CalculatorsExtensionName = "ME2";
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
            CalculatorParameters ME2CalcParams
                = CalculatorHelpers.SetCalculatorParameters(
                extDocToCalcURI, extCalcDocURI, stepNumber, urisToAnalyze,
                updates);
            ME2CalculatorHelper me2CalcHelper
                = new ME2CalculatorHelper(ME2CalcParams);
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
                    //clear updates collection
                    updates.Clear();
                    //just save this step's edits
                    bHasCalculation = true;
                    extDocToCalcURI.URIDataManager.NeedsFullView = false;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepthree:
                    //get rid of any update member that was added after running the same step 2x
                    bHasUpdates = await CalculatorHelpers.RefreshUpdates(ME2CalcParams, stepNumber, updates);
                    if (ME2CalcParams != null)
                    {
                        bHasCalculation = await me2CalcHelper.RunME2CalculatorCalculations();
                        extDocToCalcURI.ErrorMessage = ME2CalcParams.ErrorMessage;
                        extDocToCalcURI.ErrorMessage += ME2CalcParams.ExtensionDocToCalcURI.ErrorMessage;
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
                            CheckForLastStepCalculator(ME2CalcParams,
                                eStepNumber, extDocToCalcURI);
                            //replace the old calculator with the new one
                            //and save the new calculations document
                            bHasCalculation
                                = await CalculatorHelpers.SaveNewCalculationsDocument(ME2CalcParams);
                        }
                        else
                        {
                            bHasCalculation = false;
                        }
                    }
                    extDocToCalcURI.URIDataManager.NeedsFullView = true;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepfour:
                    //show all indicators (unlike most calculators)
                    extDocToCalcURI.URIDataManager.NeedsFullView = true;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    extCalcDocURI.URIFileExtensionType = CalculatorHelpers.GetAttribute(ME2CalcParams.LinkedViewElement,
                            Calculator1.cFileExtensionType);
                    bHasCalculation = true;
                    //tells addinhelper to save calcs
                    CalculatorHelpers.SetTempDocSaveCalcsProperty(extDocToCalcURI);
                    break;
                default:
                    //as many steps as needed can be added to this addin
                    break;
            }
            extDocToCalcURI.ErrorMessage += ME2CalcParams.ErrorMessage;
            return bHasCalculation;
        }
        private static void CheckForLastStepCalculator(
            CalculatorParameters me2CalcParams,
            ContractHelpers.EXTENSION_STEPS stepNumber,
            ExtensionContentURI extDocToCalcURI)
        {
            ME2CalculatorHelper.CALCULATOR_TYPES eCalculatorType
                = ME2CalculatorHelper.GetCalculatorType(
                me2CalcParams.CalculatorType);
            //other projects have code for handling different
            //numbers of steps in calculators
        }
        public async Task<bool> RunAnalyzerStep(
            ExtensionContentURI extDocToCalcURI, ExtensionContentURI extCalcDocURI,
            string stepNumber, IList<string> urisToAnalyze,
            IDictionary<string, string> updates, CancellationToken cancellationToken)
        {
            bool bHasAnalysis = false;
            CalculatorParameters me2CalcParams
                = CalculatorHelpers.SetCalculatorParameters(
                extDocToCalcURI, extCalcDocURI, stepNumber, urisToAnalyze,
                updates);
            ME2AnalyzerHelper me2AnalyzerHelper = new ME2AnalyzerHelper(me2CalcParams);
            //check to make sure the analyzer can be run
            me2AnalyzerHelper.SetOptions();
            if (me2AnalyzerHelper.ME2CalculatorParams.ErrorMessage != string.Empty)
            {
                extDocToCalcURI.ErrorMessage += me2AnalyzerHelper.ME2CalculatorParams.ErrorMessage;
                return false;
            }
            bool bHasUpdates = false;
            ContractHelpers.EXTENSION_STEPS eStepNumber
                = ContractHelpers.GetEnumStepNumber(stepNumber);
            switch (eStepNumber)
            {
                case ContractHelpers.EXTENSION_STEPS.stepzero:
                    bHasAnalysis = true;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepone:
                    bHasAnalysis = true;
                    break;
                case ContractHelpers.EXTENSION_STEPS.steptwo:
                    //clear updates collection
                    updates.Clear();
                    //just need the html form edits in this step
                    bHasAnalysis = true;
                    extDocToCalcURI.URIDataManager.NeedsFullView = false;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepthree:
                    //get rid of any update member that was added after running the same step 2x
                    bHasUpdates = await CalculatorHelpers.RefreshUpdates(me2CalcParams, stepNumber, updates);
                    //linked view insertions needs some analysis parameters
                    SetAnalyzerParameters(me2AnalyzerHelper, me2CalcParams);
                    if (me2CalcParams.SubApplicationType == Constants.SUBAPPLICATION_TYPES.devpacks)
                    {
                        bHasAnalysis = await me2AnalyzerHelper.RunAnalysis(me2CalcParams.UrisToAnalyze);
                    }
                    else
                    {
                        bHasAnalysis = await me2AnalyzerHelper.RunAnalysis();
                    }
                    extDocToCalcURI.ErrorMessage = me2CalcParams.ErrorMessage;
                    extDocToCalcURI.ErrorMessage += me2AnalyzerHelper.ME2CalculatorParams.ErrorMessage;
                    if (!bHasAnalysis)
                    {
                        extDocToCalcURI.ErrorMessage = (extDocToCalcURI.ErrorMessage == string.Empty) ?
                            Errors.MakeStandardErrorMsg("CALCULATORS_URI_MISMATCH")
                            : extDocToCalcURI.ErrorMessage;
                        return bHasAnalysis;
                    }
                    if (string.IsNullOrEmpty(extDocToCalcURI.ErrorMessage))
                    {
                        //two step analyzers need to be saved now
                        ME2AnalyzerHelper.ANALYZER_TYPES eAnalyzerType
                            = me2AnalyzerHelper.GetAnalyzerType(
                            me2AnalyzerHelper.ME2CalculatorParams.AnalyzerParms.AnalyzerType);
                        //when 3+ step analyzers start being used
                        CheckForLastStepAnalyzer(eAnalyzerType,
                            eStepNumber, extDocToCalcURI);
                        if (!CalculatorHelpers.IsSaveAction(extDocToCalcURI))
                        {
                            //replace the old calculator with the new one
                            //and save the new calculations document
                            bool bHasReplacedCalcDoc
                                = await CalculatorHelpers.SaveNewCalculationsDocument(
                                me2AnalyzerHelper.ME2CalculatorParams);
                            if (bHasReplacedCalcDoc)
                            {
                                bHasAnalysis = true;
                            }
                            else
                            {
                                extDocToCalcURI.ErrorMessage = Errors.MakeStandardErrorMsg("ANALYSES_ID_MISMATCH");
                            }
                        }
                    }
                    else
                    {
                        bHasAnalysis = false;
                    }
                    extDocToCalcURI.URIDataManager.NeedsFullView = true;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepfour:
                    bHasAnalysis = true;
                    if (me2AnalyzerHelper.AnalyzerType
                         == ME2AnalyzerHelper.ANALYZER_TYPES.metotal1
                         || me2AnalyzerHelper.AnalyzerType
                         == ME2AnalyzerHelper.ANALYZER_TYPES.mestat1
                         || me2AnalyzerHelper.AnalyzerType
                         == ME2AnalyzerHelper.ANALYZER_TYPES.mechangeyr
                         || me2AnalyzerHelper.AnalyzerType
                         == ME2AnalyzerHelper.ANALYZER_TYPES.mechangeid
                         || me2AnalyzerHelper.AnalyzerType
                         == ME2AnalyzerHelper.ANALYZER_TYPES.mechangealt
                         || me2AnalyzerHelper.AnalyzerType
                         == ME2AnalyzerHelper.ANALYZER_TYPES.meprogress1)
                    {
                        if (me2CalcParams.NeedsFullView)
                        {
                            extDocToCalcURI.URIDataManager.NeedsFullView = true;
                            extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                        }
                        else
                        {
                            extDocToCalcURI.URIDataManager.NeedsFullView = false;
                            extDocToCalcURI.URIDataManager.NeedsSummaryView = true;
                        }
                        extCalcDocURI.URIFileExtensionType = CalculatorHelpers.GetAttribute(me2CalcParams.LinkedViewElement,
                            Calculator1.cFileExtensionType);
                    }
                    if (extDocToCalcURI.URIDataManager.TempDocSaveMethod
                        == Constants.SAVECALCS_METHOD.saveastext.ToString())
                    {
                        //analyzers aren't yet available
                    }
                    else
                    {
                        //tells addinhelper to save calcs
                        CalculatorHelpers.SetTempDocSaveAnalysesProperty(extDocToCalcURI);
                    }
                    break;
                default:
                    //as many steps as needed can be added to this addin
                    break;
            }
            extDocToCalcURI.ErrorMessage += me2AnalyzerHelper.ME2CalculatorParams.ErrorMessage;
            return bHasAnalysis;
        }
        private static void CheckForLastStepAnalyzer(
            ME2AnalyzerHelper.ANALYZER_TYPES analyzerType,
            ContractHelpers.EXTENSION_STEPS stepNumber,
            ExtensionContentURI extDocToCalcURI)
        {
            //this release only includes three step analyzers
        }
        private static void SetAnalyzerParameters(
            ME2AnalyzerHelper me2AnalyzerHelper, CalculatorParameters me2CalcParams)
        {
            me2AnalyzerHelper.SetAnalysisParameters();
            me2CalcParams.FileExtensionType
                = me2AnalyzerHelper.ME2CalculatorParams.FileExtensionType;
            me2CalcParams.Stylesheet2Name
                = me2AnalyzerHelper.ME2CalculatorParams.Stylesheet2Name;
            me2CalcParams.Stylesheet2ObjectNS
                = me2AnalyzerHelper.ME2CalculatorParams.Stylesheet2ObjectNS;
        }
    }
}