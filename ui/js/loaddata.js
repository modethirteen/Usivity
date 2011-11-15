/*
	loaddata.js - Batch loads data into the data library
	
	FUNCTIONS
		- loadopenstream()
		- loaduserstream()
		- loadusercontacts()
*/

$(document).ready(function() {

	/*LOAD OPEN STREAM*/	
 	$(window).load(function() {	
	 	// 0 second delay for loading my stream
	 	loadopenstream();
	 	setInterval("newopenstreammessage()",usivity.messageinterval.value);
 	});
	
	/*LOAD MY STREAM*/	
 	$(window).load(function() {	
	 	// 2 second delay for loading my stream
	 	setTimeout("loaduserstream();",2000);
 	});
 	
 	/*LOAD MY CONTACTS*/	
 	$(window).load(function() {	
	 	// 4 second delay for loading my contacts
	 	setTimeout("loadusercontacts();",4000);
 	});
 	
});

// LOAD THE OPEN STREAM
function loadopenstream()
{
	var templateuri = "/template/message_open.htm"; // TODO:  PUT IN SETTINGS.jS
	openstreamparams = {
		"stream" : "open",
		"dream.out.format" : "jsonp",
		"dream.out.pre": "callback"
	};
	var objecturi = apiuri(usivity.openstream.url,openstreamparams);
 	$.get(templateuri, function(templatehtml) {
		preparedata(templatehtml, objecturi, function(html) {
			$(".openstream .target").html(html);
			$(".openstream").removeClass("loading");
			$(".openstream .target").fadeIn();
		});
	});	
}

// LOAD THE USER STREAM - DELAYED FOR 2 SECOND TO AVOID TIMING CONFLICTS WITH THE TEMPLATING ENGINE
function loaduserstream()
{
	var templateuri = "/template/message_user.htm";  //TODO:  PUT IN SETTINGS.JS
	userstreamparams = {
		"stream" : "user",
		"dream.out.format" : "jsonp",
		"dream.out.pre": "callback"
	};
	var objecturi = apiuri(usivity.openstream.url,userstreamparams);
	
 	
 	$.get(templateuri, function(templatehtml) {
		preparedata(templatehtml, objecturi, function(html) {
			$(".mystream .target").html(html); // TODO:  CHANGE MYSTREAM TO USERSTREAM
			$(".mystream").removeClass("loading");
			$(".mystream .target").fadeIn();
		});
	});	
}

// LOAD THE CONTACTS - DELAYED FOR 4 SECOND TO AVOID TIMING CONFLICTS WITH THE TEMPLATING ENGINE
function loadusercontacts()
{
	
	//TODO: PUT LOADING ICON ON MY CONTACTS
 	var templateuri = "/template/contact.htm";
 	contactparams = {
		"dream.out.format" : "jsonp",
		"dream.out.pre": "callback"
	};
	var objecturi = apiuri(usivity.contacts.url,contactparams);
 	
 	$.get(templateuri, function(templatehtml) {
		preparedata(templatehtml, objecturi, function(html) {
			$(".contacts .target").html(html);
			$(".contacts").removeClass("loading");
			$(".contacts .target").fadeIn();
		});
	});
}










