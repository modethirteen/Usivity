/*
	utility.js - all common utility functions that are used across the app
	
	FUNCTIONS
		- 
		-
*/

// REGEX REPLACE CONTENT SUCH AS LINKS, TWITTER NAMES, ETC
function fixmessage(text) 
{
	if (text)
	{
		// CONVERT URL TO HREF
		var text = text.replace(/(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig,'<a class="external" target="_new" href="$1">$1</a>'); 
		
		// LINK TWITTER NAMES
		var text = text.replace(/(^|\s)@(\w+)/g, '$1<a target="_new" class="profile" href="http://www.twitter.com/#!/$2">@$2</a>');
		
		// REPLACE $ WITH HTML
		var text = text.replace("$","---");
		
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