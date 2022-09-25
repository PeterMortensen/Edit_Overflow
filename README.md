# Edit Overflow

Edit Overflow is an application for helping with
the mundane aspects of editing Stack Overflow
and other Stack Exchange sites posts (is in the
Markdown format).
While Stack Exchange is the primary focus, it can also
be used when editing on Quora or Wikipedia.

For instance, quick (using the clipboard and keyboard
shortcuts) correction of common spelling mistakes for
things (programming languages, software products,
company names, and hardware).

Edit Overflow was originally only for Windows, but
the first versions of [a web application][150] became
available (and operational) in July 2019. While not yet feature-complete,
it covers the most important functionality.

## Installing Edit Overflow (Windows)

Run [the ClickOnce installer][30].

## Using Edit Overflow

The simplest use is to correct the spelling of a term
(e.g. the incorrect spelling *".Net"*):

1. Copy the incorrect term to the clipboard, typically from
   a text area in a web browser.

   Note that there can be leading and trailing space and
   punctuation characters - they will be preserved.

2. Bring Edit Overflow to the front (e.g. by Alt + Tab).

3. Press F5. The contents of the clipboard is
   replaced with the corrected term.

4. Switch focus back (e.g. by Alt + Tab) and
   paste (e.g. by Ctrl + V).

A macro keyboard can help in automating this process.
For instance, an [ASUS ROG Claymore Cherry MX Brown RGB][31],
where the numeric keyboard is repurposed for macro keys.
As the ROG Claymore stores the settings
in the keyboard (is not dependent on software running on
the computer), this also works on Linux (if the web version
of Edit Overflow is used).

## Other features

### Convert TABs to spaces and remove trailing space

### Special characters

Symbols for degrees, micro, ohm, etc.

Is in menu *Text*.

### Format as code (Markdown, backtick)

Is in menu *Action*.

### Format as keyboard (Markdown)

Is in menu *Action*.

### Lookup on Wikipidia or Wiktionary

Is in menu *Action*.

Especially convenient when looking up a term
that Edit Overflow does not know.

## Other functionality unrelated to editing

### Built-in timer (e.g. for Pomodorro)

General purpose, but it was intended to limit the time
spend editing...

It can be quickly (but not accurately) set by using
the exponential growth / fall off buttons "Less time"
and "More time".

It can be used as a Pomodorro timer.

### Open a list of URLs in the browser

This will open each URLs in the, with a 3 second (default) delay
between each to not overwhelm the browser and to prevent any
blocks by the website (most are rate limited and may ban you
for some time).

Enter/paste a list of URLs into the input field in the lower left.

### YouTube comments

There is a function for converting time stamps and URLs
to a form acceptable for use in YouTube comments.

### Forth and PuTTY

There is a function for making [PuTTY][51] work with [AmForth][52] 
(most Forths in fact), that is, putting appropriate pauses in when 
pasting code into a terminal window for Forth. 
It requires [AutoIt][50] to be installed.

Note that proper operation depends on a particular keyboard layout being active
(several can be installed in Windows), English-like. "United Kingdom keyboard"
is known to work. If it is not the right current keyboard layout, e.g. Danish,
then, for instance, the line comment character, backslash, will effectively
be ignored and in most cases the result will be an error message from AmForth.

## Released versions

### Version 1.1.49 (2019-11-01)

Note: [Installation problems on some Windows system][201]

* Changed: XXXX
* Added: XXX
* Added more words ([now at 9009 input words and 2796 output words][136]).

### Version 1.1.48

* Changed: Menu command to prepare YouTube comments now handles
   email addresses and some false positives for the URL
   processing has been removed ("e.g." and "E.g.").

* Added: Support for use of PuTTY with AmForth (requires AutoIt
   to be installed). The input is text in the clipboard and the
   user must manually change focus to PuTTY within 5 seconds.
   Menu "Utility" &rarr; "Typing out characters - direct".

* Added more words ([now at 8228 input words and 2382 output words][135]).

### Version 1.1.47

Version 1.1.47:

* Changed: The menus have been somewhat reorganised - items were
   moved from menu Action to menu Text and menu Utility.

* Added: Add TAB to the clipboard (useful if a text editor has
   been configured to only use TABs, e.g. Notepad++ or UltraEdit).

