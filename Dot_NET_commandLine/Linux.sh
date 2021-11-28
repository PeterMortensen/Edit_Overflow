########################################################################
#                                                                      #
# Purpose: Compilation and running of part of Edit Overflow            #
#          on Linux. It produces SQL and HTML exports of               #
#          the Edit Overflow wordlist.                                 #
#                                                                      #
#          The running part is of C# code (.NET). The testing          #
#          is both for C# and JavaScript. And systen regression        #
#          tests (currently only for the web interface)                #
#                                                                      #
#          We also copy some generated files to the web site,          #
#          e.g. the word file in HTML (for viewing) and                #
#          JavaScript (for use in JavaScript-based word                #
#          lookup).                                                    #
#                                                                      #
#          Also, by running the C# code (exactly the same as on        #
#          Windows), a number of integrity and sanity tests are        #
#          performed on the wordlist data (thus it aids in             #
#          adding to the wordlist on Linux (shortening the             #
#          feedback loop)).                                            #
#                                                                      #
#          The SQL can be imported directly into standard LAMP         #
#          hosting (MySQL). The HTML is pushed directly to a           #
#          web page by FTP (automating this step).                     #
#                                                                      #
#          Tested on (all 64 bit):                                     #
#                                                                      #
#            Ubuntu 18.04 (Bionic Beaver)                              #
#            Ubuntu 19.10 (Eoan Ermine)                                #
#            Ubuntu 20.04 (Focal Fossa).                               #
#                                                                      #
#          We also use the opportunity to run all the unit             #
#          tests (based on NUnit).                                     #
#                                                                      #
#          We also run the PHP code (for the web application),         #
#          both from the command line and through a local web          #
#          server, effectively both unit tests and                     #
#          integration tests Selenium                                  #
#                                                                      #
# Driver script for copying only the necessary files to a              #
# work folder, compile and run part of the .NET code for               #
# Edit Overflow.                                                       #
#                                                                      #
#                                                                      #
# Installation notes:                                                  #
#                                                                      #
#   To enable compilation and running .NET Core code on                #
#   Ubuntu/Debian, these installation steps work (last                 #
#   tested on 2020-06-01):                                             #
#                                                                      #
#       wget -q https://packages.microsoft.com/config/ubuntu/19.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
#       sudo dpkg -i packages-microsoft-prod.deb                       #
#                                                                      #
#       sudo apt-get update                                            #
#       sudo apt-get install apt-transport-https                       #
#       sudo apt-get update                                            #
#       sudo apt-get install dotnet-sdk-3.1                            #
#                                                                      #
#   To enable unit testing with NUnit, install it with NuGet (tested   #
#   on 2020-06-14 - it installed version 3.12.0. NuGet is part of      #
#   the .NET Core installation - try e.g. 'dotnet nuget' to see        #
#   that is the case):                                                 #
#                                                                      #
#      dotnet add package NUnit                                        #
#                                                                      #
########################################################################


#############################################################################
#
#  TOC:
#
#     1. Internal check of the web interface regression tests
#     2. Copying files to the build folder, etc.
#
#     3. PHP code test: main lookup - EditOverflow.php
#     4. PHP code test: self test, unit tests - Text.php
#     5. PHP code test: text transformation - 'Real quotes' - Text.php
#     6. PHP code test: fixed strings - FixedStrings.php
#     7. PHP code test: edit summary fragments - EditSummaryFragments.php
#
#     8. Web server test: localWebserver_Edit_Overflow_lookup
#     9. Web server test: localWebserver_Edit_Overflow_lookup
#    10. Web server test: localWebserver_Edit_Overflow_lookup
#    11. Web server test: localWebserver_Text
#    12. Web server test: localWebserver_fixed_strings
#    13. Web server test: localWebserver_Edit_summary_fragments
#    14. Web server test: localWebserver_Text_RemoveTABsAndTrailingWhitespace
#    15. Web server test: localWebserver_Text_RemoveCommonLeadingSpace
#    16. Web server test: localWebserver_Text_RealQuotes
#
#    17. Start running C# unit tests
#    18. Exporting the word list as SQL
#    19. Opening some web pages and applications for manual operations
#    20. Exporting the word list as HTML
#    21. Starting checking generated HTML
#    22. Exporting the word list as JavaScript
#    23. Start running JavaScript unit tests
#
#    24. Keyboard shortcut consistency check: Edit Overflow lookup
#    25. Keyboard shortcut consistency check: text stuff
#    26. Keyboard shortcut consistency check: fixed string
#    27. Keyboard shortcut consistency check: edit summary
#
#    28. Updating the JavaScript word list file on pmortenen.eu (<https://pmortensen.eu/world/EditOverflowList.js>)
#    29. Updating the HTML word list file on pmortenen.eu (<https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>)
#
#    30. HTML validation: Edit Overflow lookup
#
#    31. HTML validation: Text stuff
#    32. HTML validation: Fixed strings
#    33. HTML validation: Edit summary fragments
#    34. HTML validation: Word list (HTML)
#    35. Starting checks of the generated HTML
#
#    36. Starting web interface regression tests, local
#    37. Starting web interface regression tests, local
#    38. Starting web interface regression tests, production
#
#    39. End of build. All build steps succeeded!!
#
#############################################################################

#############################################################################
#
# Meta: The TOC can be generated by this filter (though likely broken by the
#       refactoring of this script on 2021-09-16 ... a new one would also
#       have to extract near "PHP_code_test".
#
# An alternative would be to parsed the the screen output, but it may
# not be a rich in information.
#
# Old version:
#
#   | perl -nle 'printf "#    %2d%s\n", $1, $2 if /^echo\s+.(\d+)(.+)\.\.\./'
#
# New version (for build script versions after 2021-06-09):
#
#   | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. Test for: %s\n", $1, $2 if /^mustBeEqual.+\d\s+(\d+)\s+\"(.+)\"/'
#
# Newer version (for build script versions after 2021-09-16):
#
#   XXXXXXXXXXXXXX
#   | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. Test for: %s\n", $1, $2 if /^mustBeEqual.+\d\s+(\d+)\s+\"(.+)\"/; printf "#    %2d. PHP code test: %s - %s\n", $3, $2, $1 if /^PHP_code_test\s+(\S+)\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Web server test: %s\n", $3, $2 if /^webServer_test\s+\"(.+)\"\s+\"(.+)\"\s+(\d+)/;     '
#
# Removed extraction from near "mustBeEqual" as the build step number be
# manually provided or is part of an existing test (2021-09-30):
#
#   XXXXXXXXXXXXXX
#   | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. PHP code test: %s - %s\n", $3, $2, $1 if /^PHP_code_test\s+(\S+)\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Web server test: %s\n", $3, $2 if /^webServer_test\s+\"(.+)\"\s+\"(.+)\"\s+(\d+)/;     '
#
#
#   Notes:
#
#     Most build steps use helper function "startOfBuildStep". Example:
#
#         startOfBuildStep "2" "Copying files to the build folder"
#
#     Some build steps are refactored into helper functions (instead of
#     using helper function "startOfBuildStep" directly). Some
#     examples:
#
#         keyboardShortcutConsistencyCheck Text.php           "text stuff"      12
#
#         HTML_validation                  FixedStrings.php   "Fixed strings"   19
#
#         mustBeEqual                      ${MATCHING_LINES}  1                 22   "HTML anchor is not unique"
#
#         PHP_code_test  EditOverflow.php          "main lookup"              3  "OverflowStyle=Native&LookUpTerm=JS"  "Access denied for user"
#
#         webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native"                  "localWebserver_Text"                     8
#
# Example command lines:
#
#     Old:
#
#       cat ~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine/Linux.sh | grep echo  | grep '\. ' | perl -nle 'printf "#    %2d%s\n", $1, $2 if /^echo\s+.(\d+)(.+)\.\.\./'
#
#     New version:
#
#       clear ; cat ~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine/Linux.sh | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. Test for: %s\n", $1, $2 if /^mustBeEqual.+\d\s+(\d+)\s+\"(.+)\"/'
#
#     Newer version:
#
#       clear ; cat ~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine/Linux.sh | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. Test for: %s\n", $1, $2 if /^mustBeEqual.+\d\s+(\d+)\s+\"(.+)\"/; printf "#    %2d. PHP code test: %s - %s\n", $3, $2, $1 if /^PHP_code_test\s+(\S+)\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Web server test: %s\n", $3, $2 if /^webServer_test\s+\"(.+)\"\s+\"(.+)\"\s+(\d+)/;     '
#
#     Removed extraction from near "mustBeEqual":
#
#       clear ; cat ~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine/Linux.sh | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. PHP code test: %s - %s\n", $3, $2, $1 if /^PHP_code_test\s+(\S+)\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Web server test: %s\n", $3, $2 if /^webServer_test\s+\"(.+)\"\s+\"(.+)\"\s+(\d+)/;     '
#
#############################################################################

