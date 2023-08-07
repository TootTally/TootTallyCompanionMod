using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTally.GameplayModifier
{
    public static class GameModifiers
    {

        public class Hidden : GameModifierBase
        {
            public Hidden() : base("HD")
            {
            }

            public override void Initialize(GameController __instance)
            {
                throw new NotImplementedException();
            }

            public override void Update(GameController __instance)
            {
                throw new NotImplementedException();
            }
        }

        public class Flashlight : GameModifierBase
        {
            public Flashlight() : base("FL")
            {
            }

            public override void Initialize(GameController __instance)
            {
                throw new NotImplementedException();
            }

            public override void Update(GameController __instance)
            {
                throw new NotImplementedException();
            }
        }


    }
}
