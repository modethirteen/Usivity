/*
	TYPES OF DATA
		- URL 	- Load data from here
		- HTML 	- Use this pre-loaded data
		- VALUE - Use this value to execute the required action
*/

usivity = {};
usivity.ids = [];


// API SETTINGS
usivity.apiroot = {};
usivity.apiroot.url = "http://api.usivity.com";

usivity.apiformat = {};
usivity.apiformat.value = "?dream.out.format=jsonp&dream.out.pre=callback";

// MESSAGES (BOTH OPENS AND MY)
usivity.messageinterval = {};
usivity.messageinterval.value = 15000;
usivity.messagelimit = {};
usivity.messagelimit.value = 10; //TODO:  ADD BACK INTO CODE, NOT CURRENTLY BEING USED

// CONNECTIONS
usivity.connectionsapi = {};
usivity.connectionsapi.url = "/usivity/sources";

// OPENMESSAGES
usivity.openmessages = {};
usivity.openmessages.temp = "/template/message.htm";  //TODO: MORE SPECIFIC VARIABLE NAME -- TRY TO REMOVE THIS DEPENDENCY
usivity.openmessages.wrap = ".openstream";  //TODO: MORE SPECIFIC VARIABLE NAME  -- TRY TO REMOVE THIS DEPENDENCY
usivity.openmessages.datapath = "/usivity/messages/";  //TODO: MORE SPECIFIC VARIABLE NAME  -- TRY TO REMOVE THIS DEPENDENCY
usivity.openstream = {};
usivity.openstream.url = "/usivity/messages";

// SOURCES
usivity.sourcetwitter = {};
usivity.sourcetwitter.url = "/usivity/sources/twitter/connection";

// SUBSCRIPTIONS
usivity.subtwitter = {};
usivity.subtwitter.url = "/usivity/sources/twitter/subscriptions";   // TODO: THIS IS NOT NICE FORMATTING.  MAYBE INTRODUCE MULTI DIMENSIONAL SETTING OBJECTS.  VARIABLES WOULD BE NICE TOO