#############################################################################
#
# Meta: Perl one-liner to compute the run time for each
#       build step. It works on the saved output from
#       a run of the build script
#
#       The output is TAB-separated and can thus easily be
#       post processed in LibreOffice Calc or similar.
#
# perl -nle 'if (/^\d+\./) { $prevBuildLine = $buildLine; $buildLine = $_; }  if (/^At: /) { $prevHours = $hours; $prevMinutes = $minutes; $prevSeconds = $seconds; if (/T(\d+):(\d+):(\d+)_(\d+)/) { $hours = $1; $minutes = $2; $seconds = $3 + $4 / 1000000000.0} $runTimeSeconds = 3600 * ($hours - $prevHours) + 60 * ($minutes - $prevMinutes) + ($seconds - $prevSeconds); print "Run time [secs]\t$runTimeSeconds\t$prevBuildLine" if $prevHours; }   '   '/home/embo/temp2/2021-11-05/Transcript of Edit Overflow build, 2021-11-05T171256.txt'  >  _BuildStepsTimings.txt
#

echo
echo
echo 'Start of building Edit Overflow...'
echo


# ####################
# Configuration, start


# Selectively disable some test (e.g., if an external service we
# depend on is not available)
#
# Normally, ***ALL*** should be outcommented
#export DISABLE_HTMLVALIDATION=1



# Perhaps move to the client side to remove redundancy (as
# we currenly also need to update the client side (for
# postprocessing after this script) when we change
# the date - e.g., "SQLFILE_DATE")?
#
export EFFECTIVE_DATE='2020-02-05'
export EFFECTIVE_DATE='2020-02-28'
export EFFECTIVE_DATE='2020-04-23'
export EFFECTIVE_DATE='2020-06-01'
export EFFECTIVE_DATE='2020-06-03'
export EFFECTIVE_DATE='2020-06-14'
export EFFECTIVE_DATE='2020-06-16'
export EFFECTIVE_DATE='2020-09-29'
export EFFECTIVE_DATE='2020-10-24'
export EFFECTIVE_DATE='2021-02-03'
export EFFECTIVE_DATE='2021-06-14'


export SRCFOLDER_BASE='/home/embo/UserProf/At_XP64/Edit_Overflow'
export WEB_SRCFOLDER_BASE="${SRCFOLDER_BASE}/Web_Application"


export SELINUM_DRIVERSCRIPT_DIR="${WEB_SRCFOLDER_BASE}/__regressTest__"
export SELINUM_DRIVERSCRIPT_FILENAME="${SELINUM_DRIVERSCRIPT_DIR}/web_regress.py"


export SELINUM_DRIVERFOLDER="/home/embo/.wdm/drivers/geckodriver/linux64/v0.28.0"

export PATH=$PATH:${SELINUM_DRIVERFOLDER}


# To make the C# unit test run ***itself*** succeed when
# we use a single build folder, we rename a file... We
# use a file name for the file containing "Main()"
# that does not end in ".cs" in order to hide it
# (until after the C# unit tests have run).
#
# Otherwise we will get an error like this:
#
#     Program.cs(20,21): error CS0017:
#     Program has more than one entry point defined.
#
#     Compile with /main to specify the type that contains the entry
#     point.
#     [/home/embo/temp2/2020-06-14/_DotNET_tryout/EditOverflow4/EditOverflow3.csproj]
#
export FILE_WITH_MAIN_ENTRY=Program.cs
export FILE_WITH_MAIN_ENTRY_HIDE=${FILE_WITH_MAIN_ENTRY}ZZZ


# Moved to Ubuntu 20.04 MATE 2020-06-09
## Moved to Ubuntu 20.04 MATE 2020-06-03
### Moved to PC2016 after SSD-related Linux system crash 2020-05-31
###export SRCFOLDER_BASE='/home/mortense2/temp2/Edit_Overflow'
###export WORKFOLDER1=/home/mortense2/temp2/${EFFECTIVE_DATE}
##export SRCFOLDER_BASE='/home/mortensen/temp2/Edit_Overflow'
##export WORKFOLDER1=/home/mortensen/temp2/${EFFECTIVE_DATE}
#export SRCFOLDER_BASE='/home/embo/temp2/2020-06-03/temp1/Edit_Overflow'
#export WORKFOLDER1=/home/embo/temp2/${EFFECTIVE_DATE}
export WORKFOLDER1=/home/embo/temp2/${EFFECTIVE_DATE}


export WORKFOLDER2=${WORKFOLDER1}/_DotNET_tryout
export WORKFOLDER3=${WORKFOLDER2}/EditOverflow4
export WORKFOLDER=${WORKFOLDER3}


# That is, mostly for PHP files (mirroring the structure on web hosting)
export LOCAL_WEBSERVER_FOLDER=/var/www/html/world


export FTPTRANSFER_FOLDER_HTML=${WORKFOLDER}/_transfer_HTML

export FTPTRANSFER_FOLDER_JAVASCRIPT=${WORKFOLDER}/_transfer_JavaScript


export SRCFOLDER_DOTNET=${SRCFOLDER_BASE}/Dot_NET
export SRCFOLDER_DOTNETCOMMANDLINE=${SRCFOLDER_BASE}/Dot_NET_commandLine
export SRCFOLDER_WEB=${SRCFOLDER_BASE}/Web_Application


export SRCFOLDER_CORE=$SRCFOLDER_DOTNET/OverflowHelper/OverflowHelper/Source/main
export SRCFOLDER_PLATFORM_SPECIFIC=$SRCFOLDER_DOTNET/OverflowHelper/OverflowHelper/Source/platFormSpecific

export SRCFOLDER_TESTS=$SRCFOLDER_DOTNET/Tests


# Note: For now in the source folder. It should be moved to the
#       work (build) folder - we would need to copy JavaScript
#       files, including the Jest "__tests__" folder.
#
export WEBFOLDER=${SRCFOLDER_WEB}



# Fixed name, not dependent on date, etc.
export HTML_FILE_GENERIC_FILENAMEONLY=EditOverflowList_latest.html
export HTML_FILE_GENERIC=$WORKFOLDER/$HTML_FILE_GENERIC_FILENAMEONLY
export JAVASCRIPT_FILE_GENERIC=$WORKFOLDER/EditOverflowList.js


# Hardcoded for now
export LOCAL_WEB_ERRORLOG_FILE="/var/log/apache2/error.log"


#export FTP_SITE_URL='ftp://linux42.simplyZZZZZZZ.com' # Poor man's dry run
export FTP_SITE_URL='ftp://linux42.simply.com'

# That is, a local sub folder with a copy of some
# files from the web hosting (production)
export WEB_ERRORLOG_SUBFOLDER='_webErrorlog'

# Yes, that is the actual file name that has been configured at
# the web hosting (through file <public_html/world/.htaccess>)...
#
export REMOTE_WEB_ERRORLOG_FILENAME='phperrors_777.log'

export WEBFORM_CHECK_FILENAME='KeyboardShortcutConsistency.pl'

export WEBFORM_CHECK_CMD="perl -w ${SRCFOLDER_DOTNETCOMMANDLINE}/${WEBFORM_CHECK_FILENAME}"


# Configuration, end
# ####################


# #########################################################################
#
# Derive from configuration / prepare / convenience (declutter the main script)
#

export SQL_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.sql

export HTML_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.html

export JAVASCRIPT_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.js


