using GorillaLocomotion;
using SeralythTemp.Classes;
using SeralythTemp.Menu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static SeralythTemp.Menu.Main;

namespace SeralythTemp.Mods
{
    public class Movement
    {
        public static void Fly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward *
                                                        Time.deltaTime * Settings.Movement.flySpeed;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static float startX = -1;
        public static float startY = -1;
        public static float subThingy;
        public static float subThingyZ;
        public static Vector3 lastPosition = Vector3.zero;

        public static void WASDFly()
        {
            bool stationary = Main.GetIndex("Disable Stationary WASD Fly").enabled;

            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb == null) return;

            bool W = kb.wKey.isPressed;
            bool A = kb.aKey.isPressed;
            bool S = kb.sKey.isPressed;
            bool D = kb.dKey.isPressed;
            bool Space = kb.spaceKey.isPressed;
            bool Ctrl = kb.leftCtrlKey.isPressed;
            bool Shift = kb.leftShiftKey.isPressed;
            bool Alt = kb.leftAltKey.isPressed;

            bool LeftArrow = kb.leftArrowKey.isPressed;
            bool RightArrow = kb.rightArrowKey.isPressed;
            bool UpArrow = kb.upArrowKey.isPressed;
            bool DownArrow = kb.downArrowKey.isPressed;

            if (stationary || W || A || S || D || Space || Ctrl)
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;

            Transform parentTransform;
            if (menu != null)
                parentTransform = GTPlayer.Instance.GetControllerTransform(false).parent;
            else
                parentTransform = GorillaTagger.Instance.headCollider.transform;

            float turnSpeed = 250f;

            if (LeftArrow)
                parentTransform.eulerAngles += new Vector3(0, -turnSpeed, 0) * Time.deltaTime;
            if (RightArrow)
                parentTransform.eulerAngles += new Vector3(0, turnSpeed, 0) * Time.deltaTime;
            if (UpArrow)
                parentTransform.eulerAngles += new Vector3(-turnSpeed, 0, 0) * Time.deltaTime;
            if (DownArrow)
                parentTransform.eulerAngles += new Vector3(turnSpeed, 0, 0) * Time.deltaTime;

            if (mouse != null && mouse.rightButton.isPressed)
            {
                Quaternion currentRotation = parentTransform.rotation;
                Vector3 euler = currentRotation.eulerAngles;

                if (startX < 0)
                {
                    startX = euler.y;
                    subThingy = mouse.position.value.x / Screen.width;
                }

                if (startY < 0)
                {
                    startY = euler.x;
                    subThingyZ = mouse.position.value.y / Screen.height;
                }

                float newX = startY - (mouse.position.value.y / Screen.height - subThingyZ) * 360 * 1.33f;
                float newY = startX + (mouse.position.value.x / Screen.width - subThingy) * 360 * 1.33f;

                newX = newX > 180f ? newX - 360f : newX;
                newX = Mathf.Clamp(newX, -90f, 90f);

                parentTransform.rotation = Quaternion.Euler(newX, newY, euler.z);
            }
            else
            {
                startX = -1;
                startY = -1;
            }

            float speed = Settings.Movement.flySpeed;
            if (Shift)
                speed *= 2f;
            else if (Alt)
                speed /= 2;

            if (W)
                GorillaTagger.Instance.rigidbody.transform.position +=
                    parentTransform.forward * (Time.deltaTime * speed);
            if (S)
                GorillaTagger.Instance.rigidbody.transform.position +=
                    parentTransform.forward * (Time.deltaTime * -speed);
            if (A)
                GorillaTagger.Instance.rigidbody.transform.position +=
                    parentTransform.right * (Time.deltaTime * -speed);
            if (D)
                GorillaTagger.Instance.rigidbody.transform.position += parentTransform.right * (Time.deltaTime * speed);
            if (Space)
                GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * speed, 0f);
            if (Ctrl)
                GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * -speed, 0f);

            VRRig.LocalRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

            if (!W && !A && !S && !D && !Space && !Ctrl && lastPosition != Vector3.zero && stationary)
                GorillaTagger.Instance.rigidbody.transform.position = lastPosition;
            else
                lastPosition = GorillaTagger.Instance.rigidbody.transform.position;

            GorillaTagger.Instance.rigidbody.useGravity = true;
        }

        public static GameObject platl;
        public static GameObject platr;
        public static Rigidbody platlRb;
        public static Rigidbody platrRb;

        public static void Platforms()
        {
            if (ControllerInputPoller.instance.leftGrab)
            {
                if (platl == null)
                {
                    platl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platl.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platlRb = platl.AddComponent<Rigidbody>();
                    platlRb.isKinematic = true;

                    FixStickyColliders(platl);

                    ColorChanger colorChanger = platl.AddComponent<ColorChanger>();
                    colorChanger.colors = SeralythTemp.Settings.backgroundColor;
                }
                else
                {
                    platlRb.MovePosition(TrueLeftHand().position);
                    platlRb.MoveRotation(TrueLeftHand().rotation);
                }
            }
            else
            {
                if (platl != null)
                {
                    Object.Destroy(platl);
                    platl = null;
                    platlRb = null;
                }
            }

            if (ControllerInputPoller.instance.rightGrab)
            {
                if (platr == null)
                {
                    platr = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platr.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platrRb = platr.AddComponent<Rigidbody>();
                    platrRb.isKinematic = true;

                    FixStickyColliders(platr);

                    ColorChanger colorChanger = platr.AddComponent<ColorChanger>();
                    colorChanger.colors = SeralythTemp.Settings.backgroundColor;
                }
                else
                {
                    platrRb.MovePosition(TrueRightHand().position);
                    platrRb.MoveRotation(TrueRightHand().rotation);
                }
            }
            else
            {
                if (platr != null)
                {
                    Object.Destroy(platr);
                    platr = null;
                    platrRb = null;
                }
            }
        }

        public static bool previousTeleportTrigger;

        public static void TeleportGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f && !previousTeleportTrigger)
                {
                    GTPlayer.Instance.TeleportTo(NewPointer.transform.position + Vector3.up,
                        GTPlayer.Instance.transform.rotation);
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                }

                previousTeleportTrigger = ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f;
            }
        }
    }
}
