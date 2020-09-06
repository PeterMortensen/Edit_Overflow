
// First driver for using the .NET part Edit Overflow on Linux.
//
// The purpose is to run the word list integrity test
// independent of Windows and Visual Studio.


using System; // For Console

using OverflowHelper.core;

using OverflowHelper; // For class EditorOverflowApplication (that is
                      // directly in that namespace (should it be?))


namespace EditOverflow2
{
    class Program
    {
        static void Main(string[] args)
        {
            string lookupWord = Environment.GetEnvironmentVariable("LOOKUP");

            string outputType = Environment.GetEnvironmentVariable("WORDLIST_OUTPUTTYPE");


            string toOutput = ""; // No output unless explicitly indicated
                                  // by parameters passed to the program

            if (lookupWord != null) // Lookup overrides any specified
                                    // export - we will not do both
                                    // at the same time
            {
                WordCorrector someWordCorrector = new WordCorrector();
                lookupResultStructure lookupResult =
                    someWordCorrector.lookup_Central(lookupWord, false);

                string correctedText2 = lookupResult.correctedText;
                int URlcount = lookupResult.URlcount;

                if (correctedText2 != string.Empty)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "Corrected word for " + lookupWord +
                        " is: " + correctedText2);
                    Console.WriteLine();
                }
            }
            else
            {
                //Word list exports

                EditorOverflowApplication app = new EditorOverflowApplication_Unix();

                CodeFormattingCheck cfCheck = new CodeFormattingCheck();


                // This will result in running the ***first*** level of
                // integrity testing for the word list data
                WikipediaLookup someWikipediaLookup = new WikipediaLookup();

                switch (outputType)
                {
                    case "SQL":
                        // That is what we most often use (to update the
                        // deployed database with the word list data).
                        //
                        // Like for HTML generation, it will result in
                        // running ***more*** rigorous integrity
                        // testing for the word list data.

                        toOutput = someWikipediaLookup.dumpWordList_asSQL();
                        break;

                    case "HTML":
                        // This will result in running ***more*** rigorous
                        // integrity testing for the word list data


                        //toOutput = someWikipediaLookup.dumpWordList_asHTML(
                        //
                        //               // Fixed strings - sufficient for integrity testing
                        //               // of the word list data
                        //               //
                        //               "some combined regular expressions",
                        //               "some version thingie",
                        //               "some date only string");

                        toOutput = someWikipediaLookup.dumpWordList_asHTML(

                                       cfCheck.combinedAllOfRegularExpressions(),
                                       app.fullVersionStr(),
                                       app.versionString_dateOnly()
                                   );
                        break;

                    case "JavaScript":

                        toOutput = someWikipediaLookup.dumpWordList_asJavaScript();
                        break;

                   default:
                      //Console.WriteLine("Hello, World!");

                      Console.WriteLine("\n");
                      Console.WriteLine("2020-02-28T012833Z+0");
                      Console.WriteLine("\n");

                      string var1 = Environment.GetEnvironmentVariable("PATH");
                      //Console.WriteLine("Environment variable PATH: " + var1 + "\n");

                      Console.WriteLine(
                        "Output type for wordlist not specified. " +
                        "Use environment variable WORDLIST_OUTPUTTYPE " +
                        "with 'SQL' or 'HTML'.\n");
                      break;
                } //switch


                // Dump the SQL or HTML to standard output so we can
                // redirect it to a file (but note that integrity
                // error messages currently also end up there...).
                //
                // What about Unicode / UTF-8????????
                Console.WriteLine(toOutput);

            } //Exports

        } //Main()
    } //class Program
}


