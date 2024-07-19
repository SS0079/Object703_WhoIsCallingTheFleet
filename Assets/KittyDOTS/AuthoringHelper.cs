using Unity.Entities;

namespace KittyDOTS
{
    public static class AuthoringHelper
    {
        public static IBaker AddDisabledComponent<T>(this IBaker baker,Entity target ,T component) where T : unmanaged, IComponentData , IEnableableComponent
        {
            baker.AddComponent(target, component);
            baker.SetComponentEnabled<T>(target,false);
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