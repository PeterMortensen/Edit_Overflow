########################################################################
#                                                                      #
# Purpose: Compilation and running of part of the Edit Overflow        #
#          on Linux. It produces SQL and HTML exports of the           #
#          Edit Overflow wordlist.                                     #
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
#  TOC (currently out of date):
#
#     1. Internal check of the web interface regression tests
#     2. Copying files to the build folder
#     3. Start running C# unit tests
#     4. Exporting the word list as SQL
#     5. Opening some web pages for manual operations
#     6. Exporting the word list as HTML
#     7. Exporting the word list as JavaScript
#     8. Start running JavaScript unit tests
#     9. Updating the HTML word list file on pmortenen.eu (<https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>)
#    10. Updating the JavaScript word list file on pmortenen.eu (<https://pmortensen.eu/world/EditOverflowList.js>)
#    11. Starting web interface regression tests
#
#############################################################################

# Meta: The TOC can be generated by this filter (though likely broken by the
#       refactoring of this script on 2021-06-09... a new one would have
#       extract near "startOfBuildStep" and near "keyboardShortcutConsistencyCheck"
#       (for 13 through 16 (both inclusive)):
#
#   | perl -nle 'printf "#    %2d%s\n", $1, $2 if /^echo\s+.(\d+)(.+)\.\.\./'
#
# E.g.:
#
#   cat ~/UserProf/At_XP64/Edit_Overflow/Dot_NET_commandLine/Linux.sh | grep echo  | grep '\. ' | perl -nle 'printf "#    %2d%s\n", $1, $2 if /^echo\s+.(\d+)(.+)\.\.\./'




echo
echo
echo 'Start of building Edit Overflow...'
echo

echo ; echo "Start time: $(date +%FT%T_%N_ns)"  ; echo



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


# To make the unit test run ***itself*** succeed when we
# use a single build folder, we rename a file... We use
# a file name for the file containing "Main()" that
# does not end in ".cs" in order to hide it (until
# after the unit tests have run).
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


export SQL_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.sql

export HTML_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.html

export JAVASCRIPT_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.js


# Fixed name, not dependent on date, etc.
export HTML_FILE_GENERIC=$WORKFOLDER/EditOverflowList_latest.html
export JAVASCRIPT_FILE_GENERIC=$WORKFOLDER/EditOverflowList.js


#export FTP_SITE_URL='ftp://linux42.simplyZZZZZZZ.com' # Poor man's dry run
export FTP_SITE_URL='ftp://linux42.simply.com'


export WEBFORM_CHECK_FILENAME='KeyboardShortcutConsistency.pl'

export WEBFORM_CHECK_CMD="perl -w ${SRCFOLDER_DOTNETCOMMANDLINE}/${WEBFORM_CHECK_FILENAME}"





# ###########################################################################
#
# Helper function to test a build step for errors. We
# exit/stop the script ***altogether*** (not just
# this function) if there is an error.
#
#    $1   Build step number
#    $2   Result code (e.g. 0 for no error)
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
}


# ###########################################################################
#
# Helper function to reduce redundancy
#
function keyboardShortcutConsistencyCheck()
{
    echo
    echo
    echo "$3. Starting keyboard shortcut consistency check for $1..."
    echo


    #perl -w ${SRCFOLDER_DOTNETCOMMANDLINE}/${WEBFORM_CHECK_FILENAME}  ${WEB_SRCFOLDER_BASE}/$1 ; evaluateBuildResult $3  $? "Keyboard shortcut consistency for the $2 page (file $1)"
    ${WEBFORM_CHECK_CMD}  ${WEB_SRCFOLDER_BASE}/$1 ; evaluateBuildResult $3  $? "Keyboard shortcut consistency for the $2 page (file $1)"
}


# ###########################################################################
#
# Helper function to reduce redundancy.
#
# Mostly for the screen output, but we can also use it
# for marking the start of a step in time (e.g., to
# record / output ***timing*** information).
#
#    $1   Build step number
#    $2   Build step description
#
function startOfBuildStep()
{
    echo
    echo
    echo "$1. $2..."
    echo

    # Future:
    #
    #   1. Record the time (absolute or relative) - for later use
    #
    #   2. Output a timestamp (so we know the absolute time it
    #      was run)
}



