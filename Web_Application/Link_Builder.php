<?php
    # File: Link_Builder.php

    # ************************************************************************
    # *  Copyright (C) 2010 Peter Mortensen                                  *
    # *                                                                      *
    # *  Purpose: Implements the dialog to conveniently build links:         *
    # *           Markdown (both forms), Wikipedia links (both internal      *
    # *           and external), normal HTML links, etc.                     *
    # *                                                                      *
    # *           This way the user does not have to know the exact          *
    # *           syntax (and avoid having to type strange                   *
    # *           characters, like "[").                                     *
    # *                                                                      *
    # *           It is essentially a reimplementation of the dialog         *
    # *           "XXX" in the Windows version (menu item YYY -> "ZZZ"),     *
    # *           but this one in addition support accumulative use          *
    # *           (more than one link - e.g. to paste directly into          *
    # *           a comment on Stack Exchange).                              *
    # *                                                                      *
    # *                                                                      *
    # *  File: \Web_Application\Link_Builder.php                             *
    # *                                                                      *
    # *                                                                      *
    # *                                                                      *
    # *                                                                      *
    # ************************************************************************
?>

<!--
    Note:

      1. We can now use "OverflowStyle=Native" to avoid the WordPress overhead:

            <https://pmortensen.eu/world/Link_Builder.php?OverflowStyle=Native>

      2. Parameters are supported, including for GET:

            <https://pmortensen.eu/world/Link_Builder.php?LinkText=CPU&URL=https://en.wikipedia.org/wiki/Central_processing_unit&OverflowStyle=Native>

            <https://pmortensen.eu/world/Link_Builder.php?LinkText=CPU&URL=https://en.wikipedia.org/wiki/Central_processing_unit>



-->


