########################################################################
#
#  Purpose: Regression test for the main functions in Edit Overflow
#           for web, most importantly the correct built up of edit
#           summary messages, including when looking up terms not
#           in our word list.
#
#           It uses Selenium with "Selenium Webdriver" to control Firefox
#           windows and send information to and from them.
#
#             Reference: <https://en.wikipedia.org/wiki/Selenium_(software)#Selenium_WebDriver>
#
#           It uses the Python module "unittest" to run the tests
#           (which in turn calls/uses Selenium). Thus they are
#           effectively not unit tests, but integration
#           tests...
#
#           By convention of the Python module "unittest", only functions
#           starting with "test" are run. Other functions are helper
#           functions (to reduce the redundancy). Or they are functions
#           whose names are expected by "unittest" (e.g., for setup).
#
# Historical notes:
#
#   'find_element_by_name' was removed in later versions of Selenium.
#   See e.g., <https://stackoverflow.com/questions/72773206>
#
#
########################################################################

import unittest # Home page: <https://docs.python.org/3.8/library/unittest.html>
                #
                # Unconditional failure (e.g., in an if-construct
                # with some complex conditions),
                # <https://docs.python.org/3.8/library/unittest.html#unittest.TestCase.fail>:
                #
                #     fail(msg=None)
                #
                #     Signals a test failure unconditionally, with msg or None for the error message.


import time

from urllib.parse import urlparse

from selenium import webdriver
from selenium.webdriver.common.keys import Keys

# Otherwise we get:
#
#   E0602: Undefined variable 'By' (undefined-variable)
#
from selenium.webdriver.common.by import By

# Otherwise we get:
#
#   E0602: Undefined variable 'NoSuchElementException' (undefined-variable)
#
from selenium.common.exceptions import NoSuchElementException

#Not used (delete?)
#from selenium.webdriver.common.action_chains import ActionChains
#from selenium.webdriver.common.by import By



# Helper function for _core_setFieldAndSubmit() and others.
#
# Submit the current HTML form (by the default action).
#
# This one is independent of Edit Overflow - indicated
# by the prefix "core" (and thus a candidate to be
# moved to a more general place).
#
#Use the "@staticmethod" decorator instead? It seeem out of place here.
#
def _core_SubmitForm(aSomeElement):

    aSomeElement.send_keys(Keys.RETURN) # This is on a text element, but
                                        # it invokes the default action
                                        # (submitting the form).

    # This is crucial: We must wait for the update of the
    # field (server roundtrip time and Firefox update)
    time.sleep(2.0)


