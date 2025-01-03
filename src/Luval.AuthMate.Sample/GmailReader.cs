using System.Net.Http.Headers;
using System.Text.Json;

namespace Luval.AuthMate.Sample
{
    /// <summary>
    /// Represents a Gmail reader for accessing and reading emails using the Gmail API.
    /// </summary>
    public class GmailReader
    {
        private readonly string _accessToken;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GmailReader"/> class.
        /// </summary>
        /// <param name="accessToken">The access token used to authenticate with the Gmail API.</param>
        /// <exception cref="ArgumentException">Thrown when the access token is null, empty, or whitespace.</exception>
        public GmailReader(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
            }

            _accessToken = accessToken;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://gmail.googleapis.com/gmail/v1/users/me/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        /// <summary>
        /// Retrieves the top 5 emails from the user's Gmail inbox.
        /// </summary>
        /// <returns>A list of records containing From, To, Subject, and Body of the emails.</returns>
        /// <exception cref="HttpRequestException">Thrown when the Gmail API request fails.</exception>
        public async Task<List<EmailRecord>> GetTop5EmailsAsync()
        {
            // Validate API call
            var messageListResponse = await _httpClient.GetAsync("messages?maxResults=5&labelIds=INBOX").ConfigureAwait(false);
            if (!messageListResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to retrieve messages. Status code: {messageListResponse.StatusCode}");
            }

            var messageListContent = await messageListResponse.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var messageList = JsonSerializer.Deserialize<MessageListResponse>(messageListContent, options);
            if (messageList == null || messageList.Messages == null)
            {
                return new List<EmailRecord>();
            }

            var emailRecords = new List<EmailRecord>();

            // Retrieve details for each message
            foreach (var message in messageList.Messages)
            {
                var messageDetailResponse = await _httpClient.GetAsync($"messages/{message.Id}");
                if (!messageDetailResponse.IsSuccessStatusCode)
                {
                    continue; // Skip this message if it fails
                }

                var messageDetailContent = await messageDetailResponse.Content.ReadAsStringAsync();

                var messageDetail = JsonSerializer.Deserialize<MessageDetailResponse>(messageDetailContent, options);

                if (messageDetail?.Payload == null)
                {
                    continue;
                }

                var from = GetHeaderValue(messageDetail.Payload.Headers, "From");
                var to = GetHeaderValue(messageDetail.Payload.Headers, "To");
                var subject = GetHeaderValue(messageDetail.Payload.Headers, "Subject");
                var body = messageDetail.Payload.Body?.Data ?? string.Empty;

                emailRecords.Add(new EmailRecord(from, to, subject, body));
            }

            return emailRecords;
        }

        /// <summary>
        /// Extracts the value of a specific header from the list of headers.
        /// </summary>
        /// <param name="headers">The list of email headers.</param>
        /// <param name="headerName">The name of the header to extract.</param>
        /// <returns>The value of the specified header, or an empty string if not found.</returns>
        private static string GetHeaderValue(List<Header> headers, string headerName)
        {
            return headers?.FirstOrDefault(h => h.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
        }
    }

    /// <summary>
    /// Represents a record containing email details.
    /// </summary>
    /// <param name="From">The sender of the email.</param>
    /// <param name="To">The recipient(s) of the email.</param>
    /// <param name="Subject">The subject of the email.</param>
    /// <param name="Body">The body content of the email.</param>
    public record EmailRecord(string From, string To, string Subject, string Body);

    /// <summary>
    /// Represents the response from the Gmail API for a list of messages.
    /// </summary>
    public class MessageListResponse
    {
        /// <summary>
        /// Gets or sets the list of messages.
        /// </summary>
        public List<Message> Messages { get; set; }
    }

    /// <summary>
    /// Represents a message in the Gmail API.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the ID of the message.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the ThreadId
        /// </summary>
        public string ThreadId { get; set; }

        /// <summary>
        /// Gets the NextPageToken
        /// </summary>
        public string NextPageToken { get; set; }

        public long ResultSizeEstimate { get; set; }
    }

    /// <summary>
    /// Represents the detailed response for a specific Gmail message.
    /// </summary>
    public class MessageDetailResponse
    {
        /// <summary>
        /// Gets or sets the payload of the message.
        /// </summary>
        public Payload Payload { get; set; }
    }

    /// <summary>
    /// Represents the payload of a Gmail message.
    /// </summary>
    public class Payload
    {
        /// <summary>
        /// Gets or sets the list of headers in the message payload.
        /// </summary>
        public List<Header> Headers { get; set; }

        /// <summary>
        /// Gets or sets the body content of the message payload.
        /// </summary>
        public Body Body { get; set; }
    }

    /// <summary>
    /// Represents a header in the Gmail message payload.
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Gets or sets the name of the header.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the header.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Represents the body of a Gmail message payload.
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Gets or sets the data of the body content.
        /// </summary>
        public string Data { get; set; }
    }

}
