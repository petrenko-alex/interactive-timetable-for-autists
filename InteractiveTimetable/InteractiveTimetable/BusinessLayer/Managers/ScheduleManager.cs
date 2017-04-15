using System;
using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using InteractiveTimetable.DataAccessLayer;
using SQLite;

namespace InteractiveTimetable.BusinessLayer.Managers
{
    public class ScheduleManager
    {
        private readonly ScheduleRepository _repository;
        public readonly CardRepository Cards;
        public int ScheduleCount { get; set; }

        public ScheduleManager(SQLiteConnection connection)
        {
            Cards = new CardRepository(connection);
            _repository = new ScheduleRepository(connection);
            ScheduleCount = _repository.GetSchedules().Count();
        }

        public Schedule GetSchedule(int scheduleId)
        {
            return _repository.GetSchedule(scheduleId);
        }

        public IEnumerable<Schedule> GetSchedules(int userId)
        {
            return _repository.GetUserSchedules(userId);
        }

        public IEnumerable<int> GetCardIds(int scheduleId)
        {
            return GetCardIds(GetSchedule(scheduleId));
        }

        public IEnumerable<Card> GetScheduleCards(int scheduleId)
        {
            /* Get ids of cards in schedule */
            var scheduleCardIds = GetCardIds(scheduleId);

            /* Get cards by card id */
            var scheduleCards = scheduleCardIds.Select(x => Cards.GetCard(x));
            return scheduleCards;
        }

        public int SaveSchedule(int userId, List<int> cardIds)
        {
            /* Data validation */
            Validate(cardIds);

            /* Creating schedule item objects */
            var scheduleItems = CreateScheduleItems(cardIds).ToList();

            /* Creating a schedule object */
            var schedule = new Schedule()
            {
                UserId = userId,
                ScheduleItems = scheduleItems,
            };

            int savedId = _repository.SaveSchedule(schedule);

            if (savedId > 0)
            {
                ScheduleCount++;
            }

            return savedId;
        }

        public int UpdateSchedule(int scheduleId, List<int> cardIds)
        {
            var schedule = GetSchedule(scheduleId);

            /* Data validation */
            Validate(cardIds);

            /* Updating schedule date */
            UpdateScheduleItems(schedule, cardIds);

            return scheduleId;
        }

        internal void DeleteSchedule(int scheduleId)
        {
            var schedule = GetSchedule(scheduleId);
            if (schedule != null)
            {
                FinishSchedule(scheduleId);
                _repository.DeleteScheduleCascade(schedule);
                ScheduleCount--;
            }
        }

        public void CompleteSchedule(int scheduleId)
        {
            var schedule = GetSchedule(scheduleId);
            schedule.FinishTime = DateTime.Now;
            schedule.IsCompleted = true;
            _repository.SaveSchedule(schedule);
        }

        public void CompleteScheduleItem(int scheduleItemId)
        {
            var scheduleItem = _repository.ScheduleItems.GetScheduleItem(scheduleItemId);
            SetScheduleItemCompleted(scheduleItem, true);
        }

        public void UncompleteScheduleItem(int scheduleItemId)
        {
            var scheduleItem = _repository.ScheduleItems.GetScheduleItem(scheduleItemId);
            SetScheduleItemCompleted(scheduleItem, false);
        }

        public void FinishSchedule(int scheduleId)
        {
            var schedule = GetSchedule(scheduleId);
            schedule.FinishTime = DateTime.Now;
            _repository.SaveSchedule(schedule);
        }

