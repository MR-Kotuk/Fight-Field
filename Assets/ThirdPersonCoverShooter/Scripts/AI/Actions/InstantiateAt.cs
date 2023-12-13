using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [SuccessParameter("Object", ValueType.GameObject)]
    [Failure("Null")]
    [Immediate]
    [Folder("Object")]
    public class InstantiateAt : BaseAction
    {
        [ValueType(ValueType.GameObject)]
        public Value Object;

        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var obj = state.Dereference(ref Object).GameObject;

            if (obj != null)
            {
                var result = new Value[1];
                result[0] = new Value(GameObject.Instantiate(obj, state.GetPosition(ref Position), obj.transform.rotation));

                return AIResult.Finish(result);
            }
            else
                return AIResult.Failure();
        }
    }
}