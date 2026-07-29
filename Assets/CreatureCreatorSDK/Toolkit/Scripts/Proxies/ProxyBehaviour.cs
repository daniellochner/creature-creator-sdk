using UnityEngine;

namespace DanielLochner.CreatureCrafter.SDK
{
    [SelectionBase]
    public class ProxyBehaviour : MonoBehaviour
    {
        protected virtual void OnDrawGizmos()
        {
        }
        protected virtual void OnDrawGizmosSelected()
        {
        }

        public virtual bool IsValid()
        {
            bool isValid = IsValid(out string error);
            if (!isValid && !string.IsNullOrEmpty(error))
            {
                Debug.LogError(error, gameObject);
            }
            return isValid;
        }

        public virtual bool IsValid(out string error)
        {
            error = "";
            return true;
        }

        protected static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        protected static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        protected static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        protected static bool IsFinite(Color value)
        {
            return IsFinite(value.r) && IsFinite(value.g) && IsFinite(value.b) && IsFinite(value.a);
        }
    }
}
