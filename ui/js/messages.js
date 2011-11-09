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
	
// 	/*LOAD THE OPEN STREAM*/
// 	$(window).load(function() {
// 		// TODO:  BREAK UP INTO TWO SETS OF PARAMS.  URI PARAMS AND TEMPLATE PARAMS
// 		openparams = { // TODO:  MOVE TO SETTINGS.JS
// 			"stream"	: "open",
// 			"template" 	: "/template/message_open.htm",
// 			"target" 	: ".openstream .target",
// 			"node" : ["messages","message"]
// 		};
// 		var uri = "http://api.usivity.com/usivity/messages?stream=open&dream.out.format=jsonp&dream.out.pre=callback";
// 		fillqueue(uri,openparams,function(x) {
// 			outputmessages(openparams);
// 			setInterval("outputmessages(openparams)",usivity.messageinterval.value);
// 		});
// 	});	

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

function fillqueue(uri,params,callback)
{
	var storage = params.stream;
	$.ajax({
		crossDomain:true, 
		url: uri,
		dataType: 'jsonp',
		jsonp: false,
		jsonpCallback: 'callback',
		mimeType: 'application/json',
		contentType: 'application/json;',
		success: function(results)
		{
			// Loop Through the Node Param
			var val = results;
			$.each(params.node, function(key,value) { 
				val = val[value];
			});
			
			// Loop Through each of the Object Values
			$.each(val, function() { 
				this.body = fixmessage(this.body);  // TODO: Relocate this...only applies to message and openmessage
				var id = this["@id"];
				usivity[id] = this; 
				usivity.ids[storage].push(id);  // TODO: NO NEED FOR STORAGE
			});
			if (callback)
			{
				callback(uri);
			}
		}
	});
	
	
	
}

