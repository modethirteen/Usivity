using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using MindTouch;
using MindTouch.Dream;

namespace Usivity.Core.Libraries.Aws.Sqs
{
    public class SqsClient {

        //--- Class Fields ---
        private static readonly ILog _log = LogUtils.CreateLog();

        //--- Fields ---
        private readonly SqsClientConfig _config;
        private readonly uint _maxMessages;
        private readonly uint? _visibilityTimeout;
        private readonly bool _useExpires;

        //--- Constructors ---
        public SqsClient(SqsClientConfig config) {
            _config = config;
            _maxMessages = config.MaxMessages;
            if (config.VisibilityTimeout != null) {
                _visibilityTimeout = config.VisibilityTimeout;
            }
            _useExpires = config.UseExpires;
            QueueUri = config.QueueUri;
        }

        //--- Properties ---
        public XUri QueueUri { get; private set; }

        //--- Methods ---
        public List<SqsMessage> GetMessages() {
            var parameters = new Dictionary<string, string> {
                {"MaxNumberOfMessages", _maxMessages.ToString()}
            };
            if(_visibilityTimeout != null) {
                parameters.Add("VisibilityTimeout", _visibilityTimeout.ToString()); 
            }
            var response = GetResponse("ReceiveMessage", parameters);
            if(!response.IsSuccessful) {
                throw new DreamResponseException(response);
            }
            var results = response.ToDocument()
                .UsePrefix("sqs", "http://queue.amazonaws.com/doc/2009-02-01/")
                ["//sqs:ReceiveMessageResult/sqs:Message"];
            var messages = new List<SqsMessage>();
            foreach(var result in results) {
                var receiptHandle = result["ReceiptHandle"].AsText;
                var message = new SqsMessage(
                    result["MessageId"].AsText,
                    receiptHandle,
                    result["MD5OfBody"].AsText,
                    result["Body"].Contents
                    );
                messages.Add(message);
                DeleteMessage(receiptHandle);
            }
            return messages;
        }

        public DreamMessage PostMessage(string message) {
            var parameters = new Dictionary<string, string> {
                {"MessageBody", message}
            };
            var response = GetResponse("SendMessage", parameters, true);
            if(response.IsSuccessful) {
                return response;
            }
            throw new DreamResponseException(response);
        }

        public DreamMessage DeleteMessage(string receiptHandle) {
            var parameters = new Dictionary<string, string> {
                {"ReceiptHandle", receiptHandle},
            };
            var response = GetResponse("DeleteMessage", parameters, true);
            if(response.IsSuccessful) {
                return response;
            }
            throw new DreamResponseException(response);
        }

        protected DreamMessage GetResponse(string action, Dictionary<string, string> parameters, bool post) {
            var time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            parameters.Add("AWSAccessKeyId", _config.PublicKey);
            parameters.Add("Action", action);
            parameters.Add("SignatureMethod", AwsSignature.HashMethod);
            parameters.Add("SignatureVersion", "2");
            parameters.Add("Version", "2009-02-01");
            parameters.Add(_useExpires ? "Expires" : "Timestamp", time);
            var parameterPairs = parameters
                .OrderBy(param => param.Key, StringComparer.Ordinal)
                .Select(param => CreateKeyPair(param));
            var query = string.Join("&", parameterPairs.ToArray());
            var p = Plug.New(_config.QueueUri).WithTimeout(_config.Timeout);

            var signed = p.WithQuery(query);
            var request = string.Format("{0}\n{1}\n{2}\n{3}", post ? "POST" : "GET", p.Uri.Host, p.Uri.Path, query);
            var signature = new AwsSignature(_config.PrivateKey).GetSignature(request);
            signed = signed.With("Signature", signature);

            return post
               ? p.WithHeader(DreamHeaders.CONTENT_TYPE, "application/x-www-form-urlencoded")
                    .Post(DreamMessage.Ok(MimeType.TEXT, signed.Uri.Query))
               : p.WithQuery(signed.Uri.Query).Get();
        }

        protected DreamMessage GetResponse(string action, Dictionary<string, string> parameters) {
            return GetResponse(action, parameters, false);
        }

        private static string CreateKeyPair(KeyValuePair<string, string> param) {

            // Note (arnec): For proper signature generation, spaces must be in %20 format (where spaces are + after XUri.Encode)
            // Note (andyv): Amazon requires *, ), ( to be encoded, but not ~ as per RFC 3986/1738
            return param.Key + "=" + XUri.Encode(param.Value)
                 .ReplaceAll("*", "%2A")
                 .ReplaceAll("+", "%20")
                 .ReplaceAll("(", "%28")
                 .ReplaceAll(")", "%29")
                 .ReplaceAll("%7E", "~");
        }
    }
}