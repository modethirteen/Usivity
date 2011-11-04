/*
	messages.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		-
		-
*/


$(document).ready(function() {
	/*MINIMIZE/MAXIMIZE MESSAGE*/
	$(".message .minimize").live("click", function() {
		$(this).parents(".message").find(".message_text").slideToggle('fast', function() {
    
		});
		return false;
	});	
	
	/*SEND A MESSAGE*/
	$(".message_send").live("submit", function() {
		var href	= $(this).attr("action");
		
		// TODO:  ADD RESPONSE FOR FAIL EVENT
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: "sample message", //TODO:  PUT IN REAL MESSAGE STRING
			url: href,
			success: function(results)
			{
				console.log("worked");	
			},
			error: function(results) 
			{
				console.log("failed");	
			}
		});
		
		return false;	
	});
	
	
	/*DELETE MESSAGE*/
	$(".message_delete").live("submit", function() {
		
		var id = $(this).attr("id");
		var apiaction = "?dream.in.verb=DELETE";
		//TODO: URL HREF FROM API, DON'T CONSTRUCT URI
		var apiurl = (usivity.apiroot.url + usivity.openstream.url + "/" + id + apiaction);
		
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: apiurl,
			success: function(results)
			{
				closeModal();	
			}
		});
		
		
		$("#" + id).parents(".parent").remove();
		
		return false;
	});
	
	/*LOAD THE OPEN STREAM*/
	$(window).load(function() {
		var uri	= usivity.openstream.url;
		openparams = { // TODO:  MOVE TO SETTINGS.JS
			"uri" : uri,
			"dream.out.format" 	: "jsonp",
			"dream.out.pre"	: "callback",
			"stream" : "open",
			"template" 	: "/template/message.htm",
			"target" 	: ".openstream .target"
		};
		fillqueue(uri,openparams,function(x) {
			outputmessages(openparams);
			setInterval("outputmessages(openparams)",usivity.messageinterval.value);
		});
	});	
	
	/*LOAD MY STREAM*/
	$(window).load(function() {
		var uri	= usivity.openstream.url;
		userparams = { // TODO:  MOVE TO SETTINGS.JS
			"uri" : uri,
			"dream.out.format" 	: "jsonp",
			"dream.out.pre"	: "callback",
			"stream" : "user",
			"template" 	: "/template/message.htm",
			"target" 	: ".mystream .target"
		};
		fillqueue(uri,userparams,function(x) { // TODO: LOAD ALL OF THE MY MESSAGES
			outputall(userparams);
		});
	});	
});

function outputmessages(params) 
{
	// GET LAST ID FROM ARRAY
	var storage = params.stream;
	var id 		= usivity.ids[storage].pop();  
	var data 	= usivity[id]; 
	var uri		= params.uri;
	
	var template	= params.template; 
	var target		= params.target;
	
	if (id)
	{
		$.get(template, function(markup)
		{
			var html = preparedata(markup, data);
			var newele = $(document.createElement('div'));
			newele.html(html);
			newele.css("display","none");
			$(target).prepend(newele); //TODO:  GET RID OF .WRAP
			newele.slideDown();
		});
	}
	
	// Refresh the datastore from the API
	if (usivity.ids[storage].length == 3) //TODO: TRY TO GET RID OF .LENGTH, CAUSES JS PROBLEMS
	{
		fillqueue(uri,params);
	}
	
	// TODO:  Empty the stream for limit
	
}
//TODO: GET RID OF DUPLICATE FUN
function outputall(params) 
{
	// GET LAST ID FROM ARRAY
	var storage = params.stream;
	var ids 	= usivity.ids[storage];  
	
	var template	= params.template; 
	var target		= params.target;
	
	if (ids.length)
	{
		$.get(template, function(markup)
		{
			$.each(ids, function(key, value){
				var data 	= usivity[value]; 
				var html = preparedata(markup, data);
				var newele = $(document.createElement('div'));
				newele.html(html);
				newele.css("display","none");
				$(target).prepend(newele); //TODO:  GET RID OF .WRAP
				newele.show();
				//TODO:  ADD DELETE FOR OBJECTS - REMOVE USED OBJECT FROM DATA LIBRARY ONLY WHEN DONE
			});
		});
	}
	
	// Refresh the datastore from the API
// 	if (usivity.ids.length == 3) //TODO: TRY TO GET RID OF .LENGTH, CAUSES JS PROBLEMS
// 	{
// 		fillmessagequeue(usivity.openstream.url);//TODO: CHANGE TO fillqueue()
// 	}
	
	// TODO:  Empty the stream for limit
	
}

