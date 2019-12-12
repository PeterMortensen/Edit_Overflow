
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
     #
     #
     include("commonStart.php"); # Use headers supplied from WordPress, etc...

?>



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


                $lengthBefore = strlen($someText);
                $actionStr = $_REQUEST['action'];

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

                        # One TAB results in four spaces (three more characters)
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


