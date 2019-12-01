
<?php

  # Future:
  #
  #   1. Eliminate/refactor $editSummary, etc. as we now use
  #      the structured list.
  #
  #      We should have some kind of regression test
  #      because we still use the variable
  #      $editSummary in an 'if' construct.
  #
?>


<?php include("commonStart.php"); ?>



<!-- Old
<!DOCTYPE html>
<html lang="en">
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">

        <title>Edit Overflow</title>

        <style>
            body
            {
                background-color: lightgrey;
            }

            .formgrid
            {
                display: grid;
                grid-template-columns: minmax(5%, 130px) 1em 2fr;
                                       /* 10% 1fr 2fr 12em;
                                          1fr 1em 2fr
                                       */
                grid-gap: 0.3em 0.6em;
                grid-auto-flow: row;
                align-items: center;
            }

            input,
            output,
            textarea,
            select,
            button
            {
                grid-column: 2 / 4;
                width: auto;
                margin: 0;
            }

            .formgrid > div
            {
                grid-column: 3 / 4;
                width: auto;
                margin: 0;
            }

            /* label, */
            input[type="checkbox"] + label,
            input[type="radio"]    + label
            {
                grid-column: 3 / 4;
                width: auto;
                padding: 0;
                margin: 0;
            }

            input[type="checkbox"],
            input[type="radio"]
            {
                grid-column: 2 / 3;
                justify-self: end;
                margin: 0;
            }

            label + textarea
            {
                align-self: start;
            }
        </style>
    </head>

    <body>
        <h1>(Note: PoC, to be styled to escape the 1990s...)</h1>
