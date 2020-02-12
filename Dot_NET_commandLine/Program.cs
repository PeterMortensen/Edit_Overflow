
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

            // This will result in running ***more*** regorious
            // integrity testing for the word list data
            string Wordlist_HTML =
              someWikipediaLookup.dumpWordList_asHTML(

                // Fixed strings - sufficient for integrity testing
                // of the word list data
                //
                "some combined regular expressions",
                "some version thingie",
                "some date only string");
                
            string wordlist_SQL = someWikipediaLookup.dumpWordList_asSQL();

            // What about Unicode / UTF-8????????
            Console.WriteLine(wordlist_SQL);
        }
    }
}