#Not used - delete at any time
#Too complicated - use use startWatchFile() / endWatchFile() instead.
export BEFORE_LOGFILE="${WEB_ERRORLOG_SUBFOLDER}/_before_${REMOTE_WEB_ERRORLOG_FILENAME}"
export AFTER_LOGFILE="${WEB_ERRORLOG_SUBFOLDER}/_after_${REMOTE_WEB_ERRORLOG_FILENAME}"



# Avoid "unary operator expected" error. We want to make it work
# when environment variable "DISABLE_HTMLVALIDATION" is left out
# entirely (e.g., it is only defined when selectively leaving 
# out the HTML validation build step when the (external) 
# service is down) - the normal (nominal case) is that 
# it is left out (undefined). It is also to declutter 
# the main script with this test.
#
if [ -z "${DISABLE_HTMLVALIDATION}" ] ; then
    export DISABLE_HTMLVALIDATION=0
fi



# ###########################################################################
#
# A helper function to reduce redundancy.
#
# Outputs the current date and time in ISO 8601 format, followed
# by subsecond information (to standard output)
#
# Notes:
#
#   * What about leading zero for the "ns" part? Would it sort properly?
#
#   * Consider making the seconds a real decimal number. Is this
#     allowed by ISO 8601?
#
function absoluteTimestamp()
{
    # Sample output:
    #
    #     2021-11-05T14:39:34_230550955_ns

    date +%FT%T_%N_ns
} #absoluteTimestamp()


# ###########################################################################
#
# A helper function to reduce redundancy.
#
# Outputs the current date and time in ISO 8601 format (to 
# standard output), followed by subsecond information, 
# prefixed by the specified string (e.g., "Start time"), 
# and with the specified prefix, and leading and 
# trailing empty lines.
#
# Parameters:
#
#    $1   Prefix (string)
#
function timeStamp()
{
    # Sample output (with leading and trailing empty lines):
    #
    #
    #     Start time: 2021-11-05T16:19:29_537064013_ns
    #

    # Note: absoluteTimestamp() doesn't output an extra end-of-line,
    #       resulting in two empty lines between the timestamp and
    #       following output to the screen.
    #
    #       That is the fault of particular build steps.
    #
    #
    # "-n" is to turn off end-of-line output (historically,
    # this has been troublesome - see e.g.:
    #
    #   <https://stackoverflow.com/questions/11193466/>)
    #     "echo -n" prints "-n"
    #
    #echo ; echo -n "$1: $(absoluteTimestamp)" ; echo
    echo ; echo "$1: $(absoluteTimestamp)" ; echo
} #timeStamp()


# ###########################################################################
#
# A helper function to reduce redundancy.
#
# For marking start of a build step.
#
# Mostly for the screen output, but we can also use it
# for marking the start of a step in time (e.g., to
# record / output ***timing*** information) - as a
# timestamp is automatically output.
#
# Parameters:
#
#    $1   Build step number
#
#    $2   Build step description
#
function startOfBuildStep()
{
    echo
    echo
    echo "============================================================================="
    echo "$1. $2..."
    #echo

    timeStamp "At"

    # Future:
    #
    #   1. Record the time (absolute or relative) - for later use
    #
    #   2. Output a timestamp (so we know the absolute time this
    #      particular test was run)
} #startOfBuildStep()


# ###########################################################################
#
# Helper function to test a build step for errors. We
# exit/stop the script ***altogether*** (not just
# this function) if there is an error.
#
# Parameters:
#
#    $1   Build step number
#
#    $2   Result code (e.g. 0 for no error)
#
#    $3   String identifying the build step
#
# What is "7" below??? Is it spurious? Should we remove it?
#
# Note:
#
#   On the caller side, "$?" is the error code of the
#   last run command (in Bash, at least) -
#
#     "$? Gives the exit status of the most recently executed command." -
#
#       <https://www.thegeekstuff.com/2010/05/bash-shell-special-parameters/>
#         Bash special parameters explained with four example shell scripts
#
#     See also:
#
#       <https://www.gnu.org/software/bash/manual/bash.html>
#
function evaluateBuildResult()
{
    case $2 in
      0|7) echo ; echo "Build step $1 succeeded"                    >&2               ;;
      *)   echo ; echo "Build step $1 ($3) failed (error code $2)." >&2 ; echo ; exit ;;
    esac
} #evaluateBuildResult()


# ###########################################################################
#
# A helper function to reduce redundancy.
#
# Check if two strings are equal. It stops the entire build and 
# outputs a message (to standard output) if they are not. 
#
# Parameters:
#
#    $1   String 1
#
#    $2   String 2
#
#    $3   Build step number
#
#    $4   String identifying the build step
#
# Return value:
#
#   0 if            
#
function mustBeEqual()
{
    #if [ $1 == $2 ] ; evaluateBuildResult $3  $? "Two strings not equal: $1 and $2 ($4)"
    [[ $1 == $2 ]] ; evaluateBuildResult $3  $? "two strings not equal: $1 and $2 ($4)"
} #mustBeEqual()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Parameters:
#
#   $1   File name (a PHP file)
#
#   $2   Identification string (for use in the error message)
#
#   $3   Build number
#
function keyboardShortcutConsistencyCheck()
{
    startOfBuildStep $3 "Starting keyboard shortcut consistency check for $1"

    #perl -w ${SRCFOLDER_DOTNETCOMMANDLINE}/${WEBFORM_CHECK_FILENAME}  ${WEB_SRCFOLDER_BASE}/$1 ; evaluateBuildResult $3  $? "Keyboard shortcut consistency for the $2 page (file $1)"
    ${WEBFORM_CHECK_CMD}  ${WEB_SRCFOLDER_BASE}/$1 ; evaluateBuildResult $3  $? "Keyboard shortcut consistency for the $2 page (file $1)"
} #keyboardShortcutConsistencyCheck()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Parameters:
#
#   $1   File name
#
#   $2   Identification string
#
#   $3   Build number
#
#   $4   Sub folder / URL.
#
#        Examples:
#
#          %2Fworld                     for  https://pmortensen.eu/world/FixedStrings.php?OverflowStyle=Native
#
#          %2FEditOverflow%2F_Wordlist  for  https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html
#
function HTML_validation_base()
{
    startOfBuildStep $3 "Starting HTML validation for $1"

    export SUBMIT_URL="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu$4%2F$1%3FOverflowStyle=Native"

    # Output the submit URL (to the W3 validator), so we can easier and faster reproduce a problem
    echo "Submit URL for HTML validation: ${SUBMIT_URL}"
    echo

    wget -q -O- ${SUBMIT_URL} | grep -q 'The document validates according to the specified schema'      ; evaluateBuildResult $3 $? "W3 validation for $2 (file $1)"
} #HTML_validation_base()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Parameters:
#
#   $1   File name
#
#   $2   Identification string
#
#   $3   Build number
#
function HTML_validation()
{
    #echo "In HTML_validation()..."

    # Note: The quoting is needed, at least for "$2", so spaces
    #       in a string are not seen as separate parameters...

    HTML_validation_base "$1" "$2" "$3" '%2Fworld'
} #HTML_validation()


