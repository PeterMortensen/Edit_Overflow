/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: Test of assumptions about the environment (compiler versions,   *
 *          bitness, struct padding).                                       *
 *                                                                          *
 *          Some test / document our assumptions.                           *
 *                                                                          *
 *          It is sort of meta unit tests - not really about Edit Overflow  *
 *          itself, but the stuff surrounding it, including how it is       *
 *          developed.                                                      *
 *                                                                          *
 *          We also have a test to test that we have positively compiled    *
 *          the latest version of the unit tests.                           *
 *                                                                          *
 *                                                                          *
 ****************************************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


using NUnit.Framework; //For all versions of NUnit,
//file "nunit.framework.dll"

//using OverflowHelper.core;
//using OverflowHelper.platFormSpecific;


//What namespace to use?
/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.Tests
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    [TestFixture]
    public class EnvironmentTests
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        [Test]
        public void compiledNewest()
        {
            // Ensure we are actually running the expected version. But what
            // is actually our intent here? Why do we have this test?

            string presumedNewest = "1.1.51a698";

            {
                // For the main application
                string version_mainApplication =
                    EditorOverflowApplication.versionString();

                Assert.AreEqual(presumedNewest, version_mainApplication, "XYZ");
            }

        } //compiledNewest


    } //class LookUpStringTests

} //namespace OverflowHelper.Tests


