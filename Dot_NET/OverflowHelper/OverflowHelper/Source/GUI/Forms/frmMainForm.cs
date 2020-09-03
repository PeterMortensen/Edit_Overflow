/****************************************************************************
 * Copyright (C) 2010 Peter Mortensen                                       *
 * This file is part of OverflowHelper.                                     *
 *                                                                          *
 *                                                                          *
 * Purpose: main (start-up) window for Edit Overflow.                       *
 *                                                                          *
 ****************************************************************************/

/****************************************************************************
 *                       Mortensen Consulting                               *
 *                         Peter Mortensen                                  *
 *                E-mail: NUKESPAMMERSdrmortensen@get2netZZZZZZ.dk          *
 *                 WWW: http://www.cebi.sdu.dk/                             *
 *                                                                          *
 *  Program for Wiki editing.                                               *
 *                                                                          *
 *    FILENAME:   frmMainForm.cs                                            *
 *    TYPE:       CSHARP                                                    *
 *                                                                          *
 * CREATED: PM 2010-01-24   Vrs 1.0.                                        *
 * UPDATED: PM 2010-xx-xx                                                   *
 *                                                                          *
 *                                                                          *
 ****************************************************************************/


//Future:
//
//   1. Implement removing end of line comments, " \ ".
//
//      Note: What about actual use of "\" in a Forth program? We risk
//            ruining the Forth program.
//
//      In AutoItScript()
//
//      We have a problem with escaping "\" (near "Remove Forth comments").
//
//      Perhaps we can generalise removeTrailingSpace(), that uses:
//
//            string result = Regex.Replace(aInputStr,
//                                          @"[ \t]+(\r?$)",
//                                          @"$1",
//                                          RegexOptions.Multiline);
//
//          No, not exactly. This is for repeated
//          characters before a newline.
//
//          But could it be expressed in a similar way? Matching "\"
//          and then any character before newline.
//
//      Can we use "\\" instead of "\t"?
//
//   2. Include some Forth word name in the name for the
//      temporary AutoIt file.
//
//      In mnuForthTyping_direct_Click().
//
//   3. Eliminate redundancy near
//      "ApplicationDeployment.CurrentDeployment.DataDirectory"
//
//      E.g. in mnuFilterHTML_forMediaURLsAndOpenLinkPage_2_Click(),
//      menu "Text" -> "Filter HTML for media URLs and open link page""
//
//      We have mapped the current behavior near "In a non-ClickOnce context"
//
//   4.
//


//using System.ComponentModel;
//using System.Data;
//using System.Linq;
//using System.Text;


using System; //For EventArgs.
using System.Collections.Generic; //For List.
using System.Drawing; //For Color and KnownColor.
using System.Windows.Forms; //For Form.

using System.Text.RegularExpressions; //For MatchCollection.

using System.IO; //For File, Directory
using System.Diagnostics; //For Process

using System.Text; //For StringBuilder.


