/***************************************************************************
* Copyright (C) 2010 Peter Mortensen                                       *
* This file is part of Overflow Helper.                                    *
*                                                                          *
*                                                                          *
* Purpose: cached lookup in Wikipedia. E.g. "JavaScript" will return       *
*          the URL <http://en.wikipedia.org/wiki/JavaScript>. The          *
*          lookups are cached so it is much faster than going              *
*          through Google and Wikipedia every time. This typically         *
*          takes 7 seconds (at least on a 3G Internet connection).         *
*                                                                          *
*                                                                          *
* Yes, most of the content in this file ought to be in a data file.        *
*                                                                          *
*                                                                          *
****************************************************************************/

// Some bugs:
//
//   1. If a line ends in four spaces and a degree sign, all of
//      it is removed!
//
//   2.

//using System;
//using System.Linq;

using System.Collections.Generic; // For Dictionary and List
using System.Text; //For StringBuilder.
using System.Diagnostics; //For Trace. And its Assert.


//Markers
//AAA   Spell correction add point.
//BBB   Link add point.


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class WikipediaLookup
    {
        public const string kCodeQuoteStr = "`";

        private Dictionary<string, string> mCaseCorrection;
        private Dictionary<string, string> mWord2URL;

        private Dictionary<string, string> mCaseCorrection_Reverse; //Reverse
        //  mapping, for checking purposes only (at the expense of using more
        //  memory).
        //
        //  Later: perhaps get rid of it after use, to save memory.


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public WikipediaLookup()
        {
            // Note: top folder, not the one we are going to store in.
            string localAppDataFolder =
              System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.LocalApplicationData);

            // Note: actual folder. For user "Administrator":
            //   C:\Documents and Settings\Administrator\Local Settings\Application Data\EMBO\OverflowHelper\1.0.0.0
            //
            string localAppDataFolder2 =
              System.Windows.Forms.Application.LocalUserAppDataPath;

            string appDataFolder =
              System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.ApplicationData);


            //This is dependent on "x86", "x64" and "Any CPU":
            //
            //  On 64-bit Windows:
            //    "x86"    :  "C:\\Program Files (x86)"
            //    "x64"    :  "C:\\Program Files"
            //    "Any CPU":  "C:\\Program Files"
            //
            //
            string folderStr =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);

            if (true)
            {
                //Testing Stack Overflow question, "Refer to 'Program Files' on a 64-bit machine"
                //  http://stackoverflow.com/questions/1454094/refer-program-files-in-a-64-bit-machine

                string f32 = System.Environment.GetEnvironmentVariable("ProgramFiles");
                string f64 = System.Environment.GetEnvironmentVariable("ProgramW6432");

                string processorArchitecture =
                    System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

                //Result on 2013-03-21.
                //  f32                    "C:\\Program Files"
                //  f64                    null
                //  processorArchitecture  "AMD64"
            }

            mCaseCorrection = new Dictionary<string, string>();
            mCaseCorrection_Reverse = new Dictionary<string, string>();

            mWord2URL = new Dictionary<string, string>();

            addLookupData();
        } //Constructor


        // *************************************************************************
        // *  CLASS NAME:   SortBySequenceThenScore_usingIndex                     *
        // *                                                                       *
        // *    Helper class for sorting the correction list.                      *
        // *                                                                       *
        // *    We sort a list of keys (incorrect terms - as they are              *
        // *    unique) - we couldn't sort the original values anyway              *
        // *    as they are hashes                                                 *
        // *                                                                       *
        // *                                                                       *
        // d$ <summary> For sorting...                                             *
        // d$      Note: xyz                    </summary>                         *
        // *                                                                       *
        // *************************************************************************
        private class SortByCorrectThenIncorrect_usingIndex :
          System.Collections.Generic.IComparer<int>
        {

            private Dictionary<string, string> mCaseCorrection;

            private List<string> mKeys; // Of incorrect terms.

            private List<int> mIndexes; // Into mKeys


            //****************************************************************************
            //*    <placeholder for header>                                              *
            //****************************************************************************
            public SortByCorrectThenIncorrect_usingIndex(
                Dictionary<string, string> aCaseCorrection)
            {
                mCaseCorrection = aCaseCorrection; // We need to remember it for when
                // the compare function is called...

                int len = aCaseCorrection.Count;

                // 2016-01-31. XXXXXXXX

                //Prepare the list of keys
                mKeys = new List<string>(len);
                mIndexes = new List<int>(len);

                int currentIndex = 0;
                Dictionary<string, string>.Enumerator hashEnumerator2 =
                    aCaseCorrection.GetEnumerator();
                while (hashEnumerator2.MoveNext())
                {
                    string curKey = hashEnumerator2.Current.Key;
                    string curValue = hashEnumerator2.Current.Value;

                    mKeys.Add(curKey);
                    mIndexes.Add(currentIndex);

                    //string URL;
                    //if (!mWord2URL.TryGetValue(curValue, out URL))
                    //{
                    //    string msgStr = "No URL mapping for " + curValue;
                    //    System.Windows.Forms.MessageBox.Show(msgStr);
                    //}

                    currentIndex++;
                } //Hash iteration.

            } //Constructor.


            //****************************************************************************
            //*    Note: input is a key, not an actual value                             *
            //****************************************************************************
            public int Compare(int aItem1, int aItem2)
            {
                int toReturn = 0; //Default: equal.

                if (aItem1 != aItem2)
                {
                    // The unique key, the incorrect term, is the secondary key.
                    //
                    string secondary1 = mKeys[aItem1];
                    string secondary2 = mKeys[aItem2];

                    // The correct term is the primary key.
                    string primary1 = mCaseCorrection[secondary1];
                    string primary2 = mCaseCorrection[secondary2];

                    int compareResult_primary = primary1.CompareTo(primary2);

                    if (compareResult_primary != 0)
                    {
                        toReturn = compareResult_primary; //Ascending sort for primary key.
                    }
                    else
                    {
                        //Same primary key - use second key: incorrect term

                        int compareResult_secondary = secondary1.CompareTo(secondary2);

                        if (compareResult_secondary != 0)
                        {
                            toReturn = compareResult_secondary; //Ascending sort for secondary key.
                        }
                        else
                        {
                            // Both keys are equal...
                            //
                            // We should never be here as the secondary
                            // key is unique. ASSERT?
                            int peter9 = 9;

                        } //Same XYZ, use second key: ABC
                    } //JJJJJ compare
                }
                else
                {
                    int peter2 = 2; // Indexes equal. Does this ever
                    // happen??? Yes! - apparently the
                    // sorting algorithm does not check
                    // for equality before the call to
                    // this function...
                }
                return toReturn;
            } //Compare()


            //****************************************************************************
            //*                                                                          *
            //****************************************************************************
            public List<int> indexes()
            {
                return mIndexes;
            }


            //****************************************************************************
            //*                                                                          *
            //****************************************************************************
            public List<string> keys()
            {
                return mKeys;
            }


        } // private class SortByCorrectThenIncorrect_usingIndex


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        private void checkSpellingDataStructures()
        {
            // Detect if any spelling correction is missing a
            // corresponding URL (this will cause the lookup
            // to fail, even tough the correction is defined.
            // An example was "ILDASM").

            Dictionary<string, string>.Enumerator hashEnumerator2 =
                mCaseCorrection.GetEnumerator();
            while (hashEnumerator2.MoveNext())
            {
                string curKey = hashEnumerator2.Current.Key;
                string curValue = hashEnumerator2.Current.Value;


                string URL;
                if (!mWord2URL.TryGetValue(curValue, out URL))
                {
                    string msgStr = "No URL mapping for " + curValue;
                    System.Windows.Forms.MessageBox.Show(msgStr);
                }
            } //Hash iteration.
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string correctionInfoStr2(string aBadTerm, string aCorrectedTerm)
        {
            return "bad term '" +
                   aBadTerm +
                   "' (corrected term is '" + aCorrectedTerm + "').";
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string URLmappingStr2(string aCorrectedTerm, string aURL)
        {
            return "correct term '" +
                   aCorrectedTerm +
                   "' (URL is '" + aURL + "').";
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string keyValueInfoStr(string aKey, string aValue)
        {
            return "key '" +
                   aKey +
                   "' (value is '" + aValue + "').";
        }


        /****************************************************************************
         *                                                                          *
         *    Catch some common errors in the specification of corrections          *
         *                                                                          *
         ****************************************************************************/
        private void sanityCheck(string aSomeString, string aKey, string aValue)
        {
            Trace.Assert(
                aSomeString.Length > 0,
                "LANG ASSERT. aSomeString is empty in sanityCheck(). " +
                "(aKey is \"" + aKey + "\", aValue is \"" + aValue + "\"). ");

            if (aSomeString[0] == ' ')
            {
                string msg = "Leading space in '" +
                             aSomeString + "' - " +
                             keyValueInfoStr(aKey, aValue);
                System.Windows.Forms.MessageBox.Show(msg);
            }

            int lastIndex = aSomeString.Length - 1;
            if (aSomeString[lastIndex] == ' ')
            {
                string msg = "Trailing space in '" +
                             aSomeString + "' - " +
                             keyValueInfoStr(aKey, aValue);
                System.Windows.Forms.MessageBox.Show(msg);
            }
        }


        //PM_REFACTOR 2012-02-16
        /****************************************************************************
         *                                                                          *
         *    Add a correction to the word list. We also perform various            *
         *    integrity tests (e.g. double entries are not allowed)                 *
         *                                                                          *
         ****************************************************************************/
        private void correctionAdd(string aBadTerm, string aCorrectedTerm)
        {
            int badTermLength = aBadTerm.Length;

            Trace.Assert(
                 badTermLength > 0,
                "LANG ASSERT. aBadTerm is empty in correctionAdd(). " +
                "(aCorrectedTerm is \"" + aCorrectedTerm + "\"). ");

            Trace.Assert(
                aBadTerm[badTermLength - 1] != '.',
                "LANG ASSERT. aBadTerm ends with a full stop. " +
                "This is currently not allowed " +
                "(it will result in a false negative) " +
                "(aBadTerm is \"" + aBadTerm + "\"). ");

            Trace.Assert(
                aCorrectedTerm.Length > 0,
                "LANG ASSERT. aCorrectedTerm is empty in correctionAdd() " +
                "(aBadTerm is \"" + aBadTerm + "\"). ");

            if (aBadTerm == aCorrectedTerm)
            {
                string msg =
                  "Explicit identity mapping is not allowed (this is automatic). " +
                  "For \"" + aBadTerm + "\".";
                System.Windows.Forms.MessageBox.Show(msg);
            }

            string corrStr;
            if (mCaseCorrection.TryGetValue(aBadTerm, out corrStr))
            {
                //Double entry!
                string msg = "Double entry for bad term \"" +
                             keyValueInfoStr(aBadTerm, aCorrectedTerm) + "\"...";
                System.Windows.Forms.MessageBox.Show(msg);
            }
            else
            {
                // Warning only.
                sanityCheck(aBadTerm, aBadTerm, aCorrectedTerm);
                sanityCheck(aCorrectedTerm, aBadTerm, aCorrectedTerm);

                mCaseCorrection.Add(aBadTerm, aCorrectedTerm);

                string badTerm;
                if (mCaseCorrection_Reverse.TryGetValue(aCorrectedTerm, out badTerm))
                {
                    // If we are here then there is more than one corrected
                    // term. That is OK. That just means there is more than
                    // one misspelling!
                }
                else
                {
                    mCaseCorrection_Reverse.Add(aCorrectedTerm, aBadTerm);
                }
            }
        } //correctionAdd()


        //PM_REFACTOR 2012-02-16
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void URL_Add(string aCorrectedTerm, string aURL)
        {
            //Later: sanity checks, e.g. that the key already
            //       exists in mCaseCorrection.

            string URL;
            if (mWord2URL.TryGetValue(aCorrectedTerm, out URL))
            {
                //Later: factor out.

                //Double entry!
                string msg = "Double entry for the " +
                             keyValueInfoStr(aCorrectedTerm, aURL);
                System.Windows.Forms.MessageBox.Show(msg);
            }
            else
            {
                //Warning only.
                sanityCheck(aCorrectedTerm, aCorrectedTerm, aURL);
                sanityCheck(aURL, aCorrectedTerm, aURL);

                //Warning only.
                //We expect that the corresponding correction mapping has
                //already been added.
                string badTerm;
                if (mCaseCorrection_Reverse.TryGetValue(aCorrectedTerm, out badTerm))
                {

                }
                else
                {
                    //No correction mapping exist!
                    string msg = "Missing correction mapping for the corrected term '" +
                                 aCorrectedTerm +
                                 "'.";
                    System.Windows.Forms.MessageBox.Show(msg);
                }
                mWord2URL.Add(aCorrectedTerm, aURL);
            }
        }


        /****************************************************************************
         *    addLookupData                                                         *
         *                                                                          *
         *                                                                          *
         *  Yes, most of the content in this function ought to be in a data file.   *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        private void addLookupData()
        {
            // Note: We have support for converting our update files
            //       into the source code lines here by this Perl
            //       script:
            //
            //           ToToTryExtract.pl


            // Yes, most of the following content ought to be in a data file.

            correctionAdd("windows", "Windows");
            correctionAdd("WINDOWS", "Windows");
            correctionAdd("win", "Windows");
            correctionAdd("Win", "Windows");
            correctionAdd("windos", "Windows"); //Misspelling (typo?).
            correctionAdd("Windos", "Windows"); //Misspelling (typo?).
            correctionAdd("WIN", "Windows");
            correctionAdd("widnows", "Windows");
            correctionAdd("Windwos", "Windows");
            correctionAdd("Windonw", "Windows");
            correctionAdd("MS Windows", "Windows");
            correctionAdd("MSWindows", "Windows");
            correctionAdd("MS-Windows", "Windows");
            correctionAdd("Windwows", "Windows");
            correctionAdd("Widows", "Windows");
            correctionAdd("winsdows", "Windows");
            correctionAdd("winblows", "Windows");
            correctionAdd("Winblows", "Windows");
            correctionAdd("Windws", "Windows");
            correctionAdd("WIndows", "Windows");
            correctionAdd("windwos", "Windows");
            correctionAdd("Windoes", "Windows");
            correctionAdd("windoze", "Windows");
            correctionAdd("Windoze", "Windows");

            correctionAdd("java", "Java");
            correctionAdd("JAVA", "Java");
            correctionAdd("Jave", "Java");
            correctionAdd("jave", "Java");

            correctionAdd("javafx", "JavaFX"); //Not actually observed.
            correctionAdd("Java fx", "JavaFX");
            correctionAdd("java fx", "JavaFX");

            //ME = Micro Edition
            //Old name: "Java ME was formerly known as Java 2 Platform, Micro Edition (J2ME)".
            correctionAdd("j2me", "Java ME");
            correctionAdd("J2me", "Java ME");
            correctionAdd("J2ME", "Java ME");
            correctionAdd("JavaME", "Java ME");
            correctionAdd("java me", "Java ME");
            correctionAdd("j2ME", "Java ME");

            //SE = Standard Edition
            correctionAdd("JAVA SE", "Java SE");

            //EE = Enterprise Edition
            correctionAdd("java ee", "Java EE");
            correctionAdd("javaee", "Java EE");
            correctionAdd("java EE", "Java EE");
            correctionAdd("JEE", "Java EE");
            correctionAdd("J2EE", "Java EE"); //J2EE is the old product name, before 2006-05-11. See <http://en.wikipedia.org/wiki/Java_EE_version_history>.
            correctionAdd("j2ee", "Java EE");
            correctionAdd("J2ee", "Java EE");
            correctionAdd("JavaEE", "Java EE");
            correctionAdd("Jave EE", "Java EE");
            correctionAdd("javaEE", "Java EE");
            correctionAdd("j2EE", "Java EE");

            correctionAdd("jsf", "JSF"); //Not actually observed.

            correctionAdd("jsp", "JSP"); //Not actually observed.
            correctionAdd("Jsp", "JSP");

            correctionAdd("jdk", "JDK");

            correctionAdd("js", "JavaScript");
            correctionAdd("JS", "JavaScript");
            correctionAdd("Js", "JavaScript");
            correctionAdd("javascript", "JavaScript");
            correctionAdd("java script", "JavaScript");
            correctionAdd("Javascript", "JavaScript");
            correctionAdd("javscript", "JavaScript");
            correctionAdd("javasctipt", "JavaScript");
            correctionAdd("Javascipt", "JavaScript");
            correctionAdd("javaScript", "JavaScript");
            correctionAdd("javescript", "JavaScript");
            correctionAdd("Javasctipt", "JavaScript");
            correctionAdd("JAVASCRIPT", "JavaScript");
            correctionAdd("java scripts", "JavaScript");
            correctionAdd("javaccript", "JavaScript"); //Misspelling.
            correctionAdd("JavaSript", "JavaScript"); //Misspelling.
            correctionAdd("Javscript", "JavaScript"); //Misspelling.
            correctionAdd("Java-script", "JavaScript");
            correctionAdd("java-script", "JavaScript");
            correctionAdd("Javascsript", "JavaScript"); //Misspelling.
            correctionAdd("Javascaript", "JavaScript"); //Misspelling.
            correctionAdd("JavaSctript", "JavaScript"); //Misspelling.
            correctionAdd("javacsript", "JavaScript"); //Misspelling.
            correctionAdd("Javacript", "JavaScript"); //Misspelling.
            correctionAdd("Java Script", "JavaScript");
            correctionAdd("JAVA SCRIPT", "JavaScript");
            correctionAdd("Javasciript", "JavaScript"); //Misspelling.
            correctionAdd("javacript", "JavaScript"); //Misspelling.
            correctionAdd("javascrpt", "JavaScript"); //Misspelling.
            correctionAdd("javasctript", "JavaScript"); //Misspelling.
            correctionAdd("javasript", "JavaScript"); //Misspelling.
            correctionAdd("Java script", "JavaScript");
            correctionAdd("jS", "JavaScript");
            correctionAdd("javascipt", "JavaScript");
            correctionAdd("Java-Script", "JavaScript");
            correctionAdd("JavaScirpt", "JavaScript"); //Misspelling. Real typo.
            correctionAdd("javascritpt", "JavaScript");
            correctionAdd("JavaScrip", "JavaScript");
            correctionAdd("Javsscript", "JavaScript");
            correctionAdd("javsscript", "JavaScript");
            correctionAdd("Javsacript", "JavaScript");
            correctionAdd("js script", "JavaScript");

            correctionAdd("jquery", "jQuery");
            correctionAdd("JQuery", "jQuery");
            correctionAdd("Jquery", "jQuery");
            correctionAdd("JQUERY", "jQuery");
            correctionAdd("JQUery", "jQuery"); //In an email...
            correctionAdd("Jquey", "jQuery");
            correctionAdd("j-query", "jQuery");
            correctionAdd("Jquer", "jQuery"); //Misspelling.
            correctionAdd("jquey", "jQuery"); //Misspelling.
            correctionAdd("j query", "jQuery");
            correctionAdd("JQ", "jQuery");
            correctionAdd("JQueru", "jQuery"); //Misspelling.
            correctionAdd("Juery", "jQuery"); //Misspelling.
            correctionAdd("jqury", "jQuery"); //Misspelling.
            correctionAdd("jquary", "jQuery"); //Misspelling.
            correctionAdd("jQery", "jQuery"); //Misspelling.
            correctionAdd("jqyery", "jQuery"); //Misspelling.
            correctionAdd("jQuory", "jQuery");
            correctionAdd("jqery", "jQuery");

            correctionAdd("Jquery-UI", "jQuery UI");
            correctionAdd("jquery ui", "jQuery UI");
            correctionAdd("jquery-ui", "jQuery UI");
            correctionAdd("Jquery UI", "jQuery UI");
            correctionAdd("JQueryUI", "jQuery UI"); //From Wikipedia, <http://en.wikipedia.org/wiki/Wikipedia:Counter-Vandalism_Unit>.
            correctionAdd("jQueryUI", "jQuery UI");
            correctionAdd("jqueryui", "jQuery UI");
            correctionAdd("JQuery UI", "jQuery UI");
            correctionAdd("jquery UI", "jQuery UI");
            correctionAdd("JQuery-UI", "jQuery UI");
            correctionAdd("jQureyUI", "jQuery UI");
            correctionAdd("jQuery-ui", "jQuery UI");
            correctionAdd("JqueryUI", "jQuery UI");
            correctionAdd("jQuery-UI", "jQuery UI");
            correctionAdd("jQuery Ui", "jQuery UI");
            correctionAdd("jQuery ui", "jQuery UI");
            correctionAdd("JQUI", "jQuery UI");
            correctionAdd("Jquery Ui", "jQuery UI");
            correctionAdd("JQueru UI", "jQuery UI"); //Misspelling.
            correctionAdd("Jqueryui", "jQuery UI");
            correctionAdd("jQueryUi", "jQuery UI");
            correctionAdd("jquery.ui", "jQuery UI");
            correctionAdd("Jquery-ui", "jQuery UI");

            correctionAdd("AJAX", "Ajax");
            correctionAdd("ajax", "Ajax");
            correctionAdd("Aajax", "Ajax");
            correctionAdd("ajex", "Ajax"); //Misspelling.

            correctionAdd("php", "PHP");
            correctionAdd("Php", "PHP");
            correctionAdd("pHp", "PHP");
            correctionAdd("P.H.P", "PHP");
            correctionAdd("PhP", "PHP");

            correctionAdd("PHP5", "PHP&nbsp;5");
            correctionAdd("php5", "PHP&nbsp;5");
            correctionAdd("php 5", "PHP&nbsp;5");
            correctionAdd("PHP 5", "PHP&nbsp;5");

            correctionAdd("phpmyadmin", "phpMyAdmin");
            correctionAdd("PhpMyAdmin", "phpMyAdmin");
            correctionAdd("PHpMyadmin", "phpMyAdmin");
            correctionAdd("PphMyAdmin", "phpMyAdmin");
            correctionAdd("phpmyAdmin", "phpMyAdmin");
            correctionAdd("PHPMyAdmin", "phpMyAdmin");
            correctionAdd("myPHPAdmin", "phpMyAdmin");
            correctionAdd("MyPHPAdmin", "phpMyAdmin");
            correctionAdd("PHP MY ADMIN", "phpMyAdmin");
            correctionAdd("php myadmin", "phpMyAdmin");
            correctionAdd("Phpmyadmin", "phpMyAdmin");
            correctionAdd("PHPMYADMIN", "phpMyAdmin");
            correctionAdd("phpMyadmin", "phpMyAdmin");

            correctionAdd("cakePhp", "CakePHP");
            correctionAdd("cakePHP", "CakePHP");
            correctionAdd("cakephp", "CakePHP");
            correctionAdd("Cakephp", "CakePHP");
            correctionAdd("CAKEPHP", "CakePHP");
            correctionAdd("Cake", "CakePHP"); //Collisions may be possible...
            correctionAdd("cake", "CakePHP");
            correctionAdd("cake php", "CakePHP");
            correctionAdd("CakePhp", "CakePHP");
            correctionAdd("CakPHP", "CakePHP");

            correctionAdd("PHPBB", "phpBB");
            correctionAdd("phpbb", "phpBB");

            correctionAdd("PhpNuke", "PHP-Nuke");
            correctionAdd("PHPNuke", "PHP-Nuke");

            correctionAdd("mysql", "MySQL");
            correctionAdd("mySQL", "MySQL");
            correctionAdd("mySql", "MySQL");
            correctionAdd("MySql", "MySQL");
            correctionAdd("My-sql", "MySQL");
            correctionAdd("MYSQL", "MySQL");
            correctionAdd("Mysql", "MySQL");
            correctionAdd("myql", "MySQL");
            correctionAdd("MYSql", "MySQL");
            correctionAdd("MysQL", "MySQL");
            correctionAdd("MySSQL", "MySQL");
            correctionAdd("MySQl", "MySQL");
            correctionAdd("my sql", "MySQL");
            correctionAdd("My SQL", "MySQL");
            correctionAdd("MyS.Q.L", "MySQL");
            correctionAdd("my SQL", "MySQL");
            correctionAdd("myqsl", "MySQL");
            correctionAdd("Myql", "MySQL");

            correctionAdd("Linq to SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("linq to sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("linq2sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq2Sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq-to-SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq 2 Sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq Sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LinqToSql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("linq-to-sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LinqSQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq to sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LINQ-to-SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq to Sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LINQ to sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LinqToSQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq-To-SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq-To-Sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq2sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq-to-sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq2SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq To SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq 2 SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("Linq To Sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("L2S", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LINQ2SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LINQ to SQL", "LINQ&nbsp;to&nbsp;SQL"); // Effectively self (line-breaks)
            correctionAdd("linq to Sql", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LINQ 2 SQL", "LINQ&nbsp;to&nbsp;SQL");
            correctionAdd("LINQ2QL", "LINQ&nbsp;to&nbsp;SQL"); //Misspelling.
            correctionAdd("sql2linq", "LINQ&nbsp;to&nbsp;SQL");

            correctionAdd("linq to xml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("LINQ-to-XML", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("Linq to XML", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("linq-to-xml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("Linq-to-xml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("LINQ to XML", "LINQ&nbsp;to&nbsp;XML"); // Effectively self (line-breaks)
            correctionAdd("linq2xml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("Linq to Xml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("Linq to xml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("linqtoxml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("linq-xml", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("XML to LiNQ", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("XLinq", "LINQ&nbsp;to&nbsp;XML"); //Synonym
            correctionAdd("Linq 2 XML", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("Linq-XML", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("Xml Linq", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("XML Linq", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("XML LINQ", "LINQ&nbsp;to&nbsp;XML");
            correctionAdd("LINQ for XML", "LINQ&nbsp;to&nbsp;XML");


            correctionAdd("LINQ to objects", "LINQ&nbsp;to&nbsp;Objects");
            correctionAdd("Linq-to-Objects", "LINQ&nbsp;to&nbsp;Objects");
            correctionAdd("Linq to Objects", "LINQ&nbsp;to&nbsp;Objects");
            correctionAdd("LINQ to Objects", "LINQ&nbsp;to&nbsp;Objects"); // Effectively self (line-breaks)
            correctionAdd("LINQ to Object", "LINQ&nbsp;to&nbsp;Objects");
            correctionAdd("LINQ-to-Objects", "LINQ&nbsp;to&nbsp;Objects");
            correctionAdd("Linq-to-object", "LINQ&nbsp;to&nbsp;Objects");
            correctionAdd("linq to objects", "LINQ&nbsp;to&nbsp;Objects");
            correctionAdd("Linq to objects", "LINQ&nbsp;to&nbsp;Objects");

            correctionAdd("Linq-to-Entities", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("Linq to EF", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("Linq to Entities", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("LinqToEntities", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("LINQ to Entities", "LINQ&nbsp;to&nbsp;Entities"); // Effectively self (line-breaks)
            correctionAdd("linq to entities", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("LINQ to entities", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("linq-to-entities", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("linq-to-entity", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("Linq to entities", "LINQ&nbsp;to&nbsp;Entities");
            correctionAdd("Linq-to-entities", "LINQ&nbsp;to&nbsp;Entities");

            correctionAdd("Linq", "LINQ");
            correctionAdd("linq", "LINQ");
            correctionAdd("LinQ", "LINQ");
            correctionAdd("linQ", "LINQ");

            correctionAdd("sqlite", "SQLite");
            correctionAdd("Sqlite", "SQLite");
            correctionAdd("sqllite", "SQLite");
            correctionAdd("sqite", "SQLite");
            correctionAdd("SQlite", "SQLite");
            correctionAdd("SqLite", "SQLite");
            correctionAdd("SQLITE", "SQLite");
            correctionAdd("sqlLite", "SQLite");
            correctionAdd("SQLLite", "SQLite");
            correctionAdd("SQL Lite", "SQLite");
            correctionAdd("SQL lite", "SQLite");
            correctionAdd("sql lite", "SQLite");
            correctionAdd("SqlLite", "SQLite");
            correctionAdd("sqLite", "SQLite");
            correctionAdd("sq-lite", "SQLite");
            correctionAdd("sq-Lite", "SQLite");
            correctionAdd("SQLIte", "SQLite");

            correctionAdd("pgsql", "PostgreSQL");
            correctionAdd("Postgre", "PostgreSQL");
            correctionAdd("postgre", "PostgreSQL");
            correctionAdd("Postgres", "PostgreSQL");
            correctionAdd("postgres", "PostgreSQL");
            correctionAdd("postgresql", "PostgreSQL");
            correctionAdd("Postgresql", "PostgreSQL");
            correctionAdd("postgres sql", "PostgreSQL");
            correctionAdd("postgreSQL", "PostgreSQL");
            correctionAdd("PostgreSql", "PostgreSQL");
            correctionAdd("postgreql", "PostgreSQL");
            correctionAdd("postgress", "PostgreSQL");
            correctionAdd("PostGreSQL", "PostgreSQL");
            correctionAdd("postgreSql", "PostgreSQL");
            correctionAdd("PostreSQL", "PostgreSQL"); // Misspelling.
            correctionAdd("PG", "PostgreSQL"); // Expansion.
            correctionAdd("pg", "PostgreSQL"); // Expansion.
            correctionAdd("Pg", "PostgreSQL"); // Expansion.
            correctionAdd("PostgresSQL", "PostgreSQL");
            correctionAdd("POSTGRESQL", "PostgreSQL");
            correctionAdd("Postgress", "PostgreSQL");
            correctionAdd("potsgres", "PostgreSQL");
            correctionAdd("postgersql", "PostgreSQL");
            correctionAdd("PostGres", "PostgreSQL");

            correctionAdd("SQL Server", "SQL&nbsp;Server"); // Effectively self
            correctionAdd("MSSQL", "SQL&nbsp;Server");
            correctionAdd("sqlserver", "SQL&nbsp;Server");
            correctionAdd("Sql Server", "SQL&nbsp;Server");
            correctionAdd("SQL server", "SQL&nbsp;Server");
            correctionAdd("sql server", "SQL&nbsp;Server");
            correctionAdd("SqlServer", "SQL&nbsp;Server");
            correctionAdd("sql Server", "SQL&nbsp;Server");
            correctionAdd("Sql server", "SQL&nbsp;Server");
            correctionAdd("mssql", "SQL&nbsp;Server");
            correctionAdd("MS SQL", "SQL&nbsp;Server");
            correctionAdd("SQLServer", "SQL&nbsp;Server");
            correctionAdd("SQL SERVER", "SQL&nbsp;Server");
            correctionAdd("Sql Sever", "SQL&nbsp;Server");
            correctionAdd("sql-server", "SQL&nbsp;Server");
            correctionAdd("MS SQL Server", "SQL&nbsp;Server");
            correctionAdd("Ms Sql server", "SQL&nbsp;Server");
            correctionAdd("MS SQL server", "SQL&nbsp;Server");
            correctionAdd("ms sql", "SQL&nbsp;Server");
            correctionAdd("MS-SQL", "SQL&nbsp;Server");
            correctionAdd("MS Sql Server", "SQL&nbsp;Server");
            correctionAdd("Microsoft SQL", "SQL&nbsp;Server");
            correctionAdd("SQL-Server", "SQL&nbsp;Server");
            correctionAdd("MsSql", "SQL&nbsp;Server");
            correctionAdd("SQl server", "SQL&nbsp;Server");
            correctionAdd("MS Server", "SQL&nbsp;Server");
            correctionAdd("SQLSERVER", "SQL&nbsp;Server");
            correctionAdd("SQLSRV", "SQL&nbsp;Server");
            correctionAdd("slqserver", "SQL&nbsp;Server");
            correctionAdd("MsSQL", "SQL&nbsp;Server");
            correctionAdd("Microsoft SQL Server", "SQL&nbsp;Server");
            correctionAdd("MS sql", "SQL&nbsp;Server");
            correctionAdd("MSSQLSERVER", "SQL&nbsp;Server");
            correctionAdd("MS-SQL-SERVER", "SQL&nbsp;Server");

            correctionAdd("SQLServer 2000", "SQL Server 2000");
            correctionAdd("SQL2000", "SQL Server 2000");
            correctionAdd("MSSQL 2000", "SQL Server 2000");
            correctionAdd("SQLServer2000", "SQL Server 2000");
            correctionAdd("SQL 2000", "SQL Server 2000");
            correctionAdd("sql server 2000", "SQL Server 2000");

            correctionAdd("sql2005", "SQL Server 2005");
            correctionAdd("sql server 2005", "SQL Server 2005");
            correctionAdd("SQL 2005 Server", "SQL Server 2005");
            correctionAdd("SQL server 2005", "SQL Server 2005");
            correctionAdd("sql-server-2005", "SQL Server 2005");
            correctionAdd("SQL2005", "SQL Server 2005");
            correctionAdd("SqlServer2005", "SQL Server 2005");
            correctionAdd("SQLServer2005", "SQL Server 2005");
            correctionAdd("SQL 2005", "SQL Server 2005");
            correctionAdd("sql 2005", "SQL Server 2005");
            correctionAdd("Sql 2005", "SQL Server 2005");
            correctionAdd("Sql server 2005", "SQL Server 2005");
            correctionAdd("SQL&nbsp;Server 2005", "SQL Server 2005");
            correctionAdd("MS SQL 2005", "SQL Server 2005");
            correctionAdd("MSSQL 2005", "SQL Server 2005");

            correctionAdd("sql server 2008", "SQL Server 2008");
            correctionAdd("SQL 2008", "SQL Server 2008");
            correctionAdd("Sql 2008", "SQL Server 2008");
            correctionAdd("Sql Server 2008", "SQL Server 2008");
            correctionAdd("sql 2008", "SQL Server 2008");
            correctionAdd("sql 2008 server", "SQL Server 2008");
            correctionAdd("SQL SERVER 2008", "SQL Server 2008");
            correctionAdd("SQLServer2008", "SQL Server 2008");
            correctionAdd("SQL server2008", "SQL Server 2008");
            correctionAdd("SQLServer 2008", "SQL Server 2008");
            correctionAdd("SqlServer 2008", "SQL Server 2008");
            correctionAdd("MS SQL 2008", "SQL Server 2008");
            correctionAdd("SQL 2008 Server", "SQL Server 2008");
            correctionAdd("Sql server 2008", "SQL Server 2008");
            correctionAdd("MS-SQLServer 2008", "SQL Server 2008");
            correctionAdd("SQL&nbsp;Server 2008", "SQL Server 2008"); // Sort of self
            correctionAdd("SQL-Server 2008", "SQL Server 2008");
            correctionAdd("SQL server 2008", "SQL Server 2008");
            correctionAdd("MSSQL-2008", "SQL Server 2008");
            correctionAdd("MS SQL-2008", "SQL Server 2008");


            correctionAdd("SQL SERVER 2012", "SQL Server 2012");
            correctionAdd("SQL2012", "SQL Server 2012");
            correctionAdd("SQL 2012", "SQL Server 2012");
            correctionAdd("sql 2012", "SQL Server 2012");
            correctionAdd("Sql Server 2012", "SQL Server 2012");
            correctionAdd("SQL&nbsp;Server 2012", "SQL Server 2012");
            correctionAdd("sqlserver 2012", "SQL Server 2012");

            correctionAdd("SQL server 2016", "SQL Server 2016");
            correctionAdd("sql 2016", "SQL Server 2016");
            correctionAdd("SQL 2016", "SQL Server 2016");
            correctionAdd("MSSQL-2016", "SQL Server 2016");

            correctionAdd("sql ce", "SQL Server Compact");
            correctionAdd("SQL compact", "SQL Server Compact");
            correctionAdd("SQL Compact", "SQL Server Compact");
            correctionAdd("Sql Compact", "SQL Server Compact");
            correctionAdd("sql compact", "SQL Server Compact");
            correctionAdd("sqlcompact", "SQL Server Compact");
            correctionAdd("sqlCompact", "SQL Server Compact");
            correctionAdd("sqlce", "SQL Server Compact");
            correctionAdd("SQL CE", "SQL Server Compact");
            correctionAdd("SQL Server CE", "SQL Server Compact");
            correctionAdd("Sql Server CE", "SQL Server Compact");
            correctionAdd("MS SQL Compact edition", "SQL Server Compact");
            correctionAdd("SQL Server Compact edition", "SQL Server Compact");
            correctionAdd("sql server ce", "SQL Server Compact");

            correctionAdd("Tsql", "T-SQL"); //Not actually observed.
            correctionAdd("T/SQL", "T-SQL"); //Is observed!
            correctionAdd("TSQL", "T-SQL");
            correctionAdd("t-sql", "T-SQL");
            correctionAdd("t sql", "T-SQL");
            correctionAdd("TSQl", "T-SQL");
            correctionAdd("Transact-SQL", "T-SQL"); //Contraction
            correctionAdd("tsql", "T-SQL");
            correctionAdd("T-sql", "T-SQL");
            correctionAdd("Transact-Sql", "T-SQL"); //Contraction

            correctionAdd("pl/sql", "PL/SQL");
            correctionAdd("plsql", "PL/SQL");

            correctionAdd("msn", "MSN");

            correctionAdd("WinForms", "Windows Forms");
            correctionAdd("winforms", "Windows Forms");
            correctionAdd("win forms", "Windows Forms");
            correctionAdd("Winform", "Windows Forms");
            correctionAdd("Win-Forms", "Windows Forms");
            correctionAdd("Winforms", "Windows Forms");
            correctionAdd("win form", "Windows Forms");
            correctionAdd("WinForm", "Windows Forms");
            correctionAdd("winform", "Windows Forms");
            correctionAdd("Win Form", "Windows Forms");
            correctionAdd("wiform", "Windows Forms");
            correctionAdd("windows form", "Windows Forms");
            correctionAdd("windows forms", "Windows Forms");
            correctionAdd("Windows forms", "Windows Forms");
            correctionAdd("Window form", "Windows Forms");
            correctionAdd("Window Form", "Windows Forms");
            correctionAdd("WindowsForms", "Windows Forms");
            correctionAdd("Forms", "Windows Forms"); //Short form.
            correctionAdd("Win Forms", "Windows Forms");
            correctionAdd("Windows Form", "Windows Forms");
            correctionAdd("Windows form", "Windows Forms");
            correctionAdd("WIndows forms", "Windows Forms");
            correctionAdd("WINFORMS", "Windows Forms");
            correctionAdd("winForms", "Windows Forms");
            correctionAdd("widnwos forms", "Windows Forms"); //Misspelling.
            correctionAdd("windowsForm", "Windows Forms");
            correctionAdd("WindowsForm", "Windows Forms");
            correctionAdd("wiforms", "Windows Forms");
            correctionAdd("win-forms", "Windows Forms");
            correctionAdd("windows Form", "Windows Forms");
            correctionAdd("WinFroms", "Windows Forms");
            correctionAdd("WIndows Forms", "Windows Forms");
            correctionAdd("Wins Forms", "Windows Forms");
            correctionAdd("windowsfroms", "Windows Forms"); //Misspelling.
            correctionAdd("Win form", "Windows Forms");
            correctionAdd("window form", "Windows Forms");
            correctionAdd("winfrom", "Windows Forms");
            correctionAdd("Win forms", "Windows Forms");
            correctionAdd("WindowForm", "Windows Forms");

            correctionAdd("tiff", "TIFF");
            correctionAdd("tif", "TIFF");

            correctionAdd("android", "Android");
            correctionAdd("andrioid", "Android"); //Misspelling.
            correctionAdd("Anroid", "Android"); //Misspelling.
            correctionAdd("andorid", "Android"); //Misspelling.
            correctionAdd("Andriod", "Android"); //Misspelling.
            correctionAdd("androd", "Android"); //Misspelling.
            correctionAdd("Andorid", "Android"); //Misspelling.
            correctionAdd("ANDROID", "Android");
            correctionAdd("anroid", "Android");
            correctionAdd("andoird", "Android");
            correctionAdd("andiord", "Android");
            correctionAdd("andriod", "Android");

            correctionAdd("iphone", "iPhone");
            correctionAdd("Iphone", "iPhone");
            correctionAdd("i-phone", "iPhone");
            correctionAdd("I-phone", "iPhone");
            correctionAdd("IPHONE", "iPhone");
            correctionAdd("iPHONE", "iPhone");
            correctionAdd("ifone", "iPhone"); //Misspelling.
            correctionAdd("IPhone", "iPhone");

            correctionAdd(".net", ".NET"); //This does NOT work (lookup fails) with the recent changes!!!
            correctionAdd(".Net", ".NET"); //This does NOT work (lookup fails) with the recent changes!!!
            correctionAdd("dotnet", ".NET");
            correctionAdd("dot net", ".NET");
            correctionAdd("Dotnet", ".NET");
            correctionAdd(".NEt", ".NET");
            correctionAdd("DotNet", ".NET");
            correctionAdd("Dot Net", ".NET");
            correctionAdd("Net", ".NET");
            correctionAdd("dotNET", ".NET");
            correctionAdd("dotNet", ".NET");
            correctionAdd(".net framework", ".NET"); //Not 100% correct.
            correctionAdd("netframework", ".NET");
            correctionAdd(".net Framework", ".NET");
            correctionAdd(".Net F/W", ".NET");
            correctionAdd(".NET FW", ".NET");
            correctionAdd("Net framework", ".NET");
            correctionAdd(".Net Framwork", ".NET");
            correctionAdd("Dot.NET", ".NET");
            correctionAdd(".NET FrameWork", ".NET");
            correctionAdd(".NET Framework", ".NET");
            correctionAdd(".NET framework", ".NET"); //Not 100% correct.
            correctionAdd("NetFramework", ".NET");
            correctionAdd("Dot NET", ".NET");
            correctionAdd("dot new", ".NET");
            correctionAdd("Dot.Net", ".NET");
            correctionAdd(".NetFramework", ".NET");
            correctionAdd(".NET frameowkr", ".NET");

            correctionAdd(".net3.5", ".NET 3.5");

            correctionAdd("scala", "Scala");
            correctionAdd("SCALA", "Scala");

            correctionAdd("objective-c", "Objective-C");
            correctionAdd("Objective C", "Objective-C");
            correctionAdd("objective-C", "Objective-C");
            correctionAdd("objective c", "Objective-C");
            correctionAdd("ObjectiveC", "Objective-C");
            correctionAdd("Obetive-C", "Objective-C");
            correctionAdd("Objective c", "Objective-C");
            correctionAdd("objective C", "Objective-C");
            correctionAdd("Objective-c", "Objective-C");
            correctionAdd("Obj-C", "Objective-C");
            correctionAdd("ObjC", "Objective-C");
            correctionAdd("obj c", "Objective-C");
            correctionAdd("obj-c", "Objective-C");
            correctionAdd("objc", "Objective-C");
            correctionAdd("Obj-c", "Objective-C");
            correctionAdd("obj-C", "Objective-C");
            correctionAdd("objectiveC", "Objective-C");
            correctionAdd("Object-C", "Objective-C");
            correctionAdd("objcective-c", "Objective-C");
            correctionAdd("object c", "Objective-C");
            correctionAdd("obective c", "Objective-C");
            correctionAdd("objC", "Objective-C");
            correctionAdd("Obj C", "Objective-C");
            correctionAdd("objective - c", "Objective-C");
            correctionAdd("Objc", "Objective-C");

            correctionAdd("FX cop", "FxCop");
            correctionAdd("fxcop", "FxCop");
            correctionAdd("FXCop", "FxCop");

            correctionAdd("asp.net", "ASP.NET");
            correctionAdd("ASP.net", "ASP.NET");
            correctionAdd("ASP.Net", "ASP.NET");
            correctionAdd("Asp.net", "ASP.NET");
            correctionAdd("Asp.NET", "ASP.NET");
            correctionAdd("Asp.Net", "ASP.NET");
            correctionAdd("ASp.NET", "ASP.NET");
            correctionAdd("asp.NET", "ASP.NET");
            correctionAdd("ASPNET", "ASP.NET");
            correctionAdd("asp.et", "ASP.NET");
            correctionAdd("Aps.net", "ASP.NET"); //Misspelling.
            correctionAdd("AsP.NET", "ASP.NET");
            correctionAdd("APS.NET", "ASP.NET");
            correctionAdd("Asp .Net", "ASP.NET");
            correctionAdd("aspnet", "ASP.NET");
            correctionAdd("ASP .NET", "ASP.NET");
            correctionAdd("ASP NET", "ASP.NET");
            correctionAdd("asp .net", "ASP.NET");
            correctionAdd("asmx", "ASP.NET"); //Is this correct?
            correctionAdd("Asp dot net", "ASP.NET");
            correctionAdd("new ASP", "ASP.NET"); //Synonym.
            correctionAdd("aps.net", "ASP.NET"); //Misspelling.
            correctionAdd("asp .Net", "ASP.NET");
            correctionAdd("ASp.net", "ASP.NET");
            correctionAdd("asp,net", "ASP.NET");
            correctionAdd("ASP Dot NET", "ASP.NET");
            correctionAdd("AspNet", "ASP.NET");
            correctionAdd("Aspdotnet", "ASP.NET");
            correctionAdd("ASP .Net", "ASP.NET");

            correctionAdd("ASP.net MVC", "ASP.NET MVC");
            correctionAdd("ASP.Net MVC", "ASP.NET MVC");
            correctionAdd("asp.net mvc", "ASP.NET MVC");
            correctionAdd("Asp.net mvc", "ASP.NET MVC");
            correctionAdd("asp.net-mvc", "ASP.NET MVC");
            correctionAdd("asp net mvc", "ASP.NET MVC");
            correctionAdd("MVC.Net", "ASP.NET MVC");
            correctionAdd("MVC .Net", "ASP.NET MVC");
            correctionAdd("asp.net MVC", "ASP.NET MVC");
            correctionAdd("mvc.net", "ASP.NET MVC");
            correctionAdd("ASPNET MVC", "ASP.NET MVC");
            correctionAdd("aspnet mvc", "ASP.NET MVC");
            correctionAdd("ASP MVC", "ASP.NET MVC");
            correctionAdd("ASP.MVC", "ASP.NET MVC");
            correctionAdd("Asp.net MVC", "ASP.NET MVC"); //Seen on a comment to Jeff Atwood's blog [sic].
            correctionAdd("ASP.Net MvC", "ASP.NET MVC");
            correctionAdd("Aps.net MVC", "ASP.NET MVC");
            correctionAdd("MVC", "ASP.NET MVC");
            correctionAdd("MVC3", "ASP.NET MVC"); //Not strictly match - 3 is lost.
            correctionAdd("mvc3", "ASP.NET MVC"); //Not strictly match - 3 is lost.
            correctionAdd("mvc", "ASP.NET MVC"); //General, but is normally what is meant.
            correctionAdd("Asp .Net MVC", "ASP.NET MVC");
            correctionAdd("Asp.Net Mvc", "ASP.NET MVC");
            correctionAdd(".NET mvc", "ASP.NET MVC");
            correctionAdd(".net mvc", "ASP.NET MVC");
            correctionAdd(".net MVC", "ASP.NET MVC");
            correctionAdd(".NET MVC", "ASP.NET MVC");
            correctionAdd("Asp.Net MVC", "ASP.NET MVC");
            correctionAdd("ASP.NET-MVC", "ASP.NET MVC");
            correctionAdd("asp .Net MVC", "ASP.NET MVC");
            correctionAdd("Mvc", "ASP.NET MVC");
            correctionAdd("Asp.net Mvc", "ASP.NET MVC");
            correctionAdd("ASP .NET MVC", "ASP.NET MVC");

            correctionAdd("asp", "ASP Classic");
            correctionAdd("ASP", "ASP Classic");
            correctionAdd("ASP classic", "ASP Classic");
            correctionAdd("Classic ASP", "ASP Classic");
            correctionAdd("classic ASP", "ASP Classic");
            correctionAdd("Classic-ASP", "ASP Classic");
            correctionAdd("classic asp", "ASP Classic");
            correctionAdd("asp classic", "ASP Classic");
            correctionAdd("old ASP", "ASP Classic"); //Synonym.
            correctionAdd("classicASP", "ASP Classic");
            correctionAdd("ASPclassic", "ASP Classic");
            correctionAdd("classicasp", "ASP Classic");
            correctionAdd("Classic Asp", "ASP Classic");
            correctionAdd("Asp Classic", "ASP Classic");

            correctionAdd("asp core", "ASP.NET Core");
            correctionAdd("Asp.net core", "ASP.NET Core");
            correctionAdd("aspnet core", "ASP.NET Core");
            correctionAdd("asp.net core", "ASP.NET Core");
            correctionAdd("ASP core", "ASP.NET Core");
            correctionAdd("ASP.Net core", "ASP.NET Core");
            correctionAdd("ASP.Net Core", "ASP.NET Core");
            correctionAdd("ASP.NET CORE", "ASP.NET Core");
            correctionAdd("asp net core", "ASP.NET Core");
            correctionAdd(".net core mvc", "ASP.NET Core");
            correctionAdd(".NET Core mvc", "ASP.NET Core");
            correctionAdd(".NET Core MVC", "ASP.NET Core");
            correctionAdd(".NET Core ASP.NET MVC", "ASP.NET Core");

            correctionAdd("osx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("os x", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("OS X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("OSX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("OS/X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac os X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac OSX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac osx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac OS X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MacOSX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac OSx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MAC OS X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac os x", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("OS-X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("OsX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MacOS X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac os", "Mac&nbsp;OS&nbsp;X"); //Interpreting this as Mac OS X, not MacOS (System 7, etc.)
            correctionAdd("macosx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac OS X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac OSX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MACOSX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac OS-X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("os/x", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MacOs X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac OsX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MAC 0S X", "Mac&nbsp;OS&nbsp;X"); //Uses the number zero, not the letter "O".
            correctionAdd("MAC OSX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac/OSx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MacOsX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MacOSx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("OS&nbsp;X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MacOS/X", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("macos", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac OSx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Macsox", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("MAC-OS", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("mac-osx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac osx", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac OS x", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac osX", "Mac&nbsp;OS&nbsp;X");
            correctionAdd("Mac os X", "Mac&nbsp;OS&nbsp;X");

            correctionAdd("mac", "Mac"); //Collision with MAC address, but there is an entry for that, "MAC address".
            correctionAdd("macintosh", "Mac"); //Collision with MAC address, but there is an entry for that, "MAC address".

            correctionAdd("Jaguar", "Mac&nbsp;OS&nbsp;X&nbsp;v10.2 (Jaguar)");

            correctionAdd("Tiger", "Mac&nbsp;OS&nbsp;X&nbsp;v10.4 (Tiger)");
            correctionAdd("Mac&nbsp;OS&nbsp;X&nbsp;v10.4", "Mac&nbsp;OS&nbsp;X&nbsp;v10.4 (Tiger)");
            correctionAdd("MacOS 10.4", "Mac&nbsp;OS&nbsp;X&nbsp;v10.4 (Tiger)");

            correctionAdd("Leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)");
            correctionAdd("leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)");
            correctionAdd("Mac OS X Leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)");
            correctionAdd("Mac&nbsp;OS&nbsp;X&nbsp;v10.5", "Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)");
            correctionAdd("OSX 10.5", "Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)");
            correctionAdd("10.5", "Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)");
            correctionAdd("OS X 10.5", "Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)");

            correctionAdd("snow leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("Snow leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("snow-leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("Mac OS X Snow Leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("Snow Leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("SnowLeopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("OSX 10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("OS X 10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("Snow&nbsp;Leopard", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("Mac OS X 10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("OSX-10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("OS&nbsp;X&nbsp;v10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("v10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");
            correctionAdd("Mac OS 10.6", "Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)");

            correctionAdd("Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("OSX Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("Mac OS X Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("OS 10.7", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("Lion X", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("Mac Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("10.7", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("OSX 10.7", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("OS&nbsp;X 10.7", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("OS X 10.7", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("Mac OS X 10.7", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("OS X Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("mac os x lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("os x lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("osx lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("Mac OSx Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("v10.7", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");
            correctionAdd("OSX lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)");

            correctionAdd("Mountain Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("mountain lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("OS X 10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("OSX 10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("Mac OS X 10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("Mac OSX 10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("OSX Mountain Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("mac os 10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)"); //Missing "X"...
            correctionAdd("OS X version 10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("Mac&nbsp;OS&nbsp;X 10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("OS&nbsp;X&nbsp;v10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("Mountain lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("v10.8", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("Mac OSX Mountain Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");
            correctionAdd("Mac OS X Mountain Lion", "Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)");

            //Note: the identity is not working. Is it because of the
            //trailing ")"??
            correctionAdd("OSX mavericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Mavericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("OSX Mavericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Mac OS X 10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("OS X 10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("OSX 10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("maverick", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("mac OS X mavericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Mac OS 10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("OS&nbsp;X v10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Maveriks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Maverick", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("osx mavericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Mac&nbsp;OS&nbsp;X 10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("mac OSx 10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("OS X Mavericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Mac 10.9", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("makericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Maverics", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");
            correctionAdd("Mac OS X Mavericks", "Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)");

            correctionAdd("Yosemite", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");
            correctionAdd("OSX Yosemite", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");
            correctionAdd("OS&nbsp;X v10.10", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");
            correctionAdd("OS X 10.10", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");
            correctionAdd("Mac OSX Yosemite", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");
            correctionAdd("mac os 10.10", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");
            correctionAdd("Mac OS X 10.10", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");
            correctionAdd("v10.10", "Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)");

            correctionAdd("El Capitan", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("10.11", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("OS&nbsp;X v10.11", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("OS&nbsp;X&nbsp;v10.11", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("el capitan", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("os x el capitan", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("OS&nbsp;X 10.11", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("OSX 10.11", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("OS X 10.11", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("EI Capitan", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("El capitan", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("osx 10.11", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("osx 10.11 El Capitan", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("El Captain", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("OSX El Capitan", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");
            correctionAdd("El Capitain", "Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)");

            correctionAdd("Sierra", "macOS v10.12 (Sierra)");
            correctionAdd("10.12", "macOS v10.12 (Sierra)");
            correctionAdd("macOS v10.12", "macOS v10.12 (Sierra)");
            correctionAdd("Seirra", "macOS v10.12 (Sierra)");
            correctionAdd("macOS Sierra", "macOS v10.12 (Sierra)");

            correctionAdd("macOS High Sierra", "macOS v10.13 (High Sierra)");
            correctionAdd("macOS v10.13", "macOS v10.13 (High Sierra)");
            correctionAdd("10.13", "macOS v10.13 (High Sierra)");
            correctionAdd("High Sierra", "macOS v10.13 (High Sierra)");
            correctionAdd("Mac OS High Sierra", "macOS v10.13 (High Sierra)");
            correctionAdd("macos 10.13", "macOS v10.13 (High Sierra)");
            correctionAdd("high sierra", "macOS v10.13 (High Sierra)");

            correctionAdd("Mojave", "macOS v10.14 (Mojave)");
            correctionAdd("macOS Mojave", "macOS v10.14 (Mojave)");
            correctionAdd("mojave", "macOS v10.14 (Mojave)");

            correctionAdd("macOS v10.15", "macOS v10.15 (Catalina)");
            correctionAdd("Catalina", "macOS v10.15 (Catalina)");


            //Adobe Flex. Conflict with the lexical analyser Flex (http://en.wikipedia.org/wiki/Flex_lexical_analyser).
            correctionAdd("flex", "Flex");
            correctionAdd("FLEX", "Flex");

            correctionAdd("action script", "ActionScript");
            correctionAdd("actionscript", "ActionScript");
            correctionAdd("Actionscript", "ActionScript");
            correctionAdd("Actinscript", "ActionScript");
            correctionAdd("Action Script", "ActionScript");

            correctionAdd("AS3", "ActionScript"); //No stricly correct as the "3" will be left out.
            correctionAdd("as3", "ActionScript"); //No stricly correct as the "3" will be left out.

            correctionAdd("as2", "ActionScript"); //No stricly correct as the "2" will be left out.

            correctionAdd("SP", "stored procedure"); //Collision with SharePoint.
            correctionAdd("SPs", "stored procedure");
            correctionAdd("sp", "stored procedure");
            correctionAdd("sproc", "stored procedure");
            correctionAdd("sprocs", "stored procedure");
            correctionAdd("StoredProcedure", "stored procedure");
            correctionAdd("stored proc", "stored procedure");
            correctionAdd("Stored Procedure", "stored procedure");
            correctionAdd("Stored Procedures", "stored procedure"); //Not 100% correct - plural.
            correctionAdd("StoredProcedures", "stored procedure");
            correctionAdd("Stored procedures", "stored procedure");
            correctionAdd("stored procedures", "stored procedure");
            correctionAdd("Stored Procs", "stored procedure"); //Not 100% correct - plural.
            correctionAdd("store procedure", "stored procedure");
            correctionAdd("Stored procedure", "stored procedure");
            correctionAdd("stored procedured", "stored procedure");
            correctionAdd("Storedprocedure", "stored procedure");
            correctionAdd("StoreProcedure", "stored procedure");
            correctionAdd("stored porcedure", "stored procedure");

            //Expansion, not case correction.
            correctionAdd("db", "database");
            correctionAdd("DB", "database");
            correctionAdd("Db", "database");
            correctionAdd("dbase", "database");
            correctionAdd("datbase", "database");
            correctionAdd("DataBase", "database");
            correctionAdd("data base", "database");
            correctionAdd("dB", "database");
            correctionAdd("databse", "database");
            correctionAdd("databese", "database");

            correctionAdd("ria", "RIA");
            correctionAdd("Rich Internet application", "RIA");
            correctionAdd("Rich Internet Application", "RIA");
            correctionAdd("rich internet application", "RIA");
            correctionAdd("rich Internet application", "RIA");

            correctionAdd("Subsonic", "SubSonic");
            correctionAdd("subsonic", "SubSonic");

            //Expansion, not case correction.
            correctionAdd("Entity Framework", "ADO.NET Entity Framework");
            correctionAdd("EF", "ADO.NET Entity Framework");
            correctionAdd("ADO.Net Entity Framework", "ADO.NET Entity Framework");
            correctionAdd("entity framework", "ADO.NET Entity Framework");
            correctionAdd("Entities framework", "ADO.NET Entity Framework");
            correctionAdd("Ado.net entity framework", "ADO.NET Entity Framework");
            correctionAdd("ef", "ADO.NET Entity Framework");
            correctionAdd("ado.net entity framework", "ADO.NET Entity Framework");
            correctionAdd("Entity framework", "ADO.NET Entity Framework");
            correctionAdd("EntityFramework", "ADO.NET Entity Framework");
            correctionAdd("ADO.Net entity framework", "ADO.NET Entity Framework");
            correctionAdd("Entitiy framework", "ADO.NET Entity Framework");
            correctionAdd("enitity framewrok", "ADO.NET Entity Framework");
            correctionAdd("entityframwork", "ADO.NET Entity Framework");
            correctionAdd("Enity framework", "ADO.NET Entity Framework");

            correctionAdd("ado.net", "ADO.NET");
            correctionAdd("Ado.Net", "ADO.NET");
            correctionAdd("ADO.Net", "ADO.NET");
            correctionAdd("ADO.net", "ADO.NET");

            correctionAdd("wpf", "WPF");
            correctionAdd("Wpf", "WPF");
            correctionAdd("Windows Presentation Foundation", "WPF");

            correctionAdd("msdn", "MSDN");
            correctionAdd("msndn", "MSDN"); //Misspelling.
            correctionAdd("mdsn", "MSDN"); //Misspelling.

            correctionAdd("Powerbuilder", "PowerBuilder");
            correctionAdd("powerbuilder", "PowerBuilder");
            correctionAdd("PB", "PowerBuilder");

            correctionAdd("swing", "Swing");
            correctionAdd("SWING", "Swing");

            correctionAdd("qt", "Qt");
            correctionAdd("QT", "Qt");

            correctionAdd("Qt creator", "Qt Creator");
            correctionAdd("QtCreator", "Qt Creator");
            correctionAdd("Qt creater", "Qt Creator");
            correctionAdd("QT Creator", "Qt Creator");
            correctionAdd("QT-Creator", "Qt Creator");
            correctionAdd("qt-creator", "Qt Creator");
            correctionAdd("qtcreator", "Qt Creator");
            correctionAdd("qt creator", "Qt Creator");
            correctionAdd("Qtcreater", "Qt Creator");
            correctionAdd("QT creator", "Qt Creator");
            correctionAdd("QTCreator", "Qt Creator");
            correctionAdd("QtCreater", "Qt Creator");
            correctionAdd("Qt Createor", "Qt Creator");

            correctionAdd("Intellisense", "IntelliSense");
            correctionAdd("intellisense", "IntelliSense");
            correctionAdd("intelisense", "IntelliSense");
            correctionAdd("intellsense", "IntelliSense"); //Misspelling.
            correctionAdd("intellisence", "IntelliSense"); //Misspelling.

            correctionAdd("internet", "Internet");
            correctionAdd("interwebz", "Internet");
            correctionAdd("interweb", "Internet");
            correctionAdd("i-net", "Internet");
            correctionAdd("INTERNET", "Internet");
            correctionAdd("net", "Internet");
            correctionAdd("interent", "Internet");
            correctionAdd("inter-net", "Internet");
            correctionAdd("interwebs", "Internet");

            correctionAdd("Youtube", "YouTube");
            correctionAdd("youtube", "YouTube");
            correctionAdd("YT", "YouTube"); //Expansion
            correctionAdd("you-tube", "YouTube"); //Expansion
            correctionAdd("You Tube", "YouTube");
            correctionAdd("YOU TUBE", "YouTube");
            correctionAdd("you tube", "YouTube");
            correctionAdd("yt", "YouTube");
            correctionAdd("youTube", "YouTube");
            correctionAdd("You tube", "YouTube");
            correctionAdd("Youtub", "YouTube");
            correctionAdd("YouTuBe", "YouTube");
            correctionAdd("YOUTUBE", "YouTube");

            correctionAdd("utf-8", "UTF-8");
            correctionAdd("utf8", "UTF-8");
            correctionAdd("UTF8", "UTF-8");
            correctionAdd("Utf-8", "UTF-8");
            correctionAdd("Utf8", "UTF-8");
            correctionAdd("UTF 8", "UTF-8");

            correctionAdd("utf-16", "UTF-16");
            correctionAdd("UTF16", "UTF-16");
            correctionAdd("utf16", "UTF-16");

            correctionAdd("intellij", "IntelliJ IDEA");
            correctionAdd("IntellJ Idea", "IntelliJ IDEA");
            correctionAdd("IntelliJ", "IntelliJ IDEA");
            correctionAdd("Intellij", "IntelliJ IDEA");
            correctionAdd("InjelliJ", "IntelliJ IDEA");
            correctionAdd("IDEA", "IntelliJ IDEA");
            correctionAdd("IntelliJ Idea", "IntelliJ IDEA");
            correctionAdd("Intellij IDEA", "IntelliJ IDEA");
            correctionAdd("intelli", "IntelliJ IDEA"); //Expansion (although it could also be IntelliSense).
            correctionAdd("Intellij Idea", "IntelliJ IDEA");
            correctionAdd("INTELLIJ", "IntelliJ IDEA");
            correctionAdd("INTELLIJ IDEA", "IntelliJ IDEA");
            correctionAdd("intellij idea", "IntelliJ IDEA");
            correctionAdd("InteliJ", "IntelliJ IDEA");
            correctionAdd("Intellj", "IntelliJ IDEA");
            correctionAdd("IntellijIDEA", "IntelliJ IDEA");
            correctionAdd("Intelij Idea", "IntelliJ IDEA");
            correctionAdd("intelliJ IDEA", "IntelliJ IDEA");
            correctionAdd("inteliJ", "IntelliJ IDEA");
            correctionAdd("IntelliJ idea", "IntelliJ IDEA");
            correctionAdd("IntellJDE", "IntelliJ IDEA");
            correctionAdd("intelijj", "IntelliJ IDEA");
            correctionAdd("intelij", "IntelliJ IDEA");
            correctionAdd("Intelli J", "IntelliJ IDEA");
            correctionAdd("intelj", "IntelliJ IDEA");
            correctionAdd("inteilli", "IntelliJ IDEA");
            correctionAdd("Idea Intellij", "IntelliJ IDEA");
            correctionAdd("Idea intellij", "IntelliJ IDEA");
            correctionAdd("idea intellij", "IntelliJ IDEA");

            correctionAdd("couchdb", "CouchDB");
            correctionAdd("Couchdb", "CouchDB");
            correctionAdd("CouchDb", "CouchDB");

            correctionAdd(".Net reflector", ".NET Reflector");
            correctionAdd(".NET reflector", ".NET Reflector");
            correctionAdd("Reflector", ".NET Reflector");
            correctionAdd("reflector", ".NET Reflector");
            correctionAdd(".net reflector", ".NET Reflector");
            correctionAdd(".Net Reflector", ".NET Reflector");

            correctionAdd("NH", "NHibernate");
            correctionAdd("nhibernate", "NHibernate");
            correctionAdd("Nhibernate", "NHibernate");
            correctionAdd("nHibernate", "NHibernate");

            correctionAdd("hibernate", "Hibernate");
            correctionAdd("HIBERNATE", "Hibernate");
            correctionAdd("Hybernate", "Hibernate");

            correctionAdd("poco", "POCO");
            correctionAdd("PoCos", "POCO"); //Plural
            correctionAdd("PoCo", "POCO");
            correctionAdd("pocos", "POCO"); //Plural

            correctionAdd("struts", "Struts");
            correctionAdd("s2", "Struts"); //Actually Struts 2.
            correctionAdd("S2", "Struts"); //Actually Struts 2.

            correctionAdd("boost", "Boost");
            correctionAdd("Boost C++ Libraries", "Boost");
            correctionAdd("BOOST", "Boost");
            correctionAdd("bosst", "Boost");

            correctionAdd("CI", "continuous integration"); //Collision with CodeIgniter (but that is used less often).
            correctionAdd("Continuous Integration", "continuous integration");
            correctionAdd("Continuous integration", "continuous integration");

            correctionAdd("dpi", "DPI");
            correctionAdd("Dpi", "DPI");

            correctionAdd("numpy", "NumPy");
            correctionAdd("NUmPy", "NumPy");
            correctionAdd("Numpy", "NumPy");
            correctionAdd("numPy", "NumPy");
            correctionAdd("NUMPY", "NumPy");

            correctionAdd("bash", "Bash");
            correctionAdd("BASH", "Bash");

            correctionAdd("WIX", "WiX");
            correctionAdd("Wix", "WiX");
            correctionAdd("wix", "WiX");
            correctionAdd("WIx", "WiX");

            correctionAdd("perl", "Perl");
            correctionAdd("PERL", "Perl");
            correctionAdd("Pearl", "Perl");

            correctionAdd("Powershell", "PowerShell");
            correctionAdd("powershell", "PowerShell");
            correctionAdd("Power Shell", "PowerShell");
            correctionAdd("power shell", "PowerShell");
            correctionAdd("Power-shell", "PowerShell");
            correctionAdd("Poweshell", "PowerShell"); //Misspelling.
            correctionAdd("PSH", "PowerShell");
            correctionAdd("POWERSHELL", "PowerShell");
            correctionAdd("poowershell", "PowerShell");
            correctionAdd("Powereshell", "PowerShell");
            correctionAdd("Power shell", "PowerShell");
            correctionAdd("poweshell", "PowerShell"); //Misspelling.
            correctionAdd("power-shell", "PowerShell");
            correctionAdd("powershelgl", "PowerShell"); //Misspelling.
            correctionAdd("powershelll", "PowerShell");
            correctionAdd("powershel", "PowerShell");
            correctionAdd("Powerhshell", "PowerShell");
            correctionAdd("Powershelll", "PowerShell");
            correctionAdd("PoSH", "PowerShell");
            correctionAdd("poSH", "PowerShell");
            correctionAdd("POSH", "PowerShell");
            correctionAdd("PoSh", "PowerShell");
            correctionAdd("PS", "PowerShell"); // Conflict with abbr. "PS".
            correctionAdd("posh", "PowerShell");
            correctionAdd("Posh", "PowerShell");
            correctionAdd("powerShell", "PowerShell");
            correctionAdd("POsh", "PowerShell");
            correctionAdd("powsershell", "PowerShell");

            correctionAdd("Expression blend", "Expression Blend");
            correctionAdd("blend", "Expression Blend");
            correctionAdd("Blend", "Expression Blend");

            correctionAdd("vim", "Vim");
            correctionAdd("VIM", "Vim");
            correctionAdd("ViM", "Vim");
            correctionAdd("viM", "Vim");
            correctionAdd("VIm", "Vim");

            correctionAdd("gvim", "gVim");
            correctionAdd("GVIM", "gVim");
            correctionAdd("gVIM", "gVim");
            correctionAdd("Gvim", "gVim");
            correctionAdd("GVim", "gVim");
            correctionAdd("givm", "gVim");

            correctionAdd("VI", "vi");
            correctionAdd("Vi", "vi");

            correctionAdd("cygwin", "Cygwin");
            correctionAdd("CygWin", "Cygwin");
            correctionAdd("Cyqwin", "Cygwin");
            correctionAdd("cgywin", "Cygwin");
            correctionAdd("Cggwin", "Cygwin");

            correctionAdd("sourceforge", "SourceForge");
            correctionAdd("source forge", "SourceForge");
            correctionAdd("Sourceforge", "SourceForge");
            correctionAdd("Sourcefourge", "SourceForge");
            correctionAdd("Source Forge", "SourceForge");

            //Note: official name is OpenOffice.org.
            correctionAdd("open office", "OpenOffice");
            correctionAdd("Open Office", "OpenOffice");
            correctionAdd("Openoffice", "OpenOffice");
            correctionAdd("openoffice", "OpenOffice");
            correctionAdd("OOo", "OpenOffice");

            correctionAdd("vb script", "VBScript");
            correctionAdd("vbscript", "VBScript");
            correctionAdd("Vbscript", "VBScript");
            correctionAdd("VB Script", "VBScript");
            correctionAdd("VBSscript", "VBScript"); //Misspelling.
            correctionAdd("vbs", "VBScript");
            correctionAdd("VBS", "VBScript");
            correctionAdd("VBscript", "VBScript");
            correctionAdd("VB-script", "VBScript");
            correctionAdd("VB script", "VBScript");
            correctionAdd("VBS script", "VBScript");
            correctionAdd("VBSCRIPT", "VBScript");
            correctionAdd("vbsscript", "VBScript");
            correctionAdd("Visual Basic Script", "VBScript");
            correctionAdd("vbScript", "VBScript");



            correctionAdd("matlab", "MATLAB");
            correctionAdd("Matlab", "MATLAB");
            correctionAdd("MatLab", "MATLAB");
            correctionAdd("MatLAB", "MATLAB");
            correctionAdd("MATlab", "MATLAB");
            correctionAdd("Matalab", "MATLAB"); //Misspelling.
            correctionAdd("matalab", "MATLAB");
            correctionAdd("MATLab", "MATLAB");
            correctionAdd("MAtlab", "MATLAB");

            correctionAdd("segfault", "segmentation fault");
            correctionAdd("seg fault", "segmentation fault");
            correctionAdd("seg. fault", "segmentation fault");
            correctionAdd("SegFault", "segmentation fault");
            correctionAdd("segfaults", "segmentation fault");
            correctionAdd("segv", "segmentation fault");
            correctionAdd("seg-fault", "segmentation fault");
            correctionAdd("Segmentation Fault", "segmentation fault");

            correctionAdd("json", "JSON");
            correctionAdd("Json", "JSON");
            correctionAdd("JSon", "JSON");
            correctionAdd("JOSN", "JSON");

            correctionAdd("firebug", "Firebug");
            correctionAdd("FireBug", "Firebug");
            correctionAdd("fire bug", "Firebug");
            correctionAdd("fireBug", "Firebug");

            correctionAdd("drupal", "Drupal");
            correctionAdd("DRUPAL", "Drupal");

            correctionAdd("pyqt", "PyQt");

            correctionAdd("GMail", "Gmail");
            correctionAdd("gmail", "Gmail");
            correctionAdd("GMAIL", "Gmail");
            correctionAdd("G-mail", "Gmail");
            correctionAdd("g-mail", "Gmail");

            correctionAdd("resharper", "ReSharper");
            correctionAdd("Resharper", "ReSharper");
            correctionAdd("re-sharper", "ReSharper");
            correctionAdd("R#", "ReSharper"); //Actually observed!
            correctionAdd("r#", "ReSharper");
            correctionAdd("ReShapter", "ReSharper");
            correctionAdd("reSharper", "ReSharper");
            correctionAdd("Re-sharper", "ReSharper");
            correctionAdd("ReShaper", "ReSharper");

            correctionAdd("tdd", "test-driven development");
            correctionAdd("Test Driven Development", "test-driven development");
            correctionAdd("Test Driven Developmen", "test-driven development");
            correctionAdd("Test-driven development", "test-driven development");
            correctionAdd("Test-Driven Development", "test-driven development");
            correctionAdd("test driven development", "test-driven development");
            correctionAdd("TDD", "test-driven development");

            correctionAdd("Xampp", "XAMPP");
            correctionAdd("XAMMP", "XAMPP"); //Observed misspelling! P -> M
            correctionAdd("xampp", "XAMPP");
            correctionAdd("xammp", "XAMPP");
            correctionAdd("Xammp", "XAMPP");
            correctionAdd("xamp", "XAMPP");
            correctionAdd("Xamp", "XAMPP");
            correctionAdd("X.A.M.P.P", "XAMPP");
            correctionAdd("XAMP", "XAMPP");
            correctionAdd("XAAMP", "XAMPP");
            correctionAdd("XXAMP", "XAMPP");

            correctionAdd("yui", "YUI");
            correctionAdd("Yui", "YUI");
            correctionAdd("Yahoo UI Library", "YUI");

            correctionAdd("SED", "sed");
            correctionAdd("Sed", "sed");

            correctionAdd("awk", "AWK");
            correctionAdd("Awk", "AWK");

            correctionAdd("tcl", "Tcl");
            correctionAdd("TCL", "Tcl");

            correctionAdd("lua", "Lua");
            correctionAdd("LUA", "Lua");

            correctionAdd("ANT", "Ant");
            correctionAdd("ant", "Ant");

            correctionAdd("WIFI", "Wi-Fi");
            correctionAdd("Wifi", "Wi-Fi");
            correctionAdd("WiFi", "Wi-Fi");
            correctionAdd("wifi", "Wi-Fi");
            correctionAdd("wi-fi", "Wi-Fi");
            //correctionAdd("Wi-fi", "Wi-Fi");
            correctionAdd("wiFi", "Wi-Fi");
            correctionAdd("WI-FI", "Wi-Fi");
            correctionAdd("WIFi", "Wi-Fi");
            correctionAdd("Wi Fi", "Wi-Fi");

            correctionAdd("IO", "I/O");
            correctionAdd("io", "I/O");
            correctionAdd("i/o", "I/O");
            correctionAdd("i / o", "I/O");
            correctionAdd("I/o", "I/O");

            correctionAdd("vlc", "VLC media player");
            correctionAdd("VLC", "VLC media player");
            correctionAdd("vlc media player", "VLC media player");
            correctionAdd("VLC player", "VLC media player");
            correctionAdd("VLC Player", "VLC media player");
            correctionAdd("vlc player", "VLC media player");
            correctionAdd("VLC Media Player", "VLC media player");

            correctionAdd("emacs", "Emacs");
            correctionAdd("EMACS", "Emacs");
            correctionAdd("EMACs", "Emacs");

            correctionAdd("python", "Python");
            correctionAdd("PYTHON", "Python");
            correctionAdd("Pyton", "Python"); //Misspelling.
            correctionAdd("pyhton", "Python");
            correctionAdd("py", "Python");
            correctionAdd("phython", "Python");
            correctionAdd("Pthon", "Python");
            correctionAdd("Pytohon", "Python");

            correctionAdd("ipython", "IPython");
            correctionAdd("iPython", "IPython");
            correctionAdd("Ipython", "IPython");

            correctionAdd("iPython notebook", "IPython Notebook");
            correctionAdd("iPython-notebook", "IPython Notebook");
            correctionAdd("Ipython Notebook", "IPython Notebook");
            correctionAdd("python notebook", "IPython Notebook");

            correctionAdd("Jupyter nootebook", "Jupyter Notebook");
            correctionAdd("jupyter notebook", "Jupyter Notebook");
            correctionAdd("ipython notebook", "Jupyter Notebook");
            correctionAdd("JUPYTER NOTEBOOK", "Jupyter Notebook");
            correctionAdd("Jupyter Notebooks", "Jupyter Notebook");

            correctionAdd("maven", "Maven");
            correctionAdd("MAVEN", "Maven");

            correctionAdd("greasemonkey", "Greasemonkey");
            correctionAdd("grease monkey", "Greasemonkey");
            correctionAdd("GreaseMonkey", "Greasemonkey");
            correctionAdd("greasmonkey", "Greasemonkey");
            correctionAdd("GM", "Greasemonkey");

            correctionAdd("putty", "PuTTY");
            correctionAdd("Putty", "PuTTY");
            correctionAdd("PUTTY", "PuTTY");
            correctionAdd("puTTY", "PuTTY");
            correctionAdd("puTTy", "PuTTY");
            correctionAdd("PuTTy", "PuTTY");
            correctionAdd("puTTty", "PuTTY");
            correctionAdd("puttty", "PuTTY");

            correctionAdd("ta", "TA");

            correctionAdd("codeigniter", "CodeIgniter");
            correctionAdd("Codeigniter", "CodeIgniter");
            correctionAdd("ci", "CodeIgniter"); //Possible collision with continuous integration (but this was observed first).
            correctionAdd("codeiginter", "CodeIgniter");
            correctionAdd("Code Igniter", "CodeIgniter");
            correctionAdd("CodeIgnitor", "CodeIgniter");
            correctionAdd("code igniter", "CodeIgniter");
            correctionAdd("codeignitor", "CodeIgniter");
            //correctionAdd("codigniter model", "CodeIgniter");
            correctionAdd("Code igniter", "CodeIgniter");
            correctionAdd("codigniter", "CodeIgniter");
            correctionAdd("codeigntier", "CodeIgniter"); //Misspelling.
            correctionAdd("Codeignitor", "CodeIgniter"); //Misspelling.
            correctionAdd("Code-ignitor", "CodeIgniter"); //Misspelling.
            correctionAdd("codeignaitor", "CodeIgniter"); //Misspelling.
            correctionAdd("Codeignier", "CodeIgniter"); //Misspelling.
            correctionAdd("CodIgniter", "CodeIgniter"); //Misspelling.
            correctionAdd("codeIgniter", "CodeIgniter");
            correctionAdd("cideigniter", "CodeIgniter");

            correctionAdd("glassfish", "GlassFish");
            correctionAdd("glasfish", "GlassFish");
            correctionAdd("Glassfish", "GlassFish");
            correctionAdd("galssfish", "GlassFish"); //Misspelling.

            correctionAdd("7Zip", "7-Zip");
            correctionAdd("7zip", "7-Zip");
            correctionAdd("7-zip", "7-Zip");
            correctionAdd("7z", "7-Zip"); //Depending on context...
            correctionAdd("7ZIP", "7-Zip");
            correctionAdd("7xip", "7-Zip");
            correctionAdd("7 zip", "7-Zip");
            correctionAdd("7 Zip", "7-Zip");
            correctionAdd("7-ZIP", "7-Zip");

            //Not really for a Wikipedia link, but more to get the expansion.
            correctionAdd("syncornize", "synchronise");
            correctionAdd("sychronized", "synchronise"); // Not 100% correct. Add a tense feature?
            correctionAdd("syncing", "synchronise"); // Not 100% correct. Add a tense feature?
            correctionAdd("sync", "synchronise");
            correctionAdd("Sync", "synchronise");
            correctionAdd("syncs", "synchronise"); // Expansion, not 100% correct (grammar, third-parson in original)
            correctionAdd("syncronize", "synchronise");
            correctionAdd("synch", "synchronise");
            correctionAdd("synchonize", "synchronise");
            correctionAdd("sinchronyze", "synchronise");
            correctionAdd("synchronyze", "synchronise");
            correctionAdd("syncronous", "synchronise");
            correctionAdd("syncronized", "synchronise");  //Not 100% correct. Add a tense feature?

            correctionAdd("async", "asynchronously");
            correctionAdd("Async", "asynchronously");
            correctionAdd("ASync", "asynchronously");
            correctionAdd("ASYNC", "asynchronously");
            correctionAdd("asnychronus", "asynchronously"); //Not 100% correct
            correctionAdd("asyncronouse", "asynchronously"); //Not 100% correct
            correctionAdd("asyncronous", "asynchronously");
            correctionAdd("asyncrhonous", "asynchronously");

            correctionAdd("sifr", "sIFR");
            correctionAdd("Sifr", "sIFR");

            correctionAdd("BootCamp", "Boot Camp");
            correctionAdd("Bootcamp", "Boot Camp");
            correctionAdd("bootcamp", "Boot Camp");
            correctionAdd("boot camp", "Boot Camp");

            correctionAdd("ironpython", "IronPython");
            correctionAdd("Iron Python", "IronPython");
            correctionAdd("Ironpython", "IronPython");
            correctionAdd("Iron-Python", "IronPython");

            correctionAdd("groovy", "Groovy");

            correctionAdd("Netbeans", "NetBeans");
            correctionAdd("netbeans", "NetBeans");
            correctionAdd("net-beans", "NetBeans");
            correctionAdd("beans", "NetBeans");
            correctionAdd("NETBeans", "NetBeans");
            correctionAdd("net bean", "NetBeans"); //Not 100% correct...
            correctionAdd("NB", "NetBeans"); //Possible collision... - <https://en.wiktionary.org/wiki/NB#Interjection>
            correctionAdd("neatbeans", "NetBeans"); //Misspelling.
            correctionAdd("Netbean", "NetBeans");
            correctionAdd("Net-beans", "NetBeans");
            correctionAdd("netbean", "NetBeans");
            correctionAdd("NETBEANS", "NetBeans");

            correctionAdd("solr", "Solr");
            correctionAdd("SolR", "Solr");

            correctionAdd("noscript", "NoScript");
            correctionAdd("Noscript", "NoScript");
            correctionAdd("no script", "NoScript");

            correctionAdd("adblock", "Adblock");
            correctionAdd("AdBlock", "Adblock");

            correctionAdd("matplotlib", "Matplotlib");
            correctionAdd("MatPlotLib", "Matplotlib");

            correctionAdd("idle", "IDLE");
            correctionAdd("Idle", "IDLE");

            correctionAdd("Sharepoint", "SharePoint");
            correctionAdd("sharepoint", "SharePoint");
            correctionAdd("share point", "SharePoint");
            //correctionAdd("SP", "SharePoint"); //Collisions with stored procedure. And possibly Service Pack?
            correctionAdd("share-point", "SharePoint");
            correctionAdd("sharePoint", "SharePoint");

            correctionAdd("paypal", "PayPal");
            correctionAdd("pay pal", "PayPal");
            correctionAdd("Paypal", "PayPal");

            correctionAdd("mono", "Mono");
            correctionAdd("MONO", "Mono");

            correctionAdd("Monodevelop", "MonoDevelop");
            correctionAdd("mono develop", "MonoDevelop");
            correctionAdd("Mono Develop", "MonoDevelop");
            correctionAdd("mono develope", "MonoDevelop"); //Misspelling.
            correctionAdd("monoDevelop", "MonoDevelop");
            correctionAdd("MD", "MonoDevelop");
            correctionAdd("monodevelop", "MonoDevelop");

            correctionAdd("vmware", "VMware");
            correctionAdd("VMWare", "VMware");
            correctionAdd("VM Ware", "VMware");
            correctionAdd("Vmware", "VMware");
            correctionAdd("WMware", "VMware");
            correctionAdd("WM Ware", "VMware");
            correctionAdd("vmare", "VMware");
            correctionAdd("vmWare", "VMware");
            correctionAdd("VM ware", "VMware");
            correctionAdd("VmWare", "VMware");
            correctionAdd("VM-Ware", "VMware");
            correctionAdd("VW Ware", "VMware");
            correctionAdd("VW ware", "VMware");
            correctionAdd("VWware", "VMware");

            correctionAdd("scipy", "SciPy");
            correctionAdd("Scipy", "SciPy");
            correctionAdd("SciPY", "SciPy");

            correctionAdd("ldap", "LDAP");

            correctionAdd("Scite", "SciTE");
            correctionAdd("SciTe", "SciTE");
            correctionAdd("scite", "SciTE");
            correctionAdd("SCitE", "SciTE");
            correctionAdd("SCITE", "SciTE");

            correctionAdd("orm", "ORM");
            correctionAdd("OR/M", "ORM");
            correctionAdd("Object-Relational Mapping", "ORM");

            correctionAdd("gwt", "Google&nbsp;Web&nbsp;Toolkit"); //Expansion. Add new field with abbreviation?. In case it would be GWT.
            correctionAdd("GWT", "Google&nbsp;Web&nbsp;Toolkit");

            correctionAdd("mediawiki", "MediaWiki");
            correctionAdd("MEDIAWIKI", "MediaWiki");
            correctionAdd("Mediawiki", "MediaWiki");
            correctionAdd("Media Wiki", "MediaWiki");

            correctionAdd("excel", "Excel");
            correctionAdd("EXCEL", "Excel");
            correctionAdd("Excell", "Excel");
            correctionAdd("excell", "Excel");

            correctionAdd("wysiwyg", "WYSIWYG");

            correctionAdd("moinmoin", "MoinMoin");

            correctionAdd("joomla", "Joomla");
            correctionAdd("JOOMLA", "Joomla");

            correctionAdd("yaml", "YAML");
            correctionAdd("Yaml", "YAML");
            correctionAdd("yml", "YAML");

            correctionAdd("xaml", "XAML");
            correctionAdd("Xaml", "XAML");
            correctionAdd("xmal", "XAML");

            correctionAdd("inkscape", "Inkscape");
            correctionAdd("InkScape", "Inkscape");
            correctionAdd("Inckscape", "Inkscape");
            correctionAdd("incscape", "Inkscape");
            correctionAdd("Incscape", "Inkscape");

            correctionAdd("ndepend", "NDepend");

            correctionAdd("XmlRpc", "XML-RPC");
            correctionAdd("xmlprc", "XML-RPC");
            correctionAdd("xmlrpc", "XML-RPC");
            correctionAdd("XMLRPC", "XML-RPC");
            correctionAdd("xml-rpc", "XML-RPC");

            correctionAdd("coldfusion", "ColdFusion");
            correctionAdd("CF", "ColdFusion"); //Collision with CompactFlash.
            correctionAdd("cold fusion", "ColdFusion");
            correctionAdd("Coldfusion", "ColdFusion");

            correctionAdd("Mootools", "MooTools");
            correctionAdd("mootools", "MooTools");
            correctionAdd("Mootols", "MooTools");

            correctionAdd("gae", "Google App Engine"); //An expansion.
            correctionAdd("GAE", "Google App Engine"); //An expansion.
            correctionAdd("AppEngine", "Google App Engine");
            correctionAdd("google app engine", "Google App Engine");
            correctionAdd("google-app-engine", "Google App Engine");
            correctionAdd("Google AppEngine", "Google App Engine");
            correctionAdd("google appengine", "Google App Engine");
            correctionAdd("app engine", "Google App Engine");
            correctionAdd("Google App engine", "Google App Engine");
            correctionAdd("appengine", "Google App Engine");

            correctionAdd("google apps", "Google Apps");
            correctionAdd("Google apps", "Google Apps");

            correctionAdd("wordpress", "WordPress");
            correctionAdd("Wordpress", "WordPress");
            correctionAdd("wordperss", "WordPress");
            correctionAdd("word press", "WordPress");
            correctionAdd("WP", "WordPress"); //Not WordPerfect... Collision with Wikipedia... And Windows Phone! And weakest precondition.
            correctionAdd("wp", "WordPress"); //Not WordPerfect... Collision with Wikipedia... And Windows Phone! And weakest precondition.
            correctionAdd("wordrpress", "WordPress");
            correctionAdd("WORDPRESS", "WordPress");
            correctionAdd("Wordress", "WordPress");
            correctionAdd("WordpPress", "WordPress");
            correctionAdd("wordress", "WordPress");

            correctionAdd("ssl", "SSL");

            //Not actually observed.
            correctionAdd("webkit", "WebKit");
            correctionAdd("Webkit", "WebKit");
            correctionAdd("webKit", "WebKit");

            correctionAdd("arduino", "Arduino");
            correctionAdd("arduion", "Arduino");
            correctionAdd("arudino", "Arduino");
            correctionAdd("ARDUINO", "Arduino");
            correctionAdd("Adruino", "Arduino");
            correctionAdd("ardunio", "Arduino"); //Misspelling.
            correctionAdd("arduiino", "Arduino"); //Misspelling.
            correctionAdd("Audrino", "Arduino"); //Misspelling.
            correctionAdd("Arduoino", "Arduino"); //Misspelling.
            correctionAdd("Arduiono", "Arduino"); //Misspelling.
            correctionAdd("arduinio", "Arduino"); //Misspelling.
            correctionAdd("arudio", "Arduino");
            correctionAdd("Ardunio Uno", "Arduino"); //Misspelling.
            correctionAdd("Ardunio", "Arduino");

            correctionAdd("arduino Uno", "Arduino Uno");
            correctionAdd("arduino uno", "Arduino Uno");
            correctionAdd("Arduino UNO", "Arduino Uno");
            correctionAdd("arduino UNO", "Arduino Uno");
            correctionAdd("Arduino uno", "Arduino Uno");
            correctionAdd("UNO", "Arduino Uno");
            correctionAdd("Uno", "Arduino Uno");
            correctionAdd("uno", "Arduino Uno");
            correctionAdd("Arduino-UNO", "Arduino Uno");
            correctionAdd("aruino uno", "Arduino Uno");

            correctionAdd("prototype", "Prototype");
            correctionAdd("Prototype.js", "Prototype");
            correctionAdd("prototype.js", "Prototype");

            correctionAdd("silverlight", "Silverlight");
            correctionAdd("sliverlight", "Silverlight"); //Common misspelling...
            correctionAdd("Sliverlight", "Silverlight"); //Common misspelling...
            correctionAdd("Silverligt", "Silverlight");
            correctionAdd("Silvelight", "Silverlight");
            correctionAdd("Silver light", "Silverlight");
            correctionAdd("siverlight", "Silverlight");
            correctionAdd("SL", "Silverlight");
            correctionAdd("solverlight", "Silverlight"); //Misspelling...
            correctionAdd("SIlverlight", "Silverlight");
            correctionAdd("silver light", "Silverlight");
            correctionAdd("SilverLight", "Silverlight");

            correctionAdd("crystal report", "Crystal Reports"); //Common misspelling...
            correctionAdd("crystal reports", "Crystal Reports"); //Common misspelling...
            correctionAdd("Crystal report", "Crystal Reports"); //Common misspelling...
            correctionAdd("Crystal Report", "Crystal Reports");
            correctionAdd("CrystalReports", "Crystal Reports");
            correctionAdd("CRYSTEL REPORTs", "Crystal Reports");
            correctionAdd("Crystal reports", "Crystal Reports");
            correctionAdd("CR", "Crystal Reports"); //Possible collision with Carriage Return.
            correctionAdd("Crystal", "Crystal Reports");
            correctionAdd("crytal report", "Crystal Reports");

            correctionAdd("openvpn", "OpenVPN"); //Not actually observed.
            correctionAdd("openVPN", "OpenVPN");

            correctionAdd("mongodb", "MongoDB"); //Not actually observed. For the link.
            correctionAdd("Mongo db", "MongoDB");
            correctionAdd("Mongodb", "MongoDB");
            correctionAdd("Mongo DB", "MongoDB");
            correctionAdd("mongoDB", "MongoDB");
            correctionAdd("mongo", "MongoDB");
            correctionAdd("mongo db", "MongoDB");
            correctionAdd("Mongo", "MongoDB");
            correctionAdd("mongoDb", "MongoDB");
            correctionAdd("MongoDb", "MongoDB");
            correctionAdd("mongo-db", "MongoDB");
            correctionAdd("mongoo", "MongoDB");
            correctionAdd("mongo DB", "MongoDB");


            //Wikipedia can not handle having an entry, instead it is tugged into
            //the article about LinkedIn!!!
            //correctionAdd("voldemort", "Voldemort"); //Not actually observed. For the link.

            correctionAdd("Hbase", "HBase"); //Not actually observed. For the link.

            correctionAdd("subversion", "Subversion");
            correctionAdd("SubVersion", "Subversion");
            correctionAdd("SVN", "Subversion");
            correctionAdd("svn", "Subversion");
            correctionAdd("Svn", "Subversion");
            correctionAdd("subersion", "Subversion");
            correctionAdd("sub version", "Subversion");

            correctionAdd("cp", "The Code Project");
            correctionAdd("CP", "The Code Project");
            correctionAdd("codeproject", "The Code Project");
            correctionAdd("code project", "The Code Project");
            correctionAdd("CodeProject", "The Code Project");
            correctionAdd("the code project", "The Code Project");
            correctionAdd("Codeproject", "The Code Project");
            correctionAdd("Code Project", "The Code Project");
            correctionAdd("the coding project", "The Code Project");
            correctionAdd("coding project", "The Code Project");

            correctionAdd("csv", "CSV");

            correctionAdd("clr", "CLR");

            correctionAdd("magento", "Magento");
            correctionAdd("magneto", "Magento");

            correctionAdd("zend", "Zend Framework");
            correctionAdd("Zend", "Zend Framework");
            correctionAdd("ZF", "Zend Framework");
            correctionAdd("zend framework", "Zend Framework");
            correctionAdd("zf", "Zend Framework");
            correctionAdd("ZEND", "Zend Framework");

            correctionAdd("simplexml", "SimpleXML");

            correctionAdd("redis", "Redis");

            correctionAdd("tokyocabnit", "Tokyo Cabinet");

            correctionAdd("memcached", "Memcached");
            correctionAdd("Memcache", "Memcached");
            correctionAdd("memcache", "Memcached");
            correctionAdd("MC", "Memcached"); //Collisions?? Microcontroller is none...

            correctionAdd("fastcgi", "FastCGI");
            correctionAdd("Fastcgi", "FastCGI");

            correctionAdd("project euler", "Project Euler");
            correctionAdd("project Euler", "Project Euler");
            correctionAdd("Euler project", "Project Euler");
            correctionAdd("Euler", "Project Euler"); // Other meanings exist!
            correctionAdd("projectEuler", "Project Euler");

            correctionAdd("FireSheep", "Firesheep");

            correctionAdd("processing", "Processing");

            correctionAdd("flash", "Flash"); //Adobe's, not what is inside USB sticks.
            correctionAdd("Adobe Flash", "Flash"); //Adobe's, not what is inside USB sticks.

            correctionAdd("curl", "cURL");
            correctionAdd("CURL", "cURL");
            correctionAdd("Curl", "cURL");

            correctionAdd("Bittorrent", "BitTorrent");
            correctionAdd("bittorrent", "BitTorrent");
            correctionAdd("bit torrent", "BitTorrent");

            correctionAdd("notepad", "Notepad");
            correctionAdd("NotePad", "Notepad");
            correctionAdd("Note Pad", "Notepad");

            correctionAdd("wordpad", "WordPad");
            correctionAdd("Wordpad", "WordPad");
            correctionAdd("Word Pad", "WordPad");
            correctionAdd("worldpad", "WordPad");

            correctionAdd("sympy", "SymPy");

            correctionAdd("pig latin", "Pig Latin");
            correctionAdd("pig-latin", "Pig Latin");
            correctionAdd("Pig-latin", "Pig Latin");
            correctionAdd("Pig-Latin", "Pig Latin");
            correctionAdd("PigLatin", "Pig Latin");
            correctionAdd("piglatin", "Pig Latin");

            correctionAdd("pil", "PIL");

            correctionAdd("MacBook", "MacBook Pro"); //Not exactly...
            correctionAdd("macbook", "MacBook Pro"); //Not exactly...
            correctionAdd("Mac Book", "MacBook Pro"); //Not exactly...
            correctionAdd("mac book pro", "MacBook Pro");
            correctionAdd("Macbook Pro", "MacBook Pro");
            correctionAdd("MacBookPro", "MacBook Pro");
            correctionAdd("macbook pro", "MacBook Pro");
            correctionAdd("mac pro", "MacBook Pro");
            correctionAdd("macBook Pro", "MacBook Pro");
            correctionAdd("Macbook", "MacBook Pro"); //Not exactly...
            correctionAdd("Mac Book Pro", "MacBook Pro");
            correctionAdd("MBP", "MacBook Pro");
            correctionAdd("MacBook pro", "MacBook Pro");
            correctionAdd("Macbook pro", "MacBook Pro");

            correctionAdd("bluetooth", "Bluetooth");
            correctionAdd("BT", "Bluetooth"); //Conflict with BitTorrent
            correctionAdd("blue tooth", "Bluetooth");
            correctionAdd("blue-tooth", "Bluetooth");
            correctionAdd("blueooth", "Bluetooth"); //Misspelling...
            correctionAdd("BlueTooth", "Bluetooth");
            correctionAdd("bluethooth", "Bluetooth"); //Misspelling...
            correctionAdd("blueutooth", "Bluetooth");
            correctionAdd("Bluethooth", "Bluetooth");
            correctionAdd("bluetoot", "Bluetooth");

            correctionAdd("xbox", "Xbox");
            correctionAdd("XBox", "Xbox");

            correctionAdd("ps3", "PS3");

            correctionAdd("tcl/tk", "Tcl/Tk");

            correctionAdd("symfony", "Symfony");
            correctionAdd("Symphony", "Symfony");
            correctionAdd("symphony", "Symfony");

            correctionAdd("tomcat", "Tomcat");
            correctionAdd("tomacat", "Tomcat");
            correctionAdd("TOMCAT", "Tomcat");

            correctionAdd("openid", "OpenID");
            correctionAdd("openID", "OpenID");
            correctionAdd("openId", "OpenID");
            correctionAdd("open id", "OpenID");
            correctionAdd("OpenId", "OpenID");
            correctionAdd("open-id", "OpenID");
            correctionAdd("open ID", "OpenID");
            correctionAdd("Open ID", "OpenID");
            correctionAdd("Open Id", "OpenID");
            correctionAdd("OPENID", "OpenID");
            correctionAdd("Open-ID", "OpenID");

            correctionAdd("myopenid", "MyOpenID");
            correctionAdd("myOpenId", "MyOpenID");
            correctionAdd("myOpenID", "MyOpenID");
            correctionAdd("MyOpenId", "MyOpenID");

            correctionAdd("Oauth", "OAuth");
            correctionAdd("oauth", "OAuth");
            correctionAdd("OAUTH", "OAuth");
            correctionAdd("oAuth", "OAuth");
            correctionAdd("open auth", "OAuth");
            correctionAdd("outh", "OAuth");
            correctionAdd("Aouth", "OAuth");
            correctionAdd("aouth", "OAuth");

            correctionAdd("Github", "GitHub");
            correctionAdd("github", "GitHub");
            correctionAdd("git hub", "GitHub");
            correctionAdd("gitHub", "GitHub");
            correctionAdd("GITHUB", "GitHub");
            correctionAdd("gihub", "GitHub");
            correctionAdd("githhub", "GitHub");
            correctionAdd("Githhub", "GitHub");
            correctionAdd("gibhub", "GitHub"); //Misspelling.
            correctionAdd("gitub", "GitHub");
            correctionAdd("gh", "GitHub");
            correctionAdd("Git hub", "GitHub");
            correctionAdd("GITHub", "GitHub");
            correctionAdd("Gihub", "GitHub");
            correctionAdd("git-hub", "GitHub");
            correctionAdd("GiHub", "GitHub");

            correctionAdd("tinymce", "TinyMCE");
            correctionAdd("TinyMce", "TinyMCE");
            correctionAdd("tinyMCE", "TinyMCE");
            correctionAdd("tinyMce", "TinyMCE");
            correctionAdd("TINYmce", "TinyMCE");

            correctionAdd("blackberry", "BlackBerry");
            correctionAdd("Blackberry", "BlackBerry");
            correctionAdd("BB", "BlackBerry");
            correctionAdd("bb", "BlackBerry");

            correctionAdd("gcc", "GCC");
            correctionAdd("Gcc", "GCC");

            correctionAdd("gdb", "GDB");
            correctionAdd("Gdb", "GDB");

            correctionAdd("valgrind", "Valgrind");
            correctionAdd("valgring", "Valgrind"); //Misspelling.
            correctionAdd("vagrind", "Valgrind");
            correctionAdd("vaglrind", "Valgrind");

            correctionAdd("git", "Git");
            correctionAdd("GIT", "Git");
            correctionAdd("GiT", "Git");

            correctionAdd("Textmate", "TextMate");
            correctionAdd("textmate", "TextMate");
            correctionAdd("TM", "TextMate");
            correctionAdd("texmate", "TextMate");
            correctionAdd("Texmate", "TextMate");

            correctionAdd("ie", "Internet&nbsp;Explorer"); //Conflict with i.e. ...
            correctionAdd("IE", "Internet&nbsp;Explorer");
            correctionAdd("internet explorer", "Internet&nbsp;Explorer");
            correctionAdd("internet explore", "Internet&nbsp;Explorer");
            correctionAdd("I.E", "Internet&nbsp;Explorer");
            correctionAdd("Internet Explorer", "Internet&nbsp;Explorer");
            correctionAdd("internet-explorer", "Internet&nbsp;Explorer");
            correctionAdd("interne explorer", "Internet&nbsp;Explorer");
            correctionAdd("Internet explorer", "Internet&nbsp;Explorer");
            correctionAdd("IExplorer", "Internet&nbsp;Explorer");
            correctionAdd("Internet exlorer", "Internet&nbsp;Explorer"); //Misspelling
            correctionAdd("iexplore", "Internet&nbsp;Explorer");
            correctionAdd("iE", "Internet&nbsp;Explorer");
            correctionAdd("internert explorer", "Internet&nbsp;Explorer");
            correctionAdd("internet Explorer", "Internet&nbsp;Explorer");

            //correctionAdd("ie", ", that is"); //Not included as it conflicts with Internet Explorer.
            correctionAdd("I.e", ", that is, X"); //Specified without end full stop, to avoid the non-letter/non-digit XXX feature in this program.
            correctionAdd("i.e", ", that is, X"); //Specified without end full stop, to avoid the non-letter/non-digit XXX feature in this program.
            correctionAdd("i,e", ", that is, X");
            correctionAdd("i.,e", ", that is, X");
            correctionAdd("i.E", ", that is, X"); //Really "i.E."

            correctionAdd("IE6", "Internet&nbsp;Explorer&nbsp;6");
            correctionAdd("IE 6", "Internet&nbsp;Explorer&nbsp;6");
            correctionAdd("ie6", "Internet&nbsp;Explorer&nbsp;6");
            correctionAdd("iE6", "Internet&nbsp;Explorer&nbsp;6");
            correctionAdd("Internet Explorer 6", "Internet&nbsp;Explorer&nbsp;6");

            correctionAdd("IE7", "Internet&nbsp;Explorer&nbsp;7");
            correctionAdd("ie7", "Internet&nbsp;Explorer&nbsp;7");
            correctionAdd("IE 7", "Internet&nbsp;Explorer&nbsp;7");
            correctionAdd("Internet Explorer 7", "Internet&nbsp;Explorer&nbsp;7");
            correctionAdd("ie 7", "Internet&nbsp;Explorer&nbsp;7");
            correctionAdd("iE7", "Internet&nbsp;Explorer&nbsp;7");
            correctionAdd("Internet&nbsp;Explorer 7", "Internet&nbsp;Explorer&nbsp;7");
            correctionAdd("Ie7", "Internet&nbsp;Explorer&nbsp;7");

            correctionAdd("IE8", "Internet&nbsp;Explorer&nbsp;8");
            correctionAdd("ie8", "Internet&nbsp;Explorer&nbsp;8");
            correctionAdd("IE 8", "Internet&nbsp;Explorer&nbsp;8");
            correctionAdd("IE-8", "Internet&nbsp;Explorer&nbsp;8");
            correctionAdd("Internet Explorer 8", "Internet&nbsp;Explorer&nbsp;8"); // Effectively self
            correctionAdd("ie 8", "Internet&nbsp;Explorer&nbsp;8");

            correctionAdd("Internet Explorer 9", "Internet&nbsp;Explorer&nbsp;9");
            correctionAdd("IE9", "Internet&nbsp;Explorer&nbsp;9");
            correctionAdd("ie9", "Internet&nbsp;Explorer&nbsp;9");
            correctionAdd("IE 9", "Internet&nbsp;Explorer&nbsp;9");
            correctionAdd("ie 9", "Internet&nbsp;Explorer&nbsp;9");
            correctionAdd("Internet&nbsp;Explorer 9", "Internet&nbsp;Explorer&nbsp;9");

            correctionAdd("ie10", "Internet&nbsp;Explorer&nbsp;10");
            correctionAdd("ie 10", "Internet&nbsp;Explorer&nbsp;10");
            correctionAdd("IE 10", "Internet&nbsp;Explorer&nbsp;10");
            correctionAdd("IE10", "Internet&nbsp;Explorer&nbsp;10");
            correctionAdd("Internet&nbsp;Explorer 10", "Internet&nbsp;Explorer&nbsp;10");

            correctionAdd("ie11", "Internet&nbsp;Explorer&nbsp;11");
            correctionAdd("ie 11", "Internet&nbsp;Explorer&nbsp;11");
            correctionAdd("IE 11", "Internet&nbsp;Explorer&nbsp;11");
            correctionAdd("IE11", "Internet&nbsp;Explorer&nbsp;11");
            correctionAdd("Internet&nbsp;Explorer 11", "Internet&nbsp;Explorer&nbsp;11");
            correctionAdd("Internet Explorer 11", "Internet&nbsp;Explorer&nbsp;11"); // Effectively self (line-breaks)

            correctionAdd("html", "HTML");
            correctionAdd("Html", "HTML");
            correctionAdd("HTml", "HTML");
            correctionAdd("hrml", "HTML");
            correctionAdd("htmp", "HTML");

            correctionAdd("firefox", "Firefox");
            correctionAdd("firfox", "Firefox");
            correctionAdd("FF", "Firefox");
            correctionAdd("ff", "Firefox");
            correctionAdd("FireFox", "Firefox");
            correctionAdd("FFox", "Firefox");
            correctionAdd("FifreFox", "Firefox");
            correctionAdd("FX", "Firefox");
            correctionAdd("FIREFOX", "Firefox");
            correctionAdd("Fire Fox", "Firefox");
            correctionAdd("Fire fox", "Firefox");
            correctionAdd("firedfox", "Firefox");
            correctionAdd("Firfox", "Firefox");
            correctionAdd("Fx", "Firefox");
            correctionAdd("Firefix", "Firefox"); //A real typo...
            correctionAdd("FIRE FOX", "Firefox");
            correctionAdd("mozilla", "Firefox");
            correctionAdd("Mozilla", "Firefox");

            correctionAdd("chrome", "Google Chrome");
            correctionAdd("google-chrome", "Google Chrome");
            correctionAdd("google chrome", "Google Chrome");
            correctionAdd("Chrome", "Google Chrome");
            correctionAdd("Chr", "Google Chrome");
            correctionAdd("GoogleChrome", "Google Chrome");
            correctionAdd("crome", "Google Chrome");
            correctionAdd("google Chrome", "Google Chrome");
            correctionAdd("G-Chrome", "Google Chrome");
            correctionAdd("Google-chrome", "Google Chrome");
            correctionAdd("Chorme", "Google Chrome");
            correctionAdd("CHROME", "Google Chrome");

            correctionAdd("scheme", "Scheme");
            correctionAdd("SCHEME", "Scheme");

            correctionAdd("powerpoint", "PowerPoint");
            correctionAdd("power point", "PowerPoint");
            correctionAdd("Powerpoint", "PowerPoint");
            correctionAdd("Power Point", "PowerPoint");
            correctionAdd("power-point", "PowerPoint");
            correctionAdd("POWERPOINT", "PowerPoint");

            correctionAdd("c#", "C#");
            correctionAdd("C Sharp", "C#");
            correctionAdd("Csharp", "C#");
            correctionAdd("csharp", "C#");
            correctionAdd("CSharp", "C#");
            correctionAdd("c-sharp", "C#");
            correctionAdd("c sharp", "C#");

            correctionAdd("sql", "SQL");
            correctionAdd("Sql", "SQL");
            correctionAdd("SQl", "SQL");

            correctionAdd("http", "HTTP");
            correctionAdd("Http", "HTTP");
            correctionAdd("hhtp", "HTTP");

            correctionAdd("ascii", "ASCII");
            correctionAdd("acsii", "ASCII");
            correctionAdd("Ascii", "ASCII");
            correctionAdd("ASCI", "ASCII");
            correctionAdd("asci", "ASCII");
            correctionAdd("ASCIi", "ASCII");

            correctionAdd("fluent nhibernate", "Fluent NHibernate");
            correctionAdd("Fluentnhibernate", "Fluent NHibernate");
            correctionAdd("fluentnhibernate", "Fluent NHibernate");
            correctionAdd("fluent", "Fluent NHibernate");
            correctionAdd("FluentNhibernate", "Fluent NHibernate");

            correctionAdd("haskell", "Haskell");
            correctionAdd("Haskall", "Haskell");

            correctionAdd("playstation 3", "PlayStation 3");

            correctionAdd("Nunit", "NUnit");
            correctionAdd("nunit", "NUnit");
            correctionAdd("nUnit", "NUnit");

            correctionAdd("Junit", "JUnit");
            correctionAdd("junit", "JUnit");
            correctionAdd("JUNIT", "JUnit");
            correctionAdd("jUnit", "JUnit");


            //New name:   Jenkins
            //            http://en.wikipedia.org/wiki/Jenkins_%28software%29
            correctionAdd("hudson", "Hudson");

            correctionAdd("grails", "Grails");

            correctionAdd("moq", "Moq");

            correctionAdd("MsBuild", "MSBuild");
            correctionAdd("msbuild", "MSBuild");
            correctionAdd("ms build", "MSBuild");
            correctionAdd("MSbuild", "MSBuild");
            correctionAdd("MSBUILD", "MSBuild");
            correctionAdd("msBuild", "MSBuild");
            correctionAdd("Msbuild", "MSBuild");
            correctionAdd("MS Build", "MSBuild");
            correctionAdd("MS BUild", "MSBuild");

            correctionAdd("cocoa", "Cocoa");

            correctionAdd("clojure", "Clojure");

            correctionAdd("aquamacs", "Aquamacs");

            correctionAdd("slime", "SLIME");

            correctionAdd("django", "Django");
            correctionAdd("DJANGO", "Django");
            correctionAdd("Djanog", "Django"); //Misspelling.
            correctionAdd("DJango", "Django");

            correctionAdd("xcode", "Xcode");
            correctionAdd("XCode", "Xcode");
            correctionAdd("xCode", "Xcode");
            correctionAdd("x-code", "Xcode");
            correctionAdd("XCODE", "Xcode");

            correctionAdd("itunes", "iTunes");
            correctionAdd("Itunes", "iTunes");
            correctionAdd("itune", "iTunes");
            correctionAdd("ITunes", "iTunes");

            correctionAdd("wcf", "WCF");
            correctionAdd("Wcf", "WCF");
            correctionAdd("WCf", "WCF");

            correctionAdd("ruby", "Ruby");
            correctionAdd("RUBY", "Ruby");
            correctionAdd("rubi", "Ruby");
            correctionAdd("Rubi", "Ruby");

            correctionAdd("RoR", "Ruby on Rails");
            correctionAdd("ROR", "Ruby on Rails");
            correctionAdd("Rails", "Ruby on Rails");
            correctionAdd("rails", "Ruby on Rails");
            correctionAdd("ruby on rails", "Ruby on Rails");
            correctionAdd("Ruby On Rails", "Ruby on Rails");
            correctionAdd("Ruby on rails", "Ruby on Rails");
            correctionAdd("ruby-on-rails", "Ruby on Rails");
            correctionAdd("RubyOnRails", "Ruby on Rails");
            correctionAdd("Ruby-on-Rails", "Ruby on Rails");
            correctionAdd("Ruby-on-rails", "Ruby on Rails");
            correctionAdd("ruby over rails", "Ruby on Rails");
            correctionAdd("RubyonRails", "Ruby on Rails");
            correctionAdd("ruby/rails", "Ruby on Rails");
            correctionAdd("On Rails", "Ruby on Rails");
            correctionAdd("ror", "Ruby on Rails");
            correctionAdd("Ror", "Ruby on Rails");

            correctionAdd("gem", "RubyGems");
            correctionAdd("rubygem", "RubyGems");

            correctionAdd("jruby", "JRuby");

            correctionAdd("unicode", "Unicode");
            correctionAdd("UNICODE", "Unicode");
            correctionAdd("UniCode", "Unicode");

            //correctionAdd("spring", "Spring"); //Not used anymore
            correctionAdd("SPRING", "Spring Framework");
            correctionAdd("spring", "Spring Framework");
            correctionAdd("Spring", "Spring Framework");
            correctionAdd("springs", "Spring Framework");
            correctionAdd("spring framework", "Spring Framework");

            correctionAdd("Spring.Net", "Spring.NET");

            correctionAdd("spring mvc", "Spring MVC");
            correctionAdd("springmvc", "Spring MVC");
            correctionAdd("springMVC", "Spring MVC");

            correctionAdd("JBOSS", "JBoss");
            correctionAdd("jboss", "JBoss");
            correctionAdd("Jboss", "JBoss");

            correctionAdd("paint.net", "Paint.NET");
            correctionAdd("Paint.Net", "Paint.NET");
            correctionAdd("Paint.net", "Paint.NET");
            correctionAdd("Paint .Net", "Paint.NET");
            correctionAdd("paint .net", "Paint.NET");

            correctionAdd("regex", "regular expression");
            correctionAdd("Regex", "regular expression");
            correctionAdd("regexp", "regular expression");
            correctionAdd("Regexp", "regular expression");
            correctionAdd("regx", "regular expression");
            correctionAdd("REGEX", "regular expression");
            correctionAdd("regular Expression", "regular expression");
            correctionAdd("Regular experssion", "regular expression");
            correctionAdd("Regular expression", "regular expression");
            correctionAdd("Regular Expression", "regular expression");
            correctionAdd("RegEx", "regular expression");
            correctionAdd("regexes", "regular expression"); //Not 100% correct - plural.
            correctionAdd("reg-exp", "regular expression");
            correctionAdd("Regular Expressions", "regular expression"); //Not 100% correct - plural.
            correctionAdd("Regexes", "regular expression"); //Not 100% correct - plural.
            correctionAdd("RegExp", "regular expression");
            correctionAdd("RE", "regular expression");
            correctionAdd("reqular expression", "regular expression");
            correctionAdd("re", "regular expression");
            correctionAdd("regular exp", "regular expression");
            correctionAdd("regular expressions", "regular expression"); //Not 100% correct - plural.
            correctionAdd("RegE", "regular expression");
            correctionAdd("regEx", "regular expression");
            correctionAdd("regexpr", "regular expression");
            correctionAdd("RegularExpression", "regular expression");
            correctionAdd("regular express", "regular expression");

            correctionAdd("xpath", "XPath");
            correctionAdd("Xpath", "XPath");
            correctionAdd("XPATH", "XPath");

            correctionAdd("repl", "REPL");

            correctionAdd("Vtune", "VTune");

            correctionAdd("oprofile", "OProfile");
            correctionAdd("Oprofile", "OProfile");

            correctionAdd("pspad", "PSPad");

            correctionAdd("Textpad", "TextPad");
            correctionAdd("textpad", "TextPad");

            correctionAdd("e-text editor", "E Text Editor");
            correctionAdd("E", "E Text Editor"); //Not specific enough??
            correctionAdd("E-Texteditor", "E Text Editor");
            correctionAdd("e-texteditor", "E Text Editor");
            correctionAdd("e texteditor", "E Text Editor");
            correctionAdd("E-Text Editor", "E Text Editor");

            correctionAdd("WINE", "Wine");
            correctionAdd("wine", "Wine");

            correctionAdd("komodo", "Komodo Edit");
            correctionAdd("KomodoEdit", "Komodo Edit");

            correctionAdd("wxwidgets", "wxWidgets");
            correctionAdd("WxWidgets", "wxWidgets");
            correctionAdd("Wx", "wxWidgets");
            correctionAdd("wx widgets", "wxWidgets");
            correctionAdd("Wx widgets", "wxWidgets");
            correctionAdd("Wxwidgets", "wxWidgets");

            correctionAdd("p/invoke", "P/Invoke");
            correctionAdd("pinvoke", "P/Invoke");
            correctionAdd("PInvoke", "P/Invoke");
            correctionAdd("p-invoke", "P/Invoke");
            correctionAdd("pInvoke", "P/Invoke");
            correctionAdd("p/Invoke", "P/Invoke");
            correctionAdd("PInvonke", "P/Invoke"); //Misspelling.
            correctionAdd("P/invoke", "P/Invoke");
            correctionAdd("P/INVOKE", "P/Invoke");
            correctionAdd("Pinvoke", "P/Invoke");

            correctionAdd("jni", "JNI");

            correctionAdd("Cruisecontrol", "CruiseControl");
            correctionAdd("cruisecontrol", "CruiseControl");
            correctionAdd("cruise control", "CruiseControl");
            correctionAdd("Cruise Control", "CruiseControl");
            correctionAdd("crruise", "CruiseControl");
            correctionAdd("cruse control", "CruiseControl");
            correctionAdd("CC", "CruiseControl"); //Collisions? E.g. "carbon copy"?

            correctionAdd("CruiseControl.net", "CruiseControl.NET");
            correctionAdd("CruiseControl.Net", "CruiseControl.NET");
            correctionAdd("cruisecontrol.net", "CruiseControl.NET");
            correctionAdd("cruiseontrol.net", "CruiseControl.NET");
            correctionAdd("CCNet", "CruiseControl.NET");
            correctionAdd("CC.Net", "CruiseControl.NET");
            correctionAdd("ccnet", "CruiseControl.NET");
            correctionAdd("cruise control.net", "CruiseControl.NET");
            correctionAdd("Cruise Control.NET", "CruiseControl.NET");
            correctionAdd("CC.NET", "CruiseControl.NET");
            correctionAdd("cruise control.NET", "CruiseControl.NET");
            correctionAdd("Cruise-Control.Net", "CruiseControl.NET");
            correctionAdd("Cruisecontrol.net", "CruiseControl.NET");
            correctionAdd("CCNET", "CruiseControl.NET");

            correctionAdd("jdbc", "JDBC");

            correctionAdd("war", "WAR");

            correctionAdd("h2", "H2");

            correctionAdd("yum", "YUM");
            correctionAdd("Yum", "YUM");

            correctionAdd("Aptitude", "aptitude");

            correctionAdd("redmine", "Redmine");

            correctionAdd("gitorious", "Gitorious");

            correctionAdd("vps", "VPS");
            correctionAdd("Virtual Private Server", "VPS");
            correctionAdd("virtual private server", "VPS");
            correctionAdd("Vps", "VPS");

            correctionAdd("google", "Google Search");
            correctionAdd("Google", "Google Search");
            correctionAdd("google search", "Google Search");
            correctionAdd("Google search", "Google Search");

            correctionAdd("yahoo", "Yahoo Search"); //Really "Yahoo!", but we don't like special characters in names.
            correctionAdd("Yahoo", "Yahoo Search"); //Really "Yahoo!", but we don't like special characters in names.

            correctionAdd("FaceBook", "Facebook");
            correctionAdd("facebook", "Facebook");
            correctionAdd("FB", "Facebook"); //Conflict with FB for Firebug (but less frequent).
            correctionAdd("fb", "Facebook"); //Conflict with FB for Firebug (but less frequent).
            correctionAdd("Fb", "Facebook"); //Conflict with FB for Firebug (but less frequent).
            correctionAdd("FACEBOOK", "Facebook");
            correctionAdd("facebk", "Facebook");
            correctionAdd("FAcebook", "Facebook");
            correctionAdd("face book", "Facebook");
            correctionAdd("Faceboook", "Facebook"); //Misspelling.
            correctionAdd("Fecebook", "Facebook"); // !!!!
            correctionAdd("faceboook", "Facebook"); //Misspelling.
            correctionAdd("face-book", "Facebook");
            correctionAdd("Facbook", "Facebook");

            correctionAdd("flickr", "Flickr");

            correctionAdd("ejb", "EJB");

            correctionAdd("dojo", "Dojo Toolkit");
            correctionAdd("Dojo", "Dojo Toolkit");
            correctionAdd("DojoToolkit", "Dojo Toolkit");
            correctionAdd("DOJO", "Dojo Toolkit");

            correctionAdd("IB", "Interface Builder");
            correctionAdd("interface builder", "Interface Builder");
            correctionAdd("Interface builder", "Interface Builder");
            correctionAdd("xib", "Interface Builder");

            correctionAdd("ibatis", "iBATIS");

            correctionAdd("WireShark", "Wireshark");
            correctionAdd("wireshark", "Wireshark");
            correctionAdd("Whireshark", "Wireshark");


            correctionAdd("Monotouch", "MonoTouch");
            correctionAdd("Mono Touch", "MonoTouch");
            correctionAdd("monotouch", "MonoTouch");

            correctionAdd("Sql Server Management Studio", "SQL Server Management Studio");
            correctionAdd("MS SQL Server Management Studio", "SQL Server Management Studio");
            correctionAdd("sql mgmt studio", "SQL Server Management Studio");
            correctionAdd("SQL management studio", "SQL Server Management Studio");
            correctionAdd("SQL Server Mgmt Studio", "SQL Server Management Studio");
            correctionAdd("sql server management studios", "SQL Server Management Studio");
            correctionAdd("SQL Management Studio", "SQL Server Management Studio");
            correctionAdd("Sql Server Management studio", "SQL Server Management Studio");
            correctionAdd("SSMS", "SQL Server Management Studio");
            correctionAdd("Sql Server management studio", "SQL Server Management Studio");
            correctionAdd("sql server management studio", "SQL Server Management Studio");
            correctionAdd("management studio", "SQL Server Management Studio");
            correctionAdd("SQLServer Management and Studio", "SQL Server Management Studio");
            correctionAdd("SqlServermanagment studio", "SQL Server Management Studio");
            correctionAdd("Management Studio", "SQL Server Management Studio");
            correctionAdd("SQL Server Management studio", "SQL Server Management Studio");
            correctionAdd("SQl Server Management Studio", "SQL Server Management Studio");
            correctionAdd("MSSQL Management Studio", "SQL Server Management Studio");
            correctionAdd("SQL&nbsp;Server Management Studio", "SQL Server Management Studio");
            correctionAdd("sql management studio", "SQL Server Management Studio");
            correctionAdd("SQL Server Management Tools", "SQL Server Management Studio");

            correctionAdd("Vsto", "VSTO");

            correctionAdd("mime", "MIME");
            correctionAdd("Mime", "MIME");

            correctionAdd("url", "URL");
            correctionAdd("Url", "URL");
            correctionAdd("URl", "URL");

            correctionAdd("mercurial", "Mercurial");
            correctionAdd("hg", "Mercurial");
            correctionAdd("Hg", "Mercurial");
            correctionAdd("HG", "Mercurial");
            correctionAdd("mecurial", "Mercurial"); //Misspelling.
            correctionAdd("Mecurial", "Mercurial"); //Misspelling.

            correctionAdd("bitkeeper", "BitKeeper");

            correctionAdd("scsi", "SCSI");

            correctionAdd("wav", "WAV");

            correctionAdd("cpython", "CPython");

            correctionAdd("mp3", "MP3");

            correctionAdd("ogg", "Ogg");

            correctionAdd("PIP", "pip"); //Not actually observed. The Python thing.

            correctionAdd("Virtualenv", "virtualenv"); //Not actually observed.

            correctionAdd("Atlas", "ATLAS"); //Not actually observed.

            correctionAdd("r", "R"); //Not actually observed.

            correctionAdd("GEdit", "gedit");

            correctionAdd("linux", "Linux");
            correctionAdd("LINUX", "Linux");

            correctionAdd("unix", "Unix");
            correctionAdd("UNIX", "Unix");
            correctionAdd("UNix", "Unix");

            correctionAdd("wmv", "WMV");

            correctionAdd("mp4", "MP4");

            correctionAdd("h264", "H.264");
            correctionAdd("h.264", "H.264");
            correctionAdd("H264", "H.264");

            correctionAdd("aac", "AAC");

            correctionAdd("SO", "Stack&nbsp;Overflow");
            correctionAdd("S.O", "Stack&nbsp;Overflow");
            correctionAdd("StacOkverflow", "Stack&nbsp;Overflow");
            correctionAdd("StackOF", "Stack&nbsp;Overflow");
            correctionAdd("stackoverflow", "Stack&nbsp;Overflow");
            correctionAdd("Stackoverflow", "Stack&nbsp;Overflow");
            correctionAdd("stack-overflow", "Stack&nbsp;Overflow");
            correctionAdd("stack overflow", "Stack&nbsp;Overflow");
            correctionAdd("Stackover flow", "Stack&nbsp;Overflow");
            correctionAdd("StackOverflow", "Stack&nbsp;Overflow");
            correctionAdd("StackOverFlow", "Stack&nbsp;Overflow");
            correctionAdd("Stack overflow", "Stack&nbsp;Overflow");
            correctionAdd("Stack Overflow", "Stack&nbsp;Overflow");
            correctionAdd("stackovwerflow", "Stack&nbsp;Overflow");
            correctionAdd("stackoverfull", "Stack&nbsp;Overflow");
            correctionAdd("stackoverflow.SE", "Stack&nbsp;Overflow");
            correctionAdd("StackOveflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stackoveflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("Stackvoverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("StackOFlow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("Stackflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stackflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stack over flow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("StackedOverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("StackOVerflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stackoverlow", "Stack&nbsp;Overflow");
            correctionAdd("StackOverlow", "Stack&nbsp;Overflow");
            correctionAdd("SatckOverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("Syackoverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("Stacoverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stackOverflow", "Stack&nbsp;Overflow");
            correctionAdd("StackOerflow", "Stack&nbsp;Overflow");
            correctionAdd("Stack Oversflow", "Stack&nbsp;Overflow");
            correctionAdd("Stakcoverflow", "Stack&nbsp;Overflow");
            correctionAdd("Stack OverfLow", "Stack&nbsp;Overflow");
            correctionAdd("StackOverfLow", "Stack&nbsp;Overflow");
            correctionAdd("stackover flow", "Stack&nbsp;Overflow");
            correctionAdd("statckoverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("StackOverlfow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("Stalkoverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stackOverFlow", "Stack&nbsp;Overflow");
            correctionAdd("Stack OverFlow", "Stack&nbsp;Overflow");
            correctionAdd("Stack over flow", "Stack&nbsp;Overflow");
            correctionAdd("STACKOVERFLOW", "Stack&nbsp;Overflow");
            correctionAdd("Stalckoverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stack Overfolw", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("Stack-overflow", "Stack&nbsp;Overflow");
            correctionAdd("stafkoverflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stackvoerflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("stackoverlfow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("Stack Oveflow", "Stack&nbsp;Overflow"); //Misspelling.
            correctionAdd("StackoverFlow", "Stack&nbsp;Overflow");
            correctionAdd("StckOverflow", "Stack&nbsp;Overflow");
            correctionAdd("stackverflow", "Stack&nbsp;Overflow");
            correctionAdd("StackOwerflow", "Stack&nbsp;Overflow");
            correctionAdd("Stack Overlfow", "Stack&nbsp;Overflow");
            correctionAdd("Stack-Overflow", "Stack&nbsp;Overflow");
            correctionAdd("Stacked Overflow", "Stack&nbsp;Overflow");
            correctionAdd("satckoverflow", "Stack&nbsp;Overflow");
            correctionAdd("Stack OVERFLOW", "Stack&nbsp;Overflow");
            correctionAdd("S/O", "Stack&nbsp;Overflow");
            correctionAdd("Stack Ooverflow", "Stack&nbsp;Overflow");
            correctionAdd("Stack Owerflow", "Stack&nbsp;Overflow");
            correctionAdd("StrackOverflow", "Stack&nbsp;Overflow");
            correctionAdd("Stak Overflow", "Stack&nbsp;Overflow");
            correctionAdd("stackoverfow", "Stack&nbsp;Overflow");
            correctionAdd("Stackeoverflow", "Stack&nbsp;Overflow");
            correctionAdd("Stackoveflow", "Stack&nbsp;Overflow");
            correctionAdd("stackoverflopw", "Stack&nbsp;Overflow");
            correctionAdd("Stack Overlow", "Stack&nbsp;Overflow");

            correctionAdd("SE", "Stack&nbsp;Exchange");
            correctionAdd("stackexchange", "Stack&nbsp;Exchange");
            correctionAdd("stack exchange", "Stack&nbsp;Exchange");
            correctionAdd("stackExchange", "Stack&nbsp;Exchange");
            correctionAdd("StackExchange", "Stack&nbsp;Exchange");
            correctionAdd("Stack Exchange", "Stack&nbsp;Exchange");
            correctionAdd("stack-exchange", "Stack&nbsp;Exchange");
            correctionAdd("Stackexchange", "Stack&nbsp;Exchange");
            correctionAdd("StackExhange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("Stack exhange", "Stack&nbsp;Exchange");
            correctionAdd("Stackexchnage", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("StackExchnage", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("StackExchcange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("Stack Exchcange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("StackExhcange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("stackexchage", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("StackeExchange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("StackEx", "Stack&nbsp;Exchange");
            correctionAdd("StackExghange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("Stack", "Stack&nbsp;Exchange");
            correctionAdd("Stachexchange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("Stack Exhange", "Stack&nbsp;Exchange");
            correctionAdd("stack exhange", "Stack&nbsp;Exchange");
            correctionAdd("StackEchange", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("stackExchnage", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("stack", "Stack&nbsp;Exchange"); //Could also be Stack Overflow.
            correctionAdd("stack Exchange", "Stack&nbsp;Exchange");
            correctionAdd("S.E", "Stack&nbsp;Exchange");
            correctionAdd("Stack Ex", "Stack&nbsp;Exchange");
            correctionAdd("Stack exchange", "Stack&nbsp;Exchange");
            correctionAdd("SX", "Stack&nbsp;Exchange");
            correctionAdd("Stack EXCHANGE", "Stack&nbsp;Exchange");
            correctionAdd("StackExchage", "Stack&nbsp;Exchange");
            correctionAdd("Stack excahnge", "Stack&nbsp;Exchange");
            correctionAdd("Stack Excahnge", "Stack&nbsp;Exchange");
            correctionAdd("stack excahnge", "Stack&nbsp;Exchange");
            correctionAdd("Stackexchage", "Stack&nbsp;Exchange"); //Misspelling.
            correctionAdd("stack exhcange", "Stack&nbsp;Exchange");
            correctionAdd("Stackexcange", "Stack&nbsp;Exchange");

            correctionAdd("serverfault", "Server&nbsp;Fault");
            correctionAdd("SF", "Server&nbsp;Fault");
            correctionAdd("ServerFault", "Server&nbsp;Fault");
            correctionAdd("Serverfault", "Server&nbsp;Fault");
            correctionAdd("Server Fault", "Server&nbsp;Fault");
            correctionAdd("server fault", "Server&nbsp;Fault");
            correctionAdd("sever fault", "Server&nbsp;Fault"); //Misspelling.
            correctionAdd("Server fault", "Server&nbsp;Fault");
            correctionAdd("sf", "Server&nbsp;Fault");

            correctionAdd("CS", "computer science");
            correctionAdd("compsci", "computer science");
            correctionAdd("CompSci", "computer science");
            correctionAdd("Comp-Sci", "computer science");
            correctionAdd("Comp. Sci", "computer science");
            correctionAdd("C.S", "computer science"); //Should really be C.S., but we are hampered by a bug...
            correctionAdd("comp sci", "computer science");
            correctionAdd("cosci", "computer science");
            correctionAdd("Computer Science", "computer science");
            correctionAdd("cs", "computer science");
            correctionAdd("Comp Sci", "computer science");
            correctionAdd("c.s", "computer science"); //Should really be c.s., but we are hampered by a bug...
            correctionAdd("Cse", "computer science");
            correctionAdd("Computer Sc", "computer science");
            correctionAdd("Computer science", "computer science");

            correctionAdd("Virtualbox", "VirtualBox");
            correctionAdd("virtualbox", "VirtualBox");
            correctionAdd("virtual box", "VirtualBox");
            correctionAdd("Virtual Box", "VirtualBox");
            correctionAdd("VirtulBox", "VirtualBox");
            correctionAdd("Virtual box", "VirtualBox");
            correctionAdd("VBox", "VirtualBox");
            correctionAdd("VirualBox", "VirtualBox");
            correctionAdd("vbox", "VirtualBox");

            correctionAdd("VirualPC", "Virual PC");

            correctionAdd("pcre", "PCRE");

            correctionAdd("zen cart", "Zen Cart");
            correctionAdd("ZenCart", "Zen Cart");
            correctionAdd("zenCart", "Zen Cart");

            correctionAdd("jre", "JRE");
            correctionAdd("Java Runtime Environment", "JRE");
            correctionAdd("Jre", "JRE");

            correctionAdd("SAFARI", "Safari");
            correctionAdd("safari", "Safari");

            correctionAdd("opera", "Opera");
            correctionAdd("OPERA", "Opera");

            correctionAdd("smtp", "SMTP");
            correctionAdd("Smtp", "SMTP");

            //Funny alternative URL: <http://d-e-f-i-n-i-t-e-l-y.com/>
            correctionAdd("definately", "definitely");
            correctionAdd("definetely", "definitely");
            correctionAdd("definatly", "definitely");
            correctionAdd("Definitly", "definitely");
            correctionAdd("definitly", "definitely");
            correctionAdd("definitetly", "definitely");
            correctionAdd("definiately", "definitely");
            correctionAdd("definetly", "definitely");
            correctionAdd("denitely", "definitely");
            correctionAdd("definively", "definitely");
            correctionAdd("deffinetely", "definitely");
            correctionAdd("definetily", "definitely");
            correctionAdd("defiantly", "definitely");
            correctionAdd("deffo", "definitely");  //Slang term
            correctionAdd("defininetly", "definitely");
            correctionAdd("defenitely", "definitely");
            correctionAdd("defenetly", "definitely");
            correctionAdd("deffinetly", "definitely");
            correctionAdd("defo", "definitely");
            correctionAdd("definatily", "definitely");
            correctionAdd("definitelly", "definitely");
            correctionAdd("diffenately", "definitely");
            correctionAdd("definitley", "definitely");
            correctionAdd("definently", "definitely");
            correctionAdd("Definatly", "definitely");

            correctionAdd("nagios", "Nagios");

            correctionAdd("usb", "USB");
            correctionAdd("Usb", "USB");

            correctionAdd("Jqgrid", "jqGrid");
            correctionAdd("JQGrid", "jqGrid");
            correctionAdd("jqgrid", "jqGrid");
            correctionAdd("jQgrid", "jqGrid");
            correctionAdd("JqGrid", "jqGrid");
            correctionAdd("JQgrid", "jqGrid");
            correctionAdd("jQGrid", "jqGrid");
            correctionAdd("JgGrid", "jqGrid");

            correctionAdd("nat", "NAT");
            correctionAdd("Network Address Translated", "NAT");

            correctionAdd("xmpp", "XMPP");

            correctionAdd("imho", "IMHO");
            correctionAdd("Imho", "IMHO");

            correctionAdd("imo", "IMO");
            correctionAdd("Imo", "IMO");

            correctionAdd("rmi", "RMI");

            correctionAdd("opensocial", "OpenSocial");

            correctionAdd("hdd", "hard disk drive");
            correctionAdd("HDD", "hard disk drive"); //An expansion.
            correctionAdd("HD", "hard disk drive"); //An expansion.
            correctionAdd("harddrive", "hard disk drive"); //An expansion.
            correctionAdd("hd", "hard disk drive"); //An expansion.
            correctionAdd("hard drive", "hard disk drive"); //An expansion.
            correctionAdd("hard disk", "hard disk drive"); //An expansion.
            correctionAdd("Hard Disk", "hard disk drive");
            correctionAdd("Hard Drive", "hard disk drive");
            correctionAdd("Harddrive", "hard disk drive");
            correctionAdd("hard disc drive", "hard disk drive");
            correctionAdd("hard disc", "hard disk drive");
            correctionAdd("harddisk", "hard disk drive");
            correctionAdd("hard-drive", "hard disk drive");
            correctionAdd("Hard Disk Drive", "hard disk drive");
            correctionAdd("hard-disk", "hard disk drive");

            correctionAdd("ssd", "SSD");
            correctionAdd("Solid State Disk", "SSD");
            correctionAdd("SDD", "SSD");

            correctionAdd("i18n", "internationalisation and localisation");
            correctionAdd("I18n", "internationalisation and localisation");
            correctionAdd("I18N", "internationalisation and localisation");

            correctionAdd("Wamp", "WAMP");
            correctionAdd("wamp", "WAMP");

            correctionAdd("godaddy", "GoDaddy");
            correctionAdd("goaddy", "GoDaddy");
            correctionAdd("go daddy", "GoDaddy");
            correctionAdd("Godaddy", "GoDaddy");
            correctionAdd("go - daddy", "GoDaddy");
            correctionAdd("goDaddy", "GoDaddy");
            correctionAdd("Go Daddy", "GoDaddy");

            correctionAdd("di", "dependency injection"); //Not actually observed.
            correctionAdd("DI", "dependency injection"); //An expansion.
            correctionAdd("Dependency Injection", "dependency injection"); //An expansion.
            correctionAdd("depedency inejction", "dependency injection");
            correctionAdd("dependency inejction", "dependency injection");
            correctionAdd("depedency injection", "dependency injection");
            correctionAdd("Dependancy Injection", "dependency injection");
            correctionAdd("dependancy injection", "dependency injection");

            correctionAdd("seo", "SEO");

            correctionAdd("md5", "MD5");
            correctionAdd("md 5", "MD5");

            correctionAdd("xslt", "XSLT");
            correctionAdd("Xslt", "XSLT");

            correctionAdd("innodb", "InnoDB");
            correctionAdd("innoDB", "InnoDB");

            correctionAdd("myisam", "MyISAM");
            correctionAdd("MyIsam", "MyISAM");

            correctionAdd("html5", "HTML5"); //Not actually observed.
            correctionAdd("Html5", "HTML5");
            correctionAdd("HTML 5", "HTML5");
            correctionAdd("Html 5", "HTML5");
            correctionAdd("html 5", "HTML5");

            correctionAdd("confluence", "Confluence");

            correctionAdd("scrum", "Scrum");

            correctionAdd("TFS", "Team Foundation Server"); //Expansion.
            correctionAdd("tfs", "Team Foundation Server"); //Not actually observed.
            correctionAdd("Tfs", "Team Foundation Server");
            correctionAdd("TF", "Team Foundation Server");

            correctionAdd("mingw", "MinGW");
            correctionAdd("minGW", "MinGW");
            correctionAdd("MingW", "MinGW");
            correctionAdd("Mingw", "MinGW");
            correctionAdd("MINGW32", "MinGW");
            correctionAdd("MINGW", "MinGW");
            correctionAdd("MingGW", "MinGW");
            correctionAdd("MInGW", "MinGW");

            correctionAdd("CodeBlocks", "Code::Blocks");
            correctionAdd("Code Blocks", "Code::Blocks");
            correctionAdd("Code::blocks", "Code::Blocks");
            correctionAdd("codeblocks", "Code::Blocks");
            correctionAdd("Codeblocks", "Code::Blocks");
            correctionAdd("codeblock", "Code::Blocks");
            correctionAdd("code::block", "Code::Blocks");
            correctionAdd("code blocks", "Code::Blocks");

            correctionAdd("Airport", "AirPort");
            correctionAdd("airport", "AirPort");

            correctionAdd("adsl", "ADSL");

            correctionAdd("legit", "legitimate"); //Expansion, not case correction.
            correctionAdd("legimate", "legitimate");

            correctionAdd("px", "pixels"); //Expansion, not case correction.

            correctionAdd("w3", "W3C");
            correctionAdd("w3c", "W3C");
            correctionAdd("W3c", "W3C");

            correctionAdd("afaik", "as far as I know");
            correctionAdd("Afaik", "as far as I know");
            correctionAdd("AFAIK", "as far as I know");

            correctionAdd("dao", "DAO");
            correctionAdd("Dao", "DAO");

            correctionAdd("erlang", "Erlang");

            correctionAdd("sdk", "SDK");
            correctionAdd("SDk", "SDK");
            correctionAdd("Sdk", "SDK");

            correctionAdd("centos", "CentOS");
            correctionAdd("Centos", "CentOS");
            correctionAdd("Cent OS", "CentOS");
            correctionAdd("centOS", "CentOS");
            correctionAdd("CENTOS", "CentOS");
            correctionAdd("cent-os", "CentOS");
            correctionAdd("CentOs", "CentOS");

            correctionAdd("OpenWRT", "OpenWrt");
            correctionAdd("openwrt", "OpenWrt");
            correctionAdd("openWRT", "OpenWrt");

            correctionAdd("dd-wrt", "DD-WRT");
            correctionAdd("ddwrt", "DD-WRT");

            correctionAdd("jpa", "JPA"); //Not actually observed.

            correctionAdd("gac", "GAC"); //Not actually observed.
            correctionAdd("Global Assembly Cache", "GAC");
            correctionAdd("global assembly cache", "GAC");

            correctionAdd("dropbox", "Dropbox");
            correctionAdd("droppbox", "Dropbox"); //Misspelling...
            correctionAdd("DropBox", "Dropbox");
            correctionAdd("drop box", "Dropbox");
            correctionAdd("Drop box", "Dropbox");
            correctionAdd("DROPBOX", "Dropbox");

            correctionAdd("activex", "ActiveX");
            correctionAdd("Active-X", "ActiveX");
            correctionAdd("activeX", "ActiveX");
            correctionAdd("activx", "ActiveX");
            correctionAdd("Ativex", "ActiveX");
            correctionAdd("Active X", "ActiveX");
            correctionAdd("Activex", "ActiveX");
            correctionAdd("active X", "ActiveX");

            correctionAdd("wicket", "Wicket"); //Not actually observed.

            correctionAdd("jar", "JAR"); //Really "JAR file".
            correctionAdd("Jar", "JAR"); //Really "JAR file".

            correctionAdd("dom", "DOM");
            correctionAdd("Dom", "DOM");

            correctionAdd("latex", "LaTeX");
            correctionAdd("Latex", "LaTeX");
            correctionAdd("LATEX", "LaTeX");
            correctionAdd("LaTex", "LaTeX");
            correctionAdd("LateX", "LaTeX");

            correctionAdd("suse", "SUSE&nbsp;Linux");
            correctionAdd("SuSE", "SUSE&nbsp;Linux");
            correctionAdd("Suse", "SUSE&nbsp;Linux");
            correctionAdd("SUSE Linux", "SUSE&nbsp;Linux"); // Effectively self

            correctionAdd("Opensuse", "openSUSE");
            correctionAdd("opensuse", "openSUSE");
            correctionAdd("OpenSUSE", "openSUSE");
            correctionAdd("OpenSuse", "openSUSE");
            correctionAdd("openSuse", "openSUSE");
            correctionAdd("OpenSuSE", "openSUSE");
            correctionAdd("Open Suse", "openSUSE");

            correctionAdd("seperate", "separate");
            correctionAdd("separete", "separate");
            correctionAdd("Saperate", "separate");
            correctionAdd("saperate", "separate");
            correctionAdd("seprate", "separate");
            correctionAdd("separte", "separate");
            correctionAdd("deparate", "separate");
            correctionAdd("sepereate", "separate");
            correctionAdd("Separete", "separate");
            correctionAdd("Separate", "separate");

            correctionAdd("seperation", "separation");
            correctionAdd("speraration", "separation");

            correctionAdd("cpu", "CPU");
            correctionAdd("Cpu", "CPU");

            correctionAdd("sysadmin", "system administrator"); //Expansion.
            correctionAdd("Sysadmin", "system administrator");
            correctionAdd("SysAdmin", "system administrator");
            correctionAdd("sys admin", "system administrator");
            correctionAdd("SysAdmins", "system administrator"); //Not 100% correct - plural.
            correctionAdd("admins", "system administrator"); //Not 100% correct - plural.
            correctionAdd("admin", "system administrator"); //Not 100% correct.
            correctionAdd("Admin", "system administrator"); //Not 100% correct.
            correctionAdd("sysadm", "system administrator");
            correctionAdd("Sysadmins", "system administrator"); //Not 100% correct - plural.
            correctionAdd("system admin", "system administrator");
            correctionAdd("Systems Admin", "system administrator");
            correctionAdd("adim", "system administrator");
            correctionAdd("sys-admin", "system administrator");
            correctionAdd("administartor", "system administrator");
            correctionAdd("adminstrator", "system administrator");
            correctionAdd("adminsitrator", "system administrator");
            correctionAdd("adminstrators", "system administrator"); //Not 100% correct - plural.
            correctionAdd("adminitrator", "system administrator");

            correctionAdd("APP Domains", "application domain");
            correctionAdd("APP Domain", "application domain");
            correctionAdd("APP domain", "application domain");
            correctionAdd("App Domains", "application domain");
            correctionAdd("App Domain", "application domain");
            correctionAdd("AppDomains", "application domain");
            correctionAdd("AppDomain", "application domain");
            correctionAdd("app domain", "application domain");
            correctionAdd("appDomain", "application domain");
            correctionAdd("appdomain", "application domain");
            correctionAdd("Application Domain", "application domain");
            correctionAdd("App-domains", "application domain");

            correctionAdd("ui", "user interface"); //Expansion.
            correctionAdd("UI", "user interface"); //Expansion.
            correctionAdd("Ui", "user interface"); //Expansion.

            correctionAdd("xml", "XML");
            correctionAdd("Xml", "XML");
            correctionAdd("XMl", "XML");
            correctionAdd("xML", "XML");

            correctionAdd("wget", "Wget");
            correctionAdd("WGET", "Wget");

            correctionAdd("google analytics", "Google Analytics");
            correctionAdd("GA", "Google Analytics");

            correctionAdd("Gtalk", "Google Talk");
            correctionAdd("gtalk", "Google Talk");
            correctionAdd("gTalk", "Google Talk");
            correctionAdd("GTalk", "Google Talk");

            correctionAdd("Distro", "distribution"); //Expansion.
            correctionAdd("distro", "distribution"); //Expansion.
            correctionAdd("distros", "distribution"); //Expansion, not 100% correct. Add a plural feature?
            correctionAdd("distrobution", "distribution");
            correctionAdd("distos", "distribution"); //Misspelling. Expansion.
            correctionAdd("ditro", "distribution");
            correctionAdd("distor", "distribution"); //Misspelling. Expansion.
            correctionAdd("distors", "distribution"); //Expansion, not 100% correct. Add a plural feature?

            correctionAdd("soap", "SOAP");
            correctionAdd("Soap", "SOAP");
            correctionAdd("SAOP", "SOAP");

            //correctionAdd("nginx", "nginx");
            correctionAdd("Nginx", "nginx");
            correctionAdd("NGINX", "nginx");
            correctionAdd("ngix", "nginx");

            correctionAdd("ligHTTPd", "lighttpd");

            correctionAdd("wf", "Windows Workflow Foundation");
            correctionAdd("WF", "Windows Workflow Foundation");
            correctionAdd("windows workflow foundation", "Windows Workflow Foundation");

            correctionAdd("guid", "GUID");
            correctionAdd("Guid", "GUID");

            correctionAdd("liveCD", "live CD");
            correctionAdd("LiveCD", "live CD");
            correctionAdd("live cd", "live CD");
            correctionAdd("livecd", "live CD");
            correctionAdd("Live CD", "live CD");
            correctionAdd("live-cd", "live CD");
            correctionAdd("liveCd", "live CD");
            correctionAdd("Live-CD", "live CD");

            correctionAdd("octave", "Octave");
            correctionAdd("OCTAVE", "Octave");
            correctionAdd("Ocatve", "Octave");

            correctionAdd("apps", "application"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("Apps", "application"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("app", "application");
            correctionAdd("App", "application");
            correctionAdd("aplication", "application");
            correctionAdd("APP", "application");
            correctionAdd("applicatiion", "application"); //Misspelling.
            correctionAdd("Applicaiton", "application"); //Misspelling.
            correctionAdd("apllication", "application"); //Misspelling.
            correctionAdd("applicaiton", "application"); //Misspelling.
            correctionAdd("appliaction", "application"); //Misspelling.
            correctionAdd("appication", "application"); //Misspelling.
            correctionAdd("appplication", "application"); //Misspelling.
            correctionAdd("applicacion", "application"); //Misspelling.
            correctionAdd("applcation", "application"); //Misspelling.
            correctionAdd("applicaton", "application"); //Misspelling.
            correctionAdd("appliation", "application"); //Misspelling.
            correctionAdd("applucation", "application"); //Misspelling.
            correctionAdd("aplicación", "application");
            correctionAdd("applicattion", "application");
            correctionAdd("aplplication", "application");

            correctionAdd("directx", "DirectX");
            correctionAdd("direct x", "DirectX");
            correctionAdd("directX", "DirectX");
            correctionAdd("Direct X", "DirectX");

            correctionAdd("api", "API");
            correctionAdd("Api", "API");
            correctionAdd("aPI", "API");
            correctionAdd("APi", "API");

            correctionAdd("codesmith", "CodeSmith Generator");
            correctionAdd("CodeSmith", "CodeSmith Generator");
            correctionAdd("Codesmith", "CodeSmith Generator");

            correctionAdd("N-tier", "n-tier");
            correctionAdd("nTier", "n-tier");
            correctionAdd("3-tier", "n-tier"); //Not 100% correct - mostly for the Wikipedia link.
            correctionAdd("N-Tier", "n-tier");

            correctionAdd("DELPHI", "Delphi");
            correctionAdd("delphi", "Delphi");

            correctionAdd("saas", "SaaS");
            correctionAdd("Saas", "SaaS");
            correctionAdd("SAAS", "SaaS");

            correctionAdd("Pdf", "PDF");
            correctionAdd("pdf", "PDF");

            correctionAdd("google maps", "Google Maps");
            correctionAdd("google map", "Google Maps");
            correctionAdd("GMap", "Google Maps");
            correctionAdd("GoogleMaps", "Google Maps");
            correctionAdd("Google Map", "Google Maps");
            correctionAdd("Google map", "Google Maps");
            correctionAdd("Google maps", "Google Maps");

            correctionAdd("msaccess", "Microsoft Access");
            correctionAdd("ms-access", "Microsoft Access");
            correctionAdd("Access", "Microsoft Access");
            correctionAdd("ACCESS", "Microsoft Access");
            correctionAdd("MS ACCESS", "Microsoft Access");
            correctionAdd("ms access", "Microsoft Access");
            correctionAdd("microsoft access", "Microsoft Access");
            correctionAdd("MS-Access", "Microsoft Access");
            correctionAdd("MS Access", "Microsoft Access");

            correctionAdd("docs", "documentation");
            correctionAdd("doc", "documentation");
            correctionAdd("docu", "documentation");
            correctionAdd("Docs", "documentation"); //Not 100% correct - case.
            correctionAdd("Doc", "documentation"); //Not 100% correct - case.
            correctionAdd("ducumennation", "documentation"); //Misspelling.
            correctionAdd("documentatiin", "documentation"); //True typo...
            correctionAdd("documentaion", "documentation");
            correctionAdd("doco", "documentation");
            correctionAdd("Documenation", "documentation"); //Not 100% correct - case.
            correctionAdd("dox", "documentation");
            correctionAdd("documenation", "documentation");

            correctionAdd("sftp", "SFTP");

            correctionAdd("ipsec", "IPsec");
            correctionAdd("IPSEC", "IPsec");

            correctionAdd("dev", "developer"); //Expansion.
            correctionAdd("devel", "developer"); //Expansion.
            correctionAdd("develoepr", "developer");

            correctionAdd("jsBin", "JS Bin");
            correctionAdd("jsbin", "JS Bin");

            correctionAdd("jsfiddle", "JSFiddle");
            correctionAdd("JS Fiddle", "JSFiddle");
            correctionAdd("JSfiddle", "JSFiddle");
            correctionAdd("JSFIDDLE", "JSFiddle");
            correctionAdd("JsFiddle", "JSFiddle");
            correctionAdd("fiddle", "JSFiddle");
            correctionAdd("js fiddle", "JSFiddle");
            correctionAdd("Jsfiddle", "JSFiddle");
            correctionAdd("Fiddle", "JSFiddle");
            correctionAdd("jsFiddle", "JSFiddle");
            correctionAdd("JS fiddle", "JSFiddle");

            correctionAdd("cocoa-touch", "Cocoa Touch");
            correctionAdd("CocoaTouch", "Cocoa Touch");

            correctionAdd("gui", "GUI");
            correctionAdd("Gui", "GUI");

            correctionAdd("win32", "Win32");
            correctionAdd("WIN32", "Win32");
            correctionAdd("w32", "Win32"); //Expansion.
            correctionAdd("winapi", "Win32");
            correctionAdd("Windows API", "Win32");
            correctionAdd("Windows api", "Win32");
            correctionAdd("WinAPI", "Win32");
            correctionAdd("WINAPI", "Win32");
            correctionAdd("WinApi", "Win32");
            correctionAdd("WIndows Api", "Win32");
            correctionAdd("windows api", "Win32");
            correctionAdd("winAPI", "Win32");
            correctionAdd("Winapi", "Win32");
            correctionAdd("win API", "Win32");
            correctionAdd("win api", "Win32");


            correctionAdd("openssl", "OpenSSL");
            correctionAdd("Openssl", "OpenSSL");
            correctionAdd("openSSl", "OpenSSL");
            correctionAdd("openSSL", "OpenSSL");

            correctionAdd("opencv", "OpenCV");
            correctionAdd("openCV", "OpenCV");
            correctionAdd("openCv", "OpenCV");
            correctionAdd("Opencv", "OpenCV");
            correctionAdd("OpenCv", "OpenCV");
            correctionAdd("Open CV", "OpenCV");
            correctionAdd("opneCV", "OpenCV");

            correctionAdd("IIs", "IIS");
            correctionAdd("iis", "IIS");

            //It should really be "Microsoft Azure"
            correctionAdd("AZURE", "Windows Azure");
            correctionAdd("azure", "Windows Azure");
            correctionAdd("windows azure", "Windows Azure");
            correctionAdd("Azure", "Windows Azure");

            correctionAdd("cant", "can't");
            correctionAdd("cann't", "can't"); // A true typo!
            correctionAdd("can’t", "can't"); // Quora - mostly for the Wiktionary lookup (URL)
            correctionAdd("cann’t", "can't");
            correctionAdd("connot", "can't");
            correctionAdd("carn’t", "can't");
            correctionAdd("can´t", "can't");

            correctionAdd("VSS", "Visual SourceSafe");
            correctionAdd("vss", "Visual SourceSafe");
            correctionAdd("Visual source safe", "Visual SourceSafe");
            correctionAdd("source safe", "Visual SourceSafe");
            correctionAdd("SourceSafe", "Visual SourceSafe");
            correctionAdd("Visual Source Safe", "Visual SourceSafe");
            correctionAdd("Source Safe", "Visual SourceSafe");

            correctionAdd("trac", "Trac");

            correctionAdd("Fogbugz", "FogBugz");
            correctionAdd("Fog Bugz", "FogBugz");
            correctionAdd("Fog bugz", "FogBugz");

            correctionAdd("Zigbee", "ZigBee");
            correctionAdd("zigbee", "ZigBee");
            correctionAdd("ZIGBEE", "ZigBee");

            correctionAdd("xbee", "XBee");
            correctionAdd("Xbees", "XBee");
            correctionAdd("Xbee", "XBee");
            correctionAdd("xBee", "XBee");
            correctionAdd("XBEE", "XBee");
            correctionAdd("X-Bee", "XBee");

            correctionAdd("hotmail", "Hotmail");

            correctionAdd("codebehind", "code-behind");
            correctionAdd("Code Behind", "code-behind");
            correctionAdd("CodeBehind", "code-behind");
            correctionAdd("code behind", "code-behind");
            correctionAdd("codebehing", "code-behind");

            correctionAdd("bcp", "BCP");

            correctionAdd("windows explorer", "Windows&nbsp;Explorer");
            correctionAdd("explorer", "Windows&nbsp;Explorer"); //Overloaded term, and
                                       // it could mean Internet Explorer instead.
            correctionAdd("Windows explorer", "Windows&nbsp;Explorer");
            correctionAdd("Windows Explorer", "Windows&nbsp;Explorer"); // Effectively self
            correctionAdd("windows explore", "Windows&nbsp;Explorer");
            correctionAdd("Explorer", "Windows&nbsp;Explorer");
            correctionAdd("Windows Explore", "Windows&nbsp;Explorer");
            correctionAdd("Win Explorer", "Windows&nbsp;Explorer");
            correctionAdd("Windows-Explorer", "Windows&nbsp;Explorer");
            correctionAdd("win explorer", "Windows&nbsp;Explorer");

            correctionAdd("repo", "repository");
            correctionAdd("repos", "repository"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("Repos", "repository"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("Repo", "repository");
            correctionAdd("repositary", "repository");
            correctionAdd("respositary", "repository");
            correctionAdd("respository", "repository");
            correctionAdd("repoositry", "repository");
            correctionAdd("repsoitory", "repository");
            correctionAdd("reposity", "repository");
            correctionAdd("repositoy", "repository");
            correctionAdd("repositoritory", "repository");
            correctionAdd("repoistory", "repository");
            correctionAdd("repositorie", "repository");
            correctionAdd("repertory", "repository");

            correctionAdd("wmd", "WMD");

            correctionAdd("zedgraph", "ZedGraph");
            correctionAdd("zedGraph", "ZedGraph");
            correctionAdd("Zedgraph", "ZedGraph");
            correctionAdd("zed graph", "ZedGraph");

            correctionAdd("jython", "Jython");

            correctionAdd("ide", "IDE");
            correctionAdd("iDE", "IDE"); //Misspelling.
            correctionAdd("Ide", "IDE");
            correctionAdd("Integrated Development Environment", "IDE");
            correctionAdd("I.D.E", "IDE");

            correctionAdd("concat", "concatenate"); //Expansion.
            correctionAdd("cancat", "concatenate");
            correctionAdd("concatinate", "concatenate");
            correctionAdd("concenate", "concatenate");
            correctionAdd("concatation", "concatenate"); // Not 100% correct...
            correctionAdd("conctenation", "concatenate"); // Not 100% correct...
            correctionAdd("concadination", "concatenate"); // Not 100% correct...
            correctionAdd("concatinating", "concatenate"); // Not 100% correct...

            correctionAdd("gif", "GIF");
            correctionAdd("Gif", "GIF");

            correctionAdd("node.js", "Node.js");
            correctionAdd("NodeJS", "Node.js");
            correctionAdd("nodejs", "Node.js");
            correctionAdd("Nodejs", "Node.js");
            correctionAdd("NoJS", "Node.js"); //Common mishearing.
            correctionAdd("Node", "Node.js"); //??
            correctionAdd("node js", "Node.js");
            correctionAdd("NodeJs", "Node.js");
            correctionAdd("Node JS", "Node.js");
            correctionAdd("Node.JS", "Node.js");
            correctionAdd("Node js", "Node.js");
            correctionAdd("Node.Js", "Node.js");
            correctionAdd("node", "Node.js");
            correctionAdd("node.Js", "Node.js");
            correctionAdd("nodeJS", "Node.js");
            correctionAdd("nodeJs", "Node.js");
            correctionAdd("Node Js", "Node.js");
            correctionAdd("node JS", "Node.js");
            correctionAdd("Nodesj", "Node.js");
            correctionAdd("NODE", "Node.js");
            correctionAdd("NODE JS", "Node.js");

            correctionAdd("XHR", "XMLHttpRequest"); //Expansion.
            correctionAdd("xhr", "XMLHttpRequest");
            correctionAdd("XMLHTTPRequest", "XMLHttpRequest");
            correctionAdd("xmlhttprequest", "XMLHttpRequest");
            correctionAdd("xmlHttpRequest", "XMLHttpRequest");
            correctionAdd("XMLHTTPRequests", "XMLHttpRequest"); //Not 100% correct - plural.

            correctionAdd("tough", "though");
            correctionAdd("tho", "though");
            correctionAdd("thogh", "though");
            correctionAdd("Tho", "though");  //Expansion, not 100% correct. Add a plural feature?
            correctionAdd("thou", "though");
            correctionAdd("thouhg", "though");

            correctionAdd("radius", "RADIUS");
            correctionAdd("Radius", "RADIUS");

            correctionAdd("auth", "authentication"); //Expansion.
            correctionAdd("Auth", "authentication"); //Expansion, but case problem
            correctionAdd("authunticate", "authentication");  //Not 100% correct (noun vs. verb form)
            correctionAdd("authentification", "authentication");
            correctionAdd("authenticaion", "authentication");
            correctionAdd("Authentiaction", "authentication"); //Expansion, but case problem
            correctionAdd("authentiaction", "authentication");

            correctionAdd("Zip", "ZIP");
            correctionAdd("zip", "ZIP");

            correctionAdd("comet", "Comet");

            correctionAdd("pptp", "PPTP");

            correctionAdd("95", "Windows&nbsp;95");
            correctionAdd("Win95", "Windows&nbsp;95");

            correctionAdd("XP", "Windows&nbsp;XP"); //Expansion. Possible conflict with Extreme Programming.
            correctionAdd("WinXP", "Windows&nbsp;XP");
            correctionAdd("winXP", "Windows&nbsp;XP");
            correctionAdd("WindowsXp", "Windows&nbsp;XP");
            correctionAdd("windows xp", "Windows&nbsp;XP");
            correctionAdd("windox xp", "Windows&nbsp;XP");
            correctionAdd("Windows XP", "Windows&nbsp;XP");
            correctionAdd("win xp", "Windows&nbsp;XP");
            correctionAdd("Windows Xp", "Windows&nbsp;XP");
            correctionAdd("Win XP", "Windows&nbsp;XP");
            correctionAdd("windows XP", "Windows&nbsp;XP");
            correctionAdd("WinXp", "Windows&nbsp;XP");
            correctionAdd("Xp", "Windows&nbsp;XP");
            correctionAdd("xp", "Windows&nbsp;XP");
            correctionAdd("WIN XP", "Windows&nbsp;XP");
            correctionAdd("Windows xp", "Windows&nbsp;XP");
            correctionAdd("win-XP", "Windows&nbsp;XP");
            correctionAdd("WINXP", "Windows&nbsp;XP");
            correctionAdd("window xp", "Windows&nbsp;XP");
            correctionAdd("windows Xp", "Windows&nbsp;XP");
            correctionAdd("windows-XP", "Windows&nbsp;XP");
            correctionAdd("Win Xp", "Windows&nbsp;XP");
            correctionAdd("win XP", "Windows&nbsp;XP");
            correctionAdd("Window XP", "Windows&nbsp;XP");
            correctionAdd("Windows-XP", "Windows&nbsp;XP");
            correctionAdd("WindowsXP", "Windows&nbsp;XP");
            correctionAdd("winxp", "Windows&nbsp;XP");

            correctionAdd("Vista", "Windows&nbsp;Vista");
            correctionAdd("vista", "Windows&nbsp;Vista");
            correctionAdd("Windows Vista", "Windows&nbsp;Vista");
            correctionAdd("Win Vista", "Windows&nbsp;Vista");
            correctionAdd("WinVista", "Windows&nbsp;Vista");
            correctionAdd("windows vista", "Windows&nbsp;Vista");
            correctionAdd("win vista", "Windows&nbsp;Vista");
            correctionAdd("VISTA", "Windows&nbsp;Vista");
            correctionAdd("Windows vista", "Windows&nbsp;Vista");
            correctionAdd("VIsta", "Windows&nbsp;Vista");
            correctionAdd("windows-vista", "Windows&nbsp;Vista");
            correctionAdd("window vista", "Windows&nbsp;Vista");

            correctionAdd("7", "Windows&nbsp;7"); //Potential for false positives?
            correctionAdd("w7", "Windows&nbsp;7");
            correctionAdd("W7", "Windows&nbsp;7");
            correctionAdd("Win7", "Windows&nbsp;7");
            correctionAdd("win7", "Windows&nbsp;7");
            correctionAdd("Win 7", "Windows&nbsp;7");
            correctionAdd("Windows7", "Windows&nbsp;7");
            correctionAdd("windows7", "Windows&nbsp;7");
            correctionAdd("windows 7", "Windows&nbsp;7");
            correctionAdd("Windows 7", "Windows&nbsp;7");
            correctionAdd("windows seven", "Windows&nbsp;7");
            correctionAdd("window 7", "Windows&nbsp;7");
            correctionAdd("window7", "Windows&nbsp;7");
            correctionAdd("win 7", "Windows&nbsp;7");
            correctionAdd("Win-7", "Windows&nbsp;7");
            correctionAdd("Windows Seven", "Windows&nbsp;7");
            correctionAdd("Window 7", "Windows&nbsp;7");
            correctionAdd("Windows-7", "Windows&nbsp;7");
            correctionAdd("windows-7", "Windows&nbsp;7");
            correctionAdd("WIndows 7", "Windows&nbsp;7");
            correctionAdd("WIn7", "Windows&nbsp;7");
            correctionAdd("Windonw 7", "Windows&nbsp;7");
            correctionAdd("Windows seven", "Windows&nbsp;7");
            correctionAdd("wiidows 7", "Windows&nbsp;7");
            correctionAdd("Seven", "Windows&nbsp;7");
            correctionAdd("Window7", "Windows&nbsp;7");
            correctionAdd("WIN 7", "Windows&nbsp;7");
            correctionAdd("windwos 7", "Windows&nbsp;7");

            // This does not work as "&" is seen as a non-alpha... The
            // result is "Windows &"...
            //
            //correctionAdd("win &", "Windows&nbsp;7"); // On an English keyboard,
            ////                                           "&" is Shift + 7...
            //correctionAdd("win&", "Windows&nbsp;7"); // On an English keyboard,
            ////                                           "&" is Shift + 7...

            correctionAdd("config", "configuration");
            correctionAdd("Config", "configuration");
            correctionAdd("conf", "configuration");
            correctionAdd("confirguration", "configuration");
            correctionAdd("confugration", "configuration");
            correctionAdd("confiq", "configuration");
            correctionAdd("Configuaration", "configuration");
            correctionAdd("configuaration", "configuration");
            correctionAdd("congifuration", "configuration");

            correctionAdd("Voip", "VoIP");
            correctionAdd("VOIP", "VoIP");
            correctionAdd("voip", "VoIP");
            correctionAdd("VoIp", "VoIP");

            correctionAdd("fedora", "Fedora");
            correctionAdd("Fedora Linux", "Fedora");

            correctionAdd("info", "information");
            correctionAdd("Info", "information");
            correctionAdd("informatipon", "information"); //Misspelling.
            correctionAdd("inforamtion", "information"); //Misspelling.
            correctionAdd("infomation", "information"); //Misspelling.
            correctionAdd("infos", "information");
            correctionAdd("informaton", "information");
            correctionAdd("infrormation", "information");

            correctionAdd("ip", "IP address");
            correctionAdd("IP", "IP address");
            correctionAdd("@IP", "IP address");
            correctionAdd("Ipaddress", "IP address");
            correctionAdd("ip adress", "IP address");
            correctionAdd("ip address", "IP address");
            correctionAdd("ipaddress", "IP address");
            correctionAdd("IPAddress", "IP address");
            correctionAdd("IP-Address", "IP address");
            correctionAdd("Ip address", "IP address");
            correctionAdd("I.P. address", "IP address");
            correctionAdd("IP-Adress", "IP address");
            correctionAdd("IP Addres", "IP address");
            correctionAdd("IP adress", "IP address");
            correctionAdd("ip-address", "IP address");
            correctionAdd("Ip Adress", "IP address");
            correctionAdd("Ip", "IP address");
            correctionAdd("IP Address", "IP address");
            correctionAdd("ip Address", "IP address");
            correctionAdd("IpAddress", "IP address");
            correctionAdd("IpAdress", "IP address");
            correctionAdd("IPaddress", "IP address");
            correctionAdd("IPADDRESS", "IP address");
            correctionAdd("ip addresss", "IP address");

            correctionAdd("stats", "statistics");
            correctionAdd("stasistics", "statistics");

            correctionAdd("btw", "by the way");
            correctionAdd("Btw", "by the way");
            correctionAdd("BTW", "by the way");

            correctionAdd("kohana", "Kohana"); //Not actually observed.

            correctionAdd("Pear", "PEAR"); //Not actually observed.
            correctionAdd("pear", "PEAR"); //Not actually observed.

            correctionAdd("https", "HTTPS");
            correctionAdd("Https", "HTTPS");
            correctionAdd("httpS", "HTTPS");

            correctionAdd("jsonp", "JSONP");
            correctionAdd("JSON-P", "JSONP");
            correctionAdd("Jsonp", "JSONP");

            correctionAdd("param", "parameter"); //Expansion.
            correctionAdd("params", "parameter"); //Expansion, not 100% correct. Add a plural feature?
            correctionAdd("parametrs", "parameter"); //Not 100% correct - plural.
            correctionAdd("paramter", "parameter");
            correctionAdd("paramater", "parameter");
            correctionAdd("paramaters", "parameter"); //Not 100% correct - plural.
            correctionAdd("paramenter", "parameter");
            correctionAdd("parametr", "parameter");
            correctionAdd("parament", "parameter");
            correctionAdd("parametar", "parameter");
            correctionAdd("parametre", "parameter");

            correctionAdd("Ultraedit", "UltraEdit");
            correctionAdd("ultraedit", "UltraEdit");
            correctionAdd("ultra edit", "UltraEdit");
            correctionAdd("Ultra Edit", "UltraEdit");
            correctionAdd("UE", "UltraEdit");

            correctionAdd("chars", "characters"); //Expansion.
            correctionAdd("char", "characters"); //Expansion, not 100% correct. Add a plural feature?
            correctionAdd("charachter", "characters"); //Not 100% correct. Add a plural feature?
            correctionAdd("charaters", "characters");
            correctionAdd("charater", "characters");
            correctionAdd("charector", "characters"); //Not 100% correct. Add a plural feature?
            correctionAdd("charectors", "characters");
            correctionAdd("cheracter", "characters");
            correctionAdd("cheracters", "characters");

            correctionAdd("MarkDown", "Markdown");
            correctionAdd("markdown", "Markdown");
            correctionAdd("mardown", "Markdown");

            correctionAdd("xss", "XSS");
            correctionAdd("Cross Site Scripting", "XSS");
            correctionAdd("cross site scripting", "XSS");

            correctionAdd("Cross-Site Request Forgery", "cross-site request forgery");

            correctionAdd("JS Lint", "JSLint");
            correctionAdd("JS lint", "JSLint");
            correctionAdd("jslint", "JSLint");

            correctionAdd("opendns", "OpenDNS");
            correctionAdd("Opendns", "OpenDNS");

            correctionAdd("dns", "DNS");

            correctionAdd("ram", "RAM");
            correctionAdd("Ram", "RAM");

            correctionAdd("rake", "Rake");

            correctionAdd("temp", "temporary");
            correctionAdd("tmp", "temporary");
            correctionAdd("Temporatly", "temporary");
            correctionAdd("temporatly", "temporary");

            correctionAdd("google-fu", "Google-fu");
            correctionAdd("google fu", "Google-fu");
            correctionAdd("googlefu", "Google-fu");
            correctionAdd("Google fo", "Google-fu");
            correctionAdd("Google foo", "Google-fu");
            correctionAdd("google fo", "Google-fu");
            correctionAdd("google foo", "Google-fu");
            correctionAdd("Google fu", "Google-fu");

            correctionAdd("ssh", "SSH");

            correctionAdd("vpn", "VPN");

            correctionAdd("faq", "FAQ");
            correctionAdd("Faq", "FAQ");

            correctionAdd("axis", "Axis"); // That is, Apache Axis

            correctionAdd("cert", "certificate");
            correctionAdd("Cert", "certificate");
            correctionAdd("certs", "certificate"); //Expansion, not 100% correct. Add a plural feature?
            correctionAdd("certicificate", "certificate"); //Misspelling.
            correctionAdd("certicate", "certificate");
            correctionAdd("certifcate", "certificate");
            correctionAdd("certitificate", "certificate");

            correctionAdd("ebay", "eBay");
            correctionAdd("Ebay", "eBay");
            correctionAdd("EBay", "eBay");

            correctionAdd("ok", "OK");
            correctionAdd("Ok", "OK");

            correctionAdd("lgpl", "LGPL");

            correctionAdd("gpl", "GPL");

            correctionAdd("iirc", "IIRC");
            correctionAdd("Iirc", "IIRC");

            correctionAdd("var", "variable");
            correctionAdd("vars", "variable"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("varialbe", "variable");
            correctionAdd("varibale", "variable");
            correctionAdd("variabe", "variable");
            correctionAdd("valiarble", "variable");
            correctionAdd("varible", "variable");
            correctionAdd("variales", "variable");
            correctionAdd("varaible", "variable");
            correctionAdd("vailble", "variable");
            correctionAdd("varialble", "variable");

            correctionAdd("utorrent", "µTorrent");
            correctionAdd("uTorrent", "µTorrent");
            correctionAdd("Utorrent", "µTorrent");

            correctionAdd("ios", "iOS");
            correctionAdd("IOS", "iOS");
            correctionAdd("iOs", "iOS");
            correctionAdd("iO.S", "iOS");
            correctionAdd("Ios", "iOS");

            correctionAdd("sbt", "SBT");

            correctionAdd("vimeo", "Vimeo");

            correctionAdd("netflix", "Netflix");
            correctionAdd("Netfix", "Netflix");

            correctionAdd("linked in", "LinkedIn");
            correctionAdd("Linkedin", "LinkedIn");
            correctionAdd("LI", "LinkedIn");
            correctionAdd("linkedin", "LinkedIn");
            correctionAdd("Linkin", "LinkedIn");
            correctionAdd("linkedIn", "LinkedIn");
            correctionAdd("Linkldln", "LinkedIn");
            correctionAdd("Linked-In", "LinkedIn");
            correctionAdd("likedin", "LinkedIn");
            correctionAdd("Linked-in", "LinkedIn");
            correctionAdd("Linked In", "LinkedIn");

            correctionAdd("tumblr", "Tumblr");

            correctionAdd("wikipedia", "Wikipedia");
            correctionAdd("WikiPedia", "Wikipedia");
            correctionAdd("Wipedia", "Wikipedia");
            correctionAdd("Wikpedia", "Wikipedia");

            correctionAdd("HAML", "Haml");
            correctionAdd("haml", "Haml");

            correctionAdd("udp", "UDP");
            correctionAdd("Udp", "UDP");

            correctionAdd("vnc", "VNC");

            correctionAdd("macports", "MacPorts");
            correctionAdd("mac ports", "MacPorts");
            correctionAdd("Macports", "MacPorts");
            correctionAdd("MacPort", "MacPorts");
            correctionAdd("macport", "MacPorts");

            correctionAdd("spec", "specification");
            correctionAdd("Spec", "specification");
            correctionAdd("Specs", "specification"); //Not 100% correct - plural.
            correctionAdd("specs", "specification"); //Not 100% correct - plural.
            correctionAdd("specificaiton", "specification");
            correctionAdd("specficiation", "specification");

            correctionAdd("midlet", "MIDlet");

            correctionAdd("s60", "S60");

            correctionAdd("wsdl", "WSDL"); //Not actually observed.
            correctionAdd("Wsdl", "WSDL");

            correctionAdd("cocos2d", "Cocos2d");

            correctionAdd("Ipad", "iPad");
            correctionAdd("ipad", "iPad");
            correctionAdd("iPAD", "iPad");
            correctionAdd("IPad", "iPad");
            correctionAdd("IPAD", "iPad");

            correctionAdd("SuperUser", "Super&nbsp;User");
            correctionAdd("SU", "Super&nbsp;User");
            correctionAdd("super user", "Super&nbsp;User");
            correctionAdd("Super User", "Super&nbsp;User");
            correctionAdd("Superuser", "Super&nbsp;User");
            correctionAdd("superuser", "Super&nbsp;User");
            correctionAdd("Super users", "Super&nbsp;User");
            correctionAdd("super-user", "Super&nbsp;User");

            correctionAdd("MSO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("mso", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta Stack Overflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta.StackOverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("meta.so", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("meta stack overflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("meta stackoverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("meta.SO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("meta SO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("meta-stackoverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("metastackoverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("meta.stackoverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta-stackoverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta Stackoverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Metastackoverflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta.SO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("META SO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta SO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta-SO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("MetaSO", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Meta Stack&nbsp;Overflow", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("stackoverflow META", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("stack overflow META", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Stack Overflow META", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Stack Overflow Meta", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("Stack&nbsp;Overflow META", "Meta&nbsp;Stack&nbsp;Overflow");
            correctionAdd("StackOverflow meta", "Meta&nbsp;Stack&nbsp;Overflow");

            correctionAdd("coffeescript", "CoffeeScript");
            correctionAdd("Coffeescript", "CoffeeScript");
            correctionAdd("Coffee Script", "CoffeeScript");
            correctionAdd("coffee-script", "CoffeeScript");
            correctionAdd("coffescript", "CoffeeScript");
            correctionAdd("Coffescript", "CoffeeScript");
            correctionAdd("coffee script", "CoffeeScript");
            correctionAdd("CoffeScript", "CoffeeScript");
            correctionAdd("coffeeScript", "CoffeeScript");

            correctionAdd("Jscript", "JScript");
            correctionAdd("jscript", "JScript");
            correctionAdd("JSCRIPT", "JScript");

            correctionAdd("rhino", "Rhino");

            correctionAdd("nas", "NAS");

            correctionAdd("svg", "SVG");

            correctionAdd("dotnetnuke", "DotNetNuke");

            correctionAdd("adobe air", "Adobe AIR");
            correctionAdd("AIR", "Adobe AIR");
            correctionAdd("Adobe air", "Adobe AIR");
            correctionAdd("adobe Air", "Adobe AIR");
            correctionAdd("Air", "Adobe AIR");
            correctionAdd("Adobe Air", "Adobe AIR");

            correctionAdd("awt", "AWT");

            correctionAdd("irfanview", "IrfanView");
            correctionAdd("Irfanview", "IrfanView");
            correctionAdd("irfan view", "IrfanView");

            correctionAdd("UX", "user experience"); //Expansion.
            correctionAdd("ux", "user experience"); //Expansion.

            correctionAdd("sqlalchemy", "SQLAlchemy");
            correctionAdd("Sqlalchemy", "SQLAlchemy");

            correctionAdd("bios", "BIOS");
            correctionAdd("Bios", "BIOS");

            correctionAdd("plone", "Plone");

            correctionAdd("simpledb", "SimpleDB");

            correctionAdd("RRDTool", "RRDtool");
            correctionAdd("rrdtool", "RRDtool");

            correctionAdd("nfs", "NFS");
            correctionAdd("Nfs", "NFS");

            correctionAdd("RS232", "RS-232");
            correctionAdd("rs232", "RS-232");
            correctionAdd("Rs232", "RS-232");
            correctionAdd("rs-232", "RS-232");

            correctionAdd("skype", "Skype");

            correctionAdd("FORTH", "Forth");
            correctionAdd("forth", "Forth");

            correctionAdd("arg", "argument");
            correctionAdd("args", "argument"); //Not 100% correct - plural.
            correctionAdd("agurment", "argument");
            correctionAdd("arugument", "argument");
            correctionAdd("arguement", "argument");

            correctionAdd("epub", "EPUB");
            correctionAdd("ePub", "EPUB");

            correctionAdd("SASS", "Sass");
            correctionAdd("sass", "Sass");

            correctionAdd("coda", "Coda");

            correctionAdd("espresso", "Espresso"); //Web editor from MacRabbit.
            correctionAdd("Expresso", "Espresso");

            correctionAdd("swig", "SWIG");

            correctionAdd("dir", "directory");
            correctionAdd("dirs", "directory");  //Not 100% correct - plural.
            correctionAdd("directoty", "directory");
            correctionAdd("diretory", "directory");
            correctionAdd("direcoty", "directory");
            correctionAdd("diretcory", "directory");
            correctionAdd("directoy", "directory");
            correctionAdd("dirctory", "directory");
            correctionAdd("direcotires", "directory");

            correctionAdd("dont", "don’t");
            correctionAdd("Dont", "don’t"); //Not 100% correct - case.
            correctionAdd("dom't", "don’t");
            correctionAdd("don't", "don’t");
            correctionAdd("dont't", "don’t");
            correctionAdd("don´t", "don’t");
            correctionAdd("do't", "don’t");

            correctionAdd("lisp", "Lisp");
            correctionAdd("LISP", "Lisp");

            correctionAdd("elisp", "Emacs&nbsp;Lisp");
            correctionAdd("Emacs Lisp", "Emacs&nbsp;Lisp");

            correctionAdd("Lapack", "LAPACK");
            correctionAdd("lapack", "LAPACK");

            correctionAdd("nVidia", "Nvidia");
            correctionAdd("nvidia", "Nvidia");
            correctionAdd("NVIDIA", "Nvidia");
            correctionAdd("NVidia", "Nvidia");
            correctionAdd("Nvivdia", "Nvidia");

            correctionAdd("registery", "Windows Registry");
            correctionAdd("registry", "Windows Registry");
            correctionAdd("regestry", "Windows Registry");
            correctionAdd("Registry", "Windows Registry");

            correctionAdd("MS", "Microsoft"); //Overloaded term!!. Mass spectrometry, multiple sclerosis, master of science, millisecond, etc.
            correctionAdd("ms", "Microsoft"); //Overloaded term!!. Mass spectrometry, multiple sclerosis, master of science, millisecond, etc.
            correctionAdd("M-Soft", "Microsoft");
            correctionAdd("MSFT", "Microsoft");
            correctionAdd("Microsof", "Microsoft");
            correctionAdd("Micrsoft", "Microsoft");
            correctionAdd("MIcrosoft", "Microsoft");
            correctionAdd("microsoft", "Microsoft");
            correctionAdd("MicroSoft", "Microsoft");
            correctionAdd("microsft", "Microsoft");
            correctionAdd("Miscrosoft", "Microsoft");
            correctionAdd("Micorsoft", "Microsoft");
            correctionAdd("Microsft", "Microsoft");
            correctionAdd("msoft", "Microsoft");

            correctionAdd("area51", "Area&nbsp;51");
            correctionAdd("Area 51", "Area&nbsp;51");
            correctionAdd("Area51", "Area&nbsp;51");
            correctionAdd("A51", "Area&nbsp;51");
            correctionAdd("area 51", "Area&nbsp;51");
            correctionAdd("AREA 51", "Area&nbsp;51");
            correctionAdd("Arae51", "Area&nbsp;51");
            correctionAdd("a51", "Area&nbsp;51");
            correctionAdd("Area-51", "Area&nbsp;51");

            correctionAdd("jpeg", "JPEG");
            correctionAdd("Jpeg", "JPEG");
            correctionAdd("jpg", "JPEG");
            correctionAdd("JPG", "JPEG");
            correctionAdd("Jpg", "JPEG");

            correctionAdd("PW", "password");
            correctionAdd("pw", "password");

            correctionAdd("phonegap", "PhoneGap");
            correctionAdd("Phonegap", "PhoneGap");
            correctionAdd("phongap", "PhoneGap"); //Misspelling.
            correctionAdd("phoneGap", "PhoneGap");
            correctionAdd("phone gap", "PhoneGap");

            correctionAdd("MEF", "Managed Extensibility Framework");

            correctionAdd("cassandra", "Cassandra"); //Possibly overloaded.

            correctionAdd("rep", "reputation points");
            correctionAdd("reputation", "reputation points");
            correctionAdd("repuration", "reputation points");
            correctionAdd("reputational points", "reputation points");
            correctionAdd("repuatation", "reputation points");
            correctionAdd("reps", "reputation points");
            correctionAdd("repu", "reputation points");
            correctionAdd("repu points", "reputation points");
            correctionAdd("rp", "reputation points");
            correctionAdd("reputations", "reputation points");
            correctionAdd("rep point", "reputation points");
            correctionAdd("rep. points", "reputation points");

            correctionAdd("squid", "Squid");

            correctionAdd("hulu", "Hulu");

            correctionAdd("selinux", "SELinux");
            correctionAdd("SELINUX", "SELinux");
            correctionAdd("SElinux", "SELinux");
            correctionAdd("Selinux", "SELinux");
            correctionAdd("SeLinux", "SELinux");

            correctionAdd("mod", "moderator"); //Not unique. E.g. modification.
            correctionAdd("mods", "moderator"); //Expansion, not 100% correct. Add a plural feature?
            correctionAdd("Mod", "moderator");

            correctionAdd("cdn", "CDN");
            correctionAdd("content delivery network", "CDN");
            correctionAdd("Content Delivery Network", "CDN");

            correctionAdd("RedHat", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("RHEL", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("redhat", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("red-hat", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("Redhat", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("Red Hat", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("Redhat Linux", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("linux redhat", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("RH", "Red Hat Linux"); //The OS, not the company.
            correctionAdd("RedHat Linux", "Red Hat Linux");
            correctionAdd("REDHAT", "Red Hat Linux");

            correctionAdd("phising", "phishing");

            correctionAdd("dhcp", "DHCP");
            correctionAdd("DCHP", "DHCP");

            correctionAdd("ur", "your");

            correctionAdd("havent", "haven't");
            correctionAdd("havn't", "haven't");

            correctionAdd("fbml", "FBML");
            correctionAdd("fmbl", "FBML");

            correctionAdd("win2000", "Windows 2000");
            correctionAdd("2K", "Windows 2000"); //Highly likely to collide with something else.
            correctionAdd("Win 2000", "Windows 2000");
            correctionAdd("2k", "Windows 2000"); //Highly likely to collide with something else.
            correctionAdd("Win2k", "Windows 2000");
            correctionAdd("Win 2K", "Windows 2000");
            correctionAdd("2000", "Windows 2000");

            correctionAdd("ipa", "IPA");

            correctionAdd("dma", "DMA");

            correctionAdd("cms", "content management system (CMS)");
            correctionAdd("Content Management System", "content management system (CMS)");
            correctionAdd("CMS", "content management system (CMS)");
            correctionAdd("content management system", "content management system (CMS)");

            correctionAdd("Hootsuite", "HootSuite");
            correctionAdd("hootsuite", "HootSuite");

            correctionAdd("tweetdeck", "TweetDeck");
            correctionAdd("Tweetdeck", "TweetDeck");
            correctionAdd("tweet deck", "TweetDeck");

            correctionAdd("bugzilla", "Bugzilla");

            correctionAdd("b/c", "because");
            correctionAdd("cuz", "because");
            correctionAdd("bc", "because"); //Does it conflict?
            correctionAdd("cos", "because"); //Posibble conflict with cosine.
            correctionAdd("cause", "because");
            correctionAdd("becaus", "because");
            correctionAdd("Becaus", "because");
            correctionAdd("becuase", "because");
            correctionAdd("becasue", "because");
            correctionAdd("coz", "because");
            correctionAdd("Cos", "because");
            correctionAdd("b'caz", "because");
            correctionAdd("beazse", "because");
            correctionAdd("bcoz", "because");
            correctionAdd("becase", "because"); //Likely a typo
            correctionAdd("beacause", "because"); //Likely a typo
            correctionAdd("Coz", "because"); //Not 100% correct - casing.
            correctionAdd("bec", "because");
            correctionAdd("'Cause", "because");
            correctionAdd("'cause", "because");
            // correctionAdd("bec", "because"); //Should really be "bec.".  It broke with some recent changes!!!
            correctionAdd("Cuz", "because"); //Not 100% correct - casing.
            correctionAdd("B'se", "because");
            correctionAdd("becoz", "because");
            correctionAdd("becouse", "because");
            correctionAdd("beause", "because"); //Misspelling
            correctionAdd("bcz", "because");
            correctionAdd("'cos", "because");
            correctionAdd("beacuse", "because"); //Misspelling
            correctionAdd("beacuase", "because"); //Misspelling
            correctionAdd("becuse", "because"); //Misspelling
            correctionAdd("becaue", "because"); //Misspelling
            correctionAdd("Becuase", "because"); //Not 100% correct - casing.
            correctionAdd("becaise", "because");
            correctionAdd("becasuse", "because");
            correctionAdd("Cause", "because"); //Not 100% correct - casing.
            correctionAdd("becaouse", "because"); //Misspelling
            correctionAdd("b'cos", "because");
            correctionAdd("Becasue", "because"); //Not 100% correct - casing.
            //correctionAdd("becasue", "because");
            correctionAdd("bacause", "because");
            correctionAdd("bcs", "because");
            correctionAdd("becaause", "because"); //Misspelling
            correctionAdd("Bcoz", "because");
            correctionAdd("Bcz", "because"); //Not 100% correct - casing.
            correctionAdd("cose", "because");
            correctionAdd("baecause", "because");
            correctionAdd("Beacuse", "because"); //Not 100% correct - casing.
            correctionAdd("Bcause", "because"); //Not 100% correct - casing.
            correctionAdd("becayse", "because");
            correctionAdd("becauae", "because");
            correctionAdd("’Cause", "because");
            correctionAdd("’cause", "because");
            correctionAdd("Beecause", "because");
            correctionAdd("beecause", "because");
            correctionAdd("be cause", "because");


            correctionAdd("adk", "ADK"); //Note: the Wikipedia page has
            //  unfortunately been deleted: http://en.wikipedia.org/wiki/ADK_(Android_Open_Accessory_Development_Kit)

            correctionAdd("debian", "Debian");
            correctionAdd("deb", "Debian");
            correctionAdd("Debain", "Debian");
            correctionAdd("debain", "Debian");

            correctionAdd("debian lenny", "Debian&nbsp;5.0 (Lenny)");
            correctionAdd("lenny", "Debian&nbsp;5.0 (Lenny)");
            correctionAdd("Lenny", "Debian&nbsp;5.0 (Lenny)");

            correctionAdd("Squeeze", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("squeeze", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("Debian 6.0", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("Debian 6", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("debian 6", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("Debian Squeeze", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("debian squeeze", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("Debian&nbsp;6.0", "Debian&nbsp;6.0 (Squeeze)");
            correctionAdd("Debian squeeze", "Debian&nbsp;6.0 (Squeeze)");

            correctionAdd("wheezy", "Debian&nbsp;7 (Wheezy)");
            correctionAdd("Wheezy", "Debian&nbsp;7 (Wheezy)");
            correctionAdd("Debian 7", "Debian&nbsp;7 (Wheezy)");
            correctionAdd("debian 7", "Debian&nbsp;7 (Wheezy)");
            correctionAdd("Debian Wheezy", "Debian&nbsp;7 (Wheezy)");
            correctionAdd("debian wheezy", "Debian&nbsp;7 (Wheezy)");
            correctionAdd("Debian&nbsp;7", "Debian&nbsp;7 (Wheezy)");
            correctionAdd("Debian wheezy", "Debian&nbsp;7 (Wheezy)");

            correctionAdd("jessie", "Debian&nbsp;8 (Jessie)");
            correctionAdd("Jessie", "Debian&nbsp;8 (Jessie)");
            correctionAdd("Debian 8", "Debian&nbsp;8 (Jessie)");
            correctionAdd("debian 8", "Debian&nbsp;8 (Jessie)");
            correctionAdd("Debian Jessie", "Debian&nbsp;8 (Jessie)");
            correctionAdd("Debian&nbsp;8", "Debian&nbsp;8 (Jessie)");
            correctionAdd("debian jessie", "Debian&nbsp;8 (Jessie)");
            correctionAdd("Debian-Jessie", "Debian&nbsp;8 (Jessie)");

            correctionAdd("mathematica", "Mathematica");
            correctionAdd("Mma", "Mathematica");

            correctionAdd("Jit", "JIT");
            correctionAdd("jit", "JIT");
            correctionAdd("Just-In-Time", "JIT");

            correctionAdd("Dry", "DRY");
            correctionAdd("dry", "DRY"); //Context dependent!

            correctionAdd("grub", "GRUB");
            correctionAdd("Grub", "GRUB");

            correctionAdd("gnome", "GNOME");
            correctionAdd("Gnome", "GNOME");

            correctionAdd("avd", "Android Virtual Device"); //Expansion. AVD.

            correctionAdd("ocaml", "OCaml");
            correctionAdd("OCaML", "OCaml");
            correctionAdd("Ocaml", "OCaml");

            correctionAdd("boo", "Boo");

            correctionAdd("jad", "JAD");

            correctionAdd("symbian", "Symbian");

            correctionAdd("windows phone 7", "Windows Phone"); //That is the physical phone.
            correctionAdd("windows phone", "Windows Phone");
            correctionAdd("Windows phone", "Windows Phone");
            correctionAdd("windowsphone", "Windows Phone");
            correctionAdd("WP7", "Windows Phone"); //Expansion, not 100% correct.
            correctionAdd("wp7", "Windows Phone"); //Expansion, not 100% correct..
            correctionAdd("Windows Phone 7", "Windows Phone"); //Not 100% correct.
            correctionAdd("windows phone 8", "Windows Phone"); //Not 100% correct.
            correctionAdd("Windows Mobile 6", "Windows Phone"); //Not 100% correct.
            correctionAdd("windows-phone-8", "Windows Phone"); //Not 100% correct.
            correctionAdd("windows-phone", "Windows Phone");
            correctionAdd("WinPhone", "Windows Phone");

            correctionAdd("windows mobile", "Windows Mobile"); //That is the operating system.
            correctionAdd("MS Mobile", "Windows Mobile"); //That is the operating system.
            correctionAdd("WM", "Windows Mobile"); //Expansion
            correctionAdd("winmo", "Windows Mobile"); //Expansion
            correctionAdd("Windows mobile", "Windows Mobile"); //That is the operating system.

            correctionAdd("JQuery Mobile", "jQuery Mobile");
            correctionAdd("jquery mobile", "jQuery Mobile");
            correctionAdd("jqm", "jQuery Mobile");
            correctionAdd("JqueryMobile", "jQuery Mobile");
            correctionAdd("jQM", "jQuery Mobile");
            correctionAdd("jQueryMobile", "jQuery Mobile");
            correctionAdd("Jquery mobile", "jQuery Mobile");

            correctionAdd("smalltalk", "Smalltalk");
            correctionAdd("SmallTalk", "Smalltalk");

            correctionAdd("Sharpdevelop", "SharpDevelop");
            correctionAdd("sharpdevelop", "SharpDevelop");
            correctionAdd("sharp develop", "SharpDevelop");

            correctionAdd("ZX spectrum", "ZX Spectrum");
            correctionAdd("ZX-Spectrum", "ZX Spectrum");

            correctionAdd("IoC", "inversion of control"); //Expansion.
            correctionAdd("IOC", "inversion of control"); //Expansion. And correction.
            correctionAdd("Ioc", "inversion of control");
            correctionAdd("Invesion of Control", "inversion of control");
            correctionAdd("Inversion of Control", "inversion of control");
            correctionAdd("inversion Of control", "inversion of control");
            correctionAdd("ioc", "inversion of control");

            correctionAdd("Websphere", "WebSphere");

            correctionAdd("BBedit", "BBEdit");

            correctionAdd("wont", "won't");
            correctionAdd("won’t", "won't"); //Quora...
            correctionAdd("won´t", "won't");

            correctionAdd("xsl", "XSL");

            correctionAdd("xhtml", "XHTML");
            correctionAdd("xHTML", "XHTML");
            correctionAdd("X HTML", "XHTML");

            correctionAdd("cvs", "CVS");

            correctionAdd("openssh", "OpenSSH");
            correctionAdd("openSSH", "OpenSSH");
            correctionAdd("Open SSH", "OpenSSH");
            correctionAdd("open-ssh", "OpenSSH");

            correctionAdd("Opml", "OPML");

            correctionAdd("wap", "WAP");

            correctionAdd("google translate", "Google Translate");
            correctionAdd("Google translate", "Google Translate");
            correctionAdd("Google translator", "Google Translate");
            correctionAdd("google translator", "Google Translate");
            correctionAdd("Google Translator", "Google Translate");
            correctionAdd("google-translate", "Google Translate");
            correctionAdd("Google translation", "Google Translate");
            correctionAdd("google translation", "Google Translate");

            correctionAdd("xsd", "XSD");
            correctionAdd("Xsd", "XSD");

            correctionAdd("Glide", "Glide&nbsp;API");
            correctionAdd("glide", "Glide&nbsp;API");
            correctionAdd("GLide", "Glide&nbsp;API");

            correctionAdd("povray", "POV-Ray");

            correctionAdd("xna", "XNA");

            correctionAdd("Direct3d", "Direct3D");
            correctionAdd("direct3d", "Direct3D");

            correctionAdd("bezier", "Bézier curve");
            correctionAdd("bezier curve", "Bézier curve");
            correctionAdd("beizer", "Bézier curve");
            correctionAdd("beizer curve", "Bézier curve");

            correctionAdd("w/o", "without");
            correctionAdd("w/out", "without");
            correctionAdd("Whithout", "without");
            correctionAdd("whithout", "without");
            correctionAdd("W/O", "without");
            correctionAdd("witouth", "without");
            correctionAdd("with out", "without");

            correctionAdd("blender", "Blender");

            correctionAdd("Installshield", "InstallShield");
            correctionAdd("installshield", "InstallShield");
            correctionAdd("install shield", "InstallShield");
            correctionAdd("Install Shield", "InstallShield");
            correctionAdd("installShield", "InstallShield");
            correctionAdd("installsheild", "InstallShield");

            correctionAdd("msi", "MSI");
            correctionAdd("Windows Installer", "MSI"); //Going for the abbreviation. Is this good?
            correctionAdd("Windows installer", "MSI"); //Going for the abbreviation. Is this good?
            correctionAdd("Win inst", "MSI");
            correctionAdd("win inst", "MSI");
            correctionAdd("Msi", "MSI");
            correctionAdd("windows installer", "MSI");

            correctionAdd("rtf", "RTF");

            correctionAdd("aes", "AES");

            correctionAdd("process explorer", "Process&nbsp;Explorer");
            correctionAdd("Process Explorer", "Process&nbsp;Explorer");
            correctionAdd("ProcessExplorer", "Process&nbsp;Explorer");
            correctionAdd("processexplorer", "Process&nbsp;Explorer");
            correctionAdd("process Explorer", "Process&nbsp;Explorer");
            correctionAdd("procexp", "Process&nbsp;Explorer");
            correctionAdd("Procexp", "Process&nbsp;Explorer");

            correctionAdd("Ilasm", "ILAsm");

            correctionAdd("Ildasm", "ILDASM");
            correctionAdd("ildasm", "ILDASM");

            correctionAdd("il", "CIL"); //The .NET one.
            correctionAdd("IL", "CIL"); //The .NET one.
            correctionAdd("MSIL", "CIL"); //The .NET one.
            correctionAdd("msil", "CIL"); //The .NET one.
            correctionAdd("cil", "CIL"); //The .NET one.

            correctionAdd("gc", "GC"); //Possible conflict with "Google Chrome".
            correctionAdd("Gargbage collection", "GC");
            correctionAdd("garbage collection", "GC");

            correctionAdd("uri", "URI");
            correctionAdd("Uri", "URI");

            correctionAdd("trough", "through");
            correctionAdd("thru", "through"); //Expansion
            correctionAdd("througt", "through");
            correctionAdd("threw", "through");
            correctionAdd("thorugh", "through");
            correctionAdd("throgh", "through");
            correctionAdd("thro", "through");
            correctionAdd("throuth", "through");
            correctionAdd("tru", "through");
            correctionAdd("througth", "through");
            correctionAdd("throu", "through");
            correctionAdd("throough", "through");
            correctionAdd("througg", "through");
            correctionAdd("throuh", "through");
            correctionAdd("Throught", "through");
            correctionAdd("throught", "through");


            correctionAdd("tut", "tutorial");
            correctionAdd("tutorail", "tutorial"); //Misspelling
            correctionAdd("tutiorial", "tutorial"); //Misspelling

            correctionAdd("Isotope", "jQuery&nbsp;Isotope");
            correctionAdd("JQuery Isotope", "jQuery&nbsp;Isotope");
            correctionAdd("isotope", "jQuery&nbsp;Isotope");

            correctionAdd("twitter", "Twitter");
            correctionAdd("tritter", "Twitter");
            correctionAdd("twittter", "Twitter");

            correctionAdd("dos", "DOS");
            correctionAdd("MS DOS", "DOS");
            correctionAdd("MS-DOS", "DOS");
            correctionAdd("msdos", "DOS");
            correctionAdd("Dos", "DOS");
            correctionAdd("ms dos", "DOS");
            correctionAdd("ms-dos", "DOS");
            correctionAdd("MS-Dos", "DOS");

            correctionAdd("AppStore", "App&nbsp;Store");
            correctionAdd("App Store", "App&nbsp;Store");
            correctionAdd("app store", "App&nbsp;Store");
            correctionAdd("appstore", "App&nbsp;Store");
            correctionAdd("Apple store", "App&nbsp;Store"); //Not 100% correct

            correctionAdd("Antlr", "ANTLR");

            correctionAdd("ast", "AST");
            correctionAdd("abstract syntax tree", "AST"); //Going to the abbreviation
            correctionAdd("Abstract Syntax Tree", "AST"); //Going to the abbreviation

            correctionAdd("uml", "UML");

            correctionAdd("swf", "SWF"); //Usually "swf" would be replaced with "SWF file".

            correctionAdd("png", "PNG");

            correctionAdd("imagemagik", "ImageMagick");
            correctionAdd("Imagemagick", "ImageMagick");
            correctionAdd("imagemagick", "ImageMagick");
            correctionAdd("Image magick", "ImageMagick");
            correctionAdd("Imagick", "ImageMagick");
            correctionAdd("image magick", "ImageMagick");

            correctionAdd("im", "I'm"); //Collision with Instant messaging and ImageMagick.
            correctionAdd("i m", "I'm");
            correctionAdd("Im", "I'm");
            correctionAdd("i'm", "I'm");
            correctionAdd("I`m", "I'm");

            correctionAdd("compass", "Compass"); //Conflicts?

            correctionAdd("nic", "NIC");

            correctionAdd("hadoop", "Hadoop");
            correctionAdd("HADOOP", "Hadoop");

            correctionAdd("base64", "Base64");
            correctionAdd("base 64", "Base64");
            correctionAdd("Base-64", "Base64");
            correctionAdd("base46", "Base64");

            correctionAdd("WinCE", "Windows&nbsp;CE");
            correctionAdd("wince", "Windows&nbsp;CE");

            correctionAdd("rds", "Amazon RDS");

            correctionAdd("amazon ec2", "Amazon EC2");
            correctionAdd("ec2", "Amazon EC2");
            correctionAdd("EC2", "Amazon EC2");
            correctionAdd("amazon EC2", "Amazon EC2");
            correctionAdd("Ec2", "Amazon EC2");
            correctionAdd("Amazon ec2", "Amazon EC2");

            correctionAdd("amazon s3", "Amazon S3");
            correctionAdd("AmazonS3", "Amazon S3");
            correctionAdd("s3", "Amazon S3");
            correctionAdd("S3", "Amazon S3");
            correctionAdd("Amazon EC3", "Amazon S3");
            correctionAdd("Amazon s3", "Amazon S3");
            correctionAdd("AWS s3", "Amazon S3"); //S3 is more specific - is part of AWS
            correctionAdd("amazon S3", "Amazon S3");
            correctionAdd("amazon-s3", "Amazon S3");
            correctionAdd("aws-s3", "Amazon S3");
            correctionAdd("aws s3", "Amazon S3");

            correctionAdd("Amazon aws", "Amazon AWS");
            correctionAdd("aws", "Amazon AWS");
            correctionAdd("AWS", "Amazon AWS");
            correctionAdd("Amazon Web Services", "Amazon AWS");
            correctionAdd("amazon AWS", "Amazon AWS");
            correctionAdd("amazon aws", "Amazon AWS");
            correctionAdd("Aws", "Amazon AWS");

            correctionAdd("amazon", "Amazon.com"); //The name is actually Amazon.com.
            correctionAdd("Amazon", "Amazon.com");
            correctionAdd("amazon.com", "Amazon.com");

            correctionAdd("VB.Net", "VB.NET");
            correctionAdd("Vb.Net", "VB.NET");
            correctionAdd("vb.net", "VB.NET");
            correctionAdd("VB.net", "VB.NET");
            correctionAdd("Vb.net", "VB.NET");
            correctionAdd("vbnet", "VB.NET");
            correctionAdd("vb.Net", "VB.NET");
            correctionAdd("VB .Net", "VB.NET");
            correctionAdd("vb dot net", "VB.NET");
            correctionAdd("vb .net", "VB.NET");
            correctionAdd("VB.NEt", "VB.NET");
            correctionAdd("VB .net", "VB.NET");
            correctionAdd("vb.NET", "VB.NET");
            correctionAdd("Vb.NET", "VB.NET");
            correctionAdd("VBNet", "VB.NET");
            correctionAdd("Visual Basic.Net", "VB.NET");
            correctionAdd("visual basic.net", "VB.NET");

            correctionAdd("VB6", "Visual Basic 6.0");
            correctionAdd("vb6", "Visual Basic 6.0");
            correctionAdd("vb 6", "Visual Basic 6.0");
            correctionAdd("Vb6", "Visual Basic 6.0");
            correctionAdd("visual basic 6", "Visual Basic 6.0");
            correctionAdd("VB 6", "Visual Basic 6.0");
            correctionAdd("vb 6.0", "Visual Basic 6.0");
            correctionAdd("visual basic 6.0", "Visual Basic 6.0");
            correctionAdd("Visual basic 6.0", "Visual Basic 6.0");
            correctionAdd("vb6.0", "Visual Basic 6.0");
            correctionAdd("VB 6.0", "Visual Basic 6.0");
            correctionAdd("Visual Basic 6", "Visual Basic 6.0");

            correctionAdd("Vb", "Visual Basic"); //Expansion.
            correctionAdd("VB", "Visual Basic"); //Expansion. Conflict with VirtualBox.
            correctionAdd("visual basic", "Visual Basic");
            correctionAdd("vb", "Visual Basic"); //Conflicts?
            correctionAdd("Visual basic", "Visual Basic");
            correctionAdd("Visual BASIC", "Visual Basic");

            correctionAdd("Ex", "for example");
            correctionAdd("e.x", "for example"); //Specified without end full stop, to avoid the non-letter/non-digit XXX feature in this program.
            correctionAdd("eg", "for example");
            correctionAdd("Eg", "for example");
            correctionAdd("e.g", "for example"); //Specified without end full stop, to avoid the non-letter/non-digit XXX feature in this program.
            correctionAdd("E.g", "for example");
            correctionAdd("e. g", "for example"); //Does not work!!!!
            correctionAdd("E.G", "for example");
            correctionAdd("exa", "for example");
            correctionAdd("for ex", "for example"); //Specified without end full stop, to avoid the non-letter/non-digit XXX feature in this program.
            correctionAdd("ex", "for example");
            correctionAdd("f.e", "for example"); //Specified without end full stop, to avoid the non-letter/non-digit XXX feature in this program.
            correctionAdd("EG", "for example");
            correctionAdd("f.ex", "for example");
            correctionAdd("For exampe", "for example"); //Misspelling
            correctionAdd("for exemple", "for example");
            correctionAdd("For exemple", "for example");
            correctionAdd("For ex", "for example");
            correctionAdd("EX", "for example");
            correctionAdd("for e.g", "for example");
            correctionAdd("e.q", "for example"); //Later: add "e.q." when it is allowed.
            correctionAdd("for eg", "for example");
            correctionAdd("for examlpe", "for example"); //Misspelling
            correctionAdd("For examlpe", "for example"); //Misspelling
            correctionAdd("For Eg", "for example");
            correctionAdd("For eg", "for example");
            correctionAdd("For Ex", "for example");
            correctionAdd("fx", "for example");
            correctionAdd("for exapmle", "for example");  //Misspelling
            correctionAdd("For e.g", "for example");
            correctionAdd("Forexample", "for example");
            correctionAdd("forexample", "for example");
            correctionAdd("For Example", "for example");
            correctionAdd("fe", "for example");
            correctionAdd("e,g", "for example");
            correctionAdd("for Ex", "for example");
            correctionAdd("for examle", "for example");
            correctionAdd("examp", "for example");
            correctionAdd("for examp", "for example");
            correctionAdd("for excample", "for example");

            correctionAdd("Etc", "etc.");
            correctionAdd("ETC", "etc.");
            correctionAdd("et cetera", "etc.");
            correctionAdd("e.t.c", "etc.");
            correctionAdd("ets", "etc.");
            correctionAdd("ect", "etc.");
            correctionAdd("etc", "etc.");

            correctionAdd("midi", "MIDI");
            correctionAdd("Midi", "MIDI");

            correctionAdd("witch", "which");
            correctionAdd("whitch", "which");
            correctionAdd("wich", "which");
            correctionAdd("whcih", "which"); //True typo...
            correctionAdd("Whitch", "which");  //Not 100% correct - casing.
            correctionAdd("whihc", "which");

            correctionAdd("fiddler", "Fiddler");

            correctionAdd("outlook", "Outlook");

            correctionAdd("Google plus", "Google+");
            correctionAdd("Google Plus", "Google+");
            correctionAdd("google+", "Google+");
            correctionAdd("g+", "Google+");
            correctionAdd("G+", "Google+");
            correctionAdd("google +", "Google+");
            correctionAdd("google plus", "Google+");

            correctionAdd("Ncover", "NCover");

            correctionAdd("bps", "bit/s"); //Different unit.

            correctionAdd("kbps", "kbit/s"); //Different unit.
            correctionAdd("Kbps", "kbit/s"); //Different unit.
            correctionAdd("kBits/s", "kbit/s");
            correctionAdd("kbits/s", "kbit/s");
            correctionAdd("Kb/sec", "kbit/s");
            correctionAdd("kBit/s", "kbit/s");
            correctionAdd("KBPS", "kbit/s");

            correctionAdd("mbps", "Mbit/s"); //Different unit.
            correctionAdd("Mbps", "Mbit/s"); //Different unit.
            correctionAdd("MBps", "Mbit/s"); //Different unit.
            correctionAdd("mbs", "Mbit/s"); //Misspelling
            correctionAdd("MBit/s", "Mbit/s"); //Case
            correctionAdd("mbit", "Mbit/s"); //Misspelling

            correctionAdd("gbps", "Gbit/s"); //Different unit.
            correctionAdd("gbit", "Gbit/s");
            correctionAdd("Gbit", "Gbit/s");
            correctionAdd("Gbps", "Gbit/s");

            correctionAdd("KHz", "kHz"); //Spelling/case.
            correctionAdd("Khz", "kHz"); //Spelling/case.
            correctionAdd("khz", "kHz"); //Spelling/case.
            correctionAdd("KHZ", "kHz"); //Spelling/case.

            correctionAdd("Mhz", "MHz"); //Spelling.
            correctionAdd("mhz", "MHz"); //Spelling. Could be millihertz
            correctionAdd("MHZ", "MHz"); //Spelling.
            correctionAdd("mHz", "MHz"); //Spelling. Could be millihertz
            correctionAdd("Mz", "MHz");
            correctionAdd("megahert", "MHz");
            correctionAdd("megahertz", "MHz");

            correctionAdd("Ghz", "GHz"); //Spelling.
            correctionAdd("GHZ", "GHz"); //Spelling.
            correctionAdd("ghz", "GHz"); //Spelling.
            correctionAdd("gHz", "GHz"); //Spelling.
            correctionAdd("gigahert", "GHz");
            correctionAdd("gigahertz", "GHz");

            correctionAdd("hz", "Hz"); //Spelling.
            correctionAdd("Hertz", "Hz");
            correctionAdd("HZ", "Hz");

            correctionAdd("cscript", "CScript");
            correctionAdd("Cscript", "CScript");

            correctionAdd("smartsvn", "SmartSVN");

            correctionAdd("sms", "SMS");

            correctionAdd("rtmp", "RTMP");

            correctionAdd("gentoo", "Gentoo Linux");
            correctionAdd("Gentoo linux", "Gentoo Linux");

            correctionAdd("java webstart", "Java Web Start");

            correctionAdd("APACHE", "Apache HTTP Server");
            correctionAdd("apache", "Apache HTTP Server");
            correctionAdd("Appache", "Apache HTTP Server");
            correctionAdd("Apache Web Server", "Apache HTTP Server");
            correctionAdd("Apache", "Apache HTTP Server");

            correctionAdd("doxygen", "Doxygen");

            correctionAdd("oxigen", "oxygen");

            correctionAdd("graphViz", "Graphviz");
            correctionAdd("graphviz", "Graphviz");

            correctionAdd("ad", "Active Directory"); //Expansion.
            correctionAdd("AD", "Active Directory"); //Expansion.
            correctionAdd("active directory", "Active Directory");
            correctionAdd("ActiveDirectory", "Active Directory");

            correctionAdd("Ads", "advertisement"); //Not 100% correct - capitalisation and plural.
            correctionAdd("ads", "advertisement"); //Not 100% correct - capitalisation and plural.
            //correctionAdd("ad", "advertisement");  collision with Active Directory.
            correctionAdd("adverts", "advertisement"); //Not 100% correct - plural.
            correctionAdd("advert", "advertisement");
            correctionAdd("advertisment", "advertisement");

            correctionAdd("mpi", "MPI");

            correctionAdd("openmpi", "Open&nbsp;MPI");
            correctionAdd("open mpi", "Open&nbsp;MPI");
            correctionAdd("OpenMPI", "Open&nbsp;MPI");
            correctionAdd("Open MPI", "Open&nbsp;MPI");

            correctionAdd("virtual PC", "Virtual&nbsp;PC");
            correctionAdd("Virtual PC", "Virtual&nbsp;PC");
            correctionAdd("virtual pc", "Virtual&nbsp;PC");
            correctionAdd("virtualPC", "Virtual&nbsp;PC");
            //correctionAdd("VPC", "Virtual&nbsp;PC"); This is now reserved for "virtual private cloud"...

            correctionAdd("Nant", "NAnt");
            correctionAdd("nant", "NAnt");
            correctionAdd("nAnt", "NAnt");
            correctionAdd("NANT", "NAnt");

            correctionAdd("Red-gate", "Redgate Software");
            correctionAdd("Redgate", "Redgate Software");
            correctionAdd("Red Gate", "Redgate Software");
            correctionAdd("RedGate", "Redgate Software");
            correctionAdd("Red Gate Software", "Redgate Software"); // The old name...
            correctionAdd("Red-Gate", "Redgate Software");

            correctionAdd("swt", "SWT");

            correctionAdd("jetty", "Jetty");

            correctionAdd("jabber", "Jabber"); //Jabber is the former name, XMPP the new.

            correctionAdd("facebook connect", "Facebook Connect");
            correctionAdd("Facebook connect", "Facebook Connect");
            correctionAdd("FBConnect", "Facebook Connect");
            correctionAdd("FB Connect", "Facebook Connect");
            correctionAdd("fb-connect", "Facebook Connect");
            correctionAdd("fconnect", "Facebook Connect");
            correctionAdd("FB connect", "Facebook Connect");
            correctionAdd("fb connect", "Facebook Connect");

            correctionAdd("fc", "Fedora Core"); //Since Fedora 7, the Core and Extras repositories have been merged, hence the distribution dropping Core from its name.

            correctionAdd("fc 11", "Fedora Core 11"); //Since Fedora 7, the Core and Extras repositories have been merged, hence the distribution dropping Core from its name.

            correctionAdd("shiro", "Apache Shiro");

            correctionAdd("ehcache", "Ehcache");

            correctionAdd("crypto", "cryptography");
            correctionAdd("crypt", "cryptography");

            correctionAdd("dll", "DLL file"); //Expansion, as in context it is usually a file.
            correctionAdd("DLL", "DLL file"); //Expansion, as in context it is usually a file.
            correctionAdd("Dll", "DLL file"); //Expansion, as in context it is usually a file.
            correctionAdd("dll file", "DLL file");
            correctionAdd("DLLs", "DLL file");

            correctionAdd("siss", "SSIS"); //Misspelling.
            correctionAdd("ssis", "SSIS");

            correctionAdd("enterprise library", "Enterprise Library"); //Really "Microsoft Enterprise Library"

            correctionAdd("HyperV", "Hyper-V");
            correctionAdd("hyper-v", "Hyper-V");
            correctionAdd("hyperv", "Hyper-V");
            correctionAdd("hyper-V", "Hyper-V");
            correctionAdd("hyper v", "Hyper-V");
            correctionAdd("Hyper V", "Hyper-V");
            correctionAdd("Hyper-v", "Hyper-V");
            correctionAdd("hyperV", "Hyper-V");

            correctionAdd("BluRay", "Blu-ray");
            correctionAdd("blu-ray", "Blu-ray");
            correctionAdd("Blu-Ray", "Blu-ray");
            correctionAdd("blu ray", "Blu-ray");
            correctionAdd("blueray", "Blu-ray");
            correctionAdd("Blu Ray", "Blu-ray");

            correctionAdd("AV", "antivirus software");
            correctionAdd("AntiVirus", "antivirus software");
            correctionAdd("Anti Virus", "antivirus software");
            correctionAdd("Anti virus", "antivirus software");
            correctionAdd("anti virus", "antivirus software");
            correctionAdd("antivirus", "antivirus software");
            correctionAdd("Anti-virus", "antivirus software");
            correctionAdd("anti-virus", "antivirus software");
            correctionAdd("A-V", "antivirus software");
            correctionAdd("Anti virus Software", "antivirus software");
            correctionAdd("anti virus Software", "antivirus software");
            correctionAdd("Anti-Virus", "antivirus software");

            correctionAdd("x-ray", "X-ray");
            correctionAdd("X-Ray", "X-ray");
            correctionAdd("Xray", "X-ray");
            correctionAdd("xray", "X-ray");
            correctionAdd("x ray", "X-ray");
            correctionAdd("X ray", "X-ray");

            correctionAdd("BSOD", "BSoD"); //Should we expand to "Blue Screen of Death"?
            correctionAdd("bsod", "BSoD"); //Should we expand to "Blue Screen of Death"?

            correctionAdd("lynx", "Lynx");

            correctionAdd("PM", "project management"); //PM is an overloaded term...

            correctionAdd("windows update", "Windows Update");
            correctionAdd("Windows update", "Windows Update");

            correctionAdd("Tortoisesvn", "TortoiseSVN");
            correctionAdd("Tortoise svn", "TortoiseSVN");
            correctionAdd("Tortoise SVN", "TortoiseSVN");
            correctionAdd("Tortoise", "TortoiseSVN"); //Most common - could also be TortoiseCVS, TortoiseGit, TortoiseBzr and TortoiseHg.
            correctionAdd("tortise", "TortoiseSVN"); //Most common - could also be TortoiseCVS, TortoiseGit, TortoiseBzr and TortoiseHg.
            correctionAdd("tortoise", "TortoiseSVN"); //Most common - could also be TortoiseCVS, TortoiseGit, TortoiseBzr and TortoiseHg.
            //correctionAdd("tortise", "TortoiseSVN");
            correctionAdd("tortoiseSVN", "TortoiseSVN");
            correctionAdd("tortoise SVN", "TortoiseSVN");
            correctionAdd("tortoisesvn", "TortoiseSVN");
            correctionAdd("tortoise svn", "TortoiseSVN");
            correctionAdd("TSVN", "TortoiseSVN");
            correctionAdd("tsvn", "TortoiseSVN");
            correctionAdd("TSvn", "TortoiseSVN");
            correctionAdd("Tortiose SVN", "TortoiseSVN");
            correctionAdd("TortoiseSvn", "TortoiseSVN");
            correctionAdd("tortoiseSvn", "TortoiseSVN");
            correctionAdd("ToritiseSVN", "TortoiseSVN"); //Misspelling.
            correctionAdd("tortiseSVN", "TortoiseSVN");
            correctionAdd("Tortoise-SVN", "TortoiseSVN");
            correctionAdd("tortise-SVN", "TortoiseSVN");
            correctionAdd("torotoisesvn", "TortoiseSVN");
            correctionAdd("TORTOISE SVN", "TortoiseSVN");
            correctionAdd("svn tortoise", "TortoiseSVN");
            correctionAdd("tortoisSVN", "TortoiseSVN");
            correctionAdd("TortiseSVN", "TortoiseSVN");

            correctionAdd("tortoisehg", "TortoiseHg");

            correctionAdd("structure map", "StructureMap");
            correctionAdd("structuremap", "StructureMap");

            correctionAdd("apk", "APK");
            correctionAdd("Apk", "APK");

            correctionAdd("drm", "DRM");

            correctionAdd("tkinter", "Tkinter");

            correctionAdd("graph api", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("graph API", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("Graph api", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("Graph Api", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("GRAPH API", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("graphAPI", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("Graph-api", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("graph-api", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("Facebook Graph", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("Facebook Graph API", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("graph.facebook api", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("graph.facebook", "Graph API"); //Part of the Facebook Platform.
            correctionAdd("GraphAPI", "Graph API");

            correctionAdd("tortoisegit", "TortoiseGit");
            correctionAdd("Tortoisegit", "TortoiseGit");
            correctionAdd("tortoise git", "TortoiseGit");
            correctionAdd("tortisegit", "TortoiseGit");
            correctionAdd("tortoisgit", "TortoiseGit");
            correctionAdd("Tortoise Git", "TortoiseGit");
            correctionAdd("Tortoise git", "TortoiseGit");
            correctionAdd("TortoiseGIT", "TortoiseGit");
            correctionAdd("tortoise-git", "TortoiseGit");

            correctionAdd("CanOpen", "CANopen");
            correctionAdd("CANOpen", "CANopen");
            correctionAdd("canOpen", "CANopen");
            correctionAdd("canopen", "CANopen");
            correctionAdd("Canopen", "CANopen");
            correctionAdd("canopan", "CANopen");

            correctionAdd("og", "Open Graph"); //Conflicts?
            correctionAdd("open graph", "Open Graph");
            correctionAdd("OpenGraph", "Open Graph");
            correctionAdd("Open graph", "Open Graph");
            correctionAdd("Opengraph", "Open Graph");
            correctionAdd("OG", "Open Graph");

            //Can not currently be looked up due to a bug!!!!
            correctionAdd("devc++", "Dev-C++");
            correctionAdd("devcpp", "Dev-C++");
            correctionAdd("Dev-CPP", "Dev-C++");

            correctionAdd("fql", "FQL");

            correctionAdd("sth", "something");
            correctionAdd("smth", "something");
            correctionAdd("sumthin", "something");
            correctionAdd("sthg", "something");
            correctionAdd("somthing", "something");
            correctionAdd("soemthign", "something");
            correctionAdd("some thing", "something");
            correctionAdd("Soemthing", "something");
            correctionAdd("soemthing", "something");
            correctionAdd("somethign", "something");
            correctionAdd("somhing", "something");
            correctionAdd("somenting", "something");
            correctionAdd("Somting", "something");
            correctionAdd("somting", "something"); //Not 100% correct - casing.
            correctionAdd("somthig", "something");
            correctionAdd("Somthig", "something"); //Not 100% correct - casing.
            correctionAdd("somethng", "something");
            correctionAdd("somethink", "something");
            correctionAdd("somethig", "something");
            correctionAdd("someting", "something");
            correctionAdd("somethin", "something");
            correctionAdd("somithing", "something");
            correctionAdd("somethnig", "something");
            correctionAdd("sonething", "something");
            correctionAdd("Someting", "something");
            correctionAdd("sum’n", "something");
            correctionAdd("Somthing", "something");
            correctionAdd("sumthing", "something");
            correctionAdd("somerthing", "something");
            correctionAdd("seomthing", "something");

            correctionAdd("WS2003", "Windows Server 2003");
            correctionAdd("Server 2003", "Windows Server 2003");
            correctionAdd("server 2003", "Windows Server 2003");
            correctionAdd("windows 2003 server", "Windows Server 2003");
            correctionAdd("2k3", "Windows Server 2003");
            correctionAdd("win2003", "Windows Server 2003");
            correctionAdd("Windows 2003", "Windows Server 2003");
            correctionAdd("2003", "Windows Server 2003");
            correctionAdd("windows server 2003", "Windows Server 2003");
            correctionAdd("win 2003", "Windows Server 2003");
            correctionAdd("WIndows 2003 server", "Windows Server 2003");
            correctionAdd("Windows server 2003", "Windows Server 2003");
            correctionAdd("WIndows2003", "Windows Server 2003");
            correctionAdd("Windows2003", "Windows Server 2003");
            correctionAdd("WIndows2003 server", "Windows Server 2003");
            correctionAdd("windows 2003", "Windows Server 2003");
            correctionAdd("Windows 2003 Server", "Windows Server 2003");
            correctionAdd("Win2003", "Windows Server 2003");
            correctionAdd("Win2k3", "Windows Server 2003");
            correctionAdd("win2k3", "Windows Server 2003");
            correctionAdd("win 2k3", "Windows Server 2003");
            correctionAdd("Srv2003", "Windows Server 2003");
            correctionAdd("Srv 2003", "Windows Server 2003");

            correctionAdd("exchange", "Microsoft Exchange Server");
            correctionAdd("Exchange", "Microsoft Exchange Server");

            correctionAdd("exchange 2003 server", "Exchange Server 2003");
            correctionAdd("exchange server 2003", "Exchange Server 2003");

            correctionAdd("google calendar", "Google Calendar");

            correctionAdd("taskmgr", "Task Manager");
            correctionAdd("task manager", "Task Manager");
            correctionAdd("taskmanager", "Task Manager");
            correctionAdd("Task manager", "Task Manager");
            correctionAdd("TaskManager", "Task Manager");

            correctionAdd("SysInternals", "Sysinternals");
            correctionAdd("Sys Internals", "Sysinternals");
            correctionAdd("sysInternals", "Sysinternals");
            correctionAdd("sysinternals", "Sysinternals");

            correctionAdd("unc", "UNC");

            correctionAdd("rf", "RF");

            correctionAdd("lob", "LOB");

            correctionAdd("SQL Server Reporting Services", "SQL Server Reporting Services (SSRS)");
            correctionAdd("ssrs", "SQL Server Reporting Services (SSRS)");
            correctionAdd("SSRS", "SQL Server Reporting Services (SSRS)");

            correctionAdd("tls", "TLS");

            correctionAdd("pojo", "POJO");
            correctionAdd("Pojo", "POJO");

            correctionAdd("hql", "HQL");

            correctionAdd("perforce", "Perforce");

            correctionAdd("bazaar", "Bazaar");
            correctionAdd("Bzr", "Bazaar");
            correctionAdd("bzr", "Bazaar");

            correctionAdd("mfc", "MFC");
            correctionAdd("Mfc", "MFC");

            correctionAdd("lamp", "LAMP");
            correctionAdd("Lamp", "LAMP");

            correctionAdd("Fisheye", "FishEye");
            correctionAdd("fisheye", "FishEye");

            correctionAdd("CRON", "cron");

            correctionAdd("celery", "Celery");

            correctionAdd("archlinux", "Arch Linux");
            correctionAdd("Archlinux", "Arch Linux");
            correctionAdd("ArchLinux", "Arch Linux");
            correctionAdd("arch linux", "Arch Linux");
            correctionAdd("arch Linux", "Arch Linux");
            correctionAdd("archLinux", "Arch Linux");
            correctionAdd("Arch", "Arch Linux");
            correctionAdd("Arch linux", "Arch Linux");

            correctionAdd("pacman", "Pacman");

            correctionAdd("grepp", "grep");

            correctionAdd("arch", "architecture"); //Expansion. For a computer hardware context.
            correctionAdd("archetecture", "architecture");
            correctionAdd("archtecture", "architecture");

            correctionAdd("wxython", "wxPython");
            correctionAdd("wxpython", "wxPython");
            correctionAdd("wx", "wxPython");
            correctionAdd("wpython", "wxPython");
            correctionAdd("WxPython", "wxPython");

            correctionAdd("stl", "STL");
            correctionAdd("Stl", "STL");

            correctionAdd("dalvik", "Dalvik");
            correctionAdd("davlik", "Dalvik"); //Misspelling.
            correctionAdd("DALVIK", "Dalvik");
            correctionAdd("Dalivk", "Dalvik"); //Misspelling.
            correctionAdd("Davlik", "Dalvik");

            correctionAdd("Tracert", "tracert");

            correctionAdd("dtrace", "DTrace");
            correctionAdd("Dtrace", "DTrace");

            correctionAdd("watin", "WatiN");
            correctionAdd("Watin", "WatiN");

            correctionAdd("device manager", "Device Manager");
            correctionAdd("Device manager", "Device Manager");
            correctionAdd("Devices-Manager", "Device Manager");
            correctionAdd("Devices Manager", "Device Manager");
            correctionAdd("Devices manager", "Device Manager");
            correctionAdd("devices manager", "Device Manager");
            correctionAdd("device managment", "Device Manager");
            correctionAdd("device maanger", "Device Manager");
            correctionAdd("Dev Mgr", "Device Manager");
            correctionAdd("dev mgr", "Device Manager");
            correctionAdd("Device Mangager", "Device Manager");

            correctionAdd("dupe", "duplicate");
            correctionAdd("dup", "duplicate");
            correctionAdd("dupplicate", "duplicate");
            correctionAdd("dupilicate", "duplicate");
            correctionAdd("douplicate", "duplicate");

            correctionAdd("stackapps", "Stack&nbsp;Apps");
            correctionAdd("stack apps", "Stack&nbsp;Apps");
            correctionAdd("Stack Apps", "Stack&nbsp;Apps");
            correctionAdd("StackApps", "Stack&nbsp;Apps");

            correctionAdd("execjs", "ExecJS");

            correctionAdd("ruby racer", "Ruby Racer");
            correctionAdd("rubyracer", "Ruby Racer");

            correctionAdd("jvm", "JVM");
            correctionAdd("VM", "JVM"); //Note: ambiguity with general virtual machines - should we add an entry for that instead???
            correctionAdd("vm", "JVM"); //Note: ambiguity with general virtual machines - should we add an entry for that instead???
            correctionAdd("Java Virtual Machine", "JVM"); //Note: Perhaps separate, "Java virtual machine"?


            correctionAdd("Truecrypt", "TrueCrypt");
            correctionAdd("truecrypt", "TrueCrypt");
            correctionAdd("true crypt", "TrueCrypt");

            correctionAdd("versions", "Versions");

            correctionAdd("jaws", "JAWS");

            correctionAdd("symlink", "symbolic link");
            correctionAdd("symlinked", "symbolic link"); //Not 100% correct - verbified.

            correctionAdd("adt", "ADT");

            correctionAdd("pdb", "PDB");

            correctionAdd("env", "environment");
            correctionAdd("evironment", "environment");
            correctionAdd("ENV", "environment");
            correctionAdd("environement", "environment");
            correctionAdd("enviroment", "environment");
            correctionAdd("environnement", "environment");
            correctionAdd("enviorenment", "environment");
            correctionAdd("env variable", "environment");
            correctionAdd("enviorment", "environment");
            correctionAdd("enviromnment", "environment");
            correctionAdd("environmanet", "environment");

            correctionAdd("lan", "LAN");

            correctionAdd("javadoc", "Javadoc");

            correctionAdd("usenet", "Usenet");
            correctionAdd("USENET", "Usenet");

            correctionAdd("aka", "AKA");
            correctionAdd("a.k.a", "AKA"); //Workaround for the cut-off-period at the end bug...
            //correctionAdd("a.k.a.", "AKA"); //Does not currently work due to the cut-off-period at the end bug.
            correctionAdd("Aka", "AKA");

            correctionAdd("Sketch", "sketch");
            correctionAdd("skectch", "sketch");

            correctionAdd("POD", "plain old data structure");
            correctionAdd("pod", "plain old data structure");

            correctionAdd("fk", "foreign key");
            correctionAdd("FK", "foreign key");
            correctionAdd("foreignkey", "foreign key");
            correctionAdd("foreingkey", "foreign key");
            correctionAdd("Foreing Key", "foreign key");
            correctionAdd("foreing key", "foreign key");
            correctionAdd("foreign Key", "foreign key");

            correctionAdd("PK", "primary key");
            correctionAdd("pK", "primary key");
            correctionAdd("pk", "primary key");
            correctionAdd("Primary Key", "primary key");
            correctionAdd("PRAMARY KEY", "primary key");
            correctionAdd("pramary key", "primary key");
            correctionAdd("PRIMARY KEY", "primary key");

            correctionAdd("pwm", "PWM");

            correctionAdd("crud", "CRUD");

            correctionAdd("pdo", "PDO"); // Consider alternative URL <http://php.net/manual/en/intro.pdo.php>

            correctionAdd("db2", "DB2");

            correctionAdd("mariadb", "MariaDB");
            correctionAdd("Maria DB", "MariaDB");

            correctionAdd("led", "LED");
            correctionAdd("Led", "LED");

            correctionAdd("experts exchange", "Experts-Exchange");
            correctionAdd("EE", "Experts-Exchange");
            correctionAdd("Experts Exchange", "Experts-Exchange");
            correctionAdd("expertsexchange", "Experts-Exchange");
            correctionAdd("Experts exchange", "Experts-Exchange");
            correctionAdd("expertexchange", "Experts-Exchange");
            correctionAdd("experts-exchange", "Experts-Exchange");
            correctionAdd("hyphen site", "Experts-Exchange");
            correctionAdd("the hyphen site", "Experts-Exchange");

            correctionAdd("asus", "Asus");
            correctionAdd("ASUS", "Asus");

            correctionAdd("captcha", "CAPTCHA");
            correctionAdd("catpcha", "CAPTCHA");
            correctionAdd("Captcha", "CAPTCHA");
            correctionAdd("capthca", "CAPTCHA");

            correctionAdd("Recaptcha", "reCAPTCHA");
            correctionAdd("reCaptcha", "reCAPTCHA");
            correctionAdd("recaptcha", "reCAPTCHA");
            correctionAdd("recpatcha", "reCAPTCHA");
            correctionAdd("ReCaptcha", "reCAPTCHA");
            correctionAdd("re-captcha", "reCAPTCHA");
            correctionAdd("reCaptuav", "reCAPTCHA");
            correctionAdd("RECAPTCHA", "reCAPTCHA");

            correctionAdd("Spinrite", "SpinRite");
            correctionAdd("spinrite", "SpinRite");
            correctionAdd("Spinright", "SpinRite");

            correctionAdd("lat", "latitude"); //Expansion.
            correctionAdd("Lat", "latitude"); //Expansion.

            correctionAdd("long", "longitude"); //Expansion.
            correctionAdd("Long", "longitude"); //Expansion.
            correctionAdd("lng", "longitude"); //Expansion.
            correctionAdd("lon", "longitude");

            correctionAdd("ipod", "iPod");
            correctionAdd("Ipod", "iPod");

            correctionAdd("Windows 7 starter", "Windows&nbsp;7 Starter");
            correctionAdd("Windows 7 Starter", "Windows&nbsp;7 Starter");

            correctionAdd("firewire", "FireWire");
            correctionAdd("Firewire", "FireWire");

            correctionAdd("WinAMP", "Winamp");
            correctionAdd("winamp", "Winamp");

            correctionAdd("Windows media player", "Windows Media Player");
            correctionAdd("WMP", "Windows Media Player");
            correctionAdd("windows media player", "Windows Media Player");
            correctionAdd("media player", "Windows Media Player"); //Unfortunate abbreviation.
            correctionAdd("wmp", "Windows Media Player");
            correctionAdd("mediaplayer", "Windows Media Player"); //Unfortunate abbreviation, e.g. as in <http://superuser.com/questions/228979>.

            correctionAdd("applescript", "AppleScript");
            correctionAdd("Applescript", "AppleScript");
            correctionAdd("apple script", "AppleScript");
            correctionAdd("Apple Script", "AppleScript");

            correctionAdd("Mathjax", "MathJax");
            correctionAdd("mathjax", "MathJax");
            correctionAdd("MatJaX", "MathJax");
            correctionAdd("MathJaX", "MathJax");
            correctionAdd("MathJAX", "MathJax");

            correctionAdd("notepad2", "Notepad2");

            correctionAdd("solution explorer", "Solution Explorer");
            correctionAdd("SolutionExplorer", "Solution Explorer");
            correctionAdd("solution expolorer", "Solution Explorer"); //Misspelling.

            correctionAdd("RSYNC", "rsync");
            correctionAdd("Rsync", "rsync");

            correctionAdd("scp", "SCP");

            correctionAdd("cython", "Cython");

            correctionAdd("RockBox", "Rockbox");

            correctionAdd("selenium", "Selenium");
            correctionAdd("seleium", "Selenium");

            correctionAdd("ethernet", "Ethernet");
            correctionAdd("ETHERNET", "Ethernet");
            correctionAdd("ehternet", "Ethernet");
            correctionAdd("eth", "Ethernet");

            correctionAdd("f#", "F#");

            correctionAdd("c", "C"); //Possible conflicts!!!

            correctionAdd("ScottGu", "Scott Guthrie");

            correctionAdd("brief", "Brief");

            correctionAdd("core 2 duo", "Core 2 Duo");

            correctionAdd("rss", "RSS");

            correctionAdd("combo", "combination"); //combo is slang - <http://en.wiktionary.orgz/wiki/combo#Noun>.
            correctionAdd("comb", "combination");

            correctionAdd("IPV6", "IPv6");
            correctionAdd("ipv6", "IPv6");
            correctionAdd("IP6", "IPv6");
            correctionAdd("inet6", "IPv6");

            correctionAdd("ip4", "IPv4");
            correctionAdd("IPV4", "IPv4");
            correctionAdd("ipv4", "IPv4");
            correctionAdd("IP4", "IPv4");
            correctionAdd("V4", "IPv4"); //Collision?
            correctionAdd("inet4", "IPv4");
            correctionAdd("IP_V4", "IPv4");

            correctionAdd("sax", "SAX");

            correctionAdd("codeplex", "CodePlex");
            correctionAdd("Codeplex", "CodePlex");

            correctionAdd("aol", "AOL");
            correctionAdd("Aol", "AOL");

            correctionAdd("benifit", "benefit");
            correctionAdd("benift", "benefit");

            correctionAdd("TestDriven.Net", "TestDriven.NET");
            correctionAdd("TD.NET", "TestDriven.NET");
            correctionAdd("TestDriven.net", "TestDriven.NET");
            //correctionAdd("TestDriven.Net", "TestDriven.NET");

            correctionAdd("knockout", "KnockoutJS");
            correctionAdd("Knockout.js", "KnockoutJS");
            correctionAdd("'Knockout'.js", "KnockoutJS");
            correctionAdd("knockoutjs", "KnockoutJS");
            correctionAdd("Knockout", "KnockoutJS");
            correctionAdd("KO", "KnockoutJS");
            correctionAdd("knockout js", "KnockoutJS");
            correctionAdd("knockout.js", "KnockoutJS");
            correctionAdd("knockoutJS", "KnockoutJS");

            correctionAdd("underscore", "Underscore.js");
            correctionAdd("Underscore", "Underscore.js");
            correctionAdd("underscore.js", "Underscore.js");
            correctionAdd("underscore js", "Underscore.js");
            correctionAdd("underscorejs", "Underscore.js");
            correctionAdd("underscoreJS", "Underscore.js");
            correctionAdd("Underscore JS", "Underscore.js");

            correctionAdd("dll hell", "DLL&nbsp;Hell");

            //Now used for PowerShell:
            //
            //correctionAdd("p.s", "PS"); //Conflict with PowerShell.
            //correctionAdd("P/S", "PS"); //Conflict with PowerShell.
            //correctionAdd("P.s", "PS"); //Conflict with PowerShell. Really "P.s."

            correctionAdd("qemu", "QEMU");
            correctionAdd("QEmu", "QEMU");
            correctionAdd("Qemu", "QEMU");
            correctionAdd("qEmu", "QEMU");

            correctionAdd("myspace", "Myspace");
            correctionAdd("MySpace", "Myspace");

            correctionAdd("gnu", "GNU");
            correctionAdd("Gnu", "GNU");

            correctionAdd("Jeff", "Jeff Atwood");

            correctionAdd("Joel", "Joel Spolsky");

            correctionAdd("Yslow", "YSlow");
            correctionAdd("yslow", "YSlow");

            correctionAdd("Clickonce", "ClickOnce");
            correctionAdd("clickonce", "ClickOnce");
            correctionAdd("click once", "ClickOnce");
            correctionAdd("click-once", "ClickOnce");
            correctionAdd("clickOnce", "ClickOnce");
            correctionAdd("Click Once", "ClickOnce");
            correctionAdd("Click-Once", "ClickOnce");
            correctionAdd("clickone", "ClickOnce");
            correctionAdd("OneClick", "ClickOnce");
            correctionAdd("Click once", "ClickOnce");
            correctionAdd("Click-once", "ClickOnce");
            correctionAdd("ClckOnce", "ClickOnce"); //Misspelling.
            correctionAdd("CO", "ClickOnce");
            correctionAdd("one click", "ClickOnce");
            correctionAdd("cliconce", "ClickOnce");
            correctionAdd("ClicOnse", "ClickOnce");

            correctionAdd("Ccleaner", "CCleaner");
            correctionAdd("ccleaner", "CCleaner");

            correctionAdd("isp", "ISP");

            correctionAdd("KATE", "Kate");
            correctionAdd("kate", "Kate");

            correctionAdd("remote desktop", "Remote Desktop Connection");
            correctionAdd("Remote Desktop", "Remote Desktop Connection");
            correctionAdd("Remote desktop", "Remote Desktop Connection");

            correctionAdd("pic", "PIC");
            correctionAdd("Pic", "PIC");

            correctionAdd("WS2008", "Windows Server 2008");
            correctionAdd("windows 2008 server", "Windows Server 2008");
            correctionAdd("windows server 2008", "Windows Server 2008");
            correctionAdd("win 2008 server", "Windows Server 2008");
            correctionAdd("2k8", "Windows Server 2008"); //Could also be Visual Studio 2008...
            correctionAdd("Win 2k8", "Windows Server 2008");
            correctionAdd("W2K8", "Windows Server 2008");
            correctionAdd("Server 2008", "Windows Server 2008");
            correctionAdd("Win 2008", "Windows Server 2008");
            correctionAdd("ws2008", "Windows Server 2008");
            correctionAdd("server 2008", "Windows Server 2008");
            correctionAdd("Win Server 2008", "Windows Server 2008");
            correctionAdd("win 2008", "Windows Server 2008");
            correctionAdd("Windows 2008", "Windows Server 2008");
            correctionAdd("windows 2008", "Windows Server 2008");
            correctionAdd("win2k8", "Windows Server 2008");
            correctionAdd("Win2008", "Windows Server 2008");
            correctionAdd("Win2008 Server", "Windows Server 2008");
            correctionAdd("Sever 2008", "Windows Server 2008"); //Misspelling, etc.
            correctionAdd("Windows 2008 Server", "Windows Server 2008");
            correctionAdd("2008", "Windows Server 2008"); //Could also be SQL Server 2008, Visual Studio 2008 or even Visual Basic 2008 Express Edition...
            correctionAdd("windows 2008 Server", "Windows Server 2008");
            correctionAdd("Windows 2k8", "Windows Server 2008");
            correctionAdd("Windows server 2008", "Windows Server 2008");
            correctionAdd("Windows2k8", "Windows Server 2008");
            correctionAdd("Win2k8", "Windows Server 2008");
            correctionAdd("W2008", "Windows Server 2008");

            correctionAdd("windows server 2012", "Windows Server 2012");
            correctionAdd("WS 2012", "Windows Server 2012");
            correctionAdd("2012", "Windows Server 2012");
            correctionAdd("Windows 2012", "Windows Server 2012");
            correctionAdd("server 2012", "Windows Server 2012");
            correctionAdd("Server 2012", "Windows Server 2012"); //Could also be SQL Server 2012...
            correctionAdd("windows sever 2012", "Windows Server 2012");
            correctionAdd("windows 2012", "Windows Server 2012");
            correctionAdd("2012 Server", "Windows Server 2012");
            correctionAdd("win 2012", "Windows Server 2012");
            correctionAdd("win2012", "Windows Server 2012");
            correctionAdd("W2K12", "Windows Server 2012");
            correctionAdd("Server2012", "Windows Server 2012");
            correctionAdd("windows server 12", "Windows Server 2012");
            correctionAdd("server 12", "Windows Server 2012");
            correctionAdd("server2012", "Windows Server 2012");
            correctionAdd("Win 2012 server", "Windows Server 2012");
            correctionAdd("Win 2012", "Windows Server 2012");
            correctionAdd("Win2012", "Windows Server 2012");
            correctionAdd("Win Server 2012", "Windows Server 2012");
            correctionAdd("Windows 2k12", "Windows Server 2012");
            correctionAdd("Windows 2K12", "Windows Server 2012");

            correctionAdd("windows live", "Windows Live");

            correctionAdd("BeyondCompare", "Beyond Compare");
            correctionAdd("beyondcompare", "Beyond Compare");
            correctionAdd("BC", "Beyond Compare"); //Possibly others!
            correctionAdd("beyond compare", "Beyond Compare");

            correctionAdd("rdp", "RDP");

            correctionAdd("kvm", "KVM"); //Conflict with KVM switch, "keyboard, visual display unit, mouse", <http://en.wikipedia.org/wiki/KVM_switch>. ALTERNATIVE.
            correctionAdd("KVm", "KVM");

            correctionAdd("Teamviewer", "TeamViewer");
            correctionAdd("teamviewer", "TeamViewer");
            correctionAdd("team viewer", "TeamViewer");
            correctionAdd("Team Viewer", "TeamViewer");

            correctionAdd("craigslist", "Craigslist");
            correctionAdd("craiglist", "Craigslist");

            correctionAdd("NOsql", "NoSQL");
            correctionAdd("NoSql", "NoSQL");
            correctionAdd("NOSQL", "NoSQL");
            correctionAdd("nosql", "NoSQL");
            correctionAdd("noSQL", "NoSQL");
            correctionAdd("no sql", "NoSQL");
            correctionAdd("no-sql", "NoSQL");

            correctionAdd("mplayer", "MPlayer");
            correctionAdd("Mplayer", "MPlayer");

            correctionAdd("iPod touch", "iPod Touch");
            correctionAdd("ipod touch", "iPod Touch");

            correctionAdd("mic", "microphone");

            correctionAdd("Truetype", "TrueType");

            correctionAdd("ExtJS", "Ext&nbsp;JS");
            correctionAdd("Ext JS", "Ext&nbsp;JS");
            correctionAdd("ExtJs", "Ext&nbsp;JS");
            correctionAdd("extjs", "Ext&nbsp;JS");
            correctionAdd("ext", "Ext&nbsp;JS");
            correctionAdd("Ext", "Ext&nbsp;JS");
            correctionAdd("ext-js", "Ext&nbsp;JS");
            correctionAdd("Ext Js", "Ext&nbsp;JS");
            correctionAdd("EXT JS", "Ext&nbsp;JS");
            correctionAdd("Ext-JS", "Ext&nbsp;JS");
            correctionAdd("extJS", "Ext&nbsp;JS");
            correctionAdd("Extjs", "Ext&nbsp;JS");
            correctionAdd("etxjs", "Ext&nbsp;JS");

            correctionAdd("Openstreetmap", "OpenStreetMap");
            correctionAdd("openstreetmap", "OpenStreetMap");
            correctionAdd("OSM", "OpenStreetMap");
            correctionAdd("Open Street Maps", "OpenStreetMap");
            correctionAdd("open street map", "OpenStreetMap");
            correctionAdd("OpenStreetMaps", "OpenStreetMap");

            correctionAdd("winrar", "WinRAR");
            correctionAdd("WinRar", "WinRAR");
            correctionAdd("WirRAR", "WinRAR");
            correctionAdd("Winrar", "WinRAR");

            correctionAdd("Rar", "RAR");
            correctionAdd("rar", "RAR");

            correctionAdd("arm", "ARM");
            correctionAdd("Arm", "ARM");

            correctionAdd("ANN", "artificial neural network"); //Expansion
            correctionAdd("Neural Network", "artificial neural network");
            correctionAdd("neural network", "artificial neural network");

            correctionAdd("Divx", "DivX");
            correctionAdd("divx", "DivX");

            correctionAdd("wether", "whether");
            correctionAdd("weather", "whether"); //Unless it is really about rain, clouds, and temperature.
            correctionAdd("wheter", "whether");
            correctionAdd("wheather", "whether"); // Stuf with rain, snow, etc. is weather.
            correctionAdd("whhethher", "whether");
            correctionAdd("whethee", "whether");

            // Consider:
            //   weaher
            //   <https://en.wiktionary.org/wiki/weather#Noun>


            correctionAdd("dragonfly", "Dragonfly");

            correctionAdd("i2c", "I²C");
            correctionAdd("I2C", "I²C");
            correctionAdd("i2C", "I²C");
            correctionAdd("I2c", "I²C");

            correctionAdd("sdl", "SDL");

            correctionAdd("cobol", "COBOL");
            correctionAdd("Cobol", "COBOL");

            correctionAdd("FORTRAN", "Fortran");
            correctionAdd("fortran", "Fortran");
            correctionAdd("fotran", "Fortran");

            correctionAdd("gof", "Design Patterns: Elements of Reusable Object-Oriented Software (GoF book)");
            correctionAdd("Gof", "Design Patterns: Elements of Reusable Object-Oriented Software (GoF book)");
            correctionAdd("GoF", "Design Patterns: Elements of Reusable Object-Oriented Software (GoF book)"); // Effectively self.
            correctionAdd("Gang of Four", "Design Patterns: Elements of Reusable Object-Oriented Software (GoF book)");
            correctionAdd("GOF", "Design Patterns: Elements of Reusable Object-Oriented Software (GoF book)");

            correctionAdd("basic", "BASIC");
            correctionAdd("Basic", "BASIC");

            correctionAdd("DC", "domain controller"); //Expansion. Direct collision with direct current.
            correctionAdd("dc", "domain controller"); //Expansion
            correctionAdd("Domain Controller", "domain controller");
            correctionAdd("Domain controller", "domain controller");
            correctionAdd("d/c", "domain controller");

            correctionAdd("ghostscript", "Ghostscript");

            correctionAdd("Power Grep", "PowerGrep");

            correctionAdd("razor", "Razor");

            correctionAdd("winmerge", "WinMerge");
            correctionAdd("Winmerge", "WinMerge");

            correctionAdd("imgur", "Imgur");

            correctionAdd("gimp", "GIMP");
            correctionAdd("Gimp", "GIMP");

            correctionAdd("jtag", "JTAG");
            correctionAdd("Jtag", "JTAG");

            correctionAdd("Uart", "UART");
            correctionAdd("uart", "UART");

            correctionAdd("ive", "I’ve");
            correctionAdd("Ive", "I’ve");
            correctionAdd("Iv'e", "I’ve");
            correctionAdd("I've", "I’ve");

            correctionAdd("foursquare", "Foursquare");
            correctionAdd("Four square", "Foursquare");

            correctionAdd("Google code", "Google Code");
            correctionAdd("google code", "Google Code");
            correctionAdd("google.code", "Google Code");
            correctionAdd("googlecode", "Google Code");

            correctionAdd("WOW", "WoW"); //Windows on Window, but collision with World of Wordcraft...

            correctionAdd("XARGS", "xargs");

            correctionAdd("Find", "find");

            correctionAdd("moonlight", "Moonlight");

            correctionAdd("Qmake", "qmake");
            correctionAdd("QMake", "qmake");

            correctionAdd("Cmake", "CMake");
            correctionAdd("cmake", "CMake");
            correctionAdd("CMAKE", "CMake");

            correctionAdd("make", "Make");

            correctionAdd("Raspberry PI", "Raspberry Pi");
            correctionAdd("Raspberry pi", "Raspberry Pi");
            correctionAdd("raspberry Pi", "Raspberry Pi");
            correctionAdd("raspberrypi", "Raspberry Pi");
            correctionAdd("raspberry pi", "Raspberry Pi");
            correctionAdd("Rasberry Pi", "Raspberry Pi");
            correctionAdd("raspberri pi", "Raspberry Pi");
            correctionAdd("raspberry PI", "Raspberry Pi");
            correctionAdd("rasberry pi", "Raspberry Pi");
            correctionAdd("Raspberry pie", "Raspberry Pi");
            correctionAdd("RaspberryPi", "Raspberry Pi");
            correctionAdd("raspberry", "Raspberry Pi");
            correctionAdd("RaspberryPI", "Raspberry Pi");
            correctionAdd("raspbery pi", "Raspberry Pi"); //Misspelling.
            correctionAdd("PI", "Raspberry Pi");
            correctionAdd("pi", "Raspberry Pi");
            correctionAdd("Pi", "Raspberry Pi");
            correctionAdd("Rpi", "Raspberry Pi");
            correctionAdd("R-Pi", "Raspberry Pi");
            correctionAdd("RaPi", "Raspberry Pi");
            correctionAdd("RPi", "Raspberry Pi");
            correctionAdd("rasp", "Raspberry Pi");
            correctionAdd("raspi", "Raspberry Pi");
            correctionAdd("RasPi", "Raspberry Pi");
            correctionAdd("Raspi", "Raspberry Pi");
            correctionAdd("rpi", "Raspberry Pi");
            correctionAdd("Raspberry", "Raspberry Pi");
            correctionAdd("Rasp Pi", "Raspberry Pi");
            correctionAdd("RPI", "Raspberry Pi");
            correctionAdd("r pi", "Raspberry Pi");
            correctionAdd("raspberryPi", "Raspberry Pi");
            correctionAdd("RP", "Raspberry Pi");
            correctionAdd("raspberry-pi", "Raspberry Pi");
            correctionAdd("Razberrypi", "Raspberry Pi");
            correctionAdd("razberrypi", "Raspberry Pi");
            correctionAdd("rasperry pi", "Raspberry Pi");
            correctionAdd("rasbperry pi", "Raspberry Pi");
            correctionAdd("rasbperry", "Raspberry Pi");
            correctionAdd("Rasperry", "Raspberry Pi");
            correctionAdd("Rasberry PI", "Raspberry Pi");
            correctionAdd("RASPI", "Raspberry Pi");
            correctionAdd("Rasberry-Pi", "Raspberry Pi");
            correctionAdd("Raspbeery Pi", "Raspberry Pi");
            correctionAdd("Ras-Pi", "Raspberry Pi");
            correctionAdd("Raspberry-pi", "Raspberry Pi");
            correctionAdd("RaspBerry Pi", "Raspberry Pi");
            correctionAdd("rPi", "Raspberry Pi");
            correctionAdd("rasPi", "Raspberry Pi");
            correctionAdd("rasp pi", "Raspberry Pi");
            correctionAdd("r-pi", "Raspberry Pi");
            correctionAdd("Rasbperry Pi", "Raspberry Pi");
            correctionAdd("Rapsberry Pi", "Raspberry Pi");
            correctionAdd("Rasberry Py", "Raspberry Pi");
            correctionAdd("RasperryPi", "Raspberry Pi");
            correctionAdd("Rapberry pi", "Raspberry Pi");
            correctionAdd("rasperri pi", "Raspberry Pi");
            correctionAdd("raSPI", "Raspberry Pi");
            correctionAdd("Rasperry Pi", "Raspberry Pi");
            correctionAdd("Rasberry", "Raspberry Pi");
            correctionAdd("RaspBerryPi", "Raspberry Pi");
            correctionAdd("Rapserry pi", "Raspberry Pi");

            correctionAdd("gps", "GPS");
            correctionAdd("Gps", "GPS");

            correctionAdd("bmp", "BMP");

            correctionAdd("nuget", "NuGet");
            correctionAdd("nuGet", "NuGet");
            correctionAdd("Nuget", "NuGet");
            correctionAdd("nugget", "NuGet");
            correctionAdd("NUGET", "NuGet");
            correctionAdd("nu-get", "NuGet");

            correctionAdd("cmd", "Command&nbsp;Prompt");
            correctionAdd("cmd.exe", "Command&nbsp;Prompt");

            correctionAdd("Disk cleanup", "Disk Cleanup");
            correctionAdd("Disk Cleaner", "Disk Cleanup"); //Misremembered name...

            //correctionAdd("ansi", "XXX    //Too many meanings? ANSI esacpe sequence, etc.

            correctionAdd("Cyanogenmod", "CyanogenMod");
            correctionAdd("Cyanogen Mod", "CyanogenMod");
            correctionAdd("CM", "CyanogenMod"); //Expansion. Collisions?
            correctionAdd("cyanogenmod", "CyanogenMod");

            correctionAdd("adb", "ADB");
            correctionAdd("Adb", "ADB");

            correctionAdd("Kies", "Samsung Kies");
            correctionAdd("kies", "Samsung Kies");
            correctionAdd("Samsung kies", "Samsung Kies");

            correctionAdd("ffmpeg", "FFmpeg");
            correctionAdd("FFMPEG", "FFmpeg");
            correctionAdd("Ffmpeg", "FFmpeg");

            correctionAdd("knoppix", "Knoppix");

            correctionAdd("cpan", "CPAN");

            correctionAdd("strawberry perl", "Strawberry Perl");
            correctionAdd("Strawberry perl", "Strawberry Perl");
            correctionAdd("Strawberry", "Strawberry Perl");

            correctionAdd("ntfs", "NTFS");
            correctionAdd("NFTS", "NTFS");

            correctionAdd("rfid", "RFID");

            correctionAdd("google earth", "Google Earth");

            correctionAdd("wimax", "WiMAX");

            correctionAdd("filezilla", "FileZilla");
            correctionAdd("Filezilla", "FileZilla");
            correctionAdd("file zilla", "FileZilla");
            correctionAdd("fileZilla", "FileZilla");

            correctionAdd("spi", "SPI");

            correctionAdd("ftp", "FTP");

            correctionAdd("Avi", "AVI");
            correctionAdd("avi", "AVI");

            correctionAdd("Javabean", "JavaBeans");
            correctionAdd("Javabeans", "JavaBeans");
            correctionAdd("JavaBean", "JavaBeans");
            correctionAdd("javabean", "JavaBeans");

            correctionAdd("hosts", "hosts file");

            correctionAdd("chkdsk", "CHKDSK");
            correctionAdd("ChkDsk", "CHKDSK");
            correctionAdd("Chkdsk", "CHKDSK");

            correctionAdd("mft", "MFT");

            correctionAdd("irc", "IRC");

            correctionAdd("telnet", "Telnet");

            correctionAdd("Autohotkey", "AutoHotkey");
            correctionAdd("AHK", "AutoHotkey");
            correctionAdd("AutoHotKey", "AutoHotkey");
            correctionAdd("autohotkey", "AutoHotkey");
            correctionAdd("Auto Hot Key", "AutoHotkey");
            correctionAdd("AutoHotKeys", "AutoHotkey");
            correctionAdd("autohotkeys", "AutoHotkey");

            // What about SQL import, with the single quotes?
            correctionAdd("Like button", "'like button'");
            //correctionAdd("like", "'like button'"); // Potential conflict... - we have
                                                    // We have a corrected term, "like"...
                                                    // E.g. we can't get the URL for
                                                    // "like" as it maps to "'like button'".
                                                    //
                                                    // In fact, our check at SQL export
                                                    // time complains about this, so
                                                    // we have disabled it for now.
            correctionAdd("Like", "'like button'");

            correctionAdd("Shield", "shield"); //As in Arduino...
            correctionAdd("SHEILD", "shield"); //As in Arduino...

            correctionAdd("freemat", "FreeMat");

            correctionAdd("efi", "EFI");
            correctionAdd("Efi", "EFI");

            correctionAdd("logcat", "LogCat");
            correctionAdd("Logcat", "LogCat");
            correctionAdd("logCat", "LogCat");
            correctionAdd("Log cat", "LogCat");
            correctionAdd("log cat", "LogCat");
            correctionAdd("LOGCAT", "LogCat");

            correctionAdd("b/w", "between");
            correctionAdd("beetween", "between");
            correctionAdd("beteen", "between");

            correctionAdd("Corba", "CORBA");
            correctionAdd("corba", "CORBA");

            correctionAdd("dependency walker", "Dependency Walker");
            correctionAdd("Dependency Wolker", "Dependency Walker");
            correctionAdd("Dependency walker", "Dependency Walker");
            correctionAdd("dependencywalker", "Dependency Walker");

            correctionAdd("photoshop", "Photoshop");
            correctionAdd("Adobe Photoshop", "Photoshop"); //Shorten...

            correctionAdd("Sd Card", "SD card");
            correctionAdd("SDCard", "SD card");
            correctionAdd("sd card", "SD card");
            correctionAdd("sdcard", "SD card");
            correctionAdd("SD", "SD card"); //Collisions?
            correctionAdd("sd-card", "SD card");
            correctionAdd("SD-Card", "SD card");
            correctionAdd("SD Card", "SD card");
            correctionAdd("SD - card", "SD card");
            correctionAdd("SD-card", "SD card");
            correctionAdd("SDCARD", "SD card");
            correctionAdd("SdCard", "SD card");
            correctionAdd("SDcard", "SD card");
            correctionAdd("Sdcard", "SD card");
            correctionAdd("Sd card", "SD card");
            correctionAdd("sdCard", "SD card");

            correctionAdd("nmap", "Nmap");

            correctionAdd("CTRL", "Ctrl");
            correctionAdd("CRTL", "Ctrl");
            correctionAdd("ctrl", "Ctrl");

            correctionAdd("reddit", "Reddit");
            correctionAdd("Redit", "Reddit");

            correctionAdd("qr", "QR code");
            correctionAdd("qr code", "QR code");
            correctionAdd("QR-code", "QR code");

            correctionAdd("Intent", "intent");
            correctionAdd("intents", "intent"); //Plural, to avoid nearly duplicate entries.

            correctionAdd("now days", "nowadays");
            correctionAdd("now-a-days", "nowadays");
            correctionAdd("now a days", "nowadays");
            correctionAdd("Nowdays", "nowadays"); //Not 100% correct - case.
            correctionAdd("nowdays", "nowadays");
            correctionAdd("Nowadys", "nowadays"); //Not 100% correct - case.
            correctionAdd("nowadys", "nowadays");
            correctionAdd("Now a day", "nowadays"); //Not 100% correct - case.
            correctionAdd("now a day", "nowadays");
            correctionAdd("now-a-day", "nowadays");
            correctionAdd("nowaday", "nowadays");
            correctionAdd("nowerdays", "nowadays");
            correctionAdd("knowadays", "nowadays");
            correctionAdd("Now days", "nowadays");

            correctionAdd("yii", "Yii");

            correctionAdd("sata", "SATA");
            correctionAdd("Sata", "SATA");

            correctionAdd("SQL Injection", "SQL injection");
            correctionAdd("sql injection", "SQL injection");
            correctionAdd("sql Injection", "SQL injection");
            correctionAdd("sql injetion", "SQL injection");
            correctionAdd("sql-injection", "SQL injection");
            correctionAdd("Sql Injection", "SQL injection");
            correctionAdd("SQL-Injection", "SQL injection");

            correctionAdd("mysqli", "MySQLi");
            correctionAdd("Mysqli", "MySQLi");
            correctionAdd("SQLI", "MySQLi");
            correctionAdd("sqli", "MySQLi");
            correctionAdd("MYSQLi", "MySQLi");
            correctionAdd("MySqli", "MySQLi");

            correctionAdd("GD", "GD Graphics Library");
            correctionAdd("GD library", "GD Graphics Library");

            correctionAdd(".Net micro framework", ".NET Micro Framework");
            correctionAdd(".NET micro framework", ".NET Micro Framework");
            correctionAdd(".Net Micro Framework", ".NET Micro Framework");

            correctionAdd(".NetMF", ".NET Micro Framework");
            correctionAdd(".netmf", ".NET Micro Framework");
            correctionAdd(".NET MF", ".NET Micro Framework");
            correctionAdd(".NETMF", ".NET Micro Framework");
            correctionAdd("netmf", ".NET Micro Framework");
            correctionAdd("Netmf", ".NET Micro Framework");
            correctionAdd("NETMF", ".NET Micro Framework");

            correctionAdd("Micro SD", "microSD");
            correctionAdd("microSd", "microSD");
            correctionAdd("MicroSD", "microSD");
            correctionAdd("micro-SD", "microSD");
            correctionAdd("Micro Sd", "microSD");
            correctionAdd("uSD", "microSD");
            correctionAdd("MicroSd", "microSD");
            correctionAdd("micro SD", "microSD");
            correctionAdd("Micro-SD", "microSD");
            correctionAdd("microsd", "microSD");
            correctionAdd("micro sd", "microSD");
            correctionAdd("µSD", "microSD");
            correctionAdd("μSD", "microSD"); //It looks very similar to the above, but on the binary level they are different.

            correctionAdd("apple", "Apple");
            correctionAdd("APPLE", "Apple");

            correctionAdd("tl;dr", "TLDR");
            correctionAdd("tldr", "TLDR");
            correctionAdd("TL;DR", "TLDR");
            correctionAdd("TL;Dr", "TLDR");
            correctionAdd("tl; tr", "TLDR");
            correctionAdd("tl;tr", "TLDR");
            correctionAdd("Tl; Dr", "TLDR");
            correctionAdd("Tl;dr", "TLDR");
            correctionAdd("Tl;Dr", "TLDR");
            correctionAdd("Tldr", "TLDR");

            correctionAdd("lcd", "LCD");
            correctionAdd("Lcd", "LCD");

            correctionAdd("EXT3", "ext3");

            correctionAdd("EXT4", "ext4");

            correctionAdd("u-boot", "U-Boot"); //Really "Das U-Boot"
            correctionAdd("U-boot", "U-Boot"); //Really "Das U-Boot"
            correctionAdd("uboot", "U-Boot"); //Really "Das U-Boot"

            correctionAdd("X", "X Window");
            correctionAdd("x window", "X Window");
            correctionAdd("x windows", "X Window");
            correctionAdd("x-window", "X Window");
            correctionAdd("x-windows", "X Window");
            correctionAdd("X-Windows", "X Window");
            correctionAdd("xwindows", "X Window");
            correctionAdd("X Windows", "X Window");
            correctionAdd("XWindows", "X Window");
            correctionAdd("XWindow", "X Window");
            correctionAdd("X windows", "X Window");
            correctionAdd("X11", "X Window");
            correctionAdd("x11", "X Window");
            correctionAdd("X Window System", "X Window");

            correctionAdd(".exe", "EXE file");
            correctionAdd("exe", "EXE file");
            correctionAdd("Exe", "EXE file");
            correctionAdd("EXE", "EXE file");
            correctionAdd("exe file", "EXE file");

            correctionAdd("simulink", "Simulink");

            correctionAdd("url-encoding", "URL encoding");
            correctionAdd("URLencoded", "URL encoding"); //Not 100% correct.
            correctionAdd("URL encoded", "URL encoding");

            correctionAdd("play", "Play Framework");
            correctionAdd("Play", "Play Framework");

            correctionAdd("ical", "iCal"); //iCal is now known as the (unfortunate generic term) "Calendar".

            correctionAdd("sparkfun", "SparkFun");
            correctionAdd("Sparkfun", "SparkFun");
            correctionAdd("SPARKFUN", "SparkFun");

            correctionAdd("factor", "Factor");

            correctionAdd("eps", "EPS"); //Encapsulated PostScript

            correctionAdd("Postscript", "PostScript");
            correctionAdd("postscript", "PostScript");
            correctionAdd("ps", "PostScript"); //Possible collision with the Unix command-line command. And PowerShell alias for Get-Process...

            correctionAdd("SHA256", "SHA-256");
            correctionAdd("SHA 256", "SHA-256");
            correctionAdd("sha256", "SHA-256");
            correctionAdd("sha-256", "SHA-256");
            correctionAdd("sha 256", "SHA-256");

            correctionAdd("sha1", "SHA-1");
            correctionAdd("SHA1", "SHA-1");
            correctionAdd("sha 1", "SHA-1");
            correctionAdd("Sha1", "SHA-1");

            correctionAdd("SHA2", "SHA-2");
            correctionAdd("sha2", "SHA-2");
            correctionAdd("sha-2", "SHA-2");

            //"CF" is taken by ColdFusion.
            correctionAdd("compactflash", "CompactFlash");
            correctionAdd("Compactflash", "CompactFlash");

            correctionAdd("Devexpress", "DevExpress");
            correctionAdd("devexpress", "DevExpress");

            correctionAdd("hdmi", "HDMI");
            correctionAdd("HDMi", "HDMI");

            correctionAdd("ctor", "constructor");
            correctionAdd("CTOR", "constructor");
            correctionAdd("c'tor", "constructor");
            correctionAdd("contructor", "constructor");
            correctionAdd("ctr", "constructor");
            correctionAdd("CTor", "constructor");

            correctionAdd("cli", "CLI");

            correctionAdd("Autoit", "AutoIt");
            correctionAdd("autoit", "AutoIt");
            correctionAdd("autoIt", "AutoIt");
            correctionAdd("AutoIT", "AutoIt");
            correctionAdd("auoti", "AutoIt");
            correctionAdd("Auto-it", "AutoIt");
            correctionAdd("Auto IT", "AutoIt");
            correctionAdd("auto IT", "AutoIt");
            correctionAdd("auto-it", "AutoIt");
            correctionAdd("auto it", "AutoIt");
            correctionAdd("autoIT", "AutoIt");
            correctionAdd("AutiIT", "AutoIt"); // True typo...

            correctionAdd("t4", "T4");

            correctionAdd("myfaces", "MyFaces");

            correctionAdd("xstream", "XStream");

            correctionAdd("jersey", "Jersey");

            correctionAdd("opera mini", "Opera Mini");

            correctionAdd("jail broken", "jailbreaking");  //Not exact...
            correctionAdd("jail breaking", "jailbreaking");

            correctionAdd("clang", "Clang");

            correctionAdd("llvm", "LLVM");

            correctionAdd("Singleton", "singleton");

            correctionAdd("sdhc", "SDHC");

            correctionAdd("IM", "instant messaging"); //Expansion
            correctionAdd("Instant Messaging", "instant messaging");

            correctionAdd("lmgtfy", "LMGTFY");
            correctionAdd("lmgty", "LMGTFY"); //Misspelling.

            correctionAdd("ISO8601", "ISO&nbsp;8601");
            correctionAdd("iso8601", "ISO&nbsp;8601");
            correctionAdd("iso 8601", "ISO&nbsp;8601");
            correctionAdd("8601", "ISO&nbsp;8601");
            correctionAdd("ISO 8601", "ISO&nbsp;8601");
            correctionAdd("ISO-8601", "ISO&nbsp;8601");
            correctionAdd("iso-8601", "ISO&nbsp;8601");

            correctionAdd("wlan", "WLAN");
            correctionAdd("WLan", "WLAN");

            correctionAdd("raspian", "Raspbian"); //Misspelling.
            correctionAdd("raspbian", "Raspbian");
            correctionAdd("rasbian", "Raspbian");
            correctionAdd("RaspBian", "Raspbian");
            correctionAdd("Raspian", "Raspbian");
            correctionAdd("Rasbian", "Raspbian");
            correctionAdd("Raspabian", "Raspbian");
            correctionAdd("Rasbpian", "Raspbian");
            correctionAdd("rasperian", "Raspbian");
            correctionAdd("Raspberian", "Raspbian");
            correctionAdd("raspbi", "Raspbian");
            correctionAdd("RASPBIAN", "Raspbian");
            correctionAdd("rasphian", "Raspbian"); //Misspelling.

            correctionAdd("raspbmc", "Raspbmc");
            correctionAdd("raspmbc", "Raspbmc");
            correctionAdd("Raspmc", "Raspbmc");
            correctionAdd("RASPBMC", "Raspbmc");

            correctionAdd("Sandisk", "SanDisk");

            correctionAdd("xbmc", "XBMC");

            correctionAdd("Ace", "ACE");
            correctionAdd("ace", "ACE");

            correctionAdd("hta", "HTA");

            correctionAdd("fat32", "FAT32");
            correctionAdd("Fat 32", "FAT32");
            correctionAdd("FAT 32", "FAT32");
            correctionAdd("Fat32", "FAT32");
            correctionAdd("FAT-32", "FAT32");
            correctionAdd("fat 32", "FAT32");

            correctionAdd("fat16", "FAT16");
            correctionAdd("fat 16", "FAT16");

            correctionAdd("rfc", "RFC");

            correctionAdd("winRt", "WinRT");

            correctionAdd("JIRA", "Jira");
            correctionAdd("JIra", "Jira");
            correctionAdd("jira", "Jira");

            correctionAdd("telco", "telecommunications company");

            correctionAdd("lucene", "Lucene"); //Not actually observed. For the link.

            correctionAdd("Lucene.net", "Lucene.NET");
            correctionAdd("Lucene.Net", "Lucene.NET");

            correctionAdd("rc", "RC");
            correctionAdd("r/c", "RC");
            correctionAdd("R/C", "RC");

            correctionAdd("xul", "XUL");

            correctionAdd("mint", "Linux Mint");
            correctionAdd("Linux mint", "Linux Mint");
            correctionAdd("linux mint", "Linux Mint");
            correctionAdd("Mint", "Linux Mint");
            correctionAdd("LinuxMint", "Linux Mint");
            correctionAdd("Linuxmint", "Linux Mint");


            correctionAdd("signalr", "SignalR");
            correctionAdd("signalR", "SignalR");
            correctionAdd("Signalr", "SignalR");

            correctionAdd("webOS", "WebOS");

            correctionAdd("LabView", "LabVIEW");
            correctionAdd("Labview", "LabVIEW");
            correctionAdd("labview", "LabVIEW");
            correctionAdd("labVIEW", "LabVIEW");

            correctionAdd("pinterest", "Pinterest");

            correctionAdd("algo", "algorithm");
            correctionAdd("alghorithm", "algorithm");
            correctionAdd("algoritms", "algorithm");  //Not 100% correct - case.
            correctionAdd("algoritm", "algorithm");
            correctionAdd("algirithms", "algorithm");  //Not 100% correct - case.
            correctionAdd("algirithm", "algorithm");
            correctionAdd("algorith", "algorithm");
            correctionAdd("algorithim", "algorithm");
            correctionAdd("algoritmn", "algorithm");
            correctionAdd("algorithum", "algorithm");
            correctionAdd("Algorethems", "algorithm");
            correctionAdd("algorethems", "algorithm");
            correctionAdd("alorithum", "algorithm");

            correctionAdd("nfc", "NFC");

            correctionAdd("FPC", "Free Pascal");
            correctionAdd("fpc", "Free Pascal");
            correctionAdd("FreePascal", "Free Pascal");
            correctionAdd("free pascal", "Free Pascal");

            correctionAdd("Lilo", "LILO");
            correctionAdd("lilo", "LILO");

            correctionAdd("solaris", "Solaris");

            correctionAdd("TAR", "tar");

            correctionAdd("mbr", "MBR");

            correctionAdd("gparted", "GParted");
            correctionAdd("GPartEd", "GParted");
            correctionAdd("Gparted", "GParted");
            correctionAdd("parted", "GParted");

            correctionAdd("IPtables", "iptables");
            correctionAdd("iptable", "iptables");
            correctionAdd("IPTables", "iptables");

            correctionAdd("pentium", "Pentium");

            //Note: "Sam-ba" or "SAM-BA" is
            //"Atmel SAM-BA In-system Programmer", <http://www.atmel.com/tools/ATMELSAM-BAIN-SYSTEMPROGRAMMER.aspx>.
            correctionAdd("SAMBA", "Samba");
            correctionAdd("samba", "Samba");

            correctionAdd("os", "operating system");
            correctionAdd("OS", "operating system");
            correctionAdd("O/S", "operating system");
            correctionAdd("o/s", "operating system");
            correctionAdd("Operating System", "operating system");
            correctionAdd("Os", "operating system");
            correctionAdd("Operating Sytem", "operating system");
            correctionAdd("operation system", "operating system");
            correctionAdd("operational system", "operating system");
            correctionAdd("Operative system", "operating system");
            correctionAdd("operative system", "operating system");


            correctionAdd("colo", "colocation centre");

            correctionAdd("windbg", "WinDbg");
            correctionAdd("Windbg", "WinDbg");
            correctionAdd("WinDBG", "WinDbg");
            correctionAdd("WINDBG", "WinDbg");

            correctionAdd("ftdi", "FTDI");

            correctionAdd("gforth", "Gforth");
            correctionAdd("GForth", "Gforth");
            correctionAdd("gForth", "Gforth");

            correctionAdd("lazarus", "Lazarus");

            correctionAdd("PASCAL", "Pascal");
            correctionAdd("pascal", "Pascal");

            correctionAdd("Application verifier", "Application Verifier");
            correctionAdd("application verifier", "Application Verifier");

            correctionAdd("reccomand", "recommend");
            correctionAdd("recommand", "recommend");
            correctionAdd("recomand", "recommend");
            correctionAdd("reccomend", "recommend");
            correctionAdd("recommendend", "recommend"); //Not 100% correct - is for "recommended".
            correctionAdd("recomend", "recommend");
            correctionAdd("reccommend", "recommend");
            correctionAdd("reccomened", "recommend");

            correctionAdd("PDT", "PHP Development Tools");

            correctionAdd("priv", "privilege");
            correctionAdd("priveledges", "privilege"); //Not 100% correct - plural.
            correctionAdd("priveledge", "privilege");
            correctionAdd("privilegges", "privilege"); //Not 100% correct - plural.
            correctionAdd("privilegge", "privilege");
            correctionAdd("privileg", "privilege");
            correctionAdd("privile", "privilege");
            correctionAdd("privil", "privilege");
            correctionAdd("priveliges", "privilege"); //Not 100% correct - plural.
            correctionAdd("privelages", "privilege"); //Not 100% correct - plural.
            correctionAdd("privs", "privilege"); //Not 100% correct - plural.
            correctionAdd("priviledge", "privilege");
            correctionAdd("privilage", "privilege");
            correctionAdd("privelage", "privilege");
            correctionAdd("privilegies", "privilege"); //Not 100% correct - plural.
            correctionAdd("privilige", "privilege");
            correctionAdd("previlige", "privilege");
            correctionAdd("previliged", "privilege"); //Not 100% correct - tense.
            correctionAdd("privlige", "privilege");
            correctionAdd("privlege", "privilege");
            correctionAdd("privillage", "privilege");
            correctionAdd("privelege", "privilege");
            correctionAdd("privilegde", "privilege");
            correctionAdd("previlage", "privilege");
            correctionAdd("previlege", "privilege");
            correctionAdd("privilegie", "privilege");
            correctionAdd("privilge", "privilege");
            correctionAdd("privillege", "privilege");
            correctionAdd("priveldge", "privilege");

            correctionAdd("UpnP", "UPnP");

            correctionAdd("sw", "software");
            correctionAdd("SW", "software");
            correctionAdd("s/w", "software");
            correctionAdd("S/W", "software");
            correctionAdd("sofware", "software");

            correctionAdd("HW", "hardware");
            correctionAdd("hw", "hardware");
            correctionAdd("H/W", "hardware");
            correctionAdd("h/w", "hardware");
            correctionAdd("harwade", "hardware");

            correctionAdd("mac address", "MAC address");
            correctionAdd("MAC-Adress", "MAC address");
            correctionAdd("MAC-Address", "MAC address");
            correctionAdd("MAC Address", "MAC address");
            correctionAdd("MAC-adress", "MAC address");
            correctionAdd("MAC adress", "MAC address");
            correctionAdd("Mac address", "MAC address");
            correctionAdd("MAC", "MAC address"); //Note: collision with Mac (as in Macintosh).
            correctionAdd("mac-address", "MAC address");
            correctionAdd("MAC addr", "MAC address");
            correctionAdd("Mac Address", "MAC address");
            correctionAdd("MAC Adresse", "MAC address");

            correctionAdd("rest", "REST");
            correctionAdd("Rest", "REST");

            correctionAdd("Restful", "RESTful");
            correctionAdd("Restfull", "RESTful");
            correctionAdd("restfull", "RESTful");
            correctionAdd("restful", "RESTful");
            correctionAdd("RestFull", "RESTful");
            correctionAdd("RESTfull", "RESTful");
            correctionAdd("RESTFULL", "RESTful");
            correctionAdd("RESTFUL", "RESTful");

            correctionAdd("BI", "business intelligence"); //Expansion

            correctionAdd("quora", "Quora");
            correctionAdd("quoraa", "Quora");

            correctionAdd("evernote", "Evernote");

            correctionAdd("vcl", "VCL");

            correctionAdd("bitbucket", "Bitbucket");
            correctionAdd("bitbuket", "Bitbucket");
            correctionAdd("BitBucket", "Bitbucket");

            correctionAdd("oop", "object-oriented programming");
            correctionAdd("Object Oriented Programming", "object-oriented programming"); // Mostly for the URL
            correctionAdd("object oriented programming", "object-oriented programming");
            correctionAdd("object orientated programming", "object-oriented programming");

            correctionAdd("squeak", "Squeak");

            correctionAdd("allright", "all right");
            correctionAdd("alright", "all right");
            correctionAdd("al right", "all right");
            correctionAdd("Alright", "all right"); //Not 100% correct - case.
            correctionAdd("Aright", "all right");
            correctionAdd("aright", "all right");

            correctionAdd("rgb", "RGB");

            correctionAdd("google docs", "Google Docs");
            correctionAdd("Google Doc", "Google Docs");
            correctionAdd("google doc", "Google Docs");

            correctionAdd("IR", "infrared"); //Expansion

            correctionAdd("bsd", "BSD");

            correctionAdd("CAN Bus", "CAN bus");
            correctionAdd("CANbus", "CAN bus");
            correctionAdd("CAN-Bus", "CAN bus");
            correctionAdd("CAN BUS", "CAN bus");
            correctionAdd("can bus", "CAN bus");
            correctionAdd("CAN", "CAN bus");
            correctionAdd("CANBus", "CAN bus");
            correctionAdd("CANBUS", "CAN bus");
            correctionAdd("CAN-bus", "CAN bus");

            correctionAdd("vba", "VBA");
            correctionAdd("Vba", "VBA");

            correctionAdd("bay area", "Bay Area");
            correctionAdd("Bay area", "Bay Area");

            correctionAdd("mosfet", "MOSFET");
            correctionAdd("Mosfet", "MOSFET");

            correctionAdd("w3Schools", "W3Schools");
            correctionAdd("w3schools", "W3Schools");
            correctionAdd("w3shool", "W3Schools");
            correctionAdd("W3schools", "W3Schools");
            correctionAdd("w3cschools", "W3Schools"); //Misspelling.
            correctionAdd("W3 Schools", "W3Schools");
            correctionAdd("w3cshcools", "W3Schools");
            correctionAdd("W3School", "W3Schools");
            correctionAdd("w3school", "W3Schools");
            correctionAdd("W3SCHOOLS", "W3Schools");

            correctionAdd("samsung galaxy s III", "Samsung Galaxy&nbsp;S&nbsp;III");
            correctionAdd("Samsung S3", "Samsung Galaxy&nbsp;S&nbsp;III");
            correctionAdd("galaxyS", "Samsung Galaxy&nbsp;S&nbsp;III"); //Really S3?
            correctionAdd("galaxy S", "Samsung Galaxy&nbsp;S&nbsp;III"); //Really S3?
            correctionAdd("Galaxy S3", "Samsung Galaxy&nbsp;S&nbsp;III");
            correctionAdd("Samsung Galaxy S3", "Samsung Galaxy&nbsp;S&nbsp;III");
            correctionAdd("Galaxy III", "Samsung Galaxy&nbsp;S&nbsp;III");
            correctionAdd("Galaxy S III", "Samsung Galaxy&nbsp;S&nbsp;III");
            correctionAdd("Galaxy s3", "Samsung Galaxy&nbsp;S&nbsp;III");

            correctionAdd("heroku", "Heroku");
            correctionAdd("HEROKU", "Heroku");

            correctionAdd("Foxit", "Foxit Reader");
            correctionAdd("Foxit reader", "Foxit Reader");

            correctionAdd("2's compliment", "two's complement");
            correctionAdd("2's Compliment", "two's complement");
            correctionAdd("2’s complement", "two's complement");
            correctionAdd("2's complement", "two's complement");
            correctionAdd("2s compliment", "two's complement");
            correctionAdd("two's compliment", "two's complement");
            correctionAdd("twos complement", "two's complement");
            correctionAdd("2s complement", "two's complement");
            correctionAdd("Two's Complement", "two's complement");
            correctionAdd("two compliment", "two's complement");
            correctionAdd("two's-complement", "two's complement");
            correctionAdd("Two's complement", "two's complement");

            correctionAdd("msvc", "Microsoft Visual C++");
            correctionAdd("ms vc", "Microsoft Visual C++");
            correctionAdd("ms vc++", "Microsoft Visual C++"); //Will this work???? (considering a recent bug)
            correctionAdd("vc", "Microsoft Visual C++");
            correctionAdd("vc++", "Microsoft Visual C++");
            correctionAdd("VC++", "Microsoft Visual C++");
            correctionAdd("MSVC", "Microsoft Visual C++");
            correctionAdd("visual C", "Microsoft Visual C++");
            correctionAdd("Visual C", "Microsoft Visual C++");
            correctionAdd("visual c++", "Microsoft Visual C++");
            correctionAdd("Visual c++", "Microsoft Visual C++");
            correctionAdd("visual C++", "Microsoft Visual C++");
            correctionAdd("virsual c++", "Microsoft Visual C++"); //Misspelling.
            correctionAdd("Viusal C", "Microsoft Visual C++");
            correctionAdd("msvc++", "Microsoft Visual C++");
            correctionAdd("MSC", "Microsoft Visual C++");
            correctionAdd("Microsoft C", "Microsoft Visual C++");
            correctionAdd("Microsoft C++", "Microsoft Visual C++");
            correctionAdd("MSC++", "Microsoft Visual C++");
            correctionAdd("MS Visual C", "Microsoft Visual C++");
            correctionAdd("Visual C++", "Microsoft Visual C++");
            correctionAdd("MSVS C++", "Microsoft Visual C++");
            correctionAdd("virtual c++", "Microsoft Visual C++");

            correctionAdd("VC2005", "Microsoft Visual C++ 8.0");
            correctionAdd("Visual C++ 2005", "Microsoft Visual C++ 8.0");
            correctionAdd("VC++ 2005", "Microsoft Visual C++ 8.0");
            correctionAdd("Visual C++ 8.0", "Microsoft Visual C++ 8.0");

            correctionAdd("VC++2010", "Visual C++ 2010");

            correctionAdd("Google play", "Google Play");
            correctionAdd("google play", "Google Play");
            correctionAdd("Play Store", "Google Play"); //Old vs. new name???
            correctionAdd("Play store", "Google Play"); //Old vs. new name???
            correctionAdd("play store", "Google Play"); //Old vs. new name???
            correctionAdd("Andoid Play", "Google Play"); //Does Andoid Play exist????

            correctionAdd("kinect", "Kinect");

            correctionAdd("3Dsmax", "3ds&nbsp;Max");
            correctionAdd("3ds Max", "3ds&nbsp;Max"); // Effectively self.
            correctionAdd("3ds", "3ds&nbsp;Max");
            correctionAdd("3ds max", "3ds&nbsp;Max");
            correctionAdd("3dsmax", "3ds&nbsp;Max");
            correctionAdd("3DS Max", "3ds&nbsp;Max");

            correctionAdd("equilivant", "equivalent");
            correctionAdd("equivilent", "equivalent");
            correctionAdd("equalant", "equivalent");
            correctionAdd("equilivent", "equivalent");
            correctionAdd("equivelent", "equivalent");
            correctionAdd("equivelant", "equivalent");
            correctionAdd("eqivilate", "equivalent");
            correctionAdd("equivilate", "equivalent");
            correctionAdd("equivalate", "equivalent");

            correctionAdd("parallels", "Parallels");

            correctionAdd("exelent", "excellent");
            correctionAdd("excelent", "excellent");
            correctionAdd("execelent", "excellent");
            correctionAdd("execellent", "excellent");

            correctionAdd("pygame", "Pygame");

            //This one maps one.
            correctionAdd("SQL Express", "SQL Server Express Edition");
            correctionAdd("SQL Server Express", "SQL Server Express Edition");
            correctionAdd("sql server express", "SQL Server Express Edition");
            correctionAdd("sql-express", "SQL Server Express Edition");
            correctionAdd("sql express", "SQL Server Express Edition");
            correctionAdd("SQLEXPRESS", "SQL Server Express Edition");
            correctionAdd("SQL Server express", "SQL Server Express Edition");
            correctionAdd("Sql Express", "SQL Server Express Edition");
            correctionAdd("Sql server Express", "SQL Server Express Edition");
            correctionAdd("sql-server-express", "SQL Server Express Edition");
            correctionAdd("SQLExpress", "SQL Server Express Edition");
            correctionAdd("SQL Server Epxress", "SQL Server Express Edition");
            correctionAdd("SQL Server Epxress edition", "SQL Server Express Edition");
            correctionAdd("sql experss", "SQL Server Express Edition");

            //For now: map to generic as there is current no
            //         version information in the Wikipedia article.
            correctionAdd("sql server 2005 express", "SQL Server Express Edition");
            correctionAdd("SQL Server 2005 Express", "SQL Server Express Edition");

            //For now: map to generic as there is current no
            //         version information in the Wikipedia article.
            correctionAdd("sql server 2008 express", "SQL Server Express Edition");
            correctionAdd("SQL Express 2008", "SQL Server Express Edition");

            correctionAdd("com", "COM"); //Here it is Microsoft's binary interface. There is
            //conflict with serial ports (e.g. COM1)

            correctionAdd("balsamic", "Balsamiq"); //Spelling

            correctionAdd("omaha", "Omaha");

            correctionAdd("digg", "Digg");

            correctionAdd("immediatly", "immediately");
            correctionAdd("immediatelly", "immediately");
            correctionAdd("inmediatly", "immediately");
            correctionAdd("immediatley", "immediately");
            correctionAdd("immadietly", "immediately");
            correctionAdd("imedeately", "immediately");
            correctionAdd("immedately", "immediately");
            correctionAdd("immidiatelly", "immediately");
            correctionAdd("imediately", "immediately");
            correctionAdd("immedieately", "immediately");
            correctionAdd("immedetly", "immediately");
            correctionAdd("imidietally", "immediately");
            correctionAdd("immedietally", "immediately");

            correctionAdd("nsis", "NSIS");
            correctionAdd("Nsis", "NSIS");

            correctionAdd("InnoSetup", "Inno Setup");
            correctionAdd("innosetup", "Inno Setup");
            correctionAdd("Innosetup", "Inno Setup");
            correctionAdd("Inno setup", "Inno Setup");
            correctionAdd("inno setup", "Inno Setup");

            correctionAdd("rxtx", "RXTX");

            correctionAdd("3rd party", "third-party"); //As adjective
            correctionAdd("third party", "third-party"); //As adjective
            correctionAdd("3rd part", "third-party"); //As adjective
            correctionAdd("thidr-party", "third-party"); //As adjective. Misspelling.
            correctionAdd("thrid-party", "third-party"); //As adjective. Misspelling.
            correctionAdd("3rd Party", "third-party"); //As adjective
            correctionAdd("3rd-party", "third-party"); //As adjective
            correctionAdd("3rd parties", "third-party");
            correctionAdd("3-rd party", "third-party");
            correctionAdd("3rd pary", "third-party");
            correctionAdd("3'rd paty", "third-party");
            correctionAdd("thrid party", "third-party");
            correctionAdd("Third Party", "third-party");
            correctionAdd("3rd-part", "third-party");
            correctionAdd("thirdparty", "third-party");

            correctionAdd("usecase", "use case");
            correctionAdd("use-case", "use case");

            correctionAdd("Defination", "definition");
            correctionAdd("defination", "definition");
            correctionAdd("difinition", "definition");
            correctionAdd("definiton", "definition");
            correctionAdd("defintion", "definition");

            correctionAdd("Asm", "assembly language");
            correctionAdd("asm", "assembly language");
            correctionAdd("ASM", "assembly language");
            correctionAdd("assembly", "assembly language");
            correctionAdd("Assembly", "assembly language");
            correctionAdd("Assembler", "assembly language");
            correctionAdd("assembler", "assembly language");
            correctionAdd("assmebly", "assembly language");
            correctionAdd("Assembly language", "assembly language");
            correctionAdd("ASSEMBLER", "assembly language");

            correctionAdd("B2C", "business-to-consumer");
            correctionAdd("b2c", "business-to-consumer");

            correctionAdd("Process monitor", "Process Monitor");
            correctionAdd("process monitor", "Process Monitor");
            correctionAdd("processmon", "Process Monitor");
            correctionAdd("ProcMon", "Process Monitor");
            correctionAdd("procmon", "Process Monitor");
            correctionAdd("ProcessMonitor", "Process Monitor");
            correctionAdd("Procmon", "Process Monitor");
            correctionAdd("proc Mon", "Process Monitor");

            correctionAdd("qa", "QA");

            correctionAdd("noone", "no one");
            correctionAdd("no-one", "no one");

            correctionAdd("gprs", "GPRS");

            correctionAdd("sinatra", "Sinatra");

            correctionAdd("quick sort", "quicksort");
            correctionAdd("Quicksort", "quicksort");
            correctionAdd("Quick-sort", "quicksort");
            correctionAdd("Quick sort", "quicksort");
            correctionAdd("QuickSort", "quicksort");
            correctionAdd("Quick Sort", "quicksort");

            correctionAdd("Wiki Media", "Wikimedia Foundation");
            correctionAdd("wikimedia", "Wikimedia Foundation");
            correctionAdd("Wikimedia", "Wikimedia Foundation");

            correctionAdd("Lol", "LOL");
            correctionAdd("lol", "LOL");

            correctionAdd("verisign", "Verisign");
            correctionAdd("versign", "Verisign");
            correctionAdd("VeriSign", "Verisign");

            correctionAdd("code warrior", "CodeWarrior");
            correctionAdd("Codewarrior", "CodeWarrior");

            correctionAdd("GP", "Group Policy");

            correctionAdd("Web-app", "web application");
            correctionAdd("web-app", "web application");
            correctionAdd("web app", "web application");
            correctionAdd("webapp", "web application");
            correctionAdd("webApp", "web application");
            correctionAdd("Web app", "web application");
            correctionAdd("Web App", "web application");
            correctionAdd("WebApp", "web application");
            correctionAdd("Web-Apps", "web application"); //Plural, to avoid
            //nearly duplicate entries.
            correctionAdd("Web Application", "web application");
            correctionAdd("web-application", "web application");
            correctionAdd("Web-App", "web application");

            correctionAdd("webservice", "web service");
            correctionAdd("WS", "web service");
            correctionAdd("webService", "web service");
            correctionAdd("Webservice", "web service");

            correctionAdd("Libusb", "libusb");
            correctionAdd("libUsb", "libusb");
            correctionAdd("LibUsb", "libusb");
            correctionAdd("LibUSB", "libusb");
            correctionAdd("LIBUSB", "libusb");

            correctionAdd("win8", "Windows&nbsp;8");
            correctionAdd("Win8", "Windows&nbsp;8");
            correctionAdd("Windows 8", "Windows&nbsp;8");
            correctionAdd("windows 8", "Windows&nbsp;8");
            correctionAdd("win 8", "Windows&nbsp;8");
            correctionAdd("Win 8", "Windows&nbsp;8");
            correctionAdd("W8", "Windows&nbsp;8");
            correctionAdd("Windows8", "Windows&nbsp;8");
            correctionAdd("window-8", "Windows&nbsp;8");
            correctionAdd("windows-8", "Windows&nbsp;8");
            correctionAdd("WIn 8", "Windows&nbsp;8");
            correctionAdd("WIN 8", "Windows&nbsp;8");
            correctionAdd("8", "Windows&nbsp;8"); // Ambiguous!
            correctionAdd("windows8", "Windows&nbsp;8");
            correctionAdd("Win-8", "Windows&nbsp;8");

            correctionAdd("Win 8.1", "Windows&nbsp;8.1");
            correctionAdd("Windows 8.1", "Windows&nbsp;8.1");
            correctionAdd("windows 8.1", "Windows&nbsp;8.1");
            correctionAdd("8.1", "Windows&nbsp;8.1"); // Somewhat ambiguous!
            correctionAdd("win8.1", "Windows&nbsp;8.1");
            correctionAdd("win 8.1", "Windows&nbsp;8.1");
            correctionAdd("Win8.1", "Windows&nbsp;8.1");

            correctionAdd("windows 10", "Windows&nbsp;10");
            correctionAdd("Windows 10", "Windows&nbsp;10"); // Effectively self
            correctionAdd("win 10", "Windows&nbsp;10");
            correctionAdd("Win10", "Windows&nbsp;10");
            correctionAdd("win10", "Windows&nbsp;10");
            correctionAdd("Win 10", "Windows&nbsp;10");
            correctionAdd("W10", "Windows&nbsp;10");
            correctionAdd("Windows10", "Windows&nbsp;10");
            correctionAdd("windows-10", "Windows&nbsp;10");
            correctionAdd("Win. 10", "Windows&nbsp;10");
            correctionAdd("10", "Windows&nbsp;10");
            correctionAdd("WIndows 10", "Windows&nbsp;10");
            correctionAdd("w10", "Windows&nbsp;10");
            correctionAdd("Window10", "Windows&nbsp;10");
            correctionAdd("Window 10", "Windows&nbsp;10");
            correctionAdd("windows10", "Windows&nbsp;10");

            correctionAdd("UAC", "User Account Control");
            correctionAdd("uac", "User Account Control");

            correctionAdd("Load Runner", "LoadRunner");

            correctionAdd("dcom", "DCOM");
            correctionAdd("Dcom", "DCOM");

            correctionAdd("v8", "V8");

            correctionAdd("funciton", "function");
            correctionAdd("fucktion", "function");
            correctionAdd("fonction", "function");
            correctionAdd("funtion", "function");
            correctionAdd("fcn", "function");
            correctionAdd("fuction", "function"); // 2017-11-16: 2921 results
            correctionAdd("functon", "function");
            correctionAdd("func", "function");
            correctionAdd("fucntion", "function");
            correctionAdd("funcion", "function");

            correctionAdd("cgi", "CGI");

            correctionAdd("posix", "POSIX");
            correctionAdd("Posix", "POSIX");

            correctionAdd("burn", "Burn"); // Of WiX...

            correctionAdd("candle", "Candle"); // Of WiX...

            correctionAdd("bing", "Bing");
            correctionAdd("BING", "Bing");

            correctionAdd("mobo", "motherboard");
            correctionAdd("Mobo", "motherboard");
            correctionAdd("MOBO", "motherboard");
            correctionAdd("mother board", "motherboard");
            correctionAdd("MOB", "motherboard");
            correctionAdd("MBO", "motherboard");

            correctionAdd("DSL", "domain-specific language");

            correctionAdd("ocx", "OCX");

            correctionAdd("Google reader", "Google Reader");

            correctionAdd("hid", "HID");

            correctionAdd("Cdecl", "cdecl");

            correctionAdd("macbook air", "MacBook Air");
            correctionAdd("Macbook Air", "MacBook Air");
            correctionAdd("mac air", "MacBook Air");
            correctionAdd("airbook", "MacBook Air");
            correctionAdd("mac airbook", "MacBook Air");
            correctionAdd("Mac AirBook", "MacBook Air");
            correctionAdd("MacBook air", "MacBook Air");

            correctionAdd("Aptana studio", "Aptana Studio");
            correctionAdd("aptana studio", "Aptana Studio");
            correctionAdd("Aptana", "Aptana Studio");
            correctionAdd("apatana studio", "Aptana Studio");

            correctionAdd("HowTo", "how-to");
            correctionAdd("Howto", "how-to");
            correctionAdd("howto", "how-to");
            correctionAdd("How to", "how-to");
            correctionAdd("how to", "how-to");

            correctionAdd("Filemon", "FileMon");
            correctionAdd("filemon", "FileMon");
            correctionAdd("FILEMON", "FileMon");
            correctionAdd("filemonitor", "FileMon");

            correctionAdd("tor", "Tor");
            correctionAdd("TOR", "Tor");

            correctionAdd("us", "µs"); //Microseconds.
            correctionAdd("uS", "µs"); //Microseconds.
            correctionAdd("uSec", "µs"); //Microseconds.

            correctionAdd("GPO", "Group Policy Object");
            correctionAdd("gpo", "Group Policy Object");

            correctionAdd("n/w", "network");

            correctionAdd("pep", "PEP");
            correctionAdd("Pep", "PEP");

            correctionAdd("AngularJs", "AngularJS");
            correctionAdd("Angular", "AngularJS");
            correctionAdd("angularjs", "AngularJS");
            correctionAdd("angular.js", "AngularJS");
            correctionAdd("Angular.js", "AngularJS");
            correctionAdd("AJS", "AngularJS");
            correctionAdd("Angular js", "AngularJS");
            correctionAdd("angular", "AngularJS");
            correctionAdd("Angularjs", "AngularJS");
            correctionAdd("angularJS", "AngularJS");
            correctionAdd("Angular Javascript", "AngularJS");
            correctionAdd("angluar", "AngularJS");
            correctionAdd("Angular Js", "AngularJS");
            correctionAdd("angular js", "AngularJS");
            correctionAdd("angularJs", "AngularJS");
            correctionAdd("Angular JS", "AngularJS");
            correctionAdd("angular JS", "AngularJS");
            correctionAdd("Angular.JS", "AngularJS");
            correctionAdd("Angualr", "AngularJS");
            correctionAdd("AngluarJS", "AngularJS");
            correctionAdd("angulerjs", "AngularJS");
            correctionAdd("ANGULARJS", "AngularJS");
            correctionAdd("Angular-js", "AngularJS");
            correctionAdd("angularhs", "AngularJS");

            //Alternative: database administration
            correctionAdd("DBA", "database administrator");
            correctionAdd("dba", "database administrator");

            correctionAdd("esp", "especially");
            correctionAdd("especally", "especially");
            correctionAdd("espcially", "especially");
            correctionAdd("especialy", "especially");
            correctionAdd("specialy", "especially");
            correctionAdd("Esp", "especially");
            correctionAdd("especifiqly", "especially");
            correctionAdd("espascially", "especially");
            correctionAdd("expecially", "especially");

            correctionAdd("LibO", "LibreOffice");
            correctionAdd("Libreoffice", "LibreOffice");
            correctionAdd("Libre Office", "LibreOffice");
            correctionAdd("libre office", "LibreOffice");
            correctionAdd("libreoffice", "LibreOffice");
            correctionAdd("libreOffice", "LibreOffice");

            correctionAdd("CKeditor", "CKEditor");
            correctionAdd("ckeditor", "CKEditor");
            correctionAdd("FCKeditor", "CKEditor"); //Not 100% correct. FCKeditor is the predecessor to CKEditor (removed "F" and "e" is capitalised).
            correctionAdd("CK Editor", "CKEditor");
            correctionAdd("Ck Editor", "CKEditor");

            correctionAdd("Webpage", "web page");
            correctionAdd("webpage", "web page");

            correctionAdd("map/reduce", "MapReduce");
            correctionAdd("Map/Reduce", "MapReduce");
            correctionAdd("Map/reduce", "MapReduce");
            correctionAdd("map and reduce", "MapReduce");
            correctionAdd("mapreduce", "MapReduce");

            correctionAdd("hdfs", "HDFS");

            correctionAdd("WebSockets", "WebSocket");
            correctionAdd("Websocket", "WebSocket");
            correctionAdd("websocket", "WebSocket");
            correctionAdd("websockets", "WebSocket");
            correctionAdd("WebSocktes", "WebSocket"); //Misspelling.
            correctionAdd("web sockets", "WebSocket");
            correctionAdd("Websockets", "WebSocket");

            correctionAdd("of cource", "of course");
            correctionAdd("Of cource", "of course"); //Not 100% correct (case)
            correctionAdd("Of cause", "of course"); //Not 100% correct (case)
            correctionAdd("of cause", "of course");
            correctionAdd("Og cause", "of course"); //Not 100% correct (case)
            correctionAdd("og cause", "of course");
            correctionAdd("Off-course", "of course");
            correctionAdd("off-course", "of course");
            correctionAdd("Off course", "of course");
            correctionAdd("off course", "of course");
            correctionAdd("Of couse", "of course");
            correctionAdd("of couse", "of course");
            correctionAdd("ofcourse", "of course");
            correctionAdd("Ofc", "of course");
            correctionAdd("ofc", "of course"); //Not 100% correct (case)
            correctionAdd("offcourse", "of course");
            correctionAdd("offcouse", "of course");
            correctionAdd("Of courses", "of course");
            correctionAdd("of courses", "of course");
            correctionAdd("Ofcourse", "of course"); //Not 100% correct (case)
            correctionAdd("of-course", "of course");
            correctionAdd("OF-COURSE", "of course");
            //correctionAdd("Ofcourse", "of course"); //Not 100% correct (case)
            correctionAdd("of corse", "of course");
            correctionAdd("Of coarse", "of course");
            correctionAdd("of coarse", "of course");
            correctionAdd("Offcours", "of course");
            correctionAdd("offcours", "of course");


            correctionAdd("Sublime text", "Sublime Text");
            correctionAdd("sublime text", "Sublime Text");
            correctionAdd("ST", "Sublime Text"); //It could also be STMicroelectronics. For instance, the Internet domain is st.com
            correctionAdd("sublime-text", "Sublime Text");
            correctionAdd("Sublime", "Sublime Text"); //Risk of false positives.
            correctionAdd("SublimeText", "Sublime Text");
            correctionAdd("sublime", "Sublime Text");
            correctionAdd("subline text", "Sublime Text");
            correctionAdd("sublimetext", "Sublime Text");
            correctionAdd("Sublimetext", "Sublime Text");
            correctionAdd("SUBLIME", "Sublime Text");
            correctionAdd("sublimeText", "Sublime Text");

            correctionAdd("propriety", "proprietary");
            correctionAdd("propriotary", "proprietary");

            correctionAdd("meteor", "Meteor");
            correctionAdd("Meteor.js", "Meteor");
            correctionAdd("MeteorJS", "Meteor");
            correctionAdd("meteor.js", "Meteor");
            correctionAdd("meteorJS", "Meteor");
            correctionAdd("Meteor.JS", "Meteor");
            correctionAdd("MeteorJs", "Meteor");

            correctionAdd("robocopy", "Robocopy");

            correctionAdd("pfx", "PFX");

            correctionAdd("CA", "certificate authority");
            correctionAdd("Certificate authority", "certificate authority");
            correctionAdd("Certificate Authority", "certificate authority");

            correctionAdd("althou", "although");
            correctionAdd("altough", "although");
            correctionAdd("athough", "although");
            correctionAdd("Althougt", "although"); //Not 100% correct (case)
            correctionAdd("althougt", "although");
            correctionAdd("altought", "although");
            correctionAdd("altho", "although");

            correctionAdd("Scriptaculous", "Script.aculo.us");
            correctionAdd("scriptaculous", "Script.aculo.us");
            correctionAdd("script.aculo.us", "Script.aculo.us");

            correctionAdd("parelell", "parallel");
            correctionAdd("paralell", "parallel");
            correctionAdd("parrallel", "parallel");
            correctionAdd("Parrellel", "parallel");
            correctionAdd("parrellel", "parallel");
            correctionAdd("paralel", "parallel");
            correctionAdd("parallell", "parallel");
            correctionAdd("parralel", "parallel");

            correctionAdd("frontpage", "FrontPage");

            correctionAdd("arp", "ARP");

            correctionAdd("Adsense", "AdSense");
            correctionAdd("google adsense", "AdSense");
            correctionAdd("Google adsense", "AdSense");
            correctionAdd("Google Adsense", "AdSense");
            correctionAdd("adsense", "AdSense");

            correctionAdd("uefi", "UEFI");

            correctionAdd("raii", "RAII");
            correctionAdd("RAAI", "RAII"); //Misspelling...

            correctionAdd("relevent", "relevant");
            correctionAdd("relavent", "relevant");
            correctionAdd("relavant", "relevant");

            correctionAdd("relete", "relate");
            correctionAdd("releted", "relate"); //Not 100% correct - word type.

            correctionAdd("Bigquery", "BigQuery");
            correctionAdd("bigquery", "BigQuery");
            correctionAdd("bq", "BigQuery");
            correctionAdd("BQ", "BigQuery");

            correctionAdd("google storage", "Google Storage");
            correctionAdd("Google storage", "Google Storage");

            correctionAdd("com port", "COM port");
            correctionAdd("Com port", "COM port");

            correctionAdd("Netbios", "NetBIOS");
            correctionAdd("NetBios", "NetBIOS");
            correctionAdd("netbios", "NetBIOS");

            correctionAdd("Beagle Board", "BeagleBoard");
            correctionAdd("beagleboard", "BeagleBoard");
            correctionAdd("BEAGLE-BOARD", "BeagleBoard");
            correctionAdd("beagle board", "BeagleBoard");
            correctionAdd("Beagle board", "BeagleBoard");
            correctionAdd("Beagleboard", "BeagleBoard");

            correctionAdd("Beaglebone Black", "BeagleBone Black");
            correctionAdd("Beaglebone black", "BeagleBone Black");
            correctionAdd("beaglebone Black", "BeagleBone Black");
            correctionAdd("BBB", "BeagleBone Black");
            correctionAdd("BeagleBone black", "BeagleBone Black");
            correctionAdd("beaglebone black", "BeagleBone Black");
            correctionAdd("Beagleboard black", "BeagleBone Black");
            correctionAdd("Beagle bone black", "BeagleBone Black");
            correctionAdd("Beagle Bone Black", "BeagleBone Black");
            correctionAdd("Beagle bone", "BeagleBone Black"); //Not 100% correct
            correctionAdd("beaglebone", "BeagleBone Black"); //Not 100% correct
            correctionAdd("Beaglebone", "BeagleBone Black"); //Not 100% correct
            correctionAdd("beagle bone", "BeagleBone Black"); //Not 100% correct
            correctionAdd("BeagleBone", "BeagleBone Black");  //Not 100% correct
            correctionAdd("beagle bone black", "BeagleBone Black");
            correctionAdd("Beagleboard Black", "BeagleBone Black");

            correctionAdd("ambig", "ambiguity");
            correctionAdd("ambi", "ambiguity");

            correctionAdd("mage", "Mage");
            correctionAdd("mage.exe", "Mage");
            correctionAdd("Mage.exe", "Mage");

            correctionAdd("WSH", "Windows Script Host");
            correctionAdd("wsh", "Windows Script Host");
            correctionAdd("windows script host", "Windows Script Host");
            correctionAdd("Windows Scripting Host", "Windows Script Host");

            correctionAdd("pre-req", "prerequisite");
            correctionAdd("prereq", "prerequisite");
            correctionAdd("prereqs", "prerequisite"); //Plural, to avoid
            //  nearly duplicate entries.
            correctionAdd("pre-requiests", "prerequisite"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("prerequiests", "prerequisite"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("pre- requisites", "prerequisite"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("pre-requisites", "prerequisite"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("prerequisites", "prerequisite"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("prerequisities", "prerequisite"); //Plural, to avoid nearly duplicate entries. Misspelling.
            correctionAdd("pre requisities", "prerequisite"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("prerequisties", "prerequisite"); //Plural, to avoid nearly duplicate entries. Misspelling.
            correctionAdd("pre-requisite", "prerequisite"); //Misspelling.
            correctionAdd("Prerequites", "prerequisite"); //Misspelling.
            correctionAdd("prerequites", "prerequisite"); //Misspelling.
            correctionAdd("prerequite", "prerequisite"); //Misspelling.
            correctionAdd("prequisates", "prerequisite");  //Misspelling. Plural, to avoid
            //  nearly duplicate entries.
            correctionAdd("Pre-req", "prerequisite"); //Not 100% correct - case.
            correctionAdd("prerequisit", "prerequisite"); //Misspelling.
            correctionAdd("prerequsite", "prerequisite");

            correctionAdd("requites", "requisites"); //Misspelling.

            correctionAdd("unity", "Unity Application Block");
            correctionAdd("Unity", "Unity Application Block");
            correctionAdd("Unity Framework", "Unity Application Block");
            correctionAdd("Unity framework", "Unity Application Block");

            correctionAdd("gpu", "GPU");
            correctionAdd("Gpu", "GPU");

            correctionAdd("straight forward", "straightforward");
            correctionAdd("straight-forwards", "straightforward");
            correctionAdd("straight-forward", "straightforward");
            correctionAdd("straight ford", "straightforward");
            correctionAdd("straigforward", "straightforward");
            correctionAdd("straigtforward", "straightforward");
            correctionAdd("Straightfoward", "straightforward"); //Not 100% correct (case)
            correctionAdd("straightfoward", "straightforward");
            correctionAdd("straigth forward", "straightforward");
            correctionAdd("strait-forward", "straightforward");
            correctionAdd("staightforward", "straightforward");

            correctionAdd("ai", "artificial intelligence");
            correctionAdd("AI", "artificial intelligence");
            correctionAdd("Aritifical Intellegence", "artificial intelligence");
            correctionAdd("Artificial Intelligence", "artificial intelligence");

            correctionAdd("dvd", "DVD");

            correctionAdd("CD-rom", "CD-ROM");
            correctionAdd("cd-rom", "CD-ROM");
            correctionAdd("CD ROM", "CD-ROM");
            correctionAdd("CD-Rom", "CD-ROM");
            correctionAdd("CD rom", "CD-ROM");
            correctionAdd("cdrom", "CD-ROM");
            correctionAdd("CDRom", "CD-ROM");

            correctionAdd("rom", "ROM");
            correctionAdd("Rom", "ROM");

            correctionAdd("office", "Microsoft Office"); //False positives possible!!
            correctionAdd("MSoffice", "Microsoft Office");
            correctionAdd("MSOffice", "Microsoft Office");
            correctionAdd("MS-Office", "Microsoft Office");
            correctionAdd("MS Office", "Microsoft Office");
            correctionAdd("MS office", "Microsoft Office");
            correctionAdd("Office", "Microsoft Office");
            correctionAdd("Ms Office", "Microsoft Office");
            correctionAdd("ms office", "Microsoft Office");

            correctionAdd("DL", "download");
            correctionAdd("dwnld", "download");
            correctionAdd("DLing", "download"); //Not 100% correct ("ing" should be added)
            correctionAdd("downlaod", "download");
            correctionAdd("dl", "download");
            correctionAdd("dowload", "download");
            correctionAdd("donwload", "download");

            correctionAdd("ubuntu", "Ubuntu");
            correctionAdd("UBUNTU", "Ubuntu");
            correctionAdd("Ubunutu", "Ubuntu"); //Misspelling.
            correctionAdd("ubunutu", "Ubuntu"); //Misspelling.
            correctionAdd("ubunto", "Ubuntu");
            correctionAdd("Ubunto", "Ubuntu");
            correctionAdd("unbuntu", "Ubuntu");
            correctionAdd("ubutu", "Ubuntu");
            correctionAdd("ubunty", "Ubuntu");

            correctionAdd("Gutsy", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");
            correctionAdd("7.1-ubuntu", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");
            correctionAdd("ubuntu7", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");
            correctionAdd("ubuntu 7.1", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");
            correctionAdd("Ubuntu 7.1", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");
            correctionAdd("Ubuntu 7.10", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");
            correctionAdd("ubuntu 7.10", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");
            correctionAdd("Gutsy Gibbon", "Ubuntu&nbsp;7.10 (Gutsy Gibbon)");

            correctionAdd("Ubuntu 8.04", "Ubuntu&nbsp;8.04 (Hardy Heron)");
            correctionAdd("Ubuntu Linux 8.04", "Ubuntu&nbsp;8.04 (Hardy Heron)");
            correctionAdd("Hardy", "Ubuntu&nbsp;8.04 (Hardy Heron)");
            correctionAdd("Ubuntu Hardy", "Ubuntu&nbsp;8.04 (Hardy Heron)");
            correctionAdd("08.04", "Ubuntu&nbsp;8.04 (Hardy Heron)");
            correctionAdd("8.04", "Ubuntu&nbsp;8.04 (Hardy Heron)");
            correctionAdd("Hardy Heron", "Ubuntu&nbsp;8.04 (Hardy Heron)");
            correctionAdd("Ubuntu 8.04 (Hardy Heron)", "Ubuntu&nbsp;8.04 (Hardy Heron)");

            correctionAdd("Ubuntu Jaunty", "Ubuntu&nbsp;9.04 (Jaunty Jackalope)");
            correctionAdd("9.04", "Ubuntu&nbsp;9.04 (Jaunty Jackalope)");
            correctionAdd("Jaunty", "Ubuntu&nbsp;9.04 (Jaunty Jackalope)");
            correctionAdd("Ubuntu 9.04 (Jaunty Jackalope)", "Ubuntu&nbsp;9.04 (Jaunty Jackalope)");

            correctionAdd("karmic", "Ubuntu&nbsp;9.10 (Karmic Koala)");
            correctionAdd("Karmic", "Ubuntu&nbsp;9.10 (Karmic Koala)");
            correctionAdd("9.10", "Ubuntu&nbsp;9.10 (Karmic Koala)");
            correctionAdd("Ubuntu 9.10", "Ubuntu&nbsp;9.10 (Karmic Koala)");
            correctionAdd("ubuntu 9.10", "Ubuntu&nbsp;9.10 (Karmic Koala)");
            correctionAdd("Ubuntu&nbsp;9.10", "Ubuntu&nbsp;9.10 (Karmic Koala)");
            correctionAdd("Ubuntu 9.10 (Karmic Koala)", "Ubuntu&nbsp;9.10 (Karmic Koala)");
            correctionAdd("Ubuntu Karmic", "Ubuntu&nbsp;9.10 (Karmic Koala)");

            correctionAdd("lucid", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("Lucid", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("10.04", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("Ubuntu 10.04", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("ubuntu 10.04", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("Ubuntu&nbsp;10.04", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("UBUNTU 10.04", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("ubuntu 10", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("Ubuntu 10.4", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("ubuntu 10.4", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");
            correctionAdd("Ubuntu 10.04 LTS (Lucid Lynx)", "Ubuntu&nbsp;10.04 LTS (Lucid Lynx)");

            correctionAdd("10.10", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");
            correctionAdd("Ubuntu 10.10", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");
            correctionAdd("ubuntu 10.10", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");
            correctionAdd("Ubuntu Maverick", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");
            correctionAdd("1.10", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");
            correctionAdd("ubuntu 1.10", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");
            correctionAdd("Maverick Meerkat", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");
            correctionAdd("Ubuntu 10.10 (Maverick Meerkat)", "Ubuntu&nbsp;10.10 (Maverick Meerkat)");

            correctionAdd("11.04", "Ubuntu&nbsp;11.04 (Natty Narwhal)"); // But it does not work... "11.04" is converted to "104"...
            correctionAdd("ubuntu 11.04", "Ubuntu&nbsp;11.04 (Natty Narwhal)");
            correctionAdd("Ubuntu 11.04", "Ubuntu&nbsp;11.04 (Natty Narwhal)");
            correctionAdd("Ubuntu&nbsp;11.04", "Ubuntu&nbsp;11.04 (Natty Narwhal)");
            correctionAdd("Ubuntu natty", "Ubuntu&nbsp;11.04 (Natty Narwhal)");
            correctionAdd("Ubuntu Narwhal", "Ubuntu&nbsp;11.04 (Natty Narwhal)");
            correctionAdd("Natty", "Ubuntu&nbsp;11.04 (Natty Narwhal)");
            correctionAdd("Ubuntu 11.04 (Natty Narwhal)", "Ubuntu&nbsp;11.04 (Natty Narwhal)");

            correctionAdd("11.10", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)");
            correctionAdd("ubuntu 11.10", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)");
            correctionAdd("Ubuntu 11.10", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)");
            correctionAdd("Ubuntu&nbsp;11.10", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)");
            correctionAdd("oneiric", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)");
            correctionAdd("Ubuntu 11", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)"); //11 being 11.04 depends on context...
            correctionAdd("Oneiric", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)");
            correctionAdd("Ubuntu 11.10 (Oneiric Ocelot)", "Ubuntu&nbsp;11.10 (Oneiric Ocelot)");

            correctionAdd("12.04", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Ubuntu 12.04", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("ubuntu 12 .04", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Ubuntu 12 .04", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("ubuntu 12.04", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("ubuntu 12.04 lts", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Precise", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Ubuntu&nbsp;12.04", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("precise", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("unbuntu 12.04", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)"); //Misspelling.
            correctionAdd("12.0.4", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("ubuntu 12.0", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Ubuntu 12.0", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Ubuntu 12", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Ubuntu 12.4", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("ubuntu precise", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Precise Pangolin", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");
            correctionAdd("Ubuntu 12.04 LTS (Precise Pangolin)", "Ubuntu&nbsp;12.04 LTS (Precise Pangolin)");

            correctionAdd("12.10", "Ubuntu&nbsp;12.10 (Quantal Quetzal)");
            correctionAdd("Ubuntu 12.10", "Ubuntu&nbsp;12.10 (Quantal Quetzal)");
            correctionAdd("Ubuntu&nbsp;12.10", "Ubuntu&nbsp;12.10 (Quantal Quetzal)");
            correctionAdd("quantal", "Ubuntu&nbsp;12.10 (Quantal Quetzal)");
            correctionAdd("ubunut12.10", "Ubuntu&nbsp;12.10 (Quantal Quetzal)");
            correctionAdd("ubunut 12.10", "Ubuntu&nbsp;12.10 (Quantal Quetzal)");
            correctionAdd("Ubuntu 12.10 (Quantal Quetzal)", "Ubuntu&nbsp;12.10 (Quantal Quetzal)");

            correctionAdd("13.04", "Ubuntu&nbsp;13.04 (Raring Ringtail)");
            correctionAdd("Ubuntu 13.04", "Ubuntu&nbsp;13.04 (Raring Ringtail)");
            correctionAdd("Ubuntu&nbsp;13.04", "Ubuntu&nbsp;13.04 (Raring Ringtail)");
            correctionAdd("raring", "Ubuntu&nbsp;13.04 (Raring Ringtail)");
            correctionAdd("ubuntu 13.04", "Ubuntu&nbsp;13.04 (Raring Ringtail)");
            correctionAdd("Ubuntu 13.04 (Raring Ringtail)", "Ubuntu&nbsp;13.04 (Raring Ringtail)");

            correctionAdd("Ubuntu 13.10", "Ubuntu&nbsp;13.10 (Saucy Salamander)");
            correctionAdd("ubuntu 13.10", "Ubuntu&nbsp;13.10 (Saucy Salamander)");
            correctionAdd("Ubuntu&nbsp;13.10", "Ubuntu&nbsp;13.10 (Saucy Salamander)");
            correctionAdd("saucy", "Ubuntu&nbsp;13.10 (Saucy Salamander)");
            correctionAdd("13.10", "Ubuntu&nbsp;13.10 (Saucy Salamander)");
            correctionAdd("Ubuntu 13.10 (Saucy Salamander)", "Ubuntu&nbsp;13.10 (Saucy Salamander)");

            correctionAdd("Ubuntu 14.04", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("Ubuntu14.04", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("ubunru 14.04lts", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("Trusty", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("ubuntu 14.04", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("14.04", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("Ubuntu&nbsp;14.04", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("Trusty Tahr", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("14.04 Trusty", "Ubuntu&nbsp;14.04 (Trusty Tahr)"); //Really "Ubuntu 14.04 LTS (Trusty Tahr)"?
            correctionAdd("trusty", "Ubuntu&nbsp;14.04 (Trusty Tahr)");
            correctionAdd("Ubuntu 14", "Ubuntu&nbsp;14.04 (Trusty Tahr)");
            correctionAdd("Ubuntu 14.04 (Trusty Tahr)", "Ubuntu&nbsp;14.04 (Trusty Tahr)");

            correctionAdd("14.10", "Ubuntu&nbsp;14.10 (Utopic Unicorn)");
            correctionAdd("Ubuntu 14.10", "Ubuntu&nbsp;14.10 (Utopic Unicorn)");
            correctionAdd("ubuntu 14.10", "Ubuntu&nbsp;14.10 (Utopic Unicorn)");
            correctionAdd("Ubuntu&nbsp;14.10", "Ubuntu&nbsp;14.10 (Utopic Unicorn)");
            correctionAdd("Ubuntu Server 14.10", "Ubuntu&nbsp;14.10 (Utopic Unicorn)");
            correctionAdd("Ubuntu 14.10 (Utopic Unicorn)", "Ubuntu&nbsp;14.10 (Utopic Unicorn)");

            correctionAdd("Ubuntu&nbsp;15.04", "Ubuntu&nbsp;15.04 (Vivid Vervet)");
            correctionAdd("Ubuntu 15.04", "Ubuntu&nbsp;15.04 (Vivid Vervet)");
            correctionAdd("15.04", "Ubuntu&nbsp;15.04 (Vivid Vervet)");
            correctionAdd("ubuntu 15.04", "Ubuntu&nbsp;15.04 (Vivid Vervet)");
            correctionAdd("Ubuntu15", "Ubuntu&nbsp;15.04 (Vivid Vervet)");
            correctionAdd("Ubuntu 15", "Ubuntu&nbsp;15.04 (Vivid Vervet)");
            correctionAdd("Ubuntu 15.04 (Vivid Vervet)", "Ubuntu&nbsp;15.04 (Vivid Vervet)");

            correctionAdd("Ubuntu 15.10", "Ubuntu&nbsp;15.10 (Wily Werewolf)");
            correctionAdd("15.10", "Ubuntu&nbsp;15.10 (Wily Werewolf)");
            correctionAdd("Ubuntu 15.10 (Wily Werewolf)", "Ubuntu&nbsp;15.10 (Wily Werewolf)");

            correctionAdd("Ubuntu&nbsp;16.04", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("16.04", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("ubuntu 16.04", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("Ubuntu 16.04", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("UBUNTU 16.04", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("Ubuntu Xenial", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("Xenial", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("xenial", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("Ubuntu 16.04 (Xenial Xerus)", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("Ubuntu 16", "Ubuntu&nbsp;16.04 (Xenial Xerus)");
            correctionAdd("ubuntu 16", "Ubuntu&nbsp;16.04 (Xenial Xerus)");

            correctionAdd("16.10", "Ubuntu&nbsp;16.10 (Yakkety Yak)");
            correctionAdd("Ubuntu 16.10", "Ubuntu&nbsp;16.10 (Yakkety Yak)");
            correctionAdd("Ubuntu 16.10 (Yakkety Yak)", "Ubuntu&nbsp;16.10 (Yakkety Yak)");

            correctionAdd("17.04", "Ubuntu&nbsp;17.04 (Zesty Zapus)");
            correctionAdd("ubuntu 17.04", "Ubuntu&nbsp;17.04 (Zesty Zapus)");
            correctionAdd("Ubuntu 17.04", "Ubuntu&nbsp;17.04 (Zesty Zapus)");
            correctionAdd("zesty", "Ubuntu&nbsp;17.04 (Zesty Zapus)");
            correctionAdd("Ubuntu 17.04 (Zesty Zapus)", "Ubuntu&nbsp;17.04 (Zesty Zapus)");

            correctionAdd("artful", "Ubuntu&nbsp;17.10 (Artful Aardvark)");
            correctionAdd("17.10", "Ubuntu&nbsp;17.10 (Artful Aardvark)");
            correctionAdd("Ubuntu 17.10", "Ubuntu&nbsp;17.10 (Artful Aardvark)");
            correctionAdd("ubuntu17.10", "Ubuntu&nbsp;17.10 (Artful Aardvark)");
            correctionAdd("ubuntu 17.10", "Ubuntu&nbsp;17.10 (Artful Aardvark)");
            correctionAdd("Ubuntu 17", "Ubuntu&nbsp;17.10 (Artful Aardvark)");
            correctionAdd("Ubuntu 17.1", "Ubuntu&nbsp;17.10 (Artful Aardvark)");
            correctionAdd("Ubuntu 17.10 (Artful Aardvark)", "Ubuntu&nbsp;17.10 (Artful Aardvark)");

            correctionAdd("bionic", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("18.04", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("Ubuntu 18.04", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("Bionic Beaver", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("bionic beaver", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("Bionic", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("Ubuntu-18.04", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("ubuntu 18.04", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("Ubuntu&nbsp;18.04", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("Ubuntu 18.04 (Bionic Beaver)", "Ubuntu&nbsp;18.04 (Bionic Beaver)");
            correctionAdd("Ubuntu 18", "Ubuntu&nbsp;18.04 (Bionic Beaver)");

            correctionAdd("Ubuntu Cosmic", "Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)");
            correctionAdd("18.10", "Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)");
            correctionAdd("Ubuntu 18.10", "Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)");
            correctionAdd("Cosmic Cuttlefish", "Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)");
            correctionAdd("Cosmic", "Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)");
            correctionAdd("Ubuntu 18.10 (Cosmic Cuttlefish)", "Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)");
            correctionAdd("cosmic", "Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)");

            correctionAdd("19.04", "Ubuntu&nbsp;19.04 (Disco Dingo)");
            correctionAdd("disco", "Ubuntu&nbsp;19.04 (Disco Dingo)");
            correctionAdd("Disco", "Ubuntu&nbsp;19.04 (Disco Dingo)");
            correctionAdd("Disco Dingo", "Ubuntu&nbsp;19.04 (Disco Dingo)");
            correctionAdd("Ubuntu 19.04 (Disco Dingo)", "Ubuntu&nbsp;19.04 (Disco Dingo)");
            correctionAdd("ubuntu 19", "Ubuntu&nbsp;19.04 (Disco Dingo)");
            correctionAdd("Ubuntu 19", "Ubuntu&nbsp;19.04 (Disco Dingo)");
            correctionAdd("Ubuntu 19.04", "Ubuntu&nbsp;19.04 (Disco Dingo)");

            correctionAdd("19.10", "Ubuntu&nbsp;19.10 (Eoan Ermine)");
            correctionAdd("Ubuntu 19.10", "Ubuntu&nbsp;19.10 (Eoan Ermine)");

            correctionAdd("VS", "Visual Studio");
            correctionAdd("visual studio", "Visual Studio");
            correctionAdd("vs", "Visual Studio");
            correctionAdd("vs.net", "Visual Studio");
            correctionAdd("visual stduio", "Visual Studio");
            correctionAdd("visual studios", "Visual Studio");
            correctionAdd("Visual studio", "Visual Studio");
            correctionAdd("MSVS", "Visual Studio");
            correctionAdd("Visual Studios", "Visual Studio");
            correctionAdd("VisualStudio", "Visual Studio");
            correctionAdd("VS.NET", "Visual Studio"); //There is no special version of Visual Studio for .NET.
            correctionAdd("visual sutdio", "Visual Studio"); //Misspelling.
            correctionAdd("VStudio", "Visual Studio");
            correctionAdd("Visual-Studio", "Visual Studio");
            correctionAdd("viual studio", "Visual Studio"); //Misspelling.
            correctionAdd("vusual studio", "Visual Studio"); //True typo!
            correctionAdd("Vs", "Visual Studio");
            correctionAdd("Studio", "Visual Studio");
            correctionAdd("Vstudio", "Visual Studio");
            correctionAdd("viusal studio", "Visual Studio");
            correctionAdd("Visual", "Visual Studio");
            correctionAdd("V.Studio", "Visual Studio");
            correctionAdd("visual Studio", "Visual Studio");
            correctionAdd("studio", "Visual Studio");
            correctionAdd("visulastudio", "Visual Studio");
            correctionAdd("Visual Stuidio", "Visual Studio");
            correctionAdd("Visual Vtudio", "Visual Studio");
            correctionAdd("visal studio", "Visual Studio");
            correctionAdd("visual-studio", "Visual Studio");
            correctionAdd("VS.Net", "Visual Studio");
            correctionAdd("Visualstudio", "Visual Studio");
            correctionAdd("visualstudio", "Visual Studio");
            correctionAdd("visual study", "Visual Studio");
            correctionAdd("vis studio", "Visual Studio");
            correctionAdd("Visual Studi", "Visual Studio");
            correctionAdd("visual sudio", "Visual Studio");

            correctionAdd("vb 2005", "Visual Basic 2005 Express Edition");
            correctionAdd("vb2005", "Visual Basic 2005 Express Edition");

            correctionAdd("VB Express 2008", "Visual Basic 2008 Express Edition");
            correctionAdd("Visual Basic Express 2008", "Visual Basic 2008 Express Edition");
            correctionAdd("VB.Net Express 2008", "Visual Basic 2008 Express Edition");
            correctionAdd("vb 2008", "Visual Basic 2008 Express Edition");

            correctionAdd("Visual C# 2008 Express", "Visual C# 2008 Express Edition");
            correctionAdd("visual studio express (2008", "Visual C# 2008 Express Edition"); //C# for that particular instance. Note: no ending ")" because of our bug - thus a ")" will remain in the Markdown text in the browser (that must be manually removed...).

            correctionAdd("visual c++ express 2010", "Visual C++ 2010 Express");
            correctionAdd("Visual C++ Express", "Visual C++ 2010 Express"); //Not exact...
            correctionAdd("VS C++ 2010 express", "Visual C++ 2010 Express");
            correctionAdd("microsoft visual c++ 2010 express", "Visual C++ 2010 Express");

            correctionAdd("VS 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("VS2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("vs 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("visual studio 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("vs2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("studio 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("VS10", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("visual studio 10", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("visual studios 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("VS 10", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("VisualStudio 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("Visual Studio 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("VIsual Studio 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("Visual Studio 10", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("VS.NET 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("Vs2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("Visual 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("visual sudio 2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("2010", "Visual&nbsp;Studio&nbsp;2010");
            correctionAdd("VS 2k10", "Visual&nbsp;Studio&nbsp;2010");


            correctionAdd("Visual Studio 2010 Express Edition", "Visual Studio 2010 Express");
            correctionAdd("VS-2010 Express", "Visual Studio 2010 Express");
            correctionAdd("VS2010 Express", "Visual Studio 2010 Express");
            correctionAdd("VS2010 EXPRESS", "Visual Studio 2010 Express");
            correctionAdd("visual studio 2010 express", "Visual Studio 2010 Express");

            correctionAdd("Visual Basic 2010 Express", "Visual Basic 2010 Express Edition");
            correctionAdd("VB 2010 Express", "Visual Basic 2010 Express Edition");
            correctionAdd("VB express2010", "Visual Basic 2010 Express Edition");
            correctionAdd("vb 2010", "Visual Basic 2010 Express Edition");

            correctionAdd("Visual Studio Express 2012", "Visual Studio 2012 Express");
            correctionAdd("Visual Studio 2012 express", "Visual Studio 2012 Express");
            correctionAdd("VS Express 2012", "Visual Studio 2012 Express");
            correctionAdd("visual studio 2012 Express", "Visual Studio 2012 Express");
            correctionAdd("VS 2012 Express", "Visual Studio 2012 Express");

            correctionAdd("VS express C#", "Visual C# Express");
            correctionAdd("C# express", "Visual C# Express");

            correctionAdd("VS express", "Visual Studio Express");
            correctionAdd("vs express", "Visual Studio Express");
            correctionAdd("visual studio express", "Visual Studio Express");

            correctionAdd("VS2003", "Visual&nbsp;Studio&nbsp;2003");
            correctionAdd("VS 2003", "Visual&nbsp;Studio&nbsp;2003");

            correctionAdd("VS2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("VS 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("vs2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("Visual Studio 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("visualstudio2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("visualstudio 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("visual studio 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("MSVS2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("vs.net 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("Vs2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("VisualStudio 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("vs 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("Visual studio 2005", "Visual&nbsp;Studio&nbsp;2005");
            correctionAdd("2005", "Visual&nbsp;Studio&nbsp;2005");

            correctionAdd("VS 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("visual studio 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS.Net 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS.NET 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("Visual Studio2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("studio 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("Visual Studio 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("vs08", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("vs 08", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("Vs08", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS'08", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS2k8", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("vs.net 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("vs2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("Visual Studio 2K8", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("Vs 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS08", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("visualstudio2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("visualstudio 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("vs 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("Visual 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS 08", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("Visual studio 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("visual studio 8", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("visual studio.net 2008", "Visual&nbsp;Studio&nbsp;2008");
            correctionAdd("VS2K8", "Visual&nbsp;Studio&nbsp;2008");

            correctionAdd("VS 2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("VS2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("visual studio 2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("vs2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("Visual studio 2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("vs 2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("Visual Studio 2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("VS11", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("visual studio 11", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("visual studio 2011", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("VS12", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("vs12", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("MVS 2011", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("visual studios 2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("VS.2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("Visual 2012", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("VS 12", "Visual&nbsp;Studio&nbsp;2012");
            correctionAdd("VS 11/12", "Visual&nbsp;Studio&nbsp;2012");

            correctionAdd("VS2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("visual studio 2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("VS 2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("Visual Studio 2013", "Visual&nbsp;Studio&nbsp;2013"); // Effectively self
            correctionAdd("Visual studio 2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("vs2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("VISUAL STUDIO 2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("VS13", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("visual 2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("vs 2013", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("visual studio 13", "Visual&nbsp;Studio&nbsp;2013");
            correctionAdd("VS2103", "Visual&nbsp;Studio&nbsp;2013");

            correctionAdd("VS 2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("VS2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("Visual Studio 2015", "Visual&nbsp;Studio&nbsp;2015"); // Effectively self
            correctionAdd("Visual studio 2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("vs2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("MSVS2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("vs 2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("Visual Studion 2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("visual studio 2015", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("visual studio 15", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("vs15", "Visual&nbsp;Studio&nbsp;2015");
            correctionAdd("VisualStudio 2015", "Visual&nbsp;Studio&nbsp;2015");

            correctionAdd("Visual Studio 17", "Visual&nbsp;Studio&nbsp;2017");
            correctionAdd("Visual Studio 2017", "Visual&nbsp;Studio&nbsp;2017");
            correctionAdd("vs2017", "Visual&nbsp;Studio&nbsp;2017");
            correctionAdd("VS2017", "Visual&nbsp;Studio&nbsp;2017");
            correctionAdd("VS 2017", "Visual&nbsp;Studio&nbsp;2017");
            correctionAdd("visual studio 2017", "Visual&nbsp;Studio&nbsp;2017");
            correctionAdd("visual 2017", "Visual&nbsp;Studio&nbsp;2017");
            correctionAdd("VISUAL STUDIO 2017", "Visual&nbsp;Studio&nbsp;2017");

            correctionAdd("openGl", "OpenGL");
            correctionAdd("opengl", "OpenGL");
            correctionAdd("Open Gl", "OpenGL");
            correctionAdd("open gl", "OpenGL");
            correctionAdd("Open GL", "OpenGL");
            correctionAdd("open Gl", "OpenGL");
            correctionAdd("OpenGl", "OpenGL");
            correctionAdd("Opengl", "OpenGL");
            correctionAdd("openGL", "OpenGL");
            correctionAdd("open-gl", "OpenGL");
            correctionAdd("OPENGL", "OpenGL");

            correctionAdd("openCL", "OpenCL");
            correctionAdd("Opencl", "OpenCL");
            correctionAdd("openCl", "OpenCL");
            correctionAdd("OpenCl", "OpenCL");
            correctionAdd("opencl", "OpenCL");
            correctionAdd("Open CL", "OpenCL");

            correctionAdd("BitCoin", "Bitcoin");
            correctionAdd("bitcoin", "Bitcoin"); //In most cases (not if it is actual bitcoins).
            correctionAdd("BTC", "Bitcoin"); //Expansion
            correctionAdd("btc", "Bitcoin"); //Expansion
            correctionAdd("BITCOINS", "Bitcoin");
            correctionAdd("BITCOIN", "Bitcoin");

            correctionAdd("bitcoinqt", "Bitcoin-Qt");
            correctionAdd("Bitcoinqt", "Bitcoin-Qt");
            correctionAdd("Bitcoin-qt", "Bitcoin-Qt");
            correctionAdd("Bitcoin-QT", "Bitcoin-Qt");
            correctionAdd("Bitcoin QT", "Bitcoin-Qt");
            correctionAdd("bitcoin qt", "Bitcoin-Qt");
            correctionAdd("bitcoin-qt", "Bitcoin-Qt");
            correctionAdd("BitCoin-Qt", "Bitcoin-Qt");
            correctionAdd("Bitoin QT", "Bitcoin-Qt");
            correctionAdd("bitcoint-qt", "Bitcoin-Qt");

            correctionAdd("INTEL", "Intel");
            correctionAdd("intel", "Intel");

            correctionAdd("Textwrangler", "TextWrangler");

            correctionAdd("Slickedit", "SlickEdit");

            correctionAdd("d3js", "D3.js");
            correctionAdd("D3", "D3.js");
            correctionAdd("d3.js", "D3.js");
            correctionAdd("d3", "D3.js");

            correctionAdd("guaranties", "guarantee"); //Plural, to avoid nearly duplicate entries.
            correctionAdd("gaurantee", "guarantee");
            correctionAdd("guarantieed", "guarantee"); //XXX, to avoid nearly duplicate entries.
            correctionAdd("guareente", "guarantee");
            correctionAdd("guaranty", "guarantee");
            correctionAdd("garantee", "guarantee");
            correctionAdd("guarenteed", "guarantee"); //Not 100% correct, tense.
            correctionAdd("guarentee", "guarantee");
            correctionAdd("garanteed", "guarantee");
            correctionAdd("guarantuee", "guarantee");
            correctionAdd("guarantued", "guarantee");
            correctionAdd("guarantue", "guarantee");


            correctionAdd("eeprom", "EEPROM");
            correctionAdd("EEprom", "EEPROM");

            correctionAdd("eprom", "EPROM");
            correctionAdd("Eprom", "EPROM");

            correctionAdd("oo", "object-oriented (OO)");
            correctionAdd("OO", "object-oriented (OO)"); //Expansion
            correctionAdd("Object Oriented", "object-oriented (OO)");

            correctionAdd("Processor explorer", "Process&nbsp;Explorer");
            correctionAdd("Processor Explorer", "Process&nbsp;Explorer"); // Effectively self.

            correctionAdd("google IO", "Google I/O");
            correctionAdd("Google io", "Google I/O");

            correctionAdd("BrainFuck", "Brainfuck");
            correctionAdd("BRAINFUCK", "Brainfuck");

            correctionAdd("Op Amps", "operational amplifier"); //Plural thing
            correctionAdd("op-amp", "operational amplifier"); //Expansion
            correctionAdd("op amp", "operational amplifier");
            correctionAdd("Op amp", "operational amplifier");
            correctionAdd("opamp", "operational amplifier");
            correctionAdd("OpAmp", "operational amplifier");
            correctionAdd("Opamp", "operational amplifier");
            correctionAdd("Op Amp", "operational amplifier"); //The official short form is "op-amp" - <https://en.wiktionary.org/wiki/op-amp#English>
            correctionAdd("OPAmps", "operational amplifier"); //Not 100% correct - plural.
            correctionAdd("OPAmp", "operational amplifier");
            correctionAdd("OP-AMP", "operational amplifier");
            correctionAdd("opAmp", "operational amplifier");
            correctionAdd("OA", "operational amplifier"); //Expansion
            correctionAdd("opmap", "operational amplifier");

            correctionAdd("ASN1", "ASN.1");
            correctionAdd("ASN-1", "ASN.1");

            correctionAdd("nexus s", "Nexus S");

            correctionAdd("nexus 4", "Nexus&nbsp;4");
            correctionAdd("Nexus 4", "Nexus&nbsp;4");

            correctionAdd("Nexus 5", "Nexus&nbsp;5");
            correctionAdd("nexus 5", "Nexus&nbsp;5");

            correctionAdd("nexus 7", "Nexus&nbsp;7");
            correctionAdd("Nexus 7", "Nexus&nbsp;7");
            correctionAdd("Nexus Seven", "Nexus&nbsp;7");
            correctionAdd("Nexus7", "Nexus&nbsp;7");

            correctionAdd("govt", "government");
            correctionAdd("Govt", "government");
            correctionAdd("gov’t", "government");
            correctionAdd("gov't", "government");
            correctionAdd("Gov’t", "government");
            correctionAdd("govement", "government");
            correctionAdd("gov", "government");
            correctionAdd("goverment", "government");
            correctionAdd("gouverment", "government");
            correctionAdd("governent", "government");

            correctionAdd("msg", "message");
            correctionAdd("massege", "message");

            correctionAdd("heartbleed", "Heartbleed");
            correctionAdd("HeartBleed", "Heartbleed");

            correctionAdd("cap", "capacitor");
            correctionAdd("capasitor", "capacitor");
            correctionAdd("Capcitors", "capacitor"); //Not 100% correct - plural and case.
            correctionAdd("capcitors", "capacitor"); //Not 100% correct - plural.
            correctionAdd("Capcitor", "capacitor");
            correctionAdd("capcitor", "capacitor");

            correctionAdd("Meta StackExchange", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("MSE", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("mse", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("meta.SE", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("Meta SE", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("MetaSE", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("Meta.SE", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("meta-SE", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("Stack-exchange Meta", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("Stack exchange Meta", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("Meta Stack Exchange", "Meta&nbsp;Stack&nbsp;Exchange"); // Effectively self.
            correctionAdd("meta.stackexchange", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("meta stock exchange", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("META StackExchange", "Meta&nbsp;Stack&nbsp;Exchange");
            correctionAdd("Meta.StackExchange", "Meta&nbsp;Stack&nbsp;Exchange");

            correctionAdd("ppl", "people");
            correctionAdd("peole", "people");
            correctionAdd("poeple", "people");
            correctionAdd("peaple", "people");
            correctionAdd("peoole", "people");
            correctionAdd("peopel", "people");

            correctionAdd("Loc", "LOC");
            correctionAdd("loc", "LOC");

            correctionAdd("scart", "SCART");
            correctionAdd("Scart", "SCART");

            correctionAdd("guiminer", "GUIMiner");

            correctionAdd("Esr", "ESR");
            correctionAdd("esr", "ESR");
            correctionAdd("equivalent series resistance", "ESR");

            correctionAdd("ctags", "Ctags");

            correctionAdd("codecademy", "Codecademy");
            correctionAdd("Code Academy", "Codecademy");
            correctionAdd("Code academy", "Codecademy");
            correctionAdd("CodeAcademy", "Codecademy");
            correctionAdd("codacademy", "Codecademy");
            correctionAdd("code academy", "Codecademy");
            correctionAdd("codeacademy", "Codecademy");
            correctionAdd("codeaccademy", "Codecademy");

            correctionAdd("QBASIC", "QBasic");
            correctionAdd("Qbasic", "QBasic");
            correctionAdd("qbasic", "QBasic");

            correctionAdd("Unit test", "unit test");
            correctionAdd("Unit Test", "unit test");

            correctionAdd("SMPS", "switched-mode power supply");
            correctionAdd("smps", "switched-mode power supply");
            correctionAdd("Smps", "switched-mode power supply");

            correctionAdd("swift", "Swift");
            correctionAdd("SWIFT", "Swift");

            correctionAdd("noobs", "NOOBS");
            correctionAdd("Noobs", "NOOBS");

            correctionAdd("smd", "SMD");

            correctionAdd("Lambda expression", "lambda expression");

            correctionAdd("GO", "Go");
            correctionAdd("go", "Go");
            correctionAdd("golang", "Go");
            correctionAdd("GoLang", "Go");
            correctionAdd("go-lang", "Go");

            correctionAdd("OpenElec", "OpenELEC");
            correctionAdd("Openelec", "OpenELEC");
            correctionAdd("Open Elec", "OpenELEC");
            correctionAdd("openelec", "OpenELEC");

            correctionAdd("fpga", "FPGA");

            correctionAdd("whatsapp", "WhatsApp");
            correctionAdd("Whatsapp", "WhatsApp");
            correctionAdd("watsapp", "WhatsApp");
            correctionAdd("whatsup", "WhatsApp");
            correctionAdd("whatsApp", "WhatsApp");
            correctionAdd("Whats app", "WhatsApp");
            correctionAdd("whats app", "WhatsApp");


            //Currently defunc due to some recent changes!!!
            //  correctionAdd("Textpad++", "Textpad++");
            //
            //  correctionAdd("notepad++", "Notepad++");
            //  correctionAdd("Notepadd++", "Notepad++");
            //  correctionAdd("NotePad++", "Notepad++");
            //  correctionAdd("notepadd++", "Notepad++");
            //  correctionAdd("notepadd ++", "Notepad++");
            //  correctionAdd("Np++", "Notepad++");
            //  correctionAdd("notepad ++", "Notepad++");
            //  correctionAdd("N++", "Notepad++");
            correctionAdd("npp", "Notepad++"); //A shortcut so it is still possible to lookup...
            correctionAdd("NPP", "Notepad++"); //A shortcut so it is still possible to lookup...
            correctionAdd("NP++", "Notepad++"); //A shortcut so it is still possible to lookup...
            correctionAdd("NotePad++", "Notepad++"); //A shortcut so it is still possible to lookup...
            correctionAdd("notepad++", "Notepad++");
            correctionAdd("NOTEPAD++", "Notepad++");
            correctionAdd("Notepad ++", "Notepad++");
            correctionAdd("notepadd++", "Notepad++");
            correctionAdd("Notepadd++", "Notepad++");
            correctionAdd("N++", "Notepad++");

            correctionAdd("MS-Word", "Microsoft Word");
            correctionAdd("MS Word", "Microsoft Word");
            correctionAdd("Word", "Microsoft Word");
            correctionAdd("Msword", "Microsoft Word");
            correctionAdd("MS word", "Microsoft Word");
            correctionAdd("ms word", "Microsoft Word");
            correctionAdd("word", "Microsoft Word");
            correctionAdd("MSWord", "Microsoft Word");
            correctionAdd("Microsoft word", "Microsoft Word");
            correctionAdd("microsoft word", "Microsoft Word");
            correctionAdd("msword", "Microsoft Word");
            correctionAdd("winword", "Microsoft Word");

            correctionAdd("bootstrap", "Bootstrap");
            correctionAdd("Boostrap", "Bootstrap");
            correctionAdd("boostrap", "Bootstrap");
            correctionAdd("Bootsrap", "Bootstrap");
            correctionAdd("BootStrap", "Bootstrap");

            correctionAdd("iso", "ISO image");
            correctionAdd("ISO", "ISO image");
            correctionAdd("iso image", "ISO image");
            correctionAdd("ISO-image", "ISO image");

            correctionAdd("Doctype", "DOCTYPE");
            correctionAdd("doctype", "DOCTYPE");

            correctionAdd("wpa", "WPA");

            correctionAdd("wpa2", "WPA2");

            correctionAdd("english", "English");

            correctionAdd("lets", "let’s");
            correctionAdd("Lets", "let’s"); //Not 100% correct - case.
            correctionAdd("let's", "let’s");
            correctionAdd("Let’s", "let’s"); //Not 100% correct - case.
            correctionAdd("Let's", "let’s"); //Not 100% correct - case.

            correctionAdd("disambig", "disambiguation");

            correctionAdd("wud", "would");
            correctionAdd("whould", "would");

            correctionAdd("minecraft", "Minecraft");
            correctionAdd("mine craft", "Minecraft");
            correctionAdd("Mine craft", "Minecraft");

            correctionAdd("AP", "access point");
            correctionAdd("Access point", "access point");
            correctionAdd("Access Point", "access point");
            correctionAdd("access-point", "access point");

            correctionAdd("pop3", "POP3");

            correctionAdd("imap", "IMAP");

            correctionAdd("flask", "Flask");

            correctionAdd("o-scope", "oscilloscope");
            correctionAdd("o'scope", "oscilloscope");
            correctionAdd("scope", "oscilloscope");
            correctionAdd("osc", "oscilloscope");
            correctionAdd("osciliscope", "oscilloscope");
            correctionAdd("Osciloscope", "oscilloscope");
            correctionAdd("osciloscope", "oscilloscope");
            correctionAdd("oscilliscope", "oscilloscope");

            correctionAdd("Cat5", "Cat&nbsp;5");
            correctionAdd("cat5", "Cat&nbsp;5");
            correctionAdd("cat 5", "Cat&nbsp;5");
            correctionAdd("CAT5", "Cat&nbsp;5");
            correctionAdd("CAT 5", "Cat&nbsp;5");

            correctionAdd("Cat6", "Cat&nbsp;6");
            correctionAdd("cat6", "Cat&nbsp;6");
            correctionAdd("cat 6", "Cat&nbsp;6");
            correctionAdd("CAT6", "Cat&nbsp;6");
            correctionAdd("CAT 6", "Cat&nbsp;6");

            correctionAdd("jenkins", "Jenkins");
            correctionAdd("Jnekins", "Jenkins");
            correctionAdd("Jekins", "Jenkins");
            correctionAdd("Jennkins", "Jenkins");

            correctionAdd("Pypy", "PyPy");
            correctionAdd("pypy", "PyPy");

            correctionAdd("SocketIO", "Socket.IO");
            correctionAdd("socket.io", "Socket.IO");

            correctionAdd("oData", "OData");

            correctionAdd("ready boost", "ReadyBoost");
            correctionAdd("readyboost", "ReadyBoost");

            correctionAdd("hacker news", "Hacker&nbsp;News");
            correctionAdd("Hacker news", "Hacker&nbsp;News");
            correctionAdd("HN", "Hacker&nbsp;News");

            correctionAdd("alot", "a lot");
            correctionAdd("allot", "a lot");
            correctionAdd("Alot", "a lot");

            correctionAdd("slashdot", "Slashdot");

            correctionAdd("Ml", "ML");

            correctionAdd("rust", "Rust");

            correctionAdd("yo", "-year-old");
            correctionAdd("year old", "-year-old");
            correctionAdd("YO", "-year-old");
            correctionAdd("yr old", "-year-old");
            correctionAdd("Year old", "-year-old");
            correctionAdd("y.o", "-year-old");
            correctionAdd("y/o", "-year-old");
            correctionAdd("year-old", "-year-old");

            correctionAdd("boot repair", "Boot-Repair");

            correctionAdd("independant", "independent"); //Common misspelling...
            correctionAdd("indepndent", "independent");
            correctionAdd("indepent", "independent");
            correctionAdd("indepedent", "independent");

            correctionAdd("shellshock", "Shellshock");
            correctionAdd("shelllock", "Shellshock");
            correctionAdd("Shell Shock", "Shellshock");
            correctionAdd("shell shock", "Shellshock");

            correctionAdd("DDOS", "DDoS");
            correctionAdd("DoS", "DDoS"); //Not strictly correct (the extra "D" means "distributed")
            correctionAdd("ddos", "DDoS");

            correctionAdd("Pylon", "Pylons");
            correctionAdd("pylon", "Pylons");
            correctionAdd("pylons", "Pylons");

            correctionAdd("wmi", "WMI");

            //Perhaps repurpose for PowerShell 2.0 instead of PowerShell 4.0:
            //
            //  PowerShell 2.0:  http://technet.microsoft.com/en-us/library/dd347685.aspx
            //
            //  PowerShell 4.0:  http://technet.microsoft.com/en-us/library/hh849945.aspx
            //
            //
            //Of PowerShell...
            correctionAdd("read-host", "Read-Host");

            //Of PowerShell...
            correctionAdd("get-childitem", "Get-ChildItem");
            correctionAdd("gci", "Get-ChildItem");
            correctionAdd("Get-childitem", "Get-ChildItem");
            correctionAdd("Get-Childitem", "Get-ChildItem");
            correctionAdd("get-childItem", "Get-ChildItem");
            correctionAdd("Get-Children", "Get-ChildItem"); //Actually, there is no such thing as Get-Children in PowerShell.
            correctionAdd("gcm", "Get-ChildItem");

            //Of PowerShell...
            correctionAdd("measure-object", "Measure-Object");

            //Of PowerShell...
            correctionAdd("get-process", "Get-Process");
            correctionAdd("Get-process", "Get-Process");

            //Of PowerShell...
            correctionAdd("select-object", "Select-Object");
            correctionAdd("select", "Select-Object");
            correctionAdd("Select", "Select-Object");

            //Of PowerShell...
            correctionAdd("get-member", "Get-Member");
            correctionAdd("gm", "Get-Member"); // The alias

            //Of PowerShell...
            correctionAdd("get-adgroupmember", "Get-ADGroupMember");

            //Of PowerShell...
            correctionAdd("where-object", "Where-Object");
            correctionAdd("where", "Where-Object"); // The alias
            correctionAdd("Where-object", "Where-Object");

            correctionAdd("Kali", "Kali Linux");
            correctionAdd("kali", "Kali Linux");
            correctionAdd("Linux Kali", "Kali Linux");

            correctionAdd("unetbootin", "UNetbootin");
            correctionAdd("Unetbootin", "UNetbootin");
            correctionAdd("UnetBootin", "UNetbootin");
            correctionAdd("uNetBootin", "UNetbootin");

            //Of PowerShell...
            correctionAdd("group-object", "Group-Object");

            //Of PowerShell...
            correctionAdd("sort-object", "Sort-Object");
            correctionAdd("sort", "Sort-Object"); // An alias.

            //Of PowerShell...
            correctionAdd("add-member", "Add-Member");
            correctionAdd("Add-member", "Add-Member");

            //Of PowerShell...
            correctionAdd("new-object", "New-Object");

            //Of PowerShell...
            correctionAdd("get-content", "Get-Content");
            correctionAdd("GET-CONTENT", "Get-Content");

            //Of PowerShell...
            correctionAdd("get-help", "Get-Help");

            //Of PowerShell...
            correctionAdd("remove-item", "Remove-Item");
            correctionAdd("rd", "Remove-Item");

            //Of PowerShell...
            correctionAdd("add-content", "Add-Content");

            //Of PowerShell...
            correctionAdd("get-date", "Get-Date");
            correctionAdd("get-Date", "Get-Date");
            correctionAdd("Get-date", "Get-Date");

            //Of PowerShell...
            correctionAdd("Write-Object", "Write-Output");
            correctionAdd("write-output", "Write-Output");
            correctionAdd("write-Output", "Write-Output");
            correctionAdd("write", "Write-Output");
            correctionAdd("Write", "Write-Output");
            correctionAdd("Write-output", "Write-Output");
            correctionAdd("write-ouptut", "Write-Output");

            //Of PowerShell...
            correctionAdd("write-host", "Write-Host");
            correctionAdd("Write-host", "Write-Host");
            correctionAdd("write-Host", "Write-Host");

            //Of PowerShell...
            correctionAdd("get-itemproperty", "Get-ItemProperty");

            //Of PowerShell...
            correctionAdd("format-list", "Format-List");

            //Of PowerShell...
            correctionAdd("format-table", "Format-Table");

            //Of PowerShell...
            correctionAdd("out-file", "Out-File");

            //Of PowerShell...
            correctionAdd("get-command", "Get-Command");

            //Of PowerShell...
            correctionAdd("get-host", "Get-Host");

            //Of PowerShell...
            correctionAdd("tee-object", "Tee-Object");
            correctionAdd("tee", "Tee-Object");
            correctionAdd("Tee-object", "Tee-Object");
            correctionAdd("Tea", "Tee-Object");

            //Of PowerShell...
            correctionAdd("invoke-command", "Invoke-Command");

            //Of PowerShell...
            correctionAdd("set-executionPolicy", "Set-ExecutionPolicy");
            correctionAdd("set-executionpolicy", "Set-ExecutionPolicy");

            //Of PowerShell...
            correctionAdd("get-wmiobject", "Get-WmiObject");
            correctionAdd("get-WmiObject", "Get-WmiObject");
            correctionAdd("Get-WMIObject", "Get-WmiObject");
            correctionAdd("gwmi", "Get-WmiObject"); // An alias.

            //Of PowerShell...
            correctionAdd("export-csv", "Export-Csv");
            correctionAdd("export-CSV", "Export-Csv");
            correctionAdd("Export-CSV", "Export-Csv");

            //Of PowerShell...
            correctionAdd("copy-item", "Copy-Item");

            //Of PowerShell...
            correctionAdd("set-item", "Set-Item");

            //Of PowerShell...
            correctionAdd("get-item", "Get-Item");

            //Of PowerShell...
            correctionAdd("ConvertFrom-CSV", "ConvertFrom-Csv");
            correctionAdd("convertfrom-csv", "ConvertFrom-Csv");

            //Of PowerShell...
            correctionAdd("resolve-path", "Resolve-Path");

            //Of PowerShell...
            correctionAdd("import-csv", "Import-Csv");
            correctionAdd("Import-CSV", "Import-Csv");
            correctionAdd("Import-csv", "Import-Csv");

            //Of PowerShell...
            correctionAdd("foreach-object", "ForEach-Object");
            correctionAdd("ForEach", "ForEach-Object");
            correctionAdd("Foreach", "ForEach-Object");
            correctionAdd("foreach", "ForEach-Object");
            correctionAdd("%", "ForEach-Object");
            correctionAdd("Foreach-Object", "ForEach-Object");

            //Of PowerShell...
            correctionAdd("get-aduser", "Get-ADUser");

            //Of PowerShell...
            correctionAdd("select-string", "Select-String");

            //Of PowerShell...
            correctionAdd("set-strictmode", "Set-StrictMode");

            //Of PowerShell... What about "clear"?
            correctionAdd("clear-host", "Clear-Host");
            correctionAdd("cls", "Clear-Host");
            correctionAdd("CLS", "Clear-Host");

            //Of PowerShell...
            correctionAdd("exit", "Exit");

            //Of PowerShell...
            correctionAdd("join-path", "Join-Path");

            //Of PowerShell...
            correctionAdd("new-item", "New-Item");
            correctionAdd("ni", "New-Item");

            //Of PowerShell...
            correctionAdd("out-null", "Out-Null");

            //Of PowerShell...
            correctionAdd("Invoke-Web-Request", "Invoke-WebRequest");

            correctionAdd("Unfortunatly", "unfortunately");
            correctionAdd("unfortunatly", "unfortunately");
            correctionAdd("unfortunatley", "unfortunately");
            correctionAdd("unfortunatelly", "unfortunately");
            correctionAdd("unfortunly", "unfortunately");
            correctionAdd("unfortunalty", "unfortunately");
            correctionAdd("unfortunetly", "unfortunately");
            correctionAdd("Unfornunately", "unfortunately");
            correctionAdd("unfornunately", "unfortunately");
            correctionAdd("Unfurtunatly", "unfortunately");
            correctionAdd("unfurtunatly", "unfortunately");
            correctionAdd("Unfurtunately", "unfortunately");
            correctionAdd("unfurtunately", "unfortunately");

            correctionAdd("beggining", "beginning");
            correctionAdd("beginng", "beginning");
            correctionAdd("begining", "beginning");
            correctionAdd("begginning", "beginning");
            correctionAdd("beginnin", "beginning");
            correctionAdd("beginging", "beginning");
            correctionAdd("begning", "beginning");

            correctionAdd("begginer", "beginner");
            correctionAdd("bigginer", "beginner");
            correctionAdd("begginner", "beginner");
            correctionAdd("beginer", "beginner");

            correctionAdd("bom", "BOM");
            correctionAdd("byte order mark", "BOM");

            correctionAdd("mpeg", "MPEG");

            correctionAdd("Rtsp", "RTSP");
            correctionAdd("rtsp", "RTSP");

            correctionAdd("FileSystem", "file system");
            correctionAdd("FS", "file system");
            correctionAdd("filesystem", "file system");
            correctionAdd("file-system", "file system");
            correctionAdd("file sytem", "file system");
            correctionAdd("fs", "file system");
            correctionAdd("filesytem", "file system");

            correctionAdd("mmc", "MMC");

            correctionAdd("angstrom", "Ångström Linux");
            correctionAdd("angstrom linux", "Ångström Linux");
            correctionAdd("Angstrom Linux", "Ångström Linux");
            correctionAdd("Angstrom", "Ångström Linux");

            correctionAdd("scifi", "science fiction");
            correctionAdd("sci-fi", "science fiction");
            correctionAdd("sci fi", "science fiction");
            correctionAdd("Sci Fi", "science fiction");
            correctionAdd("Scifi", "science fiction");
            correctionAdd("Sci-Fi", "science fiction");
            correctionAdd("SciFi", "science fiction");
            correctionAdd("SFI", "science fiction");

            correctionAdd("Ping", "ping");

            correctionAdd("dslr", "DSLR");

            correctionAdd("atm", "at the moment");

            correctionAdd("RavenDb", "RavenDB");
            correctionAdd("ravendb", "RavenDB");

            correctionAdd("SalesForce", "Salesforce");
            correctionAdd("salesforce", "Salesforce");
            correctionAdd("sales force", "Salesforce");

            correctionAdd("WEB API", "Web API");
            correctionAdd("WebAPI", "Web API");
            correctionAdd("web API", "Web API");
            correctionAdd("webapi", "Web API");
            correctionAdd("web api", "Web API");

            correctionAdd("duck-typing", "duck typing");

            correctionAdd("cassini", "Cassini Web server");
            correctionAdd("cassini web server", "Cassini Web server");

            correctionAdd("wdsl", "WDSL");

            correctionAdd("Blas", "BLAS");
            correctionAdd("blas", "BLAS");

            correctionAdd("homebrew", "Homebrew");
            correctionAdd("home brew", "Homebrew");
            correctionAdd("brew", "Homebrew");
            correctionAdd("Brew", "Homebrew");

            correctionAdd("EGG", "egg");

            correctionAdd("usart", "USART");

            correctionAdd("isr", "ISR");
            correctionAdd("Interrupt Service Routine", "ISR");

            correctionAdd("avrstudio", "AVR Studio");
            correctionAdd("AVR STUDIO", "AVR Studio");
            correctionAdd("AVRstudio", "AVR Studio");

            correctionAdd("avrdude", "AVRDUDE");
            correctionAdd("avr-dude", "AVRDUDE");
            correctionAdd("AVRDude", "AVRDUDE");

            correctionAdd("avr", "AVR");

            correctionAdd("Sketchup", "SketchUp");

            correctionAdd("kml", "KML");

            correctionAdd("puppy linux", "Puppy Linux");

            correctionAdd("elasticsearch", "Elasticsearch");
            correctionAdd("Elasticsearches", "Elasticsearch");
            correctionAdd("ElasticSearch", "Elasticsearch");
            correctionAdd("Elastic Search", "Elasticsearch");
            correctionAdd("Elastic search", "Elasticsearch");
            correctionAdd("elastic search", "Elasticsearch");

            correctionAdd("webrtc", "WebRTC");
            correctionAdd("webRTC", "WebRTC");

            correctionAdd("puppet", "Puppet");

            correctionAdd("ISE", "PowerShell ISE");
            correctionAdd("powershell ISE", "PowerShell ISE");
            correctionAdd("powershell ise", "PowerShell ISE");
            correctionAdd("ise", "PowerShell ISE");
            correctionAdd("Powershell ISE", "PowerShell ISE");

            correctionAdd("existance", "existence");
            correctionAdd("existanse", "existence");
            correctionAdd("existense", "existence");

            correctionAdd("Ps2", "PS/2");
            correctionAdd("ps2", "PS/2");
            correctionAdd("PS2", "PS/2");

            correctionAdd("kernal", "kernel");

            correctionAdd("kde4", "KDE&nbsp;4");
            correctionAdd("KDE4", "KDE&nbsp;4");

            correctionAdd("kde", "KDE");

            correctionAdd("DAL", "data access layer");
            correctionAdd("Data Access Layer", "data access layer");

            correctionAdd("scilab", "Scilab");
            correctionAdd("SciLab", "Scilab");
            correctionAdd("SCILAB", "Scilab");

            correctionAdd("maths", "mathematics");
            correctionAdd("math", "mathematics");

            correctionAdd("task scheduler", "Task Scheduler");
            correctionAdd("Task Scheudler", "Task Scheduler");

            correctionAdd("N Audio", "NAudio");
            correctionAdd("Naudio", "NAudio");

            correctionAdd("iscsi", "iSCSI");
            correctionAdd("ISCSI", "iSCSI");

            correctionAdd("iops", "IOPS");

            correctionAdd("launchpad", "LaunchPad"); // The Texas Instruments hardware platform,
                                                     // not Ubuntu/Canonical's "Launchpad",
                                                     // <https://wiki.ubuntu.com/Launchpad>.

            correctionAdd("Launchpad", "LaunchPad"); // The Texas Instruments hardware platform,
                                                     // not Ubuntu/Canonical's "Launchpad",
                                                     // <https://wiki.ubuntu.com/Launchpad>.

            correctionAdd("Volt", "volt");
            correctionAdd("v", "volt");
            correctionAdd("V", "volt"); //Not incorrect, but in order to lookup "V".
            correctionAdd("Volts", "volt");
            correctionAdd("volts", "volt");

            correctionAdd("a", "ampere"); //Lots of ambiguity!!!
            correctionAdd("A", "ampere"); //Not incorrect, just another form
            correctionAdd("Amps", "ampere"); //Not incorrect, just another form
            correctionAdd("amps", "ampere"); //Not incorrect, just another form
            correctionAdd("amp", "ampere"); //Not incorrect, just another form
            correctionAdd("Amp", "ampere"); //Not incorrect, just another form
            correctionAdd("Ampere", "ampere");

            correctionAdd("w", "watt"); //Lots of ambiguity!!!
            correctionAdd("W", "watt"); //Not incorrect, just another form
            correctionAdd("Watt", "watt");

            correctionAdd("work around", "workaround");
            correctionAdd("work-around", "workaround");
            correctionAdd("work arround", "workaround");

            correctionAdd("thunderbird", "Thunderbird");
            correctionAdd("thunterbird", "Thunderbird");
            correctionAdd("ThunderBird", "Thunderbird");

            correctionAdd("raid", "RAID");
            correctionAdd("Raid", "RAID");

            correctionAdd("raid 1", "RAID&nbsp;1");

            correctionAdd("o/p", "output"); //Can it be confused with "OP"?
            correctionAdd("ouptut", "output");
            correctionAdd("O/p", "output"); //Can it be confused with "OP"? // Not 100% correct - case.
            correctionAdd("O/P", "output");
            correctionAdd("ouput", "output");
            correctionAdd("OutPut", "output");
            correctionAdd("out put", "output");
            correctionAdd("Ouput", "output");
            correctionAdd("Output", "output");
            correctionAdd("out out", "output");

            correctionAdd("Bit-locker", "BitLocker");
            correctionAdd("bitlocker", "BitLocker");
            correctionAdd("Bitlocker", "BitLocker");

            correctionAdd("Common lisp", "Common Lisp");
            correctionAdd("common list", "Common Lisp");
            correctionAdd("common-lisp", "Common Lisp");
            correctionAdd("common lisp", "Common Lisp");
            correctionAdd("common Lisp", "Common Lisp");

            correctionAdd("prolog", "Prolog");

            correctionAdd("sse", "SSE");

            correctionAdd("clos", "CLOS");

            correctionAdd("fxml", "FXML");

            correctionAdd("icmp", "ICMP");

            correctionAdd("kickstarter", "Kickstarter");
            correctionAdd("KickStarter", "Kickstarter");

            correctionAdd("quicktime", "QuickTime");
            correctionAdd("Quicktime", "QuickTime");

            correctionAdd("Express JS", "Express.js");
            correctionAdd("Express.JS", "Express.js");
            correctionAdd("express.js", "Express.js");
            correctionAdd("express", "Express.js");
            correctionAdd("Express", "Express.js");
            correctionAdd("expressjs", "Express.js");
            correctionAdd("expressJs", "Express.js");
            correctionAdd("ExpressJS", "Express.js");
            correctionAdd("espress", "Express.js");
            correctionAdd("express js", "Express.js");

            correctionAdd("ember", "Ember.js");
            correctionAdd("Ember", "Ember.js");
            correctionAdd("Emberjs", "Ember.js");
            correctionAdd("EmberJS", "Ember.js");
            correctionAdd("EmberJs", "Ember.js");
            correctionAdd("Ember.Js", "Ember.js");
            correctionAdd("Ember.JS", "Ember.js");

            correctionAdd("backbone", "Backbone.js");
            correctionAdd("Backbone", "Backbone.js");
            correctionAdd("Backbonejs", "Backbone.js");
            correctionAdd("BackboneJS", "Backbone.js");
            correctionAdd("BackboneJs", "Backbone.js");
            correctionAdd("backbonejs", "Backbone.js");
            correctionAdd("backbone.js", "Backbone.js");
            correctionAdd("Backbone JS", "Backbone.js");
            correctionAdd("Backboone", "Backbone.js");

            correctionAdd("Froyo", "Android&nbsp;2.2 (Froyo)");
            correctionAdd("2.2", "Android&nbsp;2.2 (Froyo)");
            correctionAdd("FROYO", "Android&nbsp;2.2 (Froyo)");
            correctionAdd("froyo", "Android&nbsp;2.2 (Froyo)");

            correctionAdd("gingerbread", "Android&nbsp;2.3 (Gingerbread)");
            correctionAdd("Android 2.3", "Android&nbsp;2.3 (Gingerbread)");
            correctionAdd("Android version 2.3", "Android&nbsp;2.3 (Gingerbread)");
            correctionAdd("Gingerbread", "Android&nbsp;2.3 (Gingerbread)");

            correctionAdd("Honeycomb", "Android&nbsp;3 (Honeycomb)");
            correctionAdd("HoneyComb", "Android&nbsp;3 (Honeycomb)");
            correctionAdd("Android&nbsp;3", "Android&nbsp;3 (Honeycomb)");

            correctionAdd("Android 4", "Android&nbsp;4.0 (Ice Cream Sandwich)");
            correctionAdd("Android 4.0", "Android&nbsp;4.0 (Ice Cream Sandwich)");
            correctionAdd("Android version 4.0", "Android&nbsp;4.0 (Ice Cream Sandwich)");
            correctionAdd("Ice Cream Sandwich", "Android&nbsp;4.0 (Ice Cream Sandwich)");
            correctionAdd("ice cream sandwich", "Android&nbsp;4.0 (Ice Cream Sandwich)");
            correctionAdd("icecream", "Android&nbsp;4.0 (Ice Cream Sandwich)");
            correctionAdd("Android&nbsp;4.0", "Android&nbsp;4.0 (Ice Cream Sandwich)");
            correctionAdd("ICS", "Android&nbsp;4.0 (Ice Cream Sandwich)");

            correctionAdd("JB", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("Jelly Bean", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("JellyBean", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("Jellybean 4.2", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("Jellybean", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("4.2", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("4.2.1", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("Android 4.2.1", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("Android 4.2", "Android&nbsp;4.1 (Jelly Bean)"); // Actually 4.1 - 4.3.1.

            correctionAdd("4.3", "Android&nbsp;4.3 (Jelly Bean)"); // Actually 4.1 - 4.3.1.
            correctionAdd("Android 4.3", "Android&nbsp;4.3 (Jelly Bean)"); // Actually 4.1 - 4.3.1.

            correctionAdd("KitKat", "Android 4.4.x (KitKat)");
            correctionAdd("Kitkat", "Android 4.4.x (KitKat)");
            correctionAdd("Android 4.4", "Android 4.4.x (KitKat)");
            correctionAdd("Android 4.4.x", "Android 4.4.x (KitKat)");
            correctionAdd("kitkat", "Android 4.4.x (KitKat)");
            correctionAdd("4.4", "Android 4.4.x (KitKat)");
            correctionAdd("KITKAT", "Android 4.4.x (KitKat)");
            correctionAdd("Kit Kat", "Android 4.4.x (KitKat)");

            correctionAdd("Lollipop", "Android 5.0 (Lollipop)");
            correctionAdd("Android 5", "Android 5.0 (Lollipop)");
            correctionAdd("Lolipop", "Android 5.0 (Lollipop)");
            correctionAdd("Android Lollipop", "Android 5.0 (Lollipop)");
            correctionAdd("lollipop", "Android 5.0 (Lollipop)");
            correctionAdd("5.1", "Android 5.1 (Lollipop)");

            correctionAdd("Marshmallow", "Android 6.0 (Marshmallow)");
            correctionAdd("Android 6", "Android 6.0 (Marshmallow)");
            correctionAdd("Marshmallo", "Android 6.0 (Marshmallow)");
            correctionAdd("marshmallo", "Android 6.0 (Marshmallow)");
            correctionAdd("Marshamallow", "Android 6.0 (Marshmallow)");
            correctionAdd("marshmallow", "Android 6.0 (Marshmallow)");
            correctionAdd("MARSHMALLOW", "Android 6.0 (Marshmallow)");
            correctionAdd("MarshMallow", "Android 6.0 (Marshmallow)");

            correctionAdd("inheritence", "inheritance");
            correctionAdd("inherittance", "inheritance");

            //Other: "distributed version control", "decentralized version control"
            correctionAdd("DVCS", "Distributed revision control"); //Expansion, actually "distributed version control system"
            correctionAdd("DRCS", "Distributed revision control"); //Expansion, actually "distributed revision control system"
            correctionAdd("distributed version control system", "Distributed revision control");
            correctionAdd("distributed revision control system", "Distributed revision control");

            correctionAdd("VCS", "version control system"); //Expansion, actually ""
            correctionAdd("vcs", "version control system");

            correctionAdd("mb", "&nbsp;MB"); //Assuming bytes, not bits...
            correctionAdd("MB", "&nbsp;MB"); //Assuming bytes, not bits...
            correctionAdd("mega byte", "&nbsp;MB");
            correctionAdd("megabyte", "&nbsp;MB");

            correctionAdd("gb", "&nbsp;GB"); //Assuming bytes, not bits...
            correctionAdd("GByte", "&nbsp;GB");
            correctionAdd("GB", "&nbsp;GB"); //For convenience
            correctionAdd("gigabyte", "&nbsp;GB"); //Shrinking...

            correctionAdd("kb", "&nbsp;KB"); //Assuming bytes, not bits...
            correctionAdd("KB", "&nbsp;KB"); //Assuming bytes, not bits...
            correctionAdd("Kbyte", "&nbsp;KB");

            correctionAdd("TurboC++", "Turbo C++");
            correctionAdd("turbo c", "Turbo C++"); //Is this true?
            correctionAdd("turbo C", "Turbo C++");
            correctionAdd("Turbo C", "Turbo C++");
            correctionAdd("TurboC", "Turbo C++");

            correctionAdd("rdbms", "RDBMS");

            correctionAdd("tcp", "TCP");

            correctionAdd("tcp/ip", "TCP/IP");
            correctionAdd("TCPIP", "TCP/IP");
            correctionAdd("TCP / IP", "TCP/IP");
            correctionAdd("tcp-ip", "TCP/IP");
            correctionAdd("tcp ip", "TCP/IP");
            correctionAdd("TCP IP", "TCP/IP");

            correctionAdd("IOT", "IoT");
            correctionAdd("Internet of Things", "IoT"); //Contraction...
            correctionAdd("internet of things", "IoT"); //Contraction...
            correctionAdd("Io T", "IoT");
            correctionAdd("Internet of things", "IoT");
            correctionAdd("iOT", "IoT");
            correctionAdd("iot", "IoT");

            correctionAdd("approx", "approximately");
            correctionAdd("aprox", "approximately");
            correctionAdd("~", "approximately"); //Not unique...
            correctionAdd("approximatedly", "approximately");
            correctionAdd("approssimaly", "approximately");
            correctionAdd("aproximate", "approximately"); // Not 100% correct
            correctionAdd("approximate", "approximately"); // Not 100% correct

            correctionAdd("corflags", "CorFlags");
            correctionAdd("Corflags", "CorFlags");
            correctionAdd("corflags.exe", "CorFlags");

            correctionAdd("StdOut", "standard output");
            correctionAdd("stdout", "standard output");

            correctionAdd("Solid", "SOLID");
            correctionAdd("solid", "SOLID");

            correctionAdd("yagni", "YAGNI");

            correctionAdd("Kiss", "KISS");

            correctionAdd("2 cent", "two cents");
            correctionAdd("2 cents", "two cents");
            correctionAdd("2cents", "two cents");
            correctionAdd("2 Cents", "two cents");

            correctionAdd("Big Endian", "big-endian");
            correctionAdd("BigEndian", "big-endian");
            correctionAdd("bigendian", "big-endian");

            correctionAdd("Little Endian", "little-endian");
            correctionAdd("LittleEndian", "little-endian");
            correctionAdd("littleendian", "little-endian");

            correctionAdd("fits", "FITS");

            correctionAdd("start-process", "Start-Process");

            correctionAdd("eternal September", "Eternal September");

            correctionAdd("msgbox", "MsgBox");

            correctionAdd("K&R", "The C Programming Language");
            correctionAdd("k&r", "The C Programming Language");
            correctionAdd("k&R", "The C Programming Language");

            correctionAdd("phd", "PhD");
            correctionAdd("PHD", "PhD");

            correctionAdd("togheter", "together");

            correctionAdd("follownig", "following");
            correctionAdd("follwoing", "following");
            correctionAdd("folloiwng", "following");
            correctionAdd("folowing", "following");
            correctionAdd("follwing", "following");

            correctionAdd("thougth", "thought");
            correctionAdd("thougt", "thought");
            correctionAdd("tought", "thought");

            correctionAdd("idl", "IDL");

            correctionAdd("K12", "K–12");
            correctionAdd("k-12", "K–12");

            correctionAdd("psu", "PSU");

            correctionAdd("visualforce", "Visualforce");
            correctionAdd("visual force", "Visualforce");

            correctionAdd("elementary os", "Elementary&nbsp;OS");
            correctionAdd("elementary OS", "Elementary&nbsp;OS");


            correctionAdd("vhdl", "VHDL");

            correctionAdd("type script", "TypeScript");
            correctionAdd("typescript", "TypeScript");
            correctionAdd("Typescript", "TypeScript");
            correctionAdd("TS", "TypeScript");
            correctionAdd("Typescritp", "TypeScript");
            correctionAdd("typescritp", "TypeScript");
            correctionAdd("typoscript", "TypeScript");
            // "I have this code in my typoscript"
            correctionAdd("typescrypt", "TypeScript");
            correctionAdd("TSC", "TypeScript");
            correctionAdd("tsc", "TypeScript");
            correctionAdd("ts", "TypeScript");

            correctionAdd("OBD2", "OBD-II");
            correctionAdd("ODB2", "OBD-II");
            correctionAdd("obd II", "OBD-II");
            correctionAdd("OBD II", "OBD-II");
            correctionAdd("OBD", "OBD-II");
            correctionAdd("OBDII", "OBD-II");
            correctionAdd("ODBII", "OBD-II");
            correctionAdd("odb2", "OBD-II");
            correctionAdd("obd 2", "OBD-II");

            correctionAdd("cuda", "CUDA");
            correctionAdd("Cuda", "CUDA");

            correctionAdd("INSTAGRAM", "Instagram");
            correctionAdd("instagram", "Instagram");
            correctionAdd("insta", "Instagram");
            correctionAdd("Insta", "Instagram");

            correctionAdd("beautifulsoap", "Beautiful Soup");
            correctionAdd("beautifulsoup", "Beautiful Soup");
            correctionAdd("BeautifulSoup", "Beautiful Soup");
            correctionAdd("beautiful soup", "Beautiful Soup");
            correctionAdd("Beautiful soup", "Beautiful Soup");
            correctionAdd("BeatifulSoup", "Beautiful Soup");

            correctionAdd("aix", "AIX");

            correctionAdd("hpux", "HP-UX");
            correctionAdd("HPUX", "HP-UX");

            correctionAdd("SUNOS", "SunOS");
            correctionAdd("sunos", "SunOS");

            correctionAdd("usefull", "useful");
            correctionAdd("use full", "useful");

            correctionAdd("gprof", "Gprof");

            correctionAdd("Memoization", "memoization");

            correctionAdd("spa", "single-page application");
            correctionAdd("single page application", "single-page application");
            correctionAdd("Single Page Applications", "single-page application");
            correctionAdd("single page applications", "single-page application");
            correctionAdd("SPA", "single-page application");
            correctionAdd("Single-Page Application", "single-page application");
            correctionAdd("single page app", "single-page application");

            correctionAdd("german", "German");

            correctionAdd("germany", "Germany");

            correctionAdd("french", "French");

            correctionAdd("british", "British");

            correctionAdd("f-15", "F-15");
            correctionAdd("f15", "F-15");
            correctionAdd("F15", "F-15");

            correctionAdd("f-16", "F-16");
            correctionAdd("f16", "F-16");
            correctionAdd("F16", "F-16");
            correctionAdd("F 16", "F-16");
            correctionAdd("f 16", "F-16");

            correctionAdd("pls", "please");
            correctionAdd("plz", "please");
            correctionAdd("Plz", "please"); // Not 100% correct - case.
            correctionAdd("Pls", "please"); // Not 100% correct - case.

            correctionAdd("1 Liner", "one-liner");
            correctionAdd("one liner", "one-liner");
            correctionAdd("oneliner", "one-liner");
            correctionAdd("one line", "one-liner");
            correctionAdd("1 liner", "one-liner");
            correctionAdd("oneline", "one-liner");
            correctionAdd("onliner", "one-liner");

            correctionAdd("min", "minimum");
            correctionAdd("minmal", "minimum");
            correctionAdd("minium", "minimum");

            correctionAdd("max", "maximum");
            correctionAdd("maxium", "maximum");

            correctionAdd("ecom", "E-commerce");
            correctionAdd("e-Commerce", "E-commerce");
            correctionAdd("eCommerce", "E-commerce");
            correctionAdd("ecommerce", "E-commerce");
            correctionAdd("ECommerce", "E-commerce");
            correctionAdd("Ecommerce", "E-commerce");
            correctionAdd("e-commerce", "E-commerce");

            correctionAdd("activeperl", "ActivePerl");
            correctionAdd("active perl", "ActivePerl");
            correctionAdd("Active perl", "ActivePerl");
            correctionAdd("Activeperl", "ActivePerl");
            correctionAdd("Active Perl", "ActivePerl");
            correctionAdd("activePerl", "ActivePerl");
            correctionAdd("Activestate Perl", "ActivePerl");
            correctionAdd("ActiveState Perl", "ActivePerl");

            correctionAdd("setup", "set up"); //As a verb
            correctionAdd("set-up", "set up");

            correctionAdd("runon", "run-on sentence"); //For expansion
            correctionAdd("run on", "run-on sentence"); //For expansion
            correctionAdd("run-on", "run-on sentence");

            correctionAdd("gpio", "GPIO");

            correctionAdd("dreamweaver", "Dreamweaver");
            correctionAdd("dreame weaver", "Dreamweaver");
            correctionAdd("DW", "Dreamweaver");
            correctionAdd("DreamWeawer", "Dreamweaver");
            correctionAdd("dreamwaver", "Dreamweaver");
            correctionAdd("DreamWeaver", "Dreamweaver");

            correctionAdd("css", "CSS");
            correctionAdd("Css", "CSS");

            correctionAdd("CSS3", "CSS&nbsp;3");
            correctionAdd("CSS 3", "CSS&nbsp;3"); //Sort of self.
            correctionAdd("css3", "CSS&nbsp;3");
            correctionAdd("CSS-3", "CSS&nbsp;3");

            correctionAdd("perms", "permissions");

            correctionAdd("login", "log in");

            correctionAdd("i3", "Core&nbsp;i3");
            correctionAdd("Core i3", "Core&nbsp;i3");
            correctionAdd("core i3", "Core&nbsp;i3");

            correctionAdd("i5", "Core&nbsp;i5");
            correctionAdd("core i5", "Core&nbsp;i5");
            correctionAdd("Core i5", "Core&nbsp;i5");

            correctionAdd("i7", "Core&nbsp;i7");
            correctionAdd("Core i7", "Core&nbsp;i7");
            correctionAdd("core i7", "Core&nbsp;i7");
            correctionAdd("core-i7", "Core&nbsp;i7");

            correctionAdd("orca", "Orca");
            correctionAdd("ORCA", "Orca");

            correctionAdd("pseudo code", "pseudocode");
            correctionAdd("Pseudo Code", "pseudocode");
            correctionAdd("pseudyo", "pseudocode"); //Not exact, no "code" in the input
            correctionAdd("Pseudo-code", "pseudocode");
            correctionAdd("pseudo-code", "pseudocode");
            correctionAdd("psuedocode", "pseudocode");
            correctionAdd("psudocode", "pseudocode");
            correctionAdd("psuedo code", "pseudocode");
            correctionAdd("psuedo-code", "pseudocode");
            correctionAdd("Psuedocode", "pseudocode");

            correctionAdd("ldo", "LDO");

            correctionAdd("t-shirt", "T-shirt");
            correctionAdd("Tshirt", "T-shirt");
            correctionAdd("Tee", "T-shirt");
            correctionAdd("T-Shirt", "T-shirt");

            correctionAdd("osmc", "OSMC");

            correctionAdd("Black-Holes", "black hole"); //Not 100% correct - plural.
            correctionAdd("Black-holes", "black hole"); //Not 100% correct - plural.
            correctionAdd("black-holes", "black hole"); //Not 100% correct - plural.
            correctionAdd("Black-hole", "black hole"); //Not 100% correct - plural.
            correctionAdd("black-hole", "black hole"); //Not 100% correct - plural.
            correctionAdd("Blackhole", "black hole");
            correctionAdd("blackhole", "black hole");

            correctionAdd("schottky", "Schottky");
            correctionAdd("shottky", "Schottky");
            correctionAdd("Schottkey", "Schottky");
            correctionAdd("SCHOTTKY", "Schottky");

            correctionAdd("doesnt", "doesn’t");
            correctionAdd("doest", "doesn’t");
            correctionAdd("dosent", "doesn’t");
            correctionAdd("deosn't", "doesn’t");
            correctionAdd("dosen't", "doesn’t");
            correctionAdd("doens't", "doesn’t");
            correctionAdd("doestnt", "doesn’t");
            correctionAdd("dosn't", "doesn’t");
            correctionAdd("doen't", "doesn’t");
            correctionAdd("doenst", "doesn’t");
            correctionAdd("doesent", "doesn’t");
            correctionAdd("desn’t", "doesn’t");
            correctionAdd("dosnt", "doesn’t");
            correctionAdd("does’t", "doesn’t");
            correctionAdd("doesn't", "doesn’t");
            correctionAdd("doesn`t", "doesn’t");
            correctionAdd("doesn´t", "doesn’t");

            correctionAdd("smart phone", "smartphone");
            correctionAdd("SmartPhone", "smartphone");

            correctionAdd("grammer", "grammar");

            correctionAdd("chinese", "Chinese");

            correctionAdd("atleast", "at least");
            correctionAdd("alteast", "at least");
            correctionAdd("at-least", "at least");

            correctionAdd("freq", "frequency");
            correctionAdd("frequancy", "frequency");

            correctionAdd("basicly", "basically");
            correctionAdd("Basicaly", "basically"); // Not 100% correct (case)
            correctionAdd("basicaly", "basically");
            correctionAdd("Basicly", "basically");
            correctionAdd("basicially", "basically");
            correctionAdd("basiclly", "basically");

            correctionAdd("mdi", "MDI");

            correctionAdd("chocolatey", "Chocolatey");
            correctionAdd("chochcolately", "Chocolatey");
            correctionAdd("chocolately", "Chocolatey");
            correctionAdd("choco", "Chocolatey");
            correctionAdd("Choco", "Chocolatey");

            correctionAdd("psobject", "PSObject");
            correctionAdd("PsObject", "PSObject");

            correctionAdd("get-variable", "Get-Variable");

            correctionAdd("SRP", "single responsibility principle");
            correctionAdd("Single responsability principle", "single responsibility principle");
            correctionAdd("single responsability principle", "single responsibility principle");
            correctionAdd("Single Responsibility Principal", "single responsibility principle");
            correctionAdd("Single Object Responsibility", "single responsibility principle");
            correctionAdd("single responsibility", "single responsibility principle");
            correctionAdd("Single Responsibility principle", "single responsibility principle");

            correctionAdd("pcb", "PCB");

            correctionAdd("autuomn", "autumn");

            correctionAdd("accomodate", "accommodate");

            correctionAdd("obviusly", "obviously");
            correctionAdd("obvisouly", "obviously");

            correctionAdd("VisualSvn", "VisualSVN");

            correctionAdd("trello", "Trello");

            correctionAdd("logo", "Logo");

            correctionAdd("algol", "ALGOL");
            correctionAdd("Algol", "ALGOL");

            correctionAdd("what so ever", "whatsoever");
            correctionAdd("what-so-ever", "whatsoever");
            correctionAdd("whatsover", "whatsoever");

            correctionAdd("dmz", "DMZ");

            correctionAdd("didnt", "didn’t");
            correctionAdd("didin't", "didn’t");
            correctionAdd("din't", "didn’t");
            correctionAdd("din`t", "didn’t");
            correctionAdd("didn`t", "didn’t");
            correctionAdd("did't", "didn’t");
            correctionAdd("dint", "didn’t");
            correctionAdd("didn't", "didn’t"); // Quora...

            correctionAdd("thats", "that's");
            correctionAdd("Thats", "that's"); // Not 100% correct (case)
            correctionAdd("That’s", "that's"); // Not 100% correct (case)
            correctionAdd("that’s", "that's");

            // correctionAdd("v.s", "vs."); // Bug...
            correctionAdd("versus", "vs."); //
            correctionAdd("v.s", "vs."); // We can not use "v.s."
            // correctionAdd("vs", "vs."); // Bug... Disabled - including it will
            // result in "Double entry for the
            // key 'vs' (value is 'vs.')."
            correctionAdd("v/s", "vs."); //

            correctionAdd("Where ever", "wherever");
            correctionAdd("where ever", "wherever");

            correctionAdd("whats", "what's");
            correctionAdd("Whats", "what's"); // Not 100% correct - case.
            correctionAdd("wats", "what's");
            correctionAdd("what’s", "what's"); // For Quora.

            correctionAdd("american", "American");

            correctionAdd("indian", "Indian");
            //  correctionAdd("", "Indian");   missing one
            //     from <https://www.quora.com/Is-it-true-that-Indian-software-engineers-are-considered-very-bad-hires-in-the-USA>...

            correctionAdd("grad", "graduate");

            correctionAdd("Tant", "tantalum capacitor");

            correctionAdd("knowlege", "knowledge");
            correctionAdd("knowlodge", "knowledge");
            correctionAdd("KNAWLEDGE", "knowledge");
            correctionAdd("knawledge", "knowledge");

            correctionAdd("necessaraily", "necessarily");
            correctionAdd("nessessarily", "necessarily");
            correctionAdd("neccesarily", "necessarily");
            correctionAdd("necesarily", "necessarily");
            correctionAdd("necessarilly", "necessarily");
            correctionAdd("neceserly", "necessarily");
            correctionAdd("necessaryly", "necessarily");
            correctionAdd("necessaraly", "necessarily");
            correctionAdd("neccessarily", "necessarily");
            correctionAdd("neccessaraly", "necessarily");
            correctionAdd("necesseraly", "necessarily");
            correctionAdd("necesserily", "necessarily");
            correctionAdd("necesseraily", "necessarily");
            correctionAdd("necesserailly", "necessarily");
            correctionAdd("necesserilly", "necessarily");
            correctionAdd("neccesserilly", "necessarily");
            correctionAdd("necceserilly", "necessarily");
            correctionAdd("necceserily", "necessarily");

            correctionAdd("neccesary", "necessary");
            correctionAdd("necassary", "necessary");
            correctionAdd("neccessary", "necessary");
            correctionAdd("necesary", "necessary");
            correctionAdd("nessary", "necessary");
            correctionAdd("nessisary", "necessary");
            correctionAdd("necssary", "necessary");
            correctionAdd("necessairy", "necessary");
            correctionAdd("nessecary", "necessary");
            correctionAdd("necesery", "necessary");
            correctionAdd("necessery", "necessary");

            correctionAdd("unneccessary", "unnecessary");
            correctionAdd("unncessary", "unnecessary");
            correctionAdd("unessary", "unnecessary");
            correctionAdd("Unnessessary", "unnecessary");
            correctionAdd("unnessessary", "unnecessary");
            correctionAdd("unnessesary", "unnecessary");
            correctionAdd("Unneccesary", "unnecessary");
            correctionAdd("uneccesary", "unnecessary");
            correctionAdd("unneccesary", "unnecessary");
            correctionAdd("unnecesary", "unnecessary");
            correctionAdd("unnecesassry", "unnecessary");
            correctionAdd("unnecesessry", "unnecessary");
            correctionAdd("unnecesesary", "unnecessary");
            correctionAdd("unnecessesary", "unnecessary");
            correctionAdd("unnecessessary", "unnecessary");

            correctionAdd("CD", "continuous delivery"); // Potentially conflict with CD / CD-ROM

            correctionAdd("Topcoder", "TopCoder");
            correctionAdd("topcoder", "TopCoder");

            correctionAdd("codeforces", "Codeforces");

            correctionAdd("codechef", "CodeChef");

            correctionAdd("firemonkey", "FireMonkey");
            correctionAdd("Firemonkey", "FireMonkey");

            correctionAdd("Berekeley", "Berkeley");

            correctionAdd("Berekeley DB", "Berkeley&nbsp;DB");
            correctionAdd("Berkeley DB", "Berkeley&nbsp;DB"); //Self (effectively)
            correctionAdd("BerekelyDB", "Berkeley&nbsp;DB");

            correctionAdd("verilog", "Verilog");

            correctionAdd("isnt", "isn't");
            correctionAdd("aint", "isn't");
            correctionAdd("ain't", "isn't");
            correctionAdd("isn’t", "isn't"); //Quora
            correctionAdd("isn´t", "isn't");

            correctionAdd("Ironruby", "IronRuby");

            correctionAdd("newb", "newbie");
            correctionAdd("newbs", "newbie"); // Not 100% correct - plural
            correctionAdd("noob", "newbie");
            correctionAdd("newby", "newbie");
            correctionAdd("newbee", "newbie");

            correctionAdd("Foxpro", "FoxPro");
            correctionAdd("foxpro", "FoxPro");

            correctionAdd("opencart", "OpenCart");
            correctionAdd("Opencart", "OpenCart");
            correctionAdd("open cart", "OpenCart");

            correctionAdd("vivaldi", "Vivaldi");

            correctionAdd("some one", "someone");
            correctionAdd("somone", "someone");

            correctionAdd("2-factor auth", "two-factor authentication");
            correctionAdd("2 factor auth", "two-factor authentication");
            correctionAdd("two-factor auth", "two-factor authentication");
            correctionAdd("two factor auth", "two-factor authentication");
            correctionAdd("Two-Factor Authentication", "two-factor authentication");
            correctionAdd("Two Factor Authentication", "two-factor authentication");
            correctionAdd("2FA", "two-factor authentication");
            correctionAdd("two factor authentication", "two-factor authentication");
            correctionAdd("2-factor authentication", "two-factor authentication");
            correctionAdd("Two-factor", "two-factor authentication");
            correctionAdd("two-factor", "two-factor authentication");
            correctionAdd("2 factor", "two-factor authentication");

            correctionAdd("pla", "PLA");

            correctionAdd("abs", "ABS");

            // Conflict with the Unix 'less',
            // <https://linux.die.net/man/1/less>,
            // <https://en.wikipedia.org/wiki/Less_(Unix)>
            correctionAdd("LESS", "Less");
            correctionAdd("less", "Less");

            correctionAdd("anymore", "any more");

            correctionAdd("NGEN", "NGen");
            correctionAdd("ngen", "NGen");
            correctionAdd("Ngen", "NGen");

            correctionAdd("openocd", "OpenOCD");
            correctionAdd("openOCD", "OpenOCD");
            correctionAdd("openosd", "OpenOCD");

            correctionAdd("couldnt", "couldn't");
            correctionAdd("Couldnt", "couldn't"); // Not 100% correct - case.
            correctionAdd("coudln't", "couldn't");
            correctionAdd("coulden't", "couldn't");
            correctionAdd("could'nt", "couldn't");
            correctionAdd("coulnd", "couldn't");
            correctionAdd("coulnd't", "couldn't");
            correctionAdd("cudn't", "couldn't");
            correctionAdd("cudnt", "couldn't");
            correctionAdd("coudn't", "couldn't");

            correctionAdd("no where", "nowhere");

            correctionAdd("avaliable", "available");
            correctionAdd("abailable", "available");
            correctionAdd("availble", "available");
            correctionAdd("availaible", "available");
            correctionAdd("avalible", "available");
            correctionAdd("availible", "available");

            correctionAdd("CPNAEL", "cPanel");
            correctionAdd("cpanel", "cPanel");
            correctionAdd("Cpanel", "cPanel");
            correctionAdd("C-Panel", "cPanel");
            correctionAdd("CPanel", "cPanel");

            correctionAdd("plesk", "Plesk");

            correctionAdd("Bere", "bear");
            correctionAdd("bere", "bear");
            correctionAdd("bare", "bear");

            correctionAdd("gradle", "Gradle");

            correctionAdd("stand alone", "stand-alone");
            correctionAdd("standalone", "stand-alone");
            correctionAdd("stand alowne", "stand-alone");

            correctionAdd("Silican Valley", "Silicon Valley");
            correctionAdd("silicon valley", "Silicon Valley");
            correctionAdd("Silicon valley", "Silicon Valley");
            correctionAdd("The Valley", "Silicon Valley");

            correctionAdd("no body", "nobody");

            correctionAdd("airbnb", "Airbnb");

            correctionAdd("FREEBSD", "FreeBSD");
            correctionAdd("freebsd", "FreeBSD");
            correctionAdd("FressBSD", "FreeBSD");

            correctionAdd("nautilus", "Nautilus");

            correctionAdd("winscp", "WinSCP");
            correctionAdd("winSCP", "WinSCP");
            correctionAdd("WinScp", "WinSCP");
            correctionAdd("Winscp", "WinSCP");

            correctionAdd("google group", "Google Groups");

            correctionAdd("pandas", "Pandas");

            correctionAdd("MITM", "man-in-the-middle attack");
            correctionAdd("man in the middle attack", "man-in-the-middle attack");
            correctionAdd("MitM", "man-in-the-middle attack");
            correctionAdd("mitm", "man-in-the-middle attack");

            correctionAdd("adobe illustrator", "Adobe Illustrator");

            correctionAdd("x axis", "x-axis");

            correctionAdd("jupyter", "Jupyter");

            correctionAdd("zener diode", "Zener diode");
            correctionAdd("zener", "Zener diode");
            correctionAdd("Zener", "Zener diode"); //Expansion
            correctionAdd("Z-diode", "Zener diode"); //Expansion
            correctionAdd("z-diode", "Zener diode"); //Expansion

            correctionAdd("amd", "AMD");
            correctionAdd("AMd", "AMD");

            correctionAdd("mvvm", "MVVM");

            correctionAdd("NPE", "null pointer exception");

            correctionAdd("edge", "Edge");
            correctionAdd("EDGE", "Edge");

            correctionAdd("transitor", "transistor");

            correctionAdd("android studio", "Android Studio");
            correctionAdd("AS", "Android Studio");
            correctionAdd("AndroidStudio", "Android Studio");
            correctionAdd("ANDROID STUDIO", "Android Studio");
            correctionAdd("android-studio", "Android Studio");
            correctionAdd("android studios", "Android Studio");
            correctionAdd("Android studio", "Android Studio");
            correctionAdd("andriod studio", "Android Studio");

            correctionAdd("western digital", "Western Digital");
            correctionAdd("WD", "Western Digital");

            correctionAdd("up-vote", "upvote");
            correctionAdd("UV", "upvote");
            correctionAdd("UpVote", "upvote");
            correctionAdd("up vote", "upvote");

            correctionAdd("down-vote", "downvote");
            correctionAdd("dv", "downvote");
            correctionAdd("DV", "downvote");
            correctionAdd("donwvote", "downvote");
            correctionAdd("down vote", "downvote");

            correctionAdd("4", "every time"); //What is this????
            correctionAdd("every", "every time"); //What is this????
            correctionAdd("everytime", "every time");

            correctionAdd("requirejs", "RequireJS");
            correctionAdd("RequireJs", "RequireJS");

            correctionAdd("mongoose", "Mongoose");

            correctionAdd("principal", "principle");
            correctionAdd("priciple", "principle");

            correctionAdd("etl", "ETL");

            correctionAdd("comparision", "comparison");
            correctionAdd("comaparison", "comparison");
            correctionAdd("comparisson", "comparison");
            correctionAdd("comparrison", "comparison");
            correctionAdd("comparasion", "comparison");
            correctionAdd("comparsion", "comparison");

            correctionAdd("quesition", "question");
            correctionAdd("thread", "question");
            correctionAdd("quesiton", "question");
            correctionAdd("qestion", "question");

            //Note: "c++" does not currently work due to our prefiltering. It would be looked up as "C".
            correctionAdd("Cpp", "C++");
            correctionAdd("cpp", "C++");
            correctionAdd("c++", "C++");
            correctionAdd("CPP", "C++");
            correctionAdd("c plus plus", "C++");

            correctionAdd("c++11", "C++11");
            correctionAdd("C++ 11", "C++11");

            correctionAdd("c++14", "C++14");

            correctionAdd("C++builder", "C++Builder");
            correctionAdd("c++builder", "C++Builder");
            correctionAdd("c++ builder", "C++Builder"); //Does not work!
            correctionAdd("C++ builder", "C++Builder");

            correctionAdd("automatatically", "automatically");
            correctionAdd("automaticlly", "automatically");
            correctionAdd("autoamtically", "automatically");
            correctionAdd("automaticaly", "automatically");

            correctionAdd("Compatability", "compatibility"); //Common misspelling...
            correctionAdd("compatability", "compatibility"); //Common misspelling...
            correctionAdd("compatbile", "compatibility"); //Not 100% correct
            correctionAdd("comatibility", "compatibility");
            correctionAdd("compatibiltity", "compatibility");
            correctionAdd("compatablity", "compatibility");
            correctionAdd("compatibilty", "compatibility");
            correctionAdd("compat", "compatibility");

            correctionAdd("compatable", "compatible");
            correctionAdd("compatibile", "compatible");

            correctionAdd("throughly", "thoroughly");
            correctionAdd("thorougly", "thoroughly");

            correctionAdd("z80", "Z80");

            correctionAdd("lipo", "lithium polymer");
            correctionAdd("Li-Po", "lithium polymer");
            correctionAdd("li-po", "lithium polymer");
            correctionAdd("LiPo", "lithium polymer");
            correctionAdd("Lithium Polymer", "lithium polymer");
            correctionAdd("lithumpolymer", "lithium polymer");

            correctionAdd("LIon", "Li-ion");
            correctionAdd("Lithium-Ion", "Li-ion"); //Contraction!
            correctionAdd("lithium-ion", "Li-ion"); //Contraction!
            correctionAdd("LiIon", "Li-ion");
            correctionAdd("li ion", "Li-ion");
            correctionAdd("li-ion", "Li-ion");

            correctionAdd("Ni-cad", "NiCad");

            correctionAdd("danish", "Danish");

            correctionAdd("DD", "dd"); //Not actual observed

            correctionAdd("bottle neck", "bottleneck");

            correctionAdd("wordperfect", "WordPerfect");
            correctionAdd("word perfect", "WordPerfect");

            correctionAdd("screen shot", "screenshot");
            correctionAdd("screen-shot", "screenshot");
            correctionAdd("Screen-shot", "screenshot");
            correctionAdd("screens shot", "screenshot");

            correctionAdd("world war 2", "World War II");
            correctionAdd("WWII", "World War II");
            correctionAdd("WW2", "World War II");
            correctionAdd("World War 2", "World War II");
            correctionAdd("ww2", "World War II");
            correctionAdd("World war 2", "World War II");
            correctionAdd("world war II", "World War II");
            correctionAdd("World war II", "World War II");

            correctionAdd("where as", "whereas");
            correctionAdd("wheras", "whereas");

            correctionAdd("every thing", "everything");
            correctionAdd("Every thing", "everything");
            correctionAdd("everyting", "everything");
            correctionAdd("everthing", "everything");
            correctionAdd("Everyting", "everything");

            correctionAdd("continiously", "continuously");
            correctionAdd("continously", "continuously");
            correctionAdd("continuosly", "continuously");

            correctionAdd("soa", "SOA");

            correctionAdd("phpStorm", "PhpStorm");
            correctionAdd("PHPstorm", "PhpStorm");
            correctionAdd("PHP Storm", "PhpStorm");
            correctionAdd("PHP storm", "PhpStorm");
            correctionAdd("PHPStorm", "PhpStorm");
            correctionAdd("PHPSTORM", "PhpStorm");
            correctionAdd("phpstorm", "PhpStorm");
            correctionAdd("php storm", "PhpStorm");
            correctionAdd("Phpstorm", "PhpStorm");

            correctionAdd("perf", "performance");
            correctionAdd("preformance", "performance");
            correctionAdd("performace", "performance");

            correctionAdd("esata", "eSATA");

            correctionAdd("slack", "Slack");

            correctionAdd("docker", "Docker");

            correctionAdd("scrapy", "Scrapy");

            correctionAdd("occuring", "occurring");

            correctionAdd("Openstack", "OpenStack");
            correctionAdd("openstack", "OpenStack");

            correctionAdd("built in", "built-in");
            correctionAdd("builtin", "built-in");
            correctionAdd("build in", "built-in");
            correctionAdd("build-in", "built-in");
            correctionAdd("in-build", "built-in");
            correctionAdd("buildin", "built-in");

            correctionAdd("dissapear", "disappear");
            correctionAdd("dissappear", "disappear");

            correctionAdd("heirarchy", "hierarchy");
            correctionAdd("heirarachy", "hierarchy");
            correctionAdd("hirarachy", "hierarchy");
            correctionAdd("hiarachy", "hierarchy");
            correctionAdd("hirarchy", "hierarchy");
            correctionAdd("hiarchy", "hierarchy");
            correctionAdd("Hiaerchy", "hierarchy");
            correctionAdd("hiaerchy", "hierarchy");
            correctionAdd("hierachy", "hierarchy");

            correctionAdd("netduiono", "Netduino");

            correctionAdd("Machine learning", "machine learning");
            correctionAdd("Machine Learning", "machine learning");
            correctionAdd("MachineLearning", "machine learning");

            correctionAdd(".NetCF", ".NET Compact Framework");
            correctionAdd("Dot.net CF", ".NET Compact Framework");
            correctionAdd(".Net CF", ".NET Compact Framework");
            correctionAdd("compact framework", ".NET Compact Framework");

            correctionAdd("lldb", "LLDB");

            correctionAdd("xamarian", "Xamarin");
            correctionAdd("Xamarian", "Xamarin");
            correctionAdd("xamarin", "Xamarin");

            correctionAdd("ymmv", "your mileage may vary");
            correctionAdd("YMMV", "your mileage may vary");

            correctionAdd("vxworks", "VxWorks");

            correctionAdd("More over", "moreover"); //Not 100% correct - case.
            correctionAdd("more over", "moreover");

            correctionAdd("elf", "ELF");

            correctionAdd("when ever", "whenever");
            correctionAdd("whenver", "whenever");

            correctionAdd("Ohm", "ohm");
            correctionAdd("OHM", "ohm");
            correctionAdd("Om", "ohm");

            correctionAdd("Kohm", "kilohm");
            correctionAdd("KOhm", "kilohm");
            correctionAdd("kohm", "kilohm");
            correctionAdd("kOhm", "kilohm"); // Partially an expansion. The opposite is kΩ.
            correctionAdd("kiloohm", "kilohm");
            correctionAdd("kilo ohm", "kilohm");

            correctionAdd("Mohm", "megohm");
            correctionAdd("megaohm", "megohm");
            correctionAdd("MegOhm", "megohm");
            correctionAdd("MOhm", "megohm");
            // correctionAdd("Meg", "megohm"); // Could also be MB (megabyte)

            correctionAdd("micro farad", "µF"); //Also to abbreviate
            correctionAdd("uf", "µF");
            correctionAdd("uF", "µF");

            correctionAdd("appearence", "appearance");

            correctionAdd("dymancyly", "dynamically");
            correctionAdd("dynamicly", "dynamically");
            correctionAdd("dinamycally", "dynamically");

            correctionAdd("diffrent", "different");
            correctionAdd("diferent", "different");

            correctionAdd("beyong", "beyond");

            correctionAdd("russian", "Russian");

            correctionAdd("any one", "anyone");

            correctionAdd("eclipse", "Eclipse");
            correctionAdd("ECLIPSE", "Eclipse");
            correctionAdd("EClpise", "Eclipse"); //Misspelling.
            correctionAdd("Eclispe", "Eclipse"); //Misspelling.
            correctionAdd("eclpse", "Eclipse"); //Misspelling.
            correctionAdd("eclipde", "Eclipse"); //Misspelling.
            correctionAdd("ecliplse", "Eclipse"); //Misspelling.
            correctionAdd("eclipses", "Eclipse");
            correctionAdd("eclips", "Eclipse"); //Misspelling.
            correctionAdd("Ecilpse", "Eclipse"); //Misspelling.
            correctionAdd("eclise", "Eclipse"); //Misspelling.
            correctionAdd("ecplise", "Eclipse"); //Misspelling.

            correctionAdd("Eclipse Galileo", "Eclipse&nbsp;v3.5 (Galileo)");
            correctionAdd("Galileo", "Eclipse&nbsp;v3.5 (Galileo)");
            correctionAdd("galileo", "Eclipse&nbsp;v3.5 (Galileo)");

            correctionAdd("Eclipse indigo", "Eclipse&nbsp;v3.7 (Indigo)");
            correctionAdd("eclipse indigo", "Eclipse&nbsp;v3.7 (Indigo)");

            correctionAdd("Juno", "Eclipse&nbsp;v4.2 (Juno)");
            correctionAdd("Eclipse Juno", "Eclipse&nbsp;v4.2 (Juno)");
            correctionAdd("eclipse Juno", "Eclipse&nbsp;v4.2 (Juno)");
            correctionAdd("juno", "Eclipse&nbsp;v4.2 (Juno)");

            correctionAdd("Kepler", "Eclipse&nbsp;v4.3 (Kepler)");
            correctionAdd("Keplar", "Eclipse&nbsp;v4.3 (Kepler)");
            correctionAdd("Eclipse Kepler", "Eclipse&nbsp;v4.3 (Kepler)");
            correctionAdd("kepler", "Eclipse&nbsp;v4.3 (Kepler)");

            correctionAdd("eclipse luna", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("luna", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("Luna", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("eclipse 4.4", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("eclipse 4.4 Luna", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("Eclipse Luna 4.4", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("Eclipse 4.4 Luna", "Eclipse&nbsp;v4.4 (Luna)");
            //Does not work due to the ending ")"...
            // correctionAdd("Eclipse Luna(4.4)", "Eclipse&nbsp;v4.4 (Luna)");
            // correctionAdd("Eclipse Luna (4.4)", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("Eclipse Luna", "Eclipse&nbsp;v4.4 (Luna)");
            correctionAdd("eclipse Luna", "Eclipse&nbsp;v4.4 (Luna)");

            correctionAdd("Eclipse Mars", "Eclipse&nbsp;v4.5 (Mars)");
            correctionAdd("eclipse Mars", "Eclipse&nbsp;v4.5 (Mars)");

            // Resolve the conclict with the planet Mars by adding
            // a qualifying "Eclipse" with order and capitalisation
            // variations (though it does require the user to
            // know this).
            correctionAdd("Mars eclipse", "Eclipse&nbsp;v4.5 (Mars)");
            correctionAdd("Mars Eclipse", "Eclipse&nbsp;v4.5 (Mars)");

            correctionAdd("Neon", "Eclipse&nbsp;v4.6 (Neon)");
            correctionAdd("eclipse neon", "Eclipse&nbsp;v4.6 (Neon)");
            correctionAdd("Eclipse Neon", "Eclipse&nbsp;v4.6 (Neon)");
            correctionAdd("neon", "Eclipse&nbsp;v4.6 (Neon)");
            correctionAdd("eclipse-neon", "Eclipse&nbsp;v4.6 (Neon)");

            correctionAdd("recieving", "receiving");

            correctionAdd("recieve", "receive");
            correctionAdd("recive", "receive");

            correctionAdd("allways", "always");
            correctionAdd("alwys", "always");

            correctionAdd("Gitlab", "GitLab");
            correctionAdd("gitlab", "GitLab");
            correctionAdd("Git-lab", "GitLab");

            correctionAdd("spanish", "Spanish");

            // Part of Castle Project: Castle Windsor
            correctionAdd("castle", "Castle Project");
            correctionAdd("Castle", "Castle Project");

            correctionAdd("castle Windsor", "Castle Windsor"); // Alternative URL: <http://www.castleproject.org/container/index.html>

            correctionAdd("iisexpress", "IIS&nbsp;Express");
            correctionAdd("IISExpress", "IIS&nbsp;Express");

            correctionAdd("refering", "referring");
            correctionAdd("reffering", "referring");

            correctionAdd("DynamoDb", "DynamoDB");

            correctionAdd("pythonic", "Pythonic");

            correctionAdd("pullup", "pull-up resistor");
            correctionAdd("pull up", "pull-up resistor");
            correctionAdd("pullup resistor", "pull-up resistor");
            correctionAdd("pull up resistor", "pull-up resistor");
            correctionAdd("pull-up", "pull-up resistor");

            correctionAdd("snmp", "SNMP");

            correctionAdd("Autocad", "AutoCAD");
            correctionAdd("autocad", "AutoCAD");
            correctionAdd("AutoCad", "AutoCAD");

            correctionAdd("stm32", "STM32");
            correctionAdd("Stm32", "STM32");

            correctionAdd("authenticode", "Authenticode");
            correctionAdd("AuthentiCode", "Authenticode");

            correctionAdd("gulp", "Gulp.js");
            correctionAdd("Gulp", "Gulp.js");

            correctionAdd("intel hex", "Intel HEX");

            correctionAdd("winavr", "WinAVR");

            correctionAdd("Model-View-Controller", "model–view–controller");
            correctionAdd("Model-View-Control", "model–view–controller");

            correctionAdd("Model-View-Presenter", "model–view–presenter");

            correctionAdd("Phantomjs", "PhantomJS");
            correctionAdd("phantom", "PhantomJS");
            correctionAdd("phantomjs", "PhantomJS");

            correctionAdd("crsf", "CSRF");

            correctionAdd("potenciometer", "potentiometer");
            correctionAdd("pot", "potentiometer");

            correctionAdd("persistance", "persistence");
            correctionAdd("persitance", "persistence");

            correctionAdd("programitaly", "programmatically");
            correctionAdd("programatically", "programmatically");
            correctionAdd("programically", "programmatically");
            correctionAdd("programtically", "programmatically");
            correctionAdd("programmaticaly", "programmatically");

            correctionAdd("Hex", "hexadecimal");
            correctionAdd("hex", "hexadecimal");
            correctionAdd("Hexdecimal", "hexadecimal");
            correctionAdd("heximal", "hexadecimal");
            correctionAdd("HEX", "hexadecimal");
            correctionAdd("hexadecemimal", "hexadecimal");
            correctionAdd("hexa", "hexadecimal");
            correctionAdd("hexi-decimal", "hexadecimal");
            correctionAdd("hexdec", "hexadecimal");

            correctionAdd("neiboughood", "neighbourhood");

            correctionAdd("Functional programming", "functional programming");
            correctionAdd("Functional Programming", "functional programming");
            correctionAdd("FP", "functional programming");
            correctionAdd("functionnal programming", "functional programming");

            correctionAdd("shud", "should");
            correctionAdd("shuold", "should");
            correctionAdd("shold", "should");
            correctionAdd("shoud", "should");
            correctionAdd("shuld", "should");
            correctionAdd("hould", "should");

            correctionAdd("Shoudn't", "shouldn't");
            correctionAdd("shoudn't", "shouldn't");
            correctionAdd("Should't", "shouldn't");
            correctionAdd("should't", "shouldn't");
            correctionAdd("shouldnt", "shouldn't");
            correctionAdd("shoulnd´t", "shouldn't");
            correctionAdd("shouldn´t", "shouldn't");
            correctionAdd("shoulndt", "shouldn't");
            correctionAdd("shoulnd't", "shouldn't");

            correctionAdd("g-drive", "Google Drive");

            correctionAdd(".net core", ".NET Core");
            correctionAdd("dot net core", ".NET Core");
            correctionAdd(".Net Core", ".NET Core");
            correctionAdd("dotnet core", ".NET Core");
            correctionAdd("dotnetcore", ".NET Core");
            correctionAdd("Core", ".NET Core");
            correctionAdd(".net Core", ".NET Core");
            correctionAdd(".NET core", ".NET Core");
            correctionAdd(".Net core", ".NET Core");
            correctionAdd(".Net-core", ".NET Core");
            correctionAdd("netcore", ".NET Core");
            correctionAdd(".NET CORE", ".NET Core");
            correctionAdd("Net Core", ".NET Core");
            correctionAdd(".Net.core", ".NET Core");
            correctionAdd("DotNet Core", ".NET Core");
            correctionAdd("Dot net core", ".NET Core");
            correctionAdd("net core", ".NET Core");

            correctionAdd("Rosyln", "Roslyn");
            correctionAdd("Roselyn", "Roslyn");

            correctionAdd("drwatson", "Dr. Watson");

            correctionAdd("EcmaScript", "ECMAScript");
            correctionAdd("ecmascript", "ECMAScript");
            correctionAdd("Ecmascript", "ECMAScript");
            correctionAdd("ECMA", "ECMAScript");
            correctionAdd("ECMASCRIPT", "ECMAScript");
            correctionAdd("ES", "ECMAScript");
            correctionAdd("es", "ECMAScript");
            correctionAdd("ecma", "ECMAScript");
            correctionAdd("ECMAscript", "ECMAScript");

            correctionAdd("es6", "ECMAScript&nbsp;6");
            correctionAdd("ES6", "ECMAScript&nbsp;6");
            correctionAdd("e6", "ECMAScript&nbsp;6");
            correctionAdd("ECMAScript 2015", "ECMAScript&nbsp;6");
            correctionAdd("ES2015", "ECMAScript&nbsp;6");
            correctionAdd("ECMAScript6", "ECMAScript&nbsp;6");
            correctionAdd("ECMAScript 6", "ECMAScript&nbsp;6");
            correctionAdd("ES 6", "ECMAScript&nbsp;6");

            correctionAdd("redux", "Redux");

            correctionAdd("firebase", "Firebase");
            correctionAdd("fire base", "Firebase");

            correctionAdd("exsample", "example");
            correctionAdd("Exaplme", "example"); //Not 100% correct - case.
            correctionAdd("exaplme", "example");
            correctionAdd("exemple", "example");
            correctionAdd("exmaple", "example");
            correctionAdd("excample", "example");
            correctionAdd("Exampe", "example");
            correctionAdd("exampe", "example");

            correctionAdd("european", "European");

            correctionAdd("europe", "Europe");

            correctionAdd("total Commander", "Total Commander");
            correctionAdd("total commander", "Total Commander");
            correctionAdd("Total commander", "Total Commander");

            correctionAdd("altap Salamander", "Altap Salamander");
            correctionAdd("Slamander", "Altap Salamander");

            correctionAdd("BaudRate", "baud rate");
            correctionAdd("baudRate", "baud rate");
            correctionAdd("baudrate", "baud rate");

            correctionAdd("Tektronics", "Tektronix");
            correctionAdd("Tek", "Tektronix");
            correctionAdd("tektronix", "Tektronix");
            correctionAdd("tek", "Tektronix");

            correctionAdd("gpib", "GPIB");

            correctionAdd("futher more", "furthermore");
            correctionAdd("further more", "furthermore");

            //Of PowerShell...
            correctionAdd("get-winevent", "Get-WinEvent");

            //Of PowerShell...
            correctionAdd("get-eventlog", "Get-EventLog");
            correctionAdd("Get-eventlog", "Get-EventLog");

            //Of PowerShell...
            correctionAdd("compare-object", "Compare-Object");

            correctionAdd("arabic", "Arabic");

            correctionAdd("roman", "Roman");

            // Of PowerShell...
            correctionAdd("move-item", "Move-Item");

            // Of PowerShell...
            correctionAdd("get-location", "Get-Location");

            // Of PowerShell...
            correctionAdd("rename-item", "Rename-Item");

            correctionAdd("tensorflow", "TensorFlow");
            correctionAdd("Tensorflow", "TensorFlow");
            correctionAdd("tensorFlow", "TensorFlow");

            correctionAdd("retrive", "retrieve");
            correctionAdd("retreive", "retrieve");

            correctionAdd("ressource", "resource");

            correctionAdd("freertos", "FreeRTOS");

            correctionAdd("cortex-m", "Cortex-M");
            correctionAdd("Cortex-m", "Cortex-M");
            correctionAdd("ARM Cortex M", "Cortex-M"); // Not 100% correct.
            correctionAdd("Cortex M", "Cortex-M");


            correctionAdd("sims", "seems");
            correctionAdd("seams", "seems");

            correctionAdd("F35", "F-35");

            // Of PowerShell...
            correctionAdd("Set-Aliast", "Set-Alias");
            correctionAdd("set-alias", "Set-Alias");

            // Of PowerShell...
            correctionAdd("get-alias", "Get-Alias");

            correctionAdd("kicad", "KiCad");
            correctionAdd("Kicad", "KiCad");
            correctionAdd("KICAD", "KiCad");
            correctionAdd("KiCAD", "KiCad");

            correctionAdd("vuejs", "Vue.js");
            correctionAdd("vue.js", "Vue.js");
            correctionAdd("Vue Js", "Vue.js");
            correctionAdd("Vue.Js", "Vue.js");
            correctionAdd("vue", "Vue.js");
            correctionAdd("VueJS", "Vue.js");
            correctionAdd("Vuejs", "Vue.js");

            correctionAdd("Farad", "farad");

            correctionAdd("Coulomb", "coulomb");

            correctionAdd("SYbase", "Sybase");

            correctionAdd("thnx", "thanks");

            correctionAdd("vuln", "vulnerability");

            correctionAdd("collegaue", "colleague");
            correctionAdd("collegue", "colleague");
            correctionAdd("collegues", "colleague");
            correctionAdd("colleage", "colleague");

            correctionAdd("backgroud", "background");
            correctionAdd("bg", "background");

            correctionAdd("kotlin", "Kotlin");

            correctionAdd("arent", "aren't");

            correctionAdd("Email", "email");
            correctionAdd("meail", "email");
            correctionAdd("meails", "email");

            correctionAdd("altogheter", "altogether");
            correctionAdd("all together", "altogether");
            correctionAdd("alltogether", "altogether");

            correctionAdd("middle east", "Middle East");

            correctionAdd("what ever", "whatever");
            correctionAdd("w/e", "whatever");

            correctionAdd("pydev", "PyDev");
            correctionAdd("Pydev", "PyDev");

            correctionAdd("pycharm", "PyCharm");
            correctionAdd("Pycharm", "PyCharm");
            correctionAdd("pyCharm", "PyCharm");

            correctionAdd("BDD", "behavior-driven development");
            correctionAdd("Behavior Driven Development", "behavior-driven development");
            correctionAdd("Behavior-Driven Development", "behavior-driven development");
            correctionAdd("bdd", "behavior-driven development");

            correctionAdd("apperently", "apparently");
            correctionAdd("aparently", "apparently");

            correctionAdd("digikey", "Digi-Key");
            correctionAdd("Digikey", "Digi-Key");

            correctionAdd("rs485", "RS-485");
            correctionAdd("Rs485", "RS-485");
            correctionAdd("RS485", "RS-485");

            correctionAdd("some times", "sometimes");

            correctionAdd("pci-e", "PCIe");
            correctionAdd("PCI-e", "PCIe");
            correctionAdd("PCI-E", "PCIe");
            correctionAdd("PCIE", "PCIe");

            correctionAdd("pci", "PCI");

            correctionAdd("youre", "you're");
            correctionAdd("Ur", "you're");

            correctionAdd("umbraco", "Umbraco");

            correctionAdd("leonardo", "Leonardo");

            //Of PowerShell...
            correctionAdd("get-service", "Get-Service");
            correctionAdd("Get-service", "Get-Service");
            correctionAdd("get-Service", "Get-Service");

            //Of PowerShell...
            correctionAdd("set-service", "Set-Service");

            correctionAdd("AtTiny85", "ATtiny85");

            correctionAdd("fritzing", "Fritzing");

            correctionAdd("communcation", "communication");
            correctionAdd("comms", "communication");
            correctionAdd("comm", "communication");

            correctionAdd("altera", "Altera");

            correctionAdd("xilinx", "Xilinx");

            correctionAdd("quartus", "Quartus");

            correctionAdd("openscad", "OpenSCAD");

            correctionAdd("japanese", "Japanese");

            correctionAdd("c++17", "C++17");

            correctionAdd("embaraased", "embarrassed");

            correctionAdd("ridicous", "ridiculous");
            correctionAdd("ridicolous", "ridiculous");
            correctionAdd("redicoulous", "ridiculous");
            correctionAdd("redicoulush", "ridiculous");

            correctionAdd("actaully", "actually");
            correctionAdd("actualy", "actually");

            correctionAdd("post", "POST");

            correctionAdd("get", "GET");

            correctionAdd("put", "PUT");

            correctionAdd("delete", "DELETE");

            correctionAdd("fieasable", "feasible");
            correctionAdd("fisible", "feasible");
            correctionAdd("feasable", "feasible");

            correctionAdd("Schrodinger", "Schrödinger equation");

            correctionAdd("hasnt", "hasn't");

            correctionAdd("maintanance", "maintenance");
            correctionAdd("maitenance", "maintenance");
            correctionAdd("mainatin", "maintenance");
            correctionAdd("maintanence", "maintenance");
            correctionAdd("maintenence", "maintenance");

            correctionAdd("mantained", "maintained");

            correctionAdd("Visual Studio code", "Visual Studio Code");
            correctionAdd("VSCode", "Visual Studio Code");
            correctionAdd("VS Code", "Visual Studio Code");
            correctionAdd("visual studio code", "Visual Studio Code");
            correctionAdd("Visual studio code", "Visual Studio Code");
            correctionAdd("vscode", "Visual Studio Code");
            correctionAdd("vs code", "Visual Studio Code");
            correctionAdd("VS code", "Visual Studio Code");
            correctionAdd("VsCode", "Visual Studio Code");
            correctionAdd("visual code", "Visual Studio Code");
            correctionAdd("vsCode", "Visual Studio Code");
            correctionAdd("vs Code", "Visual Studio Code");
            correctionAdd("Vscode", "Visual Studio Code");
            correctionAdd("VScode", "Visual Studio Code");
            correctionAdd("vs-code", "Visual Studio Code");
            correctionAdd("Code", "Visual Studio Code");
            correctionAdd("Visual studio Code", "Visual Studio Code");
            correctionAdd("VSC", "Visual Studio Code");
            correctionAdd("VSCODE", "Visual Studio Code");
            correctionAdd("Visual Code", "Visual Studio Code");

            correctionAdd("WebStrom", "WebStorm");
            correctionAdd("Webstorm", "WebStorm");
            correctionAdd("webstorm", "WebStorm");
            correctionAdd("Web Storm", "WebStorm");
            correctionAdd("webstrome", "WebStorm");

            correctionAdd("gonna", "is going to");

            correctionAdd("intro", "introduction");
            correctionAdd("introdution", "introduction");
            correctionAdd("Introdction", "introduction");
            correctionAdd("Introdcution", "introduction");
            correctionAdd("introdction", "introduction");
            correctionAdd("introdcution", "introduction");

            correctionAdd("who's", "whose");

            correctionAdd("lib", "library");
            correctionAdd("Lib", "library"); //Not 100% correct - casing.
            correctionAdd("librairies", "library"); // Not 100% correct - plural
            correctionAdd("libraties", "library"); // Not 100% correct - plural
            correctionAdd("libary", "library");

            correctionAdd("libs", "libraries");
            correctionAdd("Libs", "libraries");
            correctionAdd("libaries", "libraries");

            correctionAdd("webgl", "WebGL");

            correctionAdd("accidently", "accidentally");
            correctionAdd("accidentaly", "accidentally");

            correctionAdd("gotta", "have got to");

            correctionAdd("three.js", "Three.js");
            correctionAdd("three js", "Three.js");

            correctionAdd("0day", "zero-day");

            correctionAdd("Perl 6", "Perl&nbsp;6");
            correctionAdd("Perl6", "Perl&nbsp;6");
            correctionAdd("perl6", "Perl&nbsp;6");

            correctionAdd("paranthesis", "parenthesis");
            correctionAdd("paren", "parenthesis");
            correctionAdd("paranthesys", "parenthesis");
            correctionAdd("parenthes", "parenthesis");

            correctionAdd("parens", "parentheses");
            correctionAdd("parantheses", "parentheses");
            correctionAdd("paratheses", "parentheses");

            correctionAdd("utils", "utilities");

            correctionAdd("wouldnt", "wouldn't");
            correctionAdd("wouldn’t", "wouldn't");
            correctionAdd("woulnd't", "wouldn't");
            correctionAdd("woudn't", "wouldn't");
            correctionAdd("would't", "wouldn't");
            correctionAdd("wouldn´t", "wouldn't");
            correctionAdd("Wouldnt", "wouldn't");
            correctionAdd("woudln't", "wouldn't");
            correctionAdd("woudlnt", "wouldn't");

            correctionAdd("implemention", "implementation");
            correctionAdd("implemenation", "implementation");
            correctionAdd("implementaion", "implementation");

            correctionAdd("explaination", "explanation");
            correctionAdd("explenation", "explanation");
            correctionAdd("explenations", "explanation"); //Not 100% correct - plural.
            correctionAdd("explination", "explanation");
            correctionAdd("expanation", "explanation");
            correctionAdd("Expanation", "explanation");
            correctionAdd("Explaination", "explanation");

            correctionAdd("similiar", "similar");
            correctionAdd("simillar", "similar");
            correctionAdd("simmiller", "similar");
            correctionAdd("smililar", "similar");
            correctionAdd("simlar", "similar");
            correctionAdd("Simillar", "similar");
            correctionAdd("Similiar", "similar");


            // But Wiktionary says: "(obsolete) Similar."
            //
            correctionAdd("Similary", "similary"); //Not 100% correct - case.
            correctionAdd("similarly", "similary");
            correctionAdd("Similarly", "similary"); //Not 100% correct - case.
            correctionAdd("similaly", "similary");
            correctionAdd("Similaly", "similary"); //Not 100% correct - case.
            correctionAdd("simillarly", "similary");

            correctionAdd("Instaltaion", "installation"); //Not 100% correct - case.
            correctionAdd("instaltaion", "installation");
            correctionAdd("instalation", "installation");

            correctionAdd("openbsd", "OpenBSD");

            correctionAdd("Theorically", "theoretically");  //Not 100% correct - case.
            correctionAdd("theorically", "theoretically");
            correctionAdd("theoritically", "theoretically");

            correctionAdd("Debugview", "DebugView");
            correctionAdd("debugview", "DebugView");
            correctionAdd("debug view", "DebugView");
            correctionAdd("Dbgview", "DebugView");
            correctionAdd("DbgView", "DebugView");
            correctionAdd("debugiview", "DebugView");

            correctionAdd("ollydbg", "OllyDbg");

            // Of PowerShell...
            correctionAdd("write-debug", "Write-Debug");
            correctionAdd("write-Debug", "Write-Debug");

            // Of PowerShell...
            correctionAdd("set-psdebug", "Set-PSDebug");

            correctionAdd("Arduino MEGA", "Arduino Mega");
            correctionAdd("Arduino mega", "Arduino Mega");
            correctionAdd("arduino Mega", "Arduino Mega");

            correctionAdd("Mips", "MIPS");
            correctionAdd("mips", "MIPS");

            correctionAdd("wanna", "want to");
            correctionAdd("wanne", "want to");

            correctionAdd("laravel", "Laravel");
            correctionAdd("laraval", "Laravel");
            correctionAdd("Laraval", "Laravel");

            correctionAdd("wasnt", "wasn't");

            correctionAdd("challange", "challenge");
            correctionAdd("chanlenge", "challenge");
            correctionAdd("chalenge", "challenge");
            correctionAdd("chalendge", "challenge");

            correctionAdd("atmega328", "ATmega328");
            correctionAdd("ATMega328", "ATmega328");
            correctionAdd("ATMEGA328", "ATmega328");
            correctionAdd("AtMega328", "ATmega328");
            correctionAdd("Atmega328", "ATmega328");

            correctionAdd("ATMEGA 328p", "ATmega328P");
            correctionAdd("ATMEGA328p", "ATmega328P");
            correctionAdd("Atmega328p", "ATmega328P");
            correctionAdd("Atmega328P", "ATmega328P");
            correctionAdd("atmega328p", "ATmega328P");
            correctionAdd("atmega328P", "ATmega328P");
            correctionAdd("Atmega 328p", "ATmega328P");
            correctionAdd("atmega 328p", "ATmega328P");
            correctionAdd("ATmega 328p", "ATmega328P");
            correctionAdd("ATmega 328P", "ATmega328P");

            correctionAdd("anaconda", "Anaconda");

            correctionAdd("conda", "Conda");

            correctionAdd("jekyll", "Jekyll");

            correctionAdd("posability", "possibility");
            correctionAdd("possibilty", "possibility");

            correctionAdd("c89", "C89");

            correctionAdd("c99", "C99");

            correctionAdd("occured", "occurred");

            correctionAdd("developement", "development");

            correctionAdd("simulateous", "simultaneous");
            correctionAdd("Similtanous", "simultaneous");  //Not 100% correct - case.

            correctionAdd("redifine", "redefine");

            correctionAdd("Objective-c++", "Objective-C++");
            correctionAdd("Objective C++", "Objective-C++");

            correctionAdd("glib", "GLib");

            correctionAdd("clearcase", "ClearCase");
            correctionAdd("Clearcase", "ClearCase");
            correctionAdd("clear case", "ClearCase");

            correctionAdd("off topic", "off-topic");
            correctionAdd("offtopic", "off-topic");
            correctionAdd("OT", "off-topic");
            correctionAdd("Off Topic", "off-topic");

            correctionAdd("prefered", "preferred");
            correctionAdd("preffered", "preferred");

            correctionAdd("disapointed", "disappointed");

            correctionAdd("excercise", "exercise");
            correctionAdd("excersise", "exercise");
            correctionAdd("Excersise", "exercise");

            correctionAdd("contiguos", "contiguous");

            correctionAdd("consequtive", "consecutive");

            correctionAdd("beleive", "believe");
            correctionAdd("belive", "believe");

            correctionAdd("usuable", "usable");
            correctionAdd("useble", "usable");
            correctionAdd("useable", "usable");

            correctionAdd("uContoller", "microcontroller");
            correctionAdd("uController", "microcontroller");
            correctionAdd("micro-controller", "microcontroller");
            correctionAdd("uC", "microcontroller"); //Expansion
            correctionAdd("mc", "microcontroller"); //Colision with something else?
            correctionAdd("MCU", "microcontroller"); //Expansion
            correctionAdd("micro controllers", "microcontroller"); //Plural...
            correctionAdd("micro controller", "microcontroller");
            correctionAdd("mcu", "microcontroller"); //Expansion
            correctionAdd("mictrocontroller", "microcontroller"); //Misspelling
            correctionAdd("uc", "microcontroller"); //Expansion
            correctionAdd("Microcontroller", "microcontroller"); //Depends on context...
            correctionAdd("ucontroller", "microcontroller"); //Expansion
            correctionAdd("Micro-controller", "microcontroller");
            correctionAdd("μc", "microcontroller");
            correctionAdd("microController", "microcontroller");
            correctionAdd("microcontroler", "microcontroller");
            correctionAdd("MicroController", "microcontroller");
            correctionAdd("microntroller", "microcontroller"); //Misspelling
            correctionAdd("µC", "microcontroller");
            correctionAdd("mControlers", "microcontroller"); //Plural...
            correctionAdd("mControler", "microcontroller");
            correctionAdd("mController", "microcontroller");
            correctionAdd("microncontroller", "microcontroller");
            correctionAdd("mictrocontrollers", "microcontroller");

            correctionAdd("uP", "microprocessor");
            correctionAdd("µp", "microprocessor");
            correctionAdd("µP", "microprocessor");
            correctionAdd("mP", "microprocessor");

            correctionAdd("pronounciation", "pronunciation");

            correctionAdd("requrie", "require");
            correctionAdd("requries", "require"); // Not 100% correct, third-person singular
            correctionAdd("requiere", "require");

            correctionAdd("particuarly", "particularly");

            correctionAdd("particuliar", "particular");
            correctionAdd("perticular", "particular");

            correctionAdd("dunno", "don't know");

            correctionAdd("canadian", "Canadian");

            correctionAdd("pulseview", "PulseView");

            correctionAdd("Arcgis", "ArcGIS");

            correctionAdd("persistant", "persistent");

            correctionAdd("WinNT", "Windows NT");

            correctionAdd("lubuntu", "Lubuntu");

            correctionAdd("resister", "resistor");
            correctionAdd("resitor", "resistor");

            correctionAdd("openjdk", "OpenJDK");
            correctionAdd("open_jdk", "OpenJDK");
            correctionAdd("Open JDK", "OpenJDK");
            correctionAdd("openJDK", "OpenJDK");

            correctionAdd("callisto", "Eclipse v3.2 (Callisto)");

            correctionAdd("europa", "Eclipse v3.3 (Europa)");

            correctionAdd("ganymede", "Eclipse v3.4 (Ganymede)");

            correctionAdd("instanciate", "instantiate");

            correctionAdd("1st", "first");

            correctionAdd("j1939", "J1939");

            correctionAdd("mcp2515", "MCP2515");

            correctionAdd("occurence", "occurrence");
            correctionAdd("occurrance", "occurrence");
            correctionAdd("occurance", "occurrence");

            correctionAdd("cred", "credential");
            correctionAdd("crendentials", "credential");

            correctionAdd("react", "React");
            correctionAdd("reactjs", "React");
            correctionAdd("Reactjs", "React");
            correctionAdd("react.js", "React");
            correctionAdd("React.JS", "React");
            correctionAdd("react JS", "React");
            correctionAdd("ReactJs", "React");
            correctionAdd("ReactJS", "React");
            correctionAdd("React JS", "React");
            correctionAdd("react js", "React");
            correctionAdd("React js", "React");

            correctionAdd("dependant", "dependent"); //Common misspelling...

            correctionAdd("dependancy", "dependency"); //Common misspelling...
            correctionAdd("Dependancy", "dependency");

            // -***** NEW

            correctionAdd("dependacies", "dependencies");
            correctionAdd("dependancies", "dependencies"); //Common misspelling... Not 100% correct. Plural...
            correctionAdd("dependecies", "dependencies");
            correctionAdd("depencies", "dependencies");
            correctionAdd("Dependancies", "dependencies");

            correctionAdd("LinqPad", "LINQPad");
            correctionAdd("linqPad", "LINQPad");

            correctionAdd("effecient", "efficient");

            correctionAdd("indendation", "indentation");
            correctionAdd("indentitation", "indentation");
            correctionAdd("ident", "indentation");
            correctionAdd("indent", "indentation");

            correctionAdd("horizentaly", "horizontally");

            correctionAdd("askubuntu", "Ask&nbsp;Ubuntu");
            correctionAdd("AskUbuntu", "Ask&nbsp;Ubuntu");
            correctionAdd("Askubuntu", "Ask&nbsp;Ubuntu");
            correctionAdd("Ask Ubuntu", "Ask&nbsp;Ubuntu");

            correctionAdd("managament", "management");
            correctionAdd("mngt", "management");

            correctionAdd("portuguese", "Portuguese");

            correctionAdd("mockito", "Mockito");

            correctionAdd("refered", "referred");
            correctionAdd("reffered", "referred");

            correctionAdd("brake point", "breakpoint");
            correctionAdd("break point", "breakpoint");

            correctionAdd("up to date", "up-to-date");
            correctionAdd("up2date", "up-to-date");
            correctionAdd("uptodate", "up-to-date");

            correctionAdd("react native", "React Native");
            correctionAdd("react-native", "React Native");
            correctionAdd("React-native", "React Native");
            correctionAdd("React native", "React Native");
            correctionAdd("ReactNative", "React Native");
            correctionAdd("React-Native", "React Native");

            correctionAdd("ionic", "Ionic");
            correctionAdd("IONIC", "Ionic");

            correctionAdd("tranceiver", "transceiver");

            correctionAdd("buttom", "bottom");
            correctionAdd("bottum", "bottom");
            correctionAdd("buttum", "bottom");
            correctionAdd("bottem", "bottom");


            //DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD

            correctionAdd("BLE", "Bluetooth LE");

            correctionAdd("cmakelist", "CMakeLists.txt");
            correctionAdd("CMakeLists", "CMakeLists.txt");
            correctionAdd("cmakelists.txt", "CMakeLists.txt");
            correctionAdd("cmakelists", "CMakeLists.txt");

            correctionAdd("fpu", "FPU");

            correctionAdd("FWIW", "For what it's worth");

            correctionAdd("git BASH", "Git Bash");
            correctionAdd("gitBash", "Git Bash");
            correctionAdd("git Bash", "Git Bash");
            correctionAdd("git bash", "Git Bash");
            correctionAdd("gitbash", "Git Bash");
            correctionAdd("git-bash", "Git Bash");
            correctionAdd("Git BASH", "Git Bash");
            correctionAdd("GIT Bash", "Git Bash");
            correctionAdd("GitBash", "Git Bash");
            correctionAdd("Git bash", "Git Bash");
            correctionAdd("Gitbash", "Git Bash");

            correctionAdd("git-extensions", "Git Extensions");

            correctionAdd("idk", "I don't know");
            correctionAdd("Idk", "I don't know");
            correctionAdd("IDK", "I don't know");

            correctionAdd("invoke-expression", "Invoke-Expression");
            correctionAdd("Invoke-expression", "Invoke-Expression");

            correctionAdd("Kdiff3", "KDiff3");

            correctionAdd("kubernetes", "Kubernetes");

            correctionAdd("mamp", "MAMP");

            correctionAdd("Mac OS X Sierra", "Mac OS X v10.12 (Sierra)");

            correctionAdd("Nmake", "NMAKE");
            correctionAdd("nmake", "NMAKE");
            correctionAdd("NMake", "NMAKE");

            correctionAdd("pom", "POM");

            correctionAdd("PyPi", "PyPI");
            correctionAdd("pypi", "PyPI");

            correctionAdd("robotframework", "Robot Framework");
            correctionAdd("robot framework", "Robot Framework");
            correctionAdd("Robot framework", "Robot Framework");

            correctionAdd("scmitt trigger", "Schmitt trigger");
            correctionAdd("Schmidt", "Schmitt trigger");
            correctionAdd("Schmidt trigger", "Schmitt trigger");

            correctionAdd("start-job", "Start-Job");

            correctionAdd("swedish", "Swedish");

            correctionAdd("Ubuntu Mate", "Ubuntu&nbsp;MATE");

            correctionAdd("vsts", "VSTS");

            correctionAdd("achive", "achieve");
            correctionAdd("acheive", "achieve");

            correctionAdd("anwser", "answer");
            correctionAdd("awnser", "answer");
            correctionAdd("anwswer", "answer");
            correctionAdd("ans", "answer");

            correctionAdd("acward", "awkward");

            correctionAdd("before hand", "beforehand");

            correctionAdd("behavoir", "behavior");

            correctionAdd("bellow", "below");

            correctionAdd("Cmdlet", "cmdlet");
            correctionAdd("CmdLet", "cmdlet");
            correctionAdd("commandlet", "cmdlet");

            correctionAdd("communciate", "communicate");

            correctionAdd("consistant", "consistent");

            correctionAdd("cont", "continuous");
            correctionAdd("continous", "continuous");
            correctionAdd("continious", "continuous");

            correctionAdd("Continuous Deployment", "continuous deployment");

            correctionAdd("CRONTAB", "crontab");

            correctionAdd("de-facto", "de facto");

            correctionAdd("decison", "decision");
            correctionAdd("desicion", "decision");

            correctionAdd("enought", "enough");
            correctionAdd("Nuff", "enough");
            correctionAdd("nuff", "enough");

            correctionAdd("exisiting", "existing");

            correctionAdd("explainign", "explaining");
            correctionAdd("explainig", "explaining");

            correctionAdd("gen", "generation");

            correctionAdd("lenght", "length");

            correctionAdd("mean time", "meantime");
            correctionAdd("meantine", "meantime");

            correctionAdd("paraghaph", "paragraph");

            correctionAdd("readonly", "read-only");

            correctionAdd("realy", "really");

            correctionAdd("recomendation", "recommendation");
            correctionAdd("reccomendation", "recommendation");
            correctionAdd("recommandation", "recommendation");

            correctionAdd("recommanded", "recommended");
            correctionAdd("recoomended", "recommended");
            correctionAdd("commend", "recommended");

            correctionAdd("seperator", "separator");

            correctionAdd("statment", "statement");
            correctionAdd("statemet", "statement");
            correctionAdd("statemetns", "statement");

            correctionAdd("staritaway", "straight away");

            correctionAdd("strenhgt", "strength");

            correctionAdd("successfull", "successful");
            correctionAdd("sucessful", "successful");
            correctionAdd("succesful", "successful");
            correctionAdd("succesfull", "successful");

            correctionAdd("succesfully", "successfully");
            correctionAdd("secsessfully", "successfully");
            correctionAdd("sucessfully", "successfully");
            correctionAdd("Succesfully", "successfully");
            correctionAdd("succesfuly", "successfully");

            correctionAdd("theres", "there's");

            correctionAdd("transfered", "transferred");

            correctionAdd("transfering", "transferring");

            correctionAdd("well known", "well-known");

            correctionAdd("XUnit", "xUnit");
            correctionAdd("Xunit", "xUnit");

            correctionAdd("set-content", "Set-Content");

            correctionAdd("ansible", "Ansible");

            correctionAdd("APACHE POI", "Apache POI");
            correctionAdd("apache poi", "Apache POI");
            correctionAdd("Apache poi", "Apache POI");
            correctionAdd("poi", "Apache POI");

            correctionAdd("artifactory", "Artifactory");

            correctionAdd("boolean", "Boolean");
            correctionAdd("bolean", "Boolean");

            correctionAdd("crm", "CRM");

            correctionAdd("CPPcheck", "Cppcheck");
            correctionAdd("cppcheck", "Cppcheck");
            correctionAdd("CPPCheck", "Cppcheck");
            correctionAdd("CppCheck", "Cppcheck");

            correctionAdd("Cocumber", "Cucumber");

            correctionAdd("dto", "DTO");

            correctionAdd("Debian 9", "Debian&nbsp;9 (Stretch)");
            correctionAdd("Debian Stretch", "Debian&nbsp;9 (Stretch)");
            correctionAdd("Stretch", "Debian&nbsp;9 (Stretch)");

            correctionAdd("export-clixml", "Export-Clixml");

            correctionAdd("MICROEMACS", "MicroEMACS");
            correctionAdd("uEmacs", "MicroEMACS");
            correctionAdd("microemacs", "MicroEMACS");
            correctionAdd("Microemacs", "MicroEMACS");
            correctionAdd("MicroEmacs", "MicroEMACS");
            correctionAdd("MICROEmacs", "MicroEMACS");
            correctionAdd("MICROemacs", "MicroEMACS");

            correctionAdd("netmon", "Network Monitor");
            correctionAdd("Netmon", "Network Monitor");

            correctionAdd("oracle", "Oracle");

            correctionAdd("out-gridview", "Out-GridView");

            correctionAdd("pscustomobject", "PSCustomObject");
            correctionAdd("PsCustomObject", "PSCustomObject");

            correctionAdd("python2", "Python&nbsp;2");
            correctionAdd("Python 2", "Python&nbsp;2");
            correctionAdd("py2", "Python&nbsp;2");
            correctionAdd("python 2", "Python&nbsp;2");
            correctionAdd("Python2", "Python&nbsp;2");

            correctionAdd("python3", "Python&nbsp;3");
            correctionAdd("Python 3", "Python&nbsp;3");
            correctionAdd("python 3", "Python&nbsp;3");
            correctionAdd("py3", "Python&nbsp;3");
            correctionAdd("Python3", "Python&nbsp;3");

            correctionAdd("quran", "Qur'an");

            correctionAdd("star wars", "Star Wars");
            correctionAdd("star-wars", "Star Wars");

            correctionAdd("tikz", "TikZ");

            correctionAdd("utc", "UTC");

            correctionAdd("acros", "across");
            correctionAdd("accross", "across");
            correctionAdd("accros", "across");

            correctionAdd("alteratively", "alternatively");
            correctionAdd("Alteratively", "alternatively");

            correctionAdd("aparent", "apparent");

            correctionAdd("bare with", "bear with");

            correctionAdd("bare with me", "bear with me");

            correctionAdd("beeing", "being");

            correctionAdd("concatention", "concatenation");
            correctionAdd("concatanation", "concatenation");

            correctionAdd("dtor", "destructor");
            correctionAdd("destructur", "destructor");
            correctionAdd("d'tor", "destructor");
            correctionAdd("DTOR", "destructor");
            correctionAdd("DTor", "destructor");

            correctionAdd("initalize", "initialise");
            correctionAdd("initiliazed", "initialise");
            correctionAdd("initialized", "initialise");
            correctionAdd("initialize", "initialise");
            correctionAdd("init", "initialise");

            correctionAdd("loose", "lose");

            correctionAdd("loosing", "losing");

            correctionAdd("managin", "managing");
            correctionAdd("Managin", "managing");

            correctionAdd("prevous", "previous");
            correctionAdd("previus", "previous");
            correctionAdd("prev", "previous");

            correctionAdd("responsabilities", "responsibilities");

            correctionAdd("responsability", "responsibility");
            correctionAdd("responsbility", "responsibility");

            correctionAdd("Silicon", "silicon");

            correctionAdd("StdErr", "standard error");
            correctionAdd("stderr", "standard error");

            correctionAdd("suprised", "surprised");

            correctionAdd("3rd", "third");

            correctionAdd("visa versa", "vice versa");
            correctionAdd("Vis versa", "vice versa");
            correctionAdd("vise versa", "vice versa");
            correctionAdd("visversa", "vice versa");

            correctionAdd("your self", "yourself");
            correctionAdd("yourselves", "yourself");
            correctionAdd("yourselv", "yourself");

            correctionAdd("atom", "Atom");
            correctionAdd("ATOM", "Atom");

            correctionAdd("convertto-xml", "ConvertTo-Xml");

            correctionAdd("cyrillic", "Cyrillic");

            //correctionAdd("oxygen", "Eclipse&nbsp;v4.7 (Oxygen)"); For now, we prefer the element "oxygen".
            correctionAdd("Eclipse oxygen", "Eclipse&nbsp;v4.7 (Oxygen)");

            correctionAdd("find-package", "Find-Package");

            correctionAdd("get-module", "Get-Module");

            correctionAdd("import-clixml", "Import-Clixml");
            correctionAdd("import-cliXml", "Import-Clixml");

            correctionAdd("import-module", "Import-Module");

            correctionAdd("install-package", "Install-Package");

            correctionAdd("latin", "Latin");

            correctionAdd("Measure-command", "Measure-Command");
            correctionAdd("measure-command", "Measure-Command");

            correctionAdd("ros", "ROS");

            correctionAdd("set-variable", "Set-Variable");

            correctionAdd("Tex", "TeX");
            correctionAdd("TEX", "TeX");
            correctionAdd("tex", "TeX");

            correctionAdd("twig", "Twig");

            correctionAdd("uk", "UK");
            correctionAdd("united kingdom", "UK");
            correctionAdd("Uk", "UK");

            correctionAdd("vlan", "VLAN");

            correctionAdd("Windows defender", "Windows Defender");
            correctionAdd("windows defender", "Windows Defender");

            correctionAdd("world war 1", "World War I");
            correctionAdd("ww1", "World War I");
            correctionAdd("WW1", "World War I");

            correctionAdd("write-verbose", "Write-Verbose");
            correctionAdd("Write-verbose", "Write-Verbose");

            correctionAdd("complier", "compiler");

            correctionAdd("compter", "computer");

            correctionAdd("eigth", "eight");

            correctionAdd("8th", "eighth");

            correctionAdd("helpfull", "helpful");

            correctionAdd("intermeidate", "intermediate");

            correctionAdd("peice", "piece");

            correctionAdd("previosly", "previously");
            correctionAdd("previusly", "previously");

            correctionAdd("profissional", "professional");
            correctionAdd("pro", "professional");
            correctionAdd("profesional", "professional");

            correctionAdd("succcess", "success");
            correctionAdd("successs", "success");

            correctionAdd("christian", "Christian");

            correctionAdd("elo", "Elo");

            correctionAdd("fide", "FIDE");

            correctionAdd("Nasa", "NASA");
            correctionAdd("nasa", "NASA");

            correctionAdd("out-string", "Out-String");

            correctionAdd("scandinavian", "Scandinavian");
            correctionAdd("Scandinvian", "Scandinavian");
            correctionAdd("Scandanavian", "Scandinavian");

            correctionAdd("venus", "Venus");

            correctionAdd("Big O", "big O");
            correctionAdd("BigO", "big O");

            correctionAdd("correclty", "correctly");

            correctionAdd("dict", "dictionary");
            correctionAdd("disctionary", "dictionary");

            correctionAdd("Kg", "kg");
            correctionAdd("KG", "kg");

            correctionAdd("6th", "sixth");

            correctionAdd("add-type", "Add-Type");

            correctionAdd("Android 5.1", "Android 5.1 (Lollipop)");

            correctionAdd("Arduino-cli", "Arduino CLI");

            correctionAdd("clear-history", "Clear-History");

            correctionAdd("C64", "Commodore 64");

            correctionAdd("dna", "DNA");

            correctionAdd("Devops", "DevOps");
            correctionAdd("devops", "DevOps");
            correctionAdd("dev ops", "DevOps");

            correctionAdd("germanic", "Germanic");

            correctionAdd("Get-history", "Get-History");

            correctionAdd("op", "OP");

            correctionAdd("powershell core", "PowerShell Core");
            correctionAdd("Powershell Core", "PowerShell Core");

            correctionAdd("slavic", "Slavic");

            correctionAdd("SCCM", "System Center Configuration Manager");

            correctionAdd("woocommerce", "WooCommerce");
            correctionAdd("Woocommerce", "WooCommerce");

            correctionAdd("acomplishment", "accomplishment");

            correctionAdd("braught", "brought");
            correctionAdd("brough", "brought");

            correctionAdd("certianly", "certainly");

            correctionAdd("critisicm", "criticism");

            correctionAdd("instaead", "instead");
            correctionAdd("isntead", "instead");
            correctionAdd("Insted", "instead");
            correctionAdd("insted", "instead");

            correctionAdd("maybee", "maybe");
            correctionAdd("maby", "maybe");

            correctionAdd("Plutonium", "plutonium");

            correctionAdd("possbile", "possible");
            correctionAdd("posible", "possible");

            correctionAdd("programing", "programming");

            correctionAdd("rethorical", "rhetorical");
            correctionAdd("rethorocal", "rhetorical");

            correctionAdd("Thorium", "thorium");

            correctionAdd("Uranium", "uranium");

            correctionAdd("whit", "with");
            correctionAdd("W/", "with");

            correctionAdd("mcve", "MCVE");

            correctionAdd("new-variable", "New-Variable");

            correctionAdd("convertto-csv", "ConvertTo-Csv");

            correctionAdd("doppler", "Doppler");

            correctionAdd("mathworks", "MathWorks");
            correctionAdd("Mathworks", "MathWorks");

            correctionAdd("sunday", "Sunday");

            correctionAdd("VertX", "Vert.x");

            correctionAdd("appropiate", "appropriate");
            correctionAdd("apropriate", "appropriate");
            correctionAdd("appriopriate", "appropriate");

            correctionAdd("carefull", "careful");

            correctionAdd("parti", "party");

            correctionAdd("rarher", "rather");

            correctionAdd("2nd", "second");

            correctionAdd("to do", "todo");
            correctionAdd("to-do", "todo");

            correctionAdd("irish", "Irish");

            correctionAdd("Googling", "googling");

            correctionAdd("denmark", "Denmark");

            correctionAdd("ocd", "OCD");

            correctionAdd("unity 3d", "Unity 3D");
            correctionAdd("unity3d", "Unity 3D");

            correctionAdd("unconfortable", "uncomfortable");

            correctionAdd("invoke-item", "Invoke-Item");

            correctionAdd("Dansk", "dansk");

            correctionAdd("Engelsk", "engelsk");

            correctionAdd("thier", "their");

            correctionAdd("BREXIT", "Brexit");
            correctionAdd("brexit", "Brexit");

            correctionAdd("docker hub", "Docker Hub");
            correctionAdd("DockerHub", "Docker Hub");
            correctionAdd("Docker hub", "Docker Hub");
            correctionAdd("dockerhub", "Docker Hub");

            correctionAdd("marxism", "Marxism");

            correctionAdd("affraid", "afraid");

            correctionAdd("breifly", "briefly");

            correctionAdd("excetpion", "exception");

            correctionAdd("neighbor", "neighbour");
            correctionAdd("neigbour", "neighbour");

            correctionAdd("untill", "until");

            correctionAdd("Yestarday", "yesterday");

            correctionAdd("duckduckgo", "DuckDuckGo");

            correctionAdd("yahoo answers", "Yahoo Answers");

            correctionAdd("democraticly", "democratically");

            correctionAdd("imposibility", "impossibility");

            correctionAdd("juge", "judge");

            correctionAdd("necssity", "necessity");

            correctionAdd("negociation", "negotiation");

            correctionAdd("oportunity", "opportunity");

            correctionAdd("parlement", "parliament");
            correctionAdd("Parlement", "parliament"); //Not 100% correct - case.
            correctionAdd("parlament", "parliament");

            correctionAdd("china", "China");

            correctionAdd("japan", "Japan");

            correctionAdd("infact", "in fact");
            correctionAdd("in-fact", "in fact");

            correctionAdd("parlamentary", "parliamentary");

            correctionAdd("Android 7", "Android 7.0 (Nougat)");

            correctionAdd("Android 8.1", "Android 8.1 (Oreo)");
            correctionAdd("Android O", "Android 8.1 (Oreo)");
            correctionAdd("Android Oreo", "Android 8.1 (Oreo)");
            correctionAdd("oreo", "Android 8.1 (Oreo)");
            correctionAdd("Oreo", "Android 8.1 (Oreo)");

            correctionAdd("Android 9", "Android 9.0 (Pie)");
            correctionAdd("Android P", "Android 9.0 (Pie)");
            correctionAdd("Android 9.0", "Android 9.0 (Pie)");

            correctionAdd("cordova", "Cordova");

            correctionAdd("attenttion", "attention");
            correctionAdd("atenttion", "attention");

            correctionAdd("inf", "infinite");
            correctionAdd("infinte", "infinite");

            correctionAdd("aero", "Aero");

            correctionAdd("BitDefender", "Bitdefender");

            correctionAdd("european union", "European Union");

            correctionAdd("jfrog", "JFrog");

            correctionAdd("scintilla", "Scintilla");

            correctionAdd("ambigious", "ambiguous");
            correctionAdd("ambigous", "ambiguous");
            correctionAdd("ambiguious", "ambiguous");

            correctionAdd("approch", "approach");
            correctionAdd("aproach", "approach");
            correctionAdd("aproch", "approach");

            correctionAdd("clip-board", "clipboard");
            correctionAdd("CLIP-BOARD", "clipboard");
            correctionAdd("CLIPBOARD", "clipboard");
            correctionAdd("clip board", "clipboard");

            correctionAdd("eaquation", "equation");

            correctionAdd("extensinon", "extension");
            correctionAdd("extention", "extension");
            correctionAdd("Extenstion", "extension");
            correctionAdd("extenstion", "extension");

            correctionAdd("framwork", "framework");

            correctionAdd("hierarchial", "hierarchical");
            correctionAdd("hiarchial", "hierarchical");

            correctionAdd("interprete", "interpret");

            correctionAdd("proceedure", "procedure");
            correctionAdd("precedure", "procedure");

            correctionAdd("seperately", "separately");
            correctionAdd("seperatly", "separately");

            correctionAdd("terriable", "terrible");

            correctionAdd("tittle", "title");

            correctionAdd("visualastion", "visualisation");

            correctionAdd("ltspice", "LTspice");

            correctionAdd("Nato", "NATO");
            correctionAdd("nato", "NATO");

            correctionAdd("R-Studio", "RStudio");

            correctionAdd("brodcast", "broadcast");

            correctionAdd("Capitalist", "capitalist");

            correctionAdd("completly", "completely");
            correctionAdd("comletely", "completely");

            correctionAdd("flexbox", "Flexbox");
            correctionAdd("flex-box", "Flexbox");
            correctionAdd("flex box", "Flexbox");

            correctionAdd("incase", "in case");

            correctionAdd("permisson", "permission");
            correctionAdd("perm", "permission");
            correctionAdd("Permision", "permission");
            correctionAdd("permision", "permission");

            correctionAdd("personnal", "personal");

            correctionAdd("probaly", "probably");
            correctionAdd("Problably", "probably");

            correctionAdd("sereval", "several");
            correctionAdd("sevral", "several");

            correctionAdd("Socialism", "socialism");

            correctionAdd("tryed", "tried");
            correctionAdd("tryd", "tried");

            correctionAdd("justfied", "justified");

            correctionAdd("perfrom", "perform");

            correctionAdd("dutch", "Dutch");

            correctionAdd("broswer", "browser");

            correctionAdd("Printf", "printf");

            correctionAdd("Fet", "FET");
            correctionAdd("fet", "FET");

            correctionAdd("Lora", "LoRa");
            correctionAdd("lora", "LoRa");

            correctionAdd("squarespace", "Squarespace");

            correctionAdd("Teamcity", "TeamCity");
            correctionAdd("teamcity", "TeamCity");
            correctionAdd("Team City", "TeamCity");

            correctionAdd("write-error", "Write-Error");

            correctionAdd("directy", "directly");

            correctionAdd("dicovered", "discovered");

            correctionAdd("descrete", "discrete");

            correctionAdd("litle", "little");
            correctionAdd("litte", "little");

            correctionAdd("soltuion", "solution");
            correctionAdd("sollution", "solution");
            correctionAdd("Soloution", "solution");
            correctionAdd("soloution", "solution");
            correctionAdd("solutuion", "solution");

            correctionAdd("threeshold", "threshold");
            correctionAdd("treeshold", "threshold");
            correctionAdd("thershold", "threshold");
            correctionAdd("treshold", "threshold");

            correctionAdd("verision", "version");
            correctionAdd("verison", "version");

            correctionAdd("aliexpress", "AliExpress");
            correctionAdd("aliexpres", "AliExpress");

            correctionAdd("BW", "bandwidth");
            correctionAdd("bandwith", "bandwidth");

            correctionAdd("buracracy", "bureaucracy");
            correctionAdd("buracrocy", "bureaucracy");

            correctionAdd("buereaucrat", "bureaucrat");
            correctionAdd("buesuresucrat", "bureaucrat");

            correctionAdd("giberish", "gibberish");

            correctionAdd("happynness", "happiness");
            correctionAdd("happinness", "happiness");

            correctionAdd("manger", "manager");

            correctionAdd("realease", "release");
            correctionAdd("releas", "release");

            correctionAdd("seperatist", "separatist");

            correctionAdd("activestate", "ActiveState");
            correctionAdd("Activestate", "ActiveState");
            correctionAdd("Active state", "ActiveState");
            correctionAdd("Active State", "ActiveState");
            correctionAdd("active state", "ActiveState");

            correctionAdd("diac", "DIAC");
            correctionAdd("Diac", "DIAC");

            correctionAdd("anoying", "annoying");

            correctionAdd("apprear", "appear");
            correctionAdd("apear", "appear");

            correctionAdd("mispell", "misspell");

            correctionAdd("uA", "µA");

            correctionAdd("Windows 2016", "Windows Server 2016");
            correctionAdd("windows 2016", "Windows Server 2016");
            correctionAdd("Server 2016", "Windows Server 2016");

            correctionAdd("appriciated", "appreciated");
            correctionAdd("appericated", "appreciated");

            correctionAdd("LA", "logic analyser");
            correctionAdd("Logic Analyzer", "logic analyser");

            correctionAdd("netbios-ns", "port 137");

            correctionAdd("netbios-dgm", "port 138");

            correctionAdd("netbios-ssn", "port 139");

            correctionAdd("microsoft-ds", "port 445");

            correctionAdd("ansi", "ANSI");

            correctionAdd("fibonacci", "Fibonacci");
            correctionAdd("Fibbonaci", "Fibonacci");
            correctionAdd("Fibbonacci", "Fibonacci");

            correctionAdd("graphql", "GraphQL");
            correctionAdd("graphQL", "GraphQL");

            correctionAdd("MSys", "MSYS");

            correctionAdd("Mac mini", "Mac Mini");
            correctionAdd("mac mini", "Mac Mini");

            correctionAdd("scpi", "SCPI");

            correctionAdd("usbtmc", "USBTMC");

            correctionAdd("inorder", "in order");

            correctionAdd("incriment", "increment");

            correctionAdd("ma", "mA");

            correctionAdd("acl", "ACL");

            correctionAdd("midnight commander", "Midnight Commander");

            correctionAdd("SHIFT", "Shift");
            correctionAdd("shift", "Shift");

            correctionAdd("x509", "X.509");

            correctionAdd("allready", "already");
            correctionAdd("alredy", "already");

            correctionAdd("buisness", "business");
            correctionAdd("busines", "business");
            correctionAdd("biz", "business");
            correctionAdd("buisiness", "business");

            correctionAdd("explicitely", "explicitly");
            correctionAdd("expicitely", "explicitly");
            correctionAdd("explicity", "explicitly");

            correctionAdd("hebrew", "Hebrew");

            correctionAdd("implimenting", "implementing");

            correctionAdd("prefectly", "perfectly");

            correctionAdd("phonemenia", "phonomena");

            correctionAdd("possebilities", "possibilities");

            correctionAdd("pr", "pull request");
            correctionAdd("PR", "pull request");
            correctionAdd("Pull Request", "pull request");

            correctionAdd("recursivly", "recursively");

            correctionAdd("stubled", "stumble");

            correctionAdd("writen", "written");

            correctionAdd("BSC", "BSc");

            correctionAdd("hindi", "Hindi");

            correctionAdd("accouting", "accounting");

            correctionAdd("bio", "biography");
            correctionAdd("biografy", "biography");

            correctionAdd("discriptive", "descriptive");

            correctionAdd("exp", "experience");
            correctionAdd("experince", "experience");
            correctionAdd("expirians", "experience");

            correctionAdd("low level", "low-level");
            correctionAdd("low-kevel", "low-level");

            correctionAdd("PWA", "progressive web application");
            correctionAdd("Progressive Web Application", "progressive web application");
            correctionAdd("Progressive Web App", "progressive web application");
            correctionAdd("Progressive Web app", "progressive web application");
            correctionAdd("progressive web app", "progressive web application");
            correctionAdd("Progressive web app", "progressive web application");
            correctionAdd("pwa", "progressive web application");

            correctionAdd("RWD", "responsive web design");

            correctionAdd("themself", "themselves");

            correctionAdd("121gw", "121GW");

            correctionAdd("BJT", "bipolar junction transistor");

            correctionAdd("cam", "camera");

            correctionAdd("checkout", "check out");
            correctionAdd("Checkout", "check out");

            correctionAdd("prof", "professor");

            correctionAdd("re-writing", "rewriting");

            correctionAdd("anyways", "anyway");
            correctionAdd("Anyyay", "anyway");
            correctionAdd("anyyay", "anyway");


            correctionAdd("breadbord", "breadboard");

            correctionAdd("convinient", "convenient");
            correctionAdd("conventient", "convenient");

            correctionAdd("howver", "however");
            correctionAdd("how ever", "however");

            correctionAdd("Perf Board", "perfboard");
            correctionAdd("Purfboard", "perfboard");
            correctionAdd("perf board", "perfboard");
            correctionAdd("purfboard", "perfboard");

            correctionAdd("Responsive Web App", "responsive web application");

            correctionAdd("skeptic", "sceptic");

            correctionAdd("Breadbord", "breadboard");
            correctionAdd("Breadboard", "breadboard");

            correctionAdd("blitz++", "Blitz++");
            correctionAdd("Blitz", "Blitz++");
            correctionAdd("blitz", "Blitz++");

            correctionAdd("c++20", "C++20");

            correctionAdd("GoT", "Game of Thrones");
            correctionAdd("game of thrones", "Game of Thrones");
            correctionAdd("GOT", "Game of Thrones");

            correctionAdd("google cloud", "Google Cloud Platform");

            correctionAdd("Huwei", "Huawei");

            correctionAdd("ammount", "amount");

            correctionAdd("any where", "anywhere");
            correctionAdd("anyhere", "anywhere");
            correctionAdd("anywehre", "anywhere");

            correctionAdd("aksing", "asking");

            correctionAdd("calc", "calculate");
            correctionAdd("claculate", "calculate");
            correctionAdd("calcualte", "calculate");

            correctionAdd("consistantly", "consistently");

            correctionAdd("else where", "elsewhere");

            correctionAdd("familar", "familiar");

            correctionAdd("IIUC", "if I understand correctly");

            correctionAdd("key word", "keyword");

            correctionAdd("Lemme", "let me");
            correctionAdd("lemme", "let me");

            correctionAdd("milisecond", "millisecond");
            correctionAdd("millisec", "millisecond");
            correctionAdd("mS", "millisecond");

            correctionAdd("programm", "program");

            correctionAdd("RLE", "run-length encoding");

            correctionAdd("runnig", "running");

            correctionAdd("spread-sheet", "spreadsheet");

            correctionAdd("sintax", "syntax");
            correctionAdd("sytax", "syntax");

            correctionAdd("YUML", "yUML");
            correctionAdd("yuml", "yUML");

            correctionAdd("atmega", "ATmega");
            correctionAdd("ATMega", "ATmega");
            correctionAdd("ATMEGA", "ATmega");

            correctionAdd("delhi", "Delhi");

            correctionAdd("electron", "Electron");

            correctionAdd("india", "India");

            correctionAdd("Puttygen", "PuTTYgen");

            correctionAdd("ack", "acknowledge");
            correctionAdd("ackknowledge", "acknowledge");

            correctionAdd("committment", "commitment");

            correctionAdd("defualt", "default");
            correctionAdd("defult", "default");
            correctionAdd("defaul", "default");
            correctionAdd("dafault", "default");

            correctionAdd("4th", "fourth");

            correctionAdd("fullfill", "fulfill");
            correctionAdd("full fill", "fulfill");
            correctionAdd("ful fill", "fulfill");

            correctionAdd("it self", "itself");
            correctionAdd("itsself", "itself");

            correctionAdd("knowhow", "know-how");

            correctionAdd("milliamps", "milliamperes");

            correctionAdd("over written", "overwritten");
            correctionAdd("over writen", "overwritten");

            correctionAdd("Personal Access Token", "personal access token");

            correctionAdd("repeatative", "repetitive");

            correctionAdd("sinewave", "sine wave");

            correctionAdd("UB", "undefined behaviour");

            correctionAdd("TI", "Texas Instruments");

            correctionAdd("webpack", "Webpack");
            correctionAdd("WebPack", "Webpack");
            correctionAdd("Web pack", "Webpack");
            correctionAdd("web pack", "Webpack");

            correctionAdd("Ad Hoc", "ad hoc");
            correctionAdd("adhoc", "ad hoc");
            correctionAdd("ad-hoc", "ad hoc");

            correctionAdd("cappacitive", "capacitive");

            correctionAdd("coord", "coordinate");

            correctionAdd("finaly", "finally");
            correctionAdd("finnaly", "finally");

            correctionAdd("Hydrogen", "hydrogen");

            correctionAdd("lang", "language");
            correctionAdd("langauge", "language");
            correctionAdd("lanuguage", "language");

            correctionAdd("mins", "minutes");

            correctionAdd("sourcetree", "Sourcetree");
            correctionAdd("SourceTree", "Sourcetree");

            correctionAdd("env var", "environment variable");
            correctionAdd("Env Var", "environment variable");
            correctionAdd("ENV-Variable", "environment variable");

            correctionAdd("WINDOWS EXPLORER", "Windows&nbsp;Explorer");

            correctionAdd("asap", "ASAP");

            correctionAdd("qwerty", "QWERTY");

            correctionAdd("spring boot", "Spring Boot");
            correctionAdd("Spring boot", "Spring Boot");

            correctionAdd("accesible", "accessible");

            correctionAdd("collumn", "column");
            correctionAdd("colum", "column");
            correctionAdd("colume", "column");
            correctionAdd("col", "column");

            correctionAdd("crated", "created");
            correctionAdd("creaded", "created");

            correctionAdd("exlusive", "exclusive");

            correctionAdd("Iron", "iron");

            correctionAdd("loks", "looks");

            correctionAdd("meaddle", "middle");

            correctionAdd("Nitrogen", "nitrogen");

            correctionAdd("scalling", "scaling");

            correctionAdd("Service Worker", "service worker");

            correctionAdd("sign-in", "sign in");

            correctionAdd("substution", "substitution");

            correctionAdd("symetrically", "symmetrically");

            correctionAdd("trouble shooting", "troubleshooting");

            correctionAdd("workorder", "work order");

            correctionAdd("costomer", "customer");
            correctionAdd("costumer", "customer");

            correctionAdd("jetbrains", "JetBrains");
            correctionAdd("Jetbrains", "JetBrains");

            correctionAdd("SQL workbench", "MySQL Workbench");

            correctionAdd("Blockchain", "blockchain");

            correctionAdd("commited", "committed");

            correctionAdd("High School", "high school");

            correctionAdd("no-op", "no operation");

            correctionAdd("profficient", "proficient");

            correctionAdd("so called", "so-called");

            correctionAdd("Software Engineer", "software engineer");

            correctionAdd("Swarm Intelligence", "swarm intelligence");

            correctionAdd("thingie", "thingy");

            correctionAdd("stripe", "Stripe");

            correctionAdd("upwork", "Upwork");
            correctionAdd("UpWork", "Upwork");
            correctionAdd("Up work", "Upwork");

            correctionAdd("à la", "a la");

            correctionAdd("asc", "ascending");

            correctionAdd("Freelancer", "freelancer");

            correctionAdd("genereated", "generated");

            correctionAdd("hight", "height");
            correctionAdd("hieght", "height");
            correctionAdd("heigh", "height");
            correctionAdd("heght", "height");

            correctionAdd("malcious", "malicious");

            correctionAdd("neglegible", "negligible");

            correctionAdd("recntly", "recently");

            correctionAdd("runtime", "run time");

            correctionAdd("specifiy", "specify");

            correctionAdd("splitted", "split");

            correctionAdd("spontaniously", "spontaneously");

            correctionAdd("Truely", "truly");
            correctionAdd("truely", "truly");

            correctionAdd("Voilla", "voilà");
            correctionAdd("voila", "voilà");
            correctionAdd("voilá", "voilà"); // This doesn't work - the last
            //                                  letter, "á", is not seen
            //                                  in the lookup...:
            //
            //                                  "Could not lookup voil!"

            correctionAdd("catch 22", "Catch-22");
            correctionAdd("catch-22", "Catch-22");
            correctionAdd("Catch 22", "Catch-22");

            correctionAdd("msc", "MSc");

            correctionAdd("Buster", "Raspbian&nbsp;10 (Buster)");

            correctionAdd("thz", "THz");
            correctionAdd("Thz", "THz");

            correctionAdd("transifex", "Transifex");

            correctionAdd("Additionaly", "additionally");
            correctionAdd("additionaly", "additionally");
            correctionAdd("Additionnally", "additionally");
            correctionAdd("additionnally", "additionally");

            correctionAdd("bare bones", "bare-bones");

            correctionAdd("amperage", "current");
            correctionAdd("ampacity", "current");

            correctionAdd("never-the-less", "nevertheless");
            correctionAdd("never the less", "nevertheless");
            correctionAdd("Never the less", "nevertheless");

            correctionAdd("none the less", "nonetheless");

            correctionAdd("odesk", "oDesk");

            correctionAdd("obejct", "object");

            correctionAdd("preceede", "precede");

            correctionAdd("ref", "reference");
            correctionAdd("refernce", "reference");
            correctionAdd("refrence", "reference");

            correctionAdd("stdin", "standard input");

            correctionAdd("2D", "two-dimensional");
            correctionAdd("2-d", "two-dimensional");

            correctionAdd("u-block origin", "uBlock Origin");
            correctionAdd("ublock origin", "uBlock Origin");
            correctionAdd("Ublock Origin", "uBlock Origin");

            correctionAdd("nsfw", "NSFW");

            correctionAdd("pde", "PDE");

            correctionAdd("waffen SS", "Waffen-SS");
            correctionAdd("Waffen SS", "Waffen-SS");
            correctionAdd("waffen ss", "Waffen-SS");

            correctionAdd("aircrafts", "aircraft");
            correctionAdd("air craft", "aircraft");

            correctionAdd("boarder", "border");

            correctionAdd("civillian", "civilian");

            correctionAdd("diffrentiate", "differentiate");
            correctionAdd("differ", "differentiate");
            correctionAdd("differenciate", "differentiate");

            correctionAdd("Uwsgi", "uWSGI");
            correctionAdd("UWSGI", "uWSGI");

            correctionAdd("wellbeing", "well-being");

            correctionAdd("with in", "within");

            correctionAdd("warry", "worry");

            correctionAdd("fortnite", "Fortnite");

            correctionAdd("ISO-8859-1", "ISO 8859-1");

            correctionAdd("lxc", "LXC");

            correctionAdd("mstest", "MSTest");

            correctionAdd("MDN", "Mozilla Developer Network");

            correctionAdd("pluarlsight", "Pluralsight");
            correctionAdd("PluralSight", "Pluralsight");
            correctionAdd("pluralsight", "Pluralsight");

            correctionAdd("Stylecop", "StyleCop");
            correctionAdd("stylecop", "StyleCop");

            correctionAdd("windows 1252", "Windows-1252");
            correctionAdd("WINDOWS-1252", "Windows-1252");
            correctionAdd("windows-1252", "Windows-1252");

            correctionAdd("appricate", "appreciate");
            correctionAdd("appreicate", "appreciate");
            correctionAdd("appriciate", "appreciate");

            correctionAdd("asume", "assume");

            correctionAdd("chalenging", "challenging");

            correctionAdd("competant", "competent");

            correctionAdd("diffrence", "difference");

            correctionAdd("disasterous", "disastrous");

            correctionAdd("eaay", "easy");

            correctionAdd("gamfication", "gamification");
            correctionAdd("gameification", "gamification");
            correctionAdd("gamefication", "gamification");

            correctionAdd("guide line", "guideline");

            correctionAdd("highlited", "highlighted");

            correctionAdd("idealy", "ideally");

            correctionAdd("judgment", "judgement");

            correctionAdd("on going", "ongoing");

            correctionAdd("pos", "position");
            correctionAdd("postion", "position");
            correctionAdd("possition", "position");

            correctionAdd("vehical", "vehicle");

            correctionAdd("wierd", "weird");
            correctionAdd("weried", "weird");

            correctionAdd("squach", "squash");

            correctionAdd("ocr", "OCR");

            correctionAdd("odbc", "ODBC");

            correctionAdd("docker-compose", "Docker Compose");

            // Of PowerShell...
            correctionAdd("get-diskimage", "Get-DiskImage");

            correctionAdd("konqueror", "Konqueror");

            correctionAdd("anyother", "another");

            correctionAdd("catagory", "category");

            correctionAdd("Ccache", "ccache");

            correctionAdd("constitude", "constitute");

            correctionAdd("debuging", "debugging");
            correctionAdd("debuggin", "debugging");

            correctionAdd("Distcc", "distcc");

            correctionAdd("eigther", "either");
            correctionAdd("eiether", "either");

            correctionAdd("FPS", "fps");

            correctionAdd("furhter", "further");

            correctionAdd("internaly", "internally");

            correctionAdd("mondane", "mundane");

            correctionAdd("neigher", "neither");
            correctionAdd("neigter", "neither");
            correctionAdd("nither", "neither");

            correctionAdd("offical", "official");

            correctionAdd("other wise", "otherwise");

            correctionAdd("res", "resolution");

            correctionAdd("scrollbar", "scroll bar");
            correctionAdd("scroll-bar", "scroll bar");

            correctionAdd("writting", "writing");

            correctionAdd("vagrant", "Vagrant");

            correctionAdd("stil", "still");
            correctionAdd("steal", "still");

            correctionAdd("Godbolt", "Compiler Explorer");
            correctionAdd("godbolt", "Compiler Explorer");

            correctionAdd("Digital Ocean", "DigitalOcean");

            correctionAdd("EXIF", "Exif");
            correctionAdd("exif", "Exif");

            correctionAdd("fifo", "FIFO");

            correctionAdd("google scholar", "Google Scholar");

            correctionAdd("javafaces", "JavaServer Faces");

            correctionAdd("php 7", "PHP&nbsp;7");
            correctionAdd("php7", "PHP&nbsp;7");

            correctionAdd("Power Bi", "Power BI");
            correctionAdd("powerbi", "Power BI");
            correctionAdd("Powerbi", "Power BI");
            correctionAdd("PowerBI", "Power BI");

            correctionAdd("SEDE", "Stack Exchange Data Explorer");
            correctionAdd("sede", "Stack Exchange Data Explorer");

            correctionAdd("alledged", "alleged");
            correctionAdd("allegded", "alleged");

            correctionAdd("alllow", "allow");

            correctionAdd("almos", "almost");

            correctionAdd("answeared", "answered");

            correctionAdd("anti aliasing", "antialiasing");
            correctionAdd("Anti aliasing", "antialiasing");

            correctionAdd("AFAICT", "as far as I can tell");
            correctionAdd("Afaict", "as far as I can tell");

            correctionAdd("asignment", "assignment");

            correctionAdd("childs", "children");

            correctionAdd("commmented", "commented");

            correctionAdd("daugther", "daughter");

            correctionAdd("essentialy", "essentially");

            correctionAdd("formating", "formatting");

            correctionAdd("framerate", "frame rate");

            correctionAdd("independantly", "independently");

            correctionAdd("manuver", "maneuver");

            correctionAdd("mind set", "mindset");

            correctionAdd("negatvie", "negative");
            correctionAdd("neg", "negative");

            correctionAdd("opinon", "opinion");

            correctionAdd("prio", "priority");

            correctionAdd("prject", "project");

            correctionAdd("simultaneusly", "simultaneously");

            correctionAdd("sligtly", "slightly");

            correctionAdd("supose", "suppose");
            correctionAdd("supost", "suppose");

            correctionAdd("jure", "sure");
            correctionAdd("shure", "sure");

            correctionAdd("verical", "vertical");
            correctionAdd("verticle", "vertical");

            correctionAdd("you ll", "you'll");

            correctionAdd("APOLLO", "Apollo");

            correctionAdd("Applocker", "AppLocker");
            correctionAdd("applocker", "AppLocker");

            correctionAdd("argentina", "Argentina");

            correctionAdd("ARTEMIS", "Artemis");

            correctionAdd("Buzzfeed", "BuzzFeed");

            correctionAdd("cr2032", "CR2032");

            correctionAdd("CSS 2", "CSS&nbsp;2");
            correctionAdd("css2", "CSS&nbsp;2");

            correctionAdd("celsius", "Celsius");
            correctionAdd("celcius", "Celsius");

            correctionAdd("eu", "EU");
            correctionAdd("Eu", "EU");

            correctionAdd("england", "England");

            correctionAdd("Fyi", "FYI");

            correctionAdd("file explorer", "File Explorer");
            correctionAdd("Windows file explorer", "File Explorer");

            correctionAdd("fluke", "Fluke");
            correctionAdd("FLuke", "Fluke");

            correctionAdd("gecko", "Gecko");

            correctionAdd("gibraltar", "Gibraltar");

            correctionAdd("kubuntu", "Kubuntu");

            correctionAdd("Luke skywalker", "Luke Skywalker");

            correctionAdd("milky way", "Milky Way");
            correctionAdd("Milkyway", "Milky Way");

            correctionAdd("ninja", "Ninja");

            correctionAdd("norther ireland", "Northern Ireland");

            correctionAdd("Ponzi Scheme", "Ponzi scheme");
            correctionAdd("ponzi scheme", "Ponzi scheme");

            correctionAdd("sandboxie", "Sandboxie");

            correctionAdd("scotland", "Scotland");

            correctionAdd("spacex", "SpaceX");

            correctionAdd("tcpview", "TCPView");

            correctionAdd("Tera-term", "Tera Term");
            correctionAdd("teraterm", "Tera Term");

            correctionAdd("visual studio professional", "Visual Studio Professional");

            correctionAdd("wto", "WTO");

            correctionAdd("wales", "Wales");

            correctionAdd("xubuntu", "Xubuntu");

            correctionAdd("adress", "address");
            correctionAdd("addres", "address");
            correctionAdd("adres", "address");

            correctionAdd("bare in mind", "bear in mind");

            correctionAdd("baring in mind", "bearing in mind");

            correctionAdd("bombardement", "bombardment");

            correctionAdd("bonafide", "bona fide");

            correctionAdd("brillant", "brilliant");

            correctionAdd("costal", "coastal");

            correctionAdd("collpase", "collapse");

            correctionAdd("comming", "coming");

            correctionAdd("compition", "competition");

            correctionAdd("concious", "conscious");

            correctionAdd("dept", "department");

            correctionAdd("en counter", "encounter");

            correctionAdd("excatly", "exactly");
            correctionAdd("EXACLTY", "exactly");
            correctionAdd("exaclty", "exactly");

            correctionAdd("GammaRay Burst", "gamma-ray burst");

            correctionAdd("genious", "genius");

            correctionAdd("geniouss", "geniuses");

            correctionAdd("hydrozine", "hydrazine");

            correctionAdd("inheritly", "inherently");
            correctionAdd("inhereting", "inherently");

            correctionAdd("insecting", "inspecting");
            correctionAdd("Insecting", "inspecting");

            correctionAdd("instuction", "instruction");

            correctionAdd("Km", "km");
            correctionAdd("KM", "km");

            correctionAdd("lastest", "latest");

            correctionAdd("Lithium", "lithium");

            correctionAdd("M", "m");

            correctionAdd("Mac Os", "macOS");

            correctionAdd("Netstat", "netstat");

            correctionAdd("new comer", "newcomer");
            correctionAdd("new-comer", "newcomer");

            correctionAdd("no brainer", "no-brainer");

            correctionAdd("on-premise", "on-premises");
            correctionAdd("on premise", "on-premises");

            correctionAdd("outputing", "outputting");

            correctionAdd("ph", "pH");

            correctionAdd("Per Se", "per se");
            correctionAdd("per-se", "per se");

            correctionAdd("preceeding", "preceding");

            correctionAdd("prestigous", "prestigious");

            correctionAdd("psycological", "psychological");

            correctionAdd("rememer", "remember");

            correctionAdd("re-written", "rewritten");

            correctionAdd("roughy", "roughly");

            correctionAdd("scallable", "scalable");
            correctionAdd("scaleable", "scalable");

            correctionAdd("scrol", "scroll");

            correctionAdd("sentance", "sentence");
            correctionAdd("sentencen", "sentence");

            correctionAdd("sideeffect", "side effect");

            correctionAdd("spell-check", "spellcheck");
            correctionAdd("spell check", "spellcheck");

            correctionAdd("strugling", "struggling");

            correctionAdd("superflous", "superfluous");

            correctionAdd("Supervolcano", "supervolcano");

            correctionAdd("ulitmately", "ultimately");
            correctionAdd("Ulitmately", "ultimately");

            correctionAdd("undergrad", "undergraduate");

            correctionAdd("validade", "validate");

            correctionAdd("Visa", "visa");
            correctionAdd("VISA", "visa");

            correctionAdd("wan't", "want");
            correctionAdd("whant", "want");

            correctionAdd("work force", "workforce");

            correctionAdd("writeable", "writable");

            correctionAdd("net core 3.0", ".NET Core 3.0");

            correctionAdd("asp net core 3.0", "ASP.NET Core 3.0");

            correctionAdd("eslint", "ESLint");

            correctionAdd("elance", "Elance");

            correctionAdd("material design", "Material Design");
            correctionAdd("Material design", "Material Design");

            correctionAdd("Ntp", "NTP");
            correctionAdd("ntp", "NTP");

            correctionAdd("pomodoro", "Pomodoro");
            correctionAdd("pomodorro", "Pomodoro");

            correctionAdd("Remote Procedure Call", "RPC");
            correctionAdd("rpc", "RPC");

            correctionAdd("ufw", "UFW");

            correctionAdd("VS2019", "Visual&nbsp;Studio&nbsp;2019");
            correctionAdd("vs2019", "Visual&nbsp;Studio&nbsp;2019");
            correctionAdd("Visual studio 2019", "Visual&nbsp;Studio&nbsp;2019");
            correctionAdd("Visual Studio 2019", "Visual&nbsp;Studio&nbsp;2019");

            correctionAdd("xor", "XOR");
            correctionAdd("Xor", "XOR");

            correctionAdd("absorbtion", "absorption");

            correctionAdd("arxiv", "arXiv");

            correctionAdd("buddhist", "Buddhist");

            correctionAdd("calulation", "calculation");

            correctionAdd("childd", "child");

            correctionAdd("contruct", "construct");

            correctionAdd("counter productive", "counterproductive");

            correctionAdd("efort", "effort");

            correctionAdd("5th", "fifth");

            correctionAdd("greather", "greater");

            correctionAdd("induvidual", "individual");

            correctionAdd("Nevermind", "never mind");
            correctionAdd("nevermind", "never mind");

            correctionAdd("procastination", "procrastination");

            correctionAdd("b2b", "B2B");

            correctionAdd("baidu", "Baidu");

            correctionAdd("muslim", "Muslim");

            correctionAdd("pyvisa", "PyVISA");
            correctionAdd("PyVisa", "PyVISA");
            correctionAdd("pyVisa", "PyVISA");
            correctionAdd("Pyvisa", "PyVISA");

            correctionAdd("samsung", "Samsung");

            correctionAdd("Unix&Linux", "Unix & Linux");
            correctionAdd("U&L", "Unix & Linux");

            correctionAdd("Whitehouse", "White House");
            correctionAdd("white house", "White House");
            correctionAdd("WHouse", "White House");

            correctionAdd("comminicating", "communicating");

            correctionAdd("lobbyisits", "lobbyists");

            correctionAdd("manualy", "manually");
            correctionAdd("mannually", "manually");

            correctionAdd("multi meter", "multimeter");

            correctionAdd("serure", "secure");

            correctionAdd("stucked", "stuck");

            correctionAdd("swtich", "switch");

            correctionAdd("Yak shaving", "yak shaving");

            correctionAdd("CSS Grid", "CSS grid");

            correctionAdd("cloudfront", "Amazon CloudFront");

            correctionAdd("creative commons", "Creative Commons");
            correctionAdd("Creative Common", "Creative Commons");

            correctionAdd("flutter", "Flutter");

            correctionAdd("Hawai", "Hawaii");

            correctionAdd("Hongkong", "Hong Kong");

            correctionAdd("Komodo ide", "Komodo IDE");

            correctionAdd("Peurto Rico", "Puerto Rico");
            correctionAdd("Purto Rico", "Puerto Rico");

            correctionAdd("trump", "Trump");

            correctionAdd("Yugoeslavia", "Yugoslavia");

            correctionAdd("abades", "abate");

            correctionAdd("Acre", "acre");

            correctionAdd("acticity", "activity");

            correctionAdd("anually", "annually");

            correctionAdd("back tick", "backtick");
            correctionAdd("back-tick", "backtick");

            correctionAdd("buch", "bunch");

            correctionAdd("exagerrated", "exaggerated");

            correctionAdd("expection", "expectation");

            correctionAdd("incidently", "incidentally");

            correctionAdd("Midevil", "medieval");

            correctionAdd("misspelled", "misspelt");

            correctionAdd("prefrence", "preference");

            correctionAdd("profiterable", "profitable");

            correctionAdd("receeding", "receding");

            correctionAdd("stoped", "stopped");

            correctionAdd("stratigic", "strategic");

            correctionAdd("sugest", "suggest");

            correctionAdd("top level domain", "top-level domain");
            correctionAdd("Top Level Domain", "top-level domain");
            correctionAdd("TLD", "top-level domain");

            correctionAdd("tranfer", "transfer");

            correctionAdd("Trillion", "trillion");

            correctionAdd("useage", "usage");

            correctionAdd("onedrive", "OneDrive");

            correctionAdd("Apache spark", "Apache Spark");
            correctionAdd("spark", "Apache Spark");
            correctionAdd("Spark", "Apache Spark");
            correctionAdd("sPARK", "Apache Spark");

            correctionAdd("ES 5", "ECMAScript&nbsp;5");
            correctionAdd("ES5", "ECMAScript&nbsp;5");
            correctionAdd("ECMAScript5", "ECMAScript&nbsp;5");

            correctionAdd("Isreal", "Israel");

            correctionAdd("Jurasalem", "Jerusalem");

            correctionAdd("kaggle", "Kaggle");

            correctionAdd("kindle", "Kindle");

            correctionAdd("Mw", "MW");

            correctionAdd("noble prize", "Nobel Prize");

            correctionAdd("abilites", "abilities");

            correctionAdd("advetise", "advertise");

            correctionAdd("Capacitance", "capacitance");

            correctionAdd("cost effective", "cost-effective");

            correctionAdd("encrytpion", "encryption");

            correctionAdd("guess work", "guesswork");

            correctionAdd("imporant", "important");

            correctionAdd("intrested", "interested");

            correctionAdd("Iodine", "iodine");

            correctionAdd("irreplacable", "irreplaceable");

            correctionAdd("literaly", "literally");
            correctionAdd("literarily", "literally");

            correctionAdd("maintable", "maintainable");

            correctionAdd("object orientated", "object-oriented");
            correctionAdd("object oriented", "object-oriented");
            correctionAdd("Object orientated", "object-oriented");

            correctionAdd("obselete", "obsolete");

            correctionAdd("para mount", "paramount");

            correctionAdd("powerplant", "power plant");

            correctionAdd("publising", "publishing");

            correctionAdd("quick and dirty", "quick-and-dirty");

            correctionAdd("recipint", "recipient");
            correctionAdd("receipent", "recipient");

            correctionAdd("requirment", "requirement");

            correctionAdd("vonuravle", "vulnerable");

            correctionAdd("adhd", "ADHD");

            correctionAdd("crc32", "CRC-32");

            correctionAdd("ES 2016", "ECMAScript&nbsp;7");
            correctionAdd("ES2016", "ECMAScript&nbsp;7");

            correctionAdd("october", "October");

            correctionAdd("swi prolog", "SWI-Prolog");
            correctionAdd("swi Prolog", "SWI-Prolog");

            correctionAdd("UES", "UEStudio");

            correctionAdd("aswell", "as well");

            correctionAdd("documentated", "documented");

            correctionAdd("expirienced", "experienced");

            correctionAdd("openning", "opening");

            correctionAdd("requst", "request");

            correctionAdd("temorarily", "temporarily");

            correctionAdd("wieght", "weight");

            correctionAdd("Beligum", "Belgium");
            correctionAdd("beligum", "Belgium");

            correctionAdd("bunsenlabs", "BunsenLabs (Debian Linux-based)");
            correctionAdd("Bunsen", "BunsenLabs (Debian Linux-based)");

            correctionAdd("codepen", "CodePen");
            correctionAdd("Codepen", "CodePen");

            correctionAdd("Emporer", "Emperor");

            correctionAdd("wep", "WEP");

            correctionAdd("anoymous", "anonymous");

            correctionAdd("aproved", "approved");

            correctionAdd("asigning", "assigning");

            correctionAdd("coockies", "cookies");

            correctionAdd("hidding", "hiding");

            correctionAdd("in-house", "in house");
            correctionAdd("inhouse", "in house");

            correctionAdd("knwo", "know");
            correctionAdd("khow", "know");

            correctionAdd("manu", "menu");

            correctionAdd("mising", "missing");

            correctionAdd("premiss", "premise");

            correctionAdd("promp", "prompt");

            correctionAdd("seting", "setting");

            correctionAdd("suposed", "supposed");

            correctionAdd("timezone", "time zone");

            correctionAdd("Busybox", "BusyBox");

            correctionAdd("IRIS", "Iris data set");
            correctionAdd("iris data set", "Iris data set");
            correctionAdd("iris", "Iris data set");
            correctionAdd("Iris", "Iris data set");

            correctionAdd("khtml", "KHTML");

            correctionAdd("msysGit", "MSysGit");
            correctionAdd("msysgit", "MSysGit");

            correctionAdd("netscape", "Netscape");

            correctionAdd("sse2", "SSE2");

            correctionAdd("toybox", "Toybox");

            correctionAdd("constatnt", "constant");

            correctionAdd("dataset", "data set");

            correctionAdd("delimeter", "delimiter");

            correctionAdd("democrasy", "democracy");
            correctionAdd("democacy", "democracy");
            correctionAdd("democrazy", "democracy");
            correctionAdd("busybox", "BusyBox");

            correctionAdd("en mass", "en masse");

            correctionAdd("flaging", "flagging");

            correctionAdd("flexiblity", "flexibilty");

            correctionAdd("iliterate", "illiterate");

            correctionAdd("mutiple", "multiple");

            correctionAdd("pronunce", "pronounce");

            correctionAdd("quiting", "quitting");

            correctionAdd("responce", "response");

            correctionAdd("re-use", "reuse");

            correctionAdd("seperating", "separating");

            correctionAdd("CloudFlare", "Cloudflare");

            correctionAdd("Debian&nbsp;10", "Debian&nbsp;10 (Buster)");
            correctionAdd("Debian 10", "Debian&nbsp;10 (Buster)");

            correctionAdd("discord", "Discord");

            correctionAdd("hfs", "HFS");

            correctionAdd("HFS+", "HFS Plus");

            correctionAdd("PL-1", "PL/I");
            correctionAdd("PL1", "PL/I");

            correctionAdd("xfs", "XFS");

            correctionAdd("calss", "class");
            correctionAdd("Class", "class");

            correctionAdd("idear", "idea");

            correctionAdd("incomming", "incoming");

            correctionAdd("plagarism", "plagiarism");

            correctionAdd("Proxy", "proxy");

            correctionAdd("Ransomware", "ransomware");

            correctionAdd("reciver", "receiver");
            correctionAdd("reciever", "receiver");

            correctionAdd("unbeknowst", "unbeknownst");

            correctionAdd("hello world", "Hello, World!");
            correctionAdd("Hello, world", "Hello, World!");
            correctionAdd("Hello, world!", "Hello, World!");
            correctionAdd("Hello World", "Hello, World!");
            correctionAdd("Hello world", "Hello, World!");
            correctionAdd("hello word", "Hello, World!");
            correctionAdd("hallo world", "Hello, World!");
            correctionAdd("Hello, World", "Hello, World!");
            correctionAdd("hello, world", "Hello, World!");
            correctionAdd("hellowolrd", "Hello, World!");
            correctionAdd("helloworld", "Hello, World!");
            correctionAdd("Hello World!", "Hello, World!");
            correctionAdd("hello World", "Hello, World!");

            correctionAdd("Social media", "social media");
            correctionAdd("Social Media", "social media");

            correctionAdd("DDD", "domain-driven design"); //Expansion.
            correctionAdd("Domain Driven Design", "domain-driven design");
            correctionAdd("Domain-Driven Design", "domain-driven design");
            correctionAdd("ddd", "domain-driven design");

            correctionAdd("hive", "Apache Hive");

            correctionAdd("Geforce", "GeForce");
            correctionAdd("geforce", "GeForce");

            // Of PowerShell...
            correctionAdd("get-unique", "Get-Unique");

            correctionAdd("IPS", "Interpersonal Skills");

            correctionAdd("kerberos", "Kerberos");

            correctionAdd("lightbox", "Lightbox");

            correctionAdd("mern", "MERN");

            correctionAdd("neo4j", "Neo4j");

            correctionAdd("nigerian", "Nigerian");

            correctionAdd("rtos", "RTOS");

            // Of PowerShell...
            correctionAdd("start-transcript", "Start-Transcript");

            // Of PowerShell...
            correctionAdd("stop-transcript", "Stop-Transcript");

            correctionAdd("US", "USA");
            correctionAdd("united states", "USA");
            correctionAdd("United States", "USA");

            correctionAdd("Vic20", "VIC-20");
            correctionAdd("VIC 20", "VIC-20");
            correctionAdd("Vic 20", "VIC-20");

            correctionAdd("vpc", "VPC");
            correctionAdd("virtual private cloud", "VPC");

            correctionAdd("acomplish", "accomplish");

            correctionAdd("alineation", "alienation");

            correctionAdd("acient", "ancient");

            correctionAdd("anyones", "anyone's guess");

            correctionAdd("buttun", "button");
            correctionAdd("botten", "button");
            correctionAdd("bottun", "button");
            correctionAdd("botton", "button");

            correctionAdd("command line", "command-line");
            correctionAdd("CML", "command-line");

            correctionAdd("complilation", "compilation");

            correctionAdd("correctnes", "correctness");

            correctionAdd("datapoint", "data point");

            correctionAdd("dimentional", "dimensional");

            correctionAdd("eloquenctly", "eloquently");

            correctionAdd("gettign", "getting");
            correctionAdd("gettting", "getting");

            correctionAdd("goel", "goal");

            correctionAdd("imposse", "impose");

            correctionAdd("instaling", "installing");

            correctionAdd("unviable", "inviable");

            correctionAdd("liek", "like");

            correctionAdd("Mathematician", "mathematician");
            correctionAdd("mathematicien", "mathematician");

            correctionAdd("migitate", "mitigate");

            correctionAdd("nusiance", "nuisance");

            correctionAdd("ocurr", "occur");

            correctionAdd("oxegen", "oxygen");

            correctionAdd("permanant", "permanent");

            correctionAdd("Physics", "physics");

            correctionAdd("place holder", "placeholder");

            correctionAdd("plagarise", "plagiarise");
            correctionAdd("plagirise", "plagiarise");
            correctionAdd("plagirase", "plagiarise");

            correctionAdd("pronon", "pronoun");

            correctionAdd("propery", "property");
            correctionAdd("proprty", "property");

            correctionAdd("Quantum mechanics", "quantum mechanics");

            correctionAdd("quatation mark", "quotation mark");

            correctionAdd("quaote", "quote");
            correctionAdd("wuote", "quote");

            correctionAdd("reccessive", "recessive");
            correctionAdd("Reccessive", "recessive");

            correctionAdd("resistence", "resistance");

            correctionAdd("re-structure", "restructure");

            correctionAdd("screenfull", "screenful");

            correctionAdd("simpel", "simple");

            correctionAdd("soundcard", "sound card");

            correctionAdd("suggestiong", "suggestion");
            correctionAdd("Suggestiong", "suggestion");

            correctionAdd("friday", "Friday");

            correctionAdd("gnu-make", "GNU Make");
            correctionAdd("GNU make", "GNU Make");
            correctionAdd("Gnu-make", "GNU Make");

            correctionAdd("gitweb", "GitWeb");
            correctionAdd("Gitweb", "GitWeb");

            correctionAdd("Reilly", "O'Reilly");

            // Of PowerShell...
            correctionAdd("set-location", "Set-Location");

            // Of PowerShell...
            correctionAdd("split-path", "Split-Path");

            correctionAdd("TL", "Teachers' Lounge");

            // Of PowerShell...
            correctionAdd("test-path", "Test-Path");

            correctionAdd("TVS", "Transient voltage suppressor");

            correctionAdd("winrm", "WinRM");
            correctionAdd("Windows Remote Management", "WinRM");

            correctionAdd("accorging", "according");

            correctionAdd("coerse", "coerce");

            correctionAdd("coersive", "coercive");

            correctionAdd("contrieved", "contrived");

            correctionAdd("crach", "crash");

            correctionAdd("day to day", "day-to-day");

            correctionAdd("emmigrate", "emigrate");

            correctionAdd("every body", "everybody");

            correctionAdd("hilarius", "hilarious");

            correctionAdd("irrelvant", "irrelevant");

            correctionAdd("leage", "league");
            correctionAdd("Leage", "league");

            correctionAdd("lier", "liar");

            correctionAdd("likleyhood", "likelihood");
            correctionAdd("likelyhood", "likelihood");

            correctionAdd("monetization", "monetisation");
            correctionAdd("monitasation", "monetisation");
            correctionAdd("monitisation", "monetisation");

            correctionAdd("on wards", "onwards");

            correctionAdd("P Channel mosfet", "p-channel MOSFET");
            correctionAdd("PMOS", "p-channel MOSFET");

            correctionAdd("pro-bono", "pro bono");

            correctionAdd("retun", "return");

            correctionAdd("strewam", "stream");

            correctionAdd("synthetize", "synthetise");
            correctionAdd("sinthetize", "synthetise");

            correctionAdd("transparancy", "transparency");

            // Of PowerShell...
            correctionAdd("get-credential", "Get-Credential");

            // Of PowerShell...
            correctionAdd("new-module", "New-Module");

            correctionAdd("ohm's law", "Ohm's law");

            correctionAdd("php composer", "PHP Composer");
            correctionAdd("Composer", "PHP Composer");
            correctionAdd("composer", "PHP Composer");
            correctionAdd("PHP composer", "PHP Composer");

            correctionAdd("pii", "PII");

            correctionAdd("send-mailmessage", "Send-MailMessage");

            correctionAdd("XDebug", "Xdebug");
            correctionAdd("xdebug", "Xdebug");

            correctionAdd("acct", "account");

            correctionAdd("contributer", "contributor");

            correctionAdd("equall", "equal");
            correctionAdd("queal", "equal");

            correctionAdd("IFF", "iff");

            correctionAdd("non sense", "nonsense");

            correctionAdd("thourough", "thorough");

            correctionAdd("unnecesserily", "unnecessarily");
            correctionAdd("uneceesirly", "unnecessarily");

            correctionAdd("usuallally", "usually");

            correctionAdd("britain", "Britain");

            correctionAdd("tory", "Tory");

            correctionAdd("beseiged", "besieged");

            correctionAdd("Colonialism", "colonialism");

            correctionAdd("colnialist", "colonialist");
            correctionAdd("Colonialist", "colonialist");
            correctionAdd("Colnialist", "colonialist");

            correctionAdd("colnies", "colonies");

            correctionAdd("Empire", "empire");

            correctionAdd("emprisoned", "imprisoned");

            correctionAdd("Independence", "independence");
            correctionAdd("Independance", "independence");

            correctionAdd("King", "king");

            correctionAdd("offerred", "offered");

            correctionAdd("prime Minister", "prime minister");
            correctionAdd("Prime Minister", "prime minister");

            correctionAdd("Queen", "queen");

            correctionAdd("Royal", "royal");

            correctionAdd("scinetific", "scientific");
            correctionAdd("Scinetific", "scientific");

            correctionAdd("bower", "Bower");

            correctionAdd("Cherry mx", "Cherry MX");
            correctionAdd("cherry mx", "Cherry MX");

            correctionAdd("gtk", "GTK");

            correctionAdd("Indigogogo", "Indiegogo");
            correctionAdd("indigogogo", "Indiegogo");

            correctionAdd("keil", "Keil");

            correctionAdd("luamacros", "LuaMacros");

            correctionAdd("mars", "Mars");

            correctionAdd("Blackwidow", "Razer BlackWidow");
            correctionAdd("blackwidow", "Razer BlackWidow");

            correctionAdd("tvs", "TVS Electronics");

            correctionAdd("non-mechanical", "nonmechanical");
            correctionAdd("non mechanical", "nonmechanical");

            correctionAdd("probelm", "problem");

            correctionAdd("suport", "support");

            correctionAdd("fltk", "FLTK");

            correctionAdd("Git Flow", "GitFlow");
            correctionAdd("gitflow", "GitFlow");
            correctionAdd("git-flow", "GitFlow");

            correctionAdd("venn diagram", "Venn diagram");

            correctionAdd("difficalt", "difficult");

            correctionAdd("every where", "everywhere");

            correctionAdd("key-press", "key press");
            correctionAdd("Key-Press", "key press");

            correctionAdd("spase", "space");

            correctionAdd("strategi pattern", "strategy pattern");

            correctionAdd("Achiles Heel", "Achilles heel");

            correctionAdd("737 MAX", "Boeing 737 MAX");
            correctionAdd("737 Max", "Boeing 737 MAX");

            correctionAdd("Code Lens", "CodeLens");
            correctionAdd("Codelens", "CodeLens");
            correctionAdd("code lens", "CodeLens");

            correctionAdd("Creative Common by-sa", "Creative Commons BY-SA");

            correctionAdd("Fuhrer", "Führer");

            correctionAdd("Goering", "Göring");

            correctionAdd("Lastpass", "LastPass");
            correctionAdd("lastpass", "LastPass");

            correctionAdd("pow", "POW");

            correctionAdd("scapy", "Scapy");

            correctionAdd("Sothern", "Southern");

            correctionAdd("ToS", "TOS");

            correctionAdd("aggresive", "aggressive");

            correctionAdd("anedocte", "anecdote");

            correctionAdd("formated", "formatted");

            correctionAdd("Mediaeval", "mediaeval");

            correctionAdd("skillful", "skilful");

            correctionAdd("Soviet", "soviet");

            correctionAdd("theoritical", "theoretical");

            correctionAdd("voluntorily", "voluntarily");

            correctionAdd("ethereum", "Ethereum");

            correctionAdd("ammend", "amend");

            correctionAdd("coporation", "corporation");

            correctionAdd("femails", "females");

            correctionAdd("womans", "women");

            correctionAdd("closure", "Closure");

            correctionAdd("fiverr", "Fiverr");

            correctionAdd("get-filehash", "Get-FileHash");

            correctionAdd("Jmeter", "JMeter");

            correctionAdd("mtp", "MTP");

            correctionAdd("new york", "New York");

            correctionAdd("onenote", "OneNote");

            correctionAdd("press key", "Press Key");

            correctionAdd("press keys", "Press Keys");

            correctionAdd("pyspark", "PySpark");

            correctionAdd("star trek", "Star Trek");

            correctionAdd("starbucks", "Starbucks");

            correctionAdd("vco", "VCO");

            correctionAdd("write-progress", "Write-Progress");

            correctionAdd("yolo", "YOLO");

            correctionAdd("abscence", "absence");

            correctionAdd("BitWise", "bitwise");

            correctionAdd("choce", "choice");
            correctionAdd("cohoice", "choice");

            correctionAdd("commiting", "committing");

            correctionAdd("compliment", "complement");

            correctionAdd("Democratic", "democratic");

            correctionAdd("de-reference", "dereference");

            correctionAdd("elgent", "elegant");

            correctionAdd("emphasize", "emphasise");
            correctionAdd("emphisize", "emphasise");

            correctionAdd("employe", "employee");

            correctionAdd("excecute", "execute");

            correctionAdd("extremeties", "extremities");
            correctionAdd("extremitys", "extremities");

            correctionAdd("fare", "fair");

            correctionAdd("illigal", "illegal");

            correctionAdd("idx", "index");

            correctionAdd("in side", "inside");

            correctionAdd("laung", "lounge");

            correctionAdd("monetize", "monetise");
            correctionAdd("monitise", "monetise");

            correctionAdd("Monkey patching", "monkey patching");

            correctionAdd("neo-nazi", "neo-Nazi");

            correctionAdd("ommit", "omit");

            correctionAdd("org", "organisation");
            correctionAdd("organiation", "organisation");

            correctionAdd("portefolio", "portfolio");

            correctionAdd("reccomending", "recommending");

            correctionAdd("religous", "religious");

            correctionAdd("re-scale", "rescale");

            correctionAdd("rewiev", "review");

            correctionAdd("semi colon", "semicolon");
            correctionAdd("semi-colon", "semicolon");

            correctionAdd("snipet", "snippet");

            correctionAdd("user defined", "user-defined");

            correctionAdd("wonderfull", "wonderful");
            correctionAdd("wonder full", "wonderful");

            correctionAdd("exitor", "editor");

            correctionAdd("wil", "will");

            correctionAdd("braintree", "Braintree");

            correctionAdd("mgtow", "MGTOW");

            correctionAdd("mta", "MTA");

            correctionAdd("phpmailer", "PHPMailer");

            correctionAdd("postfix", "Postfix");

            correctionAdd("sendmail", "Sendmail");

            correctionAdd("url encode", "URL encode");
            correctionAdd("URL-encode", "URL encode");
            correctionAdd("url-encode", "URL encode");

            correctionAdd("existant", "existent");

            correctionAdd("inpersonation", "impersonation");

            correctionAdd("Latter", "latter");

            correctionAdd("local host", "localhost");

            correctionAdd("protion", "portion");

            correctionAdd("powers-that-be", "powers that be");
            correctionAdd("Powers-That-Be", "powers that be");

            correctionAdd("seque", "segue");

            correctionAdd("thig", "thing");

            correctionAdd("tooo", "too");

            correctionAdd("distination", "destination");

            correctionAdd("1-on-1", "one-on-one");
            correctionAdd("one on one", "one-on-one");

            correctionAdd("seam", "seem");

            correctionAdd("powerpc", "PowerPC");
            correctionAdd("power-pc", "PowerPC");

            correctionAdd("WOW64", "WoW64");

            correctionAdd("alegedly", "allegedly");
            correctionAdd("alledgedly", "allegedly");
            correctionAdd("allegdedly", "allegedly");

            correctionAdd("contiue", "continue");

            correctionAdd("keepign", "keeping");

            correctionAdd("miminal", "minimal");

            correctionAdd("occassion", "occasion");

            correctionAdd("IFS", "internal field separator");

            correctionAdd("invokation", "invocation");

            correctionAdd("mistep", "misstep");

            correctionAdd("qutation", "quotation");

            correctionAdd("arial", "Arial");

            correctionAdd("Coderush", "CodeRush");

            correctionAdd("geany", "Geany");

            correctionAdd("gerrit", "Gerrit");

            correctionAdd("React mui", "MUI React");
            correctionAdd("React mui material", "MUI React");

            correctionAdd("material ui", "Material-UI");
            correctionAdd("MATERIAL-UI", "Material-UI");
            correctionAdd("React Material-UI", "Material-UI");
            correctionAdd("material-ui", "Material-UI");
            correctionAdd("Material UI", "Material-UI");

            correctionAdd("Technet", "TechNet");

            correctionAdd("test-connection", "Test-Connection");

            correctionAdd("test-netconnection", "Test-NetConnection");

            correctionAdd("Textedit", "TextEdit");
            correctionAdd("textedit", "TextEdit");

            correctionAdd("androidx", "Android X");
            correctionAdd("android x", "Android X");

            correctionAdd("beatiful", "beautiful");

            correctionAdd("gotsha", "gotcha");

            correctionAdd("Telegram", "Telegram Messenger");

            correctionAdd("Hyperterminal", "HyperTerminal");

            correctionAdd("PortMon", "Portmon");

            correctionAdd("OTG", "USB On-The-Go");
            correctionAdd("USB-to-go", "USB On-The-Go");
            correctionAdd("USB OTG", "USB On-The-Go");

            correctionAdd("Anode", "anode");

            correctionAdd("asterix", "asterisk");
            correctionAdd("asteriscs", "asterisk");

            correctionAdd("cetian", "certain");

            correctionAdd("Collector", "collector");

            correctionAdd("Drain", "drain");

            correctionAdd("endcode", "encode");

            correctionAdd("gaue", "gauge");
            correctionAdd("guage", "gauge");

            correctionAdd("happlily", "happily");
            correctionAdd("happilly", "happily");

            correctionAdd("lock-down", "lock down");

            correctionAdd("measureable", "measurable");

            correctionAdd("pickup", "pick up");

            correctionAdd("presicesly", "precisely");

            correctionAdd("Thyristor", "thyristor");

            correctionAdd("rsa", "RSA");

            correctionAdd("DOS2UNIX", "dos2unix");

            correctionAdd("Line Feed", "line feed");

            correctionAdd("photoshoping", "photoshopping");

            correctionAdd("UNIX2DOS", "unix2dos");

            correctionAdd("Cmdr", "Cmder");
            correctionAdd("cmder", "Cmder");

            correctionAdd("webdav", "WebDAV");

            correctionAdd("cleanup", "clean up");

            correctionAdd("surly", "surely");
            correctionAdd("shurely", "surely");

            correctionAdd("therefor", "therefore");

            correctionAdd("alpine", "Alpine Linux");
            correctionAdd("alpine linux", "Alpine Linux");
            correctionAdd("Alpine", "Alpine Linux");

            correctionAdd("elixir", "Elixir");

            correctionAdd("presing", "pressing");

            correctionAdd("ac3", "AC-3");

            correctionAdd("bob's your uncle", "Bob's your uncle");
            correctionAdd("Bob's your Uncle", "Bob's your uncle");

            correctionAdd("dts", "DTS");

            correctionAdd("Soundcloud", "SoundCloud");

            correctionAdd("condidate", "candidate");

            correctionAdd("conection", "connection");

            correctionAdd("containng", "containing");

            correctionAdd("inspite of", "in spite of");
            correctionAdd("inspite", "in spite of");

            correctionAdd("verifed", "verified");

            correctionAdd("IMDB", "IMDb");
            correctionAdd("imdb", "IMDb");

            correctionAdd("clear-cut", "clear cut");

            correctionAdd("correcponding", "corresponding");
            correctionAdd("Correcponding", "corresponding");

            correctionAdd("substract", "subtract");

            correctionAdd("bouncy", "Bouncy Castle");
            correctionAdd("bouncy castle", "Bouncy Castle");
            correctionAdd("BouncyCastle", "Bouncy Castle");
            correctionAdd("Bouncycastle", "Bouncy Castle");

            correctionAdd("perldoc", "Perl Programming Documentation");

            correctionAdd("avarage", "average");

            correctionAdd("pacakge", "package");
            correctionAdd("Pacakge", "package");

            correctionAdd("Strace", "strace");

            correctionAdd("time stamp", "timestamp");

            correctionAdd("1803", "Windows 10, 2018-04-10 Update (version 1803)");

            correctionAdd("cross-validated", "Cross Validated");

            correctionAdd("enterprise resource planning", "ERP");

            correctionAdd("uwp", "UWP");

            correctionAdd("acess", "access");
            correctionAdd("acces", "access");

            correctionAdd("nCurses", "ncurses");

            correctionAdd("Apache commons", "Apache Commons");
            correctionAdd("apache commons", "Apache Commons");

            correctionAdd("guava", "Google Guava");
            correctionAdd("Guava", "Google Guava");

            correctionAdd("Java6", "Java&nbsp;6");
            correctionAdd("java 6", "Java&nbsp;6");

            correctionAdd("Java7", "Java&nbsp;7");
            correctionAdd("java 7", "Java&nbsp;7");

            correctionAdd("java 8", "Java&nbsp;8");
            correctionAdd("Java8", "Java&nbsp;8");

            correctionAdd("mutt", "Mutt");

            correctionAdd("pico", "Pico");

            correctionAdd("pine", "Pine");

            correctionAdd("screen", "Screen");

            correctionAdd("wtf", "WTF");

            correctionAdd("xtail", "Xtail");

            correctionAdd("CFDISK", "cfdisk");

            correctionAdd("CODEC", "codec");

            correctionAdd("FDISK", "fdisk");

            correctionAdd("IFCONFIG", "ifconfig");

            correctionAdd("IOW", "in other words");

            correctionAdd("IPROUTE2", "iproute2");

            correctionAdd("IWCONFIG", "iwconfig");

            correctionAdd("LOCATE", "locate");

            correctionAdd("NANO", "nano");

            correctionAdd("POPD", "popd");

            correctionAdd("PUSHD", "pushd");

            correctionAdd("ROUTE", "route");

            correctionAdd("TAIL", "tail");

            correctionAdd("TMUX", "tmux");

            correctionAdd("verbage", "verbiage");

            correctionAdd("week day", "weekday");

            correctionAdd("ppm", "PPM");
            correctionAdd("Perl Package Manager", "PPM");

            correctionAdd("perlmonks", "PerlMonks");

            correctionAdd("antyhing", "anything");

            correctionAdd("bleeding edge", "bleeding-edge");

            correctionAdd("developming", "developing");

            correctionAdd("moducle", "module");

            correctionAdd("babel", "Babel");

            correctionAdd("bluehost", "Bluehost");

            correctionAdd("flink", "Flink");

            correctionAdd("next js", "Next.js");
            correctionAdd("Next JS", "Next.js");
            correctionAdd("next.js", "Next.js");

            correctionAdd("optix", "OptiX");

            correctionAdd("Peachpie", "PeachPie");

            correctionAdd("sudoku", "Sudoku");

            correctionAdd("absolutly", "absolutely");

            correctionAdd("artifact", "artefact");

            correctionAdd("communite", "community");

            correctionAdd("easyest", "easiest");

            correctionAdd("exept", "except");
            correctionAdd("cept", "except");

            correctionAdd("extrenal", "external");

            correctionAdd("in sake of", "for the sake of");

            correctionAdd("integrade", "integrate");

            correctionAdd("minmalistic", "minimalistic");

            correctionAdd("planing", "planning");

            correctionAdd("re-run", "rerun");

            correctionAdd("substaction", "subtraction");

            correctionAdd("wen", "when");

            correctionAdd("costom", "custom");
            correctionAdd("costum", "custom");
            correctionAdd("custum", "custom");

            correctionAdd("folter", "folder");

            correctionAdd("highligting", "highlighting");

            correctionAdd("misterious", "mysterious");

            correctionAdd("c++03", "C++03");

            correctionAdd("norwegian", "Norwegian");

            correctionAdd("activally", "actively");

            correctionAdd("color", "colour");

            correctionAdd("creat", "create");

            correctionAdd("down voting", "downvoting");

            correctionAdd("forground", "foreground");

            correctionAdd("longwinded", "long-winded");

            correctionAdd("long-time", "longtime");

            correctionAdd("ment", "meant");

            correctionAdd("out-of-date", "out of date");

            correctionAdd("run of the mill", "run-of-the-mill");

            correctionAdd("sub-process", "subprocess");

            correctionAdd("tranformation", "transformation");

            correctionAdd("welcomness", "welcomeness");

            correctionAdd("c++98", "C++98");
            correctionAdd("C++99", "C++98");
            correctionAdd("c++99", "C++98");

            correctionAdd("guarantied", "guaranteed");

            correctionAdd("heres", "here's");
            correctionAdd("Heres", "here's");

            correctionAdd("freelancer.com", "Freelancer.com");

            correctionAdd("GlassDoor", "Glassdoor");

            correctionAdd("Indeed.com", "Indeed");

            correctionAdd("readme", "README");

            correctionAdd("scratch", "Scratch");

            correctionAdd("toptal", "Toptal");

            correctionAdd("absobr", "absorb");

            correctionAdd("acceptible", "acceptable");

            correctionAdd("anouncement", "announcement");

            correctionAdd("genge", "change");

            correctionAdd("chanel", "channel");

            correctionAdd("counter-act", "counteract");

            correctionAdd("grate", "great");

            correctionAdd("Open source", "open source");
            correctionAdd("Open Source", "open source");

            correctionAdd("psedo", "pseudo");

            correctionAdd("shcool", "school");

            correctionAdd("where in", "wherein");

            correctionAdd("mindstorm", "Lego Mindstorms");
            correctionAdd("Lego Mindstorm", "Lego Mindstorms");
            correctionAdd("Mindstorm", "Lego Mindstorms");

            correctionAdd("htacces", ".htaccess");
            correctionAdd("htaccess", ".htaccess");
            correctionAdd(".htaccess file", ".htaccess");
            correctionAdd("htaccess file", ".htaccess");
            correctionAdd(".htacces", ".htaccess");

            correctionAdd("1and1", "1&1 IONOS");
            correctionAdd("1&1", "1&1 IONOS");

            correctionAdd("avada", "Avada");

            correctionAdd("c11", "C11");

            correctionAdd("Cern", "CERN");

            correctionAdd("divi", "Divi");

            correctionAdd("easyphp", "EasyPHP");

            correctionAdd("git2", "Git&nbsp;2.0");
            correctionAdd("Git2", "Git&nbsp;2.0");

            correctionAdd("Higgs Boson", "Higgs boson");

            correctionAdd("Mev", "MeV");

            correctionAdd("tui", "TUI");

            correctionAdd("Axion", "axion");

            correctionAdd("cwd", "current working directory");
            correctionAdd("CWD", "current working directory");

            correctionAdd("Dark Energy", "dark energy");

            correctionAdd("Dark Matter", "dark matter");

            correctionAdd("dilema", "dilemma");

            correctionAdd("espaced", "escape");

            correctionAdd("GLIBC", "glibc");

            correctionAdd("Helium", "helium");

            correctionAdd("Neutrino", "neutrino");

            correctionAdd("POV", "point of view");
            correctionAdd("PoV", "point of view");

            correctionAdd("sensative", "sensitive");

            correctionAdd("tounge", "tongue");

            correctionAdd("modrewrite", "mod_rewrite");

            correctionAdd("rewrited", "rewrote");

            correctionAdd("re-write", "rewrite");

            correctionAdd("Mod Rewrite", "URL rewriting");
            correctionAdd("url rewriting", "URL rewriting");
            //correctionAdd("rewrite", "URL rewriting"); //Likely conflict with other stuff... Yep, on 2019-12-01, with a correction for "re-write"
            correctionAdd("URL rewrite", "URL rewriting");
            correctionAdd("URL rewrites", "URL rewriting"); //Plural thing...
            correctionAdd("URL Rewriting", "URL rewriting");
            correctionAdd("URI Rewriting", "URL rewriting");
            correctionAdd("URI rewriting", "URL rewriting");

            correctionAdd("B5", "Babylon&nbsp;5");

            correctionAdd("iommu", "IOMMU");

            correctionAdd("logitech", "Logitech");

            correctionAdd("poertop", "PowerTOP");
            correctionAdd("powertop", "PowerTOP");
            correctionAdd("Powertop", "PowerTOP");

            correctionAdd("pywin32", "PyWin32");
            correctionAdd("pyWin32", "PyWin32");

            correctionAdd("SQL Search", "SQL&nbsp;Search");
            correctionAdd("SQLSearch", "SQL&nbsp;Search");

            correctionAdd("svm", "SVM");

            correctionAdd("add in", "add-in");

            correctionAdd("Boost converter", "boost converter");

            correctionAdd("help-vampire", "help vampire");
            correctionAdd("Help Vampire", "help vampire");

            correctionAdd("Udev", "udev");

            correctionAdd("visonary", "visionary");

            correctionAdd("bashrc", ".bashrc file");

            correctionAdd("ps1", "PS1");

            correctionAdd("SMBUS", "SMBus");

            correctionAdd("hemissphere", "hemisphere");

            correctionAdd("PSQL", "psql");

            correctionAdd("resignment", "resignation");

            correctionAdd("specificially", "specifically");
            correctionAdd("Specificially", "specifically");

            correctionAdd("Summar", "summer");
            correctionAdd("summar", "summer");

            correctionAdd("feynman diagram", "Feynman diagram");

            correctionAdd("Repetitive strain injury", "RSI");

            correctionAdd("ST Link", "ST-LINK");
            correctionAdd("st Link", "ST-LINK");
            correctionAdd("St Link", "ST-LINK");

            correctionAdd("swagger", "Swagger");

            correctionAdd("auto-generated", "autogenerated");
            correctionAdd("auto generated", "autogenerated");

            correctionAdd("code gen", "code generation");
            correctionAdd("codegen", "code generation");

            correctionAdd("multi-core", "multicore");

            correctionAdd("precission", "precision");

            correctionAdd("re-add", "readd");

            correctionAdd("shutdown", "shut down");

            correctionAdd("perlbrew", "Perlbrew");
            correctionAdd("PerlBrew", "Perlbrew");
            correctionAdd("Perl Brew", "Perlbrew");

            correctionAdd("Prestashop", "PrestaShop");

            correctionAdd("typo3", "TYPO3");

            correctionAdd("Extension Method", "extension method");

            correctionAdd("fav", "favourite");

            correctionAdd("indend", "intend");

            correctionAdd("Reflection", "reflection");





            //HACK: AASDASD
            //TODO: OIDSOPAUSD


            //Does not work due to a bug (when terms end in ".")
            //correctionAdd("SMART", "S.M.A.R.T.");
            //  https://en.wikipedia.org/wiki/S.M.A.R.T.


            //========================================================
            //AAAAAAAAAAAAAAAAAAAA   A marker...

            //Candidates:
            //
            //  4. http://en.wikipedia.org/wiki/IPv6
            //     IPv6
            //
            //  5. Microsoft Chart Controls (correct name? What is the Wikipedia page?)


            //string msg5 = "Starting to add URL mappings...";
            //System.Windows.Forms.MessageBox.Show(msg5);

            URL_Add("JavaScript", "https://en.wikipedia.org/wiki/JavaScript"); // Old: http://en.wikipedia.org/wiki/JavaScript
            URL_Add("jQuery", "https://en.wikipedia.org/wiki/JQuery"); // Old: http://en.wikipedia.org/wiki/JQuery
            URL_Add("Ajax", "http://en.wikipedia.org/wiki/Ajax_%28programming%29");

            URL_Add("MSN", "http://en.wikipedia.org/wiki/MSN");
            URL_Add("MFC", "http://en.wikipedia.org/wiki/Microsoft_Foundation_Class_Library");
            URL_Add("Windows Forms", "http://en.wikipedia.org/wiki/Windows_Forms");

            URL_Add("TIFF", "http://en.wikipedia.org/wiki/Tagged_Image_File_Format");

            URL_Add("Android", "http://en.wikipedia.org/wiki/Android_%28operating_system%29");

            URL_Add("LAMP", "http://en.wikipedia.org/wiki/LAMP_%28software_bundle%29");

            URL_Add("MySQL", "http://en.wikipedia.org/wiki/MySQL");

            URL_Add("iPhone", "http://en.wikipedia.org/wiki/IPhone");

            URL_Add(".NET", "http://en.wikipedia.org/wiki/.NET_Framework");

            URL_Add("Scala", "http://en.wikipedia.org/wiki/Scala_%28programming_language%29");

            URL_Add("Objective-C", "http://en.wikipedia.org/wiki/Objective-C");

            URL_Add("FxCop", "http://en.wikipedia.org/wiki/FxCop");

            URL_Add("ASP.NET MVC", "https://en.wikipedia.org/wiki/ASP.NET_MVC"); // Old: http://en.wikipedia.org/wiki/ASP.NET_MVC_Framework

            URL_Add("ASP.NET", "http://en.wikipedia.org/wiki/ASP.NET");

            URL_Add("Mac&nbsp;OS&nbsp;X", "http://en.wikipedia.org/wiki/Mac_OS_X");

            URL_Add("Flex", "http://en.wikipedia.org/wiki/Adobe_Flex");

            URL_Add("ActionScript", "http://en.wikipedia.org/wiki/ActionScript");

            URL_Add("ADO.NET", "http://en.wikipedia.org/wiki/ADO.NET");

            URL_Add("stored procedure", "http://en.wikipedia.org/wiki/Stored_procedure");

            URL_Add("SQL Server 2005", "https://en.wikipedia.org/wiki/History_of_Microsoft_SQL_Server#SQL_Server_2005"); // Old: http://en.wikipedia.org/wiki/Microsoft_SQL_Server#SQL_Server_2005

            URL_Add("SQL&nbsp;Server", "http://en.wikipedia.org/wiki/Microsoft_SQL_Server");

            //Relational database management system.
            URL_Add("database", "http://en.wikipedia.org/wiki/Relational_database_management_system");

            URL_Add("RIA", "https://en.wikipedia.org/wiki/Rich_web_application"); // Old: http://en.wikipedia.org/wiki/Rich_Internet_application

            URL_Add("SubSonic", "http://en.wikipedia.org/wiki/Subsonic_%28software%29");

            URL_Add("ADO.NET Entity Framework", "http://en.wikipedia.org/wiki/ADO.NET_Entity_Framework");

            URL_Add("WPF", "http://en.wikipedia.org/wiki/Windows_Presentation_Foundation");

            URL_Add("MSDN", "http://en.wikipedia.org/wiki/Microsoft_Developer_Network");

            URL_Add("SQL Server Express Edition", "http://en.wikipedia.org/wiki/SQL_Server_Express");

            URL_Add("PowerBuilder", "http://en.wikipedia.org/wiki/PowerBuilder");

            URL_Add("Swing", "http://en.wikipedia.org/wiki/Swing_%28Java%29");

            URL_Add("Qt", "https://en.wikipedia.org/wiki/Qt_%28software%29"); //The URL changed. Old: <http://en.wikipedia.org/wiki/Qt_%28toolkit%29>.

            URL_Add("IntelliSense", "https://en.wikipedia.org/wiki/Intelligent_code_completion#IntelliSense"); // Previous: <https://en.wikipedia.org/wiki/Intelli-sense>

            URL_Add("Internet", "http://en.wikipedia.org/wiki/Internet");

            URL_Add("YouTube", "http://en.wikipedia.org/wiki/YouTube");

            URL_Add("UTF-8", "http://en.wikipedia.org/wiki/UTF-8");

            URL_Add("UTF-16", "https://en.wikipedia.org/wiki/UTF-16"); //Old: <http://en.wikipedia.org/wiki/UTF-16/UCS-2>.

            URL_Add("IntelliJ IDEA", "http://en.wikipedia.org/wiki/IntelliJ_IDEA");

            URL_Add("CouchDB", "http://en.wikipedia.org/wiki/CouchDB");

            URL_Add(".NET Reflector", "http://en.wikipedia.org/wiki/.NET_Reflector");

            URL_Add("NHibernate", "http://en.wikipedia.org/wiki/NHibernate");

            URL_Add("Hibernate", "http://en.wikipedia.org/wiki/Hibernate_%28Java%29");

            URL_Add("POCO", "http://en.wikipedia.org/wiki/Plain_Old_CLR_Object");

            URL_Add("Struts", "http://en.wikipedia.org/wiki/Apache_Struts");

            URL_Add("Boost", "http://en.wikipedia.org/wiki/Boost_%28C%2B%2B_libraries%29"); //Old: http://en.wikipedia.org/wiki/Boost_C%2B%2B_Libraries

            URL_Add("continuous integration", "http://en.wikipedia.org/wiki/Continuous_integration");

            URL_Add("ImageMagick", "http://en.wikipedia.org/wiki/ImageMagick");

            URL_Add("DPI", "http://en.wikipedia.org/wiki/Dots_per_inch");

            URL_Add("NumPy", "http://en.wikipedia.org/wiki/NumPy");

            //Fixed 2010-03-14...
            //URL_Add("Bash", "http://en.wikipedia.org/wiki/Bash");
            URL_Add("Bash", "http://en.wikipedia.org/wiki/Bash_%28Unix_shell%29");

            URL_Add("WiX", "http://en.wikipedia.org/wiki/WiX");

            URL_Add("Perl", "http://en.wikipedia.org/wiki/Perl");

            URL_Add("LINQ", "http://en.wikipedia.org/wiki/Language_Integrated_Query");

            //Changed at Wikipedia
            //URL_Add("LINQ&nbsp;to&nbsp;SQL", "http://en.wikipedia.org/wiki/Language_Integrated_Query#LINQ_to_SQL");
            URL_Add("LINQ&nbsp;to&nbsp;SQL", "http://en.wikipedia.org/wiki/Language_Integrated_Query#LINQ_to_SQL_.28formerly_called_DLINQ.29");

            //Changed at Wikipedia
            //URL_Add("LINQ&nbsp;to&nbsp;XML", "http://en.wikipedia.org/wiki/Language_Integrated_Query#LINQ_to_XML");
            URL_Add("LINQ&nbsp;to&nbsp;XML", "http://en.wikipedia.org/wiki/Language_Integrated_Query#LINQ_to_XML_.28formerly_called_XLINQ.29");

            URL_Add("CakePHP", "http://en.wikipedia.org/wiki/CakePHP");

            URL_Add("PowerShell", "http://en.wikipedia.org/wiki/Windows_PowerShell");

            URL_Add("SQLite", "http://en.wikipedia.org/wiki/SQLite");

            URL_Add("Expression Blend", "http://en.wikipedia.org/wiki/Microsoft_Expression_Blend");

            URL_Add("Vim", "http://en.wikipedia.org/wiki/Vim_%28text_editor%29");
            URL_Add("gVim", "http://en.wikipedia.org/wiki/Vim_%28text_editor%29#Interface");

            URL_Add("Cygwin", "http://en.wikipedia.org/wiki/Cygwin");

            URL_Add("SourceForge", "http://en.wikipedia.org/wiki/SourceForge");

            URL_Add("OpenOffice", "http://en.wikipedia.org/wiki/OpenOffice.org");

            URL_Add("VBScript", "http://en.wikipedia.org/wiki/VBScript");

            URL_Add("MATLAB", "http://en.wikipedia.org/wiki/MATLAB");

            URL_Add("PostgreSQL", "http://en.wikipedia.org/wiki/PostgreSQL");

            URL_Add("segmentation fault", "http://en.wikipedia.org/wiki/Segmentation_fault");

            URL_Add("JSON", "http://en.wikipedia.org/wiki/JSON");

            //Fixed 2012-07-17. Was the insect, "http://en.wikipedia.org/wiki/Firebug"!
            URL_Add("Firebug", "http://en.wikipedia.org/wiki/Firebug_%28software%29");

            URL_Add("Drupal", "http://en.wikipedia.org/wiki/Drupal");

            URL_Add("PyQt", "http://en.wikipedia.org/wiki/PyQt");

            URL_Add("Gmail", "http://en.wikipedia.org/wiki/Gmail");

            URL_Add("Ruby", "http://en.wikipedia.org/wiki/Ruby_%28programming_language%29");

            URL_Add("ReSharper", "http://en.wikipedia.org/wiki/ReSharper");

            URL_Add("test-driven development", "http://en.wikipedia.org/wiki/Test-driven_development");

            URL_Add("XAMPP", "http://en.wikipedia.org/wiki/XAMPP");

            URL_Add("YUI", "http://en.wikipedia.org/wiki/Yahoo!_UI_Library");

            URL_Add("sed", "http://en.wikipedia.org/wiki/Sed");

            URL_Add("AWK", "http://en.wikipedia.org/wiki/AWK");

            URL_Add("Tcl", "http://en.wikipedia.org/wiki/Tcl");

            URL_Add("Lua", "http://en.wikipedia.org/wiki/Lua_%28programming_language%29");

            URL_Add("Ant", "http://en.wikipedia.org/wiki/Apache_Ant");

            URL_Add("VB.NET", "http://en.wikipedia.org/wiki/Visual_Basic_.NET");

            URL_Add("Wi-Fi", "http://en.wikipedia.org/wiki/Wi-Fi");

            URL_Add("I/O", "http://en.wikipedia.org/wiki/Input/output");

            URL_Add("VLC media player", "http://en.wikipedia.org/wiki/VLC_media_player");

            URL_Add("Emacs", "http://en.wikipedia.org/wiki/Emacs");

            URL_Add("Python", "http://en.wikipedia.org/wiki/Python_%28programming_language%29");

            URL_Add("Maven", "http://en.wikipedia.org/wiki/Apache_Maven");

            URL_Add("Greasemonkey", "http://en.wikipedia.org/wiki/Greasemonkey");

            URL_Add("PuTTY", "http://en.wikipedia.org/wiki/PuTTY");

            URL_Add("TA", "http://en.wikipedia.org/wiki/Teaching_assistant");

            URL_Add("CodeIgniter", "http://en.wikipedia.org/wiki/Codeigniter#CodeIgniter");

            URL_Add("GlassFish", "http://en.wikipedia.org/wiki/GlassFish");

            URL_Add("7-Zip", "http://en.wikipedia.org/wiki/7-Zip");

            URL_Add("synchronise", "http://en.wikipedia.org/wiki/Synchronization");

            URL_Add("sIFR", "http://en.wikipedia.org/wiki/Scalable_Inman_Flash_Replacement");

            URL_Add("Boot Camp", "http://en.wikipedia.org/wiki/Boot_Camp_%28software%29");

            URL_Add("IronPython", "http://en.wikipedia.org/wiki/IronPython");

            URL_Add("Groovy", "http://en.wikipedia.org/wiki/Groovy_%28programming_language%29");

            URL_Add("NetBeans", "http://en.wikipedia.org/wiki/NetBeans");

            URL_Add("Solr", "http://en.wikipedia.org/wiki/Apache_Solr");

            URL_Add("NoScript", "http://en.wikipedia.org/wiki/NoScript");

            URL_Add("Adblock", "http://en.wikipedia.org/wiki/Adblock_Plus");

            URL_Add("Matplotlib", "http://en.wikipedia.org/wiki/Matplotlib");

            URL_Add("IDLE", "http://en.wikipedia.org/wiki/IDLE_%28Python%29");

            URL_Add("SharePoint", "https://en.wikipedia.org/wiki/SharePoint");

            URL_Add("PayPal", "http://en.wikipedia.org/wiki/PayPal");

            URL_Add("Mono", "http://en.wikipedia.org/wiki/Mono_%28software%29");

            URL_Add("Pig Latin", "http://en.wikipedia.org/wiki/Pig_Latin");

            URL_Add("VMware", "http://en.wikipedia.org/wiki/VMware");

            URL_Add("SciPy", "http://en.wikipedia.org/wiki/SciPy");

            URL_Add("phpMyAdmin", "http://en.wikipedia.org/wiki/PhpMyAdmin");

            URL_Add("LDAP", "http://en.wikipedia.org/wiki/LDAP");

            URL_Add("SciTE", "http://en.wikipedia.org/wiki/SciTE");

            URL_Add("ORM", "http://en.wikipedia.org/wiki/Object-relational_mapping");

            URL_Add("Ext&nbsp;JS", "http://en.wikipedia.org/wiki/Ext_JS");

            URL_Add("Google&nbsp;Web&nbsp;Toolkit", "http://en.wikipedia.org/wiki/Google_Web_Toolkit");

            URL_Add("MediaWiki", "http://en.wikipedia.org/wiki/MediaWiki");

            URL_Add("Excel", "http://en.wikipedia.org/wiki/Microsoft_Excel");

            URL_Add("WYSIWYG", "http://en.wikipedia.org/wiki/WYSIWYG");

            URL_Add("MoinMoin", "http://en.wikipedia.org/wiki/MoinMoin");

            URL_Add("Joomla", "http://en.wikipedia.org/wiki/Joomla");

            URL_Add("YAML", "http://en.wikipedia.org/wiki/YAML");

            URL_Add("XAML", "http://en.wikipedia.org/wiki/Extensible_Application_Markup_Language");

            URL_Add("Google App Engine", "http://en.wikipedia.org/wiki/Google_App_Engine");

            URL_Add("Inkscape", "http://en.wikipedia.org/wiki/Inkscape");

            URL_Add("NDepend", "http://en.wikipedia.org/wiki/NDepend");

            URL_Add("XML-RPC", "http://en.wikipedia.org/wiki/XML-RPC");

            URL_Add("OpenDNS", "http://en.wikipedia.org/wiki/OpenDNS");

            URL_Add("ColdFusion", "http://en.wikipedia.org/wiki/Adobe_ColdFusion"); //Old: http://en.wikipedia.org/wiki/ColdFusion

            URL_Add("MooTools", "http://en.wikipedia.org/wiki/MooTools");

            URL_Add("Google Apps", "http://en.wikipedia.org/wiki/Google_Apps");

            URL_Add("WordPress", "http://en.wikipedia.org/wiki/WordPress");

            URL_Add("SSL", "http://en.wikipedia.org/wiki/Transport_Layer_Security");

            URL_Add("JavaFX", "http://en.wikipedia.org/wiki/JavaFX");

            URL_Add("WebKit", "http://en.wikipedia.org/wiki/WebKit");

            URL_Add("Arduino", "http://en.wikipedia.org/wiki/Arduino");

            URL_Add("Prototype", "http://en.wikipedia.org/wiki/Prototype_JavaScript_Framework");

            URL_Add("Silverlight", "http://en.wikipedia.org/wiki/Microsoft_Silverlight");

            URL_Add("Crystal Reports", "http://en.wikipedia.org/wiki/Crystal_Reports");

            URL_Add("OpenVPN", "http://en.wikipedia.org/wiki/OpenVPN");

            URL_Add("T-SQL", "http://en.wikipedia.org/wiki/Transact-SQL");

            URL_Add("MongoDB", "http://en.wikipedia.org/wiki/MongoDB");

            URL_Add("HBase", "http://en.wikipedia.org/wiki/HBase");

            URL_Add("Lucene", "http://en.wikipedia.org/wiki/Lucene");

            URL_Add("Subversion", "http://en.wikipedia.org/wiki/Apache_Subversion");

            URL_Add("The Code Project", "https://en.wikipedia.org/wiki/Code_Project"); //Was https://en.wikipedia.org/wiki/The_Code_Project

            URL_Add("phpBB", "http://en.wikipedia.org/wiki/PhpBB");

            URL_Add("CSV", "http://en.wikipedia.org/wiki/Comma-separated_values");

            URL_Add("foreign key", "http://en.wikipedia.org/wiki/Foreign_key");

            URL_Add("CLR", "http://en.wikipedia.org/wiki/Common_Language_Runtime");

            URL_Add("Magento", "http://en.wikipedia.org/wiki/Magento");

            URL_Add("Zend Framework", "http://en.wikipedia.org/wiki/Zend_Framework");

            URL_Add("PL/SQL", "http://en.wikipedia.org/wiki/PL/SQL");

            URL_Add("SimpleXML", "http://en.wikipedia.org/wiki/SimpleXML");

            URL_Add("Redis", "http://en.wikipedia.org/wiki/Redis_%28data_store%29");

            URL_Add("Tokyo Cabinet", "http://fallabs.com/tokyocabinet/");

            URL_Add("Memcached", "http://en.wikipedia.org/wiki/Memcached");

            URL_Add("FastCGI", "http://en.wikipedia.org/wiki/FastCGI");

            URL_Add("Project Euler", "http://en.wikipedia.org/wiki/Project_Euler");

            URL_Add("Firesheep", "http://en.wikipedia.org/wiki/Firesheep");

            URL_Add("Processing", "http://en.wikipedia.org/wiki/Processing_%28programming_language%29");

            URL_Add("Flash", "http://en.wikipedia.org/wiki/Adobe_Flash");

            URL_Add("jQuery UI", "http://en.wikipedia.org/wiki/JQuery_UI");

            URL_Add("cURL", "http://en.wikipedia.org/wiki/CURL");

            URL_Add("PHP-Nuke", "http://en.wikipedia.org/wiki/PHP-Nuke");

            URL_Add("BitTorrent", "http://en.wikipedia.org/wiki/BitTorrent");

            URL_Add("Notepad", "http://en.wikipedia.org/wiki/Notepad_%28software%29");

            URL_Add("WordPad", "http://en.wikipedia.org/wiki/WordPad");

            URL_Add("CAPTCHA", "http://en.wikipedia.org/wiki/CAPTCHA");

            URL_Add("OpenGL", "http://en.wikipedia.org/wiki/OpenGL");

            URL_Add("SymPy", "http://en.wikipedia.org/wiki/SymPy");

            URL_Add("PIL", "http://en.wikipedia.org/wiki/Python_Imaging_Library");

            URL_Add("MacBook Pro", "http://en.wikipedia.org/wiki/MacBook_Pro");

            URL_Add("Java ME", "http://en.wikipedia.org/wiki/Java_Platform,_Micro_Edition");

            URL_Add("Bluetooth", "http://en.wikipedia.org/wiki/Bluetooth");

            URL_Add("Xbox", "http://en.wikipedia.org/wiki/Xbox");

            URL_Add("PS3", "http://en.wikipedia.org/wiki/PlayStation_3");

            URL_Add("Tcl/Tk", "http://en.wikipedia.org/wiki/Tcl");

            URL_Add("PHP", "http://en.wikipedia.org/wiki/PHP");

            URL_Add("Symfony", "http://en.wikipedia.org/wiki/Symfony");

            URL_Add("Tomcat", "http://en.wikipedia.org/wiki/Apache_Tomcat");

            URL_Add("OpenID", "http://en.wikipedia.org/wiki/OpenID");

            URL_Add("MyOpenID", "http://myopenid.com/");

            URL_Add("OAuth", "http://en.wikipedia.org/wiki/OAuth");

            URL_Add("CSS", "http://en.wikipedia.org/wiki/Cascading_Style_Sheets");

            URL_Add("GitHub", "http://en.wikipedia.org/wiki/GitHub");

            URL_Add("TinyMCE", "http://en.wikipedia.org/wiki/TinyMCE");

            URL_Add("BlackBerry", "http://en.wikipedia.org/wiki/BlackBerry");

            URL_Add("GCC", "http://en.wikipedia.org/wiki/GNU_Compiler_Collection");

            URL_Add("GDB", "http://en.wikipedia.org/wiki/GNU_Debugger");

            URL_Add("Valgrind", "http://en.wikipedia.org/wiki/Valgrind");

            URL_Add("Git", "https://en.wikipedia.org/wiki/Git"); // Old: http://en.wikipedia.org/wiki/Git_%28software%29

            URL_Add("TextMate", "http://en.wikipedia.org/wiki/TextMate");

            URL_Add("HTML", "http://en.wikipedia.org/wiki/HTML");

            URL_Add("Firefox", "http://en.wikipedia.org/wiki/Mozilla_Firefox");

            URL_Add("Google Chrome", "http://en.wikipedia.org/wiki/Google_Chrome");

            URL_Add("Scheme", "http://en.wikipedia.org/wiki/Scheme_%28programming_language%29");

            URL_Add("PowerPoint", "http://en.wikipedia.org/wiki/Microsoft_PowerPoint");

            URL_Add("C#", "http://en.wikipedia.org/wiki/C_Sharp_%28programming_language%29");

            URL_Add("SQL Server 2008", "https://en.wikipedia.org/wiki/History_of_Microsoft_SQL_Server#SQL_Server_2008"); // Old: http://en.wikipedia.org/wiki/Microsoft_SQL_Server#SQL_Server_2008

            URL_Add("SQL", "http://en.wikipedia.org/wiki/SQL");

            URL_Add("HTTP", "http://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol");

            URL_Add("ASCII", "http://en.wikipedia.org/wiki/ASCII");

            URL_Add("Fluent NHibernate", "http://fluentnhibernate.org/");

            URL_Add("asynchronously", "http://en.wiktionary.org/wiki/asynchronously");

            URL_Add("Haskell", "http://en.wikipedia.org/wiki/Haskell_%28programming_language%29");

            URL_Add("PlayStation 3", "http://en.wikipedia.org/wiki/PlayStation_3");

            URL_Add("NUnit", "http://en.wikipedia.org/wiki/NUnit");

            URL_Add("Hudson", "http://en.wikipedia.org/wiki/Hudson_%28software%29");

            URL_Add("Grails", "http://en.wikipedia.org/wiki/Grails_%28framework%29");

            URL_Add("Moq", "http://en.wikipedia.org/wiki/Moq");

            URL_Add("MonoDevelop", "http://en.wikipedia.org/wiki/MonoDevelop");

            URL_Add("MSBuild", "http://en.wikipedia.org/wiki/MSBuild");

            URL_Add("Cocoa", "http://en.wikipedia.org/wiki/Cocoa_%28API%29");

            URL_Add("Clojure", "http://en.wikipedia.org/wiki/Clojure");

            URL_Add("Aquamacs", "http://en.wikipedia.org/wiki/Aquamacs");

            URL_Add("SLIME", "http://en.wikipedia.org/wiki/SLIME");

            URL_Add("Django", "http://en.wikipedia.org/wiki/Django_%28web_framework%29");

            URL_Add("Xcode", "http://en.wikipedia.org/wiki/Xcode");

            URL_Add("iTunes", "http://en.wikipedia.org/wiki/ITunes");

            URL_Add("WCF", "http://en.wikipedia.org/wiki/Windows_Communication_Foundation");

            URL_Add("Ruby on Rails", "http://en.wikipedia.org/wiki/Ruby_on_Rails");

            URL_Add("Java EE", "http://en.wikipedia.org/wiki/Java_Platform,_Enterprise_Edition");

            URL_Add("Unicode", "http://en.wikipedia.org/wiki/Unicode");

            URL_Add("Java", "http://en.wikipedia.org/wiki/Java_%28programming_language%29");

            //Not used anymore.
            //URL_Add("Spring", "http://en.wikipedia.org/wiki/Spring_Framework");

            URL_Add("JBoss", "http://en.wikipedia.org/wiki/JBoss");

            URL_Add("JSF", "http://en.wikipedia.org/wiki/JavaServer_Faces");

            URL_Add("JSP", "http://en.wikipedia.org/wiki/JavaServer_Pages");

            URL_Add("Paint.NET", "http://en.wikipedia.org/wiki/Paint.NET");

            URL_Add("regular expression", "http://en.wikipedia.org/wiki/Regular_expression");

            URL_Add("XPath", "http://en.wikipedia.org/wiki/XPath");

            URL_Add("REPL", "http://en.wikipedia.org/wiki/Read-eval-print_loop");

            URL_Add("VTune", "http://en.wikipedia.org/wiki/VTune");

            URL_Add("OProfile", "http://oprofile.sourceforge.net/about/");

            URL_Add("PSPad", "http://en.wikipedia.org/wiki/PSPad");

            URL_Add("TextPad", "http://en.wikipedia.org/wiki/TextPad");

            URL_Add("E Text Editor", "http://en.wikipedia.org/wiki/E_Text_Editor");

            URL_Add("Wine", "http://en.wikipedia.org/wiki/Wine_%28software%29");

            URL_Add("Komodo Edit", "https://en.wikipedia.org/wiki/Komodo_Edit"); // Old: http://en.wikipedia.org/wiki/ActiveState_Komodo

            URL_Add("wxWidgets", "http://en.wikipedia.org/wiki/WxWidgets");

            URL_Add("P/Invoke", "http://en.wikipedia.org/wiki/Platform_Invocation_Services");

            URL_Add("JNI", "http://en.wikipedia.org/wiki/Java_Native_Interface");

            URL_Add("ILAsm", "http://en.wikipedia.org/wiki/ILAsm");

            URL_Add("CruiseControl", "http://en.wikipedia.org/wiki/CruiseControl");

            //Map to the same because there is no separate CruiseControl.NET article.
            URL_Add("CruiseControl.NET", "http://en.wikipedia.org/wiki/CruiseControl");

            URL_Add("JDBC", "http://en.wikipedia.org/wiki/Java_Database_Connectivity");

            URL_Add("WAR", "http://en.wikipedia.org/wiki/WAR_%28Sun_file_format%29");

            URL_Add("H2", "http://en.wikipedia.org/wiki/H2_%28DBMS%29");

            URL_Add("YUM", "http://en.wikipedia.org/wiki/Yellowdog_Updater,_Modified");

            URL_Add("aptitude", "http://en.wikipedia.org/wiki/Aptitude_%28software%29");

            URL_Add("Redmine", "http://en.wikipedia.org/wiki/Redmine");

            URL_Add("Gitorious", "http://en.wikipedia.org/wiki/Gitorious");

            URL_Add("VPS", "http://en.wikipedia.org/wiki/Virtual_private_server");

            URL_Add("Google Search", "http://en.wikipedia.org/wiki/Google_Search");

            URL_Add("Yahoo Search", "http://en.wikipedia.org/wiki/Yahoo!_Search");

            URL_Add("Facebook", "http://en.wikipedia.org/wiki/Facebook");

            URL_Add("Flickr", "http://en.wikipedia.org/wiki/Flickr");

            URL_Add("VirtualBox", "http://en.wikipedia.org/wiki/VirtualBox");

            URL_Add("EJB", "http://en.wikipedia.org/wiki/Enterprise_JavaBean");

            URL_Add("Dojo Toolkit", "http://en.wikipedia.org/wiki/Dojo_Toolkit");

            URL_Add("Interface Builder", "http://en.wikipedia.org/wiki/Interface_Builder");

            URL_Add("LINQ&nbsp;to&nbsp;Objects", "http://en.wikipedia.org/wiki/Language_Integrated_Query#LINQ_to_Objects");

            URL_Add("iBATIS", "http://en.wikipedia.org/wiki/IBATIS");

            URL_Add("Wireshark", "http://en.wikipedia.org/wiki/Wireshark");

            URL_Add("MonoTouch", "http://en.wikipedia.org/wiki/Mono_(software)#MonoTouch");

            URL_Add(".NET 3.5", "http://en.wikipedia.org/wiki/.NET_Framework_version_history#.NET_Framework_3.5");

            URL_Add("SQL Server Management Studio", "http://en.wikipedia.org/wiki/SQL_Server_Management_Studio");

            URL_Add("VSTO", "http://en.wikipedia.org/wiki/Visual_Studio_Tools_for_Office");

            URL_Add("MIME", "http://en.wikipedia.org/wiki/MIME");

            URL_Add("URL", "http://en.wikipedia.org/wiki/Uniform_resource_locator");

            URL_Add("Mercurial", "http://en.wikipedia.org/wiki/Mercurial");

            URL_Add("BitKeeper", "http://en.wikipedia.org/wiki/BitKeeper");

            URL_Add("RAID", "http://en.wikipedia.org/wiki/RAID");

            URL_Add("SCSI", "http://en.wikipedia.org/wiki/SCSI");

            URL_Add("WAV", "http://en.wikipedia.org/wiki/WAV");

            URL_Add("CPython", "http://en.wikipedia.org/wiki/CPython");

            URL_Add("MP3", "http://en.wikipedia.org/wiki/MP3");

            URL_Add("Ogg", "http://en.wikipedia.org/wiki/Ogg");

            URL_Add("pip", "https://en.wikipedia.org/wiki/Pip_%28package_manager%29");

            URL_Add("virtualenv", "http://pypi.python.org/pypi/virtualenv");

            URL_Add("ATLAS", "http://en.wikipedia.org/wiki/Automatically_Tuned_Linear_Algebra_Software");

            URL_Add("R", "http://en.wikipedia.org/wiki/R_%28programming_language%29");

            URL_Add("gedit", "http://en.wikipedia.org/wiki/Gedit");

            URL_Add("vi", "http://en.wikipedia.org/wiki/Vi");

            URL_Add("Linux", "http://en.wikipedia.org/wiki/Linux");

            URL_Add("WMV", "http://en.wikipedia.org/wiki/Windows_Media_Video");

            URL_Add("MP4", "http://en.wikipedia.org/wiki/MPEG-4_Part_14");

            URL_Add("H.264", "http://en.wikipedia.org/wiki/H.264/MPEG-4_AVC");

            URL_Add("AAC", "http://en.wikipedia.org/wiki/Advanced_Audio_Coding");

            URL_Add("Stack&nbsp;Overflow", "http://en.wikipedia.org/wiki/Stack_Overflow");

            URL_Add("computer science", "http://en.wikipedia.org/wiki/Computer_science");

            URL_Add("Virual PC", "http://en.wikipedia.org/wiki/Windows_Virtual_PC");

            URL_Add("PCRE", "http://en.wikipedia.org/wiki/Perl_Compatible_Regular_Expressions");

            URL_Add("Zen Cart", "http://en.wikipedia.org/wiki/Zen_Cart");

            URL_Add("JRE", "https://en.wikipedia.org/wiki/Java_virtual_machine"); //Was http://en.wikipedia.org/wiki/Java_%28software_platform%29#Platform

            URL_Add("Safari", "http://en.wikipedia.org/wiki/Safari_%28web_browser%29");

            URL_Add("Opera", "http://en.wikipedia.org/wiki/Opera_%28web_browser%29");

            URL_Add("SMTP", "http://en.wikipedia.org/wiki/Simple_Mail_Transfer_Protocol");

            URL_Add("definitely", "http://en.wiktionary.org/wiki/definately"); //Funny alternative URL: <http://d-e-f-i-n-i-t-e-l-y.com/>

            URL_Add("Nagios", "http://en.wikipedia.org/wiki/Nagios");

            URL_Add("USB", "http://en.wikipedia.org/wiki/USB");

            //No Wikipedia article so far.
            URL_Add("jqGrid", "http://www.trirand.com/jqgridwiki/doku.php?id=start");

            URL_Add("NAT", "http://en.wikipedia.org/wiki/Network_address_translation");

            URL_Add("XMPP", "http://en.wikipedia.org/wiki/Extensible_Messaging_and_Presence_Protocol");

            URL_Add("IMHO", "http://en.wiktionary.org/wiki/IMHO");

            URL_Add("RMI", "http://en.wikipedia.org/wiki/Java_remote_method_invocation");

            URL_Add("OpenSocial", "http://en.wikipedia.org/wiki/OpenSocial");

            URL_Add("hard disk drive", "http://en.wikipedia.org/wiki/Hard_disk_drive");

            URL_Add("internationalisation and localisation", "http://en.wikipedia.org/wiki/Internationalization_and_localization");

            URL_Add("WAMP", "https://en.wikipedia.org/wiki/LAMP_(software_bundle)#Variants"); // Old (now redirects): http://en.wikipedia.org/wiki/WAMP

            URL_Add("GoDaddy", "https://en.wikipedia.org/wiki/GoDaddy");

            URL_Add("dependency injection", "http://en.wikipedia.org/wiki/Dependency_injection");

            URL_Add("SEO", "http://en.wikipedia.org/wiki/Search_engine_optimization");

            URL_Add("MD5", "http://en.wikipedia.org/wiki/MD5");

            URL_Add("SHA-1", "http://en.wikipedia.org/wiki/SHA-1");

            URL_Add("XSLT", "http://en.wikipedia.org/wiki/XSLT");

            URL_Add("InnoDB", "http://en.wikipedia.org/wiki/InnoDB");

            URL_Add("MyISAM", "http://en.wikipedia.org/wiki/MyISAM");

            URL_Add("HTML5", "http://en.wikipedia.org/wiki/HTML5");

            URL_Add("Confluence", "http://en.wikipedia.org/wiki/Confluence_%28software%29");

            URL_Add("repository", "http://en.wikipedia.org/wiki/Software_repository");

            URL_Add("Scrum", "http://en.wikipedia.org/wiki/Scrum_%28development%29");

            URL_Add("Team Foundation Server", "http://en.wikipedia.org/wiki/Team_Foundation_Server");

            URL_Add("MinGW", "http://en.wikipedia.org/wiki/MinGW");

            URL_Add("Code::Blocks", "http://en.wikipedia.org/wiki/Code::Blocks");

            URL_Add("AirPort", "http://en.wikipedia.org/wiki/AirPort");

            URL_Add("ADSL", "http://en.wikipedia.org/wiki/Asymmetric_Digital_Subscriber_Line");

            URL_Add("legitimate", "http://en.wiktionary.org/wiki/legit");

            URL_Add("pixels", "http://en.wikipedia.org/wiki/Pixel");

            URL_Add("W3C", "http://en.wikipedia.org/wiki/World_Wide_Web_Consortium");

            URL_Add("as far as I know", "http://en.wiktionary.org/wiki/AFAIK");

            URL_Add("Ubuntu", "http://en.wikipedia.org/wiki/Ubuntu_%28operating_system%29");

            URL_Add("DAO", "http://en.wikipedia.org/wiki/Data_access_object");

            URL_Add("Erlang", "http://en.wikipedia.org/wiki/Erlang_%28programming_language%29");

            URL_Add("SDK", "http://en.wikipedia.org/wiki/Software_development_kit");

            URL_Add("CentOS", "http://en.wikipedia.org/wiki/CentOS");

            URL_Add("OpenWrt", "http://en.wikipedia.org/wiki/OpenWrt");

            URL_Add("JPA", "http://en.wikipedia.org/wiki/Java_Persistence_API");

            URL_Add("GAC", "http://en.wikipedia.org/wiki/Global_Assembly_Cache");

            URL_Add("Dropbox", "http://en.wikipedia.org/wiki/Dropbox_%28service%29");

            URL_Add("ActiveX", "http://en.wikipedia.org/wiki/ActiveX");

            URL_Add("compatibility", "http://en.wiktionary.org/wiki/compatability");

            URL_Add("Wicket", "http://en.wikipedia.org/wiki/Apache_Wicket");

            URL_Add("dependent", "http://en.wiktionary.org/wiki/dependant#Noun");

            URL_Add("DOM", "http://en.wikipedia.org/wiki/Document_Object_Model");

            URL_Add("LaTeX", "http://en.wikipedia.org/wiki/LaTeX");

            URL_Add("SUSE&nbsp;Linux", "https://en.wikipedia.org/wiki/SUSE_Linux"); // Old: http://en.wikipedia.org/wiki/SUSE_Linux_distributions

            URL_Add("separate", "http://en.wiktionary.org/wiki/seperate");

            URL_Add("CPU", "http://en.wikipedia.org/wiki/Central_processing_unit");

            URL_Add("system administrator", "http://en.wikipedia.org/wiki/System_administrator");

            URL_Add("application domain", "https://en.wikipedia.org/wiki/Application_domain");

            URL_Add("MPI", "http://en.wikipedia.org/wiki/Message_Passing_Interface");

            URL_Add("Amazon EC2", "http://en.wikipedia.org/wiki/Amazon_Elastic_Compute_Cloud");

            URL_Add("Amazon RDS", "http://en.wikipedia.org/wiki/Amazon_RDS");

            URL_Add("user interface", "http://en.wikipedia.org/wiki/User_interface");

            URL_Add("Amazon S3", "http://en.wikipedia.org/wiki/Amazon_S3");

            URL_Add("XML", "http://en.wikipedia.org/wiki/XML");

            URL_Add("Wget", "http://en.wikipedia.org/wiki/Wget");

            URL_Add("SSD", "http://en.wikipedia.org/wiki/Solid-state_drive");

            URL_Add("Active Directory", "http://en.wikipedia.org/wiki/Active_Directory");

            URL_Add("Google Analytics", "http://en.wikipedia.org/wiki/Google_Analytics");

            URL_Add("Google Talk", "http://en.wikipedia.org/wiki/Google_Talk");

            URL_Add("distribution", "http://en.wikipedia.org/wiki/Linux_distribution");

            URL_Add("SOAP", "http://en.wikipedia.org/wiki/SOAP");

            URL_Add("nginx", "http://en.wikipedia.org/wiki/Nginx");

            URL_Add("lighttpd", "http://en.wikipedia.org/wiki/Lighttpd");

            URL_Add("Internet&nbsp;Explorer", "http://en.wikipedia.org/wiki/Internet_Explorer");

            URL_Add("Internet&nbsp;Explorer&nbsp;6", "http://en.wikipedia.org/wiki/Internet_Explorer_6");

            URL_Add("Internet&nbsp;Explorer&nbsp;7", "http://en.wikipedia.org/wiki/Internet_Explorer_7");

            URL_Add("Windows Workflow Foundation", "http://en.wikipedia.org/wiki/Windows_Workflow_Foundation");

            URL_Add("ASP Classic", "http://en.wikipedia.org/wiki/Active_Server_Pages");

            URL_Add("GUID", "http://en.wikipedia.org/wiki/Globally_unique_identifier");

            URL_Add("live CD", "http://en.wikipedia.org/wiki/Live_CD");

            URL_Add("Octave", "http://en.wikipedia.org/wiki/GNU_Octave");

            URL_Add("application", "http://en.wikipedia.org/wiki/Application_software");

            URL_Add("DirectX", "http://en.wikipedia.org/wiki/DirectX");

            URL_Add("API", "https://en.wikipedia.org/wiki/Application_programming_interface"); // http://en.wikipedia.org/wiki/Application_programming_interface

            URL_Add("CodeSmith Generator", "http://www.codesmithtools.com/product/generator");

            URL_Add("n-tier", "http://en.wikipedia.org/wiki/Multitier_architecture");

            //Was http://en.wikipedia.org/wiki/Delphi!!!
            //
            //Old URL was http://en.wikipedia.org/wiki/Delphi_%28language%29, now redirected. And
            //Incorrectly so, to Object Pascal, <http://en.wikipedia.org/wiki/Object_Pascal>.
            //
            URL_Add("Delphi", "http://en.wikipedia.org/wiki/Embarcadero_Delphi");

            URL_Add("SaaS", "http://en.wikipedia.org/wiki/Software_as_a_service");

            URL_Add("PDF", "http://en.wikipedia.org/wiki/Portable_Document_Format");

            URL_Add("Google Maps", "http://en.wikipedia.org/wiki/Google_Maps");

            URL_Add("Microsoft Access", "http://en.wikipedia.org/wiki/Microsoft_Access");

            URL_Add("documentation", "http://en.wikipedia.org/wiki/Documentation");

            URL_Add("SFTP", "http://en.wikipedia.org/wiki/SFTP");

            URL_Add("IPsec", "http://en.wikipedia.org/wiki/IPsec");

            URL_Add("openSUSE", "http://en.wikipedia.org/wiki/OpenSUSE");

            URL_Add("Qt Creator", "http://en.wikipedia.org/wiki/Qt_Creator");

            URL_Add("Internet&nbsp;Explorer&nbsp;8", "http://en.wikipedia.org/wiki/Internet_Explorer_8");

            URL_Add("Stack&nbsp;Exchange", "http://en.wikipedia.org/wiki/Stack_Exchange"); //Old: http://en.wikipedia.org/wiki/Stack_Exchange_Network

            URL_Add("developer", "https://en.wiktionary.org/wiki/developer#Noun"); // Old: http://en.wikipedia.org/wiki/Software_developer

            URL_Add("RubyGems", "http://en.wikipedia.org/wiki/RubyGems");

            URL_Add("Unix", "http://en.wikipedia.org/wiki/Unix");

            URL_Add("JS Bin", "http://jsbin.com/");

            URL_Add("JSFiddle", "http://jsfiddle.net/");

            URL_Add("Cocoa Touch", "http://en.wikipedia.org/wiki/Cocoa_Touch");

            URL_Add("JDK", "http://en.wikipedia.org/wiki/Java_Development_Kit");

            URL_Add("GUI", "http://en.wikipedia.org/wiki/Graphical_user_interface");

            URL_Add("qmake", "http://en.wikipedia.org/wiki/Qmake");

            URL_Add("Win32", "http://en.wikipedia.org/wiki/Windows_API");

            URL_Add("OpenSSL", "http://en.wikipedia.org/wiki/OpenSSL");

            URL_Add("OpenCV", "http://en.wikipedia.org/wiki/OpenCV");

            URL_Add("Server&nbsp;Fault", "https://serverfault.com/tour"); // Old: http://en.wikipedia.org/wiki/Server_Fault

            URL_Add("JUnit", "http://en.wikipedia.org/wiki/JUnit");

            URL_Add("I'm", "https://en.wiktionary.org/wiki/I'm#Contraction");

            URL_Add("IIS", "http://en.wikipedia.org/wiki/Internet_Information_Services");

            URL_Add("Windows Azure", "https://en.wikipedia.org/wiki/Microsoft_Azure"); //Was http://en.wikipedia.org/wiki/Azure_Services_Platform

            URL_Add("can't", "http://en.wiktionary.org/wiki/can%27t");

            URL_Add("Visual SourceSafe", "http://en.wikipedia.org/wiki/Microsoft_Visual_SourceSafe");

            URL_Add("Trac", "http://en.wikipedia.org/wiki/Trac");

            URL_Add("FogBugz", "http://en.wikipedia.org/wiki/FogBugz");

            URL_Add("ZigBee", "http://en.wikipedia.org/wiki/ZigBee");

            URL_Add("Hotmail", "http://en.wikipedia.org/wiki/Hotmail");

            URL_Add("code-behind", "http://en.wiktionary.org/wiki/code-behind");

            URL_Add("BCP", "http://en.wikipedia.org/wiki/Bulk_Copy_Program");

            URL_Add("Unity Application Block", "http://unity.codeplex.com/");

            URL_Add("Windows&nbsp;Explorer", "https://en.wikipedia.org/wiki/File_Explorer"); // Old: http://en.wikipedia.org/wiki/Windows_Explorer

            URL_Add("Castle Project", "http://en.wikipedia.org/wiki/Castle_Project"); //Alternative: <http://en.wikipedia.org/wiki/Castle_Project>

            URL_Add("SQL Server Compact", "http://en.wikipedia.org/wiki/SQL_Server_Compact");

            URL_Add("WMD", "http://code.google.com/p/wmd/");

            URL_Add("ZedGraph", "http://www.codeproject.com/KB/graphics/zedgraph.aspx");

            URL_Add("Jython", "http://en.wikipedia.org/wiki/Jython");

            URL_Add("IDE", "http://en.wikipedia.org/wiki/Integrated_development_environment");

            URL_Add("concatenate", "http://en.wiktionary.org/wiki/concatenate");

            URL_Add("GIF", "http://en.wikipedia.org/wiki/Graphics_Interchange_Format");

            URL_Add("XBee", "http://en.wikipedia.org/wiki/XBee"); //Old: http://en.wikipedia.org/wiki/Digi_International#Wireless

            URL_Add("Node.js", "http://en.wikipedia.org/wiki/Node.js"); //Corrected 2012-04-10

            URL_Add("XMLHttpRequest", "http://en.wikipedia.org/wiki/XMLHttpRequest");

            URL_Add("though", "http://en.wiktionary.org/wiki/though");

            URL_Add("RADIUS", "http://en.wikipedia.org/wiki/RADIUS");

            URL_Add("authentication", "http://en.wiktionary.org/wiki/authentication");

            URL_Add("ZIP", "http://en.wikipedia.org/wiki/ZIP_%28file_format%29");

            URL_Add("Comet", "http://en.wikipedia.org/wiki/Comet_%28programming%29");

            URL_Add("PPTP", "http://en.wikipedia.org/wiki/Point-to-Point_Tunneling_Protocol");

            URL_Add("Windows&nbsp;XP", "http://en.wikipedia.org/wiki/Windows_XP");

            URL_Add("Windows&nbsp;Vista", "http://en.wikipedia.org/wiki/Windows_Vista");

            URL_Add("Windows&nbsp;7", "http://en.wikipedia.org/wiki/Windows_7");

            URL_Add("configuration", "http://en.wiktionary.org/wiki/configuration");

            URL_Add("VoIP", "http://en.wikipedia.org/wiki/Voice_over_IP");

            URL_Add("Fedora", "http://en.wikipedia.org/wiki/Fedora_%28operating_system%29");

            URL_Add("Windows", "http://en.wikipedia.org/wiki/Microsoft_Windows");

            URL_Add("information", "http://en.wiktionary.org/wiki/info#Noun");

            URL_Add("IP address", "http://en.wikipedia.org/wiki/IP_address");

            URL_Add("statistics", "http://en.wiktionary.org/wiki/statistics");

            URL_Add("by the way", "http://en.wiktionary.org/wiki/BTW");

            URL_Add("for example", "http://en.wiktionary.org/wiki/for_example");

            URL_Add(", that is, X", "http://en.wiktionary.org/wiki/i.e.");

            URL_Add("Kohana", "http://en.wikipedia.org/wiki/Kohana");

            URL_Add("PHP&nbsp;5", "http://en.wikipedia.org/wiki/PHP#Release_history");

            URL_Add("PEAR", "http://en.wikipedia.org/wiki/PEAR");

            URL_Add("HTTPS", "http://en.wikipedia.org/wiki/HTTP_Secure");

            URL_Add("JSONP", "http://en.wikipedia.org/wiki/JSONP");

            URL_Add("parameter", "http://en.wiktionary.org/wiki/parameter");

            URL_Add("UltraEdit", "http://en.wikipedia.org/wiki/UltraEdit");

            URL_Add("characters", "http://en.wiktionary.org/wiki/characters");

            URL_Add("Markdown", "http://en.wikipedia.org/wiki/Markdown");

            URL_Add("XSS", "http://en.wikipedia.org/wiki/Cross-site_scripting");

            URL_Add("JSLint", "http://en.wikipedia.org/wiki/JSLint");

            URL_Add("DNS", "http://en.wikipedia.org/wiki/Domain_Name_System");

            URL_Add("RAM", "http://en.wikipedia.org/wiki/Random-access_memory");

            URL_Add("SQL Server 2000", "https://en.wikipedia.org/wiki/History_of_Microsoft_SQL_Server#SQL_Server_2000"); // Old: http://en.wikipedia.org/wiki/Microsoft_SQL_Server#Genesis

            URL_Add("Rake", "http://en.wikipedia.org/wiki/Rake_(software)");

            URL_Add("temporary", "http://en.wiktionary.org/wiki/temp#Abbreviation");

            URL_Add("IMO", "https://en.wiktionary.org/wiki/IMO#Initialism");

            URL_Add("Google-fu", "http://en.wiktionary.org/wiki/Google-fu");

            URL_Add("SSH", "http://en.wikipedia.org/wiki/Secure_Shell");

            URL_Add("VPN", "http://en.wikipedia.org/wiki/Virtual_private_network");

            URL_Add("FAQ", "http://en.wikipedia.org/wiki/FAQ");

            URL_Add("Axis", "http://en.wikipedia.org/wiki/Apache_Axis");

            URL_Add("certificate", "http://en.wikipedia.org/wiki/Public_key_certificate");

            URL_Add("eBay", "http://en.wikipedia.org/wiki/EBay");

            URL_Add("OK", "http://en.wiktionary.org/wiki/OK#Etymology_1");

            URL_Add("LGPL", "http://en.wikipedia.org/wiki/GNU_Lesser_General_Public_License");

            URL_Add("IIRC", "http://en.wiktionary.org/wiki/IIRC");

            URL_Add("variable", "http://en.wikipedia.org/wiki/Variable_%28computer_science%29");

            URL_Add("µTorrent", "http://en.wikipedia.org/wiki/%CE%9CTorrent");

            URL_Add("iOS", "http://en.wikipedia.org/wiki/IOS"); //Old: http://en.wikipedia.org/wiki/IOS_%28Apple%29

            URL_Add("SBT", "http://en.wikipedia.org/wiki/Simple_Build_Tool");

            URL_Add("Vimeo", "http://en.wikipedia.org/wiki/Vimeo");

            URL_Add("Netflix", "http://en.wikipedia.org/wiki/Netflix");

            URL_Add("LinkedIn", "http://en.wikipedia.org/wiki/LinkedIn");

            URL_Add("Tumblr", "http://en.wikipedia.org/wiki/Tumblr");

            URL_Add("Mac&nbsp;OS&nbsp;X&nbsp;v10.6 (Snow&nbsp;Leopard)", "http://en.wikipedia.org/wiki/Mac_OS_X_Snow_Leopard");

            URL_Add("Wikipedia", "http://en.wiktionary.org/wiki/Wikipedia#Proper_noun");

            URL_Add("Haml", "http://en.wikipedia.org/wiki/Haml");

            URL_Add("UDP", "http://en.wikipedia.org/wiki/User_Datagram_Protocol");

            URL_Add("VNC", "http://en.wikipedia.org/wiki/Virtual_Network_Computing");

            URL_Add("MacPorts", "http://en.wikipedia.org/wiki/MacPorts");

            URL_Add("specification", "http://en.wiktionary.org/wiki/specification");

            URL_Add("MIDlet", "http://en.wikipedia.org/wiki/MIDlet");

            URL_Add("S60", "http://en.wikipedia.org/wiki/S60_%28software_platform%29");

            URL_Add("WSDL", "http://en.wikipedia.org/wiki/Web_Services_Description_Language");

            URL_Add("Cocos2d", "http://en.wikipedia.org/wiki/Cocos2d");

            URL_Add("iPad", "http://en.wikipedia.org/wiki/IPad");

            URL_Add("Super&nbsp;User", "https://superuser.com/tour"); // Old: http://en.wikipedia.org/wiki/Stack_Overflow#Super_User

            URL_Add("Meta&nbsp;Stack&nbsp;Overflow", "http://en.wikipedia.org/wiki/Stack_Overflow#Meta_Stack_Overflow");

            URL_Add("CoffeeScript", "http://en.wikipedia.org/wiki/CoffeeScript");

            URL_Add("JScript", "http://en.wikipedia.org/wiki/JScript");

            URL_Add("Rhino", "http://en.wikipedia.org/wiki/Rhino_%28JavaScript_engine%29");

            URL_Add("NAS", "http://en.wikipedia.org/wiki/Network-attached_storage");

            URL_Add("SVG", "http://en.wikipedia.org/wiki/Scalable_Vector_Graphics");

            URL_Add("DotNetNuke", "http://en.wikipedia.org/wiki/DotNetNuke");

            URL_Add("Adobe AIR", "http://en.wikipedia.org/wiki/Adobe_Integrated_Runtime");

            URL_Add("AWT", "http://en.wikipedia.org/wiki/Abstract_Window_Toolkit");

            URL_Add("IrfanView", "http://en.wikipedia.org/wiki/IrfanView");

            URL_Add("user experience", "http://en.wikipedia.org/wiki/User_experience");

            URL_Add("SQLAlchemy", "http://en.wikipedia.org/wiki/SQLAlchemy");

            URL_Add("BIOS", "http://en.wikipedia.org/wiki/BIOS");

            URL_Add("Plone", "http://en.wikipedia.org/wiki/Plone_%28software%29");

            URL_Add("Android&nbsp;2.3 (Gingerbread)", "https://en.wikipedia.org/wiki/Android_Gingerbread");

            URL_Add("SimpleDB", "http://en.wikipedia.org/wiki/Amazon_SimpleDB");

            URL_Add("RRDtool", "http://en.wikipedia.org/wiki/RRDtool");

            URL_Add("NFS", "http://en.wikipedia.org/wiki/Network_File_System_%28protocol%29");

            URL_Add("RS-232", "http://en.wikipedia.org/wiki/RS-232");

            URL_Add("Skype", "http://en.wikipedia.org/wiki/Skype");

            URL_Add("Forth", "http://en.wikipedia.org/wiki/Forth_%28programming_language%29");

            URL_Add("argument", "http://en.wikipedia.org/wiki/Parameter_%28computer_programming%29");

            URL_Add("EPUB", "http://en.wikipedia.org/wiki/EPUB");

            URL_Add("Sass", "http://en.wikipedia.org/wiki/Sass_%28stylesheet_language%29");

            URL_Add("Coda", "http://en.wikipedia.org/wiki/Coda_%28web_development_software%29");

            URL_Add("Espresso", "http://macrabbit.com/espresso/");

            URL_Add("SWIG", "http://en.wikipedia.org/wiki/SWIG");

            URL_Add("directory", "http://en.wikipedia.org/wiki/Folder_%28computing%29");

            URL_Add("don’t", "http://en.wiktionary.org/wiki/don%27t");

            URL_Add("Lisp", "http://en.wikipedia.org/wiki/Lisp_%28programming_language%29");

            URL_Add("LAPACK", "http://en.wikipedia.org/wiki/LAPACK");

            URL_Add("JRuby", "http://en.wikipedia.org/wiki/JRuby");

            URL_Add("Nvidia", "http://en.wikipedia.org/wiki/Nvidia");

            URL_Add("Windows Registry", "http://en.wikipedia.org/wiki/Windows_Registry");

            URL_Add("Microsoft", "http://en.wikipedia.org/wiki/Microsoft");

            URL_Add("Area&nbsp;51", "http://en.wikipedia.org/wiki/Stack_Exchange#History"); //URL was: http://en.wikipedia.org/wiki/Stack_Exchange_Network#area51

            URL_Add("JPEG", "http://en.wikipedia.org/wiki/JPEG");

            URL_Add("password", "http://en.wikipedia.org/wiki/Password");

            URL_Add("PhoneGap", "https://en.wikipedia.org/wiki/Apache_Cordova"); //URL was (project renamed to "Cordova"): http://en.wikipedia.org/wiki/PhoneGap

            URL_Add("Managed Extensibility Framework", "http://en.wikipedia.org/wiki/Managed_Extensibility_Framework");

            URL_Add("Cassandra", "http://en.wikipedia.org/wiki/Apache_Cassandra");

            URL_Add("reputation points", "http://meta.stackoverflow.com/questions/7237/how-does-reputation-work");

            URL_Add("GPL", "http://en.wikipedia.org/wiki/GNU_General_Public_License");

            URL_Add("Squid", "http://en.wikipedia.org/wiki/Squid_%28software%29");

            URL_Add("Hulu", "http://en.wikipedia.org/wiki/Hulu");

            URL_Add("WPA", "http://en.wikipedia.org/wiki/Wi-Fi_Protected_Access");

            URL_Add("SELinux", "http://en.wikipedia.org/wiki/Security-Enhanced_Linux");

            URL_Add("moderator", "http://en.wikipedia.org/wiki/Moderator#Internet");

            URL_Add("CDN", "http://en.wikipedia.org/wiki/Content_delivery_network");

            URL_Add("Amazon AWS", "http://en.wikipedia.org/wiki/Amazon_Web_Services");

            URL_Add("Red Hat Linux", "http://en.wikipedia.org/wiki/Red_Hat_Linux");

            URL_Add("phishing", "http://en.wikipedia.org/wiki/Phishing");

            URL_Add("DHCP", "http://en.wikipedia.org/wiki/Dynamic_Host_Configuration_Protocol");

            URL_Add("IPython", "http://en.wikipedia.org/wiki/IPython");

            URL_Add("your", "http://en.wiktionary.org/wiki/your");

            URL_Add("haven't", "http://en.wiktionary.org/wiki/haven%27t#Contraction");

            URL_Add("Emacs&nbsp;Lisp", "http://en.wikipedia.org/wiki/Emacs_Lisp");

            URL_Add("FBML", "http://en.wikipedia.org/wiki/Facebook_features#FBML");

            URL_Add("REST", "http://en.wikipedia.org/wiki/Representational_State_Transfer");

            URL_Add("Windows 2000", "http://en.wikipedia.org/wiki/Windows_2000");

            URL_Add("IPA", "http://en.wikipedia.org/wiki/.ipa_%28file_extension%29");

            URL_Add("DMA", "http://en.wikipedia.org/wiki/Direct_memory_access");

            URL_Add("content management system (CMS)", "http://en.wikipedia.org/wiki/Content_management_system");

            URL_Add("HootSuite", "http://en.wikipedia.org/wiki/HootSuite");

            URL_Add("TweetDeck", "http://en.wikipedia.org/wiki/TweetDeck");

            URL_Add("Bugzilla", "http://en.wikipedia.org/wiki/Bugzilla");

            URL_Add("Microsoft Exchange Server", "http://en.wikipedia.org/wiki/Microsoft_Exchange_Server");

            URL_Add("because", "http://en.wiktionary.org/wiki/because");

            URL_Add("ADK", "http://developer.android.com/guide/topics/usb/adk.html");

            URL_Add("Debian", "http://en.wikipedia.org/wiki/Debian");

            URL_Add("Mathematica", "http://en.wikipedia.org/wiki/Mathematica");

            URL_Add("JIT", "http://en.wikipedia.org/wiki/Just-in-time_compilation");

            URL_Add("DRY", "http://en.wikipedia.org/wiki/Don%27t_repeat_yourself");

            URL_Add("GRUB", "http://en.wikipedia.org/wiki/GNU_GRUB");

            URL_Add("GNOME", "http://en.wikipedia.org/wiki/GNOME");

            URL_Add("Android Virtual Device", "http://developer.android.com/guide/developing/devices/index.html");

            URL_Add("OCaml", "http://en.wikipedia.org/wiki/Objective_Caml");

            URL_Add("Boo", "http://en.wikipedia.org/wiki/Boo_%28programming_language%29");

            URL_Add("JAD", "http://en.wikipedia.org/wiki/JAD_%28file_format%29");

            URL_Add("SD card", "http://en.wikipedia.org/wiki/Secure_Digital");

            URL_Add("Symbian", "http://en.wikipedia.org/wiki/Symbian");

            URL_Add("Windows Mobile", "http://en.wikipedia.org/wiki/Windows_Mobile");

            URL_Add("Windows Phone", "https://en.wikipedia.org/wiki/Windows_Phone");

            URL_Add("jQuery Mobile", "http://en.wikipedia.org/wiki/JQuery_Mobile");

            URL_Add("Smalltalk", "http://en.wikipedia.org/wiki/Smalltalk");

            URL_Add("SharpDevelop", "http://en.wikipedia.org/wiki/SharpDevelop");

            URL_Add("ZX Spectrum", "http://en.wikipedia.org/wiki/ZX_Spectrum");

            URL_Add("inversion of control", "http://en.wikipedia.org/wiki/Inversion_of_control");

            URL_Add("WebSphere", "http://en.wikipedia.org/wiki/IBM_WebSphere");

            URL_Add("BBEdit", "http://en.wikipedia.org/wiki/BBEdit");

            URL_Add("won't", "http://en.wiktionary.org/wiki/won%27t");

            URL_Add("XSL", "http://en.wikipedia.org/wiki/XSL");

            URL_Add("XHTML", "http://en.wikipedia.org/wiki/XHTML");

            URL_Add("CVS", "http://en.wikipedia.org/wiki/Concurrent_Versions_System");

            URL_Add("OpenSSH", "http://en.wikipedia.org/wiki/OpenSSH");

            URL_Add("OPML", "http://en.wikipedia.org/wiki/OPML");

            URL_Add("WAP", "http://en.wikipedia.org/wiki/Wireless_Application_Protocol");

            URL_Add("Google Translate", "http://en.wikipedia.org/wiki/Google_Translate");

            URL_Add("XSD", "http://en.wikipedia.org/wiki/XML_Schema_%28W3C%29");

            URL_Add("Glide&nbsp;API", "http://en.wikipedia.org/wiki/Glide_API");

            URL_Add("POV-Ray", "http://en.wikipedia.org/wiki/POV-Ray");

            URL_Add("XNA", "http://en.wikipedia.org/wiki/Microsoft_XNA");

            URL_Add("Direct3D", "http://en.wikipedia.org/wiki/Microsoft_Direct3D");

            URL_Add("Bézier curve", "http://en.wikipedia.org/wiki/B%C3%A9zier_curve");

            URL_Add("without", "http://en.wiktionary.org/wiki/without");

            URL_Add("Blender", "http://en.wikipedia.org/wiki/Blender_%28software%29");

            URL_Add("InstallShield", "http://en.wikipedia.org/wiki/InstallShield");

            URL_Add("MSI", "http://en.wikipedia.org/wiki/Windows_Installer");

            URL_Add("RTF", "http://en.wikipedia.org/wiki/Rich_Text_Format");

            URL_Add("AES", "http://en.wikipedia.org/wiki/Advanced_Encryption_Standard");

            URL_Add("CIL", "http://en.wikipedia.org/wiki/Common_Intermediate_Language");

            URL_Add("GC", "http://en.wikipedia.org/wiki/Garbage_collection_%28computer_science%29");

            URL_Add("URI", "http://en.wikipedia.org/wiki/Uniform_Resource_Identifier");

            URL_Add("through", "http://en.wiktionary.org/wiki/through");

            URL_Add("tutorial", "http://en.wiktionary.org/wiki/tutorial");

            URL_Add("jQuery&nbsp;Isotope", "http://isotope.metafizzy.co/docs/introduction.html");

            URL_Add("Twitter", "http://en.wikipedia.org/wiki/Twitter");

            URL_Add("DOS", "http://en.wikipedia.org/wiki/DOS");

            URL_Add("App&nbsp;Store", "http://en.wikipedia.org/wiki/App_Store_%28iOS%29");

            URL_Add("Mac", "http://en.wikipedia.org/wiki/Macintosh"); //Collision with MAC, Media Access Control (computer networking related).

            URL_Add("ANTLR", "http://en.wikipedia.org/wiki/ANTLR");

            URL_Add("AST", "http://en.wikipedia.org/wiki/Abstract_syntax_tree");

            URL_Add("UML", "http://en.wikipedia.org/wiki/Unified_Modeling_Language");

            URL_Add("SWF", "http://en.wikipedia.org/wiki/SWF");

            URL_Add("PNG", "http://en.wikipedia.org/wiki/Portable_Network_Graphics");

            URL_Add("Compass", "http://en.wikipedia.org/wiki/Compass_Project");

            URL_Add("NIC", "http://en.wikipedia.org/wiki/Network_interface_controller");

            URL_Add("CMake", "http://en.wikipedia.org/wiki/CMake");

            URL_Add("Hadoop", "http://en.wikipedia.org/wiki/Apache_Hadoop");

            URL_Add("Base64", "http://en.wikipedia.org/wiki/Base64");

            URL_Add("bit/s", "http://en.wikipedia.org/wiki/Data_rate_units");

            URL_Add("kbit/s", "http://en.wikipedia.org/wiki/Data_rate_units");

            URL_Add("Mbit/s", "http://en.wikipedia.org/wiki/Data_rate_units");

            URL_Add("Gbit/s", "http://en.wikipedia.org/wiki/Data_rate_units");

            URL_Add("Windows&nbsp;CE", "http://en.wikipedia.org/wiki/Windows_CE");

            URL_Add("Amazon.com", "http://en.wikipedia.org/wiki/Amazon.com");

            URL_Add("microcontroller", "http://en.wikipedia.org/wiki/Microcontroller");

            URL_Add("Visual Basic 6.0", "http://en.wikipedia.org/wiki/Visual_Basic#Timeline");

            URL_Add("Visual Basic", "http://en.wikipedia.org/wiki/Visual_Basic");

            URL_Add("etc.", "http://en.wiktionary.org/wiki/etc.#Adverb");

            URL_Add("MIDI", "http://en.wikipedia.org/wiki/MIDI");

            URL_Add("WinRAR", "http://en.wikipedia.org/wiki/WinRAR");

            URL_Add("which", "http://en.wiktionary.org/wiki/which");

            URL_Add("Fiddler", "http://en.wikipedia.org/wiki/Fiddler_%28software%29");

            URL_Add("Outlook", "http://en.wikipedia.org/wiki/Microsoft_Outlook");

            URL_Add("Google+", "http://en.wikipedia.org/wiki/Google%2B");

            URL_Add("software", "http://en.wiktionary.org/wiki/software");

            URL_Add("NCover", "http://en.wikipedia.org/wiki/NCover");

            URL_Add("kHz", "http://en.wikipedia.org/wiki/Hertz#SI_multiples");
            URL_Add("MHz", "http://en.wikipedia.org/wiki/Hertz#SI_multiples");
            URL_Add("GHz", "http://en.wikipedia.org/wiki/Hertz#SI_multiples");

            URL_Add("CScript", "http://en.wikipedia.org/wiki/Windows_Script_Host#Available_scripting_engines");

            URL_Add("SmartSVN", "http://en.wikipedia.org/wiki/SmartSVN");

            URL_Add("SMS", "http://en.wikipedia.org/wiki/SMS");

            URL_Add("EEPROM", "http://en.wikipedia.org/wiki/EEPROM");

            URL_Add("RTMP", "http://en.wikipedia.org/wiki/Real_Time_Messaging_Protocol");

            URL_Add("Gentoo Linux", "http://en.wikipedia.org/wiki/Gentoo_Linux");

            URL_Add("Java Web Start", "http://en.wikipedia.org/wiki/Java_Web_Start");

            URL_Add("Apache HTTP Server", "http://en.wikipedia.org/wiki/Apache_HTTP_Server");

            URL_Add("Doxygen", "http://en.wikipedia.org/wiki/Doxygen");

            URL_Add("Graphviz", "http://en.wikipedia.org/wiki/Graphviz");

            URL_Add("library", "http://en.wikipedia.org/wiki/Library_%28computing%29");

            URL_Add("advertisement", "http://en.wikipedia.org/wiki/Advertisements");

            URL_Add("Open&nbsp;MPI", "http://en.wikipedia.org/wiki/Open_MPI");

            URL_Add("Mac&nbsp;OS&nbsp;X&nbsp;v10.7 (Lion)", "http://en.wikipedia.org/wiki/Mac_OS_X_Lion");

            URL_Add("Virtual&nbsp;PC", "http://en.wikipedia.org/wiki/Windows_Virtual_PC");

            URL_Add("NAnt", "http://en.wikipedia.org/wiki/NAnt");

            URL_Add("Redgate Software", "https://en.wikipedia.org/wiki/Redgate"); // Old: http://en.wikipedia.org/wiki/Red_Gate_Software

            URL_Add("SWT", "http://en.wikipedia.org/wiki/Standard_Widget_Toolkit");

            URL_Add("Jetty", "http://en.wikipedia.org/wiki/Jetty_%28web_server%29");

            URL_Add("Spring MVC", "http://en.wikipedia.org/wiki/Spring_Framework#Model-view-controller_framework");

            URL_Add("Jabber", "http://en.wikipedia.org/wiki/Extensible_Messaging_and_Presence_Protocol");

            URL_Add("Facebook Connect", "http://en.wikipedia.org/wiki/Facebook_Platform#Facebook_Connect");

            URL_Add("Fedora Core", "http://en.wikipedia.org/wiki/Fedora_(operating_system)");

            URL_Add("Fedora Core 11", "http://en.wikipedia.org/wiki/List_of_Fedora_versions#Fedora_11");

            URL_Add("Apache Shiro", "http://en.wikipedia.org/wiki/Apache_Shiro");

            URL_Add("Ehcache", "http://en.wikipedia.org/wiki/Ehcache");

            URL_Add("cryptography", "http://en.wikipedia.org/wiki/Cryptography");

            URL_Add("DLL file", "http://en.wikipedia.org/wiki/Dynamic-link_library");

            URL_Add("SSIS", "http://en.wikipedia.org/wiki/SQL_Server_Integration_Services");

            URL_Add("Enterprise Library", "http://en.wikipedia.org/wiki/Microsoft_Enterprise_Library");

            URL_Add("Hyper-V", "http://en.wikipedia.org/wiki/Hyper-V");

            URL_Add("Blu-ray", "http://en.wikipedia.org/wiki/Blu-ray_Disc");

            URL_Add("antivirus software", "http://en.wikipedia.org/wiki/Antivirus_software");

            URL_Add("X-ray", "http://en.wikipedia.org/wiki/X-ray");

            URL_Add("BSoD", "http://en.wikipedia.org/wiki/Blue_Screen_of_Death");

            URL_Add("Lynx", "http://en.wikipedia.org/wiki/Lynx_%28web_browser%29");

            URL_Add("project management", "http://en.wikipedia.org/wiki/Project_management");

            URL_Add("Windows Update", "http://en.wikipedia.org/wiki/Windows_Update");

            URL_Add("TortoiseSVN", "http://en.wikipedia.org/wiki/TortoiseSVN");

            URL_Add("StructureMap", "http://structuremap.net/structuremap/");

            URL_Add("DD-WRT", "http://en.wikipedia.org/wiki/DD-WRT");

            URL_Add("APK", "http://en.wikipedia.org/wiki/APK_%28file_format%29");

            URL_Add("DRM", "http://en.wikipedia.org/wiki/Digital_rights_management");

            URL_Add("Tkinter", "http://en.wikipedia.org/wiki/Tkinter");

            URL_Add("Graph API", "http://en.wikipedia.org/wiki/Facebook_Platform#Graph_API");

            URL_Add("TortoiseGit", "http://en.wikipedia.org/wiki/TortoiseGit");

            URL_Add("CANopen", "http://en.wikipedia.org/wiki/CANopen");

            URL_Add("Open Graph", "http://en.wikipedia.org/wiki/Facebook_Platform#Open_Graph_protocol");

            URL_Add("Dev-C++", "http://en.wikipedia.org/wiki/Dev-C%2B%2B");

            URL_Add("FQL", "http://en.wikipedia.org/wiki/Facebook_Query_Language");

            URL_Add("something", "http://en.wiktionary.org/wiki/something");

            URL_Add("Windows Server 2003", "http://en.wikipedia.org/wiki/Windows_Server_2003");

            URL_Add("Exchange Server 2003", "http://en.wikipedia.org/wiki/Microsoft_Exchange_Server#Exchange_Server_2003");

            URL_Add("SHA-256", "http://en.wikipedia.org/wiki/SHA-2");

            URL_Add("Google Calendar", "http://en.wikipedia.org/wiki/Google_Calendar");

            URL_Add("Task Manager", "http://en.wikipedia.org/wiki/Windows_Task_Manager");

            URL_Add("Sysinternals", "https://en.wikipedia.org/wiki/Sysinternals"); // Old: http://en.wikipedia.org/wiki/Winternals

            URL_Add("UNC", "http://en.wikipedia.org/wiki/Path_%28computing%29#Uniform_Naming_Convention");

            URL_Add("RF", "http://en.wikipedia.org/wiki/Radio_frequency");

            URL_Add("LOB", "http://en.wikipedia.org/wiki/Line_of_business");

            URL_Add("SQL Server Reporting Services (SSRS)", "http://en.wikipedia.org/wiki/SQL_Server_Reporting_Services");

            URL_Add("TLS", "http://en.wikipedia.org/wiki/Transport_Layer_Security");

            URL_Add("POJO", "http://en.wikipedia.org/wiki/Plain_Old_Java_Object");

            URL_Add("HQL", "http://en.wikipedia.org/wiki/Hibernate_%28Java%29#Hibernate_Query_Language_.28HQL.29");

            URL_Add("Perforce", "http://en.wikipedia.org/wiki/Perforce");

            URL_Add("Bazaar", "http://en.wikipedia.org/wiki/Bazaar_%28software%29");

            URL_Add("FishEye", "http://en.wikipedia.org/wiki/FishEye_%28software%29");

            URL_Add("cron", "http://en.wikipedia.org/wiki/Cron");

            URL_Add("Celery", "http://ask.github.com/celery/getting-started/introduction.html");

            URL_Add("Arch Linux", "http://en.wikipedia.org/wiki/Arch_Linux");

            URL_Add("Pacman", "http://en.wikipedia.org/wiki/Pacman_%28package_manager%29");

            URL_Add("grep", "http://en.wikipedia.org/wiki/Grep");

            URL_Add("architecture", "http://en.wikipedia.org/wiki/Computer_architecture");

            URL_Add("wxPython", "http://en.wikipedia.org/wiki/WxPython");

            URL_Add("STL", "http://en.wikipedia.org/wiki/Standard_Template_Library");

            URL_Add("Dalvik", "http://en.wikipedia.org/wiki/Dalvik_%28software%29");

            URL_Add("tracert", "http://en.wikipedia.org/wiki/Traceroute");

            URL_Add("DTrace", "http://en.wikipedia.org/wiki/DTrace");

            URL_Add("WatiN", "http://en.wikipedia.org/wiki/Watir#Similar_tools");

            URL_Add("Device Manager", "http://en.wikipedia.org/wiki/Device_Manager");

            URL_Add("duplicate", "http://en.wiktionary.org/wiki/duplicate");

            URL_Add("Stack&nbsp;Apps", "http://stackapps.com/about");

            URL_Add("primary key", "http://en.wikipedia.org/wiki/Unique_key#Defining_primary_keys");

            URL_Add("Arduino Uno", "http://arduino.cc/en/Main/ArduinoBoardUno"); //Old: broken now, but there is no real good article on Wikipedia. http://en.wikipedia.org/wiki/Arduino#ArduinoUno

            URL_Add("Nexus S", "http://en.wikipedia.org/wiki/Nexus_S");

            URL_Add("ExecJS", "http://rubygems.org/gems/execjs");

            URL_Add("Ruby Racer", "https://github.com/cowboyd/therubyracer");

            URL_Add("JVM", "http://en.wikipedia.org/wiki/Java_virtual_machine");

            URL_Add("TrueCrypt", "http://en.wikipedia.org/wiki/TrueCrypt");

            URL_Add("Versions", "http://www.versionsapp.com/");

            URL_Add("JAWS", "http://en.wikipedia.org/wiki/JAWS_%28screen_reader%29");

            URL_Add("URL rewriting", "http://en.wikipedia.org/wiki/Rewrite_engine");

            URL_Add("symbolic link", "http://en.wikipedia.org/wiki/Symbolic_link");

            URL_Add("ADT", "http://developer.android.com/guide/developing/tools/adt.html");

            URL_Add("PDB", "http://en.wikipedia.org/wiki/Program_database");

            URL_Add("Raspberry Pi", "http://en.wikipedia.org/wiki/Raspberry_Pi");

            URL_Add("environment", "http://en.wikipedia.org/wiki/Environment_variable");

            URL_Add("LAN", "http://en.wikipedia.org/wiki/Local_area_network");

            URL_Add("Javadoc", "http://en.wikipedia.org/wiki/Javadoc");

            URL_Add("Usenet", "http://en.wikipedia.org/wiki/Usenet");

            URL_Add("AKA", "http://en.wiktionary.org/wiki/AKA");

            URL_Add("sketch", "http://www.arduino.cc/en/Tutorial/Sketch");

            URL_Add("plain old data structure", "http://en.wikipedia.org/wiki/Plain_old_data_structure");

            URL_Add("PWM", "http://en.wikipedia.org/wiki/Pulse-width_modulation");

            URL_Add("CRUD", "http://en.wikipedia.org/wiki/Create,_read,_update_and_delete");

            URL_Add("PDO", "https://en.wikipedia.org/wiki/PHP#Development_and_community"); //Was http://en.wikipedia.org/wiki/PHP#History

            URL_Add("DB2", "http://en.wikipedia.org/wiki/IBM_DB2");

            URL_Add("MariaDB", "http://en.wikipedia.org/wiki/MariaDB");

            URL_Add("LED", "http://en.wikipedia.org/wiki/Light-emitting_diode");

            URL_Add("Experts-Exchange", "http://en.wikipedia.org/wiki/Experts-Exchange");            

            URL_Add("Asus", "http://en.wikipedia.org/wiki/Asus");

            URL_Add("reCAPTCHA", "http://en.wikipedia.org/wiki/ReCAPTCHA");

            URL_Add("SpinRite", "http://en.wikipedia.org/wiki/SpinRite");

            URL_Add("Microsoft Visual C++", "https://en.wikipedia.org/wiki/Microsoft_Visual_C%2B%2B"); // Old: http://en.wikipedia.org/wiki/Visual_C%2B%2B#32-bit_versions Old: https://en.wikipedia.org/wiki/Microsoft_Visual_C%2B%2B#32-bit_and_64-bit_versions

            URL_Add("latitude", "http://en.wikipedia.org/wiki/Latitude");

            URL_Add("longitude", "http://en.wikipedia.org/wiki/Longitude");

            URL_Add("iPod", "http://en.wikipedia.org/wiki/IPod");

            URL_Add("Windows&nbsp;7 Starter", "http://en.wikipedia.org/wiki/Windows_7_editions#Comparison_chart");

            URL_Add("FireWire", "http://en.wikipedia.org/wiki/IEEE_1394");

            URL_Add("Winamp", "http://en.wikipedia.org/wiki/Winamp");

            URL_Add("Windows Media Player", "http://en.wikipedia.org/wiki/Windows_Media_Player");

            URL_Add("AppleScript", "http://en.wikipedia.org/wiki/AppleScript");

            URL_Add("MathJax", "http://en.wikipedia.org/wiki/MathJax");

            URL_Add("Notepad2", "http://en.wikipedia.org/wiki/Notepad2");

            URL_Add("Solution Explorer", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Other_tools");

            URL_Add("rsync", "http://en.wikipedia.org/wiki/Rsync");

            URL_Add("SCP", "http://en.wikipedia.org/wiki/Secure_copy");

            URL_Add("Cython", "http://en.wikipedia.org/wiki/Cython");

            URL_Add("Rockbox", "http://en.wikipedia.org/wiki/Rockbox");

            URL_Add("Selenium", "http://en.wikipedia.org/wiki/Selenium_%28software%29");

            URL_Add("Ethernet", "http://en.wikipedia.org/wiki/Ethernet");

            URL_Add("F#", "http://en.wikipedia.org/wiki/F_Sharp_%28programming_language%29");

            URL_Add("C", "http://en.wikipedia.org/wiki/C_%28programming_language%29");

            URL_Add("Scott Guthrie", "http://en.wikipedia.org/wiki/Scott_Guthrie");

            URL_Add("Brief", "http://en.wikipedia.org/wiki/Brief_%28text_editor%29");

            URL_Add("Core 2 Duo", "http://en.wikipedia.org/wiki/List_of_Intel_Core_2_microprocessors#Core_2_Duo_2");

            URL_Add("RSS", "http://en.wikipedia.org/wiki/RSS");

            URL_Add("AVR Studio", "http://www.atmel.com/microsite/avr_studio_5/default.aspx");

            URL_Add("combination", "http://en.wiktionary.org/wiki/combination");

            URL_Add("IPv6", "http://en.wikipedia.org/wiki/IPv6");

            URL_Add("IPv4", "http://en.wikipedia.org/wiki/IPv4");

            URL_Add("SAX", "http://en.wikipedia.org/wiki/Simple_API_for_XML");

            URL_Add("TCP", "http://en.wikipedia.org/wiki/Transmission_Control_Protocol");

            URL_Add("CodePlex", "http://en.wikipedia.org/wiki/CodePlex");

            URL_Add("AOL", "http://en.wikipedia.org/wiki/AOL");

            URL_Add("benefit", "http://en.wiktionary.org/wiki/benefit");

            URL_Add("TestDriven.NET", "http://www.testdriven.net/");

            URL_Add("KnockoutJS", "http://en.wikipedia.org/wiki/KnockoutJS");

            URL_Add("Underscore.js", "https://en.wikipedia.org/wiki/Underscore.js"); //Old: http://documentcloud.github.com/underscore/

            URL_Add("DLL&nbsp;Hell", "http://en.wikipedia.org/wiki/DLL_Hell");

            //URL_Add("PS", "http://en.wiktionary.org/wiki/PS"); // Now PowerShell

            URL_Add("QEMU", "http://en.wikipedia.org/wiki/QEMU");

            URL_Add("Myspace", "http://en.wikipedia.org/wiki/Myspace");

            URL_Add("GNU", "http://en.wikipedia.org/wiki/GNU");

            URL_Add("Jeff Atwood", "http://en.wikipedia.org/wiki/Jeff_Atwood");

            URL_Add("Joel Spolsky", "http://en.wikipedia.org/wiki/Joel_Spolsky");

            URL_Add("LINQ&nbsp;to&nbsp;Entities", "http://en.wikipedia.org/wiki/ADO.NET_Entity_Framework#LINQ_to_Entities");

            URL_Add("YSlow", "http://yslow.org/");

            URL_Add("ClickOnce", "http://en.wikipedia.org/wiki/ClickOnce");

            URL_Add("CCleaner", "http://en.wikipedia.org/wiki/CCleaner");

            URL_Add("ISP", "http://en.wikipedia.org/wiki/Internet_service_provider");
            URL_Add("Kate", "http://en.wikipedia.org/wiki/Kate_%28text_editor%29");
            URL_Add("Remote Desktop Connection", "http://en.wikipedia.org/wiki/Remote_Desktop_Services#Remote_Desktop_Connection");

            URL_Add("PIC", "http://en.wikipedia.org/wiki/PIC_microcontroller");

            URL_Add("Windows Server 2008", "http://en.wikipedia.org/wiki/Windows_Server_2008");

            URL_Add("Windows Live", "http://en.wikipedia.org/wiki/Windows_Live");

            URL_Add("Beyond Compare", "http://en.wikipedia.org/wiki/Beyond_Compare");

            URL_Add("RDP", "http://en.wikipedia.org/wiki/Remote_Desktop_Protocol");

            URL_Add("KVM", "http://en.wikipedia.org/wiki/Kernel-based_Virtual_Machine");

            URL_Add("Process Monitor", "http://en.wikipedia.org/wiki/Process_Monitor");

            URL_Add("ILDASM", "http://en.wikipedia.org/wiki/Common_Intermediate_Language#Generation");

            URL_Add("Notepad++", "http://en.wikipedia.org/wiki/Notepad%2B%2B");

            URL_Add("TeamViewer", "http://en.wikipedia.org/wiki/TeamViewer");

            URL_Add("Craigslist", "http://en.wikipedia.org/wiki/Craigslist");

            URL_Add("NoSQL", "http://en.wikipedia.org/wiki/NoSQL");

            URL_Add("MPlayer", "http://en.wikipedia.org/wiki/MPlayer");

            URL_Add("iPod Touch", "http://en.wikipedia.org/wiki/IPod_Touch");

            URL_Add("microphone", "http://en.wikipedia.org/wiki/Microphone");

            URL_Add("TrueType", "http://en.wikipedia.org/wiki/TrueType");

            URL_Add("ext3", "http://en.wikipedia.org/wiki/Ext3");

            URL_Add("OpenStreetMap", "http://en.wikipedia.org/wiki/OpenStreetMap");

            URL_Add("RAR", "http://en.wikipedia.org/wiki/RAR");

            URL_Add("ARM", "http://en.wikipedia.org/wiki/ARM_architecture");

            URL_Add("artificial neural network", "http://en.wikipedia.org/wiki/Artificial_neural_network");

            URL_Add("DivX", "http://en.wikipedia.org/wiki/DivX");

            URL_Add("whether", "http://en.wiktionary.org/wiki/whether#Conjunction");

            URL_Add("Dragonfly", "http://en.wikipedia.org/wiki/Dragonfly");

            URL_Add("I²C", "http://en.wikipedia.org/wiki/I%C2%B2C");

            URL_Add("SDL", "http://en.wikipedia.org/wiki/Simple_DirectMedia_Layer");

            URL_Add("COBOL", "http://en.wikipedia.org/wiki/COBOL");

            URL_Add("Fortran", "http://en.wikipedia.org/wiki/Fortran");

            URL_Add("Design Patterns: Elements of Reusable Object-Oriented Software (GoF book)", "http://en.wikipedia.org/wiki/Design_Patterns");

            URL_Add("BASIC", "http://en.wikipedia.org/wiki/BASIC");

            URL_Add("domain controller", "http://en.wikipedia.org/wiki/Domain_controller");

            URL_Add("Ghostscript", "http://en.wikipedia.org/wiki/Ghostscript");

            URL_Add("JAR", "http://en.wikipedia.org/wiki/JAR_%28file_format%29");

            URL_Add("PowerGrep", "http://www.powergrep.com/");

            URL_Add("Razor", "http://en.wikipedia.org/wiki/Microsoft_ASP.NET_Razor_view_engine");

            URL_Add("WinMerge", "http://en.wikipedia.org/wiki/WinMerge");

            URL_Add("Imgur", "http://en.wikipedia.org/wiki/Imgur");

            URL_Add("GIMP", "http://en.wikipedia.org/wiki/GIMP");

            URL_Add("JTAG", "http://en.wikipedia.org/wiki/Joint_Test_Action_Group");

            URL_Add("UART", "http://en.wikipedia.org/wiki/Universal_asynchronous_receiver/transmitter");

            URL_Add("I’ve", "http://en.wiktionary.org/wiki/I%27ve#Contraction");

            URL_Add("Foursquare", "http://en.wikipedia.org/wiki/Foursquare");

            URL_Add("Google Code", "https://en.wikipedia.org/wiki/Google_Developers#Google_Code"); // Old: http://en.wikipedia.org/wiki/Google_Code

            URL_Add("KDE&nbsp;4", "http://en.wikipedia.org/wiki/KDE_Software_Compilation_4#Released_versions");

            URL_Add("WoW", "http://en.wikipedia.org/wiki/Windows_on_Windows");

            URL_Add("xargs", "http://en.wikipedia.org/wiki/Xargs");

            URL_Add("find", "https://en.wikipedia.org/wiki/Find_(Unix)"); // Old: http://en.wikipedia.org/wiki/Find

            URL_Add("Moonlight", "http://en.wikipedia.org/wiki/Moonlight_%28runtime%29");

            URL_Add("Make", "http://en.wikipedia.org/wiki/Make_%28software%29");

            URL_Add("GPS", "http://en.wikipedia.org/wiki/Global_Positioning_System");

            URL_Add("BMP", "http://en.wikipedia.org/wiki/BMP_file_format");

            URL_Add("NuGet", "http://en.wikipedia.org/wiki/NuGet");

            URL_Add("Command&nbsp;Prompt", "http://en.wikipedia.org/wiki/Command_Prompt");

            URL_Add("Disk Cleanup", "http://en.wikipedia.org/wiki/Disk_Cleanup");

            URL_Add("CyanogenMod", "http://en.wikipedia.org/wiki/CyanogenMod");

            URL_Add("ADB", "https://en.wikipedia.org/wiki/Android_Debug_Bridge#Android_Debug_Bridge");

            URL_Add("Samsung Kies", "http://en.wikipedia.org/wiki/Samsung_Kies");

            URL_Add("FFmpeg", "http://en.wikipedia.org/wiki/FFmpeg");

            URL_Add("Knoppix", "http://en.wikipedia.org/wiki/Knoppix");

            // URL_Add("NETMF", "http://en.wikipedia.org/wiki/.NET_Micro_Framework");

            URL_Add("CPAN", "http://en.wikipedia.org/wiki/CPAN");

            URL_Add("Strawberry Perl", "http://en.wikipedia.org/wiki/Strawberry_Perl");

            URL_Add("NTFS", "http://en.wikipedia.org/wiki/NTFS");

            URL_Add("RFID", "http://en.wikipedia.org/wiki/Radio-frequency_identification");

            URL_Add("Google Earth", "http://en.wikipedia.org/wiki/Google_Earth");

            URL_Add("WiMAX", "http://en.wikipedia.org/wiki/WiMAX");

            URL_Add("FileZilla", "http://en.wikipedia.org/wiki/FileZilla");

            URL_Add("SPI", "http://en.wikipedia.org/wiki/Serial_Peripheral_Interface_Bus");

            URL_Add("FTP", "http://en.wikipedia.org/wiki/File_Transfer_Protocol");

            URL_Add("AVI", "http://en.wikipedia.org/wiki/Audio_Video_Interleave");

            URL_Add("JavaBeans", "http://en.wikipedia.org/wiki/JavaBeans");

            URL_Add("hosts file", "http://en.wikipedia.org/wiki/Hosts_(file)");

            URL_Add("CHKDSK", "http://en.wikipedia.org/wiki/CHKDSK");

            URL_Add("MFT", "http://en.wikipedia.org/wiki/NTFS#Internals");

            URL_Add("IRC", "http://en.wikipedia.org/wiki/Internet_Relay_Chat");

            URL_Add("Telnet", "http://en.wikipedia.org/wiki/Telnet");

            URL_Add("AutoHotkey", "http://en.wikipedia.org/wiki/AutoHotkey");

            URL_Add("BeagleBoard", "http://en.wikipedia.org/wiki/BeagleBoard");

            URL_Add("'like button'", "http://en.wikipedia.org/wiki/Like_button");

            URL_Add("shield", "http://www.arduino.cc/en/Main/ArduinoShields");

            URL_Add("FreeMat", "http://en.wikipedia.org/wiki/FreeMat");

            URL_Add("EPS", "http://en.wikipedia.org/wiki/Encapsulated_PostScript");

            URL_Add("EFI", "http://en.wikipedia.org/wiki/Unified_Extensible_Firmware_Interface");

            URL_Add("LogCat", "https://sites.google.com/site/androidhowto/how-to-1/save-logcat-to-a-text-file");

            URL_Add("between", "http://en.wiktionary.org/wiki/between");

            URL_Add("CORBA", "http://en.wikipedia.org/wiki/Common_Object_Request_Broker_Architecture");

            URL_Add("Dependency Walker", "http://en.wikipedia.org/wiki/Dependency_Walker");

            URL_Add("Photoshop", "http://en.wikipedia.org/wiki/Adobe_Photoshop");

            URL_Add("microSD", "http://en.wikipedia.org/wiki/Secure_Digital#Physical_size");

            URL_Add("Nmap", "http://en.wikipedia.org/wiki/Nmap");

            URL_Add("Ctrl", "http://en.wikipedia.org/wiki/Control_key");

            URL_Add("Reddit", "http://en.wikipedia.org/wiki/Reddit");

            URL_Add("QR code", "http://en.wikipedia.org/wiki/QR_code");

            URL_Add("intent", "http://developer.android.com/guide/components/intents-filters.html");

            URL_Add("nowadays", "http://en.wiktionary.org/wiki/nowadays");

            URL_Add("Yii", "http://en.wikipedia.org/wiki/Yii");

            URL_Add("SATA", "http://en.wikipedia.org/wiki/Serial_ATA");

            URL_Add("SQL injection", "http://en.wikipedia.org/wiki/SQL_injection");

            URL_Add("MySQLi", "http://en.wikipedia.org/wiki/MySQLi");

            URL_Add("GD Graphics Library", "http://en.wikipedia.org/wiki/GD_Graphics_Library");

            URL_Add(".NET Micro Framework", "http://en.wikipedia.org/wiki/.NET_Micro_Framework");

            URL_Add("FAT32", "http://en.wikipedia.org/wiki/File_Allocation_Table#FAT32");

            URL_Add("Apple", "http://en.wikipedia.org/wiki/Apple_Inc.");

            URL_Add("TLDR", "http://en.wiktionary.org/wiki/TLDR");

            URL_Add("LCD", "http://en.wikipedia.org/wiki/Liquid_crystal_display");

            URL_Add("ext4", "http://en.wikipedia.org/wiki/Ext4");

            URL_Add("U-Boot", "http://en.wikipedia.org/wiki/Das_U-Boot");

            URL_Add("X Window", "http://en.wikipedia.org/wiki/X_Window_System");

            URL_Add("EXE file", "http://en.wikipedia.org/wiki/EXE");

            URL_Add("Simulink", "http://en.wikipedia.org/wiki/Simulink");

            URL_Add("URL encoding", "http://en.wikipedia.org/wiki/Percent-encoding");

            URL_Add("Play Framework", "http://en.wikipedia.org/wiki/Play_Framework");

            URL_Add("iCal", "http://en.wikipedia.org/wiki/Calendar_%28application%29");

            URL_Add("SparkFun", "http://en.wikipedia.org/wiki/SparkFun_Electronics");

            URL_Add("Factor", "http://en.wikipedia.org/wiki/Factor_%28programming_language%29");

            URL_Add("PostScript", "http://en.wikipedia.org/wiki/PostScript");

            URL_Add("SHA-2", "http://en.wikipedia.org/wiki/SHA-2");

            URL_Add("CompactFlash", "http://en.wikipedia.org/wiki/CompactFlash");

            URL_Add("C++", "http://en.wikipedia.org/wiki/C%2B%2B");

            URL_Add("DevExpress", "http://www.devexpress.com/Home/Mission.xml");

            URL_Add("HDMI", "https://en.wikipedia.org/wiki/HDMI");

            URL_Add("constructor", "http://en.wikipedia.org/wiki/Constructor_%28object-oriented_programming%29");

            URL_Add("POST", "http://en.wikipedia.org/wiki/POST_%28HTTP%29");

            URL_Add("GET", "http://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol#Request_methods");

            URL_Add("CLI", "http://en.wikipedia.org/wiki/Command-line_interface");

            URL_Add("AutoIt", "http://en.wikipedia.org/wiki/AutoIt");

            URL_Add("T4", "http://en.wikipedia.org/wiki/Text_Template_Transformation_Toolkit");

            URL_Add("MyFaces", "http://en.wikipedia.org/wiki/Apache_MyFaces");

            URL_Add("XStream", "http://en.wikipedia.org/wiki/XStream");

            URL_Add("Jersey", "http://en.wikipedia.org/wiki/Java_API_for_RESTful_Web_Services#Jersey");

            URL_Add("Opera Mini", "http://en.wikipedia.org/wiki/Opera_Mini");

            URL_Add("jailbreaking", "http://en.wikipedia.org/wiki/IOS_jailbreaking");

            URL_Add("Clang", "http://en.wikipedia.org/wiki/Clang");

            URL_Add("LLVM", "http://en.wikipedia.org/wiki/LLVM");

            URL_Add("Mac&nbsp;OS&nbsp;X&nbsp;v10.8 (Mountain Lion)", "http://en.wikipedia.org/wiki/OS_X_Mountain_Lion");

            URL_Add("singleton", "http://en.wikipedia.org/wiki/Singleton_pattern");

            URL_Add("SDHC", "http://en.wikipedia.org/wiki/Secure_Digital#SDHC");

            URL_Add("instant messaging", "http://en.wikipedia.org/wiki/Instant_messaging");

            URL_Add("LMGTFY", "http://en.wikipedia.org/wiki/RTFM");

            URL_Add("ISO&nbsp;8601", "http://en.wikipedia.org/wiki/ISO_8601");

            URL_Add("WLAN", "http://en.wikipedia.org/wiki/Wireless_LAN");

            URL_Add("Raspbian", "http://en.wikipedia.org/wiki/Raspberry_Pi#Software"); //Was http://en.wikipedia.org/wiki/Raspbian

            URL_Add("Raspbmc", "http://en.wikipedia.org/wiki/List_of_software_based_on_XBMC#Raspbmc");

            URL_Add("SanDisk", "http://en.wikipedia.org/wiki/SanDisk");

            URL_Add("XBMC", "http://en.wikipedia.org/wiki/XBMC");

            URL_Add("ACE", "http://en.wikipedia.org/wiki/Adaptive_Communication_Environment");

            URL_Add("HTA", "http://en.wikipedia.org/wiki/HTML_Application");

            URL_Add("FAT16", "http://en.wikipedia.org/wiki/File_Allocation_Table#Final_FAT16");

            URL_Add("RFC", "http://en.wikipedia.org/wiki/Request_for_Comments");

            URL_Add("WinRT", "http://en.wikipedia.org/wiki/Windows_Runtime");

            URL_Add("Jira", "https://en.wikipedia.org/wiki/Jira_(software)"); // http://en.wikipedia.org/wiki/JIRA

            URL_Add("telecommunications company", "http://en.wikipedia.org/wiki/Telephone_company");

            URL_Add("Lucene.NET", "http://en.wikipedia.org/wiki/Lucene#Lucene.NET");

            URL_Add("RC", "http://en.wikipedia.org/wiki/Radio-controlled_model");

            URL_Add("XUL", "http://en.wikipedia.org/wiki/XUL");

            URL_Add("Linux Mint", "http://en.wikipedia.org/wiki/Linux_Mint");

            URL_Add("SignalR", "http://en.wikipedia.org/wiki/SignalR"); //Old: http://signalr.net/

            URL_Add("WebOS", "http://en.wikipedia.org/wiki/WebOS");

            URL_Add("LabVIEW", "http://en.wikipedia.org/wiki/LabVIEW");

            URL_Add("Pinterest", "http://en.wikipedia.org/wiki/Pinterest");

            URL_Add("algorithm", "http://en.wikipedia.org/wiki/Algorithm");

            URL_Add("NFC", "http://en.wikipedia.org/wiki/Near_field_communication");

            URL_Add("web application", "https://en.wikipedia.org/wiki/Web_application"); // http://en.wikipedia.org/wiki/Web_application

            URL_Add("Free Pascal", "http://en.wikipedia.org/wiki/Free_Pascal");

            URL_Add("LILO", "http://en.wikipedia.org/wiki/LILO_%28boot_loader%29");

            URL_Add("Solaris", "http://en.wikipedia.org/wiki/Solaris_%28operating_system%29");

            URL_Add("tar", "http://en.wikipedia.org/wiki/Tar_%28computing%29");

            URL_Add("MBR", "http://en.wikipedia.org/wiki/Master_boot_record");

            URL_Add("GParted", "http://en.wikipedia.org/wiki/GParted");

            URL_Add("iptables", "http://en.wikipedia.org/wiki/Iptables");

            URL_Add("Pentium", "http://en.wikipedia.org/wiki/Pentium");

            URL_Add("Samba", "http://en.wikipedia.org/wiki/Samba_%28software%29");

            URL_Add("operating system", "http://en.wikipedia.org/wiki/Operating_system");

            URL_Add("colocation centre", "http://en.wikipedia.org/wiki/Colocation_centre");

            URL_Add("WinDbg", "http://en.wikipedia.org/wiki/WinDbg");

            URL_Add("FTDI", "http://en.wikipedia.org/wiki/FTDI");

            URL_Add("Gforth", "http://en.wikipedia.org/wiki/Gforth");

            URL_Add("Lazarus", "http://en.wikipedia.org/wiki/Lazarus_%28IDE%29");

            URL_Add("Pascal", "http://en.wikipedia.org/wiki/Pascal_%28programming_language%29");

            URL_Add("Application Verifier", "http://msdn.microsoft.com/en-us/library/windows/desktop/dd371695%28v=vs.85%29.aspx");

            URL_Add("recommend", "http://en.wiktionary.org/wiki/recommend");

            URL_Add("PHP Development Tools", "http://en.wikipedia.org/wiki/PHP_Development_Tools");

            URL_Add("privilege", "http://en.wikipedia.org/wiki/Privilege_%28computing%29");

            URL_Add("UPnP", "http://en.wikipedia.org/wiki/Universal_Plug_and_Play");

            URL_Add("hardware", "http://en.wikipedia.org/wiki/Computer_hardware");

            URL_Add("MAC address", "http://en.wikipedia.org/wiki/MAC_address");

            URL_Add("RESTful", "http://en.wikipedia.org/wiki/Representational_state_transfer#RESTful_web_services");

            URL_Add("business intelligence", "http://en.wikipedia.org/wiki/Business_intelligence");

            URL_Add("Quora", "http://en.wikipedia.org/wiki/Quora");

            URL_Add("Evernote", "http://en.wikipedia.org/wiki/Evernote");

            URL_Add("C++Builder", "http://en.wikipedia.org/wiki/C%2B%2BBuilder");

            URL_Add("VCL", "http://en.wikipedia.org/wiki/Visual_Component_Library");

            URL_Add("Bitbucket", "http://en.wikipedia.org/wiki/Bitbucket");

            URL_Add("object-oriented programming", "http://en.wikipedia.org/wiki/Object-oriented_programming");

            URL_Add("Squeak", "http://en.wikipedia.org/wiki/Squeak");

            URL_Add("all right", "http://en.wiktionary.org/wiki/all_right");

            URL_Add("RGB", "http://en.wikipedia.org/wiki/RGB_color_model");

            URL_Add("Google Docs", "http://en.wikipedia.org/wiki/Google_Docs");

            URL_Add("infrared", "http://en.wikipedia.org/wiki/Infrared");

            URL_Add("BSD", "http://en.wikipedia.org/wiki/Berkeley_Software_Distribution");

            URL_Add("CAN bus", "http://en.wikipedia.org/wiki/CAN_bus");

            URL_Add("VBA", "http://en.wikipedia.org/wiki/Visual_Basic_for_Applications");

            URL_Add("Bay Area", "http://en.wikipedia.org/wiki/San_Francisco_Bay_Area");

            URL_Add("MOSFET", "http://en.wikipedia.org/wiki/MOSFET");

            URL_Add("W3Schools", "http://en.wikipedia.org/wiki/W3Schools");

            URL_Add("Samsung Galaxy&nbsp;S&nbsp;III", "http://en.wikipedia.org/wiki/Samsung_Galaxy_S_III");

            URL_Add("Internet&nbsp;Explorer&nbsp;9", "http://en.wikipedia.org/wiki/Internet_Explorer_9");

            URL_Add("Internet&nbsp;Explorer&nbsp;10", "http://en.wikipedia.org/wiki/Internet_Explorer_11#Internet_Explorer_10");

            URL_Add("Internet&nbsp;Explorer&nbsp;11", "http://en.wikipedia.org/wiki/Internet_Explorer_11#Internet_Explorer_11");

            URL_Add("Heroku", "http://en.wikipedia.org/wiki/Heroku");

            URL_Add("Foxit Reader", "http://en.wikipedia.org/wiki/Foxit_Reader");

            URL_Add("two's complement", "http://en.wikipedia.org/wiki/Two%27s_complement");

            URL_Add("Google Play", "http://en.wikipedia.org/wiki/Google_Play");

            URL_Add("Kinect", "http://en.wikipedia.org/wiki/Kinect");

            URL_Add("3ds&nbsp;Max", "https://en.wikipedia.org/wiki/Autodesk_3ds_Max");

            URL_Add("equivalent", "http://en.wiktionary.org/wiki/equivalent");

            URL_Add("Parallels", "http://en.wikipedia.org/wiki/Parallels_Desktop_for_Mac");

            URL_Add("excellent", "http://en.wiktionary.org/wiki/excellent#Adjective");

            URL_Add("Pygame", "http://en.wikipedia.org/wiki/Pygame");

            URL_Add("COM", "http://en.wikipedia.org/wiki/Component_Object_Model");

            URL_Add("Balsamiq", "http://en.wikipedia.org/wiki/Balsamiq");

            URL_Add("Omaha", "http://omaha.googlecode.com/svn/wiki/OmahaOverview.html");

            URL_Add("Digg", "http://en.wikipedia.org/wiki/Digg");

            URL_Add("immediately", "http://en.wiktionary.org/wiki/immediately");

            URL_Add("NSIS", "http://en.wikipedia.org/wiki/Nullsoft_Scriptable_Install_System");

            URL_Add("Inno Setup", "http://en.wikipedia.org/wiki/Inno_Setup");

            URL_Add("prerequisite", "http://en.wiktionary.org/wiki/prerequisite");

            URL_Add("RXTX", "http://jlog.org/rxtx-mac.html");

            URL_Add("third-party", "http://en.wiktionary.org/wiki/third-party#Adjective");

            URL_Add("use case", "http://en.wikipedia.org/wiki/Use_case");

            URL_Add("definition", "http://en.wiktionary.org/wiki/definition");

            URL_Add("assembly language", "http://en.wikipedia.org/wiki/Assembly_language");

            URL_Add("business-to-consumer", "http://en.wikipedia.org/wiki/Retail");

            URL_Add("QA", "http://en.wikipedia.org/wiki/Software_quality_assurance");

            URL_Add("no one", "http://en.wiktionary.org/wiki/no_one#Pronoun");

            URL_Add("GPRS", "http://en.wikipedia.org/wiki/General_Packet_Radio_Service");

            URL_Add("Sinatra", "http://en.wikipedia.org/wiki/Sinatra_%28software%29");

            URL_Add("quicksort", "http://en.wikipedia.org/wiki/Quicksort");

            URL_Add("Mage", "http://msdn.microsoft.com/en-us/library/acz3y3te%28v=vs.80%29.aspx");

            URL_Add("Wikimedia Foundation", "http://en.wikipedia.org/wiki/Wikimedia_Foundation");

            URL_Add("LOL", "http://en.wiktionary.org/wiki/LOL#Initialism");

            URL_Add("Verisign", "http://en.wikipedia.org/wiki/Verisign");

            URL_Add("CodeWarrior", "http://en.wikipedia.org/wiki/CodeWarrior");

            URL_Add("Group Policy", "http://en.wikipedia.org/wiki/Group_Policy");

            URL_Add("web service", "http://en.wikipedia.org/wiki/Web_service");

            URL_Add("libusb", "http://en.wikipedia.org/wiki/Libusb");

            URL_Add("Windows&nbsp;8", "http://en.wikipedia.org/wiki/Windows_8");

            URL_Add("User Account Control", "http://en.wikipedia.org/wiki/User_Account_Control");

            URL_Add("LoadRunner", "http://en.wikipedia.org/wiki/HP_LoadRunner");

            URL_Add("DCOM", "http://en.wikipedia.org/wiki/Distributed_Component_Object_Model");

            URL_Add("V8", "http://en.wikipedia.org/wiki/V8_%28JavaScript_engine%29");

            URL_Add("function", "http://en.wikipedia.org/wiki/Subroutine");

            URL_Add("CGI", "http://en.wikipedia.org/wiki/Common_Gateway_Interface");

            URL_Add("POSIX", "http://en.wikipedia.org/wiki/POSIX");

            URL_Add("Burn", "http://en.wikipedia.org/wiki/WiX#Burn");

            URL_Add("Bing", "http://en.wikipedia.org/wiki/Bing");

            URL_Add("motherboard", "http://en.wikipedia.org/wiki/Motherboard");

            URL_Add("domain-specific language", "http://en.wikipedia.org/wiki/Domain-specific_language");

            URL_Add("OCX", "http://en.wikipedia.org/wiki/Object_Linking_and_Embedding");

            URL_Add("Google Reader", "http://en.wikipedia.org/wiki/Google_Reader");

            URL_Add("HID", "http://en.wikipedia.org/wiki/Human_interface_device");

            URL_Add("cdecl", "http://en.wikipedia.org/wiki/X86_calling_conventions#cdecl");

            URL_Add("MacBook Air", "http://en.wikipedia.org/wiki/MacBook_Air");

            URL_Add("FileMon", "http://en.wikipedia.org/wiki/Process_Monitor#FileMon");

            URL_Add("how-to", "https://en.wikipedia.org/wiki/How-to");

            URL_Add("Tor", "http://en.wikipedia.org/wiki/Tor_%28anonymity_network%29");

            URL_Add("µs", "http://en.wikipedia.org/wiki/Microsecond");

            URL_Add("AVRDUDE", "http://www.nongnu.org/avrdude/");

            URL_Add("Group Policy Object", "http://en.wikipedia.org/wiki/Group_Policy#Operation"); //Alternative URL: http://searchwindowsserver.techtarget.com/definition/Group-Policy-Object (end of comment)

            URL_Add("network", "http://en.wikipedia.org/wiki/Telecommunications_network");

            URL_Add("PEP", "http://en.wikipedia.org/wiki/Python_Enhancement_Proposal#Development");

            URL_Add("AngularJS", "http://en.wikipedia.org/wiki/AngularJS");

            URL_Add("database administrator", "http://en.wikipedia.org/wiki/Database_administrator");

            URL_Add("especially", "http://en.wiktionary.org/wiki/especially");

            URL_Add("LibreOffice", "http://en.wikipedia.org/wiki/LibreOffice");

            URL_Add("CKEditor", "http://en.wikipedia.org/wiki/CKEditor");

            URL_Add("web page", "http://en.wikipedia.org/wiki/Web_page");

            URL_Add("MapReduce", "http://en.wikipedia.org/wiki/MapReduce");

            URL_Add("HDFS", "http://en.wikipedia.org/wiki/Apache_Hadoop#Hadoop_distributed_file_system");

            URL_Add("WebSocket", "http://en.wikipedia.org/wiki/WebSocket");

            URL_Add("of course", "https://en.wiktionary.org/wiki/of_course#Adverb");

            URL_Add("Sublime Text", "http://en.wikipedia.org/wiki/Sublime_Text");

            URL_Add("proprietary", "http://en.wiktionary.org/wiki/proprietary");

            URL_Add("Meteor", "https://en.wikipedia.org/wiki/Meteor_%28web_framework%29");

            URL_Add("Robocopy", "http://en.wikipedia.org/wiki/Robocopy");

            URL_Add("PFX", "https://en.wikipedia.org/wiki/X.509#Certificate_filename_extensions");

            URL_Add("certificate authority", "https://en.wikipedia.org/wiki/Certificate_authority");

            URL_Add("although", "http://en.wiktionary.org/wiki/although");

            URL_Add("Script.aculo.us", "http://en.wikipedia.org/wiki/Script.aculo.us");

            URL_Add("parallel", "http://en.wiktionary.org/wiki/parallel");

            URL_Add("FrontPage", "http://en.wikipedia.org/wiki/Microsoft_FrontPage");

            URL_Add("ARP", "http://en.wikipedia.org/wiki/Address_Resolution_Protocol");

            URL_Add("AdSense", "https://en.wikipedia.org/wiki/AdSense");

            URL_Add("UEFI", "http://en.wikipedia.org/wiki/Unified_Extensible_Firmware_Interface");

            URL_Add("RAII", "https://en.wikipedia.org/wiki/Resource_Acquisition_Is_Initialization");

            URL_Add("relevant", "http://en.wiktionary.org/wiki/relevent");

            URL_Add("BigQuery", "https://en.wikipedia.org/wiki/BigQuery");

            URL_Add("Google Storage", "https://en.wikipedia.org/wiki/Google_Storage");

            URL_Add("COM port", "http://en.wikipedia.org/wiki/COM_%28hardware_interface%29");

            URL_Add("NetBIOS", "https://en.wikipedia.org/wiki/NetBIOS");

            URL_Add("BeagleBone Black", "https://en.wikipedia.org/wiki/BeagleBoard#BeagleBone_Black");

            URL_Add("ambiguity", "http://en.wiktionary.org/wiki/ambiguity");

            URL_Add("dependency", "http://en.wiktionary.org/wiki/dependancy");

            URL_Add("Windows Script Host", "http://en.wikipedia.org/wiki/Windows_Script_Host");

            URL_Add("GPU", "https://en.wikipedia.org/wiki/Graphics_processing_unit");

            URL_Add("straightforward", "http://en.wiktionary.org/wiki/straightforward");

            URL_Add("artificial intelligence", "https://en.wikipedia.org/wiki/Artificial_intelligence");

            URL_Add("DVD", "http://en.wikipedia.org/wiki/DVD");

            URL_Add("CD-ROM", "http://en.wikipedia.org/wiki/CD-ROM");

            URL_Add("Mac&nbsp;OS&nbsp;X v10.9 (Mavericks)", "http://en.wikipedia.org/wiki/OS_X_Mavericks");

            URL_Add("ROM", "https://en.wikipedia.org/wiki/Read-only_memory");

            URL_Add("Microsoft Office", "http://en.wikipedia.org/wiki/Microsoft_Office");

            URL_Add("download", "http://en.wikipedia.org/wiki/Download");

            URL_Add("Ubuntu&nbsp;12.04 LTS (Precise Pangolin)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_12.04_LTS_.28Precise_Pangolin.29");

            URL_Add("Ubuntu&nbsp;12.10 (Quantal Quetzal)", "http://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_12.10_.28Quantal_Quetzal.29"); // Old: <http://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_12.10_.28Quantal_Quetzal.29>

            URL_Add("Ubuntu&nbsp;13.04 (Raring Ringtail)", "http://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_13.04_.28Raring_Ringtail.29"); // Old: <http://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_13.04_.28Raring_Ringtail.29>

            URL_Add("Microsoft Visual C++ 8.0", "http://en.wikipedia.org/wiki/Visual_C%2B%2B#32-bit_versions");

            URL_Add("Visual C++ 2010 Express", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express#Visual_C.2B.2B_Express");

            URL_Add("Visual Studio 2010 Express", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express");

            URL_Add("Visual Studio 2012 Express", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express");


            URL_Add("Visual Basic 2010 Express Edition", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express#Visual_Basic_Express");

            URL_Add("Visual Basic 2008 Express Edition", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express#Visual_Basic_Express");

            URL_Add("Visual Studio", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio");

            URL_Add("OpenCL", "https://en.wikipedia.org/wiki/OpenCL");

            URL_Add("Bitcoin", "https://en.wikipedia.org/wiki/Bitcoin");

            URL_Add("Bitcoin-Qt", "https://en.bitcoin.it/wiki/Bitcoin-Qt");

            URL_Add("Intel", "https://en.wikipedia.org/wiki/Intel");

            URL_Add("TextWrangler", "https://en.wikipedia.org/wiki/TextWrangler");

            URL_Add("SlickEdit", "https://en.wikipedia.org/wiki/SlickEdit");

            URL_Add("D3.js", "https://en.wikipedia.org/wiki/D3.js");

            URL_Add("guarantee", "http://en.wiktionary.org/wiki/guarantee");

            URL_Add("EPROM", "http://en.wikipedia.org/wiki/EPROM");

            URL_Add("Android&nbsp;4.1 (Jelly Bean)", "https://en.wikipedia.org/wiki/Android_version_history#Android_4.1_Jelly_Bean_.28API_level_16.29");

            URL_Add("Nexus&nbsp;7", "https://en.wikipedia.org/wiki/Nexus_7_%282012_version%29");

            URL_Add("object-oriented (OO)", "https://en.wikipedia.org/wiki/Object-oriented_programming");

            URL_Add("Process&nbsp;Explorer", "https://en.wikipedia.org/wiki/Process_Explorer");

            URL_Add("Google I/O", "https://en.wikipedia.org/wiki/Google_I/O");

            URL_Add("Brainfuck", "https://en.wikipedia.org/wiki/Brainfuck");

            URL_Add("operational amplifier", "http://en.wikipedia.org/wiki/Operational_amplifier");

            URL_Add("ASN.1", "https://en.wikipedia.org/wiki/Abstract_Syntax_Notation_One");

            URL_Add("Nexus&nbsp;4", "https://en.wikipedia.org/wiki/Nexus_4");

            URL_Add("PCIe", "https://en.wikipedia.org/wiki/PCI_Express");

            URL_Add("government", "https://en.wikipedia.org/wiki/Government");

            URL_Add("Ubuntu&nbsp;13.10 (Saucy Salamander)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_13.10_.28Saucy_Salamander.29"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_13.10_.28Saucy_Salamander.29>

            URL_Add("message", "https://en.wiktionary.org/wiki/message#Noun");

            URL_Add("Heartbleed", "https://en.wikipedia.org/wiki/Heartbleed");

            URL_Add("capacitor", "http://en.wikipedia.org/wiki/Capacitor");

            URL_Add("Meta&nbsp;Stack&nbsp;Exchange", "http://meta.stackexchange.com/help/whats-meta");

            URL_Add("people", "https://en.wiktionary.org/wiki/people");

            URL_Add("LOC", "http://en.wikipedia.org/wiki/Source_lines_of_code");

            URL_Add("SCART", "http://en.wikipedia.org/wiki/SCART");

            URL_Add("GUIMiner", "http://guiminer.org/");

            URL_Add("ESR", "https://en.wikipedia.org/wiki/Equivalent_series_resistance");

            URL_Add("Ctags", "http://en.wikipedia.org/wiki/Ctags");

            URL_Add("Codecademy", "https://en.wikipedia.org/wiki/Codecademy");

            URL_Add("QBasic", "https://en.wikipedia.org/wiki/QBasic");

            URL_Add("unit test", "http://en.wikipedia.org/wiki/Unit_testing");

            URL_Add("switched-mode power supply", "https://en.wikipedia.org/wiki/Switched-mode_power_supply");

            URL_Add("Swift", "https://en.wikipedia.org/wiki/Swift_%28Apple_programming_language%29");

            URL_Add("Ubuntu&nbsp;14.04 (Trusty Tahr)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_14.04_LTS_.28Trusty_Tahr.29"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_14.04_LTS_.28Trusty_Tahr.29>

            URL_Add("Ubuntu&nbsp;14.10 (Utopic Unicorn)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_14.10_.28Utopic_Unicorn.29"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_14.10_.28Utopic_Unicorn.29>

            URL_Add("NOOBS", "https://github.com/raspberrypi/noobs");

            URL_Add("SMD", "http://en.wikipedia.org/wiki/Surface-mount_technology");

            URL_Add("lambda expression", "https://en.wikipedia.org/wiki/Anonymous_function");

            URL_Add("Go", "http://en.wikipedia.org/wiki/Go_%28programming_language%29");

            URL_Add("OpenELEC", "https://en.wikipedia.org/wiki/OpenELEC");

            URL_Add("FPGA", "http://en.wikipedia.org/wiki/Field-programmable_gate_array");

            URL_Add("WhatsApp", "https://en.wikipedia.org/wiki/WhatsApp");

            URL_Add("Microsoft Word", "https://en.wikipedia.org/wiki/Microsoft_Word");

            URL_Add("Bootstrap", "https://en.wikipedia.org/wiki/Bootstrap_%28front-end_framework%29");

            URL_Add("ISO image", "http://en.wikipedia.org/wiki/ISO_image");

            URL_Add("Express.js", "https://en.wikipedia.org/wiki/Express.js");

            URL_Add("Windows Server 2012", "https://en.wikipedia.org/wiki/Windows_Server_2012");

            URL_Add("SQL Server 2012", "https://en.wikipedia.org/wiki/History_of_Microsoft_SQL_Server#SQL_Server_2012"); // https://en.wikipedia.org/wiki/Microsoft_SQL_Server#SQL_Server_2012

            URL_Add("should", "https://en.wiktionary.org/wiki/should");

            URL_Add("DOCTYPE", "https://en.wikipedia.org/wiki/Document_type_declaration");

            URL_Add("WPA2", "https://en.wikipedia.org/wiki/IEEE_802.11i-2004");

            URL_Add("English", "https://en.wiktionary.org/wiki/English#Adjective");

            URL_Add("let’s", "https://en.wiktionary.org/wiki/let%27s#Etymology"); // Old: https://en.wiktionary.org/wiki/let%27s#Contraction

            URL_Add("maintenance", "https://en.wiktionary.org/wiki/maintenance#Noun");

            URL_Add("disambiguation", "https://en.wiktionary.org/wiki/disambiguation#Noun");

            URL_Add("would", "https://en.wiktionary.org/wiki/would");

            URL_Add("Minecraft", "https://en.wikipedia.org/wiki/Minecraft");

            URL_Add("access point", "http://en.wikipedia.org/wiki/Wireless_access_point");

            URL_Add("POP3", "http://en.wikipedia.org/wiki/Post_Office_Protocol");

            URL_Add("IMAP", "http://en.wikipedia.org/wiki/Internet_Message_Access_Protocol");

            URL_Add("Ubuntu&nbsp;11.04 (Natty Narwhal)", "http://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_11.04_.28Natty_Narwhal.29"); // Old: <http://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_11.04_.28Natty_Narwhal.29>

            URL_Add("Nexus&nbsp;5", "https://en.wikipedia.org/wiki/Nexus_5");

            URL_Add("Flask", "http://en.wikipedia.org/wiki/Flask_%28web_framework%29");

            URL_Add("oscilloscope", "https://en.wikipedia.org/wiki/Oscilloscope");

            URL_Add("Cat&nbsp;5", "http://en.wikipedia.org/wiki/Category_5_cable");

            URL_Add("Cat&nbsp;6", "http://en.wikipedia.org/wiki/Category_6_cable");

            URL_Add("Ubuntu&nbsp;11.10 (Oneiric Ocelot)", "http://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_11.10_.28Oneiric_Ocelot.29"); // Old: <http://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_11.10_.28Oneiric_Ocelot.29>

            URL_Add("Visual C# Express", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express#Visual_C.23_Express");

            URL_Add("Visual Studio Express", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express");

            URL_Add("Backbone.js", "https://en.wikipedia.org/wiki/Backbone.js");

            URL_Add("Jenkins", "https://en.wikipedia.org/wiki/Jenkins_%28software%29");

            URL_Add("PyPy", "http://en.wikipedia.org/wiki/PyPy");

            URL_Add("Socket.IO", "https://en.wikipedia.org/wiki/Socket.IO");

            URL_Add("OData", "http://en.wikipedia.org/wiki/Open_Data_Protocol");

            URL_Add("ReadyBoost", "http://en.wikipedia.org/wiki/ReadyBoost");

            URL_Add("Hacker&nbsp;News", "http://en.wikipedia.org/wiki/Hacker_News");

            URL_Add("a lot", "https://en.wiktionary.org/wiki/alot#Adverb");

            URL_Add("Slashdot", "https://en.wikipedia.org/wiki/Slashdot");

            URL_Add("ML", "http://en.wikipedia.org/wiki/ML_%28programming_language%29");

            URL_Add("Rust", "https://en.wikipedia.org/wiki/Rust_%28programming_language%29");

            URL_Add("-year-old", "https://en.wiktionary.org/wiki/-year-old#Suffix");

            URL_Add("Boot-Repair", "https://help.ubuntu.com/community/Boot-Repair");

            URL_Add("independent", "https://en.wiktionary.org/wiki/independent#Adjective");

            URL_Add("Ubuntu&nbsp;10.10 (Maverick Meerkat)", "http://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_10.10_.28Maverick_Meerkat.29"); // Old: <http://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_10.10_.28Maverick_Meerkat.29>

            URL_Add("Ubuntu&nbsp;9.10 (Karmic Koala)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_9.10_.28Karmic_Koala.29"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_9.10_.28Karmic_Koala.29>

            URL_Add("Ubuntu&nbsp;10.04 LTS (Lucid Lynx)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_10.04_LTS_.28Lucid_Lynx.29"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_10.04_LTS_.28Lucid_Lynx.29>

            URL_Add("Debian&nbsp;6.0 (Squeeze)", "https://en.wikipedia.org/wiki/Debian_version_history#Debian_6.0_(Squeeze)");

            URL_Add("Debian&nbsp;7 (Wheezy)", "https://en.wikipedia.org/wiki/Debian_version_history#Debian_7_(Wheezy)");

            URL_Add("Debian&nbsp;8 (Jessie)", "https://en.wikipedia.org/wiki/Debian_version_history#Debian_8_(Jessie)");

            URL_Add("Shellshock", "https://en.wikipedia.org/wiki/Shellshock_%28software_bug%29");

            URL_Add("requisites", "https://en.wiktionary.org/wiki/requisites");

            URL_Add("DDoS", "https://en.wikipedia.org/wiki/Denial-of-service_attack");

            URL_Add("Get-ChildItem", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Get-ChildItem");

            URL_Add("Java SE", "http://en.wikipedia.org/wiki/Java_Platform,_Standard_Edition");

            URL_Add("Measure-Object", "http://technet.microsoft.com/en-us/library/hh849965.aspx");

            URL_Add("Get-Process", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Get-Process"); // Old URL: http://technet.microsoft.com/en-us/library/hh849832.aspx

            URL_Add("Select-Object", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Select-Object"); // Old URL: http://technet.microsoft.com/en-us/library/hh849895.aspx

            URL_Add("Get-Member", "http://technet.microsoft.com/en-us/library/hh849928.aspx");

            URL_Add("Where-Object", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.core/Where-Object"); //Old URL: http://technet.microsoft.com/en-us/library/hh849715.aspx

            URL_Add("Kali Linux", "https://en.wikipedia.org/wiki/Kali_Linux");

            URL_Add("UNetbootin", "http://en.wikipedia.org/wiki/UNetbootin");

            URL_Add("Windows&nbsp;8.1", "https://en.wikipedia.org/wiki/Windows_8.1");

            URL_Add("Group-Object", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Group-Object"); // Old: http://technet.microsoft.com/en-us/library/hh849907.aspx

            URL_Add("Sort-Object", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Sort-Object"); // Old: http://technet.microsoft.com/en-us/library/hh849912.aspx

            URL_Add("Pylons", "https://en.wikipedia.org/wiki/Pylons_project");

            URL_Add("New-Object", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/New-Object"); // Old: http://technet.microsoft.com/en-us/library/hh849885.aspx

            URL_Add("Get-Content", "https://docs.microsoft.com/en-gb/powershell/module/Microsoft.PowerShell.Management/Get-Content"); // Even older: http://technet.microsoft.com/en-us/library/hh849787.aspx. Old: https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.management/Get-Content

            URL_Add("Get-Help", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.core/Get-Help"); // Old: http://technet.microsoft.com/en-us/library/hh849696.aspx

            URL_Add("Remove-Item", "http://technet.microsoft.com/en-us/library/hh849765.aspx");

            URL_Add("WMI", "https://en.wikipedia.org/wiki/Windows_Management_Instrumentation");

            URL_Add("Add-Content", "http://technet.microsoft.com/en-us/library/hh849859.aspx");

            URL_Add("Get-Date", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Get-Date"); // Old URL: http://technet.microsoft.com/en-us/library/hh849887.aspx

            URL_Add("Read-Host", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Read-Host"); // Old: http://technet.microsoft.com/en-us/library/hh849945.aspx

            URL_Add("Write-Output", "https://docs.microsoft.com/en-gb/powershell/module/Microsoft.PowerShell.Utility/Write-Output"); // Old: http://technet.microsoft.com/en-us/library/hh849921.aspx https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Write-Output

            URL_Add("Write-Host", "https://docs.microsoft.com/en-gb/powershell/module/Microsoft.PowerShell.Utility/Write-Host"); // Even older: http://technet.microsoft.com/en-us/library/hh849877.aspx. Old: https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Write-Host.

            URL_Add("Get-ItemProperty", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.management/Get-ItemProperty"); //Old URL: <http://technet.microsoft.com/en-us/library/hh849851.aspx>

            URL_Add("Format-List", "http://technet.microsoft.com/en-us/library/hh849957.aspx");

            URL_Add("Format-Table", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Format-Table"); // Old: http://technet.microsoft.com/en-us/library/hh849892.aspx

            URL_Add("Out-File", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Out-File"); // Old URL: http://technet.microsoft.com/en-us/library/hh849882.aspx

            URL_Add("Get-Command", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Core/Get-Command");

            URL_Add("unfortunately", "https://en.wiktionary.org/wiki/unfortunately#Adverb");

            URL_Add("beginning", "https://en.wiktionary.org/wiki/beginning");

            URL_Add("Get-Host", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Get-Host"); // Old: http://technet.microsoft.com/en-us/library/hh849946.aspx

            URL_Add("Tee-Object", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Tee-Object");

            URL_Add("BOM", "http://en.wikipedia.org/wiki/Byte_order_mark");

            URL_Add("MPEG", "https://en.wikipedia.org/wiki/MPEG-1");

            URL_Add("RTSP", "http://en.wikipedia.org/wiki/Real_Time_Streaming_Protocol");

            URL_Add("file system", "http://en.wikipedia.org/wiki/File_system");

            URL_Add("MMC", "http://en.wikipedia.org/wiki/MultiMediaCard");

            URL_Add("Ångström Linux", "https://en.wikipedia.org/wiki/%C3%85ngstr%C3%B6m_distribution");

            URL_Add("science fiction", "http://en.wikipedia.org/wiki/Science_fiction");

            URL_Add("ping", "http://en.wikipedia.org/wiki/Ping_%28networking_utility%29");

            URL_Add("Mac&nbsp;OS&nbsp;X&nbsp;v10.2 (Jaguar)", "http://en.wikipedia.org/wiki/Mac_OS_X_v10.2");

            URL_Add("Mac&nbsp;OS&nbsp;X&nbsp;v10.5 (Leopard)", "http://en.wikipedia.org/wiki/Mac_OS_X_Leopard");

            URL_Add("DSLR", "http://en.wikipedia.org/wiki/Digital_single-lens_reflex_camera");

            URL_Add("Visual&nbsp;Studio&nbsp;2003", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Visual_Studio_.NET_2003");

            URL_Add("Visual&nbsp;Studio&nbsp;2005", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Visual_Studio_2005");

            URL_Add("Visual&nbsp;Studio&nbsp;2008", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Visual_Studio_2008");

            URL_Add("Visual&nbsp;Studio&nbsp;2010", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Visual_Studio_2010");

            URL_Add("Visual&nbsp;Studio&nbsp;2012", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Visual_Studio_2012");

            URL_Add("Visual&nbsp;Studio&nbsp;2013", "http://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Visual_Studio_2013");

            URL_Add("at the moment", "https://en.wiktionary.org/wiki/atm#Abbreviation");

            URL_Add("Ubuntu&nbsp;8.04 (Hardy Heron)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_8.04_LTS_(Hardy_Heron)"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_8.04_LTS_.28Hardy_Heron.29>

            URL_Add("Ubuntu&nbsp;9.04 (Jaunty Jackalope)", "https://en.wikipedia.org/wiki/Jaunty#Ubuntu_9.04_(Jaunty_Jackalope)");

            URL_Add("RavenDB", "http://ravendb.net/");

            URL_Add("Android&nbsp;3 (Honeycomb)", "http://en.wikipedia.org/wiki/Android_version_history#Android_3.0_Honeycomb_.28API_level_11.29");

            URL_Add("Salesforce", "http://en.wikipedia.org/wiki/Salesforce.com");

            URL_Add("Web API", "https://en.wikipedia.org/wiki/ASP.NET_MVC_Framework#Apache_License_2.0_release");

            URL_Add("duck typing", "https://en.wikipedia.org/wiki/Duck_typing");

            URL_Add("Cassini Web server", "http://support.microsoft.com/kb/893391");

            URL_Add("WDSL", "https://en.wikipedia.org/wiki/Web_Services_Description_Language");

            URL_Add("BLAS", "https://en.wikipedia.org/wiki/Basic_Linear_Algebra_Subprograms");

            URL_Add("Homebrew", "https://en.wikipedia.org/wiki/Homebrew_%28package_management_software%29");

            URL_Add("egg", "http://stackoverflow.com/questions/2051192");

            URL_Add("USART", "https://en.wikipedia.org/wiki/Universal_asynchronous_receiver/transmitter#Synchronous_transmission");

            URL_Add("ISR", "https://en.wikipedia.org/wiki/Interrupt_handler");

            URL_Add("AVR", "https://en.wikipedia.org/wiki/Atmel_AVR");

            URL_Add("SketchUp", "http://en.wikipedia.org/wiki/SketchUp");

            URL_Add("Visual C++ 2010", "https://en.wikipedia.org/wiki/Visual_C%2B%2B#32-bit_and_64-bit_versions");

            URL_Add("KML", "http://en.wikipedia.org/wiki/Keyhole_Markup_Language");

            URL_Add("Puppy Linux", "https://en.wikipedia.org/wiki/Puppy_Linux");

            URL_Add("Elasticsearch", "http://en.wikipedia.org/wiki/Elasticsearch");

            URL_Add("Invoke-Command", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Core/Invoke-Command"); // Old: https://technet.microsoft.com/en-us/library/hh849719.aspx

            URL_Add("WebRTC", "http://en.wikipedia.org/wiki/WebRTC");

            URL_Add("Puppet", "http://en.wikipedia.org/wiki/Puppet_%28software%29");

            URL_Add("Set-ExecutionPolicy", "https://technet.microsoft.com/en-us/library/hh849812.aspx");

            URL_Add("PowerShell ISE", "https://technet.microsoft.com/en-us/library/dd315244.aspx");

            URL_Add("existence", "https://en.wiktionary.org/wiki/existance");

            URL_Add("PS/2", "https://en.wikipedia.org/wiki/PS/2_port");

            URL_Add("kernel", "https://en.wikipedia.org/wiki/Kernel_%28operating_system%29");

            URL_Add("KDE", "http://en.wikipedia.org/wiki/KDE_Software_Compilation");

            URL_Add("data access layer", "https://en.wikipedia.org/wiki/Data_access_layer");

            URL_Add("Mac&nbsp;OS&nbsp;X v10.10 (Yosemite)", "https://en.wikipedia.org/wiki/OS_X_Yosemite");

            URL_Add("Scilab", "https://en.wikipedia.org/wiki/Scilab");

            URL_Add("mathematics", "https://en.wikipedia.org/wiki/Mathematics");

            URL_Add("Task Scheduler", "https://en.wikipedia.org/wiki/Windows_Task_Scheduler");

            URL_Add("Hz", "https://en.wikipedia.org/wiki/Hertz");

            URL_Add("volt", "https://en.wikipedia.org/wiki/Volt");

            URL_Add("NAudio", "http://naudio.codeplex.com/");

            URL_Add("iSCSI", "https://en.wikipedia.org/wiki/ISCSI");

            URL_Add("IOPS", "https://en.wikipedia.org/wiki/IOPS");

            URL_Add("LaunchPad", "http://www.ti.com/ww/en/launchpad/about.html");

            URL_Add("ampere", "https://en.wikipedia.org/wiki/Ampere");

            URL_Add("watt", "https://en.wikipedia.org/wiki/Watt");

            URL_Add("workaround", "https://en.wiktionary.org/wiki/workaround#Noun");

            URL_Add("Thunderbird", "https://en.wikipedia.org/wiki/Mozilla_Thunderbird");

            URL_Add("&nbsp;MB", "http://en.wikipedia.org/wiki/Megabyte");

            URL_Add("&nbsp;GB", "https://en.wikipedia.org/wiki/Gigabyte");

            URL_Add("RAID&nbsp;1", "https://en.wikipedia.org/wiki/RAID#Standard_levels");

            URL_Add("C++11", "https://en.wikipedia.org/wiki/C%2B%2B11");

            URL_Add("output", "https://en.wiktionary.org/wiki/output#Noun");

            URL_Add("BitLocker", "https://en.wikipedia.org/wiki/BitLocker");

            URL_Add("Total Commander", "https://en.wikipedia.org/wiki/Total_Commander");

            URL_Add("Common Lisp", "https://en.wikipedia.org/wiki/Common_Lisp");

            URL_Add("Prolog", "https://en.wikipedia.org/wiki/Prolog");

            URL_Add("SSE", "https://en.wikipedia.org/wiki/Streaming_SIMD_Extensions");

            URL_Add("CLOS", "https://en.wikipedia.org/wiki/Common_Lisp_Object_System");

            URL_Add("ohm", "https://en.wikipedia.org/wiki/Ohm");

            URL_Add("FXML", "https://en.wikipedia.org/wiki/FXML");

            URL_Add("ICMP", "https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol");

            URL_Add("Kickstarter", "https://en.wikipedia.org/wiki/Kickstarter");

            URL_Add("QuickTime", "https://en.wikipedia.org/wiki/QuickTime");

            URL_Add("Core&nbsp;i3", "https://en.wikipedia.org/wiki/Intel_Core#Core_i3");

            URL_Add("Core&nbsp;i7", "https://en.wikipedia.org/wiki/Intel_Core#Core_i7");

            URL_Add("Ember.js", "https://en.wikipedia.org/wiki/Ember.js");

            URL_Add("Android 5.0 (Lollipop)", "https://en.wikipedia.org/wiki/Android_Lollipop");

            URL_Add("Android 4.4.x (KitKat)", "https://en.wikipedia.org/wiki/Android_version_history#Android_4.4_KitKat_.28API_level_19.29");

            URL_Add("inheritance", "https://en.wiktionary.org/wiki/inheritance#Noun");

            URL_Add("ECMAScript", "https://en.wikipedia.org/wiki/ECMAScript");

            URL_Add("Distributed revision control", "https://en.wikipedia.org/wiki/Distributed_version_control");

            URL_Add("version control system", "https://en.wikipedia.org/wiki/Version_control"); //Old: https://en.wikipedia.org/wiki/Revision_control

            URL_Add("&nbsp;KB", "https://en.wikipedia.org/wiki/Kilobyte");

            URL_Add("Turbo C++", "https://en.wikipedia.org/wiki/Turbo_C%2B%2B");

            URL_Add("RDBMS", "https://en.wikipedia.org/wiki/Relational_database_management_system");

            URL_Add("TCP/IP", "https://en.wikipedia.org/wiki/Internet_protocol_suite");

            URL_Add("IoT", "https://en.wikipedia.org/wiki/Internet_of_things"); // Old: https://en.wikipedia.org/wiki/Internet_of_Things

            URL_Add("approximately", "https://en.wiktionary.org/wiki/approximately");

            URL_Add("CorFlags", "http://msdn.microsoft.com/en-us/library/ms164699%28v=vs.80%29.aspx");

            URL_Add("standard output", "https://en.wikipedia.org/wiki/Standard_streams#Standard_output_.28stdout.29");

            URL_Add("SOLID", "https://en.wikipedia.org/wiki/SOLID_%28object-oriented_design%29");

            URL_Add("YAGNI", "https://en.wikipedia.org/wiki/You_aren%27t_gonna_need_it");

            URL_Add("KISS", "https://en.wikipedia.org/wiki/KISS_principle#In_software_development");

            URL_Add("two cents", "https://en.wiktionary.org/wiki/two_cents#Noun");

            URL_Add("Ubuntu&nbsp;15.04 (Vivid Vervet)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_15.04_.28Vivid_Vervet.29"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_15.04_.28Vivid_Vervet.29>

            URL_Add("big-endian", "https://en.wikipedia.org/wiki/Endianness#Big-endian");

            URL_Add("little-endian", "https://en.wikipedia.org/wiki/Endianness#Little-endian");

            URL_Add("FITS", "https://en.wikipedia.org/wiki/FITS");

            URL_Add("Start-Process", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Start-Process"); // Old: https://technet.microsoft.com/en-us/library/hh849848.aspx

            URL_Add("Eternal September", "https://en.wikipedia.org/wiki/Eternal_September");

            URL_Add("MsgBox", "http://msdn.microsoft.com/en-us/library/139z2azd%28v=vs.90%29.aspx");

            URL_Add("The C Programming Language", "https://en.wikipedia.org/wiki/The_C_Programming_Language");

            URL_Add("PhD", "https://en.wikipedia.org/wiki/Doctor_of_Philosophy");

            URL_Add("together", "https://en.wiktionary.org/wiki/together#Adverb");

            URL_Add("following", "https://en.wiktionary.org/wiki/following#Noun");

            URL_Add("thought", "https://en.wiktionary.org/wiki/thought");

            URL_Add("IDL", "https://en.wikipedia.org/wiki/IDL_%28programming_language%29");

            URL_Add("K–12", "https://en.wikipedia.org/wiki/K%E2%80%9312");

            URL_Add("Get-WmiObject", "https://technet.microsoft.com/en-us/library/hh849824.aspx");

            URL_Add("Add-Member", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Add-Member"); // Old URL: https://technet.microsoft.com/en-us/library/hh849879.aspx

            URL_Add("Export-Csv", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Export-Csv"); // Old URL: https://technet.microsoft.com/en-us/library/hh849932.aspx

            URL_Add("PSU", "https://en.wikipedia.org/wiki/Power_supply_unit_%28computer%29");

            URL_Add("Visualforce", "https://en.wikipedia.org/wiki/Salesforce.com#Visualforce");

            URL_Add("Elementary&nbsp;OS", "https://en.wikipedia.org/wiki/Elementary_OS");

            URL_Add("VHDL", "https://en.wikipedia.org/wiki/VHDL");

            URL_Add("Windows&nbsp;10", "https://en.wikipedia.org/wiki/Windows_10");

            URL_Add("TypeScript", "https://en.wikipedia.org/wiki/TypeScript");

            URL_Add("OBD-II", "https://en.wikipedia.org/wiki/On-board_diagnostics#OBD-II");

            URL_Add("CUDA", "https://en.wikipedia.org/wiki/CUDA");

            URL_Add("Instagram", "https://en.wikipedia.org/wiki/Instagram");

            URL_Add("Beautiful Soup", "https://en.wikipedia.org/wiki/Beautiful_Soup");

            URL_Add("AIX", "https://en.wikipedia.org/wiki/IBM_AIX");

            URL_Add("HP-UX", "https://en.wikipedia.org/wiki/HP-UX");

            URL_Add("SunOS", "https://en.wikipedia.org/wiki/SunOS");

            URL_Add("useful", "https://en.wiktionary.org/wiki/usefull");

            URL_Add("Gprof", "https://en.wikipedia.org/wiki/Gprof");

            URL_Add("memoization", "https://en.wikipedia.org/wiki/Memoization");

            URL_Add("single-page application", "https://en.wikipedia.org/wiki/Single-page_application");

            URL_Add("German", "https://en.wiktionary.org/wiki/German#Noun"); // Old (is "A surname​."  - Wiktionary error?): https://en.wiktionary.org/wiki/German#Proper_noun

            URL_Add("F-15", "https://en.wikipedia.org/wiki/McDonnell_Douglas_F-15_Eagle");

            URL_Add("F-16", "https://en.wikipedia.org/wiki/General_Dynamics_F-16_Fighting_Falcon");

            URL_Add("Copy-Item", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Copy-Item"); // Old: https://technet.microsoft.com/en-us/library/hh849793.aspx

            URL_Add("Set-Item", "https://technet.microsoft.com/en-us/library/hh849797.aspx");

            URL_Add("Get-Item", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Get-Item"); // Old: https://technet.microsoft.com/en-us/library/hh849788.aspx

            URL_Add("please", "https://en.wiktionary.org/wiki/please#Adverb");

            URL_Add("one-liner", "https://en.wikipedia.org/wiki/One-liner_program");

            URL_Add("minimum", "https://en.wiktionary.org/wiki/minimum#Adjective");

            URL_Add("maximum", "https://en.wiktionary.org/wiki/maximum#Adjective");

            URL_Add("E-commerce", "https://en.wikipedia.org/wiki/E-commerce");

            URL_Add("ConvertFrom-Csv", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/ConvertFrom-Csv"); // Old: https://technet.microsoft.com/en-us/library/hh849890.aspx

            URL_Add("ActivePerl", "https://en.wikipedia.org/wiki/ActivePerl");

            URL_Add("set up", "https://en.wiktionary.org/wiki/set_up#Verb");

            URL_Add("run-on sentence", "https://en.wikipedia.org/wiki/Sentence_clause_structure#Run-on_(fused)_sentences");

            URL_Add("GPIO", "http://www.mosaic-industries.com/embedded-systems/microcontroller-projects/raspberry-pi/gpio-pin-electrical-specifications");

            URL_Add("Dreamweaver", "http://en.wikipedia.org/wiki/Adobe_Dreamweaver");

            URL_Add("CSS&nbsp;3", "https://en.wikipedia.org/wiki/Cascading_Style_Sheets#CSS_3");

            URL_Add("lithium polymer", "https://en.wikipedia.org/wiki/Lithium_polymer_battery");

            URL_Add("Mac&nbsp;OS&nbsp;X&nbsp;v10.4 (Tiger)", "https://en.wikipedia.org/wiki/Mac_OS_X_Tiger");

            URL_Add("permissions", "https://en.wikipedia.org/wiki/File_system_permissions");

            URL_Add("log in", "https://en.wiktionary.org/wiki/log_in#Verb");

            URL_Add("Core&nbsp;i5", "https://en.wikipedia.org/wiki/Intel_Core#Core_i5");

            URL_Add("Orca", "http://msdn.microsoft.com/en-us/library/aa370557(v=vs.85).aspx");

            URL_Add("pseudocode", "https://en.wikipedia.org/wiki/Pseudocode");

            URL_Add("LDO", "https://en.wikipedia.org/wiki/Low-dropout_regulator");

            URL_Add("Android&nbsp;4.0 (Ice Cream Sandwich)", "https://en.wikipedia.org/wiki/Android_Ice_Cream_Sandwich");

            URL_Add("T-shirt", "https://en.wiktionary.org/wiki/T-shirt#Noun");

            URL_Add("French", "https://en.wiktionary.org/wiki/French#Adjective");

            URL_Add("parenthesis", "https://en.wiktionary.org/wiki/parenthesis#Noun");

            URL_Add("OSMC", "https://en.wikipedia.org/wiki/List_of_software_based_on_Kodi_and_XBMC#OSMC_.28formerly_Raspbmc.29");

            URL_Add("black hole", "https://en.wikipedia.org/wiki/Black_hole");

            URL_Add("Schottky", "https://en.wikipedia.org/wiki/Schottky_diode");

            URL_Add("doesn’t", "https://en.wiktionary.org/wiki/doesnt#Verb"); // Old: https://en.wiktionary.org/wiki/doesnt

            URL_Add("smartphone", "https://en.wikipedia.org/wiki/Smartphone");

            URL_Add("grammar", "https://en.wiktionary.org/wiki/grammer");

            URL_Add("Chinese", "https://en.wiktionary.org/wiki/Chinese#Adjective");

            URL_Add("at least", "https://en.wiktionary.org/wiki/at_least");

            URL_Add("frequency", "https://en.wikipedia.org/wiki/Frequency");

            URL_Add("basically", "https://en.wiktionary.org/wiki/basicly");

            URL_Add("MDI", "https://en.wikipedia.org/wiki/Multiple_document_interface");

            URL_Add("Chocolatey", "https://en.wikipedia.org/wiki/NuGet#Chocolatey"); // Old: https://en.wikipedia.org/wiki/Chocolatey

            URL_Add("Visual C# 2008 Express Edition", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express#Visual_C.23_Express");

            URL_Add("PSObject", "https://msdn.microsoft.com/en-us/library/system.management.automation.psobject%28v=vs.85%29.aspx");

            URL_Add("Get-Variable", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Get-Variable"); // Old: https://technet.microsoft.com/en-us/library/hh849899.aspx

            URL_Add("Germany", "https://en.wiktionary.org/wiki/Germany#Proper_noun");

            URL_Add("single responsibility principle", "https://en.wikipedia.org/wiki/Single_responsibility_principle");

            URL_Add("PCB", "https://en.wikipedia.org/wiki/Printed_circuit_board");

            URL_Add("British", "https://en.wiktionary.org/wiki/British#Adjective");

            URL_Add("autumn", "https://en.wiktionary.org/wiki/autumn#Noun");

            URL_Add("accommodate", "https://en.wiktionary.org/wiki/accomodate#Verb");

            URL_Add("obviously", "https://en.wiktionary.org/wiki/obviously#Adverb");

            URL_Add("VisualSVN", "https://en.wikipedia.org/wiki/VisualSVN");

            URL_Add("Trello", "https://en.wikipedia.org/wiki/Trello");

            URL_Add("C99", "https://en.wikipedia.org/wiki/C99");

            URL_Add("Logo", "https://en.wikipedia.org/wiki/Logo_%28programming_language%29");

            URL_Add("ALGOL", "https://en.wikipedia.org/wiki/ALGOL");

            URL_Add("whatsoever", "https://en.wiktionary.org/wiki/whatsoever#Adjective");

            URL_Add("DMZ", "https://en.wikipedia.org/wiki/DMZ_%28computing%29");

            URL_Add("didn’t", "https://en.wiktionary.org/wiki/didn't#Contraction");

            URL_Add("that's", "https://en.wiktionary.org/wiki/that's#Contraction");

            URL_Add("vs.", "https://en.wiktionary.org/wiki/vs.#Preposition");

            URL_Add("wherever", "https://en.wiktionary.org/wiki/wherever#Adverb");

            URL_Add("what's", "https://en.wiktionary.org/wiki/what's#Contraction");

            URL_Add("American", "https://en.wiktionary.org/wiki/American#Adjective");

            URL_Add("Indian", "https://en.wiktionary.org/wiki/Indian#Adjective");

            URL_Add("graduate", "https://en.wiktionary.org/wiki/graduate#Noun");

            URL_Add("Resolve-Path", "https://technet.microsoft.com/en-us/library/hh849858.aspx");

            URL_Add("Import-Csv", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Import-Csv"); // Old: https://technet.microsoft.com/en-us/library/hh849891.aspx

            URL_Add("ForEach-Object", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.core/ForEach-Object"); // Old: <https://technet.microsoft.com/en-us/library/hh849731.aspx>

            URL_Add("Get-ADUser", "https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/ee617241(v=technet.10)"); // Old: https://technet.microsoft.com/en-us/library/ee617241.aspx

            URL_Add("Select-String", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Select-String"); // Even older: https://technet.microsoft.com/en-us/library/hh849903.aspx. Old: https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Select-String

            URL_Add("Set-StrictMode", "https://technet.microsoft.com/en-us/library/hh849692.aspx");

            URL_Add("Clear-Host", "https://technet.microsoft.com/en-us/library/hh852689.aspx");

            URL_Add("Exit", "https://technet.microsoft.com/en-us/library/hh847744.aspx");

            URL_Add("Join-Path", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.management/Join-Path"); // Old: https://technet.microsoft.com/en-us/library/hh849799.aspx

            URL_Add("New-Item", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/New-Item"); // Old: https://technet.microsoft.com/en-us/library/hh849795.aspx

            URL_Add("Out-Null", "https://technet.microsoft.com/en-us/library/hh849716.aspx");

            URL_Add("React", "https://en.wikipedia.org/wiki/React_%28JavaScript_library%29");

            URL_Add("tantalum capacitor", "https://en.wikipedia.org/wiki/Tantalum_capacitor");

            URL_Add("cross-site request forgery", "https://en.wikipedia.org/wiki/Cross-site_request_forgery");

            URL_Add("knowledge", "https://en.wiktionary.org/wiki/knowledge#Noun");

            URL_Add("necessarily", "https://en.wiktionary.org/wiki/necessarily#Adverb");

            URL_Add("continuous delivery", "https://en.wikipedia.org/wiki/Continuous_delivery");

            URL_Add("TopCoder", "https://en.wikipedia.org/wiki/TopCoder");

            URL_Add("Codeforces", "https://en.wikipedia.org/wiki/Codeforces");

            URL_Add("CodeChef", "https://en.wikipedia.org/wiki/CodeChef");

            URL_Add("relate", "https://en.wiktionary.org/wiki/relate#Verb");

            URL_Add("Get-ADGroupMember", "https://technet.microsoft.com/en-us/library/ee617193.aspx");

            URL_Add("FireMonkey", "https://en.wikipedia.org/wiki/FireMonkey");

            URL_Add("Berkeley", "https://en.wikipedia.org/wiki/Berkeley,_California");

            URL_Add("Berkeley&nbsp;DB", "https://en.wikipedia.org/wiki/Berkeley_DB");

            URL_Add("beginner", "https://en.wiktionary.org/wiki/beginner#Noun");

            URL_Add("Verilog", "https://en.wikipedia.org/wiki/Verilog");

            URL_Add("isn't", "https://en.wiktionary.org/wiki/isn't#Verb"); // Old: https://en.wiktionary.org/wiki/isn't#Contraction

            URL_Add("IronRuby", "https://en.wikipedia.org/wiki/IronRuby");

            URL_Add("newbie", "https://en.wiktionary.org/wiki/newbie#Noun");

            URL_Add("Mac&nbsp;OS&nbsp;X v10.11 (El Capitan)", "https://en.wikipedia.org/wiki/OS_X_El_Capitan");

            URL_Add("FoxPro", "https://en.wikipedia.org/wiki/Visual_FoxPro");

            URL_Add("OpenCart", "https://en.wikipedia.org/wiki/OpenCart");

            URL_Add("Windows&nbsp;95", "https://en.wikipedia.org/wiki/Windows_95");

            URL_Add("Vivaldi", "https://en.wikipedia.org/wiki/Vivaldi_%28web_browser%29");

            URL_Add("someone", "https://en.wiktionary.org/wiki/someone#Pronoun");

            URL_Add("Visual&nbsp;Studio&nbsp;2015", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Visual_Studio_2015");

            URL_Add("two-factor authentication", "https://en.wikipedia.org/wiki/Two-factor_authentication");

            URL_Add("PLA", "https://en.wikipedia.org/wiki/Polylactic_acid");

            URL_Add("ABS", "https://en.wikipedia.org/wiki/Acrylonitrile_butadiene_styrene");

            URL_Add("Ubuntu&nbsp;16.04 (Xenial Xerus)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_16.04_LTS_.28Xenial_Xerus.29");

            URL_Add("Less", "https://en.wikipedia.org/wiki/Less_%28stylesheet_language%29");

            URL_Add("Android&nbsp;2.2 (Froyo)", "https://en.wikipedia.org/wiki/Android_Froyo");

            URL_Add("any more", "https://en.wiktionary.org/wiki/any_more#Adverb");

            URL_Add("NGen", "https://en.wikipedia.org/wiki/Native_Image_Generator");

            URL_Add("OpenOCD", "https://en.wikipedia.org/wiki/List_of_ARM_Cortex-M_development_tools#Debugging_tools");

            URL_Add("microprocessor", "https://en.wikipedia.org/wiki/Microprocessor");

            URL_Add("Invoke-WebRequest", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Invoke-WebRequest"); //Old URL: https://technet.microsoft.com/en-us/library/hh849901.aspx

            URL_Add("KiCad", "https://en.wikipedia.org/wiki/KiCad");

            URL_Add("couldn't", "https://en.wiktionary.org/wiki/couldn't#Contraction");

            URL_Add("nowhere", "https://en.wiktionary.org/wiki/nowhere#Adverb");

            URL_Add("available", "https://en.wiktionary.org/wiki/available#Adjective");

            URL_Add("cPanel", "https://en.wikipedia.org/wiki/CPanel");

            URL_Add("Plesk", "https://en.wikipedia.org/wiki/Plesk");

            URL_Add("bear", "https://en.wiktionary.org/wiki/bear_with#Verb");

            URL_Add("Gradle", "https://en.wikipedia.org/wiki/Gradle");

            URL_Add("stand-alone", "https://en.wiktionary.org/wiki/stand-alone#Adjective");

            URL_Add("Silicon Valley", "https://en.wikipedia.org/wiki/Silicon_Valley");

            URL_Add("nobody", "https://en.wiktionary.org/wiki/nobody#Pronoun");

            URL_Add("Airbnb", "https://en.wikipedia.org/wiki/Airbnb");

            URL_Add("FreeBSD", "https://en.wikipedia.org/wiki/FreeBSD");

            URL_Add("Nautilus", "https://en.wikipedia.org/wiki/GNOME_Files");

            URL_Add("Ubuntu&nbsp;15.10 (Wily Werewolf)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_15.10_.28Wily_Werewolf.29"); // Old: <https://en.wikipedia.org/wiki/List_of_Ubuntu_releases#Ubuntu_15.10_.28Wily_Werewolf.29>

            URL_Add("WinSCP", "https://en.wikipedia.org/wiki/WinSCP");

            URL_Add("Google Groups", "https://en.wikipedia.org/wiki/Google_Groups");

            URL_Add("Pandas", "https://en.wikipedia.org/wiki/Pandas_%28software%29");

            URL_Add("man-in-the-middle attack", "https://en.wikipedia.org/wiki/Man-in-the-middle_attack");

            URL_Add("Adobe Illustrator", "https://en.wikipedia.org/wiki/Adobe_Illustrator");

            URL_Add("x-axis", "https://en.wiktionary.org/wiki/x-axis#Noun");

            URL_Add("Jupyter", "https://en.wikipedia.org/wiki/IPython#Project_Jupyter");

            URL_Add("Zener diode", "https://en.wikipedia.org/wiki/Zener_diode");

            URL_Add("AMD", "https://en.wikipedia.org/wiki/Advanced_Micro_Devices");

            URL_Add("MVVM", "https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel");

            URL_Add("null pointer exception", "https://en.wikipedia.org/wiki/Null_pointer#Dereferencing");

            URL_Add("IPython Notebook", "https://en.wikipedia.org/wiki/IPython#Notebook");

            URL_Add("Edge", "https://en.wikipedia.org/wiki/Microsoft_Edge");

            URL_Add("transistor", "https://en.wikipedia.org/wiki/Transistor");

            URL_Add("Android Studio", "https://en.wikipedia.org/wiki/Android_Studio");

            URL_Add("Western Digital", "https://en.wikipedia.org/wiki/Western_Digital");

            URL_Add("upvote", "https://en.wiktionary.org/wiki/upvote");

            URL_Add("downvote", "https://en.wiktionary.org/wiki/downvote");

            URL_Add("every time", "https://en.wiktionary.org/wiki/every_time#Adverb");

            URL_Add("RequireJS", "http://www.requirejs.org/");

            URL_Add("Mongoose", "https://en.wikipedia.org/wiki/Mongoose_%28web_server%29");

            URL_Add("principle", "https://en.wiktionary.org/wiki/principle#Noun");

            URL_Add("ETL", "https://en.wikipedia.org/wiki/Extract,_transform,_load");

            URL_Add("comparison", "https://en.wiktionary.org/wiki/comparison#Noun");

            URL_Add("question", "https://en.wiktionary.org/wiki/question#Noun");

            URL_Add("similar", "https://en.wiktionary.org/wiki/similiar");

            URL_Add("C++14", "https://en.wikipedia.org/wiki/C%2B%2B14");

            URL_Add("automatically", "https://en.wiktionary.org/wiki/automatically#Adverb");

            URL_Add("compatible", "https://en.wiktionary.org/wiki/compatable");

            URL_Add("thoroughly", "https://en.wiktionary.org/wiki/thoroughly#Adverb");

            URL_Add("Z80", "https://en.wikipedia.org/wiki/Zilog_Z80");

            URL_Add("Li-ion", "https://en.wikipedia.org/wiki/Lithium-ion_battery");

            URL_Add("NiCad", "https://en.wikipedia.org/wiki/Nickel%E2%80%93cadmium_battery");

            URL_Add("Danish", "https://en.wiktionary.org/wiki/Danish#Adjective");

            URL_Add("dd", "https://en.wikipedia.org/wiki/Dd_%28Unix%29");

            URL_Add("necessary", "https://en.wiktionary.org/wiki/necessary#Adjective");

            URL_Add("bottleneck", "https://en.wiktionary.org/wiki/bottleneck#Noun");

            URL_Add("WordPerfect", "https://en.wikipedia.org/wiki/WordPerfect");

            URL_Add("screenshot", "https://en.wiktionary.org/wiki/screenshot#Noun");

            URL_Add("receiving", "https://en.wiktionary.org/wiki/receiving#Verb");

            URL_Add("World War II", "https://en.wikipedia.org/wiki/World_War_II");


            // URL_Add("", "");
            // URL_Add("", "");
            // URL_Add("", "");
            // URL_Add("", "");
            // URL_Add("", "");

            URL_Add("whereas", "https://en.wiktionary.org/wiki/whereas#Adverb");

            URL_Add("everything", "https://en.wiktionary.org/wiki/everything#Pronoun");

            URL_Add("continuously", "https://en.wiktionary.org/wiki/continuously#Adverb");

            URL_Add("SOA", "https://en.wikipedia.org/wiki/Service-oriented_architecture");

            URL_Add("PhpStorm", "https://en.wikipedia.org/wiki/PhpStorm");

            URL_Add("performance", "https://en.wiktionary.org/wiki/performance#English");

            URL_Add("eSATA", "https://en.wikipedia.org/wiki/Serial_ATA#eSATA");

            URL_Add("Slack", "https://en.wikipedia.org/wiki/Slack_(software)");

            URL_Add("Docker", "https://en.wikipedia.org/wiki/Docker_%28software%29");

            URL_Add("Android&nbsp;4.3 (Jelly Bean)", "https://en.wikipedia.org/wiki/Android_Jelly_Bean");

            URL_Add("Scrapy", "https://en.wikipedia.org/wiki/Scrapy");

            URL_Add("occurring", "https://en.wiktionary.org/wiki/occurring#Verb");

            URL_Add("OpenStack", "https://en.wikipedia.org/wiki/OpenStack");

            URL_Add("built-in", "https://en.wiktionary.org/wiki/built-in#Adjective");

            URL_Add("disappear", "https://en.wiktionary.org/wiki/disappear#Verb");

            URL_Add("hierarchy", "https://en.wiktionary.org/wiki/hierarchy#Noun");

            URL_Add("Netduino", "https://en.wikipedia.org/wiki/Netduino");

            URL_Add("Android 6.0 (Marshmallow)", "https://en.wikipedia.org/wiki/Android_Marshmallow");

            URL_Add("machine learning", "https://en.wikipedia.org/wiki/Machine_learning");

            URL_Add("Candle", "https://en.wikipedia.org/wiki/WiX#Candle");

            URL_Add("Ubuntu&nbsp;16.10 (Yakkety Yak)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_16.10_.28Yakkety_Yak.29");

            URL_Add("Ubuntu&nbsp;17.04 (Zesty Zapus)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_17.04_.28Zesty_Zapus.29");

            URL_Add("Eclipse", "http://en.wikipedia.org/wiki/Eclipse_%28software%29");

            URL_Add("Eclipse&nbsp;v3.7 (Indigo)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add("Eclipse&nbsp;v4.2 (Juno)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add("Eclipse&nbsp;v4.3 (Kepler)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add("Eclipse&nbsp;v4.4 (Luna)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add("Eclipse&nbsp;v4.5 (Mars)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add("Eclipse&nbsp;v4.6 (Neon)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add(".NET Compact Framework", "https://en.wikipedia.org/wiki/.NET_Compact_Framework");

            URL_Add("Visual Basic 2005 Express Edition", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio_Express#Visual_Basic_Express");

            URL_Add("LLDB", "https://en.wikipedia.org/wiki/LLDB_(debugger)");

            URL_Add("Xamarin", "https://en.wikipedia.org/wiki/Xamarin");

            URL_Add("your mileage may vary", "https://en.wiktionary.org/wiki/YMMV");

            URL_Add("ASP.NET Core", "https://en.wikipedia.org/wiki/ASP.NET_Core");

            URL_Add("VxWorks", "https://en.wikipedia.org/wiki/VxWorks");

            URL_Add("moreover", "https://en.wiktionary.org/wiki/moreover#Adverb");

            URL_Add("ELF", "https://en.wikipedia.org/wiki/Executable_and_Linkable_Format");

            URL_Add("whenever", "https://en.wiktionary.org/wiki/whenever#Conjunction");

            URL_Add("kilohm", "https://en.wikipedia.org/wiki/Ohm#Definition");

            URL_Add("megohm", "https://en.wikipedia.org/wiki/Ohm#Definition");

            URL_Add("µF", "https://en.wikipedia.org/wiki/Farad#Definition");

            URL_Add("appearance", "https://en.wiktionary.org/wiki/appearance#Noun");

            URL_Add("dynamically", "https://en.wiktionary.org/wiki/dynamically#Adverb");

            URL_Add("different", "https://en.wiktionary.org/wiki/different#Adjective");

            URL_Add("beyond", "https://en.wiktionary.org/wiki/beyond#Preposition");

            URL_Add("Russian", "https://en.wiktionary.org/wiki/Russian#Adjective");

            URL_Add("anyone", "https://en.wiktionary.org/wiki/anyone#Pronoun");

            URL_Add("Eclipse&nbsp;v3.5 (Galileo)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add("receive", "https://en.wiktionary.org/wiki/receive#Verb");

            URL_Add("always", "https://en.wiktionary.org/wiki/allways");

            URL_Add("GitLab", "https://en.wikipedia.org/wiki/GitLab");

            URL_Add("Spanish", "https://en.wiktionary.org/wiki/Spanish");

            URL_Add("macOS v10.12 (Sierra)", "https://en.wikipedia.org/wiki/MacOS_Sierra");

            URL_Add("Castle Windsor", "https://en.wikipedia.org/wiki/Castle_Project#Features"); // Alternative URL: <http://www.castleproject.org/container/index.html>

            URL_Add("IIS&nbsp;Express", "https://en.wikipedia.org/wiki/Internet_Information_Services#IIS_Express");

            URL_Add("referring", "https://en.wiktionary.org/wiki/refering");

            URL_Add("DynamoDB", "https://en.wikipedia.org/wiki/Amazon_DynamoDB");

            URL_Add("Pythonic", "https://en.wiktionary.org/wiki/Pythonic#Adjective");

            URL_Add("separation", "https://en.wiktionary.org/wiki/seperation");

            URL_Add("pull-up resistor", "https://en.wikipedia.org/wiki/Pull-up_resistor");

            URL_Add("SNMP", "https://en.wikipedia.org/wiki/Simple_Network_Management_Protocol");

            URL_Add("AutoCAD", "https://en.wikipedia.org/wiki/AutoCAD");

            URL_Add("STM32", "https://en.wikipedia.org/wiki/STM32");

            URL_Add("Authenticode", "https://msdn.microsoft.com/en-us/library/ms537359(v=vs.85).aspx");

            URL_Add("Gulp.js", "https://en.wikipedia.org/wiki/Gulp.js");

            URL_Add("Intel HEX", "https://en.wikipedia.org/wiki/Intel_HEX");

            URL_Add("WinAVR", "https://sourceforge.net/projects/winavr/");

            URL_Add("model–view–controller", "https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller");

            URL_Add("model–view–presenter", "https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93presenter");

            URL_Add("PhantomJS", "https://en.wikipedia.org/wiki/PhantomJS");

            URL_Add("CSRF", "https://en.wikipedia.org/wiki/Cross-site_request_forgery");

            URL_Add("potentiometer", "https://en.wikipedia.org/wiki/Potentiometer");

            URL_Add("persistence", "https://en.wiktionary.org/wiki/persistence#Noun");

            URL_Add("unnecessary", "https://en.wiktionary.org/wiki/unnecessary#Adjective");

            URL_Add("programmatically", "https://en.wiktionary.org/wiki/programmatically#Adverb");

            URL_Add("hexadecimal", "https://en.wikipedia.org/wiki/Hexadecimal");

            URL_Add("neighbourhood", "https://en.wiktionary.org/wiki/neighbourhood");

            URL_Add("shouldn't", "https://en.wiktionary.org/wiki/shouldn%27t#Contraction");

            URL_Add("functional programming", "https://en.wikipedia.org/wiki/Functional_programming");

            URL_Add("Google Drive", "https://en.wikipedia.org/wiki/Google_Drive");

            URL_Add(".NET Core", "https://en.wikipedia.org/wiki/.NET_Core"); // Old: https://en.wikipedia.org/wiki/.NET_Framework#.NET_Core

            URL_Add("Roslyn", "https://en.wikipedia.org/wiki/.NET_Compiler_Platform");

            URL_Add("Dr. Watson", "https://en.wikipedia.org/wiki/Dr._Watson_(debugger)");

            URL_Add("ECMAScript&nbsp;6", "https://en.wikipedia.org/wiki/ECMAScript#6th_Edition_-_ECMAScript_2015");

            URL_Add("Redux", "https://en.wikipedia.org/wiki/Redux_(JavaScript_library)");

            URL_Add("Firebase", "https://en.wikipedia.org/wiki/Firebase");

            URL_Add("example", "https://en.wiktionary.org/wiki/example#Noun");

            URL_Add("European", "https://en.wiktionary.org/wiki/European#Adjective");

            URL_Add("Europe", "https://en.wiktionary.org/wiki/Europe#Proper_noun");

            URL_Add("Altap Salamander", "https://en.wikipedia.org/wiki/Altap_Salamander");

            URL_Add("baud rate", "https://en.wikipedia.org/wiki/Symbol_rate");

            URL_Add("GPIB", "https://en.wikipedia.org/wiki/IEEE-488");

            URL_Add("Tektronix", "https://en.wikipedia.org/wiki/Tektronix");

            URL_Add("furthermore", "https://en.wiktionary.org/wiki/furthermore#Adverb");

            URL_Add("Get-WinEvent", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.diagnostics/Get-WinEvent");

            URL_Add("Get-EventLog", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Get-EventLog"); // Old URL: https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.management/Get-EventLog

            URL_Add("Compare-Object", "https://msdn.microsoft.com/en-us/powershell/reference/5.1/microsoft.powershell.utility/compare-object");

            URL_Add("Move-Item", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.management/Move-Item");

            URL_Add("Arabic", "https://en.wiktionary.org/wiki/Arabic#Adjective");

            URL_Add("Roman", "https://en.wiktionary.org/wiki/Roman#Adjective");

            URL_Add("Get-Location", "https://msdn.microsoft.com/en-us/powershell/reference/5.1/microsoft.powershell.management/get-location");

            URL_Add("Rename-Item", "https://msdn.microsoft.com/en-us/powershell/reference/3.0/microsoft.powershell.management/rename-item");

            URL_Add("TensorFlow", "https://en.wikipedia.org/wiki/TensorFlow");

            URL_Add("retrieve", "https://en.wiktionary.org/wiki/retrieve");

            URL_Add("resource", "https://en.wiktionary.org/wiki/resource#Noun");

            URL_Add("FreeRTOS", "https://en.wikipedia.org/wiki/FreeRTOS");

            URL_Add("Cortex-M", "https://en.wikipedia.org/wiki/ARM_Cortex-M");

            URL_Add("seems", "https://en.wiktionary.org/wiki/seems#Verb");

            URL_Add("Visual&nbsp;Studio&nbsp;2017", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio#2017");

            URL_Add("F-35", "https://en.wikipedia.org/wiki/Lockheed_Martin_F-35_Lightning_II");

            URL_Add("Set-Alias", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Set-Alias");

            URL_Add("Get-Alias", "https://msdn.microsoft.com/powershell/reference/5.1/microsoft.powershell.utility/Get-Alias");

            URL_Add("Vue.js", "https://en.wikipedia.org/wiki/Vue.js");

            URL_Add("farad", "https://en.wikipedia.org/wiki/Farad");

            URL_Add("coulomb", "https://en.wikipedia.org/wiki/Coulomb");

            URL_Add("Sybase", "https://en.wikipedia.org/wiki/Sybase");

            URL_Add("thanks", "https://en.wiktionary.org/wiki/thanks#Noun");

            URL_Add("vulnerability", "https://en.wiktionary.org/wiki/vulnerability#Noun");

            URL_Add("colleague", "https://en.wiktionary.org/wiki/colleague#Noun");

            URL_Add("background", "https://en.wiktionary.org/wiki/background#Noun");

            URL_Add("Kotlin", "https://en.wikipedia.org/wiki/Kotlin_(programming_language)");

            URL_Add("aren't", "https://en.wiktionary.org/wiki/aren%27t#Contraction");

            URL_Add("email", "https://en.wiktionary.org/wiki/email#Noun");

            URL_Add("altogether", "https://en.wiktionary.org/wiki/altogether#Adverb");

            URL_Add("Middle East", "https://en.wiktionary.org/wiki/Middle_East#Proper_noun");

            URL_Add("whatever", "https://en.wiktionary.org/wiki/whatever#Determiner");

            URL_Add("PyDev", "https://en.wikipedia.org/wiki/PyDev");

            URL_Add("PyCharm", "https://en.wikipedia.org/wiki/PyCharm");

            URL_Add("behavior-driven development", "https://en.wikipedia.org/wiki/Behavior-driven_development");

            URL_Add("apparently", "https://en.wiktionary.org/wiki/apparently#Adverb");

            URL_Add("Digi-Key", "https://en.wikipedia.org/wiki/Digi-Key");

            URL_Add("RS-485", "https://en.wikipedia.org/wiki/RS-485");

            URL_Add("sometimes", "https://en.wiktionary.org/wiki/sometimes#Adverb");

            URL_Add("PCI", "https://en.wikipedia.org/wiki/Conventional_PCI");

            URL_Add("you're", "https://en.wiktionary.org/wiki/you're#Contraction");

            URL_Add("Umbraco", "https://en.wikipedia.org/wiki/Umbraco");

            URL_Add("Leonardo", "https://store.arduino.cc/arduino-leonardo-with-headers");

            URL_Add("Get-Service", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/get-service");

            URL_Add("Set-Service", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/set-service");

            URL_Add("ATtiny85", "http://www.microchip.com/wwwproducts/en/ATtiny85");

            URL_Add("Fritzing", "https://en.wikipedia.org/wiki/Fritzing");

            URL_Add("communication", "https://en.wiktionary.org/wiki/communication#Noun");

            URL_Add("Altera", "https://en.wikipedia.org/wiki/Altera");

            URL_Add("Xilinx", "https://en.wikipedia.org/wiki/Xilinx");

            URL_Add("Quartus", "https://en.wikipedia.org/wiki/Altera_Quartus");

            URL_Add("OpenSCAD", "https://en.wikipedia.org/wiki/OpenSCAD");

            URL_Add("Japanese", "https://en.wiktionary.org/wiki/Japanese#Adjective");

            URL_Add("C++17", "https://en.wikipedia.org/wiki/C%2B%2B17");

            URL_Add("embarrassed", "https://en.wiktionary.org/wiki/embarrassed#Adjective");

            URL_Add("ridiculous", "https://en.wiktionary.org/wiki/ridiculous#Adjective");

            URL_Add("actually", "https://en.wiktionary.org/wiki/actually#Adverb");

            URL_Add("PUT", "https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol#Request_methods");

            URL_Add("DELETE", "https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol#Request_methods");

            URL_Add("feasible", "https://en.wiktionary.org/wiki/feasible#Adjective");

            URL_Add("SQL Server 2016", "https://en.wikipedia.org/wiki/History_of_Microsoft_SQL_Server#SQL_Server_2016");

            URL_Add("Schrödinger equation", "https://en.wikipedia.org/wiki/Schr%C3%B6dinger_equation");

            URL_Add("hasn't", "https://en.wiktionary.org/wiki/hasn%27t#Verb");

            URL_Add("maintained", "https://en.wiktionary.org/wiki/maintained#Verb");

            URL_Add("Visual Studio Code", "https://en.wikipedia.org/wiki/Visual_Studio_Code");

            URL_Add("WebStorm", "https://en.wikipedia.org/wiki/JetBrains#WebStorm");

            URL_Add("is going to", "https://en.wiktionary.org/wiki/going_to#English");

            URL_Add("introduction", "https://en.wiktionary.org/wiki/introduction#Noun");

            URL_Add("whose", "https://en.wiktionary.org/wiki/who%27s#Contraction");

            URL_Add("libraries", "https://en.wiktionary.org/wiki/libraries#Noun");

            URL_Add("WebGL", "https://en.wikipedia.org/wiki/WebGL");

            URL_Add("accidentally", "https://en.wiktionary.org/wiki/accidentally#Adverb");

            URL_Add("have got to", "https://en.wiktionary.org/wiki/gotta#Contraction");

            URL_Add("Three.js", "https://en.wikipedia.org/wiki/Three.js");

            URL_Add("zero-day", "https://en.wikipedia.org/wiki/Zero-day_(computing)");

            URL_Add("macOS v10.13 (High Sierra)", "https://en.wikipedia.org/wiki/MacOS_High_Sierra");

            URL_Add("Perl&nbsp;6", "https://en.wikipedia.org/wiki/Perl_6");

            URL_Add("parentheses", "https://en.wiktionary.org/wiki/parentheses#Noun");

            URL_Add("utilities", "https://en.wiktionary.org/wiki/utilities#English");

            URL_Add("wouldn't", "https://en.wiktionary.org/wiki/wouldn%27t#Contraction");

            URL_Add("implementation", "https://en.wiktionary.org/wiki/implementation#Noun");

            URL_Add("explanation", "https://en.wiktionary.org/wiki/explanation#Noun");

            URL_Add("similary", "https://en.wiktionary.org/wiki/similary#Adjective");

            URL_Add("installation", "https://en.wiktionary.org/wiki/installation#Noun");

            URL_Add("Write-Debug", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-debug");

            URL_Add("OpenBSD", "https://en.wikipedia.org/wiki/OpenBSD");

            URL_Add("theoretically", "https://en.wiktionary.org/wiki/theoretically#Adverb");

            URL_Add("DebugView", "https://docs.microsoft.com/en-us/sysinternals/downloads/debugview");

            URL_Add("OllyDbg", "https://en.wikipedia.org/wiki/OllyDbg");

            URL_Add("Set-PSDebug", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Core/Set-PSDebug");

            URL_Add("domain-driven design", "https://en.wikipedia.org/wiki/Domain-driven_design");

            URL_Add("Arduino Mega", "https://www.arduino.cc/en/Main/ArduinoBoardMega");

            URL_Add("MIPS", "https://en.wikipedia.org/wiki/MIPS_architecture");

            URL_Add("want to", "https://en.wiktionary.org/wiki/wanna#Contraction_2");

            URL_Add("Laravel", "https://en.wikipedia.org/wiki/Laravel");

            URL_Add("wasn't", "https://en.wiktionary.org/wiki/wasn%27t#Verb");

            URL_Add("TortoiseHg", "https://en.wikipedia.org/wiki/TortoiseHg");

            URL_Add("Ubuntu&nbsp;17.10 (Artful Aardvark)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_17.10_.28Artful_Aardvark.29");

            URL_Add("Ubuntu&nbsp;18.04 (Bionic Beaver)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_18.04_LTS_.28Bionic_Beaver.29");

            URL_Add("challenge", "https://en.wiktionary.org/wiki/challenge#Noun");

            URL_Add("ATmega328", "https://en.wikipedia.org/wiki/ATmega328");

            URL_Add("ATmega328P", "https://en.wikipedia.org/wiki/ATmega328");

            URL_Add("Jupyter Notebook", "https://en.wikipedia.org/wiki/IPython#Project_Jupyter");

            URL_Add("Anaconda", "https://en.wikipedia.org/wiki/Anaconda_(Python_distribution)");

            URL_Add("Conda", "https://en.wikipedia.org/wiki/Conda_(package_manager)");

            URL_Add("Jekyll", "https://en.wikipedia.org/wiki/Jekyll_(software)");

            URL_Add("possibility", "https://en.wiktionary.org/wiki/possibility#Noun");

            URL_Add("C89", "https://en.wikipedia.org/wiki/ANSI_C#C89");

            URL_Add("occurred", "https://en.wiktionary.org/wiki/occured");

            URL_Add("development", "https://en.wiktionary.org/wiki/development#Noun");

            URL_Add("simultaneous", "https://en.wiktionary.org/wiki/simultaneous#Adjective");

            URL_Add("redefine", "https://en.wiktionary.org/wiki/redefine#Verb");

            URL_Add("Objective-C++", "https://en.wikipedia.org/wiki/Objective-C#Objective-C++");

            URL_Add("GLib", "https://en.wikipedia.org/wiki/GLib");

            URL_Add("ClearCase", "https://en.wikipedia.org/wiki/Rational_ClearCase");

            URL_Add("off-topic", "https://en.wiktionary.org/wiki/off-topic");

            URL_Add("particularly", "https://en.wiktionary.org/wiki/particularly#Adverb");

            URL_Add("preferred", "https://en.wiktionary.org/wiki/prefered");

            URL_Add("disappointed", "https://en.wiktionary.org/wiki/disappointed#Adjective");

            URL_Add("exercise", "https://en.wiktionary.org/wiki/excercise#Noun");

            URL_Add("contiguous", "https://en.wiktionary.org/wiki/contiguous#Adjective");

            URL_Add("consecutive", "https://en.wiktionary.org/wiki/consecutive#Adjective");

            URL_Add("believe", "https://en.wiktionary.org/wiki/believe#Verb");

            URL_Add("usable", "https://en.wiktionary.org/wiki/usable#Adjective");

            URL_Add("pronunciation", "https://en.wiktionary.org/wiki/pronounciation");

            URL_Add("require", "https://en.wiktionary.org/wiki/require#Verb");

            URL_Add("particular", "https://en.wiktionary.org/wiki/particular");

            URL_Add("don't know", "https://en.wiktionary.org/wiki/I_don%27t_know#Phrase");

            URL_Add("Canadian", "https://en.wiktionary.org/wiki/Canadian#Adjective");

            URL_Add("Debian&nbsp;5.0 (Lenny)", "https://en.wikipedia.org/wiki/Debian_version_history#Debian_5.0_(Lenny)");

            URL_Add("PulseView", "https://sigrok.org/wiki/PulseView");

            URL_Add("ArcGIS", "https://en.wikipedia.org/wiki/ArcGIS");

            URL_Add("persistent", "https://en.wiktionary.org/wiki/persistant#Adjective");

            URL_Add("Windows NT", "https://en.wikipedia.org/wiki/Windows_NT");

            URL_Add("Lubuntu", "https://en.wikipedia.org/wiki/Lubuntu");

            URL_Add("resistor", "https://en.wikipedia.org/wiki/Resistor");

            URL_Add("OpenJDK", "https://en.wikipedia.org/wiki/OpenJDK");

            URL_Add("Eclipse v3.2 (Callisto)", "https://en.wikipedia.org/wiki/Eclipse_(software)#Releases");

            URL_Add("Eclipse v3.3 (Europa)", "https://en.wikipedia.org/wiki/Eclipse_(software)#Releases");

            URL_Add("Eclipse v3.4 (Ganymede)", "https://en.wikipedia.org/wiki/Eclipse_(software)#Releases");

            URL_Add("instantiate", "https://en.wiktionary.org/wiki/instantiate#Verb");

            URL_Add("first", "https://en.wiktionary.org/wiki/first#Adjective");

            URL_Add("J1939", "https://en.wikipedia.org/wiki/SAE_J1939");

            URL_Add("MCP2515", "http://www.microchip.com/wwwproducts/en/en010406");

            URL_Add("occurrence", "https://en.wiktionary.org/wiki/occurence");

            URL_Add("credential", "https://en.wiktionary.org/wiki/credential#Noun"); // Old: https://en.wiktionary.org/wiki/cred#Noun

            URL_Add("dependencies", "https://en.wiktionary.org/wiki/dependencies#Noun");

            URL_Add("LINQPad", "https://en.wikipedia.org/wiki/LINQPad");

            URL_Add("efficient", "https://en.wiktionary.org/wiki/efficient");

            URL_Add("indentation", "https://en.wiktionary.org/wiki/indentation#Noun");

            URL_Add("horizontally", "https://en.wiktionary.org/wiki/horizontally#Adverb");

            URL_Add("Ask&nbsp;Ubuntu", "https://askubuntu.com/tour");

            URL_Add("management", "https://en.wiktionary.org/wiki/management#Noun");

            URL_Add("Portuguese", "https://en.wiktionary.org/wiki/Portuguese#Proper_noun");

            URL_Add("Mockito", "https://en.wikipedia.org/wiki/Mockito");

            URL_Add("referred", "https://en.wiktionary.org/wiki/referred#Verb");

            URL_Add("breakpoint", "https://en.wiktionary.org/wiki/breakpoint#Noun");

            URL_Add("up-to-date", "https://en.wiktionary.org/wiki/up-to-date#Adjective");

            URL_Add("React Native", "https://en.wikipedia.org/wiki/React_(JavaScript_library)#React_Native");

            URL_Add("Ionic", "https://en.wikipedia.org/wiki/Ionic_(mobile_app_framework)");

            URL_Add("transceiver", "https://en.wikipedia.org/wiki/Transceiver");

            //DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD

            URL_Add("Bluetooth LE", "https://en.wikipedia.org/wiki/Bluetooth_Low_Energy");

            URL_Add("CMakeLists.txt", "https://cmake.org/cmake/help/latest/manual/cmake-language.7.html#organization");

            URL_Add("FPU", "https://en.wikipedia.org/wiki/Floating-point_unit");

            URL_Add("For what it's worth", "https://en.wiktionary.org/wiki/FWIW#Interjection");

            URL_Add("Git Bash", "https://superuser.com/questions/1053633"); // Old: https://superuser.com/questions/1053633/what-is-git-bash-for-windows-anyway

            URL_Add("Git Extensions", "https://gitextensions.github.io/");

            URL_Add("I don't know", "https://en.wiktionary.org/wiki/IDK#Initialism");

            URL_Add("Invoke-Expression", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/invoke-expression");

            URL_Add("KDiff3", "http://kdiff3.sourceforge.net/");

            URL_Add("Kubernetes", "https://en.wikipedia.org/wiki/Kubernetes");

            URL_Add("MAMP", "https://en.wikipedia.org/wiki/MAMP");

            URL_Add("Mac OS X v10.12 (Sierra)", "https://en.wikipedia.org/wiki/MacOS_Sierra");

            URL_Add("NMAKE", "https://msdn.microsoft.com/en-us/library/dd9y37ha.aspx");

            URL_Add("POM", "https://en.wikipedia.org/wiki/Apache_Maven#Project_Object_Model");

            URL_Add("PyPI", "https://en.wikipedia.org/wiki/Python_Package_Index");

            URL_Add("Robot Framework", "https://en.wikipedia.org/wiki/Robot_Framework");

            URL_Add("Schmitt trigger", "https://en.wikipedia.org/wiki/Schmitt_trigger");

            URL_Add("Start-Job", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/start-job");

            URL_Add("Swedish", "https://en.wiktionary.org/wiki/Swedish#Noun");

            URL_Add("Ubuntu&nbsp;18.10 (Cosmic Cuttlefish)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_18.10_(Cosmic_Cuttlefish)");

            URL_Add("Ubuntu&nbsp;MATE", "https://en.wikipedia.org/wiki/Ubuntu_MATE");

            URL_Add("VSTS", "https://visualstudio.microsoft.com/team-services/");

            URL_Add("achieve", "https://en.wiktionary.org/wiki/achieve#Verb");

            URL_Add("answer", "https://en.wiktionary.org/wiki/answer#Noun");

            URL_Add("awkward", "https://en.wiktionary.org/wiki/awkward#Adjective");

            URL_Add("beforehand", "https://en.wiktionary.org/wiki/beforehand#Adverb");

            URL_Add("behavior", "https://en.wiktionary.org/wiki/behavior#Noun");

            URL_Add("below", "https://en.wiktionary.org/wiki/below#Preposition");

            URL_Add("cmdlet", "https://en.wikipedia.org/wiki/Windows_PowerShell#Cmdlets");

            URL_Add("communicate", "https://en.wiktionary.org/wiki/communicate#Verb");

            URL_Add("consistent", "https://en.wiktionary.org/wiki/consistant#Adjective");

            URL_Add("continuous", "https://en.wiktionary.org/wiki/continuous#Adjective");

            URL_Add("continuous deployment", "https://en.wikipedia.org/wiki/Continuous_delivery#Relationship_to_continuous_deployment");

            URL_Add("crontab", "https://en.wikipedia.org/wiki/Cron#Overview");

            URL_Add("de facto", "https://en.wiktionary.org/wiki/de_facto#Adjective");

            URL_Add("decision", "https://en.wiktionary.org/wiki/decision#Noun");

            URL_Add("enough", "https://en.wiktionary.org/wiki/enough#Determiner");

            URL_Add("existing", "https://en.wiktionary.org/wiki/existing#Adjective");

            URL_Add("explaining", "https://en.wiktionary.org/wiki/explaining#Verb");

            URL_Add("generation", "https://en.wiktionary.org/wiki/generation#Noun");

            URL_Add("length", "https://en.wiktionary.org/wiki/length#Noun");

            URL_Add("meantime", "https://en.wiktionary.org/wiki/in_the_meantime");

            URL_Add("paragraph", "https://en.wiktionary.org/wiki/paragraph#Noun");

            URL_Add("read-only", "https://en.wiktionary.org/wiki/read-only#Adjective");

            URL_Add("really", "https://en.wiktionary.org/wiki/really#Adverb");

            URL_Add("recommendation", "https://en.wiktionary.org/wiki/recommendation#Noun");

            URL_Add("recommended", "https://en.wiktionary.org/wiki/recommended#Verb");

            URL_Add("separator", "https://en.wiktionary.org/wiki/separator#Noun");

            URL_Add("statement", "https://en.wiktionary.org/wiki/statement#Noun");

            URL_Add("straight away", "https://en.wiktionary.org/wiki/straight_away#Adverb");

            URL_Add("strength", "https://en.wiktionary.org/wiki/strength#Noun");

            URL_Add("successful", "https://en.wiktionary.org/wiki/successful#Adjective");

            URL_Add("successfully", "https://en.wiktionary.org/wiki/successfully#Adverb");

            URL_Add("there's", "https://en.wiktionary.org/wiki/there%27s#Contraction");

            URL_Add("transferred", "https://en.wiktionary.org/wiki/transfered#Verb");

            URL_Add("transferring", "https://en.wiktionary.org/wiki/transferring#Verb");

            URL_Add("well-known", "https://en.wiktionary.org/wiki/well-known#Adjective");

            URL_Add("xUnit", "https://en.wikipedia.org/wiki/XUnit");

            URL_Add("Set-Content", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Set-Content");

            URL_Add("macOS v10.14 (Mojave)", "https://en.wikipedia.org/wiki/MacOS_Mojave");

            URL_Add("Ansible", "https://en.wikipedia.org/wiki/Ansible_(software)");

            URL_Add("Apache POI", "https://en.wikipedia.org/wiki/Apache_POI");

            URL_Add("Artifactory", "https://jfrog.com/artifactory/");

            URL_Add("Boolean", "https://en.wiktionary.org/wiki/Boolean#Noun");

            URL_Add("CRM", "https://en.wikipedia.org/wiki/Customer-relationship_management");

            URL_Add("Cppcheck", "https://en.wikipedia.org/wiki/Cppcheck");

            URL_Add("Cucumber", "https://en.wikipedia.org/wiki/Cucumber_(software)");

            URL_Add("DTO", "https://en.wikipedia.org/wiki/Data_transfer_object");

            URL_Add("Debian&nbsp;9 (Stretch)", "https://en.wikipedia.org/wiki/Debian_version_history#Debian_9_(Stretch)");

            URL_Add("Export-Clixml", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/export-clixml");

            URL_Add("MicroEMACS", "https://en.wikipedia.org/wiki/MicroEMACS");

            URL_Add("Network Monitor", "https://en.wikipedia.org/wiki/Microsoft_Network_Monitor");

            URL_Add("Oracle", "https://en.wikipedia.org/wiki/Oracle_Database");

            URL_Add("Out-GridView", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/out-gridview");

            URL_Add("PSCustomObject", "https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.pscustomobject?view=powershellsdk-1.1.0");

            URL_Add("Python&nbsp;2", "https://en.wikipedia.org/wiki/History_of_Python#Version_2");

            URL_Add("Python&nbsp;3", "https://en.wikipedia.org/wiki/History_of_Python#Version_3");

            URL_Add("Qur'an", "https://en.wiktionary.org/wiki/Qur%27an#Proper_noun");

            URL_Add("Star Wars", "https://en.wikipedia.org/wiki/Star_Wars");

            URL_Add("TikZ", "https://en.wikipedia.org/wiki/PGF/TikZ");

            URL_Add("UTC", "https://en.wikipedia.org/wiki/Coordinated_Universal_Time");

            URL_Add("across", "https://en.wiktionary.org/wiki/across#Preposition");

            URL_Add("alternatively", "https://en.wiktionary.org/wiki/alternatively#Adverb");

            URL_Add("apparent", "https://en.wiktionary.org/wiki/apparent#Adjective");

            URL_Add("bear with", "https://en.wiktionary.org/wiki/bear_with#Verb");

            URL_Add("bear with me", "https://en.wiktionary.org/wiki/bear_with#Verb");

            URL_Add("being", "https://en.wiktionary.org/wiki/being#Verbs");

            URL_Add("concatenation", "https://en.wiktionary.org/wiki/concatenation#Noun");

            URL_Add("destructor", "https://en.wikipedia.org/wiki/Destructor_(computer_programming)");

            URL_Add("initialise", "https://en.wiktionary.org/wiki/initialise#Verb");

            URL_Add("lose", "https://en.wiktionary.org/wiki/lose#Verb");

            URL_Add("losing", "https://en.wiktionary.org/wiki/lose#Verb");

            URL_Add("managing", "https://en.wiktionary.org/wiki/managing#English");

            URL_Add("previous", "https://en.wiktionary.org/wiki/previous#Adjective");

            URL_Add("responsibilities", "https://en.wiktionary.org/wiki/responsibilities#Noun");

            URL_Add("responsibility", "https://en.wiktionary.org/wiki/responsibility#Noun");

            URL_Add("silicon", "https://en.wikipedia.org/wiki/Silicon");

            URL_Add("standard error", "https://en.wikipedia.org/wiki/Standard_streams#Standard_error_(stderr)");

            URL_Add("surprised", "https://en.wiktionary.org/wiki/surprise#Verb");

            URL_Add("third", "https://en.wiktionary.org/wiki/third#Adjective");

            URL_Add("vice versa", "https://en.wiktionary.org/wiki/vice_versa#Adverb");

            URL_Add("yourself", "https://en.wiktionary.org/wiki/yourself#Pronoun");

            URL_Add("Atom", "https://en.wikipedia.org/wiki/Atom_(text_editor)");

            URL_Add("ConvertTo-Xml", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/ConvertTo-Xml");

            URL_Add("Cyrillic", "https://en.wiktionary.org/wiki/Cyrillic#Adjective");

            URL_Add("Eclipse&nbsp;v4.7 (Oxygen)", "https://en.wikipedia.org/wiki/Eclipse_%28software%29#Releases");

            URL_Add("Find-Package", "https://docs.microsoft.com/en-us/powershell/module/PackageManagement/Find-Package");

            URL_Add("Get-Module", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Core/Get-Module");

            URL_Add("Import-Clixml", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Import-Clixml");

            URL_Add("Import-Module", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/import-module");

            URL_Add("Install-Package", "https://docs.microsoft.com/en-us/powershell/module/PackageManagement/Install-Package");

            URL_Add("Latin", "https://en.wiktionary.org/wiki/Latin#Noun");

            URL_Add("Measure-Command", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Measure-Command");

            URL_Add("ROS", "https://en.wikipedia.org/wiki/Robot_Operating_System");

            URL_Add("Set-Variable", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Set-Variable");

            URL_Add("TeX", "https://en.wikipedia.org/wiki/TeX");

            URL_Add("Twig", "https://en.wikipedia.org/wiki/Twig_(template_engine)");

            URL_Add("UK", "https://en.wiktionary.org/wiki/UK#Adjective");

            URL_Add("VLAN", "https://en.wikipedia.org/wiki/Virtual_LAN");

            URL_Add("Windows Defender", "https://en.wikipedia.org/wiki/Windows_Defender");

            URL_Add("World War I", "https://en.wikipedia.org/wiki/World_War_I");

            URL_Add("Write-Verbose", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-verbose");

            URL_Add("compiler", "https://en.wikipedia.org/wiki/Compiler");

            URL_Add("computer", "https://en.wikipedia.org/wiki/Computer");

            URL_Add("eight", "https://en.wiktionary.org/wiki/8#Symbol");

            URL_Add("eighth", "https://en.wiktionary.org/wiki/eighth#Adjective");

            URL_Add("helpful", "https://en.wiktionary.org/wiki/helpful#Adjective");

            URL_Add("intermediate", "https://en.wiktionary.org/wiki/intermediate#Adjective");

            URL_Add("piece", "https://en.wiktionary.org/wiki/piece#Noun");

            URL_Add("previously", "https://en.wiktionary.org/wiki/previously#Adverb");

            URL_Add("professional", "https://en.wiktionary.org/wiki/professional#Adjective");

            URL_Add("success", "https://en.wiktionary.org/wiki/success#Noun");

            URL_Add("Christian", "https://en.wiktionary.org/wiki/Christian#Adjective");

            URL_Add("Elo", "https://en.wikipedia.org/wiki/Elo_rating_system");

            URL_Add("FIDE", "https://en.wikipedia.org/wiki/FIDE");

            URL_Add("NASA", "https://en.wikipedia.org/wiki/NASA");

            URL_Add("Out-String", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Out-String");

            URL_Add("Scandinavian", "https://en.wiktionary.org/wiki/Scandinavian#Adjective");

            URL_Add("Venus", "https://en.wikipedia.org/wiki/Venus");

            URL_Add("big O", "https://en.wikipedia.org/wiki/Big_O_notation");

            URL_Add("correctly", "https://en.wiktionary.org/wiki/correctly#Adverb");

            URL_Add("dictionary", "https://en.wiktionary.org/wiki/dictionary#Noun");

            URL_Add("kg", "https://en.wikipedia.org/wiki/Kilogram");

            URL_Add("sixth", "https://en.wiktionary.org/wiki/sixth#Adjective");

            URL_Add("Add-Type", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Add-Type");

            URL_Add("Android 5.1 (Lollipop)", "https://en.wikipedia.org/wiki/Android_Lollipop");

            URL_Add("Arduino CLI", "https://blog.arduino.cc/2018/08/24/announcing-the-arduino-command-line-interface-cli/");

            URL_Add("Clear-History", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Core/Clear-History");

            URL_Add("Commodore 64", "https://en.wikipedia.org/wiki/Commodore_64");

            URL_Add("DNA", "https://en.wikipedia.org/wiki/DNA");

            URL_Add("DevOps", "https://en.wikipedia.org/wiki/DevOps");

            URL_Add("Germanic", "https://en.wiktionary.org/wiki/Germanic#Adjective");

            URL_Add("Get-History", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Core/Get-History");

            URL_Add("OP", "https://en.wiktionary.org/wiki/OP#Initialism");

            URL_Add("PowerShell Core", "https://en.wikipedia.org/wiki/PowerShell#PowerShell_Core_6.0");

            URL_Add("Slavic", "https://en.wiktionary.org/wiki/Slavic#Adjective");

            URL_Add("System Center Configuration Manager", "https://en.wikipedia.org/wiki/Microsoft_System_Center_Configuration_Manager");

            URL_Add("WooCommerce", "https://en.wikipedia.org/wiki/WooCommerce");

            URL_Add("accomplishment", "https://en.wiktionary.org/wiki/accomplishment#Noun");

            URL_Add("brought", "https://en.wiktionary.org/wiki/brought#Verb");

            URL_Add("certainly", "https://en.wiktionary.org/wiki/certainly#Adverb");

            URL_Add("criticism", "https://en.wiktionary.org/wiki/criticism#Noun");

            URL_Add("instead", "https://en.wiktionary.org/wiki/instead#Adverb");

            URL_Add("maybe", "https://en.wiktionary.org/wiki/maybe#Adverb");

            URL_Add("plutonium", "https://en.wikipedia.org/wiki/Plutonium");

            URL_Add("possible", "https://en.wiktionary.org/wiki/possible#Adjective");

            URL_Add("programming", "https://en.wiktionary.org/wiki/programming#Noun");

            URL_Add("rhetorical", "https://en.wiktionary.org/wiki/rhetorical#Adjective");

            URL_Add("thorium", "https://en.wikipedia.org/wiki/Thorium");

            URL_Add("uranium", "https://en.wikipedia.org/wiki/Uranium");

            URL_Add("with", "https://en.wiktionary.org/wiki/with#Preposition");

            URL_Add("MCVE", "https://stackoverflow.com/help/mcve");

            URL_Add("New-Variable", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/New-Variable");

            URL_Add("ConvertTo-Csv", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/ConvertTo-Csv");

            URL_Add("Doppler", "https://en.wikipedia.org/wiki/Doppler_effect");

            URL_Add("MathWorks", "https://en.wikipedia.org/wiki/MathWorks");

            URL_Add("Sunday", "https://en.wiktionary.org/wiki/Sunday#Noun");

            URL_Add("Vert.x", "https://en.wikipedia.org/wiki/Vert.x");

            URL_Add("appropriate", "https://en.wiktionary.org/wiki/appropriate#Adjective");

            URL_Add("careful", "https://en.wiktionary.org/wiki/careful#Adjective");

            URL_Add("party", "https://en.wiktionary.org/wiki/party#Noun");

            URL_Add("rather", "https://en.wiktionary.org/wiki/rather#Adverb");

            URL_Add("second", "https://en.wiktionary.org/wiki/second#Adjective");

            URL_Add("todo", "https://en.wiktionary.org/wiki/todo#Noun");

            URL_Add("Irish", "https://en.wiktionary.org/wiki/Irish#Adjective");

            URL_Add("googling", "https://en.wiktionary.org/wiki/googling#Verb");

            URL_Add("Denmark", "https://en.wiktionary.org/wiki/Denmark#Proper_noun");

            URL_Add("OCD", "https://en.wikipedia.org/wiki/Obsessive%E2%80%93compulsive_disorder");

            URL_Add("Unity 3D", "https://en.wikipedia.org/wiki/Unity_(game_engine)");

            URL_Add("uncomfortable", "https://en.wiktionary.org/wiki/uncomfortable#Adjective");

            URL_Add("Invoke-Item", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Management/Invoke-Item");

            URL_Add("dansk", "https://ordnet.dk/ddo/ordbog?query=dansk");

            URL_Add("engelsk", "https://ordnet.dk/ddo/ordbog?query=engelsk");

            URL_Add("their", "https://en.wiktionary.org/wiki/their#Determiner");

            URL_Add("Brexit", "https://en.wiktionary.org/wiki/Brexit#Proper_noun");

            URL_Add("Docker Hub", "https://en.wikipedia.org/wiki/Docker_(software)#Components");

            URL_Add("Marxism", "https://en.wiktionary.org/wiki/Marxism#English");

            URL_Add("afraid", "https://en.wiktionary.org/wiki/afraid#Adjective");

            URL_Add("briefly", "https://en.wiktionary.org/wiki/briefly#Adverb");

            URL_Add("exception", "https://en.wiktionary.org/wiki/exception#Noun");

            URL_Add("neighbour", "https://en.wiktionary.org/wiki/neighbour#Noun");

            URL_Add("until", "https://en.wiktionary.org/wiki/until#Conjunction");

            URL_Add("yesterday", "https://en.wiktionary.org/wiki/yesterday#Adverb");

            URL_Add("DuckDuckGo", "https://en.wikipedia.org/wiki/DuckDuckGo");

            URL_Add("Yahoo Answers", "https://en.wikipedia.org/wiki/Yahoo!_Answers");

            URL_Add("democratically", "https://en.wiktionary.org/wiki/democratically#Adverb");

            URL_Add("impossibility", "https://en.wiktionary.org/wiki/impossibility#Noun");

            URL_Add("judge", "https://en.wiktionary.org/wiki/judge#Verb");

            URL_Add("necessity", "https://en.wiktionary.org/wiki/necessity#Noun");

            URL_Add("negotiation", "https://en.wiktionary.org/wiki/negotiation#Noun");

            URL_Add("opportunity", "https://en.wiktionary.org/wiki/opportunity#Noun");

            URL_Add("parliament", "https://en.wiktionary.org/wiki/parliament#Noun");

            URL_Add("China", "https://en.wiktionary.org/wiki/China#Proper_noun");

            URL_Add("Japan", "https://en.wikipedia.org/wiki/Japan");

            URL_Add("in fact", "https://en.wiktionary.org/wiki/in_fact#Prepositional_phrase");

            URL_Add("parliamentary", "https://en.wiktionary.org/wiki/parliamentary#Adjective");

            URL_Add("Android 7.0 (Nougat)", "https://en.wikipedia.org/wiki/Android_Nougat");

            URL_Add("Android 8.1 (Oreo)", "https://en.wikipedia.org/wiki/Android_Oreo");

            URL_Add("Android 9.0 (Pie)", "https://en.wikipedia.org/wiki/Android_Pie");

            URL_Add("Cordova", "https://en.wikipedia.org/wiki/Apache_Cordova");

            URL_Add("attention", "https://en.wiktionary.org/wiki/attention#Noun");

            URL_Add("infinite", "https://en.wiktionary.org/wiki/infinite#Adjective");

            URL_Add("Aero", "https://en.wikipedia.org/wiki/Windows_Aero");

            URL_Add("Bitdefender", "https://en.wikipedia.org/wiki/Bitdefender#Bitdefender_products");

            URL_Add("European Union", "https://en.wiktionary.org/wiki/European_Union#Proper_noun");

            URL_Add("JFrog", "https://en.wikipedia.org/wiki/Software_repository#Repository_managers");

            URL_Add("Scintilla", "https://en.wikipedia.org/wiki/Scintilla_(software)");

            URL_Add("ambiguous", "https://en.wiktionary.org/wiki/ambiguous#Adjective");

            URL_Add("approach", "https://en.wiktionary.org/wiki/approach#Noun");

            URL_Add("clipboard", "https://en.wiktionary.org/wiki/clipboard#Noun");

            URL_Add("equation", "https://en.wiktionary.org/wiki/equation#Noun");

            URL_Add("extension", "https://en.wiktionary.org/wiki/extension#Noun");

            URL_Add("framework", "https://en.wiktionary.org/wiki/framework#Noun");

            URL_Add("hierarchical", "https://en.wiktionary.org/wiki/hierarchical#Adjective");

            URL_Add("interpret", "https://en.wiktionary.org/wiki/interpret#Verb");

            URL_Add("procedure", "https://en.wiktionary.org/wiki/procedure#Noun");

            URL_Add("separately", "https://en.wiktionary.org/wiki/separately#Adverb");

            URL_Add("terrible", "https://en.wiktionary.org/wiki/terrible#Adjective");

            URL_Add("title", "https://en.wiktionary.org/wiki/title#Noun");

            URL_Add("visualisation", "https://en.wiktionary.org/wiki/visualisation#Noun");

            URL_Add("LTspice", "https://en.wikipedia.org/wiki/LTspice");

            URL_Add("NATO", "https://en.wikipedia.org/wiki/NATO");

            URL_Add("RStudio", "https://en.wikipedia.org/wiki/RStudio");

            URL_Add("broadcast", "https://en.wiktionary.org/wiki/broadcast#Adjective");

            URL_Add("capitalist", "https://en.wiktionary.org/wiki/capitalist#Adjective");

            URL_Add("completely", "https://en.wiktionary.org/wiki/completely#Adverb");

            URL_Add("Flexbox", "https://en.wikipedia.org/wiki/CSS_Flexible_Box_Layout");

            URL_Add("in case", "https://en.wiktionary.org/wiki/incase#Conjunction");

            URL_Add("permission", "https://en.wiktionary.org/wiki/permission#Noun");

            URL_Add("personal", "https://en.wiktionary.org/wiki/personal#Adjective");

            URL_Add("probably", "https://en.wiktionary.org/wiki/probably#Adverb");

            URL_Add("several", "https://en.wiktionary.org/wiki/several#Determiner");

            URL_Add("socialism", "https://en.wiktionary.org/wiki/socialism#Noun");

            URL_Add("tried", "https://en.wiktionary.org/wiki/tried#Verb");

            URL_Add("justified", "https://en.wiktionary.org/wiki/justified#Adjective");

            URL_Add("perform", "https://en.wiktionary.org/wiki/perform#Verb");

            URL_Add("Dutch", "https://en.wiktionary.org/wiki/Dutch#Noun");

            URL_Add("browser", "https://en.wiktionary.org/wiki/browser#Noun");

            URL_Add("printf", "https://en.wikipedia.org/wiki/Printf_format_string");

            URL_Add("FET", "https://en.wikipedia.org/wiki/Field-effect_transistor");

            URL_Add("LoRa", "https://en.wikipedia.org/wiki/LoRa");

            URL_Add("Squarespace", "https://en.wikipedia.org/wiki/Squarespace#Software");

            URL_Add("TeamCity", "https://en.wikipedia.org/wiki/TeamCity");

            URL_Add("Write-Error", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Write-Error");

            URL_Add("directly", "https://en.wiktionary.org/wiki/directly#Adverb");

            URL_Add("discovered", "https://en.wiktionary.org/wiki/discovered#Verb");

            URL_Add("discrete", "https://en.wiktionary.org/wiki/discrete#Adjective");

            URL_Add("little", "https://en.wiktionary.org/wiki/little#Adverb");

            URL_Add("solution", "https://en.wiktionary.org/wiki/solution#Noun");

            URL_Add("threshold", "https://en.wiktionary.org/wiki/threshold#Noun");

            URL_Add("version", "https://en.wiktionary.org/wiki/version#Noun");

            URL_Add("AliExpress", "https://en.wikipedia.org/wiki/AliExpress");

            URL_Add("bandwidth", "https://en.wiktionary.org/wiki/bandwidth#Noun");

            URL_Add("bureaucracy", "https://en.wiktionary.org/wiki/bureaucracy#Noun");

            URL_Add("bureaucrat", "https://en.wiktionary.org/wiki/bureaucrat#Noun");

            URL_Add("gibberish", "https://en.wiktionary.org/wiki/gibberish#Noun");

            URL_Add("happiness", "https://en.wiktionary.org/wiki/happiness#Noun");

            URL_Add("manager", "https://en.wiktionary.org/wiki/manager#Noun");

            URL_Add("release", "https://en.wiktionary.org/wiki/release#Noun");

            URL_Add("separatist", "https://en.wiktionary.org/wiki/separatist#Noun");

            URL_Add("ActiveState", "https://en.wikipedia.org/wiki/ActiveState");

            URL_Add("DIAC", "https://en.wikipedia.org/wiki/DIAC");

            URL_Add("annoying", "https://en.wiktionary.org/wiki/annoying#Verb");

            URL_Add("appear", "https://en.wiktionary.org/wiki/appear#Verb");

            URL_Add("misspell", "https://en.wiktionary.org/wiki/misspell#Verb");

            URL_Add("µA", "https://en.wikipedia.org/wiki/Ampere#Portable_devices");

            URL_Add("Windows Server 2016", "https://en.wikipedia.org/wiki/Windows_Server_2016");

            URL_Add("appreciated", "https://en.wiktionary.org/wiki/appreciated#Verb");

            URL_Add("logic analyser", "https://en.wikipedia.org/wiki/Logic_analyzer");

            URL_Add("port 137", "https://www.grc.com/port_137.htm");

            URL_Add("port 138", "https://www.grc.com/port_138.htm");

            URL_Add("port 139", "https://www.grc.com/port_139.htm");

            URL_Add("port 445", "https://www.grc.com/port_445.htm");

            URL_Add("ANSI", "https://en.wikipedia.org/wiki/ANSI_character_set");

            URL_Add("Fibonacci", "https://en.wikipedia.org/wiki/Fibonacci_number");

            URL_Add("GraphQL", "https://en.wikipedia.org/wiki/GraphQL");

            URL_Add("MSYS", "https://en.wikipedia.org/wiki/MinGW#History");

            URL_Add("Mac Mini", "https://en.wikipedia.org/wiki/Mac_Mini");

            URL_Add("SCPI", "https://en.wikipedia.org/wiki/Standard_Commands_for_Programmable_Instruments");

            URL_Add("USBTMC", "https://en.wikipedia.org/wiki/USB#Device_classes");

            URL_Add("in order", "https://en.wiktionary.org/wiki/in_order#Adverb");

            URL_Add("increment", "https://en.wiktionary.org/wiki/increment#Verb");

            URL_Add("mA", "https://en.wiktionary.org/wiki/mA#Translingual");

            URL_Add("ACL", "https://en.wikipedia.org/wiki/Access_control_list");

            URL_Add("Midnight Commander", "https://en.wikipedia.org/wiki/Midnight_Commander");

            URL_Add("Shift", "https://en.wikipedia.org/wiki/Shift_key");

            URL_Add("Ubuntu&nbsp;19.04 (Disco Dingo)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_19.04_(Disco_Dingo)");

            URL_Add("X.509", "https://en.wikipedia.org/wiki/X.509");

            URL_Add("already", "https://en.wiktionary.org/wiki/already#Adverb");

            URL_Add("business", "https://en.wiktionary.org/wiki/business#Noun");

            URL_Add("explicitly", "https://en.wiktionary.org/wiki/explicitly#Adverb");

            URL_Add("Hebrew", "https://en.wiktionary.org/wiki/Hebrew#Noun");

            URL_Add("implementing", "https://en.wiktionary.org/wiki/implementing#Verb");

            URL_Add("perfectly", "https://en.wiktionary.org/wiki/perfectly#Adverb");

            URL_Add("phonomena", "https://en.wiktionary.org/wiki/phenomena#Noun");

            URL_Add("possibilities", "https://en.wiktionary.org/wiki/possibility#Noun");

            URL_Add("pull request", "https://en.wikipedia.org/wiki/Distributed_version_control#Pull_requests");

            URL_Add("recursively", "https://en.wiktionary.org/wiki/recursively#Adverb");

            URL_Add("stumble", "https://en.wiktionary.org/wiki/stumble#Verb");

            URL_Add("written", "https://en.wiktionary.org/wiki/written#Verb");

            URL_Add("BSc", "https://en.wiktionary.org/wiki/Bachelor_of_Science#Abbreviations");

            URL_Add("Hindi", "https://en.wiktionary.org/wiki/Hindi#Noun");

            URL_Add("accounting", "https://en.wiktionary.org/wiki/accounting#Noun");

            URL_Add("biography", "https://en.wiktionary.org/wiki/biography#Noun");

            URL_Add("descriptive", "https://en.wiktionary.org/wiki/descriptive#Adjective");

            URL_Add("experience", "https://en.wiktionary.org/wiki/experience#Noun");

            URL_Add("low-level", "https://en.wiktionary.org/wiki/low-level#Adjective");

            URL_Add("progressive web application", "https://en.wikipedia.org/wiki/Progressive_web_applications");

            URL_Add("responsive web design", "https://en.wikipedia.org/wiki/Responsive_web_design");

            URL_Add("themselves", "https://en.wiktionary.org/wiki/themselves#Pronoun");

            URL_Add("121GW", "https://www.eevblog.com/product/121gw/");

            URL_Add("bipolar junction transistor", "https://en.wikipedia.org/wiki/Bipolar_junction_transistor");

            URL_Add("camera", "https://en.wiktionary.org/wiki/camera#Noun");

            URL_Add("check out", "https://en.wiktionary.org/wiki/check_out#Verb");

            URL_Add("professor", "https://en.wiktionary.org/wiki/professor#Noun");

            URL_Add("rewriting", "https://en.wiktionary.org/wiki/rewriting#Verb");

            URL_Add("Blitz++", "https://en.wikipedia.org/wiki/Blitz%2B%2B");

            URL_Add("anyway", "https://en.wiktionary.org/wiki/anyway#Adverb");

            URL_Add("breadboard", "https://en.wikipedia.org/wiki/Breadboard");

            URL_Add("convenient", "https://en.wiktionary.org/wiki/convenient#Adjective");

            URL_Add("however", "https://en.wiktionary.org/wiki/however#Adverb");

            URL_Add("perfboard", "https://en.wikipedia.org/wiki/Perfboard");

            URL_Add("responsive web application", "https://en.wikipedia.org/wiki/Responsive_web_design");

            URL_Add("sceptic", "https://en.wiktionary.org/wiki/sceptic#Noun");

            URL_Add("C++20", "https://en.wikipedia.org/wiki/C%2B%2B20");

            URL_Add("Game of Thrones", "https://en.wikipedia.org/wiki/Game_of_Thrones");

            URL_Add("Google Cloud Platform", "https://en.wikipedia.org/wiki/Google_Cloud_Platform");

            URL_Add("Huawei", "https://en.wikipedia.org/wiki/Huawei");

            URL_Add("amount", "https://en.wiktionary.org/wiki/amount#Noun");

            URL_Add("anywhere", "https://en.wiktionary.org/wiki/anywhere#Adverb");

            URL_Add("asking", "https://en.wiktionary.org/wiki/asking#Verb");

            URL_Add("calculate", "https://en.wiktionary.org/wiki/calculate#Verb");

            URL_Add("consistently", "https://en.wiktionary.org/wiki/consistently#Adverb");

            URL_Add("elsewhere", "https://en.wiktionary.org/wiki/elsewhere#Adverb");

            URL_Add("familiar", "https://en.wiktionary.org/wiki/familiar#Adjective");

            URL_Add("if I understand correctly", "https://en.wiktionary.org/wiki/IIUC#Phrase");

            URL_Add("keyword", "https://en.wiktionary.org/wiki/keyword#Noun");

            URL_Add("let me", "https://en.wiktionary.org/wiki/lemme#Contraction");

            URL_Add("millisecond", "https://en.wiktionary.org/wiki/millisecond#Noun");

            URL_Add("program", "https://en.wiktionary.org/wiki/program#Noun");

            URL_Add("run-length encoding", "https://en.wikipedia.org/wiki/Run-length_encoding");

            URL_Add("running", "https://en.wiktionary.org/wiki/running#Verb");

            URL_Add("spreadsheet", "https://en.wiktionary.org/wiki/spreadsheet#Noun");

            URL_Add("syntax", "https://en.wiktionary.org/wiki/syntax#Noun");

            URL_Add("yUML", "https://yuml.me/");

            URL_Add("ATmega", "https://en.wikipedia.org/wiki/AVR_microcontrollers#Basic_families");

            URL_Add("Delhi", "https://en.wikipedia.org/wiki/Delhi");

            URL_Add("Electron", "https://en.wikipedia.org/wiki/Electron_(software_framework)");

            URL_Add("India", "https://en.wikipedia.org/wiki/India");

            URL_Add("PuTTYgen", "https://en.wikipedia.org/wiki/PuTTY#Components");

            URL_Add("acknowledge", "https://en.wiktionary.org/wiki/acknowledge#Verb");

            URL_Add("commitment", "https://en.wiktionary.org/wiki/commitment#Noun");

            URL_Add("default", "https://en.wiktionary.org/wiki/default#Noun");

            URL_Add("fourth", "https://en.wiktionary.org/wiki/fourth#Adjective");

            URL_Add("fulfill", "https://en.wiktionary.org/wiki/fulfill#Verb");

            URL_Add("itself", "https://en.wiktionary.org/wiki/itself#Pronoun");

            URL_Add("know-how", "https://en.wiktionary.org/wiki/know-how#Noun");

            URL_Add("milliamperes", "https://en.wiktionary.org/wiki/milliampere#Noun");

            URL_Add("overwritten", "https://en.wiktionary.org/wiki/overwritten#Verb");

            URL_Add("personal access token", "https://microclimate.dev/creatingpat");

            URL_Add("repetitive", "https://en.wiktionary.org/wiki/repetitive#Adjective");

            URL_Add("sine wave", "https://en.wikipedia.org/wiki/Sine_wave");

            URL_Add("undefined behaviour", "https://en.wikipedia.org/wiki/Undefined_behavior");

            URL_Add("Texas Instruments", "https://en.wikipedia.org/wiki/Texas_Instruments");

            URL_Add("Webpack", "https://en.wikipedia.org/wiki/Webpack");

            URL_Add("ad hoc", "https://en.wiktionary.org/wiki/ad_hoc#Adjective");

            URL_Add("capacitive", "https://en.wiktionary.org/wiki/capacitive#Adjective");

            URL_Add("coordinate", "https://en.wiktionary.org/wiki/coordinate#Noun");

            URL_Add("finally", "https://en.wiktionary.org/wiki/finally#Adverb");

            URL_Add("hydrogen", "https://en.wikipedia.org/wiki/Hydrogen");

            URL_Add("language", "https://en.wiktionary.org/wiki/language#Noun");

            URL_Add("minutes", "https://en.wiktionary.org/wiki/minute#Noun");

            URL_Add("Sourcetree", "https://en.wikipedia.org/wiki/Atlassian#Acquisitions_and_product_announcements");

            URL_Add("environment variable", "https://en.wiktionary.org/wiki/environment_variable");

            URL_Add("ASAP", "https://en.wiktionary.org/wiki/ASAP#Adverb");

            URL_Add("QWERTY", "https://en.wikipedia.org/wiki/QWERTY");

            URL_Add("Spring Boot", "https://en.wikipedia.org/wiki/Spring_Framework#Spring_Boot");

            URL_Add("accessible", "https://en.wiktionary.org/wiki/accessible#Adjective");

            URL_Add("column", "https://en.wiktionary.org/wiki/column#Noun");

            URL_Add("created", "https://en.wiktionary.org/wiki/created#Verb");

            URL_Add("exclusive", "https://en.wiktionary.org/wiki/exclusive#Adjective");

            URL_Add("iron", "https://en.wikipedia.org/wiki/Iron");

            URL_Add("looks", "https://en.wiktionary.org/wiki/looks#Verb");

            URL_Add("middle", "https://en.wiktionary.org/wiki/middle#Noun");

            URL_Add("nitrogen", "https://en.wikipedia.org/wiki/Nitrogen");

            URL_Add("scaling", "https://en.wiktionary.org/wiki/scaling#Verb");

            URL_Add("service worker", "https://en.wikipedia.org/wiki/Progressive_web_applications#Service_workers");

            URL_Add("sign in", "https://en.wiktionary.org/wiki/sign_in#Verb");

            URL_Add("substitution", "https://en.wiktionary.org/wiki/substitution#Noun");

            URL_Add("symmetrically", "https://en.wiktionary.org/wiki/symmetrically#Adverb");

            URL_Add("troubleshooting", "https://en.wiktionary.org/wiki/troubleshoot#Verb");

            URL_Add("work order", "https://en.wiktionary.org/wiki/work_order#Noun");

            URL_Add("customer", "https://en.wiktionary.org/wiki/customer#Noun");

            URL_Add("voilà", "https://en.wiktionary.org/wiki/voil%C3%A0#Interjection");

            URL_Add("JetBrains", "https://en.wikipedia.org/wiki/JetBrains");

            URL_Add("MySQL Workbench", "https://en.wikipedia.org/wiki/MySQL_Workbench");

            URL_Add("blockchain", "https://en.wikipedia.org/wiki/Blockchain");

            URL_Add("committed", "https://en.wiktionary.org/wiki/committed#Verb");

            URL_Add("high school", "https://en.wikipedia.org/wiki/High_school_(North_America)");

            URL_Add("no operation", "https://en.wikipedia.org/wiki/NOP_(code)");

            URL_Add("proficient", "https://en.wiktionary.org/wiki/proficient#Adjective");

            URL_Add("so-called", "https://en.wiktionary.org/wiki/so-called#Adjective");

            URL_Add("software engineer", "https://en.wikipedia.org/wiki/Software_engineer");

            URL_Add("swarm intelligence", "https://en.wikipedia.org/wiki/Swarm_intelligence");

            URL_Add("thingy", "https://en.wiktionary.org/wiki/thingy#Noun");

            URL_Add("Stripe", "https://en.wikipedia.org/wiki/Stripe_(company)");

            URL_Add("Upwork", "https://en.wikipedia.org/wiki/Upwork");

            URL_Add("a la", "https://en.wiktionary.org/wiki/a_la#Preposition");

            URL_Add("ascending", "https://en.wiktionary.org/wiki/ascending#Adjective");

            URL_Add("freelancer", "https://en.wiktionary.org/wiki/freelancer#Noun");

            URL_Add("generated", "https://en.wiktionary.org/wiki/generate#Verb");

            URL_Add("height", "https://en.wiktionary.org/wiki/height#Noun");

            URL_Add("malicious", "https://en.wiktionary.org/wiki/malicious#Adjective");

            URL_Add("negligible", "https://en.wiktionary.org/wiki/negligible#Adjective");

            URL_Add("recently", "https://en.wiktionary.org/wiki/recently#Adverb");

            URL_Add("run time", "https://en.wiktionary.org/wiki/run_time#Noun");

            URL_Add("specify", "https://en.wiktionary.org/wiki/specify#Verb");

            URL_Add("split", "https://en.wiktionary.org/wiki/split#Verb");

            URL_Add("spontaneously", "https://en.wiktionary.org/wiki/spontaneously#Adverb");

            URL_Add("truly", "https://en.wiktionary.org/wiki/truly#Adverb");

            URL_Add("Catch-22", "https://en.wiktionary.org/wiki/Catch-22#Noun");

            URL_Add("MSc", "https://en.wiktionary.org/wiki/MSc#Noun");

            URL_Add("Raspbian&nbsp;10 (Buster)", "https://en.wikipedia.org/wiki/Raspbian#Version_history");

            URL_Add("THz", "http://en.wikipedia.org/wiki/Hertz#SI_multiples");

            URL_Add("Transifex", "https://en.wikipedia.org/wiki/Transifex");

            URL_Add("Ubuntu&nbsp;19.10 (Eoan Ermine)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_19.10_(Eoan_Ermine)");

            URL_Add("additionally", "https://en.wiktionary.org/wiki/additionally#Adverb");

            URL_Add("bare-bones", "https://en.wiktionary.org/wiki/bare-bones#Adjective");

            URL_Add("current", "https://en.wikipedia.org/wiki/Electric_current");

            URL_Add("nevertheless", "https://en.wiktionary.org/wiki/nevertheless#Adverb");

            URL_Add("nonetheless", "https://en.wiktionary.org/wiki/nonetheless#Adverb");

            URL_Add("oDesk", "https://en.wikipedia.org/wiki/Upwork");

            URL_Add("object", "https://en.wiktionary.org/wiki/object#Noun");

            URL_Add("precede", "https://en.wiktionary.org/wiki/precede#Verb");

            URL_Add("reference", "https://en.wiktionary.org/wiki/reference#Noun");

            URL_Add("standard input", "https://en.wikipedia.org/wiki/Standard_streams#Standard_input_(stdin)");

            URL_Add("two-dimensional", "https://en.wiktionary.org/wiki/two-dimensional#Adjective");

            URL_Add("uBlock Origin", "https://en.wikipedia.org/wiki/UBlock_Origin");

            URL_Add("NSFW", "https://en.wiktionary.org/wiki/NSFW#Adjective");

            URL_Add("PDE", "https://en.wikipedia.org/wiki/Partial_differential_equation");

            URL_Add("Waffen-SS", "https://en.wikipedia.org/wiki/Waffen-SS");

            URL_Add("aircraft", "https://en.wiktionary.org/wiki/aircraft#Noun");

            URL_Add("border", "https://en.wiktionary.org/wiki/border#Noun");

            URL_Add("civilian", "https://en.wiktionary.org/wiki/civilian#Noun");

            URL_Add("differentiate", "https://en.wiktionary.org/wiki/differentiate#Verb");

            URL_Add("uWSGI", "https://en.wikipedia.org/wiki/UWSGI");

            URL_Add("well-being", "https://en.wiktionary.org/wiki/well-being#Noun");

            URL_Add("within", "https://en.wiktionary.org/wiki/within#Preposition");

            URL_Add("worry", "https://en.wiktionary.org/wiki/worry#Verb");

            URL_Add("Aptana Studio", "http://en.wikipedia.org/wiki/Aptana#Aptana_Studio");

            URL_Add("Spring Framework", "http://en.wikipedia.org/wiki/Spring_Framework");

            URL_Add("Spring.NET", "http://www.springframework.net/");

            URL_Add("Fortnite", "https://en.wikipedia.org/wiki/Fortnite");

            URL_Add("ISO 8859-1", "https://en.wikipedia.org/wiki/ISO/IEC_8859-1");

            URL_Add("LXC", "https://en.wikipedia.org/wiki/LXC");

            URL_Add("MSTest", "https://en.wikipedia.org/wiki/Visual_Studio_Unit_Testing_Framework");

            URL_Add("Mozilla Developer Network", "https://en.wikipedia.org/wiki/MDN_Web_Docs");

            URL_Add("Pluralsight", "https://en.wikipedia.org/wiki/Pluralsight");

            URL_Add("StyleCop", "https://en.wikipedia.org/wiki/StyleCop");

            URL_Add("Windows-1252", "https://en.wikipedia.org/wiki/Windows-1252");

            URL_Add("appreciate", "https://en.wiktionary.org/wiki/appreciate#Verb");

            URL_Add("assume", "https://en.wiktionary.org/wiki/assume#Verb");

            URL_Add("challenging", "https://en.wiktionary.org/wiki/challenging#Adjective");

            URL_Add("competent", "https://en.wiktionary.org/wiki/competent#Adjective");

            URL_Add("difference", "https://en.wiktionary.org/wiki/difference#Noun");

            URL_Add("disastrous", "https://en.wiktionary.org/wiki/disastrous#Adjective");

            URL_Add("easy", "https://en.wiktionary.org/wiki/easy#Adjective");

            URL_Add("gamification", "https://en.wiktionary.org/wiki/gamification#Noun");

            URL_Add("guideline", "https://en.wiktionary.org/wiki/guideline#Noun");

            URL_Add("highlighted", "https://en.wiktionary.org/wiki/highlight#Verb");

            URL_Add("ideally", "https://en.wiktionary.org/wiki/ideally#Adverb");

            URL_Add("judgement", "https://en.wiktionary.org/wiki/judgement#Noun");

            URL_Add("ongoing", "https://en.wiktionary.org/wiki/ongoing#Adjective");

            URL_Add("position", "https://en.wiktionary.org/wiki/position#Noun");

            URL_Add("vehicle", "https://en.wiktionary.org/wiki/vehicle#Noun");

            URL_Add("weird", "https://en.wiktionary.org/wiki/weird#Adjective");

            URL_Add("squash", "https://en.wiktionary.org/wiki/squash#Verb");

            URL_Add("OCR", "https://en.wikipedia.org/wiki/Optical_character_recognition");

            URL_Add("ODBC", "https://en.wikipedia.org/wiki/Open_Database_Connectivity");

            URL_Add("Docker Compose", "https://en.wikipedia.org/wiki/Docker_%28software%29#Tools");

            URL_Add("Get-DiskImage", "https://docs.microsoft.com/en-us/powershell/module/storage/get-diskimage");

            URL_Add("Konqueror", "https://en.wikipedia.org/wiki/Konqueror");

            URL_Add("another", "https://en.wiktionary.org/wiki/another#Determiner");

            URL_Add("category", "https://en.wiktionary.org/wiki/category#Noun");

            URL_Add("ccache", "https://en.wikipedia.org/wiki/Ccache");

            URL_Add("constitute", "https://en.wiktionary.org/wiki/constitute#Verb");

            URL_Add("debugging", "https://en.wiktionary.org/wiki/debugging#Verb");

            URL_Add("distcc", "https://en.wikipedia.org/wiki/Distcc");

            URL_Add("either", "https://en.wiktionary.org/wiki/either#Pronoun");

            URL_Add("fps", "https://en.wikipedia.org/wiki/Frames_per_second");

            URL_Add("further", "https://en.wiktionary.org/wiki/further#Adverb");

            URL_Add("internally", "https://en.wiktionary.org/wiki/internally#Adverb");

            URL_Add("mundane", "https://en.wiktionary.org/wiki/mundane#Adjective");

            URL_Add("neither", "https://en.wiktionary.org/wiki/neither#Pronoun");

            URL_Add("official", "https://en.wiktionary.org/wiki/official#Adjective");

            URL_Add("otherwise", "https://en.wiktionary.org/wiki/otherwise#Adverb");

            URL_Add("resolution", "https://en.wiktionary.org/wiki/resolution#Noun");

            URL_Add("scroll bar", "https://en.wiktionary.org/wiki/scroll_bar#Noun");

            URL_Add("writing", "https://en.wiktionary.org/wiki/writing#Verb");

            URL_Add("Vagrant", "https://en.wikipedia.org/wiki/Vagrant_%28software%29");

            URL_Add("still", "https://en.wiktionary.org/wiki/still#Adverb");

            URL_Add("Compiler Explorer", "https://github.com/mattgodbolt/compiler-explorer");

            URL_Add("DigitalOcean", "https://en.wikipedia.org/wiki/DigitalOcean");

            URL_Add("Exif", "https://en.wikipedia.org/wiki/Exif");

            URL_Add("FIFO", "https://en.wikipedia.org/wiki/FIFO_(computing_and_electronics)");

            URL_Add("Google Scholar", "https://en.wikipedia.org/wiki/Google_Scholar");

            URL_Add("JavaServer Faces", "https://en.wikipedia.org/wiki/JavaServer_Faces");

            URL_Add("PHP&nbsp;7", "https://en.wikipedia.org/wiki/PHP#PHP_7");

            URL_Add("Power BI", "https://en.wikipedia.org/wiki/Power_BI");

            URL_Add("Stack Exchange Data Explorer", "https://meta.stackexchange.com/tags/data-explorer/info");

            URL_Add("alleged", "https://en.wiktionary.org/wiki/alleged#Adjective");

            URL_Add("allow", "https://en.wiktionary.org/wiki/allow#Verb");

            URL_Add("almost", "https://en.wiktionary.org/wiki/almost#Adverb");

            URL_Add("answered", "https://en.wiktionary.org/wiki/answer#Verb");

            URL_Add("antialiasing", "https://en.wiktionary.org/wiki/antialiasing#Noun");

            URL_Add("as far as I can tell", "https://en.wiktionary.org/wiki/AFAICT#Phrase");

            URL_Add("assignment", "https://en.wiktionary.org/wiki/assignment#Noun");

            URL_Add("children", "https://en.wiktionary.org/wiki/children#Noun");

            URL_Add("commented", "https://en.wiktionary.org/wiki/comment#Verb");

            URL_Add("daughter", "https://en.wiktionary.org/wiki/daughter#Noun");

            URL_Add("essentially", "https://en.wiktionary.org/wiki/essentially#Adverb");

            URL_Add("formatting", "https://en.wiktionary.org/wiki/formatting#Noun");

            URL_Add("frame rate", "https://en.wiktionary.org/wiki/frame_rate#Noun");

            URL_Add("independently", "https://en.wiktionary.org/wiki/independently#Adverb");

            URL_Add("maneuver", "https://en.wiktionary.org/wiki/maneuver#Noun");

            URL_Add("mindset", "https://en.wiktionary.org/wiki/mindset#Noun");

            URL_Add("negative", "https://en.wiktionary.org/wiki/negative#Adjective");

            URL_Add("opinion", "https://en.wiktionary.org/wiki/opinion#Noun");

            URL_Add("priority", "https://en.wiktionary.org/wiki/priority#Noun");

            URL_Add("project", "https://en.wiktionary.org/wiki/project#Noun");

            URL_Add("simultaneously", "https://en.wiktionary.org/wiki/simultaneously#Adverb");

            URL_Add("slightly", "https://en.wiktionary.org/wiki/slightly#Adverb");

            URL_Add("suppose", "https://en.wiktionary.org/wiki/suppose#Verb");

            URL_Add("sure", "https://en.wiktionary.org/wiki/sure#Adverb");

            URL_Add("vertical", "https://en.wiktionary.org/wiki/vertical#Adjective");

            URL_Add("you'll", "https://en.wiktionary.org/wiki/you%27ll#Contraction");

            URL_Add("Apollo", "https://en.wikipedia.org/wiki/Apollo_program");

            URL_Add("AppLocker", "https://en.wikipedia.org/wiki/AppLocker");

            URL_Add("Argentina", "https://en.wikipedia.org/wiki/Argentina");

            URL_Add("Artemis", "https://en.wikipedia.org/wiki/Artemis_Project");

            URL_Add("BuzzFeed", "https://en.wikipedia.org/wiki/BuzzFeed");

            URL_Add("CR2032", "https://en.wikipedia.org/wiki/Button_cell#Package_size");

            URL_Add("CSS&nbsp;2", "https://en.wikipedia.org/wiki/Cascading_Style_Sheets#CSS_2");

            URL_Add("Celsius", "https://en.wiktionary.org/wiki/Celsius#Adjective");

            URL_Add("EU", "https://en.wiktionary.org/wiki/EU#Proper_noun");

            URL_Add("England", "https://en.wikipedia.org/wiki/England");

            URL_Add("FYI", "https://en.wiktionary.org/wiki/FYI#Phrase");

            URL_Add("File Explorer", "https://en.wikipedia.org/wiki/File_Explorer");

            URL_Add("Fluke", "https://en.wikipedia.org/wiki/Fluke_Corporation");

            URL_Add("Gecko", "https://en.wikipedia.org/wiki/Gecko_(software)");

            URL_Add("Gibraltar", "https://en.wikipedia.org/wiki/Gibraltar");

            URL_Add("Kubuntu", "https://en.wikipedia.org/wiki/Kubuntu");

            URL_Add("Luke Skywalker", "https://en.wikipedia.org/wiki/Luke_Skywalker");

            URL_Add("Milky Way", "https://en.wikipedia.org/wiki/Milky_Way");

            URL_Add("Ninja", "https://ninja-build.org/manual.html");

            URL_Add("Northern Ireland", "https://en.wikipedia.org/wiki/Northern_Ireland");

            URL_Add("Ponzi scheme", "https://en.wikipedia.org/wiki/Ponzi_scheme");

            URL_Add("Sandboxie", "https://en.wikipedia.org/wiki/Sandboxie");

            URL_Add("Scotland", "https://en.wiktionary.org/wiki/Scotland#Proper_noun");

            URL_Add("SpaceX", "https://en.wikipedia.org/wiki/SpaceX");

            URL_Add("TCPView", "https://docs.microsoft.com/en-us/sysinternals/downloads/tcpview");

            URL_Add("Tera Term", "https://en.wikipedia.org/wiki/Tera_Term");

            URL_Add("Visual Studio Professional", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio#Professional");

            URL_Add("WTO", "https://en.wikipedia.org/wiki/World_Trade_Organization");

            URL_Add("Wales", "https://en.wikipedia.org/wiki/Wales");

            URL_Add("Xubuntu", "https://en.wikipedia.org/wiki/Xubuntu");

            URL_Add("address", "https://en.wiktionary.org/wiki/address#Noun");

            URL_Add("bear in mind", "https://en.wiktionary.org/wiki/bear_in_mind#Verb");

            URL_Add("bearing in mind", "https://en.wiktionary.org/wiki/bear_in_mind#Verb");

            URL_Add("bombardment", "https://en.wiktionary.org/wiki/bombardment#Noun");

            URL_Add("bona fide", "https://en.wiktionary.org/wiki/bona_fide#Adjective");

            URL_Add("brilliant", "https://en.wiktionary.org/wiki/brilliant#Adjective");

            URL_Add("coastal", "https://en.wiktionary.org/wiki/coastal#Adjective");

            URL_Add("collapse", "https://en.wiktionary.org/wiki/collapse#Verb");

            URL_Add("coming", "https://en.wiktionary.org/wiki/coming#Verb");

            URL_Add("competition", "https://en.wiktionary.org/wiki/competition#Noun");

            URL_Add("conscious", "https://en.wiktionary.org/wiki/concious");

            URL_Add("department", "https://en.wiktionary.org/wiki/department#Noun");

            URL_Add("encounter", "https://en.wiktionary.org/wiki/encounter#Verb");

            URL_Add("exactly", "https://en.wiktionary.org/wiki/exactly#Adverb");

            URL_Add("gamma-ray burst", "https://en.wikipedia.org/wiki/Gamma-ray_burst");

            URL_Add("genius", "https://en.wiktionary.org/wiki/genious#Noun");

            URL_Add("geniuses", "https://en.wiktionary.org/wiki/genious#Noun");

            URL_Add("hydrazine", "https://en.wikipedia.org/wiki/Hydrazine");

            URL_Add("inherently", "https://en.wiktionary.org/wiki/inherently#Adverb");

            URL_Add("inspecting", "https://en.wiktionary.org/wiki/inspect#Verb");

            URL_Add("instruction", "https://en.wiktionary.org/wiki/instruction#Noun");

            URL_Add("km", "https://en.wiktionary.org/wiki/km#Abbreviation");

            URL_Add("latest", "https://en.wiktionary.org/wiki/latest#Adjective");

            URL_Add("lithium", "https://en.wikipedia.org/wiki/Lithium");

            URL_Add("m", "https://en.wikipedia.org/wiki/Metre");

            URL_Add("macOS", "https://en.wikipedia.org/wiki/MacOS");

            URL_Add("netstat", "https://en.wikipedia.org/wiki/Netstat");

            URL_Add("newcomer", "https://en.wiktionary.org/wiki/newcomer#Noun");

            URL_Add("no-brainer", "https://en.wiktionary.org/wiki/no-brainer#Noun");

            URL_Add("on-premises", "https://en.wikipedia.org/wiki/On-premises_software");

            URL_Add("outputting", "https://en.wiktionary.org/wiki/output#Verb");

            URL_Add("pH", "https://en.wiktionary.org/wiki/pH#Symbol");

            URL_Add("per se", "https://en.wiktionary.org/wiki/per_se#Adverb");

            URL_Add("preceding", "https://en.wiktionary.org/wiki/preceding#Adjective");

            URL_Add("prestigious", "https://en.wiktionary.org/wiki/prestigious#Adjective");

            URL_Add("psychological", "https://en.wiktionary.org/wiki/psychological#Adjective");

            URL_Add("remember", "https://en.wiktionary.org/wiki/remember#Verb");

            URL_Add("rewritten", "https://en.wiktionary.org/wiki/rewritten#Verb");

            URL_Add("roughly", "https://en.wiktionary.org/wiki/roughly#Adverb");

            URL_Add("scalable", "https://en.wiktionary.org/wiki/scalable#Adjective");

            URL_Add("scroll", "https://en.wiktionary.org/wiki/scroll#Verb");

            URL_Add("sentence", "https://en.wiktionary.org/wiki/sentence#Noun");

            URL_Add("side effect", "https://en.wiktionary.org/wiki/side_effect#Noun");

            URL_Add("spellcheck", "https://en.wiktionary.org/wiki/spellcheck#English");

            URL_Add("struggling", "https://en.wiktionary.org/wiki/struggle#Verb");

            URL_Add("superfluous", "https://en.wiktionary.org/wiki/superfluous#Adjective");

            URL_Add("supervolcano", "https://en.wikipedia.org/wiki/Supervolcano");

            URL_Add("ultimately", "https://en.wiktionary.org/wiki/ultimately#Adverb");

            URL_Add("undergraduate", "https://en.wiktionary.org/wiki/undergraduate#Adjective");

            URL_Add("validate", "https://en.wiktionary.org/wiki/validate#Verb");

            URL_Add("visa", "https://en.wiktionary.org/wiki/visa#Noun");

            URL_Add("want", "https://en.wiktionary.org/wiki/want#Verb");

            URL_Add("workforce", "https://en.wiktionary.org/wiki/workforce#Noun");

            URL_Add("writable", "https://en.wiktionary.org/wiki/writable#Adjective");

            URL_Add(".NET Core 3.0", "https://en.wikipedia.org/wiki/.NET_Core#History");

            URL_Add("ASP.NET Core 3.0", "https://en.wikipedia.org/wiki/ASP.NET_Core#Release_history");

            URL_Add("ESLint", "https://en.wikipedia.org/wiki/ESLint");

            URL_Add("Elance", "https://en.wikipedia.org/wiki/Upwork");

            URL_Add("Material Design", "https://en.wikipedia.org/wiki/Material_Design");

            URL_Add("NTP", "https://en.wikipedia.org/wiki/Network_Time_Protocol");

            URL_Add("Pomodoro", "https://en.wikipedia.org/wiki/Pomodoro_Technique");

            URL_Add("RPC", "https://en.wikipedia.org/wiki/Remote_procedure_call");

            URL_Add("UFW", "https://en.wikipedia.org/wiki/Uncomplicated_Firewall");

            URL_Add("Visual&nbsp;Studio&nbsp;2019", "https://en.wikipedia.org/wiki/Microsoft_Visual_Studio#2019");

            URL_Add("XOR", "https://en.wikipedia.org/wiki/Exclusive_or");

            URL_Add("absorption", "https://en.wiktionary.org/wiki/absorption#Noun");

            URL_Add("arXiv", "https://en.wikipedia.org/wiki/ArXiv");

            URL_Add("Buddhist", "https://en.wiktionary.org/wiki/Buddhist#Noun");

            URL_Add("calculation", "https://en.wiktionary.org/wiki/calculation#Noun");

            URL_Add("child", "https://en.wiktionary.org/wiki/child#Noun");

            URL_Add("construct", "https://en.wiktionary.org/wiki/construct#Verb");

            URL_Add("counterproductive", "https://en.wiktionary.org/wiki/counterproductive#Adjective");

            URL_Add("effort", "https://en.wiktionary.org/wiki/effort#Noun");

            URL_Add("fifth", "https://en.wiktionary.org/wiki/fifth#Adjective");

            URL_Add("greater", "https://en.wiktionary.org/wiki/greater#Adjective");

            URL_Add("individual", "https://en.wiktionary.org/wiki/individual#Adjective");

            URL_Add("never mind", "https://en.wiktionary.org/wiki/never_mind#Verb");

            URL_Add("procrastination", "https://en.wiktionary.org/wiki/procrastination#Noun");

            URL_Add("B2B", "https://en.wikipedia.org/wiki/Business-to-business");

            URL_Add("Baidu", "https://en.wikipedia.org/wiki/Baidu");

            URL_Add("Muslim", "https://en.wiktionary.org/wiki/Muslim#Noun");

            URL_Add("PyVISA", "https://pyvisa.readthedocs.io/en/latest/");

            URL_Add("Samsung", "https://en.wikipedia.org/wiki/Samsung");

            URL_Add("Unix & Linux", "https://unix.stackexchange.com/tour");

            URL_Add("White House", "https://en.wikipedia.org/wiki/White_House");

            URL_Add("communicating", "https://en.wiktionary.org/wiki/communicate#Verb");

            URL_Add("lobbyists", "https://en.wiktionary.org/wiki/lobbyist#Noun");

            URL_Add("manually", "https://en.wiktionary.org/wiki/manually#Adverb");

            URL_Add("multimeter", "https://en.wikipedia.org/wiki/Multimeter");

            URL_Add("secure", "https://en.wiktionary.org/wiki/secure#Adjective");

            URL_Add("stuck", "https://en.wiktionary.org/wiki/stick#Verb_2");

            URL_Add("switch", "https://en.wiktionary.org/wiki/switch#Noun");

            URL_Add("yak shaving", "https://en.wiktionary.org/wiki/yak_shaving#Noun");

            URL_Add("CSS grid", "https://en.wikipedia.org/wiki/CSS_grid_layout");

            URL_Add("Amazon CloudFront", "https://en.wikipedia.org/wiki/Amazon_CloudFront");

            URL_Add("Creative Commons", "https://en.wikipedia.org/wiki/Creative_Commons");

            URL_Add("Flutter", "https://en.wikipedia.org/wiki/Flutter_(software)");

            URL_Add("Hawaii", "https://en.wikipedia.org/wiki/Hawaii");

            URL_Add("Hong Kong", "https://en.wikipedia.org/wiki/Hong_Kong");

            URL_Add("Komodo IDE", "https://en.wikipedia.org/wiki/Komodo_IDE");

            URL_Add("Puerto Rico", "https://en.wikipedia.org/wiki/Puerto_Rico");

            URL_Add("Trump", "https://en.wikipedia.org/wiki/Donald_Trump");

            URL_Add("Yugoslavia", "https://en.wikipedia.org/wiki/Yugoslavia");

            URL_Add("abate", "https://en.wiktionary.org/wiki/abate#Verb");

            URL_Add("acre", "https://en.wiktionary.org/wiki/acre#Noun");

            URL_Add("activity", "https://en.wiktionary.org/wiki/activity#Noun");

            URL_Add("annually", "https://en.wiktionary.org/wiki/annually#Adverb");

            URL_Add("backtick", "https://en.wiktionary.org/wiki/backtick#Noun");

            URL_Add("bunch", "https://en.wiktionary.org/wiki/bunch#Noun");

            URL_Add("exaggerated", "https://en.wiktionary.org/wiki/exaggerated#Adjective");

            URL_Add("expectation", "https://en.wiktionary.org/wiki/expectation#Noun");

            URL_Add("incidentally", "https://en.wiktionary.org/wiki/incidentally#Adverb");

            URL_Add("medieval", "https://en.wiktionary.org/wiki/medieval#Adjective");

            URL_Add("misspelt", "https://en.wiktionary.org/wiki/misspelt");

            URL_Add("preference", "https://en.wiktionary.org/wiki/preference#Noun");

            URL_Add("profitable", "https://en.wiktionary.org/wiki/profitable#Adjective");

            URL_Add("receding", "https://en.wiktionary.org/wiki/recede#Verb");

            URL_Add("stopped", "https://en.wiktionary.org/wiki/stop#Verb");

            URL_Add("strategic", "https://en.wiktionary.org/wiki/strategic#Adjective");

            URL_Add("suggest", "https://en.wiktionary.org/wiki/suggest#Verb");

            URL_Add("top-level domain", "https://en.wikipedia.org/wiki/Top-level_domain");

            URL_Add("transfer", "https://en.wiktionary.org/wiki/transfer#Noun");

            URL_Add("trillion", "https://en.wiktionary.org/wiki/trillion#Numeral");

            URL_Add("usage", "https://en.wiktionary.org/wiki/usage#Noun");

            URL_Add("OneDrive", "https://en.wikipedia.org/wiki/OneDrive");

            URL_Add("Apache Spark", "https://en.wikipedia.org/wiki/Apache_Spark");

            URL_Add("ECMAScript&nbsp;5", "https://en.wikipedia.org/wiki/ECMAScript#5th_Edition");

            URL_Add("Israel", "https://en.wikipedia.org/wiki/Israel");

            URL_Add("Jerusalem", "https://en.wiktionary.org/wiki/Jerusalem#Proper_noun");

            URL_Add("Kaggle", "https://en.wikipedia.org/wiki/Kaggle");

            URL_Add("Kindle", "https://en.wikipedia.org/wiki/Amazon_Kindle");

            URL_Add("MW", "https://en.wikipedia.org/wiki/Watt#Megawatt");

            URL_Add("Nobel Prize", "https://en.wikipedia.org/wiki/Nobel_Prize");

            URL_Add("abilities", "https://en.wiktionary.org/wiki/ability#Noun");

            URL_Add("advertise", "https://en.wiktionary.org/wiki/advertise#Verb");

            URL_Add("capacitance", "https://en.wikipedia.org/wiki/Capacitance");

            URL_Add("cost-effective", "https://en.wiktionary.org/wiki/cost-effective");

            URL_Add("encryption", "https://en.wiktionary.org/wiki/encryption#Noun");

            URL_Add("guesswork", "https://en.wiktionary.org/wiki/guesswork#Noun");

            URL_Add("important", "https://en.wiktionary.org/wiki/important#Adjective");

            URL_Add("interested", "https://en.wiktionary.org/wiki/interested#Adjective");

            URL_Add("iodine", "https://en.wikipedia.org/wiki/Iodine");

            URL_Add("irreplaceable", "https://en.wiktionary.org/wiki/irreplacable");

            URL_Add("literally", "https://en.wiktionary.org/wiki/literally#Adverb");

            URL_Add("maintainable", "https://en.wiktionary.org/wiki/maintainable#Adjective");

            URL_Add("object-oriented", "https://en.wikipedia.org/wiki/Object-orientation");

            URL_Add("obsolete", "https://en.wiktionary.org/wiki/obsolete#Adjective");

            URL_Add("paramount", "https://en.wiktionary.org/wiki/paramount#Adjective");

            URL_Add("power plant", "https://en.wiktionary.org/wiki/power_plant#Noun");

            URL_Add("publishing", "https://en.wiktionary.org/wiki/publish#Verb");

            URL_Add("quick-and-dirty", "https://en.wiktionary.org/wiki/quick-and-dirty#Adjective");

            URL_Add("recipient", "https://en.wiktionary.org/wiki/recipient#Noun");

            URL_Add("requirement", "https://en.wiktionary.org/wiki/requirement#Noun");

            URL_Add("vulnerable", "https://en.wiktionary.org/wiki/vulnerable#Adjective");

            URL_Add("ADHD", "https://en.wikipedia.org/wiki/Attention_deficit_hyperactivity_disorder");

            URL_Add("CRC-32", "https://en.wikipedia.org/wiki/Cyclic_redundancy_check#CRC-32_algorithm");

            URL_Add("ECMAScript&nbsp;7", "https://en.wikipedia.org/wiki/ECMAScript#7th_Edition_-_ECMAScript_2016");

            URL_Add("October", "https://en.wiktionary.org/wiki/October#Proper_noun");

            URL_Add("SWI-Prolog", "https://en.wikipedia.org/wiki/SWI-Prolog");

            URL_Add("UEStudio", "https://en.wikipedia.org/wiki/UltraEdit#UEStudio");

            URL_Add("as well", "https://en.wiktionary.org/wiki/as_well#Adverb");

            URL_Add("documented", "https://en.wiktionary.org/wiki/document#Verb");

            URL_Add("experienced", "https://en.wiktionary.org/wiki/experienced#Adjective");

            URL_Add("opening", "https://en.wiktionary.org/wiki/open#Verb");

            URL_Add("request", "https://en.wiktionary.org/wiki/request#Noun");

            URL_Add("temporarily", "https://en.wiktionary.org/wiki/temporarily#Adverb");

            URL_Add("weight", "https://en.wiktionary.org/wiki/weight#Noun");

            URL_Add("Belgium", "https://en.wikipedia.org/wiki/Belgium");

            URL_Add("BunsenLabs (Debian Linux-based)", "https://en.wikipedia.org/wiki/CrunchBang_Linux#BunsenLabs");

            URL_Add("CodePen", "https://en.wikipedia.org/wiki/CodePen");

            URL_Add("Emperor", "https://en.wikipedia.org/wiki/Emperor_of_Japan");

            URL_Add("WEP", "https://en.wikipedia.org/wiki/Wired_Equivalent_Privacy");

            URL_Add("anonymous", "https://en.wiktionary.org/wiki/anonymous#Adjective");

            URL_Add("approved", "https://en.wiktionary.org/wiki/approved#Adjective");

            URL_Add("assigning", "https://en.wiktionary.org/wiki/assign#Verb");

            URL_Add("cookies", "https://en.wikipedia.org/wiki/HTTP_cookie");

            URL_Add("hiding", "https://en.wiktionary.org/wiki/hide#Verb");

            URL_Add("in house", "https://en.wiktionary.org/wiki/in_house#Adjective");

            URL_Add("know", "https://en.wiktionary.org/wiki/know#Verb");

            URL_Add("menu", "https://en.wikipedia.org/wiki/Menu_%28computing%29");

            URL_Add("missing", "https://en.wiktionary.org/wiki/missing#Adjective");

            URL_Add("premise", "https://en.wiktionary.org/wiki/premise#Noun");

            URL_Add("prompt", "https://en.wiktionary.org/wiki/prompt#Noun");

            URL_Add("setting", "https://en.wiktionary.org/wiki/set#Verb");

            URL_Add("supposed", "https://en.wiktionary.org/wiki/supposed#Adjective");

            URL_Add("time zone", "https://en.wiktionary.org/wiki/time_zone#Noun");

            URL_Add("BusyBox", "https://en.wikipedia.org/wiki/BusyBox");

            URL_Add("Iris data set", "https://en.wikipedia.org/wiki/Iris_flower_data_set");

            URL_Add("KHTML", "https://en.wikipedia.org/wiki/KHTML");

            URL_Add("MSysGit", "https://github.com/msysgit/");

            URL_Add("Netscape", "https://en.wikipedia.org/wiki/Netscape_(web_browser)");

            URL_Add("SSE2", "https://en.wikipedia.org/wiki/SSE2");

            URL_Add("Toybox", "https://en.wikipedia.org/wiki/Toybox");

            URL_Add("constant", "https://en.wiktionary.org/wiki/constant#Noun");

            URL_Add("data set", "https://en.wiktionary.org/wiki/data_set#Noun");

            URL_Add("delimiter", "https://en.wiktionary.org/wiki/delimiter#Noun");

            URL_Add("democracy", "https://en.wiktionary.org/wiki/democracy#Noun");

            URL_Add("en masse", "https://en.wiktionary.org/wiki/en_masse#Adverb");

            URL_Add("flagging", "https://en.wiktionary.org/wiki/flag#Verb");

            URL_Add("flexibilty", "https://en.wiktionary.org/wiki/flexibility#Noun");

            URL_Add("illiterate", "https://en.wiktionary.org/wiki/illiterate#Adjective");

            URL_Add("multiple", "https://en.wiktionary.org/wiki/multiple#Adjective");

            URL_Add("pronounce", "https://en.wiktionary.org/wiki/pronounce#Verb");

            URL_Add("quitting", "https://en.wiktionary.org/wiki/quit#Verb");

            URL_Add("response", "https://en.wiktionary.org/wiki/response#Noun");

            URL_Add("reuse", "https://en.wiktionary.org/wiki/reuse#Verb");

            URL_Add("separating", "https://en.wiktionary.org/wiki/separating#Adjective");

            URL_Add("Cloudflare", "https://en.wikipedia.org/wiki/Cloudflare");

            URL_Add("Debian&nbsp;10 (Buster)", "https://en.wikipedia.org/wiki/Debian_version_history#Debian_10_(Buster)");

            URL_Add("Discord", "https://en.wikipedia.org/wiki/Discord_(software)");

            URL_Add("HFS", "https://en.wikipedia.org/wiki/Hierarchical_File_System");

            URL_Add("HFS Plus", "https://en.wikipedia.org/wiki/HFS_Plus");

            URL_Add("PL/I", "https://en.wikipedia.org/wiki/PL/I");

            URL_Add("XFS", "https://en.wikipedia.org/wiki/XFS");

            URL_Add("class", "https://en.wiktionary.org/wiki/class#Noun");

            URL_Add("here's", "https://en.wiktionary.org/wiki/here%27s#Contraction");

            URL_Add("idea", "https://en.wiktionary.org/wiki/idea#Noun");

            URL_Add("incoming", "https://en.wiktionary.org/wiki/incoming#Adjective");

            URL_Add("plagiarism", "https://en.wiktionary.org/wiki/plagiarism#Noun");

            URL_Add("proxy", "https://en.wikipedia.org/wiki/Proxy_server");

            URL_Add("ransomware", "https://en.wikipedia.org/wiki/Ransomware");

            URL_Add("receiver", "https://en.wiktionary.org/wiki/receiver#Noun");

            URL_Add("unbeknownst", "https://en.wiktionary.org/wiki/unbeknownst#Adverb");

            URL_Add("Hello, World!", "https://en.wikipedia.org/wiki/%22Hello,_World!%22_program"); // Old: http://en.wikipedia.org/wiki/%22Hello,_world!%22_program

            URL_Add("social media", "https://en.wikipedia.org/wiki/Social_media");

            URL_Add("bottom", "https://en.wiktionary.org/wiki/bottom#Noun");

            URL_Add("Apache Hive", "https://en.wikipedia.org/wiki/Apache_Hive");

            URL_Add("GeForce", "https://en.wikipedia.org/wiki/GeForce");

            URL_Add("Get-Unique", "https://docs.microsoft.com/en-us/PowerShell/module/microsoft.powershell.utility/Get-Unique");

            URL_Add("Interpersonal Skills", "https://interpersonal.stackexchange.com/tour");

            URL_Add("Kerberos", "https://en.wikipedia.org/wiki/Kerberos_%28protocol%29");

            URL_Add("Lightbox", "https://en.wikipedia.org/wiki/Lightbox_(JavaScript)");

            URL_Add("MERN", "https://en.wikipedia.org/wiki/Solution_stack");

            URL_Add("Neo4j", "https://en.wikipedia.org/wiki/Neo4j");

            URL_Add("Nigerian", "https://en.wiktionary.org/wiki/Nigerian#Adjective");

            URL_Add("RTOS", "https://en.wikipedia.org/wiki/Real-time_operating_system");

            URL_Add("Start-Transcript", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.host/start-transcript");

            URL_Add("Stop-Transcript", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.host/stop-transcript");

            URL_Add("USA", "https://en.wikipedia.org/wiki/United_States");

            URL_Add("VIC-20", "https://en.wikipedia.org/wiki/Commodore_VIC-20");

            URL_Add("VPC", "https://en.wikipedia.org/wiki/Virtual_private_cloud");

            URL_Add("accomplish", "https://en.wiktionary.org/wiki/accomplish#Verb");

            URL_Add("alienation", "https://en.wiktionary.org/wiki/alienation#Noun");

            URL_Add("ancient", "https://en.wiktionary.org/wiki/ancient#Adjective");

            URL_Add("anyone's guess", "https://en.wiktionary.org/wiki/anyone%27s_guess#Noun");

            URL_Add("button", "https://en.wiktionary.org/wiki/button#Noun");

            URL_Add("command-line", "https://en.wikipedia.org/wiki/Command-line_interface");

            URL_Add("compilation", "https://en.wiktionary.org/wiki/compilation#Noun");

            URL_Add("correctness", "https://en.wiktionary.org/wiki/correctness#Noun");

            URL_Add("data point", "https://en.wiktionary.org/wiki/data_point#Noun");

            URL_Add("dimensional", "https://en.wiktionary.org/wiki/dimensional#Adjective");

            URL_Add("eloquently", "https://en.wiktionary.org/wiki/eloquently#Adverb");

            URL_Add("getting", "https://en.wiktionary.org/wiki/get#Verb");

            URL_Add("goal", "https://en.wiktionary.org/wiki/goal#Noun");

            URL_Add("impose", "https://en.wiktionary.org/wiki/impose#Verb");

            URL_Add("installing", "https://en.wiktionary.org/wiki/install#Verb");

            URL_Add("inviable", "https://en.wiktionary.org/wiki/inviable#Adjective");

            URL_Add("like", "https://en.wiktionary.org/wiki/like#Adverb");

            URL_Add("mathematician", "https://en.wiktionary.org/wiki/mathematician#Noun");

            URL_Add("mitigate", "https://en.wiktionary.org/wiki/mitigate#Verb");

            URL_Add("nuisance", "https://en.wiktionary.org/wiki/nuisance#Noun");

            URL_Add("occur", "https://en.wiktionary.org/wiki/occur#Verb");

            URL_Add("oxygen", "https://en.wikipedia.org/wiki/Oxygen");

            URL_Add("permanent", "https://en.wiktionary.org/wiki/permanent#Adjective");

            URL_Add("physics", "https://en.wikipedia.org/wiki/Physics");

            URL_Add("placeholder", "https://en.wiktionary.org/wiki/placeholder#Noun");

            URL_Add("plagiarise", "https://en.wiktionary.org/wiki/plagiarise#Verb");

            URL_Add("pronoun", "https://en.wiktionary.org/wiki/pronoun#Noun");

            URL_Add("property", "https://en.wiktionary.org/wiki/property#Noun");

            URL_Add("quantum mechanics", "https://en.wikipedia.org/wiki/Quantum_mechanics");

            URL_Add("quotation mark", "https://en.wiktionary.org/wiki/quotation_mark#Noun");

            URL_Add("quote", "https://en.wiktionary.org/wiki/quote#Noun");

            URL_Add("recessive", "https://en.wiktionary.org/wiki/recessive#Adjective");

            URL_Add("resistance", "https://en.wiktionary.org/wiki/resistance#Noun");

            URL_Add("restructure", "https://en.wiktionary.org/wiki/restructure#Verb");

            URL_Add("screenful", "https://en.wiktionary.org/wiki/screenful#Noun");

            URL_Add("simple", "https://en.wiktionary.org/wiki/simple#Adjective");

            URL_Add("sound card", "https://en.wikipedia.org/wiki/Sound_card");

            URL_Add("suggestion", "https://en.wiktionary.org/wiki/suggestion#Noun");

            URL_Add("Friday", "https://en.wiktionary.org/wiki/Friday#Noun");

            URL_Add("GNU Make", "https://en.wikipedia.org/wiki/Make_(software)#Derivatives");

            URL_Add("GitWeb", "https://git-scm.com/book/en/v2/Git-on-the-Server-GitWeb");

            URL_Add("O'Reilly", "https://en.wikipedia.org/wiki/O%27Reilly_Media");

            URL_Add("Set-Location", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/set-location");

            URL_Add("Split-Path", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/split-path");

            URL_Add("Teachers' Lounge", "https://meta.stackexchange.com/questions/tagged/teachers-lounge");

            URL_Add("Test-Path", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/test-path");

            URL_Add("Transient voltage suppressor", "https://en.wikipedia.org/wiki/Transient_voltage_suppressor");

            URL_Add("WinRM", "https://en.wikipedia.org/wiki/Windows_Remote_Management");

            URL_Add("according", "https://en.wiktionary.org/wiki/according#Adverb");

            URL_Add("coerce", "https://en.wiktionary.org/wiki/coerce#Verb");

            URL_Add("coercive", "https://en.wiktionary.org/wiki/coercive#Adjective");

            URL_Add("contrived", "https://en.wiktionary.org/wiki/contrived#Adjective");

            URL_Add("crash", "https://en.wiktionary.org/wiki/crash#Verb");

            URL_Add("day-to-day", "https://en.wiktionary.org/wiki/day-to-day#Adjective");

            URL_Add("emigrate", "https://en.wiktionary.org/wiki/emigrate#Verb");

            URL_Add("everybody", "https://en.wiktionary.org/wiki/everybody#Pronoun");

            URL_Add("hilarious", "https://en.wiktionary.org/wiki/hilarious#Adjective");

            URL_Add("irrelevant", "https://en.wiktionary.org/wiki/irrelevant#Adjective");

            URL_Add("league", "https://en.wiktionary.org/wiki/league#Noun");

            URL_Add("liar", "https://en.wiktionary.org/wiki/liar#Noun");

            URL_Add("likelihood", "https://en.wiktionary.org/wiki/likelihood#Noun");

            URL_Add("monetisation", "https://en.wiktionary.org/wiki/monetisation");

            URL_Add("onwards", "https://en.wiktionary.org/wiki/onwards#Adverb");

            URL_Add("p-channel MOSFET", "https://en.wikipedia.org/wiki/MOSFET#PMOS_and_NMOS_logic");

            URL_Add("pro bono", "https://en.wiktionary.org/wiki/pro_bono#Adjective");

            URL_Add("return", "https://en.wiktionary.org/wiki/return#Verb");

            URL_Add("stream", "https://en.wiktionary.org/wiki/stream#Noun");

            URL_Add("synthetise", "https://en.wiktionary.org/wiki/synthetise");

            URL_Add("transparency", "https://en.wiktionary.org/wiki/transparency#Noun");

            URL_Add("Get-Credential", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.security/get-credential");

            URL_Add("New-Module", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/new-module");

            URL_Add("Ohm's law", "https://en.wikipedia.org/wiki/Ohm's_law");

            URL_Add("PHP Composer", "https://en.wikipedia.org/wiki/Composer_(software)");

            URL_Add("PII", "https://en.wikipedia.org/wiki/Personal_data");

            URL_Add("Send-MailMessage", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/send-mailmessage");

            URL_Add("Xdebug", "https://en.wikipedia.org/wiki/Xdebug");

            URL_Add("account", "https://en.wiktionary.org/wiki/account#Noun");

            URL_Add("contributor", "https://en.wiktionary.org/wiki/contributor#Noun");

            URL_Add("equal", "https://en.wiktionary.org/wiki/equal#Adjective");

            URL_Add("iff", "https://en.wiktionary.org/wiki/iff#Conjunction");

            URL_Add("macOS v10.15 (Catalina)", "https://en.wikipedia.org/wiki/MacOS_Catalina");

            URL_Add("nonsense", "https://en.wiktionary.org/wiki/nonsense#Noun");

            URL_Add("thorough", "https://en.wiktionary.org/wiki/thorough#Adjective");

            URL_Add("unnecessarily", "https://en.wiktionary.org/wiki/unnecessarily#Adverb");

            URL_Add("usually", "https://en.wiktionary.org/wiki/usually#Adverb");

            URL_Add("Britain", "https://en.wiktionary.org/wiki/Britain#Proper_noun");

            URL_Add("Tory", "https://en.wiktionary.org/wiki/Tory#Noun");

            URL_Add("besieged", "https://en.wiktionary.org/wiki/besiege#Verb");

            URL_Add("colonialism", "https://en.wiktionary.org/wiki/colonialism#Noun");

            URL_Add("colonialist", "https://en.wiktionary.org/wiki/colonialist#Adjective");

            URL_Add("colonies", "https://en.wiktionary.org/wiki/colony#Noun");

            URL_Add("empire", "https://en.wiktionary.org/wiki/empire#Noun");

            URL_Add("imprisoned", "https://en.wiktionary.org/wiki/imprison#Verb");

            URL_Add("independence", "https://en.wiktionary.org/wiki/independence#Noun");

            URL_Add("king", "https://en.wiktionary.org/wiki/king#Noun");

            URL_Add("offered", "https://en.wiktionary.org/wiki/offer#Verb");

            URL_Add("prime minister", "https://en.wiktionary.org/wiki/prime_minister#Noun");

            URL_Add("queen", "https://en.wiktionary.org/wiki/queen#Noun");

            URL_Add("royal", "https://en.wiktionary.org/wiki/royal#Adjective");

            URL_Add("scientific", "https://en.wiktionary.org/wiki/scientific#Adjective");


            URL_Add("Bower", "https://bower.io/");

            URL_Add("Cherry MX", "https://en.wikipedia.org/wiki/Cherry_(keyboards)#Cherry_MX_switches_in_consumer_keyboards");

            URL_Add("GTK", "https://en.wikipedia.org/wiki/GTK");

            URL_Add("Indiegogo", "https://en.wikipedia.org/wiki/Indiegogo");

            URL_Add("Keil", "https://en.wikipedia.org/wiki/Keil_(company)");

            URL_Add("LuaMacros", "https://github.com/me2d13/luamacros");

            URL_Add("Mars", "https://en.wikipedia.org/wiki/Mars");

            URL_Add("Razer BlackWidow", "https://www.razer.com/gaming-keyboards-keypads/razer-blackwidow");

            URL_Add("TVS Electronics", "https://en.wikipedia.org/wiki/TVS_Electronics");

            URL_Add("nonmechanical", "https://en.wiktionary.org/wiki/nonmechanical#Adjective");

            URL_Add("problem", "https://en.wiktionary.org/wiki/problem#Noun");

            URL_Add("support", "https://en.wiktionary.org/wiki/support#Noun");

            URL_Add("FLTK", "https://en.wikipedia.org/wiki/FLTK");

            URL_Add("GitFlow", "https://www.gitflow.com/");

            URL_Add("Venn diagram", "https://en.wikipedia.org/wiki/Venn_diagram");

            URL_Add("difficult", "https://en.wiktionary.org/wiki/difficult#Adjective");

            URL_Add("everywhere", "https://en.wiktionary.org/wiki/everywhere#Adverb");

            URL_Add("key press", "https://en.wiktionary.org/wiki/key_press");

            URL_Add("space", "https://en.wiktionary.org/wiki/space#Noun");

            URL_Add("strategy pattern", "https://en.wikipedia.org/wiki/Strategy_pattern");

            URL_Add("Achilles heel", "https://en.wiktionary.org/wiki/Achilles_heel#Noun");

            URL_Add("Boeing 737 MAX", "https://en.wikipedia.org/wiki/Boeing_737_MAX");

            URL_Add("CodeLens", "https://docs.microsoft.com/en-us/visualstudio/ide/find-code-changes-and-other-history-with-codelens");

            URL_Add("Creative Commons BY-SA", "https://en.wikipedia.org/wiki/Creative_Commons_license#Seven_regularly_used_licenses");

            URL_Add("Führer", "https://en.wikipedia.org/wiki/F%C3%BChrer");

            URL_Add("Göring", "https://en.wikipedia.org/wiki/Hermann_G%C3%B6ring");

            URL_Add("LastPass", "https://en.wikipedia.org/wiki/LastPass");

            URL_Add("POW", "https://en.wiktionary.org/wiki/POW#Noun");

            URL_Add("Scapy", "https://en.wikipedia.org/wiki/Scapy");

            URL_Add("Southern", "https://en.wiktionary.org/wiki/Southern#Adjective");

            URL_Add("TOS", "https://en.wiktionary.org/wiki/TOS#Noun");

            URL_Add("aggressive", "https://en.wiktionary.org/wiki/aggressive#Adjective");

            URL_Add("anecdote", "https://en.wiktionary.org/wiki/anecdote#Noun");

            URL_Add("formatted", "https://en.wiktionary.org/wiki/formatted#Adjective");

            URL_Add("mediaeval", "https://en.wiktionary.org/wiki/mediaeval#Adjective");

            URL_Add("skilful", "https://en.wiktionary.org/wiki/skilful#Adjective");

            URL_Add("soviet", "https://en.wiktionary.org/wiki/soviet#Noun");

            URL_Add("theoretical", "https://en.wiktionary.org/wiki/theoretical#Adjective");

            URL_Add("voluntarily", "https://en.wiktionary.org/wiki/voluntarily#Adverb");

            URL_Add("Ethereum", "https://en.wikipedia.org/wiki/Ethereum");

            URL_Add("amend", "https://en.wiktionary.org/wiki/amend#Verb");

            URL_Add("corporation", "https://en.wiktionary.org/wiki/corporation#Noun");

            URL_Add("females", "https://en.wiktionary.org/wiki/female#Noun");

            URL_Add("women", "https://en.wiktionary.org/wiki/woman#Noun");

            URL_Add("Closure", "https://en.wikipedia.org/wiki/Closure_(computer_programming)");

            URL_Add("Fiverr", "https://en.wikipedia.org/wiki/Fiverr");

            URL_Add("Get-FileHash", "https://docs.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Get-FileHash");

            URL_Add("JMeter", "https://en.wikipedia.org/wiki/Apache_JMeter");

            URL_Add("MTP", "https://en.wikipedia.org/wiki/Media_Transfer_Protocol");

            URL_Add("New York", "https://en.wikipedia.org/wiki/New_York_City");

            URL_Add("OneNote", "https://en.wikipedia.org/wiki/Microsoft_OneNote");

            URL_Add("Press Key", "https://robotframework.org/SeleniumLibrary/SeleniumLibrary.html#Press%20Key");

            URL_Add("Press Keys", "https://robotframework.org/SeleniumLibrary/SeleniumLibrary.html#Press%20Keys");

            URL_Add("PySpark", "https://www.tutorialspoint.com/pyspark/index.htm");

            URL_Add("Star Trek", "https://en.wikipedia.org/wiki/Star_Trek");

            URL_Add("Starbucks", "https://en.wikipedia.org/wiki/Starbucks");

            URL_Add("VCO", "https://en.wikipedia.org/wiki/Voltage-controlled_oscillator");

            URL_Add("Write-Progress", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/write-progress");

            URL_Add("YOLO", "https://en.wikipedia.org/wiki/Object_detection#Methods");

            URL_Add("absence", "https://en.wiktionary.org/wiki/absence#Noun");

            URL_Add("bitwise", "https://en.wiktionary.org/wiki/bitwise#Adjective");

            URL_Add("choice", "https://en.wiktionary.org/wiki/choice#Noun");

            URL_Add("committing", "https://en.wiktionary.org/wiki/commit#Verb");

            URL_Add("complement", "https://en.wiktionary.org/wiki/complement#Noun");

            URL_Add("democratic", "https://en.wiktionary.org/wiki/democratic#Adjective");

            URL_Add("dereference", "https://en.wiktionary.org/wiki/dereference#Verb");

            URL_Add("elegant", "https://en.wiktionary.org/wiki/elegant#Adjective");

            URL_Add("emphasise", "https://en.wiktionary.org/wiki/emphasise#Verb");

            URL_Add("employee", "https://en.wiktionary.org/wiki/employee#Noun");

            URL_Add("execute", "https://en.wiktionary.org/wiki/execute#Verb");

            URL_Add("extremities", "https://en.wiktionary.org/wiki/extremity#Noun");

            URL_Add("fair", "https://en.wiktionary.org/wiki/fair#Adjective");

            URL_Add("illegal", "https://en.wiktionary.org/wiki/illegal#Adjective");

            URL_Add("index", "https://en.wiktionary.org/wiki/index#Noun");

            URL_Add("inside", "https://en.wiktionary.org/wiki/inside#Adverb");

            URL_Add("lounge", "https://en.wiktionary.org/wiki/lounge#Noun");

            URL_Add("monetise", "https://en.wiktionary.org/wiki/monetise#Verb");

            URL_Add("monkey patching", "https://en.wikipedia.org/wiki/Monkey_patch");

            URL_Add("neo-Nazi", "https://en.wiktionary.org/wiki/neo-Nazi#Noun");

            URL_Add("omit", "https://en.wiktionary.org/wiki/omit#Verb");

            URL_Add("organisation", "https://en.wiktionary.org/wiki/organisation#Noun");

            URL_Add("portfolio", "https://en.wiktionary.org/wiki/portfolio#Noun");

            URL_Add("recommending", "http://en.wiktionary.org/wiki/recommend");

            URL_Add("religious", "https://en.wiktionary.org/wiki/religious#Adjective");

            URL_Add("rescale", "https://en.wiktionary.org/wiki/rescale#Verb");

            URL_Add("review", "https://en.wiktionary.org/wiki/review#Verb");

            URL_Add("semicolon", "https://en.wiktionary.org/wiki/semicolon#Noun");

            URL_Add("snippet", "https://en.wiktionary.org/wiki/snippet#Noun");

            URL_Add("user-defined", "https://en.wiktionary.org/wiki/user-defined_function");

            URL_Add("wonderful", "https://en.wiktionary.org/wiki/wonderful#Adjective");

            URL_Add("editor", "https://en.wiktionary.org/wiki/editor#Noun");

            URL_Add("will", "https://en.wiktionary.org/wiki/will#Verb");

            URL_Add("Braintree", "https://en.wikipedia.org/wiki/Braintree_%28company%29");

            URL_Add("MGTOW", "https://en.wikipedia.org/wiki/Men_Going_Their_Own_Way");

            URL_Add("MTA", "https://en.wikipedia.org/wiki/Message_transfer_agent");

            URL_Add("PHPMailer", "https://en.wikipedia.org/wiki/PHPMailer");

            URL_Add("Postfix", "https://en.wikipedia.org/wiki/Postfix_(software)");

            URL_Add("Sendmail", "https://en.wikipedia.org/wiki/Sendmail");

            URL_Add("URL encode", "http://en.wikipedia.org/wiki/Percent-encoding");

            URL_Add("existent", "https://en.wiktionary.org/wiki/existent#Adjective");

            URL_Add("impersonation", "https://en.wiktionary.org/wiki/impersonation#Noun");

            URL_Add("latter", "https://en.wiktionary.org/wiki/latter#Adjective");

            URL_Add("localhost", "https://en.wikipedia.org/wiki/Localhost");

            URL_Add("portion", "https://en.wiktionary.org/wiki/portion#Noun");

            URL_Add("powers that be", "https://en.wiktionary.org/wiki/powers_that_be#Noun");

            URL_Add("segue", "https://en.wiktionary.org/wiki/segue#Verb");

            URL_Add("thing", "https://en.wiktionary.org/wiki/thing#Noun");

            URL_Add("too", "https://en.wiktionary.org/wiki/too#Adverb");

            URL_Add("destination", "https://en.wiktionary.org/wiki/destination#Noun");

            URL_Add("one-on-one", "https://en.wiktionary.org/wiki/one-on-one#Adjective");

            URL_Add("seem", "https://en.wiktionary.org/wiki/seem#Verb");

            URL_Add("PowerPC", "https://en.wikipedia.org/wiki/PowerPC");

            URL_Add("WoW64", "https://en.wikipedia.org/wiki/WoW64");

            URL_Add("allegedly", "https://en.wiktionary.org/wiki/allegedly#Adverb");

            URL_Add("continue", "https://en.wiktionary.org/wiki/continue#Verb");

            URL_Add("keeping", "https://en.wiktionary.org/wiki/keep#Verb");

            URL_Add("minimal", "https://en.wiktionary.org/wiki/minimal#Adjective");

            URL_Add("occasion", "https://en.wiktionary.org/wiki/occasion#Noun");

            URL_Add("internal field separator", "https://en.wikipedia.org/wiki/Internal_field_separator");

            URL_Add("invocation", "https://en.wiktionary.org/wiki/invocation#Noun");

            URL_Add("misstep", "https://en.wiktionary.org/wiki/misstep#Noun");

            URL_Add("quotation", "https://en.wiktionary.org/wiki/quotation#Noun");

            URL_Add("Arial", "https://en.wikipedia.org/wiki/Arial");

            URL_Add("CodeRush", "https://en.wikipedia.org/wiki/CodeRush");

            URL_Add("Geany", "https://en.wikipedia.org/wiki/Geany");

            URL_Add("Gerrit", "https://en.wikipedia.org/wiki/Gerrit_(software)");

            URL_Add("MUI React", "https://www.muicss.com/docs/v1/react/introduction");

            URL_Add("Material-UI", "https://material-ui.com/");

            URL_Add("TechNet", "https://en.wikipedia.org/wiki/Microsoft_TechNet");

            URL_Add("Test-Connection", "https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/test-connection");

            URL_Add("Test-NetConnection", "https://docs.microsoft.com/en-us/powershell/module/nettcpip/test-netconnection");

            URL_Add("TextEdit", "https://en.wikipedia.org/wiki/TextEdit");

            URL_Add("Android X", "https://developer.android.com/jetpack/androidx");

            URL_Add("beautiful", "https://en.wiktionary.org/wiki/beautiful#Adjective");

            URL_Add("gotcha", "https://en.wiktionary.org/wiki/gotcha#Noun");

            URL_Add("Telegram Messenger", "https://en.wikipedia.org/wiki/Telegram_(software)");

            URL_Add("HyperTerminal", "https://en.wikipedia.org/wiki/HyperACCESS");

            URL_Add("Portmon", "https://docs.microsoft.com/en-us/sysinternals/downloads/portmon");

            URL_Add("USB On-The-Go", "https://en.wikipedia.org/wiki/USB_On-The-Go");

            URL_Add("anode", "https://en.wikipedia.org/wiki/Anode");

            URL_Add("asterisk", "https://en.wiktionary.org/wiki/asterisk#Noun");

            URL_Add("certain", "https://en.wiktionary.org/wiki/certain#Adjective");

            URL_Add("collector", "https://en.wikipedia.org/wiki/Bipolar_junction_transistor#Structure");

            URL_Add("drain", "https://en.wikipedia.org/wiki/Field-effect_transistor#Basic_information");

            URL_Add("encode", "https://en.wiktionary.org/wiki/encode#Verb");

            URL_Add("gauge", "https://en.wiktionary.org/wiki/gauge#Noun");

            URL_Add("happily", "https://en.wiktionary.org/wiki/happily#Adverb");

            URL_Add("lock down", "https://en.wiktionary.org/wiki/lock_down#Verb");

            URL_Add("measurable", "https://en.wiktionary.org/wiki/measurable#Adjective");

            URL_Add("pick up", "https://en.wiktionary.org/wiki/pick_up#Verb");

            URL_Add("precisely", "https://en.wiktionary.org/wiki/precisely#Adverb");

            URL_Add("thyristor", "https://en.wikipedia.org/wiki/Thyristor");

            URL_Add("RSA", "https://en.wikipedia.org/wiki/RSA_(cryptosystem)");

            URL_Add("dos2unix", "https://en.wikipedia.org/wiki/Unix2dos");

            URL_Add("line feed", "https://en.wiktionary.org/wiki/line_feed#Noun");

            URL_Add("photoshopping", "https://en.wiktionary.org/wiki/photoshop#Verb");

            URL_Add("unix2dos", "https://en.wikipedia.org/wiki/Unix2dos");

            URL_Add("Cmder", "https://cmder.net/");

            URL_Add("WebDAV", "https://en.wikipedia.org/wiki/WebDAV");

            URL_Add("clean up", "https://en.wiktionary.org/wiki/clean_up#Verb");

            URL_Add("surely", "https://en.wiktionary.org/wiki/surely#Adverb");

            URL_Add("therefore", "https://en.wiktionary.org/wiki/therefore#Adverb");

            URL_Add("Alpine Linux", "https://en.wikipedia.org/wiki/Alpine_Linux");

            URL_Add("Elixir", "https://en.wikipedia.org/wiki/Elixir_(programming_language)");

            URL_Add("pressing", "https://en.wiktionary.org/wiki/press#Verb");

            URL_Add("AC-3", "https://en.wikipedia.org/wiki/Dolby_Digital");

            URL_Add("Bob's your uncle", "https://en.wiktionary.org/wiki/Bob%27s_your_uncle#Phrase");

            URL_Add("DTS", "https://en.wikipedia.org/wiki/DTS_(sound_system)#DTS_audio_codec");

            URL_Add("SoundCloud", "https://en.wikipedia.org/wiki/SoundCloud");

            URL_Add("candidate", "https://en.wiktionary.org/wiki/candidate#Noun");

            URL_Add("connection", "https://en.wiktionary.org/wiki/connection#Noun");

            URL_Add("containing", "https://en.wiktionary.org/wiki/contain#Verb");

            URL_Add("in spite of", "https://en.wiktionary.org/wiki/in_spite_of#Preposition");

            URL_Add("rewrote", "https://en.wiktionary.org/wiki/rewrite#Verb");

            URL_Add("verified", "https://en.wiktionary.org/wiki/verify#Verb");

            URL_Add("IMDb", "https://en.wikipedia.org/wiki/IMDb");

            URL_Add("clear cut", "https://en.wiktionary.org/wiki/clear_cut#Adjective");

            URL_Add("corresponding", "https://en.wiktionary.org/wiki/correspond#Verb");

            URL_Add("subtract", "https://en.wiktionary.org/wiki/subtract#Verb");

            URL_Add("Bouncy Castle", "https://en.wikipedia.org/wiki/Bouncy_Castle_(cryptography)");

            URL_Add("Perl Programming Documentation", "https://en.wikipedia.org/wiki/Perl_Programming_Documentation");

            URL_Add("average", "https://en.wiktionary.org/wiki/average#Noun");

            URL_Add("package", "https://en.wiktionary.org/wiki/package#Noun");

            URL_Add("strace", "https://en.wikipedia.org/wiki/Strace");

            URL_Add("timestamp", "https://en.wiktionary.org/wiki/timestamp#Noun");

            URL_Add("Windows 10, 2018-04-10 Update (version 1803)", "https://en.wikipedia.org/wiki/Windows_10_version_history#Version_1803_(April_2018_Update)");

            URL_Add("Cross Validated", "https://stats.stackexchange.com/tour");

            URL_Add("ERP", "https://en.wikipedia.org/wiki/Enterprise_resource_planning");

            URL_Add("UWP", "https://en.wikipedia.org/wiki/Universal_Windows_Platform");

            URL_Add("access", "https://en.wiktionary.org/wiki/access#Noun");

            URL_Add("ncurses", "https://en.wikipedia.org/wiki/Ncurses");

            URL_Add("Apache Commons", "https://en.wikipedia.org/wiki/Apache_Commons");

            URL_Add("Google Guava", "https://en.wikipedia.org/wiki/Google_Guava");

            URL_Add("Java&nbsp;6", "https://en.wikipedia.org/wiki/Java_version_history#Java_SE_6");

            URL_Add("Java&nbsp;7", "https://en.wikipedia.org/wiki/Java_version_history#Java_SE_7");

            URL_Add("Java&nbsp;8", "https://en.wikipedia.org/wiki/Java_version_history#Java_SE_8");

            URL_Add("Mutt", "https://en.wikipedia.org/wiki/Mutt_(email_client)");

            URL_Add("Pico", "https://en.wikipedia.org/wiki/Pico_(text_editor)");

            URL_Add("Pine", "https://en.wikipedia.org/wiki/Pine_(email_client)");

            URL_Add("Screen", "https://en.wikipedia.org/wiki/GNU_Screen");

            URL_Add("WTF", "https://en.wiktionary.org/wiki/WTF#Phrase");

            URL_Add("Xtail", "http://manpages.ubuntu.com/manpages/trusty/man1/xtail.1.html");

            URL_Add("cfdisk", "https://en.wikipedia.org/wiki/Cfdisk");

            URL_Add("codec", "https://en.wikipedia.org/wiki/Codec");

            URL_Add("fdisk", "https://en.wikipedia.org/wiki/Fdisk");

            URL_Add("ifconfig", "https://en.wikipedia.org/wiki/Ifconfig");

            URL_Add("in other words", "https://en.wiktionary.org/wiki/IOW#Adverb");

            URL_Add("iproute2", "https://en.wikipedia.org/wiki/Iproute2");

            URL_Add("iwconfig", "https://en.wikipedia.org/wiki/Wireless_tools_for_Linux#iwconfig");

            URL_Add("locate", "https://en.wikipedia.org/wiki/Locate_(Unix)");

            URL_Add("nano", "https://en.wikipedia.org/wiki/GNU_nano");

            URL_Add("popd", "https://en.wikipedia.org/wiki/Pushd_and_popd");

            URL_Add("pushd", "https://en.wikipedia.org/wiki/Pushd_and_popd");

            URL_Add("route", "https://en.wikipedia.org/wiki/Route_(command)");

            URL_Add("tail", "https://en.wikipedia.org/wiki/Tail_(Unix)");

            URL_Add("tmux", "https://en.wikipedia.org/wiki/Tmux");

            URL_Add("verbiage", "https://en.wiktionary.org/wiki/verbiage#Noun");

            URL_Add("weekday", "https://en.wiktionary.org/wiki/weekday#Noun");

            URL_Add("PPM", "https://en.wikipedia.org/wiki/Perl_package_manager");

            URL_Add("PerlMonks", "https://en.wikipedia.org/wiki/PerlMonks");

            URL_Add("anything", "https://en.wiktionary.org/wiki/anything#Pronoun");

            URL_Add("bleeding-edge", "https://en.wiktionary.org/wiki/bleeding-edge#Adjective");

            URL_Add("developing", "https://en.wiktionary.org/wiki/develop#Verb");

            URL_Add("module", "https://en.wiktionary.org/wiki/module#Noun");

            URL_Add("Babel", "https://en.wikipedia.org/wiki/Babel_(compiler)");

            URL_Add("Bluehost", "https://en.wikipedia.org/wiki/Bluehost");

            URL_Add("Flink", "https://en.wikipedia.org/wiki/Apache_Flink");

            URL_Add("Next.js", "https://nextjs.org/");

            URL_Add("OptiX", "https://en.wikipedia.org/wiki/OptiX");

            URL_Add("PeachPie", "https://en.wikipedia.org/wiki/PeachPie");

            URL_Add("Sudoku", "https://en.wikipedia.org/wiki/Sudoku");

            URL_Add("absolutely", "https://en.wiktionary.org/wiki/absolutely#Adverb");

            URL_Add("artefact", "https://en.wiktionary.org/wiki/artefact#Noun");

            URL_Add("community", "https://en.wiktionary.org/wiki/community#Noun");

            URL_Add("easiest", "https://en.wiktionary.org/wiki/easiest#Adjective");

            URL_Add("except", "https://en.wiktionary.org/wiki/except#Conjunction");

            URL_Add("external", "https://en.wiktionary.org/wiki/external#Adjective");

            URL_Add("for the sake of", "https://en.wiktionary.org/wiki/for_the_sake_of#Preposition");

            URL_Add("integrate", "https://en.wiktionary.org/wiki/integrate#Verb");

            URL_Add("minimalistic", "https://en.wiktionary.org/wiki/minimalistic#Adjective");

            URL_Add("planning", "https://en.wiktionary.org/wiki/plan#Verb");

            URL_Add("rerun", "https://en.wiktionary.org/wiki/rerun#Verb");

            URL_Add("subtraction", "https://en.wiktionary.org/wiki/subtraction#Noun");

            URL_Add("when", "https://en.wiktionary.org/wiki/when#Adverb");

            URL_Add("custom", "https://en.wiktionary.org/wiki/custom#Adjective");

            URL_Add("folder", "https://en.wiktionary.org/wiki/folder#Noun");

            URL_Add("highlighting", "https://en.wiktionary.org/wiki/highlight#Verb");

            URL_Add("mysterious", "https://en.wiktionary.org/wiki/mysterious#Adjective");

            URL_Add("C++03", "https://en.wikipedia.org/wiki/C%2B%2B03");

            URL_Add("Norwegian", "https://en.wiktionary.org/wiki/Norwegian#Adjective");

            URL_Add("actively", "https://en.wiktionary.org/wiki/actively#Adverb");

            URL_Add("colour", "https://en.wiktionary.org/wiki/colour#Noun");

            URL_Add("create", "https://en.wiktionary.org/wiki/create#Verb");

            URL_Add("downvoting", "https://en.wiktionary.org/wiki/downvote#Verb");

            URL_Add("foreground", "https://en.wiktionary.org/wiki/foreground#Noun");

            URL_Add("guaranteed", "https://en.wiktionary.org/wiki/guaranteed#Adjective");

            URL_Add("long-winded", "https://en.wiktionary.org/wiki/long-winded#Adjective");

            URL_Add("longtime", "https://en.wiktionary.org/wiki/longtime#Adjective");

            URL_Add("meant", "https://en.wiktionary.org/wiki/mean#Verb");

            URL_Add("out of date", "https://en.wiktionary.org/wiki/out_of_date#Prepositional_phrase");

            URL_Add("run-of-the-mill", "https://en.wiktionary.org/wiki/run-of-the-mill#Adjective");

            URL_Add("subprocess", "https://en.wiktionary.org/wiki/subprocess#Noun");

            URL_Add("transformation", "https://en.wiktionary.org/wiki/transformation#Noun");

            URL_Add("welcomeness", "https://en.wiktionary.org/wiki/welcomeness#Noun");

            URL_Add("C++98", "https://en.wikipedia.org/wiki/C%2B%2B#Standardization");

            URL_Add("Freelancer.com", "https://en.wikipedia.org/wiki/Freelancer.com");

            URL_Add("Glassdoor", "https://en.wikipedia.org/wiki/Glassdoor");

            URL_Add("Indeed", "https://en.wikipedia.org/wiki/Indeed");

            URL_Add("README", "https://en.wikipedia.org/wiki/README");

            URL_Add("Scratch", "https://en.wikipedia.org/wiki/Scratch_%28programming_language%29");

            URL_Add("Toptal", "https://en.wikipedia.org/wiki/Toptal");

            URL_Add("absorb", "https://en.wiktionary.org/wiki/absorb#Verb");

            URL_Add("acceptable", "https://en.wiktionary.org/wiki/acceptable#Adjective");

            URL_Add("announcement", "https://en.wiktionary.org/wiki/announcement#Noun");

            URL_Add("change", "https://en.wiktionary.org/wiki/change#Verb");

            URL_Add("channel", "https://en.wiktionary.org/wiki/channel#Noun");

            URL_Add("counteract", "https://en.wiktionary.org/wiki/counteract#Verb");

            URL_Add("great", "https://en.wiktionary.org/wiki/great#Adjective");

            URL_Add("open source", "https://en.wikipedia.org/wiki/Open_source");

            URL_Add("pseudo", "https://en.wiktionary.org/wiki/pseudo-#Prefix");

            URL_Add("school", "https://en.wiktionary.org/wiki/school#Noun_2");

            URL_Add("wherein", "https://en.wiktionary.org/wiki/wherein#Conjunction");

            URL_Add("Lego Mindstorms", "https://en.wikipedia.org/wiki/Lego_Mindstorms");

            URL_Add(".htaccess", "https://en.wikipedia.org/wiki/.htaccess");

            URL_Add("1&1 IONOS", "https://en.wikipedia.org/wiki/1%261_Ionos");

            URL_Add("Avada", "https://www.wpcrafter.com/review/avada/");

            URL_Add("C11", "https://en.wikipedia.org/wiki/C11_%28C_standard_revision%29");

            URL_Add("CERN", "https://en.wikipedia.org/wiki/CERN");

            URL_Add("Divi", "https://www.wpcrafter.com/review/divi/");

            URL_Add("EasyPHP", "https://de.wikipedia.org/wiki/EasyPHP");

            URL_Add("Git&nbsp;2.0", "https://en.wikipedia.org/wiki/Git#Releases[27][28]");

            URL_Add("Higgs boson", "https://en.wikipedia.org/wiki/Higgs_boson");

            URL_Add("MeV", "https://en.wikipedia.org/wiki/Electronvolt");

            URL_Add("TUI", "https://sourceware.org/gdb/onlinedocs/gdb/TUI.html");

            URL_Add("Ubuntu&nbsp;7.10 (Gutsy Gibbon)", "https://en.wikipedia.org/wiki/Ubuntu_version_history#Ubuntu_7.10_(Gutsy_Gibbon)");

            URL_Add("axion", "https://en.wikipedia.org/wiki/Axion");

            URL_Add("current working directory", "https://sourceware.org/gdb/onlinedocs/gdb/Working-Directory.html");

            URL_Add("dark energy", "https://en.wikipedia.org/wiki/Dark_energy");

            URL_Add("dark matter", "https://en.wikipedia.org/wiki/Dark_matter");

            URL_Add("dilemma", "https://en.wiktionary.org/wiki/dilemma#Noun");

            URL_Add("escape", "https://en.wiktionary.org/wiki/escape#Verb");

            URL_Add("glibc", "https://en.wikipedia.org/wiki/GNU_C_Library");

            URL_Add("helium", "https://en.wikipedia.org/wiki/Helium");

            URL_Add("mod_rewrite", "https://en.wikipedia.org/wiki/URL_redirection#Apache_HTTP_Server_mod_rewrite");

            URL_Add("neutrino", "https://en.wikipedia.org/wiki/Neutrino");

            URL_Add("point of view", "https://en.wiktionary.org/wiki/POV#Noun");

            URL_Add("rewrite", "https://en.wiktionary.org/wiki/rewrite#Verb");

            URL_Add("sensitive", "https://en.wiktionary.org/wiki/sensitive#Adjective");

            URL_Add("tongue", "https://en.wiktionary.org/wiki/tongue#Noun");

            URL_Add("Babylon&nbsp;5", "https://en.wikipedia.org/wiki/Babylon_5");

            URL_Add("IOMMU", "https://en.wikipedia.org/wiki/Input%E2%80%93output_memory_management_unit");

            URL_Add("Logitech", "https://en.wikipedia.org/wiki/Logitech");

            URL_Add("PowerTOP", "https://en.wikipedia.org/wiki/PowerTOP");

            URL_Add("PyWin32", "https://wiki.python.org/moin/PyWin32");

            URL_Add("SQL&nbsp;Search", "https://en.wikipedia.org/wiki/Redgate#History");

            URL_Add("SVM", "https://en.wikipedia.org/wiki/X86_virtualization#AMD_virtualization_(AMD-V)");

            URL_Add("add-in", "https://en.wiktionary.org/wiki/add-in#Noun");

            URL_Add("boost converter", "https://en.wikipedia.org/wiki/Boost_converter");

            URL_Add("help vampire", "https://en.wiktionary.org/wiki/help_vampire");

            URL_Add("udev", "https://en.wikipedia.org/wiki/Udev");

            URL_Add("visionary", "https://en.wiktionary.org/wiki/visionary#Noun");

            URL_Add(".bashrc file", "https://en.wikipedia.org/wiki/Bash_(Unix_shell)#Aliases_and_functions");

            URL_Add("PS1", "https://en.wikipedia.org/wiki/Command-line_interface#Command_prompt");

            URL_Add("SMBus", "https://en.wikipedia.org/wiki/System_Management_Bus");

            URL_Add("hemisphere", "https://en.wiktionary.org/wiki/hemisphere#Noun");

            URL_Add("psql", "https://en.wikipedia.org/wiki/PostgreSQL#Database_administration");

            URL_Add("resignation", "https://en.wiktionary.org/wiki/resignation#Noun");

            URL_Add("specifically", "https://en.wiktionary.org/wiki/specifically#Adverb");

            URL_Add("summer", "https://en.wiktionary.org/wiki/summer#Noun");

            URL_Add("Feynman diagram", "https://en.wikipedia.org/wiki/Feynman_diagram");

            URL_Add("RSI", "https://en.wikipedia.org/wiki/Repetitive_strain_injury");

            URL_Add("ST-LINK", "https://www.st.com/en/development-tools/st-link-v2.html");

            URL_Add("Swagger", "https://en.wikipedia.org/wiki/Swagger_(software)");

            URL_Add("autogenerated", "https://en.wiktionary.org/wiki/autogenerate#Verb");

            URL_Add("code generation", "https://en.wikipedia.org/wiki/Automatic_programming#Source-code_generation");

            URL_Add("multicore", "https://en.wiktionary.org/wiki/multicore#Adjective");

            URL_Add("precision", "https://en.wiktionary.org/wiki/precision#Noun");

            URL_Add("readd", "https://en.wiktionary.org/wiki/readd#Verb");

            URL_Add("shut down", "https://en.wiktionary.org/wiki/shut_down#Verb");

            URL_Add("Perlbrew", "https://perlbrew.pl/");

            URL_Add("PrestaShop", "https://en.wikipedia.org/wiki/PrestaShop");

            URL_Add("TYPO3", "https://en.wikipedia.org/wiki/TYPO3");

            URL_Add("extension method", "https://en.wikipedia.org/wiki/Extension_method");

            URL_Add("favourite", "https://en.wiktionary.org/wiki/favourite#Adjective");

            URL_Add("intend", "https://en.wiktionary.org/wiki/intend#Verb");

            URL_Add("reflection", "https://en.wikipedia.org/wiki/Reflection_(computer_programming)");



            //========================================================
            //BBBBBBBBBBBBBBBBBBBBBBBBBBBBBB   A marker...


            //Future:
            //
            // 1. add check to see if every URL mapping has a corresponding
            //    spelling mapping (we already check for the other way
            //    around).

            checkSpellingDataStructures();
        } // addLookupData()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string lookUp(
            string aQueryStr,
            bool aGuessURLifFailedLookup,
            out string anOutCorrectedWOrd)
        {
            string toReturn = ""; //Default: value indicating failure.

            string corrStr;
            if (mCaseCorrection.TryGetValue(aQueryStr, out corrStr))
            {
                int peter2 = 2; //Found case correction!
            }
            else
            {
                corrStr = aQueryStr;
            }

            //Even if there is no case correction it may already be the correct case!
            anOutCorrectedWOrd = corrStr;
            string URLStr;
            if (mWord2URL.TryGetValue(corrStr, out URLStr))
            {
                toReturn = URLStr; //We have a match!
            }

            //If lookup failed then we will guess the Wikipedia URL (if allowed to do so).
            if (toReturn.Length == 0)
            {
                if (aGuessURLifFailedLookup)
                {
                    toReturn = "http://en.wikipedia.org/wiki/" + aQueryStr;
                }
            }
            return toReturn;
        } //lookUp()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static string escapeSQL(string aStringForSQL)
        {
            return aStringForSQL.Replace("'", "''");
        } //escapeSQL()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static void addTermsToOutput_SQL(string aBadTerm,
                                          string aCorrectedTerm,
                                          ref StringBuilder aSomeScratch,
                                          string aURL)
        {
            if (aBadTerm.IndexOf("'") >= 0)
            {
                int peter7 = 7;
            }

            aSomeScratch.Append("INSERT INTO EditOverflow\n");
            aSomeScratch.Append("  (incorrectTerm, correctTerm, URL)\n");
            aSomeScratch.Append("  VALUES(");

            aSomeScratch.Append("'");
            aSomeScratch.Append(escapeSQL(aBadTerm));
            aSomeScratch.Append("', '");

            aSomeScratch.Append(escapeSQL(aCorrectedTerm));
            aSomeScratch.Append("', '");

            // Example of where escape of single quotes is necessary:
            //
            //   https://en.wiktionary.org/wiki/couldn't#Contraction
            //
            aSomeScratch.Append(escapeSQL(aURL));

            aSomeScratch.Append("');");

            aSomeScratch.Append("\n\n");
        } //addTermsToOutput_SQL()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static void addTermsToOutput_HTML(string aBadTerm,
                                                  string aCorrectedTerm,
                                                  ref HTML_builder aInOutBuilder,
                                                  string aURL)
        {
            aInOutBuilder.singleLineTagOnSeparateLine(
                "tr",
                " " +
                  aInOutBuilder.singleLineTagStr("td", aBadTerm) + " " +
                  aInOutBuilder.singleLineTagStr("td", aCorrectedTerm) + " " +
                  aInOutBuilder.singleLineTagStr("td", aURL) + " "
                );
        } //addTermsToOutput_HTML()


        /****************************************************************************
         *                                                                          *
         *    Helper function for dumpWordList_asHTML()                             *
         *                                                                          *
         ****************************************************************************/
        private static string encloseInTag_HTML(string aTagName, string aText)
        {
            const string start = "&lt;";
            const string end = "&lt;/";
            return "" + start + aTagName + ">" + aText + end + aTagName + "> ";
        } //encloseInTag_HTML()


        /****************************************************************************
         *                                                                          *
         *   Encapsulate the loop where we run through the whole word list (of      *
         *   incorrect terms) in a certain order. E.g. to generate the row          *
         *   for an HTML table                                                      *
         *                                                                          *
         *    Note: For now, aGenerateHTML false, means SQL (later: use an          *
         *          enum, or better something where the formatting type             *
         *          (e.g. HTML or SQL) is some user defined thing (e.g.             *
         *          a concrete instance of an abstract class))                      *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        private static void generateMainTable(
            ref StringBuilder aSomeScratch,
            bool aGenerateHTML,
            ref string aLongestInCorrectTerm,
            ref string aLongestCorrectTerm,
            ref string aLongestURL,
            ref Dictionary<string, string> aCaseCorrection,
            ref Dictionary<string, string> aWord2URL)
        {
            SortByCorrectThenIncorrect_usingIndex sortObject =
                new SortByCorrectThenIncorrect_usingIndex(aCaseCorrection);

            List<int> indexes = sortObject.indexes(); // Unsorted at this point

            indexes.Sort(sortObject); // After: Those indexes are now
            //        sorted such that looking up the keys in order in
            //        mCaseCorrection will return the entries in the
            //        given defined sort order, in this case the
            //        correct term as the primary key and the incorrect
            //        term in as the secondary key. In order words,
            //        grouping according to the correct term...

            List<string> someKeys_incorrectTerms = sortObject.keys();

            string prevCorrectTerm = "";

            HTML_builder builder = new HTML_builder();
            for (int i = 0; i < 3; i++)
            {
                builder.indentLevelUp();
            }

            foreach (int someIndex in indexes)
            {
                string someIncorrectTerm = someKeys_incorrectTerms[someIndex];
                string someCorrectTerm = aCaseCorrection[someIncorrectTerm];

                string someURL = null;

                string msg = string.Empty; // Default: empty - flag for no errors.

                if (!aWord2URL.TryGetValue(someCorrectTerm, out someURL))
                {
                    // Fail. What should we do?

                    msg =
                      "A correct term \"" + someCorrectTerm +
                      "\", could not be looked up in the term-to-URL mappiong."
                      ;
                }

                // On-the-fly check (but it would be better if this check was
                // done at program startup)
                if (aWord2URL.ContainsKey(someIncorrectTerm))
                {
                    msg =
                      "Incorrect term \"" + someIncorrectTerm +
                      "\" has been entered as a correct term...";
                }

                // On-the-fly check (but it would be better if this check was
                // done at program startup)
                //
                // URLs should look like ones.
                //
                // For now, we just check if they start with "http"...
                //
                if (!someURL.StartsWith("http"))
                {
                    msg =
                      "A URL, <" + someURL + ">, does not look like one. " +
                      "For correct term \"" + someCorrectTerm + "\".";
                }

                // Report error, if any. For now, blocking dialogs...
                if (msg != string.Empty)
                {
                    System.Windows.Forms.MessageBox.Show(msg);
                }

                // Using a flag for now (for the type of output, HTML, SQL, etc.)
                if (aGenerateHTML)
                {
                    addTermsToOutput_HTML(someIncorrectTerm,
                                          someCorrectTerm,
                                          ref builder,
                                          someURL);
                }
                else
                {
                    addTermsToOutput_SQL(someIncorrectTerm,
                                         someCorrectTerm,
                                         ref aSomeScratch,
                                         someURL);

                    // We can rely on the sorted order, first by incorrect and
                    // then correct. Thus we will get a corrected term one or
                    // more times consecutively
                    if (prevCorrectTerm != someCorrectTerm)
                    {
                        // "Identity mapping" - a lookup of a correct
                        // term should also succeeed - e.g. if we only
                        // want to get the URL.
                        addTermsToOutput_SQL(someCorrectTerm,
                                             someCorrectTerm,
                                             ref aSomeScratch,
                                             someURL);
                    }
                }

                if (someIncorrectTerm.Length > aLongestInCorrectTerm.Length)
                {
                    aLongestInCorrectTerm = someIncorrectTerm;
                }
                if (someCorrectTerm.Length > aLongestCorrectTerm.Length)
                {
                    aLongestCorrectTerm = someCorrectTerm;
                }
                if (someURL.Length > aLongestURL.Length)
                {
                    aLongestURL = someURL;
                }

                prevCorrectTerm = someCorrectTerm;
            } //Through the list of incorrect words

            if (aGenerateHTML)
            {
                aSomeScratch.Append(builder.currentHTML());
            }
        } //generateMainTable()


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public string dumpWordList_asSQL()
        {
            StringBuilder SQL_tableRows =
              new StringBuilder(1200000); // 20% margin, 2016-01-31, for 610823.

            StringBuilder scratchSB =
              new StringBuilder(1200000); // 20% margin, 2016-01-31, for 610823.
            // 2016-07-22: Now, 676067, still 7% margin.

            string longestInCorrectTerm = "";
            string longestCorrectTerm = "";
            string longestURL = "";
            generateMainTable(ref SQL_tableRows,
                              false,
                              ref longestInCorrectTerm,
                              ref longestCorrectTerm,
                              ref longestURL,
                              ref mCaseCorrection,
                              ref mWord2URL);

            scratchSB.Append(SQL_tableRows);
            return scratchSB.ToString();
        } //dumpWordList_asSQL()


        /****************************************************************************
         *                                                                          *
         *  Add:  T h e   s t a r t   o f   t h e   H T M L   d o c u m e n t       *
         *                                                                          *
         ****************************************************************************/
        public static void startOfHTML_document(
            ref HTML_builder aInOutBuilder,
            string aTitle,
            string aLongestInCorrectTerm,
            string aLongestCorrectTerm,
            string aLongestURL,
            string aLenLongestInCorrectTermStr,
            string aLenLongestCorrectTermStr,
            string aLenLongestURLStr
            )
        {
            aInOutBuilder.startHTML(aTitle);

            //CSS for table
            {
                aInOutBuilder.startTagWithEmptyLine("style");

                aInOutBuilder.addContentOnSeparateLine("body");
                aInOutBuilder.addContentOnSeparateLine("{");
                aInOutBuilder.indentLevelUp();
                aInOutBuilder.addContentOnSeparateLine("background-color: lightgrey;");
                aInOutBuilder.indentLevelDown();
                aInOutBuilder.addContentOnSeparateLine("}");

                aInOutBuilder.addContentOnSeparateLine("th");
                aInOutBuilder.addContentOnSeparateLine("{");
                aInOutBuilder.indentLevelUp();
                aInOutBuilder.addContentOnSeparateLine("text-align: left;");
                aInOutBuilder.addContentOnSeparateLine("background-color: orange;");
                aInOutBuilder.indentLevelDown();
                aInOutBuilder.addContentOnSeparateLine("}");

                aInOutBuilder.addContentOnSeparateLine("tr");
                aInOutBuilder.addContentOnSeparateLine("{");
                aInOutBuilder.indentLevelUp();
                aInOutBuilder.addContentOnSeparateLine("text-align: left;");
                aInOutBuilder.addContentOnSeparateLine("background-color: lightblue;");
                aInOutBuilder.indentLevelDown();
                aInOutBuilder.addContentOnSeparateLine("}");

                aInOutBuilder.endTagOneSeparateLine("style");
            } // Block. Output CSS part of the HTML content.


            aInOutBuilder.endTagOneSeparateLine("head");


            //Start of body
            aInOutBuilder.startTagWithEmptyLine("body");

            //Headline
            aInOutBuilder.addHeader(1, aTitle);

            // Justification for its existence...
            aInOutBuilder.addParagraph(
                "The content of this list is 99% from what someone on " +
                "the Internet actually posted (in most cases misspelled) - " +
                "it is not a made-up list.");

            // Some disclaimers regarding the accuracy...
            aInOutBuilder.addParagraph(
                "Note: Some terms are not actually " +
                "incorrect, but serve as:");

            aInOutBuilder.startTagWithEmptyLine("ul");

            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Expansions (for example, expanding \"JS\" to \"JavaScript\").");
            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Go the other way, from expanded to abbreviation.");
            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Include line-break protection (though only in " +
                "the HTML source, not as displayed here) - by &amp;nbsp;.");
            aInOutBuilder.singleLineTagOnSeparateLine(
                "li",
                "Casing of some words and tense of some verbs are not " +
                "always 100% correct (this must be dealt with manually) - the " +
                "reason is to avoid redundancy.");

            aInOutBuilder.endTagOneSeparateLine("ul");

            aInOutBuilder.addParagraph(
                "Also note that this should not be applied " +
                  "blindly (high rate of false positives) - " +
                  "every match should be manually checked."
                );

            //Some statistics
            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            //There is some redundancy here...
            aInOutBuilder.addParagraph(
                "Longest incorrect term: \"" +
                  aLongestInCorrectTerm +
                  "\" (" +
                  aLenLongestInCorrectTermStr +
                  " characters)"
                );

            aInOutBuilder.addParagraph(
                "Longest correct term: \"" +
                  aLongestCorrectTerm +
                  "\" (" +
                  aLenLongestCorrectTermStr +
                  " characters)"
                );

            aInOutBuilder.addParagraph(
                "Longest URL: \"" +
                  aLongestURL +
                  "\" (" +
                  aLenLongestURLStr +
                  " characters)"
                );

        } //startOfHTML_document()


        /****************************************************************************
         *                                                                          *
         *  Add:  T h e   s t a r t   o f   t h e   H T M L   d o c u m e n t       *
         *                                                                          *
         ****************************************************************************/
        public static void startOfHTML_Table(ref HTML_builder aInOutBuilder)
        {
            //// Start of table, incl. table headers
            aInOutBuilder.startTagWithEmptyLine("table");

            aInOutBuilder.singleLineTagOnSeparateLine(
                "tr",
                " " +
                  aInOutBuilder.singleLineTagStr("th", "Incorrect") + " " +
                  aInOutBuilder.singleLineTagStr("th", "Correct") + " " +
                  aInOutBuilder.singleLineTagStr("th", "URL") + " "
                );
            aInOutBuilder.addEmptyLine();
        } //startOfHTML_Table()


        /****************************************************************************
         *                                                                          *
         *  Add:  T h e   e n d   o f   t h e   H T M L   d o c u m e n t           *
         *                                                                          *
         ****************************************************************************/
        public static void endOfHTML_document(ref HTML_builder aInOutBuilder,
                                              string aCodeCheck_regularExpression,
                                              string aDateStr)
        {
            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            //Headline
            aInOutBuilder.addHeader(2, "Some common strings and characters");

            //Things for copy-paste
            aInOutBuilder.addParagraph(
                "&nbsp;" +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Code quotes",
                  WikipediaLookup.kCodeQuoteStr +
                    WikipediaLookup.kCodeQuoteStr) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Keyboard", "&lt;kbd>&lt;/kbd>") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Non-breaking space", "&amp;nbsp;") +

                aInOutBuilder.smallTextItemWithSpacing("Micro", "µ") +

                aInOutBuilder.smallTextItemWithSpacing("Degree", "°") +

                aInOutBuilder.smallTextItemWithSpacing("Ohm", "&ohm;") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Right arrow", "&rarr; (&amp;rarr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Up arrow", "&uarr; (&amp;uarr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Down arrow", "&darr; (&amp;darr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Left arrow", "&larr; (&amp;larr;)") +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Line break", "&nbsp;&lt;br/>")
            );

            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            ////Headline

            //Later: From the program (when we generalise
            //       these wrappings in HTML tags)
            //builder.singleLineTagWithEmptyLine("h2", "Code formatting check");
            aInOutBuilder.addHeader(2, "Code formatting check");

            aInOutBuilder.addParagraph(
                "&nbsp;Regular expression " +
                "(rudimentary, high false-positive rate - " +
                "every hit should be checked manually): <br/>" +
                aCodeCheck_regularExpression);

            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            aInOutBuilder.addHeader(2, "Templates");

            aInOutBuilder.addParagraph(
                aInOutBuilder.smallTextItemWithSpacing(
                  "Subscript",
                  encloseInTag_HTML("sub", "SUB")) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Keyboard formatting",
                  encloseInTag_HTML("kbd", "Key")) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Superscript",
                  encloseInTag_HTML("sup", "SUP")) +

                aInOutBuilder.smallTextItemWithSpacing(
                  "Proper formatting for things enclosed " +
                    "in &quot;&lt;>&quot; (square brackets)",
                  "&amp;lt;XYZ>")
            );

            aInOutBuilder.addContentWithEmptyLine("<hr/>");

            string presumedURL =
                "pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_" +
                aDateStr + ".html";

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"http://pmortensen.eu/world/EditOverflow1.html\">" +
                "Edit Overflow for Web</a>"
            );

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"http://validator.w3.org/check?uri=" +
                presumedURL +
                "&amp;charset=utf-8\">" +
                "HTML validation</a>"
            );

            aInOutBuilder.addContentWithEmptyLine(
                "<a href=\"http://pmortensen.eu/\">" +
                "Back to Edit Overflow page</a>"
            );

            aInOutBuilder.addEmptyLine();
            aInOutBuilder.endTagOneSeparateLine("body");

            aInOutBuilder.addEmptyLine();
            aInOutBuilder.endTagOneSeparateLine("html");
        } //endOfHTML_document()


        /****************************************************************************
         *                                                                          *
         *    Note: Parameter "aCodeCheck_regularExpression" does not do            *
         *          anything - it is for output (in the HTML content).              *
         *                                                                          *
         ****************************************************************************/
        public static string dumpWordList_asHTML(
            string aCodeCheck_regularExpression,
            ref Dictionary<string, string> aCaseCorrection,
            int aUniqueWords,
            ref Dictionary<string, string> aWord2URL
            )
        {
            StringBuilder scratchSB =
              new StringBuilder(1200000); // 20% margin, 2016-01-31, for 610823.
            // 2016-07-22: Now, 676067, still 7% margin.
            // 2019-07-01: Now 1,039,246 bytes.
            // 2019-08-21: Now 1,079,071 bytes (11% margin)

            StringBuilder HTML_tableRows =
              new StringBuilder(1200000);
            // 20% margin, 2016-01-31, for 610823.
            // 2019-08-21: Now 1,076,066 bytes (12% margin)


            // First generate the rows of the main table - so we can compute
            // various statistics while we go through the data structures
            //
            string longestInCorrectTerm = "";
            string longestCorrectTerm = "";
            string longestURL = "";
            generateMainTable(ref HTML_tableRows,
                              true,
                              ref longestInCorrectTerm,
                              ref longestCorrectTerm,
                              ref longestURL,
                              ref aCaseCorrection,
                              ref aWord2URL);
            // The main side effect is the changing of the content
            // of ref HTML_tableRows...

            // 2016-07-22: Now, 676067, still 7% margin.

            int items = aCaseCorrection.Count;

            string versionStr = EditorOverflowApplication.fullVersionStr();

            // This would if the date changes right after the call of fullVersionStr()...
            string dateStr = EditorOverflowApplication.versionString_dateOnly();

            string lenLongestInCorrectTermStr = longestInCorrectTerm.Length.ToString();
            string lenLongestCorrectTermStr = longestCorrectTerm.Length.ToString();
            string lenLongestURLStr = longestURL.Length.ToString();

            //Separate HTML generation for title (used in two places)
            scratchSB.Append("Edit Overflow wordlist. ");
            scratchSB.Append(items);
            scratchSB.Append(" input words and ");
            scratchSB.Append(aUniqueWords);
            scratchSB.Append(" output words (for ");
            scratchSB.Append(versionStr);
            scratchSB.Append(")");
            string title = scratchSB.ToString();

            scratchSB.Length = 0;

            HTML_builder builder = new HTML_builder();

            startOfHTML_document(ref builder,
              title,
              longestInCorrectTerm, longestCorrectTerm, longestURL,
              lenLongestInCorrectTermStr,
              lenLongestCorrectTermStr,
              lenLongestURLStr);

            startOfHTML_Table(ref builder);

            // That is everything before the table content, including
            // the table header...
            scratchSB.Append(builder.currentHTML());

            // The bulk of this HTML page - all the data rows in the table.
            scratchSB.Append(HTML_tableRows);

            builder.endTagOneSeparateLine("table");

            endOfHTML_document(ref builder, aCodeCheck_regularExpression, dateStr);

            scratchSB.Append(builder.currentHTML());
            //--------------------------------------------------------

            return scratchSB.ToString();
        } //dumpWordList_asHTML()


        /****************************************************************************
         *                                                                          *
         *  This is the function used by the main user-facing functionality         *
         *  (menu command), exporting the current wordlist to HTML.                 *
         *                                                                          *
         ****************************************************************************/
        public string dumpWordList_asHTML(string aCodeCheck_regularExpression)
        {
            return dumpWordList_asHTML(
                      aCodeCheck_regularExpression,
                      ref mCaseCorrection,
                      mCaseCorrection_Reverse.Count,
                      ref mWord2URL);
        } //dumpWordList_asHTML()


    } //class WikipediaLookup


} //namespace OverflowHelper.core


