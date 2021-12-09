﻿using FeedbackService.Core.Domains;
using FeedbackService.Core.Interfaces.Repositories;
using FeedbackService.Handlers;
using HelpMyStreet.Contracts;
using HelpMyStreet.Contracts.FeedbackService.Request;
using HelpMyStreet.Utils.Enums;
using HelpMyStreet.Utils.EqualityComparers;
using HelpMyStreet.Utils.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeedbackService.UnitTests.Handlers
{
    public class GetNewsTickerHandlerTests
    {
        private Mock<IRepository> _repository;
        private GetNewsTickerHandler _classUnderTest;
        private List<FeedbackRatingCount> _feedbackRatingCount;
        private IEqualityComparer<NewsTickerMessage> _equalityComparer;

        [SetUp]
        public void Setup()
        {
            SetupRepository();
            _equalityComparer = new NewsTickerMessages_EqualityComparer();
            _classUnderTest = new GetNewsTickerHandler(_repository.Object);
        }

        private void SetupRepository()
        {
            _repository = new Mock<IRepository>();
            _repository.Setup(x => x.FeedbackSummary(It.IsAny<int?>()))
                .ReturnsAsync(() => _feedbackRatingCount);
        }

        [TestCase(RequestRoles.Volunteer, 100, 10, 90.9, "**90.9%** positive feedback from volunteers", 2)]
        [TestCase(RequestRoles.Volunteer, 8, 2, 80.0, "", 0)]
        [TestCase(RequestRoles.Volunteer, 10, 5, 66.7, "", 0)]
        [TestCase(RequestRoles.Requestor, 100, 10, 90.9, "**90.9%** positive feedback from people requesting or receiving help", 2)]
        [TestCase(RequestRoles.Requestor, 8, 2, 80.0,"", 0)]
        [TestCase(RequestRoles.Requestor, 10, 5, 66.7,"", 0)]
        [Test]
        public async Task HappyPath(RequestRoles role, int happyCount, int sadCount, double positivePercentage, string message, int messageCount)
        {
            int? groupId = -3;
            _feedbackRatingCount = new List<FeedbackRatingCount>();
            _feedbackRatingCount.Add(new FeedbackRatingCount() { RequestRoles = role, Value = happyCount, FeedbackRating = FeedbackRating.HappyFace });
            _feedbackRatingCount.Add(new FeedbackRatingCount() { RequestRoles = role, Value = sadCount, FeedbackRating = FeedbackRating.SadFace });

            NewsTickerResponse response = await _classUnderTest.Handle(new NewsTickerRequest()
            {
                GroupId = groupId
            }, CancellationToken.None);

            if (positivePercentage > 90)
            {
                Assert.AreEqual(true, response.Messages.Contains(new NewsTickerMessage()
                {
                    Value = positivePercentage,
                    Message = message
                }, _equalityComparer));
            }

            if (positivePercentage > 90)
            {
                Assert.AreEqual(true, response.Messages.Contains(new NewsTickerMessage()
                {
                    Value = positivePercentage,
                    Message = $"**{ positivePercentage }%** positive feedback"
                }, _equalityComparer));
            }

            Assert.AreEqual(messageCount, response.Messages.Count);
        }
    }
}
