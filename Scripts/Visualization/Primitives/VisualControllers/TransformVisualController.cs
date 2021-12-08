using UnityEngine;
using Labust.Visualization.Primitives;

namespace Labust.Visualization
{
    public class TransformVisualController : MonoBehaviour {

        public Labust.Visualization.Primitives.Transform MyTransform;

        void OnDestroy() {
            MyTransform.Destroy();
        }
    }
}
