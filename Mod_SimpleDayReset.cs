using HarmonyLib;
using Il2Cpp;
using Il2CppFluffyUnderware.DevTools.Extensions;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using Object = Il2CppSystem.Object;

[assembly: MelonInfo(typeof(BPMod_SimpleDayReset.Mod_SimpleDayReset), "BPMod_SimpleDayReset", "1.0.0", "Borealum", null)]
[assembly: MelonGame("Dogubomb", "BLUE PRINCE")]

namespace BPMod_SimpleDayReset
{
    public class Mod_SimpleDayReset : MelonMod
    {
        public static GameObject globalPersistent;
        public static GameObject saveSystem;
        public static GameObject saveContinueObj;
        public static GameObject callItADayObj;
        public static string myButtonName = "RESET DAY";
        public static Color hoverOff = new Color(0.3882353f, 0.3882353f, 0.3882353f, 1f);
        public static bool resetTheGame = false;
        public static GameObject currSave;
        public static int saveSlot;

        public override void OnInitializeMelon()
        {
            ClassInjector.RegisterTypeInIl2Cpp<MyButtonHandler>();
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != null && sceneName.Equals("Mount Holly Estate"))
            {
                LoggerInstance.Msg("Mount Holly Estate loaded");
                globalPersistent = GameObject.Find("Global Persitent Manager");
                PlayMakerFSM persistentFSM = globalPersistent.GetComponent<PlayMakerFSM>();
                saveSystem = GameObject.Find("SAVE SYSTEM");
                PlayMakerFSM saveSystemFSM = saveSystem.GetComponent<PlayMakerFSM>();

                //Check if nothing is saved on current saveslot. If so, save.
                string saveKey = null;
                LoggerInstance.Msg($"Current save slot: {saveSlot}");
                if (saveSlot == 1)
                    saveKey = "BluePrint";
                else if (saveSlot == 2)
                    saveKey = "BluePrint2";
                else if (saveSlot == 3)
                    saveKey = "BluePrint3";
                else if (saveSlot == 4)
                    saveKey = "BluePrint4";
                LoggerInstance.Msg($"Current saveKey: {saveKey}");
                if (saveKey != null)
                {
                    if (!ES3.KeyExists(saveKey, "MtHollyBlueprint.es3"))
                    {
                        LoggerInstance.Msg($"nothing saved in current saveslot, saving day 0 dummy");
                        persistentFSM.fsm.Variables.GetFsmInt("SaveSlot").Value = saveSlot;

                        //Save them manually... originally made this because I thought a bug was happening, but I'll just leave this in, shouldn't hurt
                        PlayMakerHashTableProxy rarityShifts = globalPersistent.GetComponents<PlayMakerHashTableProxy>().FirstOrDefault(p => p.referenceName.Equals("Rarity Shifts"));
                        var keyList = rarityShifts.preFillKeyList;
                        Il2CppReferenceArray<Object> arrRarityShiftsKeys = new Il2CppReferenceArray<Object>(keyList.Count);
                        Il2CppReferenceArray<Object> arrRarityShiftsValues = new Il2CppReferenceArray<Object>(keyList.Count);
                        int i = 0;
                        foreach (string room in keyList)
                        {
                            arrRarityShiftsKeys[i] = room;
                            arrRarityShiftsValues[i] = 0;
                            i++;
                        }
                        var arrRarityShiftsKeysf = persistentFSM.fsm.Variables.GetFsmArray("Rarity Shifts Keys");
                        var arrRarityShiftsValuesf = persistentFSM.fsm.Variables.GetFsmArray("Rarity Shifts Values");
                        arrRarityShiftsKeysf.Values = arrRarityShiftsKeys;
                        arrRarityShiftsValuesf.Values = arrRarityShiftsValues;

                        PlayMakerHashTableProxy roomRecords = globalPersistent.GetComponents<PlayMakerHashTableProxy>().FirstOrDefault(p => p.referenceName.Equals("RoomRecords"));
                        keyList = roomRecords.preFillKeyList;
                        Il2CppReferenceArray<Object> arrRoomRecordsKeys = new Il2CppReferenceArray<Object>(keyList.Count);
                        Il2CppReferenceArray<Object> arrRoomRecordsValues = new Il2CppReferenceArray<Object>(keyList.Count);
                        i = 0;
                        foreach (string room in keyList)
                        {
                            arrRoomRecordsKeys[i] = room;
                            arrRoomRecordsValues[i] = 0;
                            i++;
                        }
                        var arrRoomRecordsKeysf = persistentFSM.fsm.Variables.GetFsmArray("RoomRecords Keys");
                        var arrRoomRecordsValuesf = persistentFSM.fsm.Variables.GetFsmArray("RoomRecords Values");
                        arrRoomRecordsKeysf.Values = arrRoomRecordsKeys;
                        arrRoomRecordsValuesf.Values = arrRoomRecordsValues;

                        persistentFSM.SendEvent("SAVE");
                    }
                }

                //just adding reset buttons to menus
                GameObject yesButton = FindDeep(GameObject.Find("UI OVERLAY CAM/PAUSE").transform, "SAVE & CONTINUE/YES")?.gameObject;
                saveContinueObj = yesButton.transform.parent.gameObject;
                GameObject myYesButton = GameObject.Instantiate(yesButton, saveContinueObj.transform);
                myYesButton.name = myButtonName;
                myYesButton.transform.position += new Vector3(0f, 1.3f, 0f);

                RectTransform rectTransform = myYesButton.GetComponent<RectTransform>();
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 25f);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 5f);

