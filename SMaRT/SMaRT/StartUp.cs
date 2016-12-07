#region Using directives

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GemBox.Spreadsheet;
using SobekCM.Engine_Library;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.Database;
using SobekCM.Engine_Library.Settings;
using SobekCM.Management_Tool.Config;
using SobekCM.Management_Tool.Versioning;
using SobekCM_Resource_Database;

#endregion

namespace SobekCM.Management_Tool
{
	/// <summary> Start up for the entire application looks for a new version, and pulls the basic
	/// configuration from the database before launching the main form </summary>
	public class StartUp
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			// Set the Gembox spreadsheet license key
			SpreadsheetInfo.SetLicense("EDWF-ZKV9-D793-1D2A");

			Control.CheckForIllegalCrossThreadCalls = false;

			// Set the visual rendering
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Set some defaults for round buttons
			Round_Button.Inactive_Border_Color = Color.DarkGray;
			Round_Button.Inactive_Text_Color = Color.White;
			Round_Button.Inactive_Fill_Color = Color.DarkGray;
			Round_Button.Mouse_Down_Border_Color = Color.Gray;
			Round_Button.Mouse_Down_Text_Color = Color.White;
			Round_Button.Mouse_Down_Fill_Color = Color.Gray;
			Round_Button.Active_Border_Color = Color.FromArgb(25, 68, 141);
			Round_Button.Active_Fill_Color = Color.FromArgb(25, 68, 141);
			Round_Button.Active_Text_Color = Color.White;

			bool updating = false;


			// Create a version checker to see if this is the latest version
			VersionChecker versionChecker = new VersionChecker();
			if ( versionChecker.UpdateExists() )
			{
				// Later, we will determine if the update is mandatory or not here
				if ( versionChecker.UpdateMandatory() )
				{
					MessageBox.Show(@"There is a new version of this application which must be installed.  Please stand by as the installation software is launched.      ", "New Version Needed", 
						MessageBoxButtons.OK, MessageBoxIcon.Information);

					// Start the Setup files to update this application and then exit
					versionChecker.Update();
					updating = true;
				}
				else
				{
					// A non-mandatory update exists
					DialogResult update = MessageBox.Show("A newer version of this software is available.            \n\nWould you like to upgrade now?",
						"New Version Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question );
					if ( update.Equals(DialogResult.Yes ) )
					{					
						// Start the Setup files to update this application and then exit
						versionChecker.Update();
						updating = true;
					}
				}
			}

			// Only continue if this application is not being updated
			if (!updating)
			{
				// Now, if an error was encountered anywhere, show an error message
				if (versionChecker.Error)
					MessageBox.Show("An error was encountered while performing the routine Version check.              \n\n" +
						"Your application may not be the most recent version.", "Version Check Error", MessageBoxButtons.OK,
						MessageBoxIcon.Warning);

				// Look for the configuration file
				string config_file = AppDomain.CurrentDomain.BaseDirectory + "\\config\\sobekcm.config";
				if (!File.Exists(config_file))
				{
					SMaRT_Config_Edit_Form editConfig = new SMaRT_Config_Edit_Form();
				    DialogResult result = editConfig.ShowDialog();
                    if ((result == DialogResult.Cancel) || (!File.Exists((config_file))))
                        return;
				}

				// References the library settings to retrieve the informatoin from the configuration file and subsequently
				// from the SobekCM database
                Engine_Database.Connection_String = Engine_ApplicationCache_Gateway.Settings.Database_Connection.Connection_String;
                SobekCM_Item_Database.Connection_String = Engine_ApplicationCache_Gateway.Settings.Database_Connection.Connection_String;
                
                // Set the workflow and disposition types
                InstanceWide_Settings_Builder.Set_Workflow_And_Disposition_Types(Engine_ApplicationCache_Gateway.Settings, Engine_Database.All_WorkFlow_Types, Engine_Database.All_Possible_Disposition_Types);

                // Set the metadata types
                InstanceWide_Settings_Builder.Set_Metadata_Types(Engine_ApplicationCache_Gateway.Settings, Engine_Database.Get_Metadata_Fields(null));
   
                SobekCM.Resource_Object.Configuration.ResourceObjectSettings.MetadataConfig.Set_Default_Values();
                SobekCM.Resource_Object.Configuration.ResourceObjectSettings.MetadataConfig.Finalize_Metadata_Configuration();

				// Launch the main form
				Application.Run(new MainForm( ));
			}
		}
	}
}
