﻿/****************************************************************************
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
         *   conversely, if that is not generated properly).                        *
         *                                                                          *
         *   And we get to have a combined regular expression that can be           *
         *   used elsewhere (e.g., in file FixedStrings.php - though                *
         *   double quotes must be escaped as "&quot;").                            *
         *                                                                          *
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
                  @"\S\=|\=\S|\s{2,}\=|\=\s{2,}",
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

            // Single-line if statements
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                Assert.AreEqual(
                  @"\sif.*\).{3,}",
                  cfCheck.getRegularExpression(
                    codeFormattingsRegexEnum.single_lineIfStatements),
                  "");
            }


            // =========================================================

            // A combined / all one
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Note: Double quote (") is escaped as "" (two
                //       double quotes).
                //
                //       Backslash is NOT escaped (using "@")
                Assert.AreEqual(
                  @"(\S\{|:\S|,\S|\S\=|\=\S|\s{2,}\=|\=\s{2,}|\S\+|\+\S|\s,|\s:|\s\)|\s;|\(\s|\S&&|&&\S|('|\""|\)|(\$\w+\[.+\]))\.|\.['\""\]]|(\/\/|\/\*|\#|<!--)\s*\p{Ll}|(\/\/|\/\*|\#|<!--)\S|\S(\/\/|\/\*|\#|<!--)|\sif.*\).{3,})",
                  cfCheck.combinedAllOfRegularExpressions(),
                  "");
            }

            // A combined / all one
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Notes:
                //
                //   1. Double quote (") is escaped as "" (two double quotes).
                //
                //   2. The first letter in each sub string must be
                //      in lowercase (in contrast to the primary
                //      source of it (used elsewhere as menu
                //      items in a GUI))
                //
                Assert.AreEqual(
                  @"(""missing space before {"", ""missing space after colon"", ""missing space after comma"", ""missing or too much space around equal sign"", ""missing space around string concatenation (by ""+"")"", ""space before comma"", ""space before colon"", ""space before right parenthesis"", ""space before semicolon"", ""space after left parenthesis"", ""missing space around some operators"", ""missing capitalisation in comment (Jon Skeet decree)"", ""missing space in comment (Jon Skeet decree)"", and ""single-line 'if' statements"")",
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
            }

            // For missing or too much space close to equal sign.
            {
                CodeFormattingCheck cfCheck = new CodeFormattingCheck();

                // Too much space
                {
                    // Bad code should be detected
                    //
                    
                    string badCode1 = "rows =  driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode1,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode1,
                            cfCheck.combinedAllOfRegularExpressions()));
                }

                // Too much space. But also covered by another match.
                {
                    // Bad code should be detected
                    //
                    
                    string badCode3 = "rows=  driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode3,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode3,
                            cfCheck.combinedAllOfRegularExpressions()));
                }

                // Too much space, even more (one more than the 
                // supposed lower in the regular expression.
                {
                    // Bad code should be detected
                    //
                    
                    string badCode4 = "rows=   driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode4,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode4,
                            cfCheck.combinedAllOfRegularExpressions()));
                }

                // Too much space. But also covered by another match.
                {
                    // Bad code should be detected
                    //
                    
                    string badCode5 = "rows =  driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode5,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode5,
                            cfCheck.combinedAllOfRegularExpressions()));
                }

                // Too much space, even more (one more than the 
                // supposed lower in the regular expression.
                {
                    // Bad code should be detected
                    //
                    
                    string badCode6 = "rows =   driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode6,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode6,
                            cfCheck.combinedAllOfRegularExpressions()));
                }

                // Too much space to the left
                {
                    // Bad code should be detected
                    //                    
                    string badCode7 = "rows  = driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode7,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode7,
                            cfCheck.combinedAllOfRegularExpressions()));
                }

                // Too much space to both sides
                {
                    // Bad code should be detected
                    //                    
                    string badCode8 = "rows  =  driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode8,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode8,
                            cfCheck.combinedAllOfRegularExpressions()));
                }


                // Too little space. Probably covered by the 
                // very first test in this function.
                {
                    // Bad code should be detected
                    string badCode2 = "rows =driver.findElements";
                    Assert.IsTrue(
                        RegExExecutor.match(
                          badCode2,
                          cfCheck.getRegularExpression(
                            codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));

                    // Also using the full (combined) regular expression
                    Assert.IsTrue(
                        RegExExecutor.match(
                            badCode2,
                            cfCheck.combinedAllOfRegularExpressions()));
                }


                // The corresponding fixed code should not be
                // detected (neither real or false positives).
                Assert.IsFalse(
                    RegExExecutor.match(
                      "rows = driver.findElements",
                      cfCheck.getRegularExpression(
                        codeFormattingsRegexEnum.missingSpaceAroundEqualSign)));
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

            // For debugging, etc.
            //TestContext.WriteLine("\nRegular expression for tight operators: " + regex);
            //TestContext.WriteLine("\nAll regular expressions: " + regex_All);

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


            // Some test input

            string badCap1 = "// respect the Jon Skeet decree!";
            string badSpace1 = "//Respect the Jon Skeet decree!";
            string badSpace2 = ";// Respect the Jon Skeet decree!";

            string badHTML1 = "<!--Respect the Jon Skeet decree!-->";

            // All 3 problems at the same time
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


        /****************************************************************************
         *                                                                          *
         *    Single-line if statements                                             *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void single_lineIfStatements()
        {
            CodeFormattingCheck cfCheck = new CodeFormattingCheck();

            // Shortcut / alias
            string regex = cfCheck.getRegularExpression(
              codeFormattingsRegexEnum.single_lineIfStatements);

            string regex_All = cfCheck.combinedAllOfRegularExpressions();


            // Some test input. Note the leading space!

            string badIf1 = "    if (transferstatus == ES_DONE) result = SFTPrename(rfn, (char *)tempfilename.c_str());";

            string falsePositiveIf1 = "    if (sftp_get_error(sftp) != SSH_FX_FILE_ALREADY_EXISTS)";


            // Base test
            {
                TestContext.WriteLine("\nbadIf1: " + badIf1);
                TestContext.WriteLine("\nregex: " + regex);

                // Bad code should be detected
                Assert.IsTrue(RegExExecutor.match(badIf1, regex));
                //Assert.IsTrue(RegExExecutor.match(badIf1, regex_All));
            }

            // False positives (with the current regular expression)
            {
                // Documenting a false postive
                Assert.IsTrue(RegExExecutor.match(falsePositiveIf1, regex));
                Assert.IsTrue(RegExExecutor.match(falsePositiveIf1, regex_All));
            }

        } //single_lineIfStatements()


    } //class CodeFormattingCheckTests


} //namespace CodeFormattingCheckTests


