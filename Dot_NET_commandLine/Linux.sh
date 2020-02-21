
# Driver script for copying only the necessary files to a work folder,
# compile and run part of the .NET code for Edit Overflow that will
# result in the wordlist data's integrity be checked.


export EFFECTIVE_DATE='2020-02-05'

export SRCFOLDER_BASE='/home/mortense2/temp2/Edit_Overflow'
export      WORKFOLDER=/home/mortense2/temp2/${EFFECTIVE_DATE}/_DotNET_tryout/EditOverflow4

export SRCFOLDER_CORE=$SRCFOLDER_BASE/Dot_NET/OverflowHelper/OverflowHelper/Source/main

export SQL_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.sql


# Copy files to the workfolder
#
mkdir $WORKFOLDER
cp Program.cs                          $WORKFOLDER
cp EditOverflow3.csproj                $WORKFOLDER
cp $SRCFOLDER_CORE/WikipediaLookup.cs  $WORKFOLDER
cp $SRCFOLDER_CORE/HTML_builder.cs     $WORKFOLDER


# Compile, run, and redirect SQL output to a file
#
echo
cd $WORKFOLDER
cat /home/mortense2/temp2/2020-02-05/Header_EditOverflow_forMySQL_UTF8.sql  > $SQL_FILE

# Note: Compiler errors will be reported to standard 
#       error, but we currently don't redirect it.
dotnet run | grep -v CS0219                                                >> $SQL_FILE 

# 2> /dev/null


echo
pwd
ls -lsatr $WORKFOLDER

# Output word list statistics - the first number is close to what
# is expected in the report for the import into MySQL.
echo
echo Word statistics:
grep INSERT $SQL_FILE | wc

# ***************************************************************************
# Fish out any error messages (from checking of the integrity of the
# word list data) out of the generated SQL or HTML (the two types 
# of output are currently mixed up...)
echo
grep -v DROP $SQL_FILE | grep -v pmortensen_eu_db | grep -v CREATE | grep -v VARCHAR | grep -v '^\#' | grep -v ')'  | grep -v '^$' | grep -v INSERT | grep -v '<tr>' | grep -v ' <' | grep -v '/>' | grep -v 'nbsp' | grep -v ';'    
echo

cd -


# Open the UnoEuro import page
xdg-open "https://www.unoeuro.com/dk/controlpanel/pmortensen.eu/mysql/"

