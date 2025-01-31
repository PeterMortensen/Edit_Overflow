/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: unit testing for class LookUpString.                            *
 *                                                                          *
 ****************************************************************************/


using NUnit.Framework; //For all versions of NUnit,
                       //file "nunit.framework.dll"

//using NUnit.Engine. No!

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using OverflowHelper.core;



//What namespace should we use?
/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace LookUpStringTests
{

    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    [TestFixture]
    public class LookUpStringTests
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        [Test]
        public void noWhiteSpace()
        {
            {
                LookUpString tt = new LookUpString("Peter");
                string cs = tt.getCoreString();
                Assert.AreEqual("Peter", cs, "");
            }

            //Further tests:
            //  *1. White space only
            //  2. Empty strings
            //  3. With tabs

        } //noWhiteSpace


        /****************************************************************************
         *                                                                          *
         *  Test of our automatic filtering out of leading and                      *
         *  trailing space, punctuation, Markdown formatting,                       *
         *  etc.                                                                    *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void leadingAndTrailingWhiteSpace()
        {
            {
                LookUpString tt2 = new LookUpString("   Peter ");
                string cs = tt2.getCoreString();
                Assert.AreEqual("Peter", cs , "");

                string leading = tt2.getLeading();
                Assert.AreEqual("   ", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual(" ", trailing, "");
            }

            {
                LookUpString tt2 = new LookUpString("   r ");
                string cs = tt2.getCoreString();
                Assert.AreEqual("r", cs , "");

                string leading = tt2.getLeading();
                Assert.AreEqual("   ", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual(" ", trailing, "");
            }

            {
                LookUpString tt2 = new LookUpString(" r ");
                string cs = tt2.getCoreString();
                Assert.AreEqual("r", cs , "");

                string leading = tt2.getLeading();
                Assert.AreEqual(" ", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual(" ", trailing, "");
            }

            {
                LookUpString tt2 = new LookUpString("r ");
                string cs = tt2.getCoreString();
                Assert.AreEqual("r", cs , "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual(" ", trailing, "");
            }

            {
                LookUpString tt2 = new LookUpString("r  ");
                string cs = tt2.getCoreString();
                Assert.AreEqual("r", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("  ", trailing, "");
            }

            {
                LookUpString tt2 = new LookUpString("stackoverflow, ");
                string cs = tt2.getCoreString();
                Assert.AreEqual("stackoverflow", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual(", ", trailing, "");
            }

            // Some short words should ***not*** be treated as punctuation
            {
                LookUpString tt2 = new LookUpString("\"");

                string cs = tt2.getCoreString();
                Assert.AreEqual("\"", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("", trailing, "");
            }

            // Some short words should ***not*** be treated as punctuation
            {
                LookUpString tt2 = new LookUpString("*");

                string cs = tt2.getCoreString();
                Assert.AreEqual("*", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("", trailing, "");
            }

            // Leading double quote. From a user perspective,
            // it shouldn't be filtered out (as it is a valid
            // word to look up), but that is the current
            // behaviour. The workaround is to turn off
            // the automatic filtering.
            {
                // A valid word to look up (including the quotes):
                //
                //   "Timeless" Homerow
                //
                LookUpString tt2 =
                    new LookUpString("\"Timeless\" Homerow");
                string cs = tt2.getCoreString();
                Assert.AreEqual("Timeless\" Homerow", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("\"", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("", trailing, "");
            }

            // Test of the option to turn automatic
            // filtering (mostly) off: Space
            {
                LookUpString tt2 = new LookUpString(" OS ", false);
                string cs = tt2.getCoreString();
                Assert.AreEqual(" OS ", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("", trailing, "");
            }

            // Test of the option to turn automatic
            // filtering (mostly) off: Leading double quote
            {
                LookUpString tt2 =
                    new LookUpString("\"Timeless\" Homerow", false);
                string cs = tt2.getCoreString();
                Assert.AreEqual("\"Timeless\" Homerow", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("", trailing, "");
            }

            // Test of the option to turn automatic
            // filtering (mostly) off: Trailing double quote
            {
                LookUpString tt2 =
                    new LookUpString("Debian 10 \"Buster\"", false);
                string cs = tt2.getCoreString();
                Assert.AreEqual("Debian 10 \"Buster\"", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("", trailing, "");
            }


        } //leadingAndTrailingWhiteSpace


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        [Test]
        public void whiteSpaceOnly()
        {
            {
                LookUpString tt2 = new LookUpString("      ");
                string cs = tt2.getCoreString();
                Assert.AreEqual("", cs, "");

                string leading = tt2.getLeading();
                Assert.AreEqual("      ", leading, "");

                string trailing = tt2.getTrailing();
                Assert.AreEqual("", trailing, "");
            }
        } //leadingAndTrailingWhiteSpace


    } //class LookUpStringTests

} //namespace namespaceForPILmassCalcTests


