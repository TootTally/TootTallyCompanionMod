using System;
using System.Collections.Generic;
using System.Text;

namespace TootTally
{
    public static class SerializableSubmissionClass
    {
        [Serializable]
        public class Chart
        {
            public string tmb;
        }

        [Serializable]
        public class Score
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
    }
}
