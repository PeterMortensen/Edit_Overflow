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
         ****************************************************************************/
        public void unchangedLookup(string aCorrectWord)
        {
            LookUpString tt2 = new LookUpString(aCorrectWord);

            Assert.AreEqual(
              aCorrectWord,
              tt2.getCoreString(),
              "Trailing characters misidentified as punctuation: " +
                aCorrectWord); // Hopefully enough to identify the caller...

        } //unchangedLookup()


        /****************************************************************************
         *                                                                          *
         *  Testing some helper classes                                             *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void preprocessing()
        {
            // Preprocessing as in stripping some leading 
            // and trailing characters (or not)
            
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
            // are ***not*** stripped (correct words 
            // only for these tests).
            //
            unchangedLookup("Mac&nbsp;OS&nbsp;X&nbsp;v10.2 (Jaguar)");
            unchangedLookup("&mdash;");
            unchangedLookup("'like button'");
            unchangedLookup("Hello, World!");
            unchangedLookup("Sagittarius A*");
            unchangedLookup("[sic]");
            unchangedLookup("`man bash`");
            unchangedLookup("How Is Babby Formed?");
            unchangedLookup("!==");
            unchangedLookup("polkadot{.js}");
            unchangedLookup("/e/");
            unchangedLookup("--");
            unchangedLookup("%"); // Only one character. This works
                                  // for some reason... (without
                                  // needing an exception). Why?
            unchangedLookup("C♯");
            unchangedLookup("M$");
            unchangedLookup("8 \"Jessie\"");
            unchangedLookup("~"); // Only one character. This works
                                  // for some reason... (without
                                  // needing an exception). Why?
            unchangedLookup("2¢");
            unchangedLookup("<"); // Only one character. This works
                                  // for some reason... (without
                                  // needing an exception). Why?
            unchangedLookup("Stack Overflow на русском");

            unchangedLookup("voilà");
            unchangedLookup("Bogotá");
            unchangedLookup("Antonio Radić");
            unchangedLookup("fiancé");
            unchangedLookup("Baháʼí");
            unchangedLookup("Stack Overflow на русском");
            unchangedLookup("Malmö");

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


