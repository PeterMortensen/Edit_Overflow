
<?php
     #    Strange things are happening when this WordPress thing is
     #    included (a fix was added 2019-11-30, and finally
     #    another one fix 2019-12-10).
     #
     #      1. The YouTube convert (that uses a separate
     #         PHP class in another file) had a lot of
     #         warning lines. This turned out to be genuine
     #         bug - the WordPress include had a configuration
     #         that turned on debugging and surface the error.
     #
     #      2. Removing trailing space escapes single quotes
     #         with backslash... This is a WordPress thing
     #         (returned form data is escaped). The workaound
     #         was to remove backslashes (this may be
     #         sufficient as we don't )
?>


<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Text Stuff");
        ?>



        <?php
            require_once('StringReplacerWithRegex.php');

            # function assert()


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
                # Order doesn't matter! Trailing TABs are actually
                # removed by removeTrailingSpace().
                #
                $lengthBefore = strlen($aText);
                $text1 = removeTrailingSpace($aText);
                $lengthAfter = strlen($text1);
                $removedTrailingSpaces = $lengthBefore - $lengthAfter;

                $text2 = replaceTABS_withSpace($text1);
                $lengthAfter2 = strlen($text2);

                # One TAB results in four spaces (three more characters)
                # What about rounding??
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
            }

            # Note: This (first) version only works well if
            #       there are actually some leading space
            #       in some line.
            #
            function findCommonLeadingSpaces($aText)
            {
                #$lines = explode("\n", $aText);
                #$lines = explode("\r", $aText);
                $lines = explode("\r\n", $aText); # This works. But why???
                                                  # We are on Firefox on
                                                  # Linux. This looks like
                                                  # Windows!

                $commonLeadingSpaces = 9999;

                foreach ($lines as $key => $item)
                {
                    #echo '<p>Line: ' . $item . '</p>' . "\n";

                    if (preg_match('/^(\s+)/', $item, $out))
                    {
                        #echo '<p>Leading space: xxx' . $out[0] . 'xxx </p>' . "\n";

                        $leadingSpaces = strlen($out[0]);

                        #echo '<p>Number of leading spaces: ' . $leadingSpaces . '</p>' . "\n" . "\n";

                        # Update the minimum (for lines)
                        if ($leadingSpaces < $commonLeadingSpaces)
                        {
                            $commonLeadingSpaces = $leadingSpaces;
                        }
                    }
                }
                if ($commonLeadingSpaces > 5000)
                {
                    # We didn't find any leading space in any line...
                    $commonLeadingSpaces = 0;
                }
                return $commonLeadingSpaces;
            }

            function removeCommonLeadingSpaces($aText, $aLeadingSpaces)
            {
                $lines = explode("\n", $aText);

                # $toRemove = " " x $aLeadingSpaces;
                $toRemove = str_repeat (" ", $aLeadingSpaces);

                #echo '<p>To remove: xxx' . $toRemove . 'xxx </p>' . "\n";

                $newContent = array();
                foreach ($lines as $key => $item)
                {
                    #echo '<p>Line: ' . $item . '</p>';

                    $item2 = preg_replace("/^$toRemove/", '', $item);

                    array_push($newContent, $item2);

                    #echo '<p>New line: xxx' . $item2 . 'xxx </p>' . "\n";
                }
                return implode("\n", $newContent);
            }


            # ----------------------- End of main functions ---------------------------


            # ---------------- Sort of unit tests ----------------

            # $aLengthDiff is old minus new (so positive if the
            # new string is shorter).
            #
            function assert_strLengths($ID, $aOrigText, $aNewText, $aLengthDiff)
            {
                $lenBefore = strlen($aOrigText);
                $lenAfter  = strlen($aNewText);
                $diff = $lenBefore - $lenAfter;

                if (! ($diff === $aLengthDiff))
                {
                    echo "Failed test. ID: $ID. $lenBefore characters before. " .
                         "$lenAfter characters after. " .
                         "Expected difference: $aLengthDiff. Actual: $diff\n";

                    echo "Before: >>>$aOrigText<<<. \n";
                    echo "After:  >>>$aNewText<<<. \n";
                    assert(false);
                }
            }

            function test_removeTrailingSpacesAndTABs($ID, $aSomeText, $aLengthDiff)
            {
                [$touchedText] = removeTrailingSpacesAndTABs($aSomeText);
                assert_strLengths($ID,
                                  $aSomeText,
                                  $touchedText,
                                  $aLengthDiff);
            }

            test_removeTrailingSpacesAndTABs(1, "XX "           ,  1);
            test_removeTrailingSpacesAndTABs(2, "X XX  XXX X\t ",  2);
            test_removeTrailingSpacesAndTABs(3, "X XX  XXX X \t",  2);
            test_removeTrailingSpacesAndTABs(4, "X XX  XXX X"   ,  0);
            test_removeTrailingSpacesAndTABs(5, "X XX \t XXX X" , -3);

            [$someText, $someMessage] = removeTrailingSpacesAndTABs("X XX  XXX X \t");
            #echo "<p>someMessage: $someMessage</p>";

            # -------------------------------------------------------------


            const MAINTEXT = 'someText';

            $message = "";
            $extraMessage = "";

            #assert(array_key_exists('function', $_REQUEST));  # From example:  isset($this->records)
            #
            # We can't use assert as text.php is normally
            # invoked by GET, by a direct link.
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
                # Escape problem "fix" (ref. <https://stackoverflow.com/a/33604648>)
                # The problem is solely due to WordPress (we would't need if
                # it wasn't for the use of/integration into WordPress).
                #
                # "stripslashes_deep" is part of WordPress
                #
                $formDataSizeBefore = strlen($_REQUEST[MAINTEXT]);
                $_REQUEST = array_map('stripslashes_deep', $_REQUEST);
                $formDataSizeAfter = strlen($_REQUEST[MAINTEXT]);
                $formDataSizeDiff = $formDataSizeBefore - $formDataSizeAfter;

                # Some output to remind us that this WordPress madness
                # should be adressed some day
                if ($formDataSizeDiff > 0)
                {
                    $extraMessage =
                      $formDataSizeDiff .
                      " characters saved from WordPress madness...";
                }

                $someText = htmlentities($_REQUEST[MAINTEXT]);

                assert(array_key_exists('action', $_REQUEST));  # From example:  isset($this->records)

                # The default value could be 'Some text </textarea>', for
                # making direct HTML validation by http://validator.w3.org/
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
                ###$someText = $_REQUEST['someText']
                ###              ?? 'Some text </textarea>';
                ##$someText = htmlentities($_REQUEST['someText']
                ##                           ?? 'Some text </textarea>');
                #$someText = htmlentities($_REQUEST['someText']);


                #Remove at any time
                #$lengthBefore = strlen($someText);

                $actionStr = $_REQUEST['action'];

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

                        $replacer = new StringReplacerWithRegex($someText);

                        # Convert time to YouTube format
                        $replacer->transform('(\d+)\s+secs',   '$1 ');
                        $replacer->transform('(\d+)\s+min\s+', '$1:');
                        $replacer->transform('(\d+)\s+h\s+',   '$1:');

                        # Convert URLs so they do not look like URLs...
                        # (otherwise, the entire comment will be
                        # automatically removed by YouTube).
                        $replacer->transform('(\w)\.(\w)', '$1 DOT $2');
                        $replacer->transform('https:\/\/', ''         );
                        $replacer->transform('http:\/\/',  ''         );

                        # Reversals for some of the false positives
                        # in URL processing
                        $replacer->transform('E DOT g\.', 'E.g.');
                        $replacer->transform('e DOT g\.', 'e.g.');

                        # Convert email addresses like so... (at least to
                        # offer some protection (and avoiding objections
                        # to posting )).
                        #
                        # For now, just globally replace "@"
                        #
                        $replacer->transform('\@', ' AT ');

                        #This one does not seem to work...
                        # Convert "->" to a real arrow
                        #
                        # Note: For YouTube it can not be
                        #       the HTML entity, "&rarr;".
                        $replacer->transform('->', '→');

                        $someText = $replacer->currentString();
                        break;

                    case "Remove common leading space":

                        #echo '<h3>Lines...</h3>' . "\n";

                        $leadingSpaceToRemove = findCommonLeadingSpaces($someText);
                        $someText = removeCommonLeadingSpaces($someText, $leadingSpaceToRemove);
                        
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


            <!-- Submit button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp"
                class="XYZ3"
                value="Remove TABs and trailing whitespace"
                style="width:260px;"
                accesskey="U"
                title="Shortcut: Shift + Alt + U"
            />

            <!-- Submit button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp2"
                class="XYZ19"
                value="Format as keyboard"
                style="width:150px;"
                accesskey="B"
                title="Shortcut: Shift + Alt + B"
            />

            <!-- Submit button  -->
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

            <!-- Submit button  -->
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

            <!-- Submit button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp29"
                class="XYZ29"
                value="Transform for YouTube comments"
                style="width:240px;"
                accesskey="Y"
                title="Shortcut: Shift + Alt + Y"
            />

            <!-- Submit button  -->
            <input
                name="action"
                type="Submit"
                id="LookUp30"
                class="XYZ30"
                value="Remove common leading space"
                style="width:230px;"
                accesskey="X"
                title="Shortcut: Shift + Alt + X"
            />



        </form>

        <?php
            echo "$message $extraMessage";
        ?>

        <p>
            <!-- Note: PHP still works inside HTML comments, so we
                       need to use "#" to outcomment PHP lines.
                <?php
                    # phpinfo();
                ?>
            -->
        </p>

        <p>
            <a
                href="EditOverflow.php"
                accesskey="E"
                title="Shortcut: Shift + Alt + E"
            >Edit Overflow</a>.

            <a
                href="myInfo.php"
                accesskey="I"
                title="Shortcut: Shift + Alt + I"
            >Environment information</a>.

            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FText.php"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FText.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.
        </p>

        <p>Proudly and unapologetic powered by PHP!</p>


<?php include("commonEnd.php"); ?>

