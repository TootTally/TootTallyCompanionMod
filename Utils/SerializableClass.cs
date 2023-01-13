using System;
using System.Collections.Generic;
using System.Text;

namespace TootTally.Utils
{
    public static class SerializableClass
    {
        [Serializable]
        public class Chart
        {
            public string tmb;
        }

        [Serializable]
        public class SendableScore
        {
            public string apiKey;
            public string letterScore;
            public int score;
            public int[] noteTally; // [nasties, mehs, okays, nices, perfects]
            public string songHash;
            public int maxCombo;
            public string gameVersion;
            public int modVersion;
        }

        [Serializable]
        public class ScoreDataFromDB
        {
            public int score;
            public string player;
            public string played_on;
            public string grade;
            public int[] noteTally;
            public string replay_id;
            public int max_combo;
            public float percentage;
            public string game_version;
        }

        [Serializable]
        public class APISubmission
        {
            public string apiKey;
        }

        [Serializable]
        public class ReplayUUIDSubmission
        {
            public string apiKey;
            public string songHash;
        }

        [Serializable]
        public class ReplayJsonSubmission
        {
            public string apiKey;
            public string replayData;
            public string uuid;
        }

        [Serializable]
        public class User
        {
            public string username;
            public int id;
        }
    }
}
