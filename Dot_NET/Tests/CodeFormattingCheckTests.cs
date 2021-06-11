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
         *                                                                          *
         *   These may seem trivial (just checking what is essentially              *
         *   constants), but we have used them during refactoring. Thus             *
         *   mostly useful as regression tests.                                     *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void constants()
        {
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\S\{",
                  //cfCheck.missingSpaceBeforeOpeningBracketRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceBeforeOpeningBracket),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @":\S",
                  //cfCheck.missingSpaceAfterColonRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAfterColon),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @",\S",
                  //cfCheck.missingSpaceAfterCommaRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAfterComma),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\S\=|\=\S",
                  //cfCheck.missingSpaceAroundEqualSignRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundEqualSign),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\S\+|\+\S",
                  //cfCheck.missingSpaceAroundStringConcatenationRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundStringConcatenation),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s,",
                  //cfCheck.spaceBeforeCommaRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeComma),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s:",
                  //cfCheck.spaceBeforeColonRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeColon),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s\)",
                  //cfCheck.spaceBeforeParenthesisRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeParenthesis),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s;",
                  //cfCheck.spaceBeforeSemicommaRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeSemicomma),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\(\s",
                  //cfCheck.spaceAfterLeftParenthesisRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceAfterLeftParenthesis),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Note: Double quote (") is escaped as "" (two double quotes).
                //       Backslash is NOT escaped (using "@")
                Assert.AreEqual(
                  @"\S&&|&&\S|('|\""|(\$\w+\[.+\]))\.|\.['\""\]]",
                  //cfCheck.missingSpaceAroundOperatorsRegex(),
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundOperators),
                  "");
            }


            // A combined / all one
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Note: Double quote (") is escaped as "" (two double quotes).
                //       Backslash is NOT escaped (using "@")
                Assert.AreEqual(
                  @"(\S\{|:\S|,\S|\S\=|\=\S|\S\+|\+\S|\s,|\s:|\s\)|\s;|\(\s|\S&&|&&\S|('|\""|(\$\w+\[.+\]))\.|\.['\""\]])",
                  cfCheck.combinedAllOfRegularExpressions(),
                  "");
            }

            // A combined / all one
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Note: Double quote (") is escaped as "" (two double quotes).
                Assert.AreEqual(
                  @"(""missing space before {"", ""missing space after colon"", ""missing space after comma"", ""missing space around equal sign"", ""missing space around string concatenation (by ""+"")"", ""space before comma"", ""space before colon"", ""space before right parenthesis"", ""space before semicolon"", ""space after left parenthesis"", and ""missing space around some operators"")",
                  // @"XYZ",
                  cfCheck.combinedAllOfExplanations(),
                  "");
            }

        } //constants()


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
                    RegExExecutor.match(
                      badCode, 
                      cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                // Also using the full regular expression
                Assert.IsTrue(
                    RegExExecutor.match(badCode, cfCheck.combinedAllOfRegularExpressions()));

                // Corresponding fixed code
                Assert.IsFalse(
                    RegExExecutor.match("auto p = new Son();",
                                        cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));
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
            string regex     = cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundOperators);
            string regex_All = cfCheck.combinedAllOfRegularExpressions();


            // Write it out early as any failed test will result in
            // no output...
            //
            TestContext.WriteLine("Output from the unit test!!!!!!!!!!!!!!!!!!!!!");

            // For debugging, etc.
            TestContext.WriteLine("\nRegular expression for tight operators: " + regex);
            TestContext.WriteLine("\nAll regular expressions: " + regex_All);


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

                // No longer a false positive!
                //
                // For the concatenation operator in some languages
                Assert.IsFalse(
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


