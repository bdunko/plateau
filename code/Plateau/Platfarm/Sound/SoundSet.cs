using Microsoft.Xna.Framework.Audio;
using Platfarm.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platfarm.Sound
{
    public class SoundSet
    {
        public enum Type
        {
            FX, LOOP, SONG
        }

        private Type type;
        private SoundEffect[] sounds;

        public SoundSet(Type type, params SoundEffect[] sounds) {
            this.type = type;
            this.sounds = sounds;
        }

        public Type GetType()
        {
            return this.type;
        }

        public SoundEffect GetSound()
        {
            return sounds[Util.RandInt(0, sounds.Count() - 1)];
        }
    }
}