        private void Validate(List<int> cardIds)
        {
            /* Checking that ... */

            /* ... cardIds is set */
            if (cardIds == null || cardIds.Count == 0)
            {
                throw new ArgumentException(
                    Resources.Validation.ScheduleValidationStrings.CardsAreNotSet);
            }

            bool hasActivityCard = false;
            bool hasMotivationGoalCard = false;

            foreach (var cardId in cardIds)
            {
                /* ... all cards are exist */
                if (!Cards.IsCardExist(cardId))
                {
                    var exceptionString =
                            Resources.Validation.ScheduleValidationStrings.
                                      CardNotExist;
                    exceptionString = string.Format(exceptionString, cardId);

                    throw new ArgumentException(exceptionString);
                }

                /* ... only one motivation goal card is set */
                if (Cards.IsMotivationGoalCard(cardId) && hasMotivationGoalCard)
                {
                    throw new ArgumentException(Resources.Validation.
                                                          ScheduleValidationStrings.
                                                          MultipleGoalCards);
                }

                /* Finding motivation goal card */
                if (Cards.IsMotivationGoalCard(cardId))
                {
                    hasMotivationGoalCard = true;
                }

                /* Finding activity card */
                if (Cards.IsActivityCard(cardId))
                {
                    hasActivityCard = true;
                }
            }

            /* ... at least one activity card is set */
            if (!hasActivityCard)
            {
                throw new ArgumentException(Resources.Validation.
                                                      ScheduleValidationStrings.
                                                      NoActivityCard);
            }

            /* ... motivation goal card is set */
            if (!hasMotivationGoalCard)
            {
                throw new ArgumentException(Resources.Validation.
                                                      ScheduleValidationStrings.
                                                      NoGoalCard);
            }
        }

        private IEnumerable<ScheduleItem> CreateScheduleItems(List<int> cardIds)
        {
            var items = new List<ScheduleItem>();

            int amountOfCards = cardIds.Count;
            for (int i = 0; i < amountOfCards; ++i)
            {
                var item = new ScheduleItem()
                {
                    OrderNumber = i + 1,
                    CardId = cardIds.ElementAt(i)
                };
                
                /* Validating schedule item before saving */
                _repository.ScheduleItems.Validate(item);

                items.Add(item);
            }

            return items;
        }

        private void UpdateScheduleItems(Schedule schedule, List<int> cardIds)
        {
            /* Deleting previous schedule items */
            var items = _repository.ScheduleItems.
                                    GetScheduleItemsOfSchedule(schedule.Id);

            foreach (var scheduleItem in items)
            {
                _repository.ScheduleItems.DeleteScheduleItem(scheduleItem.Id);
            }

            /* Creating new schedule items */
            int amountOfCards = cardIds.Count;
            for (int i = 0; i < amountOfCards; ++i)
            {
                var item = new ScheduleItem()
                {
                    OrderNumber = i + 1,
                    CardId = cardIds.ElementAt(i),
                    ScheduleId = schedule.Id
                };

                /* Validate schedule item before saving */
                _repository.ScheduleItems.Validate(item);

                /* Save schedule item id DB */
                _repository.ScheduleItems.SaveScheduleItem(item);
            }

        }

        private IEnumerable<int> GetCardIds(Schedule schedule)
        {
            return schedule.ScheduleItems.
                            OrderBy(x => x.OrderNumber).
                            Select(x => x.CardId);
        }

        private void SetScheduleItemCompleted(ScheduleItem scheduleItem, bool isCompleted)
        {
            if (scheduleItem != null)
            {
                scheduleItem.IsCompleted = isCompleted;
                _repository.ScheduleItems.SaveScheduleItem(scheduleItem);
            }
        }

        public void InitializeForDebugging(UserManager userManager)
        {
            var randomizer = new Random();
            var users = userManager.GetUsers();

            /* Create schedules for every user in database */
            foreach (var user in users)
            {
                /* Get cards from database */
                var activityCards = Cards.GetActivityCards().ToList();
                var goalCards = Cards.GetMotivationGoalCards().ToList();
                int activityCardsCount = activityCards.Count;
                int goalCardsCount = goalCards.Count;

                /* Choose activity card set */
                int cardsCountForSchedule = 17;
                var cardIdsForSchedule = new List<int>();
                for (int i = 0; i < cardsCountForSchedule; ++i)
                {
                    int activityCardNumber = randomizer.Next(0, activityCardsCount);
                    cardIdsForSchedule.Add(activityCards[activityCardNumber].Id);
                }

                /* Choose motivation goal card */
                int goalCardNumber = randomizer.Next(0, goalCardsCount);
                cardIdsForSchedule.Add(goalCards[goalCardNumber].Id);

                /* Create schedule for user */
                SaveSchedule(user.Id, cardIdsForSchedule);
            }
        }
    }
}