using OverflowHelper.core;
using OverflowHelper.platFormSpecific;


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public partial class frmMainForm : Form
    {
        private WikipediaLookup mWikipediaLookup;
        private CheckinMessageBuilder mCheckinMessageBuilder;

        private timing mTiming;

        private LinkRef mLinkRefGenerator; //Essentially global/unique ref numbers

        private CommandLineParameters mCommandLineParameters;

        //private long mLastLookupTick;  not used. Why?

        private int mIdleCounter;

        private Sites mSites;

        private bool mRunning;

        private StringBuilder mScratchSB;

        private EditSummaryStyle mEditSummaryStyle;


        private EditorOverflowApplication mApplication;

        private CodeFormattingCheck mCodeFormattingCheck;


        /****************************************************************************
         *    Constructor                                                           *
         ****************************************************************************/
        public frmMainForm()
        {
            mScratchSB = new StringBuilder(200);

            mApplication = new EditorOverflowApplication_Windows();

            mCodeFormattingCheck = new CodeFormattingCheck();

            InitializeComponent();

            try
            {
                mWikipediaLookup = new WikipediaLookup();

                mSites = new Sites();

                mEditSummaryStyle = new EditSummaryStyle();
            }
            catch (Exception exceptionObject)
            {
                //Use something else than speech to get attention.
                string msg = "Crash in constructor of WikipediaLookup";
                System.Windows.Forms.MessageBox.Show(msg);
            }
            finally
            {
	            //Clean up.
            }

            //Must be after creation of mEditSummaryStyle...
            mCheckinMessageBuilder = new CheckinMessageBuilder(mEditSummaryStyle);
            mTiming = new timing(lblTimeLeft);

            mLinkRefGenerator = new LinkRef();

            Application.Idle +=
               new System.EventHandler(this.Idle_Count);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void frmMainForm_Load(object aSender, EventArgs anEvent)
        {
            //txtOutputURL.Focus();  Nope, does not work. In OnLoad() instead.

            clearCheckin(); //To get the colour for some fields...

            //Thanks Stack Overflow!, http://stackoverflow.com/questions/114983
            //  DateTime.UtcNow.ToString("yyyy-MM-ddTHH\:mm\:ss.fffffffzzz");
            txtCurrentDate.Text =
                DateTime.UtcNow.ToString("yyyy-MM-dd");

            //Clear text visible at design time.
            tstrLabel1.Text = "";
            tstrLabel2.Text = "";


            mTiming.init2(); //Second level initialisation. To stay true
            //during refactoring. But is it really necessary?

            setTitle();

            //PM_GUI_HINTS 2010-09-04
            openButtonsUpdate(txtID.Text);

            PM_WindowsForms.PM_WindowsFormsCommon.commonChecks(this);

            mCommandLineParameters = new CommandLineParameters();

            //http://stackoverflow.com/questions/1679243/
            //  Getting the thread ID from a thread
            //    "For the latest version of .NET, the current recommended way of
            //     doing it is
            //     System.Threading.Thread.CurrentThread.ManagedThreadId."
            //
            // The property "ManagedThreadId" (of a thread):
            //   <http://msdn.microsoft.com/en-us/library/system.threading.thread.managedthreadid%28v=vs.110%29.aspx>
            //
            // The *current* thread (static method):
            //   <http://msdn.microsoft.com/en-us/library/system.threading.thread.currentthread%28v=vs.110%29.aspx>

            int threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void frmMainForm_Shown(object aSender, EventArgs anEvent)
        {
            mRunning = true;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void frmMainForm_FormClosing(
          object aSender, FormClosingEventArgs anEvent)
        {
            mRunning = false;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void setTitle()
        {
            string currentSiteDomainURL = mSites.getCurrentSiteDomainURL();

            this.Text =
                mApplication.fullVersionStr() +
                "   Current site domain URL: " + currentSiteDomainURL;

            grpDoStuffOnQuestionIDs.Text =
                "For current site, " + currentSiteDomainURL;
        }


        //PM_GUI_HINTS 2010-09-04
        /****************************************************************************
         *    For hinting that some buttons (currently 3 out of 4) only makes       *
         *    sense if there is something in the text input field (from where an    *
         *    integer post ID is extracted).                                        *
         ****************************************************************************/
        private void openButtonsUpdate(string anTextWithIntegerIDs)
        {
            bool enableButtons = txtID.Text.Length > 0;
            btnOpenIDonStackOverflow.Enabled = enableButtons;
            btnOpenTimeLineInBrowser.Enabled = enableButtons;
            btnOpenRevisionsInBrowser.Enabled = enableButtons;
            btnOpenDeletedPost.Enabled = enableButtons;

            mnuOpenPost.Enabled = enableButtons;
            mnuOpenTimeLineInBrowser.Enabled = enableButtons;
            mnuOpenRevisionsInBrowser.Enabled = enableButtons;
            mnuOpenDeletedPost.Enabled = enableButtons;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnLookup_Click(object aSender, EventArgs anEvent)
        {
            doLookup(false);
        } //btnLookup_Click


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void updateCheckinMessage()
        {
            bool postHasCruft = chkCruft.Checked;
            bool postHasSignature = chkSignature.Checked;

            bool roomForImprovement = chkRoomForImprovement.Checked;
            bool splitParagraph = chkSplitParagraph.Checked;
            bool jeopardyCompliance = chkJeopardyCompliance.Checked;

            bool begging = chkBegging.Checked;
            bool indirectBegging = chkIndirectBegging.Checked;

            bool titleCasing = chkSentenceCasing.Checked;

            bool contentHiddenByHTMLtag = chkHiddenAsHTML.Checked;

            bool StackOverflowSpelling = chkSpellingOfStackOverflow.Checked;

            txtCheckinMessage.Text =
              mCheckinMessageBuilder.getCheckinMessage(
                  postHasCruft,
                  postHasSignature,
                  roomForImprovement,
                  splitParagraph,
                  jeopardyCompliance,
                  begging,
                  indirectBegging,
                  titleCasing,
                  contentHiddenByHTMLtag,
                  StackOverflowSpelling
                );
        } //updateCheckinMessage()


        //PM_REFACTOR 2011-11-02
        //
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void clipboardUpdate(string aNewText)
        {
            //PM_SPELLING_OVER_WIKIPEDIA 2011-04-05
            //string clipboardStr = WikipediaURL;
            string clipboardStr = aNewText;

            bool changeClipboardOnLookup = chkChangeClipboardOnLookup.Checked;
            if (changeClipboardOnLookup)
            {
                setClipboard2(clipboardStr);
            }

            //PM_SPELLING_OVER_WIKIPEDIA 2011-04-05
            //txtOutputURL.Focus();

            txtCorrected.Focus();
        } //updateCheckinMessage()


        /****************************************************************************
         *                                                                          *
         *    This function may change the contents of the clipboard                *
         *                                                                          *
         ****************************************************************************/
        private void doLookup_real(string aToLookUp, bool aGuessURL_ifFailedLookup)
        {
            // Is chkChangeClipboardOnLookup respected? Yes, in
            // clipboardUpdate() below.

            string toLookUP = textSearchWord.Text;


            string lookUpFailureText = string.Empty; // Default.
            string coreString = string.Empty;
            string correctedText = string.Empty; // Default. We use it as a flag.
            string WikipediaURL = string.Empty;
            int URlcount = -1;
            

            // GUI independent
            {
                //string wordToLookup = textSearchWord.Text;
                LookUpString tt2 = new LookUpString(toLookUP);
                coreString = tt2.getCoreString();

                string leading = tt2.getLeading();
                string trailing = tt2.getTrailing();

                string correctedWord;

                //Note about the variable name: It is not always
                //a Wikipedia URL... Some are for Wiktionary, MSDN, etc.
                WikipediaURL =
                    mWikipediaLookup.lookUp(
                        coreString,
                        aGuessURL_ifFailedLookup,
                        out correctedWord);
                if (WikipediaURL.Length > 0)
                {
                    mCheckinMessageBuilder.addWord(correctedWord, WikipediaURL);

                    //In user output: include leading and trailing whitespace
                    //from the input (so it works well with keyboard
                    //combination Shift + Ctrl + right Arrow).
                    correctedText = leading + correctedWord + trailing;

                    URlcount = mCheckinMessageBuilder.getNumberOfWords();
                }
                else
                {
                    lookUpFailureText = "Could not lookup " + coreString + "!";
                }
            } // GUI independent


            // GUI
            if (correctedText != string.Empty)
            {
                txtCorrected.Text = correctedText;
                txtOutputURL.Text = WikipediaURL;

                textLookupCounts.Text = URlcount.ToString();

                //textLookupCounts.BackColor = Color.Gray;
                Color colourToUse;
                if (URlcount == 1)
                {
                    colourToUse = Color.Salmon;
                }
                else
                {
                    colourToUse = Color.FromKnownColor(KnownColor.Control);

                    //Alternative?:
                    //From http://stackoverflow.com/questions/760543
                    //
                    //  SystemColors.Control
                }
                textLookupCounts.BackColor = colourToUse;

                //Color.FromKnownColor;
                //Color.FromName;
                //KnownColor.Control

                //Color.Gray
                //Later: save original and restore.

                //  Ref: <http://stackoverflow.com/questions/506641>

                updateCheckinMessage();

                clipboardUpdate(correctedText);

                // Note: not 100% correct wrt. leading/trailing white space...
                tstrLabel2.Text =
                    "\"" + coreString + "\" looked up as \"" + 
                    correctedText + "\"...";

                this.ActiveControl = txtCheckinMessage;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(lookUpFailureText);
            }


            //XXXXXXXXX Not yet
            //mLastLookupTick = System.DateTime.Now();
        } //doLookup_real()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void doLookup(bool aGuessURL_ifFailedLookup)
        {
            string toLookUp = textSearchWord.Text;

            if (toLookUp.Length > 0)
            {
                doLookup_real(toLookUp, aGuessURL_ifFailedLookup);
            }
            else
            {
                string msg = "Nothing to look up!";
                System.Windows.Forms.MessageBox.Show(msg);
            }
        } //doLookup()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        protected override void OnLoad(EventArgs anEvent)
        {
            //This works for setting (start) focus to a particular control and
            //not the one with the lowest tab order...
            //
            //From: <http://stackoverflow.com/questions/1002052/how-to-specify-which-control-should-be-focused-when-a-form-opens/1002168#1002168>
            //
            //  "How to specify which control should be focused when a form opens?"
            //

            base.OnLoad(anEvent);

            this.ActiveControl = textSearchWord;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnImport_Click(object aSender, EventArgs anEvent)
        {
            //Get the selected text from the big edit area...

            string someText = txtInputArea.Text;
            string someText2 = txtInputArea.SelectedText;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void clearCheckin()
        {
            //For now: uncheck all. Later we could have some defaults values
            chkCruft.Checked = false;
            chkRoomForImprovement.Checked = false;
            chkSplitParagraph.Checked = false;
            chkJeopardyCompliance.Checked = false;

            mCheckinMessageBuilder.resetWords();
            txtCheckinMessage.Text = "";

            textLookupCounts.Text = "";
            textLookupCounts.BackColor = Color.LightGreen;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnClearCheckinMessage_Click(object aSender, EventArgs anEvent)
        {
            clearCheckin();
        } //btnClearCheckinMessage_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnMinorEditStr_Click(object aSender, EventArgs anEvent)
        {
            clearCheckin();

            bool roomForImprovement = chkRoomForImprovement.Checked;
            txtCheckinMessage.Text =
                CheckinMessageBuilder.getMinorEditStr(roomForImprovement);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOpenURLs_Click(object aSender, EventArgs anEvent)
        {
            string URLlistText = txtInputArea.Text;

            List<string> URL_list = new List<string>(URLlistText.Split('\n'));

            double delaySecs = double.Parse(txtOpenTimeInterval.Text);

            int len = URL_list.Count;
            int i = 0;
            foreach (string someURL2 in URL_list)
            {
                string effectiveURL = someURL2.Trim(); // Removes leading AND
                                                       // trailing whitespace.

                if (effectiveURL.Length > 5) // Usually empty line. This test
                // could be more robust.
                {
                    ////For now. Will freeze the application.
                    ////One reference is:
                    ////  http://stackoverflow.com/questions/91108
                    //System.Threading.Thread.Sleep(delaySecs * 1000);
                    //Application.DoEvents();
                    network.delayWithDoEvents(delaySecs);

                    i++;
                    lblStatus.Text =
                        "To open (" + i + " of approx." + len +
                        ", interval " + delaySecs + " secs): " + effectiveURL;

                    network.openURL(effectiveURL);
                }
                else
                {
                    int peter2 = 2;
                }

                if (!mRunning)
                {
                    break;
                }
            } //Through URL_list

        } //btnOpenURLs_Click()


        //PM_REFACTOR 2010-02-24
        //Should be moved to another file...
        /****************************************************************************
         *    Opens a page in the default browser with a list of question           *
         *    with no answer. It assumes the SOFU search                            *
         *    keywords "answers", "wiki", "closed",                                 *
         ****************************************************************************/
        private void openNoAnswersPage(int aPageNumber, string aDomainName)
        {
            //answers:0 wiki:0 closed:0

            //From BAT script:
            //  http://superuser.com/search?q=answers%%3a0%%20wiki%%3a0%%20closed%%3a0^&tab=relevance^&page=3
            //

            string pageNumStr = aPageNumber.ToString();
            string urlToOpen =
                "http://" + aDomainName + "/" +
                "search?pagesize=50&q=answers%3a0%20wiki%3a0%20closed%3a0^&tab=relevance^&page=" +
                pageNumStr;
            network.openURL(urlToOpen);
        } //openNoAnswersPage()


        //Should be moved to another file...
        /****************************************************************************
         *                                                                          *
         *    Opens the specified post number in the default browser.               *
         *                                                                          *
         ****************************************************************************/
        private void openPost(int aPostNumber, string aDomainName)
        {
            string postNumberStr = aPostNumber.ToString();
            string urlToOpen =
                "http://" + aDomainName + "/" +
                "questions/" +
                postNumberStr;
            network.openURL(urlToOpen);
        } //openPost()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenRandomPage_noAnswers_2_Click(
            object aSender, EventArgs anEvent)
        {
            Random randNum = new Random();

            int unansweredQuestions = mSites.getUnansweredQuestions();
            int questionNum = randNum.Next(1, unansweredQuestions);
            int pageNum = questionNum / 50 + 1; // Presumes openNoAnswersPage()
                                                // uses a page size of 50.
            openNoAnswersPage(pageNum, mSites.getCurrentSiteDomainURL());
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuExit_Click(object aSender, EventArgs anEvent)
        {
            this.Close();
        }


        /****************************************************************************
         *    Essetially                                               *
         ****************************************************************************/
        private void mnuInsertAndFillIn_Click(object aSender, EventArgs e)
        {
            doLookupFromClipboard(true, false);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnShortenTime_Click(object aSender, EventArgs anEvent)
        {
            mTiming.shortenTime();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnMoreTime_Click(object aSender, EventArgs e)
        {
            mTiming.lengthenTime();

        } //btnMoreTime_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnResetTimer_Click(object aSender, EventArgs anEvent)
        {
            mTiming.resetTime();
        }


        //PM_REFACTOR 2010-05-01
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private int extractPostID(string aURLwithID)
        {
            string rawInputText = aURLwithID;

            //Try to match several forms of URLs.

            //Sample: <http://stackoverflow.com/questions/1858064/so-my-girlfriend-wants-to-learn-to-program/1858069#1858069>
            string IDstr = Regex.Match(rawInputText, @"/(\d+)/").Groups[1].Value;

            //Later: some error checking.
            int ID = -1;
            bool result = Int32.TryParse(IDstr, out ID);
            if (!result)
            {
                //Second try:
                //
                // Just replace anything that is ***not*** a digit
                // with an empty string (good enough for opening
                // the original page while editing).
                //
                // Failed on: <http://stackoverflow.com/questions/1858064/so-my-girlfriend-wants-to-learn-to-program/1858069#1858069>
                Regex rgx = new Regex(@"\D");
                IDstr = rgx.Replace(rawInputText, "");

                result = Int32.TryParse(IDstr, out ID);
                if (!result)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Could not extract post ID from " + aURLwithID);
                }
            }
            return ID;
        } //extractPostID()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void OpenTimeLineInBrowser(string anTextWithIntegerIDs)
        {
            int ID = extractPostID(anTextWithIntegerIDs);
            string URLtoOpen =
                "http://" + mSites.getCurrentSiteDomainURL() +
                "/posts/" + ID.ToString() + "/timeline";
            network.openURL(URLtoOpen);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOpenTimeLineInBrowser_Click(object aSender, EventArgs anEvent)
        {
            //Ref: <http://meta.stackoverflow.com/questions/36303>
            //
            //  To get to it, enter question URLs
            //  like so: http://meta.stackoverflow.com/posts/{id}/timeline
            //
            //
            OpenTimeLineInBrowser(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenTimeLineInBrowser_Click(object aSender, EventArgs e)
        {
            OpenTimeLineInBrowser(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void OpenRevisionsInBrowser(string anTextWithIntegerIDs)
        {
            int ID = extractPostID(anTextWithIntegerIDs);
            string URLtoOpen =
                "http://" + mSites.getCurrentSiteDomainURL() +
                "/posts/" + ID.ToString() + "/revisions";

            //Sample: http://stackoverflow.com/posts/2803870/revisions

            network.openURL(URLtoOpen);
        }


        //PM_GUI_HINTS 2010-09-04
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void txtID_TextChanged(object aSender, EventArgs anEvent)
        {
            openButtonsUpdate(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void QuestionDepth()
        {
            string URLtoOpen = "http://" + mSites.getCurrentSiteDomainURL() + "/";
            network.openURL(URLtoOpen);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuResetTimer_Click_1(object aSender, EventArgs anEvent)
        {
            mTiming.resetTime();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuLessTimeTool_Click(object aSender, EventArgs e)
        {
            mTiming.shortenTime();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMoreTimeTool_Click(object aSender, EventArgs e)
        {
            mTiming.lengthenTime();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnCrash1_Click(object aSender, EventArgs e)
        {
            int div = int.Parse(txtOpenTimeInterval.Text);

            int peter = 1 / div;

        }



        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkCruft_CheckedChanged(object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();

            if (chkCruft.Checked) //Use something in anEvent instead of reading
            //directly off the GUI?
            {
                //Signature is a subset of cruft. We make them mutually
                //exclusive as check (as a radio button)
                chkSignature.Checked = false;
            }
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkSignature_CheckedChanged(object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();

            if (chkSignature.Checked) //Use something in anEvent instead of reading
            //directly off the GUI?
            {
                //Signature is a subset of cruft. We make them mutually
                //exclusive as check (as a radio button)
                chkCruft.Checked = false;
            }
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkRoomForImprovement_CheckedChanged(
            object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkSplitParagraph_CheckedChanged(
            object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkJeopardyCompliance_CheckedChanged(
          object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkHiddenAsHTML_CheckedChanged(
          object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();
        } //mnuSearchOnWikipedia_Click()



        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkBegging_CheckedChanged(object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkIndirectBegging_CheckedChanged(
            object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void chkSentenceCasing_CheckedChanged(
            object aSender, EventArgs anEvent)
        {
            updateCheckinMessage();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void menuExportWordlist_Click(object aSender, EventArgs anEvent)
        {
            string Wordlist_HTML =
              mWikipediaLookup.dumpWordList_asHTML(
                mCodeFormattingCheck.combinedAllOfRegularExpressions(),

                // This would be inconsistent if the date changes right 
                // after the call of fullVersionStr()...
                mApplication.fullVersionStr(),
                mApplication.versionString_dateOnly());

            //Output for now
            txtInputArea.Text = Wordlist_HTML;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuExportWordlistAsSQL_Click(object aSender, EventArgs anEvent)
        {
            string wordlist_SQL = mWikipediaLookup.dumpWordList_asSQL();

            //Output for now
            txtInputArea.Text = wordlist_SQL;
        } // mnuExportWordlistAsSQL_Click()


        /****************************************************************************
         *    Open the main page a site to see the current time depth
         *    of questions - important if settting the timer manually
         *    in order to make sure there is only one edited question
         *    on the main page at any given time (rate-limiting edits).                                               *
         ****************************************************************************/
        private void btnQuestionDepth_Click(object aSender, EventArgs anEvent)
        {
            QuestionDepth();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuQuestionDepth_Click(object aSender, EventArgs anEvent)
        {
            QuestionDepth();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void OpenID(string anTextWithIntegerIDs)
        {
            int ID = extractPostID(anTextWithIntegerIDs);

            string URLtoOpen =
                "http://" + mSites.getCurrentSiteDomainURL() + "/questions/" +
                ID.ToString();
            network.openURL(URLtoOpen);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOpenRevisionsInBrowser_Click(object aSender, EventArgs anEvent)
        {
            OpenRevisionsInBrowser(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenRevisionsInBrowser_Click(object aSender, EventArgs e)
        {
            OpenRevisionsInBrowser(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void OpenDeletedPost(string anTextWithIntegerIDs)
        {
            int ID = extractPostID(anTextWithIntegerIDs);

            //Courtesy <http://meta.stackoverflow.com/questions/111858>! But
            //is it still working??
            string URLtoOpen =
                "http://" + mSites.getCurrentSiteDomainURL() +
                "/posts/" + ID.ToString() + "/ajax-load";
            network.openURL(URLtoOpen);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOpenDeletedPost_Click(object aSender, EventArgs anEvent)
        {
            OpenDeletedPost(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenDeletedPost_Click(object aSender, EventArgs anEvent)
        {
            OpenDeletedPost(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSelectInputField_Click(object aSender, EventArgs anEvent)
        {
            this.ActiveControl = textSearchWord;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSelectCheckinMessage_Click(object aSender, EventArgs anEvent)
        {
            this.ActiveControl = txtCheckinMessage;
        }


        /****************************************************************************
         *    Take input from clipboard, insert in lookup field and do              *
         *    the Wikipedia lookup.                                                 *
         ****************************************************************************/
        private void mnuInsertAndLookup_Click(object aSender, EventArgs anEvent)
        {
            doLookupFromClipboard(false, false);
        }


        //PM_REFACTOR 2012-01-30
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void openMarkdownUtility(bool aStartWithContent)
        {
            //Reading directly from the GUI. Is that proper?
            string term = "";
            string URL = "";

            if (aStartWithContent)
            {
                term = txtCorrected.Text;
                URL = txtOutputURL.Text;
            }

            //Note: Starting this dialog will actually change the
            //      clipboard ()
            Forms.frmMarkdown dialog =
                new Forms.frmMarkdown(term, URL, mLinkRefGenerator, mApplication);
            dialog.Show();
        } //openMarkdownUtility


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenRandomFile_Click(object aSender, EventArgs anEvent)
        {
            Forms.frmRandomFileOpen dialog =
                new Forms.frmRandomFileOpen(mApplication);
            dialog.Show();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenRandomFile_2_Click(
            object aSender, EventArgs anEvent)
        {
            Forms.frmRandomFileOpen dialog =
                new Forms.frmRandomFileOpen(mApplication);
            dialog.Show();
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMarkdownUtility_empty_2_Click(object aSender,
                                                      EventArgs anEvent)
        {
            openMarkdownUtility(false);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMarkdownUtility_2_Click(object aSender, EventArgs anEvent)
        {
            openMarkdownUtility(true);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuInsert_LookupAndOpenMarkdownUtility_Click(
            object aSender, EventArgs anEvent)
        {
            // A combination of other commands...

            doLookupFromClipboard(false, false);
            openMarkdownUtility(true);
        } //mnuInsert_LookupAndOpenMarkdownUtility_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuClearCheckinMessageInsertInLookupFieldAndLookup_Click(
            object aSender, EventArgs anEvent)
        {
            // A combination of other commands...

            clearCheckin();
            doLookupFromClipboard(false, false);
        }


        /****************************************************************************
         *                                                                          *
         *    Centralise reading from the clipboard here                            *
         *                                                                          *
         ****************************************************************************/
        private string getStringFromClipboard()
        {
            // Perhaps move to EditorOverflowApplication, like setClipboard3() -
            // for symmetry?


            // What about non-text, etc.???? Is this a bug?
            return System.Windows.Forms.Clipboard.GetText();
        }


        /****************************************************************************
         *                                                                          *
         *    Common for this class to avoid redundant code (reporting errors,      *
         *    convenient call, etc.)                                                *
         *                                                                          *
         ****************************************************************************/
        private void setClipboard2(string aClipboardStr)
        {
            string labelText;
            mApplication.setClipboard3(aClipboardStr, out labelText);

            tstrLabel2.Text = labelText;
        } //setClipboard2()



        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void wrapInputFromClipboard(string aBeforeStr, string aAfterStr)
        {
            string inputStr = getStringFromClipboard();

            textSearchWord.Text = inputStr;
            string correctedText = aBeforeStr + inputStr + aAfterStr;
            txtCorrected.Text = correctedText;
            clipboardUpdate(correctedText);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuQuoteAsCode_Click(object aSender, EventArgs anEvent)
        {
            wrapInputFromClipboard(WikipediaLookup.kCodeQuoteStr,
                                   WikipediaLookup.kCodeQuoteStr);
        }


        //PM_HTML_KEYBOARD_UTILITY 2012-01-30
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuFormatAsKeyboard_Click(object aSender, EventArgs anEvent)
        {
            wrapInputFromClipboard("<kbd>", "</kbd>");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuFormatAsSubscript_Click(object aSender, EventArgs anEvent)
        {
            wrapInputFromClipboard("<sub>", "</sub>");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuEncloseInBrackets_Click(object aSender, EventArgs anEvent)
        {
            wrapInputFromClipboard("<", ">");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSelectPostIDfield_Click(object aSender, EventArgs anEvent)
        {
            this.ActiveControl = txtID;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSelectCurrentDate_Click(object aSender, EventArgs anEvent)
        {
            this.ActiveControl = txtCurrentDate;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSelectURLfield_Click(object aSender, EventArgs anEvent)
        {
            this.ActiveControl = txtInputArea;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuHelpAbout_Click(object aSender, EventArgs anEvent)
        {
            string msg = mApplication.fullVersionStr();
            System.Windows.Forms.MessageBox.Show(msg);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public static string appPath2()
        {
            //What should it be??? The Visual Basic code is:
            //
            //  '"[Assembly]" requires System.Reflection
            //  Dim strAppDir As String = _
            //    Path.GetDirectoryName( _
            //      [Assembly].GetExecutingAssembly().GetModules(False)(0).FullyQualifiedName)
            //  Return strAppDir
            //
            // Translated by <http://www.developerfusion.com/tools/convert/vb-to-csharp/>.
            //
            ////"[Assembly]" requires System.Reflection
            //string strAppDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules(false)(0).FullyQualifiedName);
            //return strAppDir;

            return "xyz"; //Stub
        } //appPath


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string applicationPath()
        {

            string strApplicationDir =
              System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetModules(false)[0].FullyQualifiedName);

            return strApplicationDir;
        } //applicationPath()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void showFileInWindowsExplorer(
          string anInDiskItemPath, string anInFileNotExistMsg)
        {

            string effectivePath = anInDiskItemPath; //Default, if file exists...
            bool doOpen = true;

            bool fileExists2 = File.Exists(anInDiskItemPath);

            bool folderExists = Directory.Exists(anInDiskItemPath);
            bool diskItemExists = fileExists2 || folderExists;

            if (! diskItemExists)
            {
                //File/folder no longer exist. Open first
                //existing ***containing*** folder that exists...

                //'Try to find a containing folder that exists.
                string rootDir = Path.GetPathRoot(anInDiskItemPath);
                string candidate = anInDiskItemPath;
                bool done = false;
                while (! done)
                {
                    if (candidate == rootDir)
                    {
                        candidate = ""; //No containing directories exists...
                        done = true;
                    }
                    else
                    {
                        candidate = Path.GetDirectoryName(candidate);
                        if (Directory.Exists(candidate))
                        {
                            done = true;
                        }
                    }
                }
                effectivePath = candidate;

                string msgStr1 =
                  "File does not exist: " + anInDiskItemPath + ". ";

                string msgStr2 = null;
                if (effectivePath == "")
                {
                    msgStr2 = ""; // None of the containing folders exists, not
                                  // even the drive.
                    doOpen = false;
                }
                else
                {
                    msgStr2 =
                      "An existing containing folder, " +
                      effectivePath + ", will be opened instead.";
                }

                System.Windows.Forms.MessageBox.Show(msgStr1 + msgStr2);
            } //File does not exist.

            if (doOpen)
            {
                string explorerName = "explorer.exe";
                //string prefix3 = "/e,/select,"""; //To open Explorer.
                string prefix3 = "/e,/select,\""; //To open Explorer.

                if (! diskItemExists)
                {
                    prefix3 = "/e,\""; //To display the content of the
                                       //  first existing containing folder. It may or
                                       //  may not be empty.
                }

                string postfix3 = "\""; //To open Explorer.
                string params3a = prefix3 + effectivePath + postfix3;
                Process myProcess3 = Process.Start(explorerName, params3a);
            }
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenApplicationFolder_Click(
            object aSender, EventArgs anEvent)
        {
            try
            {
                string applicationFolderPath = applicationPath();

                string someFileInAppFolder =
                    applicationFolderPath + Path.DirectorySeparatorChar +
                    "EditOverflow.exe";

                //'Er dette check virkelig nødvendigt??? Sket det ikke
                //'i showFileInWindowsExplorer()?
                //Dim toOpen As String = someFileInAppFolder
                //If Not File.Exists(toOpen) Then
                //    'If the file for some reason does not exist then the containing
                //    'folder of the application will be shown/selected instead -
                //    'the user will then have to open that.
                //    toOpen = applicationFolderPath
                //End If

                string toOpen = someFileInAppFolder;
                showFileInWindowsExplorer(
                  toOpen, "Opening the closest containing folder.");


            }
            catch (Exception exceptionObject)
            {
                //handleEventHandlerException( _
                //  "Open Application Folder", "mnuOpenApplicationFolder", _
                //  exceptionObject)
            }
            finally
            {
	            //Clean up.
            }
        } //mnuOpenApplicationFolder_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenRandomQuestion_2_Click(object sender, EventArgs e)
        {
            Random randNum = new Random();
            int postNum = randNum.Next(1, mSites.getPosts()); //Old: 10642249

            openPost(postNum, mSites.getCurrentSiteDomainURL());
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOpenURL_Click(object aSender, EventArgs anEvent)
        {
            network.openURL(txtOutputURL.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string setSearchFieldFromClipboard()
        {
            string clipboardContext = getStringFromClipboard();

            //2016-08-27
            //
            // "1." is sometimes included when copying from Quora (when the
            // content is in an item in a numbered list)
            //
            // Sample:
            //
            //     1. beginner
            //
            clipboardContext = clipboardContext.Replace("1.", "");


            //2017-02-03
            //  When copying some text with a link from Quora we get the
            //  inline Markdown... Remove the link part so we can directly
            //  look it up without having to manually remove the link part.
            //
            //
            // Sample:
            //
            //     asp.net (http://asp.net)
            //
            int hasLink_index = clipboardContext.IndexOf("(http");
            if (hasLink_index > 0)
            {
                //Remove the link part (presumed to be the rest), e.g. "(http://asp.net)"
                clipboardContext = clipboardContext.Substring(0, hasLink_index);
            }

            // Nope! We must maintain information about leading and trailing
            // whitespace.
            //
            //clipboardContext = clipboardContext.Trim();

            // As per primo 2016, Quora adds a return at the end even when just
            // copying a single word on a line. Filter it out!
            clipboardContext = clipboardContext.Replace("\n", "");

            // "*" is sometimes included when copying from Quora (when the
            // content is in a list item and when it is in bold (two "*"s))
            //
            if (clipboardContext.StartsWith("*"))
            {
                clipboardContext = clipboardContext.Replace("*", "");
            }

            //Only leave one leading and one trailing whitespace (if any)
            clipboardContext = clipboardContext.Replace("  ", " ");
            clipboardContext = clipboardContext.Replace("  ", " ");
            clipboardContext = clipboardContext.Replace("  ", " ");
            clipboardContext = clipboardContext.Replace("  ", " ");
            clipboardContext = clipboardContext.Replace("  ", " ");
            clipboardContext = clipboardContext.Replace("  ", " ");
            clipboardContext = clipboardContext.Replace("  ", " ");

            textSearchWord.Text = clipboardContext;
            return clipboardContext;
        } //setSearchFieldFromClipboard()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void doLookupFromClipboard(bool aGuessURL_ifFailedLookup,
                                           bool aOpenURL)
        {
            setSearchFieldFromClipboard();
            doLookup(aGuessURL_ifFailedLookup);
            if (aOpenURL)
            {
                network.openURL(txtOutputURL.Text);
            }
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuInsertLookupAndOpenURL_Click(object aSender,
                                                     EventArgs anEvent)
        {
            // A combination of other commands...
            doLookupFromClipboard(false, true);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuCommandLineParametersDocumentation_Click(object aSender,
                                                                 EventArgs anEvent)
        {
            string msg1 = "EditOverFlow.exe [-l TERM] [-m] \n";

            string msg2 = "  -l TERM: Lookup TERM. If there are spaces in the " +
                          "term, enclose it in double quotes.\n";

            string msg3 = "  -m        : Open the Markdown window. "+
                          "If -l is specified then the lookup is performed first.\n";

            System.Windows.Forms.MessageBox.Show(
                msg1 + "\n" + msg2 + msg3,
                "Edit Overflow CommandLine Parameters Documentation");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenPost_Click(object aSender, EventArgs anEvent)
        {
            OpenID(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void btnOpenIDonStackOverflow_Click(object aSender, EventArgs anEvent)
        {
            //PM_REFACTOR 2010-05-01
            // string rawInputText = txtID.Text;
            //
            // //First try: just replace anything that is not a digit with an
            // //           empty string (good enough for opening original
            // //           page when while editing)
            //
            // Regex rgx = new Regex(@"\D");
            // string ID = rgx.Replace(rawInputText, "");

            OpenID(txtID.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenRandomPostTool_Click(object aSender, EventArgs anEvent)
        {
            Random RNG = new Random();

            // int RandomNumber = RNG.Next(14094786); //14094786: as of
            // //  2012-12-30T21:20:00. This includes both questions and answers.

            int RandomNumber = RNG.Next(mSites.getPosts()); //14094786: as of
            //  2012-12-30T21:20:00. This includes both questions and answers.

            string randomID = RandomNumber.ToString();

            OpenID(randomID);
        }


        /****************************************************************************
         *    Keeping drive O: active (USB drive that often sleeps).                *
         ****************************************************************************/
        private void timerDriveKeepAlive_Tick(object aSender, EventArgs anEvent)
        {
            int ticksSinceLastLookup = 0; // Not yet: System.DateTime.Now() - mLastLookupTick;

            if (ticksSinceLastLookup > 10000)
            {
                int peter10000 = 10000;
            }
        }


        /****************************************************************************
         *    If aSite is empty there isn't any site restriction.                   *
         ****************************************************************************/
        private void setSearchFieldFromClipboardAndSearchOnTheWeb(string aSite)
        {
            string someContent = setSearchFieldFromClipboard();
            searchOnTheWeb(aSite, someContent);
        }


        /****************************************************************************
         *    If aSite is empty there isn't any site restriction.                   *
         ****************************************************************************/
        private void searchOnTheWeb(string aSite, string aSomeSearchQuery)
        {
            if (aSomeSearchQuery != "")
            {
                //Later: trim leading and trailing spaces.
                //
                //Later: URL encode the search term

                string domainStr = "";
                if (aSite != "")
                {
                    domainStr = "site:" + aSite + "+";
                }

                //string URLforWikipediaSearch =
                //    "http://www.google.com/search?q=" + domainStr + someContent +
                //    "&num=%i&ie=utf-8&oe=utf-8";

                //DuckDuckGo. It should have been Blekko, but it died 2015-03-27
                //(<http://searchengineland.com/goodbye-blekko-search-engine-joins-ibms-watson-team-217633>)
                //Sample, https://duckduckgo.com/html/?q=site%3Aen.wikipedia.org+%s"
                string URLforWikipediaSearch =
                    "https://duckduckgo.com/html/?q=" + domainStr + aSomeSearchQuery +
                    "";
                network.openURL(URLforWikipediaSearch);
            }
        }


        /****************************************************************************
         *                                                                          *
         *    Result of menu item Action -> "Search on Wikipedia"                   *
         *                                                                          *
         ****************************************************************************/
        private void mnuSearchOnWikipedia_Click(object aSender,
                                                EventArgs anEventArgs)
        {
            //Make it work if something is ***typed*** into
            //the "Lookup" field (instead of relying on the
            //clipboard).
            //
            //setSearchFieldFromClipboardAndSearchOnTheWeb("en.wikipedia.org");
            searchOnTheWeb("en.wikipedia.org", textSearchWord.Text);
        }


        /****************************************************************************
         *                                                                          *
         *    Result of menu item Action -> "Search on DuckDuckGo"                  *
         *                                                                          *
         ****************************************************************************/
        private void mnuSearchOnGoogle_Click(object aSender, EventArgs anEventArgs)
        {
            setSearchFieldFromClipboardAndSearchOnTheWeb("");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void Idle_Count(object aSender, System.EventArgs anEvent)
        {
            mIdleCounter++;

            //label1.Text = idleCounter.ToString();
        }


        /****************************************************************************
         *    Test to see how quickly DoEvents() return...                          *
         *                                                                          *
         *    Result: about 1.2 million calls per second on one system...           *
         *                                                                          *
         *            With Thread.Sleep(1) it is was about 500 per                  *
         *            second (corresponding to about 2 ms sleep                     *
         *            instead of the specified 1 ms).                               *
         *                                                                          *
         ****************************************************************************/
        private void mnuCallingDoEvents_Click(object aSender, EventArgs anEvent)
        {
            //int calls = 5000 * 1000;
            int calls = 5 * 1000;

            int startTicks = Environment.TickCount;
            for (int i = 0; i < calls; i++)
            {
                Application.DoEvents();

                System.Threading.Thread.Sleep(1);
            } //for

            int endTicks = Environment.TickCount;
            int elapsedTicks = endTicks - startTicks;

            double elapsedSecs = elapsedTicks * 0.001;
            double callsPerSecond = calls / elapsedSecs;

            System.Windows.Forms.MessageBox.Show(
                "Elapsed ticks: " + elapsedTicks +
                " (" + callsPerSecond.ToString("0.0") +
                " calls to DoEvents() per second");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private string pageURL(int aPageNumber, string aTag)
        {
            string siteURL = mSites.getCurrentSiteDomainURL();
            string sitePrefix = "http://" + siteURL + "/questions";

            string someURL = null;
            if (aTag != "")
            {
                someURL =
                  sitePrefix + "/tagged/" + aTag +
                  "?page=" + aPageNumber.ToString() + "&sort=newest&pagesize=50";

                //Sample:
                //  http://stackoverflow.com/questions/tagged/android?page=5&sort=newest&pagesize=50
            }
            else
            {
                someURL =
                    sitePrefix + "?page=" +
                    aPageNumber.ToString() + "&sort=newest&pagesize=50";
            }
            return someURL;
        } //pageURL()


        /****************************************************************************
         *    If aTag is an empty string, it open the most recent questions,        *
         *    that is, irrespective of tags.                                        *
         ****************************************************************************/
        private void openPageSeries(string aTag)
        {
            double delaySecs = double.Parse(txtOpenTimeInterval.Text);


            for (int i = 1; i <= 12; i++)
            {
                string someURL = pageURL(i, aTag);

                network.delayWithDoEvents(delaySecs);
                network.openURL(someURL);

                if (!mRunning)
                {
                    break;
                }
            } //for
        } //openPageSeries()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenTheLast10pagesOfMostRecentQuestions_2_Click(
            object aSender, EventArgs anEvent)
        {
            openPageSeries("");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenTheLast10pagesOfMostRecentAndroidQuestions_2_Click(
            object aSender, EventArgs anEvent)
        {
            openPageSeries("android");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenTheLast10pagesOfMostRecentPHPquestions_2_Click(
            object aSender, EventArgs anEvent)
        {
            openPageSeries("php");
        }


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        private void mnuOpenTheLast12pagesOfMostRecentPythonQuestions_2_Click(
            object aSender, EventArgs anEvent)
        {
            openPageSeries("python");
        }


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        private void mnuOpenTheLast12pagesOfMostRecentDotNetQuestions_2_Click(
            object sender, EventArgs e)
        {
            openPageSeries(".net");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuGenerateURLsForTheLast12pagesOfMostRecentQuestions_2_Click(
            object aSender, EventArgs anEvent)
        {
            List<string> pages = generatePageURLs(1, 12, "");

            mScratchSB.Length = 0;
            foreach (string someURL in pages)
            {
                mScratchSB.Append(someURL);
                mScratchSB.Append("\n");
            }

            //Output for now
            txtInputArea.Text = mScratchSB.ToString();
        }


        /***************************************************************************
        *    <placeholder for header>                                              *
        ****************************************************************************/
        private void mnuSelectCurrentSite_Click(object aSender, EventArgs anEvent)
        {
            frmSelectCurrentSite dialog = 
                new frmSelectCurrentSite(mSites, mApplication);
            //dialog.Show();
            dialog.ShowDialog();

            setTitle();
        }


        /****************************************************************************
        *    <placeholder for header>                                               *
        ****************************************************************************/
        private void mnuOpenAboutPageForCurrentSite_2_Click(object aSender,
                                                          EventArgs anEvent)
        {
            string siteURL = mSites.getCurrentSiteDomainURL();

            string partial = "/tour";

            // Some exceptions.
            if (siteURL == "data.stackexchange.com")
            {
                partial = "/help"; //Also "/about", but it redirects to "/help"...
            }

            if (siteURL == "area51.stackexchange.com")
            {
                partial = "/faq"; //Also "/help", but it redirects to "/faq"...
            }

            if (siteURL == "meta.stackoverflow.com")
            {
                partial = "/"; // "/tour" for this meta site redirects to the main
                               // site,"stackoverflow.com/tour"... This is not the case
                               // for Meta Stack Exchange
            }

            //Sample:  http://stackoverflow.com/tour
            string aboutPageURL = "http://" + siteURL + partial;

            network.openURL(aboutPageURL);
        }


        /****************************************************************************
        *    <placeholder for header>                                               *
        ****************************************************************************/
        private void mnuDisablingACheckbox_Click(object aSender, EventArgs anEvent)
        {
            chkSignature.Enabled = false;
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuNonBreakingSpace_Click(object aSender, EventArgs anEvent)
        {
            setClipboard2("&nbsp;");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuCharacterMicro_Click(object aSender, EventArgs anEvent)
        {
            setClipboard2("µ");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuCharacterDegree_Click(object aSender, EventArgs anEvent)
        {
            setClipboard2("°");
        }


        //Does not belong here in the GUI - is completely general.
        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private static string removeTrailingSpace(string aInputStr)
        {
            // Unicode:
            //
            //   <http://stackoverflow.com/questions/11293994/>
            //     "The string class can only store strings in UTF-16 encoding."
            //
            //   <http://stackoverflow.com/questions/9532340/how-to-remove-trailing-white-spaces-using-a-regular-expression-without-removing/21421934#21421934>
            //     How to remove trailing white spaces using a regular expression
            //     without removing empty lines (code cleaning)
            //
            //     ([^ \t\r\n])[ \t]+$
            //
            //   <https://msdn.microsoft.com/en-us/library/taz3ak2f%28v=vs.110%29.aspx>
            //     Regex.Replace
            //
            //       Multiline:
            //
            //         <https://msdn.microsoft.com/en-us/library/yd1hzczs%28v=vs.110%29.aspx>
            //           Regular Expression Options
            //
            //           "Multiline mode. Changes the meaning of ^ and $ so
            //           they match at the beginning and end, respectively,
            //           of any line, and not just the beginning and end of
            //           the entire string. For more information, see the
            //           "Multiline Mode" section in the Regular Expression
            //           Options topic."
            //
            //         For an explanation of "\r?$", see:
            //
            //           <https://msdn.microsoft.com/en-us/library/yd1hzczs%28v=vs.110%29.aspx#Multiline>
            //             Regular Expression Options, Multiline Mode
            //
            //Remove trailing space, including on lines with only whitespace.
            //
            //Note: uses RegexOptions.Singleline to achieve global replace...
            //
            //   RegexOptions.Singleline
            //   RegexOptions.Multiline
            //
            string result = Regex.Replace(aInputStr,
                // @"([^ \t\r\n])[ \t]+$", "",
                // @"([^ \t\r\n])[ \t]+$", @"$1",
                                          @"[ \t]+(\r?$)",
                                          @"$1",
                                          RegexOptions.Multiline);
            return result;
        } //removeTrailingSpace()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void handleTABsAndTrailingWhiteSpace(bool aChange)
        {
            string clipboardContext = getStringFromClipboard();

            int initialLen = clipboardContext.Length;

            if (initialLen>0)
            {
                string result = removeTrailingSpace(clipboardContext);

                int len1 = result.Length;
                int removedSpaces = initialLen - len1;

                string result2 = Regex.Replace(result, @"\t", "    "); //4 spaces...
                int len2 = result2.Length;
                int removedTABs =
                    (len2 - len1) / 3; // We replace a TAB with four spaces -
                                       // 3 more characters per TAB...

                const string prefix = "         ";  // For some reason the displayed
                                                    // text is cut on the left. For
                                                    // now we just add leading spaces
                                                    // to avoid the problem.

                string status;
                if (removedSpaces != 0 || removedTABs != 0)
                {
                    if (aChange)
                    {
                        setClipboard2(result2);
                        status = prefix +
                            "Replaced " + removedTABs +
                            " TABs with spaces and removed " +
                            removedSpaces + " trailing spaces...";
                    }
                    else
                    {
                        status = prefix +
                            "The text in the clipboard contains " +
                            removedTABs + " TABs and " +
                            removedSpaces + " trailing spaces...";
                    }
                }
                else
                {
                    status = "Neither trailing spaces nor TABs!";
                }

                tstrLabel2.Text = status;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(
                    "There isn't any text in the clipboard! " +
                    "Use Copy to place text into the clipboard.");
            }
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuCheckForTABsAndTrailingWhiteSpace_2_Click(
            object aSender, EventArgs anEvent)
        {
            handleTABsAndTrailingWhiteSpace(false);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuRemoveTABsAndTrailingWhiteSpace_2_Click(
            object aSender, EventArgs anEvent)
        {
            handleTABsAndTrailingWhiteSpace(true);
        }


        /****************************************************************************
         *                                                                          *
         *    Result of menu item Action -> "Search on Wiktionary"                  *
         *                                                                          *
         ****************************************************************************/
        private void mnuSearchOnWiktionary_Click(object sender, EventArgs e)
        {
            // We get false negatives on DuckDuckGO (because it does not
            // understand "site:"?)
            //
            //searchOnTheWeb("en.wiktionary.org", textSearchWord.Text);
            searchOnTheWeb("", "wiktionary " + textSearchWord.Text);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuCharacterOhm_Click(object aSender, EventArgs anEvent)
        {
            setClipboard2("Ω");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuTAB_characterToClipboard_Click(
            object aSender, EventArgs anEvent)
        {
            setClipboard2("\t");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuCharacterUnicorn_Click(object sender, EventArgs e)
        {
            setClipboard2("🦄");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuHTML_linebreakToClipboard_Click(object sender, EventArgs e)
        {
            setClipboard2(" <br/>");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuOpenTheLast12pagesOfMostRecentPythonQuestions_Click(
            object aSender, EventArgs anEvent)
        {
            openPageSeries("python");
        }


        /****************************************************************************
         *    Both aStartPage and anEndPage are inclusive.                          *
         ****************************************************************************/
        private List<string> generatePageURLs(int aStartPage,
                                              int anEndPage,
                                              string aTag)
        {
            int pages = anEndPage - aStartPage + 1;
            List<string> toReturn = new List<string>(pages);

            for (int i = aStartPage; i <= anEndPage; i++)
            {
                string someURL = pageURL(i, aTag);
                toReturn.Add(someURL);
            } //for

            return toReturn;
        }


        /****************************************************************************
         *   From the clipboard                                                     *
         ****************************************************************************/
        private void mnuFilterHTML_forYouTube_2_Click(object aSender,
                                                      EventArgs anEvents)
        {
            mScratchSB.Length = 0;

            string someHTML = getStringFromClipboard();

            Tokeniser someTokeniser = new Tokeniser(someHTML, " ");

            bool end = false;
            while (!end)
            {
                string someWord = someTokeniser.getNextWord();

                if (someWord != null)
                {
                    string someWord_Upper = someWord.ToUpper();

                    if (someWord_Upper.IndexOf("YOUTUBE.") >= 0)
                    {
                        string matchedString = someWord;

                        //Prepare for direct use
                        matchedString = matchedString.Replace("/embed/", "/watch?v=");

                        // Try isolating the URL. But we could probably do
                        // more than this, e.g. cut off at the left. Example of
                        // a returned line:
                        //
                        //     src="https://www.youtube.com/watch?v=XqakD0dXdjM
                        //
                        //
                        //
                        matchedString = matchedString.Replace("data-src=\"", "");
                        if (matchedString.EndsWith("\""))
                        {
                            matchedString =
                                matchedString.Substring(0, matchedString.Length - 1);
                        }

                        mScratchSB.Append(matchedString);
                        mScratchSB.Append("\n");
                    }
                }
                else
                {
                    end = true;
                }
            }

            if (mScratchSB.Length != 0)
            {
                setClipboard2(mScratchSB.ToString());
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(
                    "YouTube URL stuff was not found in the clipboard...");
            }
        } //mnuFilterHTML_forYouTube_2_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuArrowToClipboardToolStripMenu_Click(object aSender,
                                                            EventArgs anEvent)
        {
            // <http://www.javascripter.net/faq/mathsymbols.htm>
            //   Special Symbols and Math Symbols in HTML and JavaScript
            //
            // http://graphemica.com/%E2%86%92
            //
            //   rightwards arrow (U+2192)
            //   Unicode Code Point 	U+2192

            // HexCode  Numeric HTML entity  escape(chr)  encodeURI(chr)  Description
            // -----------------------------------------------------------------------------
            // \u2192   &#8594;	&rarr;	     %u2192	      %E2%86%92	      Rightwards arrow

            // Note: "&rarr;" actually seems to be supported on Stack Overflow.
            //       Example: <https://stackoverflow.com/questions/3368590/show-diff-between-commits/35331102#35331102>
            //

            setClipboard2("→");

        } //mnuArrowToClipboardToolStripMenu_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuArrowToClipboard_HTML_entity_Click(object aSender,
                                                           EventArgs anEvent)
        {
            // ID 10101
            setClipboard2("&rarr;");
        } //mnuArrowToClipboard_HTML_entity_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMenuFormattingToClipboard_Click(object aSender,
                                                        EventArgs anEvent)
        {
            setClipboard2("* &rarr; *");
        } //mnuMenuFormattingToClipboard_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMissingSpaceBeforeOpeningBracket_Click(
            object aSender, EventArgs anEvent)
        {
            setClipboard2(mCodeFormattingCheck.missingSpaceBeforeOpeningBracketRegex()); // 0
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMissingSpaceAfterColon_Click(object sender, EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.missingSpaceAfterColonRegex()); // 1
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMissingSpaceAfterComma_Click(object sender, EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.missingSpaceAfterCommaRegex()); // 2
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMissingSpaceAroundEqualSign_Click(object sender,
                                                          EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.missingSpaceAroundEqualSign()); // 3
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuMissingSpaceAroundStringConcatenation_Click(
            object sender, EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.missingSpaceAroundStringConcatenationRegex()); // 4
        }

        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSpaceBeforeComma_Click(object sender, EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.spaceBeforeCommaRegex()); // 5

        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSpaceBeforeColon_Click(object sender, EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.spaceBeforeColonRegex()); // 6
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSpaceBeforeParenthesis_Click(object aSender,
                                                     EventArgs anEvent)
        {
            setClipboard2(mCodeFormattingCheck.spaceBeforeParenthesisRegex()); // 7
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSpaceBeforeSemicolon_Click(object sender, EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.spaceBeforeSemicommaRegex());
        }


        /****************************************************************************
         *                                                                          *
         *    Combined...                                                           *
         *                                                                          *
         ****************************************************************************/
        private void mnuAllCheckRegexes_Click(object sender, EventArgs e)
        {
            setClipboard2(mCodeFormattingCheck.combinedAllOfRegularExpressions());
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void simpleTextReplaceInClipboard(string anInSearchText,
                                                  string anInReplaceText)
        {
            string someText = getStringFromClipboard();
            string someNewText = someText.Replace(anInSearchText, anInReplaceText);
            setClipboard2(someNewText);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuConvert_nbspToSpace_2_Click(object aSender,
                                                    EventArgs anEvent)
        {
            simpleTextReplaceInClipboard("&nbsp;", " ");
        }


        /****************************************************************************
         *                                                                          *
         *    The idea here is to take HTML source (that may contain VERY long      *
         *    lines), try to extract direct download URLs for podcasts, and make    *
         *    it convenient to download (generate a small HTML page with the        *
         *    links and open it in the default web browser).                        *
         *                                                                          *
         *    The menu command is "Filter HTML for media URLs and open link page"   *
         *                                                                          *
         ****************************************************************************/
        private void mnuFilterHTML_forMediaURLsAndOpenLinkPage_2_Click(
                           object aSender,
                           EventArgs anEvent)
        {
            //Note: some redundancy with mnuFilterHTML_forYouTube_Click()

            // Sample source:
            //
            //   view-source:https://www.dontpanicgeocast.com/?p=807
            //

            mScratchSB.Length = 0;

            string someHTML = getStringFromClipboard();

            Tokeniser someTokeniser = new Tokeniser(someHTML, " ");

            bool end = false;
            while (!end)
            {
                string someWord = someTokeniser.getNextWord();

                if (someWord != null)
                {
                    int wordLen = someWord.Length;

                    string someWord_Upper = someWord.ToUpper();

                    string startMatch = "HREF=\"/";
                    int lenStartMatch = startMatch.Length;

                    if (someWord_Upper.IndexOf(".MP3") >= 0 ||
                        someWord_Upper.IndexOf(".M4A") >= 0 ||
                        false
                       )
                    {
                        string matchedString = someWord;

                        // Special support for "Millionærklubben"...
                        if (someWord_Upper.IndexOf(startMatch) == 0)
                        {

                            // Assume it ends in "
                            //
                            // Sample:
                            //
                            //   /attachment/19476794/21412553/17d329e2544c8f676a9eef6ba184dabd/audio/download/21412553-eksplosiv-vaekst-i-app-okonomi.mp3
                            //
                            string siteAbsoluteURL =
                                someWord.Substring(
                                  lenStartMatch - 1, wordLen - lenStartMatch);

                            string millioURL =
                                "http://arkiv.radio24syv.dk" + siteAbsoluteURL;

                            mScratchSB.Append("<p>");
                            mScratchSB.Append("<a href=\"");
                            mScratchSB.Append(millioURL);
                            mScratchSB.Append("\">");
                            mScratchSB.Append(millioURL);
                            mScratchSB.Append("</a>");
                            mScratchSB.Append("</p>");

                            mScratchSB.Append("\n");
                        }

                        // Try isolating the URL. But we could probably do
                        // more than this, e.g. cut off at the left. Example of
                        // a returned line:
                        //
                        //     src="https://www.youtube.com/watch?v=XqakD0dXdjM
                        //
                        //
                        //
                        matchedString = matchedString.Replace("data-src=\"", "");
                        if (matchedString.EndsWith("\""))
                        {
                            matchedString =
                                matchedString.Substring(0, matchedString.Length - 1);
                        }

                        mScratchSB.Append(matchedString);
                        mScratchSB.Append("\n");
                    }
                }
                else
                {
                    end = true;
                }
            }

            if (mScratchSB.Length != 0)
            {
                string someContent = mScratchSB.ToString();
                setClipboard2(someContent);

                string HTMLcontent =
                    "<html>\r\n<head>Download MP3</head>\r\n<body>\r\n" +
                    someContent + "</body>\r\n";

                //Redundant with mnuForthTyping_direct_Click() - using an
                //application user folder to store a file and
                //let Windows open it.

                // file:///D:/temp2/To%20download.html

                // <https://msdn.microsoft.com/en-us/library/system.deployment.application.applicationdeployment.currentdeployment(v=vs.110).aspx>
                // <https://msdn.microsoft.com/en-us/library/d8saf4wy.aspx>
                //   Accessing Local and Remote Data in ClickOnce Applications
                //

                string containingFolder = "";
                try
                {
                    // This will throw exception
                    // System.Deployment.Application.InvalidDeploymentException
                    // if not running in a ClickOnce context...
                    //
                    containingFolder =
                        System.Deployment.Application.ApplicationDeployment.CurrentDeployment.DataDirectory;

                    //Alternative folder location:
                    //
                    //  https://robindotnet.wordpress.com/2009/08/19/where-do-i-put-my-data-to-keep-it-safe-from-clickonce-updates/
                    //    Where do I put my data to keep it safe from ClickOnce updates?

                }
                catch (Exception exceptionObject)
                {
                    int peter2 = 2; //Ignore for now
                }

                //Marker: 2019-05-27

                string fileName = "_downloadLinks.html";

                //Note: Currently redundant with mnuForthTyping_direct_Click().
                //
                // Samples of the actual full file path for the saved temporary
                // text file (HTML file):
                //
                //   In a ClickOnce context:
                //
                //      <C:\Users\giraf\AppData\Local\Apps\2.0\Data\4V6DEPZC.GAZ\BVD5RHBN.L76\edit..tion_f97223f0904d9ab3_0001.0001_59104359ecaf44ce\Data\_downloadLinks.html>
                //
                //   In a non-ClickOnce context:
                //
                //     This is (currently) effectively the current folder,
                //     usually the same (not a good idea to due possible
                //     permissions issues - we should use a standard data
                //     folder instead):
                //
                //       <C:\UserProf\EditOverflow\OverflowHelper\bin\Debug\_downloadLinks.html>
                //
                string fileName_fullPath =
                    Path.Combine(containingFolder, fileName);

                File.WriteAllText(fileName_fullPath, HTMLcontent);

                network.openURL(fileName_fullPath);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(
                    "Links to MP3/M4A audio files were not found in the clipboard...");
            }
        } //mnuFilterHTML_forMediaURLsAndOpenLinkPage_2_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void postNotImplemented()
        {
             // For now
             Trace.Assert(false, "Sorry, not implemented");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuTransformForYouTubeComments_2_Click(
            object aSender, EventArgs anEvent)
        {
            string inputStr = getStringFromClipboard();
            int inputStrLen = inputStr.Length;

            // Sample:
            //
            //   07 min 10 secs: Book: https://www.amazon.com/Writing-Well

            //Regex rgx = new Regex(@"(\d+)\s+min\s+");

            StringReplacerWithRegex replacer =
                new StringReplacerWithRegex(inputStr);

            // Convert time to YouTube format
            //
            // Example (note that there is a trailing space in the output -
            //          this is (probably) required by YouTube to recognise
            //          at time)
            //
            //   From
            //
            //     23 min 50 secs
            //
            //   to
            //
            //     23:50
            //
            replacer.transform(@"(\d+)\s+secs", @"$1 ");
            replacer.transform(@"(\d+)\s+min\s+", @"$1:");
            replacer.transform(@"(\d+)\s+h\s+", @"$1:");

            // Convert URLs so they do not look like URLs... (otherwise,
            // the entire comment will be automatically removed
            // by YouTube).
            //
            // Example:
            //
            //   From
            //
            //     http://en.wikipedia.org/wiki/JavaScript
            //
            //   to
            //
            //     en DOT wikipedia DOT org/wiki/JavaScript
            //
            replacer.transform(@"(\w)\.(\w)", @"$1 DOT $2");
            replacer.transform(@"https:\/\/", @"");
            replacer.transform(@"http:\/\/", @"");

            // Reversals for some of the false positives in URL processing
            //
            // Example: TBD
            //
            replacer.transform(@"E DOT g\.", @"E.g.");
            replacer.transform(@"e DOT g\.", @"e.g.");

            // Convert email addresses like so... (at least to
            // offer some protection (and avoiding objections
            // to posting)).
            //
            // For now, just globally replace "@"
            //
            // Example: TBD
            //
            replacer.transform(@"\@", @" AT ");

            //Delete at any time - why did we have it here??
            //// Do some reversals for the URL conversion, to cover
            //// some
            //replacer.transform(@"e DOT g\.", @"e.g.");

            // Convert "->" to a real arrow
            replacer.transform(@"->", @"→"); // Note: For YouTube it can not
            //                                        be the HTML entity, "&rarr;".


            string outputStr = replacer.currentString();
            int outputStrLen = outputStr.Length;

            clipboardUpdate(outputStr);

            tstrLabel2.Text =
                "Converted " + inputStrLen.ToString() + " characters to " +
                outputStrLen.ToString() + " characters.";
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void frmMainForm_Resize(object aSender, EventArgs anEvent)
        {
            // Trace.WriteLine

            string msg2 = "In frmMainForm_Resize...";

            //Debug.WriteLine(msg2);

            //OverflowHelper.Source.main.Utility.DebugWriteLineWithTimeStamp()
            Utility.DebugWriteLineWithTimeStamp(msg2);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuTrademarkSymbolToClipboard_Click(
            object aSender, EventArgs anEvent)
        {
            // <https://en.wikipedia.org/wiki/Trademark_symbol>

            // HexCode  Numeric  HTML entity  escape(chr)  encodeURI(chr)  Description
            // ----------------------------------------------------------------------------
            // \u2122   &#8482;	 &trade;    %u2122                         Trademark symbol

            // Note: "&trade;" actually seems to be supported on Stack Overflow.

            // Alternative: "®"

            setClipboard2("™");
        }


        //Helper function
        // Does not belong here in the GUI...
        /****************************************************************************
         *                                                                          *
         *    Helper function for mnuForthTyping_Click()                            *
         *                                                                          *
         *    aSleepTimeSpec can also be the name of a variable in AutoIt.          *
         *                                                                          *
         ****************************************************************************/
        private static void emptyLine(StringBuilder aScratchSB)
        {
            aScratchSB.Append(Environment.NewLine);
            aScratchSB.Append(Environment.NewLine);
        }


        //Helper function for AutoItScript(), addAutoItTyping().
        // Does not belong here in the GUI...
        /****************************************************************************
         *                                                                          *
         *    Helper function for mnuForthTyping_Click()                            *
         *                                                                          *
         *    aSleepTimeSpec can also be the name of a variable in AutoIt.          *
         *                                                                          *
         ****************************************************************************/
        private static void addAutoItTyping(string aStuffToType,
                                            string aSleepTimeSpec,
                                            StringBuilder aScratchSB)
        {
            aScratchSB.Append("Send(\"");
            aScratchSB.Append(aStuffToType);
            aScratchSB.Append("\")");
            aScratchSB.Append(Environment.NewLine);

            // Pause before the next. E.g. to allow a Forth system
            // to process the send characters
            aScratchSB.Append("Sleep(");
            aScratchSB.Append(aSleepTimeSpec);
            aScratchSB.Append(")");
            emptyLine(aScratchSB);
        }


        //Helper function for AutoItScript(), AutoItFileHeader().
        // Does not belong here in the GUI...
        /****************************************************************************
         *                                                                          *
         *    Returns the content for the first part of the AutoIt script, with     *
         *    any configuration necessary and with a delay so the user have         *
         *    time to change focus to another application, e.g. PuTTY.              *
         *                                                                          *
         *    Note: A side effect is that any content in the scratch                *
         *          stringbuilder is overwritten.                                   *
         *                                                                          *
         ****************************************************************************/
        private static void AutoItFileHeader(int aBaseDelay_millisecs,
                                             string aCharacterDelayVariableName,
                                             StringBuilder aScratchSB,
                                             EditorOverflowApplication anApplication)
        {
            // Is aScratchSB implicit an in/out parameter??? Or a ref parameter??

            aScratchSB.Length = 0;

            string versionStr = anApplication.fullVersionStr();
            aScratchSB.Append("; AutoIt file generated by ");
            aScratchSB.Append(versionStr);
            emptyLine(aScratchSB);

            aScratchSB.Append(
                "; *** Note: For this to work, change the keyboard layout in Windows ");
            aScratchSB.Append(Environment.NewLine);

            aScratchSB.Append(
                ";           to UK (not Danish). Otherwise pasting/typing of ");
            aScratchSB.Append(Environment.NewLine);

            aScratchSB.Append(
                @";           characters like @ and \ (backslash) will not work...");
            emptyLine(aScratchSB);

            aScratchSB.Append(@"$baseDelay    = ");
            aScratchSB.Append(aBaseDelay_millisecs.ToString());
            aScratchSB.Append(@" ; Nominally 100 ms, 0.1 secs");
            aScratchSB.Append(Environment.NewLine);
            aScratchSB.Append(aCharacterDelayVariableName + @"  = 10 * $baseDelay");
            aScratchSB.Append(Environment.NewLine);
            aScratchSB.Append(@"$lineEndDelay = 20 * $baseDelay");
            emptyLine(aScratchSB);

            // Perhaps later:
            //
            // AutoItSetOption("SendKeyDelay", 100)  ; Unit: ms.  "Alters the length of the brief pause in
            //                                       ;            between sent keystrokes. A value of 0 removes
            //                                       ;            the delay completely."

            // Pause so the user can switch focus to where the
            // characters should be "typed" (e.g. PuTTY, connected
            // to a Forth system).
            aScratchSB.Append("Sleep(5000) ; Allow time for the user ");
            aScratchSB.Append("to switch focus, e.g. to PuTTY.");
            emptyLine(aScratchSB);
        }


        // Does not belong here in the GUI...
        /****************************************************************************
         *                                                                          *
         *    Returns an AutoIt script that, when executed simulates (types         *
         *    out) the keystrokes corresponding to the input (after                 *
         *    5 seconds, so focus can be shifted to another                         *
         *    application, e.g. a PuTTY window).                                    *
         *                                                                          *
         *    The key here is to do it sufficient slowly so e.g. a Forth system     *
         *    has enough time to process the input. One strategy is to "type"       *
         *    fast, but make a 1 second pause after every 20 characters             *
         *    and after Return.                                                     *
         *                                                                          *
         *    Note: When used, the keyboard layout must an English-like             *
         *          keyboard (not e.g. Danish)                                      *
         *                                                                          *
         *                                                                          *
         *    Side effect: The existing content of the stringbuilder                *
         *                 will be wiped out.                                       *
         *                                                                          *
         ****************************************************************************/
        private static string AutoItScript(string aToBeTypedOut,
                                           StringBuilder aScratchSB,
                                           EditorOverflowApplication anApplication)
        {
            //Future:
            //
            //   See beginning of this file.

            aScratchSB.Length = 0;

            // Note: This fails if there are quotes in the input string.
            //       Quotes should be escaped (by a quote, '""'?)

            const int kMaxLen = 14; // E.g. to not overwrite the input
            //                         buffer in a Forth. AmForth is
            //                         one example.

            // How can we make this string constant (that is, protect from
            // redefinition)???
            string kCharacterDelayVariableName =
                @"$char" + kMaxLen.ToString() + "delay";  // E.g. "$char20delay"

            //const int kBaseDelay_milliSecs = 100;
            const int kBaseDelay_milliSecs = 50;

            AutoItFileHeader(kBaseDelay_milliSecs,
                             kCharacterDelayVariableName,
                             aScratchSB,
                             anApplication);

            // "!" means the Alt key in AutoIt, so we have to escape
            // that as "{!}".
            //
            // The same for "^", key Ctrl. Escape as "{^}". And for
            // "+", the Shift key.
            //
            // "{F11}" and "{F12}" also have special meaning, e.g. "{F11}"
            // for key F11. So we would have to Escape:
            //
            //     {  →  {{}
            //     }  →  {}}
            //
            // An alternative is use {ASC <code>}, e.g. {ASC 065} to "A".
            //
            // Refs.:
            //
            //   <https://www.autoitscript.com/autoit3/docs/appendix/SendKeys.htm>
            //
            //   <http://www.mytechsupport.ca/tools/docs/AutoIt_quickref.pdf>
            //

            StringReplacerWithRegex replacer =
                new StringReplacerWithRegex(aToBeTypedOut);

            // Note: "^", "{", and "{" are special regular expression characters
            //       by themselves and must be escaped (but apparently not in the
            //       output)... What about "!"?

            // This must be applied first!
            replacer.transform(@"\{", @"{{}");
            replacer.transform(@"\}", @"{}}");

            //Escape AutoIt modifier key so those characters will
            //be literally typed.

            const string kEscapeMarker = "__X__"; // We use it later to avoid
            //                                       breaking lines in the middle
            //                                       of an escaped sequence (e.g.,
            //                                       a literal "!" that has special
            //                                       meaning for AutoIt if not escaped).

            // Ref. <https://www.autoitscript.com/autoit3/docs/functions/Send.htm>
            //
            replacer.transform(@"!", kEscapeMarker + @"{!}"); // "!" is the "Ctrl" key
            replacer.transform(@"#", kEscapeMarker + @"{#}"); // "#" could be the Basic ##.### number specification
            replacer.transform(@"\^", kEscapeMarker + @"{^}"); // "^" is the "Alt" key. We
            //                                                    must also do escape for
            //                                                    the regular expressions
            //                                                    here...
            //
            replacer.transform(@"\+", kEscapeMarker + @"{+}"); // "+" is the Shift key

            // A literal quote in an AutoIt string needs to be escaped
            // by a quote (so it becomes two quotes in a text string -
            // for AutoIt's Send() in this case).
            //
            // The same kind of quoting is needed here in C#, so we end
            // up with 4 quotes...
            //
            replacer.transform(@"""", kEscapeMarker + @""""""); // That is, each literal
            //                                                     quote is replaced by
            //                                                     two quotes (escaped by
            //                                                     a quote)

            // Avoid any problems with TABs by replacing
            // them with 4 spaces (e.g. we would
            // probably have to escape them for
            // AutoIt to properly type them out).
            //
            replacer.transform("\t", @"    ");

            // Remove Forth comments, of the form " ( ... ) "
            replacer.transform(@" \( .*\)", @"");


            //Fix later (we have problems with end of line)
            //
            //// Remove Forth comments, of the form " \ ".
            ////Will there be exceptions to this??
            ////
            //replacer.transform(@"\\.*\n", @"");

            string inputStr = removeTrailingSpace(replacer.currentString());

            // We don't use RemoveEmptyEntries as we want empty lines in the
            // input to result in empty lines in the typing.
            string[] lines = inputStr.Split(new string[] { Environment.NewLine },
                                                           StringSplitOptions.None);

            foreach (string someLine4 in lines)
            {
                // Note: We must not break AutoIt escaped sequences
                //       into separate Send() lines (this will
                //       result in incorrect behaviour - see
                //       below for an example. E.g., all of
                //       "{!}" must be in the same Send() line).
                //
                // Example:
                //
                //   Input (without the quotes):
                //
                //       "     PORTB c!"
                //
                //   String after escaping (without the quotes):
                //
                //       "     PORTB c{!}"
                //
                //   Output if we don't account for this:
                //
                //       Send("     PORTB c{!")
                //       Send("}")


                // Split on "{" so we avoid splitting AutoIt escape sequences (e.g.
                // "{!}" for a literal "!" (used for the "Ctrl" key in AutoIt)).
                //
                // In most cases there will only be one item, but not
                // if there is an AutoIt escaped item.
                //
                // We make sure such items are at the beginning of
                // an AutoIt Send() line, so they will not be
                // split up by the 20 character chunking)
                //
                //Note: not "RemoveEmptyEntries" as we want to be able
                //      to handle if the string starts with an AutoIt
                //      escaped sequence.
                //
                string[] lines4 = someLine4.Split(new string[] { kEscapeMarker },
                                                  StringSplitOptions.None);

                foreach (string someLine in lines4)
                {
                    int len = someLine.Length;
                    int startIndex = 0;

                    bool done = false;
                    while (!done)
                    {
                        int endIndex = startIndex + kMaxLen; // Not inclusive

                        if (endIndex > len)
                        {
                            endIndex = len;
                            done = true;
                        }
                        int partOfLineLen = endIndex - startIndex;
                        string partOfLine = someLine.Substring(startIndex, partOfLineLen);

                        addAutoItTyping(partOfLine, kCharacterDelayVariableName, aScratchSB);

                        startIndex += kMaxLen;
                    } // Breaking up a single line (so there is some delay, in order
                      // not overflow in the input buffer in AmForth).
                }

                addAutoItTyping("{ENTER}", "$lineEndDelay", aScratchSB);

                aScratchSB.Append(Environment.NewLine); // This will result in
                //                                         two empty lines,
                //                                         visually separating
                //                                         the AutoIt code for
                //                                         each line in the input

            } // Through lines

            return aScratchSB.ToString();
        } //AutoItScript()


        /****************************************************************************
         *                                                                          *
         *    Takes input from clipboard and generates an AutoIt script that        *
         *    when executed simulates the keystrokes corresponding to the           *
         *    input (after a few seconds so focus can be shifted to another         *
         *    application, e.g. a PuTTY window.)                                    *
         *                                                                          *
         *    The key here is to do it sufficient slowly so e.g. a Forth system     *
         *    has enough time to process the input. One strategy is to "type"       *
         *    fast, but make a 1 second pause after every 20 characters             *
         *    and after Return.                                                     *
         *                                                                          *
         ****************************************************************************/
        private void mnuForthTyping_Click(object aSender, EventArgs anEvent)
        {
            string inputStr_raw = getStringFromClipboard();

            string someAutoItScript = AutoItScript(inputStr_raw, 
                                                   mScratchSB, 
                                                   mApplication);

            clipboardUpdate(someAutoItScript);
        } // mnuForthTyping_Click()


        //Does not belong here in the GUI... Should be moved somewhere else.
        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        private static string applicationDataFolder()
        {
            string dataFolder =
                System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.LocalApplicationData);

            return dataFolder;
        } // applicationDataFolder()


        /****************************************************************************
         *                                                                          *
         *    Direct typing out of the characters in the clipboard (intended        *
         *    for use in Forth windows (in PuTTY, connected to a Forth system       *
         *    over a serial connection)                                             *
         *                                                                          *
         *                                                                          *
         *    Implementation note:                                                  *
         *                                                                          *
         *      It is not a good idea to
         *      use a keyboard shortcut                    *
         *      with Ctrl + Shift for this menu command as                          *
         *      using the keyboard shortcut may change the                          *
         *      keyboard layout (which we are dependent on                          *
         *      for the AutoIt method).                                             *
         *                                                                          *
         *                                                                          *
         ****************************************************************************/
        private void mnuForthTyping_direct_Click(object aSender, EventArgs anEvent)
        {
            // We write out an AutoIt script to a temporary file and
            // let Windows open and execute it.

            string someAutoItScript =
                AutoItScript(getStringFromClipboard(), mScratchSB, mApplication);

            string workFolder = applicationDataFolder();

            //Slight change of collision - an ISO8601 date would be better.
            //
            //We also construct temporary files
            //mnuFilterHTML_forMediaURLsAndOpenLinkPage_2_Click(), use
            //Path.Combine, and open files by Windows. This redundancy
            //should be eliminated.
            //
            string tempFileName =
                "_TypeOut_" + Environment.TickCount.ToString() + ".au3";
            string tempFileName_fullPath =
                Path.Combine(workFolder, tempFileName);

            File.WriteAllText(tempFileName_fullPath, someAutoItScript);

            network.openURL(tempFileName_fullPath);

            //Marker: 2019-05-27
        } //mnuForthTyping_direct_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSelectCurrentEditSummaryStyle_Click(
            object aSender, EventArgs anEvent)
        {
            SelectEditSummaryStyle dialog =
                new SelectEditSummaryStyle(mEditSummaryStyle);
            dialog.ShowDialog();
        }


        /****************************************************************************
         *                                                                          *
         *    Handler for menu "Utility" -> "HTML Word List Without Words - test!"  *
         *                                                                          *
         *    The menu item is usually made invisible because it                    *
         *    is only used during development.                                      *
         *                                                                          *
         *    It is a means of generating the skeleton of HTML                      *
         *    export for regression testing when doing                              *
         *    refactoring of dumpWordList_asHTML(),                                 *
         *    without being affected by changes                                     *
         *    to the word list.                                                     *
         *                                                                          *
         ****************************************************************************/
        private void mnuHTML_WordListWithoutWords_Click(object aSender,
                                                        EventArgs anEvent)
        {
            // We call it with all empty variable information to get
            // a stable output for regression testing (get skeleton
            // HTML, with only the begining and end, with an empty
            // word table)
            Dictionary<string, string> someCaseCorrection =
                new Dictionary<string, string>();
            Dictionary<string, string> someWord2URL =
                new Dictionary<string, string>();
            Dictionary<string, string> someCaseCorrection_Reverse =
                new Dictionary<string, string>();
            string Wordlist_HTML =
              WikipediaLookup.dumpWordList_asHTML(
                "",
                ref someCaseCorrection,
                someCaseCorrection_Reverse.Count,
                ref someWord2URL,

                // This would be inconsistent if the date changes right 
                // after the call of fullVersionStr()...
                mApplication.fullVersionStr(),
                mApplication.versionString_dateOnly()
              );

            int len = Wordlist_HTML.Length;

            //Output for now
            txtInputArea.Text = Wordlist_HTML;
        } //mnuHTML_WordListWithoutWords_Click()


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void mnuSpaceAfterLeftParenthesis_Click(object aSender,
                                                        EventArgs anEvent)
        {
            setClipboard2(mCodeFormattingCheck.spaceAfterLeftParenthesisRegex()); //
        }


    } //class frmMainForm


} //namespace OverflowHelper



//Green:
//  DarkGreen              #FF006400.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkgreen(printer).aspx
//  DarkOliveGreen         #FF556B2F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkolivegreen(printer).aspx
//  DarkSeaGreen           #FF8FBC8F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkseagreen(printer).aspx
//  ForestGreen            #FF228B22.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.forestgreen(printer).aspx
//  G                      green component value                   http://msdn.microsoft.com/en-us/library/system.drawing.color.g(printer).aspx
//  Green                  #FF008000.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.green(printer).aspx
//  GreenYellow            #FFADFF2F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.greenyellow(printer).aspx
//  LawnGreen              #FF7CFC00.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lawngreen(printer).aspx
//  LightGreen             #FF90EE90.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightgreen(printer).aspx
//  LightSeaGreen          #FF20B2AA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightseagreen(printer).aspx
//  LimeGreen              #FF32CD32.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.limegreen(printer).aspx
//  MediumSeaGreen         #FF3CB371.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumseagreen(printer).aspx
//  MediumSpringGreen      #FF00FA9A.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumspringgreen(printer).aspx
//  PaleGreen              #FF98FB98.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.palegreen(printer).aspx
//  SeaGreen               #FF2E8B57.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.seagreen(printer).aspx
//  SpringGreen            #FF00FF7F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.springgreen(printer).aspx
//  YellowGreen            #FF9ACD32.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.yellowgreen(printer).aspx



//  From <http://msdn.microsoft.com/en-us/library/system.drawing.color.aspx>.
//
//  Notes:
//
//    Most second column has: "Gets a system-defined color that has an
//    ARGB value of ". E.g. "... #FFF0F8FF"
//
//    "this Color": [ http://msdn.microsoft.com/en-us/library/system.drawing.color.aspx ] structure.
//
//    "A system color is a color that is used in a Windows
//    display element. System colors are represented by elements
//    of the KnownColor"
//
//  A                      alpha component value                   http://msdn.microsoft.com/en-us/library/system.drawing.color.a(printer).aspx
//  AliceBlue              #FFF0F8FF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.aliceblue(printer).aspx
//  AntiqueWhite           #FFFAEBD7.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.antiquewhite(printer).aspx
//  Aqua                   #FF00FFFF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.aqua(printer).aspx
//  Aquamarine             #FF7FFFD4.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.aquamarine(printer).aspx
//  Azure                  #FFF0FFFF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.azure(printer).aspx
//  B                      blue component value                    http://msdn.microsoft.com/en-us/library/system.drawing.color.b(printer).aspx
//  Beige                  #FFF5F5DC.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.beige(printer).aspx
//  Bisque                 #FFFFE4C4.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.bisque(printer).aspx
//  Black                  #FF000000.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.black(printer).aspx
//  BlanchedAlmond         #FFFFEBCD.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.blanchedalmond(printer).aspx
//  Blue                   #FF0000FF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.blue(printer).aspx
//  BlueViolet             #FF8A2BE2.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.blueviolet(printer).aspx
//  Brown                  #FFA52A2A.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.brown(printer).aspx
//  BurlyWood              #FFDEB887.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.burlywood(printer).aspx
//  CadetBlue              #FF5F9EA0.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.cadetblue(printer).aspx
//  Chartreuse             #FF7FFF00.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.chartreuse(printer).aspx
//  Chocolate              #FFD2691E.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.chocolate(printer).aspx
//  Coral                  #FFFF7F50.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.coral(printer).aspx
//  CornflowerBlue         #FF6495ED.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.cornflowerblue(printer).aspx
//  Cornsilk               #FFFFF8DC.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.cornsilk(printer).aspx
//  Crimson                #FFDC143C.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.crimson(printer).aspx
//  Cyan                   #FF00FFFF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.cyan(printer).aspx
//  DarkBlue               #FF00008B.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkblue(printer).aspx
//  DarkCyan               #FF008B8B.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkcyan(printer).aspx
//  DarkGoldenrod          #FFB8860B.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkgoldenrod(printer).aspx
//  DarkGray               #FFA9A9A9.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkgray(printer).aspx
//  DarkGreen              #FF006400.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkgreen(printer).aspx
//  DarkKhaki              #FFBDB76B.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkkhaki(printer).aspx
//  DarkMagenta            #FF8B008B.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkmagenta(printer).aspx
//  DarkOliveGreen         #FF556B2F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkolivegreen(printer).aspx
//  DarkOrange             #FFFF8C00.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkorange(printer).aspx
//  DarkOrchid             #FF9932CC.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkorchid(printer).aspx
//  DarkRed                #FF8B0000.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkred(printer).aspx
//  DarkSalmon             #FFE9967A.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darksalmon(printer).aspx
//  DarkSeaGreen           #FF8FBC8F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkseagreen(printer).aspx
//  DarkSlateBlue          #FF483D8B.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkslateblue(printer).aspx
//  DarkSlateGray          #FF2F4F4F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkslategray(printer).aspx
//  DarkTurquoise          #FF00CED1.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkturquoise(printer).aspx
//  DarkViolet             #FF9400D3.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.darkviolet(printer).aspx
//  DeepPink               #FFFF1493.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.deeppink(printer).aspx
//  DeepSkyBlue            #FF00BFFF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.deepskyblue(printer).aspx
//  DimGray                #FF696969.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.dimgray(printer).aspx
//  DodgerBlue             #FF1E90FF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.dodgerblue(printer).aspx
//  Firebrick              #FFB22222.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.firebrick(printer).aspx
//  FloralWhite            #FFFFFAF0.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.floralwhite(printer).aspx
//  ForestGreen            #FF228B22.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.forestgreen(printer).aspx
//  Fuchsia                #FFFF00FF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.fuchsia(printer).aspx
//  G                      green component value                   http://msdn.microsoft.com/en-us/library/system.drawing.color.g(printer).aspx
//  Gainsboro              #FFDCDCDC.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.gainsboro(printer).aspx
//  GhostWhite             #FFF8F8FF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.ghostwhite(printer).aspx
//  Gold                   #FFFFD700.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.gold(printer).aspx
//  Goldenrod              #FFDAA520.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.goldenrod(printer).aspx
//  Gray                   #FF808080.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.gray(printer).aspx
//  Green                  #FF008000.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.green(printer).aspx
//  GreenYellow            #FFADFF2F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.greenyellow(printer).aspx
//  Honeydew               #FFF0FFF0.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.honeydew(printer).aspx
//  HotPink                #FFFF69B4.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.hotpink(printer).aspx
//  IndianRed              #FFCD5C5C.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.indianred(printer).aspx
//  Indigo                 #FF4B0082.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.indigo(printer).aspx
//  IsEmpty                ... whether ...uninitialized.           http://msdn.microsoft.com/en-us/library/system.drawing.color.isempty(printer).aspx
//  IsKnownColor           ... whether ... predefined color        http://msdn.microsoft.com/en-us/library/system.drawing.color.isknowncolor(printer).aspx
//  IsNamedColor           ... named color ... member KnownColor   http://msdn.microsoft.com/en-us/library/system.drawing.color.isnamedcolor(printer).aspx
//  IsSystemColor          ... whether ... system color            http://msdn.microsoft.com/en-us/library/system.drawing.color.issystemcolor(printer).aspx
//  Ivory                  #FFFFFFF0.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.ivory(printer).aspx
//  Khaki                  #FFF0E68C.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.khaki(printer).aspx
//  Lavender               #FFE6E6FA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lavender(printer).aspx
//  LavenderBlush          #FFFFF0F5.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lavenderblush(printer).aspx
//  LawnGreen              #FF7CFC00.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lawngreen(printer).aspx
//  LemonChiffon           #FFFFFACD.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lemonchiffon(printer).aspx
//  LightBlue              #FFADD8E6.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightblue(printer).aspx
//  LightCoral             #FFF08080.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightcoral(printer).aspx
//  LightCyan              #FFE0FFFF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightcyan(printer).aspx
//  LightGoldenrodYellow   #FFFAFAD2.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightgoldenrodyellow(printer).aspx
//  LightGray              #FFD3D3D3.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightgray(printer).aspx
//  LightGreen             #FF90EE90.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightgreen(printer).aspx
//  LightPink              #FFFFB6C1.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightpink(printer).aspx
//  LightSalmon            #FFFFA07A.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightsalmon(printer).aspx
//  LightSeaGreen          #FF20B2AA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightseagreen(printer).aspx
//  LightSkyBlue           #FF87CEFA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightskyblue(printer).aspx
//  LightSlateGray         #FF778899.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightslategray(printer).aspx
//  LightSteelBlue         #FFB0C4DE.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightsteelblue(printer).aspx
//  LightYellow            #FFFFFFE0.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lightyellow(printer).aspx
//  Lime                   #FF00FF00.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.lime(printer).aspx
//  LimeGreen              #FF32CD32.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.limegreen(printer).aspx
//  Linen                  #FFFAF0E6.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.linen(printer).aspx
//  Magenta                #FFFF00FF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.magenta(printer).aspx
//  Maroon                 #FF800000.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.maroon(printer).aspx
//  MediumAquamarine       #FF66CDAA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumaquamarine(printer).aspx
//  MediumBlue             #FF0000CD.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumblue(printer).aspx
//  MediumOrchid           #FFBA55D3.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumorchid(printer).aspx
//  MediumPurple           #FF9370DB.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumpurple(printer).aspx
//  MediumSeaGreen         #FF3CB371.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumseagreen(printer).aspx
//  MediumSlateBlue        #FF7B68EE.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumslateblue(printer).aspx
//  MediumSpringGreen      #FF00FA9A.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumspringgreen(printer).aspx
//  MediumTurquoise        #FF48D1CC.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumturquoise(printer).aspx
//  MediumVioletRed        #FFC71585.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mediumvioletred(printer).aspx
//  MidnightBlue           #FF191970.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.midnightblue(printer).aspx
//  MintCream              #FFF5FFFA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mintcream(printer).aspx
//  MistyRose              #FFFFE4E1.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.mistyrose(printer).aspx
//  Moccasin               #FFFFE4B5.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.moccasin(printer).aspx
//  Name                   Gets the name                           http://msdn.microsoft.com/en-us/library/system.drawing.color.name(printer).aspx
//  NavajoWhite            #FFFFDEAD.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.navajowhite(printer).aspx
//  Navy                   #FF000080.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.navy(printer).aspx
//  OldLace                #FFFDF5E6.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.oldlace(printer).aspx
//  Olive                  #FF808000.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.olive(printer).aspx
//  OliveDrab              #FF6B8E23.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.olivedrab(printer).aspx
//  Orange                 #FFFFA500.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.orange(printer).aspx
//  OrangeRed              #FFFF4500.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.orangered(printer).aspx
//  Orchid                 #FFDA70D6.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.orchid(printer).aspx
//  PaleGoldenrod          #FFEEE8AA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.palegoldenrod(printer).aspx
//  PaleGreen              #FF98FB98.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.palegreen(printer).aspx
//  PaleTurquoise          #FFAFEEEE.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.paleturquoise(printer).aspx
//  PaleVioletRed          #FFDB7093.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.palevioletred(printer).aspx
//  PapayaWhip             #FFFFEFD5.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.papayawhip(printer).aspx
//  PeachPuff              #FFFFDAB9.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.peachpuff(printer).aspx
//  Peru                   #FFCD853F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.peru(printer).aspx
//  Pink                   #FFFFC0CB.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.pink(printer).aspx
//  Plum                   #FFDDA0DD.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.plum(printer).aspx
//  PowderBlue             #FFB0E0E6.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.powderblue(printer).aspx
//  Purple                 #FF800080.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.purple(printer).aspx
//  R                      red component value                     http://msdn.microsoft.com/en-us/library/system.drawing.color.r(printer).aspx
//  Red                    #FFFF0000.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.red(printer).aspx
//  RosyBrown              #FFBC8F8F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.rosybrown(printer).aspx
//  RoyalBlue              #FF4169E1.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.royalblue(printer).aspx
//  SaddleBrown            #FF8B4513.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.saddlebrown(printer).aspx
//  Salmon                 #FFFA8072.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.salmon(printer).aspx
//  SandyBrown             #FFF4A460.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.sandybrown(printer).aspx
//  SeaGreen               #FF2E8B57.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.seagreen(printer).aspx
//  SeaShell               #FFFFF5EE.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.seashell(printer).aspx
//  Sienna                 #FFA0522D.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.sienna(printer).aspx
//  Silver                 #FFC0C0C0.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.silver(printer).aspx
//  SkyBlue                #FF87CEEB.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.skyblue(printer).aspx
//  SlateBlue              #FF6A5ACD.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.slateblue(printer).aspx
//  SlateGray              #FF708090.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.slategray(printer).aspx
//  Snow                   #FFFFFAFA.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.snow(printer).aspx
//  SpringGreen            #FF00FF7F.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.springgreen(printer).aspx
//  SteelBlue              #FF4682B4.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.steelblue(printer).aspx
//  Tan                    #FFD2B48C.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.tan(printer).aspx
//  Teal                   #FF008080.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.teal(printer).aspx
//  Thistle                #FFD8BFD8.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.thistle(printer).aspx
//  Tomato                 #FFFF6347.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.tomato(printer).aspx
//  Transparent            ???                                     http://msdn.microsoft.com/en-us/library/system.drawing.color.transparent(printer).aspx
//  Turquoise              #FF40E0D0.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.turquoise(printer).aspx
//  Violet                 #FFEE82EE.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.violet(printer).aspx
//  Wheat                  #FFF5DEB3.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.wheat(printer).aspx
//  White                  #FFFFFFFF.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.white(printer).aspx
//  WhiteSmoke             #FFF5F5F5.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.whitesmoke(printer).aspx
//  Yellow                 #FFFFFF00.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.yellow(printer).aspx
//  YellowGreen            #FF9ACD32.                              http://msdn.microsoft.com/en-us/library/system.drawing.color.yellowgreen(printer).aspx


//  \d Match a decimal digit character
//
//  \D Match a non-digit character
//
//  \w Match a "word" character (alphanumeric plus "_", plus
//     other connector punctuation chars plus
//
//  \W Match a non-"word" character


