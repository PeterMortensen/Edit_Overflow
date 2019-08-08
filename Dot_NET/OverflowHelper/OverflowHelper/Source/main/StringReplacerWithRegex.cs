/****************************************************************************
 * Copyright (C) 2011 Peter Mortensen                                       *
 * This file is part of Edit Overflow.                                      *
 *                                                                          *
 *                                                                          *
 * Purpose: Hides details of using regular expressions, especially          *
 *          when applying a series of them on the same string. It           * 
 *          makes the client side much simpler by hiding those details.     *
 *                                                                          *
 *          And the client does not have to keep track of variables         *
 *          for this - only an instance of this class is necessary          *
 *                                                                          *
 ****************************************************************************/

//Future:
//
//  1. Perhaps relieve clients of having to escape the special 
//     characters for regular expressions, e.g. "{"
//
//     That is, provide a simplified interface for those cases where 
//     we use 
//     
//     Perhaps also for tab and return characters, "\t",?
//
//  2. Idea: Different interface - first define the transforms, then 
//           transform an input string to an output string
//
//  3. 



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using System.Text.RegularExpressions; //For Regex, etc.


/****************************************************************************
*    <placeholder for header>                                              *
****************************************************************************/
namespace OverflowHelper.core
{

    
    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class StringReplacerWithRegex
    {

        private string mCurrentString;


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public StringReplacerWithRegex(string anInputString)
        {
            mCurrentString = anInputString;
        } //Constructor.


        /****************************************************************************
         *                                                                          *
         *   We can read out the current string at any time, including the          *
         *   initial one.                                                           *
         *                                                                          *
         ****************************************************************************/
        public string currentString()
        {
            return mCurrentString;
        }


        /****************************************************************************
         *                                                                          *
         *   Replace some part of the string, using regular expression              * 
         *   notation (.NET style)                                                  *
         *                                                                          *
         ****************************************************************************/
        public string transform(string aMatchString, string aReplaceSpecification)
        {
            string result = Regex.Replace(mCurrentString,
                                          aMatchString,
                                          aReplaceSpecification,
                                          RegexOptions.Multiline);

            mCurrentString = result;
            return result;
        } 


    } //class StringReplacerWithRegex


}


