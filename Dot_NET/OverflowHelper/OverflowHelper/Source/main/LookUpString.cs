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

        //For now: A state. But it should probably be required
        //         of clients to explicitly provide it
        bool mStripSomeLeadingAndCharacters;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public LookUpString(string aRawString)
        {
            mStripSomeLeadingAndCharacters = true;

            update(aRawString, mStripSomeLeadingAndCharacters);
        } //Constructor.


        /****************************************************************************
         *    Another constructor                                                           *
         ****************************************************************************/
        public LookUpString(string aRawString,
                            bool aStripSomeLeadingAndTrailingCharacters)
        {
            mStripSomeLeadingAndCharacters = aStripSomeLeadingAndTrailingCharacters;

            update(aRawString, mStripSomeLeadingAndCharacters);
        } //Constructor.


        /****************************************************************************
         *                                                                          *
         *  Parameters:                                                             *
         *                                                                          *
         *    aStripSomeLeadingAndTrailingCharacters:                               *
         *                                                                          *
         *      Note: Even if it is false, we will still filter out                 *
         *            characters that will never be part of a word,                 *
         *            for example, leading and trailing spaces.                     *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        private void update(string aRawString,
                            bool aStripSomeLeadingAndTrailingCharacters)
        {
            // Configuration
            //
            // 'false is the old behaviour
            //
            // 'true': We have reinstated a minimim amount of
            //         automatic filtering of leading and
            //         trailing characters, in particular
            //         space, even if it has otherwise
            //         been turned off
            //         (aStripSomeLeadingAndTrailingCharacters
            //         is false).
            //
            const bool kFilterSomeCharacters = true;

            bool filterSomeCharacters = kFilterSomeCharacters;
            if (aStripSomeLeadingAndTrailingCharacters)
            {
                // As it is taken to mean: filter a
                // lot of characters
                filterSomeCharacters = true;
            }

            mLeadingWhiteSpace = null;
            mTrailingWhiteSpace = null;

            int startIndex = 0;

            // Map leading characters that we should ignore in the lookup.
            //
            int sLen = aRawString.Length;
            if (sLen > 1  // Don't strip a one-character word. Both '"'"
                          // and "*" are valid lookup words (not Markdown
                          // formatting in that context)
               )
            {
                bool done = false;
                while (!done)
                {
                    if (startIndex < sLen)
                    {
                        bool filter = false;
                        if (aStripSomeLeadingAndTrailingCharacters)
                        {
                            if (
                                aRawString[startIndex] == '"' ||
                                aRawString[startIndex] == '*' ||
                                false
                            )
                            {
                                filter = true;
                            }
                        }

                        if (filterSomeCharacters)
                        {
                            if (
                              aRawString[startIndex] == ' ' || // Space
                              false
                            )
                            {
                                filter = true;
                            }
                        }
                        if (filter)
                        {
                            startIndex++;
                        }
                        else
                        {
                            done = true;
                        }
                    }
                    else
                    {
                        done = true;
                    }
                } // Scanning for one or more leading
                  // characters to filter out
            } // Short input

            //Note: we actually crash below when aRawString is empty -
            //      endIdx becomes -1.

            int lastIndex = sLen - 1; // Inclusive
            int endIdx = lastIndex;

            // Map trailing characters that we should ignore in the lookup.
            //
            // To cover Ctrl + Right Arrow selection in some browsers,
            // for example, Firefox.
            //
            // Sample:
            //   "Stack Overflow, "
            //
            // while (aRawString[endIdx] == ' ' && endIdx > startIndex)
            //
            //Can we avoid all these lookups (they are also redundant)?
            bool doneTrailing = false;
            while (!doneTrailing)
            {
                if (endIdx > startIndex)
                {
                    if
                    (
                      (
                        // Bypass for when aStripSomeLeadingAndTrailingCharacters
                        // is true. It works because space is covered by the
                        // conditions below.
                        aStripSomeLeadingAndTrailingCharacters
                        ||
                        (
                          filterSomeCharacters
                          //(filterSomeCharacters &&
                          // !aStripSomeLeadingAndTrailingCharacters)

                          &&
                          (
                            aRawString[endIdx] == ' ' || // Space
                            false
                          )
                        )
                      ) &&
                      (
                        // Bypass, so we can still filter some characters,
                        // even if aStripSomeLeadingAndTrailingCharacters
                        // is false
                        //
                        //(filterSomeCharacters &&
                        // !aStripSomeLeadingAndTrailingCharacters)
                        !aStripSomeLeadingAndTrailingCharacters

                        ||
                        (
                          //aStripSomeLeadingAndTrailingCharacters &&
                          (

                            (aRawString[endIdx] < 'A' || aRawString[endIdx] > 'Z') &&
                            (aRawString[endIdx] < 'a' || aRawString[endIdx] > 'z') &&
                            (aRawString[endIdx] < '0' || aRawString[endIdx] > '9') &&

                            // For "C#"...
                            (aRawString[endIdx] != '#') &&

                            // For "C++" "g+", "Google+", "Dev-C++", "Visual C++",
                            // "ms vc++", etc.
                            (aRawString[endIdx] != '+') &&


                            //  T H I S   I S   G E T T I N G   O U T   O F   H A N D . . .
                            //
                            //Perhaps use a positive list for the punctuation instead?
                            //The number of possible characters should be much smaller.
                            //
                            //Perhaps make exceptions for specific words in the
                            //word set? As the more general exceptions we add,
                            //the more we weaken the general ability to directly
                            //look up words that have leading and trailing space,
                            //punctuation, etc.
                            //
                            // Note: Not added, as they appear as one character
                            //       (as incorrect terms (really expansions))
                            //       and work for some reason:
                            //
                            //          %
                            //          ~

                            // Double quotes. Sample: "8 "Jessie""
                            (aRawString[endIdx] != '\"') &&

                            // Single quotes. Sample: "'like button'"
                            (aRawString[endIdx] != '\'') &&

                            // Real apostrophe. Sample: "I’"
                            (aRawString[endIdx] != '’') &&

                            // For words in the alternative word set (we use
                            // the convention of a trailing underscore)
                            (aRawString[endIdx] != '_') &&

                            // Sample: "Mac&nbsp;OS&nbsp;X&nbsp;v10.2 (Jaguar)"
                            (aRawString[endIdx] != ')') &&

                            // Sample: "&mdash;"
                            (aRawString[endIdx] != ';') &&

                            // Sample: "Hello, World!"
                            (aRawString[endIdx] != '!') &&

                            // Though it may defeat the purpose for seamlessly
                            // handling formatted text copied in as Markdown
                            // italics or bold...
                            //
                            // Sample: "Sagittarius A*"
                            (aRawString[endIdx] != '*') &&

                            // Sample: "[sic]"
                            (aRawString[endIdx] != ']') &&

                            // Sample: "`man bash`"
                            (aRawString[endIdx] != '`') &&

                            // Sample: "How Is Babby Formed?"
                            (aRawString[endIdx] != '?') &&

                            // Sample: "!=="
                            (aRawString[endIdx] != '=') &&

                            // Sample: "polkadot{.js}"
                            (aRawString[endIdx] != '}') &&

                            // Sample: "/e/"
                            (aRawString[endIdx] != '/') &&

                            // Sample: "--"
                            (aRawString[endIdx] != '-') &&

                            // Sample: "M$"
                            (aRawString[endIdx] != '$') &&

                            // Sample: "<br>"
                            (aRawString[endIdx] != '>') &&

                            // Sample: "C♯"
                            (aRawString[endIdx] != '♯') &&

                            // Sample: "2¢"
                            (aRawString[endIdx] != '¢') &&

                            // Sample: "voilà"
                            (aRawString[endIdx] != 'à') &&

                            // Sample: "Bogotá"
                            (aRawString[endIdx] != 'á') &&

                            // Sample: "Antonio Radić"
                            (aRawString[endIdx] != 'ć') &&

                            // Sample: "fiancé"
                            (aRawString[endIdx] != 'é') &&

                            // Sample: "Baháʼí"
                            (aRawString[endIdx] != 'í') &&

                            // Sample: "Stack Overflow на русском"
                            (aRawString[endIdx] != 'м') &&

                            // Sample: "Malmö"
                            (aRawString[endIdx] != 'ö') &&

                            // Sample: "Perú"
                            (aRawString[endIdx] != 'ú') &&

                            // U+0131 (LATIN SMALL LETTER DOTLESS I). E.g.,
                            // from Turkish systems.
                            //
                            // Sample: "Hı". Though it is hidden in "hı_"
                            (aRawString[endIdx] != 'ı') &&

                            (aRawString[endIdx] != '€') &&

                            //(aRawString[endIdx] != 'Ω') &&
                            true
                          )
                        )
                      )

                    )
                    {
                        endIdx--;
                    }
                    else
                    {
                        doneTrailing = true;
                    }
                }
                else
                {
                    doneTrailing = true;
                }
            } // Scanning for one or more trailing
              // characters to filter out

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


