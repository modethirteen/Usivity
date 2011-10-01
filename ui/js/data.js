$(document).ready(function() {
	
	$(window).load(function() {
		loadapimessage(apiopenstream,openmessages,loadmessage);
		
	});
		
});

// LOAD API DATA
function loadapimessage(url,datastore,func)
{
	var apiurl = (apiroot + url + apiformat);
	
	// HIT THE API EVERY APIINTERVAL
	$.ajax({
		crossDomain:true, 
		url: apiurl,
		dataType: 'jsonp',
		jsonp: false,
		jsonpCallback: 'callback',
		mimeType: 'application/json',
		contentType: 'application/json;',
		success: function(results)
		{
			$.each(results.messages.message, function() { 
				this.body = fixmessage(this.body);
				
				// ALWAYS LOADS DATA INTO .DATA OBJECT
				datastore.data[this.id] = this;
				datastore.ids.push(this.id);
			});
			
			// CALL A DYNAMIC FUNCTION AFTER ALL DATA IS LOADED
			if (func)
			{
				call(func);
				window.setInterval(func, messageinterval);
			}
		}
	});
}

function loadmessage() 
{
	var datastore = openmessages;
	
	// GET LAST ID FROM ARRAY
	var id = datastore.ids.pop();
	var data = datastore.data[id];
	
	var geturl 	= datastore.temp;
	var class  	= datastore.class;
	var wrap	= datastore.wrap;
	
	if (id)
	{
		$.get(geturl, function(markup)
		{
			var html = maketemplate(markup, data);
			var newele = $(document.createElement('div'));
			newele.addClass(class);
			newele.addClass("parent");
			newele.html(html);
			newele.css("display","none");
			$(wrap).find(".target").prepend(newele);
			newele.slideDown();
			//TODO:  ADD DELETE FOR OBJECTS
		});
	}
	
	// Refresh the datastore from the API
	if (datastore.ids.length == 3)
	{
		loadapimessage(apiopenstream,openmessages);
	}
	
	// TODO:  Empty the stream
	
}

function call(method)
{
     method();
}

