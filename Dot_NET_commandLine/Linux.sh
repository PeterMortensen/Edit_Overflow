
# Driver script for copying only the necessary files to a work folder, 
# compile and run part of the .NET code for Edit Overflow that will
# result in the wordlist data's integrity be checked.
#



, etc. so we check the wordlist data integrity immediately after a change to the word list.

export SRCFOLDER_BASE='/home/mortense2/temp2/Edit_Overflow'
export     WORKFOLDER='/home/mortense2/temp2/2020-02-05/_DotNET_tryout/EditOverflow4'

export SRCFOLDER_CORE=$SRCFOLDER_BASE/Dot_NET/OverflowHelper/OverflowHelper/Source/main

mkdir $WORKFOLDER
cp Program.cs                          $WORKFOLDER
cp EditOverflow3.csproj                $WORKFOLDER
cp $SRCFOLDER_CORE/WikipediaLookup.cs  $WORKFOLDER
cp $SRCFOLDER_CORE/HTML_builder.cs     $WORKFOLDER

cd $WORKFOLDER
dotnet run
cd -

