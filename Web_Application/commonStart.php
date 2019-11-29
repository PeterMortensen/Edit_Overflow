
<?php

  ###########################################################################
  # WordPress specific!

  # For get the styling and other redundant content (like headers)
  # from WordPress.
  #
  # So now we have a dependency on WordPress.
  #
  define('WP_USE_THEMES', false);
  require(dirname(__FILE__) . '/wp-blog-header.php');

  get_header(); # Note: Using some WordPress themes results in
                #
                #       Some that do not give

?>


