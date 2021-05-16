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
#             3. Presumed/required order of lines with the information      #
#                                                                           #
#             4. Output of unassigned keys (optional)                       #
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
#                                                                           #
#############################################################################

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


my $exitCode = 0;

my %assignedKeyboardShortcut = ();


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


# We don't want it empty or undefined as we depend on it later on.
my $kValuePreset = "<Not set>";


my $line  = 0;
my $accKey = "";
my $value = $kValuePreset;

my %accesskeys = ();

my $detectedShortcuts = 0; # Including duplicates, if any.


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
        $exitCode = 5;
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
        $accKey = $1;
        #print "Keyboard shortcut: $accKey for $value\n";
    } #Line with "accesskey="

    # Note: The 'title=' is presumed to be after the
    #       'value=' and 'accesskey=' lines
    #
    if (/title=\"(.*)\"/) # The tooltip (for the keyboard shortcut)
    {
        my $toolTip = $1;

        if ($accKey ne "") # Allow empty keyboard shortcut
        {
            $detectedShortcuts++;

            if ($outputKeyboardShortcutInfo)
            {
                # To keep it on one line in a terminal
                my $limitedOutput = substr($value, 0, 95);

                # Esacpe "%" for printf (by a "%"). Note that we get
                # a double "%" if we use normal print...
                $limitedOutput =~ s/\%/\%\%/g;

                printf
                  "Keyboard shortcut %2d: $accKey for \"$limitedOutput...\"\n",
                       $detectedShortcuts;
            }

            my $oldValue = $accesskeys{$accKey};
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
                my $keyboardShortcut_inTooltip = $1;
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
        # Prepare for next block
        $value = $kValuePreset;
    } #Line with "title="
    
} #while()


my $possibleShortcuts = keys %keyboardShortcutSet;
my $shortcuts = keys %accesskeys;
my $expectedUnassigned = $possibleShortcuts - $shortcuts;

if ($outputKeyboardShortcutInfo)
{
    print "\n";
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
      "was not the expected ($expectedUnassigned).\n";
    $exitCode = 100 + 1;
}
if ($detectedShortcuts != $shortcuts) # Can also be due to duplicate keyboard
                                      # shortcuts, but this would be detected
                                      # before we get here.
{
    print
      "\nInternal error: The detected shortcuts counter ($detectedShortcuts) ".
      "does not correspond to data structure \"accesskeys\"'s ".
      "count ($shortcuts)\n";
    $exitCode = 100 + 2;
}


exit $exitCode;

