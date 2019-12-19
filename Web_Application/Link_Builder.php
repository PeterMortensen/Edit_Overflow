
<?php

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
            $IDcounter = 1;
            $emphasisStr = "*";


            # For "Notice: Undefined variable: ..."
            error_reporting(E_ERROR | E_WARNING | E_PARSE | E_NOTICE);

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


            ## For normal edit summary, outside Stack Overflow -
            ## with Oxford comma!
            ##
            #$URLlist2 =
            #    join(', and ',
            #         array_filter(
            #             array_merge(
            #                 array(
            #                     join(', ',
            #                          array_slice($items, 0, -1))),
            #                          array_slice($items, -1)), 'strlen'));
            #
            ##Adjust for two elements
            #
            #if ($elements == 2)
            #{
            #    $URLlist2 = str_replace(', and', ' and', $URLlist2);
            #}

            # echo "<p>URLlist: >>>$URLlist<<< <p>\n";
            # echo "<p>URLlist2: >>>$URLlist2<<< <p>\n";


            #if ($editSummary) # "$editSummary" is to be eliminated and replaced
            #                  # with an equivalent if construct.
            #{
            #    # Derived
            #    $editSummary_output = "Active reading [" . $URLlist . "].";
            #
            #    $editSummary_output2 = "Copy edited (e.g. ref. $URLlist2).";
            #
            #    ## Current compensation for not being sophisticated when generating
            #    ## a list...
            #    #$editSummary_output = str_replace(' ]', ']', $editSummary_output);
            #}

        ?>

        <form
            name="LinkBuildForm"
            method="post"
            action="Link_Builder.php"
            id="XYZ">

            <p>Build links
               Lin<u>k</u> text:

                <input
                    name="LinkText"
                    type="text"
                    id="LinkText"
                    class="XYZ25"
                    <?php the_formValue($linkText); ?>
                    style="width:610px;"
                    accesskey="K"
                    title="Shortcut: Shift + Alt + K"
                />
            </p>

            <p><u>U</u>RL:

                <input
                    name="URL"
                    type="text"
                    id="URL"
                    class="XYZ02"
                    <?php the_formValue($URL); ?>
                    style="width:500px;"
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
            </p>


            <!-- **************************************************** -->
            <h2>Output</h2>

            <p>Short Markdown (tex<u>t</u> / re<u>f</u>erence):

                <input
                    name="ShortMark_part1"
                    type="text"
                    id="ShortMark_part1"
                    class="XX20"
                    <?php the_formValue($shortMark_part1); ?>
                    style="width:600px;"
                    accesskey="T"
                    title="Shortcut: Shift + Alt + T"
                />
                
                <input
                    name="ShortMark_part2"
                    type="text"
                    id="ShortMark_part2"
                    class="XX21"
                    <?php the_formValue($shortMark_part2); ?>
                    style="width:450px;"
                    accesskey="F"
                    title="Shortcut: Shift + Alt + F"
                />
            </p>

            <p>Inline <u>M</u>arkdown:

                <input
                    name="inlineMarkdown"
                    type="text"
                    id="inlineMarkdown"
                    class="XX22"
                    <?php the_formValue($inlineMarkdown); ?>
                    style="width:650px;"
                    accesskey="M"
                    title="Shortcut: Shift + Alt + M"
                />
            </p>


            <!-- Hidden field, close to the output format for
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
            <p><u>R</u>eset lookup state:
                <input
                    name="resetState"
                    type="checkbox"
                    id="resetState"
                    class="XYZ24"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                />
            </p>

            <!-- Submit button  -->
            <!-- For 'value' (the displayed text in the button), tags 'u'
                 or 'strong' do not work!! -->
            <input
                name="LookUp"
                type="submit"
                id="LookUp"
                class="XYZ03"
                value="Generate"
                style="width:75px;"
                accesskey="U"
                title="Shortcut: Shift + Alt + U"
            />
        </form>

        <hr/>

        <p>
            <a
                href="EditOverflow.php"
                accesskey="E"
                title="Shortcut: Shift + Alt + E"
            >Edit Overflow</a>.

            <a
                href="Text.php"
                accesskey="J"
                title="Shortcut: Shift + Alt + J"
            >Text stuff</a>.

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
                href="https://validator.w3.org/nu/?showsource=yes&doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FLink_Builder.php"
                accesskey="V"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FLink_Builder.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.
        </p>

        <p>Proudly and unapologetic powered by PHP!</p>



<?php include("commonEnd.php"); ?>
