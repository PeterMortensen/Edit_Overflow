/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 * Note: "wordlist" here refers to the Edit Overflow word list              *
 *       (mapping from incorrect words to correct words)                    *
 *                                                                          *
 *                                                                          *
 * Purpose: Regression testing for word list output                         *
 *          generation (mainly HTML).                                       *
 *                                                                          *
 *          For example, detect inadvertent changes to the HTML             *
 *          generation, including encoding changes (UTF-8, etc.)            *
 *                                                                          *
 ****************************************************************************/


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
         *    Intent: State assumptions about String.Replace(), including for       *
         *            special characters, like "{"                                  *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void StringReplace()
        {
            string str1 = "xyx";
            Assert.AreEqual(str1.Replace("x", "Z"), "ZyZ", "BBB"); // Detect
            Assert.AreEqual("xyx".Replace("x", "Z"), "ZyZ", "BBB"); // Detect

            Assert.AreEqual("{".Replace("{", "<strong>{</strong>"), "<strong>{</strong>", "BBB"); // Detect

        } //StringReplace()


        /****************************************************************************
         *                                                                          *
         *    Helper function for some of the tests. To reduce redundancy.          *
         *                                                                          *
         *    It also isolates the Unix / Windows dependency.                       *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        private string wordListAsHTML(
            Dictionary<string, string> aSomeCaseCorrections,
            Dictionary<string, string> aSomeWord2URLs,
            Dictionary<string, int> aCorrect2Count)
        {
            //EditorOverflowApplication app = new EditorOverflowApplication_Windows();
            EditorOverflowApplication app = new EditorOverflowApplication_Unix();

            string Wordlist_HTML =
              TermLookup.dumpWordList_asHTML(
                "",
                ", + , operators , {", // Some of it will be transformed...
                ref aSomeCaseCorrections,
                aSomeWord2URLs.Count,
                ref aSomeWord2URLs,
                ref aCorrect2Count,

                //This is equivalent, for the refactoring, but
                //should we use fixed or empty strings instead??
                app.fullVersionStr(),
                app.versionString_dateOnly()
              );

            return Wordlist_HTML;
        } //wordListAsHTML()


        /****************************************************************************
         *                                                                          *
         *    Helper function for some of the tests                                 *
         *                                                                          *
         *    It presumes the HTML source is formatted in a certain                 *
         *    way (it is not a generally applicable function).                      *
         *    E.g., for now, we assume a particular (internal)                      *
         *    space indent).                                                        *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        private string extractHTMLtable(string aSomeHTML)
        {
            // Rudimentary, but some of the tests in this file ***will***
            // detect if we get some false positive matches (e.g., due
            // to a later change).

            //"<table>"


            //Later: Complain if not found (with a specific error message, etc.)

            // Isn't there a simple way than all these calculations?
            //const string startToken = "        <table>";

            // End of the HTML table header and an empty
            // separating it from the table itself...
            const string startToken = "</th> </tr>\n\n";

            int startIndex = aSomeHTML.IndexOf(startToken) + startToken.Length;
            int endIndex = aSomeHTML.IndexOf("        </table>");
            int len = endIndex - startIndex;

            string toReturn = aSomeHTML.Substring(startIndex, len);


            //Stub!!!!
            //return "YYYYYYYYYYYYYYYYYYYYYYYYYYYYY";
            return toReturn;
        } //extractHTMLtable()


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
            // word table. And empty XXX)
            //
            Dictionary<string, string> someCaseCorrections =
               new Dictionary<string, string>();
            Dictionary<string, string> someWord2URLs =
                new Dictionary<string, string>();
            Dictionary<string, int> someCorrect2Count =
                new Dictionary<string, int>();

            string Wordlist_HTML =
                wordListAsHTML(
                    someCaseCorrections,
                    someWord2URLs,
                    someCorrect2Count);

            int len = Wordlist_HTML.Length;

            // Poor man's hash: check length (later, use a real
            // hashing function). At least it should catch that
            // indentation in the HTML source is not broken
            // by changes (e.g. refactoring) and unintended
            // omissions deletions.
            //
            // But it will not detect single spaces replaced by single TAB...
            //
            Assert.AreEqual(
                2708 + 3 + 1 + 12 - 10 + 1 + 8 - 22 + 9 - 2 - 1 - 1 + 1 +
                    404 + 153 +
                    36 + 85 + 4 +
                    2 +
                    177 + 6 +
                    37 + 39 +
                    1 + 10 + 2 +
                    17 + 17 +
                    3 * (18 + 2 + 1) +  //2 is for CR+LF. 3 is the number of items (separated by comma)
                    -3 +
                    -6 +
                    5 * -6 +  // 5 is 3 + 2 (3 is number of items and 2 is fixed)
                    8 + 4 + 3 + 2*1 + 1 + 91 + 1 + 1 + // Two empty lines + indent + HTML comment syntax + space + some HTML comment + newline +
                    93 - 93 +
                    2 +
                    7 +
                    0,
                len,
                "XYZ");
            //    +3 because we discovered and eliminated a tab...
            //    +1 because changed the HTML slightly...
            //   +12 because the end tag for "head" was missing...
            //   -10 because we used for proper formatting for a <p> tag...
            //    +1 because we made the HTML look more like in the browser...
            //    +8 because we fixed indentation for the HTML source for list items...
            //   -22 because we made the formatting for <p> tags consistent...
            //    +9 because we fixed the indentation for <hr/>...
            //    -2 because we made the formatting for the <table> tag consistent...
            //    -1 because we made the formatting for the table header
            //       line consistent...
            //    -1 because we made the formatting for the table end
            //       tag consistent...
            //    +1 because we made the formatting for <h2> and <p>
            //       tags consistent...
            //  +404 For a new baseline, after changes to the end of
            //       the HTML content.
            //  +153 because we added a justification for the existence
            //       of the word list...
            //   +36 because we changed the formatting of the CSS...
            //   +85 because we added a link to the web version
            //       of Edit Overflow...
            //    +4 Because we went to development mode again (after
            //       the release 2019-11-01)... But why +4 (the extra
            //       "a3" counts two times).
            //    +2 New version number.
            //  +177 New paragraph added to the beginning
            //    +6 The longest incorrect term changed
            //   +37 Different static HTML content close to "Code formatting check"
            //   +39 Slightly different formatting/punctuation and internal HTML formatting.
            //   +11 Using a non-empty code regular expression explanation (something
            //       that is going to transformed (change in length))
            //   +17 Bold formatting for some special characters, like "}"
            //       (in the code regular expression explanation).
            //   +17 For a non-empty code regular expression explanation.
            //   +63 Internal linebreaks and indentation for code regular
            //       expression explanations (and some extra space). 3
            //       (simulated) items in the example input.
            //    -3 A problem with an uneven number of indent spaces was fixed...
            //    -6 We removed a weird leading (HTML) non-break space...
            //   -30 We changed the indent to be regular in the generated
            //       HTML (***not*** trying to match the indentation in
            //       file "FixedStrings.php")
            //  +110 Added an (HTML) comment
            //    +0 For Git shenanigans...
            //    +2 For alfa version 99 -> 100...
            //    +7 For making the code checking headline compatible
            //       with the keyboard shortcut on the fixed string
            //       page...

            Assert.AreEqual(Wordlist_HTML.IndexOf("\t"), -1, "XYZ"); // Detect
            // any TABs...

            System.Console.WriteLine(Wordlist_HTML);
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

            Dictionary<string, string> someCaseCorrections =
               new Dictionary<string, string>();
            Dictionary<string, string> someWord2URLs =
                new Dictionary<string, string>();
            Dictionary<string, int> someCorrect2Count =
                new Dictionary<string, int>();

            // First
            someCaseCorrections.Add("JS", "JavaScript");
            someWord2URLs.Add(
                "JavaScript",
                "https://en.wikipedia.org/wiki/JavaScript");
            someCorrect2Count.Add("JavaScript", 42);

            // Second
            someCaseCorrections.Add("angstrom", "Ångström Linux");
            someWord2URLs.Add(
                "Ångström Linux",
                "https://en.wikipedia.org/wiki/%C3%85ngstr%C3%B6m_distribution");
            someCorrect2Count.Add("Ångström Linux", 6);

            // Third
            someCaseCorrections.Add("utorrent", "µTorrent");
            someWord2URLs.Add(
                "µTorrent", "http://en.wikipedia.org/wiki/%CE%9CTorrent");
            someCorrect2Count.Add("µTorrent", 1);

            int incorrectWords = someCaseCorrections.Count;

            string Wordlist_HTML =
                wordListAsHTML(
                    someCaseCorrections,
                    someWord2URLs,
                    someCorrect2Count);

            int len = Wordlist_HTML.Length;

            // Poor man's hash: check length (later, use a real
            // hashing function). At least it should catch that
            // indentation in the HTML source is not broken
            // by changes (e.g. refactoring) and unintended
            // omissions deletions.
            //
            // But it will not detect single spaces
            // replaced by single TAB...
            //
            Assert.AreEqual(
                3572 - 24 + 153 +
                    36 + 85 + 4 +
                    2 +
                    177 + 6 +
                    37 + 39 +
                    1 + 10 + 2 +
                    17 + 17 +
                    3 * 20 + 3 +
                    -3 +
                    -6 +
                    5 * -6 +  // 5 is 3 + 2 (3 is number of items and 2 is fixed)
                    8 + 4 + 3 + 2*1 + 1 + 91 + 1 + 1 + // Two empty lines + indent + HTML comment syntax + space + some HTML comment + newline +
                    93 - 93 +
                    83 +
                    incorrectWords * -2 +
                    2 +
                    7 +
                    13 +
                    0,
                len,
                "XYZ");
            //   -24 because we removed unnecessary space...
            //  +153 because we added a justification for the existence
            //       of the word list...
            //   +36 because we changed the formatting of the CSS...
            //   +85 because we added a link to the web version
            //       of Edit Overflow...
            //    +4 Because we went to development mode again (after
            //       the release 2019-11-01)... But why +4 (the extra
            //       "a3" counts two times).
            //    +2 New version number.
            //  +177 New paragraph added to the beginning
            //    +6 The longest incorrect term changed
            //   +37 Different static HTML content close to "Code formatting check"
            //   +39 Slightly different formatting/punctuation and internal HTML formatting.
            //   +13 Using a non-empty code regular expression explanation (something
            //       that is going to transformed (change in length))
            //   +17 Bold formatting for some special characters, like "}"
            //       (in the code regular expression explanation)
            //   +17 For a non-empty code regular expression explanation.
            //   +63 Internal linebreaks and indentation for code regular
            //       expression explanations (and some extra space). 3
            //       (simulated) items in the example input.
            //    -3 A problem with an uneven number of indent spaces was fixed...
            //    -6 We removed a weird leading (HTML) non-break space...
            //   -30 We changed the indent to be regular in the generated
            //       HTML (***not*** trying to match the indentation in
            //       file "FixedStrings.php")
            //  +110 Added an (HTML) comment
            //    +0 For Git shenanigans...
            //   +83 For adding an anchor to some lines in the table
            //    -6 For removal of two spaces per HTML row
            //    +2 For alfa version 99 -> 100...
            //    +7 For making the code checking headline compatible
            //       with the keyboard shortcut on the fixed string
            //       page...
            //    +13 For temporary extra output for count of misspellings

            Assert.AreEqual(Wordlist_HTML.IndexOf("\t"), -1, "XYZ"); // Detect
            // any TABs...
        } //HTMLexport_fixedWordList()


        /****************************************************************************
         *                                                                          *
         *    Intents:                                                              *
         *                                                                          *
         *      1. Detect ***double*** HTML encoding of word list items             *
         *                                                                          *
         *      2. More generally, the generated HTML for word list mappings        *
         *         are exactly as expected, incl. any special encoding/             *
         *         escaping for it to be valid HTML                                 *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void HTMLexport_WordlistItems()
        {
            // Four HTML table rows (2021-11-09) for the correct
            // word "&lt;" (without the space indent):
            //
            //     <tr> <td><div id="&lt;"></div>&amp;LT;</td><td>&amp;lt;</td><td>https://www.w3.org/wiki/Common_HTML_entities_used_for_typography</td> </tr>
            //     <tr> <td>&lt;</td><td>&amp;lt;</td><td>https://www.w3.org/wiki/Common_HTML_entities_used_for_typography</td> </tr>
            //     <tr> <td>less-than sign</td><td>&amp;lt;</td><td>https://www.w3.org/wiki/Common_HTML_entities_used_for_typography</td> </tr>
            //     <tr> <td>lt</td><td>&amp;lt;</td><td>https://www.w3.org/wiki/Common_HTML_entities_used_for_typography</td> </tr>

            Dictionary<string, string> someCaseCorrections =
               new Dictionary<string, string>();
            Dictionary<string, string> someWord2URLs =
                new Dictionary<string, string>();
            Dictionary<string, int> someCorrect2Count =
                new Dictionary<string, int>();

            // Detect if we get double HTML encoding (an error - not what
            // we want. For example, order matters in escapeHTML()
            // in file *main/TermLookup.cs*.)
            {
                // The real item
                someCaseCorrections.Add("<", "&lt;");

                //Disabled for now - for more than one item we need a helper
                //function to pull out a single row from the HTML table.
                //When we do, we should also add a test to test (regress)
                //for the sort order in the output.
                //
                //Until then, we need to include the HTML anchor in the
                //expected result.
                //
                //The helper function should strip the indent space and the
                //end of line (so the client side becomes simple and less
                //redundant).
                //
                //// To push the real item to the second place in the
                //// (defined) sort order in the HTML output (to avoid
                //// the HTML anchor).
                //someCaseCorrections.Add("&LT;", "&lt;");

                // Only once per correct item
                someWord2URLs.Add(
                    "&lt;",
                    "https://www.w3.org/wiki/Common_HTML_entities_used_for_typography");
                someCorrect2Count.Add("&lt;", 3);

                string Wordlist_HTML =
                    wordListAsHTML(
                        someCaseCorrections,
                        someWord2URLs,
                        someCorrect2Count);

                string table = extractHTMLtable(Wordlist_HTML);

                const string expectedIndent = "            ";

                //Not yet. See above.
                //Assert.AreEqual(
                //  expectedIndent +
                //    "<tr> <td>&lt;</td><td>&amp;lt;</td><td>https://www.w3.org/wiki/Common_HTML_entities_used_for_typography</td> </tr>",
                //  table, "XYZ");
                Assert.AreEqual(
                  expectedIndent +
                    //"<tr> <td><div id=\"&lt;\"></div>&lt;</td><td>&amp;lt;</td><td>https://www.w3.org/wiki/Common_HTML_entities_used_for_typography</td> </tr>\n",
                    "<tr> <td><div id=\"&lt;\"></div>&lt;</td><td>&amp;lt; (3)</td><td>https://www.w3.org/wiki/Common_HTML_entities_used_for_typography</td> </tr>\n",
                  table, "XYZ");
            }

        } //HTMLexport_WordlistItems()


        /****************************************************************************
         *                                                                          *
         *    Intent: Test the indent (internal, in the HTML source,                *
         *            not presentation) functionality of the HTML                   *
         *            builder                                                       *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void HTMLbuilder_indent()
        {
            HTML_builder builder = new HTML_builder();

            builder.indentLevelUp();
            builder.addContentWithEmptyLine("<head>");

            string HTMLcontent = builder.currentHTML();
            int len = HTMLcontent.Length;

            Assert.AreEqual("\n    <head>\n", HTMLcontent, "XYZ");
            Assert.AreEqual(12, len, "XYZ"); // Weaker, but the report will be
            // more clear (as it may be difficult to see with spaces).

        } //HTMLbuilder_indent()


        /****************************************************************************
         *                                                                          *
         *    Intent: Test the basic append operations on the HTML builder.         *
         *    Also test the defined side effect of reading out the HTML             *
         *    content.                                                              *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void HTML_builder_appendOperations()
        {
            HTML_builder builder = new HTML_builder();

            // Note: the readout, currentHTML(), resets the buffer. That
            //       is the reason we can reuse the single HTML_builder
            //       instance... But it only works if we don't use
            //       indents (directly or indirectly) - that is, if
            //       we do then the order of tests becomes important
            //       (and then they should be changed to be completely
            //        independent)
            //
            //

            {
                builder.addContent("<head>");

                string HTMLcontent1 = builder.currentHTML(); // Side effect!
                int len1 = HTMLcontent1.Length;

                Assert.AreEqual("<head>", HTMLcontent1, "XYZ");
                Assert.AreEqual(6, len1, "XYZ"); // Weaker, but the report
                // will be more clear (as it may be difficult spaces).
            }

            // With end of line at the end...
            {
                builder.addContentOnSeparateLine("<head>");

                string HTMLcontent2 = builder.currentHTML(); // Side effect!
                int len2 = HTMLcontent2.Length;

                Assert.AreEqual("<head>\n", HTMLcontent2, "XYZ");
                Assert.AreEqual(7, len2, "XYZ"); // Weaker, but the report
                // will be more clear (as it may be difficult spaces).
            }

            // HTML comments, combined with indents. Shifting this test
            // around also tests if the indent state is reset or not.
            //
            {
                HTML_builder builder2 = new HTML_builder();

                builder2.indentLevelUp();
                builder2.addComment("Some comment");

                string HTMLcontent4 = builder2.currentHTML(); // Side effect!
                int len4 = HTMLcontent4.Length;

                Assert.AreEqual("    <!-- Some comment -->\n", HTMLcontent4, "XYZ");
                Assert.AreEqual(26, len4, "XYZ"); // Weaker, but the report
                // will be more clear (as it may be difficult spaces).
            }

            // With end of line at the end and starting with an empty line
            {
                builder.addContentWithEmptyLine("<head>");

                string HTMLcontent3 = builder.currentHTML(); // Side effect!
                int len3 = HTMLcontent3.Length;

                Assert.AreEqual("\n<head>\n", HTMLcontent3, "XYZ");
                Assert.AreEqual(8, len3, "XYZ"); // Weaker, but the report
                // will be more clear (as it may be difficult spaces).
            }

        } //HTML_builder_appendOperations()


        /****************************************************************************
         *                                                                          *
         *    Intent: Test the HTML builder's startHTML() method.                   *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void HTML_builder_startHTML()
        {
            HTML_builder builder = new HTML_builder();
            builder.startHTML("Some title");

            string HTMLcontent1 = builder.currentHTML(); // Side effect!
            int len1 = HTMLcontent1.Length;

            // Poor man's hash: check length (later, use a real
            // hashing function). At least it should catch that
            // indentation in the HTML source is not broken
            // by changes (e.g. refactoring) and unintended omissions
            // deletions.
            //
            // But it will not detect single spaces replaced by single TAB...
            //
            Assert.AreEqual(157, len1, "XYZ");
            Assert.AreEqual(HTMLcontent1.IndexOf("\t"), -1, "XYZ"); // Detect
            // any TABs...


        } //HTML_builder_appendOperations()



    } //class StringReplacerWithRegexTests


} //namespace OverflowHelper.Tests



