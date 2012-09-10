/*
	utility.js - all common utility functions that are used across the app
	
	FUNCTIONS
		- 
		-
*/

function fixcontent()
{
	$(".message_new").each( function() {
		
		// PROCESS DATE/TIME
		var date = $(this).find(".message_time").attr("value");
		var date = fixdate(date);
		$(this).find(".message_time").html(date);

	});	
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