* Added: Menu command to prepare YouTube comments - for live time
   links and encoding URLs to they don't look like URLs (to avoid
   deletion) (Transform for YouTube comments (in clipboard)) in
   menu Text).
   Sample text to convert: "07 min 10 secs:
   Book: https://www.amazon.com/Writing-Well".
   Menu "Text"* ? *"Transform for YouTube comments (in clipboard)".

* Added: Menu command to enclose text in <> (Insert in "Look up"
   field and enclose in "<>") in menu Action).

* Added more words (now at 7525 input words and 2091 output words).

## Wordlist

The list of word definitions (each entry contains:
*incorrect term*, *correct term*, and *URL*)
is currently part of the program itself. It can be
exported to HTML and SQL (with a correct header, for
direct import into a MySQL database, say, on a standard
web hosting platform).

Some versions of the wordlist have been published
([more complete list][40]):

* [2019-07-27][135]
* [2019-03-30][134]

## History

Edit Overflow was originally conceived in February 2010 as a way
to speed up using a search engine to find the correct spelling
of a technology on Wikipedia (e.g. to correct *Javascript* to
*JavaScript*) while editing posts on Stack Overflow - a sort
of caching so the Internet search would only have to be done once.
It became cumbersome and time-consuming to do the same searches
over and over again (especially on a 3G Internet connection).

In addition, the reference to the Wikipedia article, for use in
edit summaries and adding annotation to the posts, was also needed.

A countdown timer was also added early to keep the time spent
on editing to a reasonable level.

While the intent was always to have a cross-platform solution, it wasn't
until 2019 that the initial Windows desktop application (Windows Forms
application written in C#) was supplemented with a web application,
prompted by an attempted move from Windows to Linux.

## Contribute

Any corrections (even the simplest of spelling mistakes)
or constructive critisism are welcome.

### Known problems

* The application sometimes appear to not receive input.
   The reason is the (modal) dialog "Time is up!" is
   displayed (by default shown after 30 minutes (when
   the timer reaches 0 minutes)).
   Resolve it by dismissing the dialog.

* On Windows 10, the whole application window shrinks
   when the clipboard is changed by Edit Overflow for
   the first time. (This does not happen on Windows XP
   or Windows 7.)

* Some of the Alt + key keyboard shortcuts sometimes result
   in some of the checkboxes (for the edit summary string)
   be checked instead.

* The user interface needs to be reworked to be more
   modern looking. Perhaps even changing from
   Windows Forms to WPF. Or even HTML5 + CSS + JavaScript.
   Or Atom/Electron.

### Features that would be very nice to have

* Remembering some option settings.
   For example, for option *"Change clipboard on lookup"*.
   This would prevent violation of common user interface standards for the
   current default setting of this value (checked) and still allow the user to
   override it (very convenient for this application as it uses
   the clipboard for information transfer) and have it remembered for all
   following sessions.

    Possible other settings to remember are timer settings and last lookup.

    Note: A solution should be not be brittle in face of ***version changes***
    (e.g. adding a new option to be remembered). Some of the automatic
    solutions very much have this problem. A solution should be based on the
    sound web principle of ignoring what is not understood (e.g. an
    older version of the program reading a session information file
    created by a newer version) and provide reasonable defaults for
    missing elements (e.g. a newer version of the program reading a session
    information file created by an older version).

* An external wordlist (is currently compiled into the application itself)

<!-- References -->

  [30]: http://hmf-tech.com/EditOverflow/setup.exe
  [31]: https://rog.asus.com/keyboards/keyboards/aura-rgb/rog-claymore-model/spec/

  [40]: http://pmortensen.eu/

  [50]: https://en.wikipedia.org/wiki/AutoIt
  [51]: https://en.wikipedia.org/wiki/PuTTY
  [52]: http://amforth.sourceforge.net/

  [136]: http://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_2019-11-01.html
  [135]: http://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_2019-07-27.html
  [134]: http://pmortensen.eu/EditOverflow/_Wordlist/EditOverflowList_2019-03-30.html

  [150]: http://pmortensen.eu/world/EditOverflow1.html

  [201]: https://github.com/PeterMortensen/Edit_Overflow/issues/1
