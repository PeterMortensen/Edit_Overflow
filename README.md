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
the first versions of a web application became
available in July 2019. While not yet feature-complete,
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

### Remove convert TABs to space and remove trailing space

### Special characters

Symbols for degrees, micro, ohm,

Is in menu *Text*.

### Format as code (Markdown, back)

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

Enter/paste a list of URLs intot the input field in the lower left.

### YouTube comments

There is a function for converting time stamps and URLs
to a form acceptable for use in YouTube comments.

### Forth and PuTTY

There is a function for making PuTTY work with AmForth (most Forths in fact),
that is, putting appropriate pauses in when pasting code into
a terminal window for Forth. It requires AutoIt to be installed.


## Released versions

### Version 1.1.48:


 * Changed: Menu command to prepare YouTube comments now handles
   email addresses and some false positives for the URL
   processing has been removed ("e.g." and "E.g.").

 * Added: Support for use of PuTTY with AmForth (requires AutoIt
   to be installed). The input is text in the clipboard and the
   user must manually change focus to PuTTY within 5 seconds.
   Menu "Utility" ? "Typing out characters - direct".

 * Added more words (now at 8228 input words and 2382 output words).

### Version 1.1.47:

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


## Contribute

Any corrections (even the simplest of spelling mistakes)
or constructive critisism is welcome.


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

 
 * Remembering option settings. For example, for option *"Change clipboard on lookup"*.
   This would prevent violation of common user interface standards for the 
   current default setting of this value (checked) and still allow the user to
   override it (very convenient for this application as it use 
   the clipboard for information transfer) and have it remembered for all
   following sessions.
   
    Possible other settings to remember are timer settings and last lookup.

    Note: A solution should be not be brittle in face of version changes 
    (e.g. adding a new option to be remembered). Some of the automatic
    solutions very much have this problem. A solution should be based on the
    sound web principle of ignoring what is not understood (e.g. an 
    older version of the program reading a session information file 
    created by a newer version) and provide reasonable defaults for 
    missing elements ((e.g. a newer version of the program reading a session
    information file created by an older version))

 * An external wordlist (is currently compiled into the application itself)

  [30]: http://hmf-tech.com/EditOverflow/setup.exe
  [31]: https://www.asus.com/us/Keyboards-Mice/ROG-Claymore-Core/