<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Link Builder");
        ?>


        <?php
            # Stub / defaults
            #
            $shortMark_part1 = "Part 1";
            $shortMark_part2 = "Part 2";
            $inlineMarkdown = "Part 3";
            $link_HTML = "Part 4";
            $MediaWiki_link = "Part 5";
            $IDcounter = 1;
            $emphasisStr = "*";

            # The default value is for making direct HTML validation
            # by https://validator.w3.org/ work.
            #
            $linkText = get_postParameter('LinkText') ?? 'iron';

            # The default value is for making direct HTML validation
            # by https://validator.w3.org/ work.
            #
            $URL = get_postParameter('URL') ?? 'https://en.wikipedia.org/wiki/Iron';


            # Avoid warning messages for an empty input (at the
            # expense of some confusion)
            if ($linkText == '')
            {
                #$linkText = 'JavaScript';
                $linkText = 'iron'; # Conform with previous versions
            }

            # Conform with previous versions
            if ($URL == '')
            {
                $URL = 'https://en.wikipedia.org/wiki/Iron'; # Conform with previous versions
            }

            $IDcounter++;


            # Derived
            $IDref = "[$IDcounter]";
            $linkTextBracketed = "[$linkText]";


            # The core of this link building helper...
            #
            # Samples, for the 3 variables:
            #
            #   [Iron][7]
            #     [2]: https://en.wikipedia.org/wiki/Iron
            #   [Iron](https://en.wikipedia.org/wiki/Iron)
            #
            $shortMark_part1 = "$emphasisStr$linkTextBracketed$IDref$emphasisStr";
            $shortMark_part2 = "  $IDref: $URL";
            $inlineMarkdown = "$emphasisStr$linkTextBracketed($URL)$emphasisStr";


            # Note: Some redundancy with "EditOverflow.php"
            #
            # Link, in HTML format/syntax
            #
            #Note: We already have a utility function for this, get_HTMLlink(),
            #      but the escaping of double quotes is in doubt. We changed
            #      it from "&quot;" to "%22" (in get_HTMLattributeEscaped()).
            #
            #      That does not work in this case - we get the literal "%22"
            #      in the (user facing) output.
            #
            #      It ought to be resolved, so we don't have
            #      this redundancy
            #
            #$link_HTML = get_HTMLlink($correctTerm,
            #                          $URL,
            #                          "");
            #
            # Using double quotes in the form may result in "%22" due
            # to our own function, get_HTMLattributeEscaped().
            #
            $link_HTML =
              "<a href=\"" . $URL . "\"" .
              ">" . $linkText . "</a>";

            # MediaWiki link
            #
            # Note: Some characters could be better handled, especially
            #       parentheses. Example:
            #
            #     https://en.wikipedia.org/wiki/C_Sharp_%28programming_language%29
            #       C Sharp (programming language)
            #
            #     It would be better to convert the percentage encoding
            #     (%28 and %29) to "("/")" for the link text.
            #
            # Extract the MediaWiki link from the URL, including
            # converting underscores to space.
            #
            $MediaWiki_link = "";

            # Only if it is actually on Wikipedia
            if (1) # Stub
            {
                # $MediaWiki_link = preg_replace('/\*/', '', $URL);
                # $MediaWiki_link = substr_replace($URL, 'https://en.wikipedia.org/wiki/', 0, 0);

                $MediaWiki_firstPart = preg_replace('/https:\/\/en.wikipedia.org\/wiki\//', '', $URL);

                $MediaWiki_link = '[[' . $MediaWiki_firstPart . '|' . $linkText . ']]';
            }


            # Note: The rest, until the form, is not implemented yet.
            #
            #       BTW, why do we want an accumulative feature? Is it
            #       because we want to invoke link builder from a
            #       lookup page with multiple terms (so we don't
            #       have to manually invoke link builder for
            #       each term?).
            #
            #       That would cover the use case from 2020-06-25,
            #       linkyfying this for a talk page entry:
            #
            #          C#, VB.NET, F#
            #

            # For the accumulative feature (we remember past generated
            # link text/URL pairs)
            #
            # For these two, an empty string is the default / start
            # value and we don't require it to be set (e.g. from
            # the start HTML page).
            #
            $linkText_encoded = get_postParameter('URLlist_encoded')  ?? '';
            $URLlist_encoded = get_postParameter('URLlist_encoded')  ?? '';

            #$XXX = htmlentities(YYYY, ENT_QUOTES);
            #$YYY = htmlentities(YYYY, ENT_QUOTES);


            # At the end, as we want it completely blank. That is, only
            # the next lookup should be part of the checkin message.
            if (array_key_exists('resetState', $_REQUEST))
            {
                $linkText_encoded = "";
                $URLlist_encoded = "";

                $editSummary_output  = "";
                $editSummary_output2 = "";
            }

            $linkText_encoded = preg_split( '/____/', $linkText_encoded);
            $linkText_encoded = array_filter($linkText_encoded); # Get rid of empty elements

            $items_URLlist = preg_split( '/____/', $URLlist_encoded);
            $items_URLlist = array_filter($items_URLlist); # Get rid of empty elements


            # REFACTOR (for all of the below PHP code):

            # Wrap each item in "<>" (URL encoded)
            $linkText_encoded = substr_replace($linkText_encoded, '&lt;', 0, 0);
            $linkText_encoded = preg_replace('/$/', '&gt;', $linkText_encoded);
            $elements_linkText = count($linkText_encoded);

            # Wrap each item in "<>" (URL encoded)
            $items_URLlist = substr_replace($items_URLlist, '&lt;', 0, 0);
            $items_URLlist = preg_replace('/$/', '&gt;', $items_URLlist);
            $elements_URLlist = count($items_URLlist);


            # It is simple for Stack Overflow - just separate
            # each item by a space
            $URLlist = implode(' ', $items_URLlist);

        ?>

        <form
            name="LinkBuildForm"
            method="post"
            id="XYZ">

            <div class="formgrid">

                <label for="LinkText">Lin<u>k</u> text</label>
                <input
                    name="LinkText"
                    type="text"
                    id="LinkText"
                    class="XYZ25"
                    <?php the_formValue($linkText); ?>
                    style="width:200px;"
                    accesskey="K"
                    title="Shortcut: Shift + Alt + K"
                />

                <label for="URL"><u>U</u>RL</label>
                <input
                    name="URL"
                    type="text"
                    id="URL"
                    class="XYZ02"
                    <?php the_formValue($URL); ?>
                    style="width:650px;"
                    accesskey="U"
                    title="Shortcut: Shift + Alt + U"
                /><?php

                # Perhaps later: Add links / form fields for opening the provided URL.
                #if (!$correctTerm)
                #{
                #    $baseIndent        = "                ";
                #    $EOL_andBaseIndent = "\n$baseIndent";
                #    echo
                #      "\n$EOL_andBaseIndent" .
                #      "<strong>Could not look up \"$lookUpTerm\"!</strong>$EOL_andBaseIndent";
                #
                #    # Refactoring opportunity: some redundancy
                #
                #    # Provide a link to look up the term on Wikipedia
                #    echo
                #      "<a " .
                #      "href=\"" .
                #        "https://duckduckgo.com/html/?q=" .
                #        "Wikipedia%20$lookUpTerm\"$EOL_andBaseIndent" .
                #      "   accesskey=\"W\"$EOL_andBaseIndent" .
                #      "   title=\"Shortcut: Shift + Alt + V\"$EOL_andBaseIndent" .
                #      ">Look up on <strong>W</strong>ikipedia</a>$EOL_andBaseIndent";
                #
                #    # Provide a link to look up the term on Wiktionary
                #    echo
                #      "<a " .
                #      "href=\"" .
                #        "https://duckduckgo.com/html/?q=" .
                #        "Wiktionary%20$lookUpTerm\"$EOL_andBaseIndent" .
                #      "   accesskey=\"K\"$EOL_andBaseIndent" .
                #      "   title=\"Shortcut: Shift + Alt + K\"$EOL_andBaseIndent" .
                #      ">Look up on Wi<strong>k</strong>tionary</a>\n";
                #}
                ?>


                <!-- **************************************************** -->
                <h2>Output</h2>

                <p></p>

                <p></p>

                <label for="ShortMark_part1">Short Markdown, tex<u>t</u></label>
                <input
                    name="ShortMark_part1"
                    type="text"
                    id="ShortMark_part1"
                    class="XYZ20"
                    <?php the_formValue($shortMark_part1); ?>
                    style="width:200px;"
                    accesskey="T"
                    title="Shortcut: Shift + Alt + T"
                />

                <label for="ShortMark_part1">Short Markdown, re<u>f</u>erence</label>
                <input
                    name="ShortMark_part2"
                    type="text"
                    id="ShortMark_part2"
                    class="XYZ21"
                    <?php the_formValue($shortMark_part2); ?>
                    style="width:650px;"
                    accesskey="F"
                    title="Shortcut: Shift + Alt + F"
                />


                <label for="inlineMarkdown">Inline <u>M</u>arkdown</label>
                <input
                    name="inlineMarkdown"
                    type="text"
                    id="inlineMarkdown"
                    class="XYZ22"
                    <?php the_formValue($inlineMarkdown); ?>
                    style="width:650px;"
                    accesskey="M"
                    title="Shortcut: Shift + Alt + M"
                />


                <label for="HTML_link"><u>H</u>TML</label>
                <input
                    name="HTML_link"
                    type="text"
                    id="HTML_link"
                    class="XYZ26"
                    <?php
                        # Note: Some redundancy with "EditOverflow.php"

                        $link_HTML_encoded = str_replace('"', '&quot;', $link_HTML);
                        echo "value=\"$link_HTML_encoded\"\n";
                    ?>
                    style="width:650px;"
                    accesskey="H"
                    title="Shortcut: Shift + Alt + H"
                />

                <label for="MediaWiki_link">M<u>e</u>diaWiki</label>
                <input
                    name="MediaWiki_link"
                    type="text"
                    id="MediaWiki_link"
                    class="XYZ27"
                    <?php the_formValue($MediaWiki_link); ?>
                    style="width:650px;"
                    accesskey="E"
                    title="Shortcut: Shift + Alt + E"
                />



                <!--  Hidden field, close to the output format for
                      the edit summary

                  Sample:

                    <https://en.wikipedia.org/wiki/HTML> <https://en.wikipedia.org/wiki/PHP>

                -->


                <!-- Hidden field, structured format for the edit summary -->
                <!--
                <input
                    name="URLlist_encoded"
                    type="hidden"
                    id="URLlist_encoded"
                    class="XYZ23"
                    <?php the_formValue($URLlist_encoded); ?>
                />
                -->


                <!-- Reset lookup / edit summary state  -->

                <input
                    name="resetState"
                    type="checkbox"
                    id="resetState"
                    class="XYZ24"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                />
                <label for="resetState"><u>R</u>eset lookup state</label>  <!-- The order matters! -->


                <!--  Submit button  -->
                <!--  For 'value' (the displayed text in the button), tags 'u'
                      or 'strong' do not work!! -->
                <input
                    name="LookUp"
                    type="submit"
                    id="LookUp"
                    class="XYZ03"
                    value="Generate"
                    style="width:100px;"
                    accesskey="G"
                    title="Shortcut: Shift + Alt + G"
                />

            </div>

        </form><?php the_EditOverflowFooter('Link_Builder.php', $linkText, $URL); ?>


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
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FLink_Builder.php"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FLink_Builder.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.
        </p>

        <p>Proudly and unapologetic powered by PHP!</p>


<?php the_EditOverflowEnd() ?>

