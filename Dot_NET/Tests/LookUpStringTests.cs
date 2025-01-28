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
         *    <placeholder for header>                                              *
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


