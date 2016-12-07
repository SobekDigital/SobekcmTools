﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Web;
using Jil;
using SobekCM.Core.Configuration;
using SobekCM.Core.Configuration.Localization;
using SobekCM.Core.MemoryMgmt;
using SobekCM.Core.Skins;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.JSON_Client_Helpers;
using SobekCM.Core.Configuration.Engine;
using SobekCM.Engine_Library.Skins;
using SobekCM.Tools;

#endregion

namespace SobekCM.Engine_Library.Endpoints
{
    /// <summary> Class supports all the web skin-level services provided by the SobekCM engine </summary>
    public class WebSkinServices : EndpointBase
    {
        /// <summary> Gets the complete (language agnostic) web skin, by web skin code </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="QueryString"></param>
        /// <param name="Protocol"></param>
        /// <param name="IsDebug"></param>
        public void GetCompleteWebSkin(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol, bool IsDebug )
        {
            if (UrlSegments.Count > 0)
            {
                Custom_Tracer tracer = new Custom_Tracer();

                try
                {
                    string skinCode = UrlSegments[0];
                    tracer.Add_Trace("WebSkinServices.GetCompleteWebSkin", "Getting skin for '" + skinCode + "'");

                    Complete_Web_Skin_Object returnValue = get_complete_web_skin(skinCode, tracer);

                    // If this was debug mode, then just write the tracer
                    if ( IsDebug )
                    {
                        Response.ContentType = "text/plain";
                        Response.Output.WriteLine("DEBUG MODE DETECTED");
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(tracer.Text_Trace);

                        return;
                    }

                    // Get the JSON-P callback function
                    string json_callback = "parseCompleteSkin";
                    if ((Protocol == Microservice_Endpoint_Protocol_Enum.JSON_P) && (!String.IsNullOrEmpty(QueryString["callback"])))
                    {
                        json_callback = QueryString["callback"];
                    }

                    // Use the base class to serialize the object according to request protocol
                    Serialize(returnValue, Response, Protocol, json_callback);
                }
                catch (Exception ee)
                {
                    if ( IsDebug )
                    {
                        Response.ContentType = "text/plain";
                        Response.Output.WriteLine("EXCEPTION CAUGHT!");
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(ee.Message);
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(ee.StackTrace);
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(tracer.Text_Trace);
                        return;
                    }

                    Response.ContentType = "text/plain";
                    Response.Output.WriteLine("Error completing request");
                    Response.StatusCode = 500;
                }
            }
        }

        /// <summary> Gets the language-specific web skin, by web skin code and language code </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="QueryString"></param>
        /// <param name="Protocol"></param>
        /// <param name="IsDebug"></param>
        public void GetWebSkin(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol, bool IsDebug )
        {
            if (UrlSegments.Count > 1)
            {
                Custom_Tracer tracer = new Custom_Tracer();
                Web_Skin_Object returnValue;
                try
                {
                    // Get the code and language from the URL
                    string skinCode = UrlSegments[0];
                    tracer.Add_Trace("WebSkinServices.GetWebSkin", "Getting skin for '" + skinCode + "'");

                    string language = UrlSegments[1];
                    Web_Language_Enum languageEnum = Web_Language_Enum_Converter.Code_To_Enum(language);
                    tracer.Add_Trace("WebSkinServices.GetWebSkin", "Getting skin for language '" + Web_Language_Enum_Converter.Enum_To_Name(languageEnum) + "'");

                    returnValue = get_web_skin(skinCode, languageEnum, Engine_ApplicationCache_Gateway.Settings.System.Default_UI_Language, tracer);

                    // If this was debug mode, then just write the tracer
                    if ( IsDebug )
                    {
                        Response.ContentType = "text/plain";
                        Response.Output.WriteLine("DEBUG MODE DETECTED");
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(tracer.Text_Trace);

                        return;
                    }
                }
                catch ( Exception ee )
                {
                    if ( IsDebug )
                    {
                        Response.ContentType = "text/plain";
                        Response.Output.WriteLine("EXCEPTION CAUGHT!");
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(ee.Message);
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(ee.StackTrace);
                        Response.Output.WriteLine();
                        Response.Output.WriteLine(tracer.Text_Trace);

                        return;
                    }

                    Response.ContentType = "text/plain";
                    Response.Output.WriteLine("Error completing request");
                    Response.StatusCode = 500;
                    return;
                }

                // Get the JSON-P callback function
                string json_callback = "parseWebSkin";
                if (( Protocol == Microservice_Endpoint_Protocol_Enum.JSON_P) && ( !String.IsNullOrEmpty(QueryString["callback"])))
                {
                    json_callback = QueryString["callback"];
                }

                // Use the base class to serialize the object according to request protocol
                Serialize(returnValue, Response, Protocol, json_callback);
            }
        }

