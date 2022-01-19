/****************************************************************************
 * Copyright (C) 2019 Peter Mortensen                                       *
 * This file is part of OverflowHelper.                                     *
 *                                                                          *
 *                                                                          *
 * Purpose: Builds HTML, including automatic indentation and                *
 *          standard boilerplate HTML (e.g., the beginning                  *
 *          of an HTML document). This prevents a lot of                    *
 *          redundancy on the client side.                                  *
 *                                                                          *
 ****************************************************************************/


using System.Text; //For StringBuilder.


/****************************************************************************
 *    <placeholder for header>                                              *
 ****************************************************************************/
namespace OverflowHelper.core
{


    /****************************************************************************
     *    <placeholder for header>                                              *
     ****************************************************************************/
    public class HTML_builder
    {
        const int kSpacesPerIndentLevel = 4;


        StringBuilder mScratchSB;
        StringBuilder mHTMLcontentSB;

        int mIndents; // Actual number of spaces

        string mSpaces; // Cached, direct function of mIndents.


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        public HTML_builder()
        {
            mScratchSB = new StringBuilder(32);

            mHTMLcontentSB = new StringBuilder(1200000);
            mIndents = 0; //Explicit

            changeIndents(0);
        }


        /****************************************************************************
         *    <placeholder for header>                                              *
         ****************************************************************************/
        private void changeIndents(int aChange)
        {
            mIndents += aChange;

            mScratchSB.Length = 0;
            for (int i = 0; i < mIndents; i++)
			{
                mScratchSB.Append(" ");
			}
            mSpaces = mScratchSB.ToString();
        }


        /****************************************************************************
         *                                                                          *
         *    Adds the provided raw HTML content                                    *
         *                                                                          *
         *    mHTMLcontentSB should only be changed in this function.               *
         *                                                                          *
         ****************************************************************************/
        private void addContentRaw(string aContent)
        {
            mHTMLcontentSB.Append(aContent);
        }


        /****************************************************************************
         *                                                                          *
         *    Adds an (internal) empty line in the HTML content.                    *
         *                                                                          *
         ****************************************************************************/
        public void addEmptyLine()
        {
            addContentRaw("\n");
        }


        /****************************************************************************
         *                                                                          *
         *    Adds the provided HTML content - the current indentation              *
         *    level (in the HTML source) is handled automatically.                  *
         *                                                                          *
         ****************************************************************************/
        public void addContent(string aContent)
        {
            addContentRaw(mSpaces);
            addContentRaw(aContent);
        }


        /****************************************************************************
         *                                                                          *
         *  As addContent(), but with a newline after to                            *
         *  separate it from the following HTML content.                            *
         *                                                                          *
         ****************************************************************************/
        public void addContentOnSeparateLine(string aContent)
        {
            addContent(aContent);
            addContentRaw("\n");
        }


        /****************************************************************************
         *                                                                          *
         *  As addContentOnSeparateLine(), but with a newline before to             *
         *  separate it by an empty line in the HTML content from the               *
         *  previous content (if the previous content added a newline).             *
         *                                                                          *
         ****************************************************************************/
        public void addContentWithEmptyLine(string aContent)
        {
            addContentRaw("\n");
            addContentOnSeparateLine(aContent);
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public void indentLevelUp()
        {
            changeIndents(kSpacesPerIndentLevel);
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public void indentLevelDown()
        {
            changeIndents(-kSpacesPerIndentLevel);
        }


        /****************************************************************************
         *                                                                          *
         * anAttrString: Unstructured text with one or more attributes for the      *
         *               HTML tag - the client is responsible for formatting        *
         *               it properly, including a leading space (an empty           *
         *               string results in output with no spaces around             *
         *               the starting tag)                                          *
         *                                                                          *
         ****************************************************************************/
        public static string singleLineTagStrWithAttr(
            string aTagName, string aTagContentText, string anAttrString)
        {
            // Example output: <div id="IndianSpace"></div>
            // 
            return "<" + aTagName + anAttrString + ">" + aTagContentText + 
                   "</" + aTagName + ">";
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public static string singleLineTagStr(string aTagName, string aTagContentText)
        {
            return singleLineTagStrWithAttr(aTagName, aTagContentText, "");
        }


        /****************************************************************************
         *                                                                          *
         *  Note: Will insert an empty line and make the content indent             *
         *        one level further than the current indent level (but              *
         *        the current indent level will not be changed).                    *
         *                                                                          *
         ****************************************************************************/
        public string smallTextItemWithSpacing(string aName, string aContent)
        {
            indentLevelUp();
            string prefix = "\n" + mSpaces;
            indentLevelDown();

            return prefix +
                   aName + ": " + aContent + "&nbsp;&nbsp;";
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public void singleLineTagOnSeparateLine(
            string aTagName, string aTagContentText)
        {
            addContentOnSeparateLine(singleLineTagStr(aTagName, aTagContentText));
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public void singleLineTagWithEmptyLine(
            string aTagName, string aTagContentText)
        {
            addContentWithEmptyLine(singleLineTagStr(aTagName, aTagContentText));
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public void addHeader(int aHeaderLevel, string aText)
        {
            addContentRaw("\n");
            singleLineTagWithEmptyLine("h" + aHeaderLevel.ToString(), aText);
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public void startTagWithEmptyLine(string aTagName)
        {
            addContentWithEmptyLine("<" + aTagName + ">");
            indentLevelUp();
        }


        /****************************************************************************
         *                                                                          *
         ****************************************************************************/
        public void endTagOneSeparateLine(string aTagName)
        {
            indentLevelDown();
            addContentOnSeparateLine("</" + aTagName + ">");
        }


        /****************************************************************************
         *
         *  With an empty line before
         *
         ****************************************************************************/
        public void addParagraph(string aText)
        {
            singleLineTagWithEmptyLine("p", aText);
        }


        /****************************************************************************
         *                                                                          *
         *  Adds all the start of standard HTML document, including                 *
         *  the start <head> tag and a title.                                       *
         *                                                                          *
         ****************************************************************************/
        public void startHTML(string aTitle)
        {
            addContentOnSeparateLine("<!DOCTYPE html>");

            // This also works, even though it also has attributes...
            addContentWithEmptyLine("<html lang=\"en\">");
            indentLevelUp();

            startTagWithEmptyLine("head");

            //To make the special characters at the end (e.g. for arrow) actually
            //work on the resulting web opened in the browser
            addContentOnSeparateLine(
                "<meta http-equiv=\"Content-Type\" " +
                "content=\"text/html; charset=UTF-8\">");

            //Title
            singleLineTagWithEmptyLine("title", aTitle);
        }


        /****************************************************************************
         *                                                                          *
         *  Returns the current HTML content. The side effect is that               *
         *  the current HTML is cleared.                                            *
         *                                                                          *
         *  But note that the ***current indentation level*** is maintained -       * 
         *  the reason being we want to be able to insert HTML content              * 
         *  generated somewhat independently).                                      *
         *                                                                          *
         ****************************************************************************/
        public string currentHTML()
        {
            string toReturn = mHTMLcontentSB.ToString();
            mHTMLcontentSB.Length = 0;
            return toReturn;
        }


        /****************************************************************************
         *
         ****************************************************************************/
        public void addComment(string aText)
        {
            addContent("<!-- ");
            addContentRaw(aText);
            addContentRaw(" -->");
            addContentRaw("\n");
        }


    } //class HTML_builder


}


