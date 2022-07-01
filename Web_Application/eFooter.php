<?php
    # File: eFooter.php

    # Purpose: Output of common footer for Edit Overflow for Web.


    # Note: The two parameters are for the link builder and is
    #       only really used by the main lookup page - they
    #       are left empty for the other pages.
    #
    function the_EditOverflowFooter($aParentPage, $aLinkText, $aURL)
    {

        # For the last two parameters, sample output HTML for the
        # link builder part:
        #
        #   https://pmortensen.eu/world/Link_Builder.php?LinkText=CPU&URL=https://en.wikipedia.org/wiki/Central_processing_unit



        #echo "</p>Value of E_ALL: " . E_ALL . " </p>";

        # Side-effect of this function... Use the
        # opportunity for some error detaction as
        # this function is used by all...
        #
        # Results: It is 1 when using WordPress for styling, even
        #          though we have set it to 0 in .htaccess
        #
        #echo "</p>Value of PHP setting 'display_errors': " . ini_get('display_errors') . " </p>";


        # WordPress likes to override it...
        #
        # It also expresses that we don't want to risk having the
        # first 16 characters of the database password exposed...
        # (when the PDO constructor fails).
        #assert(0);
        #
        # But assert itself is affected by 'display_errors'... We don't
        # actually see an assert on the web page if 'display_errors'
        # is 0 (only in the PHP error log file)...
        #
        #assert(ini_get('display_errors') === 0);    No!!!!
        #
        # Note: The strict === identical operator will not do implicit
        #       conversions (string to integer in this case). ini_get()
        #       returns a string.
        #
        # References:
        #
        #   <https://www.php.net/manual/en/function.ini-get.php>
        #     ini_get
        #
        #   <https://www.php.net/manual/en/language.operators.comparison.php>
        #     Comparison Operators
        #
        #     "TRUE if $a is equal to $b, and they are of the same type."
        #
        #   <https://stackoverflow.com/questions/80646>
        #     How do the PHP equality (== double equals) and identity
        #     (=== triple equals) comparison operators differ?
        #
        #     "Difference between == and ==="
        #
        assert(ini_get('display_errors') === '0');


        echo get_EditOverflowFooter($aParentPage, $aLinkText, $aURL);
    } #the_EditOverflowFooter()


    # For the parameters, see the_EditOverflowFooter()
    #
    function get_EditOverflowFooter($aParentPage, $aLinkText, $aURL)
    {
        #What about encoding of the URL??? (e.g. if it contains "&")???
        #
        $linkBuilerPartialURL = "Link_Builder.php?LinkText=$aLinkText&URL=$aURL";


        # Note: Any keyboard shortcuts used in the footer can conflict
        #       with keyboard shortcuts on the pages. E.g. page 
        #       "EditSummaryFragments.php" uses most of the available
        #       keyboard shortcuts.


        # Yes, the Heredoc style makes it ugly.
        #
        return <<<HTML_END



        <hr/>


        <p>

            <a
                href="https://pmortensen.eu/"
                title="Main page"
            >Home</a>.

            <a
                href="EditOverflow.php"
                title="Main functionality: Look up (incorrect) terms"
            >Edit Overflow</a>.

            <a
                href="Text.php"
                title="Various text transformations, e.g. replacing TABs and removing trailing space."
            >Text stuff</a>.

            <a
                href="$linkBuilerPartialURL"
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
                href="https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html"
                title="The full Edit Overflow wordlist, about 10,000 incorrect terms."
            >Word list</a>.

            <a
                href="myInfo.php"
                title="Internal info for PHP (phpinfo())."
            >Environment information</a>.

            <a
                href="https://github.com/PeterMortensen/Edit_Overflow"
                title="Source repository at GitHub for both the web application, the C# application, the word list, build script, unit/integration tests, etc."
            >GitHub (source)</a>.

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
