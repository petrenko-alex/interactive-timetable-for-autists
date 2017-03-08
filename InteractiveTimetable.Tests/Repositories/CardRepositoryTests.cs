using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using NUnit.Framework;
using SQLite;

namespace InteractiveTimetable.Tests.Repositories
{
    [TestFixture]
    class CardRepositoryTests
    {
        private SQLiteConnection _connection;
        private CardRepository _repository;
        private ScheduleItemRepository _scheduleItemRepository;

        public string GenerateRandomString(int stringLength)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            return new string(Enumerable.Repeat(chars, stringLength).
                                         Select(s => s[random.Next(s.Length)]).
                                         ToArray());
        }

        [SetUp]
        public void InitializationBeforeTests()
        {
            /* Creating connection */
            string dbPath = "TestsDatabase.db3";
            _connection = new SQLiteConnection(dbPath);

            /* Initialize repository */
            _repository = new CardRepository(_connection);
            _scheduleItemRepository = new ScheduleItemRepository(_connection);
        }

        [TearDown]
        public void ShutDown()
        {
            _connection.Dispose();
            File.Delete("TestsDatabase.db3");
        }

        [Test]
        public void CreateSimpleCard()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };

            // act
            var cardId = _repository.SaveCard(card);
            var addedCard = _repository.GetCard(cardId);

            // assert
            Assert.AreEqual(card, addedCard);
        }

        [Test]
        public void CreateCardWithIncorrectPath()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = GenerateRandomString(1025),
                CardTypeId = activityType.Id
            };
            string exceptionString = "The length of the path " +
                                     "to the card's photo must be less " +
                                     "than 1024 symbols";

            // act/assert
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                _repository.SaveCard(card);
            });
            Assert.AreEqual(exceptionString, exception.Message);
        }

        [Test]
        public void CreateCardWithScheduleItems()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            ScheduleItem item1 = new ScheduleItem()
            {
                OrderNumber = 1
            };
            ScheduleItem item2 = new ScheduleItem()
            {
                OrderNumber = 2
            };
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id,
                ScheduleItems = new List<ScheduleItem>()
                {
                    item1,
                    item2
                }
            };

            // act
            var cardId = _repository.SaveCard(card);
            var addedCard = _repository.GetCard(cardId);

            // assert
            Assert.AreEqual(2, addedCard.ScheduleItems.Count);
            Assert.AreEqual(addedCard.Id, addedCard.ScheduleItems[0].CardId);
            Assert.AreEqual(addedCard.Id, addedCard.ScheduleItems[1].CardId);
        }

        [Test]
        public void UpdateSimpleCard()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            var cardId = _repository.SaveCard(card);
            var addedCard = _repository.GetCard(cardId);
            var newPath = "path/to/song.wav";

            // act
            addedCard.PhotoPath = newPath;
            _repository.SaveCard(addedCard);
            addedCard = _repository.GetCard(cardId);

            // assert
            Assert.AreEqual(newPath, addedCard.PhotoPath);
            Assert.AreEqual(card.CardTypeId, addedCard.CardTypeId);
        }

        [Test]
        public void UpdateCardWithScheduleItems()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            ScheduleItem item1 = new ScheduleItem()
            {
                OrderNumber = 1
            };
            ScheduleItem item2 = new ScheduleItem()
            {
                OrderNumber = 2
            };
            ScheduleItem item3 = new ScheduleItem()
            {
                OrderNumber = 3
            };
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id,
                ScheduleItems = new List<ScheduleItem>()
                {
                    item1,
                    item2
                }
            };
            var cardId = _repository.SaveCard(card);
            var addedCard = _repository.GetCard(cardId);

            // act

            /* Change first card */
            item2 = addedCard.ScheduleItems[1];
            item2.OrderNumber = 5;
            _scheduleItemRepository.SaveScheduleItem(item2);

            /* Add new card */
            item3.CardId = addedCard.Id;
            _scheduleItemRepository.SaveScheduleItem(item3);

            /* Delete card */
            var itemToDelete = addedCard.ScheduleItems[0];
            _scheduleItemRepository.DeleteScheduleItem(itemToDelete.Id);

            //_repository.SaveCard(addedCard);
            addedCard = _repository.GetCard(cardId);

            // assert
            Assert.AreEqual(2, addedCard.ScheduleItems.Count);
            Assert.AreEqual(5, addedCard.ScheduleItems[0].OrderNumber);
            Assert.AreEqual(3, addedCard.ScheduleItems[1].OrderNumber);
        }

        [Test]
        public void DeleteSimpleCard()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            var cardId = _repository.SaveCard(card);

            // act
            _repository.DeleteCard(cardId);
            var deletedCard = _repository.GetCard(cardId);

            // assert
            Assert.AreEqual(null, deletedCard);
        }

        [Test]
        public void DeleteWithScheduleItems()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            ScheduleItem item1 = new ScheduleItem()
            {
                OrderNumber = 1
            };
            ScheduleItem item2 = new ScheduleItem()
            {
                OrderNumber = 2
            };
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id,
                ScheduleItems = new List<ScheduleItem>()
                {
                    item1,
                    item2
                }
            };
            var cardId = _repository.SaveCard(card);
            card = _repository.GetCard(cardId);
            var item1Id = card.ScheduleItems[0].Id;
            var item2Id = card.ScheduleItems[1].Id;

            // act
            _repository.DeleteCardCascade(card);
            var deletedCard = _repository.GetCard(cardId);
            var deletedItem1 = _scheduleItemRepository.GetScheduleItem(item1Id);
            var deletedItem2 = _scheduleItemRepository.GetScheduleItem(item2Id);

            // assert
            Assert.AreEqual(null, deletedCard);
            Assert.AreEqual(null, deletedItem1);
            Assert.AreEqual(null, deletedItem2);
        }

        [Test]
        public void GetActivityCards()
        {
            // arrange
            CardType activityType = 
                _repository.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _repository.CardTypes.GetMotivationGoalCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card3 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            _repository.SaveCard(card1);
            _repository.SaveCard(card2);
            _repository.SaveCard(card3);

            // act
            var cards = _repository.GetActivityCards().ToList();

            // assert
            Assert.AreEqual(2, cards.Count);
        }

        [Test]
        public void GetMotivationGoalCards()
        {
            // arrange
            CardType activityType =
                _repository.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _repository.CardTypes.GetMotivationGoalCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card3 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            _repository.SaveCard(card1);
            _repository.SaveCard(card2);
            _repository.SaveCard(card3);

            // act
            var cards = _repository.GetMotivationGoalCards().ToList();

            // assert
            Assert.AreEqual(1, cards.Count);
        }

        [Test]
        public void IsActivityCard()
        {
            // arrange
            CardType activityType =
                _repository.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _repository.CardTypes.GetMotivationGoalCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            var card1Id = _repository.SaveCard(card1);
            var card2Id = _repository.SaveCard(card2);

            // act
            var isActivity = _repository.IsActivityCard(card1Id);
            var isNotActivity = _repository.IsActivityCard(card2Id);

            // assert
            Assert.AreEqual(true, isActivity);
            Assert.AreEqual(false, isNotActivity);
        }

        [Test]
        public void IsMotivationGoalCard()
        {
            // arrange
            CardType activityType =
                _repository.CardTypes.GetActivityCardType();
            CardType motivationGoalType =
                    _repository.CardTypes.GetMotivationGoalCardType();
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = motivationGoalType.Id
            };
            var card1Id = _repository.SaveCard(card1);
            var card2Id = _repository.SaveCard(card2);

            // act
            var isMotivationGoalCard = _repository.IsMotivationGoalCard(card2Id);
            var isNotMotivationGoalCard = _repository.IsMotivationGoalCard(card1Id);

            // assert
            Assert.AreEqual(true, isMotivationGoalCard);
            Assert.AreEqual(false, isNotMotivationGoalCard);
        }

        [Test]
        public void IsCardInPresentTimetable()
        {
            // arrange
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            ScheduleItem item1 = new ScheduleItem()
            {
                OrderNumber = 1
            };
            ScheduleItem item2 = new ScheduleItem()
            {
                OrderNumber = 2
            };
            Card card1 = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id,
                ScheduleItems = new List<ScheduleItem>()
                {
                    item1,
                    item2
                }
            };

            Card card2 = new Card()
            {
                PhotoPath = "path/to/photo1.jpg",
                CardTypeId = activityType.Id,
            };
            var card1Id = _repository.SaveCard(card1);
            var card2Id = _repository.SaveCard(card2);

            // act
            var isPresent = _repository.IsCardInPresentTimetable(card1Id);
            var notPresent = _repository.IsCardInPresentTimetable(card2Id);

            // assert
            Assert.AreEqual(true, isPresent);
            Assert.AreEqual(false, notPresent);
        }

        [Test]
        public void IsCardExist()
        {
            // arrange
            int notExistCardId = 22;
            CardType activityType = _repository.CardTypes.GetActivityCardType();
            Card card = new Card()
            {
                PhotoPath = "path/to/photo.jpg",
                CardTypeId = activityType.Id
            };
            var cardId = _repository.SaveCard(card);

            // act
            var isExist = _repository.IsCardExist(cardId);
            var notExist = _repository.IsCardExist(notExistCardId);

            // assert
            Assert.AreEqual(true, isExist);
            Assert.AreEqual(false, notExist);
        }
    }
}
