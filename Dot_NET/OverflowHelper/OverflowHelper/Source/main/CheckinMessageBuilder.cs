/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of OverflowHelper.                                     *
 *                                                                          *
 *                                                                          *
 * Purpose: building up checkin messages for the Stack Overflow edits.      *
 *                                                                          *
 ****************************************************************************/


//using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text; //For StringBuilder.

using System.Diagnostics; //For Trace. And its Assert.


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{

    struct correctedWord
    {
        public string word;
        public string URL;
    }
    

    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    class CheckinMessageBuilder
    {

        //private List<string> mWordList;
        private List<correctedWord> mWordList;
        
        private StringBuilder mScratchSB;

        private const string IMPROVEMENTSTR = 
          "(but there is still room for improvement.) ";

        private EditSummaryStyle mEditSummaryStyle;
                

        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public CheckinMessageBuilder(EditSummaryStyle anEditSummaryStyle)
        {
            mWordList = new List<correctedWord>();
            mScratchSB = new StringBuilder(200);

            mEditSummaryStyle = anEditSummaryStyle;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public void resetWords()
        {
            mWordList.Clear();
        }

        
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public void addWord(string aSomeWord, string aURL)
        {
            correctedWord someWord;

            someWord.word = aSomeWord;
            someWord.URL = aURL;                        

            mWordList.Add(someWord);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public int getNumberOfWords()
        {
            return mWordList.Count;
        }


        /****************************************************************************
         *                                                                          *
         *    aTextList:                                                            *
         *                                                                          *
         *      true: As a texual list, with an Oxford comma (if more than two      *
         *            elements)!. Example:                                          *
         *                                                                          *
         *              XXXXXX                                                      *
         *                                                                          *
         *      false: Currently interpreted as elements to be enclosed             *
         *             in "<>" and separated by space (as currently used            *
         *             for Stack Overflow). Example:                                *
         *                                                                          *
         *              XXXXXX                                                      *
         *                                                                          *
         ****************************************************************************/
        private static void formatLookupList(
            List<correctedWord> aWordList,
            string aPrefix, string aPostfix, bool aTextList, bool anAsURLs,
            StringBuilder anInOutScratchSB)
        {
            anInOutScratchSB.Append(aPrefix);

            int len = aWordList.Count;
            int lastIndex = len - 1;
            for (int i = 0; i < len; i++)
            {
                if (i != 0)
                {
                    if (aTextList)
                    {
                        if (i != lastIndex)
                        {
                            anInOutScratchSB.Append(", "); //Normal separator
                                                           //for three or more
                                                           //elements
                        }
                        else
                        {
                            if (len > 2) // Using ">=" is a logical error - candidate for unit test
                            {
                                // Oxford comma...
                                anInOutScratchSB.Append(",");
                            }
                            anInOutScratchSB.Append(" and ");
                        }
                    }
                    else
                    {
                        //We don't have any special consideration for
                        //two elements
                        anInOutScratchSB.Append(" ");
                    }
                }

                //Two steps to avoid implicit conversion 
                //Append()... (so e.g. we get "Cannot implicitly convert 
                //type 'correctedWord' to 'string'").
                string someWord = aWordList[i].word;
                string someURL = aWordList[i].URL;

                string someText = null;
                if (anAsURLs)
                {
                    someText = someURL;
                }
                else
                {
                    someText = someWord;
                }

                if (anAsURLs)
                {
                    anInOutScratchSB.Append("<");
                }

                anInOutScratchSB.Append(someText);

                if (anAsURLs)
                {
                    anInOutScratchSB.Append(">");
                }

            } //for
            anInOutScratchSB.Append(aPostfix);
        } //formatLookupList()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static void formatLookupListWithCurrentStyle(
            List<correctedWord> aWordList, StringBuilder aScratchSB,
            EditSummaryStyle aEditSummaryStyle)
        {
            EditSummaryEnum currentEditStyle = aEditSummaryStyle.getCurrentStyle();

            switch (currentEditStyle)
            {
                case EditSummaryEnum.standard:
                    formatLookupList(
                        aWordList,
                        "Copy edited (e.g. ref. ", "). ", true, true,
                        aScratchSB);
                    break;

                case EditSummaryEnum.Stack_Overflow:
                    formatLookupList(
                        aWordList,
                        "Active reading [", "].", false, true,
                        aScratchSB);
                    break;

                case EditSummaryEnum.oldStyle:
 
                    //Alternative: append directly to mScratchSB,
                    //             and use an empty prefix for
                    //             formatLookupList()
                    string prefix = "Added link";
                    int len = aWordList.Count;
                    if (len > 1)
                    {
                        prefix += "s";
                    }
                    prefix += " to ";

                    formatLookupList(
                        aWordList,
                        prefix, " [Wikipedia]. ", true, false,
                        aScratchSB);
                    break;

                default:
                    Trace.Assert(false, "Switch fall-through...");
                    break;

            }            
            
        } //formatLookupListWithCurrentStyle()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string getCheckinMessage(
            bool aHasCruft2, 
            bool aHasSignature, 
            bool aRoomForImprovement, 
            bool aSplitParagraph,
            bool aJeopardyCompliance,
            bool aHasBegging,
            bool aHasIndrectBegging,
            bool aHasUpperCasingTitle,
            bool aContentHiddenByHTMLtag, 
            bool aStackOverflowSpelling
            )
        {
            mScratchSB.Length = 0;

            if (aSplitParagraph)
            {
                mScratchSB.Append("Split the first paragraph into several (is debatable). ");
            }

            if (aJeopardyCompliance)
            {
                mScratchSB.Append("Jeopardy compliance. ");
            }

            if (aContentHiddenByHTMLtag)
            {
                // Unhid "<path-to-file>" by encoding "<" as "&lt;" (see the original revision (<http://stackoverflow.com/XXXXXXXX/view-source>).
            

                mScratchSB.Append(
                    "Unhid \"XXX\" by encoding \"<\" as \"&lt;\" ");
                mScratchSB.Append(
                    "(see the original revision ");
                mScratchSB.Append(
                    "(<http://stackoverflow.com/XXXXXXXX/view-source>)");
                mScratchSB.Append(
                    " - or use view \"side-by-side markdown\"). ");
            }

            if (aStackOverflowSpelling)
            {
                mScratchSB.Append( 
                    "Used the official name of Stack Overflow - see " +
                    "section \"Proper Use of the Stack Exchange Name\" in " +
                    "<http://stackoverflow.com/legal/trademark-guidance> " +
                    "(the last section). ");
            }

            int len = mWordList.Count;
            if (len > 0)
            {
                //2019-06-03
                formatLookupListWithCurrentStyle(mWordList, 
                                                 mScratchSB, 
                                                 mEditSummaryStyle);

                //No longer comme il faut...
                //mScratchSB.Append(
                //    "Minor edit: grammar/spelling/case/punctuation/etc. ");

                if (aRoomForImprovement)
                {
                    mScratchSB.Append(IMPROVEMENTSTR);
                }

                //PM_BEGGING_OPTIONS 2010-05-16
                if (aHasBegging)
                {
                    mScratchSB.Append("Removed begging. ");
                }
                if (aHasIndrectBegging)
                {
                    mScratchSB.Append("Removed (indirect) begging. ");
                }

                if (aHasCruft2)
                {
                    mScratchSB.Append(
                        "Removed cruft - see http://meta.stackoverflow.com/questions/30142/how-to-correct-trivial-mistakes-in-answers/30146#30146 ");
                }

                //See <http://meta.stackoverflow.com/questions/5029/are-taglines-signatures-disallowed/5038#5038>
                //and <http://meta.stackoverflow.com/questions/47433/monthly-summary-of-whats-new/47815#47815>.
                if (aHasSignature)
                {
                    mScratchSB.Append(
                        "Please don't use signatures or taglines in your posts. See <http://stackoverflow.com/faq>, \"Can I use a signature or tagline?\"");
                }

                if (aHasUpperCasingTitle)
                {
                    mScratchSB.Append(
                        "Changed to sentence casing for the title. ");
                }



            } //If Wikipedia links.
            return mScratchSB.ToString();
        } //getCheckinMessage()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string getMinorEditStr(
            bool aRoomForImprovement)
        {
            string toReturn = "Minor edit: grammar/spelling/case/punctuation/etc.";
            if (aRoomForImprovement)
            {
                toReturn = toReturn + IMPROVEMENTSTR;
            }
            return toReturn;
        } //getMinorEditStr()


    } //class CheckinMessageBuilder

} //OverflowHelper.core
