/****************************************************************************
 * Copyright (C) 2017 Peter Mortensen                                       *
 * This file is part of OverflowHelper.                                     *
 *                                                                          *
 *                                                                          *
 * Purpose: Very thin layer for clipboard access. E.g. for:                 *
 *                                                                          *
 *            * Client convenience                                          *
 *            * Breakpoints                                                 *
 *            * Catch exceptions (in one place)).                           *
 *            * Appending instead of replacing                              *
 *            * Automatic safety check (to avoid exceptions, using          *
 *              very large data, etc.)                                      *
 *            * No surprise when a string to set is empty...                *
 *            * Use of "System.Windows.Clipboard.SetText()" instead         *
 *              of "System.Windows.Forms.Clipboard.SetText()".              *
 *                                                                          *
 *                                                                          *
 ****************************************************************************/


//using System;
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
    class clipboard
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string getText()
        {
            string toReturn = System.Windows.Forms.Clipboard.GetText();

            return toReturn;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static void setText(string aTextToSet)
        {
            if (false) //This block does not cause the resize error
            {

                //if (System.Windows.Forms.Clipboard.ContainsText(
                //        System.Windows.Forms.TextDataFormat.Text))
                //{
                //    int peter3 = 3;
                //}
                //else
                //{
                //    int peter7 = 7;
                //}


                // The system function throws an exception if the string is empty...
                //
                // E.g. ref.:
                //
                //   <https://msdn.microsoft.com/en-us/library/ydby206k.aspx>  
                //     Clipboard.SetText Method (String)
                //
                //     "ArgumentNullException text is null or Empty."
                //
                // <http://stackoverflow.com/questions/11952960/copy-empty-string-using-clipboard-settextstring/11952996#11952996>
                //
                //   ""
                //
                // Even more elaborate:
                //
                //   <http://stackoverflow.com/questions/899350/how-to-copy-the-contents-of-a-string-to-the-clipboard-in-c/899506#899506>
                //     How to copy the contents of a String to the clipboard in C#?
                //
                //     ... calling SetText ... quite a few gotchas ... thread ...  
                //     the STA ... COM timing issues in the clipboard. ... We use a
                //     centralized method ... instead of calling SetText directly. 
                //
                // <http://stackoverflow.com/questions/15171492/windows-7-clipboard-copy-paste-operation-using-c-sharp/15172131#15172131>
                //   Windows 7 clipboard copy paste operation using C#    
                //
                //   "Try adding the [STAThread] attribute above your main 
                //     function. See second-last answer in this Stack Overflow 
                //     question: stackoverflow.com/questions/518701"
                //
                //   Also:
                //     <http://stackoverflow.com/questions/20407114/strange-behaviour-with-clipboard-in-c-sharp-console-application/20407690#20407690>
                //       Strange behaviour with clipboard in C# console application
                //     
                //       Also mentions STA...:
                //     
                //           staThread.SetApartmentState(ApartmentState.STA);
                //     
                //     <http://stackoverflow.com/questions/518701/clipboard-gettext-returns-null-empty-string/2586334#2586334>
                //       Clipboard.GetText returns null (empty string)
                //     
                //       [STAThread]
                //       static void Main(string[] args)
                //       { (...)
            } // For block. For debugging.


            if (true)
            {
                //System.Windows.Forms.MessageBox.Show("Hello, everyone!");

                if (aTextToSet == "") 
                {

                    // Our workaround for the resize problem for now is to 
                    // not have the Clear() as active code (outcommented).
                    // However, the problem seems to have returned
                    // when the "System.Windows.Clipboard.SetText()" (below)
                    // reenabled.
                    // 
                    // Instead we pop a message box noticing the 
                    // non-standard behaviour.
                    //
                    System.Windows.Forms.MessageBox.Show(
                       "Empty string for clipboard. " + 
                       "But the clipboard was left untouched");

                    int peter2 = 2;


                    //This line causes the resize error! Even though 
                    //it is not executed!!!
                    //
                    //System.Windows.Clipboard.Clear(); 


                    int peter3 = 3;
                }
                else
                {            
                    //Note: both versions crash for an empty string.


                    //Note: When the Clear() line above is outcommented, 
                    //      then it is this line that causes the resize...
                    //
                    System.Windows.Clipboard.SetText(aTextToSet);



                    //The Windows Forms version
                    //System.Windows.Forms.Clipboard.SetText(aTextToSet);
                }


            } // For block. For debugging.

        } //setText()


    } //class Clipboard


} //namespace OverflowHelper.platFormSpecific


