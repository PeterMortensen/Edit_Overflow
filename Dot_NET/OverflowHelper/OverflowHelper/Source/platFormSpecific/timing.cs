/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of OverflowHelper.                                     *
 *                                                                          *
 *                                                                          *
 * Purpose: Takes care of timing and jobs. E.g. to help to                  *
 *          space edits in time by a minimum amount of time.                *
 *                                                                          *
 ****************************************************************************/

/****************************************************************************
 *                       Mortensen Consulting                               *
 *                         Peter Mortensen                                  *
 *                E-mail: NUKESPAMMERSdrmortensen@get2netZZZZZZ.dk          *
 *                 WWW: http://www.cebi.sdu.dk/                             *
 *                                                                          *
 *  Program for Wiki editing.                                               *
 *                                                                          *
 *    FILENAME:   timing.cs                                                 *
 *    TYPE:       CSHARP                                                    *
 *                                                                          *
 * CREATED: PM 2010-02-28   Vrs 1.0. Refactored from frmMainForm.cs         *
 * UPDATED: PM 2010-xx-xx                                                   *
 *                                                                          *
 *                                                                          *
 ****************************************************************************/


using System; //For EventArgs.
using System.IO; //For File.


//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.platFormSpecific
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    class timing
    {
        //const int EDITCOUNTDOWN = 4;
        //const int EDITCOUNTDOWN = 15;
        //const int EDITCOUNTDOWN = 35 * 60;
        //const int DEFAULT_EDITCOUNTDOWN = 70 * 60; //1 h 10 min.
        const int DEFAULT_EDITCOUNTDOWN = 30 * 60; //As of 2010-10-30 the Stack Overflow 
                                                   //question list depth is kept 
                                                   //constant at 30 minutes.

        //const int TIMERINTERVAL_SECS = 1 * 60;
        //const int TIMERINTERVAL_SECS = 15;
        const int TIMERINTERVAL_SECS = 1;


        private int mEditCountdown;


        private System.Windows.Forms.Timer mTimer;

        private System.Media.SoundPlayer mPlayer;
        
        //For now: unconditional every XX minutes.
        private int mCountDown;


        System.Windows.Forms.Label mLabelForTimeOutput;


        //

        const int kDividerForHDD_KeepAlive = 25;

        //const string kFolderToPeakInForHDD_KeepAlive = "O:\\temp2";
        //const string<> kFoldersToPeakInForHDD_KeepAlive = new string[] { "O:\\temp2", "W:\\temp2" };


        //Currently used for .mp3 files only (in accessA_fileInAFolder())... 
        private static readonly string[] kFoldersToPeakInForHDD_KeepAlive = { 
            "O:\\temp2", 
            "W:\\DriveO\\temp2",
            "T:\\toDelete" // But we actually don't have any .mp3 files there...
            };
        
        private int mDividerForHDD_KeepAlive;
        const int kBUfferSize = 1024;


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public timing(System.Windows.Forms.Label anInLabelForTimeOutput)
        {
            mLabelForTimeOutput = anInLabelForTimeOutput;

            //See e.g.: 
            //http://msdn.microsoft.com/en-us/library/system.media.soundplayer.playsync%28printer%29.aspx
            //  SoundPlayer..::.PlaySync Method
            //
            //  "If the .wav file has not been specified or it fails to 
            //  load, the PlaySync method will play the default beep sound."

            mPlayer = new System.Media.SoundPlayer();          
            mPlayer.PlaySync();

            mEditCountdown = DEFAULT_EDITCOUNTDOWN;

            mCountDown = mEditCountdown;
            
            mDividerForHDD_KeepAlive = kDividerForHDD_KeepAlive;
        } //Constructor


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public void init2()
        {
            mTimer = new System.Windows.Forms.Timer();
            mTimer.Interval = TIMERINTERVAL_SECS * 1000;

            mTimer.Tick += new EventHandler(MyTimer_Tick);
            mTimer.Start();
        } //init2()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void accessA_fileInAFolder(string anInFolder_fullPath)
        {
            string[] filePaths =
                Directory.GetFiles(anInFolder_fullPath,
                                   "*.mp3",
                                   SearchOption.TopDirectoryOnly);
            int files = filePaths.GetLength(0);

            if (files>0)
            {
                Random r = new Random();
                int randomFilesIndex = r.Next(0, files);

                string fileToPeekAt = filePaths[randomFilesIndex];
                FileInfo someFileInfo = new FileInfo(fileToPeekAt);
                long sizeInBytes = someFileInfo.Length;

                //Actually, it would be better to try again a few times if
                //we don't find a sufficiently large file.

                if (sizeInBytes > 1000000)
                {
                    long randomFilePosition =
                        (int)(r.Next(0, (int)(sizeInBytes - 100 - kBUfferSize)));

                    try
                    {
                        if (System.IO.File.Exists(fileToPeekAt)) // This actually
                        // sometimes becomes false.
                        // E.g. if the drive is 
                        // disconnected (say a 
                        // USB drive).
                        {
                            FileStream currentFileStream =
                              new FileStream(
                                fileToPeekAt,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.ReadWrite,
                                65356,
                                FileOptions.RandomAccess);

                            byte[] currentBuffer = new byte[kBUfferSize];

                            currentFileStream.Position = randomFilePosition;
                            currentFileStream.Read(currentBuffer, 0, kBUfferSize);
                            currentFileStream.Close();
                        }
                        else
                        {
                            int peter7 = 7;
                        }
                    }
                    catch (Exception exceptionObject)
                    {
                        //We ignore any exception (they actually happen as 
                        //the list returned by GetFiles() actually 
                        //sometimes become invalid if XXXX)
                        int peter8 = 8;
                    }
                    finally
                    {
                        //Clean up.
                        int peter9 = 9;
                    }
                } //If 
            } //Any files in folder
        } //accessA_fileInAFolder()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void keepHardDiskDrivesAlive()
        {
            foreach (string someFolder in kFoldersToPeakInForHDD_KeepAlive)
            {                
                Boolean folderExists =
                    System.IO.Directory.Exists(someFolder);
                if (folderExists)
                {
                    try
                    {
                        accessA_fileInAFolder(someFolder);
                    }
                    catch (Exception)
                    {
                        // Ignore any errors...
                        int peter2 = 2;
                    }
                } //Folder exists
            }
        } //keepHardDiskDrivesAlive()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void MyTimer_Tick(object aSender, EventArgs anEvent)
        {
            mCountDown--;

            double timeLeftSecs = TIMERINTERVAL_SECS * mCountDown;
            double timeLeftMins = timeLeftSecs * 0.016666666;


            //For now: direct write back to a user interface element provided 
            //         by the client. But this creates tight coupling. A better 
            //         way could be to send an event and let the client 
            //         handle it (if it choses to.)
            //
            ////lblTimeLeft.Text = timeLeftMins.ToString("0.00") + " secs";
            //lblTimeLeft.Text = timeLeftMins.ToString("0.00") + " mins";
            mLabelForTimeOutput.Text = timeLeftMins.ToString("0.00") + " mins";

            //Send event (e.g. to the main form) that something interesting 
            //happend. E.g. in order to update the screeen. 

            if (mCountDown == 0)
            {
                //mCountDown = EDITCOUNTDOWN; No, we no longer restart. Instead 
                //  wait for the user to reset timer.

                //mPlayer.PlaySync();
                //mPlayer.PlaySync();
                //mPlayer.PlaySync();
                //mPlayer.PlaySync();
                //mPlayer.PlaySync();

                //mPlayer.PlaySync();
                //mPlayer.PlaySync();
                //mPlayer.PlaySync();
                //mPlayer.PlaySync();
                //mPlayer.PlaySync();

                bool useSpeech = false;

                string speechLoc = "D:\\temp2\\MetteEditereMono.wav";
                if (File.Exists(speechLoc))
                {
                    try
                    {
                        //mPlayer.SoundLocation = "file://D/temp2/MetteEditere.wav";
                        mPlayer.SoundLocation = speechLoc;
                        //mPlayer.Load();
                        mPlayer.Play();
                        useSpeech = true;
                    }
                    catch (Exception exceptionObject)
                    {
                        //Use something else than speech to get attention.
                    }
                    finally
                    {
	                    //Clean up.
                    }
                }
                else
                {
                    //Use something else than speech to get attention.
                }

                if (!useSpeech)
                {
                    string msg = "Time is up!";
                    System.Windows.Forms.MessageBox.Show(
                      msg, 
                      EditorOverflowApplication.applicationName());
                }
            }

            if (mCountDown < 0)
            {
                //We are waiting for user reset...

                const int MINUS_LIMIT = -3000;
                if (mCountDown < MINUS_LIMIT) //Limit to about -1 hours
                {
                    mCountDown = MINUS_LIMIT;
                }
            }


            mDividerForHDD_KeepAlive--;
            if (mDividerForHDD_KeepAlive == 0)
            {
                keepHardDiskDrivesAlive();

                mDividerForHDD_KeepAlive = kDividerForHDD_KeepAlive;
            }

        } //MyTimer_Tick()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public void shortenTime()
        {
            mCountDown = (int)(mCountDown * 0.66) + 1; // 2/3

            //PM_REMEMBER_TIMING 2010-09-13
            mEditCountdown = mCountDown; //Use the new time as the value for the
                                         //reset. In this way there is no need for 
                                         //the user to adjust the timer every time 
                                         //if the question list time depth does not 
                                         //change significantly.
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public void lengthenTime()
        {
            mCountDown = (int)(mCountDown * 1.33) + 1; //  4/3, corresponding to 3/4

            //PM_REMEMBER_TIMING 2010-09-13
            mEditCountdown = mCountDown; //Use the new time as the value for the
            //reset. In this way there is no need for 
            //the user to adjust the timer every time 
            //if the question list time depth does not 
            //change significantly.
        }
                

        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public void resetTime()
        {
            mCountDown = mEditCountdown;
        }

        

    } //class timing


} //namespace OverflowHelper.platFormSpecific

