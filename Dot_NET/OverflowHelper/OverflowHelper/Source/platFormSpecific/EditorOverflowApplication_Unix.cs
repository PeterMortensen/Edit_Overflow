

using System.Diagnostics; //For Trace. And its Assert.


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper
{

    // Why does it need to be public???
    public struct clipboardInfoStruct
    {
        public int retries;
    }


    /****************************************************************************
     *    "public" because of unit tests...                                     *
     ****************************************************************************/
    public class EditorOverflowApplication_Unix : EditorOverflowApplication
    {


        /****************************************************************************
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        public override void setClipboard3(
            string aClipboardStr,
            //System.Windows.Forms.ToolStripStatusLabel aStatusLabel
            out string aStatusString
            )
        { 
            aStatusString = "";
        
            Trace.Assert(false);
            
        } //setClipboard3()




    } //class EditorOverflowApplication


} //namespace OverflowHelper



