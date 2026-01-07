using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Net;
using System.Reflection;
using AIOHHF.Items.Upgrades;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UWE;
using Random = UnityEngine.Random;
using AIOHHF.Mono;
using Object = UnityEngine.Object;

namespace AIOHHF.Items.Equipment;

public class AllInOneHandHeldFabricator
{
    public static Dictionary<CraftTree.Type, TechType> CustomFabricators = new();
    //public static Dictionary<CraftNode, CraftTree.Type> Fabricators = new();
    public static Dictionary<CraftTree.Type, bool> PrefabRegisters = new();
    public static Dictionary<TechType, CraftNode> Nodes = new();
    public PrefabInfo PrefabInfo;
    public CustomPrefab Prefab;
    //public static FabricatorGadget Fabricator;
    public Vector3 PostScaleValue;
    public CraftTree.Type TreeType;
    private CraftNode _nodeRoot;
    public static List<CraftNode> Trees = new();
    public static List<UpgradesPrefabs>  Upgrades =  new();
    public AssetBundle Bundle;
    public void Initialize(WaitScreenHandler.WaitScreenTask task)
    {
        PrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
                        "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held(tm)!" +
                        "\nEnergy consumption is the same as a normal Fabricator")
                    .WithIcon(Bundle.LoadAsset<Sprite>("AIOHHF_Icon")).WithSizeInInventory(new Vector2int(2,2));
        Prefab = new CustomPrefab(PrefabInfo);
        Prefab.CreateFabricator(out TreeType)
            .Root.CraftTreeCreation = () =>
        {
            const string schemeId = "AIOHHFCraftTree";
            return new CraftTree(schemeId, _nodeRoot);
        };
        PrefabRegisters[TreeType] = true;
        //Fabricator = Prefab.GetGadget<FabricatorGadget>();

