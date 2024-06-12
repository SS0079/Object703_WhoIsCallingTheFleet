using System;
using UnityEngine;

namespace KittyHelpYouOut
{
    [ExecuteAlways]
    public abstract class InstallerBase : MonoBehaviour
    {
        private void Update()
        {
            var installAttributes = (InstallComponentAttribute[])Attribute.GetCustomAttributes(GetType(),typeof(InstallComponentAttribute));
            for (int i = 0,j=installAttributes.Length; i < j; i++)
            {
                installAttributes[i].Install(this);
            }
            DestroyImmediate(this);
        }
    }
    
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
    public class InstallComponentAttribute : Attribute
    {
        private readonly Type[] requiredComponentTypes;

        public InstallComponentAttribute(params Type[] requiredComponentTypes)
        {
            this.requiredComponentTypes = requiredComponentTypes;
        }

        public void Install(Component c)
        {
            for (int i = 0,j=requiredComponentTypes.Length; i < j; i++)
            {
                var type = requiredComponentTypes[i];
                if (!c.TryGetComponent(type,out _))
                {
                    c.gameObject.AddComponent(type);
                }
            }
        }
    }
}