########################################################################
#                                                                      #
# Purpose: Compilation and running of part of Edit Overflow            #
#          on Linux. It produces SQL and HTML exports of               #
#          the Edit Overflow wordlist.                                 #
#                                                                      #
#          The running part is of C# code (.NET). The testing          #
#          is both for C# and JavaScript. And system regression        #
#          tests (currently only for the web interface).               #
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
#          integration tests (Selenium).                               #
#                                                                      #
# Driver script for copying only the necessary files to a              #
# work folder, compile and run part of the .NET code for               #
# Edit Overflow.                                                       #
#                                                                      #
#                                                                      #
# Historical note:                                                     #
#                                                                      #
#     This script started as a way to automate compiling the C#        #
#     Edit Overflow source code on Linux (.NET Core) (as we had        #
#     to copy the necessary source files to a (single) build           #
#     folder in order to build). And to automate callling the          #
#     resulting executable to generate the SQL for the word            #
#     input file (for import into web hosting based on MySQL           #
#     and PHP). That was at the end of 2019 (September), but           #
#     the first public commit was not before 2020-02-06.               #
#                                                                      #
#     But it has developed into a full-blown build script to,          #
#     e.g., run unit tests and automatically deploy to a web           #
#     hosting site.                                                    #
#                                                                      #
#                                                                      #                                                                  #
# Installation notes:                                                  #
#                                                                      #
#   To enable compilation and running .NET Core code on                #
#   Ubuntu/Debian, these installation steps work (last                 #
#   tested on 2022-03-02):                                             #
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
# Run notes:                                                           #
#                                                                      #
#   Even the local part of the tests currently requires a working      #
#   Internet connection, as Jest (for JavaScript unit testing)         #
#   for unknown reasons ***sometimes*** requires it, even just         #
#   executing "jest --version" from the command line (see also         #
#   near "not always" below).                                          #
#                                                                      #
########################################################################

# Future:
#
#   1. Check for a working Internet connection in the beginning (like
#      the Python/Selenium self-test) - we are currently dependent
#      on it (unexplained why this is necessary for even the
#      webserver on 'localhost')
#
#   2. Retry some (or all) of the Selenium tests if there is a failure
#      that is due to timing issues.
#
#      Perhaps selectively for the one that fails, so we don't
#      slow testing speed if there is a genuine error.
#
#   3. Tie in the input checking script (ToToTryExtract.pl) - so
#      correct operation of it (e.g., iterative running of it
#      until its output is empty) is checked.
#
#   4. Add a check to see if the PHP files on hosting are identical
#      to the current PHP files (for informational purposes only -
#      but guiding any manual deployment steps (to production)).
#


# Markers for navigation:
#
#    FFFFFFFFFF   End of helper functions


#############################################################################
#
#  TOC:
#
#     1. Copying files to the build folder, etc.
#     2. Checking prerequisites
#     3. Internal check of the web interface regression tests
#
#     4. Source code spellcheck
#
#     5. Keyboard shortcut consistency check: Edit Overflow lookup
#     6. Keyboard shortcut consistency check: text stuff
#     7. Keyboard shortcut consistency check: fixed string
#     8. Keyboard shortcut consistency check: edit summary
#     9. Keyboard shortcut consistency check: canned comments
#    10. Keyboard shortcut consistency check: link builder
#
#    11. PHP code test: main lookup - EditOverflow.php
#    12. PHP code test: self test, unit tests - Text.php
#    13. PHP code test: text transformation - 'Real quotes' - Text.php
#    14. PHP code test: fixed strings - FixedStrings.php
#    15. PHP code test: edit summary fragments - EditSummaryFragments.php
#    16. PHP code test: canned comments - CannedComments.php
#    17. PHP code test: link builder - Link_Builder.php
#
#    18. Web server test: localWebserver_Edit_Overflow_lookup
#    19. Web server test: localWebserver_Edit_Overflow_lookup
#    20. Web server test: localWebserver_Edit_Overflow_lookup
#    21. Web server test: localWebserver_Text
#    22. Web server test: localWebserver_fixed_strings
#    23. Web server test: localWebserver_Edit_summary_fragments
#    24. Web server test: localWebserver_canned_comments
#    25. Web server test: localWebserver_Text_RemoveTABsAndTrailingWhitespace
#    26. Web server test: localWebserver_Text_RemoveCommonLeadingSpace
#    27. Web server test: localWebserver_Text_RealQuotes
#    28. Web server test: localWebserver_LinkBuilder_1
#
#    29. Start running C# unit tests
#    30. C# compilation and sanity check
#    31. Exporting the word list as SQL
#    32. Opening some web pages and applications for manual operations
#    33. Exporting the word list as HTML
#    34. Starting checking generated HTML
#    35. Exporting the word list as JavaScript
#    36. Start running JavaScript unit tests
#    37. Updating the JavaScript word list file on pmortenen.eu (<https://pmortensen.eu/world/EditOverflowList.js>)
#
#    38. Updating the HTML word list file on pmortenen.eu (<https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>)
#    39. HTML validation: Edit Overflow lookup
#    40. HTML validation: Text stuff
#    41. HTML validation: Fixed strings
#    42. HTML validation: Edit summary fragments
#    43. HTML validation: Canned comments
#    44. HTML validation: Link builder
#    45. HTML validation: Word list (HTML)
#
#    45. Starting checks of the generated HTML
#
#    47. Starting web interface regression tests. Both for the local web server and production.
#
#    48. End of build. All build steps succeeded!!
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
#     But some build steps are refactored into helper functions (instead
#     of using helper function "startOfBuildStep" directly). Some
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
#     Newer version that allows space indentation for calls to
#     helper function "HTML_validation" (as we happen to use
#     it in an 'if' statement's main clause):
#
#       clear ; cat ~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine/Linux.sh | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^\s*HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. PHP code test: %s - %s\n", $3, $2, $1 if /^PHP_code_test\s+(\S+)\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Web server test: %s\n", $3, $2 if /^webServer_test\s+\"(.+)\"\s+\"(.+)\"\s+(\d+)/;     '
#
#     Newer version that includes use of sourceSpellcheck():
#
#       clear ; cat ~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine/Linux.sh | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^\s*HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. PHP code test: %s - %s\n", $3, $2, $1 if /^PHP_code_test\s+(\S+)\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Web server test: %s\n", $3, $2 if /^webServer_test\s+\"(.+)\"\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Source code spellcheck\n", $1 if /^sourceSpellcheck\s+(.+)/;     '
#       clear ; cat ~/UserProf/At_PC2016/Edit_Overflow/Dot_NET_commandLine/Linux.sh | perl -nle 'printf "#    %2d. %s\n", $1, $2 if /^startOfBuildStep\s+\"(\d+)\"\s+\"(.+)\"/; printf "#    %2d. Keyboard shortcut consistency check: %s\n", $2, $1 if /^keyboardShortcutConsistencyCheck.+\"(.+)\"\s+(\d+)/; printf "#    %2d. HTML validation: %s\n", $2, $1 if /^\s*HTML_validation.+\"(.+)\"\s+(\d+)/; printf "#    %2d. PHP code test: %s - %s\n", $3, $2, $1 if /^PHP_code_test\s+(\S+)\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Web server test: %s\n", $3, $2 if /^webServer_test\s+\"(.+)\"\s+\"(.+)\"\s+(\d+)/; printf "#    %2d. Source code spellcheck\n", $1 if /^sourceSpellcheck\s+(.+)/;     '
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
# perl -nle 'if (/^\d+\./) { $prevBuildLine = $buildLine; $buildLine = $_; }  if (/^At: /) { $prevHours = $hours; $prevMinutes = $minutes; $prevSeconds = $seconds; if (/T(\d+):(\d+):(\d+)_(\d+)/) { $hours = $1; $minutes = $2; $seconds = $3 + $4 / 1000000000.0} $runTimeSeconds = 3600 * ($hours - $prevHours) + 60 * ($minutes - $prevMinutes) + ($seconds - $prevSeconds); print "Run time [secs]\t$runTimeSeconds\t$prevBuildLine" if $prevHours; }   '   '/home/embo/temp2/2021-11-05/Transcript of Edit Overflow build, 2021-11-05T171256.txt' > _BuildStepsTimings.txt
#

echo
echo
echo 'Start of building Edit Overflow...'
echo


# ####################
# Configuration, start


# Selectively disable some tests (e.g., if an external
# service we depend on is not available)
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
export EFFECTIVE_DATE='2022-03-01'
export EFFECTIVE_DATE='2023-07-11'
export EFFECTIVE_DATE='2024-12-07'


# Moved to PC2016 2022-02-28.
## Moved to Ubuntu 20.04 MATE 2020-06-09
### Moved to Ubuntu 20.04 MATE 2020-06-03
#### Moved to PC2016 after SSD-related Linux system crash 2020-05-31
####export SRCFOLDER_BASE='/home/mortense2/temp2/Edit_Overflow'
####export WORKFOLDER1=/home/mortense2/temp2/${EFFECTIVE_DATE}
###export SRCFOLDER_BASE='/home/mortensen/temp2/Edit_Overflow'
###export WORKFOLDER1=/home/mortensen/temp2/${EFFECTIVE_DATE}
##export SRCFOLDER_BASE='/home/embo/temp2/2020-06-03/temp1/Edit_Overflow'
##export WORKFOLDER1=/home/embo/temp2/${EFFECTIVE_DATE}
#export WORKFOLDER1=/home/embo/temp2/${EFFECTIVE_DATE}
#export WORKFOLDER1=/home/mortensen/temp2/${EFFECTIVE_DATE}
export WORKFOLDER1=${HOME}/temp2/${EFFECTIVE_DATE}

export WORKFOLDER2=${WORKFOLDER1}/_DotNET_tryout
export WORKFOLDER3=${WORKFOLDER2}/EditOverflow4
export WORKFOLDER=${WORKFOLDER3}



#export SRCFOLDER_BASE='/home/embo/UserProf/At_XP64/Edit_Overflow'
#export SRCFOLDER_BASE='/home/mortensen/UserProf/At_PC2016/Edit_Overflow'
export SRCFOLDER_BASE="$HOME/UserProf/At_PC2016/Edit_Overflow"

export WEB_SRCFOLDER_BASE="${SRCFOLDER_BASE}/Web_Application"


# The exact string from PHP apparently depends on the PHP version...
export PHP_WARNING="Undefined variable: dummy2"   # Ubuntu 20.04
#export PHP_WARNING="Undefined variable \$dummy2" # LDME 6


# Presumes particular versions of .NET installed
#
export DOT_NET_EDIT_OVERFLOW_PARTIAL_PATH="bin/Debug/netcoreapp3.1/linux-x64/EditOverflow3"                   # Ubuntu 20.04
export supposedNativeApplicationPath="${WORKFOLDER}/bin/Debug/netcoreapp3.1/linux-x64/publish/EditOverflow3"  # Ubuntu 20.04

#export DOT_NET_EDIT_OVERFLOW_PARTIAL_PATH="bin/Release/netcoreapp9.0/linux-x64/publish/EditOverflow3"   # LDME 6
#export supposedNativeApplicationPath="${WORKFOLDER}/bin/Release/netcoreapp9.0/linux-x64/EditOverflow3"  # LDME 6


export PYLINT_DISABLE_WARNINGS_SET="--disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125 --disable=R0913 --disable=C0302"                 # Ubuntu 20.04
export PYLINT_DISABLE_WARNINGS_SET="--disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125 --disable=R0913 --disable=C0302 --disable=R0914" # LDME 6


#export SELINUM_DRIVERFOLDER="/home/embo/.wdm/drivers/geckodriver/linux64/v0.28.0"   # XP64_NEW
export SELINUM_DRIVERFOLDER="$HOME/.wdm/drivers/geckodriver/linux64/v0.30.0"         # Ubuntu 20.04
#export SELINUM_DRIVERFOLDER="$HOME/temp2/2025-05-07"                                # LDME 6


