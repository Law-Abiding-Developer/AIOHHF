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
    internal static bool Registered = false;
    public IEnumerator Initialize(WaitScreenHandler.WaitScreenTask task)
    {
        if (Registered) yield break;
        Registered = true;
        task.Status = "Registering All In One Hand Held Fabricator...\nGiving PrefabInfo and loading icon";
        yield return task;
        PrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
                        "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held(tm)!" +
                        "\nUnfortunately, it holds no data of the Fabricator, you'll have to give it data. " +
                        "Alterra is not responsible for data loss due to damage, lost, or destruction of this fabricator. " +
                        "Energy consumption is the same as a normal Fabricator", "English", true)
                    .WithIcon(Bundle.LoadAsset<Sprite>("AIOHHF_Icon")).WithSizeInInventory(new Vector2int(2,2));
        task.Status = "Registering All In One Hand Held Fabricator...\nCreating CustomPrefab";
        yield return task;
        Prefab = new CustomPrefab(PrefabInfo);
        task.Status = "Registering All In One Hand Held Fabricator...\nCreating Fabricator with registered CraftTree";
        yield return task;
        Prefab.CreateFabricator(out TreeType)
            .Root.CraftTreeCreation = () =>
        {
            const string schemeId = "AIOHHFCraftTree";
            return new CraftTree(schemeId, _nodeRoot);
        };
        task.Status = "Registering All In One Hand Held Fabricator...\nEnsuring this doesn't happen twice";
        yield return task;
        //Fabricator = Prefab.GetGadget<FabricatorGadget>();

        task.Status = "Registering All In One Hand Held Fabricator...\nCloning Fabricator Model";
        yield return task;
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
                var actualModel = prefab.FindChild("submarine_fabricator_01");
                var fpModel = prefab.AddComponent<FPModel>();
                fpModel.viewModel = actualModel;
                var copy = Object.Instantiate(actualModel, prefab.transform);
                fpModel.propModel = copy;
                actualModel.transform.localEulerAngles = new Vector3(0,180,0);
                actualModel.transform.localPosition = new Vector3(0, 0, 0.15f);
            }
        };
        task.Status = "Registering All In One Hand Held Fabricator...\nSetting CustomPrefab's GameObject to the cloned fabricator";
        yield return task;
        Prefab.SetGameObject(clone);
        task.Status = "Registering All In One Hand Held Fabricator...\nAssigning Recipe";
        yield return task;
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

        
        task.Status = "Registering All In One Hand Held Fabricator...\nRegistering Fragments";
        yield return task;
        yield return Fragments.Initialize(task);
        task.Status = "Registering All In One Hand Held Fabricator...\nMaking scanning required for unlock";
        yield return task;
        PDAHandler.AddCustomScannerEntry(Fragments.FragmentsTechType, 
            PrefabInfo.TechType, true, 3);
        task.Status = "Registering All In One Hand Held Fabricator...\nSetting Equipment Type";
        yield return task;
        Prefab.SetEquipment(EquipmentType.Hand);
        task.Status = "Registering All In One Hand Held Fabricator...\nWrapping up by registering...";
        yield return task;
        Prefab.Register();
        task.Status = "Registering All In One Hand Held Fabricator...\nDone!";
        yield return task;
    }

    public IEnumerator RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Registering AIOHHF Craft Tree";
        if (Registered)
        {
            task.Status = "Already registered"; yield break;}
        _nodeRoot = new CraftNode("Root");
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                //skip stuff that either throws exceptions, is my own tree, or is an unused tree
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 ||
                    treeType == CraftTree.Type.Rocket || treeType == TreeType
                    || treeType == CraftTree.Type.Centrifuge) continue;
                
                //techtype to set with a scope outside of each if statement
                TechType techType;
                //get the craft tree's techtype
                if (!TechTypeExtensions.FromString(treeType.ToString(), out techType, false)
                    && treeType != CraftTree.Type.MapRoom && treeType != CraftTree.Type.SeamothUpgrades) continue;
                //get the techtypes for outliers because there is no techtype of "MapRoom" or "SeamothUpgrades"
                if (techType == TechType.None)
                    techType = treeType == CraftTree.Type.SeamothUpgrades
                        ? TechType.BaseUpgradeConsole
                        : TechType.BaseMapRoom;
                //is it a custom craft tree?
                if (EnumHandler.ModdedEnumExists<CraftTree.Type>(treeType.ToString()))
                    //add it if so
                    CustomFabricators.Add(treeType, techType);
                //do nothing with the vanilla ones since they are mapped manually
            }

            task.Status = "Registering Fabricator Data chip";
            _nodeRoot.AddNode(CraftTreeMethods.RegisterFabricatorUpgrade());
            task.Status = "Registering Modification Station Data chip";
            _nodeRoot.AddNode(CraftTreeMethods.RegisterWorkbenchUpgrade());
            task.Status = "Registering Cyclops Fabricator Data chip";
            _nodeRoot.AddNode(CraftTreeMethods.RegisterCyclopsFabricatorUpgrade());
            task.Status = "Registering Scanner Room Fabricator Data chip";
            _nodeRoot.AddNode(CraftTreeMethods.RegisterScannerRoomUpgrade());
            task.Status = "Registering Vehicle Upgrade Console Fabricator Data chip";
            _nodeRoot.AddNode(CraftTreeMethods.RegisterVehicleUpgradeConsoleUpgrade());
            task.Status = "Registering Prototype Fabricator Data chip (if Prototype is loaded)";
            _nodeRoot.AddNode(CraftTreeMethods.RegisterPrecursorFabricatorUpgrade());
            task.Status = "Registering all Custom Fabricator Data chips";
            foreach (CraftNode node in CraftTreeMethods.RegisterCustomFabricatorUpgrades())
            {
                task.Status = $"Registering all Custom Fabricator Data chips\nRegistered {node.techType0}!";
                _nodeRoot.AddNode(node);
            }

            task.Status = "Registering All In One Hand Held Fabricator...";
            yield return Initialize(task);
    }
}