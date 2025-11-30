
/****************************************************************************
 *                                                                          *
 *   Purpose:                                                               *
 *                                                                          *
 *     1. Representing global state and information                         *
 *                                                                          *
 *     2. Base class for isolating some platform-specific                   *
 *        things, e.g. access to the clipboard                              *
 *                                                                          *
 ****************************************************************************/



/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper
{


    /****************************************************************************
     *    "public" because of unit tests...                                     *
     ****************************************************************************/
    public abstract class EditorOverflowApplication
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string fullVersionStr()
        {
            string toReturn =
              applicationName() +
              " v. " + versionString() + " " +
              versionString_dateOnly();
            return toReturn;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string applicationName()
        {
            string toReturn = "Edit Overflow";
            return toReturn;
        }


        /****************************************************************************
         *                                                                          *
         *    Common for to avoid redundant code (reporting errors,                 *
         *    convenient call, workaround for failing clipboard,                    *
         *    operations, etc.)                                                     *
         *                                                                          *
         *    Parameters:                                                           *
         *                                                                          *
         *      aStatusLabel                                                        *
         *                                                                          *
         *        A value of to null is allowed (used when error reporting          *
         *        is not required or possible).                                     *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        public abstract void setClipboard3(
            string aClipboardStr,
            out string aStatusString);


        /****************************************************************************
         *                                                                          *
         *    Simpler version for clients that do not need the status string        *
         *                                                                          *
         ****************************************************************************/
        public void setClipboard3(string aClipboardStr)
        {
            string dummy;
            setClipboard3(aClipboardStr, out dummy);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public string versionString_dateOnly()
        {
            string toReturn = "2025-11-30"; // Note: Having the exact length
                                            //       of 10 will be caught in
                                            //       one of the units tests...
            return toReturn;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string versionString()
        {
            string toReturn = "1.1.51a671";
            return toReturn;
        }

        //Last designer content with version information for the
        //main form: "OverflowHelper v. 0.13 2010-02-15"


    } //class EditorOverflowApplication


} //namespace OverflowHelper



