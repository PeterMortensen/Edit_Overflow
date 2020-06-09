
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
#                                                                      #
# Driver script for copying only the necessary files to a              #
# work folder, compile and run part of the .NET code for               #
# Edit Overflow.                                                       #
#                                                                      #
#                                                                      #
# Installation note:                                                   #
#                                                                      #
#   To enable compilation and running .NET Core code on                #
#   Ubuntu/Debian, these installation steps works (last                #
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
########################################################################


# Perhaps move to the client side to remove redundancy?
export EFFECTIVE_DATE='2020-02-05'
export EFFECTIVE_DATE='2020-02-28'
export EFFECTIVE_DATE='2020-04-23'
export EFFECTIVE_DATE='2020-06-01'
export EFFECTIVE_DATE='2020-06-03'


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

export FTPTRANSFER_FOLDER=${WORKFOLDER}/_transfer

export SRCFOLDER_CORE=$SRCFOLDER_BASE/Dot_NET/OverflowHelper/OverflowHelper/Source/main
export SRCFOLDER_PLATFORM_SPECIFIC=$SRCFOLDER_BASE/Dot_NET/OverflowHelper/OverflowHelper/Source/platFormSpecific



export SQL_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.sql

export HTML_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.html
export HTML_FILE_GENERIC=$WORKFOLDER/EditOverflowList_latest.html # Fixed name


# Open a web page for verification of push to GitHub
xdg-open "https://github.com/PeterMortensen/Edit_Overflow"


# Open the Simply.com (formerly UnoEuro) import
# page (immediately so we can prepare while
# the rest of the script is running)
xdg-open "https://www.simply.com/dk/controlpanel/pmortensen.eu/mysql/"


# Copy files to the workfolder
#

mkdir $WORKFOLDER1
mkdir $WORKFOLDER2
mkdir $WORKFOLDER
mkdir $FTPTRANSFER_FOLDER

cd $SRCFOLDER_BASE/Dot_NET_commandLine
cp Program.cs                              $WORKFOLDER
cp EditOverflow3.csproj                    $WORKFOLDER
cp $SRCFOLDER_CORE/WikipediaLookup.cs      $WORKFOLDER
cp $SRCFOLDER_CORE/HTML_builder.cs         $WORKFOLDER
cp $SRCFOLDER_CORE/CodeFormattingCheck.cs  $WORKFOLDER


cp $SRCFOLDER_PLATFORM_SPECIFIC/EditorOverflowApplication_Unix.cs  $WORKFOLDER
cp $SRCFOLDER_PLATFORM_SPECIFIC/EditorOverflowApplication.cs       $WORKFOLDER



# Compile, run, and redirect SQL output to a file
#
echo
cd $WORKFOLDER


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
export WORDLIST_OUTPUTTYPE=SQL
dotnet run | grep -v CS0219 | grep -v CS0162                               >> $SQL_FILE

# 2> /dev/null


# Some redundancy here - to be eliminated
export WORDLIST_OUTPUTTYPE=HTML
dotnet run | grep -v CS0219 | grep -v CS0162                               > $HTML_FILE

cp  $HTML_FILE  $HTML_FILE_GENERIC


# Copy the HTML to the public web site.
#
#   Note: Environment variables FTP_USER and FTP_PASSWORD
#         must have been be set beforehand.
#
# The mirror command for 'lftp' does not work for single files...
#
cp  $HTML_FILE_GENERIC  $FTPTRANSFER_FOLDER

export FTP_COMMANDS="mirror -R --verbose ${FTPTRANSFER_FOLDER} /public_html/EditOverflow/_Wordlist ; exit"
export LFTP_COMMAND="lftp -e '${FTP_COMMANDS}' -u ${FTP_USER},${FTP_PASSWORD} ftp://linux42.simply.com"
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
echo Word statistics:
grep INSERT $SQL_FILE | wc


########################################################################
#
# Fish out any error messages (from checking of the integrity of the
# word list data) out of the generated SQL or HTML (the two types
# of output are currently mixed up...)
#
echo
grep -v DROP $SQL_FILE | grep -v pmortensen_eu_db | grep -v CREATE | grep -v VARCHAR | grep -v '^\#' | grep -v 'URL)'  | grep -v '^$' | grep -v INSERT | grep -v '<tr>' | grep -v ' <' | grep -v '/>' | grep -v 'nbsp' | grep -v ';' | grep -v '2020-'
echo

cd -

