using BepInEx;
using GorillaLocomotion;
using HarmonyLib;
using SeralythTemp.Classes;
using SeralythTemp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using static SeralythTemp.Menu.Buttons;
using static SeralythTemp.Settings;

/*
 * Hello, current and future developers!
 * This is ii's Stupid Template, a base mod menu template for Gorilla Tag.
 * 
 * Comments are placed around the code showing you how certain classes work, such as the settings, buttons, and notifications.
 * 
 * If you need help with the template, you may join my Discord server: https://discord.gg/iidk
 * It's full of talented developers that can show you the way and how things work.
 * 
 * If you want to support my, check out my Patreon: https://patreon.com/iiDk
 * Any support is appreciated, and it helps me make more free content for you all!
 * 
 * Thank you, and enjoy the template!
 */

namespace SeralythTemp.Menu
{
    [HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    public class Main : MonoBehaviour
    {
        // Constant
        public static void Prefix()
        {
            // Initialize Menu
                try
                {
                    bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerSecondaryButton);
                    bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);

                    if (menu == null)
                    {
                        if (toOpen || keyboardOpen)
                        {
                            CreateMenu();
                            RecenterMenu(rightHanded, keyboardOpen);
                            if (reference == null)
                                CreateReference(rightHanded);
                        }
                    }
                    else
                    {
                        if (toOpen || keyboardOpen)
                            RecenterMenu(rightHanded, keyboardOpen);
                        else
                        {
                            GameObject shoulderCam = GameObject.Find("Shoulder Camera");
                            if (shoulderCam != null)
                            {
                                Transform vcam = shoulderCam.transform.Find("CM vcam1");
                                if (vcam != null)
                                    vcam.gameObject.SetActive(true);
                            }

                            Rigidbody comp = menu.AddComponent(typeof(Rigidbody)) as Rigidbody;
                            comp.linearVelocity = (rightHanded ? GTPlayer.Instance.LeftHand.velocityTracker : GTPlayer.Instance.RightHand.velocityTracker).GetAverageVelocity(true, 0);

                            Destroy(menu, 2f);
                            menu = null;

                            Destroy(reference);
                            reference = null;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Debug.LogError(string.Format("{0} // Error initializing at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
                }

            // Cleanup Gun
                try
                {
                    if (GunPointer != null)
                    {
                        if (!GunPointer.activeSelf)
                            Destroy(GunPointer);
                        else
                            GunPointer.SetActive(false);
                    }

                    if (GunLine != null)
                    {
                        if (!GunLine.gameObject.activeSelf)
                        {
                            Destroy(GunLine.gameObject);
                            GunLine = null;
                        }
                        else
                            GunLine.gameObject.SetActive(false);
                    }
                } catch { }

            // Constant
                try
                {
                    // Pre-Execution
                        if (fpsObject != null)
                            fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();

                    // Execute Enabled Mods
                        foreach (ButtonInfo button in buttons
                            .SelectMany(list => list)
                            .Where(button => button.enabled && button.method != null && (!legalMode || button.legal)))
                        {
                            try
                            {
                                button.method.Invoke();
                            }
                            catch (Exception exc)
                            {
                                Debug.LogError(string.Format("{0} // Error with mod {1} at {2}: {3}", PluginInfo.Name, button.buttonText, exc.StackTrace, exc.Message));
                            }
                        }
                } catch (Exception exc)
                {
                    Debug.LogError(string.Format("{0} // Error with executing mods at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
                }
        }

        // Functions
        public static void CreateMenu()
        {
            // Menu Holder
                menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(menu.GetComponent<Rigidbody>());
                Destroy(menu.GetComponent<BoxCollider>());
                Destroy(menu.GetComponent<Renderer>());
                menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            // Menu Background
                menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(menuBackground.GetComponent<Rigidbody>());
                Destroy(menuBackground.GetComponent<BoxCollider>());
                menuBackground.transform.parent = menu.transform;
                menuBackground.transform.rotation = Quaternion.identity;
                menuBackground.transform.localScale = menuSize;
                menuBackground.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
                menuBackground.transform.position = new Vector3(0.05f, 0f, 0f);

                ColorChanger colorChanger = menuBackground.AddComponent<ColorChanger>();
                colorChanger.colors = backgroundColor;

            // Canvas
                canvasObject = new GameObject();
                canvasObject.transform.parent = menu.transform;
                Canvas canvas = canvasObject.AddComponent<Canvas>();
                CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasScaler.dynamicPixelsPerUnit = 1000f;

            // Title and FPS
                Text text = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                text.font = currentFont;
                text.text = PluginInfo.Name + " <color=grey>[</color><color=white>" + (pageNumber + 1).ToString() + "</color><color=grey>]</color>" + (searching ? " <color=grey>|</color> Search: " + searchText : "");
                text.fontSize = 1;
                text.color = textColors[0];
                text.supportRichText = true;
                text.fontStyle = FontStyle.Italic;
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                RectTransform component = text.GetComponent<RectTransform>();
                component.localPosition = Vector3.zero;
                component.sizeDelta = new Vector2(0.28f, 0.05f);
                component.position = new Vector3(0.06f, 0f, 0.165f);
                component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                if (fpsCounter)
                {
                    fpsObject = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    fpsObject.font = currentFont;
                    fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                    fpsObject.color = textColors[0];
                    fpsObject.fontSize = 1;
                    fpsObject.supportRichText = true;
                    fpsObject.fontStyle = FontStyle.Italic;
                    fpsObject.alignment = TextAnchor.MiddleCenter;
                    fpsObject.horizontalOverflow = HorizontalWrapMode.Overflow;
                    fpsObject.resizeTextForBestFit = true;
                    fpsObject.resizeTextMinSize = 0;
                    RectTransform component2 = fpsObject.GetComponent<RectTransform>();
                    component2.localPosition = Vector3.zero;
                    component2.sizeDelta = new Vector2(0.28f, 0.02f);
                    component2.position = new Vector3(0.06f, 0f, 0.135f);
                    component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                }

            // Search Keyboard
                if (currentCategory == SearchCategory)
                {
                    CreateSearchDisplay();
                    CreateKeyboard();
                    CreateSearchResults();
                    return;
                }

            // Buttons
                // Disconnect
                    if (disconnectButton)
                    {
                        GameObject disconnectbutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        if (!UnityInput.Current.GetKey(keyboardButton))
                            disconnectbutton.layer = 2;
                        Destroy(disconnectbutton.GetComponent<Rigidbody>());
                        disconnectbutton.GetComponent<BoxCollider>().isTrigger = true;
                        disconnectbutton.transform.parent = menu.transform;
                        disconnectbutton.transform.rotation = Quaternion.identity;
                        disconnectbutton.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
                        disconnectbutton.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
                        disconnectbutton.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                        disconnectbutton.AddComponent<Classes.Button>().relatedText = "Disconnect";

                        colorChanger = disconnectbutton.AddComponent<ColorChanger>();
                        colorChanger.colors = buttonColors[0];

                        Text discontext = new GameObject
                        {
                            transform =
                            {
                                parent = canvasObject.transform
                            }
                        }.AddComponent<Text>();
                        discontext.text = "Disconnect";
                        discontext.font = currentFont;
                        discontext.fontSize = 1;
                        discontext.color = textColors[0];
                        discontext.alignment = TextAnchor.MiddleCenter;
                        discontext.resizeTextForBestFit = true;
                        discontext.resizeTextMinSize = 0;

                        RectTransform rectt = discontext.GetComponent<RectTransform>();
                        rectt.localPosition = Vector3.zero;
                        rectt.sizeDelta = new Vector2(0.2f, 0.03f);
                        rectt.localPosition = new Vector3(0.064f, 0f, 0.23f);
                        rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                    }

                // Page Buttons
                    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (!UnityInput.Current.GetKey(keyboardButton))
                        gameObject.layer = 2;
                    Destroy(gameObject.GetComponent<Rigidbody>());
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    gameObject.transform.parent = menu.transform;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
                    gameObject.transform.localPosition = new Vector3(0.56f, 0.65f, 0);
                    gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                    gameObject.AddComponent<Classes.Button>().relatedText = "PreviousPage";

                    colorChanger = gameObject.AddComponent<ColorChanger>();
                    colorChanger.colors = buttonColors[0];

                    text = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    text.font = currentFont;
                    text.text = "<";
                    text.fontSize = 1;
                    text.color = textColors[0];
                    text.alignment = TextAnchor.MiddleCenter;
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 0;
                    component = text.GetComponent<RectTransform>();
                    component.localPosition = Vector3.zero;
                    component.sizeDelta = new Vector2(0.2f, 0.03f);
                    component.localPosition = new Vector3(0.064f, 0.195f, 0f);
                    component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                    gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (!UnityInput.Current.GetKey(keyboardButton))
                    {
                        gameObject.layer = 2;
                    }
                    Destroy(gameObject.GetComponent<Rigidbody>());
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    gameObject.transform.parent = menu.transform;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
                    gameObject.transform.localPosition = new Vector3(0.56f, -0.65f, 0);
                    gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                    gameObject.AddComponent<Classes.Button>().relatedText = "NextPage";

                    colorChanger = gameObject.AddComponent<ColorChanger>();
                    colorChanger.colors = buttonColors[0];

                    text = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    text.font = currentFont;
                    text.text = ">";
                    text.fontSize = 1;
                    text.color = textColors[0];
                    text.alignment = TextAnchor.MiddleCenter;
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 0;
                    component = text.GetComponent<RectTransform>();
                    component.localPosition = Vector3.zero;
                    component.sizeDelta = new Vector2(0.2f, 0.03f);
                    component.localPosition = new Vector3(0.064f, -0.195f, 0f);
                    component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                // Mod Buttons
                    ButtonInfo[] activeButtons = GetCategoryButtons(currentCategory).Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
                    for (int i = 0; i < activeButtons.Length; i++)
                        CreateButton(i * 0.1f, activeButtons[i]);
        }

        public static ButtonInfo[] GetCurrentButtons()
        {
            if (searching)
            {
                if (string.IsNullOrEmpty(searchText))
                    return buttons
                        .SelectMany(list => list)
                        .Where(button => !legalMode || button.legal)
                        .GroupBy(button => button.buttonText)
                        .Select(group => group.First())
                        .ToArray();
                return buttons
                    .SelectMany(list => list)
                    .Where(button => button.buttonText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Where(button => !legalMode || button.legal)
                    .GroupBy(button => button.buttonText)
                    .Select(group => group.First())
                    .ToArray();
            }

            return GetCategoryButtons(currentCategory);
        }

        public static ButtonInfo[] GetCategoryButtons(int category)
        {
            if (category < 0 || category >= buttons.Length)
                return new ButtonInfo[0];
            ButtonInfo[] categoryButtons = buttons[category];
            if (legalMode)
                return categoryButtons.Where(button => button.legal).ToArray();
            return categoryButtons;
        }

        public static void CreateSearchDisplay()
        {
            Text searchDisplay = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            searchDisplay.font = currentFont;
            searchDisplay.fontSize = 1;
            searchDisplay.text = "Search: " + (searchText.Length > 0 ? searchText + " <color=grey>(" + GetCurrentButtons().Length + " found)</color>" : "_");
            searchDisplay.color = textColors[0];
            searchDisplay.supportRichText = true;
            searchDisplay.alignment = TextAnchor.MiddleCenter;
            searchDisplay.resizeTextForBestFit = true;
            searchDisplay.resizeTextMinSize = 0;
            RectTransform component = searchDisplay.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.28f, 0.04f);
            component.localPosition = new Vector3(0.064f, 0f, 0.28f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void CreateKeyboard()
        {
            string[][] rows = new string[][]
            {   
                new string[] { "1", "2","3", "4", "5", "6", "7", "8", "9", "0" },
                new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
                new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
                new string[] { "Z", "X", "C", "V", "B", "N", "M" }
            };

            float rowZ = 0.28f;
            for (int r = 0; r < rows.Length; r++)
            {
                string[] row = rows[r];
                float stepY = 0.078f;
                float keyWidth = 0.065f;
                float startY = 0.35f;
                for (int i = 0; i < row.Length; i++)
                    CreateKey(row[i], row[i], keyWidth, startY - i * stepY, rowZ);
                rowZ -= 0.075f;
            }

            // Bottom Row
            float bottomZ = rowZ;
            CreateKey("Backspace", "<-", 0.09f, 0.36f, bottomZ);
            CreateKey("Clear", "CLR", 0.08f, 0.235f, bottomZ);
            CreateKey("Space", "SPACE", 0.34f, -0.02f, bottomZ);
            CreateKey("Cancel", "EXIT", 0.09f, -0.36f, bottomZ);
        }

        public static void CreateKey(string key, string label, float keyWidth, float yPos, float zPos)
        {
            GameObject keyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(keyboardButton))
                keyObject.layer = 2;
            Destroy(keyObject.GetComponent<Rigidbody>());
            keyObject.GetComponent<BoxCollider>().isTrigger = true;
            keyObject.transform.parent = menu.transform;
            keyObject.transform.rotation = Quaternion.identity;
            keyObject.transform.localScale = new Vector3(0.09f, keyWidth, 0.04f);
            keyObject.transform.localPosition = new Vector3(0.56f, yPos, zPos);
            keyObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
            keyObject.AddComponent<Classes.Button>().relatedText = "Key:" + key;

            Text keyText = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            keyText.font = currentFont;
            keyText.fontSize = 1;
            keyText.text = label;
            keyText.color = textColors[0];
            keyText.alignment = TextAnchor.MiddleCenter;
            keyText.resizeTextForBestFit = true;
            keyText.resizeTextMinSize = 0;
            RectTransform component = keyText.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(keyWidth * 0.7f, 0.018f);
            component.localPosition = new Vector3(0.064f, yPos * 0.3f, zPos * 0.3825f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void CreateSearchResults()
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            searchResultButtons.Clear();
            ButtonInfo[] results = GetCurrentButtons().Take(6).ToArray();
            float zPos = -0.12f;
            for (int i = 0; i < results.Length; i++)
            {
                ButtonInfo result = results[i];
                searchResultButtons[result.buttonText] = result;

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (!UnityInput.Current.GetKey(keyboardButton))
                    gameObject.layer = 2;
                Destroy(gameObject.GetComponent<Rigidbody>());
                gameObject.GetComponent<BoxCollider>().isTrigger = true;
                gameObject.transform.parent = menu.transform;
                gameObject.transform.rotation = Quaternion.identity;
                gameObject.transform.localScale = new Vector3(0.09f, 0.9f, 0.06f);
                gameObject.transform.localPosition = new Vector3(0.56f, 0f, zPos);
                gameObject.GetComponent<Renderer>().material.color = result.enabled ? buttonColors[1].colors[0].color : buttonColors[0].colors[0].color;
                gameObject.AddComponent<Classes.Button>().relatedText = "Result:" + result.buttonText;

                Text text = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                text.font = currentFont;
                text.text = result.buttonText;
                text.supportRichText = true;
                text.fontSize = 1;
                text.color = result.enabled ? textColors[1] : textColors[0];
                text.alignment = TextAnchor.MiddleCenter;
                text.fontStyle = FontStyle.Italic;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                RectTransform component = text.GetComponent<RectTransform>();
                component.localPosition = Vector3.zero;
                component.sizeDelta = new Vector2(0.2f, 0.03f);
                component.localPosition = new Vector3(0.064f, 0f, zPos * 0.3825f);
                component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                zPos -= 0.08f;
            }
        }

        public static void HandleSearchKey(string key)
        {
            if (key == "Space")
                searchText += " ";
            else if (key == "Backspace")
            {
                if (searchText.Length > 0)
                    searchText = searchText.Substring(0, searchText.Length - 1);
            }
            else if (key == "Clear")
                searchText = "";
            else if (key == "Cancel")
            {
                searching = false;
                searchText = "";
                currentCategory = 0;
                pageNumber = 0;
            }
            else
                searchText += key;

            RecreateMenu();
        }

        public static void ToggleModDirect(ButtonInfo target)
        {
            if (target == null)
                return;

            if (legalMode && !target.legal)
                return;

            if (target.isTogglable)
            {
                target.enabled = !target.enabled;
                if (target.enabled)
                {
                    NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);
                    if (target.enableMethod != null)
                        try { target.enableMethod.Invoke(); } catch { }
                }
                else
                {
                    NotifiLib.SendNotification("<color=grey>[</color><color=red>DISABLE</color><color=grey>]</color> " + target.toolTip);
                    if (target.disableMethod != null)
                        try { target.disableMethod.Invoke(); } catch { }
                }
            }
            else
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);
                if (target.method != null)
                    try { target.method.Invoke(); } catch { }
            }

            Preferences.AutoSave();
            RecreateMenu();
        }

        public static void CreateButton(float offset, ButtonInfo method)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(keyboardButton))
                gameObject.layer = 2;
            
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
            gameObject.AddComponent<Classes.Button>().relatedText = method.buttonText;

            if (method.incremental)
            {
                gameObject.transform.localScale = new Vector3(0.09f, 0.646f, 0.08f);
                RenderIncrementalButton(true, offset, method);
                RenderIncrementalButton(false, offset, method);
            }

            ColorChanger colorChanger = gameObject.AddComponent<ColorChanger>();
            colorChanger.colors = method.enabled ? buttonColors[1] : buttonColors[0];

            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = method.buttonText;

            if (method.overlapText != null)
                text.text = method.overlapText;
            
            text.supportRichText = true;
            text.fontSize = 1;
            text.color = method.enabled ? textColors[1] : textColors[0];
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Italic;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(.2f, .03f);
            component.localPosition = new Vector3(.064f, 0, .111f - offset / 2.6f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        private static void RenderIncrementalButton(bool increment, float offset, ButtonInfo method)
        {
            GameObject buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(keyboardButton))
                buttonObject.layer = 2;

            buttonObject.GetComponent<BoxCollider>().isTrigger = true;
            buttonObject.transform.parent = menu.transform;
            buttonObject.transform.rotation = Quaternion.identity;

            buttonObject.transform.localScale = new Vector3(0.09f, 0.102f, 0.08f);
            buttonObject.transform.localPosition = new Vector3(0.56f, 0.399f, 0.28f - offset);

            Classes.Button button = buttonObject.AddComponent<Classes.Button>();
            button.relatedText = method.buttonText;
            button.incremental = true;
            button.positive = increment;

            if (increment)
                buttonObject.transform.localPosition = new Vector3(buttonObject.transform.localPosition.x, -buttonObject.transform.localPosition.y, buttonObject.transform.localPosition.z);

            ColorChanger colorChanger = buttonObject.AddComponent<ColorChanger>();
            colorChanger.colors = buttonColors[0];

            RenderIncrementalText(increment, offset);
        }

        private static void RenderIncrementalText(bool increment, float offset)
        {
            Text buttonText = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            buttonText.font = currentFont;
            buttonText.text = increment ? "+" : "-";
            buttonText.supportRichText = true;
            buttonText.fontSize = 1;
            buttonText.color = textColors[1];
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Italic;
            buttonText.resizeTextForBestFit = true;
            buttonText.resizeTextMinSize = 0;
            RectTransform textTransform = buttonText.GetComponent<RectTransform>();
            textTransform.localPosition = Vector3.zero;
            textTransform.sizeDelta = new Vector2(.2f, .03f);
            textTransform.localPosition = new Vector3(.064f, increment ? -0.12f : 0.12f, .111f - offset / 2.6f);
            textTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void ToggleIncremental(string buttonText, bool increment)
        {
            ButtonInfo target = GetIndex(buttonText);
            if (target == null)
            {
                Debug.LogError(buttonText + " does not exist");
                RecreateMenu();
                return;
            }

            if (increment)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>INCREMENT</color><color=grey>]</color> " + target.toolTip);
                if (target.enableMethod != null)
                    try { target.enableMethod.Invoke(); } catch { }
            }
            else
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=red>DECREMENT</color><color=grey>]</color> " + target.toolTip);
                if (target.disableMethod != null)
                    try { target.disableMethod.Invoke(); } catch { }
            }

            Preferences.AutoSave();
            RecreateMenu();
        }

        public static void RecreateMenu()
        {
            if (menu != null)
            {
                Destroy(menu);
                menu = null;

                CreateMenu();
                RecenterMenu(rightHanded, UnityInput.Current.GetKey(keyboardButton));
            }
        }

        public static void UpdateButtonText()
        {
            ButtonInfo[] settingsButtons = buttons[1];
            for (int i = 0; i < settingsButtons.Length; i++)
            {
                if (settingsButtons[i].buttonText.StartsWith("Theme:"))
                {
                    string themeName = Classes.ThemeChanger.themes[Classes.ThemeChanger.currentThemeIndex].name;
                    settingsButtons[i].buttonText = "Theme: " + themeName;
                    settingsButtons[i].overlapText = "Theme: <color=grey>[</color><color=green>" + themeName + "</color><color=grey>]</color>";
                    break;
                }
            }
        }

        public static int change16 = 1;
        public static int ButtonSound = 8;

        public static void CycleButtonSound()
        {
            change16++;
            if (change16 > 6)
                change16 = 1;
            ApplyButtonSound();
        }

        public static void PrevButtonSound()
        {
            change16--;
            if (change16 < 1)
                change16 = 6;
            ApplyButtonSound();
        }

        public static void SetButtonSound(int index)
        {
            change16 = index;
            if (index == 6)
                ButtonSound = 114;
            else if (index == 5)
                ButtonSound = 66;
            else if (index == 4)
                ButtonSound = 50;
            else if (index == 3)
                ButtonSound = 203;
            else
                ButtonSound = 8;
            UpdateButtonSoundText();
        }

        private static void ApplyButtonSound()
        {
            SetButtonSound(change16);
            if (change16 == 2)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>BUTTON SOUND</color><color=grey>]</color> Button Sound: Stump</color>");
            }
            if (change16 == 3)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>BUTTON SOUND</color><color=grey>]</color> Button Sound: AK47</color>");
            }
            if (change16 == 4)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>BUTTON SOUND</color><color=grey>]</color> Button Sound: Glass</color>");
            }
            if (change16 == 5)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>BUTTON SOUND</color><color=grey>]</color> Button Sound: KeyBoard</color>");
            }
            if (change16 == 6)
            {
                NotifiLib.SendNotification("<color=grey>[</color><color=green>BUTTON SOUND</color><color=grey>]</color> Button Sound: Cayon Bridge</color>"); // this sounds the best tbh
            }

            VRRig.LocalRig.PlayHandTapLocal(ButtonSound, rightHanded, 0.4f);
            Preferences.AutoSave();
            RecreateMenu();
        }

        private static string ButtonSoundName()
        {
            switch (change16)
            {
                case 2: return "Stump";
                case 3: return "AK47";
                case 4: return "Glass";
                case 5: return "KeyBoard";
                case 6: return "Cayon Bridge";
                default: return "Default";
            }
        }

        private static void UpdateButtonSoundText()
        {
            string soundName = ButtonSoundName();
            foreach (ButtonInfo[] category in buttons)
            {
                for (int i = 0; i < category.Length; i++)
                {
                    if (category[i].buttonText.StartsWith("Button Sound:"))
                    {
                        category[i].buttonText = "Button Sound: " + soundName;
                        category[i].overlapText = "Button Sound: <color=grey>[</color><color=green>" + soundName + "</color><color=grey>]</color>";
                        return;
                    }
                }
            }
        }

        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                try
                {
                    TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                }
                catch { }

                GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-999f, -999f, -999f);
                    TPC.transform.rotation = Quaternion.identity;
                    GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bg.transform.localScale = new Vector3(10f, 10f, 0.01f);
                    bg.transform.transform.position = TPC.transform.position + TPC.transform.forward;
                    Color realcolor = backgroundColor.GetCurrentColor();
                    bg.GetComponent<Renderer>().material.color = new Color32((byte)(realcolor.r * 50), (byte)(realcolor.g * 50), (byte)(realcolor.b * 50), 255);
                    Destroy(bg, 0.05f);
                    menu.transform.parent = TPC.transform;
                    menu.transform.position = TPC.transform.position + (TPC.transform.forward * 0.5f) + (TPC.transform.up * -0.02f);
                    menu.transform.rotation = TPC.transform.rotation * Quaternion.Euler(-90f, 90f, 0f);

                    if (reference != null)
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                            bool hitButton = Physics.Raycast(ray, out RaycastHit hit, 100);
                            if (hitButton)
                            {
                                Classes.Button collide = hit.transform.gameObject.GetComponent<Classes.Button>();
                                collide?.OnTriggerEnter(buttonCollider);
                            }
                        } 
                        else
                            reference.transform.position = new Vector3(999f, -999f, -999f);
                    }
                }
            }
        }

        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference.transform.parent = isRightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
            reference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            buttonCollider = reference.GetComponent<SphereCollider>();

            ColorChanger colorChanger = reference.AddComponent<ColorChanger>();
            colorChanger.colors = backgroundColor;
        }

        public static void Toggle(string buttonText)
        {
            try
            {
                ToggleInner(buttonText);
            }
            catch (Exception exc)
            {
                Debug.LogError(string.Format("{0} // Error toggling {1} at {2}: {3}", PluginInfo.Name, buttonText, exc.StackTrace, exc.Message));
            }
        }

        private static void ToggleInner(string buttonText)
        {
            // Search Keyboard Input
            if (buttonText != null && buttonText.StartsWith("Key:"))
            {
                HandleSearchKey(buttonText.Substring(4));
                return;
            }
            if (buttonText != null && buttonText.StartsWith("Result:"))
            {
                string name = buttonText.Substring(7);
                ButtonInfo target = searchResultButtons.ContainsKey(name) ? searchResultButtons[name] : GetIndex(name);
                if (target != null)
                    ToggleModDirect(target);
                return;
            }

            if (searching && currentCategory == SearchCategory)
            {
                searching = false;
                searchText = "";
                currentCategory = 0;
                pageNumber = 0;
                RecreateMenu();
                return;
            }

            if (currentCategory < 0 || currentCategory >= buttons.Length)
                currentCategory = 0;
            int lastPage = ((GetCategoryButtons(currentCategory).Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
            if (buttonText == "PreviousPage")
            {
                pageNumber--;
                if (pageNumber < 0)
                    pageNumber = lastPage;
            } else
            {
                if (buttonText == "NextPage")
                {
                    pageNumber++;
                    if (pageNumber > lastPage)
                        pageNumber = 0;
                } else
                {
                    ButtonInfo target = GetIndex(buttonText);
                    if (target != null)
                    {
                        if (target.isTogglable)
                        {
                            target.enabled = !target.enabled;
                            if (target.enabled)
                            {
                                NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);
                                if (target.enableMethod != null)
                                    try { target.enableMethod.Invoke(); } catch { }
                            }
                            else
                            {
                                NotifiLib.SendNotification("<color=grey>[</color><color=red>DISABLE</color><color=grey>]</color> " + target.toolTip);
                                if (target.disableMethod != null)
                                    try { target.disableMethod.Invoke(); } catch { }
                            }
                        }
                        else
                        {
                            NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);
                            if (target.method != null)
                                try { target.method.Invoke(); } catch { }
                        }

                        Preferences.AutoSave();
                    }
                    else
                        Debug.LogError(buttonText + " does not exist");
                }
            }
            RecreateMenu();
        }

        private static readonly Dictionary<string, (int Category, int Index)> cacheGetIndex = new Dictionary<string, (int Category, int Index)>(); // Looping through 800 elements is not a light task :/
        public static ButtonInfo GetIndex(string buttonText)
        {
            if (buttonText == null)
                return null;

            if (cacheGetIndex.ContainsKey(buttonText))
            {
                var CacheData = cacheGetIndex[buttonText];
                try
                {
                    if (buttons[CacheData.Category][CacheData.Index].buttonText == buttonText)
                        return buttons[CacheData.Category][CacheData.Index];
                }
                catch { cacheGetIndex.Remove(buttonText); }
            }

            int categoryIndex = 0;
            foreach (ButtonInfo[] buttons in buttons)
            {
                int buttonIndex = 0;
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        try
                        {
                            cacheGetIndex.Add(buttonText, (categoryIndex, buttonIndex));
                        }
                        catch
                        {
                            if (cacheGetIndex.ContainsKey(buttonText))
                                cacheGetIndex.Remove(buttonText);
                        }

                        return button;
                    }
                    buttonIndex++;
                }
                categoryIndex++;
            }

            return null;
        }

        public static Vector3 RandomVector3(float range = 1f) =>
            new Vector3(UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range));

        public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range));

        public static Color RandomColor(byte range = 255, byte alpha = 255) =>
            new Color32((byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        alpha);

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            Quaternion rot = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handRotOffset;
            return (GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static void WorldScale(GameObject obj, Vector3 targetWorldScale)
        {
            Vector3 parentScale = obj.transform.parent.lossyScale;
            obj.transform.localScale = new Vector3(
                targetWorldScale.x / parentScale.x,
                targetWorldScale.y / parentScale.y,
                targetWorldScale.z / parentScale.z
            );
        }

        public static void FixStickyColliders(GameObject platform)
        {
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(0, 1f, 0),
                new Vector3(0, -1f, 0),
                new Vector3(1f, 0, 0),
                new Vector3(-1f, 0, 0),
                new Vector3(0, 0, 1f),
                new Vector3(0, 0, -1f)
            };
            Quaternion[] localRotations = new Quaternion[]
            {
                Quaternion.Euler(90, 0, 0),
                Quaternion.Euler(-90, 0, 0),
                Quaternion.Euler(0, -90, 0),
                Quaternion.Euler(0, 90, 0),
                Quaternion.identity,
                Quaternion.Euler(0, 180, 0)
            };
            for (int i = 0; i < localPositions.Length; i++)
            {
                GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    if (platform.GetComponent<GorillaSurfaceOverride>() != null)
                    {
                        side.AddComponent<GorillaSurfaceOverride>().overrideIndex = platform.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                    }
                }
                catch { }
                float size = 0.025f;
                side.transform.SetParent(platform.transform);
                side.transform.localPosition = localPositions[i] * (size / 2);
                side.transform.localRotation = localRotations[i];
                WorldScale(side, new Vector3(size, size, 0.01f));
                side.GetComponent<Renderer>().enabled = false;
            }
        }

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                1 << LayerMask.NameToLayer("TransparentFX") |
                1 << LayerMask.NameToLayer("Ignore Raycast") |
                1 << LayerMask.NameToLayer("Zone") |
                1 << LayerMask.NameToLayer("Gorilla Trigger") |
                1 << LayerMask.NameToLayer("Gorilla Boundary") |
                1 << LayerMask.NameToLayer("GorillaCosmetics") |
                1 << LayerMask.NameToLayer("GorillaParticle"));

            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }

        public static bool gunLocked;
        public static VRRig lockTarget;

        public static (RaycastHit Ray, GameObject NewPointer) RenderGun(int? overrideLayerMask = null)
        {
            Transform GunTransform = GorillaTagger.Instance.rightHandTransform;

            Vector3 StartPosition = GunTransform.position;
            Vector3 Direction = GunTransform.forward;

            Physics.Raycast(StartPosition + Direction / 4f, Direction, out var Ray, 512f, overrideLayerMask ?? NoInvisLayerMask());
            Vector3 EndPosition = gunLocked ? lockTarget.transform.position : Ray.point;

            if (EndPosition == Vector3.zero)
                EndPosition = StartPosition + Direction * 512f;

            if (GunPointer == null)
                GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            GunPointer.SetActive(true);
            GunPointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            GunPointer.transform.position = EndPosition;

            Renderer PointerRenderer = GunPointer.GetComponent<Renderer>();
            PointerRenderer.material.shader = Shader.Find("GUI/Text Shader");
            PointerRenderer.material.color = gunLocked || ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f ? buttonColors[1].GetCurrentColor() : buttonColors[0].GetCurrentColor();

            Destroy(GunPointer.GetComponent<Collider>());

            if (GunLine == null)
            {
                GameObject line = new GameObject("iiMenu_GunLine");
                GunLine = line.AddComponent<LineRenderer>();
            }

            GunLine.gameObject.SetActive(true);
            GunLine.material.shader = Shader.Find("GUI/Text Shader");
            GunLine.startColor = backgroundColor.GetCurrentColor();
            GunLine.endColor = backgroundColor.GetCurrentColor(0.5f);
            GunLine.startWidth = 0.025f;
            GunLine.endWidth = 0.025f;
            GunLine.positionCount = 2;
            GunLine.useWorldSpace = true;

            GunLine.SetPosition(0, StartPosition);
            GunLine.SetPosition(1, EndPosition);

            return (Ray, GunPointer);
        }

        // Variables
        // Important
        // Objects
        public static GameObject menu;
        public static GameObject menuBackground;   
        public static GameObject reference;
        public static GameObject canvasObject;

        public static SphereCollider buttonCollider;
        public static Camera TPC;
        public static Text fpsObject;

        private static GameObject GunPointer;
        private static LineRenderer GunLine;

        // Data
        public static int pageNumber = 0;
        public const int SearchCategory = 13;
        public static bool searching = false;
        public static string searchText = "";
        public static Dictionary<string, ButtonInfo> searchResultButtons = new Dictionary<string, ButtonInfo>();
        public static int _currentCategory;
        public static int currentCategory
        {
            get => _currentCategory;
            set
            {
                _currentCategory = value;
                pageNumber = 0;
            }
        }
    }
}