# ###########################################################################
#
# Helper function to reduce redundancy. For PHP code.
#
# It will also do some extra tests that are in common for
# all the different PHP scripts:
#
#     1. Detection of syntax errors
#
#     2. Detection of stray output from debugging - so it
#        does not end up on web pages
#
#     3. Detection of failed PHP unit tests (so we don't overlook them
#        (every run of the build script will check for it). The output
#        from failed tests would end up on the web page).
#
# Parameters:
#
#   $1   File name (of the PHP script)
#
#   $2   Identification string
#
#   $3   Build number
#
#   $4   Parameters to the PHP script, in URL query format.
#
#        Examples:
#
#          OverflowStyle=Native&PHP_DoWarnings=On
#
#   $5   Match string for standard error output (standard error
#        MUST contain this string for the test to pass)
#
#        Note: Specifying an empty string currently does not work. That is,
#              it can not be specified that standard error should be empty
#              (it will fail).
#
#              Thus the script MUST produce something to standard error...
#
function PHP_code_test()
{
    startOfBuildStep $3 "Start running PHP tests for $1: $2"

    #echo "In PHP_code_test()..."
    #echo "Dollar 1: $1..."
    #echo "Dollar 2: $2..."
    #echo "Dollar 3: $3..."
    #echo "Dollar 4: $4..."

    #echo
    #echo "In PHP_code_test(). Current folder:" `pwd`


    # #####################################
    #
    # 1. Sanity check of parameters
    #
    # Later: Also for build number (small integer)
    #
    # The fourth parameter should be in URL query format
    echo $4 | grep -q "=" ; evaluateBuildResult $3 $? "PHP test (step 1), internal test: The fourth parameter to function PHP_code_test() does not appear to be in URL query format: $4 "


    # #####################################
    #
    # 2. Execute the PHP script (and capture both
    #    standard output and standard error)
    #
    # Note: $3 (build number) is to make it unique (so we don't overwrite
    #       previous output files (for the current build script run)).
    #       $1 is for information only.
    #
    export PHPRUN_ID="$3_$1"
    export STDERR_FILE="_stdErr_${PHPRUN_ID}.txt"

    # The PHP script outputs HTML to standard output
    export HTML_FILE_PHP="_HTML_${PHPRUN_ID}.html"

    #php $1 "$4" > /home/embo/temp2/2021-09-14/_Text.html 2> ${STDERR_FILE}
    #php $1 "$4"
    php $1 "$4"  > ${HTML_FILE_PHP}  2> ${STDERR_FILE}

    #cat ${STDERR_FILE}
    #echo "Standard error:" `cat ${STDERR_FILE}`


    # #####################################
    #
    # 3. Detect missing files (as seen by the PHP interpreter)
    #
    export MISSINGFILES_MATCHSTRING="Could not open input file: "
    grep -q "${MISSINGFILES_MATCHSTRING}" ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 3): $2 (file $1). Extra information: \"`grep "${MISSINGFILES_MATCHSTRING}" ${HTML_FILE_PHP}`\""


    # #####################################
    #
    # 4. Evaluate the result from standard error (from the script).
    #    It must match the passed in match string.
    #
    # Note: This will indirectly detect syntax errors as
    #       the expected output to standard error will
    #       be absent (syntax errors stops any
    #       further execution).
    #
    ##export fileName=Text.php
    ##export ID_str="Self test, for undefined PHP variables"
    ##export buildNumber=17
    #grep -q $5 ${STDERR_FILE} ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: `cat ${STDERR_FILE}`"
    #
    # 'head' is for limiting the output (but there is usually only one line)
    #
    grep -q "$5" ${STDERR_FILE} ; evaluateBuildResult $3 $? "PHP test (step 14: $2 (file $1). Extra information: expected \"$5\", but got \"`head -n 3 ${STDERR_FILE}`\"  "


    # #####################################
    #
    # 5. Detect some stray debug output in our PHP code
    #    (to standard output). Common for all.
    #
    # "test $? -ne 0" is for inverting/negating the return code ("$?").
    #
    export STRAYDEBUGGING_MATCHSTRING1="First argument: "
    #! (grep -q "First argument: " ${HTML_FILE_PHP}) ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: `head -n 3 ${HTML_FILE_PHP}`"
    #grep -q "First argument: " ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: `head -n 3 ${HTML_FILE_PHP}`"
    #grep -q "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP} ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: `grep "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP}`"
    grep -q "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 5a): $2 (file $1). Extra information: \"`grep "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP}`\"  "

    export STRAYDEBUGGING_MATCHSTRING2="Some URL: "
    grep -q "${STRAYDEBUGGING_MATCHSTRING2}" ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 5b): $2 (file $1). Extra information: \"`grep "${STRAYDEBUGGING_MATCHSTRING2}" ${HTML_FILE_PHP}`\"  "


    # #####################################
    #
    # 6. Detect unit test failures. Note: Currently only executed in the
    #    context of Text.php
    #
    # Is there an easier way? Could we configure the PHP installation to
    # automatically stop on all errors and warnings?
    #
    export UNITTEST_MATCHSTRING="Failed test. ID: "
    grep -q "${UNITTEST_MATCHSTRING}" ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 6): $2 (file $1). Extra information: \"`grep "${UNITTEST_MATCHSTRING}" ${HTML_FILE_PHP}`\"  "


    # #####################################
    #
    # 7. Detect more errors that are not syntax errors (they
    #    will not happen until run time, but they still
    #    terminate the execution).
    #
    #    Stop on all fatal errors except the database
    #    error that we currently expect.
    #
    # Example:
    #
    #   PHP Fatal error:  Uncaught ArgumentCountError: Too few arguments to function WikiMedia_Link(), 1 passed in /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/Text.php on line 347 and exactly 2 expected in /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/commonStart.php:486
    #
    export FATALERROR_MATCHSTRING="PHP Fatal error: "
    #grep -q "${FATALERROR_MATCHSTRING}" ${STDERR_FILE} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: \"`grep "${FATALERROR_MATCHSTRING}" ${STDERR_FILE}`\""
    grep -v 'Access denied for user' ${STDERR_FILE} | grep -q "${FATALERROR_MATCHSTRING}" ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 7): $2 (file $1). Extra information: \"`  grep -v 'Access denied for user' ${STDERR_FILE} | grep -q "${FATALERROR_MATCHSTRING}"  `\". Standard error: `head -n 3 ${STDERR_FILE}`   "


    # #####################################
    #
    # 8. Stop on all "PHP notices", except the
    #    intentional one (variable "dummy2")
    #
    # For dummy2:
    #
    #   PHP Notice:  Undefined variable: dummy2 in /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/commonStart.php on line 120
    #
    # Example for a true undefined variable (in a function):
    #
    #   PHP Notice:  Undefined variable: URLZZZZZZZ in /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/commonStart.php on line 504
    #
    export NOTICE_MATCHSTRING="PHP Notice: "
    grep -v dummy2 ${STDERR_FILE} | grep -q "${NOTICE_MATCHSTRING}" ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test  (step 8): $2 (file $1). Extra information: \"`  grep -v dummy2 ${STDERR_FILE} | grep "${NOTICE_MATCHSTRING}"  `\""
} #PHP_code_test()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Used to detect changes to files (e.g. a web
# server error log). Used with endWatchFile().
#
# Parameters:
#
#   $1   File to watch for changes
#
function startWatchFile()
{
    # File size for now. Perhaps MD5 hash later

    export FILE_SIZE_BEFORE=`wc $1`
} #startWatchFile()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Used to detect changes to files (e.g. a web
# server error log). Used with startWatchFile()
#
# Parameters:
#
#   $1   File to check for changes
#
# Return code:
#
#   0 if not change to the file
#
#   Non-zero if there was a change to the file
#
function endWatchFile()
{
    # File size for now. Perhaps MD5 hash later

    export FILE_SIZE_AFTER=`wc $1`

    [ "${FILE_SIZE_BEFORE}" = "${FILE_SIZE_AFTER}" ]
} #endWatchFile()


# ###########################################################################
#
# Helper function to reduce redundancy. For PHP code.
#
#   For now, the primary purpose is to detect new entries in the
#   web server error log (most likely only due to PHP errors) as
#   a result of retrieving a web page (with wget).
#
#   This is to detect errors when the PHP scripts are running in
#   a web server context, as opposed to a command line context
#   that we also use for testing. For instance, a PHP file
#   gets passed command-line parameters from our tests
#   when running in command line context, whereas in
#   a web context it does not get any command line
#   parameters (we had an actual error due to this).
#
#   PHP may also be configured differently for command line
#   and web (e.g., two different )
#
# Future:
#
#   1. Positive tests to test if particular configuration
#      changes actually have the expected effect.
#
#      Will then become regression tests for the expected
#      configuration.
#
#   2. XXXXXXXXXx
#
#
# Note: For now it only works for the ***local web server***
#       (but it could be extended as soon as we have a means
#       to programmatically retrieve the error log from the
#       remote web server (we already push files in the
#       opposite direction))
#
# It is assumed the PHP files have been copied
# to the web server by some other means.
#
#
# Parameters:
#
#   $1   URL. Including any query string
#
#   $2   Identification string. Must not contain space - is used for
#        file names (and command-line arguments)
#
#   $3   Build number
#
function webServer_test()
{
    #Delete at any time
    #echo "In webServer_test()..."
    #echo "Dollar 1: $1..."
    #echo "Dollar 2: $2..."
    #echo "Dollar 3: $3..."
    #echo "Dollar 4: $4..."

    startOfBuildStep $3 "Start detection of error log entries for a web server. ID: $2"


    export RETRIEVED_HTML_FILE="_$2.html"

    #Delete at any time
    #export SIZE_BEFORE_WEBERROR_LOG=`wc ${LOCAL_WEB_ERRORLOG_FILE}`

    startWatchFile ${LOCAL_WEB_ERRORLOG_FILE}

    # That is, invoke the specified web page with PHP on the
    # web server. If this results in entries to the error
    # log, we will detect it below.
    #
    #wget -o ${RETRIEVED_HTML_FILE} "$1"
    wget -q -O ${RETRIEVED_HTML_FILE} "$1"

    #Delete at any time
    #export SIZE_AFTER_WEBERROR_LOG=`wc ${LOCAL_WEB_ERRORLOG_FILE}`

    #echo "Web server error log size before: ${SIZE_BEFORE_WEBERROR_LOG}"
    #echo "Web server error log size after: ${SIZE_AFTER_WEBERROR_LOG}"

    #echo ; date ; echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 5


    # Detect new entries in the web server error log
    # file as a result of retrieving a web page.
    #
    #[ "${SIZE_BEFORE_WEBERROR_LOG}" != "${SIZE_AFTER_WEBERROR_LOG}" ] ; test $? -ne 0 ; evaluateBuildResult $3 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "
    #[ "${SIZE_BEFORE_WEBERROR_LOG}" = "${SIZE_AFTER_WEBERROR_LOG}" ] ; evaluateBuildResult $3 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "
    endWatchFile ${LOCAL_WEB_ERRORLOG_FILE} ; evaluateBuildResult $3 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "

    #Delete at any time
    ##if [ "${SIZE_BEFORE_WEBERROR_LOG}" = "${SIZE_AFTER_WEBERROR_LOG}" ]; then
    #if [ "${SIZE_BEFORE_WEBERROR_LOG}" != "${SIZE_AFTER_WEBERROR_LOG}" ]; then
    #
    #    echo
    #    echo "New entries in the web server log!"
    #
    #    #echo ; date ;
    #    echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 2
    #fi
} #webServer_test()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Retrieve the web server error log (or is it the PHP one only?).
#
# We currently expect the name "phperrors_777.log". It is
# set up in the WordPress root folder - one of them is
# <public_html/world/.htaccess>
#
# Parameter:
#
#    $1   Prefix / ID. E.g. "" or ""
#
function retrieveWebHostingErrorLog()
{
    # The
    #
    # Or should it be </var/www/pmortensen.eu>?
    #
    export FTP_COMMANDS="mirror --verbose  _webErrorlog  / ; exit"
    export FTP_COMMANDS="mirror --verbose  _webErrorlog  /test ; exit"
    export FTP_COMMANDS="mirror --verbose  /test  _webErrorlog   ; exit"
    export FTP_COMMANDS="mirror --verbose  --include com*.php   /test  _webErrorlog   ; exit"
    export FTP_COMMANDS="mirror --verbose  --include *.html     /test  _webErrorlog   ; exit"
    export FTP_COMMANDS="mirror --verbose  --exclude *.html     /test  _webErrorlog   ; exit"
    export FTP_COMMANDS="mirror --verbose  --no-recursion       /test  _webErrorlog   ; exit"
    export FTP_COMMANDS="mirror --verbose  --no-recursion  --include *.log  /  ${WEB_ERRORLOG_SUBFOLDER}  ; exit"

    # We couldn't get the "--include" option to work. It failed
    # with an error and no files transferred. E.g.:
    #
    #     mv: cannot stat '_webErrorlog3/phperrors_777.log': No such file or directory
    #
    # Instead we download all files in the root folder
    # (currently four files and the two files
    # starting with "." are not transferred
    # be default).
    #
    # Should we use option "--include-glob" instead?
    #
    export FTP_COMMANDS="mirror --verbose  --no-recursion  /  _webErrorlog               ; exit"
    export FTP_COMMANDS="mirror --verbose  --no-recursion  /  ${WEB_ERRORLOG_SUBFOLDER}  ; exit"


    export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
    eval ${LFTP_COMMAND}
        #     ; evaluateBuildResult 25 $? "copying the HTML word list to the web site"


    #Too complicated - use startWatchFile() / endWatchFile() instead.
    export NEW_FILENAME="$1${REMOTE_WEB_ERRORLOG_FILENAME}"
    export NEW_FILENAME_PARTIALPATH="${WEB_ERRORLOG_SUBFOLDER}/${NEW_FILENAME}"

    mv ${WEB_ERRORLOG_SUBFOLDER}/${REMOTE_WEB_ERRORLOG_FILENAME}  ${NEW_FILENAME_PARTIALPATH}

    # Note: Use of md5sum this way to avoid the input file name
    #       in the output.
    #
    cat ${NEW_FILENAME_PARTIALPATH} | md5sum > "$1WebHostingErrorLog_MD5.txt"


    #ls -ls
    #ls -lsatr ${WEB_ERRORLOG_SUBFOLDER}
    #
    #echo
    #
    #ls -lsatr | grep MD5
    #
    #echo
    #tail -n 5 ${NEW_FILENAME_PARTIALPATH}
    #echo
    #echo
} #retrieveWebHostingErrorLog()




# ###########################################################################
# ###########################################################################
# ###########################################################################
#
#   Start of the build script

timeStamp "Start time"


# Force entering the password at the beginning of the
# script run (to avoid prompting for it in the middle
# of the run).
#
# Note: This will ***not*** work as expected (if entering the
#       password is required) if pasting ***several*** lines
#       ***after*** invoking this script (e.g. with
#       "./Linux.sh").
#
#       In this case, enter the following manually, as a
#       separate step. In order words, this line should
#       be the LAST line if pasting several lines.
#
sudo ls > /dev/null


# ###########################################################################
#
#  Self test of Python/Selenium script
#
startOfBuildStep "1" "Internal check of the web interface regression tests"

#
# Sort of prerequisite for testing
#
# Method names in Python are not checked for uniqueness, so only
# one will be used, risking not running all of the Seleniums
# tests (a sort of a (potential) ***false negative*** test).
#
#geany /home/embo/UserProf/At_XP64/Edit_Overflow/Web_Application/__regressTest__/web_regress.py
#
cd ${SELINUM_DRIVERSCRIPT_DIR}

# Something to consider:
#
#   The number of arguments to the helper function textTransformation() in
#   the Selenium Python script is now 6, triggering Pylint (disabled for
#   now (R0913). Pylint is OK with 5...).
#
#   Alternatively, we could configure Pylint to accept 6 instead of 5.
#   See e.g. <https://stackoverflow.com/a/816789>.
#
#pylint --disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125  $SELINUM_DRIVERSCRIPT_FILENAME ; evaluateBuildResult 1 $? "Python linting for the Selenium script"
pylint --disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125 --disable=R0913  $SELINUM_DRIVERSCRIPT_FILENAME ; evaluateBuildResult 1 $? "Python linting for the Selenium script"


# ###########################################################################
#
# Copy files to the workfolder (mostly where we compile, but also
# for the local web server (that we use for PHP code tests using
# the local web server)).
#
# We use option "-p" to avoid error output when
# the folder already exists (reruns), like:
#
#     "mkdir: cannot create directory ‘/home/embo/temp2/2020-06-03’: File exists"
#
startOfBuildStep "2" "Copying files to the build folder, etc."

echo "Build folder: ${WORKFOLDER}"
echo

mkdir -p $WORKFOLDER1
mkdir -p $WORKFOLDER2
mkdir -p $WORKFOLDER3
mkdir -p $WORKFOLDER
mkdir -p $FTPTRANSFER_FOLDER_HTML
mkdir -p $FTPTRANSFER_FOLDER_JAVASCRIPT
mkdir -p $LOCAL_WEBSERVER_FOLDER


# Remove any existing
#
# Note: If the previous run of this script stopped prematurely,
#       we may get (as a later step moved ):
#
#           mv: cannot stat '/home/embo/temp2/2021-02-03/_DotNET_tryout/EditOverflow4/Program.cs': No such file or directory
#
#
#
mv $WORKFOLDER/${FILE_WITH_MAIN_ENTRY}          $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}


cd $SRCFOLDER_DOTNETCOMMANDLINE
cp ${FILE_WITH_MAIN_ENTRY}                      $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}

cp EditOverflow3.csproj                         $WORKFOLDER
cp EditOverflow3_UnitTests.csproj               $WORKFOLDER

cp $SRCFOLDER_CORE/TermLookup.cs                $WORKFOLDER
cp $SRCFOLDER_CORE/TermData.cs                  $WORKFOLDER
cp $SRCFOLDER_CORE/HTML_builder.cs              $WORKFOLDER
cp $SRCFOLDER_CORE/CodeFormattingCheck.cs       $WORKFOLDER
cp $SRCFOLDER_CORE/LookUpString.cs              $WORKFOLDER
cp $SRCFOLDER_CORE/StringReplacerWithRegex.cs   $WORKFOLDER
cp $SRCFOLDER_CORE/Utility.cs                   $WORKFOLDER
cp $SRCFOLDER_CORE/RegExExecutor.cs             $WORKFOLDER
cp $SRCFOLDER_CORE/WordCorrector.cs             $WORKFOLDER
cp $SRCFOLDER_CORE/EditSummaryStyle.cs          $WORKFOLDER
cp $SRCFOLDER_CORE/CheckinMessageBuilder.cs     $WORKFOLDER


cp $SRCFOLDER_PLATFORM_SPECIFIC/EditorOverflowApplication_Unix.cs  $WORKFOLDER
cp $SRCFOLDER_PLATFORM_SPECIFIC/EditorOverflowApplication.cs       $WORKFOLDER

## cp $SRCFOLDER_TESTS/StringReplacerWithRegexTests.cs  $WORKFOLDER
cp $SRCFOLDER_TESTS/EnvironmentTests.cs                 $WORKFOLDER
cp $SRCFOLDER_TESTS/WordlistTests.cs                    $WORKFOLDER
cp $SRCFOLDER_TESTS/LookUpStringTests.cs                $WORKFOLDER
cp $SRCFOLDER_TESTS/StringReplacerWithRegexTests.cs     $WORKFOLDER
cp $SRCFOLDER_TESTS/CodeFormattingCheckTests.cs         $WORKFOLDER

# PHP/web files
cp $SRCFOLDER_WEB/EditOverflow.php                      $WORKFOLDER
cp $SRCFOLDER_WEB/commonStart.php                       $WORKFOLDER
cp $SRCFOLDER_WEB/eFooter.php                           $WORKFOLDER
cp $SRCFOLDER_WEB/StringReplacerWithRegex.php           $WORKFOLDER
cp $SRCFOLDER_WEB/commonEnd.php                         $WORKFOLDER
cp $SRCFOLDER_WEB/deploymentSpecific.php                $WORKFOLDER
#
cp $SRCFOLDER_WEB/Text.php                              $WORKFOLDER
cp $SRCFOLDER_WEB/FixedStrings.php                      $WORKFOLDER
cp $SRCFOLDER_WEB/EditSummaryFragments.php              $WORKFOLDER


# To the local web server
sudo cp $SRCFOLDER_WEB/EditOverflow.php                 $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/commonStart.php                  $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/eFooter.php                      $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/StringReplacerWithRegex.php      $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/commonEnd.php                    $LOCAL_WEBSERVER_FOLDER

# Only once at the web server location. Though ideally we want to
# patch it on the fly so we are it is actually updated if needed.
#
#sudo cp $SRCFOLDER_WEB/deploymentSpecific.php           $LOCAL_WEBSERVER_FOLDER

sudo cp $SRCFOLDER_WEB/Text.php                         $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/FixedStrings.php                 $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/EditSummaryFragments.php         $LOCAL_WEBSERVER_FOLDER


# Compile, run unit tests, run, and redirect SQL & HTML output to files
#
echo
cd $WORKFOLDER
#echo Work folder: $WORKFOLDER



# #################################################################
#
# Local test of web part/PHP files before remote/production
# deployment (syntax, unit tests, and perhaps local
# integration test (with local web server)), deployment.
#
# (? It also involves copying PHP files to a worker folder (as
# 'lftp' can not work on individual files))
#
# See ID 10460, d.

# ###########################################################################
#
#  Test of the main lookup function. At the moment
#  the primary purpose is to detect syntax errors
#  (so we don't completely break production) as
#  an early database error means we can
#  not test that the HTML output is
#  correct or not.
#
#  Note that we currently get this error early
#  (to standard error) and very little HTML
#  output is produced:
#
#    PHP Fatal error:  Uncaught PDOException: SQLSTATE[HY000] [1045] Access denied for user 'pmortensen_eu'@'213.83.133.56' (using password:
#startOfBuildStep "18" "Start running PHP tests, main lookup"
#
# For now, we rely/expect on the database error. Do we risk
# false negatives if the output contains BOTH this error
# and some other error? At least for syntax error that
# is not the case.

PHP_code_test  EditOverflow.php          "main lookup"              3  "OverflowStyle=Native&LookUpTerm=JS"  "Access denied for user"


# ###########################################################################
#
#  Self test of PHP configuration, syntax errors in some scripts, etc.
#
#  It also happens to run the unit tests for this 'text' window (of
#  the helper functions).
#
#  Sort of self test (that we can actually detect undefined variables
#  in PHP). Note that we don't use EditOverflow.php as it is
#  (currently) subject to an early database access error
#  when running locally.
#
#  We expect to get something like this on standard error:
#
#      PHP Notice:  Undefined variable: dummy2 in /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/commonStart.php on line 120
#
#  Note that it will also detect any syntax errors in commonStart.php
#
PHP_code_test  Text.php                  "self test, unit tests"                  4  "OverflowStyle=Native&PHP_DoWarnings=On"  "Undefined variable: dummy2"

#exit



# ###########################################################################
#
# Invoke the function "Real quotes" in the Edit Overflow "text" window
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
PHP_code_test  Text.php                  "text transformation - 'Real quotes'"    5  "OverflowStyle=Native&PHP_DoWarnings=On&someText=dasdasd&someAction%5Breal_quotes%5D=Real+quotes"  "Undefined variable: dummy2"

#exit


# ###########################################################################
#
#  Test of the fixed strings page (essentially static HTML)
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
PHP_code_test  FixedStrings.php          "fixed strings"                          6  "OverflowStyle=Native&PHP_DoWarnings=On"   "Undefined variable: dummy2"


# ###########################################################################
#
#  Test of the edit summary fragments page (essentially static HTML)
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
PHP_code_test  EditSummaryFragments.php  "edit summary fragments"                 7  "OverflowStyle=Native&PHP_DoWarnings=On"   "Undefined variable: dummy2"


# ###########################################################################
#
#  Detection of error log entries for the local web server (there shouldn't
#  be any).
#
#
# Note: Only HTTP works with our local webserver...
#
#
# We can't use EditOverflow.php at the moment because the
# database error goes to the web server log...
#
# Note: Query parameter "LookUpTerm" must be specified. Otherwise we get
#       at strange error in the line "$URL = htmlentities($row['URL']);"
#
#

# A word lookup that fails (the word does not exist in the database)
webServer_test  "http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=JS"   "localWebserver_Edit_Overflow_lookup"    8

# Missing expected query parameter "LookUpTerm"
webServer_test  "http://localhost/world/EditOverflow.php?OverflowStyle=Native"                 "localWebserver_Edit_Overflow_lookup"    9

# A word lookup that succeeds
webServer_test  "http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=Ghz"  "localWebserver_Edit_Overflow_lookup"   10

webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native"                        "localWebserver_Text"                    11
webServer_test  "http://localhost/world/FixedStrings.php?OverflowStyle=Native"                "localWebserver_fixed_strings"           12
webServer_test  "http://localhost/world/EditSummaryFragments.php?OverflowStyle=Native"        "localWebserver_Edit_summary_fragments"  13

# Invoke the function "Remove TABs and trailing whitespace" in
# the Edit Overflow "text" window
#
webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native&someText=XYZ%20%20%20&someAction%5Bremove_TABs_and_trailing_whitespace%5D=Remove+TABs+and+trailing+whitespace"  "localWebserver_Text_RemoveTABsAndTrailingWhitespace"  14

# Invoke the function "Remove common leading space" in
# the Edit Overflow "text" window
#
webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native&someText=%20%20%20XYZ&someAction%5Bremove_common_leading_space%5D=Remove+common+leading+space"  "localWebserver_Text_RemoveCommonLeadingSpace"                         15

# Invoke the function "Real quotes" in the Edit Overflow "text" window
#
webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native&someText=dasdasd&someAction%5Breal_quotes%5D=Real+quotes"  "localWebserver_Text_RealQuotes"                                                                            16

#exit


# ###########################################################################
#
# NUnit tests can actually run
# under .NET Core on Linux...
#
startOfBuildStep "17" "Start running C# unit tests"

# Note: unlike "dotnet run", "dotnet test" does not
#       use option "-p" for specifying the project
#       file name (inconsistent)
#
dotnet test EditOverflow3_UnitTests.csproj  ; evaluateBuildResult 17 $? "C# unit tests"



# Prepare for the main run (see in the beginning for an explanation)
mv  $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}  $WORKFOLDER/${FILE_WITH_MAIN_ENTRY}

# This is to hide the unit test files from the normal
# project file (for the normal run). Hardcoded for
# now (some redundancy)
#
mv  $WORKFOLDER/EnvironmentTests.cs               $WORKFOLDER/EnvironmentTests.csZZZ
mv  $WORKFOLDER/WordlistTests.cs                  $WORKFOLDER/WordlistTests.csZZZ
mv  $WORKFOLDER/LookUpStringTests.cs              $WORKFOLDER/LookUpStringTests.csZZZ
mv  $WORKFOLDER/StringReplacerWithRegexTests.cs   $WORKFOLDER/StringReplacerWithRegexTests.csZZZ
mv  $WORKFOLDER/CodeFormattingCheckTests.cs       $WORKFOLDER/CodeFormattingCheckTests.csZZZ
mv  $WORKFOLDER/RegExExecutor.cs                  $WORKFOLDER/RegExExecutor.csZZZ


# ###########################################################################
#
# Fixed header for the SQL (not generated by Edit Overflow)
#
# Note 1: If the header file does not exist the symptom is something
#         like (because the database table is not cleared):
#
#             MySQL said: Documentation
#             #1062 - Duplicate entry 'y.o' for key 'PRIMARY'
#
# Note 2: File "Header_EditOverflow_forMySQL_UTF8.sql" is in the
#         standard backup (not yet formally checked in)
#
# Note 3: The containing folder of $SQL_FILE (from the
#         client side) must also exist
#
# Moved to PC2016 after SSD-related Linux system crash 2020-05-31
#cat /home/mortense2/temp2/2020-02-05/Header_EditOverflow_forMySQL_UTF8.sql  > $SQL_FILE
#cat /home/mortensen/temp2/2020-05-30/Backup/Backup_2020-05-30_smallFiles/2020-05-30/Header_EditOverflow_forMySQL_UTF8.sql  > $SQL_FILE
cat '/home/embo/temp2/2020-06-02/Last Cinnamon backup_2020-05-30/Small files/Header_EditOverflow_forMySQL_UTF8.sql'  > $SQL_FILE

# Note: Compiler errors will be reported to standard
#       error, but we currently don't redirect it.
#
#       CS0162 is "warning : Unreachable code detected"
#
startOfBuildStep "18" "Exporting the word list as SQL"

export WORDLIST_OUTPUTTYPE=SQL
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162   >> $SQL_FILE  ; evaluateBuildResult 18 $? "generation of word list in SQL format"

echo
pwd
echo
ls -ls $SQL_FILE


# ###########################################################################
#
# Open some web pages for manual operations. We do this
# as soon as the (SQL) file for the word list is ready
# to be ***imported*** (so the focus changes (Firefox
# comes to the foreground) signals that it is
# ready for import.
#
startOfBuildStep "19" "Opening some web pages and applications for manual operations"

# Open a web page for verification of push to GitHub
xdg-open "https://github.com/PeterMortensen/Edit_Overflow"
sleep 2

# Open a web page for verification of MySQL update (for the
# wordlist) - that new words have actually been added
xdg-open "https://pmortensen.eu/world/EditOverflow.php?LookUpTerm=php&OverflowStyle=Native&UseJavaScript=no"
sleep 2

# Open the Simply.com (formerly UnoEuro) import
# page (immediately so we can prepare while
# the rest of the script is running)
xdg-open "https://www.simply.com/dk/controlpanel/pmortensen.eu/mysql/"

# Open Visual Studio Code (for version control operations (Git))
code


# ###########################################################################
#
# Some redundancy here - to be eliminated
startOfBuildStep "20" "Exporting the word list as HTML"

export WORDLIST_OUTPUTTYPE=HTML
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162   > $HTML_FILE  ; evaluateBuildResult 20 $? "generation of word list in HTML format"

cp  $HTML_FILE  $HTML_FILE_GENERIC

echo
ls -ls $HTML_FILE_GENERIC


# ###########################################################################
#
# That is, for keyboard shortcut and ID uniqueness
# rules in the generated HTML for the word list,
# etc. (using a Perl script).
#
startOfBuildStep "21" "Starting checking generated HTML"

${WEBFORM_CHECK_CMD}  ${HTML_FILE_GENERIC} ; evaluateBuildResult 21  $? "Checking generated HTML (file ${HTML_FILE_GENERIC})"



# ###########################################################################
#
# Some redundancy here - to be eliminated
startOfBuildStep "22" "Exporting the word list as JavaScript"

export WORDLIST_OUTPUTTYPE=JavaScript
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162  > $JAVASCRIPT_FILE  ; evaluateBuildResult 22 $? "generation of word list in JavaScript format"

# In the work folder
cp  $JAVASCRIPT_FILE  $JAVASCRIPT_FILE_GENERIC

# Temporary: Copy the word list in JavaScript to a source folder (for
#            unit testing - the next step).
#
#            It is not necessary if we use a build folder (it is
#            already generated there)
#
cp  $JAVASCRIPT_FILE_GENERIC  $WEBFOLDER

echo
ls -ls $JAVASCRIPT_FILE_GENERIC


# ###########################################################################
#
startOfBuildStep "23" "Start running JavaScript unit tests"

# Note: For now, directly in source folder. It should
#       be moved to the work folder.

cd $WEBFOLDER

# That is using Jest under Node.js, with the test files
# in sub folder "__tests__".
npm test  ; evaluateBuildResult 23 $? "JavaScript unit tests"

# Back to the previous folder (expected to be the work folder)
#
echo
# But will output something...
cd -



# ###########################################################################
#
# Check of:
#
#   1. keyboard shortcuts conflicts and
#   2. Indentation rules (even even number of spaces)
#
keyboardShortcutConsistencyCheck EditOverflow.php         "Edit Overflow lookup"       24

keyboardShortcutConsistencyCheck Text.php                 "text stuff"                 25

keyboardShortcutConsistencyCheck FixedStrings.php         "fixed string"               26

keyboardShortcutConsistencyCheck EditSummaryFragments.php "edit summary"               27



# ###########################################################################
#
# Copy the JavaScript code (for the word list) to the public web site.
#
#   Note: Environment variables FTP_USER and FTP_PASSWORD
#         must have been be set beforehand.
#
# The mirror command for 'lftp' does not work for single files...
#
# It is near the very end, so we don't push anything
# to production if some test fail.
#
startOfBuildStep "28" "Updating the JavaScript word list file on pmortenen.eu (<https://pmortensen.eu/world/EditOverflowList.js>)"

cp  $JAVASCRIPT_FILE_GENERIC  $FTPTRANSFER_FOLDER_JAVASCRIPT
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_JAVASCRIPT} /public_html/world ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
eval ${LFTP_COMMAND}  ; evaluateBuildResult 28 $? "copying the word list in JavaScript to the web site"


# ###########################################################################
#
# Copy the HTML to the public web site.
#
#   Note: Environment variables FTP_USER and FTP_PASSWORD
#         must have been be set beforehand.
#
# The mirror command for 'lftp' does not work for single files...
#
# It is at the very end, so we don't push anything
# to production if some test fail.
#
startOfBuildStep "29" "Updating the HTML word list file on pmortenen.eu (<https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>)"

cp  $HTML_FILE_GENERIC  $FTPTRANSFER_FOLDER_HTML
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_HTML} /public_html/EditOverflow/_Wordlist ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
eval ${LFTP_COMMAND}  ; evaluateBuildResult 29 $? "copying the HTML word list to the web site"


# ###########################################################################
#
# HTML validation, both for the semi-static HTML pages and
# the (generated) word list in HTML format.
#
# It is currently dependent on an external service,
# over the Internet. Service failures:
#
#   2021-10-05T154904   "The connection has timed out. The server at
#                        validator.w3.org is taking too long to respond."
#
#                        Still not up as of (a Tuesday):
#
#                           2021-10-05T151046Z+0
#                           2021-10-05T160837Z+0
#                           2021-10-05T161242Z+0
#                           2021-10-06T002617Z+0
#
#                        Worked again on 2021-10-06T072909.
#
if [ ${DISABLE_HTMLVALIDATION} != 1 ]; then
    HTML_validation      EditOverflow.php                   "Edit Overflow lookup"    30
    HTML_validation      Text.php                           "Text stuff"              31
    HTML_validation      FixedStrings.php                   "Fixed strings"           32
    HTML_validation      EditSummaryFragments.php           "Edit summary fragments"  33

    # The URL is <https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>
    #
    # Sometimes we get this result:
    #
    #     503 Service Unavailable
    #     No server is available to handle this request.
    #
    HTML_validation_base ${HTML_FILE_GENERIC_FILENAMEONLY}  "Word list (HTML)"        34  '%2FEditOverflow%2F_Wordlist'
fi



# ###########################################################################
#
# Some checks of the generated HTML:
#
#   1. Unique HTML anchors (only one if we 'grep' for one)
#
#   2. XXX
#
startOfBuildStep "35" "Starting checks of the generated HTML"
export MATCHING_LINES=`grep -c '<div id="Mark_Zuckerberg">'  ${HTML_FILE_GENERIC}`
mustBeEqual ${MATCHING_LINES} 1  35   "HTML anchor is not unique"



# ###########################################################################
#
# End-to-end testing of the web interface, using the local web server.
#
# Note: Only the ***"Text" window*** and limited testing on that.
#
# It uses Selenium and is quite slow, even using a local web server...
#
startOfBuildStep "36" "Starting web interface regression tests, local"

startWatchFile ${LOCAL_WEB_ERRORLOG_FILE}

#export PATH=$PATH:${SELINUM_DRIVERFOLDER}
python3 $SELINUM_DRIVERSCRIPT_FILENAME TestMainEditOverflowLookupWeb.test_local_text  ; evaluateBuildResult 36 $? "web interface regression tests"

endWatchFile ${LOCAL_WEB_ERRORLOG_FILE} ; evaluateBuildResult 36 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "


# ###########################################################################
#
# End-to-end testing of the web interface, using the local web server.
#
# Note: For the ***main look*** (primary function of Edit Overflow)
#
# It uses Selenium and is quite slow, even using a local web server...
#
startOfBuildStep "37" "Starting web interface regression tests, local"

startWatchFile ${LOCAL_WEB_ERRORLOG_FILE}

#export PATH=$PATH:${SELINUM_DRIVERFOLDER}
python3 $SELINUM_DRIVERSCRIPT_FILENAME TestMainEditOverflowLookupWeb.test_mainLookup_form_localWebserver ; evaluateBuildResult 37 $? "web interface, main lookup, using the local web server"

endWatchFile ${LOCAL_WEB_ERRORLOG_FILE} ; evaluateBuildResult 37 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "



# ###########################################################################
#
# End-to-end testing of the web interface, using the hosting (live server).
#
# Note: This presumes the PHP files have been deployed to
#       production (this is currently a manual process)
#
# It uses Selenium and is quite slow
#
startOfBuildStep "38" "Starting web interface regression tests, production"

retrieveWebHostingErrorLog  "_before_"

# Note:
#
#   1. Sometimes the test fails and we get:
#
#         selenium.common.exceptions.NoSuchElementException: Message: Unable to locate element: [name="someText"]
#
#      The reason is we have interfered by doing GUI stuff while
#      the test is running... To fix it, rerun this build script.


# For now: Not assuming executable 'geckodriver' is in the path
#export PATH=$PATH:${SELINUM_DRIVERFOLDER}

# For exploring what "discover" is actually useful for...
#
#cd ${SELINUM_DRIVERSCRIPT_DIR}
#ls -lsatr
##python3 -m unittest discover  # Nothing is output to the screen from the python3 line...
##python3 -m unittest discover $SELINUM_DRIVERSCRIPT_FILENAME
#python3 -m unittest discover $SELINUM_DRIVERSCRIPT_DIR   # No tests are run "Ran 0 tests in 0.000s"
#cd -

# For now: directly from the source folder,
#          not the work folder
#
# Note: We can run a specific test, but we can't do the opposite,
#       ***exclude*** a particular test from the command line
#       ("not" is supported in Pytest, but not in 'unittest').
#       This is instead done by modification of
#       the 'web_regress.py' file.
#
#python3 $SELINUM_DRIVERSCRIPT_FILENAME  -k "test_mainLookup_JavaScript"  ; evaluateBuildResult 11 $? "web interface regression tests"
python3 $SELINUM_DRIVERSCRIPT_FILENAME  ; evaluateBuildResult 38 $? "web interface regression tests"

# Observed 2021-11-05T003000, but only once (it
# did not happen when repeating the build):
#
#     (New entry in error log:
#
#     [04-Nov-2021 23:24:28 UTC]
#
#     PHP Warning:  Cannot modify header information - headers already sent by
#     (output started at
#     /var/www/pmortensen.eu/public_html/world/wp-includes/link-template.php:682)
#     in /var/www/pmortensen.eu/public_html/world/wp-includes/pluggable.php
#     on line 1265 )) failed (error code 1).
#
# Some temporary problem on the hosting?


retrieveWebHostingErrorLog  "_after_"


#Too complicated - use startWatchFile() / endWatchFile() instead.
# Detection of new entries to the error log (normally PHP errors) as
# a result of our excersing web pages in production
#
# Note: Not a separate build number as we are just detecting errors (on
#       the web server) as a result of running a Selenium test.
#
mustBeEqual  "`cat _before_WebHostingErrorLog_MD5.txt`"  "`cat _after_WebHostingErrorLog_MD5.txt`"  38  "New entry in error log: `tail -n 1 ${AFTER_LOGFILE}` "



# ###########################################################################
#
# List the result files - for manual checking
#
echo
pwd
ls -lsatr $WORKFOLDER


# Output word list statistics - the first number is close to what
# is expected in the report for the import into MySQL (a
# difference of 3).
echo
echo
echo Word statistics:
echo
grep INSERT $SQL_FILE | wc


########################################################################
#
# Fish out any error messages (from checking of the integrity of the
# word list data) out of the generated SQL or HTML (the two types
# of output are currently mixed up...)
#
# We do it by successively excluding "good lines".
#
echo
grep -v DROP $SQL_FILE | grep -v pmortensen_eu_db | grep -v CREATE | grep -v VARCHAR | grep -v '^\#' | grep -v 'URL)'  | grep -v '^$' | grep -v INSERT | grep -v '<tr>' | grep -v ' <' | grep -v '/>' | grep -v 'nbsp' | grep -v ';' | grep -v '2020-'
echo

# Expected to be back to the source folder,
# e.g. "~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine"
cd -


# Usually enough to spot if it succeeded - the size of the generated files
echo
echo
echo

ls -lsatr $HTML_FILE
ls -lsatr $SQL_FILE
echo
echo


# Not really a build step, but this makes it easier to spot
# if the build failed or not (as we would not be here if
# any of the previous build steps failed).
#
startOfBuildStep "39" "End of build. All build steps succeeded!!"

timeStamp "End time"