# ###########################################################################
startOfBuildStep "1" "Internal check of the web interface regression tests"

#
# Sort of prerequisite for testing
#
# Method names in Python are not checked for uniqueness, so only
# one will be used, risking not running all of the Seleniums
# tests (a sort of a (potential) ***false negative*** test).
#
#cd /home/embo/UserProf/At_XP64/Edit_Overflow/Web_Application/__regressTest__
cd ${SELINUM_DRIVERSCRIPT_DIR}
pylint --disable=C0301 --disable=C0114 --disable=C0115 --disable=C0103 --disable=C0116 --disable=W0125  $SELINUM_DRIVERSCRIPT_FILENAME ; evaluateBuildResult 1 $? "Python linting for the Selenium script"


# ###########################################################################
#
# Copy files to the workfolder
#
# We use option "-p" to avoid error output when
# the folder already exists (reruns), like:
#
#     "mkdir: cannot create directory ‘/home/embo/temp2/2020-06-03’: File exists"
#
startOfBuildStep "2" "Copying files to the build folder"

mkdir -p $WORKFOLDER1
mkdir -p $WORKFOLDER2
mkdir -p $WORKFOLDER3
mkdir -p $WORKFOLDER
mkdir -p $FTPTRANSFER_FOLDER_HTML
mkdir -p $FTPTRANSFER_FOLDER_JAVASCRIPT


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


# Compile, run unit tests, run, and redirect SQL & HTML output to files
#
echo
cd $WORKFOLDER


# ###########################################################################
#
# Experimental. NUnit tests can actually run
# under .NET Core on Linux
#
# Note: Currently we continue even if the unit tests fail (this
#       is more about getting started - we can easily run unit
#       tests separately or rerun this script until all
#       errors are gone)
#
startOfBuildStep "3" "Start running C# unit tests"

# Note: unlike "dotnet run", "dotnet test" does not
#       use option "-p" for specifying the project
#       file name (inconsistent)
#
dotnet test EditOverflow3_UnitTests.csproj  ; evaluateBuildResult 3 $? "C# unit tests"


# Prepare for the main run (see in the beginning for an explanation)
mv  $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}  $WORKFOLDER/${FILE_WITH_MAIN_ENTRY}

# This is to hide the unit test files from the normal project
# file (for normal run). Hardcoded for now (some redundancy)
#
mv  $WORKFOLDER/EnvironmentTests.cs               $WORKFOLDER/EnvironmentTests.csZZZ
mv  $WORKFOLDER/WordlistTests.cs                  $WORKFOLDER/WordlistTests.csZZZ
mv  $WORKFOLDER/LookUpStringTests.cs              $WORKFOLDER/LookUpStringTests.csZZZ
mv  $WORKFOLDER/StringReplacerWithRegexTests.cs   $WORKFOLDER/StringReplacerWithRegexTests.csZZZ
mv  $WORKFOLDER/CodeFormattingCheckTests.cs       $WORKFOLDER/CodeFormattingCheckTests.csZZZ
mv  $WORKFOLDER/RegExExecutor.cs                  $WORKFOLDER/RegExExecutor.csZZZ


#exit   # Active: Test only!!!!!!!!! (We currently use this to
        #                             iterate (aided by unit testing)


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
startOfBuildStep "4" "Exporting the word list as SQL"

export WORDLIST_OUTPUTTYPE=SQL
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162   >> $SQL_FILE  ; evaluateBuildResult 4 $? "generation of word list in SQL format"

echo
pwd
echo
ls -ls $SQL_FILE


#exit   # Active: Test only!!!!!!!!!


# ###########################################################################
#
# Open some web pages for manual operations. We do this
# as soon as the (SQL) file for the word list is ready
# to be ***imported*** (so the focus changes (Firefox
# comes to the foreground) signals that it is
# ready for import.
#
startOfBuildStep "5" "Opening some web pages for manual operations"

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


