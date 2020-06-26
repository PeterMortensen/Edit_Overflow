/****************************************************************************
 * Copyright (C) 2020 Peter Mortensen                                       *
 *                                                                          *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: Unit testing for class CodeFormattingCheck                      *
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
namespace CodeFormattingCheckTests
{

    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    [TestFixture]
    public class CodeFormattingCheckTests
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        [Test]
        public void combinedAllOfRegularExpressions()
        {

            // Trivial test (as it is unlikely to change),
            // but it is a way to get started...
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(@"\S\{", cfCheck.missingSpaceBeforeOpeningBracketRegex(), "");

            }

        } //combinedAllOfRegularExpressions()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        [Test]
        public void RegExExecutor_basics()
        {
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Bad code should be detected
                string badCode = "auto p=new Son();";
                Assert.IsTrue(
                    RegExExecutor.match(badCode, cfCheck.missingSpaceAroundEqualSign()));

                // Also using the full regular expression
                Assert.IsTrue(
                    RegExExecutor.match(badCode, cfCheck.combinedAllOfRegularExpressions()));

                // Corresponding fixed code
                Assert.IsFalse(
                    RegExExecutor.match("auto p = new Son();",
                                        cfCheck.missingSpaceAroundEqualSign()));
            }

            {
                //LookUpString tt2 = new LookUpString("   r ");
                //string cs = tt2.getCoreString();
                //Assert.AreEqual("r", cs , "");
                //
                //string leading = tt2.getLeading();
                //Assert.AreEqual("   ", leading, "");
                //
                //string trailing = tt2.getTrailing();
                //Assert.AreEqual(" ", trailing, "");
            }

            {
                //LookUpString tt2 = new LookUpString("stackoverflow, ");
                //string cs = tt2.getCoreString();
                //Assert.AreEqual("stackoverflow", cs, "");
                //
                //string leading = tt2.getLeading();
                //Assert.AreEqual("", leading, "");
                //
                //string trailing = tt2.getTrailing();
                //Assert.AreEqual(", ", trailing, "");
            }


        } //RegExExecutor_basics()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        [Test]
        public void tightOperators()
        {
            CodeFormattingCheck cfCheck = new CodeFormattingCheck();

            // Shortcuts / aliases
            string regex     = cfCheck.missingSpaceAroundOperators();
            string regex_All = cfCheck.combinedAllOfRegularExpressions();


            // Write it out early as any failed test will result in
            // no output...
            //
            TestContext.WriteLine("Output from the unit test!!!!!!!!!!!!!!!!!!!!!");

            // For debugging, etc.
            TestContext.WriteLine("Regular expression for tight operators: " + regex);
            TestContext.WriteLine("All regular expressions: " + regex_All);


            {
                // Bad code should be detected
                string badCode = "if($fn&&$em)";
                Assert.IsTrue(
                    RegExExecutor.match(badCode, regex));

                // Variation of input
                Assert.IsTrue(
                    RegExExecutor.match("if($fn &&$em)", regex));
                Assert.IsTrue(
                    RegExExecutor.match("if($fn&& $em)", regex));

                // Also using the full regular expression
                Assert.IsTrue(
                   RegExExecutor.match(badCode, regex_All));

                // Corresponding fixed code, with the full regular
                // expression to catch false positives
                Assert.IsFalse(
                    RegExExecutor.match("if($fn && $em)", regex_All));
            }

            {
                // Bad code should be detected
                string badCode = "$ARGV[0].\" \".$ARGV[2];";
                Assert.IsTrue(
                    RegExExecutor.match(badCode, regex));

                // Variation of input - space to either side
                Assert.IsTrue(
                    RegExExecutor.match("$ARGV[0] .\"", regex));
                Assert.IsTrue(
                    RegExExecutor.match("$ARGV[0]. \"", regex));

                // Variation of input - single quotes
                Assert.IsTrue(
                    RegExExecutor.match("$ARGV[0] .'", regex));
                Assert.IsTrue(
                    RegExExecutor.match("$ARGV[0]. '", regex));




                // Test for false positives (for numbers)
                Assert.IsFalse(
                    RegExExecutor.match("1.56456", regex));
                Assert.IsFalse(
                    RegExExecutor.match(" = .56", regex));

                // Documenting a false positive
                //
                // For the concatenation operator in some lamguages
                Assert.IsTrue(
                    RegExExecutor.match("$m.Groups[2].Value -as [int]", regex));


                // We have "." in many places where we don't want it to
                // match (false positive matches)
                Assert.IsFalse(
                    RegExExecutor.match("get started...", regex));
                Assert.IsFalse(
                    RegExExecutor.match("using System.Text;", regex));


                // Also using the full regular expression
                Assert.IsTrue(
                   RegExExecutor.match(badCode, regex_All));

                // Corresponding fixed code, with the full regular
                // expression to catch false positives. It also important
                // to catch if they match too much (even matching
                // everything!)
                //
                //     $ARGV[0] . " " . $ARGV[2];
                //
                Assert.IsFalse(
                    RegExExecutor.match("$ARGV[0] . \" \" . $ARGV[2];",
                                        regex_All));

                //Assert.IsFalse(true); // TestContext.WriteLine() only
                                        // works if it fails.

            }


        } //tightOperators()


    } //class CodeFormattingCheckTests


} //namespace CodeFormattingCheckTests


