
<!-- Successor to EditSummaryFragments.html, at least for now
     to get the WordPress styling. But perhaps we can instead
     move this (effectively) static page into the
     WordPress system?

     Though we don't get things like a centralised place for
     the Edit Overflow version. Or other redundancy-reducing
     measures.
-->

<!--
    Note:

      We can now use "OverflowStyle=Native" to avoid the WordPress overhead:

        <https://pmortensen.eu/world/EditSummaryFragments.php?OverflowStyle=Native>

-->


<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Some edit summary fragments");
        ?>

        <form
            name="XYZ"
            method="post"
            action="EditOverflow.php"
            id="XYZ">

            <p>The "How to ... ?" form for a question is not a good one
               (does not follow
               <a href="https://www.youtube.com/watch?v=kS5NfSzXfrI"
               >QUASM</a>)<br/>
               <input
                   name="X25"
                   type="text"
                   id="X25"
                   class=""
                   value="Fixed the question formation - missing auxiliary (or helping) verb - see e.g. <https://www.youtube.com/watch?v=t4yWEt0OSpg&t=1m49s> (see also <https://www.youtube.com/watch?v=kS5NfSzXfrI> (QUASM)). "
                   style="width:680px;"
                   accesskey="F"
                   title="Shortcut: Shift + Alt + F"
               />
            </p>

            <p>
               <input
                   name="X2"
                   type="text"
                   id="X2"
                   class=""
                   value="In English, the subjective form of the singular first-person pronoun, &quot;I&quot;, is capitalized, along with all its contractions such as I'll and I'm. "
                   style="width:810px;"
                   accesskey="Y"
                   title="Shortcut: Shift + Alt + Y"
               />
            </p>

            <p>
               <input
                   name="X7"
                   type="text"
                   id="X7"
                   class=""
                   value="Applied some formatting (as a result the diff looks more extensive than it really is - use view &quot;side-by-side markdown&quot; to compare). "
                   style="width:780px;"
                   accesskey="D"
                   title="Shortcut: Shift + Alt + D"
               />
            </p>

            <p>Meta infor<strong><i>m</i></strong>ation does not
               belong in a question <br/>
               <input
                   name="X14"
                   type="text"
                   id="X14"
                   class=""
                   value="Removed meta information (this belongs in comments). "
                   style="width:360px;"
                   accesskey="M"
                   title="Shortcut: Shift + Alt + M"
               />
            </p>

            <p>Statements like "the above" may make sense
               on a f<strong><i>o</i></strong>rum, but
               definitely not on Stack Exchange. <br/>
               <input
                   name="X17"
                   type="text"
                   id="X17"
                   class=""
                   value="(References to relative positions of answers are not reliable as they depend on the view (votes/oldest/active) and changing of the accepted answer and change over time (for votes, active, and accepted state)). "
                   style="width:1260px;"
                   accesskey="O"
                   title="Shortcut: Shift + Alt + O"
               />
            </p>

            <p>"<i>its"</i> vs. <i>"it's"</i><br/>
               <input
                   name="X19"
                   type="text"
                   id="X19"
                   class=""
                   value="[(its = possessive, it's = &quot;it is&quot; or &quot;it has&quot;. See for example <https://www.youtube.com/watch?v=8Gv0H-vPoDc&t=1m20s> and <https://www.wikihow.com/Use-Its-and-It%27s>.)] "
                   style="width:680px;"
                   accesskey="X"
                   title="Shortcut: Shift + Alt + X"
               />
            </p>




            <p>
               <input
                   name="X1"
                   type="text"
                   id="X1"
                   class=""
                   value="Active reading [<URL1> <URL2>]. "
                   style="width:230px;"
                   accesskey="Z"
                   title="Shortcut: Shift + Alt + Z"
               />
            </p>

            <p>
               <input
                   name="X3"
                   type="text"
                   id="X3"
                   class=""
                   value="Dressed the naked link. "
                   style="width:170px;"
                   accesskey="N"
                   title="Shortcut: Shift + Alt + N"
               />
            </p>

            <p>
               <input
                   name="X4"
                   type="text"
                   id="X4"
                   class=""
                   value="Applied some formatting. "
                   style="width:170px;"
                   accesskey="Z"
                   title="Shortcut: Shift + Alt + F"
               />
            </p>

            <p>
               <input
                   name="X5"
                   type="text"
                   id="X5"
                   class=""
                   value="Fixed the indentation. "
                   style="width:150px;"
                   accesskey="I"
                   title="Shortcut: Shift + Alt + I"
               />
            </p>

            <p>
               <input
                   name="X6"
                   type="text"
                   id="X6"
                   class=""
                   value="Expanded. "
                   style="width:90px;"
                   accesskey="P"
                   title="Shortcut: Shift + Alt + P"
               />
            </p>

            <p>Inappropriate shortening of "IP address" <br/>

               <input
                   name="X13"
                   type="text"
                   id="X13"
                   class=""
                   value="IP is a protocol; it is IP addresses that are static, filtered, blocked, assigned, bound, fetched, accessed, resolved, checked, banned, tracked, detected, dynamic, grabbed, scanned, whitelisted, have different representations, that devices have, etc., not the protocol itself. "
                   style="width:1600px;"
                   accesskey="K"
                   title="Shortcut: Shift + Alt + K"
               />
            </p>

            <p>Capita<strong><i>l</i></strong>isation often obscures
               the meaning when proper nouns are involved <br/>

               <input
                   name="X15"
                   type="text"
                   id="X15"
                   class=""
                   value="Changed to sentence casing for the title. "
                   style="width:270px;"
                   accesskey="L"
                   title="Shortcut: Shift + Alt + L"
               />
            </p>

            <p>Code formattin<strong><i>g</i></strong> should
               not be used for emphasis. <br/>

               <input
                   name="X16"
                   type="text"
                   id="X16"
                   class=""
                   value="Used more standard formatting. "
                   style="width:220px;"
                   accesskey="G"
                   title="Shortcut: Shift + Alt + G"
               />
            </p>

            <p>Unfortunately, Stack Exchange does not
               <strong><i>w</i></strong>arn when text is
               enclosed in "&lt;>". It is just ignored and becomes
               invisible without warning. <br/>
               <input
                   name="X18"
                   type="text"
                   id="X18"
                   class=""
                   value="Unhid &quot;<XXX>&quot; by encoding &quot;<&quot; as &quot;&amp;lt;&quot; (see the original revision (<https://stackoverflow.com/XXX/view-source>) - or use view &quot;side-by-side markdown&quot;). "
                   style="width:980px;"
                   accesskey="W"
                   title="Shortcut: Shift + Alt + W"
               />
            </p>

            <p>Things like "Update", etc. does not
               <strong><i>b</i></strong>elong in the main post
               (question or answer). A post's current version should be
               the best possible and there isn't any need to know how it
               changed over time - this information should be in edit
               summaries and in the revision history (in some cases, and/or
               in comments), but definitely not in the main post. Remember,
               99% of the users for a post comes from a Google search, and
               they have no interest whatsoever how a post changed over time -
               they just need the best content (besides, it is ugly to have
               "Update" in a post). <br/>
               <input
                   name="X20"
                   type="text"
                   id="X20"
                   class=""
                   value="Removed historical information (e.g. ref. <https://meta.stackexchange.com/a/230693> and <https://meta.stackexchange.com/a/127655>). "
                   style="width:830px;"
                   accesskey="B"
                   title="Shortcut: Shift + Alt + B"
               />
            </p>

            <p>It is not necessary to capitalise any words for emphasis.
               Both Stack Exchange, Wikipedia, Quora, and even YouTube
               (though very poorly documented) have facilities
               for <b>bold</b> and <i>italics</i>. 
               It is mostly Indians that do this, but not at all exclusively.
               Anyone who has wasted too much time with (raw text-only)
               forums will have the tendency (and need to unlearn).

               <br/>
               <input
                   name="X27"
                   type="text"
                   id="X27"
                   class=""
                   value="Used more standard formatting (we have italics and bold on this platform). "
                   style="width:830px;"
                   accesskey="X"
                   title="Shortcut: Shift + Alt + X"
               />
            </p>


            <!-- ===================================================== -->
            <hr/>
            <h2>Pure links</h2>

            <p><a href="https://en.wikipedia.org/wiki/Sentence_clause_structure#Run-on_sentences"
               >Run-on sentences</a>
               (<a href="https://twitter.com/PeterMortensen/status/1199839973215739907"
               >YouTube video</a>).
               Mostly native speakers (more pronounced for US native
               speakers), but also for minimum-effort users: <br/>

               <input
                   name="X26"
                   type="text"
                   id="X26"
                   class=""
                   value="<https://en.wikipedia.org/wiki/Sentence_clause_structure#Run-on_sentences> (see also <https://twitter.com/PeterMortensen/status/1199839973215739907>) "
                   style="width:590px;"
                   accesskey="R"
                   title="Shortcut: Shift + Alt + R"
               />
            </p>

            <p>Missing <u>a</u>rticles, Russians and Indians,
               <a href="https://www.youtube.com/watch?v=1Dax90QyXgI&t=17m54s"
               >&quot;a&quot;</a>: <br/>

               <input
                   name="X10"
                   type="text"
                   id="X10"
                   class=""
                   value="<https://www.youtube.com/watch?v=1Dax90QyXgI&t=17m54s> "
                   style="width:410px;"
                   accesskey="A"
                   title="Shortcut: Shift + Alt + A"
               />
            </p>

            <p>Missing ar<u>t</u>icles, Russians and Indians,
               <a href="https://www.youtube.com/watch?v=1Dax90QyXgI&t=19m05s"
               >&quot;the&quot;</a>: <br/>

               <input
                   name="X11"
                   type="text"
                   id="X11"
                   class=""
                   value="<https://www.youtube.com/watch?v=1Dax90QyXgI&t=19m05s> "
                   style="width:410px;"
                   accesskey="T"
                   title="Shortcut: Shift + Alt + T"
               />
            </p>

            <p><a href="https://english.stackexchange.com/questions/4645/is-it-ever-correct-to-have-a-space-before-a-question-or-exclamation-mark#comment206109_4645"
               >The Indian spa<strong><i>c</i></strong>e</a>
                (from the outdated 1935
                <a href="https://en.wikipedia.org/wiki/Wren_%26_Martin"
                ><i>Wren & Martin</i> textbook</a>): <br/>

               <input
                   name="X9"
                   type="text"
                   id="X9"
                   class=""
                   value="<https://english.stackexchange.com/questions/4645/is-it-ever-correct-to-have-a-space-before-a-question-or-exclamation-mark#comment206109_4645> "
                   style="width:910px;"
                   accesskey="C"
                   title="Shortcut: Shift + Alt + C"
               />
            </p>

            <p><u>U</u>S native speakers, especially southern US,
               <a href="https://www.wikihow.com/Use-Than-and-Then"
               >&quot;than&quot; vs. &quot;then&quot;</a>: <br/>

               <input
                   name="X8"
                   type="text"
                   id="X8"
                   class=""
                   value="<https://www.wikihow.com/Use-Than-and-Then> "
                   style="width:310px;"
                   accesskey="U"
                   title="Shortcut: Shift + Alt + U"
               />
            </p>

            <p>US native speakers, especially southern US,
               <a href="https://www.wikihow.com/Use-You%27re-and-Your"
               >&quot;you're&quot; vs. &quot;your&quot;</a>: <br/>

               <input
                   name="X22"
                   type="text"
                   id="X22"
                   class=""
                   value="<https://www.wikihow.com/Use-You%27re-and-Your> "
                   style="width:340px;"
                   accesskey="Z"
                   title="Shortcut: Shift + Alt + Z"
               />
            </p>

            <p>US native speakers, especially southern US,
               <a href="https://www.wikihow.com/Use-There,-Their-and-They%27re"
               >&quot;there&quot; vs.
                &quot;their&quot; vs.
                &quot;they're&quot;</a>: <br/>

               <input
                   name="X23"
                   type="text"
                   id="X23"
                   class=""
                   value="<https://www.wikihow.com/Use-There,-Their-and-They%27re> "
                   style="width:400px;"
                   accesskey="Z"
                   title="Shortcut: Shift + Alt + Z"
               />
            </p>

            <p>US native speakers, especially southern US,
               <a href="https://www.wikihow.com/Use-Affect-and-Effect-Properly"
               >&quot;affect&quot; vs.
                &quot;effect&quot;</a>: <br/>

               <input
                   name="X24"
                   type="text"
                   id="X24"
                   class=""
                   value="<https://www.wikihow.com/Use-Affect-and-Effect-Properly> "
                   style="width:380px;"
                   accesskey="Z"
                   title="Shortcut: Shift + Alt + Z"
               />
            </p>

            <p><a href="https://english.stackexchange.com/questions/15953"
               >Space between <strong><i>q</i></strong>uantity and unit</a>: <br/>

               <input
                   name="X12"
                   type="text"
                   id="X12"
                   class=""
                   value="<https://english.stackexchange.com/questions/15953> "
                   style="width:350px;"
                   accesskey="Q"
                   title="Shortcut: Shift + Alt + Q"
               />
            </p>

            <p><a href="https://www.youtube.com/watch?v=1Dax90QyXgI&t=0m38s"
               >Present simple tense, t<strong><i>h</i></strong>ird person</a>
               - there must be an 's': <br/>

               <input
                   name="X21"
                   type="text"
                   id="X21"
                   class=""
                   value="<https://www.youtube.com/watch?v=1Dax90QyXgI&t=0m38s> "
                   style="width:410px;"
                   accesskey="H"
                   title="Shortcut: Shift + Alt + H"
               />
            </p>


            <!-- Template
            <p>
               <input
                   name="X"
                   type="text"
                   id="X"
                   class=""
                   value="XXX. "
                   style="width:110px;"
                   accesskey="Z"
                   title="Shortcut: Shift + Alt + Z"
               />
            </p>
            -->


            <!--
                Submit button  - it ought to be invisible!
            -->
            <!-- For 'value' (the displayed text in the button), tags 'u'
                 or 'strong' do not work!! -->
            <input
                name="XYZ3"
                type="submit"
                id="NotReally"
                class="XYZ3"
                value="XX"
                style="width:75px;"
                accesskey="S"
                title="Shortcut: Shift + Alt + S"
            />
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
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FEditSummaryFragments.php"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FEditSummaryFragments.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.

        </p>


<?php include("commonEnd.php"); ?>


