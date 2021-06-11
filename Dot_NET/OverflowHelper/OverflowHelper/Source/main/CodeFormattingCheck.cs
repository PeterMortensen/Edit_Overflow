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
        //missingSpaceBeforeOpeningBracket = 1237,
        missingSpaceBeforeOpeningBracket = 1,

        missingSpaceAfterColon,
        missingSpaceAfterComma,
        missingSpaceAroundEqualSign,
        missingSpaceAroundStringConcatenation,
        spaceBeforeComma,
        spaceBeforeColon,
        spaceBeforeParenthesis,
        spaceBeforeSemicomma,
        spaceAfterLeftParenthesis,
        missingSpaceAroundOperators
    }


    public struct codeCheckItemStruct
    {
        public codeFormattingsRegexEnum ID;
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
              codeFormattingsRegexEnum.missingSpaceBeforeOpeningBracket, 1,
              @"\S\{",
              "M&issing space before {");

            addCodeCheck(
              codeFormattingsRegexEnum.missingSpaceAfterColon, 2,
              @":\S",
              "Missing space after &colon");

            addCodeCheck(
              codeFormattingsRegexEnum.missingSpaceAfterComma, 2,
              @",\S",
              "Missing space after co&mma");

            addCodeCheck(
              codeFormattingsRegexEnum.missingSpaceAroundEqualSign, 3,
              @"\S\=|\=\S",
              "Missing space around &equal sign");

            addCodeCheck(
              codeFormattingsRegexEnum.missingSpaceAroundStringConcatenation, 3,
              @"\S\+|\+\S",
              "Missing space around string concate&nation (by \"+\")");

            addCodeCheck(
              codeFormattingsRegexEnum.spaceBeforeComma, 4,
              @"\s,",
              "&Space before comma");

            addCodeCheck(
              codeFormattingsRegexEnum.spaceBeforeColon, 4,
              @"\s:",
              "Space &before colon");

            addCodeCheck(
              codeFormattingsRegexEnum.spaceBeforeParenthesis, 4,
              @"\s\)",
              "Space before right &parenthesis");

            addCodeCheck(
              codeFormattingsRegexEnum.spaceBeforeSemicomma, 4,
              @"\s;",
              "Space before semicolo&n");

            addCodeCheck(
              codeFormattingsRegexEnum.spaceAfterLeftParenthesis, 5,
              @"\(\s",
              "Space after &left parenthesis");


            string missingSpaceAroundOperatorsStr =
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

            addCodeCheck(
              codeFormattingsRegexEnum.missingSpaceAroundOperators, 5,
              missingSpaceAroundOperatorsStr,
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
        private void addCodeCheck(codeFormattingsRegexEnum anID,
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
         *    Helper function for lookup in the main datastructure                  *
         *                                                                          *
         ****************************************************************************/
        public string getRegularExpression(codeFormattingsRegexEnum anID)
        {
            // Currently relying on a convention between the ID and the index...
            // At least the assumption is isolated here (except for use
            // of the whole list by clients (currently two instance)).
            //
            return mCodeCheckItems[(int)anID-1].regularExpression;
        } //getRegularExpression()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string missingSpaceBeforeOpeningBracketRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.missingSpaceBeforeOpeningBracket);
        } //missingSpaceBeforeOpeningBracketRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string missingSpaceAfterColonRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.missingSpaceAfterColon);
        } //spaceAfterColonRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string missingSpaceAfterCommaRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.missingSpaceAfterComma);
        } //spaceAfterCommaRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string missingSpaceAroundEqualSignRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundEqualSign);
        } //missingSpaceAroundEqualSignRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string missingSpaceAroundStringConcatenationRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundStringConcatenation);
        } //spaceAroundStringConcatenationRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string spaceBeforeCommaRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.spaceBeforeComma);
        } //spaceBeforeCommaRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string spaceBeforeColonRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.spaceBeforeColon);
        } //spaceBeforeColonRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string spaceBeforeParenthesisRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.spaceBeforeParenthesis);
        } //spaceBeforeParenthesisRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string spaceBeforeSemicommaRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.spaceBeforeSemicomma);
        } //spaceBeforeSemicommaRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string spaceAfterLeftParenthesisRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.spaceAfterLeftParenthesis);
        } //spaceAfterLeftParenthesisRegex()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private string missingSpaceAroundOperatorsRegex()
        {
            return getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundOperators);
        } //missingSpaceAroundOperatorsRegex()


        /****************************************************************************
         *                                                                          *
         *    In particular: static/stateless...                                    *
         *                                                                          *
         ****************************************************************************/
        private static string combinedAllOfRegularExpressions_internal(
            List<codeCheckItemStruct> aCodeCheckItems)
        {
            StringBuilder scratchSB = new StringBuilder(200);

            //There is probably a simpler way than an explicit loop...
            //
            // Prepare a list for stirng.Join()
            int len = aCodeCheckItems.Count;
            int lastIndex = len - 1;
            List<string> CodeRegExList = new List<string>(len);
            for (int i = 0; i < len; i++)
            {
                codeCheckItemStruct someItem = aCodeCheckItems[i];

                CodeRegExList.Add(someItem.regularExpression);
            }

            scratchSB.Append("(");
            scratchSB.Append(string.Join("|", CodeRegExList));
            scratchSB.Append(")");
            
            return scratchSB.ToString();
        } //combinedAllOfExplanations_internal()


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

            return combinedAllOfRegularExpressions_internal(mCodeCheckItems);
        } //combinedAllOfRegularExpressions()


        /****************************************************************************
         *                                                                          *
         *    In particular: static/stateless...                                    *
         *                                                                          *
         ****************************************************************************/
        private static string combinedAllOfExplanations_internal(
            List<codeCheckItemStruct> aCodeCheckItems)
        {

            //Isn't there a built-in function to format a list with a
            //separator string?? E.g. like Perl's join()? Yes, String.Join()
            //
            //What about System.Configuration.CommaDelimitedStringCollection?
            //
            // Some references:
            //
            //   <https://stackoverflow.com/questions/10540584>
            //     String.Join on a List of Objects
            //
            //   <https://stackoverflow.com/questions/10347455>
            //     string.Format with string.Join
            //
            //   <https://stackoverflow.com/questions/330493>
            //     Join collection of objects into comma-separated string
            //
            //     From 2008

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


