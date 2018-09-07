using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Errors = DevTreks.Exceptions.DevTreksErrors;

namespace DevTreks.Extensions
{
    ///<summary>
    ///Purpose:		Run both calculators and analyzers for food nutrition data
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///Notes:       2.1.0 refactored to simpler mef pattern
    /// </summary>

    [Export(typeof(IDoStepsHostMetaData))]
    public class MN1 : IDoStepsHostMetaData
    {
        public MN1()
        {
            this.CalculatorsExtensionName = "MN1";
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
            CalculatorParameters MN1CalcParams
                = CalculatorHelpers.SetCalculatorParameters(
                extDocToCalcURI, extCalcDocURI, stepNumber, urisToAnalyze,
                updates);
            MN1CalculatorHelper mn1BudgetHelper
                = new MN1CalculatorHelper(MN1CalcParams);
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
                    if (MN1CalcParams != null)
                    {
                        //set constants for this step
                        bHasCalculation
                            = await MN1CalculatorHelper.SetConstants(MN1CalcParams);
                    }
                    extDocToCalcURI.URIDataManager.NeedsFullView = false;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepthree:
                    //get rid of any update member that was added after running the same step 2x
                    bHasUpdates = await CalculatorHelpers.RefreshUpdates(MN1CalcParams, stepNumber, updates);
                    if (MN1CalcParams != null)
                    {
                        //run the calculations 
                        bHasCalculation = await mn1BudgetHelper.RunMN1CalculatorCalculations();

                        extDocToCalcURI.ErrorMessage = MN1CalcParams.ErrorMessage;
                        extDocToCalcURI.ErrorMessage += MN1CalcParams.ExtensionDocToCalcURI.ErrorMessage;
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
                            CheckForLastStepCalculator(MN1CalcParams,
                                eStepNumber, extDocToCalcURI);
                            //replace the old calculator with the new one
                            //and save the new calculations document
                            bHasCalculation
                                = await CalculatorHelpers.SaveNewCalculationsDocument(MN1CalcParams);
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
                    extDocToCalcURI.URIDataManager.NeedsFullView = false;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = true;
                    extCalcDocURI.URIFileExtensionType = CalculatorHelpers.GetAttribute(MN1CalcParams.LinkedViewElement,
                            Calculator1.cFileExtensionType);
                    bHasCalculation = true;
                    //tells addinhelper to save calcs
                    CalculatorHelpers.SetTempDocSaveCalcsProperty(extDocToCalcURI);
                    break;
                default:
                    //as many steps as needed can be added to this addin
                    break;
            }
            extDocToCalcURI.ErrorMessage += MN1CalcParams.ErrorMessage;
            return bHasCalculation;
        }
        private static void CheckForLastStepCalculator(
            CalculatorParameters mn1CalcParams,
            ContractHelpers.EXTENSION_STEPS stepNumber,
            ExtensionContentURI extDocToCalcURI)
        {
            MN1CalculatorHelper.CALCULATOR_TYPES eCalculatorType
                = MN1CalculatorHelper.GetCalculatorType(
                mn1CalcParams.CalculatorType);
            //other projects have code for handling different
            //numbers of steps in calculators
        }
        public async Task<bool> RunAnalyzerStep(
            ExtensionContentURI extDocToCalcURI, ExtensionContentURI extCalcDocURI,
            string stepNumber, IList<string> urisToAnalyze,
            IDictionary<string, string> updates, CancellationToken cancellationToken)
        {
            bool bHasAnalysis = false;
            CalculatorParameters mn1CalcParams
                = CalculatorHelpers.SetCalculatorParameters(
                extDocToCalcURI, extCalcDocURI, stepNumber, urisToAnalyze,
                updates);
            MN1AnalyzerHelper mn1AnalyzerHelper = new MN1AnalyzerHelper(mn1CalcParams);
            //check to make sure the analyzer can be run
            mn1AnalyzerHelper.SetOptions();
            bool bHasUpdates = false;
            if (mn1AnalyzerHelper.MN1CalculatorParams.ErrorMessage != string.Empty)
            {
                extDocToCalcURI.ErrorMessage += mn1AnalyzerHelper.MN1CalculatorParams.ErrorMessage;
                return false;
            }
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
                    bHasUpdates = await CalculatorHelpers.RefreshUpdates(mn1CalcParams, stepNumber, updates);
                    //linked view insertions needs some analysis parameters
                    SetAnalyzerParameters(mn1AnalyzerHelper, mn1CalcParams);
                    if (mn1CalcParams.SubApplicationType == Constants.SUBAPPLICATION_TYPES.devpacks)
                    {
                        bHasAnalysis = await mn1AnalyzerHelper.RunAnalysis(mn1CalcParams.UrisToAnalyze);
                    }
                    else
                    {
                        bHasAnalysis = await mn1AnalyzerHelper.RunAnalysis();
                    }
                    extDocToCalcURI.ErrorMessage = mn1CalcParams.ErrorMessage;
                    extDocToCalcURI.ErrorMessage += mn1AnalyzerHelper.MN1CalculatorParams.ErrorMessage;
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
                        MN1AnalyzerHelper.ANALYZER_TYPES eAnalyzerType
                            = mn1AnalyzerHelper.GetAnalyzerType(
                            mn1AnalyzerHelper.MN1CalculatorParams.AnalyzerParms.AnalyzerType);
                        //when 3+ step analyzers start being used
                        CheckForLastStepAnalyzer(eAnalyzerType,
                            eStepNumber, extDocToCalcURI);
                        if (!CalculatorHelpers.IsSaveAction(extDocToCalcURI))
                        {
                            //replace the old calculator with the new one
                            //and save the new calculations document
                            bool bHasReplacedCalcDoc
                                = await CalculatorHelpers.SaveNewCalculationsDocument(
                                mn1AnalyzerHelper.MN1CalculatorParams);
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
                    if (mn1AnalyzerHelper.AnalyzerType
                        == MN1AnalyzerHelper.ANALYZER_TYPES.mntotal1
                        || mn1AnalyzerHelper.AnalyzerType
                        == MN1AnalyzerHelper.ANALYZER_TYPES.mnstat1
                        || mn1AnalyzerHelper.AnalyzerType
                        == MN1AnalyzerHelper.ANALYZER_TYPES.mnchangeyr
                        || mn1AnalyzerHelper.AnalyzerType
                        == MN1AnalyzerHelper.ANALYZER_TYPES.mnchangeid
                        || mn1AnalyzerHelper.AnalyzerType
                        == MN1AnalyzerHelper.ANALYZER_TYPES.mnchangealt
                        || mn1AnalyzerHelper.AnalyzerType
                        == MN1AnalyzerHelper.ANALYZER_TYPES.mnprogress1)
                    {
                        if (mn1CalcParams.NeedsFullView)
                        {
                            extDocToCalcURI.URIDataManager.NeedsFullView = true;
                            extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                        }
                        else
                        {
                            extDocToCalcURI.URIDataManager.NeedsFullView = false;
                            extDocToCalcURI.URIDataManager.NeedsSummaryView = true;
                        }
                        extCalcDocURI.URIFileExtensionType = CalculatorHelpers.GetAttribute(mn1CalcParams.LinkedViewElement,
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
            extDocToCalcURI.ErrorMessage += mn1AnalyzerHelper.MN1CalculatorParams.ErrorMessage;
            return bHasAnalysis;
        }
        private static void CheckForLastStepAnalyzer(
            MN1AnalyzerHelper.ANALYZER_TYPES analyzerType,
            ContractHelpers.EXTENSION_STEPS stepNumber,
            ExtensionContentURI extDocToCalcURI)
        {
            //this release only includes three step analyzers
        }
        private static void SetAnalyzerParameters(
            MN1AnalyzerHelper mn1AnalyzerHelper, CalculatorParameters mn1CalcParams)
        {
            mn1AnalyzerHelper.SetAnalysisParameters();
            mn1CalcParams.FileExtensionType
                = mn1AnalyzerHelper.MN1CalculatorParams.FileExtensionType;
            mn1CalcParams.Stylesheet2Name
                = mn1AnalyzerHelper.MN1CalculatorParams.Stylesheet2Name;
            mn1CalcParams.Stylesheet2ObjectNS
                = mn1AnalyzerHelper.MN1CalculatorParams.Stylesheet2ObjectNS;
        }
    }
}