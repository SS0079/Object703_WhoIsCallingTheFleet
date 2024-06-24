using Unity.Entities;

namespace KittyDOTS
{
    public static class AuthoringHelper
    {
        public static IBaker AddEnableComponent<T>(this IBaker baker,Entity target ,T component, bool enabled = true) where T : unmanaged, IComponentData , IEnableableComponent
        {
            baker.AddComponent(target, component);
            baker.SetComponentEnabled<T>(target,enabled);
            return baker;
        }

        public static IBaker AddBuffer<T>(this IBaker baker, Entity target, T bufferElement) where T : unmanaged, IBufferElementData
        {
            baker.AddBuffer<T>(target);
            baker.AppendToBuffer(target,bufferElement);
            return baker;
        }
    }
}