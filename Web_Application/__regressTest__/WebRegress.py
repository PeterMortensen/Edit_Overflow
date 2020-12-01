
########################################################################
#
#  Purpose: Regression test for the main function in Edit Overflow
#           for web, in particular the correct built up of edit
#           summary messages, including looking up terms not in
#           our word list.
#
#           It uses Selenium with Selenium Webdriver to control Firefox
#           windows and send information to and from them.
#
########################################################################

import unittest

from selenium import webdriver
import time
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.common.by import By


class TestMainEditOverflowLookup_Web(unittest.TestCase):


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


    # =======================================================================

    # Test of passing parameters through HTML GET
    #
    def test_mainLookup_HTTP_GET(self):

        if True: # Start out through HTML GET with an unknown term (new browser window)

            self.browser.get('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu777777&OverflowStyle=Native&UseJavaScript=no')
            self.checkEditSummary("",
                                  'Unexpected edit summary after URL GET lookup')
            time.sleep(3.0)

            #time.sleep(5.0)


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    def test_mainLookup(self):
        # Initial page, with a (known) incorrect term different
        # from the default of 'cpu': 'php'
        #
        self.browser.get('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&OverflowStyle=Native&UseJavaScript=no')
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

        if True: # Lookup after a failed lookup.

            # Second direct lookup with a known
            # correct term (identity mapping)
            self.lookUp("until",
                        'Active reading [<https://en.wikipedia.org/wiki/PHP> <https://en.wikipedia.org/wiki/Python_%28programming_language%29> <https://en.wiktionary.org/wiki/until#Conjunction>].',
                        defaultMsgForEditSummary + 'for looking up a ***correct*** term')

        if True: # Test clearing the edit summary state (user controlled
                 # by checkbox "Reset lookup state")
                 #
                 # We had a regression with an empty edit summary

            #print("Setting reset checkbox...")
            self.setCheckbox("resetState")

            # Lookup with a reset, with a known term
            #
            self.lookUp("php", singleLookup_editSummary_PHP, defaultMsgForEditSummary)
            

        if True: # Do a failed lookup after a reset - we had a regression
                 # with an edit summary of "Active reading []." (should
                 # be empty (an empty string))

            # Regression 2020-12-01 (now fixed): "Active reading []."
            #
            # The edit summary should be unchanged from the previous
            #
            self.lookUp("PHP__Z", singleLookup_editSummary_PHP, defaultMsgForEditSummary)


        if True: # Test clearing the edit summary state and with
                 # an initial lookup of an unknown term

            self.setCheckbox("resetState")
            self.lookUp("PHP__Z", "", defaultMsgForEditSummary)


if __name__ == '__main__':
    unittest.main()

