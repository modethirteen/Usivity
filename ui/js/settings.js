/*
	TYPES OF DATA
		- URL 	- Load data from here
		- HTML 	- Use this pre-loaded data
		- VALUE - Use this value to execute the required action
*/

usivity = {};
usivity.ids = {};
usivity.ids.open 		= [];
usivity.ids.user		= [];
usivity.ids.contacts 	= [];


// DATA
usivity.data = {};

// API SETTINGS
usivity.apiroot = {};
usivity.apiroot.url = "/api/1";

usivity.apiformat = {};
usivity.apiformat.value = {
	"dream.out.format" 	: "jsonp",
	"dream.out.pre"	: "callback"  // TODO:  TAKE THIS OUT OF SETTINGS  // TODO:  CHANGE TO CB()  (currently breaks)
};

// MESSAGES (BOTH OPENS AND MY)
usivity.messageinterval = {};
usivity.messageinterval.value = 300000; //300,000 = 5 minutes
usivity.messagelimit = {};
usivity.messagelimit.value = 10; //TODO:  ADD BACK INTO CODE, NOT CURRENTLY BEING USED

// CONNECTIONS
usivity.connectionsapi = {};
usivity.connectionsapi.url = "/sources";

// OPENMESSAGES  // TODO:  CHANGE TO MESSAGES, GET RID OF OPENSTREAM
usivity.openmessages = {};
usivity.openmessages.datapath = "/messages/";  	//TODO: MORE SPECIFIC VARIABLE NAME  -- TRY TO REMOVE THIS DEPENDENCY
usivity.openstream = {};
usivity.openstream.url = "/messages";

// CONTACTS
usivity.contacts = {};
usivity.contacts.url = "/contacts";

// SOURCES
usivity.sourcetwitter = {};
usivity.sourcetwitter.url = "/sources/twitter/connection";

// SUBSCRIPTIONS
usivity.subtwitter = {};
usivity.subtwitter.url = "/sources/twitter/subscriptions";   // TODO: THIS IS NOT NICE FORMATTING.  MAYBE INTRODUCE MULTI DIMENSIONAL SETTING OBJECTS.  VARIABLES WOULD BE NICE TOO






