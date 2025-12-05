#!perl
#!/usr/bin/perl

#############################################################################
#                                                                           #
# Copyright 2021     Peter Mortensen                                        #
#                                                                           #
# Purpose:  Detect inconsistencies wrt. keyboard shortcuts                  #
#           in form HTML/PHP files, in particular Edit                      #
#           Overflow for Web:                                               #
#                                                                           #
#             1. Conflicting (double) keyboard shortcuts                    #
#                                                                           #
#             2. Inconsistency between tooltip text (with                   #
#                information about what the shortcut is)                    #
#                and the actual keyboard shortcut                           #
#                                                                           #
#           Also:                                                           #
#                                                                           #
#             3. Indentation consistency                                    #
#                                                                           #
#             3. Presumed/required order of lines with the information      #
#                                                                           #
#             4. Output of unassigned keys (optional)                       #
#                                                                           #
#             5. Uniqueness of some form attributes, e.g. "id" and "name"   #
#                                                                           #
#############################################################################

#############################################################################
#                                                                           #
#                   Find Your Inner Bird Technologies                       #
#                            Peter Mortensen                                #
#                  E-mail: spamMzzayGoHere@pmortensen.eu                    #
#                        WWW: http://pmortensen.eu/                         #
#                                                                           #
#  Program for ......................................                       #
#                                                                           #
#    FILENAME:   KeyboardShortcutConsistency.pl                             #
#                                                                           #
# CREATED: PM 2021-05-15   Vrs. 1.0.                                        #
# UPDATED: PM 2021-05-16   Added (optional) output of unassigned            #
#                          keyboard shortcut. In this version               #
#                          configured by changing this script               #
#                                                                           #
#             2021-06-16   Now detects if a file name is supplied on        #
#                          the command line (standard input is NOT          #
#                          accepted as input) and that it exists.           #
#                                                                           #
#                          No longer outputs anything with empty input      #
#                          or if there is an error (so it is easier         #
#                          to spot if something went wrong).                #
#                                                                           #
#             2021-07-30   Added general check of uniqueness of             #
#                          some forms fields.                               #
#                                                                           #
#             2022-01-09   Added check of keyboard shortcut indications     #
#                          inside links (where they most probably will      #
#                          be invisible).                                   #
#                                                                           #
#                          Added check of consistency between keyboard      #
#                          shortcut indications and the actual              #
#                          shortcuts (and if the indication                 #
#                          exists at all).                                  #
#                                                                           #
#             2022-05-20   Added check of escape of double                  #
#                          quote in HTML form elements.                     #
#                                                                           #
#             2023-01-06   An exception changed (due to adding a feature    #
#                          in Edit Overflow lookup output).                 #
#                                                                           #
#             2023-02-13   Improved error message for when the error is     #
#                          caused by changed PHP source that has a          #
#                          a hardcoded exception in this script.            #
#                                                                           #
#             2024-05-11   Now insists on HTTPS links.                      #
#                                                                           #
#             2024-06-10   Refinement of detecting HTTP links, e.g.,        #
#                          avoiding some false positives.                   #
#                                                                           #
#                          Added exceptions for the remaining 11            #
#                          HTTP-only links in the word list. And            #
#                          we now effectively have detection of             #
#                          new HTTP links in the build script               #
#                          (that calls this script).                        #
#                                                                           #
#             2024-06-14   Now rejects "//" as the comment                  #
#                          character sequence                               #
#                                                                           #
#             2024-07-31   Added an exception for an HTTP link              #
#                                                                           #
#############################################################################

# Future:
#
#   1. Detect TAB characters
#
#   2. Require some fields to be equal. Or even that they should not be.
#
#      Note that we now use "id" for anchors and thus may want to
#      give them meaningful names instead of (arbitrary) numbers.
#
#      Example:
#
#        name="Non-breaking space"
#        id="Non-breaking_space"
#        class="XYZ32"
#
#   3. XXXX


use strict;

#use warnings;
use warnings FATAL => 'uninitialized'; # Stop on "Use of uninitialized value"
                                       # instead of continuing - see
                                       # <https://stackoverflow.com/questions/77954>

                   # use warnings FATAL => 'all';
                   # <https://stackoverflow.com/questions/8023959/why-use-strict-and-warnings>