export PATH=$PATH:${SELINUM_DRIVERFOLDER}

export SELINUM_DRIVERSCRIPT_DIR="${WEB_SRCFOLDER_BASE}/__regressTest__"
export SELINUM_DRIVERSCRIPT_FILENAME="${SELINUM_DRIVERSCRIPT_DIR}/web_regress.py"


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

# Yes, that is the actual file name that has (currently) been configured
# at the web hosting (through file <public_html/world/.htaccess>)...
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


# Change/reset state that may prevent us from exporting (this
# can happen if variable LOOKUP is set manually in the
# command line window where this script is run from...)
unset LOOKUP



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
#    $2   Result code (e.g., 0 for no error)
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
# Future:
#
#   * In case of an error, consider listing some files
#     in the build folder, e.g., the youngest few as
#     those often contain information related to
#     the error.
#
function evaluateBuildResult()
{
   #echo "Dollar 1: $1..."
   #echo "Dollar 2: $2..."
   #echo "Dollar 3: $3..."
   #echo

   # Sanity check of parameters (for example,
   # too few parameters)
   INTERNAL_ERR_PREFIX="Internal error in build script (error"
   INTERNAL_ERR_POSTFIX_3="missing or invalid third parameter to evaluateBuildResult() (the build step identification)). The build step number is $1 (may not be the real build step number, especially if it is 0...).\n\n"
   case $3 in
     0)  printf "${INTERNAL_ERR_PREFIX} 1: ${INTERNAL_ERR_POSTFIX_3}" >&2 ; echo ; exit ;;
     "") printf "${INTERNAL_ERR_PREFIX} 2: ${INTERNAL_ERR_POSTFIX_3}" >&2 ; echo ; exit ;;
     *)  # echo ; printf "The build number is OK..." ;;
   esac

   # Yes, an 'if' statement may be better.
   #
   # What about values of undefined / empty strings? Is
   # it covered by case "0" or not?
   #
   # Build step number
   INTERNAL_ERR_POSTFIX_1="missing or invalid first parameter to evaluateBuildResult() (build step number)).\n\n"
   case $1 in
     0)  printf "${INTERNAL_ERR_PREFIX} 3: ${INTERNAL_ERR_POSTFIX_1}" >&2 ; echo ; exit ;;
     "") printf "${INTERNAL_ERR_PREFIX} 4: ${INTERNAL_ERR_POSTFIX_1}" >&2 ; echo ; exit ;;
     *)  # echo ; printf "The build number is OK..." ;;
   esac

   # Note: We need to use 'printf' below for embedded newlines,
   #       though there are also other ways. See e.g.:
   #
   #         <https://stackoverflow.com/questions/8467424/>
   #
   export EXTRAINFO="\n\nTo list the contents of the build \nfolder in chronological order:\n\n    ls -lsatr '${WORKFOLDER}' \n\n\n\n"

   # EXITCODE=${$2:-}

   # The actual check
   #
   # Result code (e.g., 0 for no error)
   case $2 in
     0|7) echo ; echo "Build step $1 (or part of it) succeeded"                    >&2               ;;
     # *) echo ; echo "${EXTRAINFO}Build step $1 ($3) failed (error code $2)." >&2 ; echo ; exit ;;
     "")  echo ; printf "Internal error in build script (error 5: missing or invalid second (result code)).\n\n" >&2 ; echo ; exit ;;
     *)   echo ; printf "${EXTRAINFO}Build step $1 ($3) failed (error code $2).\n\n" >&2 ; echo ; exit ;;
   esac
} #evaluateBuildResult()


# ###########################################################################
#
# Purpose: Relieve the clients of boilerplate related to comparing
#          two strings (and actions if they are not equal).
#          Typically, one of the strings is the result
#          from some build action.
#
#          Check if two strings are equal. It stops the entire
#          build and outputs a message (to standard output)
#          if they are not.
#
# A helper function to reduce redundancy.
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
#   0 if the two strings are the same
#
function mustBeEqual()
{
    #if [ $1 == $2 ] ; evaluateBuildResult $3  $? "Two strings not equal: $1 and $2 ($4)"
    [[ $1 == $2 ]] ; evaluateBuildResult $3  $? "two strings not equal: \"$1\" and \"$2\" ($4)"
} #mustBeEqual()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Parameters:
#
#   $1   File name (or full path) of an executable that
#        must be able to execute in the current context
#        (e.g., is in the path)
#
#        Example (often not installed by default
#                 on a given system):
#
#            "pylint"
#
#   $2   Identification string and mitigation steps (for
#        use in the error message)
#
#   $3   Build number
#
# Future:
#
#   1. Also test if it is in the path by
#      switching to an empty directory
#      and try executing there
#
#   2.
#
function checkCommand()
{
    echo
    echo "Checking the prerequisite with command line '$1'"
    #echo   evaluateBuildResult() output an empty line first.

    #We ought to check if the second or all parameters
    #are missing (though the way it is used here,
    #evaluateBuildResult() will catch it if the
    #second parameter is missing - but only
    #if the XXX).

    #echo "Dollar 1: $1..."
    #echo "Dollar 2: $2..."
    #echo "Dollar 3: $3..."
    #echo

    export STDERR_FILE="_stdErr_command.txt"

    # Examples:
    #
    #   "pylint" may result in (to standard error):
    #
    #       "./Linux.sh: line 651: pylint: command not found"
    #
    #   "pip3" may result in (to standard error):
    #
    #       "Command 'pip3' not found, but can be installed with:"
    #
    # Note that we only get one line here in
    # the script whereas manually we get more
    # lines ***and*** a different message (at
    # least on Ubuntu 18.04). For the second
    # example, it is:
    #
    #     Command 'pip3' not found, but can be installed with:
    #
    #     sudo apt install python3-pip
    #
    # Note: RESULT only contains the output
    #       to standard output, whether we
    #       redirect standard error or not.
    #export RESULT=$(eval "$1" 2>${STDERR_FILE} )

    #echo "The result is: ${RESULT}"
    #echo
    #echo "Captured standard error: `cat ${STDERR_FILE}`"
    #echo

    ##echo ${STDERR_FILE} | grep -v -q "command not found" ; evaluateBuildResult $2 $? "Pylint is not installed. Execute these commands: ABC\nXYZ\nEFG "

    #Note: 'grep' returns an exit code different
    #      from 0 for empty input, so even
    #      though that empty input does
    #      not contain "command not found"
    #      we would still report an error...
    #
    #      Instead we demand nothing to
    #      standard error whatsoever.
    #
    #cat ${STDERR_FILE} | grep -v -q "command not found" ; evaluateBuildResult $3 $? "$2"

    # Much easier: use the exit code. It works
    # both for commands and something like
    # 'pip show' that doesn't output to
    # standard error.
    #
    # Danger! Danger! It also tries to execute
    # some output from a command... Example:
    # "Usage:" in the output from 'pip3'
    #
    #$(eval "$1" 2> ${STDERR_FILE} > /dev/null ) ; evaluateBuildResult $3 $? "$2"
    #$(eval "$1" 2> ${STDERR_FILE} > /dev/null )
    $(eval "$1 2> ${STDERR_FILE} > /dev/null") ; evaluateBuildResult $3 $? "$2"

    # Check for output to standard error as well. It must be empty.
    #[[ -s ${STDERR_FILE} ]] ; test $? -ne 0 ; evaluateBuildResult $3 $? "$2"

    #echo "End of checkCommand()"
    #echo

} #checkCommand()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Parameters:
#
#   $1   Folder name to create (that may already exist)
#
#   $2   Build number. For easier identification.
#
function createFolder()
{
    # We could maybe parse the output to get
    # a specific error, but for now we just
    # check if the folder actually exists
    # after this.
    #
    mkdir -p $1

    [[ -d "$1" ]] ; evaluateBuildResult $2  $? "Could not create folder in build step $2 (could be due to misconfiguration of the base folder): $1 "

} #createFolder()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Run a Perl script (currently "/Dot_NET_commandLine/KeyboardShortcutConsistency.pl")
# on the indicated PHP source code and check:
#
#   1. Consistency between the shortcut key and
#      the corresponding help text for it.
#
#   2. Consistency of name, ID, etc. on a number
#      of forms elements.
#
#   3. Some rules regarding indentation.
#
# Parameters:
#
#   $1   File name (a PHP file)
#
#   $2   Identification string (for use in the error message)
#
#   $3   Build number
#
# Globals:
#
#   WEB_SRCFOLDER_BASE   (so we check directly at the source location.
#                         Perhaps use the build folder instead?)
#
function keyboardShortcutConsistencyCheck()
{
    startOfBuildStep $3 "Starting keyboard shortcut consistency check for $1"

    #perl -w ${SRCFOLDER_DOTNETCOMMANDLINE}/${WEBFORM_CHECK_FILENAME}  ${WEB_SRCFOLDER_BASE}/$1 ; evaluateBuildResult $3  $? "Keyboard shortcut consistency for the $2 page (file $1)"
    ${WEBFORM_CHECK_CMD}  ${WEB_SRCFOLDER_BASE}/$1 ; evaluateBuildResult $3  $? "Keyboard shortcut consistency (or similar) for the $2 page (file $1)"
} #keyboardShortcutConsistencyCheck()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Validation of the web pages in production using the W3C HTML validator.
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
# Future:
#
#   * Check the sizes of (the captured) standard output and standard
#     error (before bailing out with evaluateBuildResult).
#
#     For example, to report whether an error is due to the HTML
#     validation itself failing (often the size of standard
#     output will then will 0 bytes) or an actual problem
#     with our HTML.
#
#     For the former case, we could optionally retry a few
#     times (with an exponentially increasing delay
#     between retries).
#
#     And/or retry with invoking 'wget' without the "-q" option
#     to get more information. Or is retrying automatic with
#     'wget'?
#
#     (The order of the timestamps for standard error and standard
#      output may or may not be important.)
#
function HTML_validation_base()
{
    startOfBuildStep $3 "Starting HTML validation for $1"

    # Alternative (the same): <https://html5.validator.nu/>
    #
    export SUBMIT_URL="https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu$4%2F$1%3FOverflowStyle=Native"

    # Output the submit URL (to the W3 validator), so we can easier and faster reproduce a problem
    echo "Submit URL for HTML validation: ${SUBMIT_URL}"
    echo

    # _last_HTML_validation_response_stdout.txt
    # _last_HTML_validation_response_stdout.txt
    export HTML_VALIDATION_ID="HTML_validationResponse_$3_$1"
    export THEPOSTFIX="_${HTML_VALIDATION_ID}.html"
    export STDOUT_FILE="_stdOut${THEPOSTFIX}"
    export STDERR_FILE="_stdErr${THEPOSTFIX}"

    # In two parts so we have all the evidence if something goes
    # wrong (it sometimes does go wrong (every few months))
    #
    #wget -q -O- ${SUBMIT_URL} | grep -q 'The document validates according to the specified schema'      ; evaluateBuildResult $3 $? "W3 validation for $2 (file $1)"
    wget -q -O- ${SUBMIT_URL} >  ${STDOUT_FILE}  2>  ${STDERR_FILE}
    cat ${STDOUT_FILE} | grep -q 'The document validates according to the specified schema'      ; evaluateBuildResult $3 $? "W3 validation for $2 (file $1)"

} #HTML_validation_base()


