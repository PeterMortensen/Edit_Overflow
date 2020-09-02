/***************************************************************************
*                                                                          *
* Copyright (C) 2020 Peter Mortensen                                       *
*                                                                          *
* This file is part of Overflow Helper.                                    *
*                                                                          *
*                                                                          *
* Purpose: The initial purpose is extraction of the part for the regular   *
*          expressions for code checking from the Windows-specific part    *
*          (so it can also be used on Linux - in the first instance for    *
*          use on the published web page with the word list).              *
*                                                                          *
*          In the first instance it doesn't check anything - it just       *
*          knows about the regular expressions that a user can use         *
*          for rudimentary checking for source code (with the              *
*          associated high rate of false positives that come               *
*          with regular expressions).                                      *
*                                                                          *
*          Or in other words: This class does not (currently) know         *
*          anything about the data it is storing... It is mostly           *
*          just a container for data.                                      *
*                                                                          *
*          It also encapsulates the string builder so the client does not  *
*          have to provide it.                                             *
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
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string missingSpaceBeforeOpeningBracketRegex()
        {
            string toAdd = @"\S\{";

            return toAdd;
        } //missingSpaceBeforeOpeningBracketRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string missingSpaceAfterColonRegex()
        {
            string toAdd = @":\S";

            return toAdd;
        } //spaceAfterColonRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string missingSpaceAfterCommaRegex()
        {
            string toAdd = @",\S";

            return toAdd;
        } //spaceAfterCommaRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      "auto p=new Son();"                                                 *
         *                                                                          *
         ****************************************************************************/
        public string missingSpaceAroundEqualSign()
        {
            string toAdd = @"\S\=|\=\S";

            return toAdd;
        } //missingSpaceAroundEqualSign()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string missingSpaceAroundStringConcatenationRegex()
        {
            string toAdd = @"\S\+|\+\S";

            return toAdd;
        } //spaceAroundStringConcatenationRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string spaceBeforeCommaRegex()
        {
            string toAdd = @"\s,";

            return toAdd;
        } //spaceBeforeCommaRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string spaceBeforeColonRegex()
        {
            string toAdd = @"\s:";

            return toAdd;
        } //spaceBeforeColonRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string spaceBeforeParenthesisRegex()
        {
            string toAdd = @"\s\)";

            return toAdd;
        } //spaceBeforeParenthesisRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string spaceBeforeSemicommaRegex()
        {
            string toAdd = @"\s;";

            return toAdd;
        } //spaceBeforeSemicommaRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string spaceAfterLeftParenthesisRegex()
        {
            string toAdd = @"\(\s";

            return toAdd;
        } //spaceAfterLeftParenthesisRegex()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public string missingSpaceAroundOperators()
        {
            // Meta: Escaping double quotes in string:
            //
            //         <https://stackoverflow.com/questions/14480724>


            // That is, around operators:
            //
            //   &&    Logical AND
            //    .    String concatenation in e.g. Perl and PHP

            // Note: No "|" after the last term
            //
            string toAdd =

                // Logical AND

                @"\S&&" + "|" + @"&&\S" + "|" +


                // String concatenation in e.g. Perl and PHP
                //
                // One possible strategy is a negative list (exclude
                // space and numbers):
                //
                //     [^ \d]
                //
                // But "." is used in many places, e.g. "using System.Text;"
                //
                // But we go with  a positive list: string literals by quotes,
                // variables with "$", and array indexing ("[]")
                //
                // Note: A double double quote is for escaping
                //       a double quote in C#...
                //
                @"('|\""|(\$\w+\[.+\]))\."  + "|" +

                    // Missing space to the left of "." (unless a number
                    // or array indexing). However, we have a hard time
                    // distingushing something like this in PowerShell,
                    //
                    //    $m.Groups[2].Value
                    //
                    // from this in PHP:
                    //
                    //    $ARGV[0]." "
                    //
                    // For now, we require simple variables starting
                    // with "$" - thus we risk false negatives (and
                    // can probably still get false positives in
                    // PowerShell in some cases).


                @"\.['\""\]]" + "" +     // Missing space to the right
                                         // of "." (unless a number)

                "";

            return toAdd;
        } //missingSpaceAroundOperators()


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
                mScratchSB.Append("|");

                mScratchSB.Append(missingSpaceAroundOperators());
            }

            mScratchSB.Append(")");

            return mScratchSB.ToString();
        } //combinedAllOfRegularExpressions()


    } //class CodeFormattingCheck


} //namespace OverflowHelper.core


