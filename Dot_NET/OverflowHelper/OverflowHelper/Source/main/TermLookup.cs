/***************************************************************************
* Copyright (C) 2020 Peter Mortensen                                       *
*                                                                          *
* This file is part of Edit Overflow.                                      *
*                                                                          *
* Purpose: Effectively cached lookup in Wikipedia, Wiktionary, and         *
*          other places (by 2020 the number of new JavaScript              *
*          libraries has grown so insane that even Wikipedia               *
*          can't keep up and in some cases there is only an                *
*          illdefined README file in some obscure GitHub                   *
*          repository).                                                    *
*                                                                          *
*          Opening a simple Wikipedia or Wiktionary page can be slow.      *
*                                                                          *
*          Examples:                                                       *
*                                                                          *
*              2020-10-25T002040Z+0: 5.6 seconds with a 4G connection      *
*                                                                          *
*          E.g. "JavaScript" will return the URL                           *
*          <https://en.wikipedia.org/wiki/JavaScript>. The lookups are     *
*          cached so it is much faster than going through Google and       *
*          Wikipedia every time. This typically takes 7 seconds (at        *
*          least on a 3G Internet connection).                             *
*                                                                          *
*                                                                          *
* Apart from the main function of looks, it knows how to export            *
* the word list in various text formats (e.g. SQL).                        *
*                                                                          *
*                                                                          *
* Note:                                                                    *
*                                                                          *
*    "term" here means (from <https://en.wiktionary.org/wiki/term#Noun>):  *
*                                                                          *
*        "A word or phrase, especially one from a                          *
*         specialised area of knowledge."                                  *
*                                                                          *
*    It is usually a single word (e.g. that is a misspelling), but         *
*    it can also be phrase like "Bob’s your uncle".                        *
*                                                                          *
****************************************************************************/


// Some bugs:
//
//   1. If a line ends in four spaces and a degree sign, all of
//      it is removed!
//
//   2.


//using System;
//using System.Linq;

using System.Collections.Generic; //For Dictionary and List
using System.Text; //For StringBuilder.

//using System.Diagnostics; //For Trace. And its Assert.

//using System; // For Console


