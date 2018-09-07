using DevTreks.Data;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using DataAppHelpers = DevTreks.Data.AppHelpers;
using DataHelpers = DevTreks.Data.Helpers;

namespace DevTreks.Helpers
{
    /// <summary>
    ///Purpose:		assist displaying DevTreks xml and xhtml views
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///References:	
    /// </summary>
    public class DisplayURIHelper
    {
        public DisplayURIHelper() { }

        public async Task<bool> SaveHTMLForAddIn(ContentURI uri,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bHasSaved = false;
            //this is run after calculator seconddocs have been saved -it will save html views
            //for children linked views that don't have any html (they were run from parent)
            //if the html was only saved in tempdocpath, see if it also needs to be copied
            //to regular doc paths
            string sDocToReadPath
                = await DataHelpers.AddInHelper.GetDevTrekPath(uri, displayDocType);
            string sTempDocString = string.Concat(uri.URIDataManager.TempDocsURIName,
                DataHelpers.FileStorageIO.GetDelimiterForFileStorage(sDocToReadPath));
            if (sDocToReadPath.Contains(sTempDocString))
            {
                //this only needs to be run once -at step zero
                bool bIsOkToRunAddIn = DataHelpers.AddInHelper.IsOkToRunExtension(
                       uri);
                if (!bIsOkToRunAddIn)
                {
                    ContentURI addInURI = DataHelpers.LinqHelpers.GetLinkedViewIsSelectedAddIn(uri);
                    if (addInURI != null)
                    {
                        bHasSaved = await CopyHtmlDocsFromTempDocPaths(addInURI, displayDocType, sDocToReadPath);
                    }
                }
            }
            bHasSaved = true;
            return bHasSaved;
        }
        
