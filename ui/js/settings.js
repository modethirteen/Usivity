// APPLICATIONS SETTINGS
app = {};
app.messagedelay 	= 300000;  //300,000 = 5 minutes
app.messagelimit 	= 100;
app.devurl			= "http://usivity.dev"
app.produrl			= "http://usivity.com"


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
	"dream.out.pre"	: "callback"  // TODO:  TAKE THIS OUT OF SETTINGS  // TODO:  CHANGE TO CB()  (currently breaks)
};









