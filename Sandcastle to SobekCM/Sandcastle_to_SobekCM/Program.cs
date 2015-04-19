using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sandcastle_to_SobekCM
{
    public enum Code_Node_Type_Enum
    {
        Unrecognized = -1,
        New_Type = 1,
        All_Members,
        Single_Event,
        Single_Field,
        Single_Method,
        Single_Property,
        Single_Overload,
        Properties,
        Methods,
        Events,
        Fields,
        Namespace,
        Constructor
    }

    class Program
    {

        private const string Version = "4.8.7";

        static string source = @"C:\HELP\";
        static string destination_directory = "C:\\SobekCM_Help";
        static Dictionary<string, string> link_replacements = new Dictionary<string, string>();
        static string current_type = String.Empty;
        static string new_url_base = " <%BASEURL%>codehelp/";
        static string new_url_end = "<%?URLOPTS%>";
        static string sitemap_url_base = "codehelp/";

        

        static void Main(string[] args)
        {
            StreamWriter error_writer = new StreamWriter("errors.txt");

            // Read the sandcastle TOC
            Console.WriteLine("Read WebTOC.xml file");
            Sandcastle_TOC toc = Sandcastle_TOC_Reader.Read_TOC_File(source + "WebTOC.xml");

            // Create folders
            foreach (Sandcastle_TOC_Node childNode in toc.RootNode.Child_Nodes)
            {
                Console.WriteLine("Create folders and new structure: " + childNode.Title);
                create_and_assign_folders_recursively(childNode, destination_directory, "SobekCM.");
            }

            // Copy the top-level R_Project file as well
            if (System.IO.File.Exists(source + "html/R_Project.htm"))
                System.IO.File.Copy(source + "html/R_Project.htm", destination_directory + "\\R_Project.html");

            // Save the SobekCM site map
            Console.WriteLine("Save SobekCM Sitemap file");
            toc.Save_SobekCM_SiteMap("codehelp.sitemap", "codehelp");

            // Step through each file in the directory
            Console.WriteLine("Merging main page and class/interface members pages at each level.");
            rewrite_all_html_files(destination_directory, error_writer);

            // Merge DEFAULT and ALL MEMBER pages
            merge_default_all_members(destination_directory);

            error_writer.Flush();
            error_writer.Close();

            // Write the link replacements to a file for testing
            System.IO.StreamWriter writer = new System.IO.StreamWriter("replacements.txt", false );
            foreach (string thisKey in link_replacements.Keys)
            {
                writer.WriteLine(thisKey + " --> " + link_replacements[thisKey]);
            }
            writer.Flush();
            writer.Close();
        }

        private static void merge_default_all_members(string directory)
        {
            if (( System.IO.File.Exists(directory + "\\all_members.html")) && ( System.IO.File.Exists(directory + "\\default.html")))
            {
                string all_members_content = System.IO.File.ReadAllText( directory + "\\all_members.html" );
                int start = all_members_content.IndexOf("<h2>");
                int end = Math.Min(all_members_content.IndexOf("<h2>See Also"), all_members_content.IndexOf("</body>"));
                if (end < 0)
                    end = all_members_content.IndexOf("</body>");
                if (( start > 0 ) && ( end > start ))
                {
                    string to_keep = all_members_content.Substring(start, end - start );

                    string all_default_content = System.IO.File.ReadAllText( directory + "\\default.html" );
                    end = Math.Min(all_default_content.IndexOf("<h2>See Also"), all_default_content.IndexOf("</body>"));
                    if (end < 0)
                        end = all_default_content.IndexOf("</body>");
                    if ( end > 0 )
                    {
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(directory + "\\default.html", false );
                        writer.WriteLine(all_default_content.Substring(0, end));
                        writer.WriteLine();
                        writer.WriteLine(to_keep);
                        writer.WriteLine();
                        writer.WriteLine(all_default_content.Substring(end));
                        writer.Flush();
                        writer.Close();

                        // Delete old all_members here
                        bool delete = true;
                        System.IO.File.Delete(directory + "\\all_members.html");
                    }
                }
            }



            string[] subdirs = System.IO.Directory.GetDirectories(directory);
            foreach (string thisSubDir in subdirs)
            {
                merge_default_all_members(thisSubDir);
            }
        }

        private static void rewrite_all_html_files( string directory, StreamWriter error_writer  )
        {
            string[] files = System.IO.Directory.GetFiles(directory, "*.html");
            foreach (string thisFile in files)
            {
                rewrite_file(thisFile, error_writer);
            }

            string[] subdirs = System.IO.Directory.GetDirectories(directory);
            foreach (string thisSubDir in subdirs)
            {
                rewrite_all_html_files(thisSubDir, error_writer);
            }
        }

        private static void rewrite_file(string SourceFile, StreamWriter error_writer)
        {
            try
            {
                string filename = (new System.IO.FileInfo(SourceFile)).Name;
                Console.WriteLine("Rewriting " + filename);

                System.IO.StreamReader reader = new System.IO.StreamReader(SourceFile);
                string complete_file = reader.ReadToEnd();
                reader.Close();

                int content_start = complete_file.IndexOf("<div class=\"summary\">");
                //if (content_start < 0)
                //    content_start = complete_file.IndexOf("<b>Namespace:</b>");
                //if ( content_start < 0 )
                content_start = complete_file.IndexOf("<img src=\"http://sobekrepository.org/design/aggregations/ufdchelp/images/banners/coll.jpg\" /><br /><br /> </span>") + 136;
                if (content_start < 200)
                    content_start = complete_file.IndexOf("<img src=\"http://sobekrepository.org/design/aggregations/ufdchelp/images/banners/coll.jpg\" /><br />") + 112;
                int content_end = complete_file.IndexOf("<div id=\"footer\">");
                string content = complete_file.Substring(content_start, content_end - content_start);

                int title_start = complete_file.IndexOf("<title>");
                int title_end = complete_file.IndexOf("</title>");
                string title = complete_file.Substring(title_start + 7, title_end - title_start - 7);


                StringBuilder fileBuilder = new StringBuilder();
                fileBuilder.AppendLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">");
                fileBuilder.AppendLine("<html>");
                fileBuilder.AppendLine("\t<head>");
                fileBuilder.AppendLine("\t\t<title>" + title + "</title>");
                fileBuilder.AppendLine("\t\t<meta name=\"banner\" content=\"&lt;%BASEURL%&gt;design/webcontent/sobek.jpg\" />");
                fileBuilder.AppendLine("\t\t<meta name=\"date\" content=\"" + DateTime.Now.ToShortDateString() + "\" />");
                fileBuilder.AppendLine("\t\t<meta name=\"author\" content=\"Sullivan, Mark\" />");
                fileBuilder.AppendLine("\t\t<meta name=\"keywords\" content=\"help pages, technical help, xml\" />");
                fileBuilder.AppendLine("\t\t<meta name=\"sitemap\" content=\"sitemaps/sobekcm.sitemap\" />");
                fileBuilder.AppendLine("\t\t<meta name=\"description\" content=\"Description of how the SobekCM works with illustrations and code help\" />");
                fileBuilder.AppendLine("\t\t<link href=\"http://sobekrepository.org/design/webcontent/sobekcm/sobekcm_tech.css\" rel=\"stylesheet\" type=\"text/css\" />");
                fileBuilder.AppendLine("\t</head>");
                fileBuilder.AppendLine("<body>");
                fileBuilder.AppendLine("<div class=\"technicalcodetext\">");
                fileBuilder.AppendLine("<h1>" + title + "</h1>");
                fileBuilder.AppendLine("<br /><br />");

                fileBuilder.AppendLine(content);

                fileBuilder.AppendLine("<br /><br />");
                fileBuilder.AppendLine("<p><em>Version " + Version + " ( last generated <%LASTMODIFIED%> - mvs ) </em></p>");
                fileBuilder.AppendLine("</div>");
                fileBuilder.AppendLine("</body>");
                fileBuilder.AppendLine("</html>");

                foreach (string thisKey in link_replacements.Keys)
                {
                    fileBuilder.Replace("\"" + thisKey.Replace("html/", "") + "\"", "\"" + link_replacements[thisKey].Trim() + "\"");
                }


                fileBuilder.Replace("<col width=\"10%\" />", "");
                fileBuilder.Replace("\"../icons/", "\"<%BASEURL%>design/webcontent/codehelp/icons/");
                fileBuilder.Replace("<p />", "<br /><br />");

                string new_file_content = fileBuilder.ToString();
                while (new_file_content.IndexOf("<h1 class=\"heading\">") > 0)
                {
                    int start_start = new_file_content.IndexOf("<h1 class=\"heading\">");
                    int start_end = new_file_content.IndexOf("collapse_all.gif\" />", start_start);

                    int end_start = new_file_content.IndexOf("</span></h1>", start_end);

                    new_file_content = new_file_content.Substring(0, start_start) + "\r\n\r\n<h2>" + new_file_content.Substring(start_end + 20, end_start - start_end - 20) + "</h2>\r\n" + new_file_content.Substring(end_start + 12);


                }




                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(classToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(classToggle, event)\" tabindex=\"0\"><img id=\"classToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Classes</span></h1>", "\r\n\r\n<h2>Classes</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(delegateToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(delegateToggle, event)\" tabindex=\"0\"><img id=\"delegateToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Delegates</span></h1>", "\r\n\r\n<h2>Delegates</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(enumerationToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(enumerationToggle, event)\" tabindex=\"0\"><img id=\"enumerationToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Enumerations</span></h1>", "\r\n\r\n<h2>Enumerations</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(syntaxToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(syntaxToggle, event)\" tabindex=\"0\"><img id=\"syntaxToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Syntax</span></h1>", "\r\n\r\n<h2>Syntax</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(membersToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(membersToggle, event)\" tabindex=\"0\"><img id=\"membersToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Members</span></h1>", "\r\n\r\n<h2>Members</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(seeAlsoToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(seeAlsoToggle, event)\" tabindex=\"0\"><img id=\"seeAlsoToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />See Also</span></h1>", "\r\n\r\n<h2>See Also</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(remarksToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(remarksToggle, event)\" tabindex=\"0\"><img id=\"remarksToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Remarks</span></h1>", "\r\n\r\n<h2>Remarks</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(exampleToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(exampleToggle, event)\" tabindex=\"0\"><img id=\"exampleToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Examples</span></h1>", "\r\n\r\n<h2>Examples</h2>\r\n");
                //fileBuilder.Replace("<h1 class=\"heading\"><span onclick=\"ExpandCollapse(familyToggle)\" style=\"cursor:default;\" onkeypress=\"ExpandCollapse_CheckKey(familyToggle, event)\" tabindex=\"0\"><img id=\"familyToggle\" class=\"toggle\" name=\"toggleSwitch\" src=\"../icons/collapse_all.gif\" />Inheritance Hierarchy</span></h1>", "\r\n\r\n<h2>Inheritance Hierarchy</h2>\r\n");




                System.IO.StreamWriter writer = new System.IO.StreamWriter(SourceFile);
                writer.Write(new_file_content);
                writer.Flush();
                writer.Close();

                bool error = false;
            }
            catch (Exception ee)
            {
                error_writer.WriteLine("Error encountered on " + SourceFile + " : " + ee.Message);
            }

        }

        private static void create_and_assign_folders_recursively(Sandcastle_TOC_Node node, string directory, string current_namespace )
        {
            if (String.IsNullOrEmpty(node.URL))
                return;

            // Only namespaces get directories
            if (node.URL.IndexOf("N_") == 0 )
            {
                string current_url = "html/" + node.URL;

                // Get the namespace
                string complete_namespace = node.Title.Replace(" Namespace", "");
                if (complete_namespace.IndexOf(current_namespace) == 0)
                {
                    complete_namespace = complete_namespace.Substring(current_namespace.Length);
                }
                current_namespace = current_namespace + complete_namespace + ".";

                node.New_Title = node.Title;
                if (( node.Parent_Node != null ) && ( node.Parent_Node.Title.IndexOf(" Namespace") > 0) && ( node.Parent_Node.Title.IndexOf("Code Namespaces") < 0 ))
                {
                    node.New_Title = complete_namespace + " Sub-Namespace";
                }

                // Create the subdirectory
                directory = directory + "\\" + complete_namespace;
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);
                string new_url = directory.Replace(destination_directory + "\\", "").Replace("\\", "/");

                // Look for the matching file
                if (node.URL.Length > 0)
                {
                    System.IO.File.Copy(source + "\\html\\" + node.URL, directory + "\\default.html", true);
                    node.New_URL = sitemap_url_base + new_url;

                    string new_complete_url = new_url_base + new_url + new_url_end;
                    link_replacements.Add(current_url, new_complete_url);

                }
                else
                {
                    node.New_URL = String.Empty;
                }

                          
                node.Node_Type = Code_Node_Type_Enum.Namespace;


            }
            else
            {
                string original_file = node.URL;
                node.New_Title = node.Title;
                string name_sans_namespace = original_file.Replace(current_namespace.Replace(".", "_"), "");

                Code_Node_Type_Enum nodeType = Code_Node_Type_Enum.Unrecognized;
                string prefix = String.Empty;
                string subdir = String.Empty;
                bool defaultFolderFile = false;
                switch (name_sans_namespace.Substring(0, 2))
                {
                    case "T_":
                        nodeType = Code_Node_Type_Enum.New_Type;
                        prefix = "T_";
                        defaultFolderFile = true;
                        node.New_Title = node.New_Title.Replace(" Class", "").Replace(" Delegate", "").Replace(" Enumeration", "");
                        break;

                    case "Al":
                        nodeType = Code_Node_Type_Enum.All_Members;
                        prefix = "AllMembers_T_";
                        node.New_Title = "Class Members";
                        break;

                    case "E_":
                        nodeType = Code_Node_Type_Enum.Single_Event;
                        prefix = "E_";
                        subdir = "events\\";
                        node.New_Title = node.New_Title.Replace(" Event", "");                  
                        break;

                    case "F_":
                        nodeType = Code_Node_Type_Enum.Single_Field;
                        prefix = "F_";
                        subdir = "fields\\";
                        node.New_Title = node.New_Title.Replace(" Field", "");
                        break;

                    case "P_":
                        nodeType = Code_Node_Type_Enum.Single_Property;
                        prefix = "P_";
                        subdir = "properties\\";
                        node.New_Title = node.New_Title.Replace(" Property", "");
                        break;

                    case "M_":
                        nodeType = Code_Node_Type_Enum.Single_Method;
                        prefix = "M_";
                        subdir = "methods\\";
                        node.New_Title = node.New_Title.Replace(" Method", "");
                        break;

                    case "Ov":
                        nodeType = Code_Node_Type_Enum.Single_Overload;
                        prefix = "Overload_";
                        subdir = "overloads\\";
                        break;

                    case "Pr":
                        nodeType = Code_Node_Type_Enum.Properties;
                        prefix = "Properties_T_";
                        subdir = "properties\\";
                        defaultFolderFile = true;
                        node.New_Title = "Properties";
                        break;

                    case "Me":
                        nodeType = Code_Node_Type_Enum.Methods;
                        prefix = "Methods_T_";
                        subdir = "methods\\";
                        defaultFolderFile = true;
                        node.New_Title = "Methods";
                        break;

                    case "Ev":
                        nodeType = Code_Node_Type_Enum.Events;
                        prefix = "Events_T_";
                        subdir = "events\\";
                        defaultFolderFile = true;
                        node.New_Title = "Events";
                        break;

                    case "Fi":
                        nodeType = Code_Node_Type_Enum.Fields;
                        prefix = "Fields_T_";
                        subdir = "fields\\";
                        defaultFolderFile = true;
                        node.New_Title = "Fields";
                        break;
                }
                

                // Is this a new type?
                string new_file_name = name_sans_namespace.Substring(prefix.Length);
                if (nodeType == Code_Node_Type_Enum.New_Type)
                {
                    current_type = name_sans_namespace.Substring(prefix.Length).Replace(".htm", "");
                }
                else
                {
                    new_file_name = new_file_name.Substring(current_type.Length + 1);
                }

                // Was this a constructor though?
                if ((nodeType == Code_Node_Type_Enum.Single_Overload) || (nodeType == Code_Node_Type_Enum.Single_Method))
                {
                    if (new_file_name.IndexOf("_ctor") == 0)
                    {
                        if (nodeType == Code_Node_Type_Enum.Single_Method)
                        {
                            new_file_name = new_file_name.Replace("_ctor", "constructor");
                            int constructor_index = node.Title.IndexOf("Constructor");
                            if (constructor_index > 0)
                                node.New_Title = node.Title.Substring(constructor_index);
                        }
                        else
                        {
                            new_file_name = new_file_name.Replace("_ctor", "constructor_overload");
                            node.New_Title = "Constructors";
                        }
                        subdir = String.Empty;
                        nodeType = Code_Node_Type_Enum.Constructor;

                    }
                }

                // Save this node type
                node.Node_Type = nodeType;

                
                string new_directory = directory + "\\" + current_type + "\\" + subdir;
                if (!System.IO.Directory.Exists(new_directory))
                    System.IO.Directory.CreateDirectory(new_directory);
                string new_file = new_directory + new_file_name + "l";
                if (defaultFolderFile)
                    new_file = new_directory + "default.html";
                if (nodeType == Code_Node_Type_Enum.All_Members)
                    new_file = new_directory + "all_members.html";

                string current_url = "html/" + original_file;
                string new_url = new_file.Replace(destination_directory + "\\", "").Replace("\\", "/").Replace("/default.html", "").Replace(".html", "");
                node.New_URL = sitemap_url_base + new_url;

                string new_complete_url = new_url_base + new_url + new_url_end;
                link_replacements.Add(current_url, new_complete_url);


                System.IO.File.Copy(source + "\\html\\" + original_file, new_file, true);

            }

            if (node.Child_Nodes_Count > 0)
            {
                foreach (Sandcastle_TOC_Node childNode in node.Child_Nodes)
                {
                    create_and_assign_folders_recursively(childNode, directory, current_namespace);
                }
            }
        }
    }
}
