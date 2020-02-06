
# Driver script for copying only the necessary files to a work folder, 
# compile and run part of the .NET code for Edit Overflow that will
# result in the wordlist data's integrity be checked.
#


export EFFECTIVE_DATE='2020-02-05'


export SRCFOLDER_BASE='/home/mortense2/temp2/Edit_Overflow'
export     WORKFOLDER=/home/mortense2/temp2/${EFFECTIVE_DATE}/_DotNET_tryout/EditOverflow4

export SRCFOLDER_CORE=$SRCFOLDER_BASE/Dot_NET/OverflowHelper/OverflowHelper/Source/main

export SQL_FILE=$WORKFOLDER/EditOverflow_$EFFECTIVE_DATE.sql


mkdir $WORKFOLDER
cp Program.cs                          $WORKFOLDER
cp EditOverflow3.csproj                $WORKFOLDER
cp $SRCFOLDER_CORE/WikipediaLookup.cs  $WORKFOLDER
cp $SRCFOLDER_CORE/HTML_builder.cs     $WORKFOLDER

cd $WORKFOLDER
cat /home/mortense2/temp2/2020-02-05/Header_EditOverflow_forMySQL_UTF8.sql  > $SQL_FILE
dotnet run | grep -v CS0219                                                >> $SQL_FILE
pwd
ls -lsatr $WORKFOLDER


cd -

