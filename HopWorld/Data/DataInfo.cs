using System.Runtime.Serialization;
using static BingoSyncAPI.BingoSyncTypes;


namespace HopWorld.Data
{
    internal static class DataInfo
    {
        private const float VERSION = 1.0f;

        [DataContract]
        internal class BingoData
        {
            [DataMember]
            private     float           version;
            internal    float           VERSION => this.version;
            internal    bool            IsVersionValid => this.VERSION == DataInfo.VERSION;

            [DataMember]
            private     string          roomID;
            internal    string          RoomID => this.roomID;

            [DataMember]
            private     string          roomPassword;
            internal    string          RoomPassword => this.roomPassword;

            [DataMember]
            private     string          roomPlayerName;
            internal    string          RoomPlayerName => this.roomPlayerName;

            [DataMember]
            private     int             roomPlayerColor;
            internal    PlayerColors    RoomPlayerColor => (PlayerColors)this.roomPlayerColor;

            internal BingoData(string roomID, string roomPassword, string roomPlayerName, PlayerColors roomPlayerColor)
            {
                this.roomID             = roomID;
                this.roomPassword       = roomPassword;
                this.roomPlayerName     = roomPlayerName;
                this.roomPlayerColor    = (int)roomPlayerColor;

                this.version = DataInfo.VERSION;
            }
        }
    }
}
