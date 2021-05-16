<?php
    # File: Text.php


    # Strange things are happening when this WordPress thing
    # is included (a fix was added 2019-11-30, and finally
    # another one fix 2019-12-10).
    #
    #   1. The YouTube convert (that uses a separate
    #      PHP class in another file) had a lot of
    #      warning lines. This turned out to be a genuine
    #      bug - the WordPress include had a configuration
    #      that turned on debugging and surfaced the error.
    #
    #   2. Removing trailing space escapes single quotes
    #      with backslash... This is a WordPress thing
    #      (returned form data is escaped). The workaound
    #      was to remove backslashes (this may be
    #      sufficient as we don't XXX).
?>


<!--
    Note:

      We can now use "OverflowStyle=Native" to avoid the WordPress overhead:

        <https://pmortensen.eu/world/Text.php?OverflowStyle=Native>

        Using HTML GET parameter and invoking function "Format as keyboard":

          <https://pmortensen.eu/world/Text.php?someText=dont&action=Format%20as%20keyboard&OverflowStyle=Native>

-->


<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Text Stuff");
        ?>

        <?php

            function removeTrailingSpace($aText)
            {
                # This actually also removes trailing TABs... (as it should)
                #
                return preg_replace('/[ \t]+(\r?$)/m', '$1', $aText);
            }


            function replaceTABS_withSpace($aText)
            {
                return preg_replace('/\t/', '    ', $aText);
            }


            function removeTrailingSpacesAndTABs($aText)
            {
                # Order doesn't matter! ***Trailing*** TABs are
                # actually removed by removeTrailingSpace().
                #
                $lengthBefore = strlen($aText);
                $text1 = removeTrailingSpace($aText);
                $lengthAfter = strlen($text1);
                $removedTrailingSpaces = $lengthBefore - $lengthAfter;

                $text2 = replaceTABS_withSpace($text1);
                $lengthAfter2 = strlen($text2);

                # One TAB results in four spaces (three more
                # characters) What about rounding??
                $replacedTABs = ($lengthAfter2 - $lengthAfter) / 3;

                $message2 = "";
                if ($lengthBefore>0)
                {
                    $message2 =
                        "<p>" .
                        "Replaced $replacedTABs TABs with spaces " .
                        "and removed $removedTrailingSpaces trailing spaces..." .
                        "</p>\n";
                }
                return [$text2, $message2];
            } #removeTrailingSpacesAndTABs()


            function findCommonLeadingSpaces($aText)
            {
                # Implicit convert of TABs (as 4 spaces)
                $someText = replaceTABS_withSpace($aText);

                #For some reason, it is Windows-like when coming from
                #the web browser (Firefox), even when all is Linux...
                #
                #But why???
                #
                $lines = explode("\r\n", $someText);

                $commonLeadingSpaces = 9999;
                $nonEmptyLines = 0;

                # Find the non-empty line with the least number
                # of leading spaces - that is how many leading
                # spaces we should remove from each line.
                #
                foreach ($lines as $key => $item)
                {
                    #echo '<p>Line: xxx' . $item . 'xxx </p>' . "\n";

                    # Ignore empty lines
                    if ($item !== "")
                    {
                        $nonEmptyLines++;

                        $leadingSpaces = 0;
                        if (preg_match('/^(\s+)/', $item, $out))
                        {
                            #echo '<p>Leading space: xxx' . $out[0] . 'xxx </p>' . "\n";

                            $leadingSpaces = strlen($out[0]);

                            #echo '<p>Number of leading spaces: ' . $leadingSpaces . '</p>' . "\n" . "\n";
                        }

                        # Update the minimum (for lines)
                        if ($leadingSpaces < $commonLeadingSpaces)
                        {
                            $commonLeadingSpaces = $leadingSpaces;
                        }
                    }
                }
                if ($nonEmptyLines <= 1)
                {
                    $commonLeadingSpaces = 0;
                }
                return $commonLeadingSpaces;
            } # findCommonLeadingSpaces()


            function removeCommonLeadingSpaces($aText, $aLeadingSpaces)
            {
                # Implicit convert of TABs (as 4 spaces)
                $someText = replaceTABS_withSpace($aText);

                #For some reason, it is Windows-like when coming from
                #the web browser (Firefox), even when all is Linux...
                $lines = explode("\r\n", $someText);

                $toRemove = str_repeat(" ", $aLeadingSpaces);

                #echo '<p>To remove: xxx' . $toRemove . 'xxx </p>' . "\n";

                $newContent = array();
                foreach ($lines as $key => $item)
                {
                    #echo '<p>Line: xxx' .     $item .  'xxx </p>' . "\n";

                    $item2 = preg_replace("/^$toRemove/", '', $item);

                    array_push($newContent, $item2);

                    #echo '<p>New line: xxx' . $item2 . 'xxx </p>' . "\n";
                }
                return implode("\r\n", $newContent);
            } #removeCommonLeadingSpaces()


            # -------------------- End of main functions ---------------------


            # ------------- Start of unit tests (sort of) section ------------

            #Yes, they should be moved somewhere else
            #and executed in a different context.


            # $aLengthDiff is old minus new (so it is ***positive***
            # if the new string is ***shorter*** - the most common
            # case in this context (this file)).
            #
            function assert_strLengths($ID, $aOrigText, $aNewText, $aLengthDiff)
            {
                $lenBefore = strlen($aOrigText);
                $lenAfter  = strlen($aNewText);
                $diff = $lenBefore - $lenAfter;

                #Some redundancy here (refactor?)

                if ($lenBefore < 2)
                {
                    echo "<p>Likely flawed test. ID: $ID. $lenBefore characters before. Original text: XXX" . $aOrigText . "XXX </p>\n";
                    assert(false);
                }
                if ($lenAfter < 2)
                {
                    echo "<p>Likely flawed test. ID: $ID. $lenAfter characters after. New text: XXX" . $aOrigText . "XXX </p>\n";
                    assert(false);
                }

                if (! ($diff === $aLengthDiff))
                {
                    echo "<br/><br/>\n";
                    echo "Failed test. ID: $ID. $lenBefore characters before. " .
                          "$lenAfter characters after. " .
                          "Expected difference: $aLengthDiff. Actual: $diff\n";

                    echo "<br/><br/>\n";
                    echo "Before: XXX" . $aOrigText . "XXX. \n";
                    echo "<br/><br/>\n";
                    echo "After:  XXX" . $aNewText  . "XXX. \n";
                    assert(false);
                }
                else
                {
                    #echo "<br/>\n";
                    ##echo "<br/>\n";
                    #echo "It fits! Text length (incl. end-of-line): " .
                    #     "$lenBefore characters\n";
                }
            } # assert_strLengths()


            # Helper function for testing
            #
            # Why can't we just substitute the call of
            # removeTrailingSpacesAndTABs()?
            #
            # Answer: because then we would have to specify the input
            #         string two times on the client side (or use a
            #         variable, making it ***two*** statements on
            #         the client side instead of just a single
            #         function call) instead of just the length
            #         difference (somewhat redundant) - though
            #         it would make for a more accurate test.
            #
            function test_removeTrailingSpacesAndTABs($ID, $aSomeText, $aLengthDiff)
            {
                # [] because removeTrailingSpacesAndTABs() is returning
                # an array and we are only using the first element...
                #
                [$touchedText] = removeTrailingSpacesAndTABs($aSomeText);
                assert_strLengths($ID,
                                  $aSomeText,
                                  $touchedText,
                                  $aLengthDiff);
            }


            #Not used yet.
            #function removeCommonLeadingSpaces2($aText)
            #{
            #    $leadingSpaceToRemove = findCommonLeadingSpaces($aText);
            #    return removeCommonLeadingSpaces($aText, $leadingSpaceToRemove);
            #}


            # Helper function for testing
            #
            #For now, it is a utility function used for testing (that is,
            #combining findCommonLeadingSpaces() and removeCommonLeadingSpaces(),
            #but it would be nice to have a ***single*** function
            #exposed to the client code. E.g., could we return
            #the output string and the number as an array? We
            #are already doing it for removeTrailingSpacesAndTABs().
            #
            function test_removeCommonLeadingSpaces($ID, $aSomeText, $aLengthDiff)
            {
                #echo "<br/><br/>\n";
                #echo "Start of test $ID ...\n";

                #Later: refactor these two calls (we also have
                #       it in the normal client code)
                $leadingSpaceToRemove = findCommonLeadingSpaces($aSomeText);

                # Note: $aLengthDiff is not the same as $leadingSpaceToRemove
                #       as $leadingSpaceToRemove is per line and there
                #       can be many lines...
                #
                # But at least $aLengthDiff should be a multiplum
                # of $leadingSpaceToRemove:

                if  (!  (
                          (  $aLengthDiff >= $leadingSpaceToRemove)       &&

                          (
                            ($leadingSpaceToRemove === 0) # Guard for division
                                                          # by zero - short circuit
                                                          # Boolean presumed

                                                      ||

                            (($aLengthDiff % $leadingSpaceToRemove) === 0)
                          )
                        )
                    )
                {
                    echo "<br/><br/>\n";
                    echo
                      "Failed test. ID: $ID. Leading spaces to remove " .
                      "(per line), $leadingSpaceToRemove, is larger than " .
                      "(or otherwise incompatiple) the total number " .
                      "of removed spaces, $aLengthDiff...\n";
                    assert(false);
                }

                #echo "In test $ID: $leadingSpaceToRemove leading spaces to remove.\n";

                $someText7 = removeCommonLeadingSpaces(
                              $aSomeText, $leadingSpaceToRemove);

                assert_strLengths($ID,
                                  $aSomeText,
                                  $someText7,
                                  $aLengthDiff);
            }


            # Helper function for testing
            #
            #As to why not substitution of transformFor_YouTubeComments(),
            #see comments for test_removeTrailingSpacesAndTABs().
            #
            function test_transformFor_YouTubeComments($ID, $aSomeText, $aLengthDiff)
            {
                $touchedText = transformFor_YouTubeComments($aSomeText);

                #print "<p>$ID: AAAA $touchedText BBBB</pp>";

                assert_strLengths($ID,
                                  $aSomeText,
                                  $touchedText,
                                  $aLengthDiff);
            }


            # General comments/notes about testing:
            #
            #   1. To simulate what happens in the browser we need
            #      to use "\r\n" (Windows like), not "\n"
            #
            #      Why is it like Windows? We are on Linux, with Firefox,
            #      using a web application based on Linux hosting and PHP.
            #
            #   2. We currently test by checking for length differences,
            #      not the actual content. It is less specific testing,
            #      but also with less redundancy.
            #
            #      We also don't detect if findCommonLeadingSpaces() return
            #      wrong values with no consequence. E.g. 9999 for content
            #      where nothing should be removed.
            #
            #   3. Many of the test cases could be reduced down to the
            #      essential part (they are arbitrary real-world
            #      examples).
            #

            test_removeTrailingSpacesAndTABs(1001, "XX "           ,  1);
            test_removeTrailingSpacesAndTABs(1002, "X XX  XXX X\t ",  2);
            test_removeTrailingSpacesAndTABs(1003, "X XX  XXX X \t",  2);
            test_removeTrailingSpacesAndTABs(1004, "X XX  XXX X"   ,  0);
            test_removeTrailingSpacesAndTABs(1005, "X XX \t XXX X" , -3);

            # Content with a single (non-empty) line should be left alone.
            test_removeCommonLeadingSpaces(1006,
                "        *https://en.wikipedia.org/wiki/Catalan_Opening*", 0);

            # Leading TABs
            test_removeCommonLeadingSpaces(1007,
                "\t  00 min 44 secs:  ABC\r\n\t  XYZ", 6);

            # Removal of "www" for the encoded YouTube URLs
            #
            test_transformFor_YouTubeComments(1008,
                "     https://www.youtube.com/watch?v=_pybvjmjLT0\r\n        ",
                8);

            # Input containing lines ***without*** leading
            # space - should always be unchanged.
            test_removeCommonLeadingSpaces(1009,
                "28:50 : On page _Sequence alignment_\r\n" .
                  "               *https://en.wikipedia.org/wiki/Sequence_alignment*\r\n",
                0);

            test_removeCommonLeadingSpaces(1010,
                " 28:50 : On page _Sequence alignment_\r\n" .
                  "                *https://en.wikipedia.org/wiki/Sequence_alignment*",
                2);

            # This one has an empty line (that should be ignored
            # when finding how many leading spaces to remove)
            test_removeCommonLeadingSpaces(1011,
                " 28:50 : On page _Sequence alignment_\r\n" .
                  "                *https://en.wikipedia.org/wiki/Sequence_alignment*\r\n",
                2);

            # Content with a single (non-empty) line should be left alone.
            test_removeCommonLeadingSpaces(1012,
                "      https://www.youtube.com/watch?v=8Tnf_J3fTgU&lc=Ugy4ijM7CwjmKeVLgDd4AaABAg\r\n",
                0);

            # Some as 1012, but without an empty line.
            test_removeCommonLeadingSpaces(1013,
                "      https://www.youtube.com/watch?v=8Tnf_J3fTgU&lc=Ugy4ijM7CwjmKeVLgDd4AaABAg",
                0);

            # Test-first for new exceptions to the general URL substitution -
            # prompted by "Node.js", both as pure text and in the Wikipedia
            # URL (but it will cover many JavaScript frameworks).
            #
            # Note: The *first* part of an HTTPS Wikipedia URL happens
            #       to have the same length after transformation (thus
            #       "0") for the third parameter. This is a pure
            #       coincidence.
            #
            test_transformFor_YouTubeComments(1014,
                "     https://en.wikipedia.org/wiki/Node.js    \r\n",
              0);

            test_transformFor_YouTubeComments(1015, "Node.js", 0);

            # Normal full stops
            test_transformFor_YouTubeComments(1016, "first job._", 0);
            test_transformFor_YouTubeComments(1017, "first job.", 0);

            # Insertion of empty space on empty lines - introduced
            # after changes to YouTube comments on 2020-05-21
            #
            test_transformFor_YouTubeComments(1018,
                                              "     Graasten\r\n" .
                                                "\r\n" .
                                                "XXXX\r\n",
                                              -1);

            test_transformFor_YouTubeComments(1019, "Which is 1.7 V higher.", 0);

            // Test of not expanding the full stop in file extensions
            //
            // Note: It happens to be 0 for the domain names with three
            //       parts because the expanded two dots correspond
            //       to the removed "https://".
            //
            //
            // Note: Positive numbers for output smaller than the input.
            //
            test_transformFor_YouTubeComments(1020, "https://www.tutorialspoint.com/design_pattern/filter_pattern.htm", 0);
            test_transformFor_YouTubeComments(1021, "https://en.wiktionary.org/wiki/File:en-us-tear-verb.ogg", 0);
            test_transformFor_YouTubeComments(1022, "https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=Javascript", 4);
            test_transformFor_YouTubeComments(1023, "https://www.dotnetrocks.com/default.aspx?ShowNum=1636", 0);
            test_transformFor_YouTubeComments(1024, "http://www.fullerton.edu/IT/_resources/pdfs/software_students/MATLAB-Toolboxes-AY2017_18.pdf", -1);

            # Test for removal of leading spaces (so the text lines up when posted to YouTube)
            #
            # Note: Currently it really only works for no leading
            #       space for the timestamps (e.g. remove common
            #       leading spaces already applied)
            #
            # More than line
            #
            # 8 is for the timestamp, -1 for the space on the empty line,
            # and 3 for removal of leading space.
            #
            test_transformFor_YouTubeComments(1025,
                                              "04 min 17 secs:  Real start of pre Q&A\r\n" .
                                              "\r\n" .
                                              "                 Why make.\r\n",
                                              8 - 1 + 3);

            # ----------------------------------------------------------------


            $message = "";
            $extraMessage = "";


            #assert(array_key_exists('function', $_REQUEST));  # From example:  isset($this->records)
            #
            # Note: We can't use the normal assert() as text.php
            #       is normally invoked by GET, by a direct link.
            #
            # Or perhaps we can distinguish between GET and POST? -
            #
            # Try to distinguish GET and POST. Is there an official way
            # to do this??
            #
            # Has this been fixed by our switch to "$_REQUEST"??????
            #
            $someText = "";
            if (array_key_exists(MAINTEXT, $_REQUEST))
            {
                # Some output to remind us that this WordPress
                # madness should be adressed some day
                if ($formDataSizeDiff > 0)
                {
                    $extraMessage =
                      $formDataSizeDiff .
                      " characters saved from WordPress madness...";
                }


                $someText = htmlentities(get_postParameter(MAINTEXT));

                assert(array_key_exists('action', $_REQUEST));  # From example:  isset($this->records)

                # The default value could be 'Some text </textarea>', for
                # making direct HTML validation by https://validator.w3.org/
                # have some non-trivial input.
                #
                # But we can't use it, because from the user side we
                # have a normal link and we don't want some default
                # text in the input - we want an empty input.
                #
                # For a regression test for a problem we had, we can
                # use the above text (with "</textarea>").
                # For instance, leaving out htmlentities() would
                # result in the HTML validation failing - but we
                # need another way for it to find its way to the
                # validation.
                #
                ###$someText = get_postParameter('someText')
                ###              ?? 'Some text </textarea>';
                ##$someText = htmlentities(get_postParameter('someText')
                ##                           ?? 'Some text </textarea>');
                #$someText = htmlentities(get_postParameter('someText');


                $actionStr = get_postParameter('action');

                switch ($actionStr)
                {
                    case "Remove TABs and trailing whitespace":

                        #echo '<p>Actions for: Remove TABs and trailing whitespace</p>';

                        [$someText, $message] = removeTrailingSpacesAndTABs($someText);
                        break;

                    case "Format as keyboard":

                        #echo '<p>Actions for: Format as keyboard</p>';

                        $someText = "<kbd>$someText</kbd>";
                        break;

                    case "Quote as code":

                        $someText = "`$someText`";
                        break;

                    case "Real quotes":

                        #Does not work... $someText is zapped and it shows two backticks...

                        $someText = "“$someText”";
                        break;

                    case "Transform for YouTube comments":

                        $someText = transformFor_YouTubeComments($someText);
                        break;

                    case "Remove common leading space":

                        #echo '<h3>Lines...</h3>' . "\n";

                        # Note: We have two functions because we need to first
                        #       scan all lines before we know how many spaces
                        #       to remove from each line (thus, it is not
                        #       just for statistics - we actually need it
                        #       for correct operation).
                        #
                        #       But perhaps we could use the same technique
                        #       as for removeTrailingSpacesAndTABs() and
                        #       combine it into one function?

                        $leadingSpaceToRemove = findCommonLeadingSpaces($someText);

                        $someText = removeCommonLeadingSpaces(
                                        $someText, $leadingSpaceToRemove);

                        $message = "<p>Removed " . $leadingSpaceToRemove .
                                    " leading spaces from all lines...</p>\n";
                        break;

                    default:
                        assert(0, "Switch fall-through...");
                }
            }
            else
            {
                #echo "<p>Initial opening of Text.php</p>\n";
            }

        # End of main PHP part
        ?>


        <form
            name="XYZ"
            method="post"
            id="XYZ">

            <p>Text:</p>

            <!--  type="text"  -->
            <!--  id="Text"  -->
            <!--  class="XYZ3"  -->
            <!--  style="width:500px;"  -->
            <textarea
                name="someText"
                cols="40"
                rows="5"
                style="width:700px;height:150px;"
                accesskey="T"
                title="Shortcut: Shift + Alt + T"
            ><?php
                echo "$someText";
            ?></textarea>


            <!-- Remove TABs and trailing whitespace button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp"
                class="XYZ3"
                value="Remove TABs and trailing whitespace"
                style="width:325px;"
                accesskey="U"
                title="Shortcut: Shift + Alt + U"
            />

            <!-- Format as keyboard button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp2"
                class="XYZ19"
                value="Format as keyboard"
                style="width:180px;"
                accesskey="B"
                title="Shortcut: Shift + Alt + B"
            />

            <!-- Quote as code button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp3"
                class="XYZ20"
                value="Quote as code"
                style="width:150px;"
                accesskey="K"
                title="Shortcut: Shift + Alt + K"
            />

            <!-- Real quotes button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp28"
                class="XYZ28"
                value="Real quotes"
                style="width:150px;"
                accesskey="Q"
                title="Shortcut: Shift + Alt + Q"
            />

            <!-- Transform for YouTube comments button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp29"
                class="XYZ29"
                value="Transform for YouTube comments"
                style="width:290px;"
                accesskey="Y"
                title="Shortcut: Shift + Alt + Y"
            />

            <!-- Remove common leading space button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp30"
                class="XYZ30"
                value="Remove common leading space"
                style="width:275px;"
                accesskey="L"
                title="Shortcut: Shift + Alt + L"
            />

        </form>

        <?php

            #Experiment disabled for now
            # # Trial of a page counter (plugin "Page Visit Counter",
            # # <https://wordpress.org/plugins/page-visit-counter/>
            #
            # #echo do_shortcode('[pvcp_1]'); # Page counter. From plugin "Page Visit Counter",
            #                                 # <https://wordpress.org/plugins/page-visit-counter/>.
            # #
            # if (function_exists('do_shortcode'))
            # {
            #     echo "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            #     echo "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            #
            #     #echo do_shortcode('[gallery]'); # Results in some CSS output and also
            #                                     # URLs to some of our images, e.g.:
            #                                     # https://pmortensen.eu/world/wp-content/uploads/2019/11/owl3-200x200.ico
            #
            #     echo do_shortcode('[pvcp_1]'); # Zero output (empty string)!
            #     echo "YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY";
            # }


            echo "$message $extraMessage\n\n";

            the_EditOverflowFooter('Text.php', "", "");
        ?>


            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FText.php"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FText.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.
        </p>

        <p>Proudly and unapologetic powered by PHP!</p>


<?php the_EditOverflowEnd() ?>

