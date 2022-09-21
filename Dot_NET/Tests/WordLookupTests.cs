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
                    someWordCorrector.lookup_Central("php", false);

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
                    someWordCorrector.lookup_Central("academia_", false);

                Assert.AreEqual(
                  @"Academia (Stack Exchange site)_",
                  lookupResult.correctedText,
                  "");
            }

        } //baseLookup()


        /****************************************************************************
         *                                                                          *
         *  Testing some helper classes                                             *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void preprocessing()
        {
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
                    someWordCorrector.lookup_Central("academia_", false);

                Assert.AreEqual(
                  @"Academia (Stack Exchange site)_",
                  lookupResult.correctedText,
                  "");
            }

        } //baseLookup()


    } //class WordLookupTestsTests


} //namespace WordLookupTestsTests


