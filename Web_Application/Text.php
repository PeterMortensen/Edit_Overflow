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
<?php include("commonStart.php"); ?>

        <?php
            $extraIndent = "            ";  # For insertion deeper down in
                                            # the HTML document.
            $allIndent   = "$extraIndent          ";
            the_EditOverflowHeadline(
              "Text Stuff",
              "Text.php",
              "",
              "3. Using HTML GET parameter and invoking function \"Format as keyboard\":\n\n" .
                $allIndent .
                "<https://pmortensen.eu/world/Text.php?someText=dont&action=Format%20as%20keyboard&OverflowStyle=Native>\n\n"
              );

            #Temp!!!!!!!!!!!!!!!!!!! Should always fail. This
            #worked (fired) for the local webserver, but
            #***not*** when running from the command-line.
            #Why????
            #
            #assert(false, "Unconditional assert failure...");

            #Temp!!!!!!!!!!!!!!!!!!! This is a static
            #test, without any of our own functions
            #
            #True...
            assert(preg_match('/php$/', "Text.phpZZZZ") !== true,
                    "Invalid file name...");

            #Also true...
            assert(preg_match('/php$/', "Text.phpZZZZ") !== false,
                    "Invalid file name...");

            #Works (false)!
            #assert(preg_match('/php$/', "Text.phpZZZZ") !== 0,
            #        "Invalid file name...");

            #True...
            assert(preg_match('/php$/', "Text.phpZZZZ") !== 1,
                    "Invalid file name...");
        ?>

        <?php
            # The main purpose:
            #
            #   Isolate the computation of the character length
            #   reported to the user (HTML shenanigans, like
            #   "<" encoded as "&lt;" might be involved).
            #
            # Note: We have replaced all use of the standard strlen() with
            #       this function, but that it only makes a difference for
            #       the absolute number.
            #
            #       For most of the built-in tests here, we operate on the
            #       character count difference, so any offset from the
            #       standard strlen() does not make any difference (no
            #       pun intended) for most tests ... An exception is
            #       non-test function findCommonLeadingSpaces(), affecting
            #       tests 1007, 1029, 1033, 1036, etc.
            #
            function strlen_POST($aSomeString)
            {
                # Note: This is a workaround for the problem with the
                #       reported number of characters (after a text
                #       transformation).
                #
                #       The underlying question is why we use htmlentities()
                #       (in function textTransformation() in the first
                #       place. Had it something to do with the
                #       WordPress madness?
                #
                #$effectiveString = $aSomeString;
                $effectiveString = html_entity_decode($aSomeString);

                return strlen($effectiveString);
            } #strlen_POST()


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
                $lengthBefore = strlen_POST($aText);
                $text1 = removeTrailingSpace($aText);
                $lengthAfter = strlen_POST($text1);
                $removedTrailingSpaces = $lengthBefore - $lengthAfter;

                $text2 = replaceTABS_withSpace($text1);
                $lengthAfter2 = strlen_POST($text2);

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


            # Helper functions.
            #
            # Used by several functions.
            #
            function preprocessTextForRemovingCommonLeadingSpaces($aText)
            {
                # Implicit convert of TABs (as 4 spaces)
                $someText = replaceTABS_withSpace($aText);

                [$someText, $message] = removeTrailingSpacesAndTABs($someText);

                return $someText;
            } #preprocessTextForRemovingCommonLeadingSpaces()


            function findCommonLeadingSpaces($aText)
            {
                $someText = preprocessTextForRemovingCommonLeadingSpaces($aText);

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
                # spaces we should remove from each line (in
                # clients).
                #
                foreach ($lines as $key => $item)  #Why "$key"??
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

                            $leadingSpaces = strlen_POST($out[0]);

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


            function removeCommonLeadingSpaces($aText)
            {
                $someText = preprocessTextForRemovingCommonLeadingSpaces($aText);

                $leadingSpaces = findCommonLeadingSpaces($someText);

                #For some reason, it is Windows-like when coming from
                #the web browser (Firefox), even when all is Linux...
                $lines = explode("\r\n", $someText);

                $toRemove = str_repeat(" ", $leadingSpaces);

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


            # Note: "none" as in Markdowm "lang_none" - syntax
            #        highlighting turned off.
            #
            # Future:
            #
            #   1. Consider adding a newline to the input content
            #      if it does not already contain it. This is
            #      to add some robustness (it does happen
            #      that Stack Overflow posts don't contain
            #      a newline at the end).
            #
            #   2.
            #
            function convertToMarkdownCodefencing_none($aText)
            {
                # First remove TABs and trailing space - especially for TABs
                # in the beginning of a line. It is especially important it
                # is converted, so we get the expected result.
                #
                [$someText, $message] = removeTrailingSpacesAndTABs($aText);


                # Remove four leading spaces from each line (if there
                # are at least four leading spaces in a line).
                #
                # ".*" seems to be non-greedy (unlike Perl). This should
                # be investigated further.
                #
                # Note: \s also seems to match the CR+LF sequence, so
                #       we use a literal space here.
                #
                #$replacer->transform('\s\s\s\s(.*)\r\n', '$1' . "\r\n");
                $replacer = new StringReplacerWithRegex($someText);
                $replacer->transform('    (.*)\r\n', '$1' . "\r\n");
                $someText = $replacer->currentString();


                #Delete at any time
                #$replacer2 = new StringReplacerWithRegex($someText);
                #
                ## Find the space by removing everything else, but
                ## leading space on the first line
                ##
                ## Note: Everything that matches is replaced. () and $
                ##       is dynamic content (e.g., some number)
                ##
                ## Major gotcha: ".*" does not match everything! It doesn't
                ## match the end of line... So instead of just ".*" we
                ## have to use "(.|\r|\n)*".
                ##
                #$replacer2->transform('^([ \t]*)(.|\r|\n)*', '$1');
                #$codefenceIndent = $replacer2->currentString();


                # Find indent for the code fences. It should be the indent
                # for the line with the minimum amount of indent (except
                # empty lines)
                #
                $minimumLeadingSpaces = 9999;

                # Note: Double quotes are necessary, in
                #       contrast to regular expressions
                $lines = explode("\r\n", $someText);

                foreach($lines as $someLine)
                {
                    # We don't want content ***ending with newline*** (the
                    # last element in the array will be empty in that
                    # case) or empty lines to affect the result.
                    if (strlen_POST($someLine) > 0)
                    {
                        #echo "Line: XXX  $someLine   ZZZZ <br/>\n";

                        $lenBefore = strlen_POST($someLine);
                        $lenAfter = strlen_POST(ltrim($someLine));
                        $leadingSpaces = $lenBefore - $lenAfter;

                        if ($leadingSpaces < $minimumLeadingSpaces)
                        {
                            $minimumLeadingSpaces = $leadingSpaces;
                            #echo "New minimum white space: $minimumLeadingSpaces\n";
                        }
                    }
                }
                $codefenceIndent = str_repeat(" ", $minimumLeadingSpaces);

                #echo
                #    "\n\ncodefenceIndent: ___" . $codefenceIndent . "___\n\n" .
                #    "someText: ___" . $someText . "___\n\n"
                #    ;

                # Wrap in Markdown code fences
                return
                    $codefenceIndent . "```lang-none\r\n" .
                    $someText .
                    $codefenceIndent . "```\r\n";

            } #convertToMarkdownCodefencing_none()

            # Change case, like in Geany: If all letters are lowercase,
            # change all letters to uppercase. Otherwise, change all
            # letters to uppercase.
            #
            function toggleCase($aText)
            {
                $lowercaseText = strtolower($aText);

                #For now (YAGNI): Also format for italics (Markdown) as this
                #is the most common use case (converting from all uppercase
                #to lowercase with formatting (Stack Exchange used as an
                #ancient text-only forum without any formatting)). We
                # can add more granularity later.
                #
                $toReturn = '*' . $lowercaseText  . '*'; # Default

                if ($lowercaseText === $aText)
                {
                    $toReturn = strtoupper($aText);
                }

                #Stub
                #return $aText . ' ZZZZZ';

                return $toReturn;
            } #toggleCase()


            # -------------------- End of main functions ---------------------


            # ------------- Start of unit tests (sort of) section ------------

            #Yes, they should be moved somewhere else
            #and executed in a different context.


            # $aLengthDiff is old minus new (so it is ***positive***
            # if the new string is ***shorter*** - the most common
            # case in this context (this file)).
            #
            # Note: We can not test aLengthDiff for 0 as some tests actually
            #       specify a value of 0 - e.g., to test that some content
            #       is NOT converted (e.g., some "."s are not seen as
            #       belonging to a URL...)
            #
            function assert_strLengths($anID, $aOrigText, $aNewText, $aLengthDiff)
            {
                $lenBefore = strlen_POST($aOrigText);
                $lenAfter  = strlen_POST($aNewText);
                $diff = $lenBefore - $lenAfter;

                # Sanity check of parameters
                #
                #Some redundancy here (refactor?)
                if ($lenBefore < 2)
                {
                    echo "<p>Likely flawed test. ID: $anID. $lenBefore characters before. Original text: ___" . $aOrigText . "___ </p>\n";
                    assert(false);
                }
                if ($lenAfter < 2)
                {
                    echo "<p>Likely flawed test. ID: $anID. $lenAfter characters after. New text: ___" . $aOrigText . "___ </p>\n";
                    assert(false);
                }

                # Actual test
                if (! ($diff === $aLengthDiff))
                {
                    echo "<br/><br/>\n";
                    echo "    Failed test. ID: $anID. $lenBefore characters before. " .
                          "$lenAfter characters after. " .
                          "Expected difference: $aLengthDiff. Actual: $diff" .
                          ". New text: >>>$aNewText<<<" .
                          #"\n\n" .
                          #"\n"
                          "";

                    echo "<br/><br/>\n";
                    echo "Before: ___" . $aOrigText . "___. \n";
                    echo "<br/><br/>\n";
                    echo "After:  ___" . $aNewText  . "___. \n";
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
            #         string ***two times*** on the client side (or
            #         use a variable, making it ***two*** statements
            #         on the client side instead of just a single
            #         function call) instead of just the length
            #         difference (somewhat redundant) - though
            #         it would make for a more accurate test.
            #
            function test_removeTrailingSpacesAndTABs($anID, $aSomeText, $aLengthDiff)
            {
                # [] because removeTrailingSpacesAndTABs() is returning
                # an array and we are only using the first element...
                #
                [$touchedText] = removeTrailingSpacesAndTABs($aSomeText);
                assert_strLengths($anID,
                                  $aSomeText,
                                  $touchedText,
                                  $aLengthDiff);

                # Also cross-check with the output from the function that
                # is closer to the user interface / form processing.
                #
                # They should be identical - but for now
                # we only check for equal length.
                #
                $someButton['remove_TABs_and_trailing_whitespace'] = 1;
                [$someText2, $message] = textTransformation($aSomeText, $someButton);

                assert_strLengths($anID,
                                  $someText2,
                                  $touchedText,
                                  0);

            } #test_removeTrailingSpacesAndTABs()


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
            function test_removeCommonLeadingSpaces($anID, $aSomeText, $aLengthDiff)
            {
                #echo "<br/><br/>\n";
                #echo "Start of test $anID ...\n";

                #Later: refactor these two calls (we also have
                #       it in the normal client code)
                $leadingSpaceToRemove = findCommonLeadingSpaces($aSomeText);

                # Note: $aLengthDiff is not (always) the same as
                #       $leadingSpaceToRemove as $leadingSpaceToRemove
                #       is per line and there can be many lines...
                #
                # But at least $aLengthDiff should be a multiplum
                # of $leadingSpaceToRemove:
                #
                if  (!  (
                          (  $aLengthDiff >= $leadingSpaceToRemove)  &&

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
                      "Failed test. ID: $anID. Leading spaces to remove " .
                      "(per line), $leadingSpaceToRemove, is larger than " .
                      "(or otherwise incompatiple) the total number " .
                      "of removed spaces, $aLengthDiff...\n";
                    assert(false);
                }

                #echo "In test $anID: $leadingSpaceToRemove leading spaces to remove.\n";

                $someText7 = removeCommonLeadingSpaces($aSomeText);

                assert_strLengths($anID,
                                  $aSomeText,
                                  $someText7,
                                  $aLengthDiff);
            } #test_removeCommonLeadingSpaces()


            # Helper function for testing
            #
            #As to why not substitution of transformFor_YouTubeComments(),
            #see comments for test_removeTrailingSpacesAndTABs().
            #
            function test_transformFor_YouTubeComments($anID, $aSomeText, $aLengthDiff)
            {
                $touchedText = transformFor_YouTubeComments($aSomeText);

                #print "<p>$anID: AAAA $touchedText BBBB</p>";

                assert_strLengths($anID,
                                  $aSomeText,
                                  $touchedText,
                                  $aLengthDiff);
            } #test_transformFor_YouTubeComments()


            # Helper function for testing
            #
            function test_StringReplacer(
                $anID,
                $anInputText,
                $aRegularExpressionSearch,
                $aRegularExpressionReplace,
                $anExpectedOutputText,
                $aLengthDiff)
            {
                $replacer2 = new StringReplacerWithRegex($anInputText);

                # We actually know the expected result - parameter
                # anExpectedOutputText (in contrast to the other
                # test), but we use the existing report system
                # for the actual assert.
                #
                #anExpectedOutputText

                $replacer2->transform($aRegularExpressionSearch,
                                      $aRegularExpressionReplace);

                $outputText = $replacer2->currentString();

                assert_strLengths($anID,
                                  $anInputText,
                                  $outputText,
                                  $aLengthDiff);
            } #test_StringReplacer()


            # Helper function for testing
            #
            function test_convert_to_Markdown_codefencing($anID, $aSomeText, $aLengthDiff)
            {
                #echo "<br/>\n<br/>\ntest_convert_to_Markdown_codefencing(): Test number $anID<br/>\n";

                $touchedText = convertToMarkdownCodefencing_none($aSomeText);

                # Counting lines... The number of lines is expected
                # to increase by 2 (due to the code fences).
                #
                $newLinesBefore = substr_count($aSomeText,   "\r\n");
                $newLinesAfter =  substr_count($touchedText, "\r\n");
                $expectedNewLinesAfter = $newLinesBefore + 2;

                # Assumptions made about this function, no matter
                # the current expected set of inputs:
                #
                # 1. No leading space (this presumes all non-empty lines
                #    in the input have four spaces indent). This is due
                #    to the current inability of function
                #    convertToMarkdownCodefencing_none() to handle it
                #    (incorrect output - missing ***leading space***
                #    (indent) for the code fences).
                #
                #    Or in other words, it fails for input that does not
                #    need to be processed (not enough space indent).
                #
                # 2. Number of newlines has increased by 2 (for the
                #    code fencing)
                #
                if ($newLinesAfter != $expectedNewLinesAfter)
                {
                    echo "<p>The output does not contain the expected number of newlines " .
                          "(it has $newLinesAfter, but $expectedNewLinesAfter was expected). " .
                          "ID: $anID. The output: ___" . $touchedText . "___ </p>\n";
                    assert(false);
                }

                assert_strLengths($anID,
                                  $aSomeText,
                                  $touchedText,
                                  $aLengthDiff);
            } #test_transformFor_YouTubeComments()


            # Helper function for testing
            #
            function test_generateWikiMedia_Link(
                $anID, $aSomeURL, $aSomeCorrectTerm, $anExpectedOutput)
            {
                $WikiLink = WikiMedia_Link($aSomeURL, $aSomeCorrectTerm);

                #print "<p>For $anID. Output: >>>$WikiLink<<<</p>";
                #print "<p>Some URL: >>>$aSomeURL<<</p>";

                if ($WikiLink !== $anExpectedOutput)
                {
                    # Note: Currently test failures are detected
                    #       by the output of this string
                    #       (without the quotes):
                    #
                    #           "Failed test. ID: "

                    echo "<br/><br/>\n";
                    echo
                      "Failed test. ID: $anID. " .
                      "The output, \"$WikiLink\" is not as expected " .
                      "(\"$anExpectedOutput\").\n";
                    #assert(false);
                }
            } #test_generateWikiMedia_Link()


            # Helper function for testing
            #
            function test_extractGrammaticalWordClass(
                $anID, $aSomeURL, $anExpectedOutput)
            {
                $wordClass = extractGrammaticalWordClass($aSomeURL);

                #print "<p>For $anID. Output: >>>$WikiLink<<<</p>";
                #print "<p>Some URL: >>>$aSomeURL<<</p>";

                if ($wordClass !== $anExpectedOutput)
                {
                    # Note: Currently test failures are detected
                    #       by the output of this string
                    #       (without the quotes):
                    #
                    #           "Failed test. ID: "

                    echo "<br/><br/>\n";
                    echo
                      "Failed test. ID: $anID. Extract grammatical word class. " .
                      "The output, \"$wordClass\" is not as expected " .
                      "(\"$anExpectedOutput\").\n";
                    assert(false);
                }
            } #test_extractGrammaticalWordClass()

            # Helper function for testing
            #
            function test_crossReferenceGeneration(
                $anID,
                $anIncorrectTerm, $aLinkText, $anAttributeID, $aSomeAnnotation, $aCrossReferenceURL,
                $anExpectedOutput)
            {
                $HTMLcontent = alternativeLink($anIncorrectTerm,
                                                $aLinkText,
                                                $anAttributeID,
                                                $aSomeAnnotation,
                                                $aCrossReferenceURL);

                if ($HTMLcontent !== $anExpectedOutput)
                {
                    # Note: Currently test failures are detected
                    #       by the output of this string
                    #       (without the quotes):
                    #
                    #           "Failed test. ID: "

                    echo "<br/><br/>\n";
                    echo
                      "Failed test. ID: $anID. Cross-reference generation. " .
                      "The output, \"$HTMLcontent\" is not as expected " .
                      "(\"$anExpectedOutput\").\n";
                    assert(false);
                }
            } #test_crossReferenceGeneration()


            # The main functionality of this page (one of various
            # text transformations, selected by the user
            # (parameters)).
            #
            # (It is stateless, but the actual indexes for the $aButton
            #  hash (e.g. "remove_TABs_and_trailing_whitespace") are
            #  dependent on part of this web page and the mechanism
            #  for this to work with the particular notation is
            #  dependent on PHP.
            #
            #  Or in other words, the $aButton hash is dictated by
            #  how PHP works and the functionality that is used
            #  for connecting a button on the HTML form page
            #  with the processing code.)
            #
            #
            function textTransformation($aSomeText2, $aButton)
            {
                #Why do we use htmlentities()????? We are going to do
                #text transformations on the input text (e.g., this
                #is probably why one of the text transformations
                #for YouTube desn't work (the string "->")).
                #
                #A comment below says "leaving out htmlentities() would
                #result in the HTML validation failing"
                #
                #Is it because we want to avoid doing it in multiple
                #places for multiple output? But we do have a single
                #variable for all output here.
                #
                $someText3 = htmlentities($aSomeText2);
                #$someText3 = $aSomeText2; # Try it without!

                $message = ""; # To always have a defined value
                $fallThrough = 1;

                # There is quite a bit of redundancy here. We could
                # have the keys in an array, use a loop to convert
                # to a numeric code (-1 of not found) and use a
                # switch statement as before.
                #
                # Do we actually need the array? Couldn't we just use,
                # e.g., "remove_TABs_and_trailing_whitespace" by
                # itself?

                if (isset($aButton['remove_TABs_and_trailing_whitespace']))
                {
                    [$someText3, $message] = removeTrailingSpacesAndTABs($someText3);
                    $fallThrough = 0;
                }

                if (isset($aButton['format_as_keyboard']))
                {
                    $someText3 = "<kbd>$someText3</kbd>";
                    $fallThrough = 0;
                }

                if (isset($aButton['quote_as_code']))
                {
                    $someText3 = "`$someText3`";
                    $fallThrough = 0;
                }

                if (isset($aButton['real_quotes']))
                {
                    # Note: For unknown reasons, we can ***not*** use string
                    #       interpolation here. A bug was fixed 2021-09-21.
                    $someText3 = "“" . $someText3 . "”";

                    $fallThrough = 0;
                }

                if (isset($aButton['transform_for_YouTube_comments']))
                {
                    $someText3 = transformFor_YouTubeComments($someText3);
                    $fallThrough = 0;
                }

                if (isset($aButton['remove_common_leading_space']))
                {
                    # Note: We have two functions because we need to first
                    #       scan all lines before we know how many spaces
                    #       to remove from each line (thus, it is not
                    #       just for statistics - we actually need it
                    #       for correct operation).
                    #
                    #       But perhaps we could use the same technique
                    #       as for removeTrailingSpacesAndTABs() and
                    #       combine it into one function?

                    $leadingSpaceToRemove = findCommonLeadingSpaces($someText3);

                    $someText3 = removeCommonLeadingSpaces($someText3);

                    $message = "<p>Removed " . $leadingSpaceToRemove .
                                " leading spaces from all lines...</p>\n";

                    $fallThrough = 0;
                }

                if (isset($aButton['convert_to_Markdown_code_fencing']))
                {
                    $someText3 = convertToMarkdownCodefencing_none($someText3);

                    $message = ""; # Explicit no message
                    #$message = "<p>Converted " . $leadingSpaceToRemove .
                    #            " lines of code Markdown code fencing...</p>\n";

                    $fallThrough = 0;
                }

                if (isset($aButton['toggle_case']))
                {
                    $someText3 = toggleCase($someText3);

                    # Stub - we may want some statistics and which major
                    #        direction it went in (upper/lower).
                    $message = "Toggle case!";

                    $fallThrough = 0;
                }


                if ($fallThrough === 1)
                {
                    assert(0, "Switch fall-through... Variable button is: >>>$aButton<<<");
                }

                $lengthAfter = strlen_POST($someText3);

                $message .= "<p name=\"Message2\">Now $lengthAfter characters (incl. newlines).</p>";


                #Keep, as we will probably use a similar construct again, but with
                #a numeric code
                #$actionStr = get_postParameter('action');
                #switch ($actionStr)
                #{
                #    case "Remove TABs and trailing whitespace":
                #
                #        #echo '<p>Actions for: Remove TABs and trailing whitespace</p>';
                #
                #        #Temp!!!!!!!!!!
                #        #[$someText3, $message] = removeTrailingSpacesAndTABs($someText3);
                #        break;
                #
                #    case "Format as keyboard":
                #
                #        #echo '<p>Actions for: Format as keyboard</p>';
                #
                #
                #        #Temp!!!!!!!!!!
                #        #$someText3 = "<kbd>$someText3</kbd>";
                #        break;
                #
                #    case "Quote as code":
                #
                #        #Temp!!!!!!!!!!
                #        #$someText3 = "`$someText3`";
                #        break;
                #
                #    case "Real quotes":
                #
                #        #Does not work... $someText3 is zapped and it shows two backticks...
                #
                #        #Temp!!!!!!!!!!
                #        #$someText3 = "“$someText3”";
                #        break;
                #
                #    case "Transform for YouTube comments":
                #
                #        #Temp!!!!!!!!!!
                #        #$someText3 = transformFor_YouTubeComments($someText3);
                #        break;
                #
                #    case "Remove common leading space":
                #
                #        #echo '<h3>Lines...</h3>' . "\n";
                #
                #        # Note: We have two functions because we need to first
                #        #       scan all lines before we know how many spaces
                #        #       to remove from each line (thus, it is not
                #        #       just for statistics - we actually need it
                #        #       for correct operation).
                #        #
                #        #       But perhaps we could use the same technique
                #        #       as for removeTrailingSpacesAndTABs() and
                #        #       combine it into one function?
                #
                #        #Temp!!!!!!!!!!
                #        #$leadingSpaceToRemove = findCommonLeadingSpaces($someText3);
                #        #
                #        #$someText3 = removeCommonLeadingSpaces(
                #        #                $someText3, $leadingSpaceToRemove);
                #        #
                #        #$message = "<p>Removed " . $leadingSpaceToRemove .
                #        #            " leading spaces from all lines...</p>\n";
                #        break;
                #
                #    default:
                #        assert(0, "Switch fall-through... Variable actionStr is: >>>$actionStr<<<");
                #}

                return [$someText3, $message];
            } #textTransformation()


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

            # Note: The two space indent is to set it apart from
            #       the second parameter...

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

            # Test of not expanding the full stop in file extensions
            #
            # Note: It happens to be 0 for the domain names with three
            #       parts because the expanded two dots correspond
            #       to the removed "https://".
            #
            #
            # Note: Positive numbers for output smaller than the input.
            #
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

            test_generateWikiMedia_Link(1026, "https://en.wikipedia.org/wiki/PHP", "PHP", "[[PHP|PHP]]");

            # Cover the result for the MediaWiki link for terms that are not in our word list
            test_generateWikiMedia_Link(1027, "https://duckduckgo.com/html/?q=Wikipedia%20phpZZ", "phpZZ", "Does not apply");

            test_generateWikiMedia_Link(1028, "https://en.wikipedia.org/wiki/Cherry_(company)#Cherry_MX_switches_in_consumer_keyboards", "Cherry MX", "[[Cherry_(company)#Cherry_MX_switches_in_consumer_keyboards|Cherry MX]]");

            # Two and three leading spaces in two content lines,
            # respectively and a space an the empty line. We
            # presume trailing space is removed first,
            # before the primary transformation.
            #
            # Note: We can currently only use test input where the
            #       size difference is a multiplum of the number
            #       of spaces removed per line... (because of
            #       previous assumptions)
            test_removeCommonLeadingSpaces(1029,
                "  00 min 44 secs:  ABC\r\n" .
                  "    \r\n" .
                  "   04 min 17 secs:  Real start of pre Q&A\r\n",
                8);

            # Note: The size test happens to be not sensitive when there
            #       are two dots in the domain (e.g. "en.wikipedia.org"
            #       or "www.youtube.com" and https:// is used). A
            #       workaround is to use http://, but it is easy to
            #       make an off-by-one error when computing the
            #       expected result.
            #
            #       Or in other words, we risk false negative tests!
            #
            # Test for not converting Markdown links (used in LBRY comments)
            #
            test_transformFor_YouTubeComments(1030,
                "01 h 06 min 51 secs: \"OZ\" = [Australia](https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=OZ)\r\n",
                10);

            # Not a real Markdown link (or misformed). We could add more lines
            # in the same test here, for more variations.
            #
            test_transformFor_YouTubeComments(1031,
                "53 min 23 secs: Dave has issues with [Neomi Wu(http://en.wikipedia.org/wiki/Naomi_Wu)\r\n" .
                "53 min 23 secs: Dave has issues with [Neomi Wu](http://en.wikipedia.org/wiki/Naomi_Wu",
                7 + 7);


            # Major gotcha: ".*" does not match everything!
            # It doesn't match the end of line...
            #
            # This will not work:
            #
            #     '^([ \t]*).*'
            #
            # Note: The result string can not be shorter than 2
            #       due to constraints in the testing system.
            #       So we need more than one leading space...
            #
            test_StringReplacer(1037,
                "   End\r\n",
                '^([ \t]+)(.|\r|\n)*', '$1',
                '   ',
                5);

            #Future: Experiment with getting modifier "/s" to work. But
            #        we would have to modify file
            #        StringReplacerWithRegex.php as it is outside
            #        the "//" pair.


            # Magic numbers (we should probably eliminate them):
            #
            #   12: "```lang-none"
            #    3: "```"
            #    2: CR+LF
            #    2: For what??
            #
            $fixed_SizeIncrease = 12 + 2 + 3 + 2;
            $space_SizeDecreasePerLine = 4;

            test_convert_to_Markdown_codefencing(1032,
                "    End\r\n",
                1*$space_SizeDecreasePerLine - $fixed_SizeIncrease);

            #Extra spaces so the indent is not zero spaces
            test_convert_to_Markdown_codefencing(1033,
                "     End\r\n",
                1*$space_SizeDecreasePerLine - 2*1 - $fixed_SizeIncrease);

            test_convert_to_Markdown_codefencing(1034,
                "    if (\$next)\r\n" .
                "    {\r\n" .
                "        \$next .= \"Extras\";\r\n" .
                "    }\r\n" .
                "",
                4*$space_SizeDecreasePerLine - $fixed_SizeIncrease);

            # With an empty line (line with less than the
            # number of spaces we remove from each line).
            test_convert_to_Markdown_codefencing(1035,
                "    AAA\r\n" .
                "\r\n" .
                "    BBB\r\n" .
                "",
                2*$space_SizeDecreasePerLine - $fixed_SizeIncrease);

            # Testing for more than 4 spaces indent - only
            # four spaces should be removed - not all spaces.
            # Also, the code fences should be indented the
            # same as the resulting content.
            #
            # This is for it to work well with code in
            # Markdown lists.
            #
            test_convert_to_Markdown_codefencing(1036,
                "         End\r\n",
                1*$space_SizeDecreasePerLine - $fixed_SizeIncrease
                - 2 * 5   # 5: remaining space after removing 4 space
                          #    indent - the indent for each of the
                          #    two code fences.
                );

            # Check for automatic removal for TABs and trailing space
            #
            test_convert_to_Markdown_codefencing(1038,
                "    AAA   \r\n" .
                "\r\n" .
                "    BBB\r\n" .
                "",
                2*$space_SizeDecreasePerLine - $fixed_SizeIncrease
                + 3 # The 3 trailing spaces
                );

            # More indent on the ***first*** line than
            # some other line(s) - unbalanced input.
            #
            test_convert_to_Markdown_codefencing(1039,
                "        Privacy - Camera Usage\r\n" .
                "    Less indent\r\n" .
                "",
                2*$space_SizeDecreasePerLine - $fixed_SizeIncrease
                );

            # Correct character count (e.g., that user
            # input "<" is not counted as 4 ("&lt;")).
            #
            test_removeTrailingSpacesAndTABs(1040, "<https", 0);

            # Correct character count (e.g., that user
            # input "<" is not counted as 4 ("&lt;")).
            #
            test_extractGrammaticalWordClass(
              1041, "https://en.wiktionary.org/wiki/is#Verb", "Verb");

            test_extractGrammaticalWordClass(
              1042, "https://en.wikipedia.org/wiki/JavaScript", "");

            test_extractGrammaticalWordClass(
              1043, "https://en.wiktionary.org/wiki/google#Verb_2", "Verb");

            # Underscore / space should not be removed if used as
            # a separator of words
            test_extractGrammaticalWordClass(
              1044, "https://en.wiktionary.org/wiki/IMO#Prepositional_phrase", "Prepositional phrase");

            # Test that we actually suppress some HTML anchors in the
            # output. Those word entries do actually have a word
            # class, but it is not in the URL.
            #
            test_extractGrammaticalWordClass(
              1045, "https://en.wiktionary.org/wiki/PS#Alternative_forms", "");

            test_crossReferenceGeneration(
              1046,
                  "is_____",
                  "is",
                  1002,
                  "verb",
                  "https://en.wiktionary.org/wiki/is#Verb",
              "<a href=\"/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=is_____\" id=\"1002\">is</a> (verb) (<a href=\"https://en.wiktionary.org/wiki/is#Verb\">ref</a>)");


            #test_generateWikiMedia_Link(1029, "https://en.wikipedia.org/wiki/Cherry_(company)#Cherry_MX_switches_in_consumer_keyboards", "XXXXX", "[[Cherry_(company)#Cherry_MX_switches_in_consumer_keyboards|Cherry MX]]");
            #For debugging
            #assert(0, "Unconditional assert failure...");
            #
            #For debugging - e.g. for easily finding the URL query string
            #for the various button actions.
            ##
            #echo "<pre>";
            ##print_r($_REQUEST);
            ###print_r($_POST);
            #print_r(file_get_contents("php://input")); # Raw URL query string
            #echo "</pre>";



            # ===============================================================
            # ===============================================================
            # ===============================================================


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
            $someText = ""; # Default (defined), if not otherwise set.
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

                #assert(array_key_exists('action', $_REQUEST));  # From example:  isset($this->records)
                assert(array_key_exists('someAction', $_REQUEST));  # From example:  isset($this->records)


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
                # For instance, leaving out htmlZZZZentities() would
                # result in the HTML validation failing - but we
                # need another way for it to find its way to the
                # validation.
                #
                ###$someText = get_postParameter('someText')
                ###              ?? 'Some text </textarea>';
                ##$someText = htmlZZZZentities(get_postParameter('someText')
                ##                           ?? 'Some text </textarea>');
                #$someText = htmlZZZZentities(get_postParameter('someText');

                # An array! (What is the content? A single element or
                # several with the user-pressed button having a
                # truthy value?)
                $button    = get_postParameter('someAction');
                $textField = get_postParameter(MAINTEXT);

                [$someText, $message] = textTransformation($textField, $button);

            } # The GET or POST parameter exists
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

            <p><u>T</u>ext:</p>

            <!-- type="text" -->
            <!-- id="Text" -->
            <!-- class="XYZ3" -->
            <!-- style="width:500px;" -->
            <textarea
                name="someText"
                cols="40"
                rows="5"
                style="width:700px;height:150px;"
                accesskey="T"
                title="Shortcut: Shift + Alt + T"
            ><?php
                echo "$someText"; # The result of the text transformation
                                  # (for this time)
            ?></textarea>


            <!-- Remove TABs and trailing whitespace button -->
            <input
                name="someAction[remove_TABs_and_trailing_whitespace]"
                type="Submit"
                id="LookUp"
                class="XYZ3"
                value="Remove TABs and trailing whitespace"
                style="width:325px;"
                accesskey="U"
                title="Shortcut: Shift + Alt + U"
            />

            <!-- Format as keyboard button -->
            <input
                name="someAction[format_as_keyboard]"
                type="Submit"
                id="LookUp2"
                class="XYZ19"
                value="Format as keyboard"
                style="width:180px;"
                accesskey="B"
                title="Shortcut: Shift + Alt + B"
            />

            <!-- Quote as code button -->
            <input
                name="someAction[quote_as_code]"
                type="Submit"
                id="LookUp3"
                class="XYZ20"
                value="Quote as code"
                style="width:150px;"
                accesskey="K"
                title="Shortcut: Shift + Alt + K"
            />

            <!-- Real quotes button -->
            <input
                name="someAction[real_quotes]"
                type="Submit"
                id="LookUp28"
                class="XYZ28"
                value="Real quotes"
                style="width:150px;"
                accesskey="Q"
                title="Shortcut: Shift + Alt + Q"
            />

            <!-- Transform for YouTube comments button -->
            <input
                name="someAction[transform_for_YouTube_comments]"
                type="Submit"
                id="LookUp29"
                class="XYZ29"
                value="Transform for YouTube comments"
                style="width:290px;"
                accesskey="Y"
                title="Shortcut: Shift + Alt + Y"
            />

            <!-- Remove common leading space button -->
            <input
                name="someAction[remove_common_leading_space]"
                type="Submit"
                id="LookUp30"
                class="XYZ30"
                value="Remove common leading space"
                style="width:275px;"
                accesskey="L"
                title="Shortcut: Shift + Alt + L"
            />

            <!-- Convert to Markdown code fencing button -->
            <input
                name="someAction[convert_to_Markdown_code_fencing]"
                type="Submit"
                id="LookUp31"
                class="XYZ31"
                value="Convert to Markdown code fencing"
                style="width:290px;"
                accesskey="M"
                title="Shortcut: Shift + Alt + M"
            />

            <!-- Toggle case (in the Geany sense -->
            <input
                name="someAction[toggle_case]"
                type="Submit"
                id="LookUp32"
                class="XYZ32"
                value="Toggle Case"
                style="width:150px;"
                accesskey="C"
                title="Shortcut: Shift + Alt + C"
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
            #     echo "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
            #     echo "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
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
