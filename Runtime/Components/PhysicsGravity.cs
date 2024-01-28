#nullable enable
using UnityEngine;

namespace Library.Components
{
    [AddComponentMenu("Physics/Gravity")]
    public class PhysicsGravity : CustomMonoBehaviour
    {
        public Vector3 gravity = new(0f, -9.81f, 0f);

        private Vector3 oldGravity;

        protected override void OnEnable()
        {
            base.OnEnable();
            oldGravity = Physics.gravity;
            Physics.gravity = gravity;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Physics.gravity = oldGravity;
        }
    }
}