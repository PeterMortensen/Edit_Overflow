########################################################################
#                                                                      #
# Purpose: Compilation and running of part of the Edit Overflow        #
#          C# code (.NET) on Linux. It produces SQL and                #
#          HTML exports of the Edit Overflow wordlist.                 #
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


echo
echo
echo 'Start of building Edit Overflow...'
echo

# Perhaps move to the client side to remove redundancy?
export EFFECTIVE_DATE='2020-02-05'
export EFFECTIVE_DATE='2020-02-28'
export EFFECTIVE_DATE='2020-04-23'
export EFFECTIVE_DATE='2020-06-01'
export EFFECTIVE_DATE='2020-06-03'
export EFFECTIVE_DATE='2020-06-14'
export EFFECTIVE_DATE='2020-06-16'
export EFFECTIVE_DATE='2020-09-29'
export EFFECTIVE_DATE='2020-10-24'


# To make the unit test run ***itself*** succeed when we
# use a single build folder. We use a file name for the
# file containing "Main()" that does not end in ".cs" in
# order to hide it (until after the unit tests have run).
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
export SRCFOLDER_BASE='/home/embo/UserProf/At_XP64/Edit_Overflow'
export WORKFOLDER1=/home/embo/temp2/${EFFECTIVE_DATE}


export WORKFOLDER2=${WORKFOLDER1}/_DotNET_tryout
export WORKFOLDER3=${WORKFOLDER2}/EditOverflow4
export WORKFOLDER=${WORKFOLDER3}

export FTPTRANSFER_FOLDER_HTML=${WORKFOLDER}/_transfer_HTML

export FTPTRANSFER_FOLDER_JAVASCRIPT=${WORKFOLDER}/_transfer_JavaScript


export SRCFOLDER_DOTNET=$SRCFOLDER_BASE/Dot_NET
export SRCFOLDER_DOTNETCOMMANDLINE=$SRCFOLDER_BASE/Dot_NET_commandLine
export SRCFOLDER_WEB=$SRCFOLDER_BASE/Web_Application


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



# Open a web page for verification of push to GitHub
xdg-open "https://github.com/PeterMortensen/Edit_Overflow"


# Open the Simply.com (formerly UnoEuro) import
# page (immediately so we can prepare while
# the rest of the script is running)
xdg-open "https://www.simply.com/dk/controlpanel/pmortensen.eu/mysql/"


# Copy files to the workfolder
#
# We use option "-p" to avoid error output when
# the folder already exists (reruns), like:
#
#     "mkdir: cannot create directory ‘/home/embo/temp2/2020-06-03’: File exists"
#
echo
echo
echo '1. Copying files to build folder...'
echo

mkdir -p $WORKFOLDER1
mkdir -p $WORKFOLDER2
mkdir -p $WORKFOLDER
mkdir -p $FTPTRANSFER_FOLDER_HTML
mkdir -p $FTPTRANSFER_FOLDER_JAVASCRIPT


# Remove any existing
mv $WORKFOLDER/${FILE_WITH_MAIN_ENTRY}          $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}


cd $SRCFOLDER_DOTNETCOMMANDLINE
cp ${FILE_WITH_MAIN_ENTRY}                      $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}

cp EditOverflow3.csproj                         $WORKFOLDER
cp EditOverflow3_UnitTests.csproj               $WORKFOLDER

cp $SRCFOLDER_CORE/TermLookup.cs                $WORKFOLDER
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
cp $SRCFOLDER_TESTS/Wordlist.cs                         $WORKFOLDER
cp $SRCFOLDER_TESTS/LookUpStringTests.cs                $WORKFOLDER
cp $SRCFOLDER_TESTS/StringReplacerWithRegexTests.cs     $WORKFOLDER
cp $SRCFOLDER_TESTS/CodeFormattingCheckTests.cs         $WORKFOLDER


# Compile, run unit tests, run, and redirect SQL & HTML output to files
#
echo
cd $WORKFOLDER


