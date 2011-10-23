/*
	loaddata.js - Batch loads data into the data library
	
	FUNCTIONS
		-
		-
*/

//TODO:  MAKE THIS WORK FOR MULTIPLE STREAMS
//TODO:  REMOVE DEPENDENCY ON 'DATASTORE'...REMOVE DATASTORE PARAMETER
function fillmessagequeue(url,callback)
{
	
	var apiurl = (usivity.apiroot.url + url + usivity.apiformat.value);
	
	$.ajax({
		/*TODO:  Make Generic function to return data with JSONP*/
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
				var id = this["@id"];
				usivity[id] = this; //TODO:  CHANGE TO NEW OBJECT NAME:USIVITY
				usivity.ids.push(id);  //TODO:  CHANGE TO NEW OBJECT NAME:USIVITY
			});
			if (callback)
			{
				callback(url);
			}
		}
	});
}

function fixmessage(text) {
	
	// Convert URL to HREF
	var text = text.replace(/(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/ig,'<a class="external" target="_new" href="$1">$1</a>'); 
	var text = text.replace(/(^|\s)@(\w+)/g, '$1<a class="profile" href="http://www.twitter.com/$2">$2</a>');
	return text;
}







