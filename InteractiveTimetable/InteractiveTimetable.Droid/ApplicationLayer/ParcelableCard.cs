using Android.OS;
using InteractiveTimetable.BusinessLayer.Models;
using Java.Interop;
using Object = Java.Lang.Object;

namespace InteractiveTimetable.Droid.ApplicationLayer
{
    internal class ParcelableCard : Object, IParcelable
    {
        private int CardId { get; }
        private string PhotoPath { get; }
        private int CardTypeId { get; }

        [ExportField("CREATOR")]
        public static ParcelableCardCreator InitializeCreator()
        {
            return new ParcelableCardCreator();
        }

        public ParcelableCard(int cardId, string photoPath, int cardTypeId)
        {
            CardId = cardId;
            PhotoPath = photoPath;
            CardTypeId = cardTypeId;
        }

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel outParcel, ParcelableWriteFlags flags)
        {
            outParcel.WriteInt(CardId);
            outParcel.WriteString(PhotoPath);
            outParcel.WriteInt(CardTypeId);
        }

        public override string ToString()
        {
            return base.ToString() +
                   $"CardId: {CardId} PhotoPath: {PhotoPath} CardTypeId: {CardTypeId}";
        }

        public static Card ToCard(ParcelableCard parcelableCard)
        {
            return new Card()
            {
                Id = parcelableCard.CardId,
                PhotoPath = parcelableCard.PhotoPath,
                CardTypeId = parcelableCard.CardTypeId
            };
        }

        public static ParcelableCard FromCard(Card card)
        {
            return new ParcelableCard(
                card.Id,
                card.PhotoPath,
                card.CardTypeId
            );
        }
    }

    internal class ParcelableCardCreator : Object, IParcelableCreator
    {
        public Object CreateFromParcel(Parcel source)
        {
            return new ParcelableCard(source.ReadInt(), source.ReadString(), source.ReadInt());
        }

        public Object[] NewArray(int size)
        {
            return new Object[size];
        }
    }

}