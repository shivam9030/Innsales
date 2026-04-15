
using System;
using System.Collections.Generic;

namespace InnSales.Services.Authentication.EventGrid
{
    public interface IAuthEventGridProvisioningService
    {
        /// Ensures topic exists 
        Task EnsureTopicAsync(string topicName);
        Task CreateClientSubscriptionAsync(
            string topicName,
            string subscriptionName,
            Uri endpoint,
            object customerFilterValue, // string "101" OR int 101 depending on your FilterEvaluator
            int maxDeliveryAttempts = 6,
            TimeSpan? baseRetryDelay = null,
            string deadLetterContainer = "deadletter");
    }
}