# Experimental. NUnit tests can actually run
# under .NET Core on Linux
#
# Note: Currently we continue even if the unit tests fail (this
#       is more about getting started - we can easily run unit
#       tests separately or rerun this script until all
#       errors are gone)
#
echo
echo
echo '2. Start running C# unit tests...'
echo

# Note: unlike "dotnet run", "dotnet test" does not
#       use option "-p" for specifying the project
#       file name (inconsistent)
#
dotnet test EditOverflow3_UnitTests.csproj


#exit   # Active: Test only!!!!!!!!! (We currently use this to
#                                       iterate (aided by unit testing)


# Prepare for the main run (see in the beginning for an explanation)
mv  $WORKFOLDER/${FILE_WITH_MAIN_ENTRY_HIDE}  $WORKFOLDER/${FILE_WITH_MAIN_ENTRY}

# This is to hide the unit test files from the normal project
# file (for normal run). Hardcoded for now (some redundancy)
#
mv  $WORKFOLDER/EnvironmentTests.cs               $WORKFOLDER/EnvironmentTests.csZZZ
mv  $WORKFOLDER/Wordlist.cs                       $WORKFOLDER/EnvironmentTests.csZZZ
mv  $WORKFOLDER/LookUpStringTests.cs              $WORKFOLDER/LookUpStringTests.csZZZ
mv  $WORKFOLDER/StringReplacerWithRegexTests.cs   $WORKFOLDER/StringReplacerWithRegexTests.csZZZ
mv  $WORKFOLDER/CodeFormattingCheckTests.cs       $WORKFOLDER/CodeFormattingCheckTests.csZZZ
mv  $WORKFOLDER/RegExExecutor.cs                  $WORKFOLDER/RegExExecutor.csZZZ


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
echo
echo
echo '3. Exporting the word list as SQL...'
echo
export WORDLIST_OUTPUTTYPE=SQL
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162   >> $SQL_FILE

#exit   # Active: Test only!!!!!!!!!


# Some redundancy here - to be eliminated
echo
echo
echo '4. Exporting the word list as HTML...'
echo
export WORDLIST_OUTPUTTYPE=HTML
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162   > $HTML_FILE

cp  $HTML_FILE  $HTML_FILE_GENERIC


# Some redundancy here - to be eliminated
echo
echo
echo '5. Exporting the word list as JavaScript...'
echo
export WORDLIST_OUTPUTTYPE=JavaScript
time dotnet run -p EditOverflow3.csproj | grep -v CS0219 | grep -v CS0162   > $JAVASCRIPT_FILE

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
echo
echo '6. Start running JavaScript unit tests...'
echo
# Note: For now, directly in source folder. It should
#       be moved to the work folder.

cd $WEBFOLDER

# That is using Jest under Node.js, with the test files
# in sub folder "__tests__".
npm test

# Back to the previous folder
cd -


#exit   # Active: Test only!!!!!!!!!



# Copy the HTML to the public web site.
#
#   Note: Environment variables FTP_USER and FTP_PASSWORD
#         must have been be set beforehand.
#
# The mirror command for 'lftp' does not work for single files...
#
echo
echo
echo '7. Updating the HTML word list file on pmortenen.eu (<https://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_latest.html>)...'
echo
cp  $HTML_FILE_GENERIC  $FTPTRANSFER_FOLDER_HTML
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_HTML} /public_html/EditOverflow/_Wordlist ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
eval ${LFTP_COMMAND}


# Copy the JavaScript code (for the word list) to the public web site.
#
#   Note: Environment variables FTP_USER and FTP_PASSWORD
#         must have been be set beforehand.
#
# The mirror command for 'lftp' does not work for single files...
#
echo
echo
echo '8. Updating the JavaScript word list file on pmortenen.eu (<https://pmortensen.eu/world/EditOverflowList.js>)...'
echo
cp  $JAVASCRIPT_FILE_GENERIC  $FTPTRANSFER_FOLDER_JAVASCRIPT
export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER_JAVASCRIPT} /public_html/world ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ${FTP_SITE_URL}"
eval ${LFTP_COMMAND}


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


