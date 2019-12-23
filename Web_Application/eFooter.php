<?php
    # File: eFooter.php

    # Purpose: Output of common footer for Edit Overflow for Web.


    function the_EditOverflowFooter()
    {
        echo get_EditOverflowFooter();
    }


    function get_EditOverflowFooter()
    {
        # Yes, the Heredoc style makes it ugly.
        #
        return <<<'HTML_END'



        <hr/>


        <p>

            <a
                href="EditOverflow.php"
                title="Main functionality: Look up (incorrect) terms"
            >Edit Overflow</a>.

            <a
                href="Text.php"
                title="Various text transformations, e.g. replacing TABs and removing trailing space."
            >Text stuff</a>.

            <a
                href="Link_Builder.php"
                title="Convenient formatting of links in HTML, Markdown, and MediaWiki (Wikipedia)."
            >Link builder</a>.

            <a
                href="FixedStrings.php"
                title="Often-used symbols and other content for easy copy-pasting, e.g. the degrees and micro symbols."
            >Fixed strings</a>.

            <a
                href="EditSummaryFragments.php"
                title="Often-used content for edit summaries. Converts the most common error by both native speakers and ESLers."
            >Edit summary fragments</a>.


            <a
                href="https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_2019-11-01.html"
                title="The full Edit Overflow wordlist, about 10,000 incorrect terms."
            >Word list</a>.

            <a
                href="myInfo.php"
                title="Internal info for PHP (phpinfo())."
            >Environment information</a>.

            <!-- Note: PHP still works inside HTML comments, so we
                       need to use "#" to outcomment PHP lines.
                <?php
                    # phpinfo();
                ?>
            -->


HTML_END;

        # End of Heredoc part.

    } # get_EditOverflowFooter()


?>


