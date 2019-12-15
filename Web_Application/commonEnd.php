
<?php

    # Purpose: Some isolation of the WordPress-specific
    #          parts


    # Note: This relies on a previous inclusion of something that
    #       defined useWordPress(), etc. (typically by including
    #       of file "commonStart.php" by the client PHP page).


    if (useWordPress())
    {
	    ###########################################################################
	    # WordPress specific!

        # From WordPress...
        get_footer();

    }
    else
    {
    	# Revert to old-style header
    	#
    	# Yes, the Heredoc style makes it ugly.
    	#
    	echo <<<'HTML_END'
    </body>
</html>
HTML_END;

    }

?>


