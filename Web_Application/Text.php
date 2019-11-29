
<?php include("commonStart.php"); ?>



<!-- Old
<!DOCTYPE html>

<html lang="en">
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">

        <title>Text stuff</title>

        <style>
            body
            {
                background-color: lightgrey;
            }
        </style>
    </head>

    <body>
        <h1>(Note: PoC, to be styled to escape the 1990s...)</h1>
-->


        <h1>Text stuff - Edit Overflow v. 1.1.49a3 2019-11-28T193537</h1>

        <?php
            require_once('StringReplacerWithRegex.php');

            # function assert()

            const MAINTEXT = 'someText';

            $message = "";


            #assert(array_key_exists('function', $_POST));  # From example:  isset($this->records)
            #
            # We can't use assert as text.php is normally
            # invoked by GET, by a direct link.
            #
            # Or perhaps we can distinguish between GET and POST? -
            #
            #Try to distinguish GET and POST. Is there an official way
            #to do this??
            $someText = "";
            if (array_key_exists(MAINTEXT, $_POST))
            {
                #echo "</p>Invoked by POST...</p>\n";
                $someText = htmlentities($_POST[MAINTEXT]);

                assert(array_key_exists('action', $_POST));  # From example:  isset($this->records)

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
                ###$someText = $_POST['someText']
                ###              ?? 'Some text </textarea>';
                ##$someText = htmlentities($_POST['someText']
                ##                           ?? 'Some text </textarea>');
                #$someText = htmlentities($_POST['someText']);


                # Remove TABs and trailing whitespace

                    ## This removes both empty line and trailing and leading whitespace...
                    #$someText = preg_replace('/^\s+|\s+$/m', '', $someText);

                    #Test
                    #$temp1 = '[ \t]+(\r?$)/m';
                    #$temp2 = preg_replace('[ \t]+', '$1', $someText);
                    #$temp3 = preg_replace('[a-z]+', '$1', $someText);

                    # $someText = "     Hello, World!  \r\n" .
                    #             "  Some other line\r\n"    .
                    #             "     The last line  "
                    #           ;
                    #
                    #$someText = "     This is sad...  \r"  .
                    #            "     ZZHello first  \r"   .
                    #            "     Hello again  \n"     .
                    #            "     Hello Worly  \r\n"   .
                    #            "     Hello, World!  \r\n" .
                    #            "  Some other line\r\n"    .
                    #            "     The last line  "
                    #          ;

                    ##$someText = preg_replace('#[ \t]+(\r?$)/m#', '$1', $someText);
                    #$someText = preg_replace('#[ \t]+(\r?$)/m#', '$1', $someText);
                    #$someText = preg_replace('#([^ \t\r\n])[ \t]+$#', '$1', $someText);
                    #$someText = preg_replace('#([^ \t\r\n])[ \t]+$#', '__XXXX__', $someText);

                    # Give syntax error "Warning: preg_replace() [function.preg-replace]: Unknown modifier '/' in /var/www/pmortensen.eu/public_html/world/Text.php on line 43"
                    #$someText = preg_replace('/([^ \t\r\n])[ \t]+$/m/', '$1', $someText);

                    ##$someText = preg_replace('#([^ \t\r\n])[ \t]+$/m#', '$1', $someText);
                    #$someText = preg_replace('#([^ \t\r\n])[ \t]+$#m', '$1', $someText);
                    #$someText = preg_replace('/([^ \t\r\n])[ \t]+$/',  '$1', $someText);
                    #$someText = preg_replace('/^([^ \t\r\n])[ \t]+/m', '$1', $someText);
                    #$someText = preg_replace('/([^ \t\r\n])[ \t]+$/m', '$1', $someText);
                    #$someText = preg_replace('/(?m)([^ \t\r\n])[ \t]+$/', '$1', $someText);
                    #$someText = preg_replace('/(?m)([^ \n])[ ]+$/', '$1', $someText);
                    #$someText = preg_replace('/(?m)([^ \n])[ ]+$/', '\1 FFFF_GGGG', $someText);

                    # Works!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    #$someText = preg_replace('/(?m)[ \t]+(\r?$)/', '$1', $someText);


                $lengthBefore = strlen($someText);

                #$actionStr = htmlentities($_POST['action']);
                $actionStr = $_POST['action'];
                #echo "<p>actionStr: $actionStr</p>";

                switch ($actionStr)
                {
                    case "Remove TABs and trailing whitespace":
                        #echo '<p>Actions for: Remove TABs and trailing whitespace</p>';

                        # This also works...
                        $someText = preg_replace('/[ \t]+(\r?$)/m', '$1', $someText);

                        $lengthAfter = strlen($someText);

                        $removedTrailingSpaces = $lengthBefore - $lengthAfter;

                        $someText = preg_replace('/\t/', '    ', $someText);
                        $lengthAfter2 = strlen($someText);

                        # 1 TAB results in 4 spaces (3 more characters)
                        $replacedTABs = ($lengthAfter2 - $lengthAfter) / 3; # What about rounding??

                        if ($lengthBefore>0)
                        {
                            $message =
                                "<p>" .
                                "Replaced $replacedTABs TABs with spaces " .
                                "and removed $removedTrailingSpaces trailing spaces..." .
                                "</p>\n";
                        }
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
                style="width:250px;"
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
                style="width:150px;"
                accesskey="Y"
                title="Shortcut: Shift + Alt + Y"
            />

        </form>

        <?php
            echo "$message";
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
                href="https://validator.w3.org/nu/?showsource=yes&amp;doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FText.php"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation</a>.
        </p>

        <p>Proudly and unapologetic powered by PHP!</p>


<!--
    </body>
</html>
-->


<?php # From WordPress...
    get_footer();
?>


