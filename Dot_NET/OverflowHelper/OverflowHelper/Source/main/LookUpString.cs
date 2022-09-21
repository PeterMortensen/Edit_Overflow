/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: encapsulates details about a lookup string, e.g. it keeps       *
 *          track of leading and trailing white space (that may have        *
 *          to be preserved in the output/to the user).                     *
 *                                                                          *
 ****************************************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


namespace OverflowHelper.core
{


    public class LookUpString
    {
        string mCenterString;

        string mLeadingWhiteSpace;
        string mTrailingWhiteSpace;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public LookUpString(string aRawString)
        {
            update(aRawString);
        } //Constructor.


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void update(string aRawString)
        {
            mLeadingWhiteSpace = null;
            mTrailingWhiteSpace = null;

            int startIndex = 0;

            // Map leading characters that we should ignore in the lookup.
            //
            int sLen = aRawString.Length;
            int lastIndex = sLen - 1;
            while (startIndex < sLen &&
                    (aRawString[startIndex] == ' ' ||
                     aRawString[startIndex] == '"' ||
                     aRawString[startIndex] == '*'
                     )
                  ) // Relying on short-circuit
                                                    // boolean...
            {
                startIndex++;
            }

            //Note: we actually crash below when aRawString is empty -
            //      endIdx becomes -1.

            int endIdx = lastIndex;

            //To cover Ctrl + Right Arrow selection in some browsers, for example, Firefox.
            //Sample:
            //  "Stack Overflow, "
            //
            //while (aRawString[endIdx] == ' ' && endIdx > startIndex)


            //PM_CSHARP_BROKEN 2011-07-18
            //4th line below.


            // Map trailing characters that we should ignore in the lookup.
            //
            //Can we avoid all these lookups (it is also redundant)?
            while (
                endIdx > startIndex && // Must be first to avoid exception
                                       // for an empty string...
                (aRawString[endIdx] < 'A' || aRawString[endIdx] > 'Z') &&
                (aRawString[endIdx] < 'a' || aRawString[endIdx] > 'z') &&
                (aRawString[endIdx] < '0' || aRawString[endIdx] > '9') &&

                // For "C#"...
                // Why not just inequality?
                (aRawString[endIdx] < '#' || aRawString[endIdx] > '#') &&

                // For "C++" "g+", "Google+", "Dev-C++", "Visual C++",
                // "ms vc++", etc.
                // Why not just inequality?
                (aRawString[endIdx] < '+' || aRawString[endIdx] > '+') &&

                // For words in the alternative word set (we use
                // the convention of a trailing underscore)
                (aRawString[endIdx] != '_') &&

                true
              )
            {
                endIdx--;
            }

            int len = endIdx - startIndex + 1;
            mCenterString = aRawString.Substring(startIndex, len);

            mLeadingWhiteSpace = aRawString.Substring(0, startIndex);

            int trailingLen = lastIndex - endIdx;
            mTrailingWhiteSpace = aRawString.Substring(endIdx + 1, trailingLen);

            //mCenterString = aRawString;
        } //update().


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string getCoreString()
        {
            return mCenterString;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string getLeading()
        {
            return mLeadingWhiteSpace;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string getTrailing()
        {
            return mTrailingWhiteSpace;
        }


    } //class LookUpString


} //namespace OverflowHelper.core


