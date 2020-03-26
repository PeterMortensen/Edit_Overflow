<?php
    # File: EditOverflow.php

    # Future:
    #
    #   1. Eliminate/refactor $editSummary, etc. as we now use
    #      the structured list.
    #
    #      We should have some kind of regression test
    #      because we still use the variable
    #      $editSummary in an 'if' construct.
?>

<!--
    Note:

      We can now use "OverflowStyle=Native" to avoid the WordPress overhead:

        <https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu&OverflowStyle=Native>
-->



<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Look up");
        ?>


        <?php
            # For "Notice: Undefined variable: ..."
            error_reporting(E_ERROR | E_WARNING | E_PARSE | E_NOTICE);

            require_once('deploymentSpecific.php');


            # These two are for proper indentation in the
            # generated HTML source (by PHP).
            $headerLevelIndent = "        ";
            $baseIndent        = "$headerLevelIndent        ";
            $EOL_andBaseIndent = "\n$baseIndent";


            # The only input field in the start HTML page
            #
            # The default value is for making direct HTML validation
            # by https://validator.w3.org/ work.
            #
            $lookUpTerm = get_postParameter('LookUpTerm') ?? 'js';

            #echo
            #    "<p>lookUpTerm through htmlentities(): " .
            #        htmlentities($lookUpTerm) .
            #    "<p>\n";
            #echo
            #    "<p>Raw lookUpTerm: " .
            #        $lookUpTerm .
            #    "<p>\n";


            # Some filtering, e.g. for content copied from a Quora editing
            # window.
            {
                #This corresponds to the filtering done in the
                #Windows Forms/C# application,
                #setSearchFieldFromClipboard(),
                #file </Dot_NET/OverflowHelper/OverflowHelper/Source/GUI/Forms/frmMainForm.cs>.
                #
                #
                #Perhaps we can eliminate this redundancy by having a
                #set of rules as data?

                # If bold in a Quora editing

                # "*" is sometimes included when copying
                # from Quora (when the content is in a
                # list item and when it is in bold
                # (two "*"s)).
                #$lookUpTerm = substr_replace('*', '', 0, 0);
                $lookUpTerm = preg_replace('/\*/', '', $lookUpTerm);

                #For more complicated replaces (regular expression)
                #$lookUpTerm = preg_replace('/$/', '&gt;', $lookUpTerm);
            }


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
            # To be eliminated
            $editSummary     = get_postParameter('editSummary')     ?? '';
            #
            # The structured way (that we actually use now)
            $URLlist_encoded = get_postParameter('URLlist_encoded') ?? '';


            // echo "<p>Lookup term: $lookUpTerm</p>\n";

            $SQLprefix =
              " SELECT incorrectTerm, correctTerm, URL " .
              " FROM EditOverflow" .
              " WHERE incorrectTerm = "
              ;

            $pdo = connectToDatabase();

            if (0)
            {
                //Obsolete - delete at any time.
            
                // Prone to SQL injection attack (though the table 
                // is effectively readonly - we overwrite it on a
                // regular basis)!
                $CustomerSQL = $SQLprefix . "'" . $lookUpTerm . "'";

                # For debugging
                #echo "<p>CustomerSQL: xxx" . $CustomerSQL . "xxx </p>\n";

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
                echo "$headerLevelIndent<p>No SQL injection, please...</p>\n";

                #Replace "name" with something else.
                $statement = $pdo->prepare($SQLprefix . ' :name');
                $statement->execute(array('name' => $lookUpTerm));
            }

            $row = $statement->fetch(PDO::FETCH_ASSOC);

            // echo htmlentities($row['correctTerm']);
            $incorrectTerm = htmlentities($row['incorrectTerm'], ENT_QUOTES);

            $correctTerm  = htmlentities($row['correctTerm'], ENT_QUOTES);
            #$correctTerm  = $row['correctTerm']; # Test for the Quora apostrofe problem -
                                                  # a term containing the U+FFFD
                                                  # REPLACEMENT CHARACTER will make
                                                  # $correctTerm an empty string...

            $URL          = htmlentities($row['URL']);

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

                $linkInlineMarkdown = "[$correctTerm]($URL)";
                # $linkInlineMarkdown = "";
            }
            else
            {
                # Avoid "Undefined variable: editSummary_output", etc.
                # Or should we simply use a default value
                # instead (now redundancy with the below)?
                $editSummary_output  = "";
                $editSummary_output2 = "";

                $linkInlineMarkdown = "";
            }

            # At the end, as we want it completely blank. That is, only
            # the next lookup should be part of the checkin message.
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

            # Revert: Adjust for two elements (no comma)
            if ($elements == 2)
            {
                $URLlist2 = str_replace(', and', ' and', $URLlist2);
            }

            #echo "<p>URLlist: xxx"  . $URLlist  . "xxx <p>\n";
            #echo "<p>URLlist2: xxx" . $URLlist2 . "xxx <p>\n";

            if ($editSummary) # "$editSummary" is to be eliminated and replaced
                              # with an equivalent 'if' construct.
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
                        #echo
                        #  "<a " .
                        #  "href=\"" .
                        #    "https://duckduckgo.com/html/?q=" .
                        #    "Wikipedia%20$lookUpTerm\"$EOL_andBaseIndent" .
                        #  "   accesskey=\"W\"$EOL_andBaseIndent" .
                        #  "   title=\"Shortcut: Shift + Alt + V\"$EOL_andBaseIndent" .
                        #  ">Look up on <strong>W</strong>ikipedia</a>" .
                        #  $EOL_andBaseIndent;

                        $linkText = "Look up on <strong>W</strong>ikipedia";

                        $URL = "https://duckduckgo.com/html/?q=" .
                               "Wikipedia%20$lookUpTerm"
                               ;
                        $extraAttributes =
                          $EOL_andBaseIndent .
                          "   accesskey=\"W\"$EOL_andBaseIndent" .
                          "   title=\"Shortcut: Shift + Alt + V\"" . $EOL_andBaseIndent
                          ;

                        $linkPart = get_HTMLlink($linkText,
                                                 $URL,
                                                 $extraAttributes);

                        echo $linkPart . $EOL_andBaseIndent;

                        # Provide a link to look up the term on Wiktionary
                        #echo
                        #  "<a " .
                        #  "href=\"" .
                        #    "https://duckduckgo.com/html/?q=" .
                        #    "Wiktionary%20$lookUpTerm\"$EOL_andBaseIndent" .
                        #  "   accesskey=\"K\"$EOL_andBaseIndent" .
                        #  "   title=\"Shortcut: Shift + Alt + K\"$EOL_andBaseIndent" .
                        #  ">Look up on Wi<strong>k</strong>tionary</a>";

                        echo
                          get_HTMLlink(
                              "Look up on Wi<strong>k</strong>tionary",

                              "https://duckduckgo.com/html/?q=" .
                                "Wiktionary%20$lookUpTerm",

                              $EOL_andBaseIndent .
                              "   accesskey=\"K\"$EOL_andBaseIndent" .
                              "   title=\"Shortcut: Shift + Alt + K\"" .
                              $EOL_andBaseIndent
                          ) .
                          $EOL_andBaseIndent;


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
                    } # Failed lookup
                ?>

                <label for="LookUpTerm"><u>L</u>ook up term</label>
                <input
                    name="LookUpTerm"
                    type="text"
                    id="LookUpTerm"
                    class="XYZ3"
                    <?php the_formValue($lookUpTerm); ?>
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
                    class="XYZ10"
                    <?php
                        # The extra trailing space in the output is for
                        # presuming the lookup terms contains a trailing
                        # space.
                        #
                        # This is only until we will use something more
                        # sophisticated (like in the Windows desktop
                        # version of Edit Overflow) - where we preserve
                        # any leading and trailing white space.

                        $itemValue = "$correctTerm ";
                        if (!$correctTerm)
                        {
                            $itemValue .= "..."; # To force the form input
                                                 # field to not be empty
                        }
                        the_formValue($itemValue);
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
                    <?php the_formValue($editSummary_output); ?>
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
                    <?php the_formValue($editSummary_output2); ?>
                    style="width:600px;"
                    accesskey="O"
                    title="Shortcut: Shift + Alt + O"
                />

                <label for="URL"><?php echo get_HTMLlink("URL", $URL, "") ?></label>
                <input
                    name="URL"
                    type="text"
                    id="URL"
                    class="XYZ11"
                    <?php the_formValue($URL); ?>
                    style="width:400px;"
                    accesskey="E"
                    title="Shortcut: Shift + Alt + E"
                />

                <label for="URL2"><b></b></label>
                <input
                    name="URL2"
                    type="text"
                    id="URL2"
                    class="XYZ90"
                    <?php the_formValue($linkInlineMarkdown); ?>

                    style="width:400px;"
                    accesskey="I"
                    title="Shortcut: Shift + Alt + I"
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

                    <https://en.wikipedia.org/wiki/HTML> <https://en.wikipedia.org/wiki/PHP>

                -->
                <!-- To be eliminated -->
                <input
                    name="editSummary"
                    type="hidden"
                    id="editSummary"
                    class="XYZ5"
                    <?php the_formValue($editSummary); ?>
                />


                <!-- Hidden field, structured format for the edit summary -->
                <input
                    name="URLlist_encoded"
                    type="hidden"
                    id="URLlist_encoded"
                    class="XYZ6"
                    <?php the_formValue($URLlist_encoded); ?>
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
                    name="XYZ"
                    type="submit"
                    id="LookUp"
                    class="XYZ12"
                    value="Look up"
                    style="width:90px;"
                    accesskey="U"
                    title="Shortcut: Shift + Alt + U"
                />

            </div>

        </form><?php the_EditOverflowFooter(); ?>


            <!--
              Note:

                This would not really work (we get a lot of strange errors -
                because of PHP warnings when certain form input is missing).
                A workaround is to use view source on a result and copy
                paste to https://validator.w3.org/, under "Validate by
                direct input"

                But we now have a default value for the input, "js",
                so this validation actually works!
            -->
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FEditOverflow.php"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FEditOverflow.php%3FLookUpTerm=don%27t%26OverflowStyle%3DNative"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.
        </p>

        <p>Proudly and unapologetic powered by PHP!</p>


<?php include("commonEnd.php"); ?>


