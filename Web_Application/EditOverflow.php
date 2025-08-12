<?php
    ##########################################################################
    #
    # File: EditOverflow.php
    #
    # This does the core of Edit Overflow: Looks up an incorrect
    # term and displays the correct term (also with a link).
    #
    ##########################################################################


    # Future:
    #
?>
<?php include("commonStart.php"); ?>

        <?php
            require_once('deploymentSpecific.php');

            # Or trailing underscore??? What was the intent?
            function stripTrailingUnderscore($aString)
            {
                #return substr($aString, 0, strlen($aString) - 1);
                return rtrim($aString, "_");
            } #stripTrailingUnderscore()

            function alternativeLink($anIncorrectTerm,
                                      $aCorrectTerm,
                                      $anID,
                                      $aSomeAnnotation)
            {
                #$baseURL = "https://pmortensen.eu/world/EditOverflow.php";
                $baseURL = "/world/EditOverflow.php?OverflowStyle=Native";

                $extraText = "";
                if ($aSomeAnnotation !== "")
                {
                    $extraText = " (" . strtolower($aSomeAnnotation) . ")";
                }

                return
                    "<a " .
                    "href=" .
                    "\"$baseURL&LookUpTerm=" .
                    urlencode($anIncorrectTerm) .
                    "\"" .
                    " id=\"$anID\"" .
                    ">" . stripTrailingUnderscore($aCorrectTerm) . "</a>" .
                    $extraText;
            } #alternativeLink()

            # For internal timing
            $startTime_secs = microtime(true);
            $databaseCalls = 0;

            function lookup($aPDO, $aLookupTerm)
            {
                global $databaseCalls;
                $databaseCalls++;

                # Default: For words that are not in our word list
                $incorrectTerm = "";
                $correctTerm   = "";
                $URL           = "";

                $SQLprefix =
                  " SELECT incorrectTerm, correctTerm, URL " .
                  " FROM EditOverflow" .
                  " WHERE incorrectTerm = "
                  ;

                if ($aLookupTerm !== '')  # Accepting empty strings as lookup.
                                          # They will be treated as failed
                                          # lookups.
                {
                    $statement = $aPDO->prepare($SQLprefix . ' :name');
                    $statement->execute(array('name' => $aLookupTerm));

                    if ($statement->rowCount() > 0)
                    {
                        # "PDO::FETCH_ASSOC" is to return the
                        # result as an associative array.
                        $row = $statement->fetch(PDO::FETCH_ASSOC);

                        # Note: This is for display, so e.g. "<" should be encoded
                        #       as "&lt;". We don't do any text processing on
                        #       the result (e.g., computing the length of a
                        #       string), except enclosing it other text.

                        # Note: This is not for display, so we can't apply
                        #       htmlentities() - clients depend on it.
                        #       It should be possible to do further
                        #       text processing.
                        #
                        #       It is up to the client to apply
                        #       htmlentities(). There, e.g., "<"
                        #       should be encoded as "&lt;".
                        #
                        $incorrectTerm = $row['incorrectTerm'];

                        $correctTerm   = $row['correctTerm'];

                        $URL           = $row['URL'];
                        #$URL          = htmlZZZZentities($row['URL'], ENT_QUOTES);  - what is the intent??
                    }
                }

                return [$incorrectTerm, $correctTerm, $URL];
            } #lookup()


            # These are for proper indentation in the
            # generated HTML source (by PHP).
            $headerLevelIndent = "        ";
            $baseIndent        = "$headerLevelIndent        ";
            $EOL_andBaseIndent = "\n$baseIndent";
            $EOL_andBaseIndent_sub = "\n    $baseIndent";

            # The only input field in the start HTML page
            #
            # The default value is for making direct HTML validation
            # by https://validator.w3.org/ work.
            #
            $lookUpTerm = get_postParameter('LookUpTerm') ?? 'js';

            #Now dynamic (shows the term in the title so we can
            #distinguish, e.g., when opening recently closed tabs
            #in Firefox), but we may need  to add a special case
            #for ***empty*** input/initial page... (right now
            #it is using some default).

            # Note: Before our workaround, this did not pass the W3C HTML
            #       validation, likely due to a 1024 bytes limit for some
            #       auto detection before the 'meta' tag specifying
            #       UTF-8 encoding. The validator error messages
            #       started with:
            #
            #           "Error: The character encoding was not declared.
            #            Proceeding using windows-1252.")"
            #
            $extraIndent = "            ";  # For insertion deeper down in
                                            # the HTML document.
            $allIndent   = "$extraIndent     ";
            the_EditOverflowHeadline(
                "Look up of \"$lookUpTerm\"",
                "EditOverflow.php",
                "LookUpTerm=cpu&",
                "3. Alternatively, to make it even faster, use JavaScript (client-side) lookup:\n\n" .
                  $allIndent . "<https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu&UseJavaScript=yes>\n\n" .
                  $allIndent . "  or\n\n" .
                  $allIndent . "<https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=cpu&UseJavaScript=yes&OverflowStyle=Native>\n\n" .
                  $allIndent . "Note:\n\n" .
                  $allIndent . "  a. JavaScript must be allowed from pmortensen.eu for this to work! (For\n" .
                  $allIndent . "     instance, when using NoScript.) If JavaScript is not allowed, then\n" .
                  $allIndent . "     it falls back to using the normal form-based lookup (requiring an\n" .
                  $allIndent . "     Internet connection).\n\n" .
                  $allIndent . "  b. Only the lookup itself is currently implemented - NONE of the\n" .

                  # Too much if in the beginning of the HTML
                  # document (HTML validation failure)...
                  $allIndent . "     other fields, like the checkin messages, are updated." .
                  ""
                );

            #echo
            #    "<p>lookUpTerm through htmlZZZentities(): " .
            #        htmlZZZZentities($lookUpTerm) .
            #    "<p>\n";
            #echo
            #    "<p>Raw lookUpTerm: " .
            #        $lookUpTerm .
            #    "<p>\n";


            # Some filtering, e.g. for content copied from a Quora editing
            # window.
            {
                #This corresponds to the filtering done in the
                #Windows Forms/C# application,
                #setSearchFieldFromClipboard(),
                #file </Dot_NET/OverflowHelper/OverflowHelper/Source/GUI/Forms/frmMainForm.cs>.
                #
                #
                #Perhaps we can eliminate this redundancy by having a
                #set of rules as data?


                # 2022-04-09: This is moot now, as the Quora
                # moderation bots of unspecified IQ have
                # effectively destroyed all copy editing
                # on Quora (from about 2020) - which is
                # against the original "Yahoo Answers with
                # slightly better grammar." slogan. Now it
                # is just the next Yahoo Answers (already
                # shut down). When will they ever learn?
                #
                # Disabling it fixed our problem with
                # looking up "*nix".
                #
                # We keep it for a while.
                #
                ## If bold in a Quora editing
                #
                ## "*" is sometimes included when copying
                ## from Quora (when the content is in a
                ## list item and when it is in bold
                ## (two "*"s)).
                ##$lookUpTerm = substr_replace('*', '', 0, 0);
                #$lookUpTerm = preg_replace('/\*/', '', $lookUpTerm);

                #For more complicated replaces (regular expression)
                #$lookUpTerm = preg_replace('/$/', '&gt;', $lookUpTerm);
            }


            # Avoid warning messages for an empty input (at the
            # expense of some confusion)
            if ($lookUpTerm == '')
            {
                $lookUpTerm = 'php';
            }

            # This is the memory of past lookups. We only save the URLs
            # (list encoded as a text string in the form data) as they
            # are the only things that need to be remembered (at the
            # moment). The URLs are used for generating the checkin
            # message (in two different formats).
            #
            # An empty string is the default / start value and we don't
            # require it to be set (e.g. from the start HTML page, <>).
            #
            $URLlist_encoded = get_postParameter('URLlist_encoded') ?? '';

            if (array_key_exists('resetState', $_REQUEST))
            {
                $URLlist_encoded = "";  # Clear out the memory (between
                                        # pages, for the edit summary)
            }

            #echo "<p>Lookup term: $lookUpTerm</p>\n";

            $SQLprefix =
              " SELECT incorrectTerm, correctTerm, URL " .
              " FROM EditOverflow" .
              " WHERE incorrectTerm = "
              ;

            $pdo = connectToDatabase();

            # Some filtering of the lookup term: Remove
            # some leading and trailing characters.
            #
            # For now, only whitespace. But to match the
            # Windows application it should also exclude
            # punctuation from the lookup (but retain
            # it in the output).
            #
            $lookUpTerm = trim($lookUpTerm);

            # Primary lookup
            [$incorrectTerm, $correctTerm, $URL] =
                lookup($pdo, $lookUpTerm);

            # The trailing underscore ("_") is for the convention we
            # currently use for words in the alternative word set.
            #
            $lookUpTerm_stripped = rtrim($lookUpTerm, "_");
            $lookUpTerm_inMainWordSet = $lookUpTerm_stripped === $lookUpTerm;

            $alternative2 = "";

            # ===========================================================================
            # =                                                                         =
            # =    Generalised look up in alternative word lists                        =
            # =                                                                         =
            # ===========================================================================
            if (1)
            {
                # Strategy: Progressively, search using the current
                #           words in all word sets. Any (unique)
                #           found words are used as new input.
                #           And so on.
                #
                #           Stop when the number of iterations have reached
                #           some limit (should not normally happen). Or if
                #           the number of current words haven't changed
                #           after a sweep through the word sets (all
                #           'word paths' have been followed).

                # First seed: the lookup word (often a misspelling)
                #
                # Note: The past tense "looked" is somewhat
                #       a misnomer during execution. We both
                #       have words that have been looked up
                #       and words that haven't (yet).
                #
                $lookedUpWords[$lookUpTerm] = $URL;

                # Guard for too many database lookups
                $lookupTerm2IncorrectTerm = [];  # Is the lookup term the same
                                                  # as the incorrect term???
                $lookupTerm2CorrectTerm = [];
                $lookupTerm2URL = [];

                $linkID = 1002; # Expected by our Selenium regression test
                                # It is an arbitrary number, but it must
                                # be coordinated with the test.
                $iterations = 0;
                $done = 0;
                while (!$done)
                {
                    $oldSize = count($lookedUpWords);

                    # Note: It could be more efficient (it is potentially
                    #       a Shlemiel the painter’s algorithm). Though
                    #       in most cases there will only be one iteration.
                    #
                    #       We repeat lookups (that we don't need to).
                    #
                    foreach ($lookedUpWords as $someWord => $value)
                    {
                        #echo "(Test: " . $someWord . ") ";

                        $wordSets = 8;
                        $postFix = "";

                        # Note: For the search, we use the base form, without
                        #       the underscores. We then construct the lookup
                        #       terms (for each alternative word set) by
                        #       appending underscores.
                        #
                        #       Whereas in "$lookedUpWords" we keep the
                        #       actual lookup terms (they need to be
                        #       distinct between the word sets).
                        #
                        $someWord_stripped = rtrim($someWord, "_");

                        # Inner loop, checking each word set
                        for ($i = 0; $i < $wordSets; $i++)
                        {
                            $someLookUpTerm = $someWord_stripped . $postFix;

                            # Guard for too many database lookups
                            $someIncorrectTerm;
                            $someCorrectTerm;
                            $someURL;
                            if (array_key_exists($someLookUpTerm,
                                                  $lookupTerm2CorrectTerm))
                            {
                                # We cache the lookups, to avoid more
                                # database lookups than necessary.

                                $someIncorrectTerm =
                                    $lookupTerm2IncorrectTerm[$someLookUpTerm];

                                $someCorrectTerm =
                                    $lookupTerm2CorrectTerm[$someLookUpTerm];

                                $someURL =
                                    $lookupTerm2URL[$someLookUpTerm];
                            }
                            else
                            {
                                [$someIncorrectTerm, $someCorrectTerm, $someURL] =
                                    lookup($pdo, $someLookUpTerm);

                                # Note: We cache both successful and
                                #       unsuccessful lookups.
                                $lookupTerm2IncorrectTerm[$someLookUpTerm] =
                                    $someIncorrectTerm;

                                $lookupTerm2CorrectTerm[$someLookUpTerm] =
                                    $someCorrectTerm;

                                $lookupTerm2URL[$someLookUpTerm] =
                                    $someURL;
                            }

                            if ($someCorrectTerm)
                            {
                                # Something was found

                                # Record the first match, in case the primary
                                # lookup did not give a result.
                                #
                                # In effect: Transparently use some other (often
                                # the alternative) word set if the primary
                                # lookup fails (e.g., seemless lookup when
                                # blindly using a macro keyboard).
                                #
                                #Use some kind of indication it was found in the
                                #alternative word set?
                                #
                                if (! $correctTerm)
                                {
                                    #Later: use stripTrailingUnderscore()
                                    $correctTerm = $someCorrectTerm;

                                    $URL = $someURL;
                                }

                                if (array_key_exists($someCorrectTerm,
                                                      $lookedUpWords))
                                {
                                    # We already have it.

                                    # But we also need to fill in the URL
                                    $lookedUpWords[$someCorrectTerm] = $someURL;
                                }
                                else
                                {
                                    if ($someIncorrectTerm === $someCorrectTerm)
                                    {
                                        # It was found, but the input was
                                        # already a correct word.
                                        #
                                        # However, we still want to look it
                                        # up as an incorrect word in the
                                        # other word sets.
                                        #
                                        # Example:
                                        #
                                        #   XXXX
                                        #
                                        $lookedUpWords[$someCorrectTerm] = 1;
                                    }
                                    else
                                    {
                                        # If we got a new (correct) word,
                                        # schedule it for lookup (as an
                                        # incorrect word in another
                                        # word set)

                                        # '1': A placeholder. The URL is not
                                        # known until the lookup has been
                                        # performed
                                        $lookedUpWords[$someCorrectTerm] = 1;

                                    } # Input is not a correct word

                                    # For output to the result page:
                                    # Add to alternatives.
                                    if (1)
                                    {
                                        # Should we use $someCorrectTerm_stripped
                                        # here??

                                        if ($someCorrectTerm !== $correctTerm)
                                        {
                                            # For debugging only
                                            #$temp2 = "<br/> AT iteration $iterations: ";
                                            #$temp2 = "<br/>$iterations: ";
                                            $temp2 = "";

                                            if ($alternative2)
                                            {
                                                $alternative2 .= " and ";
                                            }

                                            $alternative2 .=
                                                $temp2 .
                                                alternativeLink(
                                                    $someIncorrectTerm,
                                                    $someCorrectTerm,
                                                    $linkID,
                                                    extractGrammaticalWordClass($someURL));

                                            $linkID++;
                                        }
                                    } # Block

                                } # A new word?

                            } # A database lookup hit

                            $postFix .= "_";
                        } # Searching for hits in other word sets.

                    } # Through $lookedUpWords

                    # Any new hits for words, going through all
                    # the word sets?
                    if (count($lookedUpWords) === $oldSize)
                    {
                        # No. We are done.
                        $done = 1;
                    }

                    # Mostly to detect and prevent an infinite
                    # loop. Normally it would exit by other
                    # means
                    $iterations++;
                    #echo " XXX iteration: $iterations YYY ";
                    if ($iterations > 20)
                    {
                        $done = 1;
                    }
                } # Iterating (outer) loop to find alternatives in other
                  # word lists (indeterminate; it depends on the
                  # content of the word list)
            } # Block: Generalised look up in alternative word lists


            if ($lookUpTerm_inMainWordSet)
            {
                $incorrectWordInTheOtherWordSet =  $lookUpTerm . "_";
                $correctWordInTheOtherWordSet   = $correctTerm . "_";
            }
            else
            {
                $incorrectWordInTheOtherWordSet = $lookUpTerm_stripped;
                $correctWordInTheOtherWordSet   = rtrim($correctTerm, "_");
            }

            # Also look up of the ***incorrect** term in the alternative
            # word set (the convention is a trailing underscore).
            #
            # Note: As there is automatically identity mapping
            #       in the database, we will also automatically
            #       look up incorrect words in the main word
            #       set that are ***correct*** words in the
            #       alternative word set...
            #
            #         Example:
            #
            #           XXX
            #
            # Look up the incorrect word in the other (often
            # the alternative) word set
            #
            [$incorrectTerm2, $correctTerm2, $URL2] =
                lookup($pdo, $incorrectWordInTheOtherWordSet);

            # Look up the correct word in the other (often
            # the alternative) word set
            #
            # Note that "$incorrectTerm3" is slightly confusing
            # as it will actually contain the correct term...
            #
            [$incorrectTerm3, $correctTerm3, $URL3] =
                lookup($pdo, $correctWordInTheOtherWordSet);

            if (1)  # No longer needed. We now have something
                    # similar in the generalised part
            {
                # Transparently use the other (often the alternative)
                # word set if the primary lookup fails (e.g.,
                # seemless lookup when blindly using a
                # macro keyboard).
                #
                #Use some kind of indication it was found in the
                #alternative word set?
                #
                if (! $correctTerm)
                {
                    #Later: use stripTrailingUnderscore()
                    $correctTerm = $correctTerm2;
                    #$correctTerm = $correctTerm2 . "XXXXXXXXXXXXXXXXXX"; # Will break the Selenium integration test...

                    $URL = $URL2;
                }
            }

            #echo "\n\nEffective correct term: $correctTerm \n\n\n\n";

            # To avoid "Undefined variable: linkYouTubeCompatible"
            # in the PHP error log file.
            $linkYouTubeCompatible = "";
            $link_HTML = "";
            $grammaticalWordClass = "";
            $link_WikiMedia = "";
            $link_WikiMedia_External = "";
            $correctionComment = "";
            $linkWordListWithPageAnchor = "";

            # Avoid "Undefined variable: editSummary_output",
            # etc. if the lookup of the term failed.
            #
            # Or should we simply use a default value
            # instead (now redundancy with the below)?
            $editSummary_output  = "";
            $editSummary_output2 = "";
            $linkInlineMarkdown = "";

            # Report the incorrect and/or the correct word
            # if found in the alternative word set.
            #
            # 1002 and 1003: Two arbitrary unique IDs. They don't
            #                actually need to be integers.
            #
            $alternative = "";
            if ($correctTerm2 &&
                ($correctTerm2 != $correctTerm)) # Only
            {
                # Note: $URL2 is not used as a link because we link to a
                #       ***new*** Edit Overflow page where the lookup
                #       will take place and the URL be displayed...
                #
                $alternative .= alternativeLink(
                                    $incorrectTerm2,
                                    $correctTerm2,
                                    1002,
                                    extractGrammaticalWordClass($URL2));
            }
            if ($correctTerm3 &&
                ($correctTerm3 != $correctTerm2))
            {
                if ($alternative)
                {
                    $alternative .= " and ";
                }
                # Note: $URL3 is not used because we link to a ***new***
                #       Edit Overflow page where the lookup will
                #       take place and the URL be displayed...
                #
                $alternative .= alternativeLink(
                                    $incorrectTerm3,
                                    $correctTerm3,
                                    1003,
                                    extractGrammaticalWordClass($URL3));
            }

            #$alternative .= "<br><br>TRACE: " . $alternative2; # For now. For debugging.
            $alternative = $alternative2;

            # Encode for display, e.g. "&" as "&amp;" and "<" as "&lt;".
            #
            # Note: Not for $alternative as it is actual raw HTML.
            #
            $correctTerm = htmlentities($correctTerm, ENT_QUOTES);
            $URL = htmlentities($URL);

            # True if the (incorrect) term was found in our
            # huge list (as of 2020-10-29, 14852 items)
            if ($correctTerm)
            {
                # If the URL is a Wiktionary one, extract the
                # (grammatical) word class and display it
                # close to the correct word.
                $grammaticalWordClass = extractGrammaticalWordClass($URL);

                # Add to our built-up edit summary string (using
                # the carried-over state and our new lookup)

                # Update our memory (for the edit summary) - it will be
                # output in the generated HTML in a hidden form field
                # ("URLlist_encoded". See below in the HTML form part,
                # near 'name="URLlist_encoded"').
                #
                $URLlist_encoded = $URLlist_encoded . "____" . $URL;  #Note: we get a leading "____"
                                                                      #      for the first item (and
                                                                      #      thus an empty entry when
                                                                      #)     decoding it).

                $linkInlineMarkdown = "[$correctTerm]($URL)";

                # We preformat it in the most commonly used form in YouTube
                # comments:
                #
                #     1. Styled as ***bold*** (by using "*").
                #
                #        We use bold instead of italics (by "_") as Wikipedia
                #        URLs often contain underscores and thus the
                #        formatting would not work with italics).
                #
                #     2. Used indented on a separate line in a comment with
                #        a list of timestamps. Note: It is indented deeper
                #        than the text of a timestamp; this is intentional.
                #
                # Example: <https://www.youtube.com/watch?v=NZZbl-lfokQ&lc=Ugxi74factISUTWaDSd4AaABAg>
                #
                # Possible future: Should we also include a newline at the end?
                #
                $linkYouTubeCompatible =
                    "                 *" .
                    transformFor_YouTubeComments($URL) .
                    "*"
                    ;

                # Link, in HTML format/syntax
                #
                #Note: We already have a utility function for this, get_HTMLlink(),
                #      but the escaping of double quotes is in doubt. We changed
                #      it from "&quot;" to "%22" (in get_HTMLattributeEscaped()).
                #
                #      Is it WordPress that interferes?
                #
                #      That does not work in this case - we get the literal "%22"
                #      in the (user facing) output.
                #
                #      It ought to be resolved, so we don't have
                #      this redundancy
                #
                #$link_HTML = get_HTMLlink($correctTerm,
                #                          $URL,
                #                          "");
                #
                # Using double quotes in the form may result in "%22" due
                # to our own function, get_HTMLattributeEscaped().
                #
                $link_HTML =
                  "<a href=\"" . $URL . "\"" .
                  ">" . $correctTerm . "</a>";

                # Link, in MediaWiki format, internal (e.g., for Wikipedia)
                #
                # We preformat it in italics (two single quotes on Wikipedia)
                # as we will often want to indicate it is literal (e.g.,
                # on a talk page).
                #
                $link_WikiMedia =
                    "''" .
                    WikiMedia_Link($URL, $correctTerm) .
                    "''"
                    ;

                $link_WikiMedia_External =
                    "''" .
                    WikiMedia_Link_External($URL, $correctTerm) .
                    "''"
                    ;

                # $correctionComment_Quora =
                #   "It is \"" . $correctTerm . "\" (not \"" .
                #   $lookUpTerm . "\"). See e.g.: " . $URL .
                #   ". You can edit your question/comment/answer/post.";
                $correctionComment_Markdown =
                  "It is *\"[" . $correctTerm . "](" . $URL .
                  ")\"* in this context " .
                  "(not *\"" . $lookUpTerm . "\"*). " .
                  "You can edit (change) your question/comment/answer/post";

                $RedditEditInstructions_Markdown =
                  "*\"...\"* (to the right of *\"Share\"*) &rarr; " .
                  "*\"Edit comment\"*";

                $noEditMetaTalk_Markdown =
                  "Please, for such a minor change, " .
                  "*\"Edit\"*, *\"Update\"*, or similar is " .
                  "***[not](https://meta.stackexchange.com/a/131011)***" .
                  " required (near *\"Changelogs\"*).";

                # Isolation from the rest
                $correctionComment =
                    $correctionComment_Markdown .
                    " (" . $RedditEditInstructions_Markdown . "). " .
                    $noEditMetaTalk_Markdown;

                # Cross reference to the word list (in HTML format)
                # in the result, with a page anchor
                #
                # We need change or remove some of the content in the page
                # anchors to both conform to HTML rules and to reduce
                # complexity, corresponding to the transformations in
                # addTermsToOutput_HTML() in file <TermLookup.cs>.
                #
                $replacer = new StringReplacerWithRegex($correctTerm);

                # HTML with accept space in page anchor (e.g.,
                # it doesn’t validate)
                $replacer->transform(' ', '_');

                # "&" is encoded as "&amp;" at this point...
                $replacer->transform('&amp;nbsp;', '_');

                # Remove double quotes. Though we currently only have one
                # instance in the entire word set (for the correct word)...:
                #
                #     eval (near "*The args are read and concatenated together"*)_
                #
                # They are encoded as "&quot;" at this point...
                $replacer->transform('&quot;', '');

                # Remove asterisks.
                $replacer->transform('\*', '');

                $WordListPageAnchor = $replacer->currentString();

                $linkWordListWithPageAnchor =
                  "https://pmortensen.eu/" .
                  "EditOverflow/_Wordlist/EditOverflowList_latest.html" .
                  "#$WordListPageAnchor";
            } # Term lookup succeeded


            $items = preg_split('/____/', $URLlist_encoded);

            $items = array_filter($items); # Get rid of empty elements

            # Wrap each item in "<>" (URL encoded)
            $items = substr_replace($items, '&lt;', 0, 0);  # That is, insert "&lt;" at
                                                            # the beginning of the
                                                            # element of the array
            $items = preg_replace('/$/', '&gt;', $items);
            $elements = count($items);


            # It is simple for the current Stack Overflow edit
            # summary style - just separate each item by a space
            $URLlist = implode(' ', $items);


            # For normal edit summary, outside Stack Overflow -
            # with the Oxford comma!
            #
            $URLlist2 =
                join(', and ',
                      array_filter(
                          array_merge(
                              array(
                                  join(', ',
                                        array_slice($items, 0, -1))),
                                        array_slice($items, -1)), 'strlen'));

            # Revert: Adjust for two elements (no comma)
            if ($elements == 2)
            {
                $URLlist2 = str_replace(', and', ' and', $URLlist2);
            }

            #echo "<p>URLlist: xxx"  . $URLlist  . "xxx <p>\n";
            #echo "<p>URLlist2: xxx" . $URLlist2 . "xxx <p>\n";

            if ($URLlist != '')
            {
                # Derived
                $editSummary_output = "Active reading [" . $URLlist . "].";

                $editSummary_output2 = "Copy edited (e.g. ref. $URLlist2).";

                ## Current compensation for not being sophisticated when generating
                ## a list...
                #$editSummary_output = str_replace(' ]', ']', $editSummary_output);
            }

            # If the correct term can not be looked up, we substitute
            # with the incorrect term. One reason for this is the use
            # of automatic tools, like a macro keyboard, that blindly
            # applies a lookup. In this way the term in the original
            # place, e.g. an edit window will not be blanked out,
            # but effectively left as it was.
            #
            #What about empty input??
            if ($correctTerm)
            {
                $effectiveTerm = $correctTerm;
            }
            else
            {
                #$itemValue .= "..."; # To force the form input
                #                     # field to not be empty

                # Fill in the corrected field even though the lookup
                # failed. Justification: So an automatic process,
                # like a macro keyboard, will not overwrite the
                # origin (e.g. in an edit field in a
                # web browser tab).
                #
                # An alternative could be to append some text
                # to indicate failure (so we achieve not
                # overwriting, but don't pretend to have
                # succeeded).
                #
                # E.g., it could be subtle, like a few extra
                # spaces. Or an HTML comment (works for the
                # common use case).
                #
                $effectiveTerm = $lookUpTerm;
            }
        ?>

        <script src="EditOverflowList.js"></script>

        <script src="EditOverflow.js"></script>

        <script>
            /* Only if we want the lookup for the default input */
            window.onload = function() {
                /* document.lookupForm.action = get_action(); */
            }
        </script>

        <form
            name="lookupForm"
            method="post"
            id="lookupForm"
            <?php
                if (useJavaScriptLookup())
                {
                    #Aparrently also needed...
                    #
                    #But it contradicts earlier conclusions that it
                    #was only needed for the submit button...
                    #
                    #What is up???

                    echo "onsubmit=\"return get_action(); return false;\"\n";
                }

                # For proper indent in the generated HTML - regardless
                # of the return value of useJavaScriptLookup() or
                # whether we actually output anything in PHP.
                echo "\n";
            ?>
        >

            <!--
                For manually inserting it above as an attribute
                to "form" - as we can't have HTML comments
                inside an HTML tag ("form" in this case).

                But it is not required for our lookup to work
                (only for the initial opening of the page(?)) -
                just doing the same for the submit button (below)
                is sufficient.

                onsubmit="return get_action(); return false;"
            -->

            <div class="formgrid">

                <!-- Conditional: Lookup failure -->
                <?php
                    # Lookup failure... Report it and offer links for
                    # look up on Wikipedia and Wiktionary.
                    #
                    if (!$correctTerm)
                    {
                        $startDivWithIndent =
                          "$EOL_andBaseIndent" .
                          "<div>" .
                          $EOL_andBaseIndent_sub;
                        $endDivWithIndent =
                          "$EOL_andBaseIndent" .
                          "</div>" .
                          $EOL_andBaseIndent;

                        echo $startDivWithIndent;
                        echo
                          "<strong>Could not look up \"$lookUpTerm\"!" .
                          "</strong>";
                        echo $endDivWithIndent;

                        echo $startDivWithIndent;

                        # Refactoring opportunity: some redundancy

                        # Provide a link to look up the term on Wikipedia
                        #echo
                        #  "<a " .
                        #  "href=\"" .
                        #    "https://duckduckgo.com/html/?q=" .
                        #    "Wikipedia%20$lookUpTerm\"$EOL_andBaseIndent" .
                        #  "   accesskey=\"W\"$EOL_andBaseIndent" .
                        #  "   title=\"Shortcut: Shift + Alt + V\"$EOL_andBaseIndent" .
                        #  ">Look up on <strong>W</strong>ikipedia</a>" .
                        #  $EOL_andBaseIndent;

                        #Hint turned off while it is broken (see below).
                        #$linkText = "Look up on <strong>W</strong>ikipedia";
                        $linkText = "Look up on Wikipedia";

                        $WikipediaSearch_URL = "https://duckduckgo.com/html/?q=" .
                                "Wikipedia%20$lookUpTerm"
                                ;
                        $extraAttributes =

                          #Turned off while it is broken (both
                          #inconsistent ('W' vs. 'V') and
                          #conflicts with  another
                          #keyboard shortcut)
                          #$EOL_andBaseIndent_sub .
                          #"   accesskey=\"W\"$EOL_andBaseIndent_sub" .
                          #"   title=\"Shortcut: Shift + Alt + V\"" .
                          $EOL_andBaseIndent_sub
                          ;

                        #Though it should probably be encoded
                        #somehow (for example, spaces)...
                        $Wikipedia_URL_constructed =
                            "https://en.wikipedia.org/wiki/$lookUpTerm";

                        #Though it should probably be encoded
                        #somehow (for example, spaces)...
                        $Wikipedia_URL_constructed_disambig =
                            "https://en.wikipedia.org/wiki/$lookUpTerm" .
                            "_(disambiguation)";

                        $WiktionarySearch_URL =
                            "https://duckduckgo.com/html/?q=" .
                            "Wiktionary%20$lookUpTerm";

                        #Though it should probably be encoded
                        #somehow (for example, spaces)...
                        $Wiktionary_URL_constructed =
                            "https://en.wiktionary.org/wiki/$lookUpTerm";

                        $linkPart = get_HTMLlink($linkText,
                                                  $WikipediaSearch_URL,
                                                  $extraAttributes);

                        #echo $EOL_andBaseIndent_sub;
                        echo $linkPart . "\n" . $EOL_andBaseIndent_sub;

                        # Provide a link to look up the term on Wiktionary
                        #
                        # Note: Wiktionary does not accept these
                        #       characters in (constructed) URLs:
                        #
                        #           # < > [ ] { } | �
                        #
                        #       It does redirect to the "Bad title" page,
                        #       with a collection of entries containing
                        #       those characters, e.g., "<3":
                        #
                        #         <https://en.wiktionary.org/wiki/%3C3>
                        #
                        #       Perhaps use the page anchor instead (for
                        #       a less confusing result) if we detect
                        #       one of the characters? -
                        #
                        #         <https://en.wiktionary.org/wiki/%3C3#Unsupported_symbols>
                        #
                        #echo
                        #  "<a " .
                        #  "href=\"" .
                        #    "https://duckduckgo.com/html/?q=" .
                        #    "Wiktionary%20$lookUpTerm\"$EOL_andBaseIndent" .
                        #  "   accesskey=\"K\"$EOL_andBaseIndent" .
                        #  "   title=\"Shortcut: Shift + Alt + K\"$EOL_andBaseIndent" .
                        #  ">Look up on Wi<strong>k</strong>tionary</a>";

                        echo
                          get_HTMLlink(
                              #Hint turned off while it is broken
                              #(conflicts with another keyboard
                              #shortcut).
                              #"Look up on Wi<strong>k</strong>tionary",
                              "Look up on Wiktionary",

                              $WiktionarySearch_URL,

                              #Turned off while it is broken (conflicts
                              #with another keyboard shortcut)
                              #$EOL_andBaseIndent_sub .
                              #"   accesskey=\"K\"$EOL_andBaseIndent_sub" .
                              #"   title=\"Shortcut: Shift + Alt + K\"" .
                              $EOL_andBaseIndent_sub
                          ) .
                          "\n" . $EOL_andBaseIndent_sub;

                        # Constructed URLs for Wikipedia and
                        # Wiktionary (that may or may not
                        # lead to a usable page).
                        echo
                          get_HTMLlink(
                              "Wikipedia (constructed)",
                              $Wikipedia_URL_constructed,
                              $EOL_andBaseIndent_sub
                          ) .
                          "\n" . $EOL_andBaseIndent_sub;
                        echo
                          get_HTMLlink(
                              "Wikipedia (disambiguation page)",
                              $Wikipedia_URL_constructed_disambig,
                              $EOL_andBaseIndent_sub
                          ) .
                          "\n" . $EOL_andBaseIndent_sub;
                        echo
                          get_HTMLlink(
                              "Wiktionary (constructed)",
                              $Wiktionary_URL_constructed,
                              $EOL_andBaseIndent_sub
                          ) .
                          ""; # Empty because it is the last of the
                              # four links (to avoid an empty line
                              # in the HTML source).

                        echo $endDivWithIndent;

                        # Empty div, for first column in CSS grid
                        echo $startDivWithIndent;
                        echo $endDivWithIndent;

                        #No, not for now. But do consider it.
                        # In case of failure, blank out the usual
                        # (lookup result) items (to keep our mostly
                        # static HTML here in this PHP file), start.
                        # echo "\n$EOL_andBaseIndent" .
                        #    "<!--";
                    } # Failed lookup
                ?>

                <label for="LookUpTerm"><u>L</u>ook up term</label>
                <input
                    name="LookUpTerm"
                    type="text"
                    id="LookUpTerm"
                    class="XYZ3"
                    <?php the_formValue($lookUpTerm); ?>
                    style="width:170px;"
                    accesskey="L"
                    title="Shortcut: Shift + Alt + L"
                    autofocus
                >

                <p></p>

                <!-- Note:  we keep the same identifier "CorrectedTerm",
                            even if the lookup fails.
                -->
                <label for="CorrectedTerm">
                    <?php
                        #What about empty input??

                        # See below for an explanation (near
                        # "Fill in the corrected field")
                        if ($correctTerm)
                        {
                            #Note: Missing indent in HTML source
                            #      (should be fi)

                            echo "<u>C</u>orrected term"; #Used to be static HTML.

                            # Indicate if an incorrect or correct word was
                            # looked up. The rtrim is for identical words
                            # from different word sets to test equal.
                            if (rtrim($lookUpTerm, "_") ===
                                rtrim($effectiveTerm, "_"))
                            {
                                echo " (the same)";
                            }
                        }
                        else
                        {
                            # We really need to make it very apparent that
                            # the lookup failed. But we should find
                            # something more appropriate - perhaps
                            # change the background colour of
                            # the form field.

                            #echo "Incorrect term (repeated)";
                            echo "<hr/><hr/><hr/>";
                            echo "<em><strong>Look up term (repeated)</strong></em>\n";
                        }
                        echo "\n";
                    ?>
                </label>

                <input
                    name="CorrectedTerm"
                    type="text"
                    id="CorrectedTerm"
                    class="XYZ10"
                    <?php
                        # The extra trailing space in the output is for
                        # presuming the lookup term contains a trailing
                        # space.
                        #
                        # This is only until we will use something more
                        # sophisticated (like in the Windows desktop
                        # version of Edit Overflow) - where we preserve
                        # any leading and trailing white space.
                        #
                        the_formValue($effectiveTerm . " ");
                    ?>
                    style="width:170px;"
                    accesskey="C"
                    title="Shortcut: Shift + Alt + C"
                >


                <!-- <p class="grammaticalWordClass">Some word class</p> -->
                <!-- <p>e</p> -->
                <!-- <p></p> -->
                <!-- Some word class -->
                <?php
                    echo "<p class=\"grammaticalWordClass\">$grammaticalWordClass</p>"
                ?>

                <?php
                    # Optional: A "line" with one or more alternatives
                    #           to the main lookup result (from other
                    #           word sets)

                    if ($alternative)
                    {
                        echo "<label for=\"CorrectedTerm\">Alternatives</label>\n";
                        echo "                " . # Internal indent (don't we have a variable for this???)
                              "<p class=\"entry-line\">$alternative</p>\n" .
                              "<p></p>";
                    }
                ?>

                <label for="editSummary_output">Check-in <u>m</u>essages</label>
                <input
                    name="editSummary_output"
                    type="text"
                    id="editSummary_output"
                    class="XYZ4"
                    <?php the_formValue($editSummary_output); ?>
                    style="width:600px;"
                    accesskey="M"
                    title="Shortcut: Shift + Alt + M"
                >

                <p></p>

                <label for="editSummary_output2"><b></b></label>
                <input
                    name="editSummary_output2"
                    type="text"
                    id="editSummary_output2"
                    class="XYZ89"
                    <?php the_formValue($editSummary_output2); ?>
                    style="width:600px;"
                    accesskey="O"
                    title="Shortcut: Shift + Alt + O"
                >

                <p></p>

                <?php
                  # Note 1: We can not (currently) break it into several
                  #         lines due to limitations in the check script
                  #         <KeyboardShortcutConsistency.pl>...
                  #
                  # Note 2: If we change the 'label' line, then the
                  #         exception in script KeyboardShortcutConsistency.pl
                  #         must be changed as well...
                  #
                  #         Near "$formLabelText eq" (e.g., line 572)
                  #
                  #         And possible web regression script
                  #         'web_regress.py', depending on
                  #         what is changed.
                  #
                  #         Near  'linkText_wordListReference'
                ?>

                <label for="URL"><?php echo get_HTMLlink("URL.", $URL, "") ?> <?php echo get_HTMLlink("Words.", $linkWordListWithPageAnchor, ' id="1004"') ?></label>
                <input
                    name="URL"
                    type="text"
                    id="URL"
                    class="XYZ11"
                    <?php the_formValue($URL); ?>
                    style="width:400px;"
                    accesskey="E"
                    title="Shortcut: Shift + Alt + E"
                >

                <p></p>

                <label for="URL2">Link (<u>i</u>nline Markdown)</label>
                <input
                    name="URL2"
                    type="text"
                    id="URL2"
                    class="XYZ90"
                    <?php the_formValue($linkInlineMarkdown); ?>

                    style="width:400px;"
                    accesskey="I"
                    title="Shortcut: Shift + Alt + I"
                >

                <p></p>

                <label for="URL3">Link (<u>Y</u>ouTube compatible)</label>
                <input
                    name="URL3"
                    type="text"
                    id="URL3"
                    class="XYZ91"
                    <?php the_formValue($linkYouTubeCompatible); ?>

                    style="width:400px;"
                    accesskey="Y"
                    title="Shortcut: Shift + Alt + Y"
                >

                <p></p>

                <label for="URL4">Link (<u>H</u>TML)</label>
                <input
                    name="URL4"
                    type="text"
                    id="URL4"
                    class="XYZ92"

                    <?php
                        #Note: We should eliminate the redundancy (introduced
                        #      due to problems with escaping double quotes) -
                        #      see get_HTMLattributeEscaped() for details.


                        # the_formValue($link_HTML);
                        #
                        ## Direct, using single quotes, so we don't
                        ## have to escape neither in PHP nor in HTML
                        ## (but it fails for correct terms containing
                        ## single quotes, e.g. "don't"):
                        ##
                        #echo "value='$link_HTML'\n";

                        # Using "&quot;", but see notes near "$link_HTML" above.
                        #
                        $link_HTML_encoded = str_replace('"', '&quot;', $link_HTML);
                        echo "value=\"$link_HTML_encoded\"\n";
                    ?>
                    style="width:400px;"
                    accesskey="H"
                    title="Shortcut: Shift + Alt + H"
                >

                <p></p>

                <label for="URL6">Link (MediaWi<u>k</u>i)</label>
                <input
                    name="URL6"
                    type="text"
                    id="URL6"
                    class="XYZ94"

                    <?php
                        #Note: We should eliminate the redundancy (introduced
                        #      due to problems with escaping double quotes) -
                        #      see get_HTMLattributeEscaped() for details.


                        # the_formValue($link_HTML);
                        #
                        ## Direct, using single quotes, so we don't
                        ## have to escape neither in PHP nor in HTML
                        ## (but it fails for correct terms containing
                        ## single quotes, e.g. "don't"):
                        ##
                        #echo "value='$link_HTML'\n";

                        # Using "&quot;", but see notes near "$link_HTML" above.
                        #
                        $link_WikiMedia_encoded = str_replace('"', '&quot;', $link_WikiMedia);
                        echo "value=\"$link_WikiMedia_encoded\"\n";
                    ?>
                    style="width:400px;"
                    accesskey="K"
                    title="Shortcut: Shift + Alt + K"
                >

                <p></p>

                <label for="URL7">Link (MediaWiki, e<u>x</u>ternal)</label>
                <input
                    name="URL7"
                    type="text"
                    id="URL7"
                    class="XYZ95"

                    <?php
                        #Note: We should eliminate the redundancy (introduced
                        #      due to problems with escaping double quotes) -
                        #      see get_HTMLattributeEscaped() for details.


                        # the_formValue($link_HTML);
                        #
                        ## Direct, using single quotes, so we don't
                        ## have to escape neither in PHP nor in HTML
                        ## (but it fails for correct terms containing
                        ## single quotes, e.g. "don't"):
                        ##
                        #echo "value='$link_HTML'\n";

                        # Using "&quot;", but see notes near "$link_HTML" above.
                        #
                        $link_WikiMedia_External_encoded = str_replace('"', '&quot;', $link_WikiMedia_External);
                        echo "value=\"$link_WikiMedia_External_encoded\"\n";
                    ?>
                    style="width:400px;"
                    accesskey="X"
                    title="Shortcut: Shift + Alt + X"
                >

                <p></p>

                <label for="URL5">Correction commen<u>t</u></label>
                <input
                    name="URL5"
                    type="text"
                    id="URL5"
                    class="XYZ93"

                    <?php
                        # Using "&quot;", but see notes near "$link_HTML" above.
                        #
                        $correctionComment_encoded = str_replace('"', '&quot;', $correctionComment);
                        echo "value=\"$correctionComment_encoded\"\n";
                    ?>

                    style="width:400px;"
                    accesskey="T"
                    title="Shortcut: Shift + Alt + T"
                >

                <p></p>

                <?php
                    #No, not for now. But do consider it.
                    # At lookup failure, blank out the usual
                    # items (to keep our mostly static HTML),
                    # end.
                    if (!$correctTerm)
                    {
                        # echo "-->\n";
                    }
                ?>

                <!-- Hidden field, structured format for remembering the
                      items for the edit summary (currently the list
                      of associated URLs for the successful term lookups) -->
                <input
                    name="URLlist_encoded"
                    type="hidden"
                    id="URLlist_encoded"
                    class="XYZ6"
                    <?php the_formValue($URLlist_encoded); ?>
                >


                <!-- Reset lookup / edit summary state  -->
                <input
                    name="resetState"
                    type="checkbox"
                    id="resetState"
                    class="XYZ9"
                    accesskey="R"
                    title="Shortcut: Shift + Alt + R"
                >
                <label for="resetState"><u>R</u>eset lookup state</label>


                <!-- Submit button  -->
                <!-- For 'value' (the displayed text in the button), tags 'u'
                      or 'strong' do not work!! -->

                <!--
                    action="#"
                -->

                <input
                    name="LookUp"
                    type="submit"
                    id="LookUp"
                    class="XYZ12"
                    value="Look up"
                    style="width:90px;"
                    accesskey="U"
                    title="Shortcut: Shift + Alt + U"

                    <?php
                        if (useJavaScriptLookup())
                        {
                            echo "onsubmit=\"return get_action(); return false;\"";
                        }

                        # For proper indent in the generated HTML - regardless
                        # of the return value of useJavaScriptLookup() or
                        # whether we actually output anything in PHP.
                        echo "\n";
                    ?>
                >

                <!--
                    For ***manually*** inserting it above as an
                    attribute to the submit button - as we
                    can't have HTML comments inside an
                    HTML tag ("form" in this case).

                    The ***only*** thing required to get the
                    lookup to happen in JavaScript (on the
                    client side (in the browser)) is to
                    insert this for lookup button (just above):

                        onclick="get_action(); return false;"

                    But JavaScript must be enabled in the browser
                    for it to work - e.g. allowing it in NoScript
                    in Firefox!!!!
                -->

            </div>

        </form><?php the_EditOverflowFooter('EditOverflow.php', $effectiveTerm, $URL); ?>


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
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FEditOverflow.php"
                accesskey="W"
                title="Shortcut: Shift + Alt + W"
            >HTML validation</a>.
            <a
                href="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FEditOverflow.php%3FLookUpTerm=don%27t%26OverflowStyle%3DNative"
                accesskey="V"
                title="Shortcut: Shift + Alt + V"
            >HTML validation (no WordPress)</a>.
        </p>

        <p>Proudly and unapologetic powered by PHP!</p>

        <?php
            # $databaseCalls = 37; # Stub

            $endTime_secs = microtime(true);
            $elapsedTime_secs = $endTime_secs - $startTime_secs; # Seconds, fractional
            printf("<!-- Page rendered in %.3f seconds. End: %.3f seconds (Unix time). Database lookups: %s -->\n",
                    $elapsedTime_secs, $endTime_secs, $databaseCalls);
            echo "\n";
        ?>

<?php the_EditOverflowEnd() ?>