        /// <summary> Get the list of ordered web skin codes </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="QueryString"></param>
        /// <param name="Protocol"></param>
        /// <param name="IsDebug"></param>
        public void GetOrderedCodes(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol, bool IsDebug )
        {
            // Get the JSON-P callback function
            string json_callback = "parseSkinCodes";
            if ((Protocol == Microservice_Endpoint_Protocol_Enum.JSON_P) && (!String.IsNullOrEmpty(QueryString["callback"])))
            {
                json_callback = QueryString["callback"];
            }

            // Use the base class to serialize the object according to request protocol
            Serialize(Engine_ApplicationCache_Gateway.Web_Skin_Collection.Ordered_Skin_Codes, Response, Protocol, json_callback);
        }

        /// <summary> [PUBLIC] Get the list of uploaded images for a particular web skin </summary>
        /// <param name="Response"></param>
        /// <param name="UrlSegments"></param>
        /// <param name="QueryString"></param>
        /// <param name="Protocol"></param>
        /// <param name="IsDebug"></param>
        /// <remarks> This REST API should be publicly available for users that are performing administrative work </remarks>
        public void GetWebSkinUploadedImages(HttpResponse Response, List<string> UrlSegments, NameValueCollection QueryString, Microservice_Endpoint_Protocol_Enum Protocol, bool IsDebug )
        {
            if (UrlSegments.Count > 0)
            {
                string webSkin = UrlSegments[0];

                // Ensure a valid aggregation
                Complete_Web_Skin_Object skinObject = get_complete_web_skin(webSkin, null);
                if (skinObject != null)
                {
                    List<UploadedFileFolderInfo> serverFiles = new List<UploadedFileFolderInfo>();

                    string design_folder = Engine_ApplicationCache_Gateway.Settings.Servers.Base_Design_Location + "skins\\" + webSkin + "\\uploads";
                    if (Directory.Exists(design_folder))
                    {
                        string foldername = webSkin;

                        string[] files = Directory.GetFiles(design_folder);
                        foreach (string thisFile in files)
                        {
                            string filename = Path.GetFileName(thisFile);
                            string extension = Path.GetExtension(thisFile);

                            // Exclude some files
                            if ((!String.IsNullOrEmpty(extension)) && (extension.ToLower().IndexOf(".db") < 0) && (extension.ToLower().IndexOf("bridge") < 0) && (extension.ToLower().IndexOf("cache") < 0))
                            {
                                string url = Engine_ApplicationCache_Gateway.Settings.Servers.System_Base_URL + "design/skins/" + webSkin + "/uploads/" + filename;
                                serverFiles.Add(new UploadedFileFolderInfo(url, foldername));
                            }
                        }
                    }

                    JSON.Serialize(serverFiles, Response.Output, Options.ISO8601ExcludeNulls);
                }
            }
        }

        #region Helper methods, destined to be private

