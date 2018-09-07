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
    public class FoodNutrition : IDoStepsHostMetaData
    {
        public FoodNutrition()
        {
            this.CalculatorsExtensionName = "FoodNutrition";
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
            CalculatorParameters FNCalcParams
                = CalculatorHelpers.SetCalculatorParameters(
                extDocToCalcURI, extCalcDocURI, stepNumber, urisToAnalyze,
                updates);
            FNCalculatorHelper fnBudgetHelper
                = new FNCalculatorHelper(FNCalcParams);
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
                    if (FNCalcParams != null)
                    {
                        //set constants for this step
                        bHasCalculation
                            = await FNCalculatorHelper.SetConstants(FNCalcParams);
                    }
                    extDocToCalcURI.URIDataManager.NeedsFullView = false;
                    extDocToCalcURI.URIDataManager.NeedsSummaryView = false;
                    break;
                case ContractHelpers.EXTENSION_STEPS.stepthree:
                    //get rid of any update member that was added after running the same step 2x
                    bHasUpdates = await CalculatorHelpers.RefreshUpdates(FNCalcParams, stepNumber, updates);
                    if (FNCalcParams != null)
                    {
                        if (FNCalcParams.ExtensionDocToCalcURI.URIDataManager.SubAppType
                            != Constants.SUBAPPLICATION_TYPES.devpacks.ToString())
                        {
                            //run the calculations 
                            bHasCalculation = await fnBudgetHelper.RunInputCalculations();
                        }
                        else
                        {
                            //run custom document calculations
                            bHasCalculation
                                = await fnBudgetHelper.RunDevPacksCalculations(FNCalcParams);
                        }
                        extDocToCalcURI.ErrorMessage = FNCalcParams.ErrorMessage;
                        extDocToCalcURI.ErrorMessage += FNCalcParams.ExtensionDocToCalcURI.ErrorMessage;
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
                            CheckForLastStepCalculator(FNCalcParams,
                                eStepNumber, extDocToCalcURI);
                            //replace the old calculator with the new one
                            //and save the new calculations document
                            bHasCalculation
                                = await CalculatorHelpers.SaveNewCalculationsDocument(FNCalcParams);
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
                    extCalcDocURI.URIFileExtensionType = CalculatorHelpers.GetAttribute(FNCalcParams.LinkedViewElement,
                            Calculator1.cFileExtensionType);
                    bHasCalculation = true;
                    //tells addinhelper to save calcs
                    CalculatorHelpers.SetTempDocSaveCalcsProperty(extDocToCalcURI);
                    break;
                default:
                    //as many steps as needed can be added to this addin
                    break;
            }
            extDocToCalcURI.ErrorMessage += FNCalcParams.ErrorMessage;
            return bHasCalculation;
        }
        private static void CheckForLastStepCalculator(
            CalculatorParameters fnCalcParams,
            ContractHelpers.EXTENSION_STEPS stepNumber,
            ExtensionContentURI extDocToCalcURI)
        {
            FNCalculatorHelper.CALCULATOR_TYPES eCalculatorType
                = FNCalculatorHelper.GetCalculatorType(
                fnCalcParams.CalculatorType);
            //other projects have code for handling different
            //numbers of steps in calculators
        }
        public async Task<bool> RunAnalyzerStep(
            ExtensionContentURI extDocToCalcURI, ExtensionContentURI extCalcDocURI,
            string stepNumber, IList<string> urisToAnalyze,
            IDictionary<string, string> updates, CancellationToken cancellationToken)
        {
            bool bHasAnalysis = false;
            CalculatorParameters fnCalcParams
                = CalculatorHelpers.SetCalculatorParameters(
                extDocToCalcURI, extCalcDocURI, stepNumber, urisToAnalyze,
                updates);
            FNAnalyzerHelper fnAnalyzerHelper = new FNAnalyzerHelper(fnCalcParams);
            //check to make sure the analyzer can be run
            fnAnalyzerHelper.SetOptions();
            if (fnAnalyzerHelper.FNCalculatorParams.ErrorMessage != string.Empty)
            {
                extDocToCalcURI.ErrorMessage += fnAnalyzerHelper.FNCalculatorParams.ErrorMessage;
                return false;
            }
            ContractHelpers.EXTENSION_STEPS eStepNumber
                = ContractHelpers.GetEnumStepNumber(stepNumber);
            bool bHasUpdates = false;
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
                    bHasUpdates = await CalculatorHelpers.RefreshUpdates(fnCalcParams, stepNumber, updates);
                    if (fnAnalyzerHelper.AnalyzerType
                        == FNAnalyzerHelper.ANALYZER_TYPES.resources01)
                    {
                        //linked view insertions needs some analysis parameters
                        SetAnalyzerParameters(fnAnalyzerHelper, fnCalcParams);
                        //the calculator pattern handles children linked view insertions
                        //better than the analyzer pattern
                        FNCalculatorHelper fnCalculatorHelper
                            = new FNCalculatorHelper(fnCalcParams);
                        bHasAnalysis = await fnCalculatorHelper.RunCalculations();
                    }
                    else
                    {
                        //run the analysis (when analyses are available)
                        //bHasAnalysis = fnAnalyzerHelper
                        //    .RunAnalysis(urisToAnalyze);
                    }
                    extDocToCalcURI.ErrorMessage = fnCalcParams.ErrorMessage;
                    extDocToCalcURI.ErrorMessage += fnAnalyzerHelper.FNCalculatorParams.ErrorMessage;
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
                        FNAnalyzerHelper.ANALYZER_TYPES eAnalyzerType
                            = fnAnalyzerHelper.GetAnalyzerType(
                            fnAnalyzerHelper.FNCalculatorParams.AnalyzerParms.AnalyzerType);
                        //when 3+ step analyzers start being used
                        CheckForLastStepAnalyzer(eAnalyzerType,
                            eStepNumber, extDocToCalcURI);
                        if (!CalculatorHelpers.IsSaveAction(extDocToCalcURI))
                        {
                            //replace the old calculator with the new one
                            //and save the new calculations document
                            bool bHasReplacedCalcDoc
                                = await CalculatorHelpers.SaveNewCalculationsDocument(
                                fnAnalyzerHelper.FNCalculatorParams);
                            if (bHasReplacedCalcDoc)
                            {
                                //the resource01 app uses the calculator, rather
                                //than analyzer, pattern (i.e. children linked views 
                                //can be inserted)
                                if (fnAnalyzerHelper.AnalyzerType
                                    != FNAnalyzerHelper.ANALYZER_TYPES.resources01)
                                {
                                    //and add xmldoc params to update collection
                                    //only doctocalc gets updated
                                    CalculatorHelpers.AddXmlDocAndXmlDocIdsToUpdates(
                                        fnAnalyzerHelper.FNCalculatorParams);
                                }
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
                    if (fnAnalyzerHelper.AnalyzerType
                        == FNAnalyzerHelper.ANALYZER_TYPES.resources01)
                    {
                        extDocToCalcURI.URIDataManager.NeedsFullView = false;
                        extDocToCalcURI.URIDataManager.NeedsSummaryView = true;
                        extCalcDocURI.URIFileExtensionType = CalculatorHelpers.GetAttribute(fnCalcParams.LinkedViewElement,
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
            extDocToCalcURI.ErrorMessage += fnAnalyzerHelper.FNCalculatorParams.ErrorMessage;
            return bHasAnalysis;
        }
        private static void CheckForLastStepAnalyzer(
            FNAnalyzerHelper.ANALYZER_TYPES analyzerType,
            ContractHelpers.EXTENSION_STEPS stepNumber,
            ExtensionContentURI extDocToCalcURI)
        {
            //this release only includes three step analyzers
        }
        private static void SetAnalyzerParameters(
            FNAnalyzerHelper fnAnalyzerHelper, CalculatorParameters fnCalcParams)
        {
            //only resources01 analyzers are available yet
            fnAnalyzerHelper.SetAnalysisParameters();
            fnCalcParams.FileExtensionType
                = fnAnalyzerHelper.FNCalculatorParams.FileExtensionType;
            fnCalcParams.Stylesheet2Name
                = fnAnalyzerHelper.FNCalculatorParams.Stylesheet2Name;
            fnCalcParams.Stylesheet2ObjectNS
                = fnAnalyzerHelper.FNCalculatorParams.Stylesheet2ObjectNS;
        }
    }
}