        //called from ContentView.RunServerSubAction to generate the html files
        //that will be displayed in web pages using DisplayURI()
        public async Task<bool> DisplayURIAsync(ContentURI uri,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bIsCompleted = false;
            using (StringWriter writer = new StringWriter())
            {
                bIsCompleted = await DisplayURIAsync(writer, uri, displayDocType);
            }
            return bIsCompleted;
        }
        public async Task<bool> DisplayURIAsync(StringWriter writer, ContentURI uri,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bIsCompleted = false;
            if (displayDocType == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.thirddoc)
            {
                if (uri.URIDataManager.NeedsFullView == false
                    && uri.URIDataManager.NeedsSummaryView == false)
                {
                    uri.ErrorMessage += AppHelper.GetErrorMessage("DISPLAYHELPER_DONTNEEDHTMLDOC");
                    return bIsCompleted;
                }
            }
            //don't let existing errors interfere with html generation
            string sErrorMsg = uri.ErrorMessage;
            uri.ErrorMessage = string.Empty;
            //xhtml state is saved to increase performance and improve packaging
            string sDocToReadPath
                = await DataHelpers.AddInHelper.GetDevTrekPath(uri, displayDocType);
            if (await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, sDocToReadPath))
            {
                //first uri
                string sHtmlFragDocPath = string.Empty;
                string sHtmlDocPath = string.Empty;
                //ajax state management needs html fragments (for partial page updates)
                //the html fragment can be reused in full html doc
                bool bHasHtmlDoc = await SaveFullXhtmlDocFromXmlDoc2Async(uri,
                    sDocToReadPath, sHtmlFragDocPath,
                    DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag,
                    displayDocType);
                sHtmlFragDocPath = uri.URIDataManager.MiscDocPath;
                if (bHasHtmlDoc)
                {
                    //init with frag file
                    sHtmlDocPath = sHtmlFragDocPath;
                    //restful state management needs full xhtml documents (for ebook packages)
                    await SaveFullXhtmlDocFromXmlDoc2Async(uri,
                        sDocToReadPath, sHtmlDocPath,
                        DataAppHelpers.Resources.FILEEXTENSION_TYPES.html,
                        displayDocType);
                    sHtmlDocPath = uri.URIDataManager.MiscDocPath;
                    //write the fragment html doc to the writer 
                    DataHelpers.FileStorageIO oFileStorageIO = new DataHelpers.FileStorageIO();
                    await oFileStorageIO.SaveHtmlURIToWriterAsync(uri, writer, sHtmlFragDocPath);
                    if (string.IsNullOrEmpty(uri.ErrorMessage))
                    {
                        //check whether additional html files are needed 
                        //(i.e. full, rather than summary, docs)
                        await SaveFullXhtmlDocFromFullXmlDoc1Async(writer, uri,
                            sDocToReadPath, displayDocType);
                    }
                }
                else
                {
                    if (displayDocType == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.seconddoc)
                    {
                        //if it doesn't have an addin it's not an error
                        bool bHasAddIn
                            = DataHelpers.LinqHelpers.LinkedViewHaveAddIn(uri.URIDataManager.LinkedView);
                        if (bHasAddIn)
                        {
                            string sComplexMsg = uri.ErrorMessage;
                            uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NOHTMLFILE");
                            uri.ErrorMessage += sComplexMsg;
                        }
                    }
                    else
                    {
                        string sComplexMsg = uri.ErrorMessage;
                        uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NOHTMLFILE");
                        uri.ErrorMessage += sComplexMsg;
                    }
                }
            }
            else
            {
                if (displayDocType == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.seconddoc)
                {
                    if (uri.URIDataManager.UseSelectedLinkedView)
                    {
                        if ((uri.URIDataManager.AppType == DataHelpers.GeneralHelpers.APPLICATION_TYPES.linkedviews
                            || uri.URIDataManager.AppType == DataHelpers.GeneralHelpers.APPLICATION_TYPES.devpacks)
                            && uri.URIDataManager.ServerSubActionType == DataHelpers.GeneralHelpers.SERVER_SUBACTION_TYPES.runaddin)
                        {
                            //if it doesn't have an addin it's not an error
                            bool bHasAddIn
                                = DataHelpers.LinqHelpers.LinkedViewHaveAddIn(uri.URIDataManager.LinkedView);
                            if (bHasAddIn)
                            {
                                SetNoXmlDocErrorMsg(uri, displayDocType);
                            }
                        }
                        //no error -just showing a selected view explanation
                    }
                    else
                    {
                        //if it doesn't have an addin it's not an error
                        bool bHasAddIn
                            = DataHelpers.LinqHelpers.LinkedViewHaveAddIn(uri.URIDataManager.LinkedView);
                        if (bHasAddIn)
                        {
                            SetNoXmlDocErrorMsg(uri, displayDocType);
                        }
                    }
                }
                else
                {
                    SetNoXmlDocErrorMsg(uri, displayDocType);
                }
            }
            uri.ErrorMessage = string.Concat(sErrorMsg, uri.ErrorMessage);
            bIsCompleted = true;
            return bIsCompleted;
        }
        public async Task<bool> SaveFullXhtmlDocFromXmlDoc1Async(ContentURI uri,
            string xmlDocToReadPath,  string filePathToXhtmlDoc,
            DataAppHelpers.Resources.FILEEXTENSION_TYPES fileExtType,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bHasHtmlDoc = false;
            string sFragHtmlFile = string.Empty;
            if (fileExtType == DataAppHelpers.Resources.FILEEXTENSION_TYPES.html
                && filePathToXhtmlDoc != string.Empty
                && filePathToXhtmlDoc.EndsWith(
                DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag.ToString()))
            {
                //fragfile path is done first
                sFragHtmlFile = filePathToXhtmlDoc;
            }
            //std html doc path
            filePathToXhtmlDoc = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, xmlDocToReadPath, fileExtType,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            //pass back
            uri.URIDataManager.MiscDocPath = filePathToXhtmlDoc;
            //check whether a new html doc is needed
            bool bNeedsNewHtmlDoc = await DataHelpers.XmlFileIO.NeedsNewXhtmlDoc(
                uri, displayDocType, xmlDocToReadPath, filePathToXhtmlDoc);
            if (!bNeedsNewHtmlDoc)
            {
                //also returns the referenced filePathToXhtmlDoc
                bHasHtmlDoc = true;
            }
            else
            {
                //need two separate stateful html documents:
                //1) ajax responses: html fragments (i.e. no head or body elements)
                //      fragments have a uri.urifilextensiontype == "frag"
                //2) restful and package responses: full xhtml compliant documents (i.e. both head and body elements)
                //      full html documents have a uri.urifilextensiontype == "full"
                using (StringWriter writer
                        = new StringWriter())
                {
                    string sPathToHeaderFiles = string.Empty;
                    //the fragment should be written prior to the full html (for reuse)
                    if (fileExtType
                        == DataAppHelpers.Resources.FILEEXTENSION_TYPES.html)
                    {
                        //seo header
                        HtmlHelperExtensions.MakeXhtmlHeader(writer, uri, sPathToHeaderFiles);
                        if (await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, sFragHtmlFile)
                                && Path.GetExtension(sFragHtmlFile).EndsWith(
                                DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag.ToString()))
                        {
                            //use the existing html fragment 
                            await WriteXhtmlFragmentAsync(writer, uri, xmlDocToReadPath, sFragHtmlFile, displayDocType);
                        }
                        else
                        {
                            //needs to transform xml doc
                            await WriteXhtmlFragmentAsync(writer, uri, xmlDocToReadPath, displayDocType);
                        }
                        //close out the html doc (body, html)
                        HtmlHelperExtensions.MakeXhtmlFooter(writer);
                    }
                    else
                    {
                        await WriteXhtmlFragmentAsync(writer, uri, xmlDocToReadPath, displayDocType);
                    }
                    ////put the html into the stringwriter
                    //writer.Flush();
                    //writer.Close();
                    if (string.IsNullOrEmpty(uri.ErrorMessage))
                    {
                        //write the full html doc to disk
                        DataHelpers.FileStorageIO oFileStorageIO = new DataHelpers.FileStorageIO();
                        await oFileStorageIO.SaveHtmlTextURIAsync(uri, writer, filePathToXhtmlDoc);
                        if (string.IsNullOrEmpty(uri.ErrorMessage))
                        {
                            bHasHtmlDoc = true;
                        }
                    }
                }
            }
            return bHasHtmlDoc;
        }
        public async Task<bool> SaveFullXhtmlDocFromXmlDoc2Async(ContentURI uri, 
            string xmlDocToReadPath, string filePathToXhtmlDoc,
            DataAppHelpers.Resources.FILEEXTENSION_TYPES fileExtType,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bHasHtmlDoc = false;
            string sFragHtmlFile = string.Empty;
            if (fileExtType == DataAppHelpers.Resources.FILEEXTENSION_TYPES.html
                && filePathToXhtmlDoc != string.Empty
                && filePathToXhtmlDoc.EndsWith(
                DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag.ToString()))
            {
                //fragfile path is done first
                sFragHtmlFile = filePathToXhtmlDoc;
            }
            //std html doc path
            filePathToXhtmlDoc = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, xmlDocToReadPath, fileExtType,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            //pass back
            uri.URIDataManager.MiscDocPath = filePathToXhtmlDoc;
            //check whether a new html doc is needed
            bool bNeedsNewHtmlDoc = await DataHelpers.XmlFileIO.NeedsNewXhtmlDoc(
                uri, displayDocType, xmlDocToReadPath, filePathToXhtmlDoc);
            if (!bNeedsNewHtmlDoc)
            {
                //also returns the referenced filePathToXhtmlDoc
                bHasHtmlDoc = true;
            }
            else
            {
                //need two separate stateful html documents:
                //1) ajax responses: html fragments (i.e. no head or body elements)
                //      fragments have a uri.urifilextensiontype == "frag"
                //2) restful and package responses: full xhtml compliant documents (i.e. both head and body elements)
                //      full html documents have a uri.urifilextensiontype == "full"
                using (StringWriter writer
                        = new StringWriter())
                {
                    string sPathToHeaderFiles = string.Empty;
                    //the fragment should be written prior to the full html (for reuse)
                    if (fileExtType
                        == DataAppHelpers.Resources.FILEEXTENSION_TYPES.html)
                    {
                        //both server and client use same relative paths to all resources
                        HtmlHelperExtensions.MakeXhtmlHeader(writer, uri, sPathToHeaderFiles);
                        if (await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, sFragHtmlFile)
                            && Path.GetExtension(sFragHtmlFile).EndsWith(
                            DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag.ToString()))
                        {
                            //use the existing html fragment  
                            await WriteXhtmlFragmentAsync(writer, uri, xmlDocToReadPath, sFragHtmlFile, displayDocType);
                        }
                        else
                        {
                            //needs to transform xml doc
                            await WriteXhtmlFragmentAsync(writer, uri, xmlDocToReadPath, displayDocType);
                        }
                        //close out the html doc (body, html)
                        HtmlHelperExtensions.MakeXhtmlFooter(writer);
                    }
                    else
                    {
                        await WriteXhtmlFragmentAsync(writer, uri, xmlDocToReadPath, displayDocType);
                    }
                    ////put the html into the stringwriter
                    //writer.Flush();
                    //writer.Close();
                    if (string.IsNullOrEmpty(uri.ErrorMessage))
                    {
                        //write the full html doc to disk
                        DataHelpers.FileStorageIO oFileStorageIO = new DataHelpers.FileStorageIO();
                        await oFileStorageIO.SaveHtmlTextURIAsync(uri, writer, filePathToXhtmlDoc);
                        if (string.IsNullOrEmpty(uri.ErrorMessage))
                        {
                            bHasHtmlDoc = true;
                        }
                    }
                }
            }
            return bHasHtmlDoc;
        }
        public async Task<bool> WriteXhtmlFragmentAsync(StringWriter writer, ContentURI uri, 
            string xmlDocToReadPath, DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bIsCompleted = false;
            XmlReader oReader = null;
            if (await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, xmlDocToReadPath))
            {
                oReader = await DataHelpers.FileStorageIO.GetXmlReaderAsync(uri, xmlDocToReadPath);
            }
            else
            {
                SetNoXmlDocErrorMsg(uri, displayDocType);
            }
            if (oReader != null)
            {
                using (oReader)
                {
                    if (uri.URIDataManager.ServerSubActionType
                        != DataHelpers.GeneralHelpers.SERVER_SUBACTION_TYPES.respondwithxml
                        && Path.GetExtension(xmlDocToReadPath).EndsWith(
                        DataAppHelpers.Resources.GENERAL_RESOURCE_TYPES.xml.ToString()))
                    {
                        //write the transformed xml doc (ChangeDisplayParams switched apps so that servicespacks display correctly)
                        StylesheetHelper styleHelper = new StylesheetHelper();
                        await styleHelper.TransformXmlToXhtmlAsync(writer, uri, displayDocType, oReader);
                        
                    }
                    else
                    {
                        oReader.MoveToContent();
                        await writer.WriteAsync(await oReader.ReadOuterXmlAsync());
                    }
                    oReader.Close();
                }
            }
            bIsCompleted = true;
            return bIsCompleted;
        }
        public async Task<bool> WriteXhtmlFragmentAsync(StringWriter writer, 
            ContentURI uri, string xmlDocToReadPath, string htmlFragmentDocPath,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bIsCompleted = false;
            if (await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, htmlFragmentDocPath))
            {
                //write html
                DataHelpers.FileStorageIO oFileStorageIO = new DataHelpers.FileStorageIO();
                bIsCompleted = await oFileStorageIO.SaveHtmlURIToWriterAsync(uri, writer, htmlFragmentDocPath);
            }
            else
            {
                //transform xml
                bIsCompleted = await WriteXhtmlFragmentAsync(writer, uri, xmlDocToReadPath, displayDocType);
            }
            return bIsCompleted;
        }
        public static void SetNoXmlDocErrorMsg(ContentURI uri, 
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.firstdoc)
            {
                uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NODOCFORTHISNODE");
            }
            else if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.seconddoc)
            {
                uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NOADDIN");
            }
            else if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.thirddoc)
            {
                uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NOCALCULATIONS");
            }
            if (uri.URIDataManager.AppType 
                == DataHelpers.GeneralHelpers.APPLICATION_TYPES.devpacks
                && uri.URIDataManager.ServerSubActionType == DataHelpers.GeneralHelpers.SERVER_SUBACTION_TYPES.respondwithhtml)
            {
                uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_DEVPACKSINIT");
            }
        }
        public static void SetNoHtmlDocErrorMsg(ContentURI uri,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.firstdoc)
            {
                uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NODOCFORTHISNODE");
            }
            else if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.seconddoc)
            {
                uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NOADDIN");
            }
            else if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.thirddoc)
            {
                //not every step needs a calculated result
                if (string.IsNullOrEmpty(uri.ErrorMessage))
                {
                    uri.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NOCALCULATIONS");
                }
            }
        }
        private async Task<bool> SaveFullXhtmlDocFromXmlDoc1Async(StringWriter writer,
            ContentURI uri, string xmlDocToReadPath,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bIsCompleted = false;
            //check whether files with 'full' fileexttypes need to be converted to html
            //(i.e. full budgets). These files can get too big for day-to-day 
            //server side work, but the full xml and xhtml budgets will be downloaded 
            //in packages and then used on the client
            if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.thirddoc)
            {
                string sFullDocToReadPath = await DataAppHelpers.LinkedViews.GetFullXhtmlDocPath(
                    uri, xmlDocToReadPath);
                if (!string.IsNullOrEmpty(sFullDocToReadPath))
                {
                    if (await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, sFullDocToReadPath))
                    {
                        string sHtmlDocPath = string.Empty;
                        //this will be returned in ebook packages for clientside use
                        //client uses full xhtml docs
                        bool bHasHtmlDoc = await SaveFullXhtmlDocFromXmlDoc1Async(uri,
                            sFullDocToReadPath, sHtmlDocPath,
                            DataAppHelpers.Resources.FILEEXTENSION_TYPES.html,
                            displayDocType);
                        sHtmlDocPath = uri.URIDataManager.MiscDocPath;
                        if (bHasHtmlDoc)
                        {
                            //too slow on client
                            //if (writer != null)
                            //{
                            //    //write the full html doc as a string
                            //    DataHelpers.FileStorageIO oFileStorageIO = new DataHelpers.FileStorageIO();
                            //    await oFileStorageIO.SaveHtmlURIToWriterAsync(uri, writer, sHtmlDocPath);
                            //    oFileStorageIO = null;
                            //}
                        }
                    }
                }
            }
            bIsCompleted = true;
            return bIsCompleted;
        }
        private async Task<bool> SaveFullXhtmlDocFromFullXmlDoc1Async(StringWriter writer,
            DevTreks.Data.ContentURI uri, string xmlDocToReadPath, 
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType)
        {
            bool bIsCompleted = false;
            //check whether files with 'full' fileexttypes need to be converted to html
            //(i.e. full budgets). These files can get too big for day-to-day 
            //server side work, but the full xml and xhtml budgets will be downloaded 
            //in packages and then used on the client
            if (displayDocType
                == DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.thirddoc)
            {
                string sFullDocToReadPath = await DataAppHelpers.LinkedViews.GetFullXhtmlDocPath(
                    uri, xmlDocToReadPath);
                if (!string.IsNullOrEmpty(sFullDocToReadPath))
                {
                    if (await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, sFullDocToReadPath))
                    {
                        string sHtmlDocPath = string.Empty;
                        //this will be returned in ebook packages for clientside use
                        //client uses full xhtml docs
                        bool bHasHtmlDoc = await SaveFullXhtmlDocFromXmlDoc2Async(uri,
                            sFullDocToReadPath, sHtmlDocPath,
                            DataAppHelpers.Resources.FILEEXTENSION_TYPES.html,
                            displayDocType);
                        sHtmlDocPath = uri.URIDataManager.MiscDocPath;
                    }
                }
            }
            bIsCompleted = true;
            return bIsCompleted;
        }
        public static async Task<bool> CopyHtmlDocsToTempDocPaths(ContentURI uri,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType, string tempDocPath)
        {
            //addinstatehelper inits calculators by copying xml calcdoc and doctocalcs
            //to tempdocpaths; this copies corresponding html files (for extension authors
            //who write custom html views)
            string sHtmlFragmentDocPath = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, uri.URIClub.ClubDocFullPath, DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            string sTempHtmlFragmentDocPath = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, tempDocPath, DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            bool bHasCopied = await DevTreks.Data.Helpers.FileStorageIO.CopyURIsAsync(uri, sHtmlFragmentDocPath,
                sTempHtmlFragmentDocPath);
            return bHasCopied;
        }
        public static async Task<bool> CopyHtmlDocsFromTempDocPaths(ContentURI uri,
            DataHelpers.GeneralHelpers.DOC_STATE_NUMBER displayDocType, string tempDocPath)
        {
            bool bIsCompleted = false;
            //children linkedviews run from parents don't have any calcdoc html views
            //see if they need some
            string sHtmlFragmentDocPath = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, uri.URIClub.ClubDocFullPath, DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            string sTempHtmlFragmentDocPath = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, tempDocPath, DataAppHelpers.Resources.FILEEXTENSION_TYPES.frag,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            if (!await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, sHtmlFragmentDocPath))
            {
                bIsCompleted = await DevTreks.Data.Helpers.FileStorageIO.CopyURIsAsync(uri, sTempHtmlFragmentDocPath,
                    sHtmlFragmentDocPath);
            }
            string sHtmlFullDocPath = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, uri.URIClub.ClubDocFullPath, DataAppHelpers.Resources.FILEEXTENSION_TYPES.html,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            string sTempHtmlFullDocPath = DataHelpers.AppSettings.GetXhtmlDocPath(
                uri, displayDocType, tempDocPath, DataAppHelpers.Resources.FILEEXTENSION_TYPES.html,
                DataHelpers.GeneralHelpers.GetViewEditType(uri, displayDocType));
            if (!await DataHelpers.FileStorageIO.URIAbsoluteExists(uri, sHtmlFullDocPath))
            {
                bIsCompleted = await DevTreks.Data.Helpers.FileStorageIO.CopyURIsAsync(uri, sTempHtmlFullDocPath,
                    sHtmlFullDocPath);
            }
            bIsCompleted = true;
            return bIsCompleted;
        }
        
        //hold for potential future use
        public static async Task<ContentURI> DisplayResourcePackURI(ContentURI docToCalcURI)
        {
            ContentURI rpURI = new ContentURI();
            string sRPURL = string.Empty;
            //xhtml state is saved to increase performance and improve packaging
            string sDocToReadPath
                = await DataHelpers.AddInHelper.GetDevTrekPath(docToCalcURI, DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.seconddoc);
            XmlReader oReader = null;
            if (await DataHelpers.FileStorageIO.URIAbsoluteExists(docToCalcURI, sDocToReadPath))
            {
                oReader = await DataHelpers.FileStorageIO.GetXmlReaderAsync(docToCalcURI, sDocToReadPath);
            }
            else
            {
                SetNoXmlDocErrorMsg(rpURI, DataHelpers.GeneralHelpers.DOC_STATE_NUMBER.seconddoc);
            }
            if (oReader != null)
            {
                using (oReader)
                {
                    while (oReader.ReadToFollowing(DataAppHelpers.LinkedViews.LINKEDVIEWS_TYPES.linkedview.ToString()))
                    {
                        //standard Recommended IRI from Preview panel
                        sRPURL = oReader
                            .GetAttribute(DataAppHelpers.Calculator.cMediaURL);
                        if (!string.IsNullOrEmpty(sRPURL))
                        {
                            break;
                        }
                    }
                }
            }
            //sRPURL = "https://localhost:44300/agtreks/preview/crops/resourcepack/Agricultural Production, Capital Investment Media Pack/260/none ";
            if (!string.IsNullOrEmpty(sRPURL))
            {
                if (!sRPURL.Contains(DataAppHelpers.Resources.RESOURCES_TYPES.resourcepack.ToString())
                    || !sRPURL.Contains(DataHelpers.GeneralHelpers.SERVER_ACTION_TYPES.preview.ToString()))
                {
                    rpURI.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_BADMEDIAURL");
                }
                else
                {
                    rpURI = ContentURI.GetContentURIFromFullURL(sRPURL);
                    //add subactionview to url
                    rpURI.URIDataManager.SubActionView = DataHelpers.GeneralHelpers.SUBACTION_VIEWS.graph.ToString();
                }
                
            }
            else
            {
                rpURI.ErrorMessage = AppHelper.GetErrorMessage("DISPLAYHELPER_NOMEDIA");
            }
            return rpURI;
        }
    }
}
