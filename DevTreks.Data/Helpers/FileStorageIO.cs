using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DevTreks.Data.Helpers
{
    /// <summary>
    ///Purpose:		General utilities storing files on cloud and web server file storage systems
    ///Author:		www.devtreks.org
    ///Date:		2017, September
    /// </summary>
    public class FileStorageIO
    {
        /// <summary>
        /// Platform where DevTreks is deployed
        /// </summary>
        public enum PLATFORM_TYPES
        {
            none = 0,
            webserver = 1,
            azure = 2,
            amazon = 3
        }
        public static PLATFORM_TYPES GetPlatformType(ContentURI uri)
        {
            string sWebRoot = uri.URIDataManager.DefaultRootWebStoragePath;
            PLATFORM_TYPES ePlatform = PLATFORM_TYPES.azure;
            ePlatform = GetPlatformType(sWebRoot);
            return ePlatform;
        }
        public static PLATFORM_TYPES GetPlatformType(string url)
        {
            PLATFORM_TYPES ePlatform = PLATFORM_TYPES.azure;
            //PLATFORM_TYPES refers to file storage platform only
            //2.0.0 : only 3 conditions for blob storage
            if (url.Contains(".blob."))
            {
                ePlatform = PLATFORM_TYPES.azure;
            }
            else if (url.Contains("127.0.0.1"))
            {
                ePlatform = PLATFORM_TYPES.azure;
            }
            else if (url.Contains("azureml"))
            {
                //2.0.2 condition added for aml
                ePlatform = PLATFORM_TYPES.azure;
            }
            else
            {
                //if it's not azure and azure blob, it's in file system
                //could be a file system path or a web server path
                //prior conditions caused web server files to attempt retrieval 
                //from blob storage
                ePlatform = PLATFORM_TYPES.webserver;
            }
            return ePlatform;
        }
        
        
        public static bool IsFileSystemFile(string url)
        {
            bool bIsFileSystem = true;
            if (url.StartsWith("http"))
            {
                return false;
            }
            return bIsFileSystem;
        }
        public static string GetDelimiterForFileStorage(string uri)
        {
            string sDelimiter = Helpers.GeneralHelpers.FILE_PATH_DELIMITER;
            if (!Path.IsPathRooted(uri))
            {
                sDelimiter = Helpers.GeneralHelpers.WEBFILE_PATH_DELIMITER;
            }
            else
            {
                if (uri.Contains(Helpers.GeneralHelpers.WEBFILE_PATH_DELIMITER))
                {
                    sDelimiter = Helpers.GeneralHelpers.WEBFILE_PATH_DELIMITER;
                }
            }
            return sDelimiter;
        }
        public static char[] GetCDelimiterForFileStorage(string uri)
        {
            char[] cDelimiter = Helpers.GeneralHelpers.FILE_PATH_DELIMITERS;
            if (!Path.IsPathRooted(uri))
            {
                cDelimiter = Helpers.GeneralHelpers.WEBFILE_PATH_DELIMITERS;
            }
            return cDelimiter;
        }
        public static string GetDelimiterForFileStorage(ContentURI uri)
        {
            string sPathDelimiter = GeneralHelpers.FILE_PATH_DELIMITER;
            PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
            if (ePlatform == PLATFORM_TYPES.azure)
            {
                sPathDelimiter = GeneralHelpers.WEBFILE_PATH_DELIMITER;
            }
            return sPathDelimiter;
        }
        public static async Task<bool> URIAbsoluteExists(ContentURI uri, string fullURIPath)
        {
            bool bURIExists = false;
            if (!Path.HasExtension(fullURIPath))
            {
                return false;
            }
            if (Path.IsPathRooted(fullURIPath))
            {
                if (File.Exists(fullURIPath))
                {
                    bURIExists = true;
                }
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                //2.0.4 added this double check
                if (ePlatform == PLATFORM_TYPES.none)
                {
                    ePlatform = GetPlatformType(fullURIPath);
                    uri.URIDataManager.PlatformType = ePlatform;
                }
                if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.webserver)
                {
                    //176 supported dataurl with calculators
                    //this opens the file -which could interfere with the next step that also opens the file
                    //so just return the error when the file can not be opened
                    //bURIExists = WebServerFileIO.Exists(fullURIPath);
                    bURIExists = true;
                }
                else if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    //full uri paths had to be retrieved from cloud or web storage
                    bURIExists = await azureIO.BlobExistsAsync(fullURIPath);
                }
            }
            return bURIExists;
        }
        public static async Task<bool> FileExistsAsync(ContentURI uri, string fullURIPath)
        {
            bool bURIExists = false;
            if (!Path.HasExtension(fullURIPath))
            {
                return false;
            }
            if (Path.IsPathRooted(fullURIPath))
            {
                if (File.Exists(fullURIPath))
                {
                    bURIExists = true;
                }
            }
            else if (!fullURIPath.StartsWith("http"))
            {
                if (File.Exists(fullURIPath))
                {
                    bURIExists = true;
                }
            }
            else
            {
                PLATFORM_TYPES ePlatform = GetPlatformType(fullURIPath);
                if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.webserver)
                {
                    //176 supported dataurl with calculators
                    //this opens the file -which could interfere with the next step that also opens the file
                    //so just return the error when the file is opened not twice
                    //bURIExists = WebServerFileIO.Exists(fullURIPath);
                    bURIExists = true;
                }
                else if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bURIExists = await azureIO.BlobExistsAsync(fullURIPath);
                }
            }
            return bURIExists;
        }
        public static bool FileExists(string fullURIPath)
        {
            bool bURIExists = false;
            if (!Path.HasExtension(fullURIPath))
            {
                return false;
            }
            if (Path.IsPathRooted(fullURIPath))
            {
                if (File.Exists(fullURIPath))
                {
                    bURIExists = true;
                }
            }
            else
            {
                PLATFORM_TYPES ePlatform = GetPlatformType(fullURIPath);
                if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.webserver)
                {
                    //176 supported dataurl with calculators
                    //this opens the file -which could interfere with the next step that also opens the file
                    //so just return the error when the file is opened not twice
                    //bURIExists = WebServerFileIO.Exists(fullURIPath);
                    bURIExists = true;
                }
                else if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    //can't init storage with filepath alone
                    //let it fail when retrieved is it doesn't exist
                    //possibly scalable replacement for URIAbsoluteExists
                    bURIExists = true;
                }
            }
            return bURIExists;
        }
        public static bool DirectoryExists(ContentURI uri, string fullURIPath)
        {
            bool bDirectoryExists = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                string sDirectoryName = Path.GetDirectoryName(fullURIPath);
                if (Directory.Exists(sDirectoryName) == true)
                {
                    bDirectoryExists = true;
                }
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    //directories are part of blob name -they'll be built when blob is saved
                    bDirectoryExists = true;
                }
            }
            return bDirectoryExists;
        }
        public static bool DirectoryCreate(ContentURI uri, string fullURIPath)
        {
            bool bDirectoryExists = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                string sDirectoryName = Path.GetDirectoryName(fullURIPath);
                if (Directory.Exists(sDirectoryName) == false)
                {
                    Directory.CreateDirectory(sDirectoryName);
                    bDirectoryExists = true;
                }
                else
                {
                    bDirectoryExists = true;
                }
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    //directories are part of blob name -they'll be built when blob is saved
                    bDirectoryExists = true;
                }
            }
            
            return bDirectoryExists;
        }

        public static string GetDirectoryName(string fullURIPath)
        {
            string sDirectoryName = string.Empty;
            if (Path.IsPathRooted(fullURIPath))
            {
                sDirectoryName = Path.GetDirectoryName(fullURIPath);
            }
            else
            {
                if (fullURIPath.StartsWith("http"))
                {
                    //directories are part of blob name 
                    if (Path.HasExtension(fullURIPath))
                    {
                        sDirectoryName = AzureIOAsync.GetDirectoryName(fullURIPath);
                    }
                    else
                    {
                        //it is a directory name
                        sDirectoryName = fullURIPath;
                    }
                    //blob directories end with a path delimiter
                    if (!sDirectoryName.EndsWith(GeneralHelpers.WEBFILE_PATH_DELIMITER))
                    {
                        sDirectoryName = string.Concat(sDirectoryName, GeneralHelpers.WEBFILE_PATH_DELIMITER);
                    }
                }
            }
            return sDirectoryName;
        }
        public static async Task<bool> DeleteAndReplaceDirectory(ContentURI uri, string fullURIPath, 
            bool includeSubDirs)
        {
            bool bDirectoryIsDeleted = await DeleteDirectory(uri, fullURIPath, includeSubDirs);
            DirectoryCreate(uri, fullURIPath);
            return bDirectoryIsDeleted;
        }
        public static async Task<bool> DeleteDirectory(ContentURI uri, string fullURIPath, 
            bool includeSubDirs)
        {
            bool bDirectoryIsDeleted = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                bDirectoryIsDeleted = FileIO.DeleteDirectory(uri, fullURIPath, includeSubDirs);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (fullURIPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bDirectoryIsDeleted = await azureIO.DeleteDirectoryAsync(uri, fullURIPath, includeSubDirs);
                }
            }
            return bDirectoryIsDeleted;
        }
        public static async Task<string> GetDescendentDirectoryAsync(ContentURI uri, 
            string directoryPath, string directoryPattern)
        {
            string sDescDir = string.Empty;
            if (Path.IsPathRooted(directoryPath))
            {
                sDescDir = FileIO.GetDescendentDirectory(directoryPath, directoryPattern);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (directoryPath.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    sDescDir = await azureIO.GetDescendentDirectoryAsync(directoryPath, directoryPattern);
                }
            }
            return sDescDir;
        }
        
        public static async Task<bool> CopyDirectoriesAsync(ContentURI uri,
            string fromDirectory, string toDirectory, 
            bool copySubDirs, bool needsNewSubDirectories)
        {
            bool bHasCopied = false;
            PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
            if (Path.IsPathRooted(fromDirectory))
            {
                if (Path.IsPathRooted(toDirectory))
                {
                    bHasCopied = await FileIO.CopyDirectoriesAsync(
                        uri, fromDirectory, toDirectory,
                        copySubDirs, needsNewSubDirectories);
                }
                else
                {
                    if (ePlatform == PLATFORM_TYPES.azure)
                    {
                        AzureIOAsync azureIO = new AzureIOAsync(uri);
                        bHasCopied = await azureIO.CopyDirectoriesAsync(
                            uri, fromDirectory, toDirectory,
                            copySubDirs, needsNewSubDirectories);
                    }
                }
            }
            else
            {
                if (fromDirectory.StartsWith("http")
                    && ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bHasCopied = await azureIO.CopyDirectoriesAsync(
                        uri, fromDirectory, toDirectory,
                        copySubDirs, needsNewSubDirectories);
                }
            }
            return bHasCopied;
        }
        
        public static string AddDirectoryToDirectoryPath(string fromDirectoryPath, string toDirectoryPath)
        {
            string sFullDirectoryPath = toDirectoryPath;
            //deal with when absoluteuripath ends with a delimiter or file name
            if (Path.HasExtension(toDirectoryPath))
            {
                sFullDirectoryPath = GetDirectoryName(toDirectoryPath);
            }
            char[] cDelimiter = FileStorageIO.GetCDelimiterForFileStorage(fromDirectoryPath);
            string sDelimiter = FileStorageIO.GetDelimiterForFileStorage(fromDirectoryPath);
            string sLastDirectoryName = string.Empty;
            if (fromDirectoryPath.EndsWith(sDelimiter))
            {
                sLastDirectoryName = GeneralHelpers.GetSubstringFromEnd(
                   fromDirectoryPath, cDelimiter, 2);
            }
            else
            {
                sLastDirectoryName = GeneralHelpers.GetSubstringFromEnd(
                    fromDirectoryPath, cDelimiter, 1);
            }
            string sToDelimiter = FileStorageIO.GetDelimiterForFileStorage(toDirectoryPath);
            string sLastDirectoryDelimited = string.Concat(sToDelimiter,
                sLastDirectoryName, sToDelimiter);
            if (!sFullDirectoryPath.EndsWith(sLastDirectoryDelimited))
            {
                sFullDirectoryPath = Path.Combine(sFullDirectoryPath, sLastDirectoryName);
                if (!sFullDirectoryPath.EndsWith(sToDelimiter))
                {
                    sFullDirectoryPath = string.Concat(sFullDirectoryPath, sToDelimiter);
                }
            }
            return sFullDirectoryPath;
        }
        

        public async Task<bool> SaveBinaryStreamInURIAsync(ContentURI uri,
            Stream stream, string fullURIPath)
        {
            bool bHasSaved = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                //bool 
                bHasSaved = await FileIO.WriteBinaryBlobFileAsync(fullURIPath, stream);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    //string
                    fullURIPath = await azureIO.SaveResourceURLInCloudAsync(fullURIPath, stream);
                    if (!string.IsNullOrEmpty(fullURIPath))
                    {
                        bHasSaved = true;
                    }
                }
            }
            if (!bHasSaved)
            {
                uri.ErrorMessage = DevTreks.Exceptions.DevTreksErrors.GetMessage("FILESTORAGE_FILENOSAVEPOSTEDFILE");
            }
            return bHasSaved;
        }
        public async Task<bool> SaveXmlInURIAsync(ContentURI uri, 
            XmlReader reader, string fullURIPath)
        {
            bool bFileHasSaved = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                if (GeneralHelpers.IsXmlFileExt(fullURIPath))
                {
                    XmlFileIO xmlIO = new XmlFileIO();
                    bFileHasSaved = await xmlIO.WriteFileXmlAsync(
                        uri, reader, fullURIPath);
                }
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    //azure asynch is different than file system
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bFileHasSaved = await azureIO.WriteFileXmlAsync(reader, fullURIPath);
                }
            }
            if (!bFileHasSaved)
            {
                uri.ErrorMessage = DevTreks.Exceptions.DevTreksErrors.GetMessage("FILESTORAGE_FILENOSAVEXML");
            }
            return bFileHasSaved;
        }
        
       
        public async Task<bool> SaveFilesAsync(ContentURI uri, XmlDocument doc,
            string fullURIPath, List<Stream> sourceStreams)
        {
            bool bHasCompleted = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                //the xml does not need to be .xml (it could be .opf)
                XmlFileIO xmlIO = new XmlFileIO();
                bHasCompleted = await xmlIO.SaveFilesAsync(doc, fullURIPath, 
                    sourceStreams);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bHasCompleted = await azureIO.SaveBlobsAsync(doc, fullURIPath,
                        sourceStreams);
                }
            }
            return bHasCompleted;
        }
        public async Task<bool> SaveFileAsync(ContentURI uri, XmlDocument doc,
            string fullURIPath)
        {
            bool bHasCompleted = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                //the xml does not need to be .xml (it could be .opf)
                XmlFileIO xmlIO = new XmlFileIO();
                bHasCompleted = await xmlIO.SaveFileAsync(doc, fullURIPath);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bHasCompleted = await azureIO.SaveBlobAsync(doc, fullURIPath);
                }
            }
            return bHasCompleted;
        }
        
        public async Task<bool> SaveHtmlTextURIAsync(ContentURI uri, StringWriter writer,
            string fullURIPath)
        {
            bool bFileHasSaved = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                FileIO fileIO = new FileIO();
                bFileHasSaved = await fileIO.WriteHtmlTextFileAsync(fullURIPath, writer);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    //azure asynch not the same as filesystem
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bFileHasSaved = await azureIO.WriteHtmlTextFileAsync(writer, fullURIPath);
                }
            }
            if (!bFileHasSaved)
            {
                uri.ErrorMessage = DevTreks.Exceptions.DevTreksErrors.GetMessage("FILESTORAGE_FILENOSAVEHTMLORTEXT");
            }
            return bFileHasSaved;
        }
        
        public async Task<bool> SaveTextURIAsync(ContentURI uri,
            StringWriter writer, string fullURIPath)
        {
            bool bFileHasSaved = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                FileIO fileIO = new FileIO();
                bFileHasSaved = await fileIO.WriteTextFileAsync(fullURIPath, writer);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    //azure asynch not the same as filesystem
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bFileHasSaved = await azureIO.SaveXmlWriterInURIAsync(writer, fullURIPath);
                }
            }
            if (!bFileHasSaved)
            {
                uri.ErrorMessage = DevTreks.Exceptions.DevTreksErrors.GetMessage("FILESTORAGE_FILENOSAVEHTMLORTEXT");
            }
            return bFileHasSaved;
        }
        

        public async Task<bool> SaveTextURIAsync(ContentURI uri, string fullURIPath, string text)
        {
            bool bFileHasSaved = false;
            if (Path.IsPathRooted(fullURIPath))
            {
                FileIO fileIO = new FileIO();
                fileIO.WriteTextFile(fullURIPath, text);
                bFileHasSaved = true;
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;

                if (ePlatform == PLATFORM_TYPES.webserver)
                {
                    //don't use https urls or won't work)
                    string sFilePath
                        = AppSettings.ConvertPathFileandWeb(uri, fullURIPath);
                    if (Path.IsPathRooted(sFilePath))
                    {
                        FileIO fileIO = new FileIO();
                        bFileHasSaved = fileIO.WriteTextFile(sFilePath, text);
                    }
                }
                else if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bFileHasSaved = await azureIO.SaveStringInURIAsync(text, fullURIPath);
                }
            }
            if (!bFileHasSaved)
            {
                uri.ErrorMessage = DevTreks.Exceptions.DevTreksErrors.GetMessage("FILESTORAGE_FILENOSAVEHTMLORTEXT");
            }
            return bFileHasSaved;
        }

        public async Task<bool> SaveHtmlURIToWriterAsync(ContentURI uri,
            StringWriter writer, string fullURIPath)
        {
            bool bHasCompleted = false;
            //reduce memory use of html strings
            await writer.WriteAsync(await ReadTextAsync(uri, fullURIPath));
            bHasCompleted = true;
            return bHasCompleted;
        }
        
        public async Task<List<string>> ReadLinesAsync(ContentURI uri, 
            string fullURIPath, int rowIndex = -1)
        {
            List<string> lines = new List<string>();
            if (await URIAbsoluteExists(uri, fullURIPath) == false)
            {
                return lines;
            }
            PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
            if (Path.IsPathRooted(fullURIPath))
            {
                FileIO fileIO = new FileIO();
                lines = await fileIO.ReadLinesAsync(fullURIPath, rowIndex);
            }
            else
            {
                //176 starting using dataurl prop on localhost
                if (ePlatform == PLATFORM_TYPES.webserver)
                {
                    WebServerFileIO wsfileIO = new WebServerFileIO();
                    //214 optional row index
                    lines = await wsfileIO.ReadLinesAsync2(fullURIPath, rowIndex);
                    //lines = await wsfileIO.ReadLinesAsync(fullURIPath);
                }
                else if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    lines = await azureIO.ReadLinesAsync(fullURIPath, rowIndex);
                }
            }
            return lines;
        }

        public async Task<string> InvokeHttpRequestResponseServiceAsync(string fullURIPath)
        {
            string sResponse = string.Empty;
            //use configure await to make sure the response is returned to algo
            //return await WebServerFileIO.InvokeHttpRequestResponseServiceAsync().ConfigureAwait(false);
            return sResponse;
            //if (URIAbsoluteExists(fullURIPath) == false)
            //{
            //    return lines;
            //}
            //PLATFORM_TYPES ePlatform = GetPlatformType();
            //if (Path.IsPathRooted(fullURIPath))
            //{
            //    FileIO fileIO = new FileIO();
            //    lines = await fileIO.ReadLinesAsync(fullURIPath);
            //}
            //else
            //{
            //    //176 starting using dataurl prop on localhost
            //    if (ePlatform == PLATFORM_TYPES.webserver)
            //    {
            //        WebServerFileIO wsfileIO = new WebServerFileIO();
            //        lines = await wsfileIO.ReadLinesAsync(fullURIPath);
            //    }
            //    else if (ePlatform == PLATFORM_TYPES.azure)
            //    {
            //        AzureIOAsync azureIO = new AzureIOAsync(uri);
            //        lines = await azureIO.ReadLinesAsync(fullURIPath);
            //    }
            //}
        }
        public async Task<string> InvokeHttpRequestResponseService(
            ContentURI uri, string baseURL, string apiKey,
            string inputFileLocation, string outputFileLocation, string script)
        {
            string sResponseMsg = string.Empty;
            PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
            if (Path.IsPathRooted(inputFileLocation))
            {
                //use configure await to make sure the response is returned to algo
                sResponseMsg = await WebServerFileIO.InvokeHttpRequestResponseService(baseURL, apiKey,
                        inputFileLocation, outputFileLocation, script).ConfigureAwait(false);
            }
            else
            {
                if (ePlatform == PLATFORM_TYPES.webserver)
                {
                    //use configure await to make sure the response is returned to algo
                    sResponseMsg = await WebServerFileIO.InvokeHttpRequestResponseService(baseURL, apiKey,
                        inputFileLocation, outputFileLocation, script).ConfigureAwait(false);
                }
                else if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    sResponseMsg = await azureIO.InvokeHttpRequestResponseService(uri, baseURL, apiKey,
                        inputFileLocation, outputFileLocation, script).ConfigureAwait(false);
                }
            }
            return sResponseMsg;
        }
        public async Task<string> InvokeHttpRequestResponseService2(
            ContentURI uri, string baseURL, string apiKey,
            string inputBlob1Location, string inputBlob2Location,
            string outputBlob1Location, string outputBlob2Location)
        {
            string sResponseMsg = string.Empty;
            PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
            if (Path.IsPathRooted(inputBlob1Location))
            {
                //use configure await to make sure the response is returned to algo
                sResponseMsg = await WebServerFileIO.InvokeHttpRequestResponseService2(baseURL, apiKey,
                        inputBlob1Location, inputBlob2Location, outputBlob1Location, outputBlob2Location).ConfigureAwait(false);
            }
            else
            {
                if (ePlatform == PLATFORM_TYPES.webserver)
                {
                    //use configure await to make sure the response is returned to algo
                    sResponseMsg = await WebServerFileIO.InvokeHttpRequestResponseService2(baseURL, apiKey,
                        inputBlob1Location, inputBlob2Location, outputBlob1Location, outputBlob2Location).ConfigureAwait(false);
                }
                else if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    sResponseMsg = await azureIO.InvokeHttpRequestResponseService2(baseURL, apiKey,
                        inputBlob1Location, inputBlob2Location, outputBlob1Location, outputBlob2Location).ConfigureAwait(false);
                }
            }
            return sResponseMsg;
        }
        
        public async Task<string> ReadTextAsync(ContentURI uri, 
            string fullURIPath)
        {
            string sTextString = string.Empty;
            if (await URIAbsoluteExists(uri, fullURIPath) == false)
            {
                return sTextString;
            }
            if (Path.IsPathRooted(fullURIPath))
            {
                FileIO fileIO = new FileIO();
                sTextString = await fileIO.ReadTextAsync(uri, fullURIPath);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.webserver)
                {
                    //this is not debugged
                    WebServerFileIO webIO = new WebServerFileIO();
                    sTextString = await webIO.ReadTextAsync(fullURIPath);
                }
                else if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    sTextString = await azureIO.ReadTextAsync(fullURIPath);
                }
            }
            return sTextString;
        }
        public static async Task<XElement> LoadXmlElement(ContentURI uri, 
            string fullURIPath)
        {
            XElement el = null;
            //correctly retrieves blob or filesys reader
            XmlReader reader = await GetXmlReaderAsync(uri, fullURIPath);
            if (reader != null)
            {
                using (reader)
                {
                    el = XElement.Load(reader);
                }
            }
            return el;
        }
        
        public static async Task<XmlReader> GetXmlReaderAsync(ContentURI uri, 
            string fullURIPath)
        {
            XmlReader reader = null;
            if (await URIAbsoluteExists(uri, fullURIPath) == false)
            {
                return reader;
            }
            if (Path.IsPathRooted(fullURIPath))
            {
                //this reader can be read async
                reader = XmlFileIO.GetXmlFromFileAsync(fullURIPath);
            }
            else
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    reader = await azureIO.GetXmlFromURIAsync(fullURIPath);
                }
            }
            return reader;
        }
        public static async Task<XElement> LoadXmlElementAsync(ContentURI uri,
            string fullURIPath)
        {
            XElement el = null;
            //correctly retrieves blob or filesys reader
            XmlReader reader = await GetXmlReaderAsync(uri, fullURIPath);
            if (reader != null)
            {
                using (reader)
                {
                    el = XElement.Load(reader);
                }
            }
            return el;
        }
        
        public static async Task<bool> CopyURIsAsync(ContentURI uri, 
            string fromURIPath, string toURIPath)
        {
            bool bHasCopied = false;
            //2.0.0 refactored from URIAbsoluteExists(fromURIPath) because azure uses localhost to debug
            if (await FileExistsAsync(uri, fromURIPath) == true
                && fromURIPath.Equals(toURIPath) == false
                && (!string.IsNullOrEmpty(toURIPath)))
            {
                PLATFORM_TYPES ePlatform = uri.URIDataManager.PlatformType;
                if (Path.IsPathRooted(fromURIPath))
                {
                    if (Path.IsPathRooted(toURIPath))
                    {
                        bHasCopied = await FileIO.CopyFilesAsync(
                            uri, fromURIPath, toURIPath);
                    }
                    else
                    {
                        if (ePlatform == PLATFORM_TYPES.azure)
                        {
                            //2.0.0 debugs azure blob storage using localhost
                            //note topath and frompath are reversed below
                            ePlatform = GetPlatformType(toURIPath);
                            if (ePlatform == PLATFORM_TYPES.azure)
                            {
                                AzureIOAsync azureIO = new AzureIOAsync(uri);
                                bHasCopied = await azureIO.SaveFileinCloudAsync(fromURIPath, toURIPath);
                            }
                            else
                            {
                                if (ePlatform == PLATFORM_TYPES.webserver)
                                {
                                    WebServerFileIO webIO = new WebServerFileIO();
                                    bHasCopied = await webIO.CopyWebFileToFileSystemAsync(toURIPath, fromURIPath);
                                }
                            }
                        }
                        else
                        {
                            //web server doesn't handle https and should use filesystem
                        }
                    }
                }
                else
                {
                    if (Path.IsPathRooted(toURIPath))
                    {
                        if (ePlatform == PLATFORM_TYPES.azure)
                        {
                            //2.0.0 debugs azure blob storage using localhost
                            ePlatform = GetPlatformType(fromURIPath);
                            if (ePlatform == PLATFORM_TYPES.azure)
                            {
                                AzureIOAsync azureIO = new AzureIOAsync(uri);
                                bHasCopied = await azureIO.SaveCloudFileAsync(fromURIPath, toURIPath);
                            }
                            else
                            {
                                if (ePlatform == PLATFORM_TYPES.webserver)
                                {
                                    WebServerFileIO webIO = new WebServerFileIO();
                                    bHasCopied = await webIO.CopyWebFileToFileSystemAsync(fromURIPath, toURIPath);
                                }
                            }
                        }
                        else
                        {
                            //210: to debug using localhost:5000
                            ePlatform = GetPlatformType(fromURIPath);
                            if (ePlatform == PLATFORM_TYPES.webserver)
                            {
                                WebServerFileIO webIO = new WebServerFileIO();
                                bHasCopied = await webIO.CopyWebFileToFileSystemAsync(fromURIPath, toURIPath);
                            }
                        }
                    }
                    else
                    {
                        if (ePlatform == PLATFORM_TYPES.azure)
                        {
                            AzureIOAsync azureIO = new AzureIOAsync(uri);
                            bHasCopied = await azureIO.CopyBlobAsync(
                                uri, fromURIPath, toURIPath);
                        }
                        else
                        {
                            //web server doesn't handle https and should use filesystem
                            

                        }
                    }
                }
            }
            return bHasCopied;
        }
        public static async Task<bool> DeleteURIAsync(ContentURI uri, string deleteURIPath)
        {
            bool bIsDeleted = false;
            if (await URIAbsoluteExists(uri, deleteURIPath) == true
                && (!string.IsNullOrEmpty(deleteURIPath)))
            {
                if (Path.IsPathRooted(deleteURIPath))
                {
                    FileIO.DeleteFile(uri, deleteURIPath);
                    bIsDeleted = true;
                }
                else
                {
                    PLATFORM_TYPES ePlatform 
                        = uri.URIDataManager.PlatformType;
                    if (ePlatform == PLATFORM_TYPES.azure)
                    {
                        AzureIOAsync azureIO = new AzureIOAsync(uri);
                        bIsDeleted = await azureIO.DeleteBlobAsync(deleteURIPath);
                    }
                }
            }
            return bIsDeleted;
        }
        public static async Task<bool> DeleteURIsContainingSubstringAsync(ContentURI uri, 
            string changedURIPath, string subString)
        {
            bool bIsDeleted = false;
            if (await URIAbsoluteExists(uri, changedURIPath))
            {
                //if one is rooted the other is too (no mix between blob and file system storage)
                if (Path.IsPathRooted(changedURIPath))
                {
                    FileIO.DeleteDirectoryFilesContainingSubstring(
                        uri, changedURIPath, subString);
                    bIsDeleted = true;
                }
                else
                {
                    PLATFORM_TYPES ePlatform
                        = uri.URIDataManager.PlatformType;
                    if (ePlatform == PLATFORM_TYPES.azure)
                    {
                        AzureIOAsync azureIO = new AzureIOAsync(uri);
                        bIsDeleted = await azureIO.DeleteBlobAsync(changedURIPath);
                    }
                }
            }
            return bIsDeleted;
        }
        public static async Task<bool> DeleteURIsWithChangedNames(ContentURI uri, 
            string changedURIPath, string oldURIName)
        {
            bool bHasCompleted = false;
            //if one is rooted the other is too (no mix between blob and file system storage)
            if (Path.IsPathRooted(changedURIPath))
            {
                FileIO.DeleteFilesWithChangedNames(changedURIPath, oldURIName);
                bHasCompleted = true;
            }
            else
            {
                PLATFORM_TYPES ePlatform
                    = uri.URIDataManager.PlatformType;
                if (ePlatform == PLATFORM_TYPES.azure)
                {
                    AzureIOAsync azureIO = new AzureIOAsync(uri);
                    bHasCompleted = await azureIO.DeleteBlobAsync(changedURIPath);
                }
            }
            return bHasCompleted;
        }
        public static async Task<bool> MoveURIsAsync(ContentURI uri, string fromFile, string toFile)
        {
            bool bHasCompleted = false;
            if (await URIAbsoluteExists(uri, fromFile) == true
                && fromFile.Equals(toFile) == false
                && (!string.IsNullOrEmpty(fromFile))
                && (!string.IsNullOrEmpty(toFile)))
            {
                if (Path.IsPathRooted(fromFile))
                {
                    if (Path.IsPathRooted(toFile))
                    {
                        FileIO.MoveFiles(uri, fromFile, toFile);
                        bHasCompleted = true;
                    }
                    else
                    {
                        //file storage to blob storage
                        AzureIOAsync azureIO = new AzureIOAsync(uri);
                        await azureIO.MoveFileToBlobAsync(fromFile, toFile);
                        //delete the file
                        FileIO.DeleteFile(uri, fromFile);
                        bHasCompleted = true;
                    }
                }
                else
                {
                    PLATFORM_TYPES ePlatform
                        = uri.URIDataManager.PlatformType;
                    if (ePlatform == PLATFORM_TYPES.azure)
                    {
                        AzureIOAsync azureIO = new AzureIOAsync(uri);
                        bHasCompleted = await azureIO.MoveBlobAsync(fromFile, toFile);
                    }
                }
            }
            return bHasCompleted;
        }
        public static async Task<bool> CopyNewerFilesAsync(ContentURI uri, string fromDllPath, string toDllPath)
        {
            bool bHasCopied = false;
            //2.0.0 workaround for post build scripts and buildoption copy not working
            //probably better than post build scripts for cross platform use
            if (uri.URIMember.ClubInUse.PrivateAuthorizationLevel
                == DevTreks.Models.AccountHelper.AUTHORIZATION_LEVELS.fulledits)
            {
                //only update if the dll is updated
                bool bIsNewerFile = await File1IsNewerAsync(uri, fromDllPath, toDllPath);
                if (bIsNewerFile)
                {
                    bHasCopied = await CopyURIsAsync(
                            uri, fromDllPath, toDllPath);
                }
            }
            return bHasCopied;
        }
        public static async Task<bool> File1IsNewerAsync(ContentURI uri, 
            string file1Path, string file2Path)
        {
            bool bXmlIsNewer = false;
            bool bFile2Exists = await FileStorageIO.URIAbsoluteExists(uri, file2Path);
            if (!bFile2Exists)
                return true;
            if (await FileStorageIO.URIAbsoluteExists(uri, file1Path)
                && bFile2Exists)
            {
                DateTime xmlFileTime 
                    = await GetLastWriteTimeUtcAsync(uri, file1Path);
                DateTime xhtmlFileTime 
                    = await GetLastWriteTimeUtcAsync(uri, file2Path);
                if (xmlFileTime > xhtmlFileTime)
                {
                    //rule 3: if the xmldoc is older than than the html, make a new html
                    bXmlIsNewer = true;
                }
            }
            return bXmlIsNewer;
        }
        public static async Task<DateTime> GetLastWriteTimeUtcAsync(ContentURI uri, 
            string fileURIPath)
        {
            DateTime date = GeneralHelpers.GetDateShortNow();
            if (await URIAbsoluteExists(uri, fileURIPath) == true)
            {
                if (Path.IsPathRooted(fileURIPath))
                {
                    date = File.GetLastWriteTimeUtc(fileURIPath);
                }
                else
                {
                    PLATFORM_TYPES ePlatform
                        = uri.URIDataManager.PlatformType;
                    if (ePlatform == PLATFORM_TYPES.azure)
                    {
                        AzureIOAsync azureIO = new AzureIOAsync(uri);
                        CloudBlockBlob blob = await azureIO.GetBlobAsync(fileURIPath);
                        if (blob != null)
                        {
                            //have to fetch the attributes prior to reading them
                            await blob.FetchAttributesAsync();
                            DateTimeOffset blobdate = blob.Properties.LastModified.Value;
                            //165 fix
                            date = blobdate.UtcDateTime;
                        }
                    }
                }
            }
            return date;
        }
        public static async Task<string> GetResourceURIPath(ContentURI uri, string existingURIPath)
        {
            string sURIPath = string.Empty;
            string sErrorMsg = string.Empty;
            sURIPath = await GetResourceURIPathAsync(uri, 
                existingURIPath);
            return sURIPath;
        }
        public static async Task<string> GetResourceURIPathAsync(ContentURI uri,
            string existingURIPath)
        {
            string sURIPath = string.Empty;
            PLATFORM_TYPES ePlatform
                    = uri.URIDataManager.PlatformType;
            if (ePlatform == PLATFORM_TYPES.azure)
            {
                AzureIOAsync azureIO = new AzureIOAsync(uri);
                sURIPath = await azureIO.GetResourceURIPathAsync(existingURIPath, uri);
            }
            else
            {
                if (await URIAbsoluteExists(uri, existingURIPath))
                {
                    sURIPath = existingURIPath;
                }
                else
                {
                    uri.ErrorMessage
                        = DevTreks.Exceptions.DevTreksErrors.MakeStandardErrorMsg(
                        string.Empty, "RESOURCES_NOFILE");
                }
            }
            return sURIPath;
        }
        
    }
}
