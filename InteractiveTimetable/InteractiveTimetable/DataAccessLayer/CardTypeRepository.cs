using System.Collections.Generic;
using System.Linq;
using InteractiveTimetable.BusinessLayer.Models;
using SQLite;

namespace InteractiveTimetable.DataAccessLayer
{
    public class CardTypeRepository : BaseRepository
    {
        private const string ActivityTypeName = "ACTIVITY_CARD";
        private const string MotivationGoalTypeName = "MOTIVATION_GOAL_CARD";

        internal CardTypeRepository(SQLiteConnection connection) : base(connection)
        {
            /* If there are no default card types in DB, create it */
            if(GetCardTypes().Count() == 0)
            {
                CardType activivtyCardType = new CardType()
                {
                    TypeName = ActivityTypeName
                };

                CardType motivationGoalCardType = new CardType()
                {
                    TypeName = MotivationGoalTypeName
                };

                SaveCardType(activivtyCardType);
                SaveCardType(motivationGoalCardType);
            }
        }

        public CardType GetCardType(int cardTypeId)
        {
            return _database.GetItemCascade<CardType>(cardTypeId);
        }

        public IEnumerable<CardType> GetCardTypes()
        {
            return _database.GetItemsCascade<CardType>();
        }

        internal int SaveCardType(CardType cardType)
        {
            return _database.SaveItemCascade(cardType);
        }

        internal int DeleteCardType(int cardTypeId)
        {
            return _database.DeleteItem<CardType>(cardTypeId);
        }

        internal void DeleteCardTypeCascade(CardType cardType)
        {
            _database.DeleteItemCascade(cardType);
        }

        public CardType GetActivityCardType()
        {
            return GetCardTypes().
                FirstOrDefault(x => x.TypeName == ActivityTypeName);
        }

        public CardType GetMotivationGoalCardType()
        {
            return GetCardTypes().
                    FirstOrDefault(x => x.TypeName == MotivationGoalTypeName);
        }

        public bool IsActivityCardType(int cardTypeId)
        {
            var cardType = GetCardType(cardTypeId);

            if (cardType != null)
            {
                return cardType.TypeName.Equals(ActivityTypeName);
            }

            return false;
        }

        public bool IsMotivationGoalCardType(int cardTypeId)
        {
            var cardType = GetCardType(cardTypeId);
                
            if (cardType != null)
            {
                return cardType.TypeName.Equals(MotivationGoalTypeName);
            }

            return false;
        }
    }

}
