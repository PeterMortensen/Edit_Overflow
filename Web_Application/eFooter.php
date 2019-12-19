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
                accesskey="E"
                title="Shortcut: Shift + Alt + E"
            >Edit Overflow</a>.

            <a
                href="Text.php"
                accesskey="J"
                title="Shortcut: Shift + Alt + J"
            >Text stuff</a>.

            <a
                href="Link_Builder.php"
                accesskey="B"
                title="Shortcut: Shift + Alt + B"
            >Link builder</a>.

            <a
                href="FixedStrings.php"
                accesskey="F"
                title="Shortcut: Shift + Alt + F"
            >Fixed strings</a>.

            <a
                href="EditSummaryFragments.php"
                accesskey="D"
                title="Shortcut: Shift + Alt + D"
            >Edit summary fragments</a>.


            <a
                href="https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_2019-11-01.html"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >Word list</a>.

            <a
                href="myInfo.php"
                accesskey="I"
                title="Shortcut: Shift + Alt + I"
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


