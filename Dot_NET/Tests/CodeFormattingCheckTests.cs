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
         *   These may seem trivial (just checking what are essentially             *
         *   constants), but we have used them during refactoring. Thus             *
         *   they are mostly useful as regression tests.                            *
         *                                                                          *
         *   It is also useful to detect if we forget to update the                 *
         *   combined regular expression when we add new checks (or                 *
         *   conversely if that is not generated properly).                         *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void constants()
        {
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\S\{",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceBeforeOpeningBracket),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @":\S",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAfterColon),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @",\S",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAfterComma),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\S\=|\=\S",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundEqualSign),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\S\+|\+\S",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundStringConcatenation),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s,",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeComma),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s:",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeColon),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s\)",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeParenthesis),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\s;",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceBeforeSemicomma),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\(\s",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.spaceAfterLeftParenthesis),
                  "");
            }

            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Note: Double quote (") is escaped as "" (two double quotes).
                //       Backslash is NOT escaped (using "@")
                Assert.AreEqual(
                  //@"\S&&|&&\S|('|\""|(\$\w+\[.+\]))\.|\.['\""\]]",
                  @"\S&&|&&\S|('|\""|\)|(\$\w+\[.+\]))\.|\.['\""\]]",
                  cfCheck.getRegularExpression(codeFormattingsRegexEnum.missingSpaceAroundOperators),
                  "");
            }


            // Code comments. Sentence casing
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"(\/\/|\/\*|\#|<!--)\s*\p{Ll}",
                  cfCheck.getRegularExpression(
                    codeFormattingsRegexEnum.missingCapitalisationInComment_Jon_Skeet_decree),
                  "");
            }

            // Code comments. Space near.
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"(\/\/|\/\*|\#|<!--)\S|\S(\/\/|\/\*|\#|<!--)",
                  //@"(\/\/|\/\*|\#)\S|\S(\/\/|\/\*|\#|<!--)",
                  cfCheck.getRegularExpression(
                    codeFormattingsRegexEnum.missingSpaceInComment_Jon_Skeet_decree),
                  "");
            }



            // A combined / all one
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Note: Double quote (") is escaped as "" (two
                //       double quotes).
                //
                //       Backslash is NOT escaped (using "@")
                Assert.AreEqual(
                  @"(\S\{|:\S|,\S|\S\=|\=\S|\S\+|\+\S|\s,|\s:|\s\)|\s;|\(\s|\S&&|&&\S|('|\""|\)|(\$\w+\[.+\]))\.|\.['\""\]]|(\/\/|\/\*|\#|<!--)\s*\p{Ll}|(\/\/|\/\*|\#|<!--)\S|\S(\/\/|\/\*|\#|<!--))",
                  cfCheck.combinedAllOfRegularExpressions(),
                  "");
            }

            // A combined / all one
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Note: Double quote (") is escaped as "" (two double quotes).
                Assert.AreEqual(
                  @"(""missing space before {"", ""missing space after colon"", ""missing space after comma"", ""missing space around equal sign"", ""missing space around string concatenation (by ""+"")"", ""space before comma"", ""space before colon"", ""space before right parenthesis"", ""space before semicolon"", ""space after left parenthesis"", ""missing space around some operators"", ""missing capitalisation in comment (Jon Skeet decree)"", and ""missing space in comment (Jon Skeet decree)"")",
                  // @"XYZ",
                  cfCheck.combinedAllOfExplanations(),
                  "");
            }

        } //constants()


        /****************************************************************************
         *                                                                          *
         *    That is, a kind of smoke test - broad relatively unspecific tests.    *
         *                                                                          *
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
                      cfCheck.getRegularExpression(
                        codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                // Also using the full (combined) regular expression
                Assert.IsTrue(
                    RegExExecutor.match(badCode, cfCheck.combinedAllOfRegularExpressions()));

                // The corresponding fixed code should not be
                // detected (neither real or false positives).
                Assert.IsFalse(
                    RegExExecutor.match(
                      "auto p = new Son();",
                      cfCheck.getRegularExpression(
                        codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                // The full (combined) regular expression should not give
                // a false positive, etc.
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXxxxx

            }

            {
                //What is this for???
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
                //What is this for???
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
         *                                                                          *
         *    That is, too little space (as in none) around some operators          *
         *                                                                          *
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


            {
                // No longer false negatives!
                //
                //
                Assert.IsTrue(
                    RegExExecutor.match(
                        "kzXnxbnSXbcv   ' . chr(160).chr(127).chr(126);", 
                        regex));
                Assert.IsTrue(
                    RegExExecutor.match(
                        "ord($str[$i]). ')';", 
                        regex));
            }


        } //tightOperators()


        /****************************************************************************
         *                                                                          *
         *    That is, space and capitalisation requirement for code comments       *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void Jon_Skeet_decree()
        {
            CodeFormattingCheck cfCheck = new CodeFormattingCheck();

            // Shortcuts / aliases
            string regexCap   = cfCheck.getRegularExpression(
              codeFormattingsRegexEnum.missingCapitalisationInComment_Jon_Skeet_decree);

            string regexSpace = cfCheck.getRegularExpression(
              codeFormattingsRegexEnum.missingSpaceInComment_Jon_Skeet_decree);


            string regex_All = cfCheck.combinedAllOfRegularExpressions();

            string badCap1 = "// respect the Jon Skeet decree!";
            string badSpace1 = "//Respect the Jon Skeet decree!";
            string badSpace2 = ";// Respect the Jon Skeet decree!";

            string badHTML1 = "<!--Respect the Jon Skeet decree!-->";

            // All 3 problems at the same times
            string allBad1 = ";//respect the Jon Skeet decree!";

            string allGood1 = "// Respect the Jon Skeet decree!";
            string allGood2 = "XXXXX  // Respect the Jon Skeet decree!";
            string allGood3 = "/* Respect the Jon Skeet decree!";
            string allGood4 = "# Respect the Jon Skeet decree!";

            // Comments, space
            {
                // Bad code should be detected
                Assert.IsTrue(RegExExecutor.match(badSpace1, regexSpace));
                Assert.IsTrue(RegExExecutor.match(badSpace1, regex_All));
                Assert.IsTrue(RegExExecutor.match(badSpace2, regexSpace));
                Assert.IsTrue(RegExExecutor.match(badSpace2, regex_All));

                // One particular kind of comment test should not match
                // content with some of the other problems.
                //
                // Only the capitalisation problem for this one
                Assert.IsFalse(RegExExecutor.match(badCap1, regexSpace));

                // 2021-11-09.
                Assert.IsTrue(RegExExecutor.match(badHTML1, regexSpace));
            }


            // Comments, sentence capitalisation
            {
                // Bad code should be detected
                Assert.IsTrue(RegExExecutor.match(badCap1, regexCap));
                Assert.IsTrue(RegExExecutor.match(badCap1, regex_All));

                // Good code should not result in false postives
                Assert.IsFalse(RegExExecutor.match(allGood1, regexCap));
                Assert.IsFalse(RegExExecutor.match(allGood2, regexCap));
                Assert.IsFalse(RegExExecutor.match(allGood3, regexCap));
                Assert.IsFalse(RegExExecutor.match(allGood4, regexCap));

                // Good code should not result in false postives, not even
                // with the full regular expression
                Assert.IsFalse(RegExExecutor.match(allGood1, regex_All));
                Assert.IsFalse(RegExExecutor.match(allGood2, regex_All));
                Assert.IsFalse(RegExExecutor.match(allGood3, regex_All));
                Assert.IsFalse(RegExExecutor.match(allGood4, regex_All));

                // 2021-11-09.
                // This one only has the space problem, thus should not match.
                Assert.IsFalse(RegExExecutor.match(badHTML1, regexCap));
            }

            // Comments, all three problems at once
            {
                Assert.IsTrue(RegExExecutor.match(allBad1, regexCap));
                Assert.IsTrue(RegExExecutor.match(allBad1, regex_All));
            }


        } //Jon_Skeet_decree()


    } //class CodeFormattingCheckTests


} //namespace CodeFormattingCheckTests


