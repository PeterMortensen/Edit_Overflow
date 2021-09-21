########################################################################
#
#  Purpose: Regression test for the main functions in Edit Overflow
#           for web, in particular the correct built up of edit
#           summary messages, including looking up terms not in
#           our word list.
#
#           It uses Selenium with Selenium Webdriver to control Firefox
#           windows and send information to and from them.
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


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelled words)
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
    # up incorrect terms (typically misspelling words)
    #
    # The JavaScript (client-side) version
    #
    # The skip: suppress running one of the tests (it will not pass
    #           until we implement the cross-platform system -
    #           with single source for C#, PHP, and JavaScript).
    #
    #def test_mainLookup(self):
    @unittest.skip("Skipping test_mainLookup_JavaScript() for now")
    def test_mainLookup_JavaScript(self):

        self.mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=yes&OverflowStyle=Native')
        #pass


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    # The form-based (server roundtrip) version
    #
    #def test_mainLookup(self):
    def test_mainLookup_form(self):

        self.mainLookup('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&UseJavaScript=no&OverflowStyle=Native')
        #pass




if __name__ == '__main__':
    unittest.main()