        var clone = new FabricatorTemplate(PrefabInfo, TreeType)
        {
            FabricatorModel = FabricatorTemplate.Model.Fabricator,
            ModifyPrefab = prefab =>
            {
                GameObject model = prefab.gameObject; 
                model.transform.localScale = Vector3.one / 2f;
                Plugin.Aiohhf.PostScaleValue = model.transform.localScale;
                var fab = prefab.GetComponent<Fabricator>();
                if (fab != null)
                {
                    var hhf = prefab.AddComponent<AioHandHeldFabricator>().CopyComponent(fab);
                    Object.Destroy(fab);
                    hhf.craftTree = Plugin.Aiohhf.TreeType;
                }
                prefab.AddComponent<Pickupable>();
                prefab.AddComponent<Rigidbody>();
                PrefabUtils.AddWorldForces(prefab, 5f);
                PrefabUtils.AddStorageContainer(prefab, "AIOHHFStorageContainer", "ALL IN ONE HAND HELD FABRICATOR", 2 ,2);
                List<TechType> compatBats = new List<TechType>()
                {
                    TechType.Battery,
                    TechType.PrecursorIonBattery
                };
                prefab.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
                PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(prefab, 
                    "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23" +
                    "(i forgot that i made my upgradeslib hand held fabricator the same storage root class id 😭 - written by lad)", 
                    TechType.Battery, compatBats);
                prefab.AddComponent<AiohhPlayerTool>();
                var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer == null) return; 
                var texture = Bundle.LoadAsset<Texture>("AIOHHF_diffuse_and_spec");
                if (texture == null) return;
                renderer.material.mainTexture = texture;
                renderer.material.SetTexture("_SpecTex", texture);
            }
        };
        Prefab.SetGameObject(clone);
        var ingredients = new List<Ingredient>()
        {
            new Ingredient(TechType.Titanium, 3),
            new Ingredient(TechType.CopperWire, 2)
        };
        Prefab.SetRecipe(new RecipeData()
            {
                craftAmount = 1,
                Ingredients = ingredients
            })
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal","Tools")
            .WithCraftingTime(5f);

        //var fragments = EnumHandler.AddEntry<TechType>("AIOHHFFragment").Value;
        //var AIOHHFF1PI = new PrefabInfo("AIOHHFF1", "aiohhffragprefab1", fragments);
        //var AIOHHFF1CP = new CustomPrefab(AIOHHFF1PI);
        
        //TODO: FINISH FRAGMENTS
        Prefab.SetUnlock(TechType.Peeper);//TODO: Make fragments
        Prefab.SetEquipment(EquipmentType.Hand);
        Prefab.Register();
    }

    public void RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "";
        _nodeRoot = new CraftNode("Root");
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                //skip stuff that either throws exceptions, is my own tree, or is an unused tree
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 ||
                    treeType == CraftTree.Type.Rocket || treeType == TreeType
                    || treeType == CraftTree.Type.Centrifuge) continue;

                //if its not defined, add it
                if (!PrefabRegisters.ContainsKey(treeType)) PrefabRegisters.Add(treeType, false);
                //techtype to set with a scope outside of each if statement
                TechType techType;
                //get the craft tree's techtype
                if (!TechTypeExtensions.FromString(treeType.ToString(), out techType, false)
                    && treeType != CraftTree.Type.MapRoom && treeType != CraftTree.Type.SeamothUpgrades) continue;
                //get the techtypes for outliers because there is no techtype of "MapRoom" or "SeamothUpgrades"
                if (treeType == CraftTree.Type.MapRoom) techType = TechType.BaseMapRoom;
                if (treeType == CraftTree.Type.SeamothUpgrades) techType = TechType.BaseUpgradeConsole;
                //is it a custom craft tree?
                if (EnumHandler.ModdedEnumExists<CraftTree.Type>(treeType.ToString()))
                    //add it if so
                    CustomFabricators.Add(treeType, techType);
                //do nothing with the vanilla ones since they are mapped manually
            }

            _nodeRoot.AddNode(CraftTreeMethods.RegisterFabricatorUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterWorkbenchUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterCyclopsFabricatorUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterScannerRoomUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterVehicleUpgradeConsoleUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterPrecursorFabricatorUpgrade());
            foreach (CraftNode node in CraftTreeMethods.RegisterCustomFabricatorUpgrades())
            {
                _nodeRoot.AddNode(node);
            }
            
            if (!PrefabRegisters.ContainsKey(TreeType)) PrefabRegisters.Add(TreeType, false);
            if (!PrefabRegisters[TreeType]) Initialize(task);
    }

    public static IEnumerator GetGameObject(IOut<GameObject> gameObject)
    {
        CoroutineTask<GameObject> prefab = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
        yield return prefab;
        var model = prefab.GetResult();
        var go = new GameObject("AIOHHF");
        var instantiatedObject = Object.Instantiate(model, go.transform);
        instantiatedObject.SetActive(false);
        var fab = go.GetComponentInChildren<Fabricator>();
        if (fab != null)
        {
            var hhf = go.AddComponent<AioHandHeldFabricator>().CopyComponent(fab);
            Object.Destroy(fab);
            hhf.craftTree = Plugin.Aiohhf.TreeType;
        }
        Plugin.Aiohhf.PostScaleValue = instantiatedObject.transform.localScale = Vector3.one / 2f;
        go.AddComponent<Pickupable>();
        go.AddComponent<Rigidbody>();
        PrefabUtils.AddWorldForces(go, 5);
        PrefabUtils.AddStorageContainer(go, "AIOHHFStorageContainer", "ALL IN ONE HAND HELD FABRICATOR", 2 ,2);
        List<TechType> compatBats = new List<TechType>()
        {
            TechType.Battery,
            TechType.PrecursorIonBattery
        };
        go.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
        PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(go, 
            "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23" +
            "(i forgot that i made my upgradeslib hand held fabricator the same storage root class id :sob: - written by lad)", 
            TechType.Battery, compatBats);
        gameObject.Set(go);
    }
}