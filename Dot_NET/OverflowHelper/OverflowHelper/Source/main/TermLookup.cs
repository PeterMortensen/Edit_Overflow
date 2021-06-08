/***************************************************************************
* Copyright (C) 2020 Peter Mortensen                                       *
*                                                                          *
* This file is part of Edit Overflow.                                      *
*                                                                          *
* Purpose: Effectively cached lookup in Wikipedia, Wiktionary, and         *
*          other places (by 2020 the number of new JavaScript              *
*          libearies has grown so insane that even Wikiepdia               *
*          can't keep up and in some cases there is only an                *
*          illdefined README file in some obscure GitHub                   *
*          repository).                                                    *
*                                                                          *
*          Opening a simple Wikipedia or Wiktionary page can be slow.      *
*          2020-10-25T002040Z+0: 5.6 seconds with a 4G connection          *
*                                                                          *
*                                                                          *
*          E.g. "JavaScript" will return the URL                           *
*          <https://en.wikipedia.org/wiki/JavaScript>. The lookups are     *
*          cached so it is much faster than going through Google and       *
*          Wikipedia every time. This typically takes 7 seconds (at        *
*          least on a 3G Internet connection).                             *
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

using System.Collections.Generic; // For Dictionary and List
using System.Text; //For StringBuilder.
using System.Diagnostics; //For Trace. And its Assert.

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
        SQL = 1003,
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
        private Dictionary<string, string> mCorrectTerm2URL;


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

            //Adaptation, at least for now
            mIncorrect2Correct = wordList.incorrect2Correct;
            mCorrectTerm2URL = wordList.correctTerm2URL;

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
                Utility.debuggerRest(); //Found case correction!
            }
            else
            {
                corrStr = aQueryStr;
            }

            // Even if there is no case correction it may already be the correct case!
            anOutCorrectedWOrd = corrStr;
            string URLStr;
            if (mCorrectTerm2URL.TryGetValue(corrStr, out URLStr))
            {
                toReturn = URLStr; // We have a match!
            }

            // If lookup failed then we will guess the Wikipedia URL (if allowed to do so).
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
        // *    We sort a list of keys (incorrect terms - as they are              *
        // *    unique) - we couldn't sort the original values anyway              *
        // *    as they are hashes                                                 *
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


            //****************************************************************************
            //*    <placeholder for header>                                              *
            //****************************************************************************
            public SortByCorrectThenIncorrect_usingIndex(
                Dictionary<string, string> anIncorrect2Correct)
            {
                mIncorrect2Correct = anIncorrect2Correct; // We need to remember it for when
                // the compare function is called...

                int len = anIncorrect2Correct.Count;

                // 2016-01-31. XXXXXXXX

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
            //*    Note: The input is keys, not an actual values                         *
            //*                                                                          *
            //****************************************************************************
            public int Compare(int aItem1, int aItem2)
            {
                int toReturn = 0; //Default: equal.

                if (aItem1 != aItem2)
                {
                    // The unique key, the incorrect term, is the secondary key.
                    //
                    string secondary1 = mKeys[aItem1];
                    string secondary2 = mKeys[aItem2];

                    // The correct term is the primary key.
                    string primary1 = mIncorrect2Correct[secondary1];
                    string primary2 = mIncorrect2Correct[secondary2];

                    int compareResult_primary = primary1.CompareTo(primary2);

                    if (compareResult_primary != 0)
                    {
                        toReturn = compareResult_primary; //Ascending sort for primary key.
                    }
                    else
                    {
                        //Same primary key - use second key: incorrect term

                        int compareResult_secondary = secondary1.CompareTo(secondary2);

                        if (compareResult_secondary != 0)
                        {
                            toReturn = compareResult_secondary; //Ascending sort for secondary key.
                        }
                        else
                        {
                            // Both keys are equal...
                            //
                            // We should never be here as the secondary
                            // key is unique. ASSERT?
                            Utility.debuggerRest();

                        } //Same XYZ, use second key: ABC
                    } //JJJJJ compare
                }
                else
                {
                    Utility.debuggerRest(); // Indexes equal. Does this ever
                    // happen??? Yes! - apparently the
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
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string correctionInfoStr2(string aBadTerm, string aCorrectedTerm)
        {
            return "bad term '" +
                   aBadTerm +
                   "' (corrected term is '" + aCorrectedTerm + "').";
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string URLmappingStr2(string aCorrectedTerm, string aURL)
        {
            return "correct term '" +
                   aCorrectedTerm +
                   "' (URL is '" + aURL + "').";
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static string escapeSQL(string aStringForSQL)
        {
            Trace.Assert(aStringForSQL != null);

            return aStringForSQL.Replace("'", "''");
        } //escapeSQL()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static void addTermsToOutput_SQL(string aBadTerm2,
                                                 string aCorrectedTerm2,
                                                 ref StringBuilder aSomeScratch,
                                                 string aURL)
        {
            if (aBadTerm2.IndexOf("'") >= 0)
            {
                Utility.debuggerRest();
            }

            string effectiveBadTerm = aBadTerm2;
            string effectiveCorrectedTerm = aCorrectedTerm2;

            // Escaping of SQL. For now, only backslash. We need
            // more, though. We can refactor and reduce the
            // redundancy in the below when we generalise.
            //
            // In any case, we should have some unit test, including
            // when the backslash is at the beginning or the end of
            // the string.
            //
            if (effectiveBadTerm.IndexOf(@"\") >= 0)
	        {
                effectiveBadTerm = effectiveBadTerm.Replace(@"\", @"\\");
          	}
            if (effectiveCorrectedTerm.IndexOf(@"\") >= 0)
            {
                effectiveCorrectedTerm = effectiveBadTerm.Replace(@"\", @"\\");
            }

            aSomeScratch.Append("INSERT INTO EditOverflow\n");
            aSomeScratch.Append("  (incorrectTerm, correctTerm, URL)\n");
            aSomeScratch.Append("  VALUES(");

            aSomeScratch.Append("'");
            aSomeScratch.Append(escapeSQL(effectiveBadTerm));
            aSomeScratch.Append("', '");

            aSomeScratch.Append(escapeSQL(effectiveCorrectedTerm));
            aSomeScratch.Append("', '");

            // Example of where escape of single quotes is necessary:
            //
            //   https://en.wiktionary.org/wiki/couldn't#Contraction
            //
            aSomeScratch.Append(escapeSQL(aURL));

            aSomeScratch.Append("');");

            aSomeScratch.Append("\n\n");
        } //addTermsToOutput_SQL()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static void addTermsToOutput_HTML(string aBadTerm,
                                                  string aCorrectedTerm,
                                                  ref HTML_builder aInOutBuilder,
                                                  string aURL)
        {
            aInOutBuilder.singleLineTagOnSeparateLine(
                "tr",
                " " +
                  aInOutBuilder.singleLineTagStr("td", aBadTerm) + " " +
                  aInOutBuilder.singleLineTagStr("td", aCorrectedTerm) + " " +
                  aInOutBuilder.singleLineTagStr("td", aURL) + " "
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
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static void addToAssociativeArray_JavaScript(
            string aVariableName, string aKey, string aValue, ref StringBuilder aSomeScratch)
        {
            // We need to escape double quotes. Example (incorrect term):
            //
            //     Mac OS X (10.6 "Snow Leopard")
            //

            //Only the incorrect term for now (for the primary mapping).
            //
            aKey = aKey.Replace(@"""", @"\""");

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
        private static void addTermsToOutput_JavaScript(string aBadTerm2,
                                                        string aCorrectedTerm2,
                                                        ref StringBuilder aSomeScratch,
                                                        string aURL)
        {
            string effectiveBadTerm = aBadTerm2;
            string effectiveCorrectedTerm = aCorrectedTerm2;

            // Example:
            //
            //   incorrect2correct["ZX 81"] = "ZX 81";
            //
            //   correct2URL["ZX81"] = "https://en.wikipedia.org/wiki/ZX81";


            addToAssociativeArray_JavaScript("incorrect2correct",
                                             effectiveBadTerm,
                                             effectiveCorrectedTerm,
                                             ref aSomeScratch);

            // Assume that this function is called for the identity mapping
            if (aBadTerm2 == aCorrectedTerm2)
            {

                addToAssociativeArray_JavaScript("correct2URL",
                                                 effectiveCorrectedTerm,
                                                 aURL,
                                                 ref aSomeScratch);
            }

        } //addTermsToOutput_JavaScript()


        /****************************************************************************
         *                                                                          *
         *   Encapsulate the loop where we run through the whole word list (of      *
         *   incorrect terms) in a certain order. E.g., to generate the row         *
         *   for an HTML table                                                      *
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
            ref Dictionary<string, string> aCorrectTerm2URL)
        {
            SortByCorrectThenIncorrect_usingIndex sortObject =
                new SortByCorrectThenIncorrect_usingIndex(anIncorrect2Correct);

            List<int> indexes = sortObject.indexes(); // Unsorted at this point

            indexes.Sort(sortObject); // After: Those indexes are now
            //        sorted such that looking up the keys in order in
            //        mIncorrect2Correct will return the entries in the
            //        given defined sort order, in this case the
            //        correct term as the primary key and the
            //        incorrect term in as the secondary key.
            //        In order words, grouping by the
            //        correct term...

            List<string> someKeys_incorrectTerms = sortObject.keys();

            string prevCorrectTerm = "";

            HTML_builder builder = new HTML_builder();
            for (int i = 0; i < 3; i++)
            {
                builder.indentLevelUp();
            }

            foreach (int someIndex in indexes)
            {
                string someIncorrectTerm = someKeys_incorrectTerms[someIndex];
                string someCorrectTerm = anIncorrect2Correct[someIncorrectTerm];

                string msg = string.Empty; // Default: empty - flag for no errors.


                // On-the-fly check (but it would be better if
                // this check was done at program startup)
                if (aCorrectTerm2URL.ContainsKey(someIncorrectTerm))
                {
                    msg =
                      "Incorrect term \"" + someIncorrectTerm +
                      "\" has been entered as a correct term...";
                }

                string someURL = null;
                if (!aCorrectTerm2URL.TryGetValue(someCorrectTerm, out someURL))
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
                    // For now, we just check if they start with "http"...
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
                        msg =
                          "A URL, <" + someURL + ">, is a search query. " +
                          "This is not allowed.";
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
                                                  someURL);
                            break;

                        case wordListOutputTypeEnum.SQL:

                            addTermsToOutput_SQL(someIncorrectTerm,
                                                 someCorrectTerm,
                                                 ref aSomeScratch,
                                                 someURL);

                            // Add the identity mapping (so we don't special
                            // code (and a second database lookup) for
                            // looking up the correct term), but only once.
                            //
                            // We can rely on the sorted order, first by
                            // incorrect and then correct. Thus, we will
                            // get a corrected term one or more times
                            // consecutively.
                            if (prevCorrectTerm != someCorrectTerm)
                            {
                                // "Identity mapping" - a lookup of a correct
                                // term should also succeeed - e.g. if we only
                                // want to get the URL.
                                addTermsToOutput_SQL(someCorrectTerm,
                                                     someCorrectTerm,
                                                     ref aSomeScratch,
                                                     someURL);
                            }
                            break;

                        case wordListOutputTypeEnum.JavaScript:

                            addTermsToOutput_JavaScript(someIncorrectTerm,
                                                        someCorrectTerm,
                                                        ref aSomeScratch,
                                                        someURL);

                            // Once per correct term:
                            //
                            // 1. Identity mapping (so we don't need special
                            //    code for looking up correct terms)
                            //
                            // 2. Establish the mapping from correct term to URL
                            //
                            if (prevCorrectTerm != someCorrectTerm)
                            {

                                // "Identity mapping" - a lookup of a correct
                                // term should also succeeed - e.g., if we only
                                // want to get the URL.
                                //
                                // Also implicitly adds the mapping to the URL
                                //
                                addTermsToOutput_JavaScript(someCorrectTerm,
                                                            someCorrectTerm,
                                                            ref aSomeScratch,
                                                            someURL);
                                // Delineate groups

                                aSomeScratch.Append("\n\n");
                            }
                            break;

                        default:
                            Trace.Assert(false); // Fall-through. Not allowed.
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
            int correctWordEntries = mCorrectTerm2URL.Count;
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
                           ref mCorrectTerm2URL);

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
                  aInOutBuilder.singleLineTagStr("th", "Incorrect") + " " +
                  aInOutBuilder.singleLineTagStr("th", "Correct") + " " +
                  aInOutBuilder.singleLineTagStr("th", "URL") + " "
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

            //Headline

            //Later: From the program (when we generalise
            //       these wrappings in HTML tags)
            //builder.singleLineTagWithEmptyLine("h2", "Code formatting check");
            aInOutBuilder.addHeader(2, "Code formatting check");

            aInOutBuilder.addParagraph(
                "&nbsp;Regular expression " +
                "(rudimentary, high false-positive rate - " +
                "every hit should be checked manually): <br/>" +
                aCodeCheck_regularExpression);

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
                "<a href=\"http://pmortensen.eu/world/EditOverflow1.html\">" +
                "Edit Overflow for Web</a>"
            );

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"http://validator.w3.org/check?uri=" +
                presumedURL +
                "&amp;charset=utf-8\">" +
                "HTML validation</a>"
            );

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"http://pmortensen.eu/\">" +
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
         *          anything - it is for output (in the HTML content).              *
         *                                                                          *
         *      2. This function, with six parameters, is only used (directly,      *
         *         as a public function) by some regression tests to get a          *
         *         stable output (independent of the ever-growing word list).       *
         *                                                                          *
         ****************************************************************************/
        public static string dumpWordList_asHTML(
            string aCodeCheck_regularExpression,
            ref Dictionary<string, string> anIncorrect2Correct,
            int aUniqueWords,
            ref Dictionary<string, string> aCorrectTerm2URL,
            string aVersionStr,
            string aDateStr
            )
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
            int correctWordEntries = aCorrectTerm2URL.Count;
            int incorrectWordEntries = anIncorrect2Correct.Count;

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
                           ref aCorrectTerm2URL);
            // The main side effect is the changing of the content
            // of ref HTML_tableRows...


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

            endOfHTML_document(ref builder, aCodeCheck_regularExpression, aDateStr);

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
                                          string aVersionStr,
                                          string aDateStr)
        {
            return dumpWordList_asHTML(
                      aCodeCheck_regularExpression,
                      ref mIncorrect2Correct,
                      mCorrectTerm2URL.Count,
                      ref mCorrectTerm2URL,
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
            int correctWordEntries = mCorrectTerm2URL.Count;
            int incorrectWordEntries = mIncorrect2Correct.Count;

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
                           ref mCorrectTerm2URL);

            // For unit testing under Jest (which runs under Node.js)
            sb.Append("\n\n\n\n");
            sb.Append("// For JavaScript unit testing with Jest under Node.js to work...\n");
            sb.Append("// It is harmless when used in a web browser context.\n");
            sb.Append("module.exports = incorrect2correct;\n");

            return JavaScript_codeSB.ToString();
        } //dumpWordList_asJavaScript()


    } //class TermLookup


} //namespace OverflowHelper.core

