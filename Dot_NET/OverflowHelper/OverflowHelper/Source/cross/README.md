
<!-- ***********************************************************************

    File : README.md

--> 


This folder contains the core of Edit Overflow: 

 * Preprecessing of input data so the user does not have 
   to ***exactly*** select a word (e.g. for ignoring space, 
   formatting (e.g. \*\* for bold on Quora), and 
   punctuation).
 * The word lookup logic (e.g. the result when a lookup fails).
   And getting the corresponding URL.
 * Building of the checkin message.
 * Handling the state for several lookups.
 * Formatting of the checkin message in various forms 
   (e.g. with Oxford comma and Stack Overflow style).
 * Formatting of the URL in various forms (e.g. Markdown, HTML).
 * Policy for duplicate lookups (order, duplication or not in the
   edit summary).

Even though the source files are in C#, they are a sort of pseudo code:

A script transforms them to all the platforms/programming languages
we support:

   1. C# (Windows/Windows Forms and Linux/.NET Core)
   
   2. PHP (web, running server side)
   
   3. JavaScript (web, running client side)

As such, the C# source must be formatted in a certain way.