# ###########################################################################
#
# Helper function to reduce redundancy
#
# Parameters:
#
#   $1   File name (.php)
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
# Helper function to reduce redundancy. For PHP code. It
# presumes the PHP interpreter (executable 'php') can be
# run from the command line (a separate installation step).
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
#        Notes:
#
#          1. Only a partial match is necessary. Thus the caller
#             does not have to specify the whole exact string.
#
#          2. Specifying an empty string currently does not work. That is,
#             it can not be specified that standard error should be empty
#             (it will fail).
#
#             Thus the script MUST produce something to standard error...
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
    # 1. Sanity check of parameters to this function
    #
    # Later: Also for build number (small integer)
    #
    # The fourth parameter should be in URL query format
    echo
    echo "Sub step 1 (function parameter sanity checking):"
    echo $4 | grep -q "=" ; evaluateBuildResult $3 $? "PHP test (step 1), internal test: The fourth parameter to function PHP_code_test() does not appear to be in URL query format: $4 "


    # #####################################
    #
    # 2. Execute the PHP script (and capture both
    #    standard output and standard error)
    #
    # Notes:
    #
    #   $3 (build number) is to make it unique (so we don't
    #   overwrite previous output files from the same build
    #   script run (for the current build script run)).
    #
    #   $1 is for information only.
    #
    #   We don't actually check anything here. But
    #   perhaps we should - if PHP is not installed
    #   (locally) it appears as if it passed. Though
    #   the prerequisites test in the beginning
    #   of the script should not allow us to
    #   get here.
    #
    echo
    echo "Sub step 2 (PHP script main run):"
    export PHPRUN_ID="PHP_code_test_$3_$1"
    export STDERR_FILE="_stdErr_${PHPRUN_ID}.txt"

    # The PHP script outputs HTML to standard output
    export HTML_FILE_PHP="_HTML_${PHPRUN_ID}.html"

    #php $1 "$4" > /home/embo/temp2/2021-09-14/_Text.html 2> ${STDERR_FILE}
    #php $1 "$4"
    php $1 "$4" > ${HTML_FILE_PHP}  2> ${STDERR_FILE}

    #cat ${STDERR_FILE}
    #echo "Standard error:" `cat ${STDERR_FILE}`


    # #####################################
    #
    # 3. Detect missing files (as seen by the PHP interpreter)
    #
    echo
    echo "Sub step 3:"
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
    echo
    echo "Sub step 4:"
    grep -q "$5" ${STDERR_FILE} ; evaluateBuildResult $3 $? "PHP test (step 14: $2 (file $1). Extra information: expected \"$5\", but got \"`head -n 3 ${STDERR_FILE}`\"  "


    # #####################################
    #
    # 5. Detect some stray debug output in our PHP code
    #    (to standard output). Common for all.
    #
    # "test $? -ne 0" is for inverting/negating the return code ("$?").
    #
    echo
    echo "Sub step 5:"
    export STRAYDEBUGGING_MATCHSTRING1="First argument: "
    #! (grep -q "First argument: " ${HTML_FILE_PHP}) ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: `head -n 3 ${HTML_FILE_PHP}`"
    #grep -q "First argument: " ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: `head -n 3 ${HTML_FILE_PHP}`"
    #grep -q "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP} ; evaluateBuildResult $3 $? "PHP test: $2 (file $1). Extra information: `grep "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP}`"
    grep -q "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 5a): $2 (file $1). Extra information: \"`grep "${STRAYDEBUGGING_MATCHSTRING1}" ${HTML_FILE_PHP}`\"  "

    export STRAYDEBUGGING_MATCHSTRING2="Some URL: "
    grep -q "${STRAYDEBUGGING_MATCHSTRING2}" ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 5b): $2 (file $1). Extra information: \"`grep "${STRAYDEBUGGING_MATCHSTRING2}" ${HTML_FILE_PHP}`\"  "


    # #####################################
    #
    # 6. Detect unit test failures. Note: Currently only
    #    executed in the context of Text.php
    #
    # Is there an easier way? Could we configure the PHP installation to
    # automatically stop on all errors and warnings?
    #
    echo
    echo "Sub step 6:"
    export UNITTEST_MATCHSTRING="Failed test. ID: "
    grep -q "${UNITTEST_MATCHSTRING}" ${HTML_FILE_PHP} ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test (step 6): $2 (file $1). Extra information: \"`grep "${UNITTEST_MATCHSTRING}" ${HTML_FILE_PHP}`\"  "


    # #####################################
    #
    # 7. Detect more errors that are not syntax errors (they
    #    will not happen until run time, but they still
    #    terminate the execution).
    #
    #    Stop on all fatal errors except the database
    #    error that we currently expect (locally).
    #
    # Example:
    #
    #   PHP Fatal error:  Uncaught ArgumentCountError: Too few arguments to function WikiMedia_Link(), 1 passed in /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/Text.php on line 347 and exactly 2 expected in /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/commonStart.php:486
    #
    echo
    echo "Sub step 7:"
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
    echo
    echo "Sub step 8:"
    export NOTICE_MATCHSTRING="PHP Notice: "
    grep -v dummy2 ${STDERR_FILE} | grep -q "${NOTICE_MATCHSTRING}" ; test $? -ne 0 ; evaluateBuildResult $3 $? "PHP test  (step 8): $2 (file $1). Extra information: \"`  grep -v dummy2 ${STDERR_FILE} | grep "${NOTICE_MATCHSTRING}"  `\""

} #PHP_code_test()