class TestMainEditOverflowLookupWeb(unittest.TestCase):


    def setUp(self):
        # Note: It actually takes 4-5 seconds before
        #       the Firefox window appears...
        self.browser = webdriver.Firefox()

        time.sleep(1.0)


    def tearDown(self):

        time.sleep(5.0) # Let us have a look for a while...
        self.browser.close()



    #########################################################################
    #                                                                       #
    #     S t a r t   o f   f u n c t i o n s   i n d e p e n d e n t       #
    #                 o f   E d i t   O v e r f l o w                       #
    #                                                                       #
    #########################################################################


    # Helper function for _core_setFieldAndSubmit() and others.
    #
    # Change a ***single*** text field in the
    # current HTML form.
    #
    # This one is independent of Edit Overflow - indicated
    # by the prefix "core" (and thus a candidate to be
    # moved to a more general place).
    #
    def _core_setField(self, aFieldID, aNewValue):

        #someElement = self.browser.driver.find_element("name", aFieldID)
        someElement = self.browser.find_element("name", aFieldID)
        someElement.clear()
        someElement.send_keys(aNewValue)

        return someElement


    # Helper function for _submitTerm() and others.
    #
    # Change a ***single*** text field in the
    # current HTML form and submit the form
    # (by the default action). If we need
    # to change more than one field, then
    # set them using XXX() before
    # calling this one.
    #
    # This one is independent of Edit Overflow - indicated
    # by the prefix "core" (and thus a candidate to be
    # moved to a more general place).
    #
    def _core_setFieldAndSubmit(self, aFieldID, aNewValue):

        elementRef = self._core_setField(aFieldID, aNewValue)

        #self._core_SubmitForm(elementRef)
        #TestMainEditOverflowLookupWeb
        _core_SubmitForm(elementRef)


    # Helper function for testing. Mostly for _checkEditSummary()
    #
    # It gets and checks the value of a field in an HTML form.
    #
    # This one is independent of Edit Overflow - indicated
    # by the prefix "core" (and thus a candidate to be
    # moved to a more general place).
    #
    def _core_checkFieldValue(self, aFieldID, anExpectedValue, anExplanation):

        # Note: We must also repeat this to avoid this error (we
        #       can not keep using a previous instance of
        #       EditSummaryElement):
        #
        #    "The element reference of <input id="editSummary_output"
        #    class="XYZ4" name="editSummary_output" type="text">
        #    is stale; either the element is no longer attached to
        #    the DOM, it is not in the current frame context, or
        #    the document has been refreshed"
        #
        someElement = self.browser.find_element("name", aFieldID)
        someValue = someElement.get_attribute("value")

        self.assertEqual(
            someValue,
            anExpectedValue,
            f"{anExplanation}. Not equal: {someValue} != {anExpectedValue}"
            )


    # Helper function for testing. Mostly for _setGeneralTextField()
    #
    # This one is independent of Edit Overflow - indicated
    # by the prefix "core" (and thus a candidate to be
    # moved to a more general place).
    #
    #Why do we need this one??? _core_setField() seems
    #almost identical (redundancy).
    #
    def _core_setHTML_textField(self, aFieldID, aText):

        lookUpElement = self.browser.find_element("name", aFieldID)
        lookUpElement.clear()
        lookUpElement.send_keys(aText)


    # Helper function for testing.
    #
    # Purpose:
    #
    #   Hide that Selenium throws an exception when an element does not
    #   exist. There doesn't seem to be a system way to just check
    #   if an element exists or not.
    #
    #   Ref.: <https://stackoverflow.com/a/9587938>. Note that most
    #         of the other answers seem to be plain wrong.
    #
    #   Note that the plural forms, which don't throw
    #   exceptions, have been removed in Selenium.
    #   Ref.: <https://stackoverflow.com/q/30002313/#comment128785684_30025430>.
    #
    # This one is independent of Edit Overflow - indicated
    # by the prefix "core" (and thus a candidate to be
    # moved to a more general place).
    #
    def _core_elementExists(self, aFieldID):

        toReturn = True

        # Rely on an exception being thrown if it doesn't
        # exist. Conversely, there doesn't seem to be a
        # way to avoid the exception.
        #
        # However, is there some kind of time penalty using
        # the try-catch?? Some timeout period of multiple
        # seconds? This seems to indicate it is 3 seconds:
        #
        #   <https://stackoverflow.com/questions/6521270/webdriver-check-if-an-element-exists/8790334#8790334>
        #
        try:

            # Notes: By.ID requires an import, otherwise the result is:
            #
            #          "web_regress.py:271:50: E0602: Undefined
            #          variable 'By' (undefined-variable)"
            #
            #        Other possible values are "By.NAME" and By.XPATH. E.g.:
            #
            #          res3 = self.browser.find_element(By.XPATH, '//*[@id="1002"]')
            #
            self.browser.find_element(By.ID, aFieldID)

        except NoSuchElementException:
            toReturn = False

        return toReturn


    #########################################################################
    #                                                                       #
    #     S t a r t   o f   f u n c t i o n s  f o r                        #
    #                 E d i t   O v e r f l o w                             #
    #                                                                       #
    #########################################################################


    # Helper function for testing.
    #
    # Set a checkbox (not assuming anything about the current state)
    #
    def _setCheckbox(self, anCheckboxName):

        checkboxElement = self.browser.find_element("name", anCheckboxName)

        if not checkboxElement.is_selected():

            # Set the checkbox (by toggling - a present
            # unchecked state is assumed)
            #checkboxElement.click()
            checkboxElement.send_keys(Keys.SPACE)


    # Helper function for testing the main function
    # in Edit Overflow: Looking up words (more
    # generally, "terms"). For _lookUp().
    #
    # Submit a term for Edit Overflow for Web
    #
    def _submitTerm(self, aLookUpTerm):

        self._core_setFieldAndSubmit("LookUpTerm", aLookUpTerm)


    # Helper function for testing. Mostly for _lookUp()
    #
    def _checkEditSummary(self, anExpectedEditSummary, anExplanation):

        self._core_checkFieldValue("editSummary_output", anExpectedEditSummary, anExplanation)


    # Helper function for testing. Mostly for _lookUp()
    #
    def _checkInternalMediaWikiLink(self, anExpectedLink, anExplanation):

        self._core_checkFieldValue("URL6", anExpectedLink, anExplanation)


    # Helper function for testing
    #
    # Submitting a term to Edit Overflow for web and
    # checking that the result is as expected
    #
    # Parameters
    #
    #    anAlternativeLinkText:
    #
    #      The link text from the HTML link that appears if a term
    #      is also in the alternative word. This is entirely
    #      dependent on the current content of the word list.
    #
    #      Example: For correct word "PHP", the link text is
    #               "PHP (tag wiki)" (a correct word in the
    #               alternative word set).
    #
    #    aPartialAlternativeURL
    #
    #      It is not a full URL. For now, it assumes query
    #      parameter "OverflowStyle=Native" is always used.
    #
    #      E.g., only a site relative URL or only part of the
    #      query parameters.
    #
    #      Example:
    #
    #        "LookUpTerm=python_"
    #
    #        ***Not*** the full, e.g.:
    #
    #          "/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=python_"
    #
    #    aWordListReferenceURL
    #
    #      Expected to be a link to the Edit Overflow word in HTML
    #      format, with a page anchor (for direct display for the
    #      word group).
    #
    #      An empty string means the field is expected
    #      to be ***absent*** in the HTML output.
    #
    #      Sample:
    #
    #          https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html#Super_User_(Stack_Exchange_site)
    #
    def _lookUp(self,
                aLookUpTerm,
                anExpectedEditSummary,
                anExplanation,
                anExpectedLink,
                anAlternativeLinkText,
                anAlternativeURL,
                aWordListReferenceURL):

        # Internal consistency test (check for client specification
        # error). If there is a cross reference, then
        # the corresponding (expected) URL must
        # also be specified.
        if anAlternativeLinkText == "":
            self.assertTrue(
                anAlternativeURL == "",
                "Internal error in _lookUp(): Cross URL specified, but the link text is missing.")
        else:
            self.assertTrue(
                anAlternativeURL != "",
                "Internal error in _lookUp(): URL not specified for " +
                ' cross reference "' + anAlternativeLinkText + '".' )

        self._submitTerm(aLookUpTerm)

        # Regression test for the edit summary field. We ought
        # to check the other output fields as well.
        #
        self._checkEditSummary(anExpectedEditSummary, anExplanation)

        # Regression test for the result field "Link (MediaWiki)"
        #
        self._checkInternalMediaWikiLink(anExpectedLink, "Internal MediaWiki link")


        #time.sleep(3.0) # Not really necessary

        # Notes:
        #
        #   1) Selenium functions find_element_by_id(),
        #      find_element_by_name(), find_element(), etc.
        #      throw an exception if the element does not exist
        #      (this is sometimes the case for the Edit Overflow
        #      lookup result page (the existence of that particular
        #      elements depends on the input data)), e.g.:
        #
        #         selenium.common.exceptions.NoSuchElementException:
        #         Message: Unable to locate element: [id="1002"]
        #
        #      There isn't a non-deprecated way
        #      to avoid the exception(?).
        #
        #   2) The plural functions, e.g., findElements_by_id(), that
        #      don't throw exceptions have been removed in later
        #      versions Of Selenium.

        lookupReference = ' for lookup word "' + aLookUpTerm + '".'

        linkText = ""
        alternativeURL = ""

        # 1002 and 1003 are IDs for the two possible HTML <a> elements
        # for cross links to the alternative word set. 0, 1, or 2 may
        # be present on a particular Edit Overflow lookup result page.
        #
        isAlternativeIsPresent1 = self._core_elementExists("1002")
        isAlternativeIsPresent2 = self._core_elementExists("1003")

        if isAlternativeIsPresent1:

            foundElement = self.browser.find_element(By.ID, "1002")
            linkText = foundElement.text

            # Note: This returns the absolute URL (like manually in
            #       the browser), even if the HTML source contains
            #       a relative URL.
            #
            #    Example:
            #
            #      Full URL:
            #
            #        http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=python_
            #
            #      Site relative URL in HTML source:
            #
            #        /world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=python_
            #
            alternativeURL = foundElement.get_attribute("href")


        # For now: Override, if the other exists. Thus we can not
        #          really handle two alternatives at the moment.
        if isAlternativeIsPresent2:

            foundElement = self.browser.find_element(By.ID, "1003")
            linkText = foundElement.text
            alternativeURL = foundElement.get_attribute("href")

        if linkText != "":

            self.assertEqual(linkText,
                             anAlternativeLinkText,
                             "Wrong cross reference text" +
                               lookupReference)

            self.assertEqual(alternativeURL,
                             anAlternativeURL,
                             "Wrong cross reference URL" +
                               lookupReference)

        else:

            self.assertEqual("",
                             anAlternativeLinkText,
                             "Missing expected cross reference to " +
                               "the alternative word set" +
                               lookupReference + ".")

        # Cross link to the word list page. Introduced 2023-01-06
        #
        # Note: Of the four combinations of field present/absent and
        #       specified URL non-empty/empty, we only accept two:
        #
        #         1. Non-empty URL and present field.
        #         2. Empty URL and absent field.
        #
        #       The other two combinations indicate
        #       a (PHP) program error or a (test)
        #       specification error.
        #
        isWordlistReferencePresent = self._core_elementExists("1300")
        if isWordlistReferencePresent and (aWordListReferenceURL != ""):
            # One combination

            foundElement_wordListReference = self.browser.find_element(By.ID, "1300")
            linkText_wordListReference = foundElement_wordListReference.text
            URL_wordListReference = foundElement_wordListReference.get_attribute("href")

            # More sanity checking
            self.assertEqual(linkText_wordListReference,
                             "Words.",
                             "Wrong cross reference text: " +
                               linkText_wordListReference + ".")

            # The real test (of the URL, with page anchor)
            self.assertEqual(URL_wordListReference,
                             aWordListReferenceURL,
                             "Wrong cross reference link for " +
                               linkText_wordListReference + ".")

        else:
            if not isWordlistReferencePresent:
                # Covers two combinations

                # If it isn't present on the page then it should also
                # be indicated in the test specification (empty URL).
                self.assertEqual("",
                                 aWordListReferenceURL,
                                 "The element in absent, but it " +
                                   "is specified to be there.")

            else:
                # The above covers three combinations, thus
                # there is only one (invalid) one left.

                # Or conversly, if it is specified to be empty, then
                # the element should not be present on the page.
                #
                # For this test: All other combinations than
                # the two above are not valid.
                self.fail("The element is present " +
                            "but it is specified to be absent.")

    # =======================================================================


    # Local testing first (using the local web server to
    # test in a non-production environment) - at the
    # very least for practical reasons.
    #
    # Are we actually assured of the order of test execution? No,
    # at least not by position in the file. Alphabetically?


    # That is, general here means the general text field on page "Text.php".
    def _setGeneralTextField(self, aText):

        self._core_setHTML_textField("someText", aText)


    # Helper function for testing the Edit Overflow
    # text transformation page ("Text.php")
    #
    # Invoke a Shift + Alt keyboard shortcut (that are
    # defined for various buttons and text fields
    # in Edit Overflow for web)
    #
    def _webpageKeyboardShort(self, aKey):

        # For now: only works on the Text.php page!!! Should we
        #          pass in the name element? - We don't need it
        #          for now - the other forms only have one
        #          submit button.
        #
        lookUpElement = self.browser.find_element("name", "someText")

        lookUpElement.send_keys(Keys.SHIFT + Keys.ALT + aKey + Keys.SHIFT + Keys.ALT)

        time.sleep(0.5)


    # Helper function for testing the Edit Overflow text transformation
    # page, for a single test for a particular function (check if the
    # input results in the expected output in the text field).
    #
    # "Text" here means the Edit Overflow "Text.php" page. The main
    # edit field is used for both input and output (inline text
    # transformation) and various manipulation can take place, e.g.
    # using a macro keyboard (as keyboard shortcuts are defined for
    # all operations, e.g., Ctrl + Shift for formatting  of YouTube
    # comments (button "Transform for YouTube comments")
    #
    # Note: currently we have the huge overhead of creating a
    #       new browser window for each test
    #
    # Parameters:
    #
    #   anExpectedCharacters:  An integer. The number of characters
    #                          after the text transformation.
    #
    #                          E.g., 9 to match the self-reported number
    #                          of characters by the web application,
    #                          "Now 9 characters (incl. newlines)"
    #
    def _textTransformation(self,
                            aURL,
                            aTextBefore,
                            aTextAfter,
                            anExpectedCharacters,
                            aKeyboardShortcutLetter,
                            anErrorMessage
                            ):

        self.browser.get(aURL)

        self._setGeneralTextField(aTextBefore)

        time.sleep(1.0)

        self._webpageKeyboardShort(aKeyboardShortcutLetter)

        lookUpElement = self.browser.find_element("name", "someText")

        formatterResult = lookUpElement.get_attribute("value")

        self.assertEqual(formatterResult, aTextAfter, anErrorMessage)

        # For now we allow a negative value to mean it should not
        # be checked. But eventually we should require it.
        #
        if anExpectedCharacters >= 0:

            # E.g., containing "Now 9 characters (incl. newlines)".
            msg2Element = self.browser.find_element("name", "Message2")

            #msg2 = msg2Element.get_value()
            msg2 = msg2Element.text

            self.assertEqual(
                msg2,
                "Now " + str(anExpectedCharacters) + " characters (incl. newlines).",
                anErrorMessage)

        time.sleep(3.0) # Only for manual inspection of the
                        # result. Can be removed at any time


    # Text.php page: Test a single enclosing text in real quotes
    #
    # Note: The specific function / button is by the keyboard
    #       shortcut Shift + Alt + Q (the fourth parameter
    #       to _textTransformation(), "q")
    #
    # Parameters:
    #
    #   anExpectedCharacters:  An integer. The number of characters after
    #                          the text transformation.
    #
    #                          E.g., 9 to match the self-reported number
    #                          of characters by the web application,
    #                          "Now 9 characters (incl. newlines)"
    #
    def _checkRealQuotes_single(self,
        aURL,
        aTextBefore,
        aTextAfter,
        anExpectedCharacters):

        # Send Shift + Alt + Q for invoking the formatting
        self._textTransformation(aURL,
                                 aTextBefore,
                                 aTextAfter,
                                 anExpectedCharacters,
                                 "q",
                                 "The real quotes formatting result was bad!")


    # Text.php page: Several tests for enclosing text in real quotes
    #
    def _checkRealQuotes(self, aURL):

        # Was there a bug???
        #
        # 12 # For regression testing. Not necessarily the correct number!
        #
        self._checkRealQuotes_single(aURL, "secure", "“secure”", 12)

        # For bug discovered 2022-02 where the ***reported*** count
        # for some strings were longer than expected. E.g., "<"
        # contributing as if it was HTML thingamajiggied
        # as "&lt;" (HTML character entity reference), 4
        # instead of 1.
        #
        # Note that the actual text transformation, at least as
        # seen by this Selenium script, was as expected
        #
        self._checkRealQuotes_single(aURL, "secur<e", "“secur<e”", 13)


    # Text.php page: Test formatting of YouTube comments
    #
    # Note: The specific function / button (for YouTube comments) is
    #       by the keyboard shortcut Shift + Alt + Y (the fourth
    #       parameter to _textTransformation(), "y")
    #
    def _checkYouTubeFormatting(self, aURL):

        content1_in1 =  ("04 min 17 secs:  Real start of pre Q&A\n"
                             "\n"
                             "                 Why Stef doesn't make Udemy courses.")

        #content1_out_old1 = ("04:17 :  Real start of pre Q&A\n"
        #                     " \n"                                 # Note: Space on otherwise empty lines
        #                     "                 Why Stef doesn't make Udemy courses.")
        content1_out_new1 = ("04:17 :  Real start of pre Q&A\n"
                             " \n"                                 # Note: Space on otherwise empty lines
                             "              Why Stef doesn't make Udemy courses.")


        # Send Shift + Alt + Y for invoking the YouTube formatter
        self._textTransformation(
            aURL,

            ("https://en.wikipedia.org/wiki/Ad26.COV2.S\n"
             "\n"
             "        Johnson & Johnson (Ad26.COV2.S)"),

            ("en DOT wikipedia DOT org/wiki/Ad26 DOT COV2 DOT S\n"
             " \n"  # Yes, trailing space on empty lines.
             "        Johnson & Johnson (Ad26 DOT COV2 DOT S)"),
             #"     Johnson & Johnson (Ad26 DOT COV2 DOT S)"),       # Old behaviour
             -1,

            "y",
            "The YouTube formatter result was bad!")

        # Send Shift + Alt + Y for invoking the YouTube formatter
        self._textTransformation(aURL,
                                content1_in1,
                                content1_out_new1,
                                -1,

                                "y",
                                "The YouTube formatter result was bad!")


    # Text.php page: Test transformation of Markdown code formatting
    #                in one format (four-space indent) to another
    #                (code fencing, with syntax highlighting
    #                turned of)
    #
    # Note: The specific function / button (for Markdown code formatting)
    #       is by the keyboard shortcut Shift + Alt + M (the fourth
    #       parameter to _textTransformation(), "m")
    #
    def _checkMarkdownCodeFormatting(self, aURL):

        # Note: Send Shift + Alt + M for invoking the Markdown formatter
        # (button "Convert to Markdown codefencing")

        content_in_1 =  ("    ;short_open_tag = On\n"
                         "\n"
                         "    with\n"
                         "\n"
                         "    short_open_tag = On (just remove the ; and restart your Apache server)\n"
                         "")

        content_out_1 = ("```lang-none\n"
                         ";short_open_tag = On\n"
                         "\n"
                         "with\n"
                         "\n"
                         "short_open_tag = On (just remove the ; and restart your Apache server)\n"
                         "```\n"
                         "")
        self._textTransformation(
            aURL, content_in_1, content_out_1, -1,
            "m", "The Markdown formatter result was bad!")

        # Source code input with more than 4 space indent (e.g., when
        # part of a Markdown list) must be formatted correctly.
        #
        # Also test that trailing spaces and TABs are removed. Thus
        # it is also an indirect test of whether removing trailing
        # spaces and TABs works as expected.
        #
        content_in_2 =  ("        warning: ignoring #pragma untiunti\n"
                         " \t \n"
                         "        g++ pragma.cpp -Wall   \t\n"
                         "\n"
                         "")

        content_out_2 = ("    ```lang-none\n"
                         "    warning: ignoring #pragma untiunti\n"
                         "\n"
                         "    g++ pragma.cpp -Wall\n"
                         "\n"
                         "    ```\n"
                         "")
        self._textTransformation(
            aURL, content_in_2, content_out_2, -1,
            "m", "The Markdown formatter result was bad!")

    # Helper function for testing the
    # Edit Overflow link builder page.
    #
    # Notes:
    #
    #    1. We currently only test the output for the
    #       inline Markdown link.
    #
    def _linkBuild(self,
                   aURL,
                   aLinkText,
                   aLink,
                   anExpectedInlineMarkdownLink,
                   anErrorMessage):

        #We could do some sanity checking of aURL (e.g., expected to
        #contain "Link_Builder.php")

        self.browser.get(aURL)

        self._core_setHTML_textField("LinkText", aLinkText)
        self._core_setFieldAndSubmit("URL", aLink)

        self._core_checkFieldValue("inlineMarkdown", anExpectedInlineMarkdownLink, anErrorMessage)

        ####time.sleep(3.0) # Only for manual inspection of the
        ####                # result. Can be removed at any time


    def _checkLinkBuilder(self, aURL):

        # Only one test for now

        self._linkBuild(aURL,
                        "high cohesion",
                        "https://en.wikipedia.org/wiki/Cohesion_(computer_science)",
                        "*[high cohesion](https://en.wikipedia.org/wiki/Cohesion_(computer_science))*",
                        "Unexpected inline markdown link")


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelled words)
    #
    # Parameters:
    #
    #    aURL
    #
    #      A start URL to a Edit Overflow lookup page,
    #      local web server or production, with or
    #      without query parameters.
    #
    #      Thus it implicitly specifies if it runs
    #      on the local web server or production.
    #
    #      Examples:
    #
    #        http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=php
    #
    #        https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=yes&OverflowStyle=Native
    #
    # Notes:
    #
    #   1. Currently we implicitly expect it to
    #      be called with "LookUpTerm=php"....
    #
    #   2. the URL is expected to be one that results in a succesful
    #      lookup (the request word exists in the wordlist)
    #
    #   3. Incorrect words "php", "python", and "until" must exist
    #      in the word list (MySQL/MariaDB database).
    #
    #   4. We make a series of word lookups, including some that
    #      fail and some where the edit summary is set to reset,
    #      and check that the edit summary is as expected at
    #      each step (many subtle errors are possible,
    #      especially with failed word lookups).
    #
    #   5. We currently rely on the implementation to use
    #      "OverflowStyle=Native" and to use site relative
    #      URLs for the ***cross reference*** URLs (though
    #      the first could be considered a bug). Example:
    #
    #         "/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=python_"
    #
    def _mainLookup(self, aURL):
        # Initial page, with a (known) incorrect term ***different***
        # from the ***default*** of 'cpu': 'php'
        #
        self.browser.get(aURL)
        time.sleep(2.0)

        # Part of the test input must be derived for _lookUp() as
        # it doesn't know about URLs (though the lookup result
        # may contain URLs).
        #
        # Example:
        #
        #   http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=
        #
        URLparts = urlparse(aURL)
        site = URLparts.scheme + "://" + URLparts.netloc
        altURLprefix = site + "/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm="

        # Absolute URL (production) for now
        wordList_URLprefix = "https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html#"

        singleLookup_editSummary_PHP = 'Active reading [<https://en.wikipedia.org/wiki/PHP>].'
        singleLookup_editSummary_JavaScript = 'Active reading [<https://en.wikipedia.org/wiki/JavaScript>].'

        firstRealLookup_editSummary = 'Active reading [<https://en.wikipedia.org/wiki/PHP> <https://en.wikipedia.org/wiki/Python_%28programming_language%29>].'
        defaultMsgForEditSummary = 'Unexpected edit summary '

        if True: # Test a normal lookup (implicitly through HTML get). For
                 # now, only regression test for the edit summary field.

            # For the initial page, we expect a non-empty edit summary
            # field (though we actually currently make a lookup
            # through the opening URL)
            #
            self._checkEditSummary(singleLookup_editSummary_PHP,
                                  'Unexpected edit summary after URL GET lookup')

            # First direct lookup with a known incorrect term
            self._lookUp("python",
                         firstRealLookup_editSummary,
                         defaultMsgForEditSummary,
                         "''[[Python_%28programming_language%29|Python]]''",
                         "Python (tag wiki)",
                         #"http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=python_")
                         altURLprefix + "python_",
                         wordList_URLprefix + "Python")

        if True: # Test a failed lookup.

            # The edit summary should be unchanged for a
            # failed lookup. We had a regression that
            # was fixed 2020-11-29.
            #
            self._lookUp("PHP__Z",
                         firstRealLookup_editSummary,
                         'Changed edit summary for a failed Edit Overflow lookup',
                         "",
                         "", "",
                         "") # Though in the browser it shows
                             # as the current page's URL...

        if True: # Lookup ***after a failed*** lookup.

            # Second direct lookup with a known
            # correct term (identity mapping)
            self._lookUp("until",
                         'Active reading [<https://en.wikipedia.org/wiki/PHP> <https://en.wikipedia.org/wiki/Python_%28programming_language%29> <https://en.wiktionary.org/wiki/until#Conjunction>].',
                         defaultMsgForEditSummary + 'for looking up a ***correct*** term',
                         "''[[until#Conjunction|until]]''",
                         "", "",
                         wordList_URLprefix + "until")

        if True: # Test clearing the edit summary state (user controlled
                 # by checkbox "Reset lookup state")
                 #
                 # We had a regression with an empty edit summary...

            #print("Setting reset checkbox...")
            self._setCheckbox("resetState")

            # _lookUp() parameters:
            #
            #     aLookUpTerm,
            #
            #     anExpectedEditSummary,
            #     anExplanation,
            #     anExpectedLink,
            #
            #     anAlternativeLinkText,
            #     anAlternativeURL,
            #     aWordListReferenceURL):

            # Lookup with a reset, with a known term
            #
            self._lookUp("js",

                         singleLookup_editSummary_JavaScript,
                         defaultMsgForEditSummary,
                         "''[[JavaScript|JavaScript]]''",

                         # For the (first) cross reference:

                             #"JavaScript (tag wiki)",
                             #"just", # Due to a 2025-12-05 change in the wordlist
                             "is", # Due to a 2025-12-05 change in the wordlist

                             # The Edit Overflow URL, "". For ***first-level***
                             # cross references, it is the original search
                             # word (first parameter to this function), with
                             # one or more ***underscores appended***.
                             #
                             #altURLprefix + "JavaScript_",
                             #altURLprefix + "js_",
                             altURLprefix + "js_____",

                         #wordList_URLprefix + "JavaScript")
                         wordList_URLprefix + "JavaScript")

        if True: # Do a ***failing lookup*** after a reset - we had a
                 # regression with an edit summary of "Active reading []."
                 # (should be empty (an empty string))

            # Regression 2020-12-01 (now fixed): "Active reading []."
            #
            # The edit summary should be unchanged from the previous
            #
            self._lookUp("PHP__Z",
                         singleLookup_editSummary_JavaScript,
                         defaultMsgForEditSummary,
                         "",
                         "", "",
                         "") # Though in the browser it shows
                             # as the current page's URL...

        if True: # Test clearing the edit summary state and with
                 # an initial lookup of an unknown term

            self._setCheckbox("resetState")
            self._lookUp("PHP__Z", "", defaultMsgForEditSummary,
                         "", "",
                         "",
                         "") # Though in the browser it shows
                             # as the current page's URL...

        if True: #
                 #
                 # Note: The expected edit summary depends on the
                 #       previous test steps...

            # False lookup result 2022-04-09 (now fixed):
            #
            #   "*" stripped in the lookup

            self._lookUp("*nix",
                         "Active reading [<https://en.wikipedia.org/wiki/Unix-like>].",
                         defaultMsgForEditSummary,
                         "''[[Unix-like|Unix-like]]''",
                         "", "",
                         wordList_URLprefix + "Unix-like")

        #This test could be a separate test... It doesn't need to be here.
        if True: # Test an expected cross reference (in
                 # the alternative word set)
                 #
                 # This tests that "&" isn't HTML entity encoded at
                 # the wrong time... (It should be in the rendered
                 # HTML, but not when used for the cross lookup).

            self._setCheckbox("resetState")
            self._lookUp("SU",
                         "Active reading [<https://superuser.com/tour>].",
                         defaultMsgForEditSummary,
                         "''Does not apply''",
                         "superuser",
                         altURLprefix + "Super%26nbsp%3BUser+%28Stack+Exchange+site%29_",
                           # Requires a weird mapping in the word list...
                         wordList_URLprefix + "Super_User_(Stack_Exchange_site)")

                                 #"/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=Super%26nbsp%3BUser+%28Stack+Exchange+site%29_"
                                 #                                                        "Super%26nbsp%3BUser+%28Stack+Exchange+site%29_")

        #This test could be a separate test... It doesn't need to be here.
        if True: # Test an expected cross reference (in
                 # the alternative word set)
                 #
                 # This tests particular expected content of the
                 # word list
                 #
                 # Also a lookup word with a space... Encoded as
                 # "+" in the URL... (why not "%20"???)

            self._setCheckbox("resetState")
            self._lookUp("Microsoft Azure",
                         "Active reading [<https://en.wikipedia.org/wiki/Microsoft_Azure>].",
                         defaultMsgForEditSummary,
                         "''[[Microsoft_Azure|Microsoft Azure]]''",
                         "Azure (tag wiki)",
                         altURLprefix + "Microsoft+Azure_", # "+" is for space in the
                                                            # URL query parameter
                         wordList_URLprefix + "Microsoft_Azure")

        #This test could be a separate test... It doesn't need to be here.
        if True: # Test the new functionality as of 2023-01-02 to
                 # make cross references from the alternative word
                 # to the main word set (we already have it in
                 # the opposite direction).
                 #
                 # This relies on particular content of the
                 # word list ("quite_" and "quiet").

            self._setCheckbox("resetState")
            self._lookUp("quite_",
                         "Active reading [<https://en.wiktionary.org/wiki/quite#Adverb>].",
                         defaultMsgForEditSummary,
                         "''[[quite#Adverb|quite_]]''",
                         "quiet", altURLprefix + "quite",
                         wordList_URLprefix + "quite_")

        #This test could be a separate test... It doesn't need to be here.
        if True: # Test encoding, for "+", for the cross reference URL
                 #
                 # This tests particular expected content of the
                 # word list

            self._setCheckbox("resetState")
            self._lookUp("c++",
                         "Active reading [<https://en.wikipedia.org/wiki/C%2B%2B>].",
                         defaultMsgForEditSummary,
                         "''[[C%2B%2B|C++]]''",
                         "C++ (tag wiki)",
                         altURLprefix + "c%2B%2B_",
                         wordList_URLprefix + "C++")


    # Helper function for testing
    #
    # Test of some of text transformations in the 'text' window.
    #
    # Note: It is not complete yet. E.g., we don't test button
    #       "Remove TABs and trailing whitespace" yet (though
    #       some of underlying functions are indirectly
    #       tested by _checkMarkdownCodeFormatting).
    #
    def _checkText_window(self, aURL):

        # Note: The start state of the Edit Overflow text window before
        #       each of these tests is actually different, depending on
        #       the order here. While the tests of text functions
        #       should be independent of the start state (as we
        #       overwrite the contents of text field in the
        #       tests), whether we discover any state
        #       dependency may depend on the order
        #       here...

        self._checkMarkdownCodeFormatting(aURL)

        self._checkYouTubeFormatting(aURL)

        self._checkRealQuotes(aURL)


    #########################################################################
    #                                                                       #
    #     S t a r t   o f   a c t u a l   t e s t   f u n c t i o n s       #
    #                                                                       #
    #       ( c a l l e d   b y   t h e   t e s t   d r i v e r ).          #
    #                                                                       #
    #                                                                       #
    #   Note: These test function do ***not*** run in the order             #
    #         listed here. They seem to be run in alphabetical              #
    #         order, e.g., "test_local_linkBuilder" runs first.             #
    #                                                                       #
    #         Our naming convention                                         #
    #                                                                       #
    #########################################################################


    # Local webserver: Test of the text window functions, e.g.
    #                  YouTube comment formatting
    #
    def test_local_text(self):

        # Note: HTTPS does not work locally (for now)
        #
        self._checkText_window('http://localhost/world/Text.php?OverflowStyle=Native')

    # Local webserver: Test of the link builder, on the local web server
    #
    def test_local_linkBuilder(self):

        self._checkLinkBuilder('http://localhost/world/Link_Builder.php?OverflowStyle=Native')


    # Local webserver: Form-based (server roundtrip) lookup
    #
    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    def test_local_mainLookup_form(self):

        #self._mainLookup('http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=Ghz')

        # "php"
        self._mainLookup('http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=php')
        #pass



    # Production: Test of the text window functions, e.g., YouTube comment formatting
    #
    def test_production_text(self):

        self._checkText_window('https://pmortensen.eu/world/Text.php')


    # Production: Test of passing parameters through HTML GET (for the
    #             main function of Edit Overflow, looking up a word).
    #
    def test_production_mainLookup_HTTP_GET(self):

        if True: # Start out through HTML GET with an unknown
                 # term (new browser window), "cpu777777".

            self.browser.get('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu777777&OverflowStyle=Native&UseJavaScript=no')
            self._checkEditSummary("",
                                  'Unexpected edit summary after URL GET lookup')
            time.sleep(3.0)

            #time.sleep(5.0)


    # Production: Test of the link builder, on production
    #
    def test_production_linkBuilder(self):

        self._checkLinkBuilder('https://pmortensen.eu/world/Link_Builder.php')


    # Production: Using JavaScript-based (client-side) lookup, test of
    #             the central function of Edit Overflow for web:
    #             Looking up incorrect terms (typically
    #             misspelled words)
    #
    # ***Note***: The JavaScript (client-side) version
    #
    # For now: The skip suppress running one of the tests (it
    #          will not pass until we implement the
    #          cross-platform system -
    #          with single source for C#, PHP, and JavaScript).
    #
    #def test_mainLookup(self):
    @unittest.skip("Skipping test_mainLookup_JavaScript() for now")
    def test_production_mainLookup_JavaScript(self):

        # "php"
        self._mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=yes&OverflowStyle=Native')
        #pass


    # Production: Using form-based (server roundtrip) lookup, test of the
    #             central function of Edit Overflow for web: Looking up
    #             incorrect terms (typically misspelling words)
    #
    # ***Note***: The form-based version
    #
    def test_production_mainLookup_form(self):

        # "php"
        self._mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=no&OverflowStyle=Native')
        #pass


if __name__ == '__main__':
    unittest.main()


# Pylint insists on not ending the file with a newline...
# However, Geany adds one when saving...
