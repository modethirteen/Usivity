/*
	oauth.js - format and handle generica form interactions
	
	FUNCTIONS
		-
		-
		-
*/
$(document).ready(function() {

	var authtoken= queryparam("oauth_token");
	var authverifier= queryparam("oauth_verifier");
	var apiverb = "?dream.in.verb=PUT"; //TODO:  PUT THE VERBS INTO SETTINGS.JS (PUT, POST, DELETE)
	
	var authparams = ("&oauth_token=" + authtoken + "&oauth_verifier=" + authverifier); //TODO:  POST THESE CONTENTS IN THE BODY, NOT AS QUERY PARAMS.  SECURITY CONCERNS.  TALK TO ANDY
	
	var apiurl = (usivity.apiroot.url + usivity.sourcetwitter.url + apiverb + authparams);
	
	connection = {};
	connection.type 	= "oauth";
	connection.token	= authtoken;
	connection.verifier	= authverifier;
	

	$.ajax({
		type: "POST",
		crossDomain:true, 
		data: connection,
		url: apiurl,
		success: function(results)
		{
			console.log('complete');
			window.close();
		},
		error: function(results)
		{
			console.log('failed');	
		}
	});
	
});


function queryparam(name)
{
	name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
	var regexS = "[\\?&]" + name + "=([^&#]*)";
	var regex = new RegExp(regexS);
	var results = regex.exec(window.location.href);
	if(results == null)
		return "";
	else
		return decodeURIComponent(results[1].replace(/\+/g, " "));
}