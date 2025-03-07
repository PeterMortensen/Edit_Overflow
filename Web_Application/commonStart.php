<?php
    # File: commonStart.php
    #
    # Notes:
    #
    #   1. This file doesn't output anything (to standard output) by
    #      itself (though it does execute some code (command-line
    #      argument parsing for use locally for testing, defining
    #      some constants, and setting up some globals used for
    #      error detection/logging)).
    #
    #   2. Clients must explicitly call the functions here, e.g.
    #      the_EditOverflowHeadline();
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
    if (isset($argv)) # 'argv' is not defined at all when this is running in
                      # a web context. We need this to avoid entries in the
                      # web server error log, like "Trying to access
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
        return "Edit Overflow v. 1.1.50a919 2025-03-07T141419Z+0";
    }


    # Note that we are using the WordPress convention of
    # name prefixing functions (with "get_") that
    # return a value (no side effects).
    #
    function get_SEOpreferences()
    {
        return "Correct all the Internet's typos! By Peter Mortensen";
    }


    # Note that we are using the WordPress convention of name
    # prefixing functions that echo's (with "the_").
    #
    # $aPageName: For constructing a URL that works. For now it is
    #             the raw file of the current PHP file, e.g.
    #             "EditSummaryFragments.php" to construct
    #             <https://pmortensen.eu/world/EditSummaryFragments.php?OverflowStyle=Native>
    #
    # $aExtraQueryParameters: Must end in "&" (this is to allow
    #                         none at all (an empty string))
    #
    #    Example
    #
    #       "LookUpTerm=cpu"
    #
    function the_EditOverflowHeadline(
        $aHeadline,
        $aPageName,
        $aExtraQueryParameters,
        $aExtraTopCommentContent)
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
        # 4. Injection of errors (for regression testing)
        #
        # 5. Start of document, incl. <title> tag

        # Sanity check of the input (actually, if
        # the clients are well behaved...)
        #
        # Our current expectation is the file name of a PHP file.
        #
        #assert(preg_match('/\.php$/', $aPageName) !== true,
        #       "Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/\.php$/', $aPageName) === true,
        #       "YYY Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/\.php$/', $aPageName),
        #       "Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php$/', $aPageName) === true,
        #       "YYY Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php/', $aPageName) === true,
        #       "YYY Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php/', $aPageName) !== false,
        #      "YYY Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php$/', $aPageName) !== false,
        #       "YYY Invalid HTML page name: \"$aPageName\"");
        #Only in PHP 8
        #assert(str_ends_with('php', $aPageName) !== false,
        #       "YYY Invalid HTML page name: \"$aPageName\"");
        assert(preg_match('/php$/', $aPageName) !== 0,
               "Invalid HTML page name: \"$aPageName\"");

        # Later: Sanity check of $aExtraQueryParameters (must end
        #        in "&", unless it is an empty string)

        $someHeadline = "$aHeadline - " . get_EditOverflowID();

        $someTitle = $someHeadline . ". " . get_SEOpreferences() . ".";

        the_startOfDocument(
            $someTitle,
            $aPageName,
            $aExtraQueryParameters,
            $aExtraTopCommentContent);

        echo "<h1>$someHeadline</h1>\n";


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
        $toReturn = "";
        if ($aRawURL !== "") # Don't output anything for empty URLs
                             # (usually an indication of failed
                             # lookups).
        {
            $encodedURL = get_HTMLattributeEscaped($aRawURL);

            $toReturn =
                "<a href=\"" . $encodedURL . "\"" . $anExtraAttributesText .
                ">" . $aRawLinkText . "</a>";
        }
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


    #Why is it here when similar functions are in Text.php?
    #Probably because it uses StringReplacerWithRegex
    #(which is in a *separate* file).
    #
    function transformFor_YouTubeComments($aText)
    {
        $replacer = new StringReplacerWithRegex($aText);

        # Convert time to YouTube format
        $replacer->transform('(\d+)\s+secs',   '$1 ');
        $replacer->transform('(\d+)\s+min\s+', '$1:');
        $replacer->transform('(\d+)\s+h\s+',   '$1:');

        # Don't transform URLs if there just a single link in
        # Markdown format (e.g., used in LBRY/Odysee comments).
        #
        # Note that this check is global (for all the text),
        # not on a line-for-line basis.
        #
        if (! $replacer->match("\[[^\]]+\]\([^)]+\)"))
        {
            # We strip the "www" in YouTube URLs. For unknown
            # reasons, in some cases, replacing the " DOT "
            # back to "." and using it in a browser, results
            # in a ***double*** "www" and thus fails to load
            # properly. Is it a web browser problem?
            #
            # Example: www.www.youtube.com/watch?v=_pybvjmjLT0&lc=Ugw6kcW_X3ulHZugaLB4AaABAg
            #
            $replacer->transform('www\.(youtube\..*)', '$1');

            # Convert URLs so they do not look like URLs...
            # (otherwise, the entire comment will be
            # automatically removed by YouTube after
            # one or two days).
            $replacer->transform('(\w)\.(\w)', '$1 DOT $2');
            $replacer->transform('https:\/\/', ''         );
            $replacer->transform('http:\/\/',  ''         );

            # Reversals for some of the false positives
            # in URL processing, e.g. for "Node.js"
            #
            # Future: Perhaps general reversal near the end, after
            #         the last "/"? Say, for ".html".
            #
            $replacer->transform('E DOT g\.', 'E.g.');
            $replacer->transform('e DOT g\.', 'e.g.');
            $replacer->transform(' DOT js',   '.js'); # E.g. Node.js
            $replacer->transform(' DOT \_',   '._'); # Full stop near the end of a line

            # Revert for numbers, e.g. 3.141
            $replacer->transform('(\d) DOT (\d)', '$1.$2');

            # Revert for file extensions. Note: Potentially false positives
            # as we don't currently check for end of line. But it is
            # not always at the end (examples:
            # <https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=Javascript>
            # <https://www.dotnetrocks.com/default.aspx?ShowNum=1636>)
            $replacer->transform(' DOT htm',  '.htm');
            $replacer->transform(' DOT ogg',  '.ogg');
            $replacer->transform(' DOT php',  '.php');
            $replacer->transform(' DOT aspx', '.aspx');
            $replacer->transform(' DOT pdf',  '.pdf');
        } //If doing URL processing
        else
        {
            # Not for YouTube

            #Ideas: 1. Convert (naked) URLs to Markdown inline
            #          links (suitable for LBRY), at least
            #          for Wikipedia links (extracting the
            #          link text from the URL).
            #
            #       2. Presuming LBRY, add the empty line workaround (we
            #          have a similar workaround for YouTube below).
        }

        # Convert email addresses like so... (at least
        # to offer some protection (and avoiding
        # objections to posting)).
        #
        # For now, just globally replace "@". But note
        # that it affects LBRY invites (perhaps we
        # should add an exception?).
        #
        # Future: Perhaps don't replace if there is a space before "@"
        #         or at the beginning of a line.
        #
        $replacer->transform('\@', ' AT ');

        #This one does not seem to work... Why?? Do we need some
        #escaping? No, it is probably because the text we see
        #here is HTML thingamajiggied - ">" is encoded
        #as "&gt;"... - using HTML character entity references
        #(is it due to some WordPress madness (even when using
        #URL query parameter "OverflowStyle=Native")?).
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
        # For it to work in LBRY comments, a space followed by a
        # non-breakspace (" &nbsp;") would be need instead.
        # However, this is incompatible with YouTube comments...
        #
        $replacer->transform("\r\n\r\n", "\r\n \r\n"); # Note that is doesn't work
                                                       # if the LAST line is empty.

        # Adjust indent for non-timestamp lines (due
        # to non-proportional fonts).
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

            # XXX How do we replace only in leading space????

            #$replacer->transform('(\d+)\s+secs',   '$1 ');
        }

        $someText = $replacer->currentString();
        return $someText;
    } #transformFor_YouTubeComments()


    # Format a link in WikiMedia (Wikipedia format, internal link).
    #
    # The first part is related to the URL (essentially the title of
    # the Wikipedia article). The second part is what we have chosen
    # to be the output word (correct term). Sometimes the two parts
    # are the same.
    #
    # Example:
    #
    #    ''[[Uniform_resource_locator|URL]]''
    #
    # Future:
    #
    #   1. We should replace "_" in the first part with space (as
    #      it is more readable in Wikipedia source text).
    #
    #
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
            #       title on, e.g., Wikipedia may not be the
            #       same of the correct term).

            $replacer7 = new StringReplacerWithRegex($aURL);

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

            # For (English) Wikipedia
            $replacer7->transform('https:\/\/en.wikipedia.org\/wiki\/(.*)', '[[$1|SENTINEL_ZZ]]');

            # For (English) Wiktionary
            $replacer7->transform('https:\/\/en.wiktionary.org\/wiki\/(.*)', '[[$1|SENTINEL_ZZ]]');

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


    # Format a link in WikiMedia, external link (Wikipedia format).
    #
    # That is, to any web site, incl Wikipedia itself, e.g. from
    # another wiki, like Sigrok's, to a Wikipedia page.
    #
    # Example:
    #
    #    ''[https://en.wikipedia.org/wiki/ChatZilla ChatZilla]''
    #
    function WikiMedia_Link_External($aURL, $aCorrectTerm)
    {
        # This is really straightforward. Or at least it seems so.
        # However, are there any special cases, e.g. encoding
        # for some special characters?
        #
        $linkStr = "[$aURL $aCorrectTerm]";
        return $linkStr;
    } #WikiMedia_Link_External()


    # If the URL is a Wiktionary one, extract the
    # (grammatical) word class. Otherwise, return
    # an empty string.
    #
    # Example:
    #
    #   Input
    #
    #     https://en.wiktionary.org/wiki/is#Verb
    #
    #   Output
    #
    #     Verb
    #
    function extractGrammaticalWordClass($aURL)
    {
        $toReturn = "";

        # If the URL is a Wiktionary one, extract the
        # (grammatical) word class and display it
        # close to the correct word.
        if (preg_match('/^https:\/\/en.wiktionary.org\/wiki\//', $aURL) !== 0)
        {
            # Example (without the brackets):
            #
            #   <https://en.wiktionary.org/wiki/doesn%27t#Verb>

            # $grammaticalWordClass = 'Is a Wiktionary URL...';
            $replacer_g = new StringReplacerWithRegex($aURL);

            # Extract the HTML anchor part (e.g. "Prepositional_phrase")
            $replacer_g->transform(
              'https:\/\/en.wiktionary.org\/wiki\/.+#(.+)', '$1');

            $anchor = $replacer_g->currentString();

            # For now: Suppress output for those where the word class
            #          is not in the URL (an alternative could be
            #          exception list (mapping for a list of
            #          correct words to the word class))
            if (
                ($anchor !== "Etymology")          &&
                ($anchor !== "Etymology_1")        &&
                ($anchor !== "Alternative_forms")  &&
                ($anchor !== "Usage_notes")        &&
                1)
            {
                # Replace underscores with a space
                $replacer_g->transform('_', ' ');

                # Remove numbers. Ideally only at the end, but we
                # don't have any with numbers not at the end.
                $replacer_g->transform('\d', '');

                # Remove trailing space
                $replacer_g->transform('\s+$', '');

                $toReturn = $replacer_g->currentString();
            }
        }
        return $toReturn;
    } #extractGrammaticalWordClass()


    # Note that we are using the WordPress convention of name
    # prefixing functions (with "the_") that echo's.
    #
    # $aPageName: For constructing a URL that works. For now it is
    #             the raw file of the current PHP file, e.g.
    #             "EditSummaryFragments.php" to construct
    #             <https://pmortensen.eu/world/EditSummaryFragments.php?OverflowStyle=Native>
    #
    function the_startOfDocument(
        $aTitle,
        $aPageName,
        $aExtraQueryParameters,
        $aExtraTopCommentContent)
    {
        # Sanity check of the input (actually, if
        # the clients are well behaved...)
        #
        # Our current expectation is the file name of a PHP file.
        #
        #assert(preg_match('/\.php$/', $aPageName) !== true,
        #       "Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/\.php$/', $aPageName) === true,
        #       "ZZZ Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/\.php$/', $aPageName),
        #       "Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php$/', $aPageName) === true,
        #       "ZZZ Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php/', $aPageName) === true,
        #       "ZZZ2 Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php/', $aPageName) !== false,
        #       "ZZZ2 Invalid HTML page name: \"$aPageName\"");
        #assert(preg_match('/php$/', $aPageName) !== false,
        #       "ZZZ2 Invalid HTML page name: \"$aPageName\"");
        ##Only in PHP 8
        #assert(str_ends_with('php', $aPageName) !== false,
        #       "ZZZ2 Invalid HTML page name: \"$aPageName\"");
        assert(preg_match('/php$/', $aPageName) !== 0,
               "Invalid HTML page name: \"$aPageName\"");

        # Later: Sanity check of $aExtraQueryParameters (must end
        #        in "&", unless it is an empty string)


        echo "\n<!--\n";
        echo "    Notes:\n\n";
        echo "      1. We can now use \"OverflowStyle=Native\" to avoid the WordPress\n";
        echo "         overhead (and make the keyboard shortcut hints visible):\n\n";
        echo "           <https://pmortensen.eu/world/$aPageName?$aExtraQueryParameters" .
             "OverflowStyle=Native>\n\n";

        # Note: For HTML validation to succeeed, there is a limit of
        #       approximately 1024 bytes before the <meta> tag with
        #       the UTF-8 specification must appear. That is, our
        #       HTML comment must not be too long). Thus, as a
        #       workaround, we add the extra information, if
        #       any, after the <meta> tag, with a reference
        #       to it.
        #
        #       Though we have less control if WordPress is used and
        #       it is (currently) left out. Is there a way to
        #       include anyway?
        #
        if (strlen($aExtraTopCommentContent) > 0)
        {
            echo "      2. For extra information, specific to this page, see\n";
            echo "         the HTML comment below, just before the \"title\"\n";
            echo "         tag (only available with the \"OverflowStyle=Native\"\n";
            echo "         query parameter).\n";
        }
        echo "-->\n\n\n";


        ###########################################################################
        # WordPress specific!

        if (useWordPress())
        {
            # Note: For now, the passed title is not used in the WordPress
            #       part (also we have the prefix "Page not found - " in
            #       the title when using WordPress - the exact behaviour
            #       is dependent on which WordPress theme is used (for
            #       some themes, "Page not found" is obnoxiously
            #       on the page itself, not just the title). See also
            #       comments below).
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

        <!--
            $aExtraTopCommentContent
        -->

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

                /*                         Col1              Col2 Col3               Col4   */
                /*  grid-template-columns: minmax(5%, 130px) 1em  2fr                minmax(5%, 350px); */
                /*  grid-template-columns: minmax(5%, 200px) 1em  minmax(5%, 350px)  2fr; */
                    grid-template-columns: minmax(5%, 200px) 1em  170px              2fr;
                                       /* 10% 1fr 2fr 12em;
                                          1fr 1em 2fr
                                       */
                grid-gap: 0.3em 0.6em;
                grid-auto-flow: row;
                align-items: center;
            }

            /* Column 3 is for the text (<label>) for the checkbox,
               close to the narrow column 2.   */
            input,
            output,
            textarea,
            select,
            button,
            p.entry-line
            {
                grid-column: 2 / 4;
                width: auto;
                margin: 0;
            }

            p.grammaticalWordClass
            {
                grid-column: 4 / 4;
                width: auto;
                margin: 0;
                border-color: #B7873E;
            }

            .formgrid > div
            {
                grid-column: 1 / 5;
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


    } #the_startOfDocument()


    # ########   E n d   o f   f u n c t i o n   d e f i n i t i o n s   ########


?>
