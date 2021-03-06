<?php
    # File: FixedStrings.php

?>


<!--  Successor to FixedStrings.html, at least for now to get
      the WordPress styling. But perhaps we can instead
      move this (effectively) static page into the
      WordPress system?
-->


<!--
    Note:

      We can now use "OverflowStyle=Native" to avoid the WordPress overhead:

        <https://pmortensen.eu/world/FixedStrings.php?OverflowStyle=Native>

-->


<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Fixed Strings");
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

            <p>Arrow - &rarr; (<u>H</u>MTL):

                <input
                    name="Arrow - HMTL"
                    type="text"
                    id="Arrow_HMTL"
                    class="XYZ30"
                    value="&amp;rarr;"
                    style="width:50px;"
                    accesskey="H"
                    title="Shortcut: Shift + Alt + H"
                />
            </p>

            <p><u>A</u>rrow (Unicode):

                <input
                    name="Arrow - Unicode"
                    type="text"
                    id="Arrow_Unicode"
                    class="XYZ31"
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
                ><u>o</u>hm symbol</a>
                (Unicode, e.g. for
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

            <p><a href="https://en.wikipedia.org/wiki/Tilde">Ti<u>l</u>de</a> (Unicode code point 007E):

                <input
                    name="Tilde"
                    type="text"
                    id="Tilde_Unicode"
                    class="XYZ39"
                    value="~"
                    style="width:30px;"
                    accesskey="L"
                    title="Shortcut: Shift + Alt + L"
                />
            </p>

            <p><a href="https://en.wikipedia.org/wiki/Interpunct">Middle dot</a> - &middot; (Unicode code point 00B7):

                <input
                    name="middot"
                    type="text"
                    id="middot_Unicode"
                    class="XYZ40"
                    value="&amp;middot;"
                    style="width:60px;"
                    accesskey="X"
                    title="Shortcut: Shift + Alt + X"
                />
            </p>


            <!-- ******************************************************* -->

            <h2>Code formatting check</h2>

            <p>Note: It is rudimentary. As the false positive rate is high, every match should be checked manually.</p>

            <p>Regular expression
                ("missing space before <strong>{</strong>",
                "missing space after colon",
                "missing space after comma",
                "missing space around equal sign",
                "missing space around string concatenation (by "<strong>+</strong>")",
                "space before comma",
                "space before colon",
                "space before right parenthesis",
                "space before semicolon",
                "space after left parenthesis",
                and "missing space around some operators"): <br/>

                

                <input
                    name="All combined"
                    type="text"
                    id="codeRegex_AllCombined"
                    class="XYZ36"
                    value="(\S\{|:\S|,\S|\S\=|\=\S|\S\+|\+\S|\s,|\s:|\s\)|\s;|\(\s|\S&&|&&\S|('|\&quot;|(\$\w+\[.+\]))\.|\.['\&quot;\]])"
                    style="width:320px;"
                    accesskey=""
                    title=""
                />

            </p>



            <!--  Submit button  - it ought to be invisible!
            -->
            <!--  For 'value' (the displayed text in the button), tags 'u'
                  or 'strong' do not work!! -->
            <input
                name="XYZ"
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

