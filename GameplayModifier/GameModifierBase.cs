using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TootTally.GameplayModifier
{
    public abstract class GameModifierBase
    {
        public abstract string Name { get; }
        public abstract GameModifiers.ModifierType ModifierType { get; }

        public abstract void Initialize(GameController __instance);

        public abstract void Update(GameController __instance);

        public virtual void Remove()
        {
            GameModifierManager.Remove(ModifierType);
        }
    }
}