# ###########################################################################
#
# Some redundancy here - to be eliminated
startOfBuildStep "6" "Exporting the word list as HTML"

export WORDLIST_OUTPUTTYPE=HTML
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162   > $HTML_FILE  ; evaluateBuildResult 6 $? "generation of word list in HTML format"

cp  $HTML_FILE  $HTML_FILE_GENERIC

echo
ls -ls $HTML_FILE_GENERIC



# ###########################################################################
#
startOfBuildStep "7" "Starting checking generated HTML"

${WEBFORM_CHECK_CMD}  ${HTML_FILE_GENERIC} ; evaluateBuildResult 7  $? "Checking generated HTML (file ${HTML_FILE_GENERIC})"

#exit   # Active: Test only!!!!!!!!!




# ###########################################################################
#
# Some redundancy here - to be eliminated
startOfBuildStep "8" "Exporting the word list as JavaScript"

export WORDLIST_OUTPUTTYPE=JavaScript
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162  > $JAVASCRIPT_FILE  ; evaluateBuildResult 8 $? "generation of word list in JavaScript format"

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
startOfBuildStep "9" "Start running JavaScript unit tests"

# Note: For now, directly in source folder. It should
#       be moved to the work folder.

cd $WEBFOLDER

# That is using Jest under Node.js, with the test files
# in sub folder "__tests__".
npm test  ; evaluateBuildResult 9 $? "JavaScript unit tests"

# Back to the previous folder (expected to be the work folder)
#
echo
# But will output something...
cd -



#exit   # Active: Test only!!!!!!!!!


# #################################################################
#
# Future: Local test of web part/PHP files before remote/production
#         deployment (syntax, unit tests, and perhaps local
#         integration test (with local web server)), deployment.
#
#         It would also involve copying PHP files to a worker
#         folder (as 'lftp' can not work on individual files)
#
# See ID 10460, d..
#


# ###########################################################################
#
startOfBuildStep "10" "Starting web interface regression tests"

# For now: Not assuming executable 'geckodriver' is in the path
export PATH=$PATH:/home/embo/.wdm/drivers/geckodriver/linux64/v0.28.0

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
python3 $SELINUM_DRIVERSCRIPT_FILENAME  ; evaluateBuildResult 10 $? "web interface regression tests"


# ###########################################################################
#
# Check of:
#
#   1. keyboard shortcuts conflicts and
#   2. Indentation rules (even even number of spaces)
#
keyboardShortcutConsistencyCheck EditOverflow.php         "Edit Overflow lookup"       11

keyboardShortcutConsistencyCheck Text.php                 "text stuff"                 12

keyboardShortcutConsistencyCheck FixedStrings.php         "fixed string"               13

keyboardShortcutConsistencyCheck EditSummaryFragments.php "edit summary"               14




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
startOfBuildStep "15" "Updating the JavaScript word list file on pmortenen.eu (<https://pmortensen.eu/world/EditOverflowList.js>)"

cp  $JAVASCRIPT_FILE_GENERIC  $FTPTRANSFER_FOLDER_JAVASCRIPT
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_JAVASCRIPT} /public_html/world ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
eval ${LFTP_COMMAND}  ; evaluateBuildResult 15 $? "copying the word list in JavaScript to the web site"


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
startOfBuildStep "16" "Updating the HTML word list file on pmortenen.eu (<https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>)"

cp  $HTML_FILE_GENERIC  $FTPTRANSFER_FOLDER_HTML
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_HTML} /public_html/EditOverflow/_Wordlist ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
eval ${LFTP_COMMAND}  ; evaluateBuildResult 16 $? "copying the HTML word list to the web site"



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


# Not really a build step, but it is easier to spot if the build
# failed or not (as we will not get here if it fails).
startOfBuildStep "17" "End of build"



echo ; echo "End time:   $(date +%FT%T_%N_ns)"  ; echo


