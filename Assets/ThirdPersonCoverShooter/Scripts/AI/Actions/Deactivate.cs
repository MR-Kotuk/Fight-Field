using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [Failure("Null")]
    [Immediate]
    [Folder("Object")]
    public class Deactivate : BaseAction
    {
        [ValueType(ValueType.GameObject, true)]
        public Value Object;

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var deref = state.Dereference(ref Object);

            if (deref.Type == ValueType.GameObject)
            {
                if (Go(deref.GameObject))
                    return AIResult.Finish();
                else
                    return AIResult.Failure();
            }
            else if (deref.Type == ValueType.Array)
            {
                var count = 0;

                for (int i = 0; i < deref.Array.Length; i++)
                    if (Go(deref.Array[i].GameObject))
                        count++;

                if (count == 0)
                    return AIResult.Failure();
                else
                    return AIResult.Finish();
            }

            return AIResult.Failure();
        }

        private bool Go(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                return true;
            }
            else
                return false;
        }
    }
}