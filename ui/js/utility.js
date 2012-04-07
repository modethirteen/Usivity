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
		var text = text.replace(/(^|\s)@(\w+)/g, '$1<a target="_new" class="profile" href="http://www.twitter.com/#!/$2">@$2</a>');
		return text;
	}
	else
	{
		return false;	
	}
}

// VALIDATE EMAIL ADDRESS
function checkemail(email) 
{ 
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
} 

// GENERATE A DYNAMIC FUNCTION NAME TO USE AS A CALLBACK
function cb()
{
	return("cb" + Math.floor(Math.random()*11111)); //USED FOR A DYNAMIC FUNCTION	
}

// EXTRACT A QUERYPARAM BY NAME
function queryparam(name, url)
{
	name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
	var regexS = "[\\?&]" + name + "=([^&#]*)";
	var regex = new RegExp(regexS);
	if (url)
	{
		var results = regex.exec(url);
	}
	else
	{
		var results = regex.exec(window.location.href);
	}
	if(results == null)
		return "";
	else
		return decodeURIComponent(results[1].replace(/\+/g, " "));
}

// Generate an ISO-8601 TimeStamp
function ISODateString(d)
{
	function pad(n){return n<10 ? '0'+n : n}
	return d.getUTCFullYear()+'-'
		+ pad(d.getUTCMonth()+1)+'-'
		+ pad(d.getUTCDate())+'T'
		+ pad(d.getUTCHours())+':'
		+ pad(d.getUTCMinutes())+':'
		+ pad(d.getUTCSeconds())+'Z'
}

// FILL EMPTY TAGS
function fill_empty()
{
	$(".empty").each( function() {
		var p = $(this).parent();
		var message = p.attr("empty");
		$(this).html(message);
	});	
}
