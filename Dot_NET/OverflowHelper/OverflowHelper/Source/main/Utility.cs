/***************************************************************************
* Copyright (C) 2010 Peter Mortensen                                       *
* This file is part of Edit Overflow.                                      *
*                                                                          *
*                                                                          *
* Purpose: Static utility function, various.                               *
*                                                                          *
****************************************************************************/


namespace OverflowHelper.core
{
    public sealed class ControlChars
    {
        public const char Back = '\b';
        public const char Cr = '\r';
        public const string CrLf = "\r\n";
        public const char FormFeed = '\f';
        public const char Lf = '\n';
        public const string NewLine = "\r\n";
        public const char NullChar = '\0';
        public const char Quote = '"';
        public const char Tab = '\t';
        public const char VerticalTab = '\v';
    }




    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    class Utility
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string ISO8601Time_timeOnly(System.DateTime aSomeDate)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(30);

            sb.Append(aSomeDate.Hour.ToString("00"));
            sb.Append(":");
            sb.Append(aSomeDate.Minute.ToString("00"));
            sb.Append(":");
            sb.Append(aSomeDate.Second.ToString("00"));
            sb.Append(".");
            sb.Append(aSomeDate.Millisecond.ToString("000"));

            return sb.ToString();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string ISO8601Time_DateOnly(System.DateTime aSomeDate)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(30);

            sb.Append(aSomeDate.Year);
            sb.Append("-");
            sb.Append(aSomeDate.Month.ToString("00"));
            sb.Append("-");
            sb.Append(aSomeDate.Day.ToString("00"));

            return sb.ToString();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string ISO8601FullTimeWithSubsecond()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(30);

            // long ticks = getHighResolutionTicks();

            System.DateTime now2 = System.DateTime.Now;

            sb.Append(ISO8601Time_DateOnly(now2));
            sb.Append("T");
            sb.Append(ISO8601Time_timeOnly(now2));

            //sb.Append(ControlChars.Tab);
            //sb.Append(ticks.ToString("000"));

            return sb.ToString();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static void DebugWriteLineWithTimeStamp(string aMessage)
        {
            string debugMsg =
                Utility.ISO8601FullTimeWithSubsecond() +
                ControlChars.Tab + aMessage;

            // using System.Diagnostics

            System.Diagnostics.Debug.WriteLine(debugMsg);
        }


    }

}


