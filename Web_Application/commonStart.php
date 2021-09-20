<?php
    # File: commonStart.php
    #
    # Note: This file doesn't output anything (to standard output) by
    #       itself (though it does execute some code (command-line
    #       argument parsing for use locally for testng, defining
    #       some constants, and setting up some globals used for
    #       error detection/logging)).
    #
    #       Clients must explicitly call the functions here, e.g.
    #       the_EditOverflowHeadline();
    #
    # Purposes (though we should probably split the WordPress-specific
    #           parts into a separate file, as this file has now
    #           taken on more responsibilities):
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
                                 # Only function definitions (no output)

    require_once('StringReplacerWithRegex.php');

    require_once('commonEnd.php'); # Only function definitions (no output)



    # Used by one page (EditOverflow.php)
    const LOOKUPTERM = 'LookUpTerm';

    # Used by another page (Text.php)
    const MAINTEXT = 'someText';


    # For running locally on the command line with a standard HTML
    # query string, e.g., for testing purposes (faster and
    # without touching production)
    #
    if (isset($argv)) # argv is not defined at all when this is running in
                      # a web context. We need this to avoid entries in 
                      # web server error log like "Trying to access 
                      # array offset on value of type null" 
    {
        $firstArgument = $argv[1];
        if (!empty($firstArgument))
        {
            # This is required for our build script regression tests, etc. to work
            parse_str($firstArgument, $_REQUEST);

            #echo "\n\nFirst argument: $firstArgument\n\n\n";
        }
    }


    $formDataSizeDiff = -1;


    # Including version number and date
    #
    # Note that we are using the WordPress convention of
    # name prefixing functions (with "get_") that
    # return a value (no side effects).
    #
    function get_EditOverflowID()
    {
        return "Edit Overflow v. 1.1.49a131 2021-09-20T134552Z+0";
    }


    # Note that we are using the WordPress convention of name
    # prefixing functions that echo's (with "the_").
    #
    function the_EditOverflowHeadline($aHeadline)
    {
        # Note: Besides the actual <h1> headline, we use side-effects
        #       in this function (indicating we should probably rename
        #       it to reflect its actual behaviour)... Use the
        #       opportunity as this function is used by all
        #       pages (and in the beginning).
        #
        # 1. WordPress does unexpected escaping of form data
        #
        # 2. WordPress seems to override the setting of PHP
        #    configuration setting 'display_errors' (it
        #    sets it to 1). We counter it here.
        #
        # 3. Central place for setting the error level, reporting level,
        #    etc.
        #
        # 4. Inject of errors (for regression testing)
        #
        # 5. Start of document, incl. <title> tag

        $someTitle = "$aHeadline - " . get_EditOverflowID();

        get_startOfDocument($someTitle);

        echo "<h1>$someTitle</h1>\n";


        adjustForWordPressMadness();

        ini_set('display_errors', '0');

        # For "Notice: Undefined variable: ..."
        #error_reporting(E_ERROR | E_WARNING | E_PARSE | E_NOTICE);
        error_reporting(E_ALL);

        # For simulating non-strict code (even if it is not displayed
        # on a page, it should still be logged in a log file). This
        # is so we can ***positively know*** if such errors are
        # actually captured in the PHP error log file.
        #
        $PHP_DoWarnings = get_postParameter('PHP_DoWarnings');
        if ($PHP_DoWarnings === 'On')
        {
            # Note: Warnings are issued only if actually
            #       executed, not at parse time.
            #
            # With the proper error reporting level, it
            # should result in something like this:
            #
            #     PHP Notice ... Undefined variable: dummy2 in ... commonStart.php on line ...
            #
            $dummy1 = $dummy2;
        }

    } #the_EditOverflowHeadline()


    # A central place to express if we should also support
    # HTML GET parameters ($_REQUEST) or ***only*** POST
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
        # The "if" is to avoid the following error (as no POST
        # parameters are defined in $_REQUEST when just
        # initially opening a .php page):
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
    } #get_postParameter()


    # Helper function to support switching between form-based
    # lookup (server) and client-side lookup (JavaScript).
    #
    function useJavaScriptLookup()
    {
        $toReturn = false; # Default

        $clientSideLookup = get_postParameter('UseJavaScript') ?? 'no';

        if ($clientSideLookup === 'yes')
        {
            #echo "<p>Client-side lookup is on...</p>\n";
            $toReturn = true;
        }
        else
        {
            #echo "<p>Client-side lookup is off...</p>\n";
        }
        return $toReturn;
    } #useJavaScriptLookup()


    # Helper function to support switching between WordPress
    # and native 1990s HTML (the original version)
    #
    function useWordPress()
    {
        # That is, if we pass parameter "OverflowStyle" (by GET or POST),
        # we can turn off the WordPress part (e.g. to ease HTML
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
    # Note: Currently it is only done for specific elements, e.g.
    #       for "someText" (used by a specific page, Text.php)
    #
    #       That is, the elements must be explicitly included/
    #       handled in this function.
    #
    function adjustForWordPressMadness()
    {
        global $_REQUEST;
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

            # Note: Only for specific elements, e.g.
            #       "someText", only used in Text.php.
            #
            $formDataSizeDiff = $formDataSizeBefore - $formDataSizeAfter;

            #echo "<p>formDataSizeDiff: $formDataSizeDiff</p>\n";
        }
    } #adjustForWordPressMadness()


    function get_HTMLattributeEscaped($aRawContent)
    {
        # Later: Probably also single quote.

        #echo "<p>Before: xxx" . $aRawContent . "xxx</p>\n";


        # But why did we have to use "%22" instead of "&quot;"???? Was it
        # due to escaping of double quote by WordPress? Or for W3C
        # validation submit to work?
        #
        # Is there a difference between HTML links ("href") and form field
        # values ("value")?? Does one need percent encoding and the other
        # " character entity reference encoding ("&quot;")?
        #
        #$encodedContent = str_replace('"', '&quot;', $aRawContent);
        $encodedContent = str_replace('"', '%22', $aRawContent);


        #To be more complete it should also be done for
        #single quotes,e.g. by "&apos;".


        #echo "<p>After: xxx" . $aRawContent . "xxx</p>\n";

        return $encodedContent;
    } #get_HTMLattributeEscaped()


    # Single place for HTML links
    #
    function get_HTMLlink($aRawLinkText, $aRawURL, $anExtraAttributesText)
    {
        $encodedURL = get_HTMLattributeEscaped($aRawURL);

        $toReturn =
            "<a href=\"" . $encodedURL . "\"" . $anExtraAttributesText .
            ">" . $aRawLinkText . "</a>";

        return $toReturn;
    } #get_HTMLlink()


    # Single place for output of dynamic "value" attributes
    # in HTML ***form*** elements.
    #
    function the_formValue($aRawContent)
    {
        $encodedContent = get_HTMLattributeEscaped($aRawContent);

        echo "value=\"$encodedContent\"\n";
    } #the_formValue()


    function transformFor_YouTubeComments($aText)
    {
        $replacer = new StringReplacerWithRegex($aText);

        # We strip the "www" in YouTube URLs. For unknown
        # reasons, in some cases, replacing the " DOT "
        # back to "." and using it in a browser, results
        # in a ***double*** "www" and thus fails to load properly.
        #
        # Example: www.www.youtube.com/watch?v=_pybvjmjLT0&lc=Ugw6kcW_X3ulHZugaLB4AaABAg
        #
        $replacer->transform('www\.(youtube\..*)', '$1');

        # Convert time to YouTube format
        $replacer->transform('(\d+)\s+secs',   '$1 ');
        $replacer->transform('(\d+)\s+min\s+', '$1:');
        $replacer->transform('(\d+)\s+h\s+',   '$1:');

        # Convert URLs so they do not look like URLs...
        # (otherwise, the entire comment will be
        # automatically removed by YouTube after
        # one or two days).
        $replacer->transform('(\w)\.(\w)', '$1 DOT $2');
        $replacer->transform('https:\/\/', ''         );
        $replacer->transform('http:\/\/',  ''         );

        # Reversals for some of the false
        # positives in URL processing
        #
        # Future: Perhaps general reversal near the end, after
        #         the last "/"? Say, for ".html".
        #
            $replacer->transform('E DOT g\.', 'E.g.');
            $replacer->transform('e DOT g\.', 'e.g.');
            $replacer->transform(' DOT js',   '.js'); # E.g. Node.js
            $replacer->transform(' DOT \_',   '._'); # Full stop near the end of a line

            $replacer->transform('(\d) DOT (\d)', '$1.$2'); # Revert for numbers

            # Revert for file extensions. Note: Potentially false positives
            # as we don't currently check for end of line. But it is
            # not always at the end  (examples:
            # <https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=Javascript>
            # <https://www.dotnetrocks.com/default.aspx?ShowNum=1636>)
            $replacer->transform(' DOT htm',  '.htm');
            $replacer->transform(' DOT ogg',  '.ogg');
            $replacer->transform(' DOT php',  '.php');
            $replacer->transform(' DOT aspx', '.aspx');
            $replacer->transform(' DOT pdf',  '.pdf');

        # Convert email addresses like so... (at least
        # to offer some protection (and avoiding
        # objections to posting)).
        #
        # For now, just globally replace "@". But note that it
        # affects LBRY invites (perhaps we should add an exception?).
        #
        # Future: Perhaps don't replace if there is a space before "@"
        #         or at the beginning of a line.
        #
        $replacer->transform('\@', ' AT ');

        #This one does not seem to work... Why?? Do we
        #need some escaping?
        #
        # Convert "->" to a real arrow
        #
        # Note: For YouTube it can not be
        #       the HTML entity, "&rarr;".
        $replacer->transform('->', '→');

        # After 2020-05-21, empty lines no longer work in YouTube
        # comments (they are silently removed when submitting
        # the comment).
        #
        # The workaround is to add a space to empty lines.
        #
        # Note: Must be double quotes for \r and \n
        #
        $replacer->transform("\r\n\r\n", "\r\n \r\n"); # Note that is doesn't work
                                                       # if the LAST line is empty.

        # Adjust indent for non-timestamp lines
        #
        # We wait till ***last*** because the input may
        # already have timestamps in the final format
        # (so this should come after the timestamp
        # conversion).
        #
        # It is also a good idea to have any TABs
        # converted first.
        #
        if ($replacer->match("\d+:\d+\s"))

                 # If we remove common leading spaces first (as in
                 # our keyboard macro), then we don't need
                 # to explicitly detect (exclude) lines with
                 # timestamps.
        {
            #$replacer->transform('^(\s{4,4})', '');

            # Note: We can't use "^" to match beginning
            #       of lines as we use multi-line mode
            #$replacer->transform("\r\n{4,4}", "\r\n");
            #$replacer->transform("\r\n    ", "\r\n");


            # Remove three spaces from non-timestamp lines. It
            # is to adjust for the total effect of:
            #
            #   1) adjust for the reduction in space of the
            #      timestamp encoding (e.g. from
            #      "04 min 17 secs" to "04:17")
            #
            #   2) Much smaller size of spaces compared
            #      to letters, numbers, etc.
            #
            $replacer->transform("\r\n   ", "\r\n");

            # XXX How do we replace only in leading space?

            #$replacer->transform('(\d+)\s+secs',   '$1 ');
        }

        $someText = $replacer->currentString();
        return $someText;
    } #transformFor_YouTubeComments()


    function WikiMedia_Link($aURL, $aCorrectTerm)
    {
        #Note: This is redundant with the corresponding
        #      encoding in the C# source code...
        #
        # First stab (we probably also need to handle URL encoding
        # (example: <https://en.wikipedia.org/wiki/Pip_%28package_manager%29>)):
        #
        # Only for Wikipedia for now (not Wiktionary):
        #
        $linkStr = "Does not apply";    # Default
        if (1)
        {
            # Note: We need to derive the link word (reference) from
            #       the URL instead of the correct term (as the
            #       title on e.g. Wikipedia may not be the same
            #       of the correct term).

            $replacer7 = new StringReplacerWithRegex($aURL);

            #Delete at any time
            #$replacer->transform('(\d+)\s+secs',   '$1 ');

            #$replacer7->transform('https:\/\/en.wikipedia.org\/wiki\/(.*)', '[[$1|$1]]');

            # We can't use $aCorrectTerm for the second parameter(?).
            # We do it in two steps instead, by using a sentinel.
            #
            # Could we use a combination of single and double quotes? -
            #
            #     '[[$1|' . "$aCorrectTerm]]"
            #
            #       or
            #
            #     '[[$1|' . $aCorrectTerm . ']]'
            #
            $replacer7->transform('https:\/\/en.wikipedia.org\/wiki\/(.*)', '[[$1|SENTINEL_ZZ]]');
            $replacer7->transform('SENTINEL_ZZ', $aCorrectTerm);

            $linkStr = $replacer7->currentString();

            if ($linkStr === $aURL) # That is, it was not transformed at all -
                                    # it did not match the regular expression
            {
                $linkStr = "Does not apply";
            }
        }
        return $linkStr;
    } #WikiMedia_Link()


    # Note that we are using the WordPress convention of
    # name prefixing functions (with "the_") that echo's.
    #
    function get_startOfDocument($aTitle)
    {

        ###########################################################################
        # WordPress specific!

        if (useWordPress())
        {
            # Note: For now, the passed title is not used in the WordPress
            #       part (also with have the prefix "Page not found - " in
            #       the title when using WordPress)
            #
            # The full title is currently (2020-04-24):
            #
            #    Page not found &#8211; Hertil og ikke længere


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
            # Yes, the heredoc style makes it ugly.
            #
            # Note: non-quoted, HTML_END, for heredoc (not newdoc)
            #
            echo <<<HTML_END
<!DOCTYPE html>
<html lang="en">

    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">

        <title>$aTitle</title>

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

HTML_END;

            # End of Heredoc part. Yes, HTML_END needs to start in column 1.


        } # End of native HTML part (not WordPress)


    } #get_startOfDocument()


    # ########   E n d   o f   f u n c t i o n   d e f i n i t i o n s   ########


?>