                TextMeshPro textMesh = myYesButton.GetComponent<TextMeshPro>();
                textMesh.text = myButtonName;
                textMesh.color = hoverOff;
                textMesh.m_textAlignment = TextAlignmentOptions.Center;

                myYesButton.GetComponent<PlayMakerFSM>().Destroy();
                myYesButton.AddComponent<MyButtonHandler>();

                yesButton = FindDeep(GameObject.Find("UI OVERLAY CAM/PAUSE").transform, "CALL IT A DAY TEXT/Options/YES")?.gameObject;
                callItADayObj = yesButton.transform.parent.gameObject;
                myYesButton = GameObject.Instantiate(yesButton, callItADayObj.transform);
                callItADayObj = callItADayObj.transform.parent.gameObject;//careful, going up by one more
                myYesButton.name = myButtonName;
                myYesButton.transform.position += new Vector3(0.1f, 0.6f, 0f);

                rectTransform = myYesButton.GetComponent<RectTransform>();
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 25f);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 5f);

                textMesh = myYesButton.GetComponent<TextMeshPro>();
                textMesh.text = myButtonName;
                textMesh.color = hoverOff;
                textMesh.m_textAlignment = TextAlignmentOptions.Center;

                myYesButton.GetComponent<PlayMakerFSM>().Destroy();
                myYesButton.AddComponent<MyButtonHandler>();
            }
        }

        [HarmonyPatch(typeof(GameObject), "SetActive")]
        class Patch_SetActive
        {
            static void Postfix(GameObject __instance, bool value)
            {
                string name = __instance.name;
                if (name != null)
                {
                    //hiding the button if you got day1 victory
                    if (name.Equals("SAVE & CONTINUE") && value)
                    {
                        bool victory = globalPersistent?.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Room46 Day1")?.Value ?? false;
                        int day = globalPersistent?.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("DAY")?.Value ?? 1000;
                        victory = victory && (day==0 || day==1);
                        MelonLogger.Msg($"day 1 & victory: {victory}");
                        __instance.transform.Find(myButtonName)?.gameObject.SetActive(!victory);
                    }
                    //need to get current save slot value from somewhere, because at start of day 1 it's empty...
                    //preferably in main menu when new game starts loading
                    if (name.Contains("LOADING") && value)
                    {
                        currSave = GameObject.Find("CURRENT SAVE");
                        if (currSave != null)
                        {
                            saveSlot = currSave.GetComponent<PlayMakerFSM>().fsm.variables.GetFsmInt("current save").Value;
                            MelonLogger.Msg($"current save in main menu: {saveSlot}");
                        }
                    }
                }
            }
        }

        // Finds child by path, including inactive objects
        private Transform FindDeep(Transform parent, string path)
        {
            string[] parts = path.Split('/');
            Transform current = parent;
            bool logOutput = false;
            foreach (string partTxt in parts)
            {
                if (logOutput) LoggerInstance.Msg($"findDeep - current: {current.name}");
                if (logOutput) LoggerInstance.Msg($"findDeep - childCount: {current.childCount}");
                if (logOutput) LoggerInstance.Msg($"findDeep - part: {partTxt}");

                for (int i = 0; i < current.childCount; i++)
                {
                    Transform child = current.GetChild(i);
                    if (logOutput) LoggerInstance.Msg($"findDeep - current inner: {current.name}");
                    if (logOutput) LoggerInstance.Msg($"findDeep - child: {child.name}");
                    if (child.name.Equals(partTxt))
                    {
                        current = child;
                        break;
                    }
                }
            }
            return current;
        }
    }

    public class MyButtonHandler : MonoBehaviour
    {
        bool hovering = false;
        Color hoverOn = new Color(0.2627451f, 0.6039216f, 0.8f, 1f);

        void Update()
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            TextMeshPro text = gameObject.GetComponent<TextMeshPro>();
            TextMeshPro textMesh;
            if (Physics.Raycast(r, out var hit) && hit.transform == transform)
            {
                if (!hovering)
                {
                    hovering = true;
                    text.color = hoverOn;
                    //decolor other buttons
                    Transform parent = transform.parent;
                    for (int i = 0; i < parent.childCount; i++)
                    {
                        Transform sibling = parent.GetChild(i);
                        if (sibling != transform)
                        {
                            textMesh = sibling.GetComponent<TextMeshPro>();
                            if (textMesh != null)
                            {
                                textMesh.color = Mod_SimpleDayReset.hoverOff;
                            }
                        }
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    PlayMakerFSM globalPersistentFSM = Mod_SimpleDayReset.globalPersistent?.GetComponent<PlayMakerFSM>();
                    PlayMakerFSM saveSystemFSM = Mod_SimpleDayReset.saveSystem?.GetComponent<PlayMakerFSM>();
                    if (globalPersistentFSM != null && saveSystemFSM != null)
                    {
                        /*
                         * just doing this by itself wasn't enough, it played the intro cutscene and showed "day 1" but permanent changes were still there...
                         * maybe just doing some globalPersistentFSM.Reset() would have been easier...but I realized that late
                         * //globalPersistentFSM.FsmVariables.GetFsmInt("DAY").Value -= 1;
                         * //saveSystemFSM.Fsm.GetState("Dream Check 2").GetTransition(0).ToFsmState = saveSystemFSM.Fsm.GetState("Restart");
                         */

                        //try gobalPersistentFSM.Reset()? - freezes game

                        //hide buttons
                        Mod_SimpleDayReset.saveContinueObj.transform.Find("YES")?.gameObject.SetActive(false);
                        Mod_SimpleDayReset.saveContinueObj.transform.Find("NO")?.gameObject.SetActive(false);
                        Mod_SimpleDayReset.saveContinueObj.transform.Find(Mod_SimpleDayReset.myButtonName)?.gameObject.SetActive(false);
                        Mod_SimpleDayReset.callItADayObj.SetActive(false);
                        Mod_SimpleDayReset.saveContinueObj.SetActive(true);
                        PlayMakerFSM saveContinueFsm = Mod_SimpleDayReset.saveContinueObj.GetComponent<PlayMakerFSM>();
                        saveContinueFsm.Fsm.Variables.GetFsmBool("CALL IT BUTTON").Value = true;//this is set when using the original continue button, not sure if it's important

                        //rerouting save system transitions
                        //to load game instead of saving
                        saveSystemFSM.Fsm.GetState("Dream Check 2").GetTransition(0).ToFsmState = saveSystemFSM.Fsm.GetState("Load");
                        //to reload the scene after loading
                        saveSystemFSM.Fsm.GetState("Rarity Shift Convert 2").GetTransition(0).ToFsmState = saveSystemFSM.Fsm.GetState("Restart");
                        //shouldn't need to reset the fsm connections after loading, because it magically happens on scene reload?

                        MelonLogger.Msg($"Reset day - loading SaveSlot: {globalPersistentFSM.fsm.Variables.GetFsmInt("SaveSlot").Value}");
                        //fiddling with manual loading, throw away
                        //if (ES3.KeyExists("BluePrint3"))
                        //{
                        //    ES3.LoadInto("BluePrint3", globalPersistentFSM.GetComponent<PlayMakerFSM>());
                        //}

                        //execute loading
                        saveContinueFsm.SendEvent("Go");//handles stopAllTracks(), fadeOut()... and sends "SAVE DATA SAVE" to save system
                        //SceneManager.LoadSceneAsync("Mount Holly Estate", LoadSceneMode.Single);//doable, but I prefer running the existing fsm logic so everything is executed like in game
                    }
                }
            }
            else
            {
                if (hovering)
                {
                    hovering = false;
                    text.color = Mod_SimpleDayReset.hoverOff;
                }
            }
        }
    }
}