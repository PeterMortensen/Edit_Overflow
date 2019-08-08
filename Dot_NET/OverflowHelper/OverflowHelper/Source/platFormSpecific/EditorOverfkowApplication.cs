

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


using OverflowHelper.platFormSpecific;


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper
{


    public struct clipboardInfoStruct
    {
        public int retries;
    }


    /****************************************************************************
     *    "public" because of unit tests...                                     *
     ****************************************************************************/
    public class EditorOverflowApplication
    {


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string fullVersionStr()
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
         *    Isolation of writing to the clipboard. It also provides               *
         *    the try-catch protection (as setting the clipboard for                *
         *    unknown reasons sometimes fail...).                                   *
         *                                                                          *
         ****************************************************************************/
        private static void setClipboard(string aClipboardStr,
                                        out clipboardInfoStruct anOutclipboardInfo)
        {
            setClipboard(aClipboardStr, false, out anOutclipboardInfo);
        }
        

        /****************************************************************************
         *    Isolation of writing to the clipboard. It also provides               *
         *    the try-catch protection (as settings the clipboard for               *
         *    unknown reasons sometimes fail...).                                   *
         *                                                                          *
         ****************************************************************************/
        private static void setClipboard(string aClipboardStr, 
                                        bool anAddToClipboard,
                                        out clipboardInfoStruct anOutclipboardInfo)
        {
            // bool anAddToClipboard = false
            // 
            // Default parameters requires .NET 4.0...
            //
            // Otherwise:
            //
            //   Default parameter specifiers are not permitted

            anOutclipboardInfo.retries = 0;

            bool success = false;
            int tries = 0;
            
            while (!success)
            {
                try
                {
                    tries += 1;

                    string prefix = "";
                    if (anAddToClipboard)
                    {
                        prefix = System.Windows.Forms.Clipboard.GetText();
                    }

                    string textToSet = prefix + aClipboardStr;

                    // This fails sometimes. But why??
                    // System.Windows.Forms.Clipboard.SetText(textToSet);

                    clipboard.setText(textToSet);

                    success = true;
                }
                catch (System.Exception anExceptionObject)
                {
                    // 2017-10-15: Suppress the error for now by trying a 
                    //             number of times. But we should really find
                    //             out why it is failing!

                    if (tries >= 30)
                    {
                        string errMsg1 = "Error: " + anExceptionObject.ToString() + "\n";
                        string errMsg2 =
                            "\n" + "Message: " + anExceptionObject.Message + "\n";

                        //string exceptionInfo = anExceptionObject.InnerException.ToString();
                        string msg =
                            "For unknown reasons the clipboard could not be " +
                            "changed when it was attempted to set it to the " + 
                            "text (after trying " + tries.ToString() + " times) \"" +
                            aClipboardStr + "\". " +
                            System.Environment.NewLine + System.Environment.NewLine +
                            "Further information: " + errMsg1 + errMsg2;
                        System.Windows.Forms.MessageBox.Show(msg);

                        // Sample:
                        //
                        //   For unknown reasons the clipboard could not be changed when it was attempted
                        //   to set it to the text "JavaScript".
                        //
                        //
                        //   Further information: Error: System.Runtime.InteropServices.COMException (0x800401D0):
                        //                        OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))
                        //
                        //      at System.Runtime.InteropServices.Marshal.ThrowExceptionForHRInternal(Int32 errorCode, IntPtr errorInfo)
                        //
                        //      at System.Windows.Clipboard.CriticalSetDataObject(Object data, Boolean copy)
                        //
                        //      at System.Windows.Clipboard.SetDataInternal(String format, Object data)
                        //
                        //      at System.Windows.Clipboard.SetText(String text, TextDataFormat format)
                        //
                        //      at System.Windows.Clipboard.SetText(String text)
                        //
                        //      at OverflowHelper.platFormSpecific.clipboard.setText(String aTextToSet) in D:\dproj\OverflowHelper\OverflowHelper\OverflowHelper\Source\platFormSpecific\Clipboard.cs:line 120
                        //
                        //      at OverflowHelper.EditorOverflowApplication.setClipboard(String aClipboardStr, Boolean anAddToClipboard) in D:\dproj\OverflowHelper\OverflowHelper\OverflowHelper\Source\platFormSpecific\EditorOverfkowApplication.cs:line 117
                        //
                        //   Message: OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))
                    }
                }
                finally
                {
                    //Clean up.
                }
            } //while - retrying

            anOutclipboardInfo.retries = tries - 1;
        } //setClipboard()


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
        public static void setClipboard3(
            string aClipboardStr,
            System.Windows.Forms.ToolStripStatusLabel aStatusLabel)
        { 
            clipboardInfoStruct outclipboardInfo;

            outclipboardInfo.retries = -1;

            EditorOverflowApplication.setClipboard(
                aClipboardStr, out outclipboardInfo);

            int retries = outclipboardInfo.retries;
            if (retries > 0)
            {
                if (aStatusLabel != null) // It is called with nul from 
                //                           the Markdown utility...
                {
                    aStatusLabel.Text =
                        retries.ToString() + " retries accessing the clipboard!";
                }
                else
                {
                    int peter2 = 2;
                }
            }
        } //setClipboard3()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string versionString_dateOnly()
        {
            string toReturn = "2019-07-27";
            return toReturn;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string versionString()
        {
            string toReturn = "1.1.48"; 
            return toReturn;
        }

        //Last designer content with version information for the 
        //main form: "OverflowHelper v. 0.13 2010-02-15"


    } //class EditorOverflowApplication


} //namespace OverflowHelper