# ###########################################################################
#
# Helper function to reduce redundancy. Checks for the presense
# of a given string on one of the web pages, both in the source
# and in production.
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
#   $4   Forbidden string
#
#
function forbidden_content()
{
    startOfBuildStep $3 "Start checking for verbidden content in $1: $2"

    #Later
    # #####################################
    #
    # 1. Sanity check of parameters to this function


    # #####################################
    #
    # 2. Check directly in the source file
    #
    #export UNITTEST_MATCHSTRING="Failed test. ID: "
    grep -q "${4}" ${1} ; test $? -ne 0 ; evaluateBuildResult $3 $? "Verbidden test: $2 (signature $4 was found in file $1). "


    # Later: Check on the local web server (but that normally
    #        passes as this build script properly copies the
    #        file there).


    #Not now. We get error "406 Not Acceptable" from wget:
    #
    #    About download from https://pmortensen.eu/world/EditSummaryFragments.php?OverflowStyle=Native
    #    --2023-06-20 20:23:10--  https://pmortensen.eu/world/EditSummaryFragments.php?OverflowStyle=Native
    #    Resolving pmortensen.eu (pmortensen.eu)... 94.231.108.241
    #    Connecting to pmortensen.eu (pmortensen.eu)|94.231.108.241|:443... connected.
    #    HTTP request sent, awaiting response... 406 Not Acceptable
    #    2023-06-20 20:23:10 ERROR 406: Not Acceptable.
    ##
    ## #####################################
    ##
    ##  2. Check in production
    ##
    #export BASE_URL="https://pmortensen.eu/world"
    #export EXTRA_PARAMETERS="?OverflowStyle=Native"
    #export SUBMIT_URL="${BASE_URL}/${1}${EXTRA_PARAMETERS}"
    #echo "About download from ${SUBMIT_URL}"
    #
    ##Some redundancy here (only the wget part is different)
    ##wget -q -O- ${SUBMIT_URL} | grep -q "${4}" ${1} ; test $? -ne 0 ; evaluateBuildResult $3 $? "Verbidden test: $2 ($4 was found on ${SUBMIT_URL}). "
    #wget -O- ${SUBMIT_URL}

} #forbidden_content()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Used to detect changes to files (e.g., a web
# server error log). Used with endWatchFile().
#
# Parameters:
#
#   $1   File to watch for changes
#
function startWatchFile()
{
    # File size for now. Perhaps MD5 hash later

    # Yes, a global variable...
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
#   For now, the primary purpose is to detect ***new entries*** in the
#   ***web server error log*** (most likely only due to PHP errors)
#   as a result of retrieving a web page (with wget).
#
#   This is to detect errors when the PHP scripts are running in
#   a web server context, as opposed to a command-line context
#   that we also use for testing. For instance, a PHP file
#   gets passed command-line parameters from our tests
#   when running in command line context, whereas in
#   a web context it does not get any command line
#   parameters (we had an actual error due to this).
#
#   PHP may also be configured differently for command line
#   and web (e.g., two different XXX)
#
# Future:
#
#   1. Positive tests to test if particular configuration
#      changes actually have the expected effect.
#
#      Will then become regression tests for the expected
#      configuration.
#
# Note: For now, it only works for the ***local web server***
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
    startOfBuildStep $3 "Start detection of error log entries for a web server. ID: $2"

    export RETRIEVED_HTML_FILE="_$2.html"

    startWatchFile ${LOCAL_WEB_ERRORLOG_FILE}

    # That is, invoke the specified web page with PHP on the
    # web server. If this results in entries to the error
    # log, we will detect it below.
    #
    #wget -o ${RETRIEVED_HTML_FILE} "$1"
    wget -q -O ${RETRIEVED_HTML_FILE} "$1"

    #echo "Web server error log size before: ${SIZE_BEFORE_WEBERROR_LOG}"
    #echo "Web server error log size after: ${SIZE_AFTER_WEBERROR_LOG}"

    #echo ; date ; echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 5


    # Detect new entries in the web server error log
    # file as a result of retrieving a web page.
    #
    #[ "${SIZE_BEFORE_WEBERROR_LOG}" != "${SIZE_AFTER_WEBERROR_LOG}" ] ; test $? -ne 0 ; evaluateBuildResult $3 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "
    #[ "${SIZE_BEFORE_WEBERROR_LOG}" = "${SIZE_AFTER_WEBERROR_LOG}" ] ; evaluateBuildResult $3 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "
    endWatchFile ${LOCAL_WEB_ERRORLOG_FILE} ; evaluateBuildResult $3 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "

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
#
# Helper function to reduce redundancy, e.g. for wordListExport().
#
# Parameters:
#
#    $1   The name of the export file to check for
#         a file size range. E.g., the full path
#
#    $2   Minimum size. Is inclusive.
#
#    $3   Maximum size. Is inclusive.
#
#    $4   Build step number
#
#    $5   String identifying the build step
#
function checkFileSize()
{
    #echo "Minimum size: ${2}"

    # Quote with single quotes for spaces in the file name
    export COMMANDLINE="cat '${1}' | wc -c"
    export FILE_SIZE7=$(eval "$COMMANDLINE")

    #echo "File size: ${FILE_SIZE7}"

    #[ ${FILE_SIZE7} -gt ${2} ] && [ ${FILE_SIZE7} -lt ${3} ]  ; evaluateBuildResult $4 $? "File size: ${FILE_SIZE7}. Expected filesize range is ${2} - ${3} bytes (for build step $4, creating an export file in ${5} format)"
    #[ ${FILE_SIZE7} -ge ${2} ] && [ ${FILE_SIZE7} -le ${3} ]  ; evaluateBuildResult $4 $? "File size: ${FILE_SIZE7}. Expected filesize range (both inclusive) is ${2} - ${3} bytes (for build step $4, creating an export file in ${5} format)"
    [ ${FILE_SIZE7} -ge ${2} ] && [ ${FILE_SIZE7} -le ${3} ]  ; evaluateBuildResult $4 $? "File size: ${FILE_SIZE7}. Expected filesize range (both inclusive) is ${2} - ${3} bytes (for build step $4 - ${5})"

} #checkFileSize()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Export the Edit Overflow wordlist in the indicated format. And check
# for various errors that may have happended during the export.
#
# Parameter:
#
#    $1   Build step number
#
#    $2   Export type. A string. The Same as what the command-line
#                      version of Edit Overflow requires as input.
#                      E.g. "SQL" for, well, SQL.
#
#    $3   Export filename. Usually a full path name.
#
#    $4   Minimum size. For a sanity check of export file size
#
#    $5   Maximum size. For a sanity check of export file size
#
function wordListExport()
{
    #echo
    #echo
    #echo "Start of wordListExport()... Build step $1. For export in $2 format."
    #echo

    # Compile and run in one step. We could also
    # run it like this after compilation (off
    # the build folder):
    #
    #     bin/Debug/netcoreapp3.1/EditOverflow3
    #
    # If the compilation fails, the return code is "1".
    # It is masked by the grep's, but we are saved by
    # the previous build step (a compilation error
    # automatically terminates the (C#) unit
    # tests).

    # The environment variable the Edit Overflow
    # command-line program expects (as an input).
    #
    export WORDLIST_OUTPUTTYPE=$2

    # Alias here
    export WORD_EXPORT_FILE=$3

    # For now, we use the build step number to make the file
    # name for the capture standard error output unique.
    export STDERR_FILE="_stdErr_Export_${1}_${WORDLIST_OUTPUTTYPE}.txt"

    # Only used as a temporary file in order to capture an exit
    # code from the command-line version of Edit Overflow
    #
    export STDOUT_FILE="_stdOutput_Export.txt"

    #pwd
    #echo "Standard error file: ${STDERR_FILE}"
    echo
    echo "To output standard error file: cat `pwd`/${STDERR_FILE}"

    # For debugging exit code:
    #
    #   Insert (we must redirect to a file because
    #   there may be way too much output to
    #   standard output...):
    #
    #      ; echo "Exit code: ${?}" > __Exit_Code.txt
    #

    # Note: There ***must*** be ***some*** output to standard
    #       output (due to the two 'grep's), otherwise
    #       evaluateBuildResult() will stop the entire
    #       script/build.
    #
    #       This is usually not a problem when used for the
    #       primary purpose of exporting the word list, but
    #       can be when just invoking it for the purpose
    #       of compilation/sanity checking.
    #
    # Note: Using later .NET Core versions, for "-p", we got:
    #
    #         Warning NETSDK1174: The abbreviation of -p for --project
    #         is deprecated. Please use --project.
    #
    #time dotnet run -p EditOverflow3.csproj 2> ${STDERR_FILE}  | grep -v CS0219 | grep -v CS0162   >> $WORD_EXPORT_FILE  ; evaluateBuildResult $1 $? "word list export in $WORDLIST_OUTPUTTYPE format"
    #time dotnet run -p EditOverflow3.csproj 2> ${STDERR_FILE} > ${STDOUT_FILE} ; evaluateBuildResult $1 $? "word list export in $WORDLIST_OUTPUTTYPE format"
    time dotnet run --project EditOverflow3.csproj 2> ${STDERR_FILE} > ${STDOUT_FILE} ; evaluateBuildResult $1 $? "word list export in $WORDLIST_OUTPUTTYPE format"

    # Note: Because we immediately check for the exit code (and
    #       exit the entire build script if it is not 0), we
    #       will never get here in case of an error, e.g.
    #       to output the content of ${STDERR_FILE}...


    #But do we actually need to use evaluateBuildResult() the second time?
    #Could it in fact allow us to allow Edit Overflow to return nothing
    #to standard output?
    #
    grep -v CS0219 ${STDOUT_FILE} | grep -v CS0162 >> $WORD_EXPORT_FILE ; evaluateBuildResult $1 $? "word list export in $WORDLIST_OUTPUTTYPE format"

    echo

    # Note: The export file is accumulating, unless the client resets it...
    echo "Export statistics:"
    wc ${WORD_EXPORT_FILE}
    echo

    echo "Standard error output statistics:"
    wc ${STDERR_FILE}
    echo

    echo "Current directory: `pwd`"
    echo "Word export file: ${WORD_EXPORT_FILE}"
    #echo
    #echo "After 'dotnet run'..."
    #echo "Exit code captured: `cat __Exit_Code.txt`"

    # Echo any errors, no matter what. But note that
    # if the exit code is not 0 in the 'dotnet run'
    # step, we will never get here!!
    #
    cat ${STDERR_FILE}

    # Sanity check for export file size
    checkFileSize  ${WORD_EXPORT_FILE} $4 $5 $1 "$2 format"

    # Check for output to standard error. It must be empty.
    [[ -s ${STDERR_FILE} ]] ; test $? -ne 0 ; evaluateBuildResult $1 $? "empty standard error output (word list export for $2)"

    # Set up for the next call (as "LOOKUP" will override the main
    # function, exports). Callers must explicitly set it before
    # if they want it.
    unset LOOKUP
} #wordListExport()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Some rudimentary spell checking. Mostly designed to
# catch the most frequent ones, like "paramter". We
# don't want to make it comprehensive as it will
# probably result in false positives.
#
# Presumes the top of the source folder (e.g.,
# the build folder) is the current directory.
#
# Parameter:
#
#    $1   Build step number
#
# Future:
#
#   1. More words
#
#   2. Perhaps use the source folder, so we are not
#      misled as to where correct the mistakes (it
#      should not be done to files in the work
#      folder...)
#
function sourceSpellcheck()
{
    startOfBuildStep $1 "Some spellcheck of source code, incl. scripts."

    # We don't actually need it if we print the hits out on the screen
    #echo "Temp misspellings list file: `pwd`/${TEMP_MISSPELLINGS_LIST_FILE}"

    # Assuming the work folder, at least for now
    export EFFECTIVE_SOURCE_FOLDER=${WORKFOLDER}

    export TEMP_MISSPELLINGS_LIST_FILE="_sourceMisspellings.txt"

    export TEMP_PHP_COMMENTCHARACTERS_FILE="_sourcePHPcommentCharacters.txt"

    # For now: Inline Perl one-liner
    #
    # Notes:
    #
    #   1. Variable "$." is not reliable as a line
    #      number, so we don't write it out...
    #
    #   2. The match "[^\"]" is for excluding the lines where
    #      we quote a misspelling near in this very place.
    #
    # Word list:
    #
    #   Word           Notes
    #   -----------------------------------------------------
    #   "paramter"     We match in a case-sensitive way
    #                  so we will miss "Paramter".
    #                  We should probably change that.
    #
    #   "Markdowm"
    #
    #   "Aparrently"
    #
    #find ${EFFECTIVE_SOURCE_FOLDER} -type f | perl -nle 'print if /(\.sh$|\.cs$)/' | tr "\n" "\0" | xargs -0 grep -n 'paramter' | grep -v '            correctionAdd' >  ${TEMP_MISSPELLINGS_LIST_FILE}
    #find ${EFFECTIVE_SOURCE_FOLDER} -type f | perl -nle 'print if /(\.sh$|\.cs$)/' | tr "\n" "\0" | xargs -0 perl -nle 'if (/[^\"]paramter[^\"]/) { s/^\s+//g; print "$ARGV:$.: $_"; } ' | grep -v '            correctionAdd' >  ${TEMP_MISSPELLINGS_LIST_FILE}
    find ${EFFECTIVE_SOURCE_FOLDER} -type f | perl -nle 'print if /(\.sh$|\.cs$)/' | tr "\n" "\0" | xargs -0 perl -nle 'if (/[^\"]paramter[^\"]/) { s/^\s+//g; print "$ARGV: $_"; } ' | grep -v '            correctionAdd' >  ${TEMP_MISSPELLINGS_LIST_FILE}

    #echo ${TEMP_MISSPELLINGS_LIST_FILE}

    # Note: In the screen output, we would like present it for the original
    #       source location, not the build folder. This is to avoid the
    #       mistake of changing files in the build folder...
    #
    #       But how can we do it with variables containing the folder paths?
    #
    #         <https://stackoverflow.com/a/13210912>
    #
    cat ${TEMP_MISSPELLINGS_LIST_FILE}
    # | perl -nle 'm///g'

    # This would normally be 0 so we don't really need it
    #echo
    #wc ${TEMP_MISSPELLINGS_LIST_FILE}

    # It must be empty - zero hits.
    checkFileSize  ${TEMP_MISSPELLINGS_LIST_FILE} 0 0 $1 "source code spellcheck"


    # #######################################################################
    # Also some source code check: Consistent use of "#" as the comment
    # character in PHP code (that is, we don't accept "//". Though
    # we do accept C-style ones - /*   */ - this is a way to not
    # have to add exceptions by using "/* */" instead of "//"
    # for JavaScript code in a few places).
    #
    find ${EFFECTIVE_SOURCE_FOLDER} -type f | perl -nle 'print if /(\.php$)/' | tr "\n" "\0" | xargs -0 perl -nle 'if (/^\s+\/\//) { s/^\s+//g; print "$ARGV: $_"; } ' | grep -v 'XXXXXZZZ' >  ${TEMP_PHP_COMMENTCHARACTERS_FILE}
    cat ${TEMP_PHP_COMMENTCHARACTERS_FILE}
    checkFileSize  ${TEMP_PHP_COMMENTCHARACTERS_FILE} 0 0 $1 "PHP source comment character"

} #sourceSpellcheck()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Test of the lookup through the Edit Overflow
# command-line interface.
#
#    $1   Build step number
#
#    $2   Application name. Executable name,
#         e.g., a full path to it.
#
#    $3   Input word
#
#    $4   Expected lookup result
#
function checkCommandLineLookup()
{
    # Aliases: More meaningful parameter names
    export aBuildStepNumber=$1
    export anApplicationName=$2
    export anInputWord=$3
    export anExpectedResult=$4

    # Clear out for any previous invocations
    unset LOOKUP
    unset WORDLIST_OUTPUTTYPE

    timeStamp "CLI lookup, start"

    # Can we actually handle input with "*"???
    export LOOKUP="$anInputWord"

    # Example: </home/mortensen/temp2/2024-12-07/_DotNET_tryout/EditOverflow4/bin/Debug/netcoreapp3.1/linux-x64/publish/EditOverflow3>
    #
    export LOOKUP_RESULT=$($anApplicationName)
    timeStamp "CLI lookup, end  "

    # Example: "Corrected word for iy is: it_____" (with a leading newline)
    #
    echo "$LOOKUP_RESULT"

    # Notes:
    #
    #    1. This test depends on the content of
    #       the Edit Overflow word list.
    #
    #    2. Both strings must be quoted
    #
    #    3. For a successful lookup, there
    #       is a leading newline...
    #
    #mustBeEqual "${LOOKUP_RESULT}" $'\n'"Corrected word for iy isWWW: ${anExpectedResult}"  $aBuildStepNumber "The Edit Overflow commandline lookup result was not as expected."
    mustBeEqual "${LOOKUP_RESULT}" $'\n'"Corrected word for ${anInputWord} is: ${anExpectedResult}"  $aBuildStepNumber "The Edit Overflow commandline lookup result was not as expected."

    # "LOOKUP" overrides, so we must reset it for exports to work...
    unset LOOKUP

} #checkCommandLineLookup()


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Test of the Edit Overflow command-line interface.
#
# The primary purpose is to compare the normal
# .NET application and the .NET native
# compiled .NET application (they
# should behave the same).
#
#    $1   Build step number
#
#    $2   Application name. Executable name,
#         e.g., a full path to it.
#
#    $3   String identifying the build step
#         (for use in error messages)
#
function testCommandLineInterface()
{
    #Future: Sanity check for the modification date
    #        of the executable (it should only be
    #        a few minutes old).

    # Aliases: More meaningful parameter names
    export aBuildStepNumber=$1
    export anApplicationName=$2
    export aBuildIdentification=$3

    # Sanity check. That it actually exists and that the
    #               exit code when running it is 0.
    #
    # Though the exit code is effectively checked in the
    # "wordListExport" above (if the .NET and native
    # application is based on the same source code).
    #
    checkCommand "$anApplicationName" "Failed in running the $aBuildIdentification command line application of Edit Overflow"  $aBuildStepNumber


    # Possibly, the 'cold' runtime. Though it
    # may be dependent on a previous run of
    # the build script
    checkCommandLineLookup "$aBuildStepNumber" "$anApplicationName" "R2R" "ReadyToRun"

    # At this point, it should be the 'warm' runtime
    checkCommandLineLookup "$aBuildStepNumber" "$anApplicationName" "iy"  "it_____"

    checkCommandLineLookup "$aBuildStepNumber" "$anApplicationName" "php" "PHP"
} #testCommandLineInterface()


# Marker:  FFFFFFFFFF


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
#       separate step. In other words, this line should
#       be the LAST line if pasting several lines.
#
sudo ls > /dev/null

# LMDE trial, 2025-05-08
source ~/.Edit_Overflow_environment/bin/activate



# ###########################################################################
#
# Copy files to the workfolder (mostly where we compile, but also
# for the local web server (that we use for PHP code tests using
# the local web server)).
#
# Notes:
#
#   1. This step should be ***before*** the prerequisites
#      checks, because the prerequisite check for NUnit
#      depends on an actual project (ours) being in a
#      usable state (it isn't after the build script
#      has run past the C# main project runs, e.g.
#      exports. We use renaming of files in the
#      work/build folder so .NET only sees one
#      project at a time).
#
#      having the unit project (in the workfolder)
#      in  it (hiding the main project until unit
#      testing has done).
#
#   2. We use option "-p" to avoid error output
#      when the folder already exists (reruns),
#      like:
#
#        "mkdir: cannot create directory ‘/home/embo/temp2/2020-06-03’: File exists"
#
#      But we do actually get see errors like
#      (are they to standard output?):
#
#        "mkdir: cannot create directory ‘/home/embo’: Permission denied"
#
startOfBuildStep "1" "Copying files to the build folder, etc."

echo "Build folder: ${WORKFOLDER}"
echo

#Delete at any time
#mkdir -p $WORKFOLDER1
#mkdir -p $WORKFOLDER2
#mkdir -p $WORKFOLDER3
#mkdir -p $WORKFOLDER
#mkdir -p $FTPTRANSFER_FOLDER_HTML
#mkdir -p $FTPTRANSFER_FOLDER_JAVASCRIPT
#mkdir -p $LOCAL_WEBSERVER_FOLDER

# This will detect if the script is misconfigured...
# E.g. incorrect user folder.
createFolder $WORKFOLDER1                    1
createFolder $WORKFOLDER2                    1
createFolder $WORKFOLDER3                    1
createFolder $WORKFOLDER                     1
createFolder $FTPTRANSFER_FOLDER_HTML        1
createFolder $FTPTRANSFER_FOLDER_JAVASCRIPT  1

# This will fail if LAMP (in particular,
# Apache) has not been installed
#
# This will also fail without 'sudo'.
# Disabled for now. We should probably
# change permissions so sudo is not
# required. This will also remove
# the requirement to type in the
# password for sudo...
#
#createFolder $LOCAL_WEBSERVER_FOLDER         2


# Remove any existing
#
# Note: If the previous run of this script stopped prematurely,
#       we may get (as a later step moved ):
#
#           mv: cannot stat '/home/embo/temp2/2021-02-03/_DotNET_tryout/EditOverflow4/Program.cs': No such file or directory
#
mv $WORKFOLDER/${FILE_WITH_MAIN_ENTRY}          $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}

cd $SRCFOLDER_DOTNETCOMMANDLINE
cp ${FILE_WITH_MAIN_ENTRY}                      $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}

cp EditOverflow3.csproj                         $WORKFOLDER
cp EditOverflow3_UnitTests.csproj               $WORKFOLDER

# This very script. We don't execute it there -
# it is mostly for the spellchecking.
cp Linux.sh                                     $WORKFOLDER

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

# We don't have it. It is a helper class for testing, but it
# was moved to the main source folder... (as it may see
# wider use than for testing)
#cp $SRCFOLDER_TESTS/RegExExecutor.cs                    $WORKFOLDER

cp $SRCFOLDER_TESTS/WordLookupTests.cs                  $WORKFOLDER


# PHP/web files
cp $SRCFOLDER_WEB/EditOverflow.php                      $WORKFOLDER
cp $SRCFOLDER_WEB/Text.php                              $WORKFOLDER
cp $SRCFOLDER_WEB/FixedStrings.php                      $WORKFOLDER
cp $SRCFOLDER_WEB/EditSummaryFragments.php              $WORKFOLDER
cp $SRCFOLDER_WEB/CannedComments.php                    $WORKFOLDER
cp $SRCFOLDER_WEB/Link_Builder.php                      $WORKFOLDER
#
cp $SRCFOLDER_WEB/deploymentSpecific.php                $WORKFOLDER
#
cp $SRCFOLDER_WEB/commonStart.php                       $WORKFOLDER
cp $SRCFOLDER_WEB/eFooter.php                           $WORKFOLDER
cp $SRCFOLDER_WEB/StringReplacerWithRegex.php           $WORKFOLDER
cp $SRCFOLDER_WEB/commonEnd.php                         $WORKFOLDER


# To the local web server
sudo cp $SRCFOLDER_WEB/EditOverflow.php                 $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/Text.php                         $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/FixedStrings.php                 $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/EditSummaryFragments.php         $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/CannedComments.php               $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/Link_Builder.php                 $LOCAL_WEBSERVER_FOLDER

# Only once at the web server location. Though ideally
# we want to patch it on the fly so we are sure it
# is actually updated if needed.
#
#sudo cp $SRCFOLDER_WEB/deploymentSpecific.php           $LOCAL_WEBSERVER_FOLDER

sudo cp $SRCFOLDER_WEB/commonStart.php                  $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/eFooter.php                      $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/StringReplacerWithRegex.php      $LOCAL_WEBSERVER_FOLDER
sudo cp $SRCFOLDER_WEB/commonEnd.php                    $LOCAL_WEBSERVER_FOLDER

# While we are at it, save a copy of the word data
# file, so we don't lose changes due to bad UX for
# the version control system, etc...
#
# Note that we do have a copy in the work folder, but
# this is less clear than having a copy with a date
# stamp in the file name. It is also overwritten
# every time we run the build script.
#
# For now, one above the folder under Git source control.
# But we should find a better place to copy it to.
#
# TermData.cs is in
# Edit_Overflow/Dot_NET/OverflowHelper/OverflowHelper/Source/main/TermData.cs
#
# "absoluteTimestamp" is our function, defined in this file.
#
#                                                   So Ov Ov Do Ed Parent
cp $SRCFOLDER_CORE/TermData.cs   "${SRCFOLDER_CORE}/../../../../../../TermData_$(absoluteTimestamp).cs"


# ######################################################################
#
#  Check of missing prerequisites (and outputting
#  installation instructions) - instead of
#  failing later with some cryptic error
#  message.
#
#  This also helps in setting up on a new system.
#
#  APT (e.g. Ubuntu) is currently assumed.
#
# Future:
#
#   1. Check for permissions at the local
#      web server location (that we are
#      copying files to). E.g., to avoid
#      the sudo line.
#
#   2.
#
startOfBuildStep "2" "Checking prerequisites"

# Change the current folder so we don't risk writing
# error log files to source folders...
cd $WORKFOLDER


# Note:
#
#   The prerequisite Pylint itself has prerequisites which
#   in turn also has prerequisites (or prerequisites for
#   installation - that is, the installer itself may be
#   missing)... This is not handled automatically by
#   the package system.

prefix_virtual_environment="source ~/.Edit_Overflow_environment/bin/activate\n"

prefix1="\n\nThe prerequisite"
prefix2="is not installed. Execute these commands:\n\n"
postfix1="\n\n\n\n"

postfix1_Python="\ndeactivate\n\n\n"
prefix2_Python="${prefix2}${prefix_virtual_environment}"


# This is to prevent:
#
#    "/usr/bin/python3: No module named pip"
#
#checkCommand "pip3" "${prefix1} Pip 3 ${prefix2}ABC\nXYZ\nDEF\npip3 install pylint ${postfix1}"  1
checkCommand "pip3 --version" "${prefix1} Pip 3 ${prefix2}sudo apt install python3-pip ${postfix1}"  2


# Pylint itself
#
#   Or "pip3 install pylint"?
#
# 'pylint' has a return code of 32 for empty
#  input... (but 'pip3' has a return code
#  of 0 for empty input).
#
#checkCommand "pylint --version" "${prefix1} Pylint ${prefix2}sudo python3 -m pip install pylint ${postfix1}"  2
checkCommand "pylint --version" "${prefix1} Pylint ${prefix2_Python}pip install pylint ${postfix1_Python}"  2


# Python bindings for Selenium. This is sufficient to make
# web_regress.py pass the Pylint check.
#
# If it is not installed we get this from Pylint:
#
#    web_regress.py:29:0: E0401: Unable to import 'selenium' (import-error)
#    web_regress.py:30:0: E0401: Unable to import 'selenium.webdriver.common.keys' (import-error)
#
# However, the executable is not "selenium"...
# 'pip3 show selenium' will return an exit
# code different from zero if it isn't
# found, but this does not fit in
# with checkCommand().
#
# Option "-q" is for suppresing output (we are
# only interested in the return code).
#
#checkCommand "pip3 -q show selenium" "${prefix1} Selenium ${prefix2}sudo pip3 install selenium ${postfix1}"  2
checkCommand "pip3 -q show selenium" "${prefix1} Selenium ${prefix2_Python}pip install selenium ${postfix1_Python}"  2


# LAMP. PHP is used directly by this build
# script, both from the command-line and
# a web server (Apache) context. Apache
# is also associated with the existence
# of the '/var/www' folder (that we
# copy files to in this script).
#
# PHP on the command line is taken as a
# proxy for the installation of. Later
# steps also check for the '/var/www' folder.


# Apache
#
# Can we use APT to test for installation?
#
# We also ought to test for:
#
#   * And an up and running Apache webserver
#
#   * The existence of ${LOCAL_WEBSERVER_FOLDER}
#     (test if some of the installation
#      instructions have been carried out)
#
checkCommand "dpkg -s apache2" "${prefix1} Apache (part of LAMP) ${prefix2}sudo apt install apache2\nsudo systemctl status apache2\nsudo mkdir ${LOCAL_WEBSERVER_FOLDER} ${postfix1}"  2


# PHP
#
checkCommand "php --version" "${prefix1} PHP (part of LAMP) ${prefix2}sudo apt install php libapache2-mod-php php-mysql\nsudo cp $SRCFOLDER_WEB/deploymentSpecific.php  $LOCAL_WEBSERVER_FOLDER ${postfix1}"  2


# MySQL (or rather, MariaDB)
#
checkCommand "dpkg -s mariadb-server" "${prefix1} MySQL/MariaDB (part of LAMP) ${prefix2}sudo apt install mariadb-server\nsudo mysql_secure_installation\nmariadb --version $LOCAL_WEBSERVER_FOLDER ${postfix1}"  2
checkCommand "mariadb --version" "${prefix1} MySQL/MariaDB (part of LAMP) ${prefix2}sudo apt install mariadb-server\nsudo mysql_secure_installation\nmariadb --version $LOCAL_WEBSERVER_FOLDER ${postfix1}"  2


# .NET (C#)
#
# References:
#
#   <https://learn.microsoft.com/en-us/dotnet/core/install/linux>
#
#   <https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian>
#     Install the .NET SDK or the .NET Runtime on Debian
#
#     Supported: .NET 8 and .NET 9 on Debian 12 (Bookworm)
#
#         wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
#         sudo dpkg -i packages-microsoft-prod.deb
#         rm packages-microsoft-prod.deb
#
# Notes:
#
#   1. The executable 'dotnet' without parameters has
#      a return code of 129 for empty input...
#
#   2. "dotnet nuget --version" is more specific than
#      "dotnet --version" (we need NuGet to install
#      NUnit for unit testing), but it probably
#      doesn't make a difference (NuGet is
#      probably always available with .NET
#      on Linux)
#
#      After 2022-03-02 installation on Ubuntu 18.04:
#
#        * .NET: 3.1.101
#
#        * NuGet: 5.4.0.2
#
#checkCommand "dotnet nuget --version" "${prefix1} C# compiler ${prefix2}wget -q https://packages.microsoft.com/config/ubuntu/19.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb\nsudo dpkg -i packages-microsoft-prod.deb\n\nsudo apt-get update\nsudo apt-get install apt-transport-https\nsudo apt-get update\nsudo apt-get install dotnet-sdk-3.1 ${postfix1}"  2
checkCommand "dotnet nuget --version" "${prefix1} C# compiler ${prefix2}wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb\nsudo dpkg -i packages-microsoft-prod.deb\nrm packages-microsoft-prod.deb\n\nsudo apt-get update\nsudo apt-get install -y dotnet-sdk-9.0 ${postfix1}"  2


# C# unit tests (NUnit)
#
# This ***seems*** to work without explicit installation(?).
#
# Note:
#
#   This needs to run in the context of a working
#   unit project. If another project is enabled
#   (e.g., the main projects) we may get
#   something like this:
#
#     "Found more than one project in
#       `/home/mortensen/temp2/2022-03-01/_DotNET_tryout/EditOverflow4/`.
#       Specify which one to use."
#
#   Thus, for how we have set it up, the build
#   of copy files to the work folder should
#   be before this check.
#
#checkCommand "dotnet unitestZZZ" "${prefix1} NUnit (C#) ${prefix2}dotnet add package NUnit ${postfix1}"  2


#dotnet nuget
#
#g "dotnet" "nuget" check installed
#
#dotnet add package NUnit
#
#dotnet add package --help
#
#  Project dependent:
#
#    You can list the package references for your project using the dotnet list package command.


# Node.js (for Jest)
#
#   1. The executable 'npm' without parameters has
#      a return code of 1 for empty input...
#
#   2. Version v8.10.0 on 2022-03-02 on Ubuntu 18.04.
#      But this is too old to install Jest. 'nvm'
#      can be used to install a newer version of
#      Node.js. 2022 versions of Jest requires
#      at least Node.js version 16.x...
#
checkCommand "nodejs --version" "${prefix1} Nodes.js ${prefix2}sudo apt update\nsudo apt install nodejs npm ${postfix1}"  2
checkCommand "npm --version" "${prefix1} Nodes.js ${prefix2} sudo apt update\nsudo apt install nodejs npm ${postfix1}"  2


# Indirect check of a sufficient high version
# of Node.js for Jest to work: We assume if
# nvm is installed, then version of v16.X is
# also installed (even on Ubuntu 18.04 where
# the package system one is v8.10.0) -
# otherwise the next test, for Jest itself,
# will fail.
#
# Extra for enabling Jest to run on older versions
# of Ubuntu (e.g., 18.04). Not stricly necessary
# for some later versions.
#
# nvm v0.35.3 with the method above.

# As we don't presume to have 'nvm' in the path.
export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"  # This loads nvm

checkCommand "nvm --version" "${prefix1} nvm ${prefix2}\n# Note: Without 'sudo'!!!\ncurl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.35.3/install.sh | bash\nexport NVM_DIR=\"$HOME/.nvm\"\n[ -s \"$NVM_DIR/nvm.sh\" ] && \. \"$NVM_DIR/nvm.sh\"\nnvm install 12.22.12 ${postfix1}"  2

# Installation of Jest failed on 2022-03-03
# on Ubuntu 18.04, but it was possible to
# use 'nvm' to overcome it.
#
# Note:
#
#     It may (not always) also fail without a working Internet
#     connection. Why should just "jest --version"
#     require an Internet connection???? As it does
#     not always, what are the conditions for the
#     failure?
#
#     The error code is 127.
#
# Installation of the prerequisite ***sufficient high*** version
# of Node.js on Ubuntu 18.04 (using nvm):
#
#     curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.35.3/install.sh | bash
#     export NVM_DIR="$HOME/.nvm"
#     [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"  # This loads nvm
#     nvm install 16.16.0
#
# Jest (JavaScript unit testing)
#
checkCommand "jest --version" "${prefix1} Jest ${prefix2}\n# Note: Without 'sudo'!!!\nnvm install 16.16.0\nnpm install -g jest ${postfix1}"  2

# For Jest version 28 (release 2022-04-25) and our tests,
# package "jest-environment-jsdom" must be installed.
# Ref.: <https://jestjs.io/blog/2022/04/25/jest-28>
#
checkCommand "npm list -g jest-environment-jsdom" "${prefix1} Jest JSDOM module ${prefix2}\n# Note: Without 'sudo'!!!\nnpm install -g jest-environment-jsdom ${postfix1}"  2


#exit


# Selenium driver for Firefox
#
# The **test*** is not really a prerequisite (but the
# existence of the geckodriver is), but we use the
# same mechanism to test and report if this folder
# exists to the user get better information about
# the reason for a failure (for instance, its
# location could be misconfigured). Though
# the error message is off.
#
#checkCommand "ls  ${SELINUM_DRIVERFOLDER} " "${prefix1} Selenium driver folder existence ${prefix2}pip3 install webdriver-manager ${postfix1}"  2
checkCommand "ls  ${SELINUM_DRIVERFOLDER} " "${prefix1} Selenium driver folder existence ${prefix2}pip install webdriver-manager ${postfix1}"  2


# lftp (that we use to copy files to and from production)
#
#   <https://en.wikipedia.org/wiki/Lftp>
#
checkCommand "lftp --version" "${prefix1} lftp ${prefix2}\nsudo apt install lftp ${postfix1}"  2



#Future:
#
# Check for the existence of file
# "Header_EditOverflow_forMySQL_UTF8.sql"
# in the specified location.
#checkCommand "ls  ${SOME_FUTURE_VARIABLE_CONTAINING_ABSOLUTE_PATH_TO_IT} " "${prefix1} SQL header file ${prefix2} Configure the build script. ${postfix1}"  2


# ###########################################################################
#
#  Self test of Python/Selenium script
#
startOfBuildStep "3" "Internal check of the web interface regression tests"


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
#   C0302: For more than 1,000 lines...
#
#   R0914: For more than 15 local variables... Though it would be
#          better to configure it for 16 15 local variables.
#
#pylint --disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125                                                  $SELINUM_DRIVERSCRIPT_FILENAME ; evaluateBuildResult 1 $? "Python linting for the Selenium script"
#pylint --disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125 --disable=R0913 --disable=C0302                  $SELINUM_DRIVERSCRIPT_FILENAME ; evaluateBuildResult 1 $? "Python linting for the Selenium script"
#pylint --disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125 --disable=R0913 --disable=C0302 --disable=R0914  $SELINUM_DRIVERSCRIPT_FILENAME ; evaluateBuildResult 1 $? "Python linting for the Selenium script"
pylint  $PYLINT_DISABLE_WARNINGS_SET  $SELINUM_DRIVERSCRIPT_FILENAME ; evaluateBuildResult 1 $? "Python linting for the Selenium script"


# ###########################################################################
#
# The rest presumes the work folder is the current folder
#
echo
cd $WORKFOLDER
#echo Work folder: $WORKFOLDER


# ###########################################################################
#
# Rudimentary spellcheck of source code
#
#
sourceSpellcheck  4


# ###########################################################################
#
# Check of:
#
#   1. keyboard shortcuts conflicts and
#
#   2. Indentation rules (even number of spaces)
#
# Note: This is direct inspection of the PHP source code,
#       ***not*** as run by a web serber (rendered HTML).
#
keyboardShortcutConsistencyCheck  EditOverflow.php          "Edit Overflow lookup"       5

keyboardShortcutConsistencyCheck  Text.php                  "text stuff"                 6

keyboardShortcutConsistencyCheck  FixedStrings.php          "fixed string"               7

keyboardShortcutConsistencyCheck  EditSummaryFragments.php  "edit summary"               8

keyboardShortcutConsistencyCheck  CannedComments.php        "canned comments"            9

keyboardShortcutConsistencyCheck  Link_Builder.php          "link builder"              10



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
#
# Error encountered 2021-12-02T193908:
#
#     Build step 3 (PHP test (step 14: main lookup (file EditOverflow.php).
#     Extra information: expected "Access denied for user", but got
#     "PHP Fatal error:  Uncaught PDOException: PDO::__construct():
#     php_network_getaddresses: getaddrinfo failed: Temporary failure in name
#     resolution in
#     /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/deploymentSpecific.php:55
#
#     Stack trace:
#
#     0 /home/embo/temp2/2021-06-14/_DotNET_tryout/EditOverflow4/deploymentSpecific.php(55):
#     PDO->__construct()"  ) failed (error code 1).
#
#   Reason:
#
#     Missing network connection (to the Internet). Not even a connection
#     to the nearest router (without an Internet connection) is enough. Is
#     a name server (DNS) required ("failure in name resolution"
#     suggests it)? What needs to be looked up?
#
#     Is there a workaround? Putting 'localhost' in the hosts file? It
#     is already there...
#
#     Why is this required??? Interface "lo" (loopback)
#     should work for localhost.
#
#     Note that it is not the only reason for this error.
#
#
PHP_code_test  EditOverflow.php          "main lookup"             11  "OverflowStyle=Native&LookUpTerm=JS"  "Access denied for user"


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
#  LMDE: "Undefined variable: $dummy2"
#
#    Why is there a difference? Different version of PHP?
#    Different configuration of PHP?
#
PHP_code_test  Text.php                  "self test, unit tests"                 12  "OverflowStyle=Native&PHP_DoWarnings=On"  "$PHP_WARNING"

#exit

# ###########################################################################
#
# Invoke the function "Real quotes" in the Edit Overflow "text" window
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
#  LMDE 6: "Undefined variable: $dummy2" (with "$", but without ":")
#
#    Why is there a difference? Different version of PHP?
#    Different configuration of PHP?
#
PHP_code_test  Text.php                  "text transformation - 'Real quotes'"   13  "OverflowStyle=Native&PHP_DoWarnings=On&someText=dasdasd&someAction%5Breal_quotes%5D=Real+quotes"  "$PHP_WARNING"

#exit


# ###########################################################################
#
#  Test of the fixed strings page (essentially static HTML)
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
PHP_code_test  FixedStrings.php          "fixed strings"                         14  "OverflowStyle=Native&PHP_DoWarnings=On"   "$PHP_WARNING"


# ###########################################################################
#
#  Test of the edit summary fragments page (essentially static HTML)
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
PHP_code_test  EditSummaryFragments.php  "edit summary fragments"                15   "OverflowStyle=Native&PHP_DoWarnings=On"   "$PHP_WARNING"


# ###########################################################################
#
#  Test of the canned comments page (essentially static HTML)
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
PHP_code_test  CannedComments.php        "canned comments"                       16  "OverflowStyle=Native&PHP_DoWarnings=On"   "$PHP_WARNING"


# ###########################################################################
#
#  Test of the link builder page. This one is primarily for checking
#  for PHP syntax errors.
#
#  Note: The undefined variable thing and parameter "&PHP_DoWarnings=On" is
#        due to a current limitation in PHP_code_test()...
#
PHP_code_test  Link_Builder.php        "link builder"                            17  "OverflowStyle=Native&PHP_DoWarnings=On"   "$PHP_WARNING"



# ###########################################################################
#
#  Test test'ish. E.g., positively identify broken links, in
#  both source and production
#

#OK, false alarm (there might have been a temporary
#problem with a particular video on YouTube). But
#at least we now prepared...
## A broken link. On YouTube.-
#forbidden_content  EditSummaryFragments.php  "Broken link"  18  "1Dax90QyXgI"




# ###########################################################################
#
#  Detection of error log entries for the local web server (there shouldn't
#  be any).
#
#
# Notes:
#
#   1. Only HTTP works with our local webserver... Not HTTPS.
#
#   2. We can only use EditOverflow.php if the database
#      access has been set up (a user with password)
#      and file /var/www/html/world/deploymentSpecific.php
#      has been configured according only. E.g.
#
#         $datebaseServer_BaseDomain = 'localhost';
#
#      And
#      the database created with sufficient entries so
#      that the words used for testing here are include
#      (e.g. incorrect word "JS").
#
#      database error goes to the web server log...
#
# Note: Query parameter "LookUpTerm" must be specified. Otherwise we get
#       at strange error in the line "$URL = htmlentities($row['URL']);"
#

