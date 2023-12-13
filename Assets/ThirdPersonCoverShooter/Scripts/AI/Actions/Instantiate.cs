using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [SuccessParameter("Object", ValueType.GameObject)]
    [Failure("Null")]
    [Immediate]
    [Folder("Object")]
    public class Instantiate : BaseAction
    {
        [ValueType(ValueType.GameObject)]
        public Value Object;

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var obj = state.Dereference(ref Object).GameObject;

            if (obj != null)
            {
                var result = new Value[1];
                result[0] = new Value(GameObject.Instantiate(obj));

                return AIResult.Finish(result);
            }
            else
                return AIResult.Failure();
        }
    }
}