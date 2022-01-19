
// First driver for using the .NET part Edit Overflow on Linux.
//
// The purpose was to run the word list integrity test
// independent of Windows and Visual Studio.


using System; // For Console

using System.Diagnostics; //For Trace. And its Assert.

using OverflowHelper.core;

using OverflowHelper; // For class EditorOverflowApplication (that is
                      // directly in that namespace (should it be?))


namespace EditOverflow2
{
    class Program
    {
        static void Main(string[] args)
        //static int Main(string[] args)
        {
            string lookupWord = Environment.GetEnvironmentVariable("LOOKUP");

            string outputType = Environment.GetEnvironmentVariable("WORDLIST_OUTPUTTYPE");


            //Test!!!!!!!  To see if we actually detect output
            //             to standard error in the build
            //             script
            System.IO.TextWriter errorWriter = Console.Error;
            //errorWriter.WriteLine("\nSome output to standard error...\n");


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


                ////Test!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                ////return 42;
                //{
                //    Console.WriteLine("\n\nAbout to fail an assert....\n");
                //    Trace.Assert(0 == 1, "Assert failed...");
                //
                //    Console.WriteLine(
                //        "Do we continue program execution after an assert fires? No!");
                //}
                //{
                //    Console.WriteLine(
                //        "\n\nAbout to call Environment.Exit(44)....\n");
                //    Environment.Exit(44);
                //    Console.WriteLine(
                //        "Do we continue program execution after Environment.Exit()? No!");
                //}

            }
            else
            {
                // Word list exports

                EditorOverflowApplication app = new EditorOverflowApplication_Unix();

                CodeFormattingCheck cfCheck = new CodeFormattingCheck();


                // This will result in running the ***first*** level of
                // integrity testing for the word list data
                TermLookup someTermLookup = new TermLookup();

                switch (outputType)
                {
                    case "SQL":
                        // That is what we most often use (to update the
                        // deployed database with the word list data).
                        //
                        // Like for HTML generation, it will result in
                        // running ***more*** rigorous integrity
                        // testing for the word list data.

                        toOutput = someTermLookup.dumpWordList_asSQL();
                        break;

                    case "HTML":
                        // This will result in running ***more*** rigorous
                        // integrity testing for the word list data

                        string explanations = cfCheck.combinedAllOfExplanations();

                        toOutput = someTermLookup.dumpWordList_asHTML(
                                       cfCheck.combinedAllOfRegularExpressions(),
                                       explanations,
                                       app.fullVersionStr(),
                                       app.versionString_dateOnly()
                                   );
                        break;

                    case "JavaScript":

                        toOutput = someTermLookup.dumpWordList_asJavaScript();
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


