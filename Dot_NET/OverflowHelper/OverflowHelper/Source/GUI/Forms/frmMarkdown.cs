

using System;

//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;
using System.Diagnostics; //For Trace. And its Assert.
using System.Text.RegularExpressions; //For Regex.
using System.Collections.Generic; // For Dictionary 


using OverflowHelper.core;


namespace OverflowHelper.Forms
{


    //****************************************************************************
    //*    <placeholder for header>                                              *
    //****************************************************************************
    public partial class frmMarkdown : Form
    {


        struct URLparseInfoStruct
        {
            public string crossLinkString;

            public int extractOffset; // Derived - for convenience.
        }
        
        
        //For delayed initialisation only.
        private string mTerm2;
        private string mURL2;

        private LinkRef mLinkRefGenerator;


        // The key is a part of a URL (for searching)
        private Dictionary<string, URLparseInfoStruct> mURLparseInfo;


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        public frmMarkdown(string aTerm, string aURL, LinkRef aLinkRefGenerator)
        {
            InitializeComponent();

            mTerm2 = aTerm;
            mURL2 = aURL;

            mLinkRefGenerator = aLinkRefGenerator;
            
            mURLparseInfo = new Dictionary<string, URLparseInfoStruct>(2);
            addURLparseInfo("wikipedia.org/wiki", "wikipedia:");
            addURLparseInfo("wiktionary.org/wiki", "wiktionary:");
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        protected override void OnLoad(EventArgs anEvent)
        {
            base.OnLoad(anEvent);
            
            //This works for setting (start) focus to a particular control and
            //not the one with the lowest tab order...
            //
            //From: <http://stackoverflow.com/questions/1002052/how-to-specify-which-control-should-be-focused-when-a-form-opens/1002168#1002168>
            //
            //  "How to specify which control should be focused when a form opens?"
            //
            this.ActiveControl = txtLinkText;

            //Delayed initialisation (not done in the constructor where it
            //would be too early (?)).
            if (mURL2 != "")
            {
                Trace.Assert(
                    mURL2 != null,
                    "LANG ASSERT. mURL is Nothing...");

                //txtLinkText.Text = mTerm;
                //txtURL.Text = mURL;
                //generate();
                fillInAndGenerate(mTerm2, mURL2, mLinkRefGenerator);
            }

            PM_WindowsForms.PM_WindowsFormsCommon.commonChecks(this);
        }


        //PM_REFACTOR 2011-08-12
        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void fillInAndGenerate(string aLinkText, string aLink,
                                       LinkRef aLinkRefGenerator)
        {
            //Direct manipulation of GUI elements. Is it proper??
            //Direct fill into the GUI. Is this a bad idea?
            txtLinkText.Text = aLinkText;
            txtURL.Text = aLink;

            txtRefNumber.Text = aLinkRefGenerator.GetRef();

            generate();
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void btnOK_Click(object aSender, EventArgs anEvent)
        {
            this.Close();
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void btnCancel_Click(object aSender, EventArgs anEvent)
        {
            this.Close();
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void addURLparseInfo(string aSearchTerm, string aCrossLinkStr)
        {
            URLparseInfoStruct someURLparseInfo;

            someURLparseInfo.crossLinkString = aCrossLinkStr;

            someURLparseInfo.extractOffset = aSearchTerm.Length + 1;

            mURLparseInfo.Add(aSearchTerm, someURLparseInfo);
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void updateOutputs()
        {
            // Reading off settings in the GUI.
            string linkText2 = txtLinkText.Text;
            string URL2 = txtURL.Text;
            string refNum = txtRefNumber.Text;
            bool doEmphasise = chkEmphasise.Checked;

            // Filtering
            string effectiveLinkText = linkText2.Trim(); //Removes leading AND 
            // trailing whitespace.
            string effectiveURL = URL2.Trim(); //Removes leading AND 
            // trailing whitespace.


            string linkTextBracketed = "[" + effectiveLinkText + "]";
            string refNumBracketed = "[" + refNum + "]";

            string shortMarkdownText = linkTextBracketed + refNumBracketed;
            string shortMarkdownRef = "  " + refNumBracketed + ": " + effectiveURL;
            string inlineMarkdown = linkTextBracketed + "(" + effectiveURL + ")";
            string HTMLlink = 
                "<a href=\"" + effectiveURL + "\">" + effectiveLinkText + "</a>";

            // Need for refactoring here...
            string link_MediaWiki;
            string linkText_MediaWiki;


            // int idxWikipedia = URL.IndexOf("wikipedia.org/wiki");
            // bool isWikipediaLink = idxWikipedia > 0;
            // 
            // int idxWiktionary = URL.IndexOf("wiktionary.org/wiki");
            // bool isWiktionaryLink = idxWiktionary > 0;


            int idx = 0; // We use the value as a flag. 0 if it is 
                         // not a Wikipedia, Wiktionary, etc. URL.

            //string crossSpecifier = "";

            Dictionary<string, URLparseInfoStruct>.Enumerator hashEnumerator2 =
              mURLparseInfo.GetEnumerator();
            URLparseInfoStruct siteInfo;
            siteInfo.extractOffset = -1; //Keep the compiler happy.
            siteInfo.crossLinkString = ""; //Keep the compiler happy.
            while (hashEnumerator2.MoveNext())
            {
                string curKey = hashEnumerator2.Current.Key;
                siteInfo = hashEnumerator2.Current.Value;

                idx = effectiveURL.IndexOf(curKey);
                if (idx >= 0)
                {
                    break;
                }
            }

            // XXXXXXX


            {
                link_MediaWiki = "";

                //int idx = 0;
                //if (isWikipediaLink)
                //{
                //    crossSpecifier = "wikipedia:";
                //    idx = idxWikipedia;
                //}
                //if (isWiktionaryLink)
                //{
                //    crossSpecifier = "wiktionary:";
                //    idx = idxWiktionary;
                //}


                //Extract for the Wikipedia/Wiktionary link.
                if (idx > 0)
                {
                    //Later: some sanity checks. E.g. containing "http".
                    idx += siteInfo.extractOffset;
                    int newLen = effectiveURL.Length - idx;
                    link_MediaWiki = effectiveURL.Substring(idx, newLen);

                    //Substitutions, only for space now. What else should
                    //be substituted?
                    //
                    //  1. ".27" to "'"  (single quote)  (?)
                    //
                    link_MediaWiki = link_MediaWiki.Replace("_", " ");

                    //Future: make link for use cross-wiki, e.g. from Wikibooks
                    //and Wiktionary.

                    if (txtLinkText.Text.Length == 0)
                    {
                        //The link text field is empty. This is in fact the normal 
                        //use case when working with Wikipedia links.
                        //
                        //Even if the final link text is going to be
                        //different, it is still the normal use case as
                        //the link will be changed somewhere else, outside
                        //of this tool, e.g. when editing a Wikipedia page
                        //using a web browser.

                        //Derive the link text from the URL (but also see below).
                        linkText_MediaWiki = link_MediaWiki; //Later: only take
                        //                           the text after "#" (if any,
                        //                           otherwise use the full text).

                        //Back fill the link text into the input field that 
                        //we derived from the URL to indicate that this is 
                        //the one that is used for the link text...
                        txtLinkText.Text = linkText_MediaWiki;
                    }
                    else
                    {
                        // If the link text field in the dialog is not empty,
                        // we use that instead (but does that violate some 
                        // GUI principle?).
                        //
                        //This represents the user having chosen another
                        //link text than is encoded in the URL.
                        linkText_MediaWiki = txtLinkText.Text;
                    }
                }
                else
                {
                    link_MediaWiki = effectiveURL;

                    // Not found - it is not a Wikipedia link. But it doesn't need 
                    // to be - it could be an ***external*** link that is used
                    // on a Wikipedia page - we use the link text instead (if there 
                    // is any)
                    if (effectiveLinkText.Length > 0)
                    {
                        linkText_MediaWiki = effectiveLinkText; //Note that 
                        //  this is probably meaningless for the 
                        //  GUI field "MediaWiki" as there is probably 
                        //  not a page on Wikipedia with the title 
                        //  of the content of GUI field "Link text"...
                        //
                        //  On the other hand, it makes good sense for
                        //  GUI field "MediaWiki, external".
                    }
                    else
                    {
                        //What to do? For now, just
                        //use the raw URL instead.
                        linkText_MediaWiki = effectiveURL;
                    }
                }
            }

            string MediaWikiLink =
                "[[" + link_MediaWiki + "|" +
                linkText_MediaWiki + "]]";

            string MediaWikiLink_External =
                "[" + effectiveURL + " " + linkText_MediaWiki + "]";

            // Sample: [[wiktionary:compatability|compatability]]
            string MediaWikiLink_cross =
                "[[" + siteInfo.crossLinkString + link_MediaWiki + "|" +
                linkText_MediaWiki + "]]";

            if (doEmphasise)
            {
                shortMarkdownText = "*" + shortMarkdownText + "*";
                inlineMarkdown = "*" + inlineMarkdown + "*";
                HTMLlink = "<em>" + HTMLlink + "</em>";
                MediaWikiLink = "''" + MediaWikiLink + "''";
                MediaWikiLink_External = "''" + MediaWikiLink_External + "''";
            }

            txtShortMarkdown_text.Text = shortMarkdownText;
            txtShortMarkdown_reference.Text = shortMarkdownRef + "\n";

            //txtInlineMarkdown.Text = inlineMarkdown;
            txtInlineMarkdown.Text = inlineMarkdown;

            txtHTML.Text = HTMLlink;

            txtMediaWikiLink.Text = MediaWikiLink;

            txtMediaWikiLink_External.Text = MediaWikiLink_External;

            txtWikipediaFamilyCrosslink.Text = MediaWikiLink_cross;
            
        } //updateOutputs()


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void txtLinkText_TextChanged(object aSender, EventArgs anEvent)
        {
            //updateOutputs();
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void txtURL_TextChanged(object aSender, EventArgs anEvent)
        {
            //updateOutputs();
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void generate()
        {
            updateOutputs();

            string toClipboard = "";

            if (rbnClipboardDontChange.Checked)
            {
                toClipboard = "";
            }

            //Short inline form. Example:
            //
            //
            //
            if (rbnClipboardShort.Checked)
            {
                toClipboard = txtShortMarkdown_text.Text;
                this.ActiveControl = txtShortMarkdown_reference; //Note: not the
                //  same as where the clipboard content comes from. This is to
                //  get ready for copying the reference section (by the user).
            }

            if (rbnClipboardInline.Checked)
            {
                toClipboard = txtInlineMarkdown.Text;
                this.ActiveControl = txtInlineMarkdown; //Change focus, to be
                //  consistent with the behaviour for the short form (above).
            }

            if (rbnClipboardHTML.Checked)
            {
                toClipboard = txtHTML.Text;
                this.ActiveControl = txtHTML; //Change focus, to be
                //  consistent with the behaviour for the short form (above).
            }

            if (toClipboard != "")
            {
                ////toClipboard = "\n" + toClipboard + "\n";
                //System.Windows.Forms.Clipboard.SetText(toClipboard);
                
                EditorOverflowApplication.setClipboard3(toClipboard, null);

                //Later: add clipboard retry info to GUI (e.g. by reusing 
                //       something.
            }
        } //generate()


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void btnGenerate_Click(object aSender, EventArgs anEvent)
        {
            generate();

            //Prepare ref ID for the next time...
            txtRefNumber.Text = mLinkRefGenerator.GetRef();

        } //btnGenerate_Click()


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void mnuExit_Click(object aSender, EventArgs anEvent)
        {
            this.Close();
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void mnuImportInlineFormat_Click(object aSender, EventArgs anEvent)
        {
            //Sample: "[shouldn't](http://stackoverflow.com/faq#dontask)"
            string inlineStr = System.Windows.Forms.Clipboard.GetText();

            string linkText = Regex.Match(inlineStr, @"\[([^\]]*)\]").Groups[1].Value;
            string link = Regex.Match(inlineStr, @"\(([^\)]*)\)").Groups[1].Value;

            //txtLinkText.Text = linkText;
            //txtURL.Text = link;
            //generate();
            fillInAndGenerate(linkText, link, mLinkRefGenerator);
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void mnuSelectLinkTextField_Click(object sender, EventArgs e)
        {
            this.ActiveControl = txtLinkText;
        }


        //****************************************************************************
        //*    <placeholder for header>                                              *
        //****************************************************************************
        private void mnuSelectURLField_Click(object aSender, EventArgs anEvent)
        {
            this.ActiveControl = txtURL;
        }


    } //class frmMarkdown


} //namespace OverflowHelper.Forms

