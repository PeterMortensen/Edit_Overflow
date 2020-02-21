
// First driver for using the .NET part Edit Overflow on Linux.
//
// The purpose is to run the word list integrity test
// independent of Windows and Visual Studio.


using System; // For Console

using OverflowHelper.core;


namespace EditOverflow2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello, World!");


            // This will result in running the ***first*** level of
            // integrity testing for the word list data
            WikipediaLookup someWikipediaLookup = new WikipediaLookup();


            string toOutput; 
            if (true) // We don't want double error reporting for problems with
                      // the word list data - so choose either SQL or HTML
                      // generation.
            {
                // The Default is SQL as that is what we most often
                // use (to update the depluyed database with the 
                // word list data).
                toOutput = someWikipediaLookup.dumpWordList_asSQL();
            }
            else
            {
                // This will result in running ***more*** rigorous
                // integrity testing for the word list data
                toOutput = someWikipediaLookup.dumpWordList_asHTML(
                
                        // Fixed strings - sufficient for integrity testing
                        // of the word list data
                        //
                        "some combined regular expressions",
                        "some version thingie",
                        "some date only string");
            }


            // Dump the SQL or HTML to standard output so we can 
	        // redirect it to a file (but note that integrity 
	        // error messages currently also end up there...).
            //
            // What about Unicode / UTF-8????????
            Console.WriteLine(toOutput);
        }
    }
}


