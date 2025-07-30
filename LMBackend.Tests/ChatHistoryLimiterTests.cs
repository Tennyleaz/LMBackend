using NUnit.Framework;
using LMBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMBackend.Models;

namespace LMBackend.Tests;

[TestFixture()]
public class ChatHistoryLimiterTests
{
    [Test()]
    public void LimitHistory_Test()
    {
        // Arrange
        List<ChatMessage> oldMessages = new List<ChatMessage>
        {
            new ChatMessage
            {
                Text = "Hello how are you?"
            },
            new ChatMessage
            {
                Text = "Hello I'm a bot."
            },
            new ChatMessage
            {
                Text = "Ok see you later"
            }
        };
        string prompt = "Describe yourself";

        // Act
        List<ChatMessage> messages =  ChatHistoryLimiter.LimitHistory(oldMessages, 20, prompt);

        // Assert
        Assert.That(messages.Count, Is.GreaterThan(0));
    }
}