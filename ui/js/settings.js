// Declare Usivity API Settings
apiroot = "http://api.usivity.com";
apiformat = "?dream.out.format=jsonp&dream.out.pre=callback";




apiopenstream = ("/usivity/openstream/messages");

// Application Settings
messageinterval = 15000;
messagelimit	= 10;


apiinterval = 30000;  // NOT ACTUALLY USED RIGHT NOW



// Data Variables
openmessages = {}; 
openmessages.class	= "message";
openmessages.data	= {};
openmessages.ids 	= [];
openmessages.temp	= "/template/message.htm";
openmessages.wrap	= ".openstream";
openmessages.datapath = "/usivity/openstream/messages/";


