/***************************************************************************
* Copyright (C) 2010 Peter Mortensen                                       *
* This file is part of Overflow Helper.                                    *
*                                                                          *
*                                                                          *
* Purpose: The initial purpose is extraction of the part for the regular   *
*          expressions for code checking from the Windows-specific part    *
*          (so it can also be used on Linux - in the first instance for    *
*          use on the published web page with the word list).              *
*                                                                          *
*          In the first instance it doesn't check anything - it just       *
*          knows about the regular expressions that we can use for         *
*          rudimentary checking for source code (with the associated       *
*          high rate of false positives that come with regular             *
*          expressions).                                                   *
*                                                                          *
*          It also encapsulates the string builder so the client does not  *
*          have to provide it.                                             *                                                                          *
*                                                                          *
*                                                                          *
*                                                                          *
****************************************************************************/


using System.Text; //For StringBuilder.
//using System.Diagnostics; //For Trace. And its Assert.



/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class CodeFormattingCheck
    {
        private StringBuilder mScratchSB;


        public CodeFormattingCheck()
        {
            mScratchSB = new StringBuilder(200);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string missingSpaceBeforeOpeningBracketRegex()
        {
            string toAdd = @"\S\{";

            return toAdd;
        } //missingSpaceBeforeOpeningBracketRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string missingSpaceAfterColonRegex()
        {
            string toAdd = @":\S";

            return toAdd;
        } //spaceAfterColonRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string missingSpaceAfterCommaRegex()
        {
            string toAdd = @",\S";

            return toAdd;
        } //spaceAfterCommaRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string missingSpaceAroundEqualSign()
        {
            string toAdd = @"\S\=|\=\S";

            return toAdd;
        } //missingSpaceAroundEqualSign()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string missingSpaceAroundStringConcatenationRegex()
        {
            string toAdd = @"\S\+|\+\S";

            return toAdd;
        } //spaceAroundStringConcatenationRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string spaceBeforeCommaRegex()
        {
            string toAdd = @"\s,";

            return toAdd;
        } //spaceBeforeCommaRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string spaceBeforeColonRegex()
        {
            string toAdd = @"\s:";

            return toAdd;
        } //spaceBeforeColonRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string spaceBeforeParenthesisRegex()
        {
            string toAdd = @"\s\)";

            return toAdd;
        } //spaceBeforeParenthesisRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string spaceBeforeSemicommaRegex()
        {
            string toAdd = @"\s;";

            return toAdd;
        } //spaceBeforeSemicommaRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string spaceAfterLeftParenthesisRegex()
        {
            string toAdd = @"\(\s";

            return toAdd;
        } //spaceAfterLeftParenthesisRegex()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string combinedAllOfRegularExpressions()
        {
            mScratchSB.Length = 0;

            //The clipboard is way too unstable to use!!! But this could be
            //used as a test case when it is going to be fixed...
            //
            //EditorOverflowApplication.setClipboard("(", false);
            //
            //spaceAfterComma(true);
            //EditorOverflowApplication.setClipboard("|");
            //spaceAroundStringConcatenation(true);
            //EditorOverflowApplication.setClipboard("|");
            //spaceAfterColon(true);
            //
            //EditorOverflowApplication.setClipboard(")", true);

            mScratchSB.Append("(");

            {
                mScratchSB.Append(missingSpaceBeforeOpeningBracketRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(missingSpaceAfterColonRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(missingSpaceAfterCommaRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(missingSpaceAroundEqualSign());
                mScratchSB.Append("|");

                mScratchSB.Append(missingSpaceAroundStringConcatenationRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(spaceBeforeCommaRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(spaceBeforeColonRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(spaceBeforeParenthesisRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(spaceBeforeSemicommaRegex());
                mScratchSB.Append("|");

                mScratchSB.Append(spaceAfterLeftParenthesisRegex());
            }

            mScratchSB.Append(")");

            return mScratchSB.ToString();
        } //combinedAllOfRegularExpressions()


    } //class WikipediaLookup


} //namespace OverflowHelper.core


