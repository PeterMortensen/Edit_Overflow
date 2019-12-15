
<?php

  ###########################################################################
  # WordPress specific!

  # For getting the styling and other redundant
  # content (like headers) from WordPress.
  #
  # So now we have a dependency on WordPress...
  #
  define('WP_USE_THEMES', false);
  require(dirname(__FILE__) . '/wp-blog-header.php');

  get_header(); # Note: Using some WordPress themes results in the following
                #       on the page itself (though we also have it in the
                #       title for all themes - but this is less intrusive):
                #
                #           "Page not found"
                #
                #       Some themes that do not give it are:
                #
                #           "Responsive"    (the one we currently use)
                #           "Orfeo"
                #           "Hestia"
                #           "Astra"

?>


