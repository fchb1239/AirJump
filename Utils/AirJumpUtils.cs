using AirJump.Structs;
using Photon.Pun;
using UnityEngine;

namespace AirJump.Utils
{
    public static class AirJumpUtils
    {
        public static Jump CreateJump(Jump jump)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = $"Jump ({jump.Player.ActorNumber})";

            Transform transform = gameObject.transform;
            transform.position = jump.Position;
            transform.rotation = jump.Rotation;
            transform.localScale = Constants.Sizes[jump.Size];


            MaterialOverride mat = GetMaterial(jump.Material);

            if (mat.physicMaterial != null)
            {
                // I could do BoxCollider to be more specefic but in the future if I ever do something weird then it wouldn't support it hence why this is getting Collider, even though it does the same thing
                Collider collider = gameObject.GetComponent<Collider>();
                collider.material = mat.physicMaterial;
            }

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = mat.material;

            GorillaSurfaceOverride surface = gameObject.AddComponent<GorillaSurfaceOverride>();
            surface.overrideIndex = mat.surfaceOverride;

            jump.Object = gameObject;

            return jump;
        }

        public static MaterialOverride GetMaterial(int materialIndex)
        {
            switch (materialIndex)
            {
                case 1:
                    foreach (VRRig rig in GameObject.FindObjectsOfType<VRRig>())
                    {
                        if (rig.isOfflineVRRig)
                        {
                            return new MaterialOverride(rig.materialsToChangeTo[0], null, 4);
                        }
                    }
                    break;
                case 2:
                    foreach (VRRig rig in GameObject.FindObjectsOfType<VRRig>())
                    {
                        if (rig.isOfflineVRRig)
                        {
                            return new MaterialOverride(rig.materialsToChangeTo[2], null, 203);
                        }
                    }
                    break;
                case 3:
                    foreach (VRRig rig in GameObject.FindObjectsOfType<VRRig>())
                    {
                        if (rig.isOfflineVRRig)
                        {
                            return new MaterialOverride(rig.materialsToChangeTo[1], null, 144);
                        }
                    }
                    break;
                case 4:
                    foreach (VRRig rig in GameObject.FindObjectsOfType<VRRig>())
                    {
                        if (rig.isOfflineVRRig)
                        {
                            //return new MaterialOverride(rig.materialsToChangeTo[3], 42);
                            return new MaterialOverride(rig.materialsToChangeTo[3], new PhysicMaterial
                            {
                                staticFriction = 0,
                                dynamicFriction = 0,
                                frictionCombine = PhysicMaterialCombine.Minimum
                            }, 59);
                            //return new MaterialOverride(rig.materialsToChangeTo[3], 1);
                        }
                    }
                    break;
                default:
                    break;
            }

            Material material = new Material(Shader.Find("GorillaTag/UberShader"));
            material.color = Color.black;
            return new MaterialOverride(material, null, 81);
        }

        public struct MaterialOverride
        {
            public Material material;
            public PhysicMaterial physicMaterial;
            public int surfaceOverride;

            public MaterialOverride(Material mat, PhysicMaterial phys, int index)
            {
                material = mat;
                physicMaterial = phys;
                surfaceOverride = index;
            }
        }
    }
}
