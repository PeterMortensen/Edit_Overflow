

using NUnit.Framework; //For all versions of NUnit, 
//file "nunit.framework.dll"

using OverflowHelper.core;


namespace OverflowHelper.Tests
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    [TestFixture]
    class StringReplacerWithRegexTests
    {

        /****************************************************************************
         *    Intent: Creation, passing an input string, and reading it out.        *
         ****************************************************************************/
        [Test]
        public void retainedInputString()
        {
            string inputString = "Identity";
            StringReplacerWithRegex replacer = new StringReplacerWithRegex(inputString);
            Assert.AreEqual(replacer.currentString(), inputString, "XYZ");
        } //retainedInputString


        /****************************************************************************
         *                                                                          *
         *   Nominal usage of the class, only a single transform                    *
         *                                                                          *
         *   Test cases are from mnuTransformForYouTubeComments_Click()             *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void basicUsage_singleTransformation()
        {
            // For YouTube comments, minutes: "07 min " -> "07:"
            {
                StringReplacerWithRegex replacer =
                    new StringReplacerWithRegex("07 min ");

                //It returns the current value
                Assert.AreEqual("07:", replacer.transform(@"(\d+)\s+min\s+", @"$1:"),
                                "XYZ");

                //The state should also be updated by the call of transform().
                Assert.AreEqual("07:", replacer.currentString(), "XYZ");
            }

            // For YouTube comments, seconds: " 10 secs: " -> " 10 : "
            { 
                StringReplacerWithRegex replacer = 
                    new StringReplacerWithRegex(" 10 secs: ");

                //It returns the current value
                Assert.AreEqual(" 10 : ", replacer.transform(@"(\d+)\s+secs", @"$1 "), 
                                "XYZ");

                //The state should also be updated by the call of transform().
                Assert.AreEqual(" 10 : ", replacer.currentString(), "XYZ");
            }

            // For YouTube comments, hide URLs: "www.amazon.com" -> 
            // "www DOT amazon DOT com"
            {                 
                StringReplacerWithRegex replacer =
                    new StringReplacerWithRegex("www.amazon.com");

                Assert.AreEqual("www DOT amazon DOT com",
                                replacer.transform(@"(\w)\.(\w)", @"$1 DOT $2"),
                                "XYZ");
            }

        } //basicUsage_singleTransformation()


        /****************************************************************************
         *                                                                          *
         *   Nominal usage of the class, multiple transforms. This is the typical   *
         *   usage and what this class was designed for                             *
         *                                                                          *
         *   The test cases is from mnuTransformForYouTubeComments_Click()          *
         *                                                                          *
         ****************************************************************************/
        [Test]
        public void basicUsage_multipleTransformations()
        {
            // This example is for converting the format used in file 
            // "YouTube listen list.txt" into a YouTube-compatible
            // format that be pasted directly into a YouTube comment
            // such that the times are live and the URL hidden so 
            // the comment will not be deleted.

            // Sample line to be convert:
            //
            //   07 min 10 secs: Book: https://www.amazon.com/Writing-Well


            StringReplacerWithRegex replacer = new StringReplacerWithRegex(
                "07 min 10 secs: Book: https://www.amazon.com/Writing-Well");

            replacer.transform(@"(\d+)\s+secs", @"$1 ");
            replacer.transform(@"(\d+)\s+min\s+", @"$1:");

            Assert.AreEqual(
                "07:10 : Book: https://www DOT amazon DOT com/Writing-Well",
                replacer.transform(@"(\w)\.(\w)", @"$1 DOT $2"),
                "XYZ");

            //The state should also be updated by the call of transform().
            Assert.AreEqual("07:10 : Book: https://www DOT amazon DOT com/Writing-Well", 
                            replacer.currentString(), "XYZ");


            // One more step, remove the leading "http://" or "https://"
            //
            // Why don't we need to escape "/" in the regular expression?
            // Answer: This is not Perl! - we use the .NET function calls.
            //
            Assert.AreEqual(
                "07:10 : Book: www DOT amazon DOT com/Writing-Well",
                replacer.transform("https://", ""),
                "XYZ");
            Assert.AreEqual(
                "07:10 : Book: www DOT amazon DOT com/Writing-Well",
                replacer.transform("http://", ""),
                "XYZ");

        } //basicUsage_multipleTransformations()


    } //class StringReplacerWithRegexTests 


}


