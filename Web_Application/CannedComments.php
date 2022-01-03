<?php
    # File: CannedComments.php

?>


<!--
    Note:

      We can now use "OverflowStyle=Native" to avoid the WordPress overhead:

        <https://pmortensen.eu/world/CannedComments.php?OverflowStyle=Native>

-->


<?php include("commonStart.php"); ?>


        <?php
            the_EditOverflowHeadline("Some canned comments");
        ?>

        <!--  It is not really a form - just a page where we want
              to be able to tab between fields (and automatically
              selected so it is easy to copy from).
        -->

        <p>Note: This is for guidance only. The canned comments should not be used in an overly generic fashion. They should be tailored to each post.</p>

        <p> Links in the canned comments are in Markdown inline format
            (thus can be used without modication on
            <a href="https://en.wikipedia.org/wiki/Stack_Exchange">Stack&nbsp;Exchange</a>
            and
            <a href="https://odysee.com/">LBRY/Odysee</a>.</p>

        <hr/>

        <!---
        <ul>
            <li>ABV</li>
            <li>XYZ</li>
            <li>GHI</li>
            <li>MNO</li>
        </ul>
        -->


        <form
            name="XYZ"
            method="post"
            action="EditOverflow.php"
            id="XYZ">


            <p>For pr<strong><em>e</em></strong>emptive use:
                Responses to comments should normally be by editing the post,
                not in comments (and it should not contain meta
                information).
                Note: The ID in the link must be set.
                (Note:  this is not a full canned comments, only a fragment.
                        It should be combined)
                <br/>
                <input
                    name="X32"
                    type="text"
                    id="X32"
                    class="X32"
                    value="Please respond by [editing (changing) your question/answer](https://XXXX), not here in comments (***without*** &quot;Edit:&quot;, &quot;Update:&quot;, or similar - the question/answer should appear as if it was written today). "
                    style="width:830px;"
                    accesskey="E"
                    title="Shortcut: Shift + Alt + E"
                />
            </p>

            <p id="CodeOnlyComment">
                <strong><em>C</em></strong>ode-only answers normally need some explanation to be useful.

                <br/>
                <input
                    name="X33"
                    type="text"
                    id="X33"
                    class="X33"
                    value="An explanation would be in order. E.g., what is the idea/gist? Please respond by [editing (changing) your answer](https://XXXX), not here in comments (***without*** &quot;Edit:&quot;, &quot;Update:&quot;, or similar - the answer should appear as if it was written today). "
                    style="width:830px;"
                    accesskey="C"
                    title="Shortcut: Shift + Alt + C"
                />
            </p>

            <p>Spelling of Stack Overflow, Stack&nbsp;Exchange,
                and other Stack&nbsp;Exchange sites, etc.
                No matter how it looks in a logo
                <a href="http://stackoverflow.com/legal/trademark-guidance"
                >it is "Stack&nbsp;Overflow" and "Stack&nbsp;Exchange"</a>
                (the last section - <em>"Proper Use of the Stack Exchange Name"</em>).
                The <strong><em>only</em></strong> exception is "MathOverflow"
                (and Jeff Atwood should have said <strong><em>no</em></strong> at the time).

                <br/>
                <input
                    name="X28"
                    type="text"
                    id="X28"
                    class="X28"
                    value="It is *&quot;Stack&nbsp;Overflow&quot;* and *&quot;Stack&nbsp;Exchange&quot;*, not *&quot;StackOverflow&quot;* and *&quot;StackExchange&quot;* (see e.g. [this official page](http://stackoverflow.com/legal/trademark-guidance) (the very last section, near &quot;As a name&quot;))."
                    style="width:830px;"
                    accesskey="S"
                    title="Shortcut: Shift + Alt + S"
                />
            </p>

            <p>Many users come to Meta Stack Overflow to
                compla<strong><em>i</em></strong>n about
                being question-banned on Stack Overflow.
                Many of them use low English skills as
                an excuse, but capitalising "i" and
                capitalising sentences don't
                require any skills.

                <br/>
                <input
                    name="X29"
                    type="text"
                    id="X29"
                    class="X29"
                    value="For your own sake and for the sake of your readers, ***please*** capitalise &quot;i&quot;, capitalise sentences, and don't use [SMS language](https://en.wiktionary.org/wiki/u#Pronoun). ***This doesn't require any skills***, only the willingness to change habits. You disadvantage yourself right from the beginning by not doing it. This alone (due to downvotes) could have caused your question ban. Making this change will also greatly enhance your chances on the job market."
                    style="width:830px;"
                    accesskey="I"
                    title="Shortcut: Shift + Alt + I"
                />
            </p>

            <p>Many use<strong><em>r</em></strong>s on Stack Overflow post
                answers as questions (use of Stack Overflow as a
                forum) or because they can't comment below 50 reputation points.

                <br/>
                <input
                    name="X30"
                    type="text"
                    id="X30"
                    class="X30"
                    value="Related: *[Why do I need 50 reputation to comment? What can I do instead?](https://meta.stackexchange.com/questions/214173/)*."
                    style="width:830px;"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                />
            </p>

            <p>Many users on Stack Overflow think they are on 
                a <strong><em>f</em></strong>orum.
                Or treat it as such.

                <br/>
                <input
                    name="X31"
                    type="text"
                    id="X31"
                    class="X31"
                    value="[Stack Overflow is not a forum](http://meta.stackexchange.com/a/92115). It is a [think tank](http://meta.stackoverflow.com/a/325681)."
                    style="width:830px;"
                    accesskey="F"
                    title="Shortcut: Shift + Alt + F"
                />
            </p>


            <!-- Template
            <p>
                <input
                    name="X"
                    type="text"
                    id="X"
                    class=""
                    value="<template>"
                    style="width:110px;"
                    accesskey="Z"
                    title="Shortcut: Shift + Alt + Z"
                />
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
            />
        </form><?php the_EditOverflowFooter('CannedComments.php', "", ""); ?>


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
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FCannedComments.php"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FCannedComments.php%3FOverflowStyle=Native"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.

        </p>


<?php the_EditOverflowEnd() ?>

