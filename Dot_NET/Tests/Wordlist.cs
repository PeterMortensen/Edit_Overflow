

using System.Collections.Generic; //For Dictionary.


using NUnit.Framework; //For all versions of NUnit, 
//file "nunit.framework.dll"

using OverflowHelper.core;


namespace OverflowHelper.Tests
{


    /****************************************************************************
     *                                                                          *
     *    The word list for correction, incl. export to HTML and SQL            *
     *                                                                          *
     ****************************************************************************/
    [TestFixture]
    class WordlistTests
    {


        /****************************************************************************
         *                                                                          *
         *    Intent: More like a regression test (detect (unexpected) changes)     *
         *            for the HTML export result than a unit test.                  *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void HTMLexport_emptyWordList()
        {
            // In this test, we call it with all empty variable information 
            // to get a stable output for regression testing (get skeleton
            // HTML, with only the begining and end, with an empty
            // word table)
            Dictionary<string, string> someCaseCorrection =
               new Dictionary<string, string>();
            Dictionary<string, string> someWord2URL =
                new Dictionary<string, string>();
            Dictionary<string, string> someCaseCorrection_Reverse =
                new Dictionary<string, string>();
            string Wordlist_HTML =
              WikipediaLookup.dumpWordList_asHTML(
                "",
                ref someCaseCorrection,
                ref someWord2URL,
                ref someCaseCorrection_Reverse);

            int len = Wordlist_HTML.Length;

            // Poor man's hash: check length (later, use a real 
            // hashing function). At least it should catch that 
            // indentation in the HTML source is not broken 
            // by changes (e.g. refactoring).
            //
            Assert.AreEqual(2708, len, "XYZ");
        } //HTMLexport_emptyWordList()


        /****************************************************************************
         *                                                                          *
         *    Intent: More like a regression test (detect (unexpected) changes)     *
         *            for the HTML export result than a unit test.                  *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void HTMLexport_fixedWordList()
        {
            // In this test, we use a small number of fixed items in the word
            // list to get a stable output for regression testing. 
            //
            // In particular, we use items that would fail to display correctly
            // on a web page due to the wrong encoding (should be UTF-8).
            //

            //Not yet...
            // 
            // Dictionary<string, string> someCaseCorrection =
            //    new Dictionary<string, string>();
            // Dictionary<string, string> someWord2URL =
            //     new Dictionary<string, string>();
            // Dictionary<string, string> someCaseCorrection_Reverse =
            //     new Dictionary<string, string>();
            // string Wordlist_HTML =
            //   WikipediaLookup.dumpWordList_asHTML(
            //     "",
            //     ref someCaseCorrection,
            //     ref someWord2URL,
            //     ref someCaseCorrection_Reverse);
            // 
            // int len = Wordlist_HTML.Length;
            // 
            // // Poor man's hash: check length (later, use a real 
            // // hashing function). At least it should catch that 
            // // indentation in the HTML source is not broken 
            // // by changes (e.g. refactoring).
            // //
            // Assert.AreEqual(9999, len, "XYZ");
        } //HTMLexport_emptyWordList()


    } //class StringReplacerWithRegexTests 


} //namespace OverflowHelper.Tests