//Markers
//AAA   Spell correction add point.
//BBB   Link add point.


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{


    public enum wordListOutputTypeEnum
    {
        SQL = 1009,
        HTML,
        JavaScript
    }


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class TermLookup
    {
        public const string kCodeQuoteStr = "`";

        private Dictionary<string, string> mIncorrect2Correct;

        private correctTermInfoStruct mCorrectTermInfo;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public TermLookup()
        {
            //Why do we have this here?? It is not used and not available
            //on Linux... Should we delete it?
            //
            //// Note: top folder, not the one we are going to store in.
            //string localAppDataFolder =
            //  System.Environment.GetFolderPath(
            //    System.Environment.SpecialFolder.LocalApplicationData);
            //
            //// Note: actual folder. For user "Administrator":
            ////   C:\Documents and Settings\Administrator\Local Settings\Application Data\EMBO\OverflowHelper\1.0.0.0
            ////
            //string localAppDataFolder2 =
            //  System.Windows.Forms.Application.LocalUserAppDataPath;
            //
            //string appDataFolder =
            //  System.Environment.GetFolderPath(
            //    System.Environment.SpecialFolder.ApplicationData);


            //This is dependent on "x86", "x64" and "Any CPU":
            //
            //  On 64-bit Windows:
            //    "x86"    :  "C:\\Program Files (x86)"
            //    "x64"    :  "C:\\Program Files"
            //    "Any CPU":  "C:\\Program Files"
            //
            //Is this meaningful on Linux????
            string folderStr =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);

            //Windows specific. Do we need to keep it? What is the result
            //on Linux?
            //
            //if (true)
            //{
            //    //Testing Stack Overflow question, "Refer to 'Program Files'
            //    //on a 64-bit machine"
            //    //  <http://stackoverflow.com/questions/1454094/refer-program-files-in-a-64-bit-machine>
            //
            //    string f32 = System.Environment.GetEnvironmentVariable("ProgramFiles");
            //    string f64 = System.Environment.GetEnvironmentVariable("ProgramW6432");
            //
            //    //
            //    string processorArchitecture =
            //        System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            //
            //    //Result on 2013-03-21.
            //    //  f32                    "C:\\Program Files"
            //    //  f64                    null
            //    //  processorArchitecture  "AMD64"
            //}

            TermData someTermData = new TermData();

            termListStruct wordList = someTermData.getWordList();

            // Adaptation, at least for now
            mIncorrect2Correct = wordList.incorrect2Correct;

            mCorrectTermInfo = wordList.correctTermInfo;

        } //Constructor


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string lookUp(
            string aQueryStr,
            bool aGuessURLifFailedLookup,
            out string anOutCorrectedWOrd)
        {
            string toReturn = ""; //Default: value indicating failure.

            string corrStr;
            if (mIncorrect2Correct.TryGetValue(aQueryStr, out corrStr))
            {
                Utility.debuggerRest(); // Found case correction!
            }
            else
            {
                corrStr = aQueryStr;
            }

            // Even if there is no case correction it
            // may already be the correct case!
            anOutCorrectedWOrd = corrStr;
            string URLStr;
            if (mCorrectTermInfo.URLs.TryGetValue(corrStr, out URLStr))
            {
                toReturn = URLStr; // We have a match!
            }

            // If lookup failed then we will guess the
            // Wikipedia URL (if allowed to do so).
            if (toReturn.Length == 0)
            {
                if (aGuessURLifFailedLookup)
                {
                    toReturn = "https://en.wikipedia.org/wiki/" + aQueryStr;
                }
            }
            return toReturn;
        } //lookUp()


        // *************************************************************************
        // *  CLASS NAME:   SortBySequenceThenScore_usingIndex                     *
        // *                                                                       *
        // *    Helper class for sorting the correction list.                      *
        // *                                                                       *
        // *    We sort a list of keys (incorrect terms, as they are               *
        // *    unique); we couldn't sort the original values anyway               *
        // *    as they are hashes.                                                *
        // *                                                                       *
        // *                                                                       *
        // d$ <summary> For sorting...                                             *
        // d$      Note: xyz                    </summary>                         *
        // *                                                                       *
        // *************************************************************************
        private class SortByCorrectThenIncorrect_usingIndex :
          System.Collections.Generic.IComparer<int>
        {
            private Dictionary<string, string> mIncorrect2Correct;

            private List<string> mKeys; // Of incorrect terms.

            private List<int> mIndexes; // Into mKeys

            correctTermInfoStruct mCorrectTermInfo;


            //****************************************************************************
            //*                                                                          *
            //*    Constructor                                                           *
            //*                                                                          *
            //*    aCorrect2Count is for sorting by the number of misspellings           *
            //*    as the primary key (for Mortensen technology popularity               *
            //*    index (MTPI))                                                         *
            //*                                                                          *
            //****************************************************************************
            public SortByCorrectThenIncorrect_usingIndex(
                Dictionary<string, string> anIncorrect2Correct,
                correctTermInfoStruct aCorrectTermInfo)
            {
                // We need to remember it for when the
                // compare function is called...
                mIncorrect2Correct = anIncorrect2Correct;

                //mCorrect2Count = aCorrect2Count;
                //mCorrect2WordCount = aCorrect2WordCount;
                mCorrectTermInfo = aCorrectTermInfo;

                int len = anIncorrect2Correct.Count;

                // Prepare the list of keys
                mKeys = new List<string>(len);
                mIndexes = new List<int>(len);

                int currentIndex = 0;
                Dictionary<string, string>.Enumerator hashEnumerator2 =
                    anIncorrect2Correct.GetEnumerator();
                while (hashEnumerator2.MoveNext())
                {
                    string curKey = hashEnumerator2.Current.Key;
                    string curValue = hashEnumerator2.Current.Value;

                    mKeys.Add(curKey);
                    mIndexes.Add(currentIndex);

                    //string URL;
                    //if (!mCorrectTerm2URL.TryGetValue(curValue, out URL))
                    //{
                    //    string msgStr = "No URL mapping for " + curValue;
                    //    System.Windows.Forms.MessageBox.Show(msgStr);
                    //}

                    currentIndex++;
                } //Hash iteration.

            } //Constructor (for the sorter help class)


            //****************************************************************************
            //*                                                                          *
            //*    Note: The input is keys, not the actual values                        *
            //*                                                                          *
            //****************************************************************************
            public int Compare(int aItem1, int aItem2)
            {
                int toReturn = 0; // Default: equal.

                if (aItem1 != aItem2)
                {
                    // The ***unique*** key, the incorrect term,
                    // is the secondary key.
                    string secondary1 = mKeys[aItem1];
                    string secondary2 = mKeys[aItem2];

                    // The correct term is (usually) the primary key.
                    string primary1 = mIncorrect2Correct[secondary1];
                    string primary2 = mIncorrect2Correct[secondary2];

                    int incorrectTermCount1 = mCorrectTermInfo.incorrectTermCount[primary1];
                    int incorrectTermCount2 = mCorrectTermInfo.incorrectTermCount[primary2];

                    int wordCount1 = mCorrectTermInfo.wordCount[primary1];
                    int wordCount2 = mCorrectTermInfo.wordCount[primary2];

                    int uppercaseCount1 = mCorrectTermInfo.uppercaseCount[primary1];
                    int uppercaseCount2 = mCorrectTermInfo.uppercaseCount[primary2];

                    // Override: For unchanged sort functionality (sorted
                    // (and grouped) by the correct term/word). To be
                    // made dynamic in the future.
                    //
                    // Note that unit tests now enforce the default
                    // sort order, so they must be disabled or
                    // changed for the build script to succeed.
                    //
                    incorrectTermCount1 = 7;
                    incorrectTermCount2 = 7;
                    wordCount1 = 7;
                    wordCount2 = 7;
                    uppercaseCount1 = 7;
                    uppercaseCount2 = 7;

                    // Descending sort for the number of misspellings
                    int compareResult_count =
                        incorrectTermCount2.CompareTo(incorrectTermCount1);

                    // Descending sort for the number
                    // of words in a correct term
                    int compareResult_wordCount =
                        wordCount2.CompareTo(wordCount1);

                    // Ascending sort for the number of uppercase letters.
                    // That is, all lowercase first.
                    int compareResult_uppercases =
                        uppercaseCount1.CompareTo(uppercaseCount2);

                    // If enabled, group all lower case from the rest.
                    // Usually used in combination with the word
                    // count (so the longest lower case correct
                    // words comes first)
                    //
                    if (compareResult_uppercases != 0)
                    {
                        toReturn = compareResult_uppercases;
                    }
                    else
                    {
                        if (compareResult_wordCount != 0)
                        {
                            toReturn = compareResult_wordCount;
                        }
                        else
                        {
                            if (compareResult_count != 0)
                            {
                                toReturn = compareResult_count;
                            }
                            else
                            {
                                // Ascending sort for the correct word.
                                //
                                // It is apparently case insensitive.
                                //
                                int compareResult_primary = primary1.CompareTo(primary2);

                                if (compareResult_primary != 0)
                                {
                                    toReturn = compareResult_primary;
                                }
                                else
                                {
                                    // The same primary key - use the
                                    // second key: incorrect term

                                    // Ascending sort for the incorrect word.
                                    int compareResult_secondary = secondary1.CompareTo(secondary2);

                                    if (compareResult_secondary != 0)
                                    {
                                        toReturn = compareResult_secondary;
                                    }
                                    else
                                    {
                                        // Both keys are equal...
                                        //
                                        // We should never be here as the secondary
                                        // key is unique (and the outer check
                                        // is for equality of the index (the
                                        // same entry)) ASSERT?
                                        Utility.debuggerRest();

                                    } // The same incorrect word.
                                } //Different correct word
                            } //Different number of misspellings
                        }
                    }
                }
                else
                {
                    Utility.debuggerRest(); // Indexes equal. Does
                    // this ever happen??? Yes! - apparently the
                    // sorting algorithm does not check
                    // for equality before the call to
                    // this function...
                }
                return toReturn;
            } //Compare()


            //****************************************************************************
            //*                                                                          *
            //****************************************************************************
            public List<int> indexes()
            {
                return mIndexes;
            }


            //****************************************************************************
            //*                                                                          *
            //****************************************************************************
            public List<string> keys()
            {
                return mKeys;
            }


        } // private class SortByCorrectThenIncorrect_usingIndex


        /****************************************************************************
         *                                                                          *
         *    Utility formatting function                                           *
         *                                                                          *
         ****************************************************************************/
        private static string correctionInfoStr2(
            string aBadTerm, string aCorrectedTerm)
        {
            return "bad term '" +
                   aBadTerm +
                   "' (corrected term is '" + aCorrectedTerm + "').";
        }


        /****************************************************************************
         *                                                                          *
         *    Utility formatting function                                           *
         *                                                                          *
         ****************************************************************************/
        private static string URLmappingStr2(string aCorrectedTerm, string aURL)
        {
            return "correct term '" +
                   aCorrectedTerm +
                   "' (URL is '" + aURL + "').";
        }


       /*****************************************************************************
         *                                                                          *
         *    Escape backslashes in a string by means of                            *
         *    backslash itself: "\" -> "\\"                                         *
         *                                                                          *
         ****************************************************************************/
        private static string escapeBackslash(string aString)
        {
            // This is true to where it is refactored from, but is it necessary?
            if (aString.IndexOf(@"\") >= 0)
	        {
                aString = aString.Replace(@"\", @"\\"); // Escape backslash itself... (if any)
          	}

            return aString;
        } //escape_backslash()


       /*****************************************************************************
         *                                                                          *
         *    Escape various characters in a string by means of backslash (\):      *
         *                                                                          *
         *      1. Double quote                                                     *
         *                                                                          *
         *      2. Backslash                                                        *
         *                                                                          *
         *    This is suitable for JavaScript. It is not known if it is for SQL     *
         *    (we currently use single quotes for the SQL strings and thus          *
         *    double quote can be used without escape)                              *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        private static string escape_withBackslash(string aString)
        {
            // Possible optimisation (as we had in a place where we
            // refactored): Test for existence before changing
            // the string. Example:
            //
            //   if (effectiveBadTerm.IndexOf(@"\") >= 0)
	        //   {
            //       effectiveBadTerm = effectiveBadTerm.Replace(@"\", @"\\");
          	//   }
            //
            // But is it actually a problem at all? Is the string actually
            // physically copied if Replace() did not do anyting? Is it
            // taken care of automatically by the .NET runtime?


            // Escape backslash itself
            //
            // Note: This ***must*** be the first. Otherwise we risk
            //       unintended double backslashes in the output.
            //
            aString = escapeBackslash(aString);


            // Escape double quote (if any)
            aString = aString.Replace(@"""", @"\""");

            return aString;
        } //escape_withBackslash()


        /****************************************************************************
         *                                                                          *
         *    Escaping of SQL                                                       *
         *                                                                          *
         *    For now (we may need more, though):                                   *
         *                                                                          *
         ****************************************************************************/
        private static string escapeSQL(string aStringForSQL)
        {
            ourAssert(aStringForSQL != null);

            // In any case, we should have some unit test, including
            // when the backslash is at the beginning or the end of
            // the string.


            // Escape backslash ***itself***
            //
            // Note: This ***must*** be the first. Otherwise we risk unintended
            //       double backslashes in the output (if we escape other
            //       characters using backslash).
            //
            aStringForSQL = escapeBackslash(aStringForSQL);

            aStringForSQL = aStringForSQL.Replace("'", "''");

            return aStringForSQL;
        } //escapeSQL()


        /****************************************************************************
         *                                                                          *
         *    Add SQL for a word (to the buffer)                                    *
         *                                                                          *
         *    Parameter "anIdentityMapping" is only for internal testing            *
         *    purposes - it does not affect the output.                             *
         *                                                                          *
         ****************************************************************************/
        private static void addTermsToOutput_SQL(string aBadTerm3,
                                                 string aCorrectedTerm3,
                                                 bool anIdentityMapping,
                                                 ref StringBuilder aSomeScratch,
                                                 string aURL3)
        {
            if (aBadTerm3.IndexOf("'") >= 0)
            {
                Utility.debuggerRest();
            }

            // Escape the SQL, e.g. backslash
            string effectiveIncorrectTerm = escapeSQL(aBadTerm3);
            string effectiveCorrectTerm = escapeSQL(aCorrectedTerm3);
            string effectiveURL = escapeSQL(aURL3);

            // Sanity check
            if (anIdentityMapping)
            {
                // Check for errors made by the client and internally
                // here (e.g. differences in escaping).
                ourAssert(aBadTerm3  == aCorrectedTerm3);
                ourAssert(effectiveIncorrectTerm  == effectiveCorrectTerm);
           }

            aSomeScratch.Append("INSERT INTO EditOverflow\n");
            aSomeScratch.Append("  (incorrectTerm, correctTerm, URL)\n");
            aSomeScratch.Append("  VALUES(");

            aSomeScratch.Append("'");
            aSomeScratch.Append(effectiveIncorrectTerm);
            aSomeScratch.Append("', '");

            aSomeScratch.Append(effectiveCorrectTerm);
            aSomeScratch.Append("', '");

            // Example of where escape of single quotes is necessary:
            //
            //   https://en.wiktionary.org/wiki/couldn't#Contraction
            //
            aSomeScratch.Append(effectiveURL);

            aSomeScratch.Append("');");

            aSomeScratch.Append("\n\n");
        } //addTermsToOutput_SQL()


        /****************************************************************************
         *                                                                          *
         *  Purpose: Isolate the dependency on TermData (we later                   *
         *           want to eliminate it altogether (by moving                     *
         *           ourAssert() to a central place))                               *
         *                                                                          *
         *           We may keep this one for convenience                           *
         *           on the client side.                                            *
         *                                                                          *
         ****************************************************************************/
        private static void ourAssert(bool aCondition)
        {
            TermData.ourAssert(aCondition);
        }


        /****************************************************************************
         *                                                                          *
         *    More officially, HTML character entity reference encoding:            *
         *                                                                          *
         *    We should probably replace the implementation (or                     *
         *    this function) with some standard library                             *
         *    function (instead of doing it manually).                              *
         *                                                                          *
         ****************************************************************************/
        private static string escapeHTML(string aStringForHTML)
        {
            ourAssert(aStringForHTML != null);

            //Would it be more memory efficient to test first and then return
            //the original string? - very few of these calls result in an
            //actual replacement
            //
            //Does this result in actual copying of the unchanged string? -
            //or is something like "interning" of strings in effect?


            // Note: We don't want to apply more than one level of
            //       encoding at a time (that is up to the client).
            //
            //       So the order matters. E.g., if the ***input*** is
            //       "<", we ***don't*** want this to happen:
            //
            //           "<" -> "&lt;" -> "&amp;lt;"
            //

            // Correct. Note that the order matters!
            return aStringForHTML.Replace("&", "&amp;").Replace("<", "&lt;");
        } //escapeHTML()


        /****************************************************************************
         *                                                                          *
         * aFirstCorrectedTerm:                                                     *
         *                                                                          *
         *   True for the first (correct) word in a series of one or more           *
         *   (correct) words (presumes the 'addTermsToOutput_HTML' call             *
         *   order is in a sorted way, with the corrected term as the               *
         *   primary key).                                                          *
         *                                                                          *
         ****************************************************************************/
        private static void addTermsToOutput_HTML(string aBadTerm,
                                                  string aCorrectedTerm,
                                                  ref HTML_builder aInOutBuilder,
                                                  string aURL,
                                                  bool aFirstCorrectedTerm,
                                                  int aMisspellingCount)
        {
            // Only include the HTML anchor for the ***first*** word in
            // a group of words (the same correct word, but different
            // incorrect words).
            //
            // It would be wasteful and would not pass
            // HTML validation ('id' must be unique).
            //
            string anchor = "";
            string extraForCorrectWord = "";
            if (aFirstCorrectedTerm)
            {
                // Generate an acceptable identifier (for the HTML anchor)

                // Spaces are not allowed in IDs
                string escapedCorrectedTerm = aCorrectedTerm.Replace(@" ", @"_");

                // HTML non-breaking spaces do not work
                // to scroll in a web browser
                escapedCorrectedTerm = escapedCorrectedTerm.Replace(@"&nbsp;", @"_");

                // We also filter out double quotes. Unchanged they
                // will cause HTML validation to fail. We don't
                // know if it is possible to escape them in
                // this context ("&quot;"?), but it is a
                // very minor problem - we have only
                // one entry in the (current)
                // entire word list.
                escapedCorrectedTerm = escapedCorrectedTerm.Replace(@"""", @"");

                // Filter out asterisks
                escapedCorrectedTerm = escapedCorrectedTerm.Replace(@"*", @"");

                string attrStr =
                  @" id=""" + escapedCorrectedTerm + @""""; // Note: Leading space

                anchor = HTML_builder.singleLineTagStrWithAttr("div", "", attrStr);

                extraForCorrectWord = " (" + aMisspellingCount + ")";
            }

            string outerColumnsSeparator = " ";

            // Empty: We avoid extra trailing space when copy-pasting
            //        from the word list page in a web browser.
            //
            //        We may simply opt for direct substitution at
            //        some point (if this turns out to be stable).
            //
            //string innerColumnsSeparator = " ";
            string innerColumnsSeparator = ""; //It might seem like a null
                                               //operation, but we want to
                                               //keep the option open and
                                               //explicit.

            aInOutBuilder.singleLineTagOnSeparateLine(
                "tr",
                   outerColumnsSeparator +

                   HTML_builder.singleLineTagStr(
                     "td", anchor + escapeHTML(aBadTerm)) + innerColumnsSeparator +

                   HTML_builder.singleLineTagStr(
                     "td",
                     escapeHTML(aCorrectedTerm) + extraForCorrectWord) +

                         innerColumnsSeparator +

                   HTML_builder.singleLineTagStr(
                     "td", aURL) + outerColumnsSeparator
                );
        } //addTermsToOutput_HTML()


        /****************************************************************************
         *                                                                          *
         *    Helper function for dumpWordList_asHTML()                             *
         *                                                                          *
         ****************************************************************************/
        private static string encloseInTag_HTML(string aTagName, string aText)
        {
            const string start = "&lt;";
            const string end = "&lt;/";
            return "" + start + aTagName + ">" + aText + end + aTagName + "> ";
        } //encloseInTag_HTML()


       /****************************************************************************
        *                                                                          *
        *    Add JavaScript code for a word (to the buffer)                        *
        *                                                                          *
        *    Parameter "anIdentityMapping" is only for internal testing            *
        *    purposes - it does not affect the output.                             *
        *                                                                          *
        *                                                                          *
        ****************************************************************************/
        private static void addToAssociativeArray_JavaScript(
            string aVariableName, string aKey, string aValue,
            bool anIdentityMapping,
            ref StringBuilder aSomeScratch)
        {
            aSomeScratch.Append(aVariableName);
            aSomeScratch.Append(@"[""");
            aSomeScratch.Append(aKey);
            aSomeScratch.Append(@"""] = """);

            aSomeScratch.Append(aValue);
            aSomeScratch.Append(@""";");

            aSomeScratch.Append("\n");
        } //addToAssociativeArray_JavaScript()


       /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static void addTermsToOutput_JavaScript(string aBadTerm,
                                                        string aCorrectedTerm,
                                                        bool anIdentityMapping,
                                                        ref StringBuilder aSomeScratch,
                                                        string aURL)
        {
            // We need to escape double quotes. Example (incorrect term):
            //
            //     Mac OS X (10.6 "Snow Leopard")
            //
            // And backslash. Example:
            //
            //     PS\2
            //
            //   Otherwise, the result may be this with JavaScript
            //   unit testing:
            //
            //       The only valid numeric escape in strict mode is '\0'.

            string effectiveBadTerm = escape_withBackslash(aBadTerm);

            //string effectiveCorrectedTerm = aCorrectedTerm;
            string effectiveCorrectedTerm = escape_withBackslash(aCorrectedTerm);

            // Example (this example is for identity mapping so we can also
            //          look up the correct words (with explicit program
            //          code)):
            //
            //   incorrect2correct["ZX 81"] = "ZX 81";
            //
            //   correct2URL["ZX81"] = "https://en.wikipedia.org/wiki/ZX81";


            // Sanity check (we ought to also add them to
            // addTermsToOutput_HTML())
            if (anIdentityMapping)
            {
                // Check for errors made by the client and internally
                // here (e.g., differences in escaping).
                ourAssert(aBadTerm  == aCorrectedTerm);
                ourAssert(effectiveBadTerm  == effectiveCorrectedTerm);
            }

            // Word mapping
            addToAssociativeArray_JavaScript("incorrect2correct",
                                             effectiveBadTerm,
                                             effectiveCorrectedTerm,
                                             anIdentityMapping,
                                             ref aSomeScratch);

            // URL mapping
            //
            // Assume that this function is called one
            // and only time for the identity mapping.
            //
            // The actual identity mapping is implicitly
            // by the above in that case.
            //
            if (anIdentityMapping)
            {
                addToAssociativeArray_JavaScript("correct2URL",
                                                 effectiveCorrectedTerm,
                                                 aURL,
                                                 anIdentityMapping,
                                                 ref aSomeScratch);
            }

        } //addTermsToOutput_JavaScript()


        /****************************************************************************
         *                                                                          *
         *   Encapsulate the loop where we run through the whole word list (of      *
         *   incorrect terms) in a certain order. E.g., to generate the rows        *
         *   for an HTML table.                                                     *
         *                                                                          *
         *    Note: We currently use an enum. Would it be better with something     *
         *          where the formatting type (e.g., HTML or SQL) is some           *
         *          user-defined thing (e.g., a concrete instance of an             *
         *          abstract class)?)                                               *
         *                                                                          *
         ****************************************************************************/
        private static void exportWordlist(
            ref StringBuilder aSomeScratch,
            wordListOutputTypeEnum aWordListOutputType,
            ref string aLongestInCorrectTerm,
            ref string aLongestCorrectTerm,
            ref string aLongestURL,
            ref Dictionary<string, string> anIncorrect2Correct,

            correctTermInfoStruct aCorrectTermInfo
            //ref Dictionary<string, string> aCorrectTerm2URL,
            //ref Dictionary<string, int> aCorrect2Count,
            //ref Dictionary<string, int> aCorrect2WordCount
            )
        {
            SortByCorrectThenIncorrect_usingIndex sortObject =
                new SortByCorrectThenIncorrect_usingIndex(
                    anIncorrect2Correct,
                    aCorrectTermInfo);

            // Unsorted at this point. The values are arbitrary (but
            // unique), but they happen to start at 1, followed by
            // 2, 3, etc.
            List<int> indexes = sortObject.indexes();

            // After: Those indexes are now sorted such that looking
            // up the keys (in order) in mIncorrect2Correct will
            // return the entries in the given defined sort order,
            // in this case the correct term as the primary key
            // and the incorrect term as the secondary key.
            //
            // In order words, grouping by the correct term...
            indexes.Sort(sortObject);

            List<string> someKeys_incorrectTerms = sortObject.keys();

            HTML_builder builder = new HTML_builder();
            for (int i = 0; i < 3; i++)
            {
                builder.indentLevelUp();
            }

            string prevCorrectTerm = "";
            foreach (int someIndex in indexes)
            {
                string someIncorrectTerm = someKeys_incorrectTerms[someIndex];
                string someCorrectTerm = anIncorrect2Correct[someIncorrectTerm];

                int misspellingCount =
                    aCorrectTermInfo.incorrectTermCount[someCorrectTerm];

                string msg = string.Empty; // Default: empty - flag for no errors.

                bool firstCorrectedTerm = someCorrectTerm != prevCorrectTerm;

                // On-the-fly check - during the export (but it would be
                // better if this check was done at program startup)
                if (aCorrectTermInfo.URLs.ContainsKey(someIncorrectTerm))
                {
                    msg =
                      "Incorrect term \"" + someIncorrectTerm +
                      "\" has been entered as a correct term...";
                }

                string someURL = null;
                if (!aCorrectTermInfo.URLs.TryGetValue(someCorrectTerm, out someURL))
                {
                    // Fail. What should we do?

                    msg =
                      "A correct term, \"" + someCorrectTerm +
                      "\", could not be looked up in the term-to-URL mapping."
                      ;
                }
                else
                {
                    // On-the-fly checks (but it would be better if these
                    // checks were done at program startup).
                    //
                    // URLs should look like ones.
                    //
                    // For now, the main check is if they start with "http"...
                    //
                    if (!someURL.StartsWith("http"))
                    {
                        msg =
                          "A URL, <" + someURL + ">, does not look like one. " +
                          "For correct term \"" + someCorrectTerm + "\".";
                    }
                    if (someURL.Contains("duckduckgo.com") ||
                        someURL.Contains("www.google.com"))
                    {
                        // A (hardcoded) exception (only one at this time). This depends
                        // entirely on the content of the word list. But we can't
                        // really justify a more general approach at this point.
                        if (! someURL.Contains("acronym"))
                        {
                            //msg =
                            //    "A URL, <" + someURL + ">, is a search query. " +
                            //    "This is not allowed.";
                        }
                    }
                } // If URL exists

                // Report error, if any. For now, throw blocking
                // dialogs (if Windows GUI application)...
                if (msg != string.Empty)
                {
                    TermData.reportError(msg);

                    // Continue - we want all errors in one go.
                }
                else
                {
                    // Only generate output for the current item
                    // if there aren't any errors - otherwise
                    // we may crash (e.g. for an absent
                    // term-to-URL mapping).

                    // Using a flag for now (for the type of output, HTML, SQL, etc.)

                    switch (aWordListOutputType)
                    {
                        case wordListOutputTypeEnum.HTML:

                            addTermsToOutput_HTML(someIncorrectTerm,
                                                  someCorrectTerm,
                                                  ref builder,
                                                  someURL,
                                                  firstCorrectedTerm,
                                                  misspellingCount);
                            break;

                        case wordListOutputTypeEnum.SQL:

                            addTermsToOutput_SQL(someIncorrectTerm,
                                                 someCorrectTerm,
                                                 false,
                                                 ref aSomeScratch,
                                                 someURL);

                            // Add the identity mapping (so we don't special
                            // code (and a second database lookup) for
                            // looking up the correct term), but only once.
                            //
                            // Note: We can rely on the sorted order, first
                            // by incorrect and then correct. Thus, we will
                            // get a corrected term one or more times
                            // consecutively.
                            //
                            if (firstCorrectedTerm)
                            {
                                // Extra output for SQL: "Identity mapping" - a lookup
                                // of a correct term should also succeeed - e.g. if we
                                // only want to get the URL.
                                addTermsToOutput_SQL(someCorrectTerm,
                                                     someCorrectTerm,
                                                     true,
                                                     ref aSomeScratch,
                                                     someURL);
                            }
                            break;

                        case wordListOutputTypeEnum.JavaScript:

                            addTermsToOutput_JavaScript(someIncorrectTerm,
                                                        someCorrectTerm,
                                                        false,
                                                        ref aSomeScratch,
                                                        someURL);

                            // Once per correct term:
                            //
                            // 1. Identity mapping (so we don't need special
                            //    code for looking up correct terms)
                            //
                            // 2. Establish the mapping from correct term to
                            //    URL (in the generated JavaScript code)
                            //
                            if (firstCorrectedTerm)
                            {
                                // "Identity mapping" - a lookup of a correct
                                // term should also succeeed - e.g., if we only
                                // want to get the URL.
                                //
                                // Also implicitly adds the mapping to the URL
                                //
                                addTermsToOutput_JavaScript(someCorrectTerm,
                                                            someCorrectTerm,
                                                            true,
                                                            ref aSomeScratch,
                                                            someURL);
                                // Delineate groups

                                aSomeScratch.Append("\n\n");
                            }
                            break;

                        default:
                            ourAssert(false); // Fall-through. Not allowed.
                            break;
                    }


                    // Update statistics
                    if (someIncorrectTerm.Length > aLongestInCorrectTerm.Length)
                    {
                        aLongestInCorrectTerm = someIncorrectTerm;
                    }
                    if (someCorrectTerm.Length > aLongestCorrectTerm.Length)
                    {
                        aLongestCorrectTerm = someCorrectTerm;
                    }
                    if (someURL.Length > aLongestURL.Length)
                    {
                        aLongestURL = someURL;
                    }
                } // Error free (current item)

                prevCorrectTerm = someCorrectTerm;
            } //Through the list of incorrect words

            if (aWordListOutputType == wordListOutputTypeEnum.HTML)
            {
                aSomeScratch.Append(builder.currentHTML());
            }
        } //exportWordlist()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public string dumpWordList_asSQL()
        {
            // For the pre-allocation: We now use a very simple
            // prediction model - linear with the number of entries
            //
            // Data points (the number of incorrect is only
            // what is affecting the size):
            //
            //    Date        Incorrect   SQL_tableRows  Average
            //                entries     characters     size
            //    ----------------------------------------------
            //    2019-12-11  9686        1925736        199
            //
            int correctWordEntries = mCorrectTermInfo.URLs.Count;
            int incorrectWordEntries = mIncorrect2Correct.Count;

            //int capacity = 1200000;
            //int capacity = 2500000;
            //
            // 199 characters per entry, with a 5% margin
            int capacity = incorrectWordEntries * 209;

            StringBuilder SQL_tableRows = new StringBuilder(capacity);

            string longestInCorrectTerm = "";
            string longestCorrectTerm = "";
            string longestURL = "";
            exportWordlist(ref SQL_tableRows,
                           wordListOutputTypeEnum.SQL,
                           ref longestInCorrectTerm,
                           ref longestCorrectTerm,
                           ref longestURL,
                           ref mIncorrect2Correct,
                           mCorrectTermInfo);

            return SQL_tableRows.ToString();
        } //dumpWordList_asSQL()


        /****************************************************************************
         *                                                                          *
         *  Add:  T h e   s t a r t   o f   t h e   H T M L   d o c u m e n t       *
         *                                                                          *
         ****************************************************************************/
        private static void startOfHTML_document(
            ref HTML_builder aInOutBuilder,
            string aTitle,
            string aLongestInCorrectTerm,
            string aLongestCorrectTerm,
            string aLongestURL,
            string aLenLongestInCorrectTermStr,
            string aLenLongestCorrectTermStr,
            string aLenLongestURLStr
            )
        {
            aInOutBuilder.startHTML(aTitle);

            // CSS for table
            {
                aInOutBuilder.startTagWithEmptyLine("style");

                aInOutBuilder.addContentOnSeparateLine("body");
                aInOutBuilder.addContentOnSeparateLine("{");
                aInOutBuilder.indentLevelUp();
                aInOutBuilder.addContentOnSeparateLine("background-color: lightgrey;");
                aInOutBuilder.indentLevelDown();
                aInOutBuilder.addContentOnSeparateLine("}");

                aInOutBuilder.addContentOnSeparateLine("th");
                aInOutBuilder.addContentOnSeparateLine("{");
                aInOutBuilder.indentLevelUp();
                aInOutBuilder.addContentOnSeparateLine("text-align: left;");
                aInOutBuilder.addContentOnSeparateLine("background-color: orange;");
                aInOutBuilder.indentLevelDown();
                aInOutBuilder.addContentOnSeparateLine("}");

                aInOutBuilder.addContentOnSeparateLine("tr");
                aInOutBuilder.addContentOnSeparateLine("{");
                aInOutBuilder.indentLevelUp();
                aInOutBuilder.addContentOnSeparateLine("text-align: left;");
                aInOutBuilder.addContentOnSeparateLine("background-color: lightblue;");
                aInOutBuilder.indentLevelDown();
                aInOutBuilder.addContentOnSeparateLine("}");

                aInOutBuilder.endTagOneSeparateLine("style");
            } // Block. Output CSS part of the HTML content.


            aInOutBuilder.endTagOneSeparateLine("head");


            // Start of body
            aInOutBuilder.startTagWithEmptyLine("body");

            // Headline
            aInOutBuilder.addHeader(1, aTitle);

            // Link to the service
            aInOutBuilder.addParagraph(
                "There is <a " +
                "href=\"https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu\"" +
                ">a corresponding web application</a> to look up words " +
                "(both the incorrect and correct ones).");

            // Justification for its existence...
            aInOutBuilder.addParagraph(
                "The content of this list is 99% from what someone on " +
                "the Internet actually posted (in most cases misspelled) - " +
                "it is not a made-up list.");

            // Some disclaimers regarding the accuracy...
            aInOutBuilder.addParagraph(
                "Note: Some terms are not actually " +
                "incorrect, but serve as:");

            aInOutBuilder.startTagWithEmptyLine("ul");

            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Expansions (for example, expanding \"JS\" to \"JavaScript\").");
            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Go the other way, from expanded to abbreviation.");
            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Include line-break protection (though only in " +
                "the HTML source, not as displayed here) - by &amp;nbsp;.");
            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Casing of some words and tense of some verbs are not " +
                "always 100% correct (this must be dealt with manually) - the " +
                "reason is to avoid redundancy.");

            aInOutBuilder.endTagOneSeparateLine("ul");

            aInOutBuilder.addParagraph(
                "Also note that this should not be applied " +
                  "blindly (high rate of false positives) - " +
                  "every match should be manually checked."
                );

            // Some statistics
            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            // There is some redundancy here...
            aInOutBuilder.addParagraph(
                "Longest incorrect term: \"" +
                  aLongestInCorrectTerm +
                  "\" (" +
                  aLenLongestInCorrectTermStr +
                  " characters)"
                );

            aInOutBuilder.addParagraph(
                "Longest correct term: \"" +
                  aLongestCorrectTerm +
                  "\" (" +
                  aLenLongestCorrectTermStr +
                  " characters)"
                );

            aInOutBuilder.addParagraph(
                "Longest URL: \"" +
                  aLongestURL +
                  "\" (" +
                  aLenLongestURLStr +
                  " characters)"
                );

        } //startOfHTML_document()


        /****************************************************************************
         *                                                                          *
         *  Add:  T h e   s t a r t   o f   t h e   H T M L   d o c u m e n t       *
         *                                                                          *
         ****************************************************************************/
        private static void startOfHTML_Table(ref HTML_builder aInOutBuilder)
        {
            // Start of table, incl. table headers (title row)
            aInOutBuilder.startTagWithEmptyLine("table");

            aInOutBuilder.singleLineTagOnSeparateLine(
                "tr",
                " " +
                  HTML_builder.singleLineTagStr("th", "Incorrect") + " " +
                  HTML_builder.singleLineTagStr("th", "Correct") + " " +
                  HTML_builder.singleLineTagStr("th", "URL") + " "
                );
            aInOutBuilder.addEmptyLine();
        } //startOfHTML_Table()


        /****************************************************************************
         *                                                                          *
         *  Add:  T h e   e n d   o f   t h e   H T M L   d o c u m e n t           *
         *                                                                          *
         ****************************************************************************/
        private static void endOfHTML_document(ref HTML_builder aInOutBuilder,
                                               string aCodeCheck_regularExpression,
                                               string aCodeCheck_AllOfExplanations,
                                               string aDateStr)
        {
            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            // Headline
            aInOutBuilder.addHeader(2, "Some common strings and characters");

            // Things for copy-paste
            aInOutBuilder.addParagraph(
                "&nbsp;" +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Code quotes",
                  TermLookup.kCodeQuoteStr +
                    TermLookup.kCodeQuoteStr) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Keyboard", "&lt;kbd>&lt;/kbd>") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Non-breaking space", "&amp;nbsp;") +

                aInOutBuilder.smallTextItemWithSpacing("Micro", "µ") +

                aInOutBuilder.smallTextItemWithSpacing("Degree", "°") +

                aInOutBuilder.smallTextItemWithSpacing("Ohm", "&ohm;") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Right arrow", "&rarr; (&amp;rarr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Up arrow", "&uarr; (&amp;uarr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Down arrow", "&darr; (&amp;darr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Left arrow", "&larr; (&amp;larr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Line break", "&nbsp;&lt;br/>")
            );

            aInOutBuilder.addContentWithEmptyLine("<hr/>");


            // Regular expresions for code formatting check

            aInOutBuilder.addEmptyLine();
            aInOutBuilder.addEmptyLine();
            aInOutBuilder.addComment("Note: Updates to this part (e.g., more code checks) is to be updated in FixedStrings.php...");

            aInOutBuilder.addHeader(2, "Code <u>f</u>ormatting check");

            aInOutBuilder.addParagraph(
              "Note: It is rudimentary. As the false positive rate is high, " +
              "every match should be checked manually.");

            // 12 spaces. 18 would match the old indent in FixedStrings.php,
            // but we need to manually edit anyway and we want to keep it
            // regular in this generated HTML file.
            string lineBreakAndIndent = "\n            ";

            // Apply some (HTML) formatting to special characters
            // that makes it difficult to see where an item (an
            // explanation) ends (especially when near the end
            // double quote).
            //
            // Note that this is not encoding. It is
            // essentially formatting as bold.
            //
            // In particlar for a current item ending "{".
            //
            string codeCheck_AllOfExplanations_formatted =
              aCodeCheck_AllOfExplanations

                  // For the first item...
                  .Replace("{", "<strong>{</strong>")

                  // For the fifth item...
                  .Replace("+", "<strong>+</strong>")

                  // Insert (internal) linebreaks with indent. Note the
                  // trailing space in the first argument (to avoid an
                  // odd number of space in the output...)
                  //
                  .Replace(", ", "," + lineBreakAndIndent)

                  //.Replace(", ", "," + lineBreakAndIndent + " ")  // Test only!!!!!!
                ;

            // No output during NUnit run (unit test)
            //System.Console.WriteLine("aCodeCheck_AllOfExplanations2: " + aCodeCheck_AllOfExplanations2);
            //System.Diagnostics.Debug.WriteLine("aCodeCheck_AllOfExplanations2: " + aCodeCheck_AllOfExplanations2);

            // Actually outputs during NUnit run (unit test)
            //System.Console.Error.WriteLine("aCodeCheck_AllOfExplanations2: " + aCodeCheck_AllOfExplanations2);

            //What is the "&nbsp;" for??

            // Note:
            //
            //   We do not want to HTML escape 'codeCheck_AllOfExplanations_formatted'.
            //   At this point it is already formatted as actual HTML for
            //   presentation purposes (e.g. "<strong>"). If we need to
            //   escape the text itself (aCodeCheck_regularExpression)
            //   in it, it should be done above, before any HTML
            //   presentation formatting is added.
            //
            aInOutBuilder.addParagraph(
                "Regular expression" +
                lineBreakAndIndent +
                  codeCheck_AllOfExplanations_formatted +
                  ": <br/>" +
                lineBreakAndIndent +
                  escapeHTML(aCodeCheck_regularExpression));

            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            aInOutBuilder.addHeader(2, "Templates");

            aInOutBuilder.addParagraph(
                aInOutBuilder.smallTextItemWithSpacing(
                  "Subscript",
                  encloseInTag_HTML("sub", "SUB")) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Keyboard formatting",
                  encloseInTag_HTML("kbd", "Key")) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Superscript",
                  encloseInTag_HTML("sup", "SUP")) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Proper formatting for things enclosed " +
                    "in &quot;&lt;>&quot; (square brackets)",
                  "&amp;lt;XYZ>")
            );

            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            string presumedURL = "pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_";

            //For now.
            if (false) //We want to revert to this. Perhaps make it
                       //configurable/dynamic on generation time,
                       //e.g. a command-line parameter or
                       //environment variable?
            {
                //Only outcommented for avoid a compiler warning
                //presumedURL += aDateStr + ".html";
            }
            else
            {
                // For the daily updated version,
                // <https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>
                presumedURL += "latest.html";
            }

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"https://pmortensen.eu/world/EditOverflow1.html\">" +
                "Edit Overflow for Web</a>"
            );

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"https://validator.w3.org/check?uri=" +
                presumedURL +
                "&amp;charset=utf-8\">" +
                "HTML validation</a>"
            );

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"https://pmortensen.eu/\">" +
                "Back to Edit Overflow page</a>"
            );

            aInOutBuilder.addEmptyLine();
            aInOutBuilder.endTagOneSeparateLine("body");

            aInOutBuilder.addEmptyLine();
            aInOutBuilder.endTagOneSeparateLine("html");
        } //endOfHTML_document()


        /****************************************************************************
         *                                                                          *
         *    Notes:                                                                *
         *                                                                          *
         *      1. Parameter "aCodeCheck_regularExpression" does not do             *
         *         anything - it is for output (in the HTML content).               *
         *                                                                          *
         *      2. This function, with six parameters, is only used (directly,      *
         *         as a public function) by some regression tests to get a          *
         *         stable output (independent of the ever-growing word list).       *
         *                                                                          *
         ****************************************************************************/
        public static string dumpWordList_asHTML(
            string aCodeCheck_regularExpression,
            string aCodeCheck_AllOfExplanations,
            ref Dictionary<string, string> anIncorrect2Correct,
            int aUniqueWords,
            correctTermInfoStruct aCorrectTermInfo,
            string aVersionStr,
            string aDateStr)
        {
            // For the pre-allocation: We now use a very simple prediction
            // model - linear with the number of entries.
            //
            // Data points (the number of incorrect entries
            // is the only thing that affects the size):
            //
            //    Date        Incorrect   HTML_tableRows   Average
            //                entries     characters       size
            //    ------------------------------------------------
            //    2019-12-11  9686        1158618          119.6
            //
            //
            //    Date        Incorrect   scratchSB        Average
            //                entries     characters       size
            //    ------------------------------------------------
            //    2019-12-11  9686        1162309          120.0
            //
            int incorrectWordEntries = anIncorrect2Correct.Count;
            int correctWordEntries = aCorrectTermInfo.URLs.Count;

            //int capacity = 1200000;
            //
            // 120 characters per entry, with a 5% margin
            int capacity = incorrectWordEntries * 126;

            StringBuilder scratchSB = new StringBuilder(capacity);
            StringBuilder HTML_tableRows = new StringBuilder(capacity);

            // First generate the rows of the main table - so we can compute
            // various statistics while we go through the data structures
            //
            string longestInCorrectTerm = "";
            string longestCorrectTerm = "";
            string longestURL = "";
            exportWordlist(ref HTML_tableRows,
                           wordListOutputTypeEnum.HTML,
                           ref longestInCorrectTerm,
                           ref longestCorrectTerm,
                           ref longestURL,
                           ref anIncorrect2Correct,
                           aCorrectTermInfo);

            // The main side effect is the changing of
            // the content of ref HTML_tableRows...

            string lenLongestInCorrectTermStr = longestInCorrectTerm.Length.ToString();
            string lenLongestCorrectTermStr = longestCorrectTerm.Length.ToString();
            string lenLongestURLStr = longestURL.Length.ToString();

            // Separate HTML generation for title (used in two places)
            scratchSB.Append("Edit Overflow wordlist. ");
            scratchSB.Append(incorrectWordEntries);
            scratchSB.Append(" input words and ");
            scratchSB.Append(aUniqueWords);
            scratchSB.Append(" output words (for ");
            scratchSB.Append(aVersionStr);
            scratchSB.Append(")");
            string title = scratchSB.ToString();

            scratchSB.Length = 0;

            HTML_builder builder = new HTML_builder();

            startOfHTML_document(ref builder,
              title,
              longestInCorrectTerm, longestCorrectTerm, longestURL,
              lenLongestInCorrectTermStr,
              lenLongestCorrectTermStr,
              lenLongestURLStr);

            startOfHTML_Table(ref builder);

            // That is everything before the table content, including
            // the table header...
            scratchSB.Append(builder.currentHTML());

            // The bulk of this HTML page - all the data rows in the table.
            scratchSB.Append(HTML_tableRows);

            builder.endTagOneSeparateLine("table");

            endOfHTML_document(ref builder,
                               aCodeCheck_regularExpression,
                               aCodeCheck_AllOfExplanations,
                               aDateStr);

            scratchSB.Append(builder.currentHTML());
            //--------------------------------------------------------

            return scratchSB.ToString();
        } //dumpWordList_asHTML()


        /****************************************************************************
         *                                                                          *
         *  This is the function used by the main user-facing functionality         *
         *  (e.g., menu command), exporting the current wordlist to HTML.           *
         *                                                                          *
         ****************************************************************************/
        public string dumpWordList_asHTML(string aCodeCheck_regularExpression,
                                          string aCodeCheck_AllOfExplanations,
                                          string aVersionStr,
                                          string aDateStr)
        {
            return dumpWordList_asHTML(
                      aCodeCheck_regularExpression,
                      aCodeCheck_AllOfExplanations,
                      ref mIncorrect2Correct,
                      mCorrectTermInfo.URLs.Count,
                      mCorrectTermInfo,
                      aVersionStr,
                      aDateStr);
        } //dumpWordList_asHTML()


        /****************************************************************************
         *                                                                          *
         *  This output is intended for use in inline lookup in the web             *
         *  browser using JavaScript (faster than form-based), either               *
         *  as a faster version of the form lookup or directly                      *
         *  manipulating the text in an edit window (the whole                      *
         *  word list loaded into the browser - on the client-side)                 *
         *                                                                          *
         ****************************************************************************/
        public string dumpWordList_asJavaScript()
        {
            // For the pre-allocation: We use a very simple prediction
            // model - linear with the number of entries.
            //
            // Data points (the number of incorrect is only
            // what is affecting the size):
            //
            //    Date        Incorrect   XXXX           Average
            //                entries     characters       size
            //    ------------------------------------------------
            //    2020-07-12  13016        XXXX          XXX.X
            //
            //
            int incorrectWordEntries = mIncorrect2Correct.Count;
            int correctWordEntries = mCorrectTermInfo.URLs.Count;

            //int capacity = 1200000;
            //
            // XXX characters per entry, with a 5% margin
            int capacity = incorrectWordEntries * 126;

            StringBuilder JavaScript_codeSB = new StringBuilder(capacity);

            // But using an alias anyway for most operations (as
            // it is much shorter)
            StringBuilder sb = JavaScript_codeSB;

            // File header
            sb.Append("// Wordlist for Edit Overflow, in JavaScript form\n");
            sb.Append("//\n");
            sb.Append("// Note: Automatically generated file (by the export\n");
            sb.Append("//       function dumpWordList_asJavaScript() in C# \n");
            sb.Append("//       code - e.g. through the command-line\n");
            sb.Append("//       interface)\n\n\n");

            // Variable declarations (JavaScript)
            sb.Append("// Data structure declarations\n");
            sb.Append("let incorrect2correct = {};\n");
            sb.Append("let correct2URL = {};\n\n\n\n\n");

            // Header for the list, with statistics
            sb.Append("// The list... ");
            sb.Append(incorrectWordEntries);
            sb.Append(" incorrect words and ");
            sb.Append(correctWordEntries);
            sb.Append(" correct words.\n\n");

            // The main side effect of exportWordlist() is the changing
            // of the content of ref JavaScript_codeSB...
            string longestInCorrectTerm = "";
            string longestCorrectTerm = "";
            string longestURL = "";
            exportWordlist(ref JavaScript_codeSB,
                           wordListOutputTypeEnum.JavaScript,
                           ref longestInCorrectTerm,
                           ref longestCorrectTerm,
                           ref longestURL,
                           ref mIncorrect2Correct,
                           mCorrectTermInfo);

            // For unit testing under Jest (which runs under Node.js)
            sb.Append("\n\n\n\n");
            sb.Append("// For JavaScript unit testing with Jest under Node.js to work...\n");
            sb.Append("// It is harmless when used in a web browser context.\n");
            sb.Append("module.exports = incorrect2correct;\n");

            return JavaScript_codeSB.ToString();
        } //dumpWordList_asJavaScript()


    } //class TermLookup


} //namespace OverflowHelper.core

