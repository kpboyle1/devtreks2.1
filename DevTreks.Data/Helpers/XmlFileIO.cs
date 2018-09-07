using DevTreks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace DevTreks.Data.Helpers
{
    /// <summary>
    ///Purpose:		General utilities supporting xml file io
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    ///References:	www.devtreks.org/helptreks/linkedviews/help/linkedview/HelpFile/148
    /// </summary>
    public class XmlFileIO
    {
        public XmlFileIO()
        {
            //each instance holds its own state
        }
        public static async Task<bool> AuthorizedEditNeedsHtmlFragment(ContentURI uri,
            GeneralHelpers.DOC_STATE_NUMBER displayDocType, 
            string xmlDocPath)
        {
            bool bNeedsNewXhtmlFrag = false;
            //tempdocs can be edited by anyone
            if (uri.URIMember.ClubInUse.PrivateAuthorizationLevel
                == AccountHelper.AUTHORIZATION_LEVELS.fulledits
                || uri.URIFileExtensionType == GeneralHelpers.FILENAME_EXTENSIONS.temp.ToString())
            {
                if (uri.URIDataManager.ServerSubActionType
                    == GeneralHelpers.SERVER_SUBACTION_TYPES.runaddin)
                {
                    //if temp doc exists they are in the middle of running calcs
                    //if save = calcs they have just finished running calcs
                    if (await FileStorageIO.URIAbsoluteExists(
                        uri, uri.URIDataManager.TempDocPath)
                        && (!string.IsNullOrEmpty(uri.URIDataManager.TempDocSaveMethod)
                        && uri.URIDataManager.TempDocSaveMethod != Helpers.GeneralHelpers.NONE))
                    {
                        if (displayDocType
                            == GeneralHelpers.DOC_STATE_NUMBER.seconddoc)
                        {
                            //v1.2.0: need to pass params to seconddoc telling which save message to display
                            bNeedsNewXhtmlFrag = true;
                        }
                        else
                        {
                            bNeedsNewXhtmlFrag = false;
                        }
                    }
                    else
                    {
                        if (displayDocType
                            == GeneralHelpers.DOC_STATE_NUMBER.seconddoc)
                        {
                            //always init calcors/analyzers with new doc
                            bNeedsNewXhtmlFrag = true;
                        }
                    }
                }
                else
                {
                    bNeedsNewXhtmlFrag = true;
                }
            }
            return bNeedsNewXhtmlFrag;
        }
        
        public static async Task<bool> NeedsNewXhtmlDoc(ContentURI uri, 
            GeneralHelpers.DOC_STATE_NUMBER displayDocType,
            string xmlDocPath, string xhtmlDocPath)
        {
            bool bNeedsNewXhtmlDoc = false;
            //rule 1: authorized edits always get new html
            bNeedsNewXhtmlDoc = await AuthorizedEditNeedsHtmlFragment(uri,
                displayDocType, xmlDocPath);
            if (bNeedsNewXhtmlDoc)
            {
                bNeedsNewXhtmlDoc = true;
                return bNeedsNewXhtmlDoc;
            }
            if (!await FileStorageIO.URIAbsoluteExists(uri, xhtmlDocPath))
            {
                //rule 2: if the html doesn't exist, make it
                bNeedsNewXhtmlDoc = true;
                return bNeedsNewXhtmlDoc;
            }
            bNeedsNewXhtmlDoc = await FileStorageIO.File1IsNewerAsync(
                uri, xmlDocPath, xhtmlDocPath);
            return bNeedsNewXhtmlDoc;
        }
        
        public static async Task<bool> AddNewestXmlFileWithExtensionToList(
            ContentURI uri, string docPath,
            string fileExtension, IDictionary<string, string> lstFilePaths)
        {
            bool bHasCompleted = false;
            DirectoryInfo dir = null;
            if (!FileStorageIO.DirectoryExists(uri, docPath))
            {
                uri.ErrorMessage = Exceptions.DevTreksErrors.MakeStandardErrorMsg(
                    string.Empty, "DIRECTORY_NOEXIST");
                return bHasCompleted;
            }
            if (Path.HasExtension(docPath))
            {
                //if it has an extension it needs the parent directory
                dir = new DirectoryInfo(
                    Path.GetDirectoryName(docPath));
            }
            else
            {
                dir = new DirectoryInfo(docPath);
            }
            bHasCompleted = await AddNewestFileWithFileExtension(uri, dir, fileExtension,
                lstFilePaths);
            bHasCompleted = true;
            return bHasCompleted;
        }
        public static async Task<bool> AddNewestFileWithFileExtensionIO(
            ContentURI uri, DirectoryInfo folder,
            string fileExtension, IDictionary<string, string> lstFilePaths)
        {
            bool bHasCompleted = false;
            FileInfo[] files = folder.GetFiles();
            FileInfo newestFile = null;
            bool bIsNewerFile = false;
            foreach (FileInfo file in files)
            {
                if (file.FullName.EndsWith(fileExtension)
                    && file.Extension == Helpers.GeneralHelpers.EXTENSION_XML)
                {
                    //use only the latest file calculated with that fileextension
                    //if this folder had more than one file with this extension, 
                    //it could mean that an old calculation, 
                    //from a previous calculator, was not deleted properly
                    if (newestFile != null)
                    {
                        bIsNewerFile
                            = await FileStorageIO.File1IsNewerAsync(
                                uri, file.FullName, newestFile.FullName);
                        if (bIsNewerFile)
                        {
                            newestFile = file;
                        }
                    }
                    else
                    {
                        newestFile = file;
                    }
                }
            }
            if (newestFile != null)
            {
                uri.URIDataManager.ParentStartRow = 0;
                AddFileToList(newestFile, uri, lstFilePaths);
            }
            bHasCompleted = true;
            return bHasCompleted;
        }
        public static async Task<bool> AddNewestFileWithFileExtension(
            ContentURI uri, DirectoryInfo folder,
            string fileExtension, IDictionary<string, string> lstFilePaths)
        {
            bool bHasCompleted = false;
            FileInfo[] files = folder.GetFiles();
            FileInfo newestFile = null;
            bool bIsNewerFile = false;
            foreach (FileInfo file in files)
            {
                if (Path.GetFileNameWithoutExtension(file.FullName).EndsWith(fileExtension)
                    && file.Extension == Helpers.GeneralHelpers.EXTENSION_XML)
                {
                    //rule 1: analyzers can only use calculator data
                    if (file.Name.StartsWith(GeneralHelpers.ADDIN))
                    {
                        //rule2: use only the latest file calculated with that fileextension
                        //if this folder had more than one file with this extension, 
                        //it could mean that an old calculation, 
                        //from a previous calculator, was not deleted properly
                        uri.URIDataManager.ParentStartRow++;
                        if (newestFile != null)
                        {
                            bIsNewerFile
                                = await FileStorageIO.File1IsNewerAsync(
                                    uri, file.FullName, newestFile.FullName);
                            if (bIsNewerFile)
                            {
                                newestFile = file;
                            }
                        }
                        else
                        {
                            newestFile = file;
                        }
                    }
                }
            }
            if (newestFile != null)
            {
                AddFileToList(newestFile, uri, lstFilePaths);
            }
            bHasCompleted = true;
            return bHasCompleted;
        }
        public static async Task<bool> AddNewestIOFile(
            ContentURI uri, DirectoryInfo folder,
            IDictionary<string, string> lstFilePaths)
        {
            bool bHasCompleted = false;
            //inputs and outputs don't have base NPV calcs and can be used directly without running new calculations
            FileInfo[] files = folder.GetFiles();
            FileInfo newestFile = null;
            bool bIsNewerFile = false;
            foreach (FileInfo file in files)
            {
                if (file.Extension == Helpers.GeneralHelpers.EXTENSION_XML)
                {
                    //rule2: use only the latest file calculated with that fileextension
                    //if this folder had more than one file with this extension, 
                    //it could mean that an old calculation, 
                    //from a previous calculator, was not deleted properly
                    uri.URIDataManager.ParentStartRow++;
                    if (newestFile != null)
                    {
                        bIsNewerFile
                            = await FileStorageIO.File1IsNewerAsync(
                                uri, file.FullName, newestFile.FullName);
                        if (bIsNewerFile)
                        {
                            newestFile = file;
                        }
                    }
                    else
                    {
                        newestFile = file;
                    }
                }
            }
            if (newestFile != null)
            {
                AddFileToList(newestFile, uri, lstFilePaths);
            }
            bHasCompleted = true;
            return bHasCompleted;
        }
        private static void AddFileToList(FileInfo file, ContentURI uri,
            IDictionary<string, string> lstFilePaths)
        {
            if (lstFilePaths.ContainsKey(uri.URIDataManager.ParentStartRow.ToString()) == false
                && file != null)
            {
                lstFilePaths.Add(uri.URIDataManager.ParentStartRow.ToString(), file.FullName);
            }
        }
        public static string GetXmlToConvertToXhtmlPath(ContentURI uri)
        {
            string sOldFullXHtmlFilePath
                = uri.URIMember.MemberDocFullPath;
            if (uri.URIDataManager.ServerActionType
                == Helpers.GeneralHelpers.SERVER_ACTION_TYPES.edit)
            {
                //refactor: why is this needed? both paths mirror one another
                sOldFullXHtmlFilePath = uri.URIClub.ClubDocFullPath;
            }
            return sOldFullXHtmlFilePath;
        }
        
        
        public async Task<bool> SaveFilesAsync(XmlDocument doc,
           string fullURIPath, List<Stream> sourceStreams)
        {
            bool bHasCompleted = false;
            //byte[] encodedText = Encoding.Unicode.GetBytes(doc.OuterXml);
            byte[] encodedText = Encoding.UTF8.GetBytes(doc.OuterXml);
            if (encodedText.Length > 0)
            {
                //do not close this stream; the closing procedure closes parallel streams
                FileStream sourceStream = new FileStream(fullURIPath,
                    FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true);
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                sourceStreams.Add(sourceStream);
            }
            bHasCompleted = true;
            return bHasCompleted;
        }
        public async Task<bool> SaveFileAsync(XmlDocument doc, string fullURIPath)
        {
            bool bHasCompleted = false;
            //byte[] encodedText = Encoding.Unicode.GetBytes(doc.OuterXml);
            byte[] encodedText = Encoding.UTF8.GetBytes(doc.OuterXml);
            if (encodedText.Length > 0)
            {
                //do not close this stream; the closing procedure closes parallel streams
                FileStream sourceStream = new FileStream(fullURIPath,
                    FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true);
                using (sourceStream)
                {
                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                }
            }
            bHasCompleted = true;
            return bHasCompleted;
        }
        public void WriteFileXml(ContentURI uri, 
            XmlDocument doc, string filePath)
        {
            bool bCanWrite = true;
            XmlTextWriter oTextWriter;
            try
            {
                //uses FileStream to open filepath; could open filestream here asynchronously and pass filestream instead of filePath
                FileStorageIO.DirectoryCreate(uri, filePath);
                oTextWriter = new XmlTextWriter(filePath, Encoding.UTF8);
                using (oTextWriter)
                {
                    bCanWrite = oTextWriter.BaseStream.CanWrite;
                    if (bCanWrite == true)
                    {
                        doc.WriteTo(oTextWriter);
                        oTextWriter.Flush();
                    }
                    else
                    {
                        bCanWrite = false;
                    }
                }
            }
            catch (Exception x)
            {
                uri.ErrorMessage = Exceptions.DevTreksErrors.MakeStandardErrorMsg(
                    x.ToString(), "FILEIO_CANTSAVEFILE");
                File.Delete(filePath);
            }

        }
        public async Task<bool> WriteFileXmlAsync(
            ContentURI uri, XmlReader reader, string filePath)
        {
            bool bHasSaved = false;
            FileStorageIO.DirectoryCreate(uri, filePath);
            using (reader)
            {
                reader.MoveToContent();
                FileIO fileIO = new FileIO();
                bHasSaved = await fileIO.WriteTextFileAsync(filePath, reader.ReadOuterXml());
            }
            return bHasSaved;
        }
        public void WriteFileXml(ContentURI uri, 
            XmlReader reader, string filePath)
        {
            bool bCanWrite = true;
            XmlTextWriter oTextWriter;
            try
            {
                //uses FileStream to open filepath; could open filestream here asynchronously and pass filestream instead of filePath
                FileStorageIO.DirectoryCreate(uri, filePath);
                oTextWriter = new XmlTextWriter(filePath, Encoding.UTF8);
                using (oTextWriter)
                {
                    bCanWrite = oTextWriter.BaseStream.CanWrite;
                    if (bCanWrite == true)
                    {
                        using (reader)
                        {
                            reader.MoveToContent();
                            oTextWriter.WriteRaw(reader.ReadOuterXml());
                            oTextWriter.Flush();
                        }
                    }
                    else
                    {
                        bCanWrite = false;
                    }
                }
            }
            catch (Exception x)
            {
                uri.ErrorMessage = Exceptions.DevTreksErrors.MakeStandardErrorMsg(
                    x.ToString(), "FILEIO_CANTSAVEFILE");
                File.Delete(filePath);
            }
            if (!bCanWrite)
            {
                uri.ErrorMessage = Exceptions.DevTreksErrors.MakeStandardErrorMsg(
                    string.Empty, "FILEIO_STREAMCLOSED");
            }
            oTextWriter = null;
        }
        public async Task<bool> GetXmlFromFile(ContentURI uri, 
            string filePath, XPathDocument xPathDoc)
        {
            bool bHasCompleted = false;
            xPathDoc = null;
            if (await FileStorageIO.URIAbsoluteExists(uri, filePath))
            {
                XmlReader reader = await FileStorageIO.GetXmlReaderAsync(uri, filePath);
                if (reader != null)
                {
                    using (reader)
                    {
                        xPathDoc = new XPathDocument(reader);
                    }
                }
            }
            bHasCompleted = true;
            return bHasCompleted;
        }
        public async Task<bool> GetXmlFromFile(ContentURI uri, 
            string filePath, XPathNavigator xPathNav)
        {
            bool bHasCompleted = false;
            xPathNav = null;
            if (await FileStorageIO.URIAbsoluteExists(uri, filePath))
            {
                XmlDocument oSelectedDoc = new XmlDocument();
                XmlReader reader 
                    = await Helpers.FileStorageIO.GetXmlReaderAsync(uri, filePath);
                if (reader != null)
                {
                    using (reader)
                    {
                        oSelectedDoc.Load(reader);
                    }
                }
                if (oSelectedDoc != null)
                {
                    xPathNav = oSelectedDoc.CreateNavigator();
                }
            }
            bHasCompleted = true;
            return bHasCompleted;
        }
        //these were deprecated in 160 in favor of FileStorage.GetXml
        public void GetXmlFromFile(string filePath, out XmlTextReader reader)
        {
            reader = null;
            ReadFileXml(filePath, out reader);
        }
        
        private void ReadFileXml(string filePath, out XmlTextReader reader)
        {
            reader = null;
            //will generate an exception that will be passed to calling procedure
            FileStream oFileStream = File.OpenRead(filePath);
            if (oFileStream != null)
            {
                reader = new XmlTextReader(oFileStream);
                oFileStream.Flush();
                //closing the reader will close the stream
            }
        }
        public void GetXmlFromFile(Stream uploadFileStream, out XmlReader reader)
        {
            reader = null;
            if (uploadFileStream.CanRead)
            {
                //keep settings consistent with linq to xml default
                XmlReaderSettings oSettings = new XmlReaderSettings();
                oSettings.ConformanceLevel = ConformanceLevel.Document;
                oSettings.IgnoreWhitespace = true;
                oSettings.IgnoreComments = true;
                reader = XmlReader.Create(uploadFileStream, oSettings);
            }
        }
        public static void GetXmlFromFile(string filePath, 
            out XmlReader reader)
        {
            reader = null;
            //filepath can be uri with http or filesystem
            if (FileStorageIO.FileExists(filePath))
            {
                //keep settings consistent with linq to xml default
                XmlReaderSettings oSettings = new XmlReaderSettings();
                oSettings.ConformanceLevel = ConformanceLevel.Document;
                oSettings.IgnoreWhitespace = true;
                oSettings.IgnoreComments = true;
                reader = XmlReader.Create(filePath, oSettings);
            }
        }
        public static XmlReader GetXmlFromFile( 
            string filePath)
        {
            //filepath can be uri with http or filesystem
            XmlReader reader = null;
            GetXmlFromFile(filePath, out reader);
            return reader;
        }
        public static XmlReader GetXmlFromFileAsync( 
            string filePath)
        {
            //filepath can be uri with http or filesystem
            //keep settings consistent with linq to xml default
            XmlReaderSettings oSettings = new XmlReaderSettings();
            oSettings.ConformanceLevel = ConformanceLevel.Document;
            oSettings.IgnoreWhitespace = true;
            oSettings.IgnoreComments = true;
            //210 addition to avoid System.Xml.XmlException: There is no Unicode byte order mark exception
            oSettings.IgnoreProcessingInstructions = true;
            //the reader can then be read with a while(reader.ReadAsync())
            oSettings.Async = true;
            XmlReader reader = XmlReader.Create(filePath, oSettings);
            //210 tests
            //NameTable oNameTable = new NameTable();
            //XmlNamespaceManager oNameManager = new XmlNamespaceManager(oNameTable);
            //XmlParserContext context = new XmlParserContext(null, oNameManager, null, XmlSpace.None);
            //XmlReader reader = XmlReader.Create(filePath, oSettings, context);
            return reader;
        }
        public static XmlDocument MakeRootDoc(string rootElementString)
        {
            XmlDocument oRootDoc = new XmlDocument();
            if (rootElementString != string.Empty)
            {
                oRootDoc.LoadXml(rootElementString);
            }
            return oRootDoc;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
            }
        }
        ~XmlFileIO()
        {
            Dispose(false);
        }
    }
}
