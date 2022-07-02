/***************************************************************************
*                                                                          *
* Copyright (C) 2020 Peter Mortensen                                       *
*                                                                          *
* This file is part of Overflow Helper.                                    *
*                                                                          *
*                                                                          *
* Purpose: In the first instance, a helper class for testing (e.g.         *
*          used in '\Dot_NET\Tests\CodeFormattingCheckTests.cs'),          *
*          but it may later be used on user text (thus its                 *
*          position in the folder structure). We could imagine             *
*          the same search and accept/reject process as in a               *
*          text editor (perhaps even with some "learning" -                *
*          adaptive/dynamic)                                               *
*                                                                          *
*          It hides how to match some text with a regular expression.      *
*                                                                          *
* Reference: For example, used in                                          *
*                                                                          *
****************************************************************************/


//using System.Text; //For StringBuilder.
//using System.Diagnostics; //For Trace. And its Assert.

using System.Text.RegularExpressions; // For MatchCollection, etc.


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class RegExExecutor
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public RegExExecutor()
        {

        }


        /****************************************************************************
         *                                                                          *
         *    Only indicates if there is a match or not. Not how many and where.    *
         *                                                                          *
         *    Stateless version                                                     *
         *                                                                          *
         ****************************************************************************/
        public static bool match(string aSomeText, string aRegularExpression)
        {
            MatchCollection mc = Regex.Matches(aSomeText, aRegularExpression);

            bool toReturn = false;

            if (mc.Count > 0)
            {
                toReturn = true;
            }

            return toReturn;
        } //match()


    } //class RegExExecutor


} //namespace OverflowHelper.core



