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

using System.Collections.Generic; //For List.


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{


    public enum codeFormattingsRegexEnum
    {
        // Currently an assumption of IDs being consecutive and starting at 1...
        //SQL = 1237,
        missingSpaceBeforeOpeningBracket = 1,


        //missingSpaceBeforeOpeningBracket
        //missingSpaceAfterColon
        //missingSpaceAfterComma
        //missingSpaceAroundEqualSign(),
        //missingSpaceAroundStringConcatenation
        //spaceBeforeComma
        //spaceBeforeColon
        //spaceBeforeParenthesis
        //spaceBeforeSemicomma
        //spaceAfterLeftParenthesis
        //missingSpaceAroundOperators(),
        //
        //Regex(),
        //
    }


    public struct codeCheckItemStruct
    {
        public int ID;
        public int groupID;
        public string regularExpression;
        public string explanation;
        public string explanation_cleaned; // Without the "&" for indicated
                                           // keyboard shorcuts.
    };


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class CodeFormattingCheck
    {
        private StringBuilder mScratchSB;

        private List<codeCheckItemStruct> mCodeCheckItems;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public CodeFormattingCheck()
        {
            mScratchSB = new StringBuilder(200);

            mCodeCheckItems = new List<codeCheckItemStruct>();

            // Set up the datastructures

            // Note: the "&"s are bleeding of some Windows Forms
            //       specific stuff (they are only relevant in
            //       that context)

            addCodeCheck(
              1, 1,
              missingSpaceBeforeOpeningBracketRegex(),
              "M&issing space before {");

            addCodeCheck(
              2, 2,
              missingSpaceAfterColonRegex(),
              "Missing space after &colon");

            addCodeCheck(
              3, 2,
              missingSpaceAfterCommaRegex(),
              "Missing space after co&mma");

            addCodeCheck(
              4, 3,
              missingSpaceAroundEqualSignRegex(),
              "Missing space around &equal sign");

            addCodeCheck(
              5, 3,
              missingSpaceAroundStringConcatenationRegex(),
              "Missing space around string concate&nation (by \"+\")");

            addCodeCheck(
              6, 4,
              spaceBeforeCommaRegex(),
              "&Space before comma");

            addCodeCheck(
              7, 4,
              spaceBeforeColonRegex(),
              "Space &before colon");

            addCodeCheck(
              8, 4,
              spaceBeforeParenthesisRegex(),
              "Space before right &parenthesis");

            addCodeCheck(
              9, 4,
              spaceBeforeSemicommaRegex(),
              "Space before semicolo&n");

            addCodeCheck(
              10, 5,
              spaceAfterLeftParenthesisRegex(),
              "Space after &left parenthesis");

            addCodeCheck(
              11, 5,
              missingSpaceAroundOperatorsRegex(),
              "Missing space around some operators");

        } //Constructor.


        /****************************************************************************
         *                                                                          *
         *    Helper function for the constructor (for setting up the               *
         *    datastructures that defines the regular expressions for               *
         *    source code check). It is also preparation for                        *
         *    (by configuration).                                                   *
         *                                                                          *
         ****************************************************************************/
        private void addCodeCheck(int anID,
                                  int aGroupID,
                                  string aRegularExpression,
                                  string anExplanation)
        {
            codeCheckItemStruct someItem;

            // It is an (internal) client error to pass a string that
            // is too short (for example, an empty string). Or in
            // other words, the sanitation should already have
            // taken place.
            //
            const int kMinimumExplanationLength = 10;
            int explanationLen = anExplanation.Length;
            if (explanationLen < kMinimumExplanationLength)
            {
                //Somewhat confusing (the exception type) - perhaps
                //throw our own exception instead?

                throw new
                  System.IndexOutOfRangeException(
                    "The explanation (" + anExplanation + ") is too short (" +
                    explanationLen + "), below the minimum of " +
                    kMinimumExplanationLength.ToString() + " characters.");
            }


            someItem.ID = anID;
            someItem.groupID = aGroupID;
            someItem.regularExpression = aRegularExpression;
            someItem.explanation = anExplanation;

            someItem.explanation_cleaned = anExplanation.Replace("&", "");

            mCodeCheckItems.Add(someItem);
        } //addCodeCheck()


        /****************************************************************************
         *                                                                          *
         *    Example of line that will match:                                      *
         *                                                                          *
         *      XXXXXX                                                              *
         *                                                                          *
         ****************************************************************************/
        public List<codeCheckItemStruct> getCodeCheckItems()
        {
            //No real encapsulation for now, but the client needs
            //to iterate through the items (for some Windows Forms
            //specific things).
            //
            return mCodeCheckItems;
        } //getCodeCheckItems()


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
        public string missingSpaceAroundEqualSignRegex()
        {
            string toAdd = @"\S\=|\=\S";

            return toAdd;
        } //missingSpaceAroundEqualSignRegex()


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
        public string missingSpaceAroundOperatorsRegex()
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
                // But we go with a positive list: string literals by quotes,
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
                //
                @"\.['\""\]]" + "" +     // Missing space to the right
                                         // of "." (unless a number)

                "";

            return toAdd;
        } //missingSpaceAroundOperatorsRegex()


        /****************************************************************************
         *                                                                          *
         *    Returns a directly usable regular expression for source               *
         *    code checking. To get a user-oriented explanation of                  *
         *    the different parts of the regular expression, use                    *
         *    function combinedAllOfExplanations().                                 *
         *                                                                          *
         ****************************************************************************/
        public string combinedAllOfRegularExpressions()
        {
            mScratchSB.Length = 0;

            //The clipboard (in Windows) is way too unstable to use!!! But this
            //could be used as a test case when it is going to be fixed...
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

                mScratchSB.Append(missingSpaceAroundEqualSignRegex());
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

                mScratchSB.Append(missingSpaceAroundOperatorsRegex());
            }

            mScratchSB.Append(")");

            return mScratchSB.ToString();
        } //combinedAllOfRegularExpressions()


        /****************************************************************************
         *                                                                          *
         *    In particular: static/stateless...                                    *
         *                                                                          *
         ****************************************************************************/
        private static string combinedAllOfExplanations_internal(
            List<codeCheckItemStruct> aCodeCheckItems)
        {
            StringBuilder scratchSB = new StringBuilder(200);


            //Note: Some of this (outputting a list of items as an English text
            //      list with an Oxford comma) is not specific to this class/
            //      place. Perhaps move that part to a utility class?
            //      That would also make it easier to unit test.
            //
            //      The first step could be to split this into
            //      two loops, one for the filtering and one
            //      for the formatting.

            scratchSB.Append("(");

            int len = aCodeCheckItems.Count;
            int lastIndex = len - 1;
            for (int i = 0; i < len; i++)
            {
                codeCheckItemStruct someItem = aCodeCheckItems[i];

                string explanation = someItem.explanation_cleaned;

                // Make the first letter is not capitalised (it is for a
                // normal text list)
                string explanation_lower =
                    char.ToLower(explanation[0]) + explanation.Substring(1);

                scratchSB.Append(@"""");
                scratchSB.Append(explanation_lower);
                scratchSB.Append(@"""");

                // No trailing comma for the list
                if ((i != lastIndex) &&
                    len >= 3
                   )
                {
                    // Only lists with three or more items have
                    // commas - covers an Oxford comma...
                    //
                    scratchSB.Append(@", ");
                }

                // After second to last (before the last item)
                if (i == lastIndex - 1) // Will not work if the list is too short
                                        // In any case, the Oxford comma is only
                                        // for three or more elements...
                {
                    scratchSB.Append(@"and ");
                }


            } //Through code check items (generating of the explanation string
              //for the code formatting regular expressions)

            scratchSB.Append(")");

            //return "XYZ"; // Stub
            return scratchSB.ToString();
        } //combinedAllOfExplanations_internal()

//public int ID;
//public int groupID;
//public string regularExpression;
//public string explanation;


        /****************************************************************************
         *                                                                          *
         *    Returns a XXXX                                                        *
         *                                                                          *
         ****************************************************************************/
        public string combinedAllOfExplanations()
        {
            return CodeFormattingCheck.combinedAllOfExplanations_internal(mCodeCheckItems);
        } //combinedAllOfRegularExpressions()


    } //class CodeFormattingCheck


} //namespace OverflowHelper.core


