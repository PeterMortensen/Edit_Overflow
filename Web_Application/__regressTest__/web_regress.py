########################################################################
#
#  Purpose: Regression test for the main functions in Edit Overflow
#           for web, most importantly the correct built up of edit
#           summary messages, including looking up terms not in
#           our word list.
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
#           whose names are expected by "unittest" (e.g. for setup).
#
########################################################################

import unittest

import time

from selenium import webdriver
from selenium.webdriver.common.keys import Keys

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

    #def test_upper(self):
    #    self.assertEqual('foo'.upper(), 'FOO')


    # This one is independent of Edit Overflow - indicated
    # by the prefix "core" (and thus a candidate to be
    # moved to a more general place).



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

        someElement = self.browser.find_element_by_name(aFieldID)
        someElement.clear()
        someElement.send_keys(aNewValue)

        return someElement


    # Helper function for _submitTerm() and others.
    #
    # Change a ***single*** text field in the
    # current HTML form and submit the form
    # (by the default action). If we need
    # to change more than one field, then
    # set them using XXXX() before
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
        someElement = self.browser.find_element_by_name(aFieldID)
        someValue = someElement.get_attribute("value")

        self.assertEqual(someValue, anExpectedValue, anExplanation)


    # Helper function for testing. Mostly for _setGeneralTextField()
    #
    # This one is independent of Edit Overflow - indicated
    # by the prefix "core" (and thus a candidate to be
    # moved to a more general place).
    #
    def _core_setHTML_textField(self, aFieldID, aText):

        lookUpElement = self.browser.find_element_by_name(aFieldID)
        lookUpElement.clear()
        lookUpElement.send_keys(aText)


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

        checkboxElement = self.browser.find_element_by_name(anCheckboxName)

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
    def _checkEditSummary(self, aExpectedEditSummary, anExplanation):

        self._core_checkFieldValue("editSummary_output", aExpectedEditSummary, anExplanation)


    # Helper function for testing
    #
    # Submitting a term to Edit Overflow for web and
    # checking that the result is as expected
    #
    def _lookUp(self, aLookUpTerm, aExpectedEditSummary, anExplanation):

        self._submitTerm(aLookUpTerm)

        # For now, only regression test for the edit summary
        # field. We ought to check the other output fields
        # as well.
        #
        self._checkEditSummary(aExpectedEditSummary, anExplanation)

        #time.sleep(3.0) # Not really necessary


    # =======================================================================


    # Local testing first (using the local web server to
    # test in a non-production environment) - at the
    # very least for practical reasons.
    #
    # Are we actually assured of the order of test execution? No, at least
    # not by position in the file. Alphabetically?
    #


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
        lookUpElement = self.browser.find_element_by_name("someText")

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
    # all operations, e.g. Ctrl + Shift for formatting  of YouTube
    # comments (button "Transform for YouTube comments")
    #
    # Note: currently we have the huge overhead of creating a
    #       new browser window for each test
    #
    def _textTransformation(self,
                           aURL,
                           aTextBefore,
                           aTextAfter,
                           aKeyboardShortcutLetter,
                           anErrorMessage):

        self.browser.get(aURL)

        self._setGeneralTextField(aTextBefore)

        time.sleep(1.0)

        self._webpageKeyboardShort(aKeyboardShortcutLetter)

        lookUpElement = self.browser.find_element_by_name("someText")

        formatterResult = lookUpElement.get_attribute("value")

        self.assertEqual(formatterResult, aTextAfter, anErrorMessage)

        time.sleep(3.0) # Only for manual inspection of the
                        # result. Can be removed at any time


    # Text.php page: Test enclosing text in real quotes
    #
    # Note: The specific function / button (for YouTube comments) is
    #       by the keyboard shortcut Shift + Alt + Q (the fourth
    #       parameter to _textTransformation(), "q")
    #
    #
    def _checkRealQuotes(self, aURL):

        content1_in1 = "secure"
        #content1_out_new1 = "“" # Adaptation to the current bug
        content1_out_new1 = "“secure”" # Adaptation to the current bug

        # Send Shift + Alt + Q for invoking the formatting
        self._textTransformation(aURL,
                                content1_in1,
                                content1_out_new1,
                                "q",
                                "The real quotes formatting result was bad!")


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

            "y",
            "The YouTube formatter result was bad!")

        # Send Shift + Alt + Y for invoking the YouTube formatter
        self._textTransformation(aURL,
                                content1_in1,
                                content1_out_new1,
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
            aURL, content_in_1, content_out_1,
            "m", "The Markdown formatter result was bad!")

        # Source code input with more than 4 space indent (e.g., when
        # part of a Markdown list) must be formatted correctly.
        #
        content_in_2 =  ("        warning: ignoring #pragma untiunti\n"
                         "\n"
                         "        g++ pragma.cpp -Wall\n"
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
            aURL, content_in_2, content_out_2,
            "m", "The Markdown formatter result was bad!")

    #Only a stub now. It is not used anywhere
    # Helper function for testing the Edit Overflow link builder
    # page.
    #
    # Notes:
    #
    #    1. We currently only test the output for the
    #       inline Markdown link.
    #
    #    2. XXXX
    #
    def _linkBuild(self,
                   aURL,
                   aLinkText,
                   aLink,
                   anExpectedInlineMarkdownLink,
                   anErrorMessage):

        #We could do some sanity checking of aURL (e.g. expected to
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
    #   4. We make a series of word lookups, including some that fail
    #      and some where the edit summary is set to reset,
    #      and check that the edti summary is as expected at each
    #      step (many subtle errors are possible, especially
    #      with failed word lookups).
    #
    def _mainLookup(self, aURL):
        # Initial page, with a (known) incorrect term ***different***
        # from the ***default*** of 'cpu': 'php'
        #
        self.browser.get(aURL)
        time.sleep(2.0)

        singleLookup_editSummary_PHP = 'Active reading [<https://en.wikipedia.org/wiki/PHP>].'

        firstRealLookup_editSummary = 'Active reading [<https://en.wikipedia.org/wiki/PHP> <https://en.wikipedia.org/wiki/Python_%28programming_language%29>].'
        defaultMsgForEditSummary = 'Unexpected edit summary '

        if True: # Test a normal lookup (implicitly through HTML get). For
                 # now only regression test for the edit summary field.

            # For the initial page, we expect a non-empty edit summary
            # field (though we actually currently make a lookup
            # through the opening URL)
            #
            self._checkEditSummary(singleLookup_editSummary_PHP,
                                  'Unexpected edit summary after URL GET lookup')

            # First direct lookup with a known incorrect term
            self._lookUp("python", firstRealLookup_editSummary, defaultMsgForEditSummary)

        if True: # Test a failed lookup.

            # The edit summary should be unchanged for a
            # failed lookup. We had a regression that
            # was fixed 2020-11-29.
            #
            self._lookUp("PHP__Z",
                        firstRealLookup_editSummary,
                        'Changed edit summary for a failed Edit Overflow lookup')

        if True: # Lookup ***after a failed*** lookup.

            # Second direct lookup with a known
            # correct term (identity mapping)
            self._lookUp("until",
                        'Active reading [<https://en.wikipedia.org/wiki/PHP> <https://en.wikipedia.org/wiki/Python_%28programming_language%29> <https://en.wiktionary.org/wiki/until#Conjunction>].',
                        defaultMsgForEditSummary + 'for looking up a ***correct*** term')

        if True: # Test clearing the edit summary state (user controlled
                 # by checkbox "Reset lookup state")
                 #
                 # We had a regression with an empty edit summary...

            #print("Setting reset checkbox...")
            self._setCheckbox("resetState")

            # Lookup with a reset, with a known term
            #
            self._lookUp("php", singleLookup_editSummary_PHP, defaultMsgForEditSummary)


        if True: # Do a ***failing lookup*** after a reset - we had a
                 # regression with an edit summary of "Active reading []."
                 # (should be empty (an empty string))

            # Regression 2020-12-01 (now fixed): "Active reading []."
            #
            # The edit summary should be unchanged from the previous
            #
            self._lookUp("PHP__Z", singleLookup_editSummary_PHP, defaultMsgForEditSummary)


        if True: # Test clearing the edit summary state and with
                 # an initial lookup of an unknown term

            self._setCheckbox("resetState")
            self._lookUp("PHP__Z", "", defaultMsgForEditSummary)


    # Helper function for testing
    #
    # Test of text transformations in the 'text' window
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
    #########################################################################


    # Test of the text window functions, e.g. YouTube comment formatting
    #
    def test_local_text(self):

        # Note: HTTPS does not work locally (for now)
        #
        self._checkText_window('http://localhost/world/Text.php?OverflowStyle=Native')


    # Test of the text window functions, e.g., YouTube comment formatting
    #
    def test_text(self):

        self._checkText_window('https://pmortensen.eu/world/Text.php')


    # Test of passing parameters through HTML GET (for the
    # main function of Edit Overflow, looking up a word).
    #
    def test_mainLookup_HTTP_GET(self):

        if True: # Start out through HTML GET with an unknown
                 # term (new browser window), "cpu777777".

            self.browser.get('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu777777&OverflowStyle=Native&UseJavaScript=no')
            self._checkEditSummary("",
                                  'Unexpected edit summary after URL GET lookup')
            time.sleep(3.0)

            #time.sleep(5.0)


    # Test of the link builder, on the local web server
    #
    def test_local_linkBuilder(self):

        self._checkLinkBuilder('http://localhost/world/Link_Builder.php?OverflowStyle=Native')


    # Test of the link builder, on production
    #
    def test_linkBuilder(self):

        self._checkLinkBuilder('https://pmortensen.eu/world/Link_Builder.php')


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelled words)
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
    def test_mainLookup_JavaScript(self):

        # "php"
        self._mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=yes&OverflowStyle=Native')
        #pass


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    # ***Note***: The form-based (server roundtrip) version
    #
    #def test_mainLookup(self):
    def test_mainLookup_form(self):

        # "php"
        self._mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=no&OverflowStyle=Native')
        #pass


    # Form-based lookup, using a local web server
    #
    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    #def test_mainLookup(self):
    def test_mainLookup_form_localWebserver(self):

        #self._mainLookup('http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=Ghz')

        # "php"
        self._mainLookup('http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=php')
        #pass


if __name__ == '__main__':
    unittest.main()


# Pylint insists on not ending the file with a newline...
# However, Geany adds one when saving...
