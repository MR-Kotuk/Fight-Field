using UnityEngine;

namespace CoverShooter.AI
{
    [Immediate]
    [Success("Done")]
    [Folder("Audio")]
    public class PlayAudioAt : BaseAction
    {
        [ValueType(ValueType.AudioClip)]
        public Value Clip;

        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position;

        [ValueType(ValueType.Float)]
        public Value Volume = new Value(1f);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var clip = state.Dereference(ref Clip).AudioClip;

            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, state.GetPosition(ref Position), state.Dereference(ref Volume).Float);

            return AIResult.Finish();
        }
    }
}