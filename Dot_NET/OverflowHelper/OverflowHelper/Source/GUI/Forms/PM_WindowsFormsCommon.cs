
//using System;
//using System.Linq;
//using System.Text;


using System.Collections.Generic; //For Dictionary.

using System.Windows.Forms; //For Control


//****************************************************************************
//*    <placeholder for header>                                              *
//****************************************************************************
namespace PM_WindowsForms
{

    //****************************************************************************
    //*    <placeholder for header>                                              *
    //****************************************************************************
    //To be incorporated into an implementation inheritance class to be inserted
    //between Form and Windows Forms classes.
    public static class PM_WindowsFormsCommon
    {

        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        public static void commonChecks(System.Windows.Forms.Form aForm)
        {
            checkKeyboardShortcuts(aForm);
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private static void checkKeyboardShortcuts(System.Windows.Forms.Form aForm)
        {

            //To do: we need to recursively call ourselves for 
            //       containers, e.g. groupBox2 
            //       (System.Windows.Forms.GroupBox) 
            //       in frmMarkdown.cs.
            //
            // Otherwise we will not detect some duplicates.



            //The value is used for storing the first instance the GUI text (that 
            //encodes a keyboard shortcut by preceding it with "&")
            Dictionary<string, string> duplicateKeyboardShortcuts = 
                new Dictionary<string, string>();

            string name = aForm.Text;

            foreach (Control someControl in aForm.Controls)
            {
                string text = someControl.Text;
                int kIndex = text.IndexOf('&');
                if (kIndex >= 0)
                {
                    string keyboardShortcut = 
                        text[kIndex+1].ToString().ToUpper(); //+1: "&" precedes the                      
                                                             //keyboard shortcut.
                    string firstGUIstring;
                    if (duplicateKeyboardShortcuts.TryGetValue(
                          keyboardShortcut, out firstGUIstring))
                    {
                        string msg =
                            "Duplicate keyboard shortcut: \"" +
                            firstGUIstring +
                            "\" and \"" +
                            text +
                            "\".";

                        // If the text itself contains "&nbsp;" (e.g. "Eclipse&nbsp;v4.2 (Juno)") 
                        // we get false positive matches. Suppress those. But
                        // perhaps this test should be done earlier?
                        //
                        if (msg.IndexOf("&nbsp;") < 0)
                        {
                            System.Windows.Forms.MessageBox.Show(msg);
                        }
                    }
                    else
                    {
                        duplicateKeyboardShortcuts[keyboardShortcut] = text; //Store
                        //  the first instance for use in the error message if we
                        //  see another GUI element with the same .
                    }
                }

                //double yDiff = someItem;
                //if (yDiff > 5.1) 
                //{
                //  break;
                //}
            } //Through checkBoxInfoList


            //Control someControl
            //For Each someControl in aForm.Controls
            //    //If CStr(someControl.Tag) = "MOVE_ON_RESIZE" Then

            //    //    Dim loc As Point = someControl.Location
            //    //    loc.Y += Yoffset
            //    //    someControl.Location = loc
            //    //End If
            //Next
            

            //For now: do a nuisance.
        }
    }
     
} //PM_WindowsForms
