/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of OverflowHelper.                                     *
 *                                                                          *
 *                                                                          *
 * Purpose: various network related stuff.                                  *
 *                                                                          *
 ****************************************************************************/


using System.Windows.Forms; //For Application.


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace OverflowHelper.core
{


    class network
    {


        /****************************************************************************
         *                                                                          *
         *    Misnomer? It seems we also use it to open files (by letting           *
         *    Windows do the default action based on file extension, e.g.           *
         *    .html (default web browser) or .au3 (AutoIt))                         *
         *                                                                          *
         ****************************************************************************/
        public static void openURL(string aURL)
        {
            System.Diagnostics.ProcessStartInfo psInfo =
              new System.Diagnostics.ProcessStartInfo();

            //PM_URLOPEN_AMBERSAND_BROKEN 2011-02-17
            //psInfo.Arguments = "/C start " + aURL;
            psInfo.Arguments = "/C start \"\" \"" + aURL + "\" "; //To make it work with ambersands in the URL. See 

            psInfo.FileName = "cmd.exe";
            //psInfo.WorkingDirectory =    not needed for now.

            psInfo.UseShellExecute = false;

            System.Diagnostics.Process myProcess =
                System.Diagnostics.Process.Start(psInfo);

            bool waitExit = true;
            if (waitExit)
            {
                myProcess.WaitForExit();
            }
        }


        //Later: move somewhere else.
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static void delayWithDoEvents(double aDelaysSeconds)
        {
            int DOEVENT_INTERVAL_MILLISECS = 500;
            double DOEVENT_INTERVAL_SECS = 0.001 * DOEVENT_INTERVAL_MILLISECS;

            int iterations = (int)(aDelaysSeconds / DOEVENT_INTERVAL_SECS);

            for (int i = 0; i < iterations; i++)
            {
                Application.DoEvents();

                System.Threading.Thread.Sleep(DOEVENT_INTERVAL_MILLISECS);

                Application.DoEvents();
                Application.DoEvents();
            } //for


        } //delayWithDoEvents()
        

    } // class network



} //namespace OverflowHelper.core



