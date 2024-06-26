<?php
    # File: FixedStrings.php

?>


<!--  Successor to FixedStrings.html, at least for now to get
      the WordPress styling. But perhaps we can instead
      move this (effectively) static page into the
      WordPress system?
-->

<?php include("commonStart.php"); ?>

        <?php
            the_EditOverflowHeadline(
              "Fixed Strings",
              "FixedStrings.php",
              "",
              "");
        ?>

        <p><a href="https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references#Character_entity_references_in_HTML"
        >Character entity references in HTML</a></p>


        <form
            name="XYZ"
            method="post"
            action="EditOverflow.php"
            id="XYZ">

            <p>Menu for<u>m</u>atting:

                <input
                    name="Menu formatting"
                    type="text"
                    id="MenuFormatting"
                    class="XYZ29"
                    value="* &amp;rarr; *"
                    style="width:70px;"
                    accesskey="M"
                    title="Shortcut: Shift + Alt + M"
                />
            </p>

            <p>HTML non-brea<u>k</u>ing space:

                <input
                    name="Non-breaking space"
                    type="text"
                    id="Non-breaking_space"
                    class="XYZ32"
                    value="&amp;nbsp;"
                    style="width:50px;"
                    accesskey="K"
                    title="Shortcut: Shift + Alt + K"
                />
            </p>

            <p>HTML <u>b</u>reak:

                <!-- Yes, leading space in 'value' -->
                <input
                    name="Break"
                    type="text"
                    id="Break"
                    class="XYZ28"
                    value=" <br/>"
                    style="width:50px;"
                    accesskey="B"
                    title="Shortcut: Shift + Alt + B"
                />
            </p>

            <p>Back<u>t</u>ick:

                <input
                    name="Backtick"
                    type="text"
                    id="Backtick"
                    class="XYZ39"
                    value="`"
                    style="width:50px;"
                    accesskey="T"
                    title="Shortcut: Shift + Alt + T"
                />
            </p>

            <p>e<u>n</u> dash
                (<a href="https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references#Character_entity_references_in_HTML">ref</a>) -
                &ndash; (HTML):

                <input
                    name="en_dash"
                    type="text"
                    id="en_dash_HTML"
                    class="XYZ45"
                    value="&amp;ndash;"
                    style="width:60px;"
                    accesskey="N"
                    title="Shortcut: Shift + Alt + N"
                />
            </p>

            <p><a href="https://en.wiktionary.org/wiki/en_dash#Noun">en dash</a> -
                &ndash; (Unicode.
                <a href="https://www.utf8-chartable.de/unicode-utf8-table.pl?start=8192&number=128"
                >Code point 2013</a>):

                <input
                    name="en_dash_Unicode"
                    type="text"
                    id="en_dash_Unicode"
                    class="XYZ46"
                    value="–"
                    style="width:60px;"
                    accesskey=""
                    title=""
                />
            </p>

            <p>em dash
                (<a href="https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references#Character_entity_references_in_HTML">ref</a>) -
                &mdash; (HTM<u>L</u>):

                <input
                    name="em_dash"
                    type="text"
                    id="em_dash_HTML"
                    class="XYZ47"
                    value="&amp;mdash;"
                    style="width:60px;"
                    accesskey="L"
                    title="Shortcut: Shift + Alt + L"
                />
            </p>

            <p><a href="https://en.wiktionary.org/wiki/em_dash#Noun">em dash</a> -
                &mdash; (Unicode.
                <a href="https://www.utf8-chartable.de/unicode-utf8-table.pl?start=8192&number=128"
                >Code point 2014</a>):

                <input
                    name="em_dash_Unicode"
                    type="text"
                    id="em_dash_Unicode"
                    class="XYZ48"
                    value="—"
                    style="width:60px;"
                    accesskey=""
                    title=""
                />
            </p>

            <p>Arrow - &rarr; (<u>H</u>TML):

                <input
                    name="Arrow - HTML"
                    type="text"
                    id="Arrow_HTML"
                    class="XYZ30"
                    value="&amp;rarr;"
                    style="width:50px;"
                    accesskey="H"
                    title="Shortcut: Shift + Alt + H"
                />
            </p>

            <p>Subscri<u>p</u>t (HTML):

                <input
                    name="Subscript - HTML"
                    type="text"
                    id="Subscript_HTML"
                    class="XYZ31"
                    value="<sub></sub>"
                    style="width:100px;"
                    accesskey="P"
                    title="Shortcut: Shift + Alt + P"
                />
            </p>

            <p>Super<u>s</u>cript (HTML):

                <input
                    name="Superscript - HTML"
                    type="text"
                    id="Superscript_HTML"
                    class="XYZ41"
                    value="<sup></sup>"
                    style="width:100px;"
                    accesskey="S"
                    title="Shortcut: Shift + Alt + S"
                />
            </p>

            <p>Italics (HTML. Shortcut: <u>z</u>):

                <input
                    name="Italics - HTML"
                    type="text"
                    id="Italics_HTML"
                    class="XYZ50"
                    value="<em></em>"
                    style="width:80px;"
                    accesskey="Z"
                    title="Shortcut: Shift + Alt + Z"
                />
            </p>

            <p>Bold (HTML stron<u>g</u>):

                <input
                    name="Bold - HTML"
                    type="text"
                    id="Bold_HTML"
                    class="XYZ51"
                    value="<strong></strong>"
                    style="width:130px;"
                    accesskey="G"
                    title="Shortcut: Shift + Alt + G"
                />
            </p>

            <p><u>A</u>rrow (Unicode):

                <input
                    name="Arrow - Unicode"
                    type="text"
                    id="Arrow_Unicode"
                    class="XYZ42"
                    value="→"
                    style="width:30px;"
                    accesskey="A"
                    title="Shortcut: Shift + Alt + A"
                />
            </p>

            <p>Arro<u>w</u> (Unicode, alternative):

                <input
                    name="Arrow - Unicode - 2"
                    type="text"
                    id="Arrow_Unicode_2"
                    class="XYZ40"
                    value="►"
                    style="width:30px;"
                    accesskey=""
                    title=""
                />
            </p>

            <p>The
                <a href="https://en.wikipedia.org/wiki/Ohm"
                >ohm symbol</a>
                (Unic<u>o</u>de, e.g. for
                <a href="https://en.wikipedia.org/wiki/Electrical_resistance_and_conductance"
                >electrical resistance</a>):

                <input
                    name="Ohm symbol - Unicode"
                    type="text"
                    id="Ohm_Unicode"
                    class="XYZ33"
                    value="Ω"
                    style="width:30px;"
                    accesskey="O"
                    title="Shortcut: Shift + Alt + O"
                />
            </p>

            <p>The mi<u>c</u>ro symbol
                (Unicode, e.g. µs for microseconds):

                <input
                    name="Greek micro symbol - Unicode"
                    type="text"
                    id="Micro_Unicode"
                    class="XYZ35"
                    value="µ"
                    style="width:30px;"
                    accesskey="C"
                    title="Shortcut: Shift + Alt + C"
                />
            </p>

            <p>The <u>d</u>egrees symbol
                (Unicode, e.g. for temperature):

                <input
                    name="Degrees symbol - Unicode"
                    type="text"
                    id="Degrees_Unicode"
                    class="XYZ34"
                    value="°"
                    style="width:30px;"
                    accesskey="D"
                    title="Shortcut: Shift + Alt + D"
                />
            </p>

            <p>True single <u>q</u>uote (not the ASCII one):

                <input
                    name="True single quote"
                    type="text"
                    id="TrueSingleQuote_Unicode"
                    class="XYZ37"
                    value="’"
                    style="width:30px;"
                    accesskey="Q"
                    title="Shortcut: Shift + Alt + Q"
                />
            </p>

            <p>True do<u>u</u>ble quotes (not the ASCII ones):

                <input
                    name="True double quote"
                    type="text"
                    id="TrueDoubleQuote_Unicode"
                    class="XYZ38"
                    value="“”"
                    style="width:30px;"
                    accesskey="U"
                    title="Shortcut: Shift + Alt + U"
                />
            </p>

            <p><a href="https://en.wikipedia.org/wiki/Tilde">Tilde</a>
                (Unicod<u>e</u> code point 007E):

                <input
                    name="Tilde"
                    type="text"
                    id="Tilde_Unicode"
                    class="XYZ43"
                    value="~"
                    style="width:30px;"
                    accesskey="E"
                    title="Shortcut: Shift + Alt + E"
                />
            </p>

            <p><a href="https://en.wikipedia.org/wiki/Caret_(computing)">Caret</a>
                (Unicode code point 005E. '<u>r</u>'):

                <input
                    name="Caret"
                    type="text"
                    id="Caret_Unicode"
                    class="XYZ49"
                    value="^"
                    style="width:30px;"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                />
            </p>

            <p><a href="https://en.wikipedia.org/wiki/Interpunct">Middle dot</a> -
                &middot; (Un<u>i</u>code code point 00B7):

                <input
                    name="middot"
                    type="text"
                    id="middot_Unicode"
                    class="XYZ44"
                    value="&amp;middot;"
                    style="width:60px;"
                    accesskey="I"
                    title="Shortcut: Shift + Alt + I"
                />
            </p>

            <!--  ***
                    Note: Do ***not*** edit this part manually.
                    Instead, copy-paste from the machine-
                    generated file <EditOverflowList_latest.html>
                    (indent it by one unit):

                      * Copy the <h2> and two <p> parts.

                      * Indent them one indent unit

                      * Move the regular expression to the <input>'s
                        "value" attribute's content.

                      * Replace double quote with "&quot;"
                        in the regular expression.

                  *** -->

            <h2>Code <u>f</u>ormatting check</h2>

            <p>Note: It is rudimentary. As the false positive rate is high, every match should be checked manually.</p>

            <p>Regular expression
                ("missing space before <strong>{</strong>",
                "missing space after colon",
                "missing space after comma",
                "missing or too much space around equal sign",
                "missing space around string concatenation (by "<strong>+</strong>")",
                "space before comma",
                "space before colon",
                "space before right parenthesis",
                "space before semicolon",
                "space after left parenthesis",
                "missing space around some operators",
                "missing capitalisation in comment (Jon Skeet decree)",
                "missing space in comment (Jon Skeet decree)",
                and "single-line 'if' statements"): <br/>

                <!-- *** End of partly machine-generated insert *** -->

                <input
                    name="All combined"
                    type="text"
                    id="codeRegex_AllCombined"
                    class="XYZ36"
                    value="(\S\{|:\S|,\S|\S\=|\=\S|\s{2,}\=|\=\s{2,}|\S\+|\+\S|\s,|\s:|\s\)|\s;|\(\s|\S&amp;&amp;|&amp;&amp;\S|('|\&quot;|\)|(\$\w+\[.+\]))\.|\.['\&quot;\]]|(\/\/|\/\*|\#|&lt;!--)\s*\p{Ll}|(\/\/|\/\*|\#|&lt;!--)\S|\S(\/\/|\/\*|\#|&lt;!--)|\sif.*\).{3,})"
                    style="width:320px;"
                    accesskey="F"
                    title="Shortcut: Shift + Alt + F"
                />
            </p>


            <!--  Submit button  - it ought to be invisible!
            -->
            <!--  For 'value' (the displayed text in the button), tags 'u'
                  or 'strong' do not work!! -->

            <!-- Does field 'name' have any significance???   -->
            <input
                name="XYZ_7777"
                type="submit"
                id="NotReally"
                class="XYZ27"
                value="XXXXX"
                style="width:75px;"
                accesskey=""
                title=""
            />
        </form><?php the_EditOverflowFooter('FixedStrings.php', "", ""); ?>


            <!--
              Note:

                This would not really work (we get a lot of strange errors -
                because of PHP warnings when certain form input is missing).
                A workaround is to use view source on a result and
                copy-paste to https://validator.w3.org/, under
                "Validate by direct input".

                But we now have a default value for the input, "js",
                so this validation actually works!
            -->
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FFixedStrings.html"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FFixedStrings.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.

        </p>


<?php the_EditOverflowEnd() ?>

