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

        public ScheduleManager(SQLiteConnection connection)
        {
            Cards = new CardRepository(connection);
            _repository = new ScheduleRepository(connection);
        }

        public Schedule GetSchedule(int scheduleId)
        {
            return _repository.GetSchedule(scheduleId);
        }

        public IEnumerable<Schedule> GetSchedules(int userId)
        {
            return _repository.GetUserSchedules(userId);
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

            return _repository.SaveSchedule(schedule);
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
            FinishSchedule(scheduleId);
            _repository.DeleteScheduleCascade(schedule);
        }

        public void CompleteSchedule(int scheduleId)
        {
            var schedule = GetSchedule(scheduleId);
            schedule.FinishTime = DateTime.Now;
            schedule.IsCompleted = true;
            _repository.SaveSchedule(schedule);
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
            if (cardIds == null)
            {
                throw new ArgumentException("Card ids are not set");
            }

            bool hasActivityCard = false;
            bool hasMotivationGoalCard = false;

            foreach (var cardId in cardIds)
            {
                /* ... all cards are exist */
                if (!Cards.IsCardExist(cardId))
                {
                    throw new ArgumentException($"Card with id: \"{cardId}\"" +
                                                " doesn't exist" );
                }

                /* ... only one motivation goal card is set */
                if (Cards.IsMotivationGoalCard(cardId) && hasMotivationGoalCard)
                {
                    throw new ArgumentException("Multiple motivation goal " +
                                                "cards are not allowed.");
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
                throw new ArgumentException("Activity card is not set " +
                                            "in timetable. At least one " +
                                            "activity card must be set.");
            }

            /* ... motivation goal card is set */
            if (!hasMotivationGoalCard)
            {
                throw new ArgumentException("Motivation goal card is not set" +
                                            "in timetable. You need to set " +
                                            "one motivation goal card.");
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
                    CardId = cardIds.ElementAt(i)
                };

                /* Validate schedule item before saving */
                _repository.ScheduleItems.Validate(item);

                /* Save schedule item id DB */
                _repository.ScheduleItems.SaveScheduleItem(item);
            }

        }
    }
}
