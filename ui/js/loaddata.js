/*
	loaddata.js - Batch loads data into the data library
	
	FUNCTIONS
		- loadopenstream()
		- loadusercontacts()
*/

// TRIGGER ALL DATA LOADING EVENTS
function loaddata()
{
	// CHECK TO SEE IF THERE ARE CONNECTIONS, IF THERE ARE THEN LOAD THE CONTENT, IF NOT LOAD SETUP.HTM
	connectionparams = {
		"dream.out.format" 	: "json"
	};
	var objecturi = apiuri("/api/1/connections",connectionparams);
	
	// TODO, GET CONNECTION, USER AND SUBSCRIPTION COUNT IN ONE API CALL
	$.get(objecturi, function(connections) {	
		var count = connections["@count"];
		//var count = 0;
		
		// MORE THAN ONE CONNECTION EXISTS
		if (count >= 1)
		{
			// LOAD THE OPENSTREAM
			loadopenstream();
			setInterval("newopenstreammessage()",app.messageinterval);   //TODO:  PUT A TIMER KILL IN HERE IF THE API IS NOT AVAILABLE
			
			 // LOAD CONTACTS
			setTimeout("loadusercontacts()",500);
			
			// LOAD USER PANEL
			setTimeout("loaduserpanel()",1000);
		}
		
		// NO CONNECTIONS
		else
		{
			buildModal("","/template/setup.htm");	
		}
	}); 
}

// LOAD USER CREDENTIALS LABEL
function loaduserpanel()
{
	userparams = {
		"dream.out.format" 	: "jsonp",
		"dream.out.pre"	: cb()
	};
	var href = "/template/user_panel.htm";
	var objecturi = apiuri(api.current,userparams);
		
	$.get(href, function(templatehtml) {	
		template(templatehtml, objecturi, "null",function(html) {
			$(".header .menu").html(html);
			$(".header .menu").fadeIn();
		});
	});
}

// LOAD THE OPEN STREAM
function loadopenstream()
{
	var templateuri = "/template/message.htm"; // TODO:  PUT IN SETTINGS.jS
	

	// CALCULATE & SET TIMESTAMP (NOW)
	var now = new Date();
	timestamp 	= ISODateString(now);
	
	// CALCULATE & SET TIMEAGO (-messageloadspan)
	timeago = now.setSeconds(0,-app.messageloadspan);
	timeago 	= ISODateString(new Date(timeago));
	
	openstreamparams = {
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb(),
		"start" : app.messageloadspan,
		"limit" : app.messagelimit
	};
	var objecturi = apiuri(api.messages,openstreamparams);
	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "messages_message", function(html) {

			$(".openstream .target tbody").append(html);
			$(".openstream").removeClass("loading");
			$(".openstream .target").fadeIn();
			
			// PROCESS CONTENT
			fixcontent();
			
			// Add timestamp row
			var timerow = '<tr class="time_row"><td colspan="9" >Between (<span class="timeago" title="' + timestamp + '"></span>) and (<span class="timeago" title="' + timeago + '"></span>)</td></tr>';
			$(".openstream .target tbody").prepend(timerow);
			jQuery(".timeago").timeago();
			
			// REMOVE THE .MESSAGE_NEW CLASS ON ALL NEW MESSAGES
			$(".message_new").slideDown(50);
			$(".message_new").removeClass("message_new");
			
		});
	});	
}

// LOAD THE CONTACTS - DELAYED FOR 1 SECOND TO AVOID TIMING CONFLICTS WITH THE TEMPLATING ENGINE
function loadusercontacts()
{
	
	//TODO: PUT LOADING ICON ON MY CONTACTS
 	var templateuri = "/template/contact.htm";
 	contactparams = {
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb()
	};
	var objecturi = apiuri(api.contacts,contactparams);
 	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "contacts_contact", function(html) {
			$(".contacts .target tbody").html(html);
			$(".contacts").removeClass("loading");
			$(".contacts .target").fadeIn();
		});
	});
}










