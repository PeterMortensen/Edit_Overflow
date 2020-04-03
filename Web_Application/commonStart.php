<?php
    # File: commonStart.php

    # Purposes (though we should probably split the WordPress-specific
    #           parts into a separate file, as this file has now
    #           taken on more responsibilities):
    #
    #
    #   1. Centralise the WordPress-specific things. E.g. to minimise
    #      redundancy on individual pages.
    #
    #      We also have the option to turn off WordPress (e.g.
    #      to avoid a lot of noise in HTML validation).
    #
    #   2. Centralise things (by means of helper functions) to eliminate
    #      some redundancy (e.g. Edit Overflow version).

    require_once('eFooter.php'); # Our own file, not WordPress...


    # Used by one page (EditOverflow.php)
    const LOOKUPTERM = 'LookUpTerm';

    # Used by another page (Text.php)
    const MAINTEXT = 'someText';


    $formDataSizeDiff = -1;


    # Including version number and date
    #
    # Note that we are using the WordPress convention of
    # name prefixing functions (with "get_") that
    # return a value (no side effects).
    #
    function get_EditOverflowID()
    {
        return "Edit Overflow v. 1.1.49a26 2020-04-03T210449Z+0";
    }


    # Note that we are using the WordPress convention of
    # name prefixing functions (with "the_") that echo's.
    #
    function the_EditOverflowHeadline($aHeadline)
    {
        echo "<h1>$aHeadline - " . get_EditOverflowID() . "</h1>";

        # Another side-effect of this function... Use the
        # opportunity as this function is used by
        # all pages (and in the beginning).
        adjustForWordPressMadness();
    }


    # A central place to express if we should also support
    # HTML GET parameters ($_REQUEST) or only POST
    # parameters ($_POST).
    #
    # It also frees the clients for checking for existence (as in
    # most cases we don't need to distinguish between empty data
    # and absent data).
    #
    function get_postParameter($aKey)
    {
        #echo "<p>About to retrieve key >>>$aKey<<<...</p>";
        #
        # The "if" is to avoid the following error (as no POST parameters
        # are defined in $_REQUEST when just initial opening a .php page):
        #
        #    Notice: Undefined index: editSummary in ... commonStart.php ...
        #
        $supportGETandPOST = "";
        if (array_key_exists($aKey, $_REQUEST))
        {
            # Support HTML GET parameters, as well
            # as HTML POST parameters
            #
            $supportGETandPOST = $_REQUEST[$aKey];
        }


        #Possible optimisation: As in the common configuration we don't
        #                       actually use the POST-only version. We
        #                       could somehow leave it out (unnecessary
        #                       operation).
        #
        # The "if" is to avoid the following error (as the parameter
        # is not defined in $_POST when using HTML GET (parameters
        # in a URL, not as part of an HTML form post action) - in
        # contrast to $_REQUEST which is a superset of the GET
        # and POST parameters):
        #
        #    Notice: Undefined index: OverflowStyle in ... commonStart.php
        #
        $supportOnlyPOST = "";
        if (array_key_exists($aKey, $_POST))
        {
            $supportOnlyPOST = $_POST[$aKey];
        }

        #$toReturn = $supportOnlyPOST;
        $toReturn = $supportGETandPOST;

        #Do any check for undefined, etc.???

        return $toReturn;
    }


    # Helper function to support switching between WordPress
    # and native 1990s HTML (the original version)
    #
    function useWordPress()
    {
        # That is, if we pass OverflowStyle (by GET or POST), we
        # can turn off the WordPress part (e.g. to ease HTML
        # validation (for example, when using WordPress, we
        # got 29 issues in total (14 errors and 15 warnings)
        # for "EditSummaryFragments.php")).
        #
        # Examples:
        #
        #    <https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu&OverflowStyle=Native>
        #
        #    <https://pmortensen.eu/world/EditSummaryFragments.php?OverflowStyle=Native>
        #
        #    <https://pmortensen.eu/world/Text.php?OverflowStyle=Native>


        # The default is to use WordPress, if not explicitly turned off
        # (and by the exact spelling of the value of "OverflowStyle"
        # that may be absent). However, this is the ***single***
        # place to unconditionally turn off WordPress, if needed.
        #
        $toReturn = true;

        $OverflowStyle = get_postParameter('OverflowStyle') ?? 'WordPress';

        if ($OverflowStyle === 'Native')
        {
            #echo "<p>WordPress styling turned off...</p>\n";
            $toReturn = false;
        }
        else
        {
            #echo "<p>WordPress styling retained...</p>\n";
        }
        return $toReturn;
    } #useWordPress()


    # Single quotes, etc. are currently escaped if
    # using WordPress (that is, in the form data).
    #
    # Note: Currently it is only done for a specific element,
    #       "someText" (used by a specific page, Text.php)
    #
    function adjustForWordPressMadness()
    {
        global $_REQUEST;
        #global MAINTEXT;
        global $formDataSizeDiff;

        # When we open the form (URL with ".php")
        # there isn't any form data.
        if (
            array_key_exists(LOOKUPTERM, $_REQUEST) ||  # Main - lookup
            array_key_exists(MAINTEXT,   $_REQUEST)     # Textstuff page
           )
        {
            $formDataSizeBefore = strlen(get_postParameter(MAINTEXT));

            #echo "<p>formDataSizeBefore: $formDataSizeBefore</p>\n";

            # Only when WordPress is active (otherwise we get errors)
            if (function_exists('stripslashes_deep'))
            {
                #echo "<p>stripslashes_deep() exists...</p>\n";

                # Escape problem "fix" (ref. <https://stackoverflow.com/a/33604648>)
                # The problem is solely due to WordPress (we would't need it
                # if it wasn't for the use of/integration into WordPress).
                #
                # "stripslashes_deep" is part of WordPress
                #
                $_REQUEST = array_map('stripslashes_deep', $_REQUEST);

                # Only really necessary if function get_postParameter()
                # is configured for only supporting POST (not GET)...
                # Or in other words, this is an unnecessary operation
                # in most cases.
                #
                $_POST = array_map('stripslashes_deep', $_POST);
            }

            $formDataSizeAfter = strlen(get_postParameter(MAINTEXT));

            # Note: Only for one specific element,
            #       "someText", only used in Text.php.
            #
            $formDataSizeDiff = $formDataSizeBefore - $formDataSizeAfter;

            #echo "<p>formDataSizeDiff: $formDataSizeDiff</p>\n";
        }
    }


    function get_HTMLattributeEscaped($aRawContent)
    {
        # Later: Probably also single quote.

        #echo "<p>Before: xxx" . $aRawContent . "xxx</p>\n";


        # But why did we have to use "%22" instead of "&quot;"????
        #
        # Is there a difference between HTML links ("href") and form field
        # values ("value")?? Does one need percent encoding and the other
        # " character entity reference encoding ("&quot;")?
        #
        #$encodedContent = str_replace('"', '&quot;', $aRawContent);
        $encodedContent = str_replace('"', '%22', $aRawContent);

        #echo "<p>After: xxx" . $aRawContent . "xxx</p>\n";

        return $encodedContent;
    }


    # Single place for HTML links
    #
    function get_HTMLlink($aRawLinkText, $aRawURL, $anExtraAttributesText)
    {
        $encodedURL = get_HTMLattributeEscaped($aRawURL);

        $toReturn =
            "<a href=\"" . $encodedURL . "\"" . $anExtraAttributesText .
            ">" . $aRawLinkText . "</a>";

        return $toReturn;
    }


    # Single place for output of dynamic "value" attributes in HTML forms.
    function the_formValue($aRawContent)
    {
        $encodedContent = get_HTMLattributeEscaped($aRawContent);

        echo "value=\"$encodedContent\"\n";
    }


    # ########   E n d   o f   f u n c t i o n   d e f i n i t i o n s   ########



    ###########################################################################
    # WordPress specific!

    if (useWordPress())
    {
        # For getting the styling and other redundant
        # content (like headers) from WordPress.
        #
        # So now we have a dependency on WordPress...
        #
        define('WP_USE_THEMES', false);
        require(dirname(__FILE__) . '/wp-blog-header.php');


        # For a page counter, plugin "Page Visit Counter",
        # <https://wordpress.org/plugins/page-visit-counter/>
        ##require_once('shortcodes.php');
        require_once('wp-includes/shortcodes.php');

        #require_once(‘blog/wp-blog-header.php’); # But it doesn't actually exist
                                                  # in folder 'blog'.

        #require_once(‘wp-blog-header.php’);


        get_header(); # Note: Using some WordPress themes results in the following
                      #       on the page itself (though we also have it in the
                      #       title for all themes - but this is less intrusive):
                      #
                      #           "Page not found"
                      #
                      #       Some themes that do not give it are:
                      #
                      #           "Responsive"    (the one we currently use)
                      #           "Orfeo"
                      #           "Hestia"
                      #           "Astra"
    }
    else
    {
        # Revert to old-style header
        #
        # Yes, the Heredoc style makes it ugly.
        #
        echo <<<'HTML_END'
<!DOCTYPE html>
<html lang="en">

    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">

        <title>Some edit summary fragments - Edit Overflow</title>

        <style>
            body
            {
                background-color: lightgrey;
            }
        </style>


        <!-- ================= From EditOverflow.php - to be merged in the
                               next round of refactoring -->
        <style>
            body
            {
                background-color: lightgrey;
            }

            .formgrid
            {
                display: grid;
                grid-template-columns: minmax(5%, 130px) 1em 2fr;
                                       /* 10% 1fr 2fr 12em;
                                          1fr 1em 2fr
                                       */
                grid-gap: 0.3em 0.6em;
                grid-auto-flow: row;
                align-items: center;
            }

            input,
            output,
            textarea,
            select,
            button
            {
                grid-column: 2 / 4;
                width: auto;
                margin: 0;
            }

            .formgrid > div
            {
                grid-column: 3 / 4;
                width: auto;
                margin: 0;
            }

            /* label, */
            input[type="checkbox"] + label,
            input[type="radio"]    + label
            {
                grid-column: 3 / 4;
                width: auto;
                padding: 0;
                margin: 0;
            }

            input[type="checkbox"],
            input[type="radio"]
            {
                grid-column: 2 / 3;
                justify-self: end;
                margin: 0;
            }

            label + textarea
            {
                align-self: start;
            }
        </style>


    </head>

    <body>
        <h1>(Note: PoC, to be styled to escape the 1990s...)</h1>

HTML_END;

        # End of Heredoc part.

    } # End of native HTML part (not WordPress)


?>


