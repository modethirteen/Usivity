//
//	startup.js - all settings and functions needed to launch the app
//

// ####################################################################
// STARTUP SETTINGS
// ####################################################################

// APPLICATIONS SETTINGS
app = {};
app.messageinterval 	= 240000;   // 240,000ms = 4 minute
app.messagelimit 		= 50; 		// 50 message limit for initial load
app.messageloadspan		= 7200000			// 1 hour = 3,600,000 miliseconds
app.domain				= "usivity.com"


// API SETTINGS
api = {};
api.root		= "/api/1";
api.messages 	= "/messages";
api.contacts 	= "/contacts";
api.sources		= "/connections";
api.users		= "/users";
api.auth		= "/users/authentication";
api.current		= "/users/current"

// API PARAMS
api.params = {
	"dream.out.format" 	: "jsonp",
	"dream.out.pre"	: cb()  // TODO:  TAKE THIS OUT OF SETTINGS  // TODO:  CHANGE TO CB()  (currently breaks)
};

// ####################################################################
// STARTUP FUNCTIONS
// ####################################################################

function resize() 
{
	var bheight = $(window).height(); // Body Height
	var cheight = $(".header").outerHeight() // Height of Controls
	var colheight = (bheight - cheight - 5); // Column Height

	/*Wrapper Div*/
	$(".col").css("height",colheight);

	/*Height*/
	$(".mystream").css("height",(colheight - 15));
	$(".openstream").css("height",(colheight - 15));
	$(".contacts").css("height",(colheight - 15));

	$(".fwrap").fadeIn();
}


// Build A PROPERLY FORATTED URI  //TODO: REFACTOR OR GET RID OF IT
function apiuri(uri,params) 
{
	if (uri)
	{
		// CHECK TO SEE IF IT IS A USIVITY API REQUEST
		if (uri.indexOf("http") != -1 && uri.indexOf(app.domain) == -1)
		{
			return uri;
		}
		
		queryparams	= "";
		
		// Cut off any appended params
		if (uri.indexOf("?") != -1)
		{
			var uri = uri.substring(0,uri.indexOf("?")-1)
		}

		// Concatenate all params into one string
		var queryparams = "?";
		$.each(params, function(index, value) { 
			queryparams = queryparams + (index + "=" + value + "&");
		});

		// Cut off last & symbol from queryparams
		var queryparams = queryparams.substring(0,queryparams.length-1);

		// CHECK FOR DOMAIN NAME OR NOT
		if (uri.indexOf(api.root) == -1 && uri.indexOf("http") == -1) // TODO:  CLEAN UP THIS LOGIC
		{
			var fulluri = (api.root + uri + queryparams);// TODO:  Change URL to URI
		}
		else
		{
			var fulluri = (uri + queryparams);	
		}
		
		return fulluri;
	}
}

// GENERATE A DYNAMIC FUNCTION NAME TO USE AS A CALLBACK
function cb()
{
	return("cb" + Math.floor(Math.random()*11111)); //USED FOR A DYNAMIC FUNCTION	
}