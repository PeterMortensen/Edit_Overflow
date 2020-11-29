
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

    browser = 0

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


    # Test of the central function of Edit Overflow for web: Looking
    # up incorrect terms (typically misspelling words)
    #
    def test_mainLookup(self):
        # Initial page, with a (known) incorrect term different
        # from the default of 'cpu': 'php'
        #
        self.browser.get('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&OverflowStyle=Native&UseJavaScript=no')
        time.sleep(2.0)

        firstRealLookup_editSummary = 'Active reading [<https://en.wikipedia.org/wiki/PHP> <https://en.wikipedia.org/wiki/Python_%28programming_language%29>].'
        defaultMsgForEditSummary = 'Unexpected edit summary '

        if True: # Test a normal lookup (implicitly through HTML get). For
                 # now only regression test for the edit summary field.

            # For the initial page, we expect a non-empty edit summary
            # field (though we actually currently make a lookup
            # through the opening URL)
            #
            self.checkEditSummary('Active reading [<https://en.wikipedia.org/wiki/PHP>].',
                                  'Unexpected edit summary after URL POST lookup')


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


if __name__ == '__main__':
    unittest.main()

