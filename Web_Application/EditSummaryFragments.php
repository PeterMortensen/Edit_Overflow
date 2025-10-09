<?php
    # File: EditSummaryFragments.php

?>


<!--  Successor to EditSummaryFragments.html, at least for now
      to get the WordPress styling. But perhaps we can instead
      move this (effectively) static page into the
      WordPress system?

      Though we don't get things like a centralised place for
      the Edit Overflow version. Or other redundancy-reducing
      measures.
-->

<?php include("commonStart.php"); ?>

        <?php
            the_EditOverflowHeadline(
              "Some edit summary fragments",
              "EditSummaryFragments.php",
              "",
              "");
        ?>

        <p>Th<u>e</u>re is also a page
            <a
                href="https://pmortensen.eu/world/CannedComments.php"
                accesskey="E"
                title="Shortcut: Shift + Alt + E"
            >for canned comments.</a></p>

        <hr>

            <!--  It is not really a form - just a page where we want
                  to be able to tab between field (and )
            -->
        <form
            name="XYZ"
            method="post"
            action="EditOverflow.php"
            id="XYZ">

            <p>The "How to ... ?" <u>f</u>orm for a question is not a good one
                (does not follow
                <a href="https://www.youtube.com/watch?v=kS5NfSzXfrI"
                >QUASM</a>) <br>
                <input
                    name="X25"
                    type="text"
                    id="X25"
                    class="X25"
                    value="Fixed the question formation - missing auxiliary (or helping) verb - see e.g. <https://www.youtube.com/watch?v=t4yWEt0OSpg&t=1m49s> (see also <https://www.youtube.com/watch?v=kS5NfSzXfrI> (QUASM)) - alternatively, drop the question mark."
                    style="width:1700px;"
                    accesskey="F"
                    title="Shortcut: Shift + Alt + F"
                >
            </p>

            <p id="Capitalise_I">
                Capitalise "i" - yes, <u>y</u>ou!!!!! <br>
                <input
                    name="X2"
                    type="text"
                    id="X2"
                    class="X2"
                    value="In English, the subjective form of the singular first-person pronoun, &quot;I&quot;, is capitalized, along with all its contractions such as I'll and I'm. "
                    style="width:910px;"
                    accesskey="Y"
                    title="Shortcut: Shift + Alt + Y"
                >
            </p>

            <p>Small formatting changes often result in
                a large <u>d</u>iff in the default view. <br>
                <input
                    name="X7"
                    type="text"
                    id="X7"
                    class="X7"
                    value="Applied some formatting (as a result the diff looks more extensive than it really is - use view &quot;side-by-side Markdown&quot; to compare). "
                    style="width:820px;"
                    accesskey="D"
                    title="Shortcut: Shift + Alt + D"
                >
            </p>

            <p>Meta infor<u>m</u>ation does not belong
                in a question or an answer.
                See for example
                <em>
                <a href="https://meta.stackexchange.com/questions/2950/"
                >Should 'Hi', 'thanks', taglines, and salutations be removed from posts?</a>
                </em> <br>
                <input
                    name="X14"
                    type="text"
                    id="X14"
                    class="X14"
                    value="Removed meta information (this belongs in comments). "
                    style="width:400px;"
                    accesskey="M"
                    title="Shortcut: Shift + Alt + M"
                >
            </p>

            <p>Statements like "the above" may make sense
                on a f<u>o</u>rum, but
                definitely not on Stack Exchange. <br>
                <input
                    name="X17"
                    type="text"
                    id="X17"
                    class="X17"
                    value="(References to relative positions of answers are not reliable as they depend on the view (votes/oldest/active) and changing of the accepted answer and change over time (for votes, active, and accepted state)). "
                    style="width:1260px;"
                    accesskey="O"
                    title="Shortcut: Shift + Alt + O"
                >
            </p>

            <p>"<i>its"</i> vs. <i>"it's"</i> <br>
                <input
                    name="X19"
                    type="text"
                    id="X19"
                    class="X19"
                    value="[(its = possessive, it's = &quot;it is&quot; or &quot;it has&quot;. See for example <https://www.youtube.com/watch?v=8Gv0H-vPoDc&t=1m20s> and <https://www.wikihow.com/Use-Its-and-It%27s>.)] "
                    style="width:1220px;"
                    accesskey="X"
                    title=""
                >
            </p>

            <p>
                <input
                    name="X1"
                    type="text"
                    id="X1"
                    class="X1"
                    value="Active reading [<URL1> <URL2>]. "
                    style="width:240px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p>Links should<u>n</u>'t be naked -
                use an appropriate link text. <br>
                <input
                    name="X3"
                    type="text"
                    id="X3"
                    class="X3"
                    value="Dressed the naked link. "
                    style="width:170px;"
                    accesskey="N"
                    title="Shortcut: Shift + Alt + N"
                >
            </p>

            <p>
                <input
                    name="X4"
                    type="text"
                    id="X4"
                    class="X4"
                    value="Applied some formatting. "
                    style="width:170px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p>Many users post sloppy or even m<u>i</u>sleading
                indentation of code. <br>
                <input
                    name="X5"
                    type="text"
                    id="X5"
                    class="X5"
                    value="Fixed the indentation. "
                    style="width:150px;"
                    accesskey="I"
                    title="Shortcut: Shift + Alt + I"
                >
            </p>

            <p>Shorthands or TLAs often make <u>p</u>osts too obscure for
                the actual audience (from search engine hits). <br>
                <input
                    name="X6"
                    type="text"
                    id="X6"
                    class="X6"
                    value="Expanded. "
                    style="width:90px;"
                    accesskey="P"
                    title="Shortcut: Shift + Alt + P"
                >
            </p>

            <p>Inappropriate shortening of "IP address"
                (IP addresses are bloc<u>k</u>ed, not
                the protocol). <br>
                <input
                    name="X13"
                    type="text"
                    id="X13"
                    class="X13"
                    value="IP is a protocol; it is IP addresses that are static, filtered, blocked, assigned, bound, fetched, accessed, resolved, checked, banned, tracked, detected, dynamic, grabbed, scanned, whitelisted, have different representations, that devices have, etc., not the protocol itself. "
                    style="width:1600px;"
                    accesskey="K"
                    title="Shortcut: Shift + Alt + K"
                >
            </p>

            <p>Capita<u>l</u>isation often obscures
                the meaning when proper nouns are involved. <br>
                <input
                    name="X15"
                    type="text"
                    id="X15"
                    class="X15"
                    value="Changed to sentence casing for the title. "
                    style="width:280px;"
                    accesskey="L"
                    title="Shortcut: Shift + Alt + L"
                >
            </p>

            <p>Code formattin<u>g</u> should
                not be used for emphasis. <br>
                <input
                    name="X16"
                    type="text"
                    id="X16"
                    class="X16"
                    value="Used more standard formatting. "
                    style="width:220px;"
                    accesskey="G"
                    title="Shortcut: Shift + Alt + G"
                >
            </p>

            <p>Unfortunately, Stack Exchange does not
                warn when text is
                enclosed in "&lt;>". It is just ignored and becomes
                invisible without warning. <br>
                <input
                    name="X18"
                    type="text"
                    id="X18"
                    class="X18"
                    value="Unhid &quot;<XXX>&quot; by encoding &quot;<&quot; as &quot;&amp;lt;&quot; (see the original revision (<https://stackoverflow.com/XXX/view-source>) - or use view &quot;side-by-side Markdown&quot;). "
                    style="width:1050px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p>Things like "Update", etc. do not
                <u>b</u>elong in the main post
                (question or answer). A post's current version should be
                the best possible and there isn't any need to know how it
                changed over time - this information should be in edit
                summaries and in the revision history (in some cases, and/or
                in comments), but definitely not in the main post. Remember,
                99% of the users for a post comes from a Google search, and
                they have no interest whatsoever how a post changed over time -
                they just need the best content (besides, it is ugly to have
                "Update" in a post). <br>
                <input
                    name="X20"
                    type="text"
                    id="X20"
                    class="X20"
                    value="Removed historical information (that is what the revision history is for)—the question/answer should be as if it was written right now; see e.g. <https://meta.stackexchange.com/a/131011>. "
                    style="width:960px;"
                    accesskey="B"
                    title="Shortcut: Shift + Alt + B"
                >
            </p>

            <p>It is not necessary to capitalise any words for emphasis.
                Both Stack Exchange, Wikipedia, Quora, and even YouTube
                (though very poorly documented - see
                <a href="https://pmortensen.eu/world2/2020/03/22/bold-and-italics-in-youtube-comments/"
                >my blog post about how to reliably style YouTube comments</a>)
                have facilities for <b>bold</b> and <i>italics</i>.
                It is mostly Indians who do this, but not at all exclusively.
                Anyone who has wasted too much time with (raw text-only)
                forums will have the tendency (and need to unlearn). <br>
                <input
                    name="X27"
                    type="text"
                    id="X27"
                    class="X27"
                    value="Used more standard formatting (we have italics and bold on this platform). "
                    style="width:520px;"
                    accesskey=""
                    title=""
                >
            </p>

            <!-- Or should this be in the pure links section? -->
            <p>Spelling of Stack Overflow, Stack&nbsp;Exchange,
                and other Stack&nbsp;Exchange sites, etc.
                No matter how it looks in a logo,
                <a href="https://policies.stackoverflow.co/company/trademark-guidance/#h1-2de2438a74fa0"
                >it is "Stack&nbsp;Overflow" and "Stack&nbsp;Exchange"</a>
                (the last section -
                <em>"Proper Use of the Stack Exchange Name"</em>).
                See also
                <em>
                <a href="https://stackoverflow.design/brand/copywriting/naming/"
                >Naming guidelines</a>
                </em> on the "Stacks"
                <a href="https://en.wikipedia.org/wiki/Design_system"
                >design system</a>.
                The <strong><em>only</em></strong> exception is "MathOverflow"
                (and Jeff Atwood should have said <strong><em>no</em></strong>
                at the time).

                <br>
                <input
                    name="X28"
                    type="text"
                    id="X28"
                    class="X28"
                    value="<https://policies.stackoverflow.co/company/trademark-guidance/#h1-2de2438a74fa0> (the last section) "
                    style="width:500px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p>Rhetorical questions do not belong in an answer. One of the
                reasons is that an answer should not address the OP - most
                readers (e.g., 99.9999%) will be someone else. <br>
                <input
                    name="X29"
                    type="text"
                    id="X29"
                    class="X29"
                    value="More affirmative answer (this is not a forum - see e.g. <https://meta.stackexchange.com/a/92115>. It is a think tank (see <https://meta.stackoverflow.com/a/325681>). "
                    style="width:1120px;"
                    accesskey=""
                    title=""
                >
            </p>

            <!-- Note:  We can not use <pre>, as it introduces 
                        line breaks. -->
            <p id="FixedSyntaxHighlighting">Most older posts
                on Stack Overflow have strange
                <a href="https://meta.stackexchange.com/questions/184108/what-is-syntax-highlighting-and-how-does-it-wok/184109#184109"
                >syntax highlighting</a>,
                probably caused by
                <a href="https://meta.stackexchange.com/questions/353983/goodbye-prettify-hello-highlight-js-swapping-out-our-syntax-highlighter"
                >later changes </a>
                to the Stack Exchange software.
                Possible code fencing formatting (note:
                <a href="https://meta.stackexchange.com/questions/403526/syntax-highlighting-when-using-a-lang-alias-and-some-main-language-codes"
                >broken in late 2024</a>. See also 
                <a href="https://meta.stackoverflow.com/questions/432152/syntax-highlighting-is-broken-renders-as-lang-none-despite-a-correct-lang-p"
                >this meta post</a>
                (<a href="https://stackoverflow.com/help/whats-meta"
                >MSO</a>)):
                    ```lang-none,
                    ```lang-html,
                    ```lang-php,
                    ```lang-javascript,
                    ```lang-css,
                    ```lang-java,
                    ```lang-csharp,
                    ```lang-vbnet,
                    ```lang-python,
                    ```lang-xml,
                    ```lang-bash,
                    ```lang-typescript,
                    ```lang-yaml (Dockerfile), 
                    and
                    ```lang-kotlin (.kts)
                    <br>
                <input
                    name="X31"
                    type="text"
                    id="X31"
                    class="X31"
                    value="Fixed the weird syntax highlighting (as a result, the diff looks more extensive than it really is - use view &quot;Side-by-side Markdown&quot; to compare). "
                    style="width:870px;"
                    accesskey=""
                    title=""
                >
            </p>


            <!-- ===================================================== -->
            <hr>
            <h2 id="PureLinks">Pure links</h2>

            <p id="Run-on_sentences"><a href="https://en.wikipedia.org/wiki/Sentence_clause_structure#Run-on_sentences"
                >Run-on sentences</a>
                (<a href="https://twitter.com/PeterMortensen/status/1199839973215739907"
                >YouTube video</a>).
                Mostly native speake<u>r</u>s (more pronounced for US native
                speakers), but also for minimum-effort users: <br>
                <input
                    name="X26"
                    type="text"
                    id="X26"
                    class="X26"
                    value="<https://en.wikipedia.org/wiki/Sentence_clause_structure#Run-on_sentences> (see also <https://twitter.com/PeterMortensen/status/1199839973215739907>) "
                    style="width:590px;"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                >
            </p>

            <p>Missing <u>a</u>rticles, Russians and Indians,
                <a href="https://www.youtube.com/watch?v=1Dax90QyXgI&t=17m54s"
                >&quot;a&quot;</a>: <br>
                <input
                    name="X10"
                    type="text"
                    id="X10"
                    class="X10"
                    value="<https://www.youtube.com/watch?v=1Dax90QyXgI&t=17m54s> "
                    style="width:410px;"
                    accesskey="A"
                    title="Shortcut: Shift + Alt + A"
                >
            </p>

            <p>Missing ar<u>t</u>icles, Russians and Indians,
                <a href="https://www.youtube.com/watch?v=1Dax90QyXgI&t=19m05s"
                >&quot;the&quot;</a>: <br>
                <input
                    name="X11"
                    type="text"
                    id="X11"
                    class="X11"
                    value="<https://www.youtube.com/watch?v=1Dax90QyXgI&t=19m05s> "
                    style="width:410px;"
                    accesskey="T"
                    title="Shortcut: Shift + Alt + T"
                >
            </p>

            <div id="IndianSpace"> </div>
            <p><a href="https://english.stackexchange.com/questions/4645/is-it-ever-correct-to-have-a-space-before-a-question-or-exclamation-mark#comment206109_4645"
                >The Indian space</a>
                (from the outdated 1935
                <a href="https://en.wikipedia.org/wiki/Wren_%26_Martin"
                ><i>Wren & Martin</i> textbook</a>)&mdash;keyboard
                shortcut hint: <u>c</u>: <br>
                <input
                    name="X9"
                    type="text"
                    id="X9"
                    class="X9"
                    value="<https://english.stackexchange.com/questions/4645/is-it-ever-correct-to-have-a-space-before-a-question-or-exclamation-mark#comment206109_4645> "
                    style="width:910px;"
                    accesskey="C"
                    title="Shortcut: Shift + Alt + C"
                >
            </p>

            <p id="Than-and-Then"><u>U</u>S native speakers, especially southern US,
                <a href="https://www.wikihow.com/Use-Than-and-Then"
                >&quot;than&quot; vs. &quot;then&quot;</a>: <br>
                <input
                    name="X8"
                    type="text"
                    id="X8"
                    class="X8"
                    value="<https://www.wikihow.com/Use-Than-and-Then> "
                    style="width:310px;"
                    accesskey="U"
                    title="Shortcut: Shift + Alt + U"
                >
            </p>

            <p>US native speakers, especially southern US,
                <a href="https://www.wikihow.com/Use-You%27re-and-Your"
                >&quot;you're&quot; vs. &quot;your&quot;</a>: <br>
                <input
                    name="X22"
                    type="text"
                    id="X22"
                    class="X22"
                    value="<https://www.wikihow.com/Use-You%27re-and-Your> "
                    style="width:340px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p>US native speakers, especially southern US,
                <a href="https://www.wikihow.com/Use-There,-Their-and-They%27re"
                >&quot;there&quot; vs.
                &quot;their&quot; vs.
                &quot;they're&quot;</a>: <br>
                <input
                    name="X23"
                    type="text"
                    id="X23"
                    class="X23"
                    value="<https://www.wikihow.com/Use-There,-Their-and-They%27re> "
                    style="width:400px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p>US native speakers, especially southern US,
                <a href="https://www.wikihow.com/Use-Affect-and-Effect-Properly"
                >&quot;affect&quot; vs.
                &quot;effect&quot;</a>: <br>
                <input
                    name="X24"
                    type="text"
                    id="X24"
                    class="X24"
                    value="<https://www.wikihow.com/Use-Affect-and-Effect-Properly> "
                    style="width:380px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p>US native speakers, especially southern US,
                <a href="https://www.wikihow.com/Use-Where,-Were-and-We're"
                >&quot;where&quot; vs.
                &quot;were&quot;vs.
                &quot;we're&quot;</a>: <br>
                <input
                    name="X30"
                    type="text"
                    id="X30"
                    class="X30"
                    value="<https://www.wikihow.com/Use-Where,-Were-and-We're> "
                    style="width:380px;"
                    accesskey=""
                    title=""
                >
            </p>

            <p><a href="https://english.stackexchange.com/questions/15953"
                >Space between</a>
                <u>q</u>uantity and unit: <br>
                <input
                    name="X12"
                    type="text"
                    id="X12"
                    class="X12"
                    value="<https://english.stackexchange.com/questions/15953> "
                    style="width:350px;"
                    accesskey="Q"
                    title="Shortcut: Shift + Alt + Q"
                >
            </p>

            <p><a href="https://www.youtube.com/watch?v=1Dax90QyXgI&t=0m38s"
                >Present simple tense, third person singular</a>
                - t<u>h</u>ere must be an 's': <br>
                <input
                    name="X21"
                    type="text"
                    id="X21"
                    class="X21"
                    value="<https://www.youtube.com/watch?v=1Dax90QyXgI&t=0m38s> "
                    style="width:410px;"
                    accesskey="H"
                    title="Shortcut: Shift + Alt + H"
                >
            </p>


            <!-- Template
            <p>Some text with <u>Z</u>s <br>

                <input
                    name="X"
                    type="text"
                    id="X"
                    class=""
                    value="<template>"
                    style="width:110px;"
                    accesskey="Z"
                    title="Shortcut: Shift + Alt + Z"
                >
            </p>
            -->


            <!--
                  Submit button  - it ought to be invisible!
            -->
            <!--  For 'value' (the displayed text in the button), tags 'u'
                  or 'strong' do not work!! -->
            <input
                name="XYZ3"
                type="submit"
                id="NotReally"
                class="XYZ3"
                value="XX"
                style="width:75px;"
                accesskey=""
                title=""
            >
        </form><?php the_EditOverflowFooter('EditSummaryFragments.php', "", ""); ?>


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
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FEditSummaryFragments.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.

        </p>


<?php the_EditOverflowEnd() ?>

