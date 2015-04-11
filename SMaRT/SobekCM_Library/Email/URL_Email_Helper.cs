﻿#region Using directives

using System;
using System.Text;
using SobekCM.Library.Database;

#endregion

namespace SobekCM.Library.Email
{
    /// <summary> Helper class creates and sends the email when users 'share' a search or browse (essentially just a URL) </summary>
    public class URL_Email_Helper
    {
        /// <summary> Creates and sends the email when a user 'shares' a single digital resource </summary>
        /// <param name="Recepient_List"> Recepient list for this email </param>
        /// <param name="CC_List"> CC list for this email </param>
        /// <param name="Comments"> Sender's comments to be included in the email </param>
        /// <param name="User_Name"> Name of the user that sent this email </param>
        /// <param name="SobekCM_Instance_Name"> Name of the current SobekCM instance (i.e., UFDC, dLOC, etc..)</param>
        /// <param name="URL"> URL being shared </param>
        /// <param name="URL_Title"> Title associated with the URL </param>
        /// <param name="HTML_Format"> Tells if this should be sent as HMTL, otherwise it will be plain text </param>
        /// <param name="URL_Short_Type"> Short term which explains the type of URL being emailed (i.e., 'browse', 'search', etc..)</param>
        /// <returns> Any caught error message </returns>
        public static string Send_Email(string Recepient_List, string CC_List, string Comments, string User_Name, string SobekCM_Instance_Name, bool HTML_Format, string URL, string URL_Title, string URL_Short_Type )
        {
            if (HTML_Format)
            {
                return HTML_Send_Email(Recepient_List, CC_List, Comments, User_Name, SobekCM_Instance_Name, URL, URL_Title, URL_Short_Type).Length > 0 ? Text_Send_Email(Recepient_List, CC_List, Comments, User_Name, SobekCM_Instance_Name, URL, URL_Title, URL_Short_Type) : String.Empty;
            }

            return Text_Send_Email(Recepient_List, CC_List, Comments, User_Name, SobekCM_Instance_Name, URL, URL_Title, URL_Short_Type);
        }


        private static string HTML_Send_Email(string Recepient_List, string CC_List, string Comments, string User_Name, string SobekCM_Instance_Name, string URL, string URL_Title, string URL_Short_Type )
        {
            try
            {
                StringBuilder messageBuilder = new StringBuilder();

                messageBuilder.Append("<span style=\"font-family:Arial, Helvetica, sans-serif;\">");
                if ((Comments.Length > 0) && ( Comments != URL_Title ))
                {
                    messageBuilder.AppendLine(User_Name + " wanted you to see this " + URL_Short_Type + " on " + SobekCM_Instance_Name + " and included the following comments.<br /><br />\n");
                    messageBuilder.AppendLine(Comments.Replace("<", "(").Replace(">", ")").Replace("\"", "&quot;") + ".<br /><br />\n");
                }
                else
                {
                    messageBuilder.AppendLine(User_Name + " wanted you to see this " + URL_Short_Type + " on " + SobekCM_Instance_Name + ".<br /><br />\n");
                }

                messageBuilder.AppendLine("<a href=\"" + URL + "\">" + URL_Title + "</a>");
                messageBuilder.AppendLine("</span>");

                string[] email_recepients = Recepient_List.Split(";,".ToCharArray());
                foreach (string thisEmailRecepient in email_recepients)
                {
                    SobekCM_Database.Send_Database_Email(thisEmailRecepient.Trim() + "," + CC_List, URL_Short_Type + " from " + SobekCM_Instance_Name, messageBuilder.ToString(), true, false, -1);
                }
                return String.Empty;
            }
            catch (Exception ee)
            {
                return ee.Message;
            }
        }

        private static string Text_Send_Email(string Recepient_List, string CC_List, string Comments, string User_Name, string SobekCM_Instance_Name, string URL, string URL_Title, string URL_Short_Type)
        {
            try
            {
                StringBuilder messageBuilder = new StringBuilder();

                if ((Comments.Length > 0) && (Comments != URL_Title))
                {
                    messageBuilder.AppendLine(User_Name + " wanted you to see this " + URL_Short_Type + " on " + SobekCM_Instance_Name + " and included the following comments.\n");
                    messageBuilder.AppendLine("\"" + Comments.Replace("<", "(").Replace(">", ")").Replace("\"", "&quot;") + "\"\n");
                }
                else
                {
                    messageBuilder.AppendLine(User_Name + " wanted you to see this " + URL_Short_Type + " on " + SobekCM_Instance_Name + ".\n");
                }

                messageBuilder.AppendLine("\tURL:\t" + URL );
                messageBuilder.AppendLine("\tTitle:\t" + URL_Title );

                string[] email_recepients = Recepient_List.Split(";,".ToCharArray());
                foreach (string thisEmailRecepient in email_recepients)
                {
                    SobekCM_Database.Send_Database_Email(thisEmailRecepient.Trim() + "," + CC_List, URL_Short_Type + " from " + SobekCM_Instance_Name, messageBuilder.ToString(), false, false, -1);
                }
                return String.Empty;

            }
            catch (Exception ee)
            {
                return ee.Message;
            }
        }
    }
}
