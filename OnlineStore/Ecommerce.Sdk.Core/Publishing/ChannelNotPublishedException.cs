/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Retail.Ecommerce.Sdk.Core.Publishing
    {
        using System;
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Represents an exception representing a channel in not published state.
        /// </summary>
        [Serializable]
        public class ChannelNotPublishedException : InvalidOperationException
        {
            private const string PropPublishingStatus = "PublishingStatus";
            private const string PropMessageTemplate = "MessageTemplate";

            /// <summary>
            /// Initializes a new instance of the <see cref="ChannelNotPublishedException"/> class.
            /// </summary>
            /// <param name="messageTemplate">The message template with 2 parameters, one for the channel status value and one for the channel status message.</param>
            /// <param name="publishingStatus">The publishing status.</param>
            /// <param name="message">The publishing status message.</param>
            public ChannelNotPublishedException(string messageTemplate, OnlineChannelPublishStatusType publishingStatus, string message) : base(message)
            {
                this.MessageTemplate = messageTemplate;
                this.PublishingStatus = publishingStatus;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ChannelNotPublishedException"/> class.
            /// </summary>
            /// <param name="info">Serialization info.</param>
            /// <param name="context">Serialization context.</param>
            protected ChannelNotPublishedException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                this.MessageTemplate = info.GetString(PropMessageTemplate);
                this.PublishingStatus = (OnlineChannelPublishStatusType)info.GetByte(PropPublishingStatus);
            }

            /// <summary>
            /// Gets or sets a channel's Publishing Status.
            /// </summary>
            public OnlineChannelPublishStatusType PublishingStatus
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a channel's Publishing Status Message.
            /// </summary>
            public string MessageTemplate
            {
                get;
                set;
            }

            /// <summary>
            /// The exception's message.
            /// </summary>
            public override string Message
            {
                get
                {
                    return string.Format(this.MessageTemplate, this.PublishingStatus, base.Message);
                }
            }

            /// <summary>
            /// Gets data to serialize the exception.
            /// </summary>
            /// <param name="info">Serialization info.</param>
            /// <param name="context">Serialization context.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The parameter 'info' is guaranteed to not be null by .NET Framework.")]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(PropMessageTemplate, this.MessageTemplate);
                info.AddValue(PropPublishingStatus, (byte)this.PublishingStatus);

                base.GetObjectData(info, context);
            }
        }
    }
}