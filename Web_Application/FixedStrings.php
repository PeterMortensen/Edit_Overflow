
<!-- Successor to FixedStrings.html, at least for now to get
     the WordPress styling. But perhaps we can instead
     move this (effectively) static page into the
     WordPress system?   -->


<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Fixed Strings");
        ?>


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

            <p>Arrow (<u>H</u>MTL):

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

            <p>True single quote (not the ASCII one):

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

            <p>True double quotes (not the ASCII ones):

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



            <!-- ******************************************************* -->
            <h2>Regular expressions for code checking</h2>

            <p>Note: As the false positive rate is high, every match
                     should be checked manually</p>


            <p>A<u>l</u>l combined
                  ("Missing space before {",
                  "Missing space after colon",
                  "Missing space after comma",
                  "Missing space around equal sign",
                  "Missing space around string concatenation (by \"+\")",
                  "Space before comma",
                  "Space before colon",
                  "Space before parenthesis",
                  and
                  "Space before semicolon"):

               <input
                   name="All combined"
                   type="text"
                   id="codeRegex_AllCombined"
                   class="XYZ36"
                   value="(\S\{|:\S|,\S|\S\=|\=\S|\S\+|\+\S|\s,|\s:|\s\)|\s;|\(\s)"
                   style="width:320px;"
                   accesskey="L"
                   title="Shortcut: Shift + Alt + L"
               />
            </p>




            <!-- Submit button  - it ought to be invisible!
            -->
            <!-- For 'value' (the displayed text in the button), tags 'u'
                 or 'strong' do not work!! -->
            <input
                name="XYZ"
                type="submit"
                id="NotReally"
                class="XYZ27"
                value="XXXXX"
                style="width:75px;"
                accesskey="S"
                title="Shortcut: Shift + Alt + S"
            />
        </form>

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
                paste to http://validator.w3.org/, under "Validate by
                direct input"

                But we now have a default value for the input, "js",
                so this validation actually works!
            -->
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FFixedStrings.html"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=http%3A%2F%2Fpmortensen.eu%2Fworld%2FFixedStrings.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.

        </p>


<?php include("commonEnd.php"); ?>

