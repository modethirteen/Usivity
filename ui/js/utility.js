/*
	utility.js - all common utility functions that are used across the app
	
	FUNCTIONS
		- 
		-
*/

function fixcontent()
{
	$(".message_new").each( function() {
				
		// PROCESS LINKS
		var text = $(this).find(".message_text").html();
		var text = fixlinks($(this).find(".message_text").html());
		$(this).find(".message_text").html(text);
		
		// PROCESS DATE/TIME
		var date = $(this).find(".message_time").attr("value");
		var date = fixdate(date);
		$(this).find(".message_time").html(date);

	});	
}


// REGEX REPLACE CONTENT SUCH AS LINKS, TWITTER NAMES, ETC
function fixlinks(text) 
{
	if (text)
	{
		// CONVERT URL TO HREF
		var text = text.replace(/(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig,'<a class="external" target="_new" href="$1">$1</a>'); 
		
		// LINK TWITTER NAMES
		var text = text.replace(/(^|\s)@(\w+)/g, '$1<a target="_new" class="profile" href="http://www.twitter.com/#!/$2">@$2</a>');
		
		// CONVERT DATES
		var text = text.replace(/\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d\.\d+([+-][0-2]\d:[0-5]\d|Z)/,'hey');
		
		
		return text;
	}
	else
	{
		return false;	
	}
}

function fixdate (date)
{
	var date = date.replace("Z","");
	var date = new Date(date);
	var date = dateFormat(date, "ddd, mmm d, h:MM tt");
	
	return date;
}

// VALIDATE EMAIL ADDRESS
function checkemail(email) 
{ 
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
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

// CONVERT Newlines to <br> tags
function nl2br (str, is_xhtml) {   
	var breakTag = (is_xhtml || typeof is_xhtml === 'undefined') ? '<br />' : '<br>';    
	return (str + '').replace(/([^>\r\n]?)(\r\n|\n\r|\r|\n)/g, '$1'+ breakTag +'$2');
}