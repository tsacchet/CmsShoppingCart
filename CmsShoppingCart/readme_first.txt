things to do when setting up the shoppingcart on a server.

1. install localDb server either via sqlserver express or by ms sql server iso installation.
2. change the fileman conf.json file to update the "RETURN_URL_PREFIX" to include the server. example below:-

{
"FILES_ROOT":          "",
"RETURN_URL_PREFIX":   "http://sacchetta.lexi.com.au/ableshoppingcart/",
...
...
}

3. Update the ckeditor conf.js file to include the server:
file can be found in the scripts directory, C:\WebApplications\CmsShoppingCart\Scripts\ckeditor
example below:-

/*
Copyright (c) 2003-2012, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.editorConfig = function( config )
{
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';

    var server = 'http://sacchetta.lexi.com.au/ableshoppingcart/';
    var roxyFileman = '/fileman/index.html?integration=ckeditor';
    config.filebrowserBrowseUrl = server + roxyFileman ;
    config.filebrowserImageBrowseUrl = server + roxyFileman + '&type=image';
    config.removeDialogTabs = 'link:upload;image:upload';
};
