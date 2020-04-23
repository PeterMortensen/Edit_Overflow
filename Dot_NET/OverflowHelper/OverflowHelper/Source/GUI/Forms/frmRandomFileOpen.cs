

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO; //For File, Directory
using System.Diagnostics; //For Process


//Older versions of the default folder:
//
//   1. W:\DriveO\temp2
//
//



//****************************************************************************
//*    <placeholder for header>                                              *
//****************************************************************************
namespace OverflowHelper.Forms
{


    //****************************************************************************
    //*    <placeholder for header>                                              *
    //****************************************************************************
    public partial class frmRandomFileOpen : Form
    {

        //But what about the file extension in btnOpenRandomFile_Click()?????
        //Shouldn't "fileExtensionList" be built from these two?

        const string kVideoSet = 
          "*.mp4;*.m4v;*.ogv;*.wmv;*.mov;*.m4a;*.flv;*.3gp;*.avi";

        const string kAudioSet = 
          "*.mp3;*.m4a;*.mid;*.ogx;*.wav;*.wma";

        private EditorOverflowApplication mApplication;


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        public frmRandomFileOpen(EditorOverflowApplication anApplication)
        {
            InitializeComponent();

            mApplication = anApplication;
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void frmRandomFileOpen_Load(object aSender, EventArgs anEvent)
        {
            this.ActiveControl = btnOpenRandomFile;

            txtFileExtensionsFilter.Text = kVideoSet;
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void btnOpenRandomFile_Click(object aSender, EventArgs anEvent)
        {
            string folderWithContent = txtInputFolder.Text;
          
            List<string> fileExtensionList = new List<string>();

            //Old: hardcoded set...
            //fileExtensionList.Add("mp4");
            //fileExtensionList.Add("m4v");
            //fileExtensionList.Add("ogv");
            //fileExtensionList.Add("wmv");
            //fileExtensionList.Add("wma");
            //fileExtensionList.Add("mov");
            //fileExtensionList.Add("m4a");
            //fileExtensionList.Add("flv");

            string fileExtensionsSet = txtFileExtensionsFilter.Text;
            string[] fileExtensionItems = fileExtensionsSet.Split(';');

            //Isn't there a short way to do this (than explicit 
            //iteration)?
            foreach (var someItem in fileExtensionItems)
            {
                string someItem2 = someItem.Replace("*.", "");

                fileExtensionList.Add(someItem2);
            }

            Boolean folderExists = System.IO.Directory.Exists(folderWithContent);
            if (folderExists)
            {
                List<string> allFilePaths = new List<string>();
                
                foreach (string someFileExtension in fileExtensionList)
                {
                    string[] filePaths =
                        Directory.GetFiles(folderWithContent,
                                           "*." + someFileExtension,
                                           SearchOption.TopDirectoryOnly);
                    allFilePaths.AddRange(filePaths);
                } //Through file extension list
                
                int files = allFilePaths.Count;
                
                Random r = new Random();
                int randomFilesIndex = r.Next(0, files);

                string fileToOpen = allFilePaths[randomFilesIndex];
                FileInfo someFileInfo = new FileInfo(fileToOpen);
                long sizeInBytes = someFileInfo.Length;

                string forProcess = "\"" + fileToOpen + "\"";
                try
                {
                    if (sizeInBytes > 1000000)
                    {
                        Process myProcess3 = Process.Start(forProcess, "");
                        txtCurrentFile.Text=fileToOpen;
                    }
                }

                catch (Exception exceptionObject)
                {
                    //handleEventHandlerException( _
                    //  "Open Application Folder", "mnuOpenApplicationFolder", _
                    //  exceptionObject)

                    System.Windows.Forms.MessageBox.Show(
                      "Could not open " + fileToOpen + " (" + forProcess + ")...");
                }
                finally
                {
	                // Clean up.
                }
            } // Folder exists
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void btnCopyToClipboard_Click(object aSender, EventArgs anEvent)
        {
            string currentFile = txtCurrentFile.Text;
            mApplication.setClipboard3(currentFile);
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void btnUseAudioSet_Click(object aSender, EventArgs anEvent)
        {
            txtFileExtensionsFilter.Text = kAudioSet;
        } //btnUseAudioSet_Click


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void btnUseVideoSet_Click(object aSender, EventArgs anEvent)
        {
            txtFileExtensionsFilter.Text = kVideoSet;
        } //btnUseVideoSet_Click()


    } //class frmRandomFileOpen
    

} //namespace OverflowHelper.Forms