# A word lookup that fails (the word does not exist in the database)
webServer_test  "http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=JS"   "localWebserver_Edit_Overflow_lookup"   18

# Missing expected query parameter "LookUpTerm"
webServer_test  "http://localhost/world/EditOverflow.php?OverflowStyle=Native"                 "localWebserver_Edit_Overflow_lookup"   19

# A word lookup that succeeds
webServer_test  "http://localhost/world/EditOverflow.php?OverflowStyle=Native&LookUpTerm=Ghz"  "localWebserver_Edit_Overflow_lookup"   20

webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native"                        "localWebserver_Text"                    21
webServer_test  "http://localhost/world/FixedStrings.php?OverflowStyle=Native"                "localWebserver_fixed_strings"           22
webServer_test  "http://localhost/world/EditSummaryFragments.php?OverflowStyle=Native"        "localWebserver_Edit_summary_fragments"  23
webServer_test  "http://localhost/world/CannedComments.php?OverflowStyle=Native"              "localWebserver_canned_comments"         24

# Invoke the function "Remove TABs and trailing whitespace" in
# the Edit Overflow "text" window
#
webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native&someText=XYZ%20%20%20&someAction%5Bremove_TABs_and_trailing_whitespace%5D=Remove+TABs+and+trailing+whitespace"  "localWebserver_Text_RemoveTABsAndTrailingWhitespace"  25

# Invoke the function "Remove common leading space" in
# the Edit Overflow "text" window
#
webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native&someText=%20%20%20XYZ&someAction%5Bremove_common_leading_space%5D=Remove+common+leading+space"  "localWebserver_Text_RemoveCommonLeadingSpace"                         26

# Invoke the function "Real quotes" in the Edit Overflow "text" window
#
webServer_test  "http://localhost/world/Text.php?OverflowStyle=Native&someText=dasdasd&someAction%5Breal_quotes%5D=Real+quotes"  "localWebserver_Text_RealQuotes"                                                                            27


# Invoke action in the Edit Overflow "Link builder" window (we pass
# in parameters here, not relying on any default parameters)
#
webServer_test  "http://localhost/world/Link_Builder.php?OverflowStyle=Native&LinkText=the%20powers%20that%20be&URL=https%3A%2F%2Fpmortensen.eu%2Fworld%2FFixedStrings.html"  "localWebserver_LinkBuilder_1"                                 28


# ###########################################################################
#
# NUnit tests can actually run
# under .NET Core on Linux...
#
startOfBuildStep "29" "Start running C# unit tests"

# Notes:
#
#  1. Unlike "dotnet run", "dotnet test" does not
#     use option "-p" for specifying the project
#     file name (inconsistent)
#
#  2. This indirectly checks for code syntax errors, but
#     not for all the code (only what is dependent on
#     the tests).
#
#  3. If there is an ASSERT, the exit code has been
#     observed to be 139 on Linux (Ubuntu 18.04).
#     evaluateBuildResult() automatically aborts
#     the build in that case.
#
#  4. On 2022-03-24, we got a ***spurious***
#     segmentation fault running the below
#     line right after the previous run
#     resulted in an ASSERT in the build.
#     A second run did not have this problem.
#
#     'dotnet --version' returned "3.1.101"

# Note: The same build number
dotnet test EditOverflow3_UnitTests.csproj  ; evaluateBuildResult 29 $? "C# unit tests"



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
mv  $WORKFOLDER/WordLookupTests.cs                $WORKFOLDER/WordLookupTests.csZZZ


# ###########################################################################
#
# C# compile check, etc. Separate (and a requisite)
# for doing successful exports
#
startOfBuildStep "30" "C# compilation and sanity check"

# Check that it actually compiles... Running the unit tests in a
# previous build step indirectly checks for compilation errors (we
# wouldn't be here it otherwise), but they do not cover all the
# code we use here (for exporting the word list in different
# formats).
#
# It will actually check more than for syntax errors. The entire
# command-line application will be run and if it reports any
# errors during initialisation (e.g., inconsistent word
# list data structures), this will also be detected.
#
# Set up to get minimum output from the command-line .NET Core
# Edit Overflow application... But we don't want to suppress
# any output in case of an error, either.
#
export LOOKUP="NO_THERE"
export COMPILECHECK_OUT="_compileCheckOut.txt"
rm $COMPILECHECK_OUT

# It is not a real export...
# Note: The same build number
wordListExport 30 "compileCheck"  $COMPILECHECK_OUT  40 100

# Extra: Generate a native compiled version of Edit Overflow.
#        The result is also 'EditOverflow3', but deeper in
#        the folder hierarchy, in folder '/linux-x64/publish'
#
# Note: In a later version of .NET Core, it
#       is in folder "Release", not "Debug"
#
# Example: </home/mortensen/temp2/2023-07-11/_DotNET_tryout/EditOverflow4/bin/Debug/netcoreapp3.1/linux-x64/publish/EditOverflow3>

timeStamp "Creating native Edit Overflow application"
echo "Absolute path for native Edit Overflow application: ${supposedNativeApplicationPath}" ; echo
dotnet publish EditOverflow3.csproj
timeStamp "End creating native Edit Overflow application"

# Native application
testCommandLineInterface 30 $supposedNativeApplicationPath "native compiled"

# .NET application
testCommandLineInterface 30 "${WORKFOLDER}/${DOT_NET_EDIT_OVERFLOW_PARTIAL_PATH}" ".NET"

# Fails... Probably because adding RID "linux-x64"
# in the project file changed the location of
# the the executable.
#testCommandLineInterface 30 "${WORKFOLDER}/bin/Debug/netcoreapp3.1/EditOverflow3"           ".NET"

# Was affected by automatic filtering of some leading
# and trailing characters
checkCommandLineLookup 30 $supposedNativeApplicationPath "\"Timeless\" Homerow" "timeless home row keyboard modifier keys"

# We expect the lookup to succeed, even with
# leading and trailing space as input
checkCommandLineLookup 30 $supposedNativeApplicationPath  " OS "   " operating system "

# Some (trailing) characters, like "#", should not
# be automatically stripped ("c#" is a perfectly
# OK word to look up).
#
checkCommandLineLookup 30 $supposedNativeApplicationPath  "gtk#"  "Gtk#"
checkCommandLineLookup 30 $supposedNativeApplicationPath  "c#"    "C#"

# Check that the lookup looks in other word sets if
# the direct lookup did not match
checkCommandLineLookup 30 $supposedNativeApplicationPath  "group policy"  "Group Policy_"
checkCommandLineLookup 30 $supposedNativeApplicationPath  "group policy_"  "Group Policy_"

# Boundary test: Look up word in the last alternative word set
checkCommandLineLookup 30 $supposedNativeApplicationPath  "mike_______"  "Mike_______"
checkCommandLineLookup 30 $supposedNativeApplicationPath  "mike"         "Mike_______"

# Check that the lookup looks in all word sets if
# the direct lookup did not match
checkCommandLineLookup 30 $supposedNativeApplicationPath  "Attiny85_"  "ATtiny85"
checkCommandLineLookup 30 $supposedNativeApplicationPath  "ni_____"    "New-Item"
checkCommandLineLookup 30 $supposedNativeApplicationPath  "MD_"        "Markdown_"


# ###########################################################################
#
# Export the Edit Overflow word list as SQL
#
# Note: Compiler errors will be reported
#       to standard error
#
#       CS0162 is "warning : Unreachable code detected"
#
startOfBuildStep "31" "Exporting the word list as SQL"


# Main operation: Export word list to SQL
#
# Fixed header for the SQL (not generated by Edit Overflow)
#
# Note 1: If the header file does not exist the symptom is something
#         like (because the database table is not cleared):
#
#             MySQL said: Documentation
#             #1062 - Duplicate entry 'y.o' for key 'PRIMARY'
#
# Note 2: File "Header_EditOverflow_forMySQL_UTF8.sql" is in
#         the standard backup (not yet formally checked in)
#
# Note 3: The containing folder of $SQL_FILE (from the
#         client side) must also exist (that is, it
#         is not automatically created by this
#         script if it doesn't exist).
#
# Note 4: We no longer need it. wordListExport() used to concatenate
#         to the initial content, but now it overwrites it, and the
#         header is written out by the C# code.
#
#         It should be removed.
#
#Now incorporated
##Moved to PC2016 another crash Linux system crash 2022-02-26.
### Moved to PC2016 after SSD-related Linux system crash 2020-05-31
###cat /home/mortense2/temp2/2020-02-05/Header_EditOverflow_forMySQL_UTF8.sql > $SQL_FILE
###cat /home/mortensen/temp2/2020-05-30/Backup/Backup_2020-05-30_smallFiles/2020-05-30/Header_EditOverflow_forMySQL_UTF8.sql > $SQL_FILE
##cat '/home/embo/temp2/2020-06-02/Last Cinnamon backup_2020-05-30/Small files/Header_EditOverflow_forMySQL_UTF8.sql' > $SQL_FILE
#cat '/home/mortensen/temp2/2022-02-25/Backup/Backup_2022-02-25_smallFiles/2022-02-25/Header_EditOverflow_forMySQL_UTF8.sql' > $SQL_FILE
#cat '/home/mortensen/UserProf/At_PC2016/_Incorporated_files/Header_EditOverflow_forMySQL_UTF8.sql' > $SQL_FILE
cat "$HOME/UserProf/At_PC2016/_Incorporated_files/Header_EditOverflow_forMySQL_UTF8.sql" > $SQL_FILE


#export STDERR_FILE3="_stdErr_Export3.txt"

# Sanity checks of the exported file:
#
#   1. The size
#
#   2. The content, especially the header
#
# Note: The last two numbers very much depend on the actual
#       word list... We use about a 10% margin.
#
#       2022-01-25: 5374604 bytes

# Note: The same build number
wordListExport 31 "SQL" $SQL_FILE 13160000 14470000

# Note: The same build number
export MATCHING_LINES=`grep -c 'DROP TABLE EditOverflow'  ${SQL_FILE}`
mustBeEqual ${MATCHING_LINES} 1  31   "The generated SQL file is missing the header. One reason could be a misconfigured build script (this script)."

# Create a version for quick import
# (e.g., into the local database)
#
# If it didn't require a password (and thus
# disrupting the script execution), we could
# import it like this in MySQL:
#
#     date ; sudo mysql -u root -p pmortensen_eu_db < ${SQL_FILE_QUICKIMPORT} ; date
#
export SQL_FILE_QUICKIMPORT=${WORKFOLDER}/EditOverflow_${EFFECTIVE_DATE}_quickImport.sql
echo ""                   > ${SQL_FILE_QUICKIMPORT}
echo "SET autocommit=0;" >> ${SQL_FILE_QUICKIMPORT}
echo ""                  >> ${SQL_FILE_QUICKIMPORT}
cat ${SQL_FILE}          >> ${SQL_FILE_QUICKIMPORT}
echo ""                  >> ${SQL_FILE_QUICKIMPORT}
echo "COMMIT;"           >> ${SQL_FILE_QUICKIMPORT}
#ls -lsatr $WORKFOLDER


#Delete?
#echo
#pwd
#echo
#ls -ls $SQL_FILE
#exit


# ###########################################################################
#
# Open some web pages for manual operations. We do this
# as soon as the (SQL) file for the word list is ready
# to be ***imported*** (so the focus changes (Firefox
# comes to the foreground) signals that it is
# ready for import.
#
# Observed in Visual Studio Code 2023-11-23:
#
#   "The diff algorithm was stopped early (after 5000 ms.) [sic]"
#
startOfBuildStep "32" "Opening some web pages and applications for manual operations"

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
startOfBuildStep "33" "Exporting the word list as HTML"

rm $HTML_FILE

# Sanity check on the size of the exported file
#
# Note: The last two numbers very much depend on the actual
#       word list... We use about a 10% margin.
#
#       2022-01-25: 3310553 bytes

