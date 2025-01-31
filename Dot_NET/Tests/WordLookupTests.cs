/****************************************************************************
 * Copyright (C) 2022 Peter Mortensen                                       *
 *                                                                          *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: Unit testing for the central function of Edit Overflow,         *
 *          word lookup. It is not tied to a particular class               *
 *          (we have several levels of lookup).                             *
 *                                                                          *
 ****************************************************************************/


using NUnit.Framework; //For all versions of NUnit,
                       //file "nunit.framework.dll"

using OverflowHelper.core;


//What namespace should we use?
/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace WordLookupTestsTests
{

    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    [TestFixture]
    public class WordLookupTestsTests
    {


        /****************************************************************************
         *                                                                          *
         *  The simplest test, that provided by class WordCorrector                 *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void baseLookup()
        {
            {
                WordCorrector someWordCorrector = new WordCorrector();
                lookupResultStructure lookupResult =
                    someWordCorrector.lookup_Central("php", false, true);

                Assert.AreEqual(
                  @"PHP",
                  lookupResult.correctedText,
                  "");
            }

            // Check that some valid words to look up do not get
            // a trailing or a leading character(s) stripped off
            // before the look up and thus fails the lookup
            // (false negative).
            {
                WordCorrector someWordCorrector = new WordCorrector();
                lookupResultStructure lookupResult =
                    someWordCorrector.lookup_Central("Sgr A*", false, true);

                Assert.AreEqual(
                  @"Sagittarius A*",
                  lookupResult.correctedText,
                  "");
            }


            // Can we actually correctly look up words
            // in the alternative word set?
            //
            // Note: This makes presumptions about the content
            //       of the word list.
            {
                WordCorrector someWordCorrector = new WordCorrector();
                lookupResultStructure lookupResult =
                    someWordCorrector.lookup_Central("academia_", false, true);

                Assert.AreEqual(
                  @"Academia (Stack Exchange site)_",
                  lookupResult.correctedText,
                  "");
            }

        } //baseLookup()


        /****************************************************************************
         *                                                                          *
         *  Helper for preprocessing()                                              *
         *                                                                          *
         *  Note: Not the lookup itself, but the automatic filtering of             *
         *        some leading trailing and trailing characters. So in              *
         *        a way, it belongs in file 'WordLookupTests.cs'...                 *
         *                                                                          *
         ****************************************************************************/
        public void unchangedLookupTerm(string aCorrectWord)
        {
            LookUpString tt2 = new LookUpString(aCorrectWord);

            Assert.AreEqual(
              aCorrectWord,
              tt2.getCoreString(),
              "Trailing characters misidentified as punctuation: " +
                aCorrectWord); // Hopefully enough to identify the caller...

        } //unchangedLookupTerm()


        /****************************************************************************
         *                                                                          *
         *  Testing some helper classes                                             *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void preprocessing()
        {
            // Preprocessing as in stripping some leading
            // and trailing characters (or not)...

            // Characteristics of members of the alternative word
            // set should not be seen as punctuation... (Or
            // should be encoded internally in a different way.)
            {
                LookUpString tt2 = new LookUpString("reason_");

                Assert.AreEqual(
                  @"reason_",
                  tt2.getCoreString(),
                  "");
            }

            // Can we actually correctly look up words
            // in the alternative word set?
            {
                WordCorrector someWordCorrector = new WordCorrector();
                lookupResultStructure lookupResult =
                    someWordCorrector.lookup_Central("academia_", false, true);

                Assert.AreEqual(
                  @"Academia (Stack Exchange site)_",
                  lookupResult.correctedText,
                  "");
            }

            // Test that leading and trailing characters
            // are ***not*** stripped (we can use it on
            // both incorrect and correct words, as it
            // has nothing to do with the actual word
            // lookup/correction).
            //
            // Incorrect words
            unchangedLookupTerm("8 \"Jessie\"");
            unchangedLookupTerm("8 \"Jessie\"____");
            unchangedLookupTerm("M$");
            unchangedLookupTerm("M$__");
            unchangedLookupTerm("--");
            unchangedLookupTerm("<br>");
            unchangedLookupTerm("C♯");
            unchangedLookupTerm("2¢");
            unchangedLookupTerm("%"); // Only one character. This works
                                      // for some reason... (without
                                      // needing an exception). Why?
            unchangedLookupTerm("~"); // Only one character. This works
                                      // for some reason... (without
                                      // needing an exception). Why?
            unchangedLookupTerm("<"); // Only one character. This works
                                      // for some reason... (without
                                      // needing an exception). Why?
            unchangedLookupTerm("Stack Overflow на русском");
            //
            // Correct words
            unchangedLookupTerm("C#");
            unchangedLookupTerm("C++");
            unchangedLookupTerm("'like button'");
            unchangedLookupTerm("'like button'_");
            unchangedLookupTerm("I’");
            unchangedLookupTerm("Mac&nbsp;OS&nbsp;X&nbsp;v10.2 (Jaguar)");
            unchangedLookupTerm("&mdash;");
            unchangedLookupTerm("Hello, World!");
            unchangedLookupTerm("Sagittarius A*");
            unchangedLookupTerm("[sic]");
            unchangedLookupTerm("`man bash`");
            unchangedLookupTerm("How Is Babby Formed?");
            unchangedLookupTerm("!==");
            unchangedLookupTerm("polkadot{.js}");
            unchangedLookupTerm("/e/");
            unchangedLookupTerm("voilà");
            unchangedLookupTerm("Bogotá");
            unchangedLookupTerm("Antonio Radić");
            unchangedLookupTerm("fiancé");
            unchangedLookupTerm("Baháʼí");
            unchangedLookupTerm("Malmö");
            unchangedLookupTerm("Perú");
            unchangedLookupTerm("€");

            // Indirect check that incorrect word "*" is not stripped
            // before the actual search: If it is, the lookup will
            // fail
            {
                WordCorrector someWordCorrector = new WordCorrector();
                lookupResultStructure lookupResult =
                    someWordCorrector.lookup_Central("*", false, true);

                Assert.AreEqual(
                  @"asterisk",
                  lookupResult.correctedText,
                  "");
            }

            // Indirect check that incorrect word '"' (double quote)
            // is not stripped before the actual search: If it is,
            // the lookup will fail
            {
                WordCorrector someWordCorrector = new WordCorrector();
                lookupResultStructure lookupResult =
                    someWordCorrector.lookup_Central("\"", false, true);

                Assert.AreEqual(
                  @"double quote",
                  lookupResult.correctedText,
                  "");
            }

        } //preprocessing()


    } //class WordLookupTestsTests


} //namespace WordLookupTestsTests


