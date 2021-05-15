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
#             2. Inconsistency between tooltip text (with                   #
#                information about what the shortcut is)                    #
#                and the actual keyboard shortcut                           #
#                                                                           #
#           Also:                                                           #
#                                                                           #
#             3. Indentation consistency                                    #
#                                                                           #
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
# UPDATED: PM 2021-XX-XX                                                    #
#                                                                           #
#                                                                           #
#############################################################################


$exitCode = 0;

while (<>)
{
    chop;

    $line++;
    my $len = length;
    #print "$len - >>>$_<<<\n\n";

    my $leadingSpace = "";
    if (/^(\s+)/)
    {
        $leadingSpace = $1;
    }
    my $leadingSpaces = length $leadingSpace;
    #print "$leadingSpaces leading spaces - >>>$_<<<\n\n";

    my $hasUnevenSpaces = $leadingSpaces % 2;
    #print "Uneven: $leadingSpaces leading spaces - >>>$_<<<\n\n"
    #  if $hasUnevenSpaces;

    if ($hasUnevenSpaces)
    {
        print
          "\nUneven: $leadingSpaces leading spaces " .
          "on line $line. " .
          #"For \"$value\"\n"
          "";
        $exitCode = 3;
    }

    if (/value=\"(.*)\"/) # The copyable content (not strictly
                          # needed when we have line numbers -
                          # for identification)
    {
        $value = $1;
    }

    if (/accesskey=\"(.*)\"/) # The keyboard shortcut
    {
        $accKey = $1;
        #print "Keyboard shortcut: $accKey for $value\n";
    }

    if (/title=\"(.*)\"/) # The tooltip (for the keyboard shortcut)
    {
        $toolTip = $1;
        #print "Keyboard shortcut: $accKey for $value\n";

        if ($accKey ne "") # Allow empty keyboard shortcut
        {
            $oldValue = $accesskeys{$accKey};
            if ($oldValue)
            {
                print
                  "\nKeyboard shortcut is already used. " .
                  "On line $line: $accKey for \"$value\" " .
                  "(other: \"$oldValue\")\n";
                $exitCode = 1;
            }
            $accesskeys{$accKey} = $value;
        }
        else
        {
            # But then the title should be empty as well

            if ($toolTip ne "")
            {
                print
                  "\nThe tooltip text should be empty when the " .
                  "keyboard shortcut is empty. " .
                  "On line $line for \"$value\"\n";
                $exitCode = 2;
            }
        }

        if ($toolTip ne "") # Allow empty tooltip shortcut (but see above)
        {
            if ($toolTip =~ /Shortcut: Shift \+ Alt \+ (.*)/)
            {
                $keyboardShortcut_inTooltip = $1;
                #print "Keyboard shortcut in tooltip: $keyboardShortcut_inTooltip\n";

                if ($keyboardShortcut_inTooltip ne $accKey)
                {
                    print
                      "\nKeyboard shortcut in tooltip " .
                      "(\"$keyboardShortcut_inTooltip\") is not the same " .
                      "as specified (\"$accKey\"). " .
                      "On line $line. For \"$value\"\n";
                    $exitCode = 3;
                }
            }
            else
            {
                print
                  "\nThe tooltip text (\"$toolTip\") is not proper. " .
                  "On line $line for \"$value\"\n";
                $exitCode = 4;
            }
        }
    }
} #while()


exit $exitCode;


