/****************************************************************************
 * Copyright (C) 2020 Peter Mortensen                                       *
 *                                                                          *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: Encapsulates the main function of Edit Overflow: looking        *
 *          up misspelled words                                             *
 *                                                                          *
 *          It is independent of the platform (can be used from             *
 *          Windows, Linux, etc.)                                           *
 *                                                                          *
 * Also: As part of the effort to separate the Windows GUI                  *
 *       (Windows) from the lookup (e.g. so it works on                     *
 *       Linux), we also use it for:                                        *
 *                                                                          *
 *         Holding instances of some other classes (reducing                *
 *         the complexity on the Windows GUI client side).                  *
 *         And providing call-through/facade functions.                     *
 *                                                                          *
 *                                                                          *
 ****************************************************************************/

/****************************************************************************
 *                       Mortensen Consulting                               *
 *                         Peter Mortensen                                  *
 *                E-mail: XXXXX@XXXXX                                       *
 *                                                                          *
 *  Program for XXXX                                                        *
 *                                                                          *
 *    FILENAME:   WordCorrector.cs                                          *
 *    TYPE:       CSHARP                                                    *
 *                                                                          *
 * CREATED: PM 2020-09-04   Vrs 1.0.                                        *
 * UPDATED: PM 2020-xx-xx                                                   *
 *                                                                          *
 *                                                                          *
 ****************************************************************************/


using System; //For Exception
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{

    public struct lookupResultStructure
    {
        public string lookUpFailureText;
        public string coreString;
        public string correctedText;
        public string WikipediaURL;
        public int URLcount; // For remembering the total number of
                             // lookups. The other fields are only
                             // for the last lookup. The mismatch
                             // comes from the refactoring of
                             // the lookup to separate the
                             // lookup from the Windows
                             // interface to enable it
                             // on Linux, etc.
    }; //struct lookupResuls


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    class WordCorrector
    {
        private EditSummaryStyle mEditSummaryStyle;

        private CheckinMessageBuilder mCheckinMessageBuilder;

        private TermLookup mTermLookup;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public WordCorrector()
        {
            try
            {
                mTermLookup = new TermLookup();

                mEditSummaryStyle = new EditSummaryStyle();
            }
            //Disabled for now (due to the cross-platform effort)
            //catch (Exception exceptionObject)
            catch
            {
                //Disabled for now (due to the cross-platform effort)
                //
                //Use something else than speech to get attention.
                //string msg =
                //    "Crash in constructor of TermLookup (or some other)";
                //
                //What should we do?
                //System.Windows.Forms.MessageBox.Show(msg);
            }
            finally
            {
                //Clean up.
            }

            //Must be after creation of mEditSummaryStyle...
            mCheckinMessageBuilder = new CheckinMessageBuilder(mEditSummaryStyle);
        } //Constructor


        /****************************************************************************
         *                                                                          *
         *  This function is independent of the (Windows) UI.                       *
         *                                                                          *
         ****************************************************************************/
        public lookupResultStructure lookup_Central(
            string aToLookUp,
            bool aGuessURL_ifFailedLookup,
            bool aStripSomeLeadingAndCharacters)
        {
            lookupResultStructure toReturn;

            toReturn.lookUpFailureText = string.Empty; // Default.
            toReturn.coreString = string.Empty;
            toReturn.correctedText = string.Empty; // Default. We use it as a flag.
            toReturn.WikipediaURL = string.Empty;
            toReturn.URLcount = -1;

            LookUpString tt2 = 
                new LookUpString(aToLookUp, aStripSomeLeadingAndCharacters);
            toReturn.coreString = tt2.getCoreString();

            string leading = tt2.getLeading();
            string trailing = tt2.getTrailing();

            string correctedWord;

            //Note about the variable name: It is not always
            //a Wikipedia URL... Some are for Wiktionary,
            //MSDN, etc.
            toReturn.WikipediaURL =
                mTermLookup.lookUp(
                    toReturn.coreString,
                    aGuessURL_ifFailedLookup,
                    out correctedWord);
            if (toReturn.WikipediaURL.Length > 0)
            {
                mCheckinMessageBuilder.addWord(correctedWord,
                                               toReturn.WikipediaURL);

                //In user output: include leading and trailing whitespace
                //from the input (so it works well with keyboard
                //combination Shift + Ctrl + right Arrow).
                toReturn.correctedText = leading + correctedWord + trailing;

                toReturn.URLcount = mCheckinMessageBuilder.getNumberOfWords();
            }
            else
            {
                toReturn.lookUpFailureText =
                    "Could not lookup " + toReturn.coreString + "!";
            }

            return toReturn;
        } //lookup_Central()


        /****************************************************************************
         *                                                                          *
         *  Only for the first step of the separation of lookup and                 *
         *  the Windows GUI (adaption)                                              *
         *                                                                          *
         ****************************************************************************/
        public EditSummaryStyle getEditSummaryStyle()
        {
            return mEditSummaryStyle;
        }


        /****************************************************************************
         *                                                                          *
         *  Only for the first step of the separation of lookup and                 *
         *  the Windows GUI (adaption)                                              *
         *                                                                          *
         ****************************************************************************/
        public CheckinMessageBuilder getCheckinMessageBuilder()
        {
            return mCheckinMessageBuilder;
        }


        /****************************************************************************
         *                                                                          *
         *  Only for the first step of the separation of lookup and                 *
         *  the Windows GUI (adaption)                                              *
         *                                                                          *
         ****************************************************************************/
        public TermLookup getTermLookup()
        {
            return mTermLookup;
        }


    } //class WordCorrector


} //namespace OverflowHelper.core