# Note: The same build number
wordListExport 33 "HTML"  $HTML_FILE   8540000 9420000


cp  $HTML_FILE  $HTML_FILE_GENERIC

#echo
#ls -ls $HTML_FILE_GENERIC



# ###########################################################################
#
# That is, for keyboard shortcut and ID uniqueness
# rules in the generated HTML for the word list,
# etc. (using a Perl script).
#
startOfBuildStep "34" "Starting checking generated HTML"

# Note: The same build number
${WEBFORM_CHECK_CMD}  ${HTML_FILE_GENERIC} ; evaluateBuildResult 34  $? "Checking generated HTML (file ${HTML_FILE_GENERIC})"



# ###########################################################################
#
# Some redundancy here - to be eliminated
startOfBuildStep "35" "Exporting the word list as JavaScript"

rm $JAVASCRIPT_FILE

# Note: The last two numbers very much depend on the actual
#       word list... We use about a 10% margin.
#
#       2022-01-25: 2512025 bytes
#
# Note: The same build number
wordListExport 35 "JavaScript"  $JAVASCRIPT_FILE   3030000 33340000


# In the work folder
cp  $JAVASCRIPT_FILE  $JAVASCRIPT_FILE_GENERIC

# Temporary: Copy the word list in JavaScript to a source folder (for
#            unit testing - the next step).
#
#            It is not necessary if we use a build folder (it is
#            already generated there)
#
cp  $JAVASCRIPT_FILE_GENERIC  $WEBFOLDER

#echo
#ls -ls $JAVASCRIPT_FILE_GENERIC


# ###########################################################################
#
# Note: There is a huge overhead, especially cold (running the first
#       time after a computer restart). It takes 16 seconds total,
#       even on a newer computer (based on AMD Zen 2), 11 seconds,
#       reported, and 3 ms reported for the actual test...
#
startOfBuildStep "36" "Start running JavaScript unit tests"

# Note: For now, directly in source folder. It should
#       be moved to the work folder.

cd $WEBFOLDER

# That is using Jest under Node.js, with the test files
# in sub folder "__tests__" (in folder "Web_Application").
#
npm test  ; evaluateBuildResult 35 $? "JavaScript unit tests"

# Back to the previous folder (expected to be the work folder)
#
echo
# But will output something...
cd -


# The first step that requires an Internet connection...

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
# If there isn't a (working) Internet
# connection, the result is:
#
#     open: linux42.simply.com: Name or service not known
#     mirror: Not connected
#
startOfBuildStep "37" "Updating the JavaScript word list file on pmortenen.eu (<https://pmortensen.eu/world/EditOverflowList.js>)"

cp  $JAVASCRIPT_FILE_GENERIC  $FTPTRANSFER_FOLDER_JAVASCRIPT
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_JAVASCRIPT} /public_html/world ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"

# Note: The same build number
#
# Failed on 2024-11-06 (but only once):
#
#   "mirror: Login failed: 500 USER: command requires a parameter
#    1 error detected"
#
eval ${LFTP_COMMAND}  ; evaluateBuildResult 37 $? "copying the word list in JavaScript to the web site"


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
startOfBuildStep "38" "Updating the HTML word list file on pmortenen.eu (<https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>)"

cp  $HTML_FILE_GENERIC  $FTPTRANSFER_FOLDER_HTML
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_HTML} /public_html/EditOverflow/_Wordlist ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
# Note: The same build number
eval ${LFTP_COMMAND}  ; evaluateBuildResult 38 $? "copying the HTML word list to the web site"


# ###########################################################################
#
# HTML validation, both for the semi-static HTML pages and
# the (generated) word list in HTML format.
#
# Note: This can fail if something went wrong with the previous
#       update of the wordlist in production (as we presume
#       particular word mappings to be present in the
#       database)
#
# Note: The first thing to try is to submit it manually from
#       a web browser. The URL for it is output to the
#       screen
#
#
#
# It is currently dependent on an external service,
# over the Internet. Some service failures:
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
#  2022-02-26T155945   Intermittent failures, e.g. for step 39 (step 38
#                      just before worked in that particular case):
#
#                        Timed out, with:
#
#                          Submit URL for HTML validation:
#
#                              https://validator.w3.org/nu/?showsource=yes&doc=https%3A%2F%2Fpmortensen.eu%2Fworld%2FText.php%3FOverflowStyle=Native
#
#                          Build step 39 (W3 validation for Text stuff
#                          (file Text.php)) failed (error code 1).
#
#                      The HTML document validated fine when the it happended.
#
#                      Somewhat later, all the small documents validated,
#                      but extremely slowly (in a range on the order of
#                      5 seconds - 40 seconds) and validation on our
#                      5 MB HTML word list file hang for 50 minutes
#                      before Ctrl + C...
#
#                      Even later, it also failed when submitted
#                      from a web browser:
#
#                          503 Service Unavailable
#                          No server is available to handle this request.
#
#  2022-06-03T143446   Timed out (retried two times), e.g.:
#
#                      Error 522 Ray ID: 715872f9bb111d02 •
#                      2022-06-03 12:32:02 UTC
#                      Connection timed out
#
#  2023-05-30     Claimed (they probably broke something
#                 in the validation itself):
#
#                     Error: & did not start a character
#                      reference. (& probably should have
#                      been escaped as &amp;.)
#
#                     At line 491, column 66
#
#                     ?showsource=yes&doc=https%3A%2
#
#  2024-02-17T04:50  "failed (error code 1)"
#
#                    Due to a regular robot test:
#
#                      validator.w3.org
#                      Verifying you are human. This may take a few seconds.
#                      validator.w3.org needs to review the security
#                      of your connection before proceeding.
#
#                      Result:
#
#                        validator.w3.org
#                        Verification successful
#                        Waiting for validator.w3.org to respond...
#
#                    Internal IDs: ID 12749,
#
#  2024-12-10T172500  "failed (error code 1)"
#
#                     Unknown reason. Multiple intermittent failures,
#                     including a Cloudflare redirect with
#                     "Connection timed out", error 522.
#
if [ ${DISABLE_HTMLVALIDATION} != 1 ]; then
    HTML_validation      EditOverflow.php                   "Edit Overflow lookup"    39
    HTML_validation      Text.php                           "Text stuff"              40
    HTML_validation      FixedStrings.php                   "Fixed strings"           41
    HTML_validation      EditSummaryFragments.php           "Edit summary fragments"  42
    HTML_validation      CannedComments.php                 "Canned comments"         43
    HTML_validation      Link_Builder.php                   "Link builder"            44

    # The URL is <https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>
    #
    # Sometimes we get this result:
    #
    #     503 Service Unavailable
    #     No server is available to handle this request.
    #
    # It also failed 2023-09-15. Was it due to a passed size
    # threshold or some new change/change? After we manually
    # submitted it, enabled some JavaScript, and checked
    # the checkbox in the "I am a human" dialog, it
    # worked again...
    #
    HTML_validation_base ${HTML_FILE_GENERIC_FILENAMEONLY}  "Word list (HTML)"        45  '%2FEditOverflow%2F_Wordlist'
fi


# ###########################################################################
#
# Some checks of the generated HTML:
#
#   1. Unique HTML anchors (one and ***only*** one if we 'grep' for one)
#
#   2. XXX
#
startOfBuildStep "45" "Starting checks of the generated HTML"
export MATCHING_LINES=`grep -c '<div id="Mark_Zuckerberg">'  ${HTML_FILE_GENERIC}`
mustBeEqual ${MATCHING_LINES} 1  46   "HTML anchor is not unique"


#We should probably get rid of these two entirely. They are a
#way to get a test result quickly (using the local webserver),
#for a particular kind of test - useful during development,
#but it increases the total run time of the build as the
#same tests are repeated when all tests run in the
#last (real) build step.
#
#Or perhaps make them conditional?
#
#
#    # ###########################################################################
#    #
#    # End-to-end testing of the web interface, using the local web server.
#    #
#    # Note: Only the ***"Text" window*** and limited testing on that.
#    #
#    # It uses Selenium and is quite slow, even using a local web server...
#    #
#    startOfBuildStep "37" "Starting web interface regression tests, local"
#
#    startWatchFile ${LOCAL_WEB_ERRORLOG_FILE}
#
#    #export PATH=$PATH:${SELINUM_DRIVERFOLDER}
#    python3 $SELINUM_DRIVERSCRIPT_FILENAME TestMainEditOverflowLookupWeb.test_local_text  ; evaluateBuildResult 37 $? "web interface regression tests"
#
#    endWatchFile ${LOCAL_WEB_ERRORLOG_FILE} ; evaluateBuildResult 37 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "
#
#
#    # ###########################################################################
#    #
#    # End-to-end testing of the web interface, using the local web server.
#    #
#    # Note: For the ***main look*** (primary function of Edit Overflow)
#    #
#    # It uses Selenium and is quite slow, even using a local web server...
#    #
#    startOfBuildStep "38" "Starting web interface regression tests, local"
#
#    startWatchFile ${LOCAL_WEB_ERRORLOG_FILE}
#
#    #export PATH=$PATH:${SELINUM_DRIVERFOLDER}
#    python3 $SELINUM_DRIVERSCRIPT_FILENAME TestMainEditOverflowLookupWeb.test_mainLookup_form_localWebserver ; evaluateBuildResult 38 $? "web interface, main lookup, using the local web server"
#
#    endWatchFile ${LOCAL_WEB_ERRORLOG_FILE} ; evaluateBuildResult 38 $? "Web server test: $2. Extra information: \"`echo ; cat ${LOCAL_WEB_ERRORLOG_FILE} | tail -n 1`\"  "


# ###########################################################################
#
# End-to-end testing of the web interface, using both local web server
# and the hosting (live server).
#
# Notes:
#
#   1. Some of this presumes the PHP files have been
#      deployed to production (this is currently a
#      manual process)
#
#   2. It uses Selenium and is quite slow.
#
#   3. Sometimes one or more tests fails due to local timing issues (or
#      perhaps remote web site response time), not because there is an
#      actual test failure. In that case, try to rerun the build (we
#      currently don't know how to prevent it).
#
#      Example (observed 2022-02-22):
#
#        ERROR: test_text (__main__.TestMainEditOverflowLookupWeb) ...
#
#        _checkMarkdownCodeFormatting
#
#   4. Sometimes we get a resource warning (but the
#      test (apparently) succeeds):
#
#         ....s./usr/lib/python3.8/email/feedparser.py:89:
#         ResourceWarning: unclosed <socket.socket fd=7,
#         family=AddressFamily.AF_INET, type=SocketKind.SOCK_STREAM,
#         proto=6, laddr=('127.0.0.1', 36002), raddr=('127.0.0.1', 58277)>
#
#         for ateof in reversed(self._eofstack):
#
#         ResourceWarning: Enable tracemalloc to get the object allocation traceback
#
#      The reason is currently not known.
#
#   5. XXXX
#
startOfBuildStep "47" "Starting web interface regression tests. Both for the local web server and production."

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
#
# Note: The same build number
python3 $SELINUM_DRIVERSCRIPT_FILENAME  ; evaluateBuildResult 47 $? "web interface regression tests"

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

#Too complicated - we ought to use startWatchFile() / endWatchFile() instead.
# Detection of new entries to the error log (normally PHP errors) as
# a result of our excersing web pages in production
#
# Note:
#
#     ***Not a separate build number*** as we are
#     just detecting errors (on the web server)
#     as a result of running a Selenium test.
#
mustBeEqual  "`cat _before_WebHostingErrorLog_MD5.txt`"  "`cat _after_WebHostingErrorLog_MD5.txt`"  46  "New entry in error log: `tail -n 1 ${AFTER_LOGFILE}` "


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
startOfBuildStep "48" "End of build. All build steps succeeded!!"

timeStamp "End time  "

notify-send "The Edit Overflow build script has finished"