        /// <summary> [HELPER] Gets the complete (language agnostic) web skin, by web skin code </summary>
        /// <param name="SkinCode"> Web skin code </param>
        /// <param name="Tracer"></param>
        /// <returns> A built complete web skin </returns>
        /// <remarks> This may be public now, but this will be converted into a private helped class with 
        /// the release of SobekCM 5.0 </remarks>
        public static Complete_Web_Skin_Object get_complete_web_skin(string SkinCode, Custom_Tracer Tracer)
        {
            // Look in the cache for this version
            if (Tracer != null) Tracer.Add_Trace("WebSkinServices.get_complete_web_skin", "Look in the cache for a previously built complete skin object for '" + SkinCode + "'");
            Complete_Web_Skin_Object cacheObj = CachedDataManager.WebSkins.Retrieve_Complete_Skin(SkinCode, Tracer);
            if (cacheObj != null)
            {
                if (Tracer != null) Tracer.Add_Trace("WebSkinServices.get_complete_web_skin", "Found complete web skin object on the cache");
                return cacheObj;
            }

            // Get the web skin code row from the web skin collection object
            if (Tracer != null) Tracer.Add_Trace("WebSkinServices.get_complete_web_skin", "Try to get the web skin code row from the web skin collection object");
            DataRow thisRow = Engine_ApplicationCache_Gateway.Web_Skin_Collection.Skin_Row(SkinCode);
            if (thisRow == null)
            {
                if (Tracer != null) Tracer.Add_Trace("WebSkinServices.get_complete_web_skin", "Unable to find the web skin row for code '" + SkinCode + "'.  Returning NULL");
                return null;
            }

            if (Tracer != null)
            {
                Tracer.Add_Trace("WebSkinServices.get_complete_web_skin", "Found the web skin row for code '" + SkinCode + "'");
                Tracer.Add_Trace("WebSkinServices.get_complete_web_skin", "Building the copmlete web skin object");
            }

            // Build the complete web skin object
            Complete_Web_Skin_Object returnObject = Web_Skin_Utilities.Build_Skin_Complete(thisRow, Tracer);

            // If returned, cache it
            if (returnObject != null)
            {
                if (Tracer != null) Tracer.Add_Trace("WebSkinServices.get_complete_web_skin", "Passing the built complete web skin to the cache manager");
                CachedDataManager.WebSkins.Store_Complete_Skin(SkinCode, returnObject, Tracer);
            }

            return returnObject;
        }

        /// <summary> [HELPER] Gets the language-specific web skin, by web skin code and language code </summary>
        /// <param name="SkinCode"> Web skin code </param>
        /// <param name="RequestedLanguage"> Web language </param>
        /// <param name="DefaultLanguage"> Default language, in case the requested web language does nto exist </param>
        /// <param name="Tracer"></param>
        /// <returns> A build language-specific web skin </returns>
        /// <remarks> This may be public now, but this will be converted into a private helped class with 
        /// the release of SobekCM 5.0 </remarks>
        public static Web_Skin_Object get_web_skin(string SkinCode, Web_Language_Enum RequestedLanguage, Web_Language_Enum DefaultLanguage, Custom_Tracer Tracer)
        {
            Complete_Web_Skin_Object completeSkin = get_complete_web_skin(SkinCode, Tracer);

            if (completeSkin == null)
            {
                Tracer.Add_Trace("WebSkinServices.get_web_skin", "Complete skin retrieved was NULL, so returning NULL");
                return null;
            }

            // Look in the cache for this first
            Web_Skin_Object cacheObject = CachedDataManager.WebSkins.Retrieve_Skin(SkinCode, Web_Language_Enum_Converter.Enum_To_Code(RequestedLanguage), Tracer);
            if (cacheObject != null)
            {
                if (Tracer != null) Tracer.Add_Trace("WebSkinServices.get_web_skin", "Web skin found in the memory cache");
                return cacheObject;
            }

            // Try to get this language-specifi web skin
            Web_Skin_Object returnValue = Web_Skin_Utilities.Build_Skin(completeSkin, Web_Language_Enum_Converter.Enum_To_Code(RequestedLanguage), Tracer);

            // If this web skin has a value (an no exception) store in the cache
            if ((returnValue != null) && (String.IsNullOrEmpty(returnValue.Exception)))
            {
                if (Tracer != null) Tracer.Add_Trace("WebSkinServices.get_web_skin", "Store the web skin in the memory cache");
                CachedDataManager.WebSkins.Store_Skin(SkinCode, Web_Language_Enum_Converter.Enum_To_Code(RequestedLanguage), returnValue, Tracer );
            }

            // Return the object
            return returnValue;

        }

        #endregion
    }
}