use diagnostics;


# Configuration, start

    my $outputKeyboardShortcutInfo = 1;

# Configuration, end


my $exitCode = 0; # Default: no error
my $errors = 0; # For statistics


my %assignedKeyboardShortcut = ();

my %HTTP_URLs = ();


# We only use the key in this hash
my %keyboardShortcutSet =
(
    'A',  1,
    'B',  2,
    'C',  3,
    'D',  4,
    'E',  5,
    'F',  6,
    'G',  7,
    'H',  8,
    'I',  9,
    'J', 10,
    'K', 11,
    'L', 12,
    'M', 13,
    'N', 14,
    'O', 15,
    'P', 16,
    'Q', 17,
    'R', 18,
    'S', 19,
    'T', 20,
    'U', 21,
    'V', 22,
    'W', 23,
    'X', 24,
    'Y', 25,
    'Z', 26

    #'XXX', 42, # For the comma...
);


# We use the value for each key as a list (hash) of values
# that we encounter in the input file for that key (HTML
# attribute/field).
#
# For the secondary hash, we store the linenumber
# (so we can report both line numbers of
# conflicting (non-unique) values)
#
my %uniqueFields =
(
    'class',      {},
    'id',         {},
    'name',       {},

    #'title',      {},   # <Same as accesskey.>

    #'accesskey',  {},   # Not for now. Possible special
                         # treatment for an empty value.

    'value',      {},
);

#my %fields = ();


# We don't want it empty or undefined as we depend on it later on.
my $kValuePreset = "<Not set>";

my $line  = 0;
my $accessKey = ""; # "accesskey" is the HTML form attribute
                    # name for the keyboard shortcut.

my $name = ""; # "name" is an HTML form element attribute

my $value = $kValuePreset; # "value" is the HTML form attribute
                           # name for the actual content of the
                           # HTML form element.

# This is the indicated keyboard shortcut in
# the ***label*** for the form element,
# by ***our own convention***, single
# letter wrapped in underline, "<u></u>".
my $indicatedKeyboardShortcut = $kValuePreset;

my $formLabelText             = $kValuePreset;

# In "<a>" - that also can have a keyboard shortcut
my $hrefAttribute             = $kValuePreset;


my %accesskeys = ();

my $detectedShortcuts = 0; # Including duplicates, if any.

my $filename = $ARGV[0];


my $proceedWithMainProcessing = 1; # Default

if (! $filename)
{
    # While making it less flexible, it
    # also prevents some user errors.
    print "This script should be invoked with an actual file... " .
          "(standard input is not supported)\n\n";

    $proceedWithMainProcessing = 0;

    $exitCode = 6;
    $errors++; # Some redundancy here... Incl. the double "\n"
}

# File existence
if (! -e $filename)
{
    # This may prevent unusable output (so it is easier to spot)
    print
      "The script could not proceed. The file $filename does not " .
      "exist (for example, not in the current directory). " .
      "Invoke the script an existing file...\n\n";
    $proceedWithMainProcessing = 0;

    $exitCode = 7;
    $errors++; # Some redundancy here...
}