-->

        <h1>Edit Overflow v. 1.1.49a3 2019-11-28T193537</h1>

        <?php
            # For "Notice: Undefined variable: ..."
            error_reporting(E_ERROR | E_WARNING | E_PARSE | E_NOTICE);


            # Escape problem "fix" (ref. <https://stackoverflow.com/a/33604648>)
            # The problem is solely due to WordPress (we would't need it
            # if it wasn't for the use of/integration into WordPress).
            #
            # "stripslashes_deep" is part of WordPress
            #
            $_REQUEST = array_map('stripslashes_deep', $_REQUEST);


            # The only input field in the start HTML page
            #
            # The default value is for making direct HTML validation
            # by http://validator.w3.org/ work.
            #
            $lookUpTerm = $_REQUEST['LookUpTerm'] ?? 'js';

            #echo
            #    "<p>lookUpTerm through htmlentities(): " .
            #        htmlentities($lookUpTerm) .
            #    "<p>\n";
            #echo
            #    "<p>Raw lookUpTerm: " .
            #        $lookUpTerm .
            #    "<p>\n";


            # Avoid warning messages for an empty input (at the
            # expense of some confusion)
            if ($lookUpTerm == '')
            {
                $lookUpTerm = 'php';
            }

            # For these two, an empty string is the default / start
            # value and we don't require it to be set (e.g. from
            # the start HTML page).
            #
            $editSummary     = $_REQUEST['editSummary']      ?? ''; #To be eliminated
            $URLlist_encoded = $_REQUEST['URLlist_encoded']  ?? ''; # The structured way (that we actually use now)


            // echo "<p>Lookup term: $lookUpTerm</p>\n";

            // Prone to SQL injection attack (though the table is
            // effectively readonly - we overwrite it on a
            // regular basis)!
            $SQLprefix =
              " SELECT incorrectTerm, correctTerm, URL " .
              " FROM EditOverflow" .
              " WHERE incorrectTerm = "
              ;
            $CustomerSQL = $SQLprefix . "'" . $lookUpTerm . "'";

            # For debugging
            #echo "<p>CustomerSQL: >>>$CustomerSQL<<< </p>\n";


            # Note:
            #
            #  "charset=utf8" is required for e.g. lookup of "lets" to work -
            #  where 'correctTerm' contains the Quora quote character,
            #  "U+2019  E2 80 99  RIGHT SINGLE QUOTATION MARK".
            #
            #  If not, the resulting content of 'correctTerm' is
            #  "U+FFFD  EF BF BD  REPLACEMENT CHARACTER".
            #
            $pdo = new PDO(
              #'mysql:host=mysql19.unoeuro.com;dbname=pmortensen_eu_db',
              'mysql:host=mysql19.unoeuro.com;dbname=pmortensen_eu_db;charset=utf8',
              'pmortensen_eu',
              '__pw_toBeFilledInDuringDeployment_'); # Sample key: 2019-09-16

            if (0)
            {
                $statement = $pdo->query($CustomerSQL);

                # For debugging
                #$rows2 = $statement->fetchAll(PDO::FETCH_ASSOC);
                #foreach ($rows2 as $someRow)
                #{
                #    echo "<p> From fetch all: </p>" .
                #      " <p> >>>  "  . $someRow['incorrectTerm'] . " " .
                #      " (cleaned: " . htmlentities($someRow['incorrectTerm']) . ")</p> " .
                #
                #      " <p> >>>  " . $someRow['correctTerm'] .   " " .
                #      " (cleaned: " . htmlentities($someRow['correctTerm']) . ")</p> " .
                #
                #      " <p> >>>  " . $someRow['URL'] .           " " .
                #      " (cleaned: " . htmlentities($someRow['URL']) . ")</p> " .
                #
                #      " <p></p>";
                #}
            }
            else
            {
                echo "<p>No SQL injection, please...</p>\n";

                #Replace "name" with something else.
                $statement = $pdo->prepare($SQLprefix . ' :name');
                $statement->execute(array('name' => $lookUpTerm));
            }

            $row = $statement->fetch(PDO::FETCH_ASSOC);

            // echo htmlentities($row['correctTerm']);
            $incorrectTerm = htmlentities($row['incorrectTerm'], ENT_QUOTES);

            $correctTerm   = htmlentities($row['correctTerm'], ENT_QUOTES);
            #$correctTerm   = $row['correctTerm']; # Test for the Quora apostrofe problem -
                                                   # a term containing the U+FFFD
                                                   # REPLACEMENT CHARACTER will make
                                                   # $correctTerm an empty string...

            $URL           = htmlentities($row['URL']);

            if ($correctTerm)
            {
                # Add to our built-in up edit summary string (using the
                # carried-over state and our new lookup)

                # Later: more sophisticated, parsing $editSummary_encoded, etc.
                #To be eliminated
                $editSummary     = $editSummary . "<" . $URL . "> "; #Error: we get a leading space
                                                                     #       for the last item

                # The structured way (that we actually use now)
                $URLlist_encoded = $URLlist_encoded . "____" . $URL; #Note: we get a leading "____" for the
                                                                     #      first item
            }
            else
            {
                # Avoid "Undefined variable: editSummary_output", etc.
                # Or should we use simply a default value
                # instead (now redundancy with the below)?
                $editSummary_output  = "";
                $editSummary_output2 = "";
            }

            # At the end, as we want it completely blank. That is, only
            # the next lookup should be part of the checkin message.
            #if ($_REQUEST['resetState'])
            if (array_key_exists('resetState', $_REQUEST))
            {
                $editSummary = ""; #To be eliminated
                $URLlist_encoded = "";

                $editSummary_output  = "";
                $editSummary_output2 = "";
            }

            $items = preg_split('/____/', $URLlist_encoded);

            $items = array_filter($items); # Get rid of empty elements

            # Wrap each item in "<>" (URL encoded)
            $items = substr_replace($items, '&lt;', 0, 0);
            # $items = substr_replace($items, 'YYY&gt;ZZZ', 0, strlen($items));
            $items = preg_replace('/$/', '&gt;', $items);
            $elements = count($items);


            # It is simple for Stack Overflow - just separate
            # each item by a space
            $URLlist = implode(' ', $items);


            # For normal edit summary, outside Stack Overflow -
            # with Oxford comma!
            #
            $URLlist2 =
                join(', and ',
                     array_filter(
                         array_merge(
                             array(
                                 join(', ',
                                      array_slice($items, 0, -1))),
                                      array_slice($items, -1)), 'strlen'));

            #Adjust for two elements

            if ($elements == 2)
            {
                $URLlist2 = str_replace(', and', ' and', $URLlist2);
            }

            # echo "<p>URLlist: >>>$URLlist<<< <p>\n";
            # echo "<p>URLlist2: >>>$URLlist2<<< <p>\n";

            if ($editSummary) # "$editSummary" is to be eliminated and replaced
                              # with an equivalent if construct.
            {
                # Derived
                $editSummary_output = "Active reading [" . $URLlist . "].";

                $editSummary_output2 = "Copy edited (e.g. ref. $URLlist2).";

                ## Current compensation for not being sophisticated when generating
                ## a list...
                #$editSummary_output = str_replace(' ]', ']', $editSummary_output);
            }
        ?>

        <form
            name="XYZ"
            method="post"
            id="XYZ">

            <div class="formgrid">

                <!-- Conditional: Lookup failure -->
                <?php
                    # Lookup failure... Report it and offer links for
                    # look up on Wikipedia and Wiktionary.
                    #
                    if (!$correctTerm)
                    {
                        # These two are for proper indentation in the
                        # generated HTML source (by PHP).
                        $baseIndent        = "                ";
                        $EOL_andBaseIndent = "\n$baseIndent";

                        $startDivWithIndent =
                          "$EOL_andBaseIndent" .
                          "<div>" .
                          $EOL_andBaseIndent;
                        $endDivWithIndent =
                          "$EOL_andBaseIndent" .
                          "</div>" .
                          $EOL_andBaseIndent;


                        echo $startDivWithIndent;
                        echo
                          "$EOL_andBaseIndent" .
                          "<strong>Could not look up \"$lookUpTerm\"!" .
                          "</strong>" .
                          $EOL_andBaseIndent;
                        echo $endDivWithIndent;


                        echo $startDivWithIndent;

                        # Refactoring opportunity: some redundancy

                        # Provide a link to look up the term on Wikipedia
                        echo
                          "<a " .
                          "href=\"" .
                            "https://duckduckgo.com/html/?q=" .
                            "Wikipedia%20$lookUpTerm\"$EOL_andBaseIndent" .
                          "   accesskey=\"W\"$EOL_andBaseIndent" .
                          "   title=\"Shortcut: Shift + Alt + V\"$EOL_andBaseIndent" .
                          ">Look up on <strong>W</strong>ikipedia</a>" .
                          $EOL_andBaseIndent;

                        # Provide a link to look up the term on Wiktionary
                        echo
                          "<a " .
                          "href=\"" .
                            "https://duckduckgo.com/html/?q=" .
                            "Wiktionary%20$lookUpTerm\"$EOL_andBaseIndent" .
                          "   accesskey=\"K\"$EOL_andBaseIndent" .
                          "   title=\"Shortcut: Shift + Alt + K\"$EOL_andBaseIndent" .
                          ">Look up on Wi<strong>k</strong>tionary</a>";

                        echo $endDivWithIndent;

                        # Empty div, for first column in CSS grid
                        echo $startDivWithIndent;
                        echo $endDivWithIndent;

                        #No, not for now. But do consider it.
                        # In case of failure, blank out the usual
                        # (lookup result) items (to keep our mostly
                        # static HTML here in this PHP file), start.
                        # echo "\n$EOL_andBaseIndent" .
                        #    "<!--";
                    }
                ?>

                <label for="LookUpTerm"><u>L</u>ook up term</label>
                <input
                    name="LookUpTerm"
                    type="text"
                    id="LookUpTerm"
                    class="XYZ3"
                    <?php
                      echo "value=\"$lookUpTerm\"\n";
                    ?>
                    style="width:110px;"
                    accesskey="L"
                    title="Shortcut: Shift + Alt + L"
                    autofocus
                />

                <label for="CorrectedTerm"><u>C</u>orrected term</label>
                <input
                    name="CorrectedTerm"
                    type="text"
                    id="CorrectedTerm"
                    class="XYZ3"
                    <?php
                        # The extra trailing space in the output is for
                        # presuming the lookup terms contains a trailing
                        # space.
                        #
                        # This is only until we will use something more
                        # sophisticated (like in the Windows desktop
                        # version of Edit Overflow).

                        $itemValue = "$correctTerm ";
                        if (!$correctTerm)
                        {
                            $itemValue .= "..."; # To force the form input
                                                 # field to not be empty
                        }
                        echo "value=\"$itemValue\"\n";
                    ?>
                    style="width:110px;"
                    accesskey="C"
                    title="Shortcut: Shift + Alt + C"
                />

                <label for="editSummary_output">Checkin <u>m</u>essages</label>
                <input
                    name="editSummary_output"
                    type="text"
                    id="editSummary_output"
                    class="XYZ4"
                    <?php
                      echo "value=\"$editSummary_output\"\n";
                    ?>
                    style="width:600px;"
                    accesskey="M"
                    title="Shortcut: Shift + Alt + M"
                />

                <label for="editSummary_output2"><b></b></label>
                <input
                    name="editSummary_output2"
                    type="text"
                    id="editSummary_output2"
                    class="XYZ89"
                    <?php
                      echo "value=\"$editSummary_output2\"\n";
                    ?>
                    style="width:600px;"
                    accesskey="O"
                    title="Shortcut: Shift + Alt + O"
                />

                <label for="URL"><?php echo "<a href=\"$URL\">URL</a>" ?></label>
                <input
                    name="URL"
                    type="text"
                    id="URL"
                    class="XYZ3"
                    <?php
                      echo "value=\"$URL\"\n";
                    ?>
                    style="width:400px;"
                    accesskey="E"
                    title="Shortcut: Shift + Alt + E"
                />

                <?php
                    #No, not for now. But do consider it.
                    # At lookup failure, blank out the usual
                    # items (to keep our mostly static HTML),
                    # end.
                    if (!$correctTerm)
                    {
                        # echo "-->\n";
                    }
                ?>


                <!-- Hidden field, close to the output format for
                     the edit summary

                  Sample:

                    <http://en.wikipedia.org/wiki/HTML> <http://en.wikipedia.org/wiki/PHP>

                -->
                <!-- To be eliminated -->
                <input
                    name="editSummary"
                    type="hidden"
                    id="editSummary"
                    class="XYZ5"
                    <?php
                      echo "value=\"$editSummary\"\n";
                    ?>
                />


                <!-- Hidden field, structured format for the edit summary -->
                <input
                    name="URLlist_encoded"
                    type="hidden"
                    id="URLlist_encoded"
                    class="XYZ6"
                    <?php
                      echo "value=\"$URLlist_encoded\"\n";
                    ?>
                />


                <!-- Reset lookup / edit summary state  -->
                <input
                    name="resetState"
                    type="checkbox"
                    id="resetState"
                    class="XYZ9"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                />
                <label for="resetState"><u>R</u>eset lookup state</label>


                <!-- Submit button  -->
                <!-- For 'value' (the displayed text in the button), tags 'u'
                     or 'strong' do not work!! -->

                <!--
                    action="#"
                -->

                <input
                    action="EditOverflow.php"

                    name="XYZ"
                    type="submit"
                    id="LookUp"
                    class="XYZ3"
                    value="Look up"
                    style="width:75px;"
                    accesskey="U"
                    title="Shortcut: Shift + Alt + U"
                />

            </div>

        </form>

        <hr/>

        <p>
            <a
                href="Text.php"
                accesskey="J"
                title="Shortcut: Shift + Alt + J"
            >Text stuff</a>.

            <a
                href="Link_Builder.php"
                accesskey="B"
                title="Shortcut: Shift + Alt + B"
            >Link builder</a>.

            <a
                href="FixedStrings.php"
                accesskey="F"
                title="Shortcut: Shift + Alt + F"
            >Fixed strings</a>.

            <a
                href="EditSummaryFragments.php"
                accesskey="D"
                title="Shortcut: Shift + Alt + D"
            >Edit summary fragments</a>.


            <a
                href="http://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_2019-11-01.html"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >Word list</a>.


            <!--
              Note:

                This would not really work (we get a lot of strange errors -
                because of PHP warnings when certain form input is missing).
                A workaround is to use view source on a result and copy
                paste to http://validator.w3.org/, under "Validate by
                direct input"

                But we now have a default value for the input, "js",
                so this validation actually works!
            -->
            <a
                href="https://validator.w3.org/nu/?showsource=yes&amp;doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FEditOverflow.php"
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


