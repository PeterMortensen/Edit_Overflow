
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

        time.sleep(5.0) # Let us have a look for a time...
        self.browser.close()

    #def test_upper(self):
    #    self.assertEqual('foo'.upper(), 'FOO')

    #def test_isupper(self):
    #    self.assertTrue('FOO'.isupper())
    #    self.assertFalse('Foo'.isupper())
    #
    #def test_split(self):
    #    s = 'hello world'
    #    self.assertEqual(s.split(), ['hello', 'world'])
    #    # Check that s.split fails when the separator is not a string
    #    with self.assertRaises(TypeError):
    #        s.split(2)

    def test_mainLookup(self):
        #browser = webdriver.Firefox()
        #time.sleep(6.0)

        # Initial page, with a (known) incorrect term different
        # from the default of 'cpu': 'php'
        #
        self.browser.get('https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&OverflowStyle=Native&UseJavaScript=no')
        time.sleep(2.0)

        if True: # Test a normal lookup (implicitly through HTML get). For
                 # now only regression test for the edit summary field.

            EditSummaryElement = self.browser.find_element_by_name("editSummary_output")

            # For the initial page, we expect a non-empty edit summary
            # field (though we actually currently make a lookup
            # through the opening URL)
            EditSummary1 = EditSummaryElement.get_attribute("value")
            #print('Edit summary after looking up "php": ' + EditSummary1)

            self.assertEqual(EditSummary1, 'Active reading [<https://en.wikipedia.org/wiki/PHP>].')

        if True: # Test a failed lookup. For now only regression
                 # test for the edit summary field.

            lookUpElement = self.browser.find_element_by_name("LookUpTerm")
            lookUpElement.clear()
            lookUpElement.send_keys("PHP__Z")
            lookUpElement.send_keys(Keys.RETURN)

            # This is crucial: We must wait for the update of the
            # field (server roundtrip time and Firefox update)
            time.sleep(2.0)

            # We must also repeat this to avoid this error:
            #
            #    "The element reference of <input id="editSummary_output"
            #    class="XYZ4" name="editSummary_output" type="text">
            #    is stale; either the element is no longer attached to
            #    the DOM, it is not in the current frame context, or
            #    the document has been refreshed"
            #
            EditSummaryElement = self.browser.find_element_by_name("editSummary_output")
            EditSummary2 = EditSummaryElement.get_attribute("value")

            # The edit summary should be unchanged for failed lookup
            self.assertEqual(EditSummary2,
                             'Active reading [<https://en.wikipedia.org/wiki/PHP>].',
                             'Changed edit summary for a failed Edit Overflow lookup')


if __name__ == '__main__':
    unittest.main()

