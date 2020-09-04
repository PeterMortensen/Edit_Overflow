
/***************************************************************************
* Copyright (C) 2019 Peter Mortensen                                       *
* This file is part of Edit Overflow.                                      *
*                                                                          *
* Class: EditSummaryStyle  (and defines EditSummaryEnum)                   *
*                                                                          *
* Purpose:                                                                 *
*     Encapsulates knowledge of the edit summary style,                    *
*     including which one to use by default (including                     *
*     set at build time (building several versions                         *
*     of Edit Overflow)).                                                  *
*                                                                          *
****************************************************************************/


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{


    public enum EditSummaryEnum
    {
        //Note: For now, some GUI thing relies in the numbers...

        // For Quora and most Stack Exchange sites (but
        // not Stack Overflow) 
        standard = 0,


        // For Stack Overflow
        // Samples:
        //
        //   Active reading [<http://en.wikipedia.org/wiki/Windows_PowerShell>].
        Stack_Overflow = 1,


        // Old style (original in Edit Overflow, but no longer
        // used)
        oldStyle = 2


    } //enum EditSummaryEnum


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class EditSummaryStyle
    {

        EditSummaryEnum mCurrentStyle;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public EditSummaryStyle()
        {
            // Set default edit summary - for now at build 
            // time (perhaps later as configuration):
            

            // Conditional compilation for this is isolated 
            // here and only for setting a variable

            // Using conditional compilation for now (until
            // we can use configuration files, command-line
            // parameters instead, or some other method to
            // change to the revision summary style).
            //
            // Update 2019-06-03: The default is still by building,
            //                    but the user can now change it at
            //                    runtime, without the need to 
            //                    launch the other build variation.

            #if SOME_SPECIAL_COMPILATION_SYMBOL

                mCurrentStyle = EditSummaryEnum.standard;

                //string msg = "Hi there! Special configuration build of Edit Overflow!";
                //System.Windows.Forms.MessageBox.Show(
                //  msg, 
                //  EditorOverflowApplication.applicationName());

            #else
                mCurrentStyle = EditSummaryEnum.Stack_Overflow;
            #endif            
        }


        //For now, clients handles what to do depending on the current 
        //style, so this method should be changed to "private" if
        //functionality is moved into this class.
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public EditSummaryEnum getCurrentStyle()
        {
            return mCurrentStyle;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public void setCurrentStyle(EditSummaryEnum aCurrentStyle)
        {
            mCurrentStyle = aCurrentStyle;
        }


        ///****************************************************************************
        // *    <placeholder for header>                                              *
        // ****************************************************************************/
        //private void mapStyleToOutput() 
        //{
        //
        //}




    } //class EditSummaryStyle
     

}

