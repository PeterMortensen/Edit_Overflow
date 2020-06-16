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



//What namespace to use?
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
        public void XXXXX()
        {
            {

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
		

        } //XXXXX



        
    } //class CodeFormattingCheckTests


} //namespace CodeFormattingCheckTests


