using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using MindTouch.Dream;
using MindTouch.IO;
using MindTouch.Xml;
using Usivity.Entities;
using Usivity.Entities.Types;

namespace Usivity.Services.Parser {

    public class MessageContentParser {

        //--- Class Fields ---
        private static readonly Regex UriRegex = new Regex(XUri.URI_REGEX, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex MentionRegex = new Regex(@"@\w+", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        //--- Fields ---
        private readonly Source _messageSource;

        //--- Constructors ---
        public MessageContentParser(IMessage message) {
            _messageSource = message.Source;
            MessageContent = message.Body;
        }

        //--- Properties ---
        public string MessageContent { get; private set; }

        //--- Methods ---
        public void Process() {
            if(_messageSource == Source.Email) {
                RemoveMarkup();
                ProcessWhitespace();        
            }
            ProcessUris();
            if(_messageSource == Source.Twitter) {
                ProcessMentions();
            }
        }

        public void RemoveMarkup() {
            var content = "<html>" + WebUtility.HtmlDecode(MessageContent) + "</html>";
            content = content.Replace("&", "&amp;");
            var xml = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
            xml.Load(new TrimmingStringReader(content));
            MessageContent = new XDoc(xml).AsInnerText;
        }

        public void ProcessWhitespace() {
            MessageContent = MessageContent.Replace("\n", "<br />");
        }

        public void ProcessMentions() {
            MessageContent = MentionRegex.Replace(MessageContent, new MatchEvaluator(m => {
                var name = m.Value.Substring(1);
                string href;
                switch(_messageSource) {
                    case Source.Twitter:
                        href = "http://www.twitter.com/#!/" + name;
                        break;
                    default:
                        return m.Value;
                }
                return new XDoc("a")
                    .Attr("href", href)
                    .Attr("target", "_new")
                    .Attr("class", "profile")
                    .Value(m.Value)
                    .ToString();
            })); 
        }

        public void ProcessUris() {
            MessageContent = UriRegex.Replace(MessageContent, new MatchEvaluator(
                m => new XDoc("a")
                    .Attr("href", m.Value)
                    .Attr("class", "external")
                    .Value(m.Value)
                    .ToString()
            )); 
        }
    }
}
