
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

<!DOCTYPE html>
<html lang="en">
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">

        <title>Edit Overflow</title>

        <style>
            body {
                background-color: lightgrey;
            }
        </style>
    </head>

    <body>
        <h1>(Note: PoC, to be styled to escape the 1990s...)</h1>

        <h1>Edit Overflow v. 1.1.XX 2019-07-06</h1>

        <?php
            # For "Notice: Undefined variable: ..."
            error_reporting(E_ERROR | E_WARNING | E_PARSE | E_NOTICE);

            # The only input field in the start HTML page
            #
            # The default value is for making direct HTML validation
            # by http://validator.w3.org/ work.
            #
            $lookUpTerm = $_POST['LookUpTerm'] ?? 'js';

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
            $editSummary     = $_POST['editSummary']      ?? ''; #To be eliminated
            $URLlist_encoded = $_POST['URLlist_encoded']  ?? ''; # The structured way (that we actually use now)


            // echo "<p>Lookup term: $lookUpTerm</p>\n";

            // Prone to SQL injection attack (though the table is effectively readon-on - we overwrite)!
            $SQLprefix =
              " SELECT incorrectTerm, correctTerm, URL " .
              " FROM EditOverflow" .
              " WHERE incorrectTerm = ";
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
              'DetTagerAltForLangTidAtFindeEtJobIDanmark88888xt99999');


            if (0)
            {
                $statement = $pdo->query($CustomerSQL);

                # For debugging
                #$rows2 = $statement->fetchAll(PDO::FETCH_ASSOC);
                #foreach ($rows2 as $someRow) {
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
                # Later: more sophisticated, parsing $editSummary_encoded, etc.
                #To be eliminated
                $editSummary     = $editSummary . "<" . $URL . "> "; #Error: we get a leading space
                                                                     #       for the last item

                # The structured way (that we actually use now)
                $URLlist_encoded = $URLlist_encoded . "____" . $URL; #Note: we get a leading "____" for the
                                                                     #      first item
            }

            # At the end, as we want it completely blank. That is, only
            # the next lookup should be part of the checkin message.
            #if ($_POST['resetState'])
            if (array_key_exists('resetState', $_POST))
            {
                $editSummary = ""; #To be eliminated
                $URLlist_encoded = "";

                $editSummary_output  = "";
                $editSummary_output2 = "";
            }

            $items = preg_split( '/____/', $URLlist_encoded);

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
            action="EditOverflow.php"
            id="XYZ">

            <p><u>L</u>ook up term:

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
               />
            </p>

            <p><u>C</u>orrected term:

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

                        echo "value=\"$correctTerm \"\n";
                    ?>
                    style="width:110px;"
                    accesskey="C"
                    title="Shortcut: Shift + Alt + C"
                /><?php
                    if (!$correctTerm)
                    {
                        $baseIndent        = "                ";
                        $EOL_andBaseIndent = "\n$baseIndent";
                        echo
                          "\n$EOL_andBaseIndent" .
                          "<strong>Could not look up \"$lookUpTerm\"!</strong>$EOL_andBaseIndent";

                        # Refactoring opportunity: some redundancy

                        # Provide a link to look up the term on Wikipedia
                        echo
                          "<a " .
                          "href=\"" .
                            "https://duckduckgo.com/html/?q=" .
                            "Wikipedia%20$lookUpTerm\"$EOL_andBaseIndent" .
                          "   accesskey=\"W\"$EOL_andBaseIndent" .
                          "   title=\"Shortcut: Shift + Alt + V\"$EOL_andBaseIndent" .
                          ">Look up on <strong>W</strong>ikipedia</a>$EOL_andBaseIndent";
                          
                        # Provide a link to look up the term on Wiktionary
                        echo
                          "<a " .
                          "href=\"" .
                            "https://duckduckgo.com/html/?q=" .
                            "Wiktionary%20$lookUpTerm\"$EOL_andBaseIndent" .
                          "   accesskey=\"K\"$EOL_andBaseIndent" .
                          "   title=\"Shortcut: Shift + Alt + K\"$EOL_andBaseIndent" .
                          ">Look up on Wi<strong>k</strong>tionary</a>\n";
                    }
                ?>
            </p>

            <p>Checkin <u>m</u>essages:

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
            </p>

            <p><?php echo "<a href=\"$URL\">URL</a>:" ?>
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
            </p>


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
            <p><u>R</u>eset lookup state:
                <input
                    name="resetState"
                    type="checkbox"
                    id="resetState"
                    class="XYZ9"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                />
            </p>

            <!-- Submit button  -->
            <!-- For 'value' (the displayed text in the button), tags 'u'
                 or 'strong' do not work!! -->
            <input
                name="XYZ"
                type="submit"
                id="LookUp"
                class="XYZ3"
                value="Look up"
                style="width:75px;"
                accesskey="U"
                title="Shortcut: Shift + Alt + U"
            />
        </form>

        <p>
          <a
              href="Text.php"
              accesskey="T"
              title="Shortcut: Shift + Alt + T"
          >Text stuff</a>

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

    </body>
</html>

