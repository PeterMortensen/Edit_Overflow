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


    # Helper function for testing.
    #
    # Set a checkbox (not assuming anything about the current state)
    #
    def setCheckbox(self, anCheckboxName):

        checkboxElement = self.browser.find_element_by_name(anCheckboxName)

        if not checkboxElement.is_selected():

            # Set the checkbox (by toggling - a present
            # unchecked state is assumed)
            #checkboxElement.click()
            checkboxElement.send_keys(Keys.SPACE)


    # Helper function for testing. For lookUp()
    #
    # Submit a term for Edit Overflow for Web
    #
    def submitTerm(self, aLookUpTerm):
        lookUpElement = self.browser.find_element_by_name("LookUpTerm")
        lookUpElement.clear()
        lookUpElement.send_keys(aLookUpTerm)
        lookUpElement.send_keys(Keys.RETURN)

        # This is crucial: We must wait for the update of the
        # field (server roundtrip time and Firefox update)
        time.sleep(2.0)


    # Helper function for testing. For mostly for lookUp()
    #
    def checkEditSummary(self, aExpectedEditSummary, anExplanation):

        # We must also repeat this to avoid this error (we can not keep
        # using a previous instance of EditSummaryElement):
        #
        #    "The element reference of <input id="editSummary_output"
        #    class="XYZ4" name="editSummary_output" type="text">
        #    is stale; either the element is no longer attached to
        #    the DOM, it is not in the current frame context, or
        #    the document has been refreshed"
        #
        EditSummaryElement = self.browser.find_element_by_name("editSummary_output")
        EditSummary2 = EditSummaryElement.get_attribute("value")

        self.assertEqual(EditSummary2, aExpectedEditSummary, anExplanation)


    # Helper function for testing
    #
    # Submitting a term to Edit Overflow for web and
    # checking that the result is as expected
    #
    def lookUp(self, aLookUpTerm, aExpectedEditSummary, anExplanation):

        self.submitTerm(aLookUpTerm)

        # For now only regression test for
        # the edit summary field.
        self.checkEditSummary(aExpectedEditSummary, anExplanation)

        #time.sleep(3.0) # Not really necessary

    # Helper function for testing
    #
    # Test of text transformations in the 'text' window
    #
    def checkText_window(self, aURL):

        # Note: The start state of the Edit Overflow text window before
        #       each of these tests is actually different, depending on
        #       the order here. While the tests of text functions
        #       should be independent of the start state (as we
        #       overwrite the contents of text field in the
        #       tests), whether we discover any state
        #       dependency may depend on the order
        #       here...

        self.checkMarkdownCodeFormatting(aURL)

        self.checkYouTubeFormatting(aURL)

        self.checkRealQuotes(aURL)


    # =======================================================================


    # Local testing first (using the local web server to
    # test in a non-production environment) - at the
    # very least for practical reasons.
    #
    # Are we actually assured of the order of test execution? No, at least
    # not by position in the file. Alphabetically?
    #

    # Test of the text window functions, e.g. YouTube comment formatting
    #
    def test_local_text(self):

        # Note: HTTPS does not work locally (for now)
        #
        self.checkText_window('http://localhost/world/Text.php?OverflowStyle=Native')


    # Test of the text window functions, e.g. YouTube comment formatting
    #
    def test_text(self):

        self.checkText_window('https://pmortensen.eu/world/Text.php')


    # Test of passing parameters through HTML GET
    #
    def test_mainLookup_HTTP_GET(self):

        if True: # Start out through HTML GET with an unknown term (new browser window)

            self.browser.get('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu777777&OverflowStyle=Native&UseJavaScript=no')
            self.checkEditSummary("",
                                  'Unexpected edit summary after URL GET lookup')
            time.sleep(3.0)

            #time.sleep(5.0)

    # That is, general here means the general text field on page Text.php.
    def setGeneralTextField(self, aText):

        lookUpElement = self.browser.find_element_by_name("someText")
        lookUpElement.clear()
        lookUpElement.send_keys(aText)

        #lookUpElement.send_keys(aLookUpTerm)
        #lookUpElement.send_keys(Keys.RETURN)


    # Helper function for testing the Edit Overflow text transformation page
    #
    # Invoke a Shift + Alt keyboard shortcut (that are
    # defined for various buttons and text fields
    # in Edit Overflow for web)
    #
    def webpageKeyboardShort(self, aKey):

        # For now: only works on the Text.php page!!! Should we pass in
        #          element?
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
    def textTransformation(self,
                           aURL,
                           aTextBefore,
                           aTextAfter,
                           aKeyboardShortcutLetter,
                           anErrorMessage):

        self.browser.get(aURL)

        self.setGeneralTextField(aTextBefore)

        time.sleep(1.0)

        self.webpageKeyboardShort(aKeyboardShortcutLetter)

        lookUpElement = self.browser.find_element_by_name("someText")

        formatterResult = lookUpElement.get_attribute("value")

        self.assertEqual(formatterResult, aTextAfter, anErrorMessage)

        time.sleep(3.0) # Only for manual inspection of the
                        # result. Can be removed at any time


    # Text.php page: Test enclosing text in real quotes
    #
    # Note: The specific function / button (for YouTube comments) is
    #       by the keyboard shortcut Shift + Alt + Q (the fourth
    #       parameter to textTransformation(), "q")
    #
    #
    def checkRealQuotes(self, aURL):

        content1_in1 = "secure"
        #content1_out_new1 = "“" # Adaptation to the current bug
        content1_out_new1 = "“secure”" # Adaptation to the current bug

        # Send Shift + Alt + Q for invoking the formatting
        self.textTransformation(aURL,
                                content1_in1,
                                content1_out_new1,
                                "q",
                                "The real quotes formatting result was bad!")


    # Text.php page: Test formatting of YouTube comments
    #
    # Note: The specific function / button (for YouTube comments) is
    #       by the keyboard shortcut Shift + Alt + Y (the fourth
    #       parameter to textTransformation(), "y")
    #
    def checkYouTubeFormatting(self, aURL):

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
        self.textTransformation(
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
        self.textTransformation(aURL,
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
    #       parameter to textTransformation(), "m")
    #
    def checkMarkdownCodeFormatting(self, aURL):

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
        self.textTransformation(
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
        self.textTransformation(
            aURL, content_in_2, content_out_2,
            "m", "The Markdown formatter result was bad!")


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
    def mainLookup(self, aURL):
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
            self.checkEditSummary(singleLookup_editSummary_PHP,
                                  'Unexpected edit summary after URL GET lookup')

            # First direct lookup with a known incorrect term
            self.lookUp("python", firstRealLookup_editSummary, defaultMsgForEditSummary)

        if True: # Test a failed lookup.

            # The edit summary should be unchanged for a
            # failed lookup. We had a regression that
            # was fixed 2020-11-29.
            #
            self.lookUp("PHP__Z",
                        firstRealLookup_editSummary,
                        'Changed edit summary for a failed Edit Overflow lookup')

        if True: # Lookup ***after a failed*** lookup.

            # Second direct lookup with a known
            # correct term (identity mapping)
            self.lookUp("until",
                        'Active reading [<https://en.wikipedia.org/wiki/PHP> <https://en.wikipedia.org/wiki/Python_%28programming_language%29> <https://en.wiktionary.org/wiki/until#Conjunction>].',
                        defaultMsgForEditSummary + 'for looking up a ***correct*** term')

        if True: # Test clearing the edit summary state (user controlled
                 # by checkbox "Reset lookup state")
                 #
                 # We had a regression with an empty edit summary...

            #print("Setting reset checkbox...")
            self.setCheckbox("resetState")

            # Lookup with a reset, with a known term
            #
            self.lookUp("php", singleLookup_editSummary_PHP, defaultMsgForEditSummary)


        if True: # Do a ***failing lookup*** after a reset - we had a
                 # regression with an edit summary of "Active reading []."
                 # (should be empty (an empty string))

            # Regression 2020-12-01 (now fixed): "Active reading []."
            #
            # The edit summary should be unchanged from the previous
            #
            self.lookUp("PHP__Z", singleLookup_editSummary_PHP, defaultMsgForEditSummary)


        if True: # Test clearing the edit summary state and with
                 # an initial lookup of an unknown term

            self.setCheckbox("resetState")
            self.lookUp("PHP__Z", "", defaultMsgForEditSummary)


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
        self.mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=yes&OverflowStyle=Native')
        #pass


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    # ***Note***: The form-based (server roundtrip) version
    #
    #def test_mainLookup(self):
    def test_mainLookup_form(self):

        # "php"
        self.mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=no&OverflowStyle=Native')
        #pass


    # Form-based lookup, using a local web server
    #
    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    #def test_mainLookup(self):
    def test_mainLookup_form_localWebserver(self):

        #self.mainLookup('http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=Ghz')

        # "php"
        self.mainLookup('http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=php')
        #pass


if __name__ == '__main__':
    unittest.main()