if ($proceedWithMainProcessing)
{
    print "File name: $filename\n\n";

    while (<>)
    {
        chop;

        $line++;
        my $currentlLine = $_; # Because iterating through the
                               # hash (below) will overwrite
                               # $_...
        my $len = length;
        #print "$len - >>>$currentlLine<<<\n\n";

        my $leadingSpace = "";
        if (/^(\s+)/)
        {
            $leadingSpace = $1;
        }
        my $leadingSpaces = length $leadingSpace;
        #print "$leadingSpaces leading spaces - >>>$currentlLine<<<\n\n";

        my $hasUnevenSpaces = $leadingSpaces % 2;
        #print "Uneven: $leadingSpaces leading spaces - >>>$currentlLine<<<\n\n"
        #  if $hasUnevenSpaces;

        # That is the indentation check...
        if ($hasUnevenSpaces)
        {
            print
              "\nUneven: $leadingSpaces leading spaces " .
              "on line $line. " .
              #"For \"$value\"\n"
              "\n\n";
            $exitCode = 5;
            $errors++; # Some redundancy here...
        }

        # Only allow "#" as the comment character
        #
        # The ":" is for an exception for URL,
        # e.g. "https://validator"
        #
        if (/[^:]\/\//) # Blind match, independent of context.
                        # We can always add exceptions
        {
            # Exceptions
            if (!     # Gotcha: "!" has highest precedence. We must
                      #         use an extra set of parentheses.
                      #         Otherwise, "!" only applies to 
                      #         the first term...
                 (
                   (/"\/\/"/) || # That is:  "//"
                   (/\\\/\//) || # E.g.:     wiki\//'  (in a PHP file, 
                                 #                      escaped "/")
                   (/i\/\//)  || # E.g.:     wiki//'   (in generated 
                                 #                      HTML, non-escaped 
                                 #                      "/"))
                   0

                   #  \\\/\/
                 )
               )
            {
                print
                  "\nAn invalid comment character sequence was detected. " .
                  "On line $line: \n\n$_ \n\n";

                $exitCode = 13;
                $errors++; # Some redundancy here...
            }
        } # Only allow "#" as the comment character

        # Insist on HTTPS links
        if (/http:\/\//) # Blind match, independent of context.
                         # We can always add exceptions
        {
            # In the HTML word list
            my $URL = "";
            my $isA_RealHTTP_link = 1;
            if (/<\/td> <\/tr>/)
            {
                # For reporting purposes, try to extract the URL
                # (this currently only works for the URLs in
                # the HTML export).
                #
                # But also want to detect false positives from
                # embedded URLs in the Wayback Machine.
                #
                # Sample:
                #
                #   <tr> <td><div id="ADK"></div>adk</td><td>ADK (1)</td><td>http://developer.android.com/guide/topics/usb/adk.html</td> </tr>
                #
                # Note: ".*?" is the non-greedy version of ".*"
                #
                #if (/<\/td><td>(.*?)<\/td> <\/tr>$/)
                if (/<\/td><td>([^<]+)<\/td> <\/tr>$/)
                {
                    $URL = $1;

                    # We insist they must start with "http://".
                    # This is a way to avoid false positives
                    # in Wayback Machine URLs. Example:
                    #
                    #     https://web.archive.org/web/20100426125115/http://www.mattmcdole.com/boat/
                    #
                    if ($URL =~ /^http:\/\//)
                    {
                        # Only unique URLs
                        $HTTP_URLs{$URL}++;
                    }
                    else
                    {
                        $isA_RealHTTP_link = 0;
                    }
                }
            }

            # Some exceptions for now. They may not all
            # be necessary, but for now we are only
            # interested in user-facing URLs.
            #
            if (! (
                    # In PHP comments
                    (/^\s*\#/)                            ||

                    # In PHP test code
                    (/test_transformFor_YouTubeComments/) ||

                    # Also test_transformFor_YouTubeComments(),
                    # but on a separate line.
                    (/Neomi/) ||

                    # Optionally (e.g., outcomment to get a report
                    # of URLs), suppress the word list URLs
                    #
                    # Sample line:
                    #
                    #   <tr> <td><div id="ADK"></div>adk</td><td>ADK (1)</td><td>http://developer.android.com/guide/topics/usb/adk.html</td> </tr>
                    #
                    # We presume these table-related HTML tags are
                    # not in other files that are checked.
                    #
                    #(/<\/td> <\/tr>/) ||    # Outcomment to get a report

                    (! $isA_RealHTTP_link) ||

                    # List of URLs for which HTTPS does not
                    # work, only HTTP (12 in the current
                    # word list (0.06%)).
                    #
                    # A typical error is:
                    #
                    #   Error code: SSL_ERROR_BAD_CERT_DOMAIN
                    #
                    ($URL eq 'http://blog.markturansky.com/BetterJavascriptTemplates.html')                                                                   ||  #  1
                    ($URL eq 'http://php-cli.com/')                                                                                                           ||  #  2
                    ($URL eq 'http://structuremap.net/structuremap/')                                                                                         ||  #  3
                    ($URL eq 'http://tflearn.org/')                                                                                                           ||  #  4
                    ($URL eq 'http://vger.kernel.org/~davem/skb.html')                                                                                        ||  #  5
                    ($URL eq 'http://www.catb.org/jargon/html/B/bullschildt.html')                                                                            ||  #  6
                    ($URL eq 'http://www.cburch.com/logisim/')                                                                                                ||  #  7
                    ($URL eq 'http://www.minimak.org/')                                                                                                       ||  #  8
                    ($URL eq 'http://www.mosaic-industries.com/embedded-systems/microcontroller-projects/raspberry-pi/gpio-pin-electrical-specifications')    ||  #  9
                    ($URL eq 'http://www.trirand.com/jqgridwiki/doku.php?id=start')                                                                           ||  # 10
                    ($URL eq 'http://z80.info/zip/z80cpu_um.pdf')                                                                                             ||  # 11
                    ($URL eq 'http://www.laputan.org/pub/papers/Cathedral-Paper.pdf')                                                                         ||  # 12
                    0
                  )
               )
            {
                $exitCode = 12;
                $errors++; # Some redundancy here...

                # Optionally limit the error output, but
                # still count the total number of errors
                #
                if ($errors <= 5 ||
                   1  # We may want a report, with the
                      # list of HTTP URLs
                   )
                {
                    print
                      "\nError $errors: HTTP link " .
                      "on line $line. " .
                      "URL: $URL\n\n";
                    print
                      "To add an exception, add it " .
                      "near \"z80.info\" (line 428) in file " .
                      "Dot_NET_commandLine/KeyboardShortcutConsistency.pl.\n\n";
                } # Optional error output
            } # Not an exception
        } # HTTP link detected


        # Check for escape of double quotes
        # in HTML form elements.
        #
        if (/value=\"/ || 0) # Guard for HTML form text input fields.
                             # But we could just use the unconditional
                             # rudimentary check on every line.
        {
            # Rudimentary check: 3 or more double quotes on
            # the physical line in the HTML / PHP file.
            #
            if (/\".*\".*\"/)
            {
                print
                  "\nA double quote was not properly escaped " .
                  "as \"&quot;\". " .
                  "On line $line\n\n";

                $exitCode = 11;
                $errors++; # Some redundancy here...
            }
        }


        if (/name=\"(.*)\"/) # 'name' form element attribute (presumably)
        {
            $name = $1;
            #print "name: $name\n";
        }

        if (/href=\"(.*)\"/) # 'href' attribute for HTML element <a>
        {
            $hrefAttribute = $1;
            #print "hrefAttribute: $hrefAttribute\n";
        }

        # Note: In file "EditOverflow.php" most of the "value=" lines are
        #       entirely generated by a PHP function or the value is
        #       in a PHP variable. E.g.:
        #
        #          <?php the_formValue($lookUpTerm); ?>
        #
        #          echo "value=\"$correctionComment_encoded\"\n";
        #
        #       This is in contrast to the other PHP files whose
        #       main part is really static HTML.
        #
        # We only use "value" for informational purposes, so its
        # content is not that important, except it shouldn't be
        # an empty string or undefined.
        #
        if (/value=\"(.*)\"/) # The copyable content (not strictly
                              # needed when we have line numbers -
                              # for identification)
        {
            $value = $1;
            #print "Detected value: >>>$value<<<\n";
            #die;
        } #Line with "value="

        if (/accesskey=\"(.*)\"/) # The keyboard shortcut
        {
            $accessKey = $1;
            #print "Keyboard shortcut: $accessKey for $value\n";
        } #Line with "accesskey="

        # Extract the indicated keyboard shortcut from text (normal HTML
        # text or from the label element) - by (HTML) underlined text -
        # <u></u>.
        #
        if (! 0) # Exceptions? But ***not*** for 'echo' statements in PHP
                 # (/echo.*;/) - we actually want to capture the keyboard
                 # shortcut information in those...  E.g., in PHP code
                 # like 'echo "<u>C</u>orrected term";'
                 #
                 # However, we can ***not*** allow it in outcommented code,
                 # like
                 #
                 #     "echo "In<u>c</u>orrect term (repeated)";"
                 #
                 # as it is not actually active.
        {
            if (/<u>([a-zA-Z])<\/u>/) # The indicated keyboard shortcut. It
                                      # comes before the other lines. It
                                      # can both be lowercase and uppercase,
                                      # depending on where it is in the text.
            {
                $indicatedKeyboardShortcut = $1; # We are also using this value
                                                 # in other tests.

                # To uppercase, for later use in comparisons
                $indicatedKeyboardShortcut =~ tr/a-z/A-Z/;

                #print "Keyboard shortcut: >>>$indicatedKeyboardShortcut<<<. On line $line\n";
                #die;

                # Check at the keyboard shortcut indications are not
                # inside links (where they most probably will be
                # invisible in the rendered HTML).
                #
                # Note:
                #
                #     Effectively, (for simplicity here) we demand that links
                #     not be on the same (physical) line in the HTML source...
                #
                if (/<\/a>/) #Potentially false negatives if the link
                             #is over more than one line. Other tests:
                             #
                             #  / href/
                             #
                             #"</a>" is the most likely. See e.g.
                             #<FixedStrings.php>, near "hm symbol",
                             #approx. line 185
                {
                    print
                      "\nThe *indicated* keyboard shortcut is inside " .
                      "a link (where it may become invisible). " .
                      "On line $line: indicated* keyboard " .
                      "shortcut $indicatedKeyboardShortcut\n\n";

                    $exitCode = 8;
                    $errors++; # Some redundancy here...
                }

                # Disallow keyboard shortcuts on single-letter
                # words (at least for now).
                #
                if (/ <u>([a-zA-Z])<\/u> /)
                {
                    print
                      "\nThe *indicated* keyboard shortcut is for " .
                      "a single-letter word. This is not allowed. " .
                      "On line $line: indicated* keyboard " .
                      "shortcut $indicatedKeyboardShortcut\n\n";

                    $exitCode = 10;
                    $errors++; # Some redundancy here...
                }

            } #Line with an indicated keyboard shortcut

        } # An exception

        # Record HTML form element "label" - it is rudimentary - we only
        # need it for some exceptions to the checks. E.g. we don't
        # care if it goes over several lines as our exceptions don't
        # do that.
        #
        # Sample:
        #
        #     <label for="editSummary_output2"><b></b></label>
        #
        # Note:
        #
        #   For "Reset lookup state", "label" comes last...
        #
        if (/<label for=.*?>(.*)</) # The tooltip (for the keyboard shortcut)
        {
            $formLabelText = $1;

            #print "Form label: >>>$formLabelText<<<. On line $line\n";
            #die;
        }

        # Note:
        #
        #     The 'title=' is presumed to be ***after*** the
        #     'value=' and 'accesskey=' lines
        #
        if (/title=\"(.*)\"/) # The tooltip (for the keyboard shortcut)
        {
            my $toolTip = $1;

            if ($accessKey ne "") # Allow empty keyboard shortcut
            {
                $detectedShortcuts++;

                if ($outputKeyboardShortcutInfo)
                {
                    # To keep it on one line in a terminal
                    my $limitedOutput = substr($value, 0, 95);

                    # Escape "%" for printf (by a "%"). Note that we
                    # get a double "%" if we use normal print...
                    $limitedOutput =~ s/\%/\%\%/g;

                    printf
                      "Keyboard shortcut %2d: $accessKey for \"$limitedOutput...\"\n",
                           $detectedShortcuts;
                }

                my $oldValue = $accesskeys{$accessKey};
                if ($oldValue)
                {
                    print
                      "\nKeyboard shortcut is already used. " .
                      "On line $line: $accessKey for \"$value\" " .
                      "(other: \"$oldValue\")\n\n";

                    $exitCode = 1;
                    $errors++; # Some redundancy here...
                }
                $accesskeys{$accessKey} = $value;
            }
            else
            {
                # But then the title should be empty as well

                if ($toolTip ne "")
                {
                    print
                      "\nThe tooltip text should be empty when the " .
                      "keyboard shortcut is empty. " .
                      "On line $line for \"$value\"\n\n";

                    $exitCode = 2;
                    $errors++; # Some redundancy here...
                }
            }

            if ($toolTip ne "") # Allow empty tooltip shortcut (but see above)
            {
                if ($toolTip =~ /Shortcut: Shift \+ Alt \+ (.*)/)
                {
                    my $keyboardShortcut_inTooltip = $1;
                    #print "Keyboard shortcut in tooltip: $keyboardShortcut_inTooltip\n";

                    if ($keyboardShortcut_inTooltip ne $accessKey)
                    {
                        print
                          "\nKeyboard shortcut in tooltip " .
                          "(\"$keyboardShortcut_inTooltip\") is not the same " .
                          "as specified (\"$accessKey\"). " .
                          "On line $line. For \"$value\"\n\n";

                        $exitCode = 3;
                        $errors++; # Some redundancy here...
                    } # Check for indicated keyboard shortcut in tooltip

                    # Use the opportunity to also check the consistency of
                    # the keyboard shortcut ***indication*** and the
                    # actual shortcut.
                    #
                    # It also indirectly checks if the indication exists at
                    # all - though the error message may be confusing as
                    # the previous form element will probably be involved).
                    #
                    if ($indicatedKeyboardShortcut ne $accessKey)
                    {
                        my $errorText =
                              "\nThe indicated keyboard shortcut in the text " .
                              "(\"$indicatedKeyboardShortcut\") \n" .
                              "is not the same as specified (\"$accessKey\"). \n\n" .
                              "Alternatively, if the PHP source has been changed, \n" .
                              "it may be necessary to update one of the \n" .
                              "exceptions, near \"\$formLabelText eq\". \n\n" .
                              "Near line $line. For \"$value\"\n\n";

                        #print "\nformLabelText: $formLabelText\n";
                        #print $errorText;

                        # Some exceptions... Mainly for the (dynamic)
                        # main Edit Overflow lookup page
                        if (
                             # Outside because <label> is ***after***
                             # a HTML form checkbox - thus we can
                             # not use the guard...
                             (
                                 ($name ne "resetState") &&

                                 (! ($hrefAttribute =~ /validator.w3.org/ )) &&

                                 # <Text.php>: Submit buttons
                                 (! ($name =~ /someAction/ )) &&
                                 1

                             ) &&

                             (
                                 # Guard - the exceptions are ***only***
                                 # for HTML form label text
                                 ($formLabelText eq $kValuePreset) ||

                                 # List of exceptions
                                 ! (
                                       # Effectively empty text
                                       ($formLabelText eq "<b></b>") ||

                                       # Dynamically generated (but the text,
                                       # "URL" does not contain the current
                                       # shortcut letter ("E")). And it is
                                       # a link anyway, so it would be
                                       # invisible..
                                       #
                                       #
                                       #($formLabelText eq '<?php echo get_HTMLlink("URL", $URL, "") ?>') || # Until 2023-01-05.
                                       ($formLabelText eq '<?php echo get_HTMLlink("URL.", $URL, "") ?> <?php echo get_HTMLlink("Words.", $linkWordListWithPageAnchor, \' id="1300"\') ?>') ||

                                       # Presuming it is for the form submit...
                                       #
                                       # It would be more robust if we checked
                                       # for the form element type ("submit"
                                       # and "hidden").
                                       #
                                       # Or had a positive list for the types
                                       # of elements ("text" and "checkbox").
                                       #
                                       ($value eq "Look up") || # For the main lookup page

                                       ($value eq "Generate") || # For the link builder page

                                       0
                                 )
                             )
                           )
                        {
                            # Note: The last part of the error message is
                            #       confusing for file EditOverflow.php,
                            #       because there isn't any (static) value
                            #       (generated by the_formValue() at runtime).
                            #
                            print $errorText;

                            $exitCode = 9;
                            $errors++; # Some redundancy here...

                            #die;

                        } # Not an exception
                    } # Check for indicated keyboard shortcut in the text
                }
                else
                {
                    print
                      "\nThe tooltip text (\"$toolTip\") is not proper. " .
                      "On line $line for \"$value\"\n\n";

                    $exitCode = 4;
                    $errors++; # Some redundancy here...
                }
            }

            # Prepare for next block (so that a previous form element
            # does not interfere with the current (incl. confusing
            # error messages)).
            #
            $value = $kValuePreset;
            $indicatedKeyboardShortcut = $kValuePreset;
            $formLabelText             = $kValuePreset;

            # Crucial! - otherwise false negatives for some of our tests
            $hrefAttribute             = $kValuePreset;

        } #Line with "title="

        # General check of uniqueness for some fields
        #
        # Note: This must be last as it overwrites $_...
        #
        foreach (sort keys %uniqueFields) # Sort: for a deterministic behaviour
        {
            my $someFieldName = $_;
            #my $value = $accesskeys{$key};

            #print "On line $line: Looking for field \"$someFieldName\"...\n";
            #die;

            # Example line that should match:
            #
            #                       class="XYZ29"
            #
            # The anchor to the start of the line is to avoid false
            # positives, e.g. for:
            #
            #     <!--  class="XYZ3"  -->
            #
            if ($currentlLine =~ /^\s+$someFieldName=\"(.*)\"/)
            {
                my $fieldValue = $1;

                #print "On line $line: Detected field name \"$someFieldName\". " .
                #      "Value: \"$fieldValue\"\n";
                #die;

                my $prevLineNumber = $uniqueFields{$someFieldName}->{$fieldValue};
                if ($prevLineNumber)
                {
                    print
                      "\n\nOn line $line: Non-unique field value that is also " .
                      "at line $prevLineNumber. Field name \"$someFieldName\". " .
                      "Value: \"$fieldValue\"\n\n\n";

                    die; #Shouldn't we use an exit code instead?
                }
                $uniqueFields{$someFieldName}->{$fieldValue} = $line;

                #print
                #  "\nKeyboard shortcut is already used. " .
                #  "On line $line: $accessKey for \"$value\" " .
                #  "(other: \"$oldValue\")\n";

            } # Detected a field that should be unique

        } # Through looking for fields that must be unique

    } #while(). Through the input file


    if (
         ($line != 0) &&   # Don't output anything for empty input

         ($exitCode == 0)  # Or if we detected one or more errors
                           # in the input. Even though we could
                           # output some information, we prefer
                           # not to obscure the errors with too
                           # much output.
       )
    {
        my $possibleShortcuts = keys %keyboardShortcutSet;
        my $shortcuts = keys %accesskeys;
        my $expectedUnassigned = $possibleShortcuts - $shortcuts;

        if ($outputKeyboardShortcutInfo)
        {
            print "\n\n";
            print "Number of possible shortcuts: $possibleShortcuts\n";
            print "Detected shortcuts (including duplicates) in this file: $detectedShortcuts\n";
            print "Detected shortcuts in this file: $shortcuts\n";
            print "\n";
        }

        #foreach my $someShortcutLetter (%keyboardShortcutSet)
        my $unassignedCount = 0;
        foreach (sort keys %keyboardShortcutSet)
        {
            my $key = $_;
            my $value = $accesskeys{$key};

            #print "Key: $key\n";
            #print "Value: >>>$value<<<\n";

            if (! ($value))
            {
                $unassignedCount++;

                if ($outputKeyboardShortcutInfo)
                {
                    printf "Unassigned letter %2d: $_\n", $unassignedCount;
                }
            }
        }

        # Internal program checks - it would normally never fire
        if ($expectedUnassigned != $unassignedCount)
        {
            print
              "\nInternal error: The unassigned keyboard shortcut count ($unassignedCount) ".
              "was not the expected ($expectedUnassigned).\n\n";
            $exitCode = 100 + 1;
        }
        if ($detectedShortcuts != $shortcuts) # Can also be due to duplicate keyboard
                                              # shortcuts, but this would be detected
                                              # before we get here.
        {
            print
              "\nInternal error: The detected shortcuts counter ($detectedShortcuts) ".
              "does not correspond to data structure \"accesskeys\"'s ".
              "count ($shortcuts)\n\n";
            $exitCode = 100 + 2;
        }
    }
} #An actual file name (and existing file) was provided in the script invocation

if ($exitCode != 0)
{
    # As the reason for the error(s) may be
    # obscured by some other output.
    print
      "\n\n$errors errors detected. Review the beginning of the output " .
      "(as the error information may or may be obscured by other output). \n\n";

    # Output HTTP URLs. (Is this the right place? Should it be above?)
    #
    my $HTTPcount = 0;
    foreach (sort keys %HTTP_URLs)
    {
        my $URL = $_;
        my $nonUniqueCount = $HTTP_URLs{$URL};
        $HTTPcount++;

        print "HTTP URL $HTTPcount ($nonUniqueCount): $URL\n";

        #print "Key: $key\n";
        #print "Value: >>>$value<<<\n";
    }

}

exit $exitCode;

