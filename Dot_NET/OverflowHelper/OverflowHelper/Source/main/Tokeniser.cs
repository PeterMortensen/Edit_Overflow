/****************************************************************************
 * Copyright (C) 2018 Peter Mortensen                                       *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: Returns a stream of strings that are separated by the           *
 *          specified separator string (often a single character).          *
 *          CR-LFs are not returned (treated as a separator)                *
 *                                                                          *
 ****************************************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using System.Diagnostics; //For Trace.Assert
using System.Text; //For StringBuilder.


namespace OverflowHelper.core
{

    
    public class Tokeniser
    {
        string mRawString;
        string mSeparator;

        //string mRawString_upper;

        StringBuilder mScratchSB;
        int mStartIndex;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public Tokeniser(string aRawString, string aSeparator)
        {
            mRawString = aRawString;
            mSeparator = aSeparator;

            //mRawString_upper = aRawString.ToUpper();

            mScratchSB = new StringBuilder(200);

            resetRead();
        } //Constructor.


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void resetRead()
        {
            mStartIndex = 0;
            mScratchSB.Length = 0;
        }

    
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string getNextWord()
        {
            //Trace.Assert(
            //    mRawString.Length == mRawString_upper.Length,
            //    "someHTML and someUpperHTML does not have the same length. " +
            //    "In mnuFilterHTML_forYouTube_Click()");

            string toReturn = null;


            //Note: Bug - we don't consider CR/LF - the last word in a line
            //      and the first in the next line are returned as one word
            //      with a CR/LF in the middle...
            //
            if (mStartIndex < mRawString.Length)
            {
                int endIndex = mRawString.IndexOf(mSeparator, mStartIndex);

                bool end = false; //What is up with this? Its value is never used!

                if (endIndex == -1)
                {
                    endIndex = mRawString.Length;
                    end = true;
                }
                int len = endIndex - mStartIndex;

                toReturn = mRawString.Substring(mStartIndex, len);

                mStartIndex = endIndex + 1; // Jump over the " ".
            }

            return toReturn;
        } //getNextWord()


    } //class Tokeniser


}
