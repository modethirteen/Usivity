/*
	utility.js - all common utility functions that are used across the app
	
	FUNCTIONS
		- 
		-
*/


// Build A PROPERLY FORATTED URI
//TODO: REFACTOR OR GET RID OF IT
function apiuri(uri,params) 
{
	if (uri)
	{
		queryparams	= "";
		
		// If params isn't set just append the standard params
		if (!params)
		{
			var params = usivity.apiformat.value;	
		}
		
		// Cut off any appended params
		if (uri.indexOf("?") != -1)
		{
			var uri = uri.substring(0,uri.indexOf("?")-1)
		}

		// Concatenate all params into one string
		var queryparams = "?";
		$.each(params, function(index, value) { 
			queryparams = queryparams + (index + "=" + value + "&");
		});
		
		// Cut off last & symbol from queryparams
		var queryparams = queryparams.substring(0,queryparams.length-1);
	
		
		// CHECK FOR DOMAIN NAME OR NOT
		if (uri.indexOf(usivity.apiroot.url) == -1) // TODO:  CLEAN UP THIS LOGIC
		{
			var fulluri = (usivity.apiroot.url + uri + queryparams);// TODO:  Change URL to URI
		}
		else
		{
			var fulluri = (uri + queryparams);	
		}
		return fulluri;
	}
}

// REGEX REPLACE CONTENT SUCH AS LINKS, TWITTER NAMES, ETC
function fixmessage(text) 
{
	if (text)
	{
		// Convert URL to HREF
		var text = text.replace(/(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig,'<a class="external" target="_new" href="$1">$1</a>'); 
		// Link Twitter Names
		var text = text.replace(/(^|\s)@(\w+)/g, '$1<a class="profile" href="http://www.twitter.com/$2">$2</a>');
		return text;
	}
	else
	{
		return false;	
	}
}
