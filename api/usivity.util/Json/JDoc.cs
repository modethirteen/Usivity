using MindTouch.Xml;
using Newtonsoft.Json;

namespace Usivity.Util.Json {

    public class JDoc {

        //--- Fields ---
        protected readonly string _json;

        //--- Constructors ---
        public JDoc(string s) {
            _json = s;
        }

        //--- Methods ---
        public XDoc ToDocument(string root) {
            var xml = JsonConvert.DeserializeXmlNode(_json, root);
            return new XDoc(xml);
        }

        public XDoc ToDocument() {
            return ToDocument(null);
        }

        public new string ToString() {
            return _json;
        }
    }
